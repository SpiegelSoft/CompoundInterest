namespace Interestic.Common

open XamarinForms.Reactive.FSharp

open ReactiveUI

open System

open DynamicData.Binding
open DynamicData

open ObservableExtensions
open LocatorDefaults
open System.Reactive.Linq

module ReactiveCommands =
    let private create (factory:IObservable<bool> -> ReactiveCommand<'src, 'dest>) (canExecute) =
        let ce = match canExecute with | Some c -> c | None -> Observable.Return<bool>(true)
        let command = factory(ce)
        command.ThrownExceptions.Subscribe() |> ignore
        command.Catch(fun _ -> Observable.Return Unchecked.defaultof<'dest>) |> ignore
        command
    let createFromAsync(operation: 'src -> Async<'dest>, canExecute: IObservable<bool> option) =
        let factory ce = ReactiveCommand.CreateFromTask<'src, 'dest>(operation >> Async.StartAsTask, ce)
        create factory canExecute

open ReactiveCommands
open Xamarin.Forms
open System.Collections.Specialized
open System.Reactive

module Compounding =
    let options =
        dict [
            ("1 - Compounded Annually", 1)
            ("2 - Compounded Bi-annually", 2)
            ("4 - Compounded Quarterly", 4)
            ("12 - Compounded Monthly", 4)
        ]

open Telerik.XamarinForms.Common.DataAnnotations
open Telerik.XamarinForms.Input.DataForm
open System.Collections
open System.Linq

type PositiveNumberValidatorAttribute(message) =
    inherit ValidatorBaseAttribute()
    do base.NegativeFeedback <- message
    override __.ValidateCore value = match value with | :? float as num when num > 0.0 -> true | _ -> false

type CompoundingProvider() =
    inherit PropertyDataSourceProvider()
    let compoundingSource = "CompoundingSource" :> obj
    override __.GetSourceForKey(key) = 
        match key with
        | compoundingSource -> Compounding.options.Keys.ToList() :> IList
        | _ -> base.GetSourceForKey(key)

and CompoundInterestInput() = 
    inherit ReactiveObject()
    let today = DateTime.Today
    let mutable principal, interestRate, startDate, endDate, compounded, use360DayYear = 0.0, 0.0, today, today.AddYears(2), Compounding.options |> Seq.head |> (fun x -> x.Key), false
    [<DisplayOptions (Header = "Principal", PlaceholderText = "Enter Principal")>]
    [<PositiveNumberValidator("Principal must be a positive number.")>]
    member __.Principal with get() = principal and set(value) = base.RaiseAndSetIfChanged(&principal, value, "Principal") |> ignore
    [<DisplayOptions (Header = "Interest Rate (%)", PlaceholderText = "Enter Interest Rate")>]
    [<PositiveNumberValidator("Interest rate must be a positive percentage.")>]
    member __.InterestRate with get() = interestRate and set(value) = base.RaiseAndSetIfChanged(&interestRate, value, "InterestRate") |> ignore
    [<DisplayOptions (Header = "Start Date", PlaceholderText = "Enter Start Date")>]
    [<DisplayValueFormat(Date = "dd MMM yyyy")>]
    member __.StartDate with get() = startDate and set(value) = base.RaiseAndSetIfChanged(&startDate, value, "StartDate") |> ignore
    [<DisplayOptions (Header = "End Date", PlaceholderText = "Enter End Date")>]
    [<DisplayValueFormat(Date = "dd MMM yyyy")>]
    member __.EndDate with get() = endDate and set(value) = base.RaiseAndSetIfChanged(&endDate, value, "EndDate") |> ignore
    [<DisplayOptions (Header = "Rests per Year", PlaceholderText = "Enter Rests per Year")>]
    [<DataSourceKey("CompoundingSource")>]
    member __.Compounded with get() = compounded and set(value) = base.RaiseAndSetIfChanged(&compounded, value, "Compounded") |> ignore
    [<DisplayOptions (Header = "Use 360 Day Year", PlaceholderText = "Enter Rests per Year")>]
    member __.Use360DayYear with get() = use360DayYear and set(value) = base.RaiseAndSetIfChanged(&use360DayYear, value, "Use360DayYear") |> ignore

type CompoundInterestOutput = {
    TotalDue: float
    InterestAccrued: float
}

type DashboardViewModel(?host: IScreen, ?platform: IInteresticPlatform) =
    inherit PageViewModel()
    let host, platform = LocateIfNone host, LocateIfNone platform
    let mutable totalDue = 0.0
    let mutable interestAccrued = 0.0
    let mutable showResults = false
    let mutable errorMessage = Unchecked.defaultof<ObservableAsPropertyHelper<string>>
    let today = DateTime.Today
    let calculateCompoundInterest (input: CompoundInterestInput) (_:Unit) =
        let yearLength = match input.Use360DayYear with | true -> 360.0 | false -> 365.0
        let termDays = (input.EndDate - input.StartDate).TotalDays
        let rests = Compounding.options.[input.Compounded] |> float
        let exponent = (termDays * rests)/yearLength
        let firstBit = 1.0 + input.InterestRate/(rests * 100.0)
        let due = input.Principal * (firstBit ** exponent)
        { TotalDue = due; InterestAccrued = due - input.Principal }
    let getErrorMessage (input: CompoundInterestInput) =
        match input with
        | i when i.Principal = 0.0 -> "A positive principal is required."
        | i when i.InterestRate = 0.0 -> "A positive interest rate is required."
        | i when i.StartDate > i.EndDate -> "The end date cannot be earlier than the start date"
        | _ -> String.Empty
    let isValid (input: CompoundInterestInput) =
        match input with
        | i when i.Principal = 0.0 -> false
        | i when i.InterestRate = 0.0 -> false
        | i when i.StartDate > i.EndDate -> false
        | _ -> true
    member __.ShowResults with get() = showResults and set(value) = base.RaiseAndSetIfChanged(&showResults, value, "ShowResults") |> ignore
    member __.TotalDue with get() = totalDue and set(value) = base.RaiseAndSetIfChanged(&totalDue, value, "TotalDue") |> ignore
    member __.InterestAccrued with get() = interestAccrued and set(value) = base.RaiseAndSetIfChanged(&interestAccrued, value, "InterestAccrued") |> ignore
    member __.ErrorMessage = errorMessage.Value
    member val Input = new CompoundInterestInput()
    member val Calculate = Unchecked.defaultof<ReactiveCommand<Unit, CompoundInterestOutput>> with get, set
    member val CopyTextToClipboard = Unchecked.defaultof<ReactiveCommand<string, string>> with get, set
    override this.SetUpCommands() = 
        base.SetUpCommands()
        let copyTextToClipboard text = platform.CopyTextToClipboard "Calculation Results" text; text
        let inputObservable =
            Observable.CombineLatest([
                this.Input.WhenAnyValue(fun vm -> vm.Principal).Select(fun _ -> this.Input)
                this.Input.WhenAnyValue(fun vm -> vm.InterestRate).Select(fun _ -> this.Input)
                this.Input.WhenAnyValue(fun vm -> vm.StartDate).Select(fun _ -> this.Input)
                this.Input.WhenAnyValue(fun vm -> vm.EndDate).Select(fun _ -> this.Input)
            ]).Select(fun _ -> this.Input)
        errorMessage <- inputObservable.Select(getErrorMessage).ObserveOn(RxApp.MainThreadScheduler).ToProperty(this, fun vm -> vm.ErrorMessage)
        this.CopyTextToClipboard <- ReactiveCommand.Create(copyTextToClipboard) |> disposeWith this.PageDisposables
        this.CopyTextToClipboard.Select(sprintf "Copied \"%s\" to clipboard.").ObserveOn(RxApp.MainThreadScheduler).Subscribe(platform.ShowToastNotification) |> disposeWith this.PageDisposables |> ignore
        this.Calculate <- ReactiveCommand.Create(calculateCompoundInterest this.Input, inputObservable.Select(isValid)) |> disposeWith this.PageDisposables
        this.Calculate.ObserveOn(RxApp.MainThreadScheduler).Subscribe(fun result ->
            this.TotalDue <- result.TotalDue
            this.InterestAccrued <- result.InterestAccrued
            this.ShowResults <- true) |> disposeWith this.PageDisposables |> ignore
    interface IRoutableViewModel with
        member __.HostScreen = host
        member __.UrlPathSegment = "Compound Interest Calculator"

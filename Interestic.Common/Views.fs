﻿namespace Interestic.Common

open XamarinForms.Reactive.FSharp

open Themes
open Xamarin.Forms

open UiComponentExtensions
open ViewHelpers
open Telerik.XamarinForms.Primitives
open Telerik.XamarinForms.Primitives.SideDrawer
open Telerik.XamarinForms.Input

type DashboardView(theme: Theme) =
    inherit ContentPage<DashboardViewModel, DashboardView>(theme)
    member val SideDrawer = Unchecked.defaultof<RadSideDrawer> with get, set
    member val DataForm = Unchecked.defaultof<RadDataForm> with get, set
    member val ErrorMessage = Unchecked.defaultof<Label> with get, set
    member val TotalDue = Unchecked.defaultof<Label> with get, set
    member val InterestAccrued = Unchecked.defaultof<Label> with get, set
    member val CopyTotalDue = Unchecked.defaultof<RadButton> with get, set
    member val CopyInterestAccrued = Unchecked.defaultof<RadButton> with get, set
    member val CopyAll = Unchecked.defaultof<RadButton> with get, set
    new() = new DashboardView(Themes.DefaultTheme)
    override this.CreateContent() =
        theme.GenerateSideDrawer<DashboardView>(this, <@ fun v -> v.SideDrawer @>)
            |> withMainContent(
                theme.VerticalLayout() |> withBlocks(
                    [|
                        theme.GenerateDataForm<DashboardView>(this, <@ fun v -> v.DataForm @>) 
                        |> withCommitMode CommitMode.Immediate
                        |> withDataSource this.ViewModel.Input 
                        |> withDataSourceProvider (new CompoundingProvider())
                        |> withEditorType <@ fun (d: CompoundInterestInput) -> d.Compounded @> EditorType.PickerEditor
                        theme.GenerateRadButton()
                        |> withCaption "Calculate"
                        |> withBackgroundColor Color.Green
                        |> withMargin (new Thickness(12.0))
                        |> withButtonCommand this.ViewModel.Calculate
                        theme.GenerateLabel<DashboardView>(this, <@ fun v -> v.ErrorMessage @>)
                        |> withOneWayBinding(this, <@ fun vm -> vm.ErrorMessage @>, <@ fun v -> v.ErrorMessage.Text @>, id)
                        |> withLabelTextColor Color.Red
                        |> withHorizontalOptions LayoutOptions.Center
                        |> withHorizontalTextAlignment TextAlignment.Center
                    |]))
            |> withDrawerContent (
                theme.GenerateGrid([Auto; Auto], [Auto; 1 |> Star; Auto]) |> withRow(
                    [|
                        theme.GenerateLabel() |> withLabelText "Total Due" |> withMargin (new Thickness(12.0))
                        theme.GenerateLabel<DashboardView>(this, <@ fun v -> v.TotalDue @>)
                        |> withOneWayBinding(this, <@ fun vm -> vm.TotalDue @>, <@ fun v -> v.TotalDue.Text @>, fun amount -> amount.ToString("#.00"))
                        |> withHorizontalOptions LayoutOptions.EndAndExpand
                        |> withHorizontalTextAlignment TextAlignment.End
                        |> withMargin (new Thickness(12.0))
                        theme.GenerateRadButton<DashboardView>(this, <@ fun v -> v.CopyTotalDue @>)
                        |> withCaption "Copy"
                        |> withButtonCommand this.ViewModel.CopyTextToClipboard
                        |> withHeightRequest 24.0
                        |> withBackgroundColor Color.Green
                        |> withCornerRadius 12
                        |> withButtonFontSize 12.0
                        |> withButtonPadding (new Thickness(0.0))
                        |> withMargin (new Thickness(12.0))
                        |> withOneWayBinding(this, <@ fun vm -> vm.TotalDue @>, <@ fun v -> v.CopyTotalDue.CommandParameter @>, fun f -> f.ToString("#.00") :> obj)
                    |]
                ) |> thenRow(
                    [|
                        theme.GenerateLabel() |> withLabelText "Interest Accrued" |> withMargin (new Thickness(12.0))
                        theme.GenerateLabel<DashboardView>(this, <@ fun v -> v.InterestAccrued @>)
                        |> withOneWayBinding(this, <@ fun vm -> vm.InterestAccrued @>, <@ fun v -> v.InterestAccrued.Text @>, fun amount -> amount.ToString("#.00"))
                        |> withHorizontalOptions LayoutOptions.EndAndExpand
                        |> withHorizontalTextAlignment TextAlignment.End 
                        |> withMargin (new Thickness(12.0))
                        theme.GenerateRadButton<DashboardView>(this, <@ fun v -> v.CopyInterestAccrued @>)
                        |> withCaption "Copy"
                        |> withButtonCommand this.ViewModel.CopyTextToClipboard
                        |> withHeightRequest 24.0
                        |> withBackgroundColor Color.Green
                        |> withCornerRadius 12
                        |> withButtonFontSize 12.0
                        |> withButtonPadding (new Thickness(0.0))
                        |> withMargin (new Thickness(12.0))
                        |> withOneWayBinding(this, <@ fun vm -> vm.InterestAccrued @>, <@ fun v -> v.CopyInterestAccrued.CommandParameter @>, fun f -> f.ToString("#.00") :> obj)
                    |]) |> createFromRows |> withBackgroundColor Color.DarkSlateGray |> withMargin (new Thickness(12.0))
            )
            |> withDrawerLocation SideDrawerLocation.Top
            |> withDrawerTransitionType SideDrawerTransitionType.ScaleUp
            |> withDrawerLength 130.0
            |> withTwoWayBinding (this, <@ fun vm -> vm.ShowResults @>, <@ fun v -> v.SideDrawer.IsOpen @>, id, id)
            :> View

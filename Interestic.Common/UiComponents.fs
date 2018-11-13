namespace Interestic.Common

open XamarinForms.Reactive.FSharp.Themes
open Xamarin.Forms
open XamarinForms.Reactive.FSharp
open ViewHelpers
open System

type Postcard = {
    Image: string
    Description: string
}

type PostcardDisplay(theme: Theme) =
    inherit Grid()
    let image = theme.GenerateImage()
    let descriptionLabel = 
        theme.GenerateLabel() 
            |> withHorizontalTextAlignment TextAlignment.Center 
            |> withHorizontalOptions LayoutOptions.CenterAndExpand 
    do 
        base.Children.Add image; Grid.SetRow(image, 0); Grid.SetColumn(image, 0)
        base.Children.Add descriptionLabel; Grid.SetRow(descriptionLabel, 1); Grid.SetColumn(descriptionLabel, 0)
        base.WidthRequest <- 400.0
    override this.OnBindingContextChanged() =
        base.OnBindingContextChanged()
        match box this.BindingContext with 
        | :? Postcard as postcard -> 
            image.Source <- ImageSource.FromFile postcard.Image
            descriptionLabel.Text <- postcard.Description
        | _ -> 
            image.Source <- null
            descriptionLabel.Text <- null

module ExpressionConversion =
    open Microsoft.FSharp.Quotations
    open Microsoft.FSharp.Linq.RuntimeHelpers
    open System.Linq.Expressions

    let toLinq (expr : Expr<'a -> 'b>) =
        let linq = LeafExpressionConverter.QuotationToExpression expr
        let call = linq :?> MethodCallExpression
        let lambda = call.Arguments.[0] :?> LambdaExpression
        Expression.Lambda<Func<'a, 'b>>(lambda.Body, lambda.Parameters)
    let rec propertyName = function
    | Patterns.Lambda(_, expr) -> propertyName expr
    | Patterns.PropertyGet(_, propertyInfo, _) -> propertyInfo.Name
    | _ -> failwith "You have asked for the property name of an expression that does not describe a property."
    let rec setProperty instance value = function
    | Patterns.Lambda(_, expr) -> setProperty instance value expr
    | Patterns.PropertyGet(_, propertyInfo, _) -> propertyInfo.SetValue(instance, value)
    | _ -> failwith "You have tried to set a property value using an expression that does not describe a property."

module UiComponentExtensions =
    open Telerik.XamarinForms.Primitives
    open Telerik.XamarinForms.Input

    type Theme with
        member __.GenerateDataForm([<ParamArray>] setUp: (RadDataForm -> unit)[]) = new RadDataForm() |> apply setUp
        member __.GenerateDataForm(view, property, [<ParamArray>] setUp: (RadDataForm -> unit)[]) = new RadDataForm() |> initialise property view |> apply setUp
        member __.GenerateCalendar([<ParamArray>] setUp: (RadCalendar -> unit)[]) = new RadCalendar() |> apply setUp
        member __.GenerateCalendar(view, property, [<ParamArray>] setUp: (RadCalendar -> unit)[]) = new RadCalendar() |> initialise property view |> apply setUp
        member __.GenerateSideDrawer([<ParamArray>] setUp: (RadSideDrawer -> unit)[]) = new RadSideDrawer() |> apply setUp
        member __.GenerateSideDrawer(view, property, [<ParamArray>] setUp: (RadSideDrawer -> unit)[]) = new RadSideDrawer() |> initialise property view |> apply setUp
        member __.GenerateRadButton([<ParamArray>] setUp: (RadButton -> unit)[]) = new RadButton() |> apply setUp
        member __.GenerateRadButton(view, property, [<ParamArray>] setUp: (RadButton -> unit)[]) = new RadButton() |> initialise property view |> apply setUp
        member __.GenerateSlideView([<ParamArray>] setUp: (RadSlideView -> unit)[]) = new RadSlideView() |> apply setUp
        member __.GenerateSlideView(view, property, [<ParamArray>] setUp: (RadSlideView -> unit)[]) = new RadSlideView() |> initialise property view |> apply setUp

module ViewHelpers =
    open Telerik.XamarinForms.Primitives
    open Microsoft.FSharp.Quotations
    open Telerik.XamarinForms.Input
    open ExpressionConversion

    let withCommitMode mode (element: #RadDataForm) = element.CommitMode <- mode; element
    let withDataSourceProvider provider (element: #RadDataForm) = element.PropertyDataSourceProvider <- provider; element
    let withDataSource source (element: #RadDataForm) = element.Source <- source; element
    let withEditorType (property: Expr<'vm -> 'p>) editorType (element: #RadDataForm) = element.RegisterEditor(property |> propertyName, editorType); element
    let withMainContent (content:#View) (element: #RadSideDrawer) = element.MainContent <- content; element
    let withDrawerContent (content:#View) (element: #RadSideDrawer) = element.DrawerContent <- content; element
    let withDrawerLocation location (element: #RadSideDrawer) = element.DrawerLocation <- location; element
    let withDrawerLength length (element: #RadSideDrawer) = element.DrawerLength <- length; element
    let withDrawerTransitionType transitionType (element: #RadSideDrawer) = element.DrawerTransitionType <- transitionType; element
    let withSlideViewItemsSource (source:'a seq) (element: #RadSlideView) = element.ItemsSource <- source |> Seq.cast<obj>; element
    let withSlideViewItemTemplate (createTemplate: unit -> View) (element: #RadSlideView) = element.ItemTemplate <- new DataTemplate(fun() -> createTemplate() :> obj); element
    let withSlideViewTypeTemplate<'view when 'view :> View> (element: RadSlideView) = element.ItemTemplate <- new DataTemplate(typeof<'view>); element
    let animated (element: #RadSlideView) = element.IsAnimated <- true; element
    let withHorizontalContentOptions options (element: #RadSlideView) = element.HorizontalContentOptions <- options; element
    let withVerticalContentOptions options (element: #RadSlideView) = element.VerticalContentOptions <- options; element
    let withSlideButtonsSize size (element: #RadSlideView) = element.SlideButtonsSize <- size; element
    let withSelectedIndicatorFontSize size (element: #RadSlideView) = element.SelectedIndicatorFontSize <- size; element
    let withIndicatorFontSize size (element: #RadSlideView) = element.IndicatorFontSize <- size; element

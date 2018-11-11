namespace Interestic.Android

open System.IO
open System

open Android.App
open Android.Content
open Android.OS
open Android.Runtime
open Android.Views
open Android.Widget
open Android.Content.PM
open Xamarin.Forms.Platform.Android
open Xamarin.Forms
open XamarinForms.Reactive.FSharp

type Resources = Interestic.Android.Resource

open Interestic.Common

type DroidPlatform(mainActivity: Activity) =
    static let appFolderPath = Environment.GetFolderPath(Environment.SpecialFolder.Personal)
    let metaData = mainActivity.PackageManager.GetApplicationInfo(mainActivity.PackageName, PackageInfoFlags.MetaData).MetaData
    let localFilePath fileName = Path.Combine(appFolderPath, fileName)
    interface IInteresticPlatform with
        member __.HandleAppLinkRequest(_) = raise (System.NotImplementedException())
        member __.RegisterDependencies _ = 0 |> ignore
        member __.GetLocalFilePath fileName = localFilePath fileName
        member __.GetMetaDataEntry key = metaData.GetString key

open ReactiveUI

[<Activity (Label = "Compound Interest", ScreenOrientation = ScreenOrientation.Portrait, MainLauncher = true, Icon = "@drawable/icon")>]
type MainActivity () =
    inherit FormsApplicationActivity ()
    let createDashboardViewModel() = new DashboardViewModel() :> IRoutableViewModel
    override this.OnCreate (bundle) =
        base.OnCreate (bundle)
        AppDomain.CurrentDomain.UnhandledException.Subscribe(fun ex ->
            ()
        ) |> ignore
        Forms.Init(this, bundle)
        let platform = new DroidPlatform(this) :> IInteresticPlatform
        let app = new App<IInteresticPlatform>(platform, new UiContext(this), createDashboardViewModel)
        app.Init(Themes.DefaultTheme)
        base.LoadApplication app

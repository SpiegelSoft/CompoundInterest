namespace Interestic.Common

open XamarinForms.Reactive.FSharp

type IInteresticPlatform =
    inherit IPlatform
    abstract member GetMetaDataEntry: key:string -> string
    abstract member CopyTextToClipboard: description:string -> text:string -> unit
    abstract member ShowToastNotification: text:string -> unit

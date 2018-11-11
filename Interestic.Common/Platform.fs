namespace Interestic.Common

open XamarinForms.Reactive.FSharp

type IInteresticPlatform =
    inherit IPlatform
    abstract member GetMetaDataEntry: key:string -> string



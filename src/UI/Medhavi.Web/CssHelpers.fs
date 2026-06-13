namespace Medhavi.Web

open System

[<AutoOpen>]
module Css =
    let inline cls xs = String.concat " " xs

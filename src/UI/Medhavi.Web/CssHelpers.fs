namespace Medhavi.Web

open System

type QueryScope = {
    ScenarioId: string option
    PlantId: string option
    HorizonStart: DateTime
    HorizonEnd: DateTime
}

[<AutoOpen>]
module Css = let inline cls xs = String.concat " " xs
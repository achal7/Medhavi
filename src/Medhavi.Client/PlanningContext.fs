namespace Medhavi.Client

open Medhavi.Client.Primitives

[<Struct>]
type ScenarioId = private ScenarioId of string

module ScenarioId =
    let create value =
        if System.String.IsNullOrWhiteSpace value then
            Error "Scenario id must not be empty"
        else
            Ok(ScenarioId value)

    let value (ScenarioId value) = value

    let literal value =
        match create value with
        | Ok id -> id
        | Error error -> raise(System.ArgumentException error)

    let baseline = literal "baseline"

type PlanningContext =
    {
        Scenario: ScenarioId
        Horizon: DateRange
        PlantId: string option
        ScopeId: string option
    }

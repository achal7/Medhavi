namespace Medhavi.Scenario.Application

open Medhavi.SharedKernel
open Medhavi.Scenario.Domain

type ScenarioStore =
    { Load: ScenarioId -> Async<Scenario option>
      Save: Scenario -> ScenarioEvent list -> Async<unit> }

type ScenarioService(store: ScenarioStore) =
    member _.HandleAsync
        (scenarioId: ScenarioId, command: ScenarioCommand)
        : Async<Result<ScenarioEvent list, DomainError>> =
        async {
            let! current = store.Load scenarioId

            match ScenarioAgg.handle command current with
            | Error e -> return Error e
            | Ok dec ->
                do! store.Save dec.NewState dec.Events
                return Ok dec.Events
        }

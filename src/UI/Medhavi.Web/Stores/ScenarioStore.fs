namespace Medhavi.Web.Stores

open System
open System.Threading.Tasks
open Medhavi.Web
open Medhavi.Nexus
open Medhavi.SharedKernel.ScenarioContracts

type ScenarioStore = {
    GetSnapshot : unit -> ScenarioReadModel list
    Refresh     : unit -> Task<unit>
    Subscribe   : (unit -> unit) -> IDisposable
    CreateScenario : string * ScenarioType * string option -> Task<Result<unit, string>>
    AddOverride : string * ScenarioDataOverride -> Task<Result<unit, string>>
    RemoveOverride : string * ScenarioDataOverride -> Task<Result<unit, string>>
    SubmitForApproval : string -> Task<Result<unit, string>>
    ApproveScenario : string -> Task<Result<unit, string>>
    RejectScenario : string * string -> Task<Result<unit, string>>
    PublishScenario : string * string option -> Task<Result<string, string>>
    RollbackScenario : string -> Task<Result<unit, string>>
}

module ScenarioStore =
    let create (engine: MedhaviEngine) : ScenarioStore =
        let mutable cache : ScenarioReadModel list = []
        let listeners = System.Collections.Generic.List<unit -> unit>()

        let notifySubscribers () =
            for listener in listeners do
                listener ()

        let getSnapshot () = cache

        let subscribe listener =
            listeners.Add(listener)
            { new IDisposable with
                member _.Dispose() = listeners.Remove(listener) |> ignore }

        let refresh () =
            task {
                let! scenarios = engine.GetScenarios()
                cache <- scenarios
                notifySubscribers ()
            }

        let createScenario (name, scenarioType, parentId) =
            task {
                let! res = engine.CreateScenario(name, scenarioType, parentId)
                if Result.isOk res then
                    do! refresh()
                return res
            }

        let addOverride (scenId, ov) =
            task {
                let! res = engine.AddOverride(scenId, ov)
                if Result.isOk res then
                    do! refresh()
                return res
            }

        let removeOverride (scenId, ov) =
            task {
                let! res = engine.RemoveOverride(scenId, ov)
                if Result.isOk res then
                    do! refresh()
                return res
            }

        let submitForApproval scenId =
            task {
                let! res = engine.SubmitScenarioForApproval(scenId)
                if Result.isOk res then
                    do! refresh()
                return res
            }

        let approveScenario scenId =
            task {
                let! res = engine.ApproveScenario(scenId)
                if Result.isOk res then
                    do! refresh()
                return res
            }

        let rejectScenario (scenId, reason) =
            task {
                let! res = engine.RejectScenario(scenId, reason)
                if Result.isOk res then
                    do! refresh()
                return res
            }

        let publishScenario (scenId, reason) =
            task {
                let! res = engine.PublishScenario(scenId, reason)
                if Result.isOk res then
                    do! refresh()
                return res
            }

        let rollbackScenario publishId =
            task {
                let! res = engine.RollbackScenario(publishId)
                if Result.isOk res then
                    do! refresh()
                return res
            }

        { GetSnapshot = getSnapshot
          Subscribe = subscribe
          Refresh = refresh
          CreateScenario = createScenario
          AddOverride = addOverride
          RemoveOverride = removeOverride
          SubmitForApproval = submitForApproval
          ApproveScenario = approveScenario
          RejectScenario = rejectScenario
          PublishScenario = publishScenario
          RollbackScenario = rollbackScenario }

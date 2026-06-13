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

        { GetSnapshot = getSnapshot
          Subscribe = subscribe
          Refresh = refresh
          CreateScenario = createScenario }

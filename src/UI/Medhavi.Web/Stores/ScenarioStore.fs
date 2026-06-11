namespace Medhavi.Web.Stores

open System
open System.Threading.Tasks
open Medhavi.Web
open Medhavi.Nexus
open Medhavi.Scenario

type ScenarioStore = {
    GetSnapshot : unit -> ScenarioReadModel list
    Refresh     : unit -> Task<unit>
    Subscribe   : (unit -> unit) -> IDisposable
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
                let! scenarios = engine.Scenario.Queries.GetAll()
                cache <- scenarios
                notifySubscribers ()
            }

        { GetSnapshot = getSnapshot
          Subscribe = subscribe
          Refresh = refresh }

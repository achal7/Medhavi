namespace Medhavi.Web.Stores

open System
open System.Threading.Tasks
open Medhavi.Web
open Medhavi.Nexus
open Medhavi.Contracts.Capacity

type CapacityStore = {
    GetSnapshot : unit -> OperationView list
    Refresh     : unit -> Task<unit>
    Subscribe   : (unit -> unit) -> IDisposable
    SetScope    : QueryScope -> Task<unit>
}

module CapacityStore =
    let create (engine: MedhaviEngine) : CapacityStore =
        let mutable cache : OperationView list = []
        let mutable currentScope : QueryScope = {
            ScenarioId = None
            PlantId = None
            StockingPointId = None
            HorizonStart = DateTime.Today.AddDays(-7.0)
            HorizonEnd = DateTime.Today.AddDays(90.0)
        }
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
                let! ops = engine.GetCapacityOperations(currentScope.ScenarioId)
                let filtered = 
                    ops
                    |> List.filter (fun o ->
                        let start = o.StartTime.DateTime
                        start.Date >= currentScope.HorizonStart.Date && start.Date <= currentScope.HorizonEnd.Date
                    )
                cache <- filtered
                notifySubscribers ()
            }

        let setScope scope =
            task {
                currentScope <- scope
                do! refresh()
            }

        { GetSnapshot = getSnapshot
          Subscribe = subscribe
          Refresh = refresh
          SetScope = setScope }

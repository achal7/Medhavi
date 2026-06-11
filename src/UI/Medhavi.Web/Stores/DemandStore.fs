namespace Medhavi.Web.Stores

open System
open System.Threading.Tasks
open Medhavi.Web
open Medhavi.Nexus

type DemandStore = {
    GetSnapshot : unit -> DemandViewItem list
    Refresh     : unit -> Task<unit>
    Subscribe   : (unit -> unit) -> IDisposable
    SetScope    : QueryScope -> Task<unit>
}

module DemandStore =
    let create (engine: MedhaviEngine) : DemandStore =
        let mutable cache : DemandViewItem list = []
        let mutable currentScope : QueryScope = {
            ScenarioId = None
            PlantId = None
            HorizonStart = DateTime.Today
            HorizonEnd = DateTime.Today.AddDays(30.0)
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
                try
                    printfn "[DemandStore] Starting refresh..."
                    let! demands = engine.GetDemands()
                    printfn "[DemandStore] Retrieved %d demands from engine." demands.Length
                    let filtered = 
                        demands
                        |> List.filter (fun d ->
                            let plantMatch = 
                                match currentScope.PlantId with
                                | None -> true
                                | Some pid -> d.StockingPointId.Contains(pid, StringComparison.OrdinalIgnoreCase)
                            
                            let reqDate = d.RequestedDeliveryDate.Date
                            let dateMatch = 
                                reqDate >= currentScope.HorizonStart.Date && reqDate <= currentScope.HorizonEnd.Date

                            plantMatch && dateMatch
                        )
                    printfn "[DemandStore] Filtered to %d demands." filtered.Length
                    cache <- filtered
                    notifySubscribers ()
                    printfn "[DemandStore] Subscribers notified."
                with ex ->
                    printfn "[DemandStore] Error during refresh: %s\n%s" ex.Message ex.StackTrace
                    raise ex
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

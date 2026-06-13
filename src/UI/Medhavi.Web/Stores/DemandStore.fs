namespace Medhavi.Web.Stores

open System
open System.Threading.Tasks
open Medhavi.Web
open Medhavi.Nexus
open Medhavi.Contracts.Demand

type DemandStore = {
    GetSnapshot : unit -> DemandLine list
    Refresh     : unit -> Task<unit>
    Subscribe   : (unit -> unit) -> IDisposable
    SetScope    : QueryScope -> Task<unit>
}

module DemandStore =
    let create (engine: MedhaviEngine) : DemandStore =
        let mutable cache : DemandLine list = []
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
                try
                    printfn "[DemandStore] Starting refresh..."
                    let! demands = engine.GetDemands()
                    printfn "[DemandStore] Retrieved %d demands from engine." demands.Length
                    let! stockingPoints = engine.GetStockingPoints()
                    let stockingPointToPlantMap = stockingPoints |> List.map (fun sp -> sp.Id, sp.PlantId) |> readOnlyDict
                    let filtered = 
                        demands
                        |> List.filter (fun d ->
                            let plantMatch = 
                                match currentScope.PlantId with
                                | None -> true
                                | Some pid ->
                                    match stockingPointToPlantMap.TryGetValue(d.StockingPointId) with
                                    | true, plantId -> plantId.Equals(pid, StringComparison.OrdinalIgnoreCase)
                                    | false, _ -> d.StockingPointId.Contains(pid, StringComparison.OrdinalIgnoreCase)
                            
                            let spMatch =
                                match currentScope.StockingPointId with
                                | None -> true
                                | Some spid -> d.StockingPointId.Equals(spid, StringComparison.OrdinalIgnoreCase)
                            
                            let reqDate = d.RequestedDeliveryDate
                            let dateMatch = 
                                reqDate >= DateOnly.FromDateTime(currentScope.HorizonStart) && reqDate <= DateOnly.FromDateTime(currentScope.HorizonEnd)

                            plantMatch && spMatch && dateMatch
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

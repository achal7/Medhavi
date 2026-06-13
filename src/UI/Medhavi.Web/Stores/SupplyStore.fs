namespace Medhavi.Web.Stores

open System
open System.Threading.Tasks
open Medhavi.Web
open Medhavi.Nexus
open Medhavi.Contracts.Supply

type SupplyStore = {
    GetSnapshot : unit -> SupplyOrder list
    Refresh     : unit -> Task<unit>
    Subscribe   : (unit -> unit) -> IDisposable
    SetScope    : QueryScope -> Task<unit>
}

module SupplyStore =
    let create (engine: MedhaviEngine) : SupplyStore =
        let mutable cache : SupplyOrder list = []
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
                    printfn "[SupplyStore] Starting refresh..."
                    let! orders = engine.GetSupplyOrders()
                    printfn "[SupplyStore] engine.GetSupplyOrders() returned %d orders" orders.Length
                    let! stockingPoints = engine.GetStockingPoints()
                    let stockingPointToPlantMap = stockingPoints |> List.map (fun sp -> sp.Id, sp.PlantId) |> readOnlyDict
                    let filtered = 
                        orders
                        |> List.filter (fun o ->
                            let plantMatch = 
                                match currentScope.PlantId with
                                | None -> true
                                | Some pid -> 
                                    match stockingPointToPlantMap.TryGetValue(o.StockingPointId) with
                                    | true, plantId -> plantId.Equals(pid, StringComparison.OrdinalIgnoreCase)
                                    | false, _ -> o.StockingPointId.Contains(pid, StringComparison.OrdinalIgnoreCase)
                            
                            let spMatch =
                                match currentScope.StockingPointId with
                                | None -> true
                                | Some spid -> o.StockingPointId.Equals(spid, StringComparison.OrdinalIgnoreCase)
                            
                            let d = o.RequiredDeliveryDate |> Option.map (fun dt -> DateOnly.FromDateTime(dt.DateTime)) |> Option.defaultValue (DateOnly.FromDateTime(DateTime.Today))
                            let dateMatch = 
                                d >= DateOnly.FromDateTime(currentScope.HorizonStart) && d <= DateOnly.FromDateTime(currentScope.HorizonEnd)
                            plantMatch && spMatch && dateMatch
                        )
                    printfn "[SupplyStore] Filtered to %d orders." filtered.Length
                    cache <- filtered
                    notifySubscribers ()
                    printfn "[SupplyStore] Subscribers notified."
                with ex ->
                    printfn "[SupplyStore] Error during refresh: %s\n%s" ex.Message ex.StackTrace
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

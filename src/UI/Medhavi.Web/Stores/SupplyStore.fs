namespace Medhavi.Web.Stores

open System
open System.Threading.Tasks
open Medhavi.Web
open Medhavi.Nexus
open Medhavi.Analytics.PlanningHorizon

type SupplyStore = {
    GetSnapshot : unit -> SupplyElementView list
    Refresh     : unit -> Task<unit>
    Subscribe   : (unit -> unit) -> IDisposable
    SetScope    : QueryScope -> Task<unit>
}

module SupplyStore =
    let create (engine: MedhaviEngine) : SupplyStore =
        let mutable cache : SupplyElementView list = []
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
                    printfn "[SupplyStore] Starting refresh..."
                    let! orders = engine.Supply.Queries.SupplyOrder.GetAll()
                    printfn "[SupplyStore] engine.Supply.Queries.SupplyOrder.GetAll() returned %d orders" orders.Length
                    let filtered = 
                        orders
                        |> List.filter (fun o ->
                            let plantMatch = 
                                match currentScope.PlantId with
                                | None -> true
                                | Some pid -> o.StockingPointId.Contains(pid, StringComparison.OrdinalIgnoreCase)
                            
                            let dateMatch = 
                                match o.RequiredDeliveryDate with
                                | None -> true
                                | Some rdd -> 
                                    let d = rdd.Date
                                    d >= currentScope.HorizonStart.Date && d <= currentScope.HorizonEnd.Date
                            plantMatch && dateMatch
                        )
                    printfn "[SupplyStore] Filtered to %d orders." filtered.Length
                    let mapped =
                        filtered
                        |> List.map (fun o ->
                            { SupplyElementView.SupplyOrderId = o.Id
                              SupplyType = 
                                match o.OrderType.ToLower() with
                                | "workorder"
                                | "plannedworkorder" -> PlannedProductionOrder
                                | "purchaseorder"
                                | "plannedpurchaseorder" -> PlannedPurchaseOrder
                                | _ -> PlannedProductionOrder
                              SkuId = o.SkuId
                              SkuCode = o.SkuId
                              StockingPointId = o.StockingPointId
                              PlannedQty = o.Quantity
                              ConfirmedQty = o.CompletedQuantity
                              PlannedDate = 
                                o.RequiredDeliveryDate
                                |> Option.map (fun d -> DateOnly.FromDateTime(d.DateTime))
                                |> Option.defaultValue (DateOnly.FromDateTime(DateTime.Today))
                              IsFirm = o.IsFirm
                              IsLocked = o.IsLocked
                              IsExpedited = o.IsExpedited
                              RoutingId = o.RoutingId
                              SupplierId = o.SupplierId
                              LeadTimeDays = None })
                    cache <- mapped
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

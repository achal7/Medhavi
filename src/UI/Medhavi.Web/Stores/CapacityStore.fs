namespace Medhavi.Web.Stores

open System
open System.Threading.Tasks
open Medhavi.Web
open Medhavi.Nexus
open Medhavi.Analytics.PlanningHorizon
open Medhavi.SharedKernel
open Medhavi.Capacity.Domain.OperationAgg

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
                let! ops = engine.Capacity.OperationAgent.GetStateAsync()
                let filtered = 
                    ops.Values
                    |> Seq.filter (fun o ->
                        let start = (Timestamp.value o.Window.Start).DateTime
                        start.Date >= currentScope.HorizonStart.Date && start.Date <= currentScope.HorizonEnd.Date
                    )
                    |> Seq.map (fun o ->
                        { OperationView.OperationId = OperationId.value o.Id
                          WorkOrderId = None
                          SkuId = ""
                          SkuCode = ""
                          RoutingStepId = RoutingStepId.value o.RoutingStepId
                          OperationCode = RoutingStepId.value o.RoutingStepId
                          Quantity = 0m
                          SetupMinutes = 0m
                          RunMinutes =
                            o.Duration
                            |> Option.map (fun d -> decimal d.TotalMinutes)
                            |> Option.defaultValue 0m
                          StartTime = Timestamp.value o.Window.Start
                          EndTime =
                            o.Window.End
                            |> Option.map Timestamp.value
                            |> Option.defaultValue (Timestamp.value o.Window.Start)
                          Status =
                            match o.State with
                            | Scheduled -> OperationStatus.Planned
                            | InProgress -> OperationStatus.InProgress
                            | Completed -> OperationStatus.Completed
                            | Cancelled -> OperationStatus.Cancelled
                          DemandOrderId = None
                          PeggedDemandQty = None
                          IsFirm = o.IsFixed
                          IsFrozen = false
                          IsExpedited = false })
                    |> Seq.toList
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

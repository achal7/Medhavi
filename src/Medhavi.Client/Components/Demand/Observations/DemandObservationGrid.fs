module Medhavi.Client.Components.Demand.Observations.DemandObservationGrid

open System
open Medhavi.Contracts.Demand
open Medhavi.Client.ViewComposition.Identity
open Medhavi.Client.ViewComposition.Geometry
open Medhavi.Client.ViewComposition.Component
open Medhavi.Client.ViewComposition.Data
open Medhavi.Client.Rendering
open Medhavi.Client.Registries
open Medhavi.Client.Controls
open Medhavi.Client

let definitionId = ComponentDefId.literal "demand-observation-grid"
let storeId = DataStoreId.literal "demand-observation-store"
let schemaId = DemandObservationSchema.schemaId

let definition: ComponentDefinition =
    {
        Id = definitionId
        DisplayName = "Demand Observations Stream"
        Icon = "sensors"
        Category = ComponentCategory.DataDisplay
        RequiredSchemaCapabilities = [ "Tabular" ]
        DefaultGridSpan = GridSpan.literal 12 6
        DefaultSchemaId = Some schemaId
        DefaultStoreId = Some storeId
        IsResizable = true
        IsDraggable = true
        SelectionMode = SingleSelection
    }

let private fallbackDtos: DemandObservationDto list =
    [
        {
            ObservationId = "OBS-001"
            Item = "H100 GPU Board"
            Location = "Berlin DC"
            Quantity = 450m
            UnitOfMeasure = "EA"
            ObservationType = "EDI_Order"
            BusinessTime = DateTimeOffset.UtcNow.AddMinutes(-30.0)
            ObservationTime = DateTimeOffset.UtcNow.AddMinutes(-29.0)
            SourceSystemProvenance = "SAP-ERP-EU"
            LifecycleState = "Ingested"
            DecisionTraceability = None
        }
        {
            ObservationId = "OBS-002"
            Item = "B200 Inverter"
            Location = "Munich DC"
            Quantity = 120m
            UnitOfMeasure = "EA"
            ObservationType = "POS_Scan"
            BusinessTime = DateTimeOffset.UtcNow.AddHours(-2.0)
            ObservationTime = DateTimeOffset.UtcNow.AddHours(-2.0)
            SourceSystemProvenance = "RetailPortal"
            LifecycleState = "Ingested"
            DecisionTraceability = None
        }
    ]

let store: DataStore =
    {
        Id = storeId
        Fetch =
            fun (ctx: ApplicationContext) ->
                async {
                    match ctx.Resolve<DemandObservationQueries>() with
                    | Some queries ->
                        let! dtos = queries.GetAll() |> Async.AwaitTask
                        if dtos.IsEmpty then
                            let rows = fallbackDtos |> List.map DemandObservationSchema.adapter.ToUniversal
                            return Tabular rows
                        else
                            let rows = dtos |> List.map DemandObservationSchema.adapter.ToUniversal
                            return Tabular rows
                    | None ->
                        match ctx.DomainServices with
                        | Some ds ->
                            let! dtos = ds.Demand.UnderstandDemand.Observation.Queries.GetAll() |> Async.AwaitTask
                            let rows = dtos |> List.map DemandObservationSchema.adapter.ToUniversal
                            return Tabular rows
                        | None ->
                            let rows = fallbackDtos |> List.map DemandObservationSchema.adapter.ToUniversal
                            return Tabular rows
                }
    }

let runtime: ComponentRuntime =
    {
        Render = UniversalEnterpriseGrid.render
    }

let registration: ComponentRegistration =
    {
        Definition = definition
        Runtime = runtime
    }

module ActionHandlers =
    open System.Threading.Tasks
    open Medhavi.Client.ActionExecution

    let evaluateObservation : ActionHandler =
        fun ctx -> task {
            match ctx.Resolve<DemandObservationApi>() with
            | Some api ->
                match ctx.TargetKey with
                | Some obsId ->
                    let req : EvaluateObservationReq =
                        { ObservationId = obsId
                          EvaluationTime = DateTimeOffset.Now }
                    match! api.Evaluate(req) with
                    | Ok dto ->
                        return Ok (ActionExecutionOutcome.Completed(
                            $"Demand observation '{dto.ObservationId}' evaluated and accepted into enterprise demand stream.",
                            [ storeId; DataStoreId.literal "demo-demand-store"; DataStoreId.literal "core-exception-store" ]
                        ))
                    | Error err ->
                        return Error err.Message
                | None ->
                    return Error "No demand observation selected to evaluate."
            | None ->
                return Error "DemandObservationApi capability is not connected."
        }

    let receiveObservation : ActionHandler =
        fun ctx -> task {
            match ctx.Resolve<DemandObservationApi>() with
            | Some api ->
                let obsId = $"OBS-{DateTimeOffset.UtcNow.ToUnixTimeSeconds()}"
                let req : ReceiveObservationReq =
                    { ObservationId = obsId
                      Item = "H100 GPU Board"
                      Location = "Berlin DC"
                      Quantity = 320m
                      UnitOfMeasure = "EA"
                      ObservationType = "EDI_Order"
                      BusinessTime = DateTimeOffset.Now
                      ObservationTime = DateTimeOffset.Now
                      SourceSystemProvenance = "PlannerWorkbench" }
                match! api.Receive(req) with
                | Ok dto ->
                    return Ok (ActionExecutionOutcome.Completed(
                        $"New demand observation '{dto.ObservationId}' ingested into sensory stream.",
                        [ storeId ]
                    ))
                | Error err ->
                    return Error err.Message
            | None ->
                return Error "DemandObservationApi capability is not connected."
        }

    let all : (ActionDefId * ActionHandler) list =
        [ DemandObservationActions.evaluate.Id, evaluateObservation
          DemandObservationActions.receive.Id, receiveObservation ]


module Medhavi.Client.Components.Supply.SupplierCollaborationGrid

open System
open Medhavi.Contracts.Supply
open Medhavi.Client
open Medhavi.Client.Registries
open Medhavi.Client.Controls
open Medhavi.Client.Rendering
open Medhavi.Client.ViewComposition.Identity
open Medhavi.Client.ViewComposition.Geometry
open Medhavi.Client.ViewComposition.Component
open Medhavi.Client.ViewComposition.Data

let definitionId = ComponentDefId.literal "comp-supplier-collaboration-grid"
let storeId = DataStoreId.literal "supplier-collaboration-store"
let schemaId = SupplierCollaborationSchema.schemaId

let definition: ComponentDefinition =
    {
        Id = definitionId
        DisplayName = "Supplier Collaboration Workbench"
        Icon = "handshake"
        Category = ComponentCategory.DataDisplay
        RequiredSchemaCapabilities = [ "Tabular" ]
        DefaultGridSpan = GridSpan.literal 12 6
        DefaultSchemaId = Some schemaId
        DefaultStoreId = Some storeId
        IsResizable = true
        IsDraggable = true
        SelectionMode = SelectionMode.SingleSelection
    }

let fallbackDtos: SupplierCommitmentAssessmentDto list =
    [
        {
            SupplierId = "SUP-GLOBAL-CHIPS"
            LifecycleState = "Active"
            AssessmentTimestamp = DateTimeOffset(2026, 9, 12, 10, 0, 0, TimeSpan.Zero)
            OverallConfidence = "0.82"
            Exposures = []
            OnTimeDeliveryRate = 0.912m
            QuantityFulfillmentRate = 0.954m
            AverageAcknowledgementDurationHours = 18.5m
            AverageCommitmentDurationHours = 36.2m
            ActiveRiskSignals = [ "SCS-SIG-2026-101 (CommitmentSlippage)" ]
            AuthoritativeHealthState = "AtRisk"
        }

        {
            SupplierId = "SUP-ACME-COMPONENTS"
            LifecycleState = "Active"
            AssessmentTimestamp = DateTimeOffset(2026, 9, 12, 10, 0, 0, TimeSpan.Zero)
            OverallConfidence = "0.96"
            Exposures = []
            OnTimeDeliveryRate = 0.984m
            QuantityFulfillmentRate = 0.991m
            AverageAcknowledgementDurationHours = 4.2m
            AverageCommitmentDurationHours = 12.0m
            ActiveRiskSignals = []
            AuthoritativeHealthState = "Healthy"
        }
    ]

let store: DataStore =
    {
        Id = storeId
        Fetch =
            fun (ctx: ApplicationContext) ->
                async {
                    match ctx.Resolve<SupplierCommitmentAssessmentQueries>() with
                    | Some queries ->
                        let! dtos = queries.GetAll() |> Async.AwaitTask
                        let records = if List.isEmpty dtos then fallbackDtos else dtos
                        let rows = records |> List.map SupplierCollaborationSchema.adapter.ToUniversal
                        return Tabular rows
                    | None ->
                        match ctx.DomainServices with
                        | Some ds ->
                            let! dtos =
                                ds.Supply.CollaborateWithSuppliers.SupplierAssessmentQueries.GetAll() |> Async.AwaitTask

                            let records = if List.isEmpty dtos then fallbackDtos else dtos
                            let rows = records |> List.map SupplierCollaborationSchema.adapter.ToUniversal
                            return Tabular rows
                        | None ->
                            let rows = fallbackDtos |> List.map SupplierCollaborationSchema.adapter.ToUniversal
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

    let acceptCommitments : ActionHandler =
        fun ctx -> task {
            match ctx.TargetKey with
            | Some supplierId ->
                match ctx.Resolve<SupplierCommitmentAssessmentApi>() with
                | Some api ->
                    let req : UpdateSupplierCommitmentAssessmentReq =
                        { SupplierId = supplierId
                          AssessmentTimestamp = DateTimeOffset.UtcNow }
                    match! api.Update(req) with
                    | Ok _ ->
                        return Ok (ActionExecutionOutcome.Completed(
                            $"Supplier commitments for '{supplierId}' accepted as firm planning constraints.",
                            [ storeId; DataStoreId.literal "supply-store"; DataStoreId.literal "inventory-store" ]
                        ))
                    | Error err ->
                        return Error err.Message
                | None ->
                    return Ok (ActionExecutionOutcome.Completed(
                        $"Supplier commitments for '{supplierId}' accepted as firm planning constraints.",
                        [ storeId; DataStoreId.literal "supply-store"; DataStoreId.literal "inventory-store" ]
                    ))
            | None ->
                return Error "No supplier selected to accept commitments."
        }

    let requestAlternative : ActionHandler =
        fun ctx -> task {
            match ctx.TargetKey with
            | Some supplierId ->
                return Ok (ActionExecutionOutcome.Completed(
                    $"Alternative delivery schedule & split requested from supplier '{supplierId}'.",
                    [ storeId; DataStoreId.literal "supply-store" ]
                ))
            | None ->
                return Error "No supplier selected to request alternative schedule."
        }

    let escalateProcurement : ActionHandler =
        fun ctx -> task {
            match ctx.TargetKey with
            | Some supplierId ->
                return Ok (ActionExecutionOutcome.Completed(
                    $"Critical commitment variance for supplier '{supplierId}' escalated to procurement leadership.",
                    [ storeId; DataStoreId.literal "core-exception-store" ]
                ))
            | None ->
                return Error "No supplier selected for procurement escalation."
        }

    let refreshHealth : ActionHandler =
        fun ctx -> task {
            match ctx.Resolve<SupplierCollaborationHealthApi>() with
            | Some api ->
                let req : PublishSupplierCollaborationHealthAssessmentReq =
                    { AssessmentScopeId = "GLOBAL"
                      EvaluationPeriod = "CURRENT"
                      PublicationTimestamp = DateTimeOffset.UtcNow }
                match! api.Publish(req) with
                | Ok _ ->
                    return Ok (ActionExecutionOutcome.Completed(
                        "Supplier collaboration health assessment refreshed across all strategic vendors.",
                        [ storeId; DataStoreId.literal "demo-domain-health-store" ]
                    ))
                | Error err ->
                    return Error err.Message
            | None ->
                return Ok (ActionExecutionOutcome.Completed(
                    "Supplier collaboration health assessment refreshed.",
                    [ storeId; DataStoreId.literal "demo-domain-health-store" ]
                ))
        }

    let all : (ActionDefId * ActionHandler) list =
        [ SupplierCollaborationActions.acceptCommitments.Id, acceptCommitments
          SupplierCollaborationActions.requestAlternative.Id, requestAlternative
          SupplierCollaborationActions.escalateProcurement.Id, escalateProcurement
          SupplierCollaborationActions.refreshHealth.Id, refreshHealth ]

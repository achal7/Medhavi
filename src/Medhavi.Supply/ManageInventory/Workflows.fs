/// CA-S-003 Manage Inventory Workflows
/// Traces to: BW-S-020, BW-S-021, BW-S-022, FS-S-020, FS-S-021, FS-S-022
module Medhavi.Supply.ManageInventory.Workflows

open System
open System.Threading
open System.Threading.Tasks
open Medhavi.Common
open Medhavi.Foundation.Contracts
open Medhavi.Foundation.Failure
open Medhavi.Contracts.Supply
open Medhavi.Supply
open Medhavi.Supply.ArsIdentifiers

type ManageInventoryWorkflowDependencies =
    { Subscribe: EnvelopeFilter -> (Envelope -> Task<unit>) -> CancellationToken -> Task<IDisposable>
      DispatchEnvelope: Envelope -> Task<unit>
      Apis: ManageInventoryApis
      DefaultScopeId: string
      DefaultLocationId: string
      DefaultPeriodId: string }

/// FS-S-020 / BW-S-020: Triggered when Supply Understanding (BN-S-001) or Supply Plan (BN-S-010) is published
let createPositionUpdateWorkflow
    (deps: ManageInventoryWorkflowDependencies)
    (ct: CancellationToken)
    : TaskResult<IDisposable, ApplicationError> =
    task {
        let processEnvelope (envelope: Envelope) : Task<unit> =
            task {
                let parts = if envelope.AggregateId.Contains("#") then envelope.AggregateId.Split('#') else [| envelope.AggregateId; deps.DefaultLocationId |]
                let itemId = parts.[0]
                let locId = if parts.Length > 1 then parts.[1] else deps.DefaultLocationId

                let req: UpdateInventoryPositionReq =
                    { ItemId = itemId
                      LocationId = locId
                      TriggeringEvidence = [ EventId.toString envelope.EventId; envelope.EventType ] }

                let! res = deps.Apis.Position.Update req
                match res with
                | Ok _ -> ()
                | Error _ -> ()
            }

        let filter =
            [ ArsIdentifiers.BusinessNotifications.supplyUnderstandingPublished.Id
              ArsIdentifiers.BusinessNotifications.supplyPlanPublished.Id ]
            |> EnvelopeFilter.EventTypes

        try
            let! sub = deps.Subscribe filter processEnvelope ct
            return Ok sub
        with ex ->
            return Error (Infrastructure(EventStore($"Failed to subscribe position update workflow: {ex.Message}")))
    }

/// FS-S-021 / BW-S-021: Triggered when Supply Plan (BN-S-010) or Forecast (BN-D-011) is published
let createPolicyUpdateWorkflow
    (deps: ManageInventoryWorkflowDependencies)
    (ct: CancellationToken)
    : TaskResult<IDisposable, ApplicationError> =
    task {
        let processEnvelope (envelope: Envelope) : Task<unit> =
            task {
                let parts = if envelope.AggregateId.Contains("#") then envelope.AggregateId.Split('#') else [| envelope.AggregateId; deps.DefaultLocationId |]
                let itemId = parts.[0]
                let locId = if parts.Length > 1 then parts.[1] else deps.DefaultLocationId

                let req: UpdateInventoryPolicyReq =
                    { ItemId = itemId
                      LocationId = locId
                      TargetServiceLevel = None
                      ManualOverrideParameters = None }

                let! res = deps.Apis.Policy.Update req
                match res with
                | Ok _ -> ()
                | Error _ -> ()
            }

        let filter =
            [ ArsIdentifiers.BusinessNotifications.supplyPlanPublished.Id
              ArsIdentifiers.BusinessNotifications.forecastPublished.Id ]
            |> EnvelopeFilter.EventTypes

        try
            let! sub = deps.Subscribe filter processEnvelope ct
            return Ok sub
        with ex ->
            return Error (Infrastructure(EventStore($"Failed to subscribe policy update workflow: {ex.Message}")))
    }

/// FS-S-022 / BW-S-022: Periodic inventory health publication workflow
let createHealthAssessmentWorkflow
    (deps: ManageInventoryWorkflowDependencies)
    (ct: CancellationToken)
    : TaskResult<IDisposable, ApplicationError> =
    task {
        let processEnvelope (envelope: Envelope) : Task<unit> =
            task {
                let req: PublishInventoryHealthReq =
                    { AssessmentScopeId = deps.DefaultScopeId
                      EvaluationPeriodId = deps.DefaultPeriodId }

                let! res = deps.Apis.Health.Publish req
                match res with
                | Ok _ -> ()
                | Error _ -> ()
            }

        let filter =
            [ ArsIdentifiers.BusinessNotifications.supplyPlanPublished.Id ]
            |> EnvelopeFilter.EventTypes

        try
            let! sub = deps.Subscribe filter processEnvelope ct
            return Ok sub
        with ex ->
            return Error (Infrastructure(EventStore($"Failed to subscribe health assessment workflow: {ex.Message}")))
    }

/// Notification Node: Dispatches BN-S-020, BN-S-021, BN-S-022 strictly after their corresponding events exist
let createNotificationNodeWorkflow
    (deps: ManageInventoryWorkflowDependencies)
    (ct: CancellationToken)
    : TaskResult<IDisposable, ApplicationError> =
    task {
        let processEnvelope (envelope: Envelope) : Task<unit> =
            task {
                if envelope.EventType = ArsIdentifiers.EnterpriseEvents.inventoryPositionAssessmentChanged.Id then
                    let parts = if envelope.AggregateId.Contains("#") then envelope.AggregateId.Split('#') else [| envelope.AggregateId; deps.DefaultLocationId |]
                    let itemId = parts.[0]
                    let locId = if parts.Length > 1 then parts.[1] else deps.DefaultLocationId

                    let notif: InventoryPositionAssessmentChangedNotification =
                        { ItemId = itemId
                          LocationId = locId
                          HealthStatus = "HEALTHY"
                          RiskLevel = "LOW"
                          CoverageStatus = "ADEQUATE"
                          ChangeTimestamp = envelope.CreatedUtc
                          ReasonCode = "PositionUpdated" }

                    let! _ =
                        dispatchNotification
                            deps.DispatchEnvelope
                            ArsIdentifiers.BusinessNotifications.inventoryPositionAssessmentChanged.Id
                            ArsIdentifiers.Capabilities.manageInventory.Id
                            ArsIdentifiers.SemanticObjects.inventoryPositionAssessment.Id
                            envelope.AggregateId
                            notif
                    ()
                elif envelope.EventType = ArsIdentifiers.EnterpriseEvents.inventoryPolicyAssignmentUpdated.Id then
                    let parts = if envelope.AggregateId.Contains("#") then envelope.AggregateId.Split('#') else [| envelope.AggregateId; deps.DefaultLocationId |]
                    let itemId = parts.[0]
                    let locId = if parts.Length > 1 then parts.[1] else deps.DefaultLocationId

                    let notif: InventoryPolicyAssignmentUpdatedNotification =
                        { ItemId = itemId
                          LocationId = locId
                          ReplenishmentStrategy = "DYNAMIC_BUFFER"
                          SafetyStockQuantity = 0.0m
                          ReorderPointQuantity = Some 0.0m
                          AssignmentTimestamp = envelope.CreatedUtc }

                    let! _ =
                        dispatchNotification
                            deps.DispatchEnvelope
                            ArsIdentifiers.BusinessNotifications.inventoryPolicyAssignmentUpdated.Id
                            ArsIdentifiers.Capabilities.manageInventory.Id
                            ArsIdentifiers.SemanticObjects.inventoryPolicyAssignment.Id
                            envelope.AggregateId
                            notif
                    ()
                elif envelope.EventType = ArsIdentifiers.EnterpriseEvents.inventoryHealthAssessmentPublished.Id then
                    let parts = if envelope.AggregateId.Contains("#") then envelope.AggregateId.Split('#') else [| envelope.AggregateId; deps.DefaultPeriodId |]
                    let scopeId = parts.[0]
                    let periodId = if parts.Length > 1 then parts.[1] else deps.DefaultPeriodId

                    let notif: InventoryHealthAssessmentPublishedNotification =
                        { AssessmentScopeId = scopeId
                          EvaluationPeriodId = periodId
                          VersionNumber = int envelope.OccurrenceNumber
                          TotalExcessValue = 0.0m
                          TotalStockoutRiskExposure = 0.0m
                          PublicationTimestamp = envelope.CreatedUtc }

                    let! _ =
                        dispatchNotification
                            deps.DispatchEnvelope
                            ArsIdentifiers.BusinessNotifications.inventoryHealthAssessmentPublished.Id
                            ArsIdentifiers.Capabilities.manageInventory.Id
                            ArsIdentifiers.SemanticObjects.inventoryHealthAssessment.Id
                            envelope.AggregateId
                            notif
                    ()
            }

        let filter =
            [ ArsIdentifiers.EnterpriseEvents.inventoryPositionAssessmentChanged.Id
              ArsIdentifiers.EnterpriseEvents.inventoryPolicyAssignmentUpdated.Id
              ArsIdentifiers.EnterpriseEvents.inventoryHealthAssessmentPublished.Id ]
            |> EnvelopeFilter.EventTypes

        try
            let! sub = deps.Subscribe filter processEnvelope ct
            return Ok sub
        with ex ->
            return Error (Infrastructure(EventStore($"Failed to subscribe inventory notification node workflow: {ex.Message}")))
    }

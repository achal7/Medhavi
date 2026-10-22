/// BW-S-085 / FS-S-085 — Sense Supply Changes Business Workflow
/// Traces to: CR-S-021, BW-S-085, FS-S-085
module Medhavi.Supply.SenseSupplyChanges.Workflows

open System
open System.Threading
open System.Threading.Tasks
open Medhavi.Common
open Medhavi.Foundation.Contracts
open Medhavi.Foundation.Failure
open Medhavi.Contracts.Supply
open Medhavi.Supply
open Medhavi.Supply.ArsIdentifiers
open Capabilities

type SenseSupplyChangesWorkflowDependencies =
    { Subscribe: EnvelopeFilter -> (Envelope -> Task<unit>) -> CancellationToken -> Task<IDisposable>
      DispatchEnvelope: Envelope -> Task<unit>
      Apis: SenseSupplyChangesApis }

/// FS-S-085 / BW-S-085: Senses incoming supply change events & telemetry pings
let createSupplyChangeSensingWorkflow
    (deps: SenseSupplyChangesWorkflowDependencies)
    (ct: CancellationToken)
    : TaskResult<IDisposable, ApplicationError> =
    task {
        let processEnvelope (envelope: Envelope) : Task<unit> =
            task {
                let entityId = envelope.AggregateId
                if not (String.IsNullOrWhiteSpace entityId) then
                    let entityType, changeType =
                        if envelope.EventType = BusinessNotifications.supplierCommitmentChanged.Id
                           || envelope.EventType = EnterpriseEvents.supplierCommitmentChanged.Id then
                            "Supplier", "DeliveryDelay"
                        elif envelope.EventType = BusinessNotifications.capacityPositionAssessmentChanged.Id
                             || envelope.EventType = EnterpriseEvents.capacityPositionAssessmentChanged.Id then
                            "PhysicalResource", "CapacityReduction"
                        elif envelope.EventType = BusinessNotifications.supplyUnderstandingPublished.Id
                             || envelope.EventType = EnterpriseEvents.supplyUnderstandingPublished.Id then
                            "Item", "QuantityReduction"
                        else
                            "Supplier", "QuantityReduction"

                    let assessmentId = sprintf "SCA-%s-%d" entityId envelope.OccurrenceNumber

                    let req: AssessSupplyChangeReq =
                        { AssessmentId = assessmentId
                          MonitoredEntityType = entityType
                          MonitoredEntityId = entityId
                          ChangeType = changeType
                          SignalId = sprintf "SIG-%s" (envelope.EventId.ToString())
                          SourceId = envelope.CapabilityId
                          SourceType = "EnterpriseTelemetry"
                          SourceReliabilityPrior = 0.95m
                          ObservedQuantity = 0.0m
                          ObservedDate = DateTimeOffset.UtcNow
                          SignalGenerationTime = DateTimeOffset.UtcNow
                          Classification = "Planned"
                          ExpectedDuration = None
                          CorrelatedChangeIds = []
                          Timestamp = DateTimeOffset.UtcNow }

                    let! res = deps.Apis.Assess req
                    match res with
                    | Ok _ -> ()
                    | Error _ -> ()
                else
                    ()
            }

        let filter =
            [ BusinessNotifications.supplyUnderstandingPublished.Id
              BusinessNotifications.supplierCommitmentChanged.Id
              BusinessNotifications.capacityPositionAssessmentChanged.Id
              EnterpriseEvents.supplyUnderstandingPublished.Id
              EnterpriseEvents.supplierCommitmentChanged.Id
              EnterpriseEvents.capacityPositionAssessmentChanged.Id ]
            |> EnvelopeFilter.EventTypes

        try
            let! sub = deps.Subscribe filter processEnvelope ct
            return Ok sub
        with ex ->
            return Error (Infrastructure(EventStore($"Failed to subscribe supply change sensing workflow: {ex.Message}")))
    }

/// BW-S-085 Notification Node: Dispatches BN-S-080 strictly after EV-S-080 exists
let createNotificationNodeWorkflow
    (deps: SenseSupplyChangesWorkflowDependencies)
    (ct: CancellationToken)
    : TaskResult<IDisposable, ApplicationError> =
    task {
        let processEnvelope (envelope: Envelope) : Task<unit> =
            task {
                if envelope.EventType = EnterpriseEvents.supplyChangeAssessmentUpdated.Id then
                    let notif: SupplyChangeAssessmentUpdatedNotification =
                        { NotificationId = Guid.NewGuid().ToString()
                          ReferencedEventId = EnterpriseEvents.supplyChangeAssessmentUpdated.Id
                          SupplyEntityType = "Supplier"
                          SupplyEntityIdentifier = envelope.AggregateId
                          ChangeType = "ChangeDetected"
                          ChangeState = "Active"
                          Materiality = "MaterialToPlanning"
                          Confidence = "High"
                          ChangeDescription = "Supply change detected and assessed"
                          ChangeClassification = "Planned"
                          CorrelatedChangeIds = []
                          Timestamp = envelope.CreatedUtc }

                    let! _ =
                        dispatchNotification
                            deps.DispatchEnvelope
                            BusinessNotifications.supplyChangeAssessmentUpdatedNotification.Id
                            Capabilities.senseSupplyChanges.Id
                            "SupplyChangeAssessment"
                            envelope.AggregateId
                            notif
                    ()
            }

        let filter =
            [ EnterpriseEvents.supplyChangeAssessmentUpdated.Id ]
            |> EnvelopeFilter.EventTypes

        try
            let! sub = deps.Subscribe filter processEnvelope ct
            return Ok sub
        with ex ->
            return Error (Infrastructure(EventStore($"Failed to subscribe supply change notification node workflow: {ex.Message}")))
    }

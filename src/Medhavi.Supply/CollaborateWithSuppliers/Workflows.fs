namespace Medhavi.Supply.CollaborateWithSuppliers

open System
open System.Threading
open System.Threading.Tasks
open Medhavi.Common
open Medhavi.Contracts.Supply
open Medhavi.Foundation.Contracts
open Medhavi.Foundation.Failure
open Medhavi.Supply
open Medhavi.Supply.ArsIdentifiers

type CollaborateWithSuppliersWorkflowDependencies =
    { Subscribe: EnvelopeFilter -> (Envelope -> Task<unit>) -> CancellationToken -> Task<IDisposable>
      DispatchEnvelope: Envelope -> Task<unit>
      Apis: CollaborateWithSuppliersApis
      DefaultSupplierId: string
      DefaultItemId: string
      DefaultScopeId: string
      DefaultPeriodId: string }

/// CA-S-005 Collaborate with Suppliers Workflows
/// Traces to: BW-S-050, BW-S-051, FS-S-050, FS-S-051
module Workflows =

    /// FS-S-050 / BW-S-050: Supplier commitment collaboration workflow
    /// Subscribes to supply plan changes (BN-S-010), inventory position changes (BN-S-020),
    /// and commitment changes (BN-S-040) to maintain commitment assessments
    let createCommitmentCollaborationWorkflow
        (deps: CollaborateWithSuppliersWorkflowDependencies)
        (ct: CancellationToken)
        : TaskResult<IDisposable, ApplicationError> =
        task {
            let processEnvelope (envelope: Envelope) : Task<unit> =
                task {
                    let supplierId =
                        if not (String.IsNullOrWhiteSpace envelope.AggregateId) && envelope.AggregateId.Contains("#") then
                            envelope.AggregateId.Split('#')[0]
                        elif not (String.IsNullOrWhiteSpace envelope.AggregateId) then
                            envelope.AggregateId
                        else
                            deps.DefaultSupplierId

                    let req: UpdateSupplierCommitmentAssessmentReq =
                        { SupplierId = supplierId
                          AssessmentTimestamp = envelope.CreatedUtc }

                    let! res = deps.Apis.Assessment.Update req
                    match res with
                    | Ok _ -> ()
                    | Error _ -> ()
                }

            let filter =
                [ ArsIdentifiers.BusinessNotifications.supplyPlanPublished.Id
                  ArsIdentifiers.BusinessNotifications.inventoryPositionAssessmentChanged.Id
                  ArsIdentifiers.BusinessNotifications.supplierCommitmentChanged.Id ]
                |> EnvelopeFilter.EventTypes

            try
                let! sub = deps.Subscribe filter processEnvelope ct
                return Ok sub
            with ex ->
                return Error (Infrastructure(EventStore($"Failed to subscribe commitment collaboration workflow: {ex.Message}")))
        }

    /// FS-S-051 / BW-S-051: Supplier collaboration health cadence workflow
    /// Subscribes to commitment assessment changes (BN-S-041) or supply plan published (BN-S-010)
    let createHealthCadenceWorkflow
        (deps: CollaborateWithSuppliersWorkflowDependencies)
        (ct: CancellationToken)
        : TaskResult<IDisposable, ApplicationError> =
        task {
            let processEnvelope (envelope: Envelope) : Task<unit> =
                task {
                    let req: PublishSupplierCollaborationHealthAssessmentReq =
                        { AssessmentScopeId = deps.DefaultScopeId
                          EvaluationPeriod = deps.DefaultPeriodId
                          PublicationTimestamp = envelope.CreatedUtc }

                    let! res = deps.Apis.Health.Publish req
                    match res with
                    | Ok _ -> ()
                    | Error _ -> ()
                }

            let filter =
                [ ArsIdentifiers.BusinessNotifications.supplierCommitmentAssessmentChanged.Id
                  ArsIdentifiers.BusinessNotifications.supplyPlanPublished.Id ]
                |> EnvelopeFilter.EventTypes

            try
                let! sub = deps.Subscribe filter processEnvelope ct
                return Ok sub
            with ex ->
                return Error (Infrastructure(EventStore($"Failed to subscribe health cadence workflow: {ex.Message}")))
        }

    /// BW-S-050 / BW-S-051 Notification Node: Dispatches BN-S-040, BN-S-041, BN-S-042 strictly after events exist
    let createNotificationNodeWorkflow
        (deps: CollaborateWithSuppliersWorkflowDependencies)
        (ct: CancellationToken)
        : TaskResult<IDisposable, ApplicationError> =
        task {
            let processEnvelope (envelope: Envelope) : Task<unit> =
                task {
                    if envelope.EventType = ArsIdentifiers.EnterpriseEvents.supplierCommitmentChanged.Id then
                        let parts = if envelope.AggregateId.Contains("#") then envelope.AggregateId.Split('#') else [| envelope.AggregateId; deps.DefaultItemId |]
                        let supplierId = parts.[0]
                        let itemId = if parts.Length > 1 then parts.[1] else deps.DefaultItemId

                        let notif: SupplierCommitmentChangedNotification =
                            { SupplierId = supplierId
                              ItemId = itemId
                              RequiredDate = envelope.CreatedUtc
                              PreviousState = SupplierCommitmentState.Proposed
                              NewState = SupplierCommitmentState.Committed
                              CommittedQuantity = None
                              CommittedDate = None
                              Confidence = CommitmentConfidenceLevel.Conditional
                              SequenceNumber = int envelope.OccurrenceNumber }

                        let! _ =
                            dispatchNotification
                                deps.DispatchEnvelope
                                ArsIdentifiers.BusinessNotifications.supplierCommitmentChanged.Id
                                ArsIdentifiers.Capabilities.collaborateWithSuppliers.Id
                                ArsIdentifiers.SemanticObjects.supplierCommitment.Id
                                envelope.AggregateId
                                notif
                        ()
                    elif envelope.EventType = ArsIdentifiers.EnterpriseEvents.supplierCommitmentAssessmentChanged.Id then
                        let notif: SupplierCommitmentAssessmentChangedNotification =
                            { SupplierId = envelope.AggregateId
                              EvaluationPeriod = deps.DefaultPeriodId
                              CommitmentReliabilityScore = 1.0m
                              CommitmentRiskScore = 0.0m
                              AssessmentState = "ACTIVE"
                              AuthoritativeHealthState = "HEALTHY" }

                        let! _ =
                            dispatchNotification
                                deps.DispatchEnvelope
                                ArsIdentifiers.BusinessNotifications.supplierCommitmentAssessmentChanged.Id
                                ArsIdentifiers.Capabilities.collaborateWithSuppliers.Id
                                ArsIdentifiers.SemanticObjects.supplierCommitmentAssessment.Id
                                envelope.AggregateId
                                notif
                        ()
                    elif envelope.EventType = ArsIdentifiers.EnterpriseEvents.supplierCollaborationHealthAssessmentPublished.Id then
                        let parts = if envelope.AggregateId.Contains("#") then envelope.AggregateId.Split('#') else [| envelope.AggregateId; deps.DefaultPeriodId |]
                        let scopeId = parts.[0]
                        let periodId = if parts.Length > 1 then parts.[1] else deps.DefaultPeriodId

                        let notif: SupplierCollaborationHealthAssessmentPublishedNotification =
                            { AssessmentScopeId = scopeId
                              EvaluationPeriod = periodId
                              VersionNumber = int envelope.OccurrenceNumber
                              AssessmentVersionId = envelope.AggregateId
                              PublicationTimestamp = envelope.CreatedUtc
                              PublicationOutcome = "Published"
                              SupplierCount = 1 }

                        let! _ =
                            dispatchNotification
                                deps.DispatchEnvelope
                                ArsIdentifiers.BusinessNotifications.supplierCollaborationHealthAssessmentPublished.Id
                                ArsIdentifiers.Capabilities.collaborateWithSuppliers.Id
                                ArsIdentifiers.SemanticObjects.supplierCollaborationHealthAssessment.Id
                                envelope.AggregateId
                                notif
                        ()
                }

            let filter =
                [ ArsIdentifiers.EnterpriseEvents.supplierCommitmentChanged.Id
                  ArsIdentifiers.EnterpriseEvents.supplierCommitmentAssessmentChanged.Id
                  ArsIdentifiers.EnterpriseEvents.supplierCollaborationHealthAssessmentPublished.Id ]
                |> EnvelopeFilter.EventTypes

            try
                let! sub = deps.Subscribe filter processEnvelope ct
                return Ok sub
            with ex ->
                return Error (Infrastructure(EventStore($"Failed to subscribe supplier notification node workflow: {ex.Message}")))
        }

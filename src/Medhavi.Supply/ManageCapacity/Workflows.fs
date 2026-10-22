namespace Medhavi.Supply.ManageCapacity

open System
open System.Threading
open System.Threading.Tasks
open Medhavi.Common
open Medhavi.Contracts.Supply
open Medhavi.Foundation.Contracts
open Medhavi.Foundation.Failure
open Medhavi.Supply
open Medhavi.Supply.ArsIdentifiers

type ManageCapacityWorkflowDependencies =
    { Subscribe: EnvelopeFilter -> (Envelope -> Task<unit>) -> CancellationToken -> Task<IDisposable>
      DispatchEnvelope: Envelope -> Task<unit>
      Apis: ManageCapacityApis
      DefaultResourceId: string
      DefaultScopeId: string
      DefaultPeriodId: string }

/// CA-S-004 Manage Capacity Workflows
/// Traces to: BW-S-030, BW-S-031, FS-S-030, FS-S-031
module Workflows =

    /// FS-S-030 / BW-S-030: Triggered when Supply Understanding (BN-S-001) or Supply Plan (BN-S-010) is published
    let createPositionUpdateWorkflow
        (deps: ManageCapacityWorkflowDependencies)
        (ct: CancellationToken)
        : TaskResult<IDisposable, ApplicationError> =
        task {
            let processEnvelope (envelope: Envelope) : Task<unit> =
                task {
                    let resourceId =
                        if not (String.IsNullOrWhiteSpace envelope.AggregateId) then
                            envelope.AggregateId
                        else
                            deps.DefaultResourceId

                    let req: UpdateCapacityPositionReq =
                        { PhysicalResourceId = resourceId
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
                return Error (Infrastructure(EventStore($"Failed to subscribe capacity position workflow: {ex.Message}")))
        }

    /// FS-S-031 / BW-S-031: Periodic capacity health assessment workflow
    let createHealthAssessmentWorkflow
        (deps: ManageCapacityWorkflowDependencies)
        (ct: CancellationToken)
        : TaskResult<IDisposable, ApplicationError> =
        task {
            let processEnvelope (_: Envelope) : Task<unit> =
                task {
                    let req: PublishCapacityHealthReq =
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
                return Error (Infrastructure(EventStore($"Failed to subscribe capacity health workflow: {ex.Message}")))
        }

    /// BW-S-030 / BW-S-031 Notification Node: Dispatches BN-S-030, BN-S-031 strictly after events exist
    let createNotificationNodeWorkflow
        (deps: ManageCapacityWorkflowDependencies)
        (ct: CancellationToken)
        : TaskResult<IDisposable, ApplicationError> =
        task {
            let processEnvelope (envelope: Envelope) : Task<unit> =
                task {
                    if envelope.EventType = ArsIdentifiers.EnterpriseEvents.capacityPositionAssessmentChanged.Id then
                        let resourceId =
                            if not (String.IsNullOrWhiteSpace envelope.AggregateId) then envelope.AggregateId
                            else deps.DefaultResourceId

                        let notif: CapacityPositionChangedNotification =
                            { PhysicalResourceId = resourceId
                              NewUtilizationStatus = Medhavi.Contracts.Supply.CapacityUtilizationStatus.Optimal
                              NewConstraintStatus = Medhavi.Contracts.Supply.CapacityConstraintStatus.NonConstraint
                              NewBottleneckAssessment = Medhavi.Contracts.Supply.BottleneckAssessmentStatus.NotABottleneck
                              ChangeSummary = "Capacity position updated"
                              Timestamp = envelope.CreatedUtc }

                        let! _ =
                            dispatchNotification
                                deps.DispatchEnvelope
                                ArsIdentifiers.BusinessNotifications.capacityPositionAssessmentChanged.Id
                                ArsIdentifiers.Capabilities.manageCapacity.Id
                                ArsIdentifiers.SemanticObjects.capacityPositionAssessment.Id
                                envelope.AggregateId
                                notif
                        ()
                    elif envelope.EventType = ArsIdentifiers.EnterpriseEvents.capacityHealthAssessmentPublished.Id then
                        let parts = if envelope.AggregateId.Contains("#") then envelope.AggregateId.Split('#') else [| envelope.AggregateId; deps.DefaultPeriodId |]
                        let scopeId = parts.[0]
                        let periodId = if parts.Length > 1 then parts.[1] else deps.DefaultPeriodId

                        let notif: CapacityHealthPublishedNotification =
                            { AssessmentScopeId = scopeId
                              EvaluationPeriodId = periodId
                              VersionNumber = int envelope.OccurrenceNumber
                              OverallCapacityHealthState = Medhavi.Contracts.Supply.CapacityHealthState.Balanced
                              ClassificationRationale = "Governed capacity health assessment published"
                              BottleneckCount = 0
                              Timestamp = envelope.CreatedUtc }

                        let! _ =
                            dispatchNotification
                                deps.DispatchEnvelope
                                ArsIdentifiers.BusinessNotifications.capacityHealthAssessmentPublished.Id
                                ArsIdentifiers.Capabilities.manageCapacity.Id
                                ArsIdentifiers.SemanticObjects.capacityHealthAssessment.Id
                                envelope.AggregateId
                                notif
                        ()
                }

            let filter =
                [ ArsIdentifiers.EnterpriseEvents.capacityPositionAssessmentChanged.Id
                  ArsIdentifiers.EnterpriseEvents.capacityHealthAssessmentPublished.Id ]
                |> EnvelopeFilter.EventTypes

            try
                let! sub = deps.Subscribe filter processEnvelope ct
                return Ok sub
            with ex ->
                return Error (Infrastructure(EventStore($"Failed to subscribe capacity notification node workflow: {ex.Message}")))
        }

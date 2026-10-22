/// CA-S-011 Detect Supply Exceptions Workflows
/// Traces to: BW-S-100, FS-S-100
module Medhavi.Supply.DetectSupplyExceptions.Workflows

open System
open System.Threading
open System.Threading.Tasks
open Medhavi.Common
open Medhavi.Foundation.Contracts
open Medhavi.Foundation.Failure
open Medhavi.Contracts.Supply
open Medhavi.Supply
open Medhavi.Supply.ArsIdentifiers
open Medhavi.Supply.DetectSupplyExceptions.SupplyExceptionAssessment.Policies
open Capabilities

type DetectSupplyExceptionsWorkflowDependencies =
    { Subscribe: EnvelopeFilter -> (Envelope -> Task<unit>) -> CancellationToken -> Task<IDisposable>
      DispatchEnvelope: Envelope -> Task<unit>
      Apis: DetectSupplyExceptionsApis
      DefaultScopeId: string }

/// BW-S-100 / FS-S-100: Workflow evaluating supply exceptions when supply changes are updated (BN-S-080 / EV-S-080)
let createSupplyChangeExceptionWorkflow
    (deps: DetectSupplyExceptionsWorkflowDependencies)
    (ct: CancellationToken)
    : TaskResult<IDisposable, ApplicationError> =
    task {
        let processEnvelope (envelope: Envelope) : Task<unit> =
            task {
                let scopeId =
                    if String.IsNullOrWhiteSpace envelope.AggregateId then deps.DefaultScopeId
                    else envelope.AggregateId

                let req: EvaluateSupplyExceptionReq =
                    { GovernedPolicyReference = defaultPolicy.PolicyId
                      ScopeEntityType = "PlanningScope"
                      ScopeEntityId = scopeId
                      PlanningScopeId = scopeId
                      ExceptionType = "SupplyDisruption"
                      TriggeringEvidence = [ sprintf "Supply change event %s" envelope.EventType ]
                      EvaluationTimestamp = DateTimeOffset.UtcNow }

                let! res = deps.Apis.Exceptions.EvaluateException req
                match res with
                | Ok _ -> ()
                | Error _ -> ()
            }

        let filter =
            [ BusinessNotifications.supplyChangeAssessmentUpdatedNotification.Id
              EnterpriseEvents.supplyChangeAssessmentUpdated.Id ]
            |> EnvelopeFilter.EventTypes

        try
            let! sub = deps.Subscribe filter processEnvelope ct
            return Ok sub
        with ex ->
            return Error (Infrastructure(EventStore($"Failed to subscribe supply change exception workflow: {ex.Message}")))
    }

/// BW-S-100 / FS-S-100: Workflow evaluating supply exceptions when quality assessments are published (BN-S-090 / EV-S-090)
let createSupplyQualityExceptionWorkflow
    (deps: DetectSupplyExceptionsWorkflowDependencies)
    (ct: CancellationToken)
    : TaskResult<IDisposable, ApplicationError> =
    task {
        let processEnvelope (envelope: Envelope) : Task<unit> =
            task {
                let scopeId =
                    if String.IsNullOrWhiteSpace envelope.AggregateId then deps.DefaultScopeId
                    else envelope.AggregateId

                let req: EvaluateSupplyExceptionReq =
                    { GovernedPolicyReference = defaultPolicy.PolicyId
                      ScopeEntityType = "PlanningScope"
                      ScopeEntityId = scopeId
                      PlanningScopeId = scopeId
                      ExceptionType = "ServicePolicyViolation"
                      TriggeringEvidence = [ sprintf "Supply quality event %s" envelope.EventType ]
                      EvaluationTimestamp = DateTimeOffset.UtcNow }

                let! res = deps.Apis.Exceptions.EvaluateException req
                match res with
                | Ok _ -> ()
                | Error _ -> ()
            }

        let filter =
            [ BusinessNotifications.supplyQualityAssessmentPublishedNotification.Id
              EnterpriseEvents.supplyQualityAssessmentPublished.Id ]
            |> EnvelopeFilter.EventTypes

        try
            let! sub = deps.Subscribe filter processEnvelope ct
            return Ok sub
        with ex ->
            return Error (Infrastructure(EventStore($"Failed to subscribe supply quality exception workflow: {ex.Message}")))
    }

/// BW-S-100 / FS-S-100: Workflow evaluating supply exceptions on operational position changes (BN-S-020, BN-S-030, BN-S-040)
let createOperationalPositionExceptionWorkflow
    (deps: DetectSupplyExceptionsWorkflowDependencies)
    (ct: CancellationToken)
    : TaskResult<IDisposable, ApplicationError> =
    task {
        let processEnvelope (envelope: Envelope) : Task<unit> =
            task {
                let scopeId =
                    if String.IsNullOrWhiteSpace envelope.AggregateId then deps.DefaultScopeId
                    else envelope.AggregateId

                let exType =
                    if envelope.EventType = BusinessNotifications.inventoryPositionAssessmentChanged.Id then "InventoryPolicyViolation"
                    elif envelope.EventType = BusinessNotifications.capacityPositionAssessmentChanged.Id then "CapacityPolicyViolation"
                    elif envelope.EventType = BusinessNotifications.supplierCommitmentChanged.Id then "SupplierCollaborationBreach"
                    else "EmergingRisk"

                let req: EvaluateSupplyExceptionReq =
                    { GovernedPolicyReference = defaultPolicy.PolicyId
                      ScopeEntityType = "PlanningScope"
                      ScopeEntityId = scopeId
                      PlanningScopeId = scopeId
                      ExceptionType = exType
                      TriggeringEvidence = [ sprintf "Operational position event %s" envelope.EventType ]
                      EvaluationTimestamp = DateTimeOffset.UtcNow }

                let! res = deps.Apis.Exceptions.EvaluateException req
                match res with
                | Ok _ -> ()
                | Error _ -> ()
            }

        let filter =
            [ BusinessNotifications.inventoryPositionAssessmentChanged.Id
              BusinessNotifications.capacityPositionAssessmentChanged.Id
              BusinessNotifications.supplierCommitmentChanged.Id ]
            |> EnvelopeFilter.EventTypes

        try
            let! sub = deps.Subscribe filter processEnvelope ct
            return Ok sub
        with ex ->
            return Error (Infrastructure(EventStore($"Failed to subscribe operational position exception workflow: {ex.Message}")))
    }

/// BW-S-100 / FS-S-100: Workflow aligning local exceptions on Core enterprise exception events (BN-C-002, BN-C-003)
let createEnterpriseCoreExceptionWorkflow
    (deps: DetectSupplyExceptionsWorkflowDependencies)
    (ct: CancellationToken)
    : TaskResult<IDisposable, ApplicationError> =
    task {
        let processEnvelope (envelope: Envelope) : Task<unit> =
            task {
                let scopeId =
                    if String.IsNullOrWhiteSpace envelope.AggregateId then deps.DefaultScopeId
                    else envelope.AggregateId

                let req: EvaluateSupplyExceptionReq =
                    { GovernedPolicyReference = defaultPolicy.PolicyId
                      ScopeEntityType = "PlanningScope"
                      ScopeEntityId = scopeId
                      PlanningScopeId = scopeId
                      ExceptionType = "ExternalEvent"
                      TriggeringEvidence = [ sprintf "Core Enterprise Exception signal %s" envelope.EventType ]
                      EvaluationTimestamp = DateTimeOffset.UtcNow }

                let! res = deps.Apis.Exceptions.EvaluateException req
                match res with
                | Ok _ -> ()
                | Error _ -> ()
            }

        let filter =
            [ ArsIdentifiers.ExternalNotifications.enterpriseExceptionActive.Id
              ArsIdentifiers.ExternalNotifications.enterpriseExceptionResolved.Id ]
            |> EnvelopeFilter.EventTypes

        try
            let! sub = deps.Subscribe filter processEnvelope ct
            return Ok sub
        with ex ->
            return Error (Infrastructure(EventStore($"Failed to subscribe enterprise core exception workflow: {ex.Message}")))
    }

/// BW-S-100 Notification Node: Dispatches BN-S-100, BN-S-101, BN-S-102 strictly after events exist
let createNotificationNodeWorkflow
    (deps: DetectSupplyExceptionsWorkflowDependencies)
    (ct: CancellationToken)
    : TaskResult<IDisposable, ApplicationError> =
    task {
        let processEnvelope (envelope: Envelope) : Task<unit> =
            task {
                if envelope.EventType = EnterpriseEvents.supplyExceptionAssessmentUpdated.Id then
                    let notif: SupplyExceptionAssessmentUpdatedNotification =
                        { ExceptionIdentifier = envelope.AggregateId
                          ExceptionType = "SupplyDisruption"
                          GovernedPolicyReference = defaultPolicy.PolicyId
                          ExceptionScope = sprintf "PlanningScope:%s" envelope.AggregateId
                          LifecycleState = "Active"
                          BusinessImpact = "Major"
                          Urgency = "High"
                          ExceptionHorizon = "ShortTerm"
                          ChangeDescription = "Supply exception assessment updated"
                          PublicationTimestamp = envelope.CreatedUtc }

                    let! _ =
                        dispatchNotification
                            deps.DispatchEnvelope
                            BusinessNotifications.supplyExceptionAssessmentUpdatedNotification.Id
                            Capabilities.detectSupplyExceptions.Id
                            "SupplyExceptionAssessment"
                            envelope.AggregateId
                            notif
                    ()
                elif envelope.EventType = EnterpriseEvents.supplyExceptionDetectionEvidenceProduced.Id then
                    let notif: SupplyExceptionDetectionEvidenceNotification =
                        { EvidenceIdentifier = Guid.NewGuid().ToString()
                          ConstraintReference = defaultPolicy.PolicyId
                          AffectedScopeType = "PlanningScope"
                          AffectedScopeIdentifier = envelope.AggregateId
                          ExceptionClassification = "SupplyDisruption"
                          BusinessImpact = "Major"
                          Urgency = "High"
                          TriggeringEvidence = "Authoritative enterprise exception detection evidence"
                          SupplyExceptionIdentifier = envelope.AggregateId
                          PublicationTimestamp = envelope.CreatedUtc }

                    let! _ =
                        dispatchNotification
                            deps.DispatchEnvelope
                            BusinessNotifications.supplyExceptionDetectionEvidenceNotification.Id
                            Capabilities.detectSupplyExceptions.Id
                            "SupplyExceptionAssessment"
                            envelope.AggregateId
                            notif
                    ()
                elif envelope.EventType = EnterpriseEvents.supplyExceptionResolutionEvidenceProduced.Id then
                    let notif: SupplyExceptionResolutionEvidenceNotification =
                        { ResolutionIdentifier = Guid.NewGuid().ToString()
                          ConstraintReference = defaultPolicy.PolicyId
                          AffectedScopeType = "PlanningScope"
                          AffectedScopeIdentifier = envelope.AggregateId
                          ResolutionEvidence = "Supply exception resolved per governed criteria"
                          SupplyExceptionIdentifier = envelope.AggregateId
                          ResolutionTimestamp = envelope.CreatedUtc }

                    let! _ =
                        dispatchNotification
                            deps.DispatchEnvelope
                            BusinessNotifications.supplyExceptionResolutionEvidenceNotification.Id
                            Capabilities.detectSupplyExceptions.Id
                            "SupplyExceptionAssessment"
                            envelope.AggregateId
                            notif
                    ()
            }

        let filter =
            [ EnterpriseEvents.supplyExceptionAssessmentUpdated.Id
              EnterpriseEvents.supplyExceptionDetectionEvidenceProduced.Id
              EnterpriseEvents.supplyExceptionResolutionEvidenceProduced.Id ]
            |> EnvelopeFilter.EventTypes

        try
            let! sub = deps.Subscribe filter processEnvelope ct
            return Ok sub
        with ex ->
            return Error (Infrastructure(EventStore($"Failed to subscribe supply exception notification node workflow: {ex.Message}")))
    }

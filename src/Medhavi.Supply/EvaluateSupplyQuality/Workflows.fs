/// BW-S-090 / FS-S-090 — Evaluate Supply Quality Workflow
/// Traces to: CR-S-022, BW-S-090, FS-S-090
module Medhavi.Supply.EvaluateSupplyQuality.Workflows

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

type EvaluateSupplyQualityWorkflowDependencies =
    { Subscribe: EnvelopeFilter -> (Envelope -> Task<unit>) -> CancellationToken -> Task<IDisposable>
      DispatchEnvelope: Envelope -> Task<unit>
      Apis: EvaluateSupplyQualityApis
      DefaultEvaluationPeriodId: string
      DefaultPlanningHorizon: string }

/// FS-S-090 / BW-S-090: Senses Supply Understanding Published notifications & triggers quality assessment
let createQualityEvaluationWorkflow
    (deps: EvaluateSupplyQualityWorkflowDependencies)
    (ct: CancellationToken)
    : TaskResult<IDisposable, ApplicationError> =
    task {
        let processEnvelope (envelope: Envelope) : Task<unit> =
            task {
                let scopeId = envelope.AggregateId
                if not (String.IsNullOrWhiteSpace scopeId) then
                    let req : EvaluateSupplyQualityReq =
                        { PlanningScopeId = scopeId
                          PlanningHorizon = deps.DefaultPlanningHorizon
                          EvaluationPeriodId = deps.DefaultEvaluationPeriodId
                          ForceMajeureDeclared = false
                          StrategicInvestmentNotes = None }

                    let! res = deps.Apis.Evaluate req
                    match res with
                    | Ok _ -> ()
                    | Error _ -> ()
                else
                    ()
            }

        let filter =
            [ BusinessNotifications.supplyUnderstandingPublished.Id
              EnterpriseEvents.supplyUnderstandingPublished.Id ]
            |> EnvelopeFilter.EventTypes

        try
            let! sub = deps.Subscribe filter processEnvelope ct
            return Ok sub
        with ex ->
            return Error (Infrastructure(EventStore($"Failed to subscribe quality evaluation workflow: {ex.Message}")))
    }

/// BW-S-090 Notification Node: Dispatches BN-S-090 strictly after EV-S-090 exists
let createNotificationNodeWorkflow
    (deps: EvaluateSupplyQualityWorkflowDependencies)
    (ct: CancellationToken)
    : TaskResult<IDisposable, ApplicationError> =
    task {
        let processEnvelope (envelope: Envelope) : Task<unit> =
            task {
                if envelope.EventType = EnterpriseEvents.supplyQualityAssessmentPublished.Id then
                    let notif: SupplyQualityAssessmentPublishedNotificationDto =
                        { NotificationId = Guid.NewGuid().ToString()
                          EventId = EnterpriseEvents.supplyQualityAssessmentPublished.Id
                          PlanningScopeId = envelope.AggregateId
                          PlanningHorizon = deps.DefaultPlanningHorizon
                          EvaluationPeriodId = deps.DefaultEvaluationPeriodId
                          VersionNumber = int envelope.OccurrenceNumber
                          OverallClassification = "Acceptable"
                          OverallScore = 1.0m
                          OverallConfidence = "High"
                          DimensionScores = Map.empty
                          ConfidencePerDimension = Map.empty
                          IssueCountsPerDimension = Map.empty
                          PublicationFlags = []
                          Timestamp = envelope.CreatedUtc }

                    let! _ =
                        dispatchNotification
                            deps.DispatchEnvelope
                            BusinessNotifications.supplyQualityAssessmentPublishedNotification.Id
                            Capabilities.evaluateSupplyQuality.Id
                            "SupplyQualityAssessment"
                            envelope.AggregateId
                            notif
                    ()
            }

        let filter =
            [ EnterpriseEvents.supplyQualityAssessmentPublished.Id ]
            |> EnvelopeFilter.EventTypes

        try
            let! sub = deps.Subscribe filter processEnvelope ct
            return Ok sub
        with ex ->
            return Error (Infrastructure(EventStore($"Failed to subscribe supply quality notification node workflow: {ex.Message}")))
    }

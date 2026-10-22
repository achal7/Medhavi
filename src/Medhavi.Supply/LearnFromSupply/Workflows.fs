// Traceability: BW-S-120 (FS-S-120), BW-S-121 (FS-S-121), BN-S-120, BN-S-121
module Medhavi.Supply.LearnFromSupply.Workflows

open System
open System.Threading
open System.Threading.Tasks
open Medhavi.Common
open Medhavi.Foundation.Contracts
open Medhavi.Foundation.Failure
open Medhavi.Contracts
open Medhavi.Contracts.Supply
open Medhavi.Supply
open Medhavi.Supply.ArsIdentifiers

type SupplyLearningWorkflowDependencies =
    {
        Subscribe: EnvelopeFilter -> (Envelope -> Task<unit>) -> CancellationToken -> Task<IDisposable>
        DispatchEnvelope: Envelope -> Task<unit>
        LearningApi: SupplyLearningApi
    }

/// BW-S-120 / FS-S-120: Automated Supply Learning Discovery Workflow
/// Triggered by periodic quality publication (EV-S-090) or understanding publication (EV-S-004)
let createSupplyLearningDiscoveryWorkflow
    (deps: SupplyLearningWorkflowDependencies)
    (ct: CancellationToken)
    : TaskResult<IDisposable, ApplicationError> =
    task {
        let processEnvelope (envelope: Envelope) : Task<unit> =
            task {
                let scopeId = envelope.AggregateId

                if not(String.IsNullOrWhiteSpace scopeId) then
                    let now = DateTimeOffset.UtcNow

                    let emptyBundle: HistoricalSupplyEvidenceBundleDto =
                        {
                            Scope = scopeId
                            WindowStart = now.AddDays(-180.0)
                            WindowEnd = now
                            QualitySummaries = []
                            ExceptionSummaries = []
                            PlanStabilitySummaries = []
                            CapacitySummaries = []
                            SupplierSummaries = []
                            InventorySummaries = []
                        }

                    let req: DeriveSupplyLearningsReq =
                        {
                            Scope = scopeId
                            WindowStart = now.AddDays(-180.0)
                            WindowEnd = now
                            EvidenceBundle = emptyBundle
                        }

                    let! candidatesResult = deps.LearningApi.DeriveLearnings req

                    match candidatesResult with
                    | Ok candidates ->
                        for cand in candidates do
                            let learningId = $"{cand.SubjectType}#{cand.SubjectId}#{cand.LearningType}#v1"

                            let recordReq: RecordSupplyLearningReq =
                                {
                                    LearningId = learningId
                                    SubjectType = cand.SubjectType
                                    SubjectId = cand.SubjectId
                                    LearningType = cand.LearningType
                                    Scope = cand.LearningScope
                                    TimeHorizonStart = cand.TimeHorizonStart
                                    TimeHorizonEnd = cand.TimeHorizonEnd
                                    PatternDescription = cand.LearningStatement
                                    RecurrenceCount = cand.RecurrenceCount
                                    RecurrenceFrequency = cand.RecurrenceFrequency
                                    StatisticalSignificance = cand.StatisticalSignificance
                                    LearningStatement = cand.LearningStatement
                                    PatternConfidence = cand.PatternConfidence
                                    PatternConfidenceScore = cand.PatternConfidenceScore
                                    ImprovementOpportunities = cand.ImprovementOpportunities
                                    SupportingEvidence = cand.SupportingEvidence
                                    SourceEvidenceVersionSet = []
                                    PolicyVersion = Policies.learningPolicy.Id
                                }

                            let! _ = deps.LearningApi.RecordLearning recordReq
                            ()

                        return ()
                    | Error _ -> return ()
                else
                    return ()
            }

        let filter =
            [
                EnterpriseEvents.supplyQualityAssessmentPublished.Id
                EnterpriseEvents.supplyUnderstandingPublished.Id
            ]
            |> EnvelopeFilter.EventTypes

        try
            let! sub = deps.Subscribe filter processEnvelope ct
            return Ok sub
        with ex ->
            return Error (Infrastructure(EventStore($"Failed to subscribe supply learning discovery workflow: {ex.Message}")))
    }

/// BW-S-120 / BW-S-121 Notification Node: Dispatches BN-S-120 and BN-S-121 strictly after enterprise events exist
let createNotificationNodeWorkflow
    (deps: SupplyLearningWorkflowDependencies)
    (ct: CancellationToken)
    : TaskResult<IDisposable, ApplicationError> =
    task {
        let processEnvelope (envelope: Envelope) : Task<unit> =
            task {
                if envelope.EventType = EnterpriseEvents.supplyLearningRecorded.Id then
                    let notif: SupplyLearningRecordedNotificationDto =
                        {
                            LearningId = envelope.AggregateId
                            LearningStatementSummary = "Candidate supply learning discovered and recorded"
                            LearningOutcome = None
                            PatternConfidence = "Medium"
                            InterventionConfidenceSummary = "Discovered pattern under governance review"
                            GenerationTimestamp = envelope.CreatedUtc
                        }

                    let! _ =
                        dispatchNotification
                            deps.DispatchEnvelope
                            BusinessNotifications.supplyLearningRecordedNotification.Id
                            Capabilities.learnFromSupply.Id
                            "SupplyLearning"
                            envelope.AggregateId
                            notif
                    ()
                elif envelope.EventType = EnterpriseEvents.supplyLearningOutcomeRecorded.Id then
                    let notif: SupplyLearningOutcomeRecordedNotificationDto =
                        {
                            LearningId = envelope.AggregateId
                            LearningOutcome = "Approved"
                            RecordingTimestamp = envelope.CreatedUtc
                            Authority = "Enterprise Governance Authority"
                        }

                    let! _ =
                        dispatchNotification
                            deps.DispatchEnvelope
                            BusinessNotifications.supplyLearningOutcomeRecordedNotification.Id
                            Capabilities.learnFromSupply.Id
                            "SupplyLearning"
                            envelope.AggregateId
                            notif
                    ()
            }

        let filter =
            [
                EnterpriseEvents.supplyLearningRecorded.Id
                EnterpriseEvents.supplyLearningOutcomeRecorded.Id
            ]
            |> EnvelopeFilter.EventTypes

        try
            let! sub = deps.Subscribe filter processEnvelope ct
            return Ok sub
        with ex ->
            return Error (Infrastructure(EventStore($"Failed to subscribe supply learning notification node: {ex.Message}")))
    }

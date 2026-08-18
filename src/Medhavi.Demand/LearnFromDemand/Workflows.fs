/// CA-D-010 — Learn From Demand Workflows
/// Traces to: FS-D-017 (Establish Demand Learning Workflow)
module Medhavi.Demand.LearnFromDemand.Workflows

open System
open System.Threading
open System.Threading.Tasks
open Medhavi.Foundation.Contracts
open Medhavi.Contracts.Demand
open Medhavi.Demand

/// Dependencies for FS-D-017 Establish Demand Learning workflow
type DemandLearningWorkflowDependencies =
    { Subscribe: EnvelopeFilter -> (Envelope -> Task<unit>) -> CancellationToken -> Task<IDisposable>
      LearningApi: DemandLearningApi }

/// FS-D-017: Automated Establish Demand Learning Workflow
/// Triggered when periodic quality assessments or exception evaluations complete
let createDemandLearningWorkflow
    (deps: DemandLearningWorkflowDependencies)
    (ct: CancellationToken)
    : Task<IDisposable> =
    task {
        let processEnvelope (envelope: Envelope) : Task<unit> =
            task {
                let scopeId = envelope.AggregateId

                if not(String.IsNullOrWhiteSpace scopeId) then
                    let now = DateTimeOffset.UtcNow

                    let emptyBundle: HistoricalDemandEvidenceBundleDto =
                        { Scope = scopeId
                          WindowStart = now.AddDays(-90.0)
                          WindowEnd = now
                          QualityAssessments = []
                          PlannerOverrides = []
                          DemandExceptions = []
                          ClassificationChanges = [] }

                    let req: DeriveDemandLearningsReq =
                        { Scope = scopeId
                          WindowStart = now.AddDays(-90.0)
                          WindowEnd = now
                          EvidenceBundle = emptyBundle }

                    let! candidatesResult = deps.LearningApi.DeriveLearnings req

                    match candidatesResult with
                    | Ok candidates ->
                        for cand in candidates do
                            let recordReq: RecordDemandLearningReq =
                                { LearningId = cand.LearningId
                                  Scope = cand.Scope
                                  LearningType = cand.LearningType
                                  LearningStatement = cand.LearningStatement
                                  PatternConfidence = cand.PatternConfidence
                                  InterventionConfidence = cand.InterventionConfidence
                                  SupportingEvidence = cand.SupportingEvidence
                                  ImprovementOpportunities = cand.ImprovementOpportunities
                                  PolicyVersion = cand.PolicyVersion }

                            let! _ = deps.LearningApi.RecordLearning recordReq
                            ()

                        return ()
                    | Error _ -> return ()
                else
                    return ()
            }

        let filter =
            [ ArsIdentifiers.EnterpriseEvents.forecastQualityAssessmentPublished.Id
              ArsIdentifiers.EnterpriseEvents.demandExceptionEvidenceEvaluated.Id ]
            |> EnvelopeFilter.EventTypes

        return! deps.Subscribe filter processEnvelope ct
    }

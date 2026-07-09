module Medhavi.Demand.DemandLearning.Capabilities

open System.Threading.Tasks
open Medhavi.SharedKernel
open Medhavi.SharedKernel.Failure
open Medhavi.Contracts.Demand.DemandLearning
open Medhavi.Demand
open Medhavi.Demand.DemandLearning.Model
open Medhavi.Demand.DemandLearning.ACL
open Medhavi.Demand.DemandLearningAlgorithms
open Medhavi.SharedKernel.Execution
open Medhavi.Common.Validation

let private publishSuccess (learning: DemandLearning) =
    let notification: DemandLearningRecordedNotification = {
        LearningId        = DemandLearningId.value learning.Id
        LearningType      = learning.LearningType
        LearningStatement = learning.LearningStatement
        EvidenceStrength  = learning.EvidenceStrength
    }
    DomainEventBus.Publish notification

// Traceability: Implements CA‑D‑010 (Learn From Demand) Capabilities API for SE‑D‑042 (Demand Learning)
// Exposes the workflow layer: validates requests, fetches historical quality assessments,
// runs trend analysis, constructs domain commands, and publishes BN‑D‑025.

let createCapabilities
    (execute: DemandLearningCommand -> Task<ExecutionOutcome<DemandLearning, ApplicationError>>)
    (getAssessments: PlanningScopeId -> Task<QualityAssessmentSnapshot list>)
    : DemandLearningApi =

    let recordLearning (req: RecordDemandLearningReq) =
        task {
            // 1. EARLY VALIDATION
            match ACL.toRecordCmd req with
            | Invalid errors ->
                return Error (ApplicationError.Domain (DomainError.combineValidationErrors errors) |> ApplicationError.mapToApiError)
            | Valid cmd ->
                // 2. FETCH ASSESSMENTS AND ANALYZE
                let! analysisResult =
                    match cmd.PlanningScopeId with
                    | None -> task { return None }
                    | Some scopeId ->
                        task {
                            let! assessments = getAssessments scopeId
                            return analyzeQualityTrend assessments
                        }

                // 3. BUILD COMMAND (enrich with analysis if available)
                let enrichedCmd =
                    match analysisResult with
                    | Some (statement, evidence, strength) ->
                        { cmd with
                            LearningStatement  = statement
                            SupportingEvidence = evidence
                            EvidenceStrength   = strength }
                    | None ->
                        cmd

                // 4. EXECUTE
                let! outcome = execute (Record enrichedCmd)

                match Helpers.toApiResult outcome with
                | Ok learning ->
                    publishSuccess learning
                    return Ok (DemandLearningId.value learning.Id)
                | Error err -> return Error err
        }

    { RecordLearning = recordLearning }

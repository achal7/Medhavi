module Medhavi.Client.Supply.Learning.SupplyLearningActions

open System
open System.Threading.Tasks
open Medhavi.Contracts.Supply
open Medhavi.Client.ViewComposition.Identity
open Medhavi.Client.ViewComposition.Component
open Medhavi.Client.ActionExecution
open Medhavi.Client

let recordLearning: ActionHandler =
    fun ctx ->
        task {
            let api = ctx.Domain.Supply.LearnFromSupply.SupplyLearningApi
            let learningId =
                match ctx.TargetKey with
                | Some k when not (String.IsNullOrWhiteSpace k) -> k
                | _ -> $"LRN-{DateTimeOffset.UtcNow.ToUnixTimeSeconds()}"

            let subjectType, subjectId =
                match ctx.Payload with
                | CustomParameters p ->
                    let st = match Map.tryFind "SubjectType" p with Some (StringValue s) -> s | _ -> "WorkCenter"
                    let si = match Map.tryFind "SubjectId" p with Some (StringValue s) -> s | _ -> "WC-ASSEMBLY-LINE-01"
                    st, si
                | _ -> "WorkCenter", "WC-ASSEMBLY-LINE-01"

            let learningType =
                match ctx.Payload with
                | CustomParameters p ->
                    match Map.tryFind "LearningType" p with Some (StringValue s) -> s | _ -> "CapacityCalibration"
                | _ -> "CapacityCalibration"

            let req: RecordSupplyLearningReq =
                { LearningId = learningId
                  SubjectType = subjectType
                  SubjectId = subjectId
                  LearningType = learningType
                  Scope = "PS-GLOBAL-SUPPLY-CHAIN"
                  TimeHorizonStart = DateTimeOffset.UtcNow.AddDays(-30.0)
                  TimeHorizonEnd = DateTimeOffset.UtcNow
                  PatternDescription = "Empirical cycle pattern derived from multi-period operational telemetry."
                  RecurrenceCount = 3
                  RecurrenceFrequency = 0.85m
                  StatisticalSignificance = 0.95m
                  LearningStatement = $"Systemic lead time and capacity offset detected for {subjectId}."
                  PatternConfidence = "High"
                  PatternConfidenceScore = 0.92m
                  ImprovementOpportunities = []
                  SupportingEvidence = []
                  SourceEvidenceVersionSet = [ "v1.0" ]
                  PolicyVersion = "POL-LRN-01" }

            match! api.RecordLearning req with
            | Ok dto ->
                return
                    Ok(
                        ActionExecutionOutcome.Completed(
                            $"Supply learning recorded ({dto.LearningId}) for subject '{dto.SubjectId}'.",
                            [ SupplyLearningGrid.storeId ]
                        )
                    )
            | Error err -> return Error err.Message
        }

let recordOutcome: ActionHandler =
    fun ctx ->
        task {
            let api = ctx.Domain.Supply.LearnFromSupply.SupplyLearningApi
            match ctx.TargetKey with
            | Some learningId ->
                let outcome, rationale =
                    match ctx.Payload with
                    | CustomParameters p ->
                        let o = match Map.tryFind "Outcome" p with Some (StringValue s) -> s | _ -> "Accepted"
                        let r = match Map.tryFind "Rationale" p with Some (StringValue s) -> Some s | _ -> None
                        o, r
                    | _ -> "Accepted", Some "Governance committee accepted empirical calibration."

                let req: RecordLearningOutcomeReq =
                    { LearningId = learningId
                      Outcome = outcome
                      Authority = "SUPPLY-GOVERNANCE-BOARD"
                      Rationale = rationale }

                match! api.RecordOutcome req with
                | Ok dto ->
                    return
                        Ok(
                            ActionExecutionOutcome.Completed(
                                $"Learning outcome recorded for '{dto.LearningId}': Outcome = {dto.Outcome}.",
                                [ SupplyLearningGrid.storeId ]
                            )
                        )
                | Error err -> return Error err.Message
            | None -> return Error "No supply learning selected to record outcome."
        }

let all: (ActionDefId * ActionHandler) list =
    [ SupplyLearningGrid.SupplyLearningActions.recordLearning.Id, recordLearning
      SupplyLearningGrid.SupplyLearningActions.recordOutcome.Id, recordOutcome ]

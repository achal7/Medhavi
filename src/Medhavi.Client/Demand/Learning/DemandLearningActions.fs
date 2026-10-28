module Medhavi.Client.Demand.Learning.DemandLearningActions

open System
open System.Threading.Tasks
open Medhavi.Contracts.Demand
open Medhavi.Client.ViewComposition.Identity
open Medhavi.Client.ViewComposition.Component
open Medhavi.Client.ActionExecution
open Medhavi.Client

let private resolveLearning (ctx: ActionExecutionContext) : Task<DemandLearningDto option> =
    task {
        match ctx.TargetKey with
        | Some key ->
            let! dtoOpt = ctx.Domain.Demand.LearnFromDemand.Queries.GetById key
            return dtoOpt
        | None -> return None
    }

let deriveLearnings: ActionHandler =
    fun ctx ->
        task {
            let api = ctx.Domain.Demand.LearnFromDemand.Commands

            let scope =
                match ctx.Payload with
                | CustomParameters p ->
                    match Map.tryFind "Scope" p with
                    | Some (StringValue s) -> s
                    | _ -> "SCOPE-GLOBAL-DEMAND"
                | _ -> "SCOPE-GLOBAL-DEMAND"

            let now = DateTimeOffset.UtcNow
            let wStart = now.AddMonths(-3)
            let wEnd = now

            let emptyBundle: HistoricalDemandEvidenceBundleDto =
                { Scope = scope
                  WindowStart = wStart
                  WindowEnd = wEnd
                  QualityAssessments = []
                  PlannerOverrides = []
                  DemandExceptions = []
                  ClassificationChanges = [] }

            let req: DeriveDemandLearningsReq =
                { Scope = scope
                  WindowStart = wStart
                  WindowEnd = wEnd
                  EvidenceBundle = emptyBundle }

            match! api.DeriveLearnings req with
            | Ok learnings ->
                return
                    Ok(
                        ActionExecutionOutcome.Completed(
                            $"Derived {learnings.Length} learning insights for scope '{scope}'.",
                            [ DemandLearningGrid.storeId ]
                        )
                    )
            | Error err -> return Error err.Message
        }

let recordLearning: ActionHandler =
    fun ctx ->
        task {
            let api = ctx.Domain.Demand.LearnFromDemand.Commands
            let! existingOpt = resolveLearning ctx

            let lId, scope, lType, statement, patConf, intConf, polVer =
                match existingOpt with
                | Some dto ->
                    dto.LearningId,
                    dto.Scope,
                    dto.LearningType,
                    dto.LearningStatement,
                    dto.PatternConfidence,
                    dto.InterventionConfidence,
                    dto.PolicyVersion
                | None ->
                    let id = ctx.TargetKey |> Option.defaultValue ($"LRN-{Guid.NewGuid().ToString().Substring(0, 8)}")
                    id,
                    "SCOPE-GLOBAL-DEMAND",
                    "SystemicBiasReduction",
                    "Forecast model consistently overforecasting by +8% in mid-tier volume SKUs; recommend auto-damping alpha parameter.",
                    0.92m,
                    0.88m,
                    "v1.0"

            let req: RecordDemandLearningReq =
                { LearningId = lId
                  Scope = scope
                  LearningType = lType
                  LearningStatement = statement
                  PatternConfidence = patConf
                  InterventionConfidence = intConf
                  SupportingEvidence = []
                  ImprovementOpportunities = []
                  PolicyVersion = polVer }

            match! api.RecordLearning req with
            | Ok dto ->
                return
                    Ok(
                        ActionExecutionOutcome.Completed(
                            $"Learning insight '{dto.LearningId}' permanently recorded for '{dto.Scope}'.",
                            [ DemandLearningGrid.storeId ]
                        )
                    )
            | Error err -> return Error err.Message
        }

let all: (ActionDefId * ActionHandler) list =
    [ DemandLearningGrid.DemandLearningActions.deriveLearnings.Id, deriveLearnings
      DemandLearningGrid.DemandLearningActions.recordLearning.Id, recordLearning ]

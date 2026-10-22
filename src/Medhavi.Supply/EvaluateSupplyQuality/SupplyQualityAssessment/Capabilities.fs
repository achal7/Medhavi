/// SE-S-100 — Supply Quality Assessment Aggregate Capabilities
/// Traces to: FS-S-090 (Evaluate Supply Performance)
module Medhavi.Supply.EvaluateSupplyQuality.SupplyQualityAssessment.Capabilities

open System.Threading.Tasks
open Medhavi.Common
open Medhavi.Foundation.Contracts
open Medhavi.Foundation.Execution
open Medhavi.Foundation.Execution.CommandCapabilities
open Medhavi.Foundation.Failure
open Medhavi.Foundation.Observation
open Medhavi.SemanticModel
open Medhavi.Contracts.Supply
open Medhavi.Supply
open Model
open Policies

type SupplyQualityAssessmentAggregateApi =
    {
        Evaluate: EvaluateSupplyQualityReq -> Task<Result<SupplyQualityAssessment, ApplicationError>>
    }

let create
    (repo: Repository<SupplyQualityAssessment, SupplyQualityAssessmentId, SupplyQualityAssessmentEvent>)
    (publishKnowledge: KnowledgeRepresentation)
    (policy: SupplyQualityPolicy)
    (deps: AggregateStages.EnvelopeStoreDependencies<SupplyQualityAssessmentEvent>)
    (ports: SupplyPorts)
    : SupplyQualityAssessmentAggregateApi =

    let evaluate (req: EvaluateSupplyQualityReq) : Task<Result<SupplyQualityAssessment, ApplicationError>> =
        taskResult {
            let! scopeId =
                PlanningScopeId.create req.PlanningScopeId
                |> Result.mapError(fun err -> Domain(mapSemanticValidationToDomainError err))
                |> TaskResult.ofResult

            let! periodId =
                EvaluationPeriodId.create req.EvaluationPeriodId |> Result.mapError Domain |> TaskResult.ofResult

            let! horizon =
                PlanningHorizon.createFromString req.PlanningHorizon
                |> Result.mapError(mapSemanticValidationToDomainError >> ApplicationError.fromDomainError)
                |> TaskResult.ofResult

            do! requireEntityExists ports.PlanningScopeExists scopeId "PlanningScope" PlanningScopeId.value
            do! requireEntityExists ports.EvaluationPeriodExists periodId "EvaluationPeriod" EvaluationPeriodId.value

            let! prereqs = protect(ports.GetScopePrerequisites scopeId horizon periodId)
            let! outcomes = protect(ports.GetDimensionalOutcomesForScope scopeId horizon periodId)
            let! evalCtx = protect(ports.GetQualityEvaluationContext scopeId periodId)

            let! cmd = liftValidation(ACL.toEvaluateCmd req prereqs outcomes evalCtx)

            let pipeline =
                CommandPipeline.create
                    repo
                    (fun (c: EvaluateSupplyQualityCmd) ->
                        SupplyQualityAssessmentId.ofScopeHorizonPeriod
                            c.PlanningScopeId
                            c.PlanningHorizon
                            c.EvaluationPeriodId)
                    (Behaviors.evaluateAndPublish policy)
                    deps

            return! runPipeline pipeline publishKnowledge id cmd
        }

    { Evaluate = evaluate }

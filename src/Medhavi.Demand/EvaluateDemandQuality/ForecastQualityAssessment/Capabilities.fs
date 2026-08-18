module Medhavi.Demand.EvaluateDemandQuality.ForecastQualityAssessment.Capabilities

open System.Threading.Tasks
open Medhavi.Common
open Medhavi.Foundation.Contracts
open Medhavi.Foundation.Execution
open Medhavi.Foundation.Execution.AggregateStages
open Medhavi.Foundation.Execution.CommandCapabilities
open Medhavi.Foundation.Failure
open Medhavi.Foundation.Observation
open Medhavi.Contracts.Demand
open Medhavi.Demand
open Model
open Policies

type AggregateApi =
    { Evaluate: EvaluateForecastQualityReq -> Task<Result<ForecastQualityAssessment, ApplicationError>>
      Publish: PublishForecastQualityAssessmentReq -> Task<Result<ForecastQualityAssessment, ApplicationError>> }

let private liftValidation validationResult =
    validationResult
    |> Validation.toResult
    |> Result.mapError(fun errs ->
        let domainErrs = errs |> List.map(fun e -> ("", $"{e}"))
        ApplicationError.Validation domainErrs)

let create
    (repo: Repository<ForecastQualityAssessment, ForecastQualityAssessmentId, ForecastQualityEvent>)
    (publishKnowledge: KnowledgeRepresentation)
    (policy: ForecastMeasurementPolicy)
    (deps: EnvelopeStoreDependencies<ForecastQualityEvent>)
    : AggregateApi =

    let evaluate (req: EvaluateForecastQualityReq) : Task<Result<ForecastQualityAssessment, ApplicationError>> =
        taskResult {
            let! cmd = liftValidation(ACL.toEvaluateCmd req)

            let pipeline =
                CommandPipeline.create
                    repo
                    (fun (c: EvaluateForecastQualityCmd) -> c.AssessmentId)
                    (Behaviors.evaluateForecastQuality policy)
                    deps

            return! runPipeline pipeline publishKnowledge id cmd
        }

    let publish (req: PublishForecastQualityAssessmentReq) : Task<Result<ForecastQualityAssessment, ApplicationError>> =
        taskResult {
            let! cmd = liftValidation(ACL.toPublishCmd req)

            let pipeline =
                CommandPipeline.create
                    repo
                    (fun (c: PublishForecastQualityAssessmentCmd) -> c.AssessmentId)
                    (Behaviors.publishForecastQualityAssessment policy)
                    deps

            return! runPipeline pipeline publishKnowledge id cmd
        }

    { Evaluate = evaluate
      Publish = publish }

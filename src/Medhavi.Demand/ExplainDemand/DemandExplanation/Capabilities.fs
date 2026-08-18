module Medhavi.Demand.ExplainDemand.DemandExplanation.Capabilities

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
    { Establish: EstablishDemandExplanationReq -> Task<Result<DemandExplanation, ApplicationError>>
      GetById: DemandExplanationId -> Task<Result<DemandExplanation option, ApplicationError>> }

let private liftValidation validationResult =
    validationResult
    |> Validation.toResult
    |> Result.mapError(fun errs ->
        let domainErrs = errs |> List.map(fun e -> ("", $"{e}"))
        ApplicationError.Validation domainErrs)

let create
    (repo: Repository<DemandExplanation, DemandExplanationId, DemandExplanationEvent>)
    (publishKnowledge: KnowledgeRepresentation)
    (policy: ExplanationGovernancePolicy)
    (deps: EnvelopeStoreDependencies<DemandExplanationEvent>)
    : AggregateApi =

    let establish (req: EstablishDemandExplanationReq) : Task<Result<DemandExplanation, ApplicationError>> =
        taskResult {
            let! cmd = liftValidation(ACL.toEstablishCmd policy req)

            // 1. Check idempotency: If explanation already exists for this exact artifact version, return existing
            let! existingOpt =
                repo.Get cmd.ExplanationId
                |> TaskResult.ofTask
                |> TaskResult.mapError(fun repoErr -> Infrastructure(Database(sprintf "%A" repoErr)))

            match existingOpt with
            | Some existing -> return existing
            | None ->
                let pipeline =
                    CommandPipeline.create
                        repo
                        (fun (c: EstablishExplanationCmd) -> c.ExplanationId)
                        (Behaviors.establishExplanation policy)
                        deps

                return! runPipeline pipeline publishKnowledge id cmd
        }

    let getById (id: DemandExplanationId) : Task<Result<DemandExplanation option, ApplicationError>> =
        repo.Get id
        |> TaskResult.ofTask
        |> TaskResult.mapError(fun repoErr -> Infrastructure(Database(sprintf "%A" repoErr)))

    { Establish = establish
      GetById = getById }

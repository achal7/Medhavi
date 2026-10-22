/// Aggregate-level CommandPipeline execution for SE-S-120 Supply Explanation
module Medhavi.Supply.ExplainSupplyDecisions.SupplyExplanation.Capabilities

open System.Threading.Tasks
open Medhavi.Common
open Medhavi.Foundation.Contracts
open Medhavi.Foundation.Execution
open Medhavi.Foundation.Execution.AggregateStages
open Medhavi.Foundation.Execution.CommandCapabilities
open Medhavi.Foundation.Failure
open Medhavi.Foundation.Observation
open Medhavi.Contracts.Supply
open Medhavi.Supply
open Model
open Policies

type AggregateApi =
    { Record: DecisionTrace list -> RecordSupplyExplanationReq -> Task<Result<SupplyExplanation, ApplicationError>>
      GetById: SupplyExplanationId -> Task<Result<SupplyExplanation option, ApplicationError>> }

let private liftValidation validationResult =
    validationResult
    |> Validation.toResult
    |> Result.mapError (fun errs ->
        let domainErrs = errs |> List.map (fun e -> ("", $"{e}"))
        ApplicationError.Validation domainErrs)

let create
    (repo: Repository<SupplyExplanation, SupplyExplanationId, SupplyExplanationEvent>)
    (publishKnowledge: KnowledgeRepresentation)
    (policy: ExplanationPolicy)
    (deps: EnvelopeStoreDependencies<SupplyExplanationEvent>)
    : AggregateApi =

    let record (decisionTraces: DecisionTrace list) (req: RecordSupplyExplanationReq) : Task<Result<SupplyExplanation, ApplicationError>> =
        taskResult {
            let! cmd = liftValidation (ACL.toComposeCmd policy decisionTraces req)

            // Idempotency guarantee per FS-S-110: If explanation already exists for this exact artifact version, return existing
            let! existingOpt =
                repo.Get cmd.ExplanationId
                |> TaskResult.ofTask
                |> TaskResult.mapError (fun repoErr -> Infrastructure(Database(sprintf "%A" repoErr)))

            match existingOpt with
            | Some existing -> return existing
            | None ->
                let pipeline =
                    CommandPipeline.create
                        repo
                        (fun (c: ComposeSupplyExplanationCmd) -> c.ExplanationId)
                        (Behaviors.composeSupplyExplanation policy)
                        deps

                return! runPipeline pipeline publishKnowledge id cmd
        }

    let getById (id: SupplyExplanationId) : Task<Result<SupplyExplanation option, ApplicationError>> =
        repo.Get id
        |> TaskResult.ofTask
        |> TaskResult.mapError (fun repoErr -> Infrastructure(Database(sprintf "%A" repoErr)))

    { Record = record
      GetById = getById }

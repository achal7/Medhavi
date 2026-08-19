module Medhavi.Core.DemandManagement.Capabilities

open System.Threading.Tasks
open Medhavi.Common
open Medhavi.Foundation.Contracts
open Medhavi.Foundation.Observation
open Medhavi.Foundation.Execution
open Medhavi.Foundation.Execution.CommandCapabilities
open Medhavi.Foundation.Failure
open Medhavi.SemanticModel
open Medhavi.Contracts
open Medhavi.Contracts.Core.Demand
open Medhavi.Core
open Medhavi.Core.ArsIdentifiers
open Model
open Policies

let create
    (repo: Repository<Demand, DemandId, DemandEvent>)
    (publishKnowledge: KnowledgeRepresentation)
    (policy: DemandManagementPolicy)
    (deps: AggregateStages.EnvelopeStoreDependencies<DemandEvent>)
    : DemandApi =

    let record (req: RecordDemandReq) : Task<Result<DemandDto, ApiError>> =
        taskResult {
            let! cmd = liftValidation(ACL.toRecordCmd req)

            // Execute pipeline
            let pipeline =
                CommandPipeline.create repo (fun (c: RecordDemandCmd) -> c.DemandId) (Behaviors.record policy) deps

            return! runPipeline pipeline publishKnowledge Projections.mapToDto cmd
        }
        |> TaskResult.mapError mapAppErrorToApiError

    let satisfy (req: SatisfyDemandReq) : Task<Result<DemandDto, ApiError>> =
        taskResult {
            let! cmd:SatisfyDemandCmd = liftValidation(ACL.toSatisfyCmd req)

            // FIRST GATE: Ensure demand exists (from aggregate, not projection)
            let! demand: Demand =
                repo.Get cmd.DemandId
                |> TaskResult.mapError Repository.mapRepositoryErrorToApplicationError
                |> TaskResult.ofOption(
                    DomainError.notFound("Demand", (DemandId.value cmd.DemandId)) |> ApplicationError.fromDomainError
                )

            let pipeline = CommandPipeline.create repo (fun (c: SatisfyDemandCmd) -> c.DemandId) Behaviors.satisfy deps
            // Execute pipeline
            return! runPipeline pipeline publishKnowledge Projections.mapToDto cmd
        }
        |> TaskResult.mapError mapAppErrorToApiError

    let cancel (req: CancelDemandReq) : Task<Result<DemandDto, ApiError>> =
        taskResult {
            let! cmd:CancelDemandCmd = liftValidation(ACL.toCancelCmd req)

            // FIRST GATE: Ensure demand exists (from aggregate, not projection)
            let! demand: Demand =
                repo.Get cmd.DemandId
                |> TaskResult.mapError Repository.mapRepositoryErrorToApplicationError
                |> TaskResult.ofOption(
                    DomainError.notFound("Demand", (DemandId.value cmd.DemandId)) |> ApplicationError.fromDomainError
                )

            // Execute pipeline
            let pipeline = CommandPipeline.create repo (fun (c: CancelDemandCmd) -> c.DemandId) Behaviors.cancel deps
            return! runPipeline pipeline publishKnowledge Projections.mapToDto cmd
        }
        |> TaskResult.mapError mapAppErrorToApiError

    { Record = record
      Satisfy = satisfy
      Cancel = cancel }

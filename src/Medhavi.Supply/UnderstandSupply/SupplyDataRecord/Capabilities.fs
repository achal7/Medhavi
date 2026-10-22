module Medhavi.Supply.UnderstandSupply.SupplyDataRecord.Capabilities

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

type SupplyDataRecordAggregateApi =
    { Receive: ReceiveSupplyDataReq -> Task<Result<SupplyDataRecord, ApplicationError>>
      Evaluate: EvaluateSupplyDataReq -> Task<Result<SupplyDataRecord, ApplicationError>> }

let private ensureNoDuplicate
    (repo: Repository<SupplyDataRecord, SupplyDataRecordId, RecordEvent>)
    (policy: SupplyDataAcceptancePolicy)
    (recordId: SupplyDataRecordId option)
    (item: ItemId)
    (location: LocationId)
    (quantity: Quantity)
    (businessTime: Timestamp)
    (observationTime: Timestamp)
    : TaskResult<unit, ApplicationError> =
    taskResult {
        let windowStart =
            (Timestamp.value observationTime).Subtract(policy.DuplicateDetectionWindow)
            |> Timestamp.create
            |> Result.mapError (mapSemanticValidationToDomainError >> ApplicationError.fromDomainError)
            |> TaskResult.ofResult

        let! wStart = windowStart
        let windowEnd = observationTime
        let! records: SupplyDataRecord list = repo.GetAll() |> TaskResult.mapError Repository.mapRepositoryErrorToApplicationError

        let recentRecords =
            records
            |> List.filter(fun r ->
                r.Item = item
                && r.Location = location
                && r.ObservationTime >= wStart
                && r.ObservationTime <= windowEnd)

        let isDuplicate =
            recentRecords
            |> List.exists(fun rr ->
                let isDifferentRecord =
                    match recordId with
                    | Some id -> rr.RecordId <> id
                    | None -> true

                isDifferentRecord
                && rr.Quantity = quantity
                && rr.BusinessTime = businessTime)

        if isDuplicate then
            return! TaskResult.fail(Domain(DomainError.conflict $"{Rules.duplicateDataDetection.Id}: Duplicate transaction within duplicate detection window"))
        else
            return ()
    }

let create
    (repo: Repository<SupplyDataRecord, SupplyDataRecordId, RecordEvent>)
    (publishKnowledge: KnowledgeRepresentation)
    (policy: SupplyDataAcceptancePolicy)
    (deps: AggregateStages.EnvelopeStoreDependencies<RecordEvent>)
    (ports: SupplyPorts)
    : SupplyDataRecordAggregateApi =

    let receive (req: ReceiveSupplyDataReq) : Task<Result<SupplyDataRecord, ApplicationError>> =
        taskResult {
            let! cmd = liftValidation (ACL.toReceiveCmd req)

            do! requireEntityExists ports.ItemExists cmd.Item "Item" ItemId.value
            let! (_: unit) =
                match cmd.SupplierIdentifier with
                | Some supp -> requireEntityExists ports.SupplierExists supp "Supplier" SupplierId.value
                | None -> TaskResult.return' ()

            let! (_: unit) = ensureNoDuplicate repo policy (Some cmd.RecordId) cmd.Item cmd.Location cmd.Quantity cmd.BusinessTime cmd.ObservationTime

            let pipeline = CommandPipeline.create repo (fun (c: ReceiveRecordCmd) -> c.RecordId) Behaviors.receive deps
            return! runPipeline pipeline publishKnowledge id cmd
        }

    let evaluate (req: EvaluateSupplyDataReq) : Task<Result<SupplyDataRecord, ApplicationError>> =
        taskResult {
            let! cmd: EvaluateRecordCmd = liftValidation (ACL.toEvaluateCmd req)

            let! recrd: SupplyDataRecord =
                repo.Get cmd.RecordId
                |> TaskResult.mapError Repository.mapRepositoryErrorToApplicationError
                |> TaskResult.ofOption (DomainError.notFound("SupplyDataRecord", req.RecordId) |> ApplicationError.fromDomainError)

            let! sourceReliability = ports.SourceReliability recrd.SourceSystemProvenance

            let! (_: unit) = ensureNoDuplicate repo policy (Some cmd.RecordId) recrd.Item recrd.Location recrd.Quantity cmd.EvaluationTime recrd.ObservationTime

            let pipeline = CommandPipeline.create repo (fun _ -> cmd.RecordId) (Behaviors.evaluate policy sourceReliability false) deps
            return! runPipeline pipeline publishKnowledge id cmd
        }

    { Receive = receive
      Evaluate = evaluate }

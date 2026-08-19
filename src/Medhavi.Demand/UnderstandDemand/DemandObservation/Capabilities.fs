module Medhavi.Demand.UnderstandDemand.DemandObservation.Capabilities

open System.Threading.Tasks
open Medhavi.Common
open Medhavi.Foundation.Contracts
open Medhavi.Foundation.Execution
open Medhavi.Foundation.Execution.CommandCapabilities
open Medhavi.Foundation.Failure
open Medhavi.Foundation.Observation
open Medhavi.SemanticModel
open Medhavi.Contracts.Demand
open Medhavi.Demand
open Medhavi.Demand.UnderstandDemand.DemandObservation
open Model
open Policies

type DemandObservationAggregateApi =
    { Receive: ReceiveObservationReq -> Task<Result<DemandObservation, ApplicationError>>
      Evaluate: EvaluateObservationReq -> Task<Result<DemandObservation, ApplicationError>> }


let private ensureNoDuplicate
    (repo: Repository<DemandObservation, DemandObservationId, ObservationEvent>)
    (policy: DemandDataAcceptancePolicy)
    (obsId: Option<DemandObservationId>)
    (item: ItemId)
    (location: LocationId)
    (quantity: Quantity)
    (businessTime: Timestamp)
    (observationTime: Timestamp)
    : TaskResult<unit, ApplicationError> =
    taskResult {
        let! windowStart: Timestamp = (Timestamp.value observationTime).AddHours(-float policy.DuplicateDetectionWindowHours) |> Timestamp.create |> Result.mapError (mapSemanticValidationToDomainError >> ApplicationError.fromDomainError) |> TaskResult.ofResult
        let windowEnd = observationTime
        let! observations:DemandObservation list = repo.GetAll() |> TaskResult.mapError Repository.mapRepositoryErrorToApplicationError
        let recentObs = observations |> List.filter (fun o -> o.Item = item && o.Location = location && o.ObservationTime >= windowStart && o.ObservationTime <= windowEnd)

        let isDuplicate =
            recentObs
            |> List.exists (fun ro ->
                let isDifferentObservation = 
                    match obsId with
                    | Some id -> ro.ObservationId <> id
                    | None -> true
                
                isDifferentObservation 
                && ro.Quantity = quantity 
                && ro.BusinessTime = businessTime)

        if isDuplicate then
            return! TaskResult.fail(Domain(DomainError.conflict $"{Rules.duplicateDataDetection.Id}: Duplicate within window"))
        else
            return ()
    }

let create
    (repo: Repository<DemandObservation, DemandObservationId, ObservationEvent>)
    (publishKnowledge: KnowledgeRepresentation)
    (policy: DemandDataAcceptancePolicy)
    (deps: AggregateStages.EnvelopeStoreDependencies<ObservationEvent>)
    (ports: DemandPorts)
    : DemandObservationAggregateApi =

    let receive (req: ReceiveObservationReq) : Task<Result<DemandObservation, ApplicationError>> =
        taskResult {
            // Step 1: Lift validation
            let! cmd = liftValidation (ACL.toReceiveCmd req)

            // Step 2: Ensure Item exists (using primitive)
            do! requireEntityExists ports.ItemExists cmd.Item "Item" ItemId.value

            // Step 3: Ensure Location exists (using primitive)
            do! requireEntityExists ports.LocationExists cmd.Location "Location" LocationId.value

            // Step 4: Ensure no duplicate (custom guard)
            let! _:unit = ensureNoDuplicate repo policy (Some cmd.ObservationId) cmd.Item cmd.Location cmd.Quantity cmd.BusinessTime cmd.ObservationTime

            // Step 5: Execute pipeline
            let pipeline = CommandPipeline.create repo (fun (c:ReceiveObservationCmd) -> c.ObservationId) Behaviors.receive deps
            return! runPipeline
                        pipeline
                        publishKnowledge
                        id
                        cmd
        }

    let evaluate (req: EvaluateObservationReq) : Task<Result<DemandObservation, ApplicationError>> =
        taskResult {
            // Step 1: Lift validation
            let! cmd:EvaluateObservationCmd = liftValidation (ACL.toEvaluateCmd req)

            // Step 2: Fetch current observation context from Read Model
            let! obs: DemandObservation = 
                repo.Get cmd.ObservationId
                |> TaskResult.mapError(Repository.mapRepositoryErrorToApplicationError)
                |> TaskResult.ofOption (DomainError.notFound("DemandObservation", req.ObservationId) |> ApplicationError.fromDomainError)
            
            // Step 3: Fetch source reliability from port
            let! sourceReliability = ports.SourceReliability obs.SourceSystemProvenance

            // Step 4: Check duplicates dynamically within the governed window
            let! _:unit = ensureNoDuplicate repo policy (Some cmd.ObservationId) obs.Item obs.Location obs.Quantity cmd.EvaluationTime obs.ObservationTime

            // Step 5: Execute pipeline
            let pipeline = CommandPipeline.create repo (fun _ -> cmd.ObservationId) (Behaviors.evaluate policy sourceReliability false) deps
            return! runPipeline
                        pipeline
                        publishKnowledge
                        id
                        cmd
        }

    { Receive = receive
      Evaluate = evaluate }
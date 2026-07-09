namespace Medhavi.Demand.DemandObservation

open System.Threading.Tasks
open Medhavi.Common.Validation
open Medhavi.SharedKernel
open Medhavi.SharedKernel.Execution
open Medhavi.SharedKernel.Failure
open Medhavi.SharedKernel.Observation
open Medhavi.SharedKernel.Contracts.Aggregate
open Medhavi.Contracts.Demand.DemandObservation
open Medhavi.Demand
open Medhavi.Demand.DemandObservation.ACL
open Medhavi.Demand.DemandObservation.Model

// Traceability: Implements CA-D-001 (Understand Demand) Capabilities API for SE-D-001 (Demand Observation)
// Exposes the workflow layer: validates raw requests, runs algorithms, calls CommandHandler, publishes notifications.

module Capabilities =

    let createCapabilities
        (execute: ObservationCommand -> Task<ExecutionOutcome<DemandObservation, ApplicationError>>)
        (repo: Repository<DemandObservation, string, ObservationEvent>)
        (publishKnowledge: ArchitecturalKnowledge -> unit)
        : DemandObservationApi =

        /// FS-D-001 — Receive Business Observation
        let receive (req: EstablishObservationReq) =
            task {
                // 1. EARLY VALIDATION: Validate raw request at the boundary
                match toEstablishCmd req with
                | Invalid errors ->
                    let apiErr = ApplicationError.Domain(DomainError.combineValidationErrors errors) |> ApplicationError.mapToApiError
                    return Error apiErr
                | Valid cmd ->
                    // 2. EXECUTE COMMAND: Dispatch to internal CommandHandler corridor
                    let! outcome = execute (Establish cmd)
                    match outcome with
                    | Completed obs ->
                        // 3. PUBLISH NOTIFICATION: Publish BN-D-005 Demand Observation Received
                        DomainEventBus.Publish({ ObservationId = DemandObservationId.value obs.Id }: ObservationReceivedNotification)
                        return Ok (DemandObservationId.value obs.Id)
                    | Failed err ->
                        // If rejection occurs due to a business rule, the error message/rationale contains the rule information
                        return Error (ApplicationError.mapToApiError err)
                    | Cancelled -> 
                        return Error { Code = "CANCELLED"; Category = "Infrastructure"; Message = "Operation cancelled" }
            }

        /// FS-D-001 — Receive Business Observation Batch
        let receiveBatch (req: EstablishObservationBatchReq) =
            task {
                // 1. EARLY VALIDATION: Validate raw batch request at the boundary
                match toEstablishBatchCmd req with
                | Invalid errors ->
                    let apiErr = ApplicationError.Domain(DomainError.combineValidationErrors errors) |> ApplicationError.mapToApiError
                    return Error apiErr
                | Valid cmds ->
                    // 2. RUN BUSINESS LOGIC: Decide on each individual establish command
                    let decisionsRes =
                        cmds |> List.map(fun cmd -> Decisions.decide (Establish cmd) None) |> Medhavi.Common.Result.sequence

                    match decisionsRes with
                    | Error err -> 
                        return Error (ApplicationError.Domain err |> ApplicationError.mapToApiError)
                    | Ok decisions ->
                        let batch =
                            decisions
                            |> List.map(fun d ->
                                let id = d.NewState.Id |> DemandObservationId.value
                                (id, d.NewState, d.Events))

                        // 3. PERSIST BATCH: Save state in repository
                        let! saveRes = repo.SaveBatch batch

                        match saveRes with
                        | Error err -> 
                            return Error (mapRepositoryErrorToApplicationError err |> ApplicationError.mapToApiError)
                        | Ok() ->
                            decisions
                            |> List.iter(fun d ->
                                d.Trace
                                |> Option.iter(fun t ->
                                    let k: ArchitecturalKnowledge =
                                        { Name = "DecisionEvaluated"
                                          Timestamp = System.DateTimeOffset.UtcNow
                                          Attributes =
                                            Map.ofList [ "DecisionId", box t.DecisionId; "DecisionTrace", box t ] }

                                    publishKnowledge k))

                            // 4. PUBLISH NOTIFICATION: Publish BN-D-005 batch notification
                            let ids = decisions |> List.map(fun d -> DemandObservationId.value d.NewState.Id)
                            DomainEventBus.Publish({ ObservationIds = ids }: ObservationBatchReceivedNotification)
                            return Ok ids
            }

        /// FS-D-002 — Evaluate Business Observation (Acceptance rules like timeliness, bounds, reliability)
        let evaluate (req: EvaluateObservationReq) =
            task {
                // 1. EARLY VALIDATION: Validate evaluation request
                match toEvaluateCmd req with
                | Invalid errors ->
                    let apiErr = ApplicationError.Domain(DomainError.combineValidationErrors errors) |> ApplicationError.mapToApiError
                    return Error apiErr
                | Valid cmd ->
                    // 2. EXECUTE COMMAND: Evaluate observation through the decisions pipeline
                    let! outcome = execute (Evaluate cmd)
                    match outcome with
                    | Completed obs -> 
                        return Ok (DemandObservationId.value obs.Id)
                    | Failed err -> 
                        // Cover rule/policy context in the rejection reason (e.g. BR-D-010, BR-D-011, BR-D-012, BR-D-014)
                        return Error (ApplicationError.mapToApiError err)
                    | Cancelled -> 
                        return Error { Code = "CANCELLED"; Category = "Infrastructure"; Message = "Operation cancelled" }
            }

        /// Assign Scope (BR-D-004: Accepted observation must belong to exactly one Planning Scope)
        let assignScope (req: AssignScopeReq) =
            task {
                // 1. EARLY VALIDATION: Validate assign scope request
                match toAssignScopeCmd req with
                | Invalid errors ->
                    let apiErr = ApplicationError.Domain(DomainError.combineValidationErrors errors) |> ApplicationError.mapToApiError
                    return Error apiErr
                | Valid cmd ->
                    // 2. EXECUTE COMMAND: Assign scope via pipeline
                    let! outcome = execute (AssignScope cmd)
                    match outcome with
                    | Completed obs -> 
                        return Ok (DemandObservationId.value obs.Id)
                    | Failed err -> 
                        return Error (ApplicationError.mapToApiError err)
                    | Cancelled -> 
                        return Error { Code = "CANCELLED"; Category = "Infrastructure"; Message = "Operation cancelled" }
            }

        { Receive = receive
          ReceiveBatch = receiveBatch
          Evaluate = evaluate
          AssignScope = assignScope }

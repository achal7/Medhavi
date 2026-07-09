namespace Medhavi.Demand.PlanningScope

open System.Threading.Tasks
open Medhavi.Common.Validation
open Medhavi.Contracts.Demand.PlanningScope
open Medhavi.SharedKernel
open Medhavi.SharedKernel.Execution
open Medhavi.SharedKernel.Failure
open Medhavi.Demand
open Medhavi.Demand.PlanningScope.ACL
open Medhavi.Demand.PlanningScope.Model

// Traceability: Implements CA-D-001 (Understand Demand) Capabilities API for SE-D-002 (Planning Scope)
// Exposes the workflow layer: validates raw requests, runs algorithms, calls CommandHandler, publishes notifications.

module Capabilities =

    let createCapabilities
        (execute: PlanningScopeCommand -> Task<ExecutionOutcome<PlanningScope, ApplicationError>>)
        : PlanningScopeApi =

        /// FS-D-004 — Determine Planning Scope (Identity uniqueness BR-D-025, active limits BR-D-027)
        let determine (req: DeterminePlanningScopeReq) =
            task {
                // 1. EARLY VALIDATION: Validate raw request at the boundary
                match toDetermineCmd req with
                | Invalid errors ->
                    let apiErr = ApplicationError.Domain(DomainError.combineValidationErrors errors) |> ApplicationError.mapToApiError
                    return Error apiErr
                | Valid cmd ->
                    // 2. EXECUTE COMMAND: Dispatch to internal CommandHandler corridor
                    let! outcome = execute (Determine cmd)
                    match outcome with
                    | Completed scope -> 
                        return Ok (PlanningScopeId.value scope.Id)
                    | Failed err -> 
                        // Covered by decisions: BR-D-025, BR-D-027
                        return Error (ApplicationError.mapToApiError err)
                    | Cancelled -> 
                        return Error { Code = "CANCELLED"; Category = "Infrastructure"; Message = "Operation cancelled" }
            }

        /// Archive Planning Scope (BR-D-048 — Never deleted, only archived)
        let archive (req: ArchivePlanningScopeReq) =
            task {
                // 1. EARLY VALIDATION: Validate raw request at the boundary
                match toArchiveCmd req with
                | Invalid errors ->
                    let apiErr = ApplicationError.Domain(DomainError.combineValidationErrors errors) |> ApplicationError.mapToApiError
                    return Error apiErr
                | Valid cmd ->
                    // 2. EXECUTE COMMAND: Dispatch to internal CommandHandler corridor
                    let! outcome = execute (Archive cmd)
                    match outcome with
                    | Completed _ -> 
                        return Ok ()
                    | Failed err -> 
                        // Cover BR-D-048 check in the failure reason
                        return Error (ApplicationError.mapToApiError err)
                    | Cancelled -> 
                        return Error { Code = "CANCELLED"; Category = "Infrastructure"; Message = "Operation cancelled" }
            }

        { Determine = determine
          Archive = archive }

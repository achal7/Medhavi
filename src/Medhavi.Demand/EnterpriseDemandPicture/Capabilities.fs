namespace Medhavi.Demand.EnterpriseDemandPicture

open System.Threading.Tasks
open Medhavi.Contracts
open Medhavi.Common.Validation
open Medhavi.Contracts.Demand.Edp
open Medhavi.SharedKernel
open Medhavi.SharedKernel.Execution
open Medhavi.SharedKernel.Failure
open Medhavi.Demand
open Medhavi.Demand.EnterpriseDemandPicture.ACL
open Medhavi.Demand.EnterpriseDemandPicture.Model

// Traceability: Implements CA-D-001 (Understand Demand) Capabilities API for SE-D-003 (Enterprise Demand Picture)
// Exposes the workflow layer: validates raw requests, runs algorithms, calls CommandHandler, publishes notifications.

module Capabilities =

    let createCapabilities
        (execute: EdpCommand -> Task<ExecutionOutcome<EnterpriseDemandPicture, ApplicationError>>)
        (getAdjustments: PlanningScopeId -> Task<Map<PlanningPeriod, Quantity>>)
        (getOverrides: PlanningScopeId -> Task<Map<PlanningPeriod, Quantity>>)
        : EnterpriseDemandPictureApi =

        /// Revise Enterprise Demand Picture with a new business observation
        let revise (req: ReviseEnterpriseDemandPictureReq) =
            task {
                // 1. EARLY VALIDATION: Validate raw request at the boundary
                match toReviseCmd req with
                | Invalid errors ->
                    let apiErr = ApplicationError.Domain(DomainError.combineValidationErrors errors) |> ApplicationError.mapToApiError
                    return Error apiErr
                | Valid cmd ->
                    // 2. EXECUTE COMMAND: Dispatch to internal CommandHandler corridor
                    let! outcome = execute (Revise cmd)
                    match outcome with
                    | Completed edp -> 
                        return Ok (PlanningScopeId.value edp.PlanningScopeId, edp.Version)
                    | Failed err -> 
                        // Cover immutable checks: BR-D-006 (Published), BR-D-056 (Superseded)
                        return Error (ApplicationError.mapToApiError err)
                    | Cancelled -> 
                        return Error { Code = "CANCELLED"; Category = "Infrastructure"; Message = "Operation cancelled" }
            }

        /// Calculate Enterprise Demand Picture planning demand line using BA-D-001 Planning Demand formula
        let calculate (req: CalculateEnterpriseDemandPictureReq) =
            task {
                // 1. EARLY VALIDATION: Validate raw request at the boundary
                match toCalculateCmd req with
                | Invalid errors ->
                    let apiErr = ApplicationError.Domain(DomainError.combineValidationErrors errors) |> ApplicationError.mapToApiError
                    // Publish BN-D-004 on validation failure
                    DomainEventBus.Publish(
                        { PlanningScopeId = req.PlanningScopeId
                          Reason = $"Validation Error: {apiErr.Message}. Fails BR-D-008 Planning Demand derivation." }
                        : EnterpriseDemandPictureRecalculationFailedNotification
                    )
                    return Error apiErr
                | Valid cmd ->
                    // Fetch adjustments and overrides from infrastructure/repositories
                    let! adjustments = getAdjustments cmd.PlanningScopeId
                    let! overrides = getOverrides cmd.PlanningScopeId
                    let enrichedCmd = { cmd with Adjustments = adjustments; Overrides = overrides }
                    // 2. EXECUTE COMMAND: Dispatch to internal CommandHandler corridor
                    let! outcome = execute (Calculate enrichedCmd)
                    match outcome with
                    | Completed edp -> 
                        return Ok (PlanningScopeId.value edp.PlanningScopeId, edp.Version)
                    | Failed err ->
                        // 3. FAILURE NOTIFICATION: Publish BN-D-004 Enterprise Demand Picture Recalculation Failed
                        DomainEventBus.Publish(
                            { PlanningScopeId = req.PlanningScopeId
                              Reason = $"Calculation Error: {err.Message}. Fails BR-D-008 Planning Demand derivation." }
                            : EnterpriseDemandPictureRecalculationFailedNotification
                        )
                        return Error (ApplicationError.mapToApiError err)
                    | Cancelled ->
                        DomainEventBus.Publish(
                            { PlanningScopeId = req.PlanningScopeId
                              Reason = "Operation cancelled during calculation corridor." }
                            : EnterpriseDemandPictureRecalculationFailedNotification
                        )
                        return Error { Code = "CANCELLED"; Category = "Infrastructure"; Message = "Operation cancelled" }
            }

        /// Publish Enterprise Demand Picture (implements DE-D-012, BR-D-005, BR-D-006, BR-D-056)
        let publish (req: PublishEnterpriseDemandPictureReq) =
            task {
                // 1. EARLY VALIDATION: Validate raw request at the boundary
                match toPublishCmd req with
                | Invalid errors ->
                    let apiErr = ApplicationError.Domain(DomainError.combineValidationErrors errors) |> ApplicationError.mapToApiError
                    return Error apiErr
                | Valid cmd ->
                    // 2. EXECUTE COMMAND: Dispatch to internal CommandHandler corridor
                    let! outcome = execute (Publish cmd)
                    match outcome with
                    | Completed edp ->
                        // 3. SUCCESS NOTIFICATION: Publish BN-D-001 Enterprise Demand Picture Published
                        DomainEventBus.Publish(
                            { PlanningScopeId = PlanningScopeId.value edp.PlanningScopeId
                              Version = edp.Version
                              PublicationTime =
                                edp.PublicationTime |> Option.map Timestamp.value |> Option.defaultValue System.DateTimeOffset.MinValue }
                            : EnterpriseDemandPicturePublishedNotification
                        )
                        return Ok (PlanningScopeId.value edp.PlanningScopeId, edp.Version)
                    | Failed err -> 
                        return Error (ApplicationError.mapToApiError err)
                    | Cancelled -> 
                        return Error { Code = "CANCELLED"; Category = "Infrastructure"; Message = "Operation cancelled" }
            }

        { Revise = revise
          Calculate = calculate
          Publish = publish }

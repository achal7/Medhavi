module Medhavi.SharedKernel.Execution.Helpers

open System.Threading.Tasks
open Medhavi.Contracts
open Medhavi.SharedKernel.Failure

let toApiResult (outcome: ExecutionOutcome<'TOk, ApplicationError>) : Result<'TOk, ApiError> =
    match outcome with
    | Completed ok -> Ok ok
    | Failed err -> Error(ApplicationError.mapToApiError err)
    | Cancelled ->
        Error
            { Code = "CANCELLED"
              Category = "Execution"
              Message = "Request was cancelled" }

let executeStep
    (onSuccess: 'Aggregate -> Task<Result<'Result, ApiError>>)
    (onFailure: (ApplicationError -> unit) option)
    (capabilityFn: 'Req -> Task<ExecutionOutcome<'Aggregate, ApplicationError>>)
    (req: 'Req)
    : Task<Result<'Result, ApiError>> =

    task {
        let! outcome = capabilityFn req

        match outcome with
        | Completed aggregate -> return! onSuccess aggregate
        | Failed err ->
            onFailure |> Option.iter (fun f -> f err)
            return Error(ApplicationError.mapToApiError err)
        | Cancelled ->
            return
                Error
                    { Code = "cancelled"
                      Category = "Infrastructure"
                      Message = "cancelled" }
    }

let runWorkflow
    (validate: 'Req -> Medhavi.Common.Validation.Validation<'Cmd, DomainError>)
    (execute: 'Cmd -> Task<ExecutionOutcome<'Aggregate, ApplicationError>>)
    (onSuccess: 'Aggregate -> Task<Result<'Result, ApiError>>)
    (req: 'Req)
    : Task<Result<'Result, ApiError>> =
    task {
        match validate req with
        | Medhavi.Common.Validation.Invalid errors ->
            let appErr = ApplicationError.Domain(DomainError.combineValidationErrors errors)
            return Error (ApplicationError.mapToApiError appErr)
        | Medhavi.Common.Validation.Valid cmd ->
            let! outcome = execute cmd
            match outcome with
            | Completed aggregate -> return! onSuccess aggregate
            | Failed err ->
                return Error (ApplicationError.mapToApiError err)
            | Cancelled ->
                return
                    Error
                        { Code = "CANCELLED"
                          Category = "Infrastructure"
                          Message = "Operation cancelled" }
    }

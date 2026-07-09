module Medhavi.SharedKernel.Execution.ExecutionApiBridge

open Medhavi.Contracts
open Medhavi.SharedKernel.Failure

let toApiResult (outcome: ExecutionOutcome<'TOk, ApplicationError>) : Result<'TOk, ApiError> =
        match outcome with
        | ExecutionOutcome.Completed ok -> Ok ok
        | ExecutionOutcome.Failed err  -> Error (ApplicationError.mapToApiError err)
        | ExecutionOutcome.Cancelled   ->
            Error { Code = "CANCELLED"; Category = "Execution"; Message = "Request was cancelled" }

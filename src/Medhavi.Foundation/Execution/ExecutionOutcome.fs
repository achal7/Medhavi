namespace Medhavi.Foundation.Execution

type ExecutionOutcome<'TOk, 'TError> =
    | Completed of 'TOk
    | Failed of 'TError
    | Cancelled

module ExecutionOutcome =

    /// Map the success value of an ExecutionOutcome.
    let map (f: 'TOk -> 'UOk) (outcome: ExecutionOutcome<'TOk, 'TError>) : ExecutionOutcome<'UOk, 'TError> =
        match outcome with
        | Completed ok -> Completed(f ok)
        | Failed err -> Failed err
        | Cancelled -> Cancelled

    /// Map the error value of an ExecutionOutcome.
    let mapError (f: 'TError -> 'UErr) (outcome: ExecutionOutcome<'TOk, 'TError>) : ExecutionOutcome<'TOk, 'UErr> =
        match outcome with
        | Completed ok -> Completed ok
        | Failed err -> Failed(f err)
        | Cancelled -> Cancelled

    /// Convert ExecutionOutcome to Result, treating Cancelled as an error.
    let toResult (cancelledError: 'TError) (outcome: ExecutionOutcome<'TOk, 'TError>) : Result<'TOk, 'TError> =
        match outcome with
        | Completed ok -> Ok ok
        | Failed err -> Error err
        | Cancelled -> Error cancelledError

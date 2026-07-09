namespace Medhavi.SharedKernel.Execution

type ExecutionOutcome<'TOk, 'TError> =
    | Completed of 'TOk
    | Failed of 'TError
    | Cancelled

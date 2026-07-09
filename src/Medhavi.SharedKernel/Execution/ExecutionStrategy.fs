namespace Medhavi.SharedKernel.Execution

type ExecutionAction =
    | Finish
    | Retry
    | Delay
    | Escalate
    | Compensate
    | DeadLetter
    | AwaitExternalDecision

type ExecutionStrategy<'TOk, 'TError> = ExecutionOutcome<'TOk, 'TError> -> ExecutionAction

module ExecutionStrategies =
    open Medhavi.SharedKernel.Failure

    let defaultStrategy : ExecutionStrategy<'TOk, 'TError> =
        fun outcome ->
            match outcome with
            | Completed _ -> Finish
            | Failed _ -> Finish
            | Cancelled -> Finish

    let retryOnInfrastructure : ExecutionStrategy<'TOk, ApplicationError> =
        fun outcome ->
            match outcome with
            | Failed (ApplicationError.Infrastructure _) -> Retry
            | _ -> Finish

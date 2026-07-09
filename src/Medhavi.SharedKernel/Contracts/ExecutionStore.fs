namespace Medhavi.SharedKernel.Contracts

type ExecutionId = string

type ExecutionStore<'Execution> = {
    Preserve: ExecutionId -> 'Execution -> unit
    Restore: ExecutionId -> 'Execution option
}

namespace Medhavi.SharedKernel.Execution

open Medhavi.SharedKernel.ExecutionContext
open Medhavi.SharedKernel.Contracts.Aggregate

type CommandContext<'Cmd, 'Agg, 'Event> = {
    Command: 'Cmd
    ExecutionCtx: ExecutionContext
    AggregateVersion: int64 option
    Decision: Decision<'Agg, 'Event> option
}

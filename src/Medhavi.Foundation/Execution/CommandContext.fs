namespace Medhavi.Foundation.Execution

open Medhavi.Foundation.Contracts
open Medhavi.Foundation.ExecutionContext

type CommandContext<'Cmd, 'Agg, 'Event> =
    { Command: 'Cmd
      ExecutionCtx: ExecutionContext
      AggregateVersion: int64 option
      Decision: Decision<'Agg, 'Event> option }

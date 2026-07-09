module Medhavi.SharedKernel.Execution.CommandCapabilities

open System
open System.Threading
open System.Threading.Tasks
open Medhavi.Common.Validation
open Medhavi.SharedKernel.Failure
open Medhavi.SharedKernel.Observation
open Medhavi.SharedKernel.ExecutionContext

let execute
    (pipeline: ExecutionPipeline<Execution<'Agg option, CommandContext<'Cmd, 'Agg, 'Event>>,
                                  ExecutionOutcome<'Agg, ApplicationError>>)
    (publishKnowledge: ArchitecturalKnowledge -> unit)
    (domainCmd: 'Cmd)
    : Task<ExecutionOutcome<'Agg, ApplicationError>> =

    let ctx = ExecutionContextHolder.TryGet() |> Option.defaultValue (ExecutionContext.create())
    let initialModel = {
        State = None
        Context = {
            Command = domainCmd
            ExecutionCtx = ctx
            AggregateVersion = None
            Decision = None
        }
    }
    ExecutionRunner.execute
        ExecutionStrategies.defaultStrategy
        pipeline
        initialModel
        publishKnowledge
        3
        (TimeSpan.FromMilliseconds 100.0)
        CancellationToken.None

/// MRP Pipeline Types — Core pipeline infrastructure for Railway-Oriented Programming (ROP)
/// FP Pattern: Kleisli composition, monadic bind, and custom pipeline builder
module Medhavi.Planning.Mrp.Pipeline.PipelineTypes

open System
open Medhavi.SharedKernel
open Medhavi.Planning.Mrp.Domain.Types
open Medhavi.Planning.Mrp.Domain.Errors

/// Pure pipeline step (synchronous)
type MrpStep<'input, 'output> = 'input -> MrpContext -> Result<'output * MrpContext, MrpStepError>

/// Async pipeline step (for asynchronous/IO-bound operations)
type MrpStepAsync<'input, 'output> = 'input -> MrpContext -> Async<Result<'output * MrpContext, MrpStepError>>

/// Step execution metadata
type StepMetadata =
    { Name: string
      Description: string
      Order: int
      IsRequired: bool
      CanRetry: bool
      Timeout: TimeSpan option }

/// Named step wrapping an async execution
type NamedStep<'input, 'output> =
    { Metadata: StepMetadata
      Execute: MrpStepAsync<'input, 'output> }

/// Execution result of a single step
type StepResult<'output> =
    { StepName: string
      Duration: TimeSpan
      Result: Result<'output, MrpStepError> }

/// Summary of a step result for tracking and diagnostics
type StepResultSummary =
    { StepName: string
      Duration: TimeSpan
      Success: bool
      ErrorMessage: string option }

/// Pipeline execution helpers
module Pipeline =

    /// Lift a synchronous step to an async step
    let liftAsync (step: MrpStep<'a, 'b>) : MrpStepAsync<'a, 'b> =
        fun input ctx -> async { return step input ctx }

    /// Monadic bind for composition (Kleisli bind)
    let bind (step1: MrpStepAsync<'a, 'b>) (step2: MrpStepAsync<'b, 'c>) : MrpStepAsync<'a, 'c> =
        fun input ctx ->
            async {
                let! res1 = step1 input ctx
                match res1 with
                | Error e -> return Error e
                | Ok (out1, ctx1) -> return! step2 out1 ctx1
            }

    /// Inline operator for bind
    let (>>=) = bind

    /// Map step output value
    let map (f: 'b -> 'c) (step: MrpStepAsync<'a, 'b>) : MrpStepAsync<'a, 'c> =
        fun input ctx ->
            async {
                let! res = step input ctx
                return res |> Result.map (fun (out, ctx') -> (f out, ctx'))
            }

    /// Tap step execution for side-effects (e.g. logging, events)
    let tap (f: 'b -> MrpContext -> unit) (step: MrpStepAsync<'a, 'b>) : MrpStepAsync<'a, 'b> =
        fun input ctx ->
            async {
                let! res = step input ctx
                match res with
                | Ok (out, ctx') ->
                    f out ctx'
                    return Ok (out, ctx')
                | Error e -> return Error e
            }

    /// Wrap a step with duration logging and context warnings
    let withTiming (stepName: string) (step: MrpStepAsync<'a, 'b>) : MrpStepAsync<'a, 'b> =
        fun input ctx ->
            async {
                let startTime = DateTimeOffset.UtcNow
                let! res = step input ctx
                let endTime = DateTimeOffset.UtcNow
                let duration = endTime - startTime
                match res with
                | Ok (out, ctx') ->
                    let timedCtx = ctx' |> MrpContext.addWarning $"Step '{stepName}' executed in {duration.TotalMilliseconds}ms"
                    return Ok (out, timedCtx)
                | Error e -> return Error e
            }

    /// Retry step execution on failure
    let withRetry (maxRetries: int) (delay: TimeSpan) (step: MrpStepAsync<'a, 'b>) : MrpStepAsync<'a, 'b> =
        fun input ctx ->
            let rec loop retryCount =
                async {
                    let! res = step input ctx
                    match res with
                    | Ok _ -> return res
                    | Error _ when retryCount < maxRetries ->
                        do! Async.Sleep (int delay.TotalMilliseconds)
                        return! loop (retryCount + 1)
                    | Error _ -> return res
                }
            loop 0

    /// Run step with timeout enforcement
    let withTimeout (timeout: TimeSpan) (step: MrpStepAsync<'a, 'b>) : MrpStepAsync<'a, 'b> =
        fun input ctx ->
            async {
                let! child = Async.StartChild (step input ctx, int timeout.TotalMilliseconds)
                try
                    return! child
                with :? TimeoutException ->
                    return Error (Cancelled $"Step timed out after {timeout}")
            }

    /// Safeguard against unhandled step exceptions
    let catchExceptions (step: MrpStepAsync<'a, 'b>) : MrpStepAsync<'a, 'b> =
        fun input ctx ->
            async {
                try
                    return! step input ctx
                with ex ->
                    return Error (InternalError ex)
            }

    /// Identity step (no-op)
    let identity<'a> : MrpStepAsync<'a, 'a> =
        fun input ctx -> async { return Ok (input, ctx) }

/// Pipeline builder for fluent assembly
type PipelineBuilder<'input, 'output> =
    { Steps: (string * obj) list
      Execute: MrpStepAsync<'input, 'output> }

module PipelineBuilder =
    
    /// Create empty builder
    let create<'a> () : PipelineBuilder<'a, 'a> =
        { Steps = []
          Execute = Pipeline.identity }

    /// Add a generic async step to builder
    let addStep<'a, 'b, 'c>
        (name: string)
        (step: MrpStepAsync<'b, 'c>)
        (builder: PipelineBuilder<'a, 'b>)
        : PipelineBuilder<'a, 'c> =
        { Steps = builder.Steps @ [ (name, box step) ]
          Execute = Pipeline.bind builder.Execute step }

    /// Add step with timing metric hook
    let addTimedStep<'a, 'b, 'c>
        (name: string)
        (step: MrpStepAsync<'b, 'c>)
        (builder: PipelineBuilder<'a, 'b>)
        : PipelineBuilder<'a, 'c> =
        addStep name (Pipeline.withTiming name step) builder

    /// Add safe step with exception boundary
    let addSafeStep<'a, 'b, 'c>
        (name: string)
        (step: MrpStepAsync<'b, 'c>)
        (builder: PipelineBuilder<'a, 'b>)
        : PipelineBuilder<'a, 'c> =
        addStep name (Pipeline.catchExceptions step) builder

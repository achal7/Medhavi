namespace Medhavi.Common.Patterns

open System
open System.Threading.Tasks
open Microsoft.Extensions.Logging.Abstractions
open Medhavi.Common.Retry

// ==========================================
// TaskResult PATTERN - Asynchronous Computations with Error Handling
// ==========================================

/// WHAT IS TaskResult?
/// ====================
/// TaskResult combines asynchronous computation with error handling.
/// It's a monad that represents: Async<Result<'T, 'E>>
///
/// TaskResult AS A MONAD:
/// =======================
/// - Return: 'T -> Async<Result<'T, 'E>> (wraps value in task success)
/// - Bind: Async<Result<'T, 'E>> -> ('T -> Async<Result<'U, 'E>>) -> Async<Result<'U, 'E>>
/// - Chains task operations that can fail

type TaskResult<'T, 'E> = Task<Result<'T, 'E>>

[<RequireQualifiedAccess>]
module TaskResult =

    /// Convert a result to an task result
    let ofResult (result: Result<'T, 'E>) : TaskResult<'T, 'E> = task { return result }

    /// Convert an task to an task result
    let ofAsync (asyncValue: Async<'T>) : TaskResult<'T, 'E> =
        task {
            let! value = asyncValue
            return Ok value
        }

    /// Chain together two TaskResult computations (monad operation)
    let bind (f: 'T -> TaskResult<'U, 'E>) (x: TaskResult<'T, 'E>) : TaskResult<'U, 'E> =
        task {
            let! result = x

            match result with
            | Ok value -> return! f value
            | Error err -> return Error err
        }

    /// Convert a Task<Result<'T, 'E>> to TaskResult
    let ofTask (taskOp: Task<Result<'T, 'E>>) : TaskResult<'T, 'E> =
        task {
            let! result = taskOp
            return result
        }

    /// Convert a Task<'T> to TaskResult (wraps value in Ok)
    let ofTaskValue (taskOp: Task<'T>) : TaskResult<'T, 'E> =
        task {
            let! value = taskOp
            return Ok value
        }

    /// Convert TaskResult to Task<Result<'T, 'E>>
    let toTask (TaskResult: TaskResult<'T, 'E>) : Task<Result<'T, 'E>> = TaskResult

    /// Convert TaskResult to Task<'T> (throws on Error)
    /// Note: Consider using Result-based APIs instead of throwing
    let toTaskValue (taskResult: TaskResult<'T, 'E>) : Task<'T> =
        task {
            let! result = taskResult

            match result with
            | Ok value -> return value
            | Error err ->
                // Explicitly handle error case - throw with clear message
                return raise (InvalidOperationException($"TaskResult failed with error: %A{err}"))
        }

    /// Lift a function to the TaskResult world (functor operation)
    let map (f: 'T -> 'U) (x: TaskResult<'T, 'E>) : TaskResult<'U, 'E> =
        task {
            let! result = x
            return Result.map f result
        }

    /// Lift a function to the TaskResult world for the Error case
    let mapError (f: 'E -> 'F) (x: TaskResult<'T, 'E>) : TaskResult<'T, 'F> =
        task {
            let! result = x
            return Result.mapError f result
        }

    /// Combine two task results using applicative style
    let apply (fTaskResult: TaskResult<'T -> 'U, 'E>) (xTaskResult: TaskResult<'T, 'E>) : TaskResult<'U, 'E> =
        task {
            let! fResult = fTaskResult
            let! xResult = xTaskResult
            return Medhavi.Common.Result.apply fResult xResult
        }

    /// Create a successful TaskResult
    let return_ (value: 'T) : TaskResult<'T, 'E> = task { return (Ok value) }

    /// Create a failed TaskResult
    let fail (error: 'E) : TaskResult<'T, 'E> = task { return (Error error) }

    /// Execute TaskResult and ignore result
    let ignore (TaskResult: TaskResult<'T, 'E>) : TaskResult<unit, 'E> = map ignore TaskResult

    /// Convert TaskResult to Task<Option>
    let toOption (taskResult: TaskResult<'T, 'E>) : Task<Option<'T>> =
        task {
            let! result = taskResult
            return Result.toOption result
        }

    /// Convert TaskResult to Task<Result> with different error type
    let mapErrorTask (f: 'E -> 'F) (taskResult: TaskResult<'T, 'E>) : TaskResult<'T, 'F> = mapError f taskResult

    /// Sequence a list of TaskResults
    let sequence (TaskResults: TaskResult<'T, 'E> list) : TaskResult<'T list, 'E> =
        let (<*>) = apply
        let (<!>) = map
        let cons head tail = head :: tail
        let consA = return_ cons
        List.foldBack (fun x acc -> consA <*> x <*> acc) TaskResults (return_ [])

    /// Map a function over a list and sequence the results
    let mapM (f: 'T -> TaskResult<'U, 'E>) (xs: 'T list) : TaskResult<'U list, 'E> = xs |> List.map f |> sequence

    /// Execute TaskResults in sequence, keeping only the last result
    let sequence_ (TaskResults: TaskResult<'T, 'E> list) : TaskResult<unit, 'E> =
        let folder _ acc = bind (fun _ -> acc) (return_ ())
        List.foldBack folder TaskResults (return_ ())

    /// Filter TaskResults based on a predicate
    let filterM (pred: 'T -> TaskResult<bool, 'E>) (xs: 'T list) : TaskResult<'T list, 'E> =
        let folder x acc =
            bind
                (fun boolList ->
                    bind (fun predResult -> return_ (if predResult then x :: boolList else boolList)) (pred x))
                acc

        List.foldBack folder xs (return_ [])

    /// Handle exceptions in TaskResult computations
    let catch (f: exn -> 'E) (TaskResult: TaskResult<'T, 'E>) : TaskResult<'T, 'E> =
        task {
            try
                return! TaskResult
            with ex ->
                return Error(f ex)
        }

    /// Add timeout to TaskResult computation
    let timeout
        (timeoutMs: int)
        (mapTimeout: string -> 'E)
        (mapException: exn -> 'E)
        (tr: TaskResult<'T, 'E>)
        : TaskResult<'T, 'E> =
        task {
            try
                let! completed = Task.WhenAny(tr, Task.Delay(timeoutMs))

                if Object.ReferenceEquals(completed, tr) then
                    let! result = tr
                    return result
                else
                    return Error(mapTimeout (sprintf "Operation timed out after %dms" timeoutMs))
            with ex ->
                return Error(mapException ex)
        }

    /// Retry TaskResult computation with exponential backoff
    /// Uses the shared Retry module to avoid duplication
    /// Note: The taskResult is a value, not a function, so it will be evaluated once per attempt
    let retry (retries: int) (delayMs: int) (taskResult: TaskResult<'T, 'E>) : TaskResult<'T, 'E> =
        let logger = NullLogger.Instance

        let config =
            { RetryConfig.Default with
                MaxAttempts = retries
                BaseDelayMs = delayMs }

        task {
            // Operation function that will be called on each retry attempt
            // Each call will re-execute the taskResult computation
            let operation (_attempt: int) = taskResult
            let! result = ResultAsyncRetry.retryAsyncResult config operation logger
            return result
        }

    /// Parallel execution of TaskResults
    /// Uses safe pattern matching from ResultAsyncCommon
    let parallelAsync (trs: TaskResult<'T, 'E> list) : TaskResult<'T list, 'E list> =
        task {
            // Await all TaskResults at once
            let! results = trs |> Task.WhenAll

            // Use safe partition function from ResultAsyncCommon
            return ResultAsyncCommon.partitionResultsSafe results
        }

/// COMPUTATION EXPRESSION BUILDER FOR TaskResult
/// ===============================================
type TaskResultBuilder() =
    member _.Return(x: 'T) : Task<Result<'T, 'E>> = TaskResult.return_ x

    // Bind TaskResult<'T, 'E>
    member _.Bind(taskResult: Task<Result<'T, 'E>>, f: 'T -> Task<Result<'U, 'E>>) : Task<Result<'U, 'E>> =
        TaskResult.bind f taskResult

    // Bind Result<'T, 'E> directly (auto-lifts to TaskResult)
    member _.Bind(result: Result<'T, 'E>, f: 'T -> Task<Result<'U, 'E>>) : Task<Result<'U, 'E>> =
        TaskResult.bind f (TaskResult.ofResult result)

    // Bind Task<'T> directly (auto-lifts value to Ok)
    member _.Bind(taskOp: Task<'T>, f: 'T -> Task<Result<'U, 'E>>) : Task<Result<'U, 'E>> =
        TaskResult.bind f (TaskResult.ofTaskValue taskOp)

    // Bind Async<'T> directly (auto-lifts value to Ok)
    member _.Bind(asyncOp: Async<'T>, f: 'T -> Task<Result<'U, 'E>>) : Task<Result<'U, 'E>> =
        TaskResult.bind f (TaskResult.ofAsync asyncOp)

    member _.ReturnFrom(x: Task<Result<'T, 'E>>) : Task<Result<'T, 'E>> = x
    member _.Zero() : Task<Result<unit, 'E>> = TaskResult.return_ ()
    member _.Delay(f: unit -> Task<Result<'T, 'E>>) : Task<Result<'T, 'E>> = f ()
    member _.Run(f: Task<Result<'T, 'E>>) : Task<Result<'T, 'E>> = f

    // Sequential composition
    member _.Combine(a: Task<Result<unit, 'E>>, b: unit -> Task<Result<'T, 'E>>) : Task<Result<'T, 'E>> =
        TaskResult.bind b a

    // Control flow
    member _.IfThenElse
        (condition: bool, ifBody: unit -> Task<Result<'T, 'E>>, elseBody: unit -> Task<Result<'T, 'E>>)
        : Task<Result<'T, 'E>> =
        if condition then ifBody () else elseBody ()

    // Loops
    member _.For(xs: seq<'a>, body: 'a -> Task<Result<unit, 'E>>) : Task<Result<unit, 'E>> =
        let folder acc x = TaskResult.bind (fun () -> body x) acc
        Seq.fold folder (TaskResult.return_ ()) xs

    // Try-with
    member __.TryWith(body: unit -> Task<Result<'T, 'E>>, handler: exn -> Task<Result<'T, 'E>>) : Task<Result<'T, 'E>> =
        task {
            try
                return! body ()
            with ex ->
                return! handler ex
        }

    // Try-finally
    member __.TryFinally(body: unit -> Task<Result<'T, 'E>>, compensation: unit -> unit) : Task<Result<'T, 'E>> =
        task {
            try
                return! body ()
            finally
                compensation ()
        }

    // Using
    member _.Using(resource: #IDisposable, body: #IDisposable -> Task<Result<'T, 'E>>) : Task<Result<'T, 'E>> =
        task {
            use r = resource
            return! body r
        }

[<AutoOpen>]
module TaskResultBuilderExtensions =
    let taskResult = TaskResultBuilder()

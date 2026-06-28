namespace Medhavi.Common.Patterns

open System
open System.Threading.Tasks
open Microsoft.Extensions.Logging.Abstractions
open Medhavi.Common.Retry

// ==========================================
// ASYNCRESULT PATTERN - Asynchronous Computations with Error Handling
// ==========================================

/// WHAT IS ASYNCRESULT?
/// ====================
/// AsyncResult combines asynchronous computation with error handling.
/// It's a monad that represents: Async<Result<'T, 'E>>
///
/// ASYNCRESULT AS A MONAD:
/// =======================
/// - Return: 'T -> Async<Result<'T, 'E>> (wraps value in async success)
/// - Bind: Async<Result<'T, 'E>> -> ('T -> Async<Result<'U, 'E>>) -> Async<Result<'U, 'E>>
/// - Chains async operations that can fail

type AsyncResult<'T, 'E> = Async<Result<'T, 'E>>

[<RequireQualifiedAccess>]
module AsyncResult =
    open System.Threading

    /// Convert a result to an async result
    let ofResult (result: Result<'T, 'E>) : AsyncResult<'T, 'E> = async.Return result

    /// Convert an async to an async result
    let ofAsync (asyncValue: Async<'T>) : AsyncResult<'T, 'E> =
        async {
            let! value = asyncValue
            return Ok value
        }

    /// Convert a Task<Result<'T, 'E>> to AsyncResult
    let ofTask (task: Task<Result<'T, 'E>>) : AsyncResult<'T, 'E> =
        async {
            let! result = task |> Async.AwaitTask
            return result
        }

    /// Convert a Task<'T> to AsyncResult (wraps value in Ok)
    let ofTaskValue (task: Task<'T>) : AsyncResult<'T, 'E> =
        async {
            let! value = task |> Async.AwaitTask
            return Ok value
        }

    /// Convert AsyncResult to Task<Result<'T, 'E>>
    let toTask (asyncResult: AsyncResult<'T, 'E>) : Task<Result<'T, 'E>> = asyncResult |> Async.StartAsTask

    /// Convert AsyncResult to Task<'T> (throws on Error)
    /// Note: Consider using Result-based APIs instead of throwing
    let toTaskValue (asyncResult: AsyncResult<'T, 'E>) : Task<'T> =
        async {
            let! result = asyncResult

            match result with
            | Ok value -> return value
            | Error err ->
                // Explicitly handle error case - throw with clear message
                return raise(InvalidOperationException($"AsyncResult failed with error: %A{err}"))
        }
        |> Async.StartAsTask

    /// Lift a function to the AsyncResult world (functor operation)
    let map (f: 'T -> 'U) (x: AsyncResult<'T, 'E>) : AsyncResult<'U, 'E> =
        async {
            let! result = x
            return Result.map f result
        }

    /// Lift a function to the AsyncResult world for the Error case
    let mapError (f: 'E -> 'F) (x: AsyncResult<'T, 'E>) : AsyncResult<'T, 'F> =
        async {
            let! result = x
            return Result.mapError f result
        }

    /// Chain together two AsyncResult computations (monad operation)
    let bind (f: 'T -> AsyncResult<'U, 'E>) (x: AsyncResult<'T, 'E>) : AsyncResult<'U, 'E> =
        async {
            let! result = x

            match result with
            | Ok value -> return! f value
            | Error err -> return Error err
        }

    /// Combine two async results using applicative style
    let apply (fAsyncResult: AsyncResult<'T -> 'U, 'E>) (xAsyncResult: AsyncResult<'T, 'E>) : AsyncResult<'U, 'E> =
        async {
            let! fResult = fAsyncResult
            let! xResult = xAsyncResult
            return Medhavi.Common.Result.apply fResult xResult
        }

    /// Create a successful AsyncResult
    let return_ (value: 'T) : AsyncResult<'T, 'E> = async.Return(Ok value)

    /// Create a failed AsyncResult
    let fail (error: 'E) : AsyncResult<'T, 'E> = async.Return(Error error)

    /// Execute AsyncResult and ignore result
    let ignore (asyncResult: AsyncResult<'T, 'E>) : AsyncResult<unit, 'E> = map ignore asyncResult

    /// Convert AsyncResult to Async<Option>
    let toAsyncOption (asyncResult: AsyncResult<'T, 'E>) : Async<Option<'T>> =
        async {
            let! result = asyncResult
            return Result.toOption result
        }

    /// Convert AsyncResult to Async<Result> with different error type
    let mapErrorAsync (f: 'E -> 'F) (asyncResult: AsyncResult<'T, 'E>) : AsyncResult<'T, 'F> = mapError f asyncResult

    /// Sequence a list of AsyncResults
    let sequence (asyncResults: AsyncResult<'T, 'E> list) : AsyncResult<'T list, 'E> =
        let (<*>) = apply
        let (<!>) = map
        let cons head tail = head :: tail
        let consA = return_ cons
        List.foldBack (fun x acc -> consA <*> x <*> acc) asyncResults (return_ [])

    /// Map a function over a list and sequence the results
    let mapM (f: 'T -> AsyncResult<'U, 'E>) (xs: 'T list) : AsyncResult<'U list, 'E> = xs |> List.map f |> sequence

    /// Execute AsyncResults in sequence, keeping only the last result
    let sequence_ (asyncResults: AsyncResult<'T, 'E> list) : AsyncResult<unit, 'E> =
        let folder _ acc = bind (fun _ -> acc) (return_())
        List.foldBack folder asyncResults (return_())

    /// Filter AsyncResults based on a predicate
    let filterM (pred: 'T -> AsyncResult<bool, 'E>) (xs: 'T list) : AsyncResult<'T list, 'E> =
        let folder x acc =
            bind
                (fun boolList ->
                    bind (fun predResult -> return_(if predResult then x :: boolList else boolList)) (pred x))
                acc

        List.foldBack folder xs (return_ [])

    /// Handle exceptions in AsyncResult computations
    let catch (f: exn -> 'E) (asyncResult: AsyncResult<'T, 'E>) : AsyncResult<'T, 'E> =
        async {
            try
                return! asyncResult
            with ex ->
                return Error(f ex)
        }

    /// Add timeout to AsyncResult computation
    let timeout (timeoutMs: int) (asyncResult: AsyncResult<'T, 'E>) : AsyncResult<'T, 'E> =
        async {
            let! child = Async.StartChild(asyncResult, timeoutMs)

            try
                return! child
            with :? TimeoutException ->
                return Error(sprintf "Operation timed out after %dms" timeoutMs :> obj :?> 'E)
        }

    let retry (retries: int) (asyncResult: AsyncResult<'T, 'E>) (ct: CancellationToken) handleCancellationError : AsyncResult<'T, 'E> =
        let logger = NullLogger.Instance

        let config = Some <| RetryConfig.DefaultWithAttempts retries

        async {
            // Operation function that will be called on each retry attempt
            // Each call will re-execute the asyncResult computation
            let operation ct (_attempt: int) = asyncResult |> Async.StartAsTask

            let! result = executeWithRetry operation logger config ct handleCancellationError |> Async.AwaitTask

            return result
        }

    /// Parallel execution of AsyncResults
    /// Uses safe pattern matching from ResultAsyncCommon
    let parallelAsync (asyncResults: AsyncResult<'T, 'E> list) : AsyncResult<'T list, 'E list> =
        async {
            let! results =
                asyncResults
                |> List.map(fun ar ->
                    async {
                        let! r = ar
                        return r
                    })
                |> Async.Parallel

            // Use safe partition function from ResultAsyncCommon
            return Medhavi.Common.Result.partitionResultsSafe results
        }

    let (>=>) f g x = f x |> bind g

    let run (asyncResult: AsyncResult<'T, 'E>) : AsyncResult<'T, 'E> =
        async {
            let! result = asyncResult
            return result
        }

/// COMPUTATION EXPRESSION BUILDER FOR ASYNCRESULT
/// ===============================================
type AsyncResultBuilder() =
    member _.Return(x) = AsyncResult.return_ x
    member _.Bind(x, f) = AsyncResult.bind f x
    member _.ReturnFrom(x) = x
    member _.Zero() = AsyncResult.return_()
    member _.Delay(f) = f
    member _.Run(f) = f()

    // Sequential composition
    member this.Combine(a, b) = this.Bind(a, (fun () -> b()))

    // Control flow
    member this.IfThenElse(condition, ifBody, elseBody) = if condition then ifBody() else elseBody()

    // Loops
    member this.For(xs: seq<'a>, body: 'a -> AsyncResult<unit, 'b>) =
        let folder acc x = this.Bind(acc, (fun () -> body x))
        Seq.fold folder (this.Return()) xs

    // Try-with
    member __.TryWith(body, handler) = AsyncResult.catch (fun ex -> handler ex) (body())

    // Try-finally
    member __.TryFinally(body, compensation) =
        async {
            try
                return! body()
            finally
                compensation()
        }
        |> AsyncResult.ofAsync

    // Using
    member this.Using(resource: #IDisposable, body) =
        async {
            use r = resource
            return! body r
        }
        |> AsyncResult.ofAsync

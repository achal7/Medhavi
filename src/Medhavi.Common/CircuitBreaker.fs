module Medhavi.Common.CircuitBreaker

open System
open System.Threading
open System.Threading.Tasks
open Microsoft.Extensions.Logging

// -------------------- Public types --------------------

/// Visible circuit breaker states
type CircuitBreakerState =
    | Closed
    | Open of openedAt: DateTimeOffset
    | HalfOpen of trialSuccesses: int * trialFailures: int

/// Rich events emitted by the circuit breaker for metrics/observability
type CircuitBreakerEvent =
    | Opened of timestamp: DateTimeOffset * reason: string * consecutiveOpens: int
    | HalfOpened of timestamp: DateTimeOffset
    | Closed of timestamp: DateTimeOffset
    | RequestSucceeded of timestamp: DateTimeOffset
    | RequestFailed of timestamp: DateTimeOffset * error: string
    | ResetOccurred of timestamp: DateTimeOffset
    | AvailabilityChanged of timestamp: DateTimeOffset * available: bool

/// Circuit breaker configuration
type CircuitBreakerConfig =
    {
        FailureThreshold: int
        RecoveryTimeout: TimeSpan
        MaxRecoveryTimeout: TimeSpan
        BackoffFactor: float
        MonitoringPeriod: TimeSpan
        SuccessThreshold: int
        /// Optional simple callbacks
        OnBreak: (string -> unit) option
        OnReset: (unit -> unit) option
        /// Comprehensive event callback (timestamped)
        OnEvent: (CircuitBreakerEvent -> unit) option
    }

    static member Default =
        {
            FailureThreshold = 5
            RecoveryTimeout = TimeSpan.FromSeconds(30.0)
            MaxRecoveryTimeout = TimeSpan.FromMinutes(1.0)
            BackoffFactor = 2.0
            MonitoringPeriod = TimeSpan.FromSeconds(30.0)
            SuccessThreshold = 3
            OnBreak = None
            OnReset = None
            OnEvent = None
        }

/// Circuit breaker runtime statistics
type CircuitBreakerStats =
    {
        State: CircuitBreakerState
        FailureCountWindow: int
        TotalRequests: int64
        TotalFailures: int64
        TotalSuccesses: int64
        LastFailureTime: DateTimeOffset option
        LastSuccessTime: DateTimeOffset option
        ConsecutiveOpens: int
    }

/// Result from ExecuteAsync
type CircuitBreakerResult<'T> =
    | Success of 'T
    | CircuitOpen of string
    | ExecutionFailed of string

/// Public returned functional record
type CircuitBreaker<'T> =
    {
        /// Original overload (no explicit cancellation token)
        ExecuteAsync: (unit -> Task<'T>) -> Task<CircuitBreakerResult<'T>>
        /// Overload where caller can pass an explicit CancellationToken
        ExecuteAsyncWithToken: (CancellationToken -> Task<'T>) -> CancellationToken -> Task<CircuitBreakerResult<'T>>
        GetStats: unit -> CircuitBreakerStats
        Reset: unit -> unit
        IsAvailable: unit -> bool
    }

// -------------------- Implementation details --------------------

module private Impl =
    // Internal agent messages
    type AgentMsg<'T> =
        | Exec of (unit -> Task<'T>) * TaskCompletionSource<CircuitBreakerResult<'T>>
        | ExecWithToken of
            (CancellationToken -> Task<'T>) *
            CancellationToken *
            TaskCompletionSource<CircuitBreakerResult<'T>>
        | ReportSuccess of DateTimeOffset * 'T * TaskCompletionSource<CircuitBreakerResult<'T>> option
        | ReportFailure of DateTimeOffset * exn * TaskCompletionSource<CircuitBreakerResult<'T>> option
        | GetStats of TaskCompletionSource<CircuitBreakerStats>
        | Reset
        | IsAvailable of TaskCompletionSource<bool>

    // Agent internal state (single-threaded)
    type State =
        {
            CurrentState: CircuitBreakerState
            FailureTimestamps: System.Collections.Generic.List<DateTimeOffset>
            TotalRequests: int64
            TotalFailures: int64
            TotalSuccesses: int64
            LastFailure: DateTimeOffset option
            LastSuccess: DateTimeOffset option
            ConsecutiveOpens: int
        }

        static member Initial =
            {
                CurrentState = CircuitBreakerState.Closed
                FailureTimestamps = System.Collections.Generic.List<DateTimeOffset>()
                TotalRequests = 0L
                TotalFailures = 0L
                TotalSuccesses = 0L
                LastFailure = None
                LastSuccess = None
                ConsecutiveOpens = 0
            }

    let nowUtc () = DateTimeOffset.UtcNow

    /// prune failure timestamps older than the monitoring window, return remaining count
    let pruneFailures (window: TimeSpan) (failureList: System.Collections.Generic.List<DateTimeOffset>) =
        if failureList.Count = 0 then
            0
        else
            let cutoff = nowUtc () - window
            let mutable i = 0

            while i < failureList.Count && failureList.[i] < cutoff do
                i <- i + 1

            if i > 0 then
                failureList.RemoveRange(0, i)

            failureList.Count

    /// effective recovery timeout with exponential backoff
    let effectiveRecoveryTimeout (cfg: CircuitBreakerConfig) (consecutiveOpens: int) =
        if consecutiveOpens <= 1 then
            cfg.RecoveryTimeout
        else
            let exponent = float (consecutiveOpens - 1)
            let multiplier = Math.Pow(cfg.BackoffFactor, exponent)

            let scaled =
                TimeSpan.FromTicks(int64 (float cfg.RecoveryTimeout.Ticks * multiplier))

            if scaled > cfg.MaxRecoveryTimeout then
                cfg.MaxRecoveryTimeout
            else
                scaled

    /// attempt Open -> HalfOpen transition if timeout expired
    let tryTransitionOpenToHalfOpen (cfg: CircuitBreakerConfig) (s: State) =
        match s.CurrentState with
        | Open(openedAt) ->
            let timeout = effectiveRecoveryTimeout cfg s.ConsecutiveOpens

            if nowUtc () - openedAt >= timeout then
                Some { s with CurrentState = HalfOpen(0, 0) }
            else
                None
        | _ -> None

    /// handle success update and emit appropriate events
    let handleSuccess
        (cfg: CircuitBreakerConfig)
        (logger: ILogger option)
        (s: State)
        (whenHappened: DateTimeOffset)
        (result: 'T)
        =
        let s =
            { s with
                TotalRequests = s.TotalRequests + 1L
                TotalSuccesses = s.TotalSuccesses + 1L
                LastSuccess = Some whenHappened
            }

        match s.CurrentState with
        | CircuitBreakerState.Closed ->
            pruneFailures cfg.MonitoringPeriod s.FailureTimestamps
            |> ignore
            // emit RequestSucceeded event
            cfg.OnEvent
            |> Option.iter (fun f -> f (RequestSucceeded whenHappened))

            logger
            |> Option.iter (fun l -> l.LogDebug("CircuitBreaker: request succeeded (Closed)."))

            s
        | HalfOpen(successes, failures) ->
            let newSuccesses = successes + 1

            cfg.OnEvent
            |> Option.iter (fun f -> f (RequestSucceeded whenHappened))

            if newSuccesses >= cfg.SuccessThreshold then
                // close circuit
                cfg.OnReset |> Option.iter (fun cb -> cb ())

                cfg.OnEvent
                |> Option.iter (fun f -> f (Closed whenHappened))

                logger
                |> Option.iter (fun l ->
                    l.LogInformation("CircuitBreaker: HalfOpen -> Closed (success threshold reached)."))

                s.FailureTimestamps.Clear()

                { s with
                    CurrentState = CircuitBreakerState.Closed
                    ConsecutiveOpens = 0
                }
            else
                { s with
                    CurrentState = HalfOpen(newSuccesses, failures)
                }
        | Open(_) ->
            // unexpected success while open; just log it
            cfg.OnEvent
            |> Option.iter (fun f -> f (RequestSucceeded whenHappened))

            logger
            |> Option.iter (fun l -> l.LogDebug("CircuitBreaker: success seen while Open (ignored)."))

            s

    /// handle failure update and emit events
    let handleFailure
        (cfg: CircuitBreakerConfig)
        (logger: ILogger option)
        (s: State)
        (whenHappened: DateTimeOffset)
        (ex: exn)
        =
        s.FailureTimestamps.Add(whenHappened)
        let windowCount = pruneFailures cfg.MonitoringPeriod s.FailureTimestamps

        let s' =
            { s with
                TotalRequests = s.TotalRequests + 1L
                TotalFailures = s.TotalFailures + 1L
                LastFailure = Some whenHappened
            }

        match s'.CurrentState with
        | CircuitBreakerState.Closed ->
            if windowCount >= cfg.FailureThreshold then
                let newConsec = s'.ConsecutiveOpens + 1

                cfg.OnBreak
                |> Option.iter (fun cb ->
                    cb (sprintf "Threshold reached: %d failures in %A" windowCount cfg.MonitoringPeriod))

                cfg.OnEvent
                |> Option.iter (fun f -> f (Opened(whenHappened, sprintf "threshold %d" newConsec, newConsec)))

                cfg.OnEvent
                |> Option.iter (fun f -> f (RequestFailed(whenHappened, ex.Message)))

                logger
                |> Option.iter (fun l -> l.LogWarning(ex, "CircuitBreaker: tripped to Open"))

                { s' with
                    CurrentState = Open(whenHappened)
                    ConsecutiveOpens = newConsec
                }
            else
                cfg.OnEvent
                |> Option.iter (fun f -> f (RequestFailed(whenHappened, ex.Message)))

                logger
                |> Option.iter (fun l -> l.LogDebug("CircuitBreaker: recorded failure (Closed)."))

                s'
        | HalfOpen(_, _) ->
            // immediate re-open on failure during HalfOpen
            let newConsec = s'.ConsecutiveOpens + 1

            cfg.OnBreak
            |> Option.iter (fun cb -> cb (sprintf "Failure during HalfOpen: %s" ex.Message))

            cfg.OnEvent
            |> Option.iter (fun f -> f (Opened(whenHappened, "failure during half-open", newConsec)))

            cfg.OnEvent
            |> Option.iter (fun f -> f (RequestFailed(whenHappened, ex.Message)))

            logger
            |> Option.iter (fun l -> l.LogWarning(ex, "CircuitBreaker: failure in HalfOpen -> re-open"))

            { s' with
                CurrentState = Open(whenHappened)
                ConsecutiveOpens = newConsec
            }
        | Open(openedAt) ->
            // already open; still record failure event
            cfg.OnEvent
            |> Option.iter (fun f -> f (RequestFailed(whenHappened, ex.Message)))

            logger
            |> Option.iter (fun l -> l.LogDebug("CircuitBreaker: recorded failure while already Open."))

            { s' with
                CurrentState = Open(openedAt)
            }

    let buildStats (cfg: CircuitBreakerConfig) (s: State) =
        let windowCount = pruneFailures cfg.MonitoringPeriod s.FailureTimestamps

        {
            State = s.CurrentState
            FailureCountWindow = windowCount
            TotalRequests = s.TotalRequests
            TotalFailures = s.TotalFailures
            TotalSuccesses = s.TotalSuccesses
            LastFailureTime = s.LastFailure
            LastSuccessTime = s.LastSuccess
            ConsecutiveOpens = s.ConsecutiveOpens
        }

    /// start the internal agent. The agent will not block while an operation runs; it posts
    /// ReportSuccess/ReportFailure when done. This function ensures token/cancellation handling:
    /// if token cancels before completion, the tcs receives ExecutionFailed with cancellation info.
    let startAgent (cfg: CircuitBreakerConfig) (logger: ILogger option) (isSuccess: 'T -> bool) =
        MailboxProcessor.Start(fun inbox ->
            let rec loop (state: State) =
                async {
                    let! msg = inbox.Receive()

                    match msg with
                    | Exec(op, tcs) ->
                        // try transition open->halfopen if eligible
                        let state =
                            match tryTransitionOpenToHalfOpen cfg state with
                            | Some s ->
                                // emit HalfOpened event
                                cfg.OnEvent
                                |> Option.iter (fun f -> f (HalfOpened(nowUtc ())))

                                logger
                                |> Option.iter (fun l -> l.LogDebug("CircuitBreaker: Open -> HalfOpen"))

                                s
                            | None -> state

                        match state.CurrentState with
                        | Open(_) ->
                            cfg.OnEvent
                            |> Option.iter (fun f -> f (AvailabilityChanged(nowUtc (), false)))

                            tcs.SetResult(CircuitOpen "Circuit breaker is open")
                            return! loop state

                        | CircuitBreakerState.Closed
                        | HalfOpen _ ->
                            // run operation on thread pool; monitor result and post back
                            let task =
                                try
                                    op ()
                                with ex ->
                                    Task.FromException<'T>(ex)

                            // detach continuation via Task.Run so it won't execute on agent thread
                            Task.Run(fun () ->
                                task.ContinueWith(fun (t: Task<'T>) ->
                                    if t.IsFaulted then
                                        let ex =
                                            if t.Exception <> null then
                                                t.Exception.GetBaseException()
                                            else
                                                new Exception("Unknown task fault")

                                        inbox.Post(ReportFailure(nowUtc (), ex, Some tcs))
                                    elif t.IsCanceled then
                                        inbox.Post(
                                            ReportFailure(
                                                nowUtc (),
                                                TaskCanceledException("task canceled"),
                                                Some tcs
                                            )
                                        )
                                    else
                                        try
                                            let res = t.Result

                                            if isSuccess res then
                                                inbox.Post(ReportSuccess(nowUtc (), res, Some tcs))
                                            else
                                                // consider it a failure per policy
                                                let reason = "Result considered failure by policy"
                                                inbox.Post(ReportFailure(nowUtc (), new Exception(reason), Some tcs))
                                        with ex ->
                                            inbox.Post(ReportFailure(nowUtc (), ex, Some tcs)))
                                |> ignore

                                Task.CompletedTask)
                            |> ignore

                            return! loop state

                    | ExecWithToken(opWithToken, ct, tcs) ->
                        // try transition open->halfopen if eligible
                        let state =
                            match tryTransitionOpenToHalfOpen cfg state with
                            | Some s ->
                                cfg.OnEvent
                                |> Option.iter (fun f -> f (HalfOpened(nowUtc ())))

                                logger
                                |> Option.iter (fun l -> l.LogDebug("CircuitBreaker: Open -> HalfOpen"))

                                s
                            | None -> state

                        match state.CurrentState with
                        | Open(_) ->
                            cfg.OnEvent
                            |> Option.iter (fun f -> f (AvailabilityChanged(nowUtc (), false)))

                            tcs.SetResult(CircuitOpen "Circuit breaker is open")
                            return! loop state

                        | CircuitBreakerState.Closed
                        | HalfOpen _ ->
                            // Build task and cancellation race
                            let opTask =
                                try
                                    opWithToken (ct)
                                with ex ->
                                    Task.FromException<'T>(ex)

                            // Build a Task that completes when the token is cancelled
                            let cancellationTask =
                                let tcsCancel =
                                    TaskCompletionSource<unit>(TaskCreationOptions.RunContinuationsAsynchronously)

                                if ct.CanBeCanceled then
                                    let reg = ct.Register(fun () -> tcsCancel.TrySetResult(()) |> ignore)
                                    // note: reg will be released when function finishes (GC) - acceptable here
                                    ()
                                else
                                    ()

                                tcsCancel.Task

                            // run a Task that awaits whichever completes first
                            Task.Run(fun () ->
                                let race = Task.WhenAny(opTask :> Task, cancellationTask :> Task)

                                race.ContinueWith(fun (t: Task<Task>) ->
                                    // t.Result is the completed task
                                    let completed = t.Result

                                    if Object.ReferenceEquals(completed, cancellationTask :> Task) then
                                        // cancellation happened before op completed
                                        inbox.Post(
                                            ReportFailure(
                                                nowUtc (),
                                                TaskCanceledException("caller cancellation requested"),
                                                Some tcs
                                            )
                                        )
                                    else
                                        // op completed
                                        let tCompleted = opTask

                                        if tCompleted.IsFaulted then
                                            let ex =
                                                if tCompleted.Exception <> null then
                                                    tCompleted.Exception.GetBaseException()
                                                else
                                                    new Exception("Unknown task fault")

                                            inbox.Post(ReportFailure(nowUtc (), ex, Some tcs))
                                        elif tCompleted.IsCanceled then
                                            inbox.Post(
                                                ReportFailure(
                                                    nowUtc (),
                                                    TaskCanceledException("operation canceled"),
                                                    Some tcs
                                                )
                                            )
                                        else
                                            try
                                                let res = tCompleted.Result

                                                if isSuccess res then
                                                    inbox.Post(ReportSuccess(nowUtc (), res, Some tcs))
                                                else
                                                    inbox.Post(
                                                        ReportFailure(
                                                            nowUtc (),
                                                            Exception("Result considered failure by policy"),
                                                            Some tcs
                                                        )
                                                    )
                                            with ex ->
                                                inbox.Post(ReportFailure(nowUtc (), ex, Some tcs)))
                                |> ignore
                                |> ignore

                                Task.CompletedTask)
                            |> ignore

                            return! loop state

                    | ReportSuccess(whenHappened, result, maybeTcs) ->
                        let newState = handleSuccess cfg logger state whenHappened result

                        maybeTcs
                        |> Option.iter (fun tcs -> tcs.SetResult(Success result))

                        return! loop newState

                    | ReportFailure(whenHappened, ex, maybeTcs) ->
                        let newState = handleFailure cfg logger state whenHappened ex

                        maybeTcs
                        |> Option.iter (fun tcs -> tcs.SetResult(ExecutionFailed(ex.Message)))

                        return! loop newState

                    | GetStats(replyTcs) ->
                        let stats = buildStats cfg state
                        replyTcs.SetResult(stats)
                        return! loop state

                    | Reset ->
                        cfg.OnReset |> Option.iter (fun cb -> cb ())

                        cfg.OnEvent
                        |> Option.iter (fun f -> f (ResetOccurred(nowUtc ())))

                        logger
                        |> Option.iter (fun l -> l.LogInformation("CircuitBreaker: Reset requested"))

                        state.FailureTimestamps.Clear()

                        let newState =
                            { state with
                                CurrentState = CircuitBreakerState.Closed
                                ConsecutiveOpens = 0
                            }

                        return! loop newState

                    | IsAvailable(replyTcs) ->
                        let state =
                            match tryTransitionOpenToHalfOpen cfg state with
                            | Some s ->
                                cfg.OnEvent
                                |> Option.iter (fun f -> f (HalfOpened(nowUtc ())))

                                s
                            | None -> state

                        let available =
                            match state.CurrentState with
                            | Open(_) -> false
                            | _ -> true

                        cfg.OnEvent
                        |> Option.iter (fun f -> f (AvailabilityChanged(nowUtc (), available)))

                        replyTcs.SetResult(available)
                        return! loop state
                }

            loop State.Initial)

// -------------------- Public factory --------------------

/// Create a circuit breaker
let create (cfg: CircuitBreakerConfig) (logger: ILogger option) (isSuccess: ('T -> bool) option) : CircuitBreaker<'T> =

    let isSuccessFn = defaultArg isSuccess (fun (_: 'T) -> true)

    let agent = Impl.startAgent cfg logger isSuccessFn

    // main ExecuteAsync (no explicit token) - caller provides op: unit -> Task<'T'>
    let executeAsync (operation: unit -> Task<'T>) : Task<CircuitBreakerResult<'T>> =
        let tcs =
            TaskCompletionSource<CircuitBreakerResult<'T>>(TaskCreationOptions.RunContinuationsAsynchronously)

        agent.Post(Impl.AgentMsg.Exec(operation, tcs))
        tcs.Task

    // overload where caller passes op that accepts a CancellationToken and a token to control cancellation
    let executeAsyncWithToken
        (operationWithToken: CancellationToken -> Task<'T>)
        (ct: CancellationToken)
        : Task<CircuitBreakerResult<'T>> =
        let tcs =
            TaskCompletionSource<CircuitBreakerResult<'T>>(TaskCreationOptions.RunContinuationsAsynchronously)

        agent.Post(Impl.AgentMsg.ExecWithToken(operationWithToken, ct, tcs))
        tcs.Task

    let getStats () =
        let tcs =
            TaskCompletionSource<CircuitBreakerStats>(TaskCreationOptions.RunContinuationsAsynchronously)

        agent.Post(Impl.AgentMsg.GetStats tcs)
        tcs.Task.Result

    let reset () = agent.Post(Impl.AgentMsg.Reset)

    let isAvailable () =
        let tcs =
            TaskCompletionSource<bool>(TaskCreationOptions.RunContinuationsAsynchronously)

        agent.Post(Impl.AgentMsg.IsAvailable tcs)
        tcs.Task.Result

    {
        ExecuteAsync = executeAsync
        ExecuteAsyncWithToken = executeAsyncWithToken
        GetStats = getStats
        Reset = reset
        IsAvailable = isAvailable
    }

// -------------------- Example usage (HttpClient) --------------------
// Below is an example demonstrating how to use the circuit breaker with HttpClient
// and an isSuccess predicate that inspects the HTTP status code.
// Paste the snippet (outside the module) in a test program to try it.

(*
open System.Net.Http
open System.Threading

let exampleUsage () =
    let cfg = CircuitBreakerConfig.Default |> fun c -> { c with FailureThreshold = 3; MonitoringPeriod = TimeSpan.FromSeconds(20.0); SuccessThreshold = 2 }
    let logger: ILogger option = None

    // isSuccess inspects HttpResponseMessage.StatusCode (< 500 considered success)
    let isHttpSuccess (resp: HttpResponseMessage) =
        let code = int resp.StatusCode
        code < 500

    // Create breaker specialized for HttpResponseMessage
    let breaker = create<HttpResponseMessage> cfg ?logger isHttpSuccess

    use http = new HttpClient()

    // Operation without cancellation token:
    let op () = http.GetAsync("https://httpbin.org/status/500") |> Task.bind (fun t -> t.Content.ReadAsStringAsync() |> Task.map (fun _ -> t)) // returns HttpResponseMessage as result

    task {
        // call repeatedly to trip the breaker
        for i in 1..6 do
            let! res = breaker.ExecuteAsync op
            match res with
            | Success resp ->
                printfn "Request succeeded with code %d" (int resp.StatusCode)
            | CircuitOpen msg ->
                printfn "Breaker open: %s" msg
            | ExecutionFailed err ->
                printfn "Execution failed: %s" err

        // Using cancellation-aware overload: create an operation that honors CancellationToken
        let opWithToken (ct: CancellationToken) =
            http.GetAsync("https://httpbin.org/delay/5", ct) // this respects token

        use cts = new CancellationTokenSource(TimeSpan.FromSeconds(1.0)) // short timeout to trigger cancellation
        let! res2 = breaker.ExecuteAsyncWithToken opWithToken cts.Token
        match res2 with
        | Success _ -> printfn "Succeeded"
        | CircuitOpen m -> printfn "Open: %s" m
        | ExecutionFailed e -> printfn "Failed/cancelled: %s" e

        // inspect stats
        let stats = breaker.GetStats()
        printfn "Stats: State=%A, FailuresWindow=%d, TotalFailures=%d" stats.State stats.FailureCountWindow stats.TotalFailures

        // reset breaker
        breaker.Reset()
    } |> ignore
*)

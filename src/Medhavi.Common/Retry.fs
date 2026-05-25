module Medhavi.Common.Retry

open System
open System.Threading
open System.Threading.Tasks
open Microsoft.Extensions.Logging

// --------------------------------------------------------------------------
// Configuration and pure functions
// --------------------------------------------------------------------------

type RetryConfig =
    {
        MaxAttempts: int
        BaseDelayMs: int
        MaxDelayMs: int
        BackoffMultiplier: float
    }

    static member Default =
        {
            MaxAttempts = 3
            MaxDelayMs = 30000
            BackoffMultiplier = 10
            BaseDelayMs = 100
        }

/// Pure function to calculate retry delay with exponential backoff
let calculateRetryDelay (attemptNumber: int) (config: RetryConfig) : int =
    let delay =
        float config.BaseDelayMs
        * Math.Pow(config.BackoffMultiplier, float (attemptNumber - 1))

    let clampedDelay = min delay (float config.MaxDelayMs)
    int clampedDelay

/// Pure function to determine if retry should be attempted
let shouldRetry (attemptNumber: int) (config: RetryConfig) : bool = attemptNumber < config.MaxAttempts

// --------------------------------------------------------------------------
// Functional retry with result semantics
// --------------------------------------------------------------------------

let executeWithRetry
    (operation: int -> Task<Result<'T, 'TError>>)
    (config: RetryConfig)
    (logger: ILogger)
    : Task<Result<'T, 'TError>> =
    task {
        let rec retryAttempt attemptNumber =
            task {
                try
                    let! result = operation attemptNumber

                    match result with
                    | Ok success ->
                        if attemptNumber > 1 then
                            logger.LogInformation("✅ Operation succeeded after {AttemptNumber} attempts", attemptNumber)

                        return Ok success

                    | Error error ->
                        if shouldRetry attemptNumber config then
                            let delayMs = calculateRetryDelay attemptNumber config

                            logger.LogWarning(
                                "⚠️ Attempt {AttemptNumber} failed, retrying in {DelayMs}ms",
                                attemptNumber,
                                delayMs
                            )

                            do! Task.Delay(delayMs)
                            return! retryAttempt (attemptNumber + 1)
                        else
                            logger.LogError("❌ Operation failed after {MaxAttempts} attempts", config.MaxAttempts)
                            return Error error

                with ex ->
                    if shouldRetry attemptNumber config then
                        let delayMs = calculateRetryDelay attemptNumber config

                        logger.LogWarning(
                            ex,
                            "⚠️ Attempt {AttemptNumber} threw exception, retrying in {DelayMs}ms",
                            attemptNumber,
                            delayMs
                        )

                        do! Task.Delay(delayMs)
                        return! retryAttempt (attemptNumber + 1)
                    else
                        logger.LogError(
                            ex,
                            "❌ Operation threw exception after {MaxAttempts} attempts",
                            config.MaxAttempts
                        )

                        // Return exception info as Error without raising
                        return Error(unbox<'TError> ex)
            }

        return! retryAttempt 1
    }

// --------------------------------------------------------------------------
// General-purpose retry policy class (OOP friendly)
// --------------------------------------------------------------------------

type RetryPolicy(maxRetries: int, initialDelayMs: int, backoffMultiplier: float) =

    let calculateDelay attempt =
        let delay =
            float initialDelayMs
            * Math.Pow(backoffMultiplier, float attempt)

        Math.Min(float delay, 30000)
        |> TimeSpan.FromMilliseconds

    member _.ExecuteWithRetryAsync<'T>
        (
            operation: CancellationToken -> Task<'T>,
            cancellationToken: CancellationToken,
            ?onRetry: (int -> exn option -> TimeSpan -> unit)
        ) : Task<'T> =
        let rec execute attempt =
            task {
                try
                    let! result = operation cancellationToken
                    return result
                with ex ->
                    if attempt >= maxRetries then
                        printfn "❌ RETRY: Max retries (%d) exceeded: %s" maxRetries ex.Message
                        return raise ex
                    else
                        let delay = calculateDelay attempt

                        printfn
                            "🔄 RETRY: Attempt %d/%d failed, retrying in %.0f ms: %s"
                            (attempt + 1)
                            (maxRetries + 1)
                            delay.TotalMilliseconds
                            ex.Message

                        // 🔧 FIXED: Correct call syntax for optional callback
                        match onRetry with
                        | Some cb -> cb attempt (Some ex) delay
                        | None -> ()

                        do! Task.Delay(delay, cancellationToken)
                        return! execute (attempt + 1)
            }

        execute 0

// --------------------------------------------------------------------------
// Factory for standard retry policies
// --------------------------------------------------------------------------

type RetryPolicyFactory =
    static member CreateForExternalApi() = RetryPolicy(3, 1000, 2.0)
    static member CreateForDatabase() = RetryPolicy(5, 500, 1.5)

    static member CreateCustom(maxRetries, initialDelayMs, backoffMultiplier) =
        RetryPolicy(maxRetries, initialDelayMs, backoffMultiplier)

// -------------------- Examples --------------------
(*
open System.Net.Http

// Example 1: Result-based operation that returns Task<Result<'T,string>>
let exampleResultOperation () =
    let cfg = { RetryConfig.Default with MaxAttempts = 4; BaseDelay = TimeSpan.FromMilliseconds(300.0) }
    let logger: ILogger option = None

    let op (attempt: int) (ct: CancellationToken) : Task<Result<int,string>> =
        task {
            if attempt < 3 then
                return Error(sprintf "simulated fail at attempt %d" attempt)
            else
                return Ok 42
        }

    let! res = executeWithRetryResult op cfg (CancellationToken.None) (logger)
    match res with
    | Ok v -> printfn "Succeeded: %d" v
    | Error e -> printfn "Failed: %s" e

// Example 2: Plain Task<'T> operation (e.g., HttpClient)
let examplePlainOperation () =
    let policy = RetryPolicyFactory.ForExternalApi()
    use http = new HttpClient()
    let op (ct: CancellationToken) = http.GetStringAsync("https://httpbin.org/status/500", ct)
    let! res = policy.ExecuteAsync(op)
    match res with
    | Ok body -> printfn "Got body of length %d" (body.Length)
    | Error ex -> printfn "Request failed: %s" ex.Message
*)

module Medhavi.Infrastructure.Retry

open System
open System.Threading
open System.Threading.Tasks
open Microsoft.Extensions.Logging

type RetryConfig =
    { MaxAttempts: int
      BaseDelayMs: int
      MaxDelayMs: int
      BackoffMultiplier: float }

    static member Default() =
        { MaxAttempts = 3
          MaxDelayMs = 10000
          BackoffMultiplier = 2
          BaseDelayMs = 100 }

    static member DefaultWithAttempts attempts =
        { RetryConfig.Default() with
            MaxAttempts = attempts }

    static member ForExternalApi() =
        { MaxAttempts = 3
          MaxDelayMs = 30000
          BackoffMultiplier = 2.0
          BaseDelayMs = 1000 }

    static member ForDatabase() =
        { MaxAttempts = 5
          MaxDelayMs = 30000
          BackoffMultiplier = 1.5
          BaseDelayMs = 500 }

let calculateRetryDelay (attemptNumber: int) (config: RetryConfig) (jitter: float option) : int =
    let delay = float config.BaseDelayMs * Math.Pow(config.BackoffMultiplier, float(attemptNumber - 1))
    let clampedDelay = min delay (float config.MaxDelayMs)
    let jitterFactor = defaultArg jitter 1.0
    int(clampedDelay * jitterFactor)

let shouldRetry (attemptNumber: int) (config: RetryConfig) : bool = attemptNumber < config.MaxAttempts

let executeWithRetry
    (operation: CancellationToken -> int -> Task<Result<'T, 'TError>>)
    (logger: ILogger)
    (config: RetryConfig option)
    (ct: CancellationToken)
    (cancellationError: unit -> 'TError)
    : Task<Result<'T, 'TError>> =
    task {
        let mutable attemptNumber = 1
        let mutable finalResult = None
        let config = defaultArg config (RetryConfig.Default())

        while finalResult.IsNone && not ct.IsCancellationRequested do
            ct.ThrowIfCancellationRequested()
            let! result = operation ct attemptNumber

            match result with
            | Ok _ ->
                finalResult <- Some result

                if attemptNumber > 1 then
                    logger.LogInformation("✅ Operation succeeded after {AttemptNumber} attempts", attemptNumber)

            | Error _ when shouldRetry attemptNumber config ->
                let delay = calculateRetryDelay attemptNumber config None
                logger.LogWarning("⚠️ Attempt {AttemptNumber} failed, retrying in {DelayMs}ms", attemptNumber, delay)
                do! Task.Delay(delay, ct)
                attemptNumber <- attemptNumber + 1

            | Error _ ->
                logger.LogError("❌ Operation failed after {MaxAttempts} attempts", config.MaxAttempts)
                finalResult <- Some result

        if ct.IsCancellationRequested then
            logger.LogError("❌ Operation cancelled after {AttemptNumber} attempts", attemptNumber)
            return Error(cancellationError())
        else
            return finalResult.Value
    }

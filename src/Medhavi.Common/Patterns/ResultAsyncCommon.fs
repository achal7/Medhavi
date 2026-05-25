namespace Medhavi.Common.Patterns

open System
open System.Threading.Tasks

// ==========================================
// SHARED FUNCTIONALITY FOR ASYNCRESULT AND TASKRESULT
// ==========================================
// This module extracts common functionality to avoid code duplication
// between AsyncResult and TaskResult implementations.

/// Common operations that work with Result types in async contexts
module ResultAsyncCommon =

    /// Safe parallel execution - partitions results without unsafe pattern matching
    /// This replaces the unsafe `failwith "Impossible"` patterns
    let partitionResultsSafe (results: Result<'T, 'E> array) : Result<'T list, 'E list> =
        let oks, errors =
            results
            |> Array.fold
                (fun (okAcc, errAcc) result ->
                    match result with
                    | Ok value -> (value :: okAcc, errAcc)
                    | Error err -> (okAcc, err :: errAcc))
                ([], [])

        if List.isEmpty errors then
            Ok(List.rev oks)
        else
            Error(List.rev errors)

    /// Extract Ok values from array (safe - uses Array.choose with exhaustive matching)
    let extractOkValues (results: Result<'T, 'E> array) : 'T list =
        results
        |> Array.choose (function
            | Ok x -> Some x
            | Error _ -> None)
        |> Array.toList

    /// Extract Error values from array (safe - uses Array.choose with exhaustive matching)
    let extractErrorValues (results: Result<'T, 'E> array) : 'E list =
        results
        |> Array.choose (function
            | Ok _ -> None
            | Error e -> Some e)
        |> Array.toList

// ==========================================
// SHARED RETRY LOGIC
// ==========================================

module ResultAsyncRetry =

    open Medhavi.Common.Retry
    open Microsoft.Extensions.Logging

    /// Retry an async result operation using the shared Retry module
    /// Works with both Async and Task by converting to Task<Result<_,_>> first
    /// The operation function receives the attempt number and is called on each retry
    let retryAsyncResult
        (config: RetryConfig)
        (operation: int -> Task<Result<'T, 'E>>)
        (logger: ILogger)
        : Task<Result<'T, 'E>> =
        executeWithRetry operation config logger

    /// Simple retry with basic parameters (uses default config with overrides)
    /// Creates an operation that ignores the attempt number
    let retrySimple
        (retries: int)
        (delayMs: int)
        (operation: unit -> Task<Result<'T, 'E>>)
        (logger: ILogger)
        : Task<Result<'T, 'E>> =
        let config =
            { RetryConfig.Default with
                MaxAttempts = retries
                BaseDelayMs = delayMs
            }

        retryAsyncResult config (fun _ -> operation ()) logger

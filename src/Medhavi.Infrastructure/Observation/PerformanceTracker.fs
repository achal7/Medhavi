namespace Medhavi.Infrastructure.Observation

open System
open System.Diagnostics
open Medhavi.Infrastructure.Observation.Logger


type PerformanceTracker(logger: Logger, operation: string, comp: string) =
    let stopwatch = Stopwatch.StartNew()
    let mutable disposed = false
    member _.Logger = logger

    interface IDisposable with
        member _.Dispose() =
            if not disposed then
                disposed <- true
                stopwatch.Stop()
                let duration = stopwatch.Elapsed
                logger.LogPerformance(operation, comp, duration)

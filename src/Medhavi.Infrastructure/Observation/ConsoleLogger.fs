namespace Medhavi.Infrastructure.Observation

open System
open Microsoft.Extensions.Logging

/// Minimal console ILogger implementation.
type ConsoleLogger(categoryName: string) =
    interface ILogger with
        member _.BeginScope<'TState when 'TState: not null>(state: 'TState) : IDisposable =
            { new IDisposable with member _.Dispose() = () }

        member _.IsEnabled(logLevel: LogLevel) = true

        member _.Log<'TState>(logLevel: LogLevel, eventId: EventId, state: 'TState, ex: exn, formatter: Func<'TState, exn, string>) =
            let message = formatter.Invoke(state, ex)
            let timestamp = DateTimeOffset.UtcNow.ToString("O")
            printfn $"[{timestamp}] [{logLevel}] {categoryName}: {message}"
            match ex with
            | null -> ()
            | _ -> printfn $"  Exception: {ex}"

/// Logger factory that creates ConsoleLogger instances.
type ConsoleLoggerProvider() =
    interface ILoggerProvider with
        member _.CreateLogger(categoryName: string) : ILogger = ConsoleLogger(categoryName)
        member _.Dispose() = ()

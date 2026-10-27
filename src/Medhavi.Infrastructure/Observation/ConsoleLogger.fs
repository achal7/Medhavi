module Medhavi.Infrastructure.Observation.ConsoleLogger

open System
open Microsoft.Extensions.Logging

let createLogger (categoryName: string) : ILogger =
    { new ILogger with
        member _.BeginScope<'TState when 'TState: not null>(_: 'TState) =
            { new IDisposable with
                member _.Dispose() = ()
            }

        member _.IsEnabled(_: LogLevel) = true

        member _.Log<'TState>
            (logLevel: LogLevel, _: EventId, state: 'TState, ex: exn, formatter: Func<'TState, exn, string>)
            =
            let message = formatter.Invoke(state, ex)
            let timestamp = DateTimeOffset.UtcNow.ToString("O")
            printfn $"[{timestamp}] [{logLevel}] {categoryName}: {message}"

            match ex with
            | null -> ()
            | _ -> printfn $"  Exception: {ex}"
    }

let createProvider () : ILoggerProvider =
    { new ILoggerProvider with
        member _.CreateLogger(categoryName: string) = createLogger categoryName
        member _.Dispose() = ()
    }

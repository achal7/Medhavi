namespace Medhavi.Infrastructure.Observation

open System
open Medhavi.Foundation.Observation
open Medhavi.Foundation.Observation.LogContext
open Microsoft.Extensions.Logging

/// Internal log message for mailbox logger
type private LogMessage =
    | Log of LogLevel * string * LogContext
    | LogWithException of LogLevel * exn * string * LogContext
    | Flush of AsyncReplyChannel<unit>
    | Shutdown

/// Internal mailbox logger implementation (public for type system, but not part of public API)
type MailboxLogger(innerLogger: ILogger, batchSize: int, timeoutMs: int) =
    let mailbox =
        MailboxProcessor.Start(fun inbox ->
            let rec loop (batch: (LogLevel * string * LogContext * exn option) list) =
                async {
                    let! msgOpt = inbox.TryReceive(timeoutMs)

                    match msgOpt with
                    | Some(Log(level, message, ctx)) ->
                        let newBatch = (level, message, ctx, None) :: batch

                        if newBatch.Length >= batchSize then
                            // Flush batch
                            for l, m, c, _ in newBatch do
                                let state = contextToState c |> Seq.toArray
                                innerLogger.Log(l, m, state)

                            return! loop []
                        else
                            return! loop newBatch

                    | Some(LogWithException(level, ex, message, ctx)) ->
                        let newBatch = (level, message, ctx, Some ex) :: batch

                        if newBatch.Length >= batchSize then
                            // Flush batch
                            for l, m, c, e in newBatch do
                                let state = contextToState c |> Seq.toArray

                                match e with
                                | Some ex -> innerLogger.Log(l, ex, m, state)
                                | None -> innerLogger.Log(l, m, state)

                            return! loop []
                        else
                            return! loop newBatch

                    | Some(Flush replyChannel) ->
                        // Flush current batch
                        for l, m, c, e in batch do
                            let state = contextToState c |> Seq.toArray

                            match e with
                            | Some ex -> innerLogger.Log(l, ex, m, state)
                            | None -> innerLogger.Log(l, m, state)

                        replyChannel.Reply()
                        return! loop []

                    | Some Shutdown ->
                        // Flush before shutdown
                        for l, m, c, e in batch do
                            let state = contextToState c |> Seq.toArray

                            match e with
                            | Some ex -> innerLogger.Log(l, ex, m, state)
                            | None -> innerLogger.Log(l, m, state)

                        return () // Exit loop

                    | None ->
                        // Timeout - flush batch
                        if batch.Length > 0 then
                            for l, m, c, e in batch do
                                let state = contextToState c |> Seq.toArray

                                match e with
                                | Some ex -> innerLogger.Log(l, ex, m, state)
                                | None -> innerLogger.Log(l, m, state)

                        return! loop []
                }

            loop [])

    member this.Log(level: LogLevel, message: string, context: LogContext) = mailbox.Post(Log(level, message, context))

    member this.Log(level: LogLevel, ex: exn, message: string, context: LogContext) =
        mailbox.Post(LogWithException(level, ex, message, context))

    member this.Flush() =
        mailbox.PostAndAsyncReply Flush
        |> Async.RunSynchronously

    member this.Shutdown() = mailbox.Post Shutdown

    interface IDisposable with
        member this.Dispose() =
            this.Shutdown()
            (mailbox :> IDisposable).Dispose()

/// Wrapper ILogger that uses mailbox for async logging
type MailboxLoggerWrapper(mailboxLogger: MailboxLogger) =
    interface ILogger with
        member _.BeginScope<'TState when 'TState: not null>(_: 'TState) : IDisposable =
            { new IDisposable with
                member _.Dispose() = () }

        member _.IsEnabled(_: LogLevel) = true

        member _.Log<'TState>
            (logLevel: LogLevel, _: EventId, state: 'TState, ex: exn, formatter: Func<'TState, exn, string>)
            =
            // Extract LogContext from state if available, otherwise create empty
            let logCtx =
                match box state with
                | :? LogContext as ctx -> ctx
                | _ -> LogContext.Empty

            let message = formatter.Invoke(state, ex)
            mailboxLogger.Log(logLevel, message, logCtx)

            match ex with
            | null -> mailboxLogger.Log(logLevel, message, logCtx)
            | _ -> mailboxLogger.Log(logLevel, ex, message, logCtx)


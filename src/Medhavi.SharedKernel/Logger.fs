namespace Medhavi.SharedKernel.Logging

open System
open System.Diagnostics
open System.Threading.Tasks
open Microsoft.Extensions.Logging
open Microsoft.FSharp.Control
open Medhavi.Common.Result
open LogContext

type NullLogger() =
    interface ILogger with
        member _.BeginScope<'TState when 'TState: not null>(state: 'TState) : IDisposable =
            { new IDisposable with
                member _.Dispose() = () }

        member _.IsEnabled(logLevel: LogLevel) = false

        member _.Log<'TState>
            (logLevel: LogLevel, eventId: EventId, state: 'TState, ex: exn, formatter: Func<'TState, exn, string>)
            =
            // Do nothing
            ()

// =================================================================================================
// MAILBOX LOGGER
// =================================================================================================

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
                            for (l, m, c, _) in newBatch do
                                let state = contextToState c |> Seq.toArray
                                innerLogger.Log(l, m, state)

                            return! loop []
                        else
                            return! loop newBatch

                    | Some(LogWithException(level, ex, message, ctx)) ->
                        let newBatch = (level, message, ctx, Some ex) :: batch

                        if newBatch.Length >= batchSize then
                            // Flush batch
                            for (l, m, c, e) in newBatch do
                                let state = contextToState c |> Seq.toArray

                                match e with
                                | Some ex -> innerLogger.Log(l, ex, m, state)
                                | None -> innerLogger.Log(l, m, state)

                            return! loop []
                        else
                            return! loop newBatch

                    | Some(Flush replyChannel) ->
                        // Flush current batch
                        for (l, m, c, e) in batch do
                            let state = contextToState c |> Seq.toArray

                            match e with
                            | Some ex -> innerLogger.Log(l, ex, m, state)
                            | None -> innerLogger.Log(l, m, state)

                        replyChannel.Reply()
                        return! loop []

                    | Some Shutdown ->
                        // Flush before shutdown
                        for (l, m, c, e) in batch do
                            let state = contextToState c |> Seq.toArray

                            match e with
                            | Some ex -> innerLogger.Log(l, ex, m, state)
                            | None -> innerLogger.Log(l, m, state)

                        return () // Exit loop

                    | None ->
                        // Timeout - flush batch
                        if batch.Length > 0 then
                            for (l, m, c, e) in batch do
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
type private MailboxLoggerWrapper(mailboxLogger: MailboxLogger) =
    interface ILogger with
        member _.BeginScope<'TState when 'TState: not null>(state: 'TState) : IDisposable =
            { new IDisposable with
                member _.Dispose() = () }

        member _.IsEnabled(logLevel: LogLevel) = true

        member _.Log<'TState>
            (logLevel: LogLevel, eventId: EventId, state: 'TState, ex: exn, formatter: Func<'TState, exn, string>)
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


/// LOGGER TYPE (Public Interface - (consumers don't know about mailbox vs direct))
type Logger =
    { InnerLogger: ILogger
      Context: LogContext
      MailboxLogger: MailboxLogger option }

    static member Create(logger: ILogger, ?context: LogContext, ?useMailbox: bool, ?batchSize: int, ?timeoutMs: int) =
        let useMailbox = defaultArg useMailbox false
        let batchSize = defaultArg batchSize 10
        let timeoutMs = defaultArg timeoutMs 100

        if useMailbox then
            let mailboxLogger = new MailboxLogger(logger, batchSize, timeoutMs)
            let wrapper = new MailboxLoggerWrapper(mailboxLogger) :> ILogger

            { InnerLogger = wrapper
              Context = defaultArg context LogContext.Empty
              MailboxLogger = Some mailboxLogger }
        else
            { InnerLogger = logger
              Context = defaultArg context LogContext.Empty
              MailboxLogger = None }

    static member CreateWithoutLogging(?context: LogContext) = Logger.Create <| NullLogger()

    /// Flush mailbox logger if using mailbox implementation
    member this.Flush() =
        this.MailboxLogger
        |> Option.iter (fun mb -> mb.Flush())

    /// Shutdown mailbox logger if using mailbox implementation
    member this.Shutdown() =
        this.MailboxLogger
        |> Option.iter (fun mb -> mb.Shutdown())

    member this.getContext(context: LogContext option) =
        context
        |> Option.map (mergeContexts this.Context)
        |> Option.defaultValue this.Context

    member this.Log(logLevel: LogLevel, title: string, context: obj, message: string) =
        this.InnerLogger.Log(logLevel, title, context, message)

    member this.Info(message: string, ?context: LogContext) =
        this.getContext context
        |> logInformation this.InnerLogger message

    member this.Debug(message: string, ?context: LogContext) =
        this.getContext context
        |> logDebug this.InnerLogger message

    member this.Warning(message: string, ?context: LogContext) =
        this.getContext context
        |> logWarning this.InnerLogger message

    member this.Error(message: string, ?context: LogContext) =
        this.getContext context
        |> logError this.InnerLogger message

    member this.Error(ex: exn, message: string, ?context: LogContext) =
        this.getContext context
        |> logErrorWithException this.InnerLogger ex message

    member this.Critical(message: string, ?context: LogContext) =
        this.getContext context
        |> logCritical this.InnerLogger message

    member this.LogPerformance(operation: string, comp: string, duration: TimeSpan, ?context: LogContext) =
        this.getContext context
        |> logPerformance this.InnerLogger operation comp duration

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

module LoggingHelpers =
    /// Create a performance tracker for an operation
    let trackPerformance (logger: Logger) (operation: string) (comp: string) =
        new PerformanceTracker(logger, operation, comp)

    let withLogging
        (logger: Logger)
        (operation: string)
        (comp: string)
        (logSuccess: 'T -> LogContext -> unit)
        (logError: 'E -> LogContext -> unit)
        (operationFn: unit -> Result<'T, 'E>)
        : Result<'T, 'E> =
        let baseCtx =
            { LogContext.Empty with
                Operation = Some operation
                Component = comp }

        operationFn ()
        |> tee (fun result ->
            let ctx = mergeContexts logger.Context baseCtx
            logSuccess result ctx)
        |> teeError (fun error ->
            let ctx = mergeContexts logger.Context baseCtx
            logError error ctx)

    /// Wrap an async Result operation with logging
    let withLoggingAsync
        (logger: Logger)
        (operation: string)
        (comp: string)
        (logSuccess: 'T -> LogContext -> unit)
        (logError: 'E -> LogContext -> unit)
        (operationFn: unit -> Async<Result<'T, 'E>>)
        : Async<Result<'T, 'E>> =
        async {
            let baseCtx =
                { LogContext.Empty with
                    Operation = Some operation
                    Component = comp }

            let! result = operationFn ()

            return
                result
                |> tee (fun r ->
                    let ctx = mergeContexts logger.Context baseCtx
                    logSuccess r ctx)
                |> teeError (fun e ->
                    let ctx = mergeContexts logger.Context baseCtx
                    logError e ctx)
        }

    /// Wrap a Task Result operation with logging
    let withLoggingTask
        (logger: Logger)
        (operation: string)
        (comp: string)
        (logSuccess: 'T -> LogContext -> unit)
        (logError: 'E -> LogContext -> unit)
        (operationFn: unit -> Task<Result<'T, 'E>>)
        : Task<Result<'T, 'E>> =
        task {
            let baseCtx =
                { LogContext.Empty with
                    Operation = Some operation
                    Component = comp }

            let! result = operationFn ()

            return
                result
                |> tee (fun r ->
                    let ctx = mergeContexts logger.Context baseCtx
                    logSuccess r ctx)
                |> teeError (fun e ->
                    let ctx = mergeContexts logger.Context baseCtx
                    logError e ctx)
        }

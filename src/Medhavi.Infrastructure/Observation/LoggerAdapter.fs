module Medhavi.Infrastructure.Observation.Logger

open System
open Medhavi.Foundation.Observation
open Medhavi.Foundation.Observation.LogContext
open Microsoft.Extensions.Logging
open System.Collections.Generic

let private logWithLevel (logger: ILogger) (level: LogLevel) (message: string) (context: LogContext) (ex: exn option) =
    let state =
        [| yield KeyValuePair<string, obj>("{OriginalFormat}", box message)
           for k, v in contextToState context do
               yield KeyValuePair<string, obj>(k, v) |]
    let formatter = Func<KeyValuePair<string, obj>[], exn, string>(fun _ e ->
        if isNull (box e) then message else $"{message} (Exception: {e.Message})")
    let exceptionOrNull = match ex with Some e -> e | None -> null
    logger.Log(level, EventId(0), state, exceptionOrNull, formatter)

let logDebug (logger: ILogger) (message: string) (context: LogContext) =
    logWithLevel logger LogLevel.Debug message context None

let logInformation (logger: ILogger) (message: string) (context: LogContext) =
    logWithLevel logger LogLevel.Information message context None

let logWarning (logger: ILogger) (message: string) (context: LogContext) =
    logWithLevel logger LogLevel.Warning message context None

let logError (logger: ILogger) (message: string) (context: LogContext) =
    logWithLevel logger LogLevel.Error message context None

let logErrorWithException (logger: ILogger) (ex: exn) (message: string) (context: LogContext) =
    logWithLevel logger LogLevel.Error message context (Some ex)

let logCritical (logger: ILogger) (message: string) (context: LogContext) =
    logWithLevel logger LogLevel.Critical message context None

let logPerformance (logger: ILogger) (operation: string) (comp: string) (duration: TimeSpan) (_: LogContext) =
    let context =
        { LogContext.Empty with
            Operation = Some operation
            Component = comp
            Duration = Some duration }

    if duration.TotalMilliseconds > 1000.0 then
        logWarning logger $"Operation '{operation}' took {duration.TotalMilliseconds:F2}ms" context
    elif duration.TotalMilliseconds > 100.0 then
        logInformation logger $"Operation '{operation}' completed in {duration.TotalMilliseconds:F2}ms" context
    else
        logDebug logger $"Operation '{operation}' completed in {duration.TotalMilliseconds:F2}ms" context

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
            let wrapper = MailboxLoggerWrapper(mailboxLogger) :> ILogger

            { InnerLogger = wrapper
              Context = defaultArg context LogContext.Empty
              MailboxLogger = Some mailboxLogger }
        else
            { InnerLogger = logger
              Context = defaultArg context LogContext.Empty
              MailboxLogger = None }

    /// Flush mailbox logger if using mailbox implementation
    member this.Flush() = this.MailboxLogger |> Option.iter(fun mb -> mb.Flush())

    /// Shutdown mailbox logger if using mailbox implementation
    member this.Shutdown() = this.MailboxLogger |> Option.iter(fun mb -> mb.Shutdown())

    member this.getContext(context: LogContext option) =
        context |> Option.map(mergeContexts this.Context) |> Option.defaultValue this.Context

    member this.Log(logLevel: LogLevel, title: string, context: obj, message: string) =
        this.InnerLogger.Log(logLevel, title, context, message)

    member this.Info(message: string, ?context: LogContext) =
        this.getContext context |> logInformation this.InnerLogger message

    member this.Debug(message: string, ?context: LogContext) =
        this.getContext context |> logDebug this.InnerLogger message

    member this.Warning(message: string, ?context: LogContext) =
        this.getContext context |> logWarning this.InnerLogger message

    member this.Error(message: string, ?context: LogContext) =
        this.getContext context |> logError this.InnerLogger message

    member this.Error(ex: exn, message: string, ?context: LogContext) =
        this.getContext context |> logErrorWithException this.InnerLogger ex message

    member this.Critical(message: string, ?context: LogContext) =
        this.getContext context |> logCritical this.InnerLogger message

    member this.LogPerformance(operation: string, comp: string, duration: TimeSpan, ?context: LogContext) =
        this.getContext context |> logPerformance this.InnerLogger operation comp duration

let private mapLevel (knowledge: ArchitecturalKnowledge) : LogLevel =
    match knowledge.Attributes.TryFind "Severity" with
    | Some(:? string as s) ->
        match s with
        | "Critical" -> LogLevel.Critical
        | "Error" -> LogLevel.Error
        | "Warning" -> LogLevel.Warning
        | "Information" -> LogLevel.Information
        | "Debug" -> LogLevel.Debug
        | "Trace" -> LogLevel.Trace
        | _ -> LogLevel.Information
    | _ ->
        // Infer severity from the knowledge name
        if knowledge.Name.Contains "Failed" then LogLevel.Warning
        elif knowledge.Name.Contains "Error" then LogLevel.Error
        else LogLevel.Information

let toKnowledgeRepresentation (logger: Logger) : KnowledgeRepresentation =
    fun knowledge ->
        let level = mapLevel knowledge
        let logCtx =
            { LogContext.Empty with
                Component = knowledge.Name
                AdditionalData = Some knowledge.Attributes }

        match level with
        | LogLevel.Critical -> logger.Critical(knowledge.Name, logCtx)
        | LogLevel.Error -> logger.Error(knowledge.Name, logCtx)
        | LogLevel.Warning -> logger.Warning(knowledge.Name, logCtx)
        | LogLevel.Debug -> logger.Debug(knowledge.Name, logCtx)
        | LogLevel.Trace -> logger.Debug(knowledge.Name, logCtx)
        | _ -> logger.Info(knowledge.Name, logCtx)

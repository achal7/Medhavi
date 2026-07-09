module Medhavi.Infrastructure.Observation.Logger

open System
open Medhavi.SharedKernel.Observation
open Medhavi.SharedKernel.Observation.Logging.LogContext
open Microsoft.Extensions.Logging
open Medhavi.SharedKernel.Observation.Logging
open System.Collections.Generic

let logDebug (logger: ILogger) (message: string) (context: LogContext) =
    logger.LogDebug(message, contextToState context |> Seq.toArray)

let logInformation (logger: ILogger) (message: string) (context: LogContext) =
    logger.LogInformation(message, contextToState context |> Seq.toArray)

let logWarning (logger: ILogger) (message: string) (context: LogContext) =
    logger.LogWarning(message, contextToState context |> Seq.toArray)

let logError (logger: ILogger) (message: string) (context: LogContext) =
    logger.LogError(message, contextToState context |> Seq.toArray)

let logErrorWithException (logger: ILogger) (ex: exn) (message: string) (context: LogContext) =
    logger.LogError(ex, message, contextToState context |> Seq.toArray)

let logCritical (logger: ILogger) (message: string) (context: LogContext) =
    logger.LogCritical(message, contextToState context |> Seq.toArray)

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
        // Convert attributes to a state array for structured logging
        let state =
            knowledge.Attributes |> Map.toSeq |> Seq.map(fun (k, v) -> KeyValuePair<string, obj>(k, v)) |> Array.ofSeq

        let logCtx =
            { LogContext.Empty with
                Component = knowledge.Name
                AdditionalData = Some knowledge.Attributes }

        match knowledge.Attributes.TryFind "Error" with
        | Some _ -> logger.Warning(knowledge.Name, logCtx)
        | None -> logger.Info(knowledge.Name, logCtx)

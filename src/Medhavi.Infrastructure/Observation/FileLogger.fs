namespace Medhavi.Infrastructure.Observation

open System
open System.IO
open System.Text
open System.Collections.Generic
open Microsoft.Extensions.Logging
open Microsoft.FSharp.Reflection
open Medhavi.Common
open Medhavi.Common.Optics
open Medhavi.Foundation.Contracts
open Medhavi.Foundation.ExecutionContext

type FileLoggerOptions =
    { FilePathPattern: string
      MinimumLevel: LogLevel
      BatchSize: int
      FlushIntervalMs: int }

    static member Default =
        { FilePathPattern = "logs/medhavi-nexus-{yyyyMMdd}.log"
          MinimumLevel = LogLevel.Information
          BatchSize = 50
          FlushIntervalMs = 500 }

type FileLoggerSink =
    { Post: string -> unit
      Flush: unit -> unit
      Close: unit -> unit }

type private FileLogMessage =
    | Write of string
    | Flush of AsyncReplyChannel<unit>
    | Stop

module FileLoggerOptics =
    let filePathPatternLens: Lens<FileLoggerOptions, string> =
        lens (fun o -> o.FilePathPattern) (fun v o -> { o with FilePathPattern = v })

    let minimumLevelLens: Lens<FileLoggerOptions, LogLevel> =
        lens (fun o -> o.MinimumLevel) (fun v o -> { o with MinimumLevel = v })

    let batchSizeLens: Lens<FileLoggerOptions, int> =
        lens (fun o -> o.BatchSize) (fun v o -> { o with BatchSize = v })

    let flushIntervalLens: Lens<FileLoggerOptions, int> =
        lens (fun o -> o.FlushIntervalMs) (fun v o -> { o with FlushIntervalMs = v })

    let correlationIdIso: Iso<CorrelationId, string> =
        iso CorrelationId.toString (fun s ->
            match CorrelationId.fromString s with
            | Some cid -> cid
            | None -> CorrelationId.create ())

module FileLogger =

    let logLinesMonoid: Monoid<string list> = Monoid.list

    let private resolveCurrentPath (pattern: string) =
        let now = DateTime.UtcNow
        let resolved =
            if pattern.Contains("{yyyyMMdd}") then
                pattern.Replace("{yyyyMMdd}", now.ToString("yyyyMMdd"))
            elif pattern.Contains("{date}") then
                pattern.Replace("{date}", now.ToString("yyyy-MM-dd"))
            else
                pattern

        let fullPath =
            if Path.IsPathRooted(resolved) then resolved
            else Path.GetFullPath(resolved)

        let dir = Path.GetDirectoryName(fullPath)
        if not (String.IsNullOrEmpty(dir)) && not (Directory.Exists(dir)) then
            Directory.CreateDirectory(dir) |> ignore

        fullPath

    let private appendBatch (pattern: string) (lines: string list) =
        if not (List.isEmpty lines) then
            try
                let targetPath = resolveCurrentPath pattern
                let content = String.concat Environment.NewLine (List.rev lines) + Environment.NewLine
                File.AppendAllText(targetPath, content, Encoding.UTF8)
            with ex ->
                eprintfn $"[FileLogger] Error writing logs to disk: {ex.Message}"

    let rec private formatValue (v: obj) : string =
        match v with
        | null -> ""
        | :? string as s -> s.Replace("\r", "").Replace("\n", " ").Trim()
        | :? bool as b -> if b then "true" else "false"
        | :? int as i -> string i
        | :? int64 as l -> string l
        | :? float as f -> f.ToString("F2")
        | :? decimal as d -> string d
        | :? Guid as g -> g.ToString()
        | :? DateTime as dt -> dt.ToString("O")
        | :? DateTimeOffset as dto -> dto.ToString("O")
        | :? TimeSpan as ts -> $"{ts.TotalMilliseconds:F2}ms"
        | :? CorrelationId as cid -> FileLoggerOptics.correlationIdIso.Forward cid
        | :? DecisionTrace as dt ->
            let passedCount = dt.RulesEvaluated |> List.filter (fun r -> r.Passed) |> List.length
            let totalRules = dt.RulesEvaluated.Length
            let policyStr = dt.PolicyId |> Option.defaultValue "none"
            let summary = dt.Rationale.Summary.Replace("\r", "").Replace("\n", " ").Trim()
            $"Policy={policyStr}, Capability={dt.CapabilityId}, Outcome={dt.Outcome}, RulesPassed={passedCount}/{totalRules}, Rationale=\"{summary}\""
        | other ->
            let t = other.GetType()
            let itemProp =
                match t.GetProperty("Item", System.Reflection.BindingFlags.Public ||| System.Reflection.BindingFlags.NonPublic ||| System.Reflection.BindingFlags.Instance) with
                | null -> t.GetProperty("item", System.Reflection.BindingFlags.Public ||| System.Reflection.BindingFlags.NonPublic ||| System.Reflection.BindingFlags.Instance)
                | p -> p

            if not (isNull itemProp) && t.GetProperties(System.Reflection.BindingFlags.Public ||| System.Reflection.BindingFlags.NonPublic ||| System.Reflection.BindingFlags.Instance).Length = 1 then
                formatValue (itemProp.GetValue(other))
            elif FSharpType.IsUnion(t, System.Reflection.BindingFlags.Public ||| System.Reflection.BindingFlags.NonPublic) then
                let cases = FSharpType.GetUnionCases(t, System.Reflection.BindingFlags.Public ||| System.Reflection.BindingFlags.NonPublic)
                let case, fields = FSharpValue.GetUnionFields(other, t, System.Reflection.BindingFlags.Public ||| System.Reflection.BindingFlags.NonPublic)
                if t.IsGenericType && t.GetGenericTypeDefinition() = typedefof<option<_>> then
                    if case.Name = "Some" && fields.Length = 1 then
                        formatValue fields[0]
                    else
                        ""
                elif cases.Length = 1 && fields.Length = 1 then
                    formatValue fields[0]
                else
                    if fields.Length = 0 then case.Name
                    elif fields.Length = 1 then $"{case.Name}({formatValue fields[0]})"
                    else
                        let inners = fields |> Array.map formatValue |> String.concat ", "
                        $"{case.Name}({inners})"
            else
                let s = string other
                if s.EndsWith("\"") && s.Contains(" \"") then
                    let startQuote = s.IndexOf(" \"") + 2
                    s.Substring(startQuote, s.Length - startQuote - 1)
                else
                    s.Replace("\r", "").Replace("\n", " ").Trim()

    let private extractProperties (state: obj) : (string * string) list =
        match state with
        | null -> []
        | :? IReadOnlyList<KeyValuePair<string, obj>> as kvs ->
            [ for kv in kvs do
                if kv.Key <> "{OriginalFormat}" && not (isNull kv.Value) then
                    let formatted = formatValue kv.Value
                    if not (String.IsNullOrEmpty formatted) then
                        yield (kv.Key, formatted) ]
        | :? IEnumerable<KeyValuePair<string, obj>> as kvs ->
            [ for kv in kvs do
                if kv.Key <> "{OriginalFormat}" && not (isNull kv.Value) then
                    let formatted = formatValue kv.Value
                    if not (String.IsNullOrEmpty formatted) then
                        yield (kv.Key, formatted) ]
        | _ -> []

    let createSink (options: FileLoggerOptions) : FileLoggerSink =
        let mailbox =
            MailboxProcessor.Start(fun inbox ->
                let rec loop (batch: string list) =
                    async {
                        let! msgOpt = inbox.TryReceive(get FileLoggerOptics.flushIntervalLens options)
                        match msgOpt with
                        | Some (Write line) ->
                            let newBatch = Monoid.append logLinesMonoid [ line ] batch
                            if newBatch.Length >= get FileLoggerOptics.batchSizeLens options then
                                appendBatch (get FileLoggerOptics.filePathPatternLens options) newBatch
                                return! loop (Monoid.empty logLinesMonoid)
                            else
                                return! loop newBatch

                        | Some (Flush reply) ->
                            appendBatch (get FileLoggerOptics.filePathPatternLens options) batch
                            reply.Reply()
                            return! loop (Monoid.empty logLinesMonoid)

                        | Some Stop ->
                            appendBatch (get FileLoggerOptics.filePathPatternLens options) batch
                            return ()

                        | None ->
                            appendBatch (get FileLoggerOptics.filePathPatternLens options) batch
                            return! loop (Monoid.empty logLinesMonoid)
                    }
                loop (Monoid.empty logLinesMonoid))

        let flush () =
            try
                mailbox.PostAndAsyncReply(Flush, timeout = 3000) |> Async.RunSynchronously
            with _ -> ()

        let close () =
            try
                mailbox.Post Stop
            with _ -> ()

        { Post = fun line -> mailbox.Post(Write line)
          Flush = flush
          Close = close }

    let createLogger (categoryName: string) (sink: FileLoggerSink) (minLevel: LogLevel) : ILogger =
        { new ILogger with
            member _.BeginScope<'TState when 'TState: not null>(_: 'TState) =
                { new IDisposable with member _.Dispose() = () }

            member _.IsEnabled(logLevel: LogLevel) = logLevel >= minLevel

            member _.Log<'TState>(logLevel: LogLevel, _: Microsoft.Extensions.Logging.EventId, state: 'TState, ex: exn, formatter: Func<'TState, exn, string>) =
                if logLevel >= minLevel then
                    let timestamp = DateTimeOffset.UtcNow.ToString("O")
                    let rawMessage =
                        match formatter with
                        | null -> string state
                        | f -> f.Invoke(state, ex)

                    let props = extractProperties (box state)
                    let propMap = props |> Map.ofList

                    let bc = propMap.TryFind "BoundedContext" |> Option.defaultValue ""
                    let aggType = propMap.TryFind "AggregateType" |> Option.defaultValue ""
                    let aggId = propMap.TryFind "AggregateId" |> Option.defaultValue ""

                    let domainTag =
                        match bc, aggType, aggId with
                        | b, t, i when b <> "" && t <> "" && i <> "" -> $" [{b}/{t}:{i}]"
                        | b, t, _ when b <> "" && t <> "" -> $" [{b}/{t}]"
                        | _, t, i when t <> "" && i <> "" -> $" [{t}:{i}]"
                        | _, t, _ when t <> "" -> $" [{t}]"
                        | _ -> ""

                    let category =
                        if bc <> "" then $"Medhavi.{bc}" else categoryName

                    let corrSuffix =
                        match propMap.TryFind "CorrelationId" with
                        | Some c when not (String.IsNullOrEmpty c) -> $" [Corr={c}]"
                        | _ -> ""

                    let bodyProps =
                        props
                        |> List.filter (fun (k, _) ->
                            k <> "Component" &&
                            k <> "BoundedContext" &&
                            k <> "AggregateType" &&
                            k <> "AggregateId" &&
                            k <> "CorrelationId" &&
                            k <> "CausationId")

                    let propSuffix =
                        if List.isEmpty bodyProps then ""
                        else
                            let pairs = bodyProps |> List.map (fun (k, v) -> $"{k}={v}") |> String.concat ", "
                            $" | {pairs}"

                    let line =
                        if isNull (box ex) then
                            $"[{timestamp}] [{logLevel}]{domainTag} {category}: {rawMessage}{propSuffix}{corrSuffix}"
                        else
                            let exMsg = ex.ToString().Replace("\r", "").Replace("\n", " ").Trim()
                            $"[{timestamp}] [{logLevel}]{domainTag} {category}: {rawMessage}{propSuffix}{corrSuffix} | Exception={exMsg}"

                    sink.Post(line) }

    let createProvider (options: FileLoggerOptions) : ILoggerProvider * FileLoggerSink =
        let sink = createSink options
        let minLevel = get FileLoggerOptics.minimumLevelLens options
        let provider =
            { new ILoggerProvider with
                member _.CreateLogger(categoryName: string) = createLogger categoryName sink minLevel
                member _.Dispose() = sink.Close() }
        provider, sink

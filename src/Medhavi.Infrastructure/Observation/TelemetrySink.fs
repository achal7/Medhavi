namespace Medhavi.Infrastructure.Observation

open System
open System.IO
open System.Text
open System.Text.Json
open Microsoft.FSharp.Reflection
open Medhavi.Common
open Medhavi.Common.Optics
open Medhavi.Common.Codec
open Medhavi.Foundation.Observation
open Medhavi.Foundation.Contracts
open Medhavi.Foundation.ExecutionContext

type TelemetrySinkOptions =
    { ConsoleEnabled: bool
      FileEnabled: bool
      FilePathPattern: string
      MinimumSeverity: TelemetrySeverity
      BatchSize: int
      FlushIntervalMs: int }

    static member Default =
        { ConsoleEnabled = true
          FileEnabled = true
          FilePathPattern = "logs/telemetry-{yyyyMMdd}.jsonl"
          MinimumSeverity = TelemetrySeverity.Information
          BatchSize = 25
          FlushIntervalMs = 500 }

type TelemetrySink =
    { Publish: TelemetryEvent -> unit
      Flush: unit -> unit
      Shutdown: unit -> unit }

type private TelemetryMsg =
    | Ingest of TelemetryEvent
    | Flush of AsyncReplyChannel<unit>
    | Shutdown

module TelemetryOptics =
    let eventSeverityLens: Lens<TelemetryEvent, TelemetrySeverity> =
        lens (fun e -> e.Severity) (fun v e -> { e with Severity = v })

    let eventMessageLens: Lens<TelemetryEvent, string> =
        lens (fun e -> e.Message) (fun v e -> { e with Message = v })

    let eventPropertiesLens: Lens<TelemetryEvent, Map<string, obj>> =
        lens (fun e -> e.Properties) (fun v e -> { e with Properties = v })

    let eventCorrelationIdLens: Lens<TelemetryEvent, CorrelationId option> =
        lens (fun e -> e.CorrelationId) (fun v e -> { e with CorrelationId = v })

    let eventCausationIdLens: Lens<TelemetryEvent, CorrelationId option> =
        lens (fun e -> e.CausationId) (fun v e -> { e with CausationId = v })

    let sinkBatchSizeLens: Lens<TelemetrySinkOptions, int> =
        lens (fun o -> o.BatchSize) (fun v o -> { o with BatchSize = v })

    let sinkFlushIntervalLens: Lens<TelemetrySinkOptions, int> =
        lens (fun o -> o.FlushIntervalMs) (fun v o -> { o with FlushIntervalMs = v })

    let correlationIdIso: Iso<CorrelationId, string> =
        iso CorrelationId.toString (fun s ->
            match CorrelationId.fromString s with
            | Some cid -> cid
            | None -> CorrelationId.create ())

    let severityIso: Iso<TelemetrySeverity, string> =
        iso string (fun s ->
            match s.ToLowerInvariant() with
            | "trace" -> TelemetrySeverity.Trace
            | "debug" -> TelemetrySeverity.Debug
            | "information" | "info" -> TelemetrySeverity.Information
            | "warning" | "warn" -> TelemetrySeverity.Warning
            | "errors" | "error" -> TelemetrySeverity.Errors
            | "critical" -> TelemetrySeverity.Critical
            | _ -> TelemetrySeverity.Information)

module TelemetrySink =

    let telemetryBatchMonoid: Monoid<TelemetryEvent list> = Monoid.list
    let telemetryLinesMonoid: Monoid<string list> = Monoid.list

    let private severityLevel = function
        | TelemetrySeverity.Trace -> 0
        | TelemetrySeverity.Debug -> 1
        | TelemetrySeverity.Information -> 2
        | TelemetrySeverity.Warning -> 3
        | TelemetrySeverity.Errors -> 4
        | TelemetrySeverity.Critical -> 5

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

    let private unwrapDecisionTrace (dt: DecisionTrace) : obj =
        let rules =
            dt.RulesEvaluated
            |> List.map (fun r ->
                dict [ ("RuleId", box r.RuleId)
                       ("Passed", box r.Passed)
                       ("Evidence", box (r.Evidence |> List.toArray))
                       ("ReasonCode", match r.ReasonCode with Some rc -> box rc | None -> null) ])
            |> List.toArray
        let rationale =
            dict [ ("Summary", box dt.Rationale.Summary)
                   ("Evidence", box (dt.Rationale.Evidence |> List.toArray))
                   ("Alternatives", box (dt.Rationale.Alternatives |> List.map (fun (k, v) -> dict [ ("Key", box k); ("Value", box v) ]) |> List.toArray)) ]
        dict [ ("DecisionId", box dt.DecisionId)
               ("CausalDecisionIds", box (dt.CausalDecisionIds |> List.toArray))
               ("CapabilityId", box dt.CapabilityId)
               ("Outcome", box dt.Outcome)
               ("RulesEvaluated", box rules)
               ("PolicyId", match dt.PolicyId with Some p -> box p | None -> null)
               ("PolicyVersion", match dt.PolicyVersion with Some v -> box v | None -> null)
               ("SemanticObjectIds", box (dt.SemanticObjectIds |> List.toArray))
               ("Rationale", box rationale) ]
        |> box

    let rec private unwrapValue (v: obj) : obj =
        match v with
        | null -> null
        | :? string as s -> box s
        | :? bool as b -> box b
        | :? int as i -> box i
        | :? int64 as l -> box l
        | :? float as f -> box f
        | :? decimal as d -> box d
        | :? Guid as g -> box (g.ToString())
        | :? DateTime as dt -> box (dt.ToString("O"))
        | :? DateTimeOffset as dto -> box (dto.ToString("O"))
        | :? TimeSpan as ts -> box ts.TotalMilliseconds
        | :? CorrelationId as cid -> box (TelemetryOptics.correlationIdIso.Forward cid)
        | :? DecisionTrace as dt -> unwrapDecisionTrace dt
        | :? System.Collections.IEnumerable as ie when not (v :? string) ->
            let items = [ for item in ie -> unwrapValue item ] |> List.toArray
            box items
        | other ->
            let t = other.GetType()
            let itemProp =
                match t.GetProperty("Item", System.Reflection.BindingFlags.Public ||| System.Reflection.BindingFlags.NonPublic ||| System.Reflection.BindingFlags.Instance) with
                | null -> t.GetProperty("item", System.Reflection.BindingFlags.Public ||| System.Reflection.BindingFlags.NonPublic ||| System.Reflection.BindingFlags.Instance)
                | p -> p

            if not (isNull itemProp) && t.GetProperties(System.Reflection.BindingFlags.Public ||| System.Reflection.BindingFlags.NonPublic ||| System.Reflection.BindingFlags.Instance).Length = 1 then
                unwrapValue (itemProp.GetValue(other))
            elif FSharpType.IsUnion(t, System.Reflection.BindingFlags.Public ||| System.Reflection.BindingFlags.NonPublic) then
                let cases = FSharpType.GetUnionCases(t, System.Reflection.BindingFlags.Public ||| System.Reflection.BindingFlags.NonPublic)
                let case, fields = FSharpValue.GetUnionFields(other, t, System.Reflection.BindingFlags.Public ||| System.Reflection.BindingFlags.NonPublic)
                if t.IsGenericType && t.GetGenericTypeDefinition() = typedefof<option<_>> then
                    if case.Name = "Some" && fields.Length = 1 then
                        unwrapValue fields[0]
                    else
                        null
                elif cases.Length = 1 && fields.Length = 1 then
                    unwrapValue fields[0]
                else
                    if fields.Length = 0 then
                        box case.Name
                    elif fields.Length = 1 then
                        let inner = unwrapValue fields[0]
                        dict [ ("Case", box case.Name); ("Value", inner) ] |> box
                    else
                        let inners = fields |> Array.map unwrapValue
                        dict [ ("Case", box case.Name); ("Values", box inners) ] |> box
            else
                let s = string other
                if s.EndsWith("\"") && s.Contains(" \"") then
                    let startQuote = s.IndexOf(" \"") + 2
                    box (s.Substring(startQuote, s.Length - startQuote - 1))
                else
                    box (s.Replace("\r", "").Replace("\n", " ").Trim())

    let private decodeDecisionTrace (elem: JsonElement) : DecisionTrace option =
        try
            let decId = elem.GetProperty("DecisionId").GetString()
            let capId = elem.GetProperty("CapabilityId").GetString()
            let outcome = elem.GetProperty("Outcome").GetString()
            let mutable polElem = Unchecked.defaultof<JsonElement>
            let polId =
                if elem.TryGetProperty("PolicyId", &polElem) && polElem.ValueKind = JsonValueKind.String then
                    Some (polElem.GetString())
                else None
            let mutable polVerElem = Unchecked.defaultof<JsonElement>
            let polVer =
                if elem.TryGetProperty("PolicyVersion", &polVerElem) && polVerElem.ValueKind = JsonValueKind.Number then
                    Some (polVerElem.GetInt32())
                else None
            let mutable causalElem = Unchecked.defaultof<JsonElement>
            let causalIds =
                if elem.TryGetProperty("CausalDecisionIds", &causalElem) && causalElem.ValueKind = JsonValueKind.Array then
                    [ for item in causalElem.EnumerateArray() -> item.GetString() ]
                else []
            let mutable semElem = Unchecked.defaultof<JsonElement>
            let semanticIds =
                if elem.TryGetProperty("SemanticObjectIds", &semElem) && semElem.ValueKind = JsonValueKind.Array then
                    [ for item in semElem.EnumerateArray() -> item.GetString() ]
                else []
            let mutable rulesElem = Unchecked.defaultof<JsonElement>
            let rules =
                if elem.TryGetProperty("RulesEvaluated", &rulesElem) && rulesElem.ValueKind = JsonValueKind.Array then
                    [ for r in rulesElem.EnumerateArray() do
                        let rid = r.GetProperty("RuleId").GetString()
                        let passed = r.GetProperty("Passed").GetBoolean()
                        let mutable rcElem = Unchecked.defaultof<JsonElement>
                        let rcode =
                            if r.TryGetProperty("ReasonCode", &rcElem) && rcElem.ValueKind = JsonValueKind.String then
                                Some (rcElem.GetString())
                            else None
                        let mutable evElem = Unchecked.defaultof<JsonElement>
                        let evList =
                            if r.TryGetProperty("Evidence", &evElem) && evElem.ValueKind = JsonValueKind.Array then
                                [ for e in evElem.EnumerateArray() -> e.GetString() ]
                            else []
                        yield { RuleId = rid; Passed = passed; Evidence = evList; ReasonCode = rcode } ]
                else []
            let mutable ratElem = Unchecked.defaultof<JsonElement>
            let rationale =
                if elem.TryGetProperty("Rationale", &ratElem) && ratElem.ValueKind = JsonValueKind.Object then
                    let mutable sElem = Unchecked.defaultof<JsonElement>
                    let summary =
                        if ratElem.TryGetProperty("Summary", &sElem) && sElem.ValueKind = JsonValueKind.String then sElem.GetString()
                        else ""
                    let mutable evElem = Unchecked.defaultof<JsonElement>
                    let evList =
                        if ratElem.TryGetProperty("Evidence", &evElem) && evElem.ValueKind = JsonValueKind.Array then
                            [ for e in evElem.EnumerateArray() -> e.GetString() ]
                        else []
                    let mutable altElem = Unchecked.defaultof<JsonElement>
                    let alts =
                        if ratElem.TryGetProperty("Alternatives", &altElem) && altElem.ValueKind = JsonValueKind.Array then
                            [ for item in altElem.EnumerateArray() do
                                if item.ValueKind = JsonValueKind.Array && item.GetArrayLength() = 2 then
                                    let arr = item.EnumerateArray() |> Seq.toArray
                                    yield (arr.[0].GetString(), arr.[1].GetString()) ]
                        else []
                    { Summary = summary; Evidence = evList; Alternatives = alts }
                else
                    { Summary = ""; Evidence = []; Alternatives = [] }

            Some {
                DecisionId = decId
                CausalDecisionIds = causalIds
                CapabilityId = capId
                Outcome = outcome
                RulesEvaluated = rules
                PolicyId = polId
                PolicyVersion = polVer
                SemanticObjectIds = semanticIds
                Rationale = rationale
            }
        with _ -> None

    let telemetryEventCodec: Codec<TelemetryEvent> =
        { Encode = fun evt ->
            try
                let props =
                    (get TelemetryOptics.eventPropertiesLens evt)
                    |> Map.toSeq
                    |> Seq.map (fun (k, v) -> (k, unwrapValue v))
                    |> Seq.filter (fun (_, v) -> not (isNull v))
                    |> dict

                let bc =
                    (get TelemetryOptics.eventPropertiesLens evt).TryFind "BoundedContext"
                    |> Option.map unwrapValue
                    |> Option.toObj

                let aggType =
                    (get TelemetryOptics.eventPropertiesLens evt).TryFind "AggregateType"
                    |> Option.map unwrapValue
                    |> Option.toObj

                let aggId =
                    (get TelemetryOptics.eventPropertiesLens evt).TryFind "AggregateId"
                    |> Option.map unwrapValue
                    |> Option.toObj

                let cmdType =
                    (get TelemetryOptics.eventPropertiesLens evt).TryFind "CommandType"
                    |> Option.map unwrapValue
                    |> Option.toObj

                let corrId =
                    (get TelemetryOptics.eventCorrelationIdLens evt)
                    |> Option.map TelemetryOptics.correlationIdIso.Forward
                    |> Option.toObj

                let causId =
                    (get TelemetryOptics.eventCausationIdLens evt)
                    |> Option.map TelemetryOptics.correlationIdIso.Forward
                    |> Option.toObj

                let dto =
                    dict [ ("EventId", box (evt.EventId.ToString()))
                           ("Timestamp", box (evt.Timestamp.ToString("O")))
                           ("Severity", box (TelemetryOptics.severityIso.Forward evt.Severity))
                           ("BoundedContext", bc)
                           ("AggregateType", aggType)
                           ("AggregateId", aggId)
                           ("CommandType", cmdType)
                           ("Message", box (get TelemetryOptics.eventMessageLens evt))
                           ("Properties", box props)
                           ("CorrelationId", box corrId)
                           ("CausationId", box causId)
                           ("TraceId", box (Option.toObj evt.TraceId))
                           ("SpanId", box (Option.toObj evt.SpanId)) ]

                Ok (JsonSerializer.Serialize(dto))
            with ex ->
                Error (EncodeError $"Failed to encode TelemetryEvent: {ex.Message}")

          Decode = fun json ->
            try
                use doc = JsonDocument.Parse(json)
                let root = doc.RootElement
                let eventId = Guid.Parse(root.GetProperty("EventId").GetString())
                let ts = DateTimeOffset.Parse(root.GetProperty("Timestamp").GetString())
                let sev = TelemetryOptics.severityIso.Backward(root.GetProperty("Severity").GetString())
                let msg = root.GetProperty("Message").GetString()
                let corrId =
                    let mutable cElem = Unchecked.defaultof<JsonElement>
                    if root.TryGetProperty("CorrelationId", &cElem) && cElem.ValueKind = JsonValueKind.String then
                        CorrelationId.fromString (cElem.GetString())
                    else None
                let causId =
                    let mutable cElem = Unchecked.defaultof<JsonElement>
                    if root.TryGetProperty("CausationId", &cElem) && cElem.ValueKind = JsonValueKind.String then
                        CorrelationId.fromString (cElem.GetString())
                    else None
                let traceId =
                    let mutable tElem = Unchecked.defaultof<JsonElement>
                    if root.TryGetProperty("TraceId", &tElem) && tElem.ValueKind = JsonValueKind.String then
                        Some (tElem.GetString())
                    else None
                let spanId =
                    let mutable sElem = Unchecked.defaultof<JsonElement>
                    if root.TryGetProperty("SpanId", &sElem) && sElem.ValueKind = JsonValueKind.String then
                        Some (sElem.GetString())
                    else None

                let mutable propsMap = Map.empty

                let addTopProp (name: string) =
                    let mutable el = Unchecked.defaultof<JsonElement>
                    if root.TryGetProperty(name, &el) && el.ValueKind = JsonValueKind.String then
                        propsMap <- Map.add name (box (el.GetString())) propsMap

                addTopProp "BoundedContext"
                addTopProp "AggregateType"
                addTopProp "AggregateId"
                addTopProp "CommandType"

                let mutable propsElem = Unchecked.defaultof<JsonElement>
                if root.TryGetProperty("Properties", &propsElem) && propsElem.ValueKind = JsonValueKind.Object then
                    for prop in propsElem.EnumerateObject() do
                        let key = prop.Name
                        let el = prop.Value
                        match el.ValueKind with
                        | JsonValueKind.String ->
                            propsMap <- Map.add key (box (el.GetString())) propsMap
                        | JsonValueKind.Number ->
                            let mutable d = 0.0
                            if el.TryGetDouble(&d) then
                                propsMap <- Map.add key (box d) propsMap
                        | JsonValueKind.True ->
                            propsMap <- Map.add key (box true) propsMap
                        | JsonValueKind.False ->
                            propsMap <- Map.add key (box false) propsMap
                        | JsonValueKind.Object when key = "DecisionTrace" ->
                            match decodeDecisionTrace el with
                            | Some dt -> propsMap <- Map.add "DecisionTrace" (box dt) propsMap
                            | None -> ()
                        | _ -> ()

                Ok { EventId = eventId
                     Timestamp = ts
                     Severity = sev
                     Message = msg
                     Properties = propsMap
                     CorrelationId = corrId
                     CausationId = causId
                     TraceId = traceId
                     SpanId = spanId }
            with ex ->
                Error (DecodeError $"Failed to decode TelemetryEvent: {ex.Message}")
        }

    let private writeConsole (options: TelemetrySinkOptions) (isEnabled: TelemetrySeverity -> bool) (evt: TelemetryEvent) =
        if options.ConsoleEnabled && isEnabled (get TelemetryOptics.eventSeverityLens evt) then
            let ts = evt.Timestamp.ToString("O")
            let propSummary =
                if evt.Properties.IsEmpty then ""
                else
                    let pairs =
                        evt.Properties
                        |> Map.toSeq
                        |> Seq.filter (fun (k, _) -> k <> "CorrelationId" && k <> "CausationId")
                        |> Seq.map (fun (k, v) ->
                            let uv = unwrapValue v
                            if isNull uv then ""
                            elif k = "DecisionTrace" then
                                match v with
                                | :? DecisionTrace as dt ->
                                    let pol = dt.PolicyId |> Option.defaultValue "none"
                                    let passed = dt.RulesEvaluated |> List.filter (fun r -> r.Passed) |> List.length
                                    $"DecisionTrace=[Policy={pol}, Outcome={dt.Outcome}, Passed={passed}/{dt.RulesEvaluated.Length}]"
                                | _ -> $"{k}={uv}"
                            else
                                $"{k}={uv}")
                        |> Seq.filter (not << String.IsNullOrEmpty)
                        |> String.concat ", "
                    if String.IsNullOrEmpty pairs then "" else $" | {pairs}"

            let corrStr =
                match get TelemetryOptics.eventCorrelationIdLens evt with
                | Some c -> $" [Corr={TelemetryOptics.correlationIdIso.Forward c}]"
                | None -> ""

            match evt.Severity with
            | TelemetrySeverity.Critical
            | TelemetrySeverity.Errors ->
                eprintfn $"[{ts}] [Telemetry {evt.Severity}] {evt.Message}{propSummary}{corrStr}"
            | _ ->
                printfn $"[{ts}] [Telemetry {evt.Severity}] {evt.Message}{propSummary}{corrStr}"

    let private appendBatch (pattern: string) (isEnabled: TelemetrySeverity -> bool) (events: TelemetryEvent list) =
        if not (List.isEmpty events) then
            try
                let targetPath = resolveCurrentPath pattern
                let lines =
                    events
                    |> List.filter (fun e -> isEnabled (get TelemetryOptics.eventSeverityLens e))
                    |> List.choose (fun e ->
                        match Codec.encode telemetryEventCodec e with
                        | Ok jsonLine -> Some jsonLine
                        | Error (EncodeError err) ->
                            eprintfn $"[TelemetrySink] Telemetry event codec encoding failure: {err}"
                            None
                        | Error (DecodeError err) -> None)

                if not (List.isEmpty lines) then
                    let content = String.concat Environment.NewLine (List.rev lines) + Environment.NewLine
                    File.AppendAllText(targetPath, content, Encoding.UTF8)
            with ex ->
                eprintfn $"[TelemetrySink] Error writing telemetry to disk: {ex.Message}"

    let create (options: TelemetrySinkOptions) : TelemetrySink =
        let minLevel = severityLevel options.MinimumSeverity
        let isEnabled (sev: TelemetrySeverity) = severityLevel sev >= minLevel

        let mailbox =
            MailboxProcessor.Start(fun inbox ->
                let rec loop (batch: TelemetryEvent list) =
                    async {
                        let! msgOpt = inbox.TryReceive(get TelemetryOptics.sinkFlushIntervalLens options)
                        match msgOpt with
                        | Some (Ingest evt) ->
                            writeConsole options isEnabled evt
                            let newBatch = Monoid.append telemetryBatchMonoid [ evt ] batch
                            if newBatch.Length >= get TelemetryOptics.sinkBatchSizeLens options then
                                if options.FileEnabled then appendBatch options.FilePathPattern isEnabled newBatch
                                return! loop (Monoid.empty telemetryBatchMonoid)
                            else
                                return! loop newBatch

                        | Some (Flush reply) ->
                            if options.FileEnabled then appendBatch options.FilePathPattern isEnabled batch
                            reply.Reply()
                            return! loop (Monoid.empty telemetryBatchMonoid)

                        | Some Shutdown ->
                            if options.FileEnabled then appendBatch options.FilePathPattern isEnabled batch
                            return ()

                        | None ->
                            if options.FileEnabled then appendBatch options.FilePathPattern isEnabled batch
                            return! loop (Monoid.empty telemetryBatchMonoid)
                    }
                loop (Monoid.empty telemetryBatchMonoid))

        let flush () =
            try
                mailbox.PostAndAsyncReply(Flush, timeout = 3000) |> Async.RunSynchronously
            with _ -> ()

        let shutdown () =
            try
                mailbox.Post Shutdown
            with _ -> ()

        { Publish = fun evt -> mailbox.Post(Ingest evt)
          Flush = flush
          Shutdown = shutdown }

    let toTelemetryPublisher (sink: TelemetrySink) : TelemetryEvent -> unit =
        fun evt -> sink.Publish(evt)

namespace Medhavi.Infrastructure.Observation

open System
open Medhavi.Common
open Medhavi.Common.Optics
open Medhavi.Foundation.Observation
open Medhavi.Foundation.Contracts
open Medhavi.Foundation.ExecutionContext

/// Summary of a cross-domain hop for visualization (e.g., in a Sankey diagram).
type DomainTransition =
    { SourceContext: string
      TargetContext: string
      EventCount: int
      AverageLatencyMs: float }

/// Stage execution timing detail for a command.
type StageExecutionDetail =
    { StageName: string
      DurationMs: float }

/// Unified investigative dossier for a specific CorrelationId or transaction.
type TraceDossier =
    { CorrelationId: string
      RootCommandType: string option
      BoundedContextsInvolved: string list
      TotalDurationMs: float
      EventCount: int
      Events: TelemetryEvent list
      DecisionTraces: DecisionTrace list
      Transitions: DomainTransition list
      StageTimings: StageExecutionDetail list }

/// Query parameters for filtering telemetry streams.
type TelemetryQuery =
    { SearchTerm: string option
      CorrelationId: string option
      AggregateId: string option
      BoundedContext: string option
      MinimumSeverity: TelemetrySeverity option
      StartTime: DateTimeOffset option
      EndTime: DateTimeOffset option
      Limit: int
      Skip: int }

    static member Default =
        { SearchTerm = None
          CorrelationId = None
          AggregateId = None
          BoundedContext = None
          MinimumSeverity = None
          StartTime = None
          EndTime = None
          Limit = 100
          Skip = 0 }

type TelemetryQueryResult =
    { Events: TelemetryEvent list
      TotalMatches: int }

module ObservationOptics =
    let queryLimitLens: Lens<TelemetryQuery, int> =
        lens (fun q -> q.Limit) (fun v q -> { q with Limit = v })

    let querySkipLens: Lens<TelemetryQuery, int> =
        lens (fun q -> q.Skip) (fun v q -> { q with Skip = v })

    let queryStartTimeLens: Lens<TelemetryQuery, DateTimeOffset option> =
        lens (fun q -> q.StartTime) (fun v q -> { q with StartTime = v })

    let queryEndTimeLens: Lens<TelemetryQuery, DateTimeOffset option> =
        lens (fun q -> q.EndTime) (fun v q -> { q with EndTime = v })

    let queryCorrelationIdLens: Lens<TelemetryQuery, string option> =
        lens (fun q -> q.CorrelationId) (fun v q -> { q with CorrelationId = v })

    let queryAggregateIdLens: Lens<TelemetryQuery, string option> =
        lens (fun q -> q.AggregateId) (fun v q -> { q with AggregateId = v })

    let queryBoundedContextLens: Lens<TelemetryQuery, string option> =
        lens (fun q -> q.BoundedContext) (fun v q -> { q with BoundedContext = v })

    let querySearchTermLens: Lens<TelemetryQuery, string option> =
        lens (fun q -> q.SearchTerm) (fun v q -> { q with SearchTerm = v })

    let queryMinimumSeverityLens: Lens<TelemetryQuery, TelemetrySeverity option> =
        lens (fun q -> q.MinimumSeverity) (fun v q -> { q with MinimumSeverity = v })

    let dossierCorrelationIdLens: Lens<TraceDossier, string> =
        lens (fun d -> d.CorrelationId) (fun v d -> { d with CorrelationId = v })

    let dossierTotalDurationLens: Lens<TraceDossier, float> =
        lens (fun d -> d.TotalDurationMs) (fun v d -> { d with TotalDurationMs = v })

    let dossierEventsLens: Lens<TraceDossier, TelemetryEvent list> =
        lens (fun d -> d.Events) (fun v d -> { d with Events = v })

module ObservationMonoids =
    /// Monoid over filtering predicates: combines two predicates with boolean conjunction (AND).
    let predicateMonoid: Monoid<TelemetryEvent -> bool> =
        { Empty = fun _ -> true
          Combine = fun p1 p2 -> fun e -> p1 e && p2 e }

    /// Monoid for combining transition event counts and cumulative latencies.
    let transitionStatsMonoid: Monoid<int * float> =
        { Empty = (0, 0.0)
          Combine = fun (c1, l1) (c2, l2) -> (c1 + c2, l1 + l2) }

/// Pure functional capability record for querying the observation subsystem.
/// Replaces OOP interfaces with higher-order function closures.
type ObservationQueryOps =
    { Record: TelemetryEvent -> unit
      QueryEvents: TelemetryQuery -> TelemetryQueryResult
      GetTraceDossier: string -> TraceDossier option
      GetRecentTransitions: unit -> DomainTransition list
      HydrateFromFile: string -> int
      HydrateFromLines: seq<string> -> int }

type private ObservationState =
    { Capacity: int
      Count: int
      Events: TelemetryEvent list
      CorrelationIndex: Map<string, TelemetryEvent list>
      AggregateIndex: Map<string, TelemetryEvent list>
      DecisionIndex: Map<string, DecisionTrace> }

module private ObservationState =
    let empty capacity : ObservationState =
        { Capacity = capacity
          Count = 0
          Events = []
          CorrelationIndex = Map.empty
          AggregateIndex = Map.empty
          DecisionIndex = Map.empty }

    let ingest (evt: TelemetryEvent) (state: ObservationState) : ObservationState =
        let nextCount = state.Count + 1
        let nextEvents =
            if nextCount > state.Capacity + 500 then
                evt :: (state.Events |> List.truncate (state.Capacity - 1))
            else
                evt :: state.Events

        let nextCorrIndex =
            match evt.CorrelationId with
            | Some cid ->
                let cStr = CorrelationId.toString cid
                let existing = state.CorrelationIndex.TryFind cStr |> Option.defaultValue []
                state.CorrelationIndex.Add(cStr, evt :: existing)
            | None -> state.CorrelationIndex

        let nextAggIndex =
            match evt.Properties.TryFind "AggregateId" with
            | Some aggIdObj ->
                let aggStr = string aggIdObj
                if not (String.IsNullOrEmpty aggStr) then
                    let existing = state.AggregateIndex.TryFind aggStr |> Option.defaultValue []
                    state.AggregateIndex.Add(aggStr, evt :: existing)
                else state.AggregateIndex
            | None -> state.AggregateIndex

        let nextDecIndex =
            match evt.Properties.TryFind "DecisionTrace" with
            | Some (:? DecisionTrace as dt) ->
                state.DecisionIndex.Add(dt.DecisionId, dt)
            | _ -> state.DecisionIndex

        { Capacity = state.Capacity
          Count = if nextCount > state.Capacity + 500 then state.Capacity else nextCount
          Events = nextEvents
          CorrelationIndex = nextCorrIndex
          AggregateIndex = nextAggIndex
          DecisionIndex = nextDecIndex }

    let ingestBatch (evts: seq<TelemetryEvent>) (state: ObservationState) : ObservationState =
        evts |> Seq.fold (fun s e -> ingest e s) state

    let severityRank = function
        | TelemetrySeverity.Trace -> 0
        | TelemetrySeverity.Debug -> 1
        | TelemetrySeverity.Information -> 2
        | TelemetrySeverity.Warning -> 3
        | TelemetrySeverity.Errors -> 4
        | TelemetrySeverity.Critical -> 5

    let parseStageTimings (timingStr: string) : StageExecutionDetail list =
        if String.IsNullOrWhiteSpace timingStr then []
        else
            timingStr.Split(',', StringSplitOptions.RemoveEmptyEntries)
            |> Array.choose (fun part ->
                let kv = part.Trim().Split('=')
                if kv.Length = 2 then
                    let name = kv[0].Trim()
                    let rawMs = kv[1].Replace("ms", "").Trim()
                    match Double.TryParse rawMs with
                    | true, ms -> Some { StageName = name; DurationMs = ms }
                    | false, _ -> None
                else None)
            |> Array.toList

    let severityPredicate (minSevOpt: TelemetrySeverity option) : TelemetryEvent -> bool =
        match minSevOpt with
        | Some minSev ->
            let minRank = severityRank minSev
            fun e -> severityRank e.Severity >= minRank
        | None -> fun _ -> true

    let timeRangePredicate (startOpt: DateTimeOffset option) (endOpt: DateTimeOffset option) : TelemetryEvent -> bool =
        match startOpt, endOpt with
        | Some st, Some et -> fun e -> e.Timestamp >= st && e.Timestamp <= et
        | Some st, None -> fun e -> e.Timestamp >= st
        | None, Some et -> fun e -> e.Timestamp <= et
        | None, None -> fun _ -> true

    let contextPredicate (bcOpt: string option) : TelemetryEvent -> bool =
        match bcOpt with
        | Some bc ->
            fun e ->
                match e.Properties.TryFind "BoundedContext" with
                | Some b -> String.Equals(string b, bc, StringComparison.OrdinalIgnoreCase)
                | None -> false
        | None -> fun _ -> true

    let searchTermPredicate (termOpt: string option) : TelemetryEvent -> bool =
        match termOpt with
        | Some term when not (String.IsNullOrWhiteSpace term) ->
            fun e ->
                e.Message.Contains(term, StringComparison.OrdinalIgnoreCase)
                || (match e.CorrelationId with Some c -> (CorrelationId.toString c).Contains(term, StringComparison.OrdinalIgnoreCase) | None -> false)
                || (match e.Properties.TryFind "AggregateId" with Some a -> (string a).Contains(term, StringComparison.OrdinalIgnoreCase) | None -> false)
                || (match e.Properties.TryFind "CommandType" with Some c -> (string c).Contains(term, StringComparison.OrdinalIgnoreCase) | None -> false)
        | _ -> fun _ -> true

    let compositeFilter (q: TelemetryQuery) : TelemetryEvent -> bool =
        [ severityPredicate (get ObservationOptics.queryMinimumSeverityLens q)
          timeRangePredicate (get ObservationOptics.queryStartTimeLens q) (get ObservationOptics.queryEndTimeLens q)
          contextPredicate (get ObservationOptics.queryBoundedContextLens q)
          searchTermPredicate (get ObservationOptics.querySearchTermLens q) ]
        |> Monoid.combineAll ObservationMonoids.predicateMonoid

    let queryEvents (q: TelemetryQuery) (state: ObservationState) : TelemetryQueryResult =
        let candidateEvents =
            match get ObservationOptics.queryCorrelationIdLens q with
            | Some cid ->
                state.CorrelationIndex.TryFind cid |> Option.defaultValue []
            | None ->
                match get ObservationOptics.queryAggregateIdLens q with
                | Some aggId ->
                    state.AggregateIndex.TryFind aggId |> Option.defaultValue []
                | None ->
                    state.Events

        let filter = compositeFilter q
        let filtered =
            candidateEvents
            |> List.filter filter
            |> List.sortByDescending (fun e -> e.Timestamp)

        let paged =
            filtered
            |> List.skip (max 0 (get ObservationOptics.querySkipLens q))
            |> List.truncate (max 1 (get ObservationOptics.queryLimitLens q))

        { Events = paged; TotalMatches = filtered.Length }

    let buildTraceDossier (correlationId: string) (state: ObservationState) : TraceDossier option =
        match state.CorrelationIndex.TryFind correlationId with
        | Some events when not (List.isEmpty events) ->
            let sortedEvents = events |> List.sortBy (fun e -> e.Timestamp)

            let rootCmd =
                sortedEvents
                |> List.tryPick (fun e ->
                    e.Properties.TryFind "CommandType"
                    |> Option.map string)

            let contexts =
                sortedEvents
                |> List.choose (fun e ->
                    e.Properties.TryFind "BoundedContext"
                    |> Option.map string)
                |> List.distinct

            let minTime = sortedEvents.Head.Timestamp
            let maxTime = (List.last sortedEvents).Timestamp
            let totalMs = (maxTime - minTime).TotalMilliseconds

            let decisions =
                sortedEvents
                |> List.choose (fun e ->
                    match e.Properties.TryFind "DecisionTrace" with
                    | Some (:? DecisionTrace as dt) -> Some dt
                    | _ -> None)

            let stageTimings =
                sortedEvents
                |> List.choose (fun e ->
                    match e.Properties.TryFind "StageTimings" with
                    | Some t -> Some (parseStageTimings (string t))
                    | None -> None)
                |> List.concat

            let transitions =
                sortedEvents
                |> List.choose (fun e ->
                    e.Properties.TryFind "BoundedContext"
                    |> Option.map (fun bc -> (string bc, e.Timestamp)))
                |> Seq.pairwise
                |> Seq.choose (fun ((c1, t1), (c2, t2)) ->
                    if c1 <> c2 then
                        Some
                            { SourceContext = c1
                              TargetContext = c2
                              EventCount = 1
                              AverageLatencyMs = (t2 - t1).TotalMilliseconds }
                    else None)
                |> Seq.toList

            Some
                { CorrelationId = correlationId
                  RootCommandType = rootCmd
                  BoundedContextsInvolved = contexts
                  TotalDurationMs = totalMs
                  EventCount = sortedEvents.Length
                  Events = sortedEvents
                  DecisionTraces = decisions
                  Transitions = transitions
                  StageTimings = stageTimings }
        | _ -> None

    let getRecentTransitions (state: ObservationState) : DomainTransition list =
        state.Events
        |> List.rev
        |> List.choose (fun e ->
            e.Properties.TryFind "BoundedContext"
            |> Option.map (fun bc -> (string bc, e.Timestamp)))
        |> Seq.pairwise
        |> Seq.choose (fun ((c1, t1), (c2, t2)) ->
            if c1 <> c2 then
                let lat = (t2 - t1).TotalMilliseconds
                Some ((c1, c2), (1, lat))
            else None)
        |> Seq.groupBy fst
        |> Seq.map (fun (key, group) ->
            let (src, tgt) = key
            let (count, totalLat) =
                group
                |> Seq.map snd
                |> Monoid.combineAll ObservationMonoids.transitionStatsMonoid
            let avgLat = if count > 0 then totalLat / float count else 0.0
            { SourceContext = src
              TargetContext = tgt
              EventCount = count
              AverageLatencyMs = avgLat })
        |> Seq.toList

module ObservationQueryService =

    type private ObservationMsg =
        | Ingest of TelemetryEvent
        | IngestBatch of TelemetryEvent list * AsyncReplyChannel<int>
        | Query of TelemetryQuery * AsyncReplyChannel<TelemetryQueryResult>
        | GetDossier of string * AsyncReplyChannel<TraceDossier option>
        | GetTransitions of AsyncReplyChannel<DomainTransition list>

    let private hydrateFromLines (lines: seq<string>) (agent: MailboxProcessor<ObservationMsg>) : int =
        let decoded =
            lines
            |> Seq.filter (not << String.IsNullOrWhiteSpace)
            |> Seq.choose (fun line ->
                match TelemetrySink.telemetryEventCodec.Decode line with
                | Ok evt -> Some evt
                | Error _ -> None)
            |> Seq.toList

        if List.isEmpty decoded then 0
        else
            agent.PostAndReply(fun ch -> IngestBatch(decoded, ch))

    let private hydrateFromFile (filePath: string) (agent: MailboxProcessor<ObservationMsg>) : int =
        if System.IO.File.Exists filePath then
            try
                let lines = System.IO.File.ReadLines filePath
                hydrateFromLines lines agent
            with _ -> 0
        else 0

    let create (maxCapacity: int) : ObservationQueryOps =
        let capacity = if maxCapacity <= 0 then 25000 else maxCapacity
        let initialState = ObservationState.empty capacity

        let agent =
            MailboxProcessor.Start(fun inbox ->
                let rec loop (state: ObservationState) =
                    async {
                        let! msg = inbox.Receive()
                        match msg with
                        | Ingest evt ->
                            let nextState = ObservationState.ingest evt state
                            return! loop nextState

                        | IngestBatch (evts, reply) ->
                            let nextState = ObservationState.ingestBatch evts state
                            reply.Reply evts.Length
                            return! loop nextState

                        | Query (q, reply) ->
                            let result = ObservationState.queryEvents q state
                            reply.Reply result
                            return! loop state

                        | GetDossier (cid, reply) ->
                            let dossier = ObservationState.buildTraceDossier cid state
                            reply.Reply dossier
                            return! loop state

                        | GetTransitions reply ->
                            let transitions = ObservationState.getRecentTransitions state
                            reply.Reply transitions
                            return! loop state
                    }
                loop initialState)

        { Record = fun evt -> agent.Post(Ingest evt)
          QueryEvents = fun q -> agent.PostAndReply(fun ch -> Query(q, ch))
          GetTraceDossier = fun cid -> agent.PostAndReply(fun ch -> GetDossier(cid, ch))
          GetRecentTransitions = fun () -> agent.PostAndReply(GetTransitions)
          HydrateFromFile = fun path -> hydrateFromFile path agent
          HydrateFromLines = fun lines -> hydrateFromLines lines agent }

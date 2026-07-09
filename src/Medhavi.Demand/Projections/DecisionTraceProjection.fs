module Medhavi.Demand.Projections.DecisionTraceProjection

open Medhavi.Infrastructure.Projections
open Medhavi.SharedKernel.Contracts
open Medhavi.SharedKernel.Contracts.DecisionTrace
open Medhavi.Common.Serialization

type TraceProjectionState = Map<string, DecisionTrace list>

let evolveTraceProjection (state: TraceProjectionState) (env: Envelope) =
    match Envelope.tryGetMetadata "decisionTrace" env with
    | None -> state
    | Some json ->
        match deserialize<DecisionTrace> json with
        | Error _ -> state
        | Ok trace ->
            let key = Envelope.tryGetMetadata "aggregateId" env
                      |> Option.defaultValue ""
                      |> fun id ->
                          let aggType = Envelope.tryGetMetadata "aggregateType" env |> Option.defaultValue ""
                          $"{aggType}-{id}"
            state |> Map.change key (fun old -> Some (trace :: (old |> Option.defaultValue [])))

type TraceAgent = ProjectionAgent<TraceProjectionState, Envelope>

let createTraceAgent () = ProjectionAgent(evolveTraceProjection, Map.empty, "DecisionTraceReadModel")

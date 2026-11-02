/// CA-P-006 Establish Commitments — Promise Explanation Projections
module Medhavi.Promise.EstablishCommitments.PromiseExplanation.Projections

open Medhavi.SemanticModel
open Medhavi.Contracts.Promise
open Medhavi.Promise
open Model

let mapLineageNodeToDto (n: LineageNode) : LineageNodeDto =
    { StepIndex = n.StepIndex
      StageName = n.StageName
      EntityReference = n.EntityReference
      ActionTaken = n.ActionTaken
      EvidenceSummary = n.EvidenceSummary }

let mapToDto (e: PromiseExplanation) : PromiseExplanationDto =
    { ExplanationId = PromiseExplanationId.value e.ExplanationId
      CommitmentId = OrderCommitmentId.value e.CommitmentId
      OrderLineId = OrderLineId.value e.OrderLineId
      SummaryRationale = e.SummaryRationale
      EvaluatedRulesCount = e.EvaluatedRulesCount
      BindingConstraints = e.BindingConstraints
      ConsideredAlternativesCount = e.ConsideredAlternativesCount
      CausalLineage = e.CausalLineage |> List.map mapLineageNodeToDto
      LineageDigestHash = e.LineageDigestHash
      GeneratedAt = Timestamp.value e.GeneratedAt }

type State = Map<PromiseExplanationId, PromiseExplanationDto>

let initial: State = Map.empty

let apply (state: State) (event: PromiseExplanationEvent) : State =
    match event with
    | PromiseExplanationGenerated explanation ->
        state |> Map.add explanation.ExplanationId (mapToDto explanation)

let seedFromAggregates (aggregates: PromiseExplanation list) : State =
    aggregates
    |> List.fold (fun state e -> Map.add e.ExplanationId (mapToDto e) state) initial


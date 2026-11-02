/// CA-P-006 Establish Commitments — Promise Explanation Model
/// Traces to: SE-P-090, CR-P-033, CR-P-034
module Medhavi.Promise.EstablishCommitments.PromiseExplanation.Model

open System
open Medhavi.Common
open Medhavi.Foundation.Contracts
open Medhavi.SemanticModel
open Medhavi.Promise
open Medhavi.Promise.ArsIdentifiers

type LineageNode =
    { StepIndex: int
      StageName: string
      EntityReference: string
      ActionTaken: string
      EvidenceSummary: string }

type PromiseExplanation =
    { ExplanationId: PromiseExplanationId
      CommitmentId: OrderCommitmentId
      OrderLineId: OrderLineId
      SummaryRationale: string
      EvaluatedRulesCount: int
      BindingConstraints: string list
      ConsideredAlternativesCount: int
      CausalLineage: LineageNode list
      LineageDigestHash: string
      GeneratedAt: Timestamp }

type GeneratePromiseExplanationCmd =
    { ExplanationId: PromiseExplanationId
      CommitmentId: OrderCommitmentId
      OrderLineId: OrderLineId
      GenerationTime: Timestamp }

type PromiseExplanationEvent =
    | PromiseExplanationGenerated of PromiseExplanation

let evolve: Evolve<PromiseExplanation, PromiseExplanationEvent> =
    fun (_: PromiseExplanation option) (event: PromiseExplanationEvent) ->
        match event with
        | PromiseExplanationGenerated explanation -> Some explanation


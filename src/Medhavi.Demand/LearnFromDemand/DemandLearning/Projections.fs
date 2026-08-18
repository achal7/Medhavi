/// SE-D-011 — Demand Learning Read Model Projections
/// Pure Functional Projection Fold (Layer E: Catamorphism)
module Medhavi.Demand.LearnFromDemand.DemandLearning.Projections

open Medhavi.Contracts.Demand
open Medhavi.Demand
open Medhavi.SemanticModel
open Model

let mapEvidenceRefToDto (e: EvidenceReference) : EvidenceReferenceDto =
    { ArtifactType = e.ArtifactType
      ArtifactId = e.ArtifactId
      PeriodStart = Timestamp.value e.PeriodStart
      PeriodEnd = Timestamp.value e.PeriodEnd
      SummaryStatistics = e.SummaryStatistics }

let mapOpportunityToDto (o: ImprovementOpportunity) : ImprovementOpportunityDto =
    { OpportunityId = o.OpportunityId
      TargetCapability = o.TargetCapability
      TargetPolicyId = o.TargetPolicyId
      ProposedParameterChange = o.ProposedParameterChange
      ExpectedBenefit = o.ExpectedBenefit
      InterventionConfidence = o.InterventionConfidence }

let mapToDto (l: DemandLearning) : DemandLearningDto =
    { LearningId = DemandLearningId.value l.Id
      Scope = PlanningScopeId.value l.Scope
      LearningType = l.LearningType.AsString
      LearningStatement = l.LearningStatement
      PatternConfidence = l.PatternConfidence
      InterventionConfidence = l.InterventionConfidence
      SupportingEvidence = l.SupportingEvidence |> List.map mapEvidenceRefToDto
      ImprovementOpportunities = l.ImprovementOpportunities |> List.map mapOpportunityToDto
      PolicyVersion = l.PolicyVersion
      Timestamp = Timestamp.value l.CreatedAt }

/// Projection state: Map of DemandLearningId to DemandLearningDto
type State = Map<DemandLearningId, DemandLearningDto>

let initial: State = Map.empty

/// Pure projection fold (Layer E: Catamorphism)
let apply (state: State) (event: DemandLearningEvent) : State =
    match event with
    | DemandLearningEstablished learning ->
        let dto = mapToDto learning
        Map.add learning.Id dto state

/// Seed projection from existing aggregates
let seedFromAggregates (aggregates: DemandLearning list) : State =
    aggregates
    |> List.fold
        (fun state agg ->
            let dto = mapToDto agg
            Map.add agg.Id dto state)
        initial

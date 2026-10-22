// Traceability: SE-S-130 Supply Learning Read Model Projections
module Medhavi.Supply.LearnFromSupply.SupplyLearning.Projections

open System
open Medhavi.SemanticModel
open Medhavi.Contracts.Supply
open Medhavi.Supply
open Model

let private windowStart (w: TemporalWindow) : DateTimeOffset =
    w.Earliest |> Option.defaultValue w.Latest |> Timestamp.value

let private windowEnd (w: TemporalWindow) : DateTimeOffset = Timestamp.value w.Latest

let mapEvidenceRefToDto (e: HistoricalEvidenceReference) : HistoricalEvidenceReferenceDto =
    {
        CapabilityId = e.CapabilityId
        ArtifactType = e.ArtifactType
        ArtifactId = e.ArtifactId
        PeriodId = EvaluationPeriodId.value e.PeriodId
        PeriodStart = windowStart e.PeriodWindow
        PeriodEnd = windowEnd e.PeriodWindow
        SummaryMetrics = e.SummaryMetrics
    }

let mapOpportunityToDto (o: ImprovementOpportunity) : ImprovementOpportunityDto =
    {
        OpportunityId = o.OpportunityId
        TargetCapability = o.TargetCapability
        TargetPolicyId = o.TargetPolicyId
        TargetParameter = o.TargetParameter
        CurrentBaselineValue = o.CurrentBaselineValue
        RecommendedValue = o.RecommendedValue
        ExpectedBenefit = o.ExpectedBenefit
        InterventionConfidence = o.InterventionConfidence.AsString
        AnalogueSuccessRate = o.AnalogueSuccessRate
        OperationalFeasibility = o.OperationalFeasibility
    }

let mapToDto (l: SupplyLearning) : SupplyLearningDto =
    {
        LearningId = SupplyLearningId.value l.Id
        SubjectType = l.SubjectType
        SubjectId = l.SubjectId
        LearningType = l.LearningType.AsString
        Version = l.Version
        LifecycleState = l.LifecycleState.AsString
        PatternDescription = l.PatternDescription
        RecurrenceCount = l.RecurrenceCount
        RecurrenceFrequency = l.RecurrenceFrequency
        StatisticalSignificance = l.StatisticalSignificance
        Scope = l.Scope.AsString
        TimeHorizonStart = windowStart l.TimeHorizon
        TimeHorizonEnd = windowEnd l.TimeHorizon
        LearningStatement = l.LearningStatement
        PatternConfidence = l.PatternConfidence.AsString
        PatternConfidenceScore = l.PatternConfidenceScore
        ImprovementOpportunities = l.ImprovementOpportunities |> List.map mapOpportunityToDto
        Outcome = l.Outcome.AsString
        OutcomeTimestamp = l.OutcomeTimestamp |> Option.map Timestamp.value
        OutcomeAuthority = l.OutcomeAuthority
        OutcomeRationale = l.OutcomeRationale
        SupportingEvidence = l.SupportingEvidence |> List.map mapEvidenceRefToDto
        SourceEvidenceVersionSet = l.SourceEvidenceVersionSet
        PolicyVersion = l.PolicyVersion
        GenerationTimestamp = Timestamp.value l.GenerationTimestamp
        SupersededLearningId = l.SupersededLearningId |> Option.map SupplyLearningId.value
    }

let mapCandidateToDto (c: Algorithms.CandidateSupplyLearning) : CandidateSupplyLearningDto =
    {
        PatternReference = c.PatternReference
        SubjectType = c.SubjectType
        SubjectId = c.SubjectId
        LearningType = c.LearningType.AsString
        LearningScope = c.Scope.AsString
        TimeHorizonStart = windowStart c.TimeHorizon
        TimeHorizonEnd = windowEnd c.TimeHorizon
        LearningStatement = c.LearningStatement
        PatternConfidence = c.PatternConfidence.AsString
        PatternConfidenceScore = c.PatternConfidenceScore
        RecurrenceCount = c.RecurrenceCount
        RecurrenceFrequency = c.RecurrenceFrequency
        StatisticalSignificance = c.StatisticalSignificance
        SupportingEvidence = c.SupportingEvidence |> List.map mapEvidenceRefToDto
        ImprovementOpportunities = c.ImprovementOpportunities |> List.map mapOpportunityToDto
        DerivedTimestamp = Timestamp.value c.DerivedTimestamp
    }

/// Projection read model state
type State = Map<SupplyLearningId, SupplyLearningDto>

let initial: State = Map.empty

/// Pure projection fold (Layer E: Catamorphism)
let apply (state: State) (event: SupplyLearningEvent) : State =
    match event with
    | SupplyLearningRecorded learning ->
        let dto = mapToDto learning
        Map.add learning.Id dto state

    | SupplyLearningOutcomeRecorded payload ->
        match Map.tryFind payload.LearningId state with
        | Some existing ->
            let updated =
                { existing with
                    LifecycleState = LearningLifecycleState.OutcomeRecorded.AsString
                    Outcome = payload.Outcome.AsString
                    OutcomeTimestamp = Some(Timestamp.value payload.Timestamp)
                    OutcomeAuthority = Some payload.Authority
                    OutcomeRationale = payload.Rationale
                }

            Map.add payload.LearningId updated state
        | None -> state

    | SupplyLearningSuperseded payload ->
        match Map.tryFind payload.LearningId state with
        | Some existing ->
            let updated =
                { existing with
                    LifecycleState = LearningLifecycleState.Superseded.AsString
                }

            Map.add payload.LearningId updated state
        | None -> state

/// Seed projection read model from repository
let seedFromAggregates (aggregates: SupplyLearning list) : State =
    aggregates
    |> List.fold
        (fun state agg ->
            let dto = mapToDto agg
            Map.add agg.Id dto state)
        initial

/// SE-D-018 — Demand Intervention Impact Aggregate Model
/// Traces to: Demand Intelligence Specification (SE-D-018, AB-D-018, AB-D-019, Chapter 4.3.1)
module Medhavi.Demand.ModelDemandInterventions.DemandInterventionImpact.Model

open Medhavi.SemanticModel
open Medhavi.Demand

/// Type of commercial intervention (SE-C-039)
type InterventionType =
    | Promotion
    | PriceChange
    | MarketingEvent
    | NewProductIntroduction
    | ChannelShift

    member this.AsString =
        match this with
        | Promotion -> "Promotion"
        | PriceChange -> "PriceChange"
        | MarketingEvent -> "MarketingEvent"
        | NewProductIntroduction -> "NewProductIntroduction"
        | ChannelShift -> "ChannelShift"

    static member FromString(s: string) : InterventionType =
        match s with
        | "Promotion" -> Promotion
        | "PriceChange" -> PriceChange
        | "MarketingEvent" -> MarketingEvent
        | "NewProductIntroduction" -> NewProductIntroduction
        | "ChannelShift" -> ChannelShift
        | _ -> Promotion

/// Lifecycle state for Demand Intervention Impact (ARS §4.3.1)
type InterventionLifecycleState =
    | Draft
    | Published
    | Superseded

    member this.AsString =
        match this with
        | Draft -> "Draft"
        | Published -> "Published"
        | Superseded -> "Superseded"

    static member FromString(s: string) : InterventionLifecycleState =
        match s with
        | "Draft" -> Draft
        | "Published" -> Published
        | "Superseded" -> Superseded
        | _ -> Draft

/// Governed modeling approach used to compute lift (PO-D-050)
type ModelingApproach =
    | HistoricalElasticity
    | AnalogBased
    | ExpertJudgment

    member this.AsString =
        match this with
        | HistoricalElasticity -> "HistoricalElasticity"
        | AnalogBased -> "AnalogBased"
        | ExpertJudgment -> "ExpertJudgment"

    static member FromString(s: string) : ModelingApproach =
        match s with
        | "HistoricalElasticity" -> HistoricalElasticity
        | "AnalogBased" -> AnalogBased
        | "ExpertJudgment" -> ExpertJudgment
        | _ -> ExpertJudgment

/// Temporal interval during which the intervention applies
type TemporalWindow =
    { Start: Timestamp
      End: Timestamp }

/// SE-D-018 — Demand Intervention Impact Aggregate Root
/// Identity: Intervention Impact Identifier (immutable)
type DemandInterventionImpact =
    { ImpactId: DemandInterventionImpactId
      InterventionReference: ScenarioAdjustmentId
      Item: ItemId
      Location: LocationId
      AssessedDemandLift: Quantity
      LiftConfidence: decimal
      TemporalValidity: TemporalWindow
      ModelProvenance: ModelingApproach
      LifecycleState: InterventionLifecycleState
      Version: int
      CreatedAt: Timestamp
      PublishedAt: Timestamp option }

    member this.AssignmentId = DemandInterventionImpactId.value this.ImpactId

/// AB-D-018 Command: Assess Demand Intervention Impact (creates Draft)
type AssessInterventionImpactCmd =
    { ImpactId: DemandInterventionImpactId
      InterventionReference: ScenarioAdjustmentId
      Item: ItemId
      Location: LocationId
      InterventionType: InterventionType
      InterventionMagnitude: decimal
      TemporalValidity: TemporalWindow
      HistoricalPairs: (decimal * decimal) list
      BaselineDemand: decimal option
      Timestamp: Timestamp }

/// AB-D-019 Command: Publish Demand Intervention Impact (transitions Draft to Published)
type PublishInterventionImpactCmd =
    { ImpactId: DemandInterventionImpactId
      Timestamp: Timestamp }

/// Enterprise Events emitted by Demand Intervention Impact aggregate
type DemandInterventionImpactEvent =
    | InterventionImpactAssessed of DemandInterventionImpact
    | InterventionImpactPublished of Impact: DemandInterventionImpact * PreviousImpactId: DemandInterventionImpactId option

/// Pure evolution (Layer E: Catamorphism)
let evolve: Medhavi.Foundation.Contracts.Evolve<DemandInterventionImpact, DemandInterventionImpactEvent> =
    fun (_: DemandInterventionImpact option) (event: DemandInterventionImpactEvent) ->
        match event with
        | InterventionImpactAssessed impact -> Some impact
        | InterventionImpactPublished(impact, _) -> Some impact

/// Replay event sequence to rehydrate aggregate state
let replay (events: DemandInterventionImpactEvent seq) : DemandInterventionImpact option =
    Seq.fold evolve None events

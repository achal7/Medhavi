/// SE-D-003 Forecast Publication Aggregate Model
/// Traces to: SE-D-003, SE-D-013, SE-D-015, SE-D-016, SE-D-017 (Specification Chapter 4.3.1)
module Medhavi.Demand.ForecastDemand.ForecastPublication.Model

open System
open Medhavi.SemanticModel
open Medhavi.Demand

/// A single time bucket for forecasts
type ForecastBucket =
    { Start: Timestamp
      End: Timestamp }

/// Prediction interval (80%, 90%, 95% confidence)
type PredictionInterval =
    { Lower: Quantity
      Upper: Quantity
      ConfidenceLevel: decimal }

/// SE-D-015: Forecast Line for one item-location-bucket
type ForecastLine =
    { LineId: string
      Item: ItemId
      Location: LocationId
      Bucket: ForecastBucket
      Mean: Quantity
      Interval: PredictionInterval option
      ConfidenceScore: decimal
      ModelProvenance: string
      IsOverride: bool
      OriginalMean: Quantity option
      UnforecastableFlag: bool
      UnforecastableReason: string option
      FallbackMethod: string option }

/// Sign-off status for declared forecast assumptions
type AssumptionSignoffStatus =
    | Pending
    | Approved
    | Rejected

/// SE-D-016: Declared forecast assumption
type ForecastAssumption =
    { AssumptionId: string
      Category: string
      Statement: string
      SignoffStatus: AssumptionSignoffStatus
      SignoffAuthority: string option
      SignoffTimestamp: Timestamp option }

/// SE-D-017: Planner override record preserving original system forecast
type ForecastOverride =
    { OverrideId: string
      Item: ItemId
      Location: LocationId
      BucketStart: Timestamp
      OriginalValue: Quantity
      OverrideValue: Quantity
      PlannerId: string
      Justification: string
      Timestamp: Timestamp }

/// Lifecycle states of a Forecast Publication Version
type ForecastPublicationState =
    | Initialized
    | Generating
    | Generated
    | Overridden
    | Ready
    | Published
    | Superseded

/// SE-D-013: Forecast Publication Version
type ForecastPublicationVersion =
    { VersionNumber: int
      Lines: ForecastLine list
      Assumptions: ForecastAssumption list
      Overrides: ForecastOverride list
      ConfidenceIndex: decimal
      CompletenessScore: decimal
      ChampionModelId: string
      GenerationContextId: string
      CycleInitiationTime: Timestamp
      CycleInitiationReason: string
      GenerationStatus: ForecastPublicationState
      PublicationTime: Timestamp option }

/// SE-D-003: Forecast Publication Aggregate Root
type ForecastPublication =
    { PublicationId: ForecastPublicationId
      PlanningScope: PlanningScopeId
      HorizonStart: Timestamp
      HorizonEnd: Timestamp
      Versions: ForecastPublicationVersion list
      CurrentState: ForecastPublicationState }

// ---------- Commands ----------

/// AB-D-005: Command to initiate a new forecast cycle
type InitiateForecastCycleCmd =
    { PublicationId: ForecastPublicationId
      PlanningScope: PlanningScopeId
      HorizonStart: Timestamp
      HorizonEnd: Timestamp
      InitiationReason: string
      InitiationTime: Timestamp }

/// AB-D-006: Command to select champion forecasting model
type SelectChampionModelCmd =
    { PublicationId: ForecastPublicationId
      ChampionModelId: string }

/// Historical demand data point used as statistical forecasting input
type HistoricalDemandDataPoint =
    { Item: ItemId
      Location: LocationId
      Quantity: Quantity
      BusinessTime: Timestamp }

/// AB-D-007: Command to produce statistical forecast projection
type ProduceForecastProjectionCmd =
    { PublicationId: ForecastPublicationId
      HistoricalData: Map<string, HistoricalDemandDataPoint list>
      Buckets: ForecastBucket list
      ChampionModelId: string }

/// AB-D-008: Command to apply planner override on a forecast line
type ApplyPlannerOverrideCmd =
    { PublicationId: ForecastPublicationId
      Item: ItemId
      Location: LocationId
      BucketStart: Timestamp
      NewValue: Quantity
      Justification: string
      PlannerId: string
      OverrideTime: Timestamp }

/// AB-D-009: Command to publish a draft forecast publication
type PublishForecastPublicationCmd =
    { PublicationId: ForecastPublicationId
      PublicationTime: Timestamp }

// ---------- Enterprise Events ----------

type ForecastPublicationEvent =
    | ForecastCycleEstablished of ForecastPublication
    | ChampionModelSelected of Publication: ForecastPublication * ChampionModelId: string
    | ForecastProjectionProduced of ForecastPublication
    | ForecastOverrideRecorded of Publication: ForecastPublication * Override: ForecastOverride
    | ForecastPublicationPublished of Publication: ForecastPublication * VersionNumber: int * PublicationTime: Timestamp

/// Pure evolution (Layer E: Catamorphism)
let evolve: Medhavi.Foundation.Contracts.Evolve<ForecastPublication, ForecastPublicationEvent> =
    fun (state: ForecastPublication option) (event: ForecastPublicationEvent) ->
        match event with
        | ForecastCycleEstablished pub -> Some pub
        | ChampionModelSelected(pub, _) -> Some pub
        | ForecastProjectionProduced pub -> Some pub
        | ForecastOverrideRecorded(pub, _) -> Some pub
        | ForecastPublicationPublished(pub, _, _) -> Some pub

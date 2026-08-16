namespace Medhavi.SemanticModel

open System

/// SE-C-023 Quantity
/// Represents a measurable amount. Enforces non-negative values for physical quantities.
[<Struct>]
type Quantity = private Quantity of decimal

module Quantity =
    let create (value: decimal) : Result<Quantity, string> =
        if value < 0m then Error "Quantity cannot be negative." else Ok(Quantity value)

    let ofDecimal (value: decimal) = Quantity value // Use only when mathematically guaranteed non-negative
    let value (Quantity q) = q

    // Monoid Laws (Layer B / Layer E support)
    let zero = Quantity 0m
    let add (Quantity a) (Quantity b) = Quantity(a + b)

    let subtract (Quantity a) (Quantity b) =
        let result = a - b
        if result < 0m then Quantity 0m else Quantity result

/// SE-C-024 Duration
/// Represents a span of time.
[<Struct>]
type Duration = private Duration of TimeSpan

module Duration =
    let create (span: TimeSpan) : Result<Duration, string> =
        if span < TimeSpan.Zero then
            Error "Duration cannot be negative."
        else
            Ok(Duration span)

    let fromMinutes (mins: float) = Duration(TimeSpan.FromMinutes mins)
    let fromHours (hours: float) = Duration(TimeSpan.FromHours hours)
    let fromDays (days: float) = Duration(TimeSpan.FromDays days)
    let value (Duration d) = d

    // Monoid Laws
    let zero = Duration TimeSpan.Zero
    let add (Duration a) (Duration b) = Duration(a.Add(b))

/// SE-C-025 Money
[<Struct>]
type Money =
    { Amount: decimal
      CurrencyCode: string }

module Money =
    let create amount currency =
        { Amount = amount
          CurrencyCode = currency }

    let zero currency = { Amount = 0m; CurrencyCode = currency }

    let add m1 m2 =
        if m1.CurrencyCode <> m2.CurrencyCode then
            failwith "Cannot add different currencies"

        { m1 with
            Amount = m1.Amount + m2.Amount }

/// SE-C-028 Temporal Window
/// Defines a bounded period of availability or validity.
type TemporalWindow =
    { Earliest: Timestamp option
      Latest: Timestamp }

module TemporalWindow =
    let isValid (window: TemporalWindow) =
        match window.Earliest with
        | Some earliest -> Timestamp.isBefore earliest window.Latest || Timestamp.isEqual earliest window.Latest
        | None -> true

/// SE-C-029 Need Window
/// Defines the acceptable and preferred timeframes for fulfilling a demand.
type NeedWindow =
    { EarliestAcceptable: Timestamp option
      Preferred: Timestamp option
      LatestAcceptable: Timestamp }

/// SE-C-038 Scope Boundary Rule
/// Defines inclusion/exclusion criteria for a Planning Scope.
type ScopeBoundaryRule =
    { RuleIdentifier: string
      TargetSemanticType: string
      InclusionIndicator: bool
      TargetInstanceIdentifiers: string list
      TargetCategoryIdentifiers: string list }

/// SE-C-027 Planning Horizon
/// A bounded planning time interval.
type PlanningHorizon = { Start: Timestamp; End: Timestamp }

module PlanningHorizon =
    let create (start: Timestamp) (endTimestamp: Timestamp) : Result<PlanningHorizon, SemanticValidationError> =
        if Timestamp.isAfter start endTimestamp then
            Error(InvalidWindow "PlanningHorizon.Start must not be after PlanningHorizon.End.")
        else
            Ok { Start = start; End = endTimestamp }

    let start (horizon: PlanningHorizon) = horizon.Start
    let endTimestamp (horizon: PlanningHorizon) = horizon.End

    let duration (horizon: PlanningHorizon) : System.TimeSpan = Timestamp.diff horizon.End horizon.Start

/// SE-C-026 Capacity
/// Represents available capacity over a planning horizon.
type Capacity =
    { CapacityMeasure: string
      AvailableQuantity: Quantity
      Period: PlanningHorizon }

module Capacity =
    let create
        (capacityMeasure: string)
        (availableQuantity: Quantity)
        (period: PlanningHorizon)
        : Result<Capacity, SemanticValidationError> =

        if System.String.IsNullOrWhiteSpace capacityMeasure then
            Error(EmptyIdentifier "CapacityMeasure")
        else
            Ok
                { CapacityMeasure = capacityMeasure
                  AvailableQuantity = availableQuantity
                  Period = period }

    let measure (capacity: Capacity) = capacity.CapacityMeasure
    let availableQuantity (capacity: Capacity) = capacity.AvailableQuantity
    let period (capacity: Capacity) = capacity.Period

/// SE-C-030 Risk Assessment
/// A point-in-time assessment of likelihood and impact.
type RiskAssessment =
    { Likelihood: VocabularyEntryId
      Impact: VocabularyEntryId
      AssessmentTime: Timestamp
      Rationale: string option }

module RiskAssessment =
    let create
        (likelihood: VocabularyEntryId)
        (impact: VocabularyEntryId)
        (assessmentTime: Timestamp)
        (rationale: string option)
        : RiskAssessment =

        { Likelihood = likelihood
          Impact = impact
          AssessmentTime = assessmentTime
          Rationale = rationale }

/// Scenario adjustment value.
/// This is intentionally modeled as a coproduct so scenario what-if adjustments
/// remain explicit, inspectable, and interpretable by AI planners.
type ScenarioAdjustmentValue =
    | QuantityAdjustment of Quantity
    | PercentageAdjustment of decimal
    | TextualAdjustment of string

/// SE-C-039 Scenario Adjustment
type ScenarioAdjustment =
    { AdjustmentIdentifier: string
      TargetSemanticType: string
      Operator: AdjustmentOperator
      Value: ScenarioAdjustmentValue
      EffectiveWindow: TemporalWindow option }

module ScenarioAdjustment =
    let create
        (adjustmentIdentifier: string)
        (targetSemanticType: string)
        (operator: AdjustmentOperator)
        (value: ScenarioAdjustmentValue)
        (effectiveWindow: TemporalWindow option)
        : Result<ScenarioAdjustment, SemanticValidationError> =

        if System.String.IsNullOrWhiteSpace adjustmentIdentifier then
            Error(EmptyIdentifier "AdjustmentIdentifier")
        elif System.String.IsNullOrWhiteSpace targetSemanticType then
            Error(EmptyIdentifier "TargetSemanticType")
        else
            Ok
                { AdjustmentIdentifier = adjustmentIdentifier
                  TargetSemanticType = targetSemanticType
                  Operator = operator
                  Value = value
                  EffectiveWindow = effectiveWindow }

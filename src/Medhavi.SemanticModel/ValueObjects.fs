namespace Medhavi.SemanticModel

open System

/// SE-C-023 Quantity
/// Represents a measurable amount. Enforces non-negative values for physical quantities.
[<Struct>]
type Quantity = private Quantity of decimal * UnitOfMeasureId

module Quantity =
    let create (value: decimal) (unitOfMeasure: UnitOfMeasureId) : Result<Quantity, SemanticValidationError> =
        if value < 0m then Error(NegativeQuantity "Quantity") else Ok(Quantity(value, unitOfMeasure))

    let ofDecimal (value: decimal) (unitOfMeasure: UnitOfMeasureId) = Quantity(value, unitOfMeasure)
    let value (Quantity(q, _)) = q
    let unitOfMeasure (Quantity(_, u)) = u

    let zero (unitOfMeasure: UnitOfMeasureId) = Quantity(0m, unitOfMeasure)
    let add (Quantity(a, u1)) (Quantity(b, u2)) =
        if u1 <> u2 then Error(InvariantViolation("Quantity", "Cannot add different units"))
        else Ok(Quantity(a + b, u1))

    let subtract (Quantity(a, u1)) (Quantity(b, u2)) =
        if u1 <> u2 then Error(InvariantViolation("Quantity", "Cannot subtract different units"))
        else
            let result = a - b
            if result < 0m then Ok(Quantity(0m, u1)) else Ok(Quantity(result, u1))

/// SE-C-024 Duration
/// Represents a span of time.
[<Struct>]
type Duration = private Duration of decimal * UnitOfMeasureId

module Duration =
    let create (value: decimal) (unitOfMeasure: UnitOfMeasureId) : Result<Duration, SemanticValidationError> =
        if value < 0m then Error(NegativeDuration "Duration") else Ok(Duration(value, unitOfMeasure))

    let fromMinutes (mins: float) (unitOfMeasure: UnitOfMeasureId) = Duration(decimal mins, unitOfMeasure)
    let fromHours (hours: float) (unitOfMeasure: UnitOfMeasureId) = Duration(decimal hours, unitOfMeasure)
    let fromDays (days: float) (unitOfMeasure: UnitOfMeasureId) = Duration(decimal days, unitOfMeasure)
    let value (Duration(d, _)) = d
    let unitOfMeasure (Duration(_, u)) = u

    let zero (unitOfMeasure: UnitOfMeasureId) = Duration(0m, unitOfMeasure)
    let add (Duration(a, u1)) (Duration(b, u2)) =
        if u1 <> u2 then Error(InvariantViolation("Duration", "Cannot add different units"))
        else Ok(Duration(a + b, u1))

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
            Error(InvariantViolation("Money", "Cannot add different currencies"))
        else
            Ok({ m1 with Amount = m1.Amount + m2.Amount })

/// SE-C-028 Temporal Window
type TemporalWindow =
    { Earliest: Timestamp option
      Latest: Timestamp }

module TemporalWindow =
    let isValid (window: TemporalWindow) =
        match window.Earliest with
        | Some earliest -> Timestamp.isBefore earliest window.Latest || Timestamp.isEqual earliest window.Latest
        | None -> true

/// SE-C-029 Need Window
type NeedWindow =
    { EarliestAcceptable: Timestamp option
      Preferred: Timestamp option
      LatestAcceptable: Timestamp }

/// SE-C-038 Scope Boundary Rule
type ScopeBoundaryRule =
    { RuleIdentifier: string
      TargetSemanticType: VocabularyEntryId
      InclusionIndicator: bool
      TargetInstanceIdentifiers: string list
      TargetCategoryIdentifiers: VocabularyEntryId list }

/// SE-C-027 Planning Horizon
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
type Capacity =
    { CapacityMeasure: VocabularyEntryId
      OutputQuantity: Quantity
      TimePeriod: Duration }

module Capacity =
    let create
        (capacityMeasure: VocabularyEntryId)
        (outputQuantity: Quantity)
        (timePeriod: Duration)
        : Result<Capacity, SemanticValidationError> =
        Ok
            { CapacityMeasure = capacityMeasure
              OutputQuantity = outputQuantity
              TimePeriod = timePeriod }

    let measure (capacity: Capacity) = capacity.CapacityMeasure
    let outputQuantity (capacity: Capacity) = capacity.OutputQuantity
    let timePeriod (capacity: Capacity) = capacity.TimePeriod

/// SE-C-030 Risk Assessment
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

/// SE-C-039 Scenario Adjustment
type ScenarioAdjustment =
    { AdjustmentIdentifier: string
      TargetSemanticType: VocabularyEntryId
      TargetInstanceIdentifiers: string list
      TargetCategoryIdentifiers: VocabularyEntryId list
      AdjustmentType: VocabularyEntryId
      AdjustmentQuantity: Quantity option
      AdjustmentText: string option }

module ScenarioAdjustment =
    let create
        (adjustmentIdentifier: string)
        (targetSemanticType: VocabularyEntryId)
        (targetInstanceIdentifiers: string list)
        (targetCategoryIdentifiers: VocabularyEntryId list)
        (adjustmentType: VocabularyEntryId)
        (adjustmentQuantity: Quantity option)
        (adjustmentText: string option)
        : Result<ScenarioAdjustment, SemanticValidationError> =
        if System.String.IsNullOrWhiteSpace adjustmentIdentifier then
            Error(EmptyIdentifier "AdjustmentIdentifier")
        elif targetInstanceIdentifiers.IsEmpty && targetCategoryIdentifiers.IsEmpty then
            Error(InvariantViolation("ScenarioAdjustment", "At least one Target Instance Identifier or Target Category Identifier must be present."))
        elif adjustmentQuantity.IsNone && adjustmentText.IsNone then
            Error(InvariantViolation("ScenarioAdjustment", "At least one of Adjustment Quantity or Adjustment Text must be present."))
        else
            Ok
                { AdjustmentIdentifier = adjustmentIdentifier
                  TargetSemanticType = targetSemanticType
                  TargetInstanceIdentifiers = targetInstanceIdentifiers
                  TargetCategoryIdentifiers = targetCategoryIdentifiers
                  AdjustmentType = adjustmentType
                  AdjustmentQuantity = adjustmentQuantity
                  AdjustmentText = adjustmentText }

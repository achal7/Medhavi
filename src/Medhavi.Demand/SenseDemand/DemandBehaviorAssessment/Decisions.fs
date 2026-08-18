/// Sense Demand Business Decisions
/// Traces to: DE-D-006, DE-D-007 (Specification Chapter 6)
module Medhavi.Demand.SenseDemand.DemandBehaviorAssessment.Decisions

open Medhavi.Common
open Medhavi.Foundation.Contracts
open Medhavi.Foundation.Failure
open Medhavi.SemanticModel
open Model
open Policies
open Algorithms
open Rules

/// DE-D-006: Decision Alternatives for Signal Evaluation
type StateChangeAlternative =
    | NoChange
    | TransitionToElevated
    | TransitionToDepressed
    | TransitionToCritical
    | TransitionToNormal

/// DE-D-006 Decision Result payload
type StateChangeDecision =
    { SelectedAlternative: StateChangeAlternative
      DeterminedState: DemandBehaviorState
      Deviation: DeviationAssessment
      Determination: StateDetermination }

/// DE-D-006 — Evaluate Demand Signal for State Change
let evaluateSignalStateChange
    (rules: Rule<SignalEvaluationInput> list)
    (input: SignalEvaluationInput)
    (currentState: DemandBehaviorState)
    (timestamp: Timestamp)
    : Result<DecisionOutcome<StateChangeDecision>, DomainError> =
    result {
        // 1. Evaluate all business rules
        let! evaluations = Rule.evaluateAll rules input

        // 2. Perform algorithmic assessment of deviation and state
        let deviation =
            assessDeviation
                (Quantity.value input.SignalQuantity)
                input.BaselineMean
                input.BaselineStdDev
                input.CorroborationCount
                input.Policy
                input.IsHighPriority
                timestamp

        let determination = determineState currentState deviation input.Policy

        // 3. Map determined state to decision alternative
        let alternative =
            if not determination.StateTransitionOccurred then
                NoChange
            else
                match determination.DeterminedState with
                | Elevated -> TransitionToElevated
                | Depressed -> TransitionToDepressed
                | DemandBehaviorState.Critical -> TransitionToCritical
                | Normal -> TransitionToNormal

        let decision =
            { SelectedAlternative = alternative
              DeterminedState = determination.DeterminedState
              Deviation = deviation
              Determination = determination }

        return
            { Outcome = decision
              Evaluations = evaluations }
    }

/// DE-D-007: Decision Alternatives for Forecast Refresh
type ForecastRefreshAlternative =
    | TriggerRefresh
    | DeferToNextScheduledCycle

/// DE-D-007 Decision Result payload
type ForecastRefreshDecision =
    { SelectedAlternative: ForecastRefreshAlternative
      Rationale: string }

/// DE-D-007 — Trigger Forecast Refresh on Critical State
let evaluateForecastRefresh
    (rules: Rule<RefreshEvaluationInput> list)
    (input: RefreshEvaluationInput)
    : Result<DecisionOutcome<ForecastRefreshDecision>, DomainError> =
    result {
        let! evaluations = Rule.evaluateAll rules input
        let passed = evaluations |> List.forall(fun e -> e.Passed)

        let alternative = if passed then TriggerRefresh else DeferToNextScheduledCycle

        let rationale =
            if passed then
                sprintf
                    "Forecast age (%d hrs) and expected WAPE improvement (%.2f%%) warrant out-of-cycle forecast refresh per PO-D-032."
                    input.ForecastAgeHours
                    (input.ExpectedAccuracyImprovementWape * 100m)
            else
                "Criteria for out-of-cycle forecast refresh not met; deferring to next scheduled cycle."

        let decision =
            { SelectedAlternative = alternative
              Rationale = rationale }

        return
            { Outcome = decision
              Evaluations = evaluations }
    }

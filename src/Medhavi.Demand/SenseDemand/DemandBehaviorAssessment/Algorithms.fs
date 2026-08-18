/// Sense Demand Business Algorithms
/// Traces to: BA-D-014, BA-D-003, BA-D-004 (Specification Chapter 10)
module Medhavi.Demand.SenseDemand.DemandBehaviorAssessment.Algorithms

open System
open Medhavi.SemanticModel
open Model
open Policies

/// BA-D-014: Derive Demand Behavior Baseline parameters
type BaselineParameters =
    { ExpectedLevel: decimal
      StandardDeviation: decimal
      Confidence: AssessmentConfidence }

module BaselineDerivation =
    /// BA-D-014 — Derive Demand Behavior Baseline from historical observations
    let deriveBaseline (quantities: decimal list) : BaselineParameters =
        match quantities with
        | [] ->
            { ExpectedLevel = 0m
              StandardDeviation = 1m
              Confidence = Low }
        | [ single ] ->
            { ExpectedLevel = single
              StandardDeviation = 1m
              Confidence = Low }
        | list ->
            let count = decimal list.Length
            let mean = (List.sum list) / count

            let variance =
                list
                |> List.map(fun q ->
                    let diff = float(q - mean)
                    diff * diff)
                |> List.sum
                |> fun sumSquares -> decimal(sumSquares / float(list.Length - 1))

            let stdDev =
                let std = decimal(Math.Sqrt(float variance))
                if std > 0m then std else 1m

            let confidence =
                if list.Length >= 30 then High
                elif list.Length >= 10 then Medium
                else Low

            { ExpectedLevel = mean
              StandardDeviation = stdDev
              Confidence = confidence }

/// Deviation classification categories per PO-D-031
type DeviationClassification =
    | Noise
    | Significant
    | Critical

/// BA-D-003: Deviation Assessment Result
type DeviationAssessment =
    { MagnitudeSigma: decimal
      Direction: DeviationDirection
      Classification: DeviationClassification
      CorroborationCount: int
      AssessmentConfidence: AssessmentConfidence
      ReasonCodes: string list
      Timestamp: Timestamp }

/// BA-D-003 — Assess Demand Signal Deviation
let assessDeviation
    (signalQuantity: decimal)
    (baselineMean: decimal)
    (baselineStdDev: decimal)
    (corroborationCount: int)
    (policy: DemandSensingPolicy)
    (isHighPriority: bool)
    (timestamp: Timestamp)
    : DeviationAssessment =

    let stdDev = if baselineStdDev > 0m then baselineStdDev else 1m
    let rawDev = (signalQuantity - baselineMean) / stdDev
    let direction = if rawDev >= 0m then Increase else Decrease
    let absDev = abs rawDev

    let significantThreshold =
        if isHighPriority then
            policy.HighPrioritySignificantThreshold
        else
            policy.SignificantThreshold

    let classification =
        if absDev < policy.NoiseThreshold then Noise
        elif absDev < significantThreshold then Noise
        elif absDev < policy.CriticalThreshold then Significant
        else Critical

    let confidence =
        match classification with
        | Noise -> Low
        | Significant -> if corroborationCount >= 2 then High else Medium
        | Critical -> if corroborationCount >= policy.CorroborationMinimum then High else Medium

    let reasonCodes =
        [ match classification with
          | Critical -> "CriticalDeviation"
          | Significant -> "SignificantDeviation"
          | Noise -> "NoiseLevel"

          if corroborationCount >= 2 then
              "MultiSourceCorroboration"
          else
              "SingleSourceEvidence"

          if isHighPriority then
              "HighPriorityItem" ]

    { MagnitudeSigma = rawDev
      Direction = direction
      Classification = classification
      CorroborationCount = corroborationCount
      AssessmentConfidence = confidence
      ReasonCodes = reasonCodes
      Timestamp = timestamp }

/// BA-D-004: State Determination Result
type StateDetermination =
    { DeterminedState: DemandBehaviorState
      StateTransitionOccurred: bool
      Rationale: string
      Confidence: AssessmentConfidence }

/// BA-D-004 — Determine Demand Behavior State
let determineState
    (currentState: DemandBehaviorState)
    (deviation: DeviationAssessment)
    (policy: DemandSensingPolicy)
    : StateDetermination =

    let absDev = abs deviation.MagnitudeSigma

    // Rule 1 — Noise Suppression: if Noise, retain current state
    if deviation.Classification = Noise then
        { DeterminedState = currentState
          StateTransitionOccurred = false
          Rationale =
            sprintf
                "Deviation %.2fσ is below meaningful threshold (Noise threshold: %.2fσ); state remains %A."
                absDev
                policy.NoiseThreshold
                currentState
          Confidence = deviation.AssessmentConfidence }

    // Rule 2 — Critical Transition: Critical AND corroboration meets minimum
    elif deviation.Classification = Critical && deviation.CorroborationCount >= policy.CorroborationMinimum then
        let newState = DemandBehaviorState.Critical

        { DeterminedState = newState
          StateTransitionOccurred = (newState <> currentState)
          Rationale =
            sprintf
                "Deviation %.2fσ exceeds Critical threshold (%.2fσ) with %d corroborating sources (minimum %d). State determined as Critical."
                absDev
                policy.CriticalThreshold
                deviation.CorroborationCount
                policy.CorroborationMinimum
          Confidence = High }

    // Exceptional condition / Rule 3 — Critical without corroboration is capped at Elevated / Depressed per BR-D-301
    elif deviation.Classification = Critical then
        let newState = if deviation.Direction = Increase then Elevated else Depressed

        { DeterminedState = newState
          StateTransitionOccurred = (newState <> currentState)
          Rationale =
            sprintf
                "Deviation %.2fσ exceeds Critical threshold (%.2fσ) but lacks required corroboration (%d < %d). Capped at %A per BR-D-301."
                absDev
                policy.CriticalThreshold
                deviation.CorroborationCount
                policy.CorroborationMinimum
                newState
          Confidence = Medium }

    // Rule 3 — Significant Transition
    elif deviation.Classification = Significant then
        let newState = if deviation.Direction = Increase then Elevated else Depressed

        { DeterminedState = newState
          StateTransitionOccurred = (newState <> currentState)
          Rationale =
            sprintf
                "Significant deviation %.2fσ in direction %A exceeds Significant threshold. State determined as %A."
                absDev
                deviation.Direction
                newState
          Confidence = deviation.AssessmentConfidence }

    // Rule 4 — Return to Normal
    else
        let newState = Normal

        { DeterminedState = newState
          StateTransitionOccurred = (newState <> currentState)
          Rationale =
            sprintf "Deviation %.2fσ does not warrant elevated or depressed state. State determined as Normal." absDev
          Confidence = deviation.AssessmentConfidence }

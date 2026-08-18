/// Model Demand Interventions Algorithms
/// Traces to: BA-D-016 Model Intervention Lift (Specification Chapter 10)
module Medhavi.Demand.ModelDemandInterventions.DemandInterventionImpact.Algorithms

open System
open MathNet.Numerics
open MathNet.Numerics.LinearRegression
open MathNet.Numerics.Statistics
open Medhavi.SemanticModel
open Medhavi.Demand
open Model
open Policies

/// Result payload computed by BA-D-016
type LiftAssessmentResult =
    { AssessedLift: Quantity
      LiftConfidence: decimal
      ApproachUsed: ModelingApproach
      Rationale: string }

/// Estimates price elasticity via log-log regression: ln(Quantity) = a + e * ln(Price)
let computeLogLogElasticity (pairs: (float * float) list) : (float * float) option =
    let validPairs =
        pairs
        |> List.filter (fun (q, p) -> q > 0.0 && p > 0.0)
        |> List.map (fun (q, p) -> (log p, log q))

    if validPairs.Length < 3 then
        None
    else
        let xs = validPairs |> List.map fst |> List.toArray
        let ys = validPairs |> List.map snd |> List.toArray
        let r = Correlation.Pearson(xs, ys)
        let rSquared = if Double.IsNaN r then 0.0 else r * r
        let struct (intercept, slope) = SimpleRegression.Fit(xs, ys)
        Some(slope, rSquared)

/// BA-D-016: Model Intervention Lift
let modelInterventionLift
    (interventionType: InterventionType)
    (magnitude: decimal)
    (historicalPairs: (decimal * decimal) list)
    (baselineDemandOpt: decimal option)
    (policy: InterventionModelingGovernancePolicy)
    : LiftAssessmentResult =

    let baseline = baselineDemandOpt |> Option.defaultValue 100.0m
    let validHistoricalPairs = historicalPairs |> List.filter (fun (f, a) -> f > 0.0m && a > 0.0m)
    let pairCount = validHistoricalPairs.Length

    // Step 1: Attempt Historical Elasticity approach
    let historicalAttempt =
        if pairCount >= policy.MinHistoricalPeriodsForElasticity then
            let floatPairs = validHistoricalPairs |> List.map (fun (f, a) -> (float a, float f))
            match computeLogLogElasticity floatPairs with
            | Some(slope, r2) when not (Double.IsNaN slope) && not (Double.IsInfinity slope) ->
                let elasticity = decimal (abs slope)
                let liftQty =
                    match interventionType with
                    | PriceChange ->
                        // Price reduction of magnitude%: % lift = elasticity * (magnitude / 100)
                        let percentChange = magnitude / 100.0m
                        let additionalRatio = elasticity * percentChange
                        baseline * (max 0.0m additionalRatio)
                    | Promotion ->
                        let percentChange = magnitude / 100.0m
                        let additionalRatio = (elasticity * 0.8m + 0.3m) * percentChange
                        baseline * (max 0.0m additionalRatio)
                    | MarketingEvent ->
                        let boost = (magnitude / 100.0m) * 0.25m
                        baseline * (max 0.0m boost)
                    | NewProductIntroduction ->
                        let adoptionRatio = (magnitude / 100.0m) * 0.50m
                        baseline * (max 0.0m adoptionRatio)
                    | ChannelShift ->
                        let shiftRatio = (magnitude / 100.0m) * 0.15m
                        baseline * (max 0.0m shiftRatio)

                let confidence =
                    let sampleBonus = min 0.15m (decimal pairCount / 100.0m)
                    let fitBonus = min 0.20m (decimal r2 * 0.20m)
                    min 0.95m (0.60m + sampleBonus + fitBonus)

                let safeLift = max 0.0m liftQty
                let qty = Quantity.create safeLift |> Result.defaultValue (Quantity.create 0.0m |> Result.toOption |> Option.get)

                let rationale =
                    "Assessed lift of " + safeLift.ToString("N2") + " units using Historical Elasticity (elasticity = " + elasticity.ToString("F2") + ", R² = " + r2.ToString("F2") + ", sample size = " + (string pairCount) + " periods)."

                Some
                    { AssessedLift = qty
                      LiftConfidence = confidence
                      ApproachUsed = HistoricalElasticity
                      Rationale = rationale }
            | _ -> None
        else
            None

    // Step 2: Fallback to Analog / Expert Judgment approach
    match historicalAttempt with
    | Some res -> res
    | None ->
        // Use default elasticity and policy parameters (ExpertJudgment)
        let elasticity = abs policy.DefaultPriceElasticity
        let liftQty =
            match interventionType with
            | PriceChange ->
                let percentChange = magnitude / 100.0m
                baseline * (max 0.0m (elasticity * percentChange))
            | Promotion ->
                let percentChange = magnitude / 100.0m
                baseline * (max 0.0m ((policy.DefaultPromotionLiftMultiplier - 1.0m) * percentChange))
            | MarketingEvent ->
                baseline * (max 0.0m ((magnitude / 100.0m) * 0.20m))
            | NewProductIntroduction ->
                baseline * (max 0.0m ((magnitude / 100.0m) * 0.40m))
            | ChannelShift ->
                baseline * (max 0.0m ((magnitude / 100.0m) * 0.10m))

        let safeLift = max 0.0m liftQty
        let qty = Quantity.create safeLift |> Result.defaultValue (Quantity.create 0.0m |> Result.toOption |> Option.get)
        let confidence = if pairCount > 0 then 0.55m else 0.45m

        let rationale =
            "Assessed lift of " + safeLift.ToString("N2") + " units using Expert Judgment fallback (default elasticity = " + elasticity.ToString("F2") + ", historical periods = " + (string pairCount) + " < min required " + (string policy.MinHistoricalPeriodsForElasticity) + ")."

        { AssessedLift = qty
          LiftConfidence = confidence
          ApproachUsed = ExpertJudgment
          Rationale = rationale }

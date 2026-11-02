/// CA-P-011 Learn & Calibrate Promising Models — Algorithms
/// Architectural Lineage: ARS §3, §14; Parameter Drift Estimator
module Medhavi.Promise.LearnFromPromises.PromiseCalibrationModel.Algorithms

open System
open Model
open Policies

/// Derives calibrated parameter with exponential smoothing and step bounding
let calibrateParameter
    (policy: PromisingModelCalibrationPolicy)
    (paramName: string)
    (entityType: string)
    (entityId: string)
    (baselineValue: decimal)
    (observedMean: decimal) : CalibratedParameter =
    let alpha = policy.ExponentialSmoothingAlpha
    let rawAdjustment = alpha * (observedMean - baselineValue)
    let boundedAdjustment = PromisingModelCalibrationPolicy.boundAdjustment policy baselineValue rawAdjustment
    let recommendedValue = max 0.1m (baselineValue + boundedAdjustment)

    { ParameterName = paramName
      TargetEntityType = entityType
      EntityIdentifier = entityId
      BaselineValue = baselineValue
      EmpiricalObservedValue = observedMean
      RecommendedCalibratedValue = recommendedValue
      DampenedStepDelta = boundedAdjustment }

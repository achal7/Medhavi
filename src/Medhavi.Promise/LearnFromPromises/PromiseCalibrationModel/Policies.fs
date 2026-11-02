/// CA-P-011 Learn & Calibrate Promising Models — PO-P-016 Policies
/// Architectural Lineage: ARS §3, §14; Table A.4 PO-P-016
module Medhavi.Promise.LearnFromPromises.PromiseCalibrationModel.Policies

open Medhavi.SemanticModel

type PromisingModelCalibrationPolicy =
    { PolicyId: string
      Version: int
      MinimumObservationSampleSize: int
      ExponentialSmoothingAlpha: decimal
      MaxStepChangePercentage: decimal
      StewardSignOffThreshold: decimal }

module PromisingModelCalibrationPolicy =
    let defaultPolicy =
        { PolicyId = "PO-P-016"
          Version = 1
          MinimumObservationSampleSize = 30
          ExponentialSmoothingAlpha = 0.20m
          MaxStepChangePercentage = 0.25m
          StewardSignOffThreshold = 0.20m }

    let requiresStewardSignOff (policy: PromisingModelCalibrationPolicy) (deltaPct: decimal) : bool =
        abs deltaPct > policy.StewardSignOffThreshold

    let boundAdjustment (policy: PromisingModelCalibrationPolicy) (baseline: decimal) (rawAdjustment: decimal) : decimal =
        let maxDelta = abs (baseline * policy.MaxStepChangePercentage)
        if rawAdjustment > maxDelta then maxDelta
        elif rawAdjustment < -maxDelta then -maxDelta
        else rawAdjustment

/// PO-S-080 — Scheduling Policy Configuration & Governance
/// Traces to: Supply Intelligence Specification §8 (PO-S-080)
module Medhavi.Supply.ScheduleProduction.ProductionSchedule.Policies

open System
open Medhavi.Supply

[<RequireQualifiedAccess>]
type SequencingStrategy =
    | MinimizeChangeover
    | EarliestDueDate
    | PriorityBased

type SchedulingPolicyConfig =
    { PolicyVersion: int
      DefaultStrategy: SequencingStrategy
      OvertimeLimitRatio: decimal
      FrozenHorizonHours: decimal
      StabilityThreshold: decimal
      ConfidenceThreshold: ScheduleConfidence
      DueDateWeightAlpha: decimal
      WeightJobCountW1: decimal
      WeightQuantityW2: decimal
      WeightTimingW3: decimal
      DefaultChangeoverHours: decimal
      CampaignSchedulingEnabled: bool }

let defaultSchedulingPolicy: SchedulingPolicyConfig =
    { PolicyVersion = 1
      DefaultStrategy = SequencingStrategy.MinimizeChangeover
      OvertimeLimitRatio = 0.20M
      FrozenHorizonHours = 336.0M // 14 days
      StabilityThreshold = 70.0M
      ConfidenceThreshold = ScheduleConfidence.Medium
      DueDateWeightAlpha = 0.70M
      WeightJobCountW1 = 0.30M
      WeightQuantityW2 = 0.35M
      WeightTimingW3 = 0.35M
      DefaultChangeoverHours = 1.0M
      CampaignSchedulingEnabled = false }

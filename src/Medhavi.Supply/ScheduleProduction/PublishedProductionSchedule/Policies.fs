/// PO-S-080 — Baseline Publication Policy Configuration
/// Traces to: Supply Intelligence Specification §8 (PO-S-080)
module Medhavi.Supply.ScheduleProduction.PublishedProductionSchedule.Policies

open System

type PublicationPolicyConfig =
    { StabilityScoreThreshold: decimal
      AllowInfeasibleWithFlags: bool
      MaxInfeasibleAllowed: int
      RequireAllResourcesActive: bool }

let defaultPublicationPolicy: PublicationPolicyConfig =
    { StabilityScoreThreshold = 70.0M
      AllowInfeasibleWithFlags = false
      MaxInfeasibleAllowed = 0
      RequireAllResourcesActive = true }

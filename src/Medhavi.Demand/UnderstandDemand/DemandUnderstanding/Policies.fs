module Medhavi.Demand.UnderstandDemand.DemandUnderstanding.Policies

open System

// =============================================================================
// SE-D-002 — Demand Understanding Policies
// Traces to: PO-D-011 (Materiality), PO-D-012 (Publication Cadence)
// =============================================================================

/// PO-D-011 — Demand Understanding Materiality Policy.
type MaterialityPolicy =
    { PolicyId: string
      Version: int
      /// BR-D-205 — fraction [0..1] of mandatory interpretation dimensions that must be complete for publication.
      InterpretationCompletenessThreshold: decimal
      /// PO-D-011 — magnitude threshold (%) for Stable <-> Increasing/Declining continuity transitions.
      /// None = threshold not ratified in PO-D-011; BA-D-001 reports the condition NotApplicable.
      ContinuityMagnitudeThresholdPercent: decimal option }

/// PO-D-012 — Demand Understanding Publication Cadence Policy.
type CadencePolicy =
    { PolicyId: string
      Version: int
      /// Longest allowed gap between Published versions (governed default: 24h).
      MaxPublicationInterval: TimeSpan
      /// Staleness warning threshold (governed default: 12h).
      StalenessWarningThreshold: TimeSpan }

/// PO-D-011 — initial governed configuration.
let defaultMaterialityPolicy: MaterialityPolicy =
    { PolicyId = "PO-D-011"
      Version = 1
      InterpretationCompletenessThreshold = 1.0m
      ContinuityMagnitudeThresholdPercent = None }

/// PO-D-012 — initial governed configuration.
let defaultCadencePolicy: CadencePolicy =
    { PolicyId = "PO-D-012"
      Version = 1
      MaxPublicationInterval = TimeSpan.FromHours 24.0
      StalenessWarningThreshold = TimeSpan.FromHours 12.0 }

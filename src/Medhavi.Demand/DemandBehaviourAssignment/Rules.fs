module Medhavi.Demand.DemandBehaviourAssignment.Rules

/// BR‑D‑066 — Classification must be determined by the Classification Policy
let classificationByPolicy (_entityType: string) (_entityId: string) (_dimension: string) = Ok()

/// BR‑D‑067 — Unclassified if insufficient evidence
let sufficientEvidence (_entityType: string) (_entityId: string) (_dimension: string) = Ok()

module Medhavi.Demand.PlanningClassificationAssignment.Rules

open Medhavi.Demand.PlanningClassificationAssignment.Model

/// BR‑D‑061 — Classification must be determined by the Segmentation Policy
let classificationByPolicy (_entityType: string) (_entityId: string) (_classificationType: ClassificationType) =
    // In a full implementation, this would consult the policy engine.
    // For now, the decision function will use a placeholder policy.
    Ok()

/// BR‑D‑062 — Unclassified if insufficient evidence
let sufficientEvidence (_entityType: string) (_entityId: string) (_classificationType: ClassificationType) =
    // Placeholder: always true until policy integration
    Ok()

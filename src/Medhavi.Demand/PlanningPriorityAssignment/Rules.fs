module Medhavi.Demand.PlanningPriorityAssignment.Rules

/// BR‑D‑075 — Priority must be determined using the Prioritization Policy
let priorityByPolicy (_entityType: string) (_entityId: string) = Ok()

/// BR‑D‑076 — Unclassified if mandatory evidence is missing
let mandatoryEvidencePresent (_entityType: string) (_entityId: string) = Ok()

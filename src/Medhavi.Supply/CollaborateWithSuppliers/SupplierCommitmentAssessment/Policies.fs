/// PO-S-060 — Supplier Collaboration Policy
/// Traces to: Supply Intelligence Specification §8 (PO-S-060)
module Medhavi.Supply.CollaborateWithSuppliers.SupplierCommitmentAssessment.Policies

type SupplierCollaborationPolicy =
    { AcknowledgeThresholdHours: decimal
      CommitThresholdHours: decimal
      SlowAcknowledgeThresholdHours: decimal
      SlowCommitThresholdHours: decimal
      ReliableOtifThreshold: decimal
      UnreliableOtifThreshold: decimal
      CriticalOtifThreshold: decimal
      HighLeadTimeStdDevDaysThreshold: decimal
      ModerateLeadTimeStdDevDaysThreshold: decimal
      HighPromiseSlippageDaysThreshold: decimal
      ModeratePromiseSlippageDaysThreshold: decimal
      SingleSourceSpendThreshold: decimal
      HighDependencyIndexThreshold: decimal
      MinSurgeFlexibilityThreshold: decimal
      CriticalQualityAcceptanceThreshold: decimal
      HighQualityAcceptanceThreshold: decimal
      HighOrderRevisionsThreshold: int }

let defaultPolicy : SupplierCollaborationPolicy =
    { AcknowledgeThresholdHours = 24.0m
      CommitThresholdHours = 72.0m
      SlowAcknowledgeThresholdHours = 72.0m
      SlowCommitThresholdHours = 120.0m
      ReliableOtifThreshold = 0.95m
      UnreliableOtifThreshold = 0.85m
      CriticalOtifThreshold = 0.70m
      HighLeadTimeStdDevDaysThreshold = 5.0m
      ModerateLeadTimeStdDevDaysThreshold = 2.0m
      HighPromiseSlippageDaysThreshold = 3.0m
      ModeratePromiseSlippageDaysThreshold = 1.0m
      SingleSourceSpendThreshold = 0.50m
      HighDependencyIndexThreshold = 0.40m
      MinSurgeFlexibilityThreshold = 0.10m
      CriticalQualityAcceptanceThreshold = 0.95m
      HighQualityAcceptanceThreshold = 0.98m
      HighOrderRevisionsThreshold = 2 }

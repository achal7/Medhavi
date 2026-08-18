namespace Medhavi.SharedKernel.BusinessNotifications

open System
open Medhavi.SemanticModel

/// BN-D-022: Demand Exception Evidence Notification
/// Published by Demand Intelligence domain when exception conditions are detected
type DemandExceptionEvidenceNotification =
    { ExceptionId: ExceptionId
      ConstraintReference: string
      Classification: VocabularyEntryId
      AffectedScopeType: VocabularyEntryId
      AffectedScopeIdentifier: string
      EvidenceReference: string option
      Severity: VocabularyEntryId option
      EvidenceTime: Timestamp }

/// BN-S-022: Supply Exception Evidence Notification
/// Published by Supply Intelligence domain when exception conditions are detected
type SupplyExceptionEvidenceNotification =
    { ExceptionId: ExceptionId
      ConstraintReference: string
      Classification: VocabularyEntryId
      AffectedScopeType: VocabularyEntryId
      AffectedScopeIdentifier: string
      EvidenceReference: string option
      Severity: VocabularyEntryId option
      EvidenceTime: Timestamp }

/// BN-I-022: Inventory Exception Evidence Notification
/// Published by Inventory Intelligence domain when exception conditions are detected
type InventoryExceptionEvidenceNotification =
    { ExceptionId: ExceptionId
      ConstraintReference: string
      Classification: VocabularyEntryId
      AffectedScopeType: VocabularyEntryId
      AffectedScopeIdentifier: string
      EvidenceReference: string option
      Severity: VocabularyEntryId option
      EvidenceTime: Timestamp }

/// BN-D-010: Demand Understanding Published Notification
/// Published by Demand Intelligence domain when a new understanding version is published
type DemandUnderstandingPublishedNotification =
    { PlanningScopeId: PlanningScopeId
      Version: int
      EvidencePictureVersion: int
      PublicationTime: Timestamp
      MaterialChangeDetected: bool }

/// BN-S-010: Supply Understanding Published Notification
/// Published by Supply Intelligence domain when a new understanding version is published
type SupplyUnderstandingPublishedNotification =
    { PlanningScopeId: PlanningScopeId
      Version: int
      EvidencePictureVersion: int
      PublicationTime: Timestamp
      MaterialChangeDetected: bool }

/// BN-I-010: Inventory Snapshot Published Notification
/// Published by Inventory Intelligence domain when a new snapshot is published
type InventorySnapshotPublishedNotification =
    { PlanningScopeId: PlanningScopeId
      Version: int
      PublicationTime: Timestamp
      MaterialChangeDetected: bool }

/// EV-C-001 mirror notification
type PictureVersionComposedNotification =
    { PlanningScopeId: string
      VersionNumber: int
      CompositionTriggerTime: DateTimeOffset }

/// BN-C-001: Enterprise Picture Published Notification
type EnterprisePicturePublishedNotification =
    { PlanningScopeId: PlanningScopeId
      Version: int
      SupersededVersion: int option
      PublicationTime: Timestamp
      MaterialChangeSummary: MaterialChangeSummary
      PeriodicRefreshFlag: bool }

/// Material change summary for Enterprise Picture publication
and MaterialChangeSummary =
    { DemandChanged: bool
      SupplyChanged: bool
      InventoryChanged: bool
      ChangeDetails: Map<string, string> }

/// BN-C-002: Enterprise Exception Active Notification
type EnterpriseExceptionActiveNotification =
    { ExceptionId: ExceptionId
      ConstraintReference: string
      Classification: VocabularyEntryId
      AffectedScopeType: VocabularyEntryId
      AffectedScopeIdentifier: string
      Severity: VocabularyEntryId option
      RegistrationTime: Timestamp }

/// BN-C-003: Enterprise Exception Resolved Notification
type EnterpriseExceptionResolvedNotification =
    { ExceptionId: ExceptionId
      ResolutionTime: Timestamp
      ResolutionEvidence: string }

// === ADD to Medhavi.SharedKernel.BusinessNotifications ===

/// BN-C-004: Item Transition Recognized Notification
type ItemTransitionRecognizedNotification =
    { TransitionId: string
      SupersededItem: string
      SupersedingItem: string
      TransitionType: string
      EffectiveDate: DateTimeOffset
      EndDate: DateTimeOffset option }

/// BN-C-005: Item Transition Suspended Notification
type ItemTransitionSuspendedNotification =
    { TransitionId: string
      SupersededItem: string
      SupersedingItem: string
      SuspensionTime: DateTimeOffset }

/// BN-C-006: Item Transition Reinstated Notification
type ItemTransitionReinstatedNotification =
    { TransitionId: string
      SupersededItem: string
      SupersedingItem: string
      ReinstatementTime: DateTimeOffset }

/// BN-C-007: Item Transition Retired Notification
type ItemTransitionRetiredNotification =
    { TransitionId: string
      SupersededItem: string
      SupersedingItem: string
      RetirementTime: DateTimeOffset }

/// BN-D-015: Demand Behavior Changed Notification
/// Published when a demand signal causes a Demand Behavior State transition (FS-D-009)
type DemandBehaviorChangedNotification =
    { Item: ItemId
      Location: LocationId
      PreviousState: string
      NewState: string
      DeviationMagnitude: decimal
      Direction: string
      Confidence: string
      Timestamp: string }

/// BN-D-016: Critical Demand Behavior Notification
/// Published when a Critical demand behavior state is detected, requiring action (FS-D-009, FS-D-010)
type CriticalDemandBehaviorNotification =
    { Item: ItemId
      Location: LocationId
      DeviationMagnitude: decimal
      Direction: string
      CorroborationCount: int
      Timestamp: string
      ActionRequired: string }

/// BN-D-011: Forecast Published Notification
/// Published when a Forecast Publication is released as authoritative (FS-D-008)
type ForecastPublishedNotification =
    { PublicationId: string
      PlanningScopeId: PlanningScopeId
      VersionNumber: int
      HorizonStart: Timestamp
      HorizonEnd: Timestamp
      PublicationTime: Timestamp
      LineCount: int
      ChampionModelId: string }

/// BN-D-012: Forecast Override Applied Notification
/// Published when an authorized planner override modifies a forecast line (FS-D-007)
type ForecastOverrideAppliedNotification =
    { PublicationId: string
      ItemId: ItemId
      LocationId: LocationId
      BucketStart: Timestamp
      OriginalValue: decimal
      OverrideValue: decimal
      PlannerId: string
      Justification: string
      Timestamp: Timestamp }

/// BN-D-017: Planning Classification Changed Notification
/// Published when an entity's planning classification (ABC/XYZ) is assigned or modified (FS-D-011)
type PlanningClassificationChangedNotification =
    { AssignmentId: string
      EntityType: string
      EntityId: string
      ClassificationType: string
      PreviousClassification: string option
      NewClassification: string
      ClassificationScore: decimal
      ClassificationConfidence: string
      Rationale: string
      Timestamp: Timestamp }

/// BN-D-019: Demand Behavior Classification Changed Notification
/// Published when a SKU-Location's statistical behavior classification is assigned or updated (FS-D-012)
type DemandBehaviorClassificationChangedNotification =
    { AssignmentId: string
      Item: ItemId
      Location: LocationId
      Dimension: string
      PreviousClassification: string option
      NewClassification: string
      ClassificationConfidence: string
      Rationale: string
      Timestamp: Timestamp }

/// BN-D-020: Planning Priority Changed Notification
/// Published when a planning entity's priority score and level are assigned or modified (FS-D-013)
type PlanningPriorityChangedNotification =
    { AssignmentId: string
      EntityType: string
      EntityId: string
      PreviousPriority: string option
      NewPriority: string
      PriorityScore: decimal
      Rationale: string
      BusinessValidity: string
      Timestamp: Timestamp }

/// BN-D-021: Forecast Quality Alert Notification
/// Published when a Forecast Quality Assessment reveals Poor/Critical quality or out-of-control tracking signal (FS-D-014)
type ForecastQualityAlertNotification =
    { AssessmentId: string
      Scope: PlanningScopeId
      EvaluationPeriodStart: Timestamp
      EvaluationPeriodEnd: Timestamp
      VersionNumber: int
      QualityState: string
      Wape: decimal
      ForecastBias: decimal
      TrackingSignal: decimal option
      IsOutOfControl: bool
      AlertRationale: string
      Timestamp: Timestamp }

/// BN-D-022: Demand Exception Detection Evidence Notification
/// Published to Core Exception Management when a demand condition breach is detected (FS-D-015)
type DemandExceptionDetectionEvidenceNotification =
    { EvidenceId: string
      ExceptionType: string
      EntityType: string
      EntityId: string
      Scope: PlanningScopeId
      Severity: string
      TriggeringMetric: string
      MetricValue: decimal
      ThresholdValue: decimal
      Rationale: string
      Timestamp: Timestamp }

/// BN-D-023: Demand Exception Resolution Evidence Notification
/// Published to Core Exception Management when a previously breached demand condition returns to normal (FS-D-015)
type DemandExceptionResolutionEvidenceNotification =
    { EvidenceId: string
      ExceptionType: string
      EntityType: string
      EntityId: string
      Scope: PlanningScopeId
      ResolutionMetric: string
      MetricValue: decimal
      ThresholdValue: decimal
      Rationale: string
      Timestamp: Timestamp }

/// BN-D-024: Demand Explanation Established Notification
/// Published when an immutable Structured Reasoning Graph explanation is recorded for a demand conclusion (FS-D-016)
type DemandExplanationEstablishedNotification =
    { ExplanationId: string
      ExplainedArtifactType: string
      ExplainedArtifactId: string
      Version: int
      TemplateVersion: string
      PlannerSummary: string
      ExplainabilityScore: decimal
      Timestamp: Timestamp }

/// BN-D-025: Demand Learning Established Notification
/// Published when an immutable Demand Learning is derived from multi-period historical evidence (FS-D-017)
type DemandLearningEstablishedNotification =
    { LearningId: string
      Scope: PlanningScopeId
      LearningType: string
      LearningStatement: string
      PatternConfidence: decimal
      InterventionConfidence: decimal
      SupportingEvidenceCount: int
      PolicyVersion: string
      Timestamp: Timestamp }

/// BN-D-026: Demand Intervention Impact Published Notification
/// Published when an authoritative Demand Intervention Impact is published for a planned commercial intervention (FS-D-019)
type DemandInterventionImpactPublishedNotification =
    { ImpactId: string
      InterventionReference: string
      Item: ItemId
      Location: LocationId
      AssessedDemandLift: Quantity
      LiftConfidence: decimal
      TemporalValidityStart: Timestamp
      TemporalValidityEnd: Timestamp
      ModelProvenance: string
      Timestamp: Timestamp }









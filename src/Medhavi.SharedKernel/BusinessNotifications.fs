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

namespace Medhavi.SharedKernel.BusinessNotifications

open Medhavi.SemanticModel

/// BN-D-022: Demand Exception Evidence Notification
/// Published by Demand Intelligence domain when exception conditions are detected
type DemandExceptionEvidenceNotification =
    { ExceptionId: ExceptionId
      ConstraintReference: string
      Classification: VocabularyEntryId
      AffectedScopeType: VocabularyEntryId
      AffectedScopeIdentifier: string
      EvidenceReference: string
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
      EvidenceReference: string
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
      EvidenceReference: string
      Severity: VocabularyEntryId option
      EvidenceTime: Timestamp }

/// BN-C-020c: Exception SLA Warning Notification
/// Published by Core Intelligence domain when an exception approaches its SLA deadline
type ExceptionSlaWarningNotification =
    { ExceptionId: ExceptionId
      Severity: string option
      SlaDeadline: Timestamp
      WarningIssuedAt: Timestamp }

/// BN-C-020d: Exception SLA Escalation Notification
/// Published by Core Intelligence domain when an exception breaches its SLA deadline
type ExceptionSlaEscalationNotification =
    { ExceptionId: ExceptionId
      Severity: string option
      SlaDeadline: Timestamp
      EscalatedAt: Timestamp
      OverdueBy: System.TimeSpan }

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

/// BN-C-020a: Exception Registered Notification
/// Published by Core Intelligence domain when an exception is registered
type ExceptionRegisteredNotification =
    { ExceptionId: ExceptionId
      ConstraintReference: string
      Classification: VocabularyEntryId
      AffectedScopeType: VocabularyEntryId
      AffectedScopeIdentifier: string
      Severity: VocabularyEntryId option
      RegistrationTime: Timestamp }

/// BN-C-020b: Exception Resolved Notification
/// Published by Core Intelligence domain when an exception is resolved
type ExceptionResolvedNotification =
    { ExceptionId: ExceptionId
      ResolutionTime: Timestamp
      ResolutionEvidence: string }

/// BN-C-019a: Enterprise Picture Published Notification
/// Published by Core Intelligence domain when a new picture version is published
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

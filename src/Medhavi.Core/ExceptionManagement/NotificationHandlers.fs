namespace Medhavi.Core.CoreExceptionManagement

open System
open Medhavi.SemanticModel
open Medhavi.Foundation.Failure

/// Internal representation of exception evidence notifications coming from
/// other bounded contexts.
///
/// The actual Business Notification contract may live in Medhavi.Contracts.
/// The Nexus / Infrastructure boundary must decode that contract into this
/// Core-internal type before invoking Core capabilities.
type ExceptionEvidenceNotification =
    { ExceptionId: ExceptionId
      ConstraintReference: string
      Classification: VocabularyEntryId
      AffectedScopeType: VocabularyEntryId
      AffectedScopeIdentifier: string
      EvidenceReference: string
      Severity: VocabularyEntryId option
      EvidenceTime: Timestamp }

module NotificationHandlers =

    /// Pure translation from exception evidence notification to a Core command.
    let toRegisterCmd (notification: ExceptionEvidenceNotification) : Result<RegisterExceptionCmd, DomainError> =

        if String.IsNullOrWhiteSpace notification.ConstraintReference then
            Error(DomainError.validation "ExceptionEvidenceNotification.ConstraintReference must not be empty.")
        elif String.IsNullOrWhiteSpace notification.AffectedScopeIdentifier then
            Error(DomainError.validation "ExceptionEvidenceNotification.AffectedScopeIdentifier must not be empty.")
        elif String.IsNullOrWhiteSpace notification.EvidenceReference then
            Error(DomainError.validation "ExceptionEvidenceNotification.EvidenceReference must not be empty.")
        else
            Ok
                { ExceptionId = notification.ExceptionId
                  ConstraintReference = notification.ConstraintReference
                  Classification = notification.Classification
                  AffectedScopeType = notification.AffectedScopeType
                  AffectedScopeIdentifier = notification.AffectedScopeIdentifier
                  EvidenceReference = notification.EvidenceReference
                  Severity = notification.Severity
                  RegistrationTime = notification.EvidenceTime }

namespace Medhavi.SemanticModel

/// Governed semantic validation errors.
/// SemanticModel must not use raw strings for invariant or identity failures.
type SemanticValidationError =
    | EmptyIdentifier of fieldName: string
    | EmptyRequiredField of objectName: string * fieldName: string
    | NonUtcTimestamp of fieldName: string
    | NegativeQuantity of fieldName: string
    | NonPositiveQuantity of fieldName: string
    | NegativeDuration of fieldName: string
    | InvalidWindow of message: string
    | InvalidPercentage of objectName: string * fieldName: string
    | DuplicateValue of objectName: string * fieldName: string
    | InvalidCompositeIdentity of message: string
    | InvalidLifecycleTransition of message: string
    | InvariantViolation of objectName: string * message: string

namespace Medhavi.SemanticModel

open System

/// SE-C-023 Quantity
/// Represents a measurable amount. Enforces non-negative values for physical quantities.
[<Struct>]
type Quantity = private Quantity of decimal * UnitOfMeasureId

module Quantity =
    let create (value: decimal) : Result<Quantity, SemanticValidationError> =
        if value < 0m then
            Error(NegativeQuantity "Quantity")
        else
            Ok(Quantity(value, UnitOfMeasureId.defaultUoM()))

    let createWithUoM (value: decimal) (unitOfMeasure: UnitOfMeasureId) : Result<Quantity, SemanticValidationError> =
        if value < 0m then
            Error(NegativeQuantity "Quantity")
        else
            Ok(Quantity(value, unitOfMeasure))

    let ofDecimal (value: decimal) (unitOfMeasure: UnitOfMeasureId) = Quantity(value, unitOfMeasure)
    let value (Quantity(q, _)) = q
    let unitOfMeasure (Quantity(_, u)) = u

    let zero = Quantity(0m, UnitOfMeasureId.defaultUoM())
    let zeroWithUoM (unitOfMeasure: UnitOfMeasureId) = Quantity(0m, unitOfMeasure)

    let add (Quantity(a, u1)) (Quantity(b, u2)) =
        if u1 <> u2 then
            Error(InvariantViolation("Quantity", "Cannot add different units"))
        else
            Ok(Quantity(a + b, u1))

    let subtract (Quantity(a, u1)) (Quantity(b, u2)) =
        if u1 <> u2 then
            Error(InvariantViolation("Quantity", "Cannot subtract different units"))
        else
            let result = a - b
            if result < 0m then Ok(Quantity(0m, u1)) else Ok(Quantity(result, u1))

    let positiveQuantity (fieldName: string) (qty: Quantity) : Result<unit, SemanticValidationError> =
        if value qty <= 0m then Error(NonPositiveQuantity fieldName) else Ok()

    let nonNegativeQuantity (fieldName: string) (qty: Quantity) : Result<unit, SemanticValidationError> =
        if value qty < 0m then Error(NegativeQuantity fieldName) else Ok()

/// SE-C-025 Money
[<Struct>]
type Money =
    { Amount: decimal
      CurrencyCode: string }

module Money =
    let create amount currency =
        { Amount = amount
          CurrencyCode = currency }

    let zero currency = { Amount = 0m; CurrencyCode = currency }

    let add m1 m2 =
        if m1.CurrencyCode <> m2.CurrencyCode then
            Error(InvariantViolation("Money", "Cannot add different currencies"))
        else
            Ok(
                { m1 with
                    Amount = m1.Amount + m2.Amount }
            )

/// SE-C-038 Scope Boundary Rule
type ScopeBoundaryRule =
    { RuleIdentifier: string
      TargetSemanticType: VocabularyEntryId
      InclusionIndicator: bool
      TargetInstanceIdentifiers: string list
      TargetCategoryIdentifiers: VocabularyEntryId list }

module ScopeBoundaryRule =
    let validate (rule: ScopeBoundaryRule) : Result<unit, SemanticValidationError> =
        Invariants.firstError
            [ Invariants.nonEmptyIdentifier "ScopeBoundaryRule.RuleIdentifier" rule.RuleIdentifier
              Invariants.nonEmptyIdentifier
                  "ScopeBoundaryRule.TargetSemanticType"
                  (VocabularyEntryId.value rule.TargetSemanticType)
              Invariants.noEmptyStrings "ScopeBoundaryRule" "TargetInstanceIdentifiers" rule.TargetInstanceIdentifiers ]

namespace Medhavi.SemanticModel

open Medhavi.SemanticModel.Invariants

/// SE-C-039 Scenario Adjustment
type ScenarioAdjustment =
    { AdjustmentIdentifier: string
      TargetSemanticType: VocabularyEntryId
      TargetInstanceIdentifiers: string list
      TargetCategoryIdentifiers: VocabularyEntryId list
      AdjustmentType: VocabularyEntryId
      AdjustmentQuantity: Quantity option
      AdjustmentText: string option }

module ScenarioAdjustment =
    let create
        (adjustmentIdentifier: string)
        (targetSemanticType: VocabularyEntryId)
        (targetInstanceIdentifiers: string list)
        (targetCategoryIdentifiers: VocabularyEntryId list)
        (adjustmentType: VocabularyEntryId)
        (adjustmentQuantity: Quantity option)
        (adjustmentText: string option)
        : Result<ScenarioAdjustment, SemanticValidationError> =
        if System.String.IsNullOrWhiteSpace adjustmentIdentifier then
            Error(EmptyIdentifier "AdjustmentIdentifier")
        elif targetInstanceIdentifiers.IsEmpty && targetCategoryIdentifiers.IsEmpty then
            Error(
                InvariantViolation(
                    "ScenarioAdjustment",
                    "At least one Target Instance Identifier or Target Category Identifier must be present."
                )
            )
        elif adjustmentQuantity.IsNone && adjustmentText.IsNone then
            Error(
                InvariantViolation(
                    "ScenarioAdjustment",
                    "At least one of Adjustment Quantity or Adjustment Text must be present."
                )
            )
        else
            Ok
                { AdjustmentIdentifier = adjustmentIdentifier
                  TargetSemanticType = targetSemanticType
                  TargetInstanceIdentifiers = targetInstanceIdentifiers
                  TargetCategoryIdentifiers = targetCategoryIdentifiers
                  AdjustmentType = adjustmentType
                  AdjustmentQuantity = adjustmentQuantity
                  AdjustmentText = adjustmentText }

    let validateScenarioAdjustment (adjustment: ScenarioAdjustment) : Result<unit, SemanticValidationError> =
        firstError
            [ nonEmptyIdentifier "ScenarioAdjustment.AdjustmentIdentifier" adjustment.AdjustmentIdentifier
              nonEmptyIdentifier
                  "ScenarioAdjustment.TargetSemanticType"
                  (VocabularyEntryId.value adjustment.TargetSemanticType)
              nonEmptyIdentifier "ScenarioAdjustment.AdjustmentType" (VocabularyEntryId.value adjustment.AdjustmentType) ]

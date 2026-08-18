namespace Medhavi.SemanticModel

open Medhavi.SemanticModel.Invariants

/// SE-C-026 Capacity
type Capacity =
    { CapacityMeasure: VocabularyEntryId
      OutputQuantity: Quantity
      TimePeriod: Duration<mins> }

module Capacity =
    let create
        (capacityMeasure: VocabularyEntryId)
        (outputQuantity: Quantity)
        (timePeriod: Duration<mins>)
        : Result<Capacity, SemanticValidationError> =
        Ok
            { CapacityMeasure = capacityMeasure
              OutputQuantity = outputQuantity
              TimePeriod = timePeriod }

    let measure (capacity: Capacity) = capacity.CapacityMeasure
    let outputQuantity (capacity: Capacity) = capacity.OutputQuantity
    let timePeriod (capacity: Capacity) = capacity.TimePeriod

    let validateCapacity (capacity: Capacity) : Result<unit, SemanticValidationError> =
        firstError [ nonEmptyIdentifier "Capacity.CapacityMeasure" (VocabularyEntryId.value capacity.CapacityMeasure) ]

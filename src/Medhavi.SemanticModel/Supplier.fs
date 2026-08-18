namespace Medhavi.SemanticModel

type SupplierId = private SupplierId of string

module SupplierId =
    let create (id: string) = Invariants.createStringId SupplierId "SupplierId" id
    let value (SupplierId id) = id

/// SE-C-004 Supplier
type Supplier =
    { SupplierIdentifier: SupplierId
      SupplierName: string
      LifecycleState: ReferenceLifecycleState }

module Supplier =
    let validate (supplier: Supplier) : Result<unit, SemanticValidationError> =
        Invariants.firstError
            [ Invariants.nonEmptyIdentifier "SupplierId" (SupplierId.value supplier.SupplierIdentifier)
              Invariants.nonEmptyField "Supplier" "SupplierName" supplier.SupplierName ]

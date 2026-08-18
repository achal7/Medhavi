namespace Medhavi.SemanticModel

type CustomerId = private CustomerId of string

module CustomerId =
    let create (id: string) = Invariants.createStringId CustomerId "CustomerId" id
    let value (CustomerId id) = id

type CustomerClass =
    | ClassA
    | ClassB
    | ClassC
    | ClassD

/// SE-C-003 Customer
type Customer =
    { CustomerIdentifier: CustomerId
      CustomerName: string
      CustomerClass: CustomerClass option
      LifecycleState: ReferenceLifecycleState }

module Customer =
    let validate (customer: Customer) : Result<unit, SemanticValidationError> =
        Invariants.firstError
            [ Invariants.nonEmptyIdentifier "CustomerId" (CustomerId.value customer.CustomerIdentifier)
              Invariants.nonEmptyField "Customer" "CustomerName" customer.CustomerName ]

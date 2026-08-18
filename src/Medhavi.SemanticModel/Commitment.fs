namespace Medhavi.SemanticModel

type CommitmentId = private CommitmentId of string

module CommitmentId =
    let create (id: string) = Invariants.createStringId CommitmentId "CommitmentId" id
    let value (CommitmentId id) = id

type ObligationDirection =
    | Inbound
    | Outbound

/// Lifecycle states for Commitment
type CommitmentLifecycleState =
    | Committed
    | Fulfilled
    | Cancelled

module CommitmentLifecycleState =
    let validateTransition
        (fromState: CommitmentLifecycleState)
        (toState: CommitmentLifecycleState)
        : Result<unit, SemanticValidationError> =
        match fromState, toState with
        | Committed, Fulfilled
        | Committed, Cancelled -> Ok()
        | _ -> Error(InvalidLifecycleTransition(sprintf "%A -> %A is not a permitted lifecycle transition." fromState toState))

/// SE-C-017 Commitment
type Commitment =
    { CommitmentIdentifier: CommitmentId
      Direction: ObligationDirection
      Item: ItemId
      Location: LocationId
      Customer: CustomerId option
      Supplier: SupplierId option
      Quantity: Quantity
      RequestedDate: Timestamp
      CommittedDate: Timestamp option
      LifecycleState: CommitmentLifecycleState }

module Commitment =
    let validate (commitment: Commitment) : Result<unit, SemanticValidationError> =
        let counterpartyCheck =
            if commitment.Customer.IsNone && commitment.Supplier.IsNone then
                Error(
                    InvariantViolation(
                        "Commitment",
                        "A Commitment requires at least one counterparty: Customer or Supplier."
                    )
                )
            else
                Ok()

        let dateCheck =
            match commitment.CommittedDate with
            | Some committed when Timestamp.isBefore committed commitment.RequestedDate ->
                Error(InvariantViolation("Commitment", "CommittedDate must not be before RequestedDate."))
            | _ -> Ok()

        Invariants.firstError
            [ Invariants.nonEmptyIdentifier "CommitmentId" (CommitmentId.value commitment.CommitmentIdentifier)
              Quantity.positiveQuantity "Commitment.Quantity" commitment.Quantity
              counterpartyCheck
              dateCheck ]

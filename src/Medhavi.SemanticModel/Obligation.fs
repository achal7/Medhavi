namespace Medhavi.SemanticModel

/// SE-C-017 Commitment
type Commitment =
    { CommitmentIdentifier: CommitmentId
      Direction: ObligationDirection
      Item: ItemId
      Location: LocationId
      Customer: CustomerId option
      Supplier: SupplierId option
      Quantity: Quantity
      CommitmentTime: Timestamp
      DueWindow: TemporalWindow
      LifecycleState: CommitmentLifecycleState }

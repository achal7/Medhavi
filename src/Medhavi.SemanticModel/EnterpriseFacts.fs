namespace Medhavi.SemanticModel

/// SE-C-013 Demand
type Demand =
    { DemandIdentifier: DemandId
      Item: ItemId
      Location: LocationId
      Customer: CustomerId option
      Quantity: Quantity
      NeedWindow: NeedWindow
      DemandOrigin: DemandOrigin
      LifecycleState: DemandLifecycleState }

/// SE-C-014 Supply
type Supply =
    { SupplyIdentifier: SupplyId
      Item: ItemId
      Location: LocationId
      Quantity: Quantity
      AvailabilityWindow: TemporalWindow
      Provenance: SupplyProvenanceClassification
      LifecycleState: SupplyLifecycleState }

/// SE-C-015 Inventory
/// Corrected identity: Item + Location + Batch Identifier.
type Inventory =
    { Identity: InventoryIdentity
      OnHandQuantity: Quantity
      ObservationTime: Timestamp }

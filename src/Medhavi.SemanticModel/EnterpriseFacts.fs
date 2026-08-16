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
      ParentDemand: DemandId option
      LifecycleState: DemandLifecycleState }

/// SE-C-014 Supply
type Supply =
    { SupplyIdentifier: SupplyId
      Item: ItemId
      Location: LocationId
      Quantity: Quantity
      AvailabilityWindow: TemporalWindow
      Provenance: VocabularyEntryId
      LifecycleState: SupplyLifecycleState }

/// SE-C-015 Inventory
type Inventory =
    { Identity: InventoryIdentity
      OnHandQuantity: Quantity
      ObservationTime: Timestamp }

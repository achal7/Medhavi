namespace Medhavi.SemanticModel

open System

/// SE-C-032 Unit of Measure
type UnitOfMeasure =
    { UnitIdentifier: UnitOfMeasureId
      UnitName: string
      UnitClassification: string
      AdoptionState: AdoptionState }

/// SE-C-031 Time Zone
type TimeZone =
    { TimeZoneIdentifier: TimeZoneId
      DisplayName: string
      UtcOffset: TimeSpan }

/// SE-C-001 Item
type Item =
    { ItemIdentifier: ItemId
      EnterpriseBusinessIdentifier: string option
      ItemName: string
      ItemType: VocabularyEntryId option
      ItemRoles: VocabularyEntryId list
      UnitOfMeasure: UnitOfMeasureId
      LifecycleState: ReferenceLifecycleState }

/// SE-C-002 Location
type Location =
    { LocationIdentifier: LocationId
      LocationName: string
      LocationType: LocationType
      TimeZone: TimeZoneId
      LifecycleState: LocationLifecycleState }

/// SE-C-003 Customer
type Customer =
    { CustomerIdentifier: CustomerId
      CustomerName: string
      CustomerClass: CustomerClass option
      LifecycleState: ReferenceLifecycleState }

/// SE-C-004 Supplier
type Supplier =
    { SupplierIdentifier: SupplierId
      SupplierName: string
      LifecycleState: ReferenceLifecycleState }

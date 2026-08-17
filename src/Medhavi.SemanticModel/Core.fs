namespace Medhavi.SemanticModel

open System

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

/// SE-C-019 Exception
type Exception =
    { ExceptionIdentifier: ExceptionId
      ConstraintReference: string
      Classification: VocabularyEntryId
      AffectedScopeType: VocabularyEntryId
      AffectedScopeIdentifier: string
      EvidenceReference: string option
      LifecycleState: ExceptionLifecycleState }

/// SE-C-021 Enterprise Picture — PictureVersion entity
type PictureVersion =
    { VersionNumber: PictureVersionId
      DemandReferences: DemandId list
      SupplyReferences: SupplyId list
      InventoryReferences: InventoryIdentity list
      PublicationTime: Timestamp option
      LifecycleState: PictureVersionLifecycleState }

/// SE-C-021 Enterprise Picture aggregate root
type EnterprisePicture =
    { PlanningScopeIdentifier: PlanningScopeId
      Versions: PictureVersion list }

/// SE-C-031 Time Zone
type TimeZone =
    { TimeZoneIdentifier: TimeZoneId
      DisplayName: string
      UtcOffset: TimeSpan }

/// SE-C-032 Unit of Measure
type UnitOfMeasure =
    { UnitIdentifier: UnitOfMeasureId
      UnitName: string
      UnitClassification: string
      AdoptionState: AdoptionState }

/// SE-C-037 Enterprise Governed Vocabulary — VocabularyEntry entity
type VocabularyEntry =
    { VocabularyCategoryIdentifier: VocabularyEntryId
      EntryIdentifier: VocabularyEntryId
      EntryName: string
      LifecycleState: GovernedCatalogState }

/// SE-C-037 Enterprise Governed Vocabulary aggregate root
type EnterpriseGovernedVocabulary =
    { CatalogIdentifier: string
      VersionNumber: int
      Entries: VocabularyEntry list
      LifecycleState: GovernedCatalogState }

/// SE-C-040 Item Transition
type ItemTransition =
    { TransitionIdentifier: TransitionId
      SupersededItem: ItemId
      SupersedingItem: ItemId
      TransitionType: VocabularyEntryId
      EffectiveDate: Timestamp
      EndDate: Timestamp option
      LifecycleState: ItemTransitionLifecycleState }

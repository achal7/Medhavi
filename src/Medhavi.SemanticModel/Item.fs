namespace Medhavi.SemanticModel

type ItemId = private ItemId of string

module ItemId =
    let create (id: string) = Invariants.createStringId ItemId "ItemId" id
    let value (ItemId id) = id

/// SE-C-001 Item
type Item =
    { ItemIdentifier: ItemId
      EnterpriseBusinessIdentifier: string option
      ItemName: string
      ItemType: VocabularyEntryId option
      ItemRoles: VocabularyEntryId list
      UnitOfMeasure: UnitOfMeasureId
      LifecycleState: ReferenceLifecycleState }

module Item =
    let validate (item: Item) : Result<unit, SemanticValidationError> =
        Invariants.firstError
            [ Invariants.nonEmptyIdentifier "ItemId" (ItemId.value item.ItemIdentifier)
              Invariants.nonEmptyField "Item" "ItemName" item.ItemName
              Invariants.nonEmptyIdentifier "Item.UnitOfMeasure" (UnitOfMeasureId.value item.UnitOfMeasure) ]

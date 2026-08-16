namespace Medhavi.SemanticModel

/// BOM line entity.
/// This exists inside the Bill of Materials aggregate.
type BomLine =
    { ComponentItem: ItemId
      QuantityPerParent: Quantity
      LeadTimeOffset: Duration }

/// SE-C-018 Bill of Materials
type BillOfMaterials =
    { BomVersionIdentifier: BomVersionId
      ParentItem: ItemId
      Lines: BomLine list
      LifecycleState: ReferenceLifecycleState }

/// SE-C-020 Risk
type Risk =
    { RiskIdentifier: RiskId
      AffectedScopeType: VocabularyEntryId
      AffectedScopeIdentifier: string
      Assessments: RiskAssessment list
      LifecycleState: RiskLifecycleState }

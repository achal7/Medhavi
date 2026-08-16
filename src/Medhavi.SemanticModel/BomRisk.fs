namespace Medhavi.SemanticModel

/// BOM line entity.
type BomLine =
    { LineIdentifier: LineIdentifier
      SequenceNumber: int option
      ComponentItem: ItemId
      QuantityPerParent: Quantity }

/// SE-C-018 Bill of Materials
type BillOfMaterials =
    { BomVersionIdentifier: BomVersionId
      ParentItem: ItemId
      VersionNumber: int
      EffectiveDate: Timestamp option
      EndDate: Timestamp option
      Lines: BomLine list
      LifecycleState: BomLifecycleState }

/// SE-C-020 Risk
type Risk =
    { RiskIdentifier: RiskId
      RiskType: VocabularyEntryId
      RiskSubjectType: VocabularyEntryId
      RiskSubjectIdentifier: string
      Assessments: RiskAssessment list
      LifecycleState: RiskLifecycleState }

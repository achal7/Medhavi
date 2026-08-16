namespace Medhavi.SemanticModel

/// Performance Indicator entity.
type PerformanceIndicator =
    { IndicatorIdentifier: string
      Name: string
      MeasureCategory: VocabularyEntryId
      MeasureNature: VocabularyEntryId
      EnterpriseQuestion: string
      BusinessObjectivesServed: string list
      EnterpriseMeaning: string
      FormulaReference: string
      SemanticDependencies: string list }

/// SE-C-035 Performance Indicator Catalog
type PerformanceIndicatorCatalog =
    { CatalogIdentifier: string
      VersionNumber: int
      Indicators: PerformanceIndicator list
      LifecycleState: GovernedCatalogState }

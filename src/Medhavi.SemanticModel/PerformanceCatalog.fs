namespace Medhavi.SemanticModel

/// Performance Indicator entity.
/// This exists inside the Performance Indicator Catalog aggregate.
type PerformanceIndicator =
    { IndicatorIdentifier: string
      IndicatorName: string
      Description: string
      FormulaReference: string
      SemanticDependencies: string list }

/// SE-C-035 Performance Indicator Catalog
type PerformanceIndicatorCatalog =
    { CatalogIdentifier: string
      VersionNumber: int
      Indicators: PerformanceIndicator list }

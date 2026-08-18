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

module PerformanceIndicator =
    let validate (indicator: PerformanceIndicator) : Result<unit, SemanticValidationError> =
        Invariants.firstError
            [ Invariants.nonEmptyIdentifier "PerformanceIndicator.IndicatorIdentifier" indicator.IndicatorIdentifier
              Invariants.nonEmptyField "PerformanceIndicator" "Name" indicator.Name
              Invariants.nonEmptyIdentifier "PerformanceIndicator.MeasureCategory" (VocabularyEntryId.value indicator.MeasureCategory)
              Invariants.nonEmptyIdentifier "PerformanceIndicator.MeasureNature" (VocabularyEntryId.value indicator.MeasureNature)
              Invariants.nonEmptyField "PerformanceIndicator" "FormulaReference" indicator.FormulaReference
              Invariants.noEmptyStrings "PerformanceIndicator" "SemanticDependencies" indicator.SemanticDependencies ]

/// SE-C-035 Performance Indicator Catalog
type PerformanceIndicatorCatalog =
    { CatalogIdentifier: string
      VersionNumber: int
      Indicators: PerformanceIndicator list
      LifecycleState: GovernedCatalogState }

module PerformanceIndicatorCatalog =
    let validate (catalog: PerformanceIndicatorCatalog) : Result<unit, SemanticValidationError> =
        let indicatorChecks = catalog.Indicators |> List.map PerformanceIndicator.validate

        let duplicateIndicatorCheck =
            if Invariants.hasDuplicatesBy (fun indicator -> indicator.IndicatorIdentifier) catalog.Indicators then
                Error(DuplicateValue("PerformanceIndicatorCatalog", "Indicators.IndicatorIdentifier"))
            else
                Ok()

        Invariants.firstError(
            [ Invariants.nonEmptyField "PerformanceIndicatorCatalog" "CatalogIdentifier" catalog.CatalogIdentifier
              Invariants.nonNegativeInt "PerformanceIndicatorCatalog" "VersionNumber" catalog.VersionNumber
              duplicateIndicatorCheck ]
            @ indicatorChecks
        )

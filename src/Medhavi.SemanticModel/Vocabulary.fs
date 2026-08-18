namespace Medhavi.SemanticModel

type VocabularyEntryId = private VocabularyEntryId of string

module VocabularyEntryId =
    let create (id: string) = Invariants.createStringId VocabularyEntryId "VocabularyEntryId" id
    let value (VocabularyEntryId id) = id

/// Lifecycle states for Governed Catalogs (Vocabulary, PI Catalog)
type GovernedCatalogState =
    | Active
    | Deprecated
    | Retired

module GovernedCatalogState =
    let validateTransition
        (fromState: GovernedCatalogState)
        (toState: GovernedCatalogState)
        : Result<unit, SemanticValidationError> =
        match fromState, toState with
        | GovernedCatalogState.Active, GovernedCatalogState.Deprecated
        | GovernedCatalogState.Active, GovernedCatalogState.Retired
        | GovernedCatalogState.Deprecated, GovernedCatalogState.Retired -> Ok()
        | _ -> Error(InvalidLifecycleTransition(sprintf "%A -> %A is not a permitted lifecycle transition." fromState toState))

/// SE-C-037 Enterprise Governed Vocabulary — VocabularyEntry entity
type VocabularyEntry =
    { VocabularyCategoryIdentifier: VocabularyEntryId
      EntryIdentifier: VocabularyEntryId
      EntryName: string
      LifecycleState: GovernedCatalogState }

module VocabularyEntry =
    let validate (entry: VocabularyEntry) : Result<unit, SemanticValidationError> =
        Invariants.firstError
            [ Invariants.nonEmptyIdentifier "VocabularyCategoryIdentifier" (VocabularyEntryId.value entry.VocabularyCategoryIdentifier)
              Invariants.nonEmptyIdentifier "VocabularyEntryId" (VocabularyEntryId.value entry.EntryIdentifier)
              Invariants.nonEmptyField "VocabularyEntry" "EntryName" entry.EntryName ]

/// SE-C-037 Enterprise Governed Vocabulary aggregate root
type EnterpriseGovernedVocabulary =
    { CatalogIdentifier: string
      VersionNumber: int
      Entries: VocabularyEntry list
      LifecycleState: GovernedCatalogState }

module EnterpriseGovernedVocabulary =
    let validate (vocabulary: EnterpriseGovernedVocabulary) : Result<unit, SemanticValidationError> =
        let entryChecks = vocabulary.Entries |> List.map VocabularyEntry.validate

        let duplicateEntryIdentifierCheck =
            if Invariants.hasDuplicatesBy (fun entry -> entry.EntryIdentifier) vocabulary.Entries then
                Error(DuplicateValue("EnterpriseGovernedVocabulary", "Entries.EntryIdentifier"))
            else
                Ok()

        Invariants.firstError(
            [ Invariants.nonEmptyField "EnterpriseGovernedVocabulary" "CatalogIdentifier" vocabulary.CatalogIdentifier
              Invariants.nonNegativeInt "EnterpriseGovernedVocabulary" "VersionNumber" vocabulary.VersionNumber
              duplicateEntryIdentifierCheck ]
            @ entryChecks
        )

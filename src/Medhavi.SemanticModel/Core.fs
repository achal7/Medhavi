namespace Medhavi.SemanticModel

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

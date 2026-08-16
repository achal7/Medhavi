namespace Medhavi.SemanticModel

/// SE-C-019 Exception
/// Exception is governed by Core Exception Management behavior,
/// but its semantic shape belongs to the Enterprise Semantic Model.
type Exception =
    { ExceptionIdentifier: ExceptionId
      ConstraintReference: string
      Classification: VocabularyEntryId
      AffectedScopeType: VocabularyEntryId
      AffectedScopeIdentifier: string
      EvidenceReference: string
      Severity: VocabularyEntryId option
      LifecycleState: ExceptionLifecycleState }

/// SE-C-021 Enterprise Picture — PictureVersion entity
type PictureVersion =
    { VersionNumber: PictureVersionId
      DemandReferences: DemandId list
      SupplyReferences: SupplyId list
      InventoryReferences: InventoryIdentity list
      CompositionTime: Timestamp
      PublicationTime: Timestamp option
      LifecycleState: PictureVersionLifecycleState }

/// SE-C-021 Enterprise Picture aggregate root
type EnterprisePicture =
    { PlanningScopeIdentifier: PlanningScopeId
      Versions: PictureVersion list }

/// SE-C-037 Enterprise Governed Vocabulary — VocabularyEntry entity
type VocabularyEntry =
    { EntryIdentifier: VocabularyEntryId
      EntryValue: string
      LifecycleState: AdoptionState }

/// SE-C-037 Enterprise Governed Vocabulary aggregate root
type EnterpriseGovernedVocabulary =
    { CatalogIdentifier: string
      VersionNumber: int
      Entries: VocabularyEntry list }

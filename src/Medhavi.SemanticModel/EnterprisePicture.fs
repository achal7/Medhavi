namespace Medhavi.SemanticModel

type PictureVersionId = private PictureVersionId of int

module PictureVersionId =
    let create (version: int) : Result<PictureVersionId, SemanticValidationError> =
        if version < 0 then
            Error(InvariantViolation("PictureVersionId", "Picture version must be non-negative."))
        else
            Ok(PictureVersionId version)

    let value (PictureVersionId version) = version

/// Lifecycle states for Enterprise Picture version
type PictureVersionLifecycleState =
    | Draft
    | Published
    | Superseded

module PictureVersionLifecycleState =
    let validateTransition
        (fromState: PictureVersionLifecycleState)
        (toState: PictureVersionLifecycleState)
        : Result<unit, SemanticValidationError> =
        match fromState, toState with
        | Draft, Published
        | Published, Superseded -> Ok()
        | _ -> Error(InvalidLifecycleTransition(sprintf "%A -> %A is not a permitted lifecycle transition." fromState toState))

/// SE-C-021 Enterprise Picture — PictureVersion entity
type PictureVersion =
    { VersionNumber: PictureVersionId
      DemandReferences: DemandId list
      SupplyReferences: SupplyId list
      InventoryReferences: InventoryIdentity list
      PublicationTime: Timestamp option
      LifecycleState: PictureVersionLifecycleState }

module PictureVersion =
    let validate (version: PictureVersion) : Result<unit, SemanticValidationError> =
        let duplicateDemandCheck =
            if Invariants.hasDuplicatesBy id version.DemandReferences then
                Error(DuplicateValue("PictureVersion", "DemandReferences"))
            else
                Ok()

        let duplicateSupplyCheck =
            if Invariants.hasDuplicatesBy id version.SupplyReferences then
                Error(DuplicateValue("PictureVersion", "SupplyReferences"))
            else
                Ok()

        let duplicateInventoryCheck =
            if Invariants.hasDuplicatesBy id version.InventoryReferences then
                Error(DuplicateValue("PictureVersion", "InventoryReferences"))
            else
                Ok()

        Invariants.firstError [ duplicateDemandCheck; duplicateSupplyCheck; duplicateInventoryCheck ]

/// SE-C-021 Enterprise Picture aggregate root
type EnterprisePicture =
    { PlanningScopeIdentifier: PlanningScopeId
      Versions: PictureVersion list }

module EnterprisePicture =
    let validate (picture: EnterprisePicture) : Result<unit, SemanticValidationError> =
        let versionChecks = picture.Versions |> List.map PictureVersion.validate

        let duplicateVersionCheck =
            if Invariants.hasDuplicatesBy (fun (version: PictureVersion) -> version.VersionNumber) picture.Versions then
                Error(DuplicateValue("EnterprisePicture", "Versions.VersionNumber"))
            else
                Ok()

        let publishedVersions =
            picture.Versions |> List.filter(fun version -> version.LifecycleState = PictureVersionLifecycleState.Published)

        let publishedCheck =
            if List.length publishedVersions > 1 then
                Error(InvariantViolation("EnterprisePicture", "At most one PictureVersion may be Published at any time."))
            else
                Ok()

        Invariants.firstError(
            [ Invariants.nonEmptyIdentifier "PlanningScopeId" (PlanningScopeId.value picture.PlanningScopeIdentifier)
              duplicateVersionCheck
              publishedCheck ]
            @ versionChecks
        )

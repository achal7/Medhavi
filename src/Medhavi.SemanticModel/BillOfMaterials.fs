namespace Medhavi.SemanticModel

type BomVersionId = private BomVersionId of string

module BomVersionId =
    let create (id: string) = Invariants.createStringId BomVersionId "BomVersionId" id
    let value (BomVersionId id) = id

type LineIdentifier = private LineIdentifier of string

module LineIdentifier =
    let create (id: string) = Invariants.createStringId LineIdentifier "LineIdentifier" id
    let value (LineIdentifier id) = id

/// Lifecycle states for Bill of Materials
type BomLifecycleState =
    | Draft
    | Active
    | Superseded
    | Archived

module BomLifecycleState =
    let validateTransition
        (fromState: BomLifecycleState)
        (toState: BomLifecycleState)
        : Result<unit, SemanticValidationError> =
        match fromState, toState with
        | BomLifecycleState.Draft, BomLifecycleState.Active
        | BomLifecycleState.Draft, BomLifecycleState.Archived
        | BomLifecycleState.Active, BomLifecycleState.Superseded
        | BomLifecycleState.Active, BomLifecycleState.Archived -> Ok()
        | _ -> Error(InvalidLifecycleTransition(sprintf "%A -> %A is not a permitted lifecycle transition." fromState toState))

/// BOM line entity.
type BomLine =
    { LineIdentifier: LineIdentifier
      SequenceNumber: int option
      ComponentItem: ItemId
      QuantityPerParent: Quantity }

module BomLine =
    let validate (line: BomLine) : Result<unit, SemanticValidationError> =
        Invariants.firstError
            [ Invariants.nonEmptyIdentifier "BomLine.LineIdentifier" (LineIdentifier.value line.LineIdentifier)
              Quantity.positiveQuantity "BomLine.QuantityPerParent" line.QuantityPerParent ]

/// SE-C-018 Bill of Materials
type BillOfMaterials =
    { BomVersionIdentifier: BomVersionId
      ParentItem: ItemId
      VersionNumber: int
      EffectiveDate: Timestamp option
      EndDate: Timestamp option
      Lines: BomLine list
      LifecycleState: BomLifecycleState }

module BillOfMaterials =
    let validate (bom: BillOfMaterials) : Result<unit, SemanticValidationError> =
        let lineChecks = bom.Lines |> List.map BomLine.validate

        let duplicateComponentCheck =
            if Invariants.hasDuplicatesBy (fun line -> line.ComponentItem) bom.Lines then
                Error(DuplicateValue("BillOfMaterials", "Lines.ComponentItem"))
            else
                Ok()

        let selfReferenceCheck =
            if bom.Lines |> List.exists(fun line -> line.ComponentItem = bom.ParentItem) then
                Error(InvariantViolation("BillOfMaterials", "ParentItem must not appear as its own component."))
            else
                Ok()

        let dateCheck =
            match bom.EffectiveDate, bom.EndDate with
            | Some eff, Some endD when Timestamp.isAfter eff endD ->
                Error(InvariantViolation("BillOfMaterials", "EffectiveDate must not be after EndDate."))
            | _ -> Ok()

        Invariants.firstError(
            [ Invariants.nonEmptyIdentifier "BomVersionId" (BomVersionId.value bom.BomVersionIdentifier)
              Invariants.nonNegativeInt "BillOfMaterials" "VersionNumber" bom.VersionNumber
              duplicateComponentCheck
              selfReferenceCheck
              dateCheck ]
            @ lineChecks
        )

module Medhavi.SemanticModel.Invariants

open System
open Medhavi.SemanticModel.Identities

/// Pure structural and semantic invariant validators.
/// These validators belong to the Semantic Model and must remain side-effect free.

// ---------------------------------------------------------------------
// Internal helpers
// ---------------------------------------------------------------------

let private firstError (checks: Result<unit, SemanticValidationError> list) : Result<unit, SemanticValidationError> =
    checks
    |> List.tryPick (function
        | Error e -> Some e
        | Ok() -> None)
    |> function
        | Some e -> Error e
        | None -> Ok()

let private nonEmptyIdentifier (fieldName: string) (value: string) : Result<unit, SemanticValidationError> =
    if String.IsNullOrWhiteSpace value then
        Error(EmptyIdentifier fieldName)
    else
        Ok()

let private nonEmptyField
    (objectName: string)
    (fieldName: string)
    (value: string)
    : Result<unit, SemanticValidationError> =
    if String.IsNullOrWhiteSpace value then
        Error(EmptyRequiredField(objectName, fieldName))
    else
        Ok()

let private noEmptyStrings
    (objectName: string)
    (fieldName: string)
    (values: string list)
    : Result<unit, SemanticValidationError> =
    if values |> List.exists String.IsNullOrWhiteSpace then
        Error(InvariantViolation(objectName, sprintf "%s must not contain empty values." fieldName))
    else
        Ok()

let private nonNegativeInt
    (objectName: string)
    (fieldName: string)
    (value: int)
    : Result<unit, SemanticValidationError> =
    if value < 0 then
        Error(InvariantViolation(objectName, sprintf "%s must be non-negative." fieldName))
    else
        Ok()

let private nonNegativeQuantity (fieldName: string) (value: Quantity) : Result<unit, SemanticValidationError> =
    if Quantity.value value < 0m then Error(NegativeQuantity fieldName) else Ok()

let private positiveQuantity (fieldName: string) (value: Quantity) : Result<unit, SemanticValidationError> =
    if Quantity.value value <= 0m then
        Error(NonPositiveQuantity fieldName)
    else
        Ok()

let private nonNegativeDuration (fieldName: string) (value: Duration) : Result<unit, SemanticValidationError> =
    if Duration.value value < 0m then Error(NegativeDuration fieldName) else Ok()

let private hasDuplicatesBy (projection: 'a -> 'b) (items: 'a list) : bool =
    let projected = items |> List.map projection
    projected.Length <> (projected |> List.distinct |> List.length)

// ---------------------------------------------------------------------
// Value Object invariants
// ---------------------------------------------------------------------

let validateTemporalWindow (window: TemporalWindow) : Result<unit, SemanticValidationError> =
    match window.Earliest with
    | Some earliest when Timestamp.isAfter earliest window.Latest ->
        Error(InvalidWindow "TemporalWindow.Earliest must not be after TemporalWindow.Latest.")
    | _ -> Ok()

let validateNeedWindow (window: NeedWindow) : Result<unit, SemanticValidationError> =
    let earliestCheck =
        match window.EarliestAcceptable with
        | Some earliest when Timestamp.isAfter earliest window.LatestAcceptable ->
            Error(InvalidWindow "NeedWindow.EarliestAcceptable must not be after NeedWindow.LatestAcceptable.")
        | _ -> Ok()

    let preferredCheck =
        match window.Preferred with
        | Some preferred ->
            let earliestValid =
                match window.EarliestAcceptable with
                | Some earliest when Timestamp.isAfter earliest preferred ->
                    Error(InvalidWindow "NeedWindow.Preferred must not be before NeedWindow.EarliestAcceptable.")
                | _ -> Ok()

            let latestValid =
                if Timestamp.isAfter preferred window.LatestAcceptable then
                    Error(InvalidWindow "NeedWindow.Preferred must not be after NeedWindow.LatestAcceptable.")
                else
                    Ok()

            firstError [ earliestValid; latestValid ]
        | None -> Ok()

    firstError [ earliestCheck; preferredCheck ]

let validatePlanningHorizon (horizon: PlanningHorizon) : Result<unit, SemanticValidationError> =
    if Timestamp.isAfter horizon.Start horizon.End then
        Error(InvalidWindow "PlanningHorizon.Start must not be after PlanningHorizon.End.")
    else
        Ok()

let validateCapacity (capacity: Capacity) : Result<unit, SemanticValidationError> =
    firstError
        [ nonEmptyIdentifier "Capacity.CapacityMeasure" (vocabularyEntryIdValue capacity.CapacityMeasure)
          nonNegativeQuantity "Capacity.OutputQuantity" capacity.OutputQuantity
          nonNegativeDuration "Capacity.TimePeriod" capacity.TimePeriod ]

let validateScopeBoundaryRule (rule: ScopeBoundaryRule) : Result<unit, SemanticValidationError> =
    firstError
        [ nonEmptyIdentifier "ScopeBoundaryRule.RuleIdentifier" rule.RuleIdentifier
          nonEmptyIdentifier "ScopeBoundaryRule.TargetSemanticType" (vocabularyEntryIdValue rule.TargetSemanticType)
          noEmptyStrings "ScopeBoundaryRule" "TargetInstanceIdentifiers" rule.TargetInstanceIdentifiers ]

let validateScenarioAdjustment (adjustment: ScenarioAdjustment) : Result<unit, SemanticValidationError> =
    firstError
        [ nonEmptyIdentifier "ScenarioAdjustment.AdjustmentIdentifier" adjustment.AdjustmentIdentifier
          nonEmptyIdentifier
              "ScenarioAdjustment.TargetSemanticType"
              (vocabularyEntryIdValue adjustment.TargetSemanticType)
          nonEmptyIdentifier "ScenarioAdjustment.AdjustmentType" (vocabularyEntryIdValue adjustment.AdjustmentType) ]

// ---------------------------------------------------------------------
// Reference Object invariants
// ---------------------------------------------------------------------

let validateUnitOfMeasure (unit: UnitOfMeasure) : Result<unit, SemanticValidationError> =
    firstError
        [ nonEmptyIdentifier "UnitOfMeasureId" (unitOfMeasureIdValue unit.UnitIdentifier)
          nonEmptyField "UnitOfMeasure" "UnitName" unit.UnitName
          nonEmptyField "UnitOfMeasure" "UnitClassification" unit.UnitClassification ]

let validateTimeZone (timeZone: Medhavi.SemanticModel.TimeZone) : Result<unit, SemanticValidationError> =
    let minOffset = TimeSpan.FromHours -14.0
    let maxOffset = TimeSpan.FromHours 14.0

    let offsetCheck =
        if timeZone.UtcOffset < minOffset || timeZone.UtcOffset > maxOffset then
            Error(InvariantViolation("TimeZone", "UtcOffset must be within the valid global UTC offset range."))
        else
            Ok()

    firstError
        [ nonEmptyIdentifier "TimeZoneId" (timeZoneIdValue timeZone.TimeZoneIdentifier)
          nonEmptyField "TimeZone" "DisplayName" timeZone.DisplayName
          offsetCheck ]

let validateItem (item: Item) : Result<unit, SemanticValidationError> =
    firstError
        [ nonEmptyIdentifier "ItemId" (itemIdValue item.ItemIdentifier)
          nonEmptyField "Item" "ItemName" item.ItemName
          nonEmptyIdentifier "Item.UnitOfMeasure" (unitOfMeasureIdValue item.UnitOfMeasure) ]

let validateLocation (location: Location) : Result<unit, SemanticValidationError> =
    firstError
        [ nonEmptyIdentifier "LocationId" (locationIdValue location.LocationIdentifier)
          nonEmptyField "Location" "LocationName" location.LocationName ]

let validateCustomer (customer: Customer) : Result<unit, SemanticValidationError> =
    firstError
        [ nonEmptyIdentifier "CustomerId" (customerIdValue customer.CustomerIdentifier)
          nonEmptyField "Customer" "CustomerName" customer.CustomerName ]

let validateSupplier (supplier: Supplier) : Result<unit, SemanticValidationError> =
    firstError
        [ nonEmptyIdentifier "SupplierId" (supplierIdValue supplier.SupplierIdentifier)
          nonEmptyField "Supplier" "SupplierName" supplier.SupplierName ]

// ---------------------------------------------------------------------
// Resource and Network invariants
// ---------------------------------------------------------------------

let validateResourceGroup (resourceGroup: ResourceGroup) : Result<unit, SemanticValidationError> =
    firstError
        [ nonEmptyIdentifier "ResourceGroupId" (resourceGroupIdValue resourceGroup.ResourceGroupIdentifier)
          nonEmptyField "ResourceGroup" "ResourceGroupName" resourceGroup.ResourceGroupName
          nonEmptyIdentifier "ResourceGroup.Calendar" (calendarIdValue resourceGroup.Calendar) ]

let validateStandardResource (resource: StandardResource) : Result<unit, SemanticValidationError> =
    firstError
        [ nonEmptyIdentifier "StandardResourceId" (standardResourceIdValue resource.StandardResourceIdentifier)
          nonEmptyField "StandardResource" "StandardResourceName" resource.StandardResourceName
          validateCapacity resource.ReferenceCapacity
          nonEmptyIdentifier "StandardResource.Calendar" (calendarIdValue resource.Calendar) ]

let validatePhysicalResource (resource: PhysicalResource) : Result<unit, SemanticValidationError> =
    firstError
        [ nonEmptyIdentifier "PhysicalResourceId" (physicalResourceIdValue resource.PhysicalResourceIdentifier)
          nonEmptyField "PhysicalResource" "PhysicalResourceName" resource.PhysicalResourceName
          nonEmptyIdentifier "PhysicalResource.Location" (locationIdValue resource.Location)
          validateCapacity resource.AssignedCapacity
          nonEmptyIdentifier "PhysicalResource.Calendar" (calendarIdValue resource.Calendar) ]

let validateTransportationLane (lane: TransportationLane) : Result<unit, SemanticValidationError> =
    let originDestCheck =
        if lane.Origin = lane.Destination then
            Error(InvariantViolation("TransportationLane", "Origin and Destination cannot be the same location."))
        else
            Ok()

    firstError
        [ nonEmptyIdentifier "TransportationLaneId" (transportationLaneIdValue lane.LaneIdentifier)
          nonEmptyIdentifier "TransportationLane.Origin" (locationIdValue lane.Origin)
          nonEmptyIdentifier "TransportationLane.Destination" (locationIdValue lane.Destination)
          originDestCheck ]

let validateNetwork (network: Network) : Result<unit, SemanticValidationError> =
    let duplicateLaneCheck =
        if hasDuplicatesBy id network.TransportationLanes then
            Error(DuplicateValue("Network", "TransportationLanes"))
        else
            Ok()

    firstError
        [ nonEmptyIdentifier "NetworkId" (networkIdValue network.NetworkIdentifier)
          nonEmptyField "Network" "NetworkName" network.NetworkName
          duplicateLaneCheck ]

// ---------------------------------------------------------------------
// Planning Object invariants
// ---------------------------------------------------------------------

let validatePlanningScope (scope: PlanningScope) : Result<unit, SemanticValidationError> =
    let boundaryRuleChecks = scope.BoundaryRules |> List.map validateScopeBoundaryRule

    firstError(
        [ nonEmptyIdentifier "PlanningScopeId" (planningScopeIdValue scope.PlanningScopeIdentifier)
          nonEmptyField "PlanningScope" "ScopeName" scope.ScopeName ]
        @ boundaryRuleChecks
    )

let validateScenario (scenario: Scenario) : Result<unit, SemanticValidationError> =
    let adjustmentChecks = scenario.Adjustments |> List.map validateScenarioAdjustment

    firstError(
        [ nonEmptyIdentifier "ScenarioId" (scenarioIdValue scenario.ScenarioIdentifier)
          nonEmptyField "Scenario" "ScenarioName" scenario.ScenarioName ]
        @ adjustmentChecks
    )

let validatePlan (plan: Plan) : Result<unit, SemanticValidationError> =
    firstError
        [ nonEmptyIdentifier "PlanId" (planIdValue plan.PlanIdentifier)
          nonEmptyField "Plan" "PlanName" plan.PlanName
          nonEmptyIdentifier "Plan.PlanningScope" (planningScopeIdValue plan.PlanningScope)
          nonEmptyIdentifier "Plan.Scenario" (scenarioIdValue plan.Scenario)
          validatePlanningHorizon plan.PlanningHorizon ]

let validateCalendar (calendar: Calendar) : Result<unit, SemanticValidationError> =
    firstError
        [ nonEmptyIdentifier "CalendarId" (calendarIdValue calendar.CalendarIdentifier)
          nonEmptyField "Calendar" "CalendarName" calendar.CalendarName
          nonEmptyField "Calendar" "CalendarDefinitionReference" calendar.CalendarDefinitionReference
          nonNegativeInt "Calendar" "VersionNumber" calendar.VersionNumber ]

let validatePlanningPeriod (period: PlanningPeriod) : Result<unit, SemanticValidationError> =
    firstError
        [ nonEmptyIdentifier "PlanningPeriodId" (planningPeriodIdValue period.PlanningPeriodIdentifier)
          nonEmptyField "PlanningPeriod" "DisplayName" period.DisplayName ]

// ---------------------------------------------------------------------
// Enterprise Fact invariants
// ---------------------------------------------------------------------

let validateDemand (demand: Demand) : Result<unit, SemanticValidationError> =
    firstError
        [ nonEmptyIdentifier "DemandId" (demandIdValue demand.DemandIdentifier)
          positiveQuantity "Demand.Quantity" demand.Quantity
          validateNeedWindow demand.NeedWindow ]

let validateSupply (supply: Supply) : Result<unit, SemanticValidationError> =
    firstError
        [ nonEmptyIdentifier "SupplyId" (supplyIdValue supply.SupplyIdentifier)
          nonNegativeQuantity "Supply.Quantity" supply.Quantity
          validateTemporalWindow supply.AvailabilityWindow
          nonEmptyIdentifier "Supply.Provenance" (vocabularyEntryIdValue supply.Provenance) ]

let validateInventory (inventory: Inventory) : Result<unit, SemanticValidationError> =
    firstError
        [ nonEmptyIdentifier "InventoryIdentity.Batch" (batchIdentifierValue inventory.Identity.Batch)
          nonNegativeQuantity "Inventory.OnHandQuantity" inventory.OnHandQuantity ]

// ---------------------------------------------------------------------
// Obligation invariants
// ---------------------------------------------------------------------

let validateCommitment (commitment: Commitment) : Result<unit, SemanticValidationError> =
    let counterpartyCheck =
        if commitment.Customer.IsNone && commitment.Supplier.IsNone then
            Error(
                InvariantViolation(
                    "Commitment",
                    "A Commitment requires at least one counterparty: Customer or Supplier."
                )
            )
        else
            Ok()

    let dateCheck =
        match commitment.CommittedDate with
        | Some committed when Timestamp.isBefore committed commitment.RequestedDate ->
            Error(InvariantViolation("Commitment", "CommittedDate must not be before RequestedDate."))
        | _ -> Ok()

    firstError
        [ nonEmptyIdentifier "CommitmentId" (commitmentIdValue commitment.CommitmentIdentifier)
          positiveQuantity "Commitment.Quantity" commitment.Quantity
          counterpartyCheck
          dateCheck ]

// ---------------------------------------------------------------------
// BOM and Risk invariants
// ---------------------------------------------------------------------

let validateBomLine (line: BomLine) : Result<unit, SemanticValidationError> =
    firstError
        [ nonEmptyIdentifier "BomLine.LineIdentifier" (lineIdentifierValue line.LineIdentifier)
          positiveQuantity "BomLine.QuantityPerParent" line.QuantityPerParent ]

let validateBillOfMaterials (bom: BillOfMaterials) : Result<unit, SemanticValidationError> =
    let lineChecks = bom.Lines |> List.map validateBomLine

    let duplicateComponentCheck =
        if hasDuplicatesBy (fun line -> line.ComponentItem) bom.Lines then
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

    firstError(
        [ nonEmptyIdentifier "BomVersionId" (bomVersionIdValue bom.BomVersionIdentifier)
          nonNegativeInt "BillOfMaterials" "VersionNumber" bom.VersionNumber
          duplicateComponentCheck
          selfReferenceCheck
          dateCheck ]
        @ lineChecks
    )

let validateRisk (risk: Risk) : Result<unit, SemanticValidationError> =
    firstError
        [ nonEmptyIdentifier "RiskId" (riskIdValue risk.RiskIdentifier)
          nonEmptyIdentifier "Risk.RiskType" (vocabularyEntryIdValue risk.RiskType)
          nonEmptyIdentifier "Risk.RiskSubjectType" (vocabularyEntryIdValue risk.RiskSubjectType)
          nonEmptyField "Risk" "RiskSubjectIdentifier" risk.RiskSubjectIdentifier ]

// ---------------------------------------------------------------------
// Core Intelligence invariants
// ---------------------------------------------------------------------

let validateException (exceptionObject: Medhavi.SemanticModel.Exception) : Result<unit, SemanticValidationError> =
    firstError
        [ nonEmptyIdentifier "ExceptionId" (exceptionIdValue exceptionObject.ExceptionIdentifier)
          nonEmptyField "Exception" "ConstraintReference" exceptionObject.ConstraintReference
          nonEmptyIdentifier "Exception.Classification" (vocabularyEntryIdValue exceptionObject.Classification)
          nonEmptyIdentifier "Exception.AffectedScopeType" (vocabularyEntryIdValue exceptionObject.AffectedScopeType)
          nonEmptyField "Exception" "AffectedScopeIdentifier" exceptionObject.AffectedScopeIdentifier ]

let validatePictureVersion (version: PictureVersion) : Result<unit, SemanticValidationError> =
    let duplicateDemandCheck =
        if hasDuplicatesBy id version.DemandReferences then
            Error(DuplicateValue("PictureVersion", "DemandReferences"))
        else
            Ok()

    let duplicateSupplyCheck =
        if hasDuplicatesBy id version.SupplyReferences then
            Error(DuplicateValue("PictureVersion", "SupplyReferences"))
        else
            Ok()

    let duplicateInventoryCheck =
        if hasDuplicatesBy id version.InventoryReferences then
            Error(DuplicateValue("PictureVersion", "InventoryReferences"))
        else
            Ok()

    firstError [ duplicateDemandCheck; duplicateSupplyCheck; duplicateInventoryCheck ]

let validateEnterprisePicture (picture: EnterprisePicture) : Result<unit, SemanticValidationError> =
    let versionChecks = picture.Versions |> List.map validatePictureVersion

    let duplicateVersionCheck =
        if hasDuplicatesBy (fun (version: PictureVersion) -> version.VersionNumber) picture.Versions then
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

    firstError(
        [ nonEmptyIdentifier "PlanningScopeId" (planningScopeIdValue picture.PlanningScopeIdentifier)
          duplicateVersionCheck
          publishedCheck ]
        @ versionChecks
    )

// ---------------------------------------------------------------------
// Governance catalog invariants
// ---------------------------------------------------------------------

let validateVocabularyEntry (entry: VocabularyEntry) : Result<unit, SemanticValidationError> =
    firstError
        [ nonEmptyIdentifier "VocabularyCategoryIdentifier" (vocabularyEntryIdValue entry.VocabularyCategoryIdentifier)
          nonEmptyIdentifier "VocabularyEntryId" (vocabularyEntryIdValue entry.EntryIdentifier)
          nonEmptyField "VocabularyEntry" "EntryName" entry.EntryName ]

let validateEnterpriseGovernedVocabulary
    (vocabulary: EnterpriseGovernedVocabulary)
    : Result<unit, SemanticValidationError> =
    let entryChecks = vocabulary.Entries |> List.map validateVocabularyEntry

    let duplicateEntryIdentifierCheck =
        if hasDuplicatesBy (fun entry -> entry.EntryIdentifier) vocabulary.Entries then
            Error(DuplicateValue("EnterpriseGovernedVocabulary", "Entries.EntryIdentifier"))
        else
            Ok()

    firstError(
        [ nonEmptyField "EnterpriseGovernedVocabulary" "CatalogIdentifier" vocabulary.CatalogIdentifier
          nonNegativeInt "EnterpriseGovernedVocabulary" "VersionNumber" vocabulary.VersionNumber
          duplicateEntryIdentifierCheck ]
        @ entryChecks
    )

let validatePerformanceIndicator (indicator: PerformanceIndicator) : Result<unit, SemanticValidationError> =
    firstError
        [ nonEmptyIdentifier "PerformanceIndicator.IndicatorIdentifier" indicator.IndicatorIdentifier
          nonEmptyField "PerformanceIndicator" "Name" indicator.Name
          nonEmptyIdentifier "PerformanceIndicator.MeasureCategory" (vocabularyEntryIdValue indicator.MeasureCategory)
          nonEmptyIdentifier "PerformanceIndicator.MeasureNature" (vocabularyEntryIdValue indicator.MeasureNature)
          nonEmptyField "PerformanceIndicator" "FormulaReference" indicator.FormulaReference
          noEmptyStrings "PerformanceIndicator" "SemanticDependencies" indicator.SemanticDependencies ]

let validatePerformanceIndicatorCatalog (catalog: PerformanceIndicatorCatalog) : Result<unit, SemanticValidationError> =
    let indicatorChecks = catalog.Indicators |> List.map validatePerformanceIndicator

    let duplicateIndicatorCheck =
        if hasDuplicatesBy (fun indicator -> indicator.IndicatorIdentifier) catalog.Indicators then
            Error(DuplicateValue("PerformanceIndicatorCatalog", "Indicators.IndicatorIdentifier"))
        else
            Ok()

    firstError(
        [ nonEmptyField "PerformanceIndicatorCatalog" "CatalogIdentifier" catalog.CatalogIdentifier
          nonNegativeInt "PerformanceIndicatorCatalog" "VersionNumber" catalog.VersionNumber
          duplicateIndicatorCheck ]
        @ indicatorChecks
    )

/// SE-C-040 Item Transition invariants
let validateItemTransition (transition: ItemTransition) : Result<unit, SemanticValidationError> =

    let distinctItemsCheck =
        if transition.SupersededItem = transition.SupersedingItem then
            Error(
                InvariantViolation(
                    "ItemTransition",
                    "Superseded Item and Superseding Item must reference distinct items."
                )
            )
        else
            Ok()

    let effectiveDateCheck =
        if Timestamp.value transition.EffectiveDate = DateTimeOffset.MinValue then
            Error(InvariantViolation("ItemTransition", "Effective Date must be a valid UTC timestamp."))
        else
            Ok()

    let endDateCheck =
        match transition.EndDate with
        | Some endDate ->
            if Timestamp.isBefore endDate transition.EffectiveDate then
                Error(InvariantViolation("ItemTransition", "End Date must be after Effective Date."))
            else
                Ok()
        | None -> Ok()

    firstError [ distinctItemsCheck; effectiveDateCheck; endDateCheck ]

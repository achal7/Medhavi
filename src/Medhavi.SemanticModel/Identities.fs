namespace Medhavi.SemanticModel

/// Enterprise-wide immutable identifiers.
/// Each is a wrapped string to ensure type safety and prevent accidental mixing of IDs.
type ItemId = private ItemId of string
type LocationId = private LocationId of string
type CustomerId = private CustomerId of string
type SupplierId = private SupplierId of string
type ResourceGroupId = private ResourceGroupId of string
type StandardResourceId = private StandardResourceId of string
type PhysicalResourceId = private PhysicalResourceId of string
type TransportationLaneId = private TransportationLaneId of string
type NetworkId = private NetworkId of string
type PlanningScopeId = private PlanningScopeId of string
type ScenarioId = private ScenarioId of string
type PlanId = private PlanId of string
type DemandId = private DemandId of string
type SupplyId = private SupplyId of string
type CommitmentId = private CommitmentId of string
type BomVersionId = private BomVersionId of string
type LineIdentifier = private LineIdentifier of string
type ExceptionId = private ExceptionId of string
type RiskId = private RiskId of string
type PictureVersionId = private PictureVersionId of int
type VocabularyEntryId = private VocabularyEntryId of string
type CalendarId = private CalendarId of string
type CalendarDefinitionId = private CalendarDefinitionId of string
type PlanningPeriodId = private PlanningPeriodId of string
type UnitOfMeasureId = private UnitOfMeasureId of string
type TimeZoneId = private TimeZoneId of string
type BatchIdentifier = private BatchIdentifier of string

/// Inventory is identified by Item + Location + Batch Identifier.
/// This is a composite identity, not a surrogate ID.
type InventoryIdentity =
    { Item: ItemId
      Location: LocationId
      Batch: BatchIdentifier }

module Identities =
    let inline private createStringId
        (ctor: string -> 'Id)
        (fieldName: string)
        (value: string)
        : Result<'Id, SemanticValidationError> =
        if System.String.IsNullOrWhiteSpace value then
            Error(EmptyIdentifier fieldName)
        else
            Ok(ctor value)

    // Item
    let itemIdCreate = createStringId ItemId "ItemId"
    let itemIdValue (ItemId id) = id

    // Location
    let locationIdCreate = createStringId LocationId "LocationId"
    let locationIdValue (LocationId id) = id

    // Customer
    let customerIdCreate = createStringId CustomerId "CustomerId"
    let customerIdValue (CustomerId id) = id

    // Supplier
    let supplierIdCreate = createStringId SupplierId "SupplierId"
    let supplierIdValue (SupplierId id) = id

    // Planning Scope
    let planningScopeIdCreate = createStringId PlanningScopeId "PlanningScopeId"
    let planningScopeIdValue (PlanningScopeId id) = id

    // Demand
    let demandIdCreate = createStringId DemandId "DemandId"
    let demandIdValue (DemandId id) = id

    // Supply
    let supplyIdCreate = createStringId SupplyId "SupplyId"
    let supplyIdValue (SupplyId id) = id

    // Commitment
    let commitmentIdCreate = createStringId CommitmentId "CommitmentId"
    let commitmentIdValue (CommitmentId id) = id

    // Exception
    let exceptionIdCreate = createStringId ExceptionId "ExceptionId"
    let exceptionIdValue (ExceptionId id) = id

    // Picture Version
    let pictureVersionIdCreate (version: int) : Result<PictureVersionId, SemanticValidationError> =
        if version < 0 then
            Error(InvariantViolation("PictureVersionId", "Picture version must be non-negative."))
        else
            Ok(PictureVersionId version)

    let pictureVersionIdValue (PictureVersionId version) = version

    // Vocabulary Entry
    let vocabularyEntryIdCreate = createStringId VocabularyEntryId "VocabularyEntryId"
    let vocabularyEntryIdValue (VocabularyEntryId id) = id

    // Unit of Measure
    let unitOfMeasureIdCreate = createStringId UnitOfMeasureId "UnitOfMeasureId"
    let unitOfMeasureIdValue (UnitOfMeasureId id) = id

    // Time Zone
    let timeZoneIdCreate = createStringId TimeZoneId "TimeZoneId"
    let timeZoneIdValue (TimeZoneId id) = id

    // Batch Identifier
    let batchIdentifierCreate = createStringId BatchIdentifier "BatchIdentifier"
    let batchIdentifierValue (BatchIdentifier id) = id

    // Resource Group
    let resourceGroupIdCreate = createStringId ResourceGroupId "ResourceGroupId"
    let resourceGroupIdValue (ResourceGroupId id) = id

    // Standard Resource
    let standardResourceIdCreate = createStringId StandardResourceId "StandardResourceId"
    let standardResourceIdValue (StandardResourceId id) = id

    // Physical Resource
    let physicalResourceIdCreate = createStringId PhysicalResourceId "PhysicalResourceId"
    let physicalResourceIdValue (PhysicalResourceId id) = id

    // Transportation Lane
    let transportationLaneIdCreate = createStringId TransportationLaneId "TransportationLaneId"
    let transportationLaneIdValue (TransportationLaneId id) = id

    // Network
    let networkIdCreate = createStringId NetworkId "NetworkId"
    let networkIdValue (NetworkId id) = id

    // Scenario
    let scenarioIdCreate = createStringId ScenarioId "ScenarioId"
    let scenarioIdValue (ScenarioId id) = id

    // Plan
    let planIdCreate = createStringId PlanId "PlanId"
    let planIdValue (PlanId id) = id

    // Calendar
    let calendarIdCreate = createStringId CalendarId "CalendarId"
    let calendarIdValue (CalendarId id) = id

    // Calendar Definition
    let calendarDefinitionIdCreate = createStringId CalendarDefinitionId "CalendarDefinitionId"
    let calendarDefinitionIdValue (CalendarDefinitionId id) = id

    // Planning Period
    let planningPeriodIdCreate = createStringId PlanningPeriodId "PlanningPeriodId"
    let planningPeriodIdValue (PlanningPeriodId id) = id

    // BOM Version
    let bomVersionIdCreate = createStringId BomVersionId "BomVersionId"
    let bomVersionIdValue (BomVersionId id) = id

    // Line Identifier
    let lineIdentifierCreate = createStringId LineIdentifier "LineIdentifier"
    let lineIdentifierValue (LineIdentifier id) = id

    // Risk
    let riskIdCreate = createStringId RiskId "RiskId"
    let riskIdValue (RiskId id) = id

    // Inventory composite identity
    module InventoryIdentity =
        let create
            (item: ItemId)
            (location: LocationId)
            (batch: BatchIdentifier)
            : Result<InventoryIdentity, SemanticValidationError> =

            if System.String.IsNullOrWhiteSpace(batchIdentifierValue batch) then
                Error(InvalidCompositeIdentity "InventoryIdentity requires a non-empty BatchIdentifier.")
            else
                Ok
                    { Item = item
                      Location = location
                      Batch = batch }

        let item (identity: InventoryIdentity) = identity.Item
        let location (identity: InventoryIdentity) = identity.Location
        let batch (identity: InventoryIdentity) = identity.Batch

        // parse from string value
        let parse (identityRef: string) =
            let parts = identityRef.Split('-')

            if parts.Length <> 3 then
                Error(InvalidCompositeIdentity "InventoryIdentity requires a non-empty BatchIdentifier.")
            else
                itemIdCreate parts.[0]
                |> Result.bind(fun item ->
                    locationIdCreate parts.[1]
                    |> Result.bind(fun location ->
                        batchIdentifierCreate parts.[2] |> Result.bind(fun batch -> create item location batch)))

        let toString (inventoryIdentity: InventoryIdentity) =
            sprintf
                "%s-%s-%s"
                (inventoryIdentity.Item |> itemIdValue)
                (inventoryIdentity.Location |> locationIdValue)
                (inventoryIdentity.Batch |> batchIdentifierValue)

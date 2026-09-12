namespace Medhavi.Client.SemanticModel

open System

// ============================================================================
// I1 — Identities, shared value objects, Component/Schema definitions
// ============================================================================

[<Struct>]
type ComponentDefId = private ComponentDefId of string

module ComponentDefId =
    let create value =
        if String.IsNullOrWhiteSpace value then
            Error "Component definition id must not be empty"
        else
            Ok(ComponentDefId value)

    let value (ComponentDefId value) = value

    let literal value =
        match create value with
        | Ok id -> id
        | Error error -> raise(ArgumentException error)

[<Struct>]
type SchemaDefId = private SchemaDefId of string

module SchemaDefId =
    let create value =
        if String.IsNullOrWhiteSpace value then
            Error "Schema definition id must not be empty"
        else
            Ok(SchemaDefId value)

    let value (SchemaDefId value) = value

    let literal value =
        match create value with
        | Ok id -> id
        | Error error -> raise(ArgumentException error)

[<Struct>]
type DataStoreId = private DataStoreId of string

module DataStoreId =
    let create value =
        if String.IsNullOrWhiteSpace value then
            Error "Data store id must not be empty"
        else
            Ok(DataStoreId value)

    let value (DataStoreId value) = value

    let literal value =
        match create value with
        | Ok id -> id
        | Error error -> raise(ArgumentException error)

[<Struct>]
type ViewDefId = private ViewDefId of string

module ViewDefId =
    let create value =
        if String.IsNullOrWhiteSpace value then
            Error "View definition id must not be empty"
        else
            Ok(ViewDefId value)

    let value (ViewDefId value) = value

    let literal value =
        match create value with
        | Ok id -> id
        | Error error -> raise(ArgumentException error)

[<Struct>]
type ViewComponentId = private ViewComponentId of string

module ViewComponentId =
    let create value =
        if String.IsNullOrWhiteSpace value then
            Error "View component id must not be empty"
        else
            Ok(ViewComponentId value)

    let value (ViewComponentId value) = value

    let literal value =
        match create value with
        | Ok id -> id
        | Error error -> raise(ArgumentException error)

[<Struct>]
type TabId = private TabId of string

module TabId =
    let create value =
        if String.IsNullOrWhiteSpace value then
            Error "Tab id must not be empty"
        else
            Ok(TabId value)

    let value (TabId value) = value

    let literal value =
        match create value with
        | Ok id -> id
        | Error error -> raise(ArgumentException error)

[<Struct>]
type ViewSessionId = private ViewSessionId of string

module ViewSessionId =
    let create value =
        if String.IsNullOrWhiteSpace value then
            Error "View session id must not be empty"
        else
            Ok(ViewSessionId value)

    let value (ViewSessionId value) = value

    let literal value =
        match create value with
        | Ok id -> id
        | Error error -> raise(ArgumentException error)

[<Struct>]
type ScenarioId = private ScenarioId of string

module ScenarioId =
    let create value =
        if String.IsNullOrWhiteSpace value then
            Error "Scenario id must not be empty"
        else
            Ok(ScenarioId value)

    let value (ScenarioId value) = value

    let literal value =
        match create value with
        | Ok id -> id
        | Error error -> raise(ArgumentException error)

    let baseline = literal "baseline"

[<Struct>]
type ActionDefId = private ActionDefId of string

module ActionDefId =
    let create value =
        if String.IsNullOrWhiteSpace value then
            Error "Action definition id must not be empty"
        else
            Ok(ActionDefId value)

    let value (ActionDefId value) = value

    let literal value =
        match create value with
        | Ok id -> id
        | Error error -> raise(ArgumentException error)

type ActionScope =
    | GlobalScope
    | SingleEntityScope
    | MultiEntityScope

type ActionExecutionMode =
    | Direct
    | RequiresConfirmation of prompt: string
    | Parameterized of schemaId: string * title: string

type ValueObjectError =
    | EmptyIdentity of string
    | InvalidGridDimensionsValue of columns: int * rows: int
    | InvalidGridSpanValue of colSpan: int * rowSpan: int
    | InvalidTilePositionValue of col: int * row: int * colSpan: int * rowSpan: int
    | InvalidSplitRatioValue of decimal

[<Struct>]
type SplitRatio = private SplitRatio of decimal

module SplitRatio =
    let create value =
        if value <= 0.0m || value >= 1.0m then
            Error(InvalidSplitRatioValue value)
        else
            Ok(SplitRatio value)

    let value (SplitRatio value) = value

    let literal value =
        match create value with
        | Ok ratio -> ratio
        | Error error -> raise(ArgumentException(sprintf "%A" error))

[<Struct>]
type GridDimensions = private GridDimensions of int * int

module GridDimensions =
    let create columns rows =
        if columns < 1 || rows < 1 then
            Error(InvalidGridDimensionsValue(columns, rows))
        else
            Ok(GridDimensions(columns, rows))

    let columns (GridDimensions(columns, _)) = columns
    let rows (GridDimensions(_, rows)) = rows

    let literal columns rows =
        match create columns rows with
        | Ok dimensions -> dimensions
        | Error error -> raise(ArgumentException(sprintf "%A" error))

    let defaultDimensions = literal 12 12

[<Struct>]
type GridSpan = private GridSpan of int * int

module GridSpan =
    let create colSpan rowSpan =
        if colSpan < 1 || rowSpan < 1 then
            Error(InvalidGridSpanValue(colSpan, rowSpan))
        else
            Ok(GridSpan(colSpan, rowSpan))

    let colSpan (GridSpan(colSpan, _)) = colSpan
    let rowSpan (GridSpan(_, rowSpan)) = rowSpan

    let literal colSpan rowSpan =
        match create colSpan rowSpan with
        | Ok span -> span
        | Error error -> raise(ArgumentException(sprintf "%A" error))

[<Struct>]
type TilePosition = private TilePosition of int * int * int * int

module TilePosition =
    let create col row colSpan rowSpan =
        if col < 1 || row < 1 || colSpan < 1 || rowSpan < 1 then
            Error(InvalidTilePositionValue(col, row, colSpan, rowSpan))
        else
            Ok(TilePosition(col, row, colSpan, rowSpan))

    let col (TilePosition(col, _, _, _)) = col
    let row (TilePosition(_, row, _, _)) = row
    let colSpan (TilePosition(_, _, colSpan, _)) = colSpan
    let rowSpan (TilePosition(_, _, _, rowSpan)) = rowSpan

    let literal col row colSpan rowSpan =
        match create col row colSpan rowSpan with
        | Ok position -> position
        | Error error -> raise(ArgumentException(sprintf "%A" error))

type FilterOperator =
    | Equals
    | NotEquals
    | GreaterThan
    | GreaterThanOrEqual
    | LessThan
    | LessThanOrEqual
    | Contains
    | IsEmpty
    | IsNotEmpty

type FilterCriterion =
    { Field: string
      Operator: FilterOperator
      Value: string option }

type SortDirection =
    | Ascending
    | Descending

type SortCriterion =
    { Field: string
      Direction: SortDirection
      Priority: int }

type FilterValueKind =
    | Text
    | DateTimeOffset
    | Number
    | Boolean
    | Enum of string list

type DateRange =
    { Start: DateTimeOffset
      End: DateTimeOffset }

type PlanningContext =
    { Scenario: ScenarioId
      Horizon: DateRange
      PlantId: string option }

type SharedContext =
    { Planning: PlanningContext
      Filters: FilterCriterion list }

type ComponentCategory =
    | DataDisplay
    | Visualization
    | Input
    | Layout
    | Navigation
    | Utility

type ParameterValue =
    | StringValue of string
    | IntValue of int
    | DecimalValue of decimal
    | BooleanValue of bool
    | DateTimeValue of DateTimeOffset
    | StringListValue of string list
    | IntListValue of int list

type ActionPayload =
    | NoPayload
    | QuantityValue of decimal
    | PriorityValue of int
    | DateValue of DateTimeOffset
    | ReasonedAction of reasonCode: string * comment: string option
    | CustomParameters of Map<string, ParameterValue>

type ActionDescriptor =
    { Id: ActionDefId
      DisplayName: string
      Icon: string
      Scope: ActionScope
      ExecutionMode: ActionExecutionMode
      RequiredRoles: string list
      IsPrimary: bool }

type ComponentDefinition =
    { Id: ComponentDefId
      DisplayName: string
      Icon: string
      Category: ComponentCategory
      RequiredSchemaCapabilities: string list
      DefaultGridSpan: GridSpan
      DefaultSchemaId: SchemaDefId option
      DefaultStoreId: DataStoreId option
      IsResizable: bool
      IsDraggable: bool }

type SchemaColumnDefinition =
    { ColumnId: string
      PropertyName: string option
      Title: string
      DefaultOrder: int
      DefaultWidthInPixels: int option
      DefaultVisible: bool
      DefaultPinned: bool
      Sortable: bool
      FilterValueKind: FilterValueKind option
      UserConfigurable: bool }

type SchemaDefinition =
    { Id: SchemaDefId
      DisplayName: string
      Capabilities: string list
      SupportedActions: ActionDefId list
      Columns: SchemaColumnDefinition list
      DefaultSort: SortCriterion option
      DefaultFilters: FilterCriterion list }

type SplitDirection =
    | Horizontal
    | Vertical

type LayoutTopology =
    | EmptyTopology
    | SingleTopology
    | SplitTopology
    | DashboardTopology
    | TabbedTopology

type LayoutError =
    | InvalidSplitRatio of attempted: decimal
    | InvalidGridDimensions of GridDimensions
    | InvalidTileBounds of ViewComponentId * TilePosition * GridDimensions
    | OverlappingTiles of ViewComponentId * ViewComponentId
    | EmptySplitBranch of SplitDirection
    | EmptyTabList
    | EmptyTabContent of TabId
    | DuplicateTabId of TabId
    | DuplicateLayoutOccurrence of ViewComponentId
    | ComponentNotFoundInLayout of ViewComponentId
    | TopologyMismatch of expected: LayoutTopology * actual: LayoutTopology

type ViewVisibility =
    | Private
    | Team
    | Public
    | Restricted

type ViewPermissions =
    { Owner: string
      Visibility: ViewVisibility
      EditableBy: string list
      Shareable: bool }

type ViewMetadata =
    { Category: string
      Icon: string
      Roles: string list
      IsDefault: bool
      IsTemplate: bool
      CreatedAt: DateTimeOffset
      CreatedBy: string
      ModifiedAt: DateTimeOffset
      ModifiedBy: string
      Permissions: ViewPermissions }

type DataBinding =
    { StoreId: DataStoreId
      SchemaDefId: SchemaDefId }

type ComponentConfiguration =
    { LocalFilters: FilterCriterion list
      Parameters: Map<string, ParameterValue> }

type SchemaBinding = DataBinding

// ============================================================================
// Placement policy value objects and two-tier failure model
// ============================================================================

/// Everything the placement policy needs to decide. Pure geometry only.
type PlacementContext =
    { Dimensions: GridDimensions
      Occupied: TilePosition list
      Span: GridSpan }

/// Product-facing placement policy outcomes.
type PlacementError =
    | SpanExceedsGridDimensions of span: GridSpan * dimensions: GridDimensions
    | NoAvailablePosition of span: GridSpan * dimensions: GridDimensions
    | UnsupportedTopology of LayoutTopology

type PlacementInternalBug = CandidateViolatesPositionInvariant of col: int * row: int * colSpan: int * rowSpan: int

type PlacementFailure =
    | Policy of PlacementError
    | InternalBug of PlacementInternalBug

type ViewComponent =
    { ComponentDefId: ComponentDefId
      Binding: SchemaBinding
      Configuration: ComponentConfiguration }

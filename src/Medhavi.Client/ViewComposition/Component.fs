namespace Medhavi.Client.ViewComposition.Component

open Medhavi.Client.ViewComposition.Identity
open Medhavi.Client.ViewComposition.Context
open Medhavi.Client.ViewComposition.Geometry

type ParameterValue =
    | StringValue of string
    | IntValue of int
    | DecimalValue of decimal
    | BooleanValue of bool
    | DateTimeValue of System.DateTimeOffset
    | StringListValue of string list
    | IntListValue of int list

type ActionScope =
    | GlobalScope
    | SingleEntityScope
    | MultiEntityScope

type ActionExecutionMode =
    | Direct
    | RequiresConfirmation of prompt: string
    | Parameterized of schemaId: string * title: string

type ActionPayload =
    | NoPayload
    | QuantityValue of decimal
    | PriorityValue of int
    | DateValue of System.DateTimeOffset
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

type ComponentCategory =
    | DataDisplay
    | Visualization
    | Input
    | Layout
    | Navigation
    | Utility

type SelectionMode =
    | SingleSelection
    | MultiSelection
    | NoSelection

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
      IsDraggable: bool
      SelectionMode: SelectionMode }

type ComponentConfiguration =
    { LocalFilters: FilterCriterion list
      Parameters: Map<string, ParameterValue> }

// Moved here to prevent circular dependency with Schema.fs
type DataBinding =
    { StoreId: DataStoreId
      SchemaDefId: SchemaDefId }

type SchemaBinding = DataBinding

type ViewComponent =
    { ComponentDefId: ComponentDefId
      Binding: SchemaBinding
      Configuration: ComponentConfiguration }

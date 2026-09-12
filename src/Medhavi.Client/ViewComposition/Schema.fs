namespace Medhavi.Client.ViewComposition.Schema

open Medhavi.Common.Validation
open Medhavi.Client.ViewComposition.Identity
open Medhavi.Client.ViewComposition.Context
open Medhavi.Client.ViewComposition.Component

type TextAlignment =
    | Left
    | Center
    | Right

type CellType =
    | TextType
    | NumberType
    | BooleanType
    | DateTimeType
    | EnumType of string list
    | ReferenceType

type SchemaColumnDefinition =
    { ColumnId: string
      PropertyName: string option
      Title: string
      DefaultOrder: int
      DefaultWidthInPixels: int option
      DefaultVisible: bool
      DefaultPinned: bool
      Sortable: bool
      Filterable: bool
      Groupable: bool
      FilterValueKind: FilterValueKind option
      UserConfigurable: bool
      RenderHint: string option
      Alignment: TextAlignment
      CellType: CellType
      IsKey: bool
      FormatString: string option }

type SchemaDefinition =
    { Id: SchemaDefId
      DisplayName: string
      Capabilities: string list
      SupportedActions: ActionDefId list
      Columns: SchemaColumnDefinition list
      DefaultSort: SortCriterion option
      DefaultFilters: FilterCriterion list }

type CompatibilityError =
    | UnknownRequiredCapability of capability: string
    | MissingCapability of required: string * provided: string list

module SchemaCompatibility =

    let known: Set<string> = Set.ofList [ "Tabular"; "TimeSeries"; "Kpi"; "Detail" ]

    let validate (comp: ComponentDefinition) (schema: SchemaDefinition) : Validation<unit, CompatibilityError> =
        let unknownRequired =
            comp.RequiredSchemaCapabilities
            |> List.filter(fun c -> not(Set.contains c known))
            |> List.map UnknownRequiredCapability

        let missing =
            comp.RequiredSchemaCapabilities
            |> List.filter(fun c -> Set.contains c known)
            |> List.filter(fun c -> not(List.contains c schema.Capabilities))
            |> List.map(fun c -> MissingCapability(c, schema.Capabilities))

        match unknownRequired @ missing with
        | [] -> Valid()
        | errors -> Invalid errors

type SchemaValidationError =
    | DuplicateColumnId of columnId: string
    | EmptyColumnId
    | DuplicateDefaultOrder of order: int
    | DuplicatePropertyName of propertyName: string
    | NoKeyColumn
    | MultipleKeyColumns of columnIds: string list
    | KeyColumnMissingPropertyName of columnId: string

module SchemaValidation =
    let validate (schema: SchemaDefinition) : Validation<SchemaDefinition, SchemaValidationError> =
        let columns = schema.Columns

        let emptyIdErrors =
            columns
            |> List.filter(fun c -> System.String.IsNullOrWhiteSpace c.ColumnId)
            |> List.map(fun _ -> EmptyColumnId)

        let duplicateIdErrors =
            columns
            |> List.map(fun c -> c.ColumnId)
            |> List.filter(fun id -> not(System.String.IsNullOrWhiteSpace id))
            |> List.groupBy id
            |> List.filter(fun (_, group) -> List.length group > 1)
            |> List.map(fun (id, _) -> DuplicateColumnId id)

        let duplicateOrderErrors =
            columns
            |> List.map(fun c -> c.DefaultOrder)
            |> List.groupBy id
            |> List.filter(fun (_, group) -> List.length group > 1)
            |> List.map(fun (order, _) -> DuplicateDefaultOrder order)

        let duplicatePropertyErrors =
            columns
            |> List.choose(fun c -> c.PropertyName)
            |> List.groupBy id
            |> List.filter(fun (_, group) -> List.length group > 1)
            |> List.map(fun (prop, _) -> DuplicatePropertyName prop)

        // Key column validation
        let keyColumns = columns |> List.filter(fun c -> c.IsKey)

        let keyErrors =
            match keyColumns with
            | [] -> [ NoKeyColumn ]
            | [ keyCol ] ->
                match keyCol.PropertyName with
                | None -> [ KeyColumnMissingPropertyName keyCol.ColumnId ]
                | Some _ -> []
            | _ -> [ MultipleKeyColumns(keyColumns |> List.map(fun c -> c.ColumnId)) ]

        match emptyIdErrors @ duplicateIdErrors @ duplicateOrderErrors @ duplicatePropertyErrors @ keyErrors with
        | [] -> Valid schema
        | errors -> Invalid errors

type ComponentBinding =
    { Definition: ComponentDefinition
      Schema: SchemaDefinition
      Store: DataStoreId
      Configuration: ComponentConfiguration }

module ComponentBinding =
    let create
        (def: ComponentDefinition)
        (schema: SchemaDefinition)
        (store: DataStoreId)
        (config: ComponentConfiguration)
        : Validation<ComponentBinding, CompatibilityError> =
        SchemaCompatibility.validate def schema
        |> map(fun () ->
            { Definition = def
              Schema = schema
              Store = store
              Configuration = config })

    let toViewComponent (b: ComponentBinding) : ViewComponent =
        { ComponentDefId = b.Definition.Id
          Binding =
            { StoreId = b.Store
              SchemaDefId = b.Schema.Id }
          Configuration = b.Configuration }

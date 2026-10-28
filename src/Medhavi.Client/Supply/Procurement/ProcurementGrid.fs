module Medhavi.Client.Supply.Procurement.ProcurementGrid

open System
open Medhavi.Contracts.Supply
open Medhavi.Client.ViewComposition.Identity
open Medhavi.Client.ViewComposition.Geometry
open Medhavi.Client.ViewComposition.Component
open Medhavi.Client.ViewComposition.Schema
open Medhavi.Client.ViewComposition.Data
open Medhavi.Client.ViewComposition.Context
open Medhavi.Client.Registries
open Medhavi.Client.Controls
open Medhavi.Client.Rendering
open Medhavi.Common.Validation

/// Declarative metadata descriptors for Procurement actions.
module ProcurementActions =

    let createRecommendation: ActionDescriptor =
        { Id = ActionDefId.literal "procurement.create_recommendation"
          DisplayName = "Create Recommendation"
          Icon = "add_shopping_cart"
          Scope = GlobalScope
          ExecutionMode = Direct
          RequiredRoles = [ "Planner"; "Buyer" ]
          IsPrimary = true }

    let publishPlan: ActionDescriptor =
        { Id = ActionDefId.literal "procurement.publish_plan"
          DisplayName = "Publish Procurement Plan"
          Icon = "publish"
          Scope = GlobalScope
          ExecutionMode = RequiresConfirmation "Publish procurement plan baseline to enterprise picture?"
          RequiredRoles = [ "Planner"; "ProcurementManager" ]
          IsPrimary = false }

    let all: ActionDescriptor list = [ createRecommendation; publishPlan ]

let createRecommendation = ProcurementActions.createRecommendation
let publishPlan = ProcurementActions.publishPlan
let allActions = ProcurementActions.all

let storeId = DataStoreId.literal "procurement-recommendation-store"
let schemaId = SchemaDefId.literal "procurement-grid-schema"
let definitionId = ComponentDefId.literal "comp-procurement-recommendations"

let schema: SchemaDefinition =
    { Id = schemaId
      DisplayName = "Procurement Recommendations"
      Capabilities = [ "Tabular" ]
      SupportedActions = ProcurementActions.all |> List.map (fun a -> a.Id)
      Actions = ProcurementActions.all
      Columns =
        [ { ColumnId = "RequirementId"
            PropertyName = Some "RequirementId"
            Title = "Requirement ID"
            DefaultOrder = 0
            DefaultWidthInPixels = Some 140
            DefaultVisible = true
            DefaultPinned = true
            Sortable = true
            FilterValueKind = Some Text
            Filterable = true
            Groupable = false
            RenderHint = None
            Alignment = TextAlignment.Left
            CellType = CellType.TextType
            IsKey = true
            FormatString = None
            UserConfigurable = false }
          { ColumnId = "VersionNumber"
            PropertyName = Some "VersionNumber"
            Title = "Version"
            DefaultOrder = 1
            DefaultWidthInPixels = Some 90
            DefaultVisible = true
            DefaultPinned = false
            Sortable = true
            FilterValueKind = Some Number
            Filterable = true
            Groupable = true
            RenderHint = None
            Alignment = TextAlignment.Right
            CellType = CellType.NumberType
            IsKey = false
            FormatString = Some "N0"
            UserConfigurable = true }
          { ColumnId = "ItemId"
            PropertyName = Some "ItemId"
            Title = "Item / SKU"
            DefaultOrder = 2
            DefaultWidthInPixels = Some 150
            DefaultVisible = true
            DefaultPinned = false
            Sortable = true
            FilterValueKind = Some Text
            Filterable = true
            Groupable = true
            RenderHint = None
            Alignment = TextAlignment.Left
            CellType = CellType.TextType
            IsKey = false
            FormatString = None
            UserConfigurable = true }
          { ColumnId = "LocationId"
            PropertyName = Some "LocationId"
            Title = "Destination Location"
            DefaultOrder = 3
            DefaultWidthInPixels = Some 140
            DefaultVisible = true
            DefaultPinned = false
            Sortable = true
            FilterValueKind = Some Text
            Filterable = true
            Groupable = true
            RenderHint = None
            Alignment = TextAlignment.Left
            CellType = CellType.TextType
            IsKey = false
            FormatString = None
            UserConfigurable = true }
          { ColumnId = "RequiredDate"
            PropertyName = Some "RequiredDate"
            Title = "Required Date"
            DefaultOrder = 4
            DefaultWidthInPixels = Some 130
            DefaultVisible = true
            DefaultPinned = false
            Sortable = true
            FilterValueKind = Some FilterValueKind.DateTimeOffset
            Filterable = true
            Groupable = false
            RenderHint = None
            Alignment = TextAlignment.Left
            CellType = CellType.DateTimeType
            IsKey = false
            FormatString = Some "yyyy-MM-dd"
            UserConfigurable = true }
          { ColumnId = "RequiredQuantity"
            PropertyName = Some "RequiredQuantity"
            Title = "Required Qty"
            DefaultOrder = 5
            DefaultWidthInPixels = Some 120
            DefaultVisible = true
            DefaultPinned = false
            Sortable = true
            FilterValueKind = Some Number
            Filterable = true
            Groupable = false
            RenderHint = None
            Alignment = TextAlignment.Right
            CellType = CellType.NumberType
            IsKey = false
            FormatString = Some "N0"
            UserConfigurable = true }
          { ColumnId = "FeasibilityStatus"
            PropertyName = Some "FeasibilityStatus"
            Title = "Feasibility"
            DefaultOrder = 6
            DefaultWidthInPixels = Some 130
            DefaultVisible = true
            DefaultPinned = false
            Sortable = true
            FilterValueKind = Some Text
            Filterable = true
            Groupable = true
            RenderHint = Some "StatusBadge"
            Alignment = TextAlignment.Left
            CellType = CellType.TextType
            IsKey = false
            FormatString = None
            UserConfigurable = true }
          { ColumnId = "SourcingStrategy"
            PropertyName = Some "SourcingStrategy"
            Title = "Sourcing Strategy"
            DefaultOrder = 7
            DefaultWidthInPixels = Some 140
            DefaultVisible = true
            DefaultPinned = false
            Sortable = true
            FilterValueKind = Some Text
            Filterable = true
            Groupable = true
            RenderHint = Some "StatusBadge"
            Alignment = TextAlignment.Left
            CellType = CellType.TextType
            IsKey = false
            FormatString = None
            UserConfigurable = true }
          { ColumnId = "LifecycleState"
            PropertyName = Some "LifecycleState"
            Title = "State"
            DefaultOrder = 8
            DefaultWidthInPixels = Some 110
            DefaultVisible = true
            DefaultPinned = false
            Sortable = true
            FilterValueKind = Some Text
            Filterable = true
            Groupable = true
            RenderHint = Some "StatusBadge"
            Alignment = TextAlignment.Left
            CellType = CellType.TextType
            IsKey = false
            FormatString = None
            UserConfigurable = true } ]
      DefaultSort = Some { Field = "RequiredDate"; Direction = Ascending; Priority = 1 }
      DefaultFilters = [] }

let definition: SchemaDefinition = schema

let adapter: ProjectionAdapter<ProcurementRecommendationDto> =
    { ToUniversal =
        fun (dto: ProcurementRecommendationDto) ->
            { Key = dto.RequirementId
              Cells =
                Map.ofList
                    [ "RequirementId", TextValue dto.RequirementId
                      "VersionNumber", NumberValue (decimal dto.VersionNumber)
                      "ItemId", TextValue dto.ItemId
                      "LocationId", TextValue dto.LocationId
                      "RequiredDate", DateTimeValue dto.RequiredDate
                      "RequiredQuantity", NumberValue dto.RequiredQuantity
                      "FeasibilityStatus", TextValue (string dto.FeasibilityStatus)
                      "SourcingStrategy", TextValue (string dto.SourcingStrategy)
                      "LifecycleState", TextValue (string dto.LifecycleState) ] } }

let componentDefinition: ComponentDefinition =
    { Id = definitionId
      DisplayName = "Procurement Recommendations"
      Icon = "shopping_cart"
      Category = ComponentCategory.DataDisplay
      RequiredSchemaCapabilities = [ "Tabular" ]
      DefaultGridSpan = GridSpan.literal 12 6
      DefaultSchemaId = Some schemaId
      DefaultStoreId = Some storeId
      IsResizable = true
      IsDraggable = true
      SelectionMode = SingleSelection }

let runtime: ComponentRuntime =
    { Render = UniversalEnterpriseGrid.render }

let registration: ComponentRegistration =
    { Definition = componentDefinition
      Schema = schema
      Runtime = runtime }

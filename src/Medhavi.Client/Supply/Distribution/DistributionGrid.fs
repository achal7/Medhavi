module Medhavi.Client.Supply.Distribution.DistributionGrid

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

/// Declarative metadata descriptors for Distribution actions.
module DistributionActions =

    let createRecommendation: ActionDescriptor =
        { Id = ActionDefId.literal "distribution.create_recommendation"
          DisplayName = "Create Distribution Plan"
          Icon = "local_shipping"
          Scope = GlobalScope
          ExecutionMode = Direct
          RequiredRoles = [ "Planner"; "LogisticsManager" ]
          IsPrimary = true }

    let publishPlan: ActionDescriptor =
        { Id = ActionDefId.literal "distribution.publish_plan"
          DisplayName = "Publish Distribution Plan"
          Icon = "publish"
          Scope = GlobalScope
          ExecutionMode = RequiresConfirmation "Publish distribution plan baseline across transport network?"
          RequiredRoles = [ "Planner"; "LogisticsManager" ]
          IsPrimary = false }

    let all: ActionDescriptor list = [ createRecommendation; publishPlan ]

let createRecommendation = DistributionActions.createRecommendation
let publishPlan = DistributionActions.publishPlan
let allActions = DistributionActions.all

let storeId = DataStoreId.literal "distribution-recommendation-store"
let schemaId = SchemaDefId.literal "distribution-grid-schema"
let definitionId = ComponentDefId.literal "comp-distribution-recommendations"

let schema: SchemaDefinition =
    { Id = schemaId
      DisplayName = "Distribution Recommendations"
      Capabilities = [ "Tabular" ]
      SupportedActions = DistributionActions.all |> List.map (fun a -> a.Id)
      Actions = DistributionActions.all
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
          { ColumnId = "DestinationLocationId"
            PropertyName = Some "DestinationLocationId"
            Title = "Destination Location"
            DefaultOrder = 3
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
          { ColumnId = "RequiredQuantity"
            PropertyName = Some "RequiredQuantity"
            Title = "Required Qty"
            DefaultOrder = 4
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
          { ColumnId = "DepartureTiming"
            PropertyName = Some "DepartureTiming"
            Title = "Departure Timing"
            DefaultOrder = 5
            DefaultWidthInPixels = Some 140
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
            FormatString = Some "yyyy-MM-dd HH:mm"
            UserConfigurable = true }
          { ColumnId = "ExpectedArrivalTiming"
            PropertyName = Some "ExpectedArrivalTiming"
            Title = "Expected Arrival"
            DefaultOrder = 6
            DefaultWidthInPixels = Some 140
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
            FormatString = Some "yyyy-MM-dd HH:mm"
            UserConfigurable = true }
          { ColumnId = "FeasibilityStatus"
            PropertyName = Some "FeasibilityStatus"
            Title = "Feasibility"
            DefaultOrder = 7
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
          { ColumnId = "StrategyType"
            PropertyName = Some "StrategyType"
            Title = "Strategy"
            DefaultOrder = 8
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
            UserConfigurable = true } ]
      DefaultSort = Some { Field = "ExpectedArrivalTiming"; Direction = Ascending; Priority = 1 }
      DefaultFilters = [] }

let definition: SchemaDefinition = schema

let adapter: ProjectionAdapter<DistributionRecommendationDto> =
    { ToUniversal =
        fun (dto: DistributionRecommendationDto) ->
            let reqId =
                if not (String.IsNullOrWhiteSpace dto.RequirementReference) then
                    dto.RequirementReference
                else
                    dto.RecommendationId
            { Key = reqId
              Cells =
                Map.ofList
                    [ "RequirementId", TextValue reqId
                      "VersionNumber", NumberValue (decimal dto.VersionNumber)
                      "ItemId", TextValue dto.ItemId
                      "DestinationLocationId", TextValue dto.DestinationLocationId
                      "RequiredQuantity", NumberValue dto.RequiredQuantity
                      "DepartureTiming", DateTimeValue dto.DepartureTiming
                      "ExpectedArrivalTiming", DateTimeValue dto.ExpectedArrivalTiming
                      "FeasibilityStatus", TextValue (string dto.FeasibilityStatus)
                      "StrategyType", TextValue (string dto.Strategy) ] } }

let componentDefinition: ComponentDefinition =
    { Id = definitionId
      DisplayName = "Distribution Recommendations"
      Icon = "local_shipping"
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

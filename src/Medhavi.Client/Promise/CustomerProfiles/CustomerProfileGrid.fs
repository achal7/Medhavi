module Medhavi.Client.Promise.CustomerProfiles.CustomerProfileGrid

open System
open Medhavi.Contracts.Promise
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
open Medhavi.Client.Components.Shared

module CustomerProfileActions =

    let configure: ActionDescriptor =
        { Id = ActionDefId.literal "customer_profile.configure"
          DisplayName = "Configure Customer Profile"
          Icon = "person_add"
          Scope = GlobalScope
          ExecutionMode = Parameterized("customer-profile-form", "Configure Customer SLA & Governance")
          RequiredRoles = [ "Planner"; "CustomerService"; "CommercialLead" ]
          IsPrimary = true }

    let updateTerms: ActionDescriptor =
        { Id = ActionDefId.literal "customer_profile.update_terms"
          DisplayName = "Update Service Terms"
          Icon = "tune"
          Scope = SingleEntityScope
          ExecutionMode = Parameterized("update-terms-form", "Adjust SLA & Split Shipment Terms")
          RequiredRoles = [ "Planner"; "CustomerService" ]
          IsPrimary = false }

    let toggleHold: ActionDescriptor =
        { Id = ActionDefId.literal "customer_profile.toggle_hold"
          DisplayName = "Toggle Credit Hold"
          Icon = "lock_reset"
          Scope = SingleEntityScope
          ExecutionMode = RequiresConfirmation "Toggle credit hold status for selected customer profile?"
          RequiredRoles = [ "CreditManager"; "Planner" ]
          IsPrimary = false }

    let all: ActionDescriptor list = [ configure; updateTerms; toggleHold ]

let schemaId = SchemaDefId.literal "customer-profile-schema"
let storeId = DataStoreId.literal "customer-profile-store"
let definitionId = ComponentDefId.literal "customer-profile-grid"

let schema: SchemaDefinition =
    { Id = schemaId
      DisplayName = "Customer Promise Profiles (SE-P-050)"
      Capabilities = [ "Tabular" ]
      SupportedActions = CustomerProfileActions.all |> List.map (fun a -> a.Id)
      Actions = CustomerProfileActions.all
      Columns =
        [ { ColumnId = "CustomerId"
            PropertyName = Some "CustomerId"
            Title = "Customer ID"
            DefaultOrder = 0
            DefaultWidthInPixels = Some 160
            DefaultVisible = true
            DefaultPinned = true
            Sortable = true
            Filterable = true
            Groupable = false
            FilterValueKind = Some Text
            UserConfigurable = true
            RenderHint = None
            Alignment = Left
            CellType = TextType
            IsKey = true
            FormatString = None }
          { ColumnId = "CustomerTier"
            PropertyName = Some "CustomerTier"
            Title = "Tier"
            DefaultOrder = 1
            DefaultWidthInPixels = Some 120
            DefaultVisible = true
            DefaultPinned = false
            Sortable = true
            Filterable = true
            Groupable = true
            FilterValueKind = Some Text
            UserConfigurable = true
            RenderHint = Some "StatusBadge"
            Alignment = Center
            CellType = TextType
            IsKey = false
            FormatString = None }
          { ColumnId = "CreditLimit"
            PropertyName = Some "CreditLimit"
            Title = "Credit Limit"
            DefaultOrder = 2
            DefaultWidthInPixels = Some 130
            DefaultVisible = true
            DefaultPinned = false
            Sortable = true
            Filterable = true
            Groupable = false
            FilterValueKind = Some Number
            UserConfigurable = true
            RenderHint = None
            Alignment = Right
            CellType = NumberType
            IsKey = false
            FormatString = Some "$#,##0.00" }
          { ColumnId = "CreditHoldActive"
            PropertyName = Some "CreditHoldActive"
            Title = "Credit Hold"
            DefaultOrder = 3
            DefaultWidthInPixels = Some 110
            DefaultVisible = true
            DefaultPinned = false
            Sortable = true
            Filterable = true
            Groupable = true
            FilterValueKind = Some FilterValueKind.Boolean
            UserConfigurable = true
            RenderHint = Some "StatusBadge"
            Alignment = Center
            CellType = BooleanType
            IsKey = false
            FormatString = None }
          { ColumnId = "DefaultSlaDays"
            PropertyName = Some "DefaultSlaDays"
            Title = "SLA Target (Days)"
            DefaultOrder = 4
            DefaultWidthInPixels = Some 130
            DefaultVisible = true
            DefaultPinned = false
            Sortable = true
            Filterable = true
            Groupable = true
            FilterValueKind = Some Number
            UserConfigurable = true
            RenderHint = None
            Alignment = Right
            CellType = NumberType
            IsKey = false
            FormatString = Some "0" }
          { ColumnId = "AllowSplitShipments"
            PropertyName = Some "AllowSplitShipments"
            Title = "Allow Splits"
            DefaultOrder = 5
            DefaultWidthInPixels = Some 110
            DefaultVisible = true
            DefaultPinned = false
            Sortable = true
            Filterable = true
            Groupable = true
            FilterValueKind = Some FilterValueKind.Boolean
            UserConfigurable = true
            RenderHint = None
            Alignment = Center
            CellType = BooleanType
            IsKey = false
            FormatString = None }
          { ColumnId = "MaxSplitShipmentPackages"
            PropertyName = Some "MaxSplitShipmentPackages"
            Title = "Max Splits"
            DefaultOrder = 6
            DefaultWidthInPixels = Some 100
            DefaultVisible = true
            DefaultPinned = false
            Sortable = true
            Filterable = true
            Groupable = false
            FilterValueKind = Some Number
            UserConfigurable = true
            RenderHint = None
            Alignment = Right
            CellType = NumberType
            IsKey = false
            FormatString = Some "0" }
          { ColumnId = "AllowProductSubstitution"
            PropertyName = Some "AllowProductSubstitution"
            Title = "Substitution"
            DefaultOrder = 7
            DefaultWidthInPixels = Some 110
            DefaultVisible = true
            DefaultPinned = false
            Sortable = true
            Filterable = true
            Groupable = true
            FilterValueKind = Some FilterValueKind.Boolean
            UserConfigurable = true
            RenderHint = None
            Alignment = Center
            CellType = BooleanType
            IsKey = false
            FormatString = None }
          { ColumnId = "SubstitutionMarginFloorPct"
            PropertyName = Some "SubstitutionMarginFloorPct"
            Title = "Margin Floor %"
            DefaultOrder = 8
            DefaultWidthInPixels = Some 120
            DefaultVisible = true
            DefaultPinned = false
            Sortable = true
            Filterable = true
            Groupable = false
            FilterValueKind = Some Number
            UserConfigurable = true
            RenderHint = None
            Alignment = Right
            CellType = NumberType
            IsKey = false
            FormatString = Some "0.0%" }
          { ColumnId = "DailyPenaltyRatePct"
            PropertyName = Some "DailyPenaltyRatePct"
            Title = "Daily Penalty %"
            DefaultOrder = 9
            DefaultWidthInPixels = Some 120
            DefaultVisible = true
            DefaultPinned = false
            Sortable = true
            Filterable = true
            Groupable = false
            FilterValueKind = Some Number
            UserConfigurable = true
            RenderHint = None
            Alignment = Right
            CellType = NumberType
            IsKey = false
            FormatString = Some "0.00%" }
          { ColumnId = "MaxPenaltyCapPct"
            PropertyName = Some "MaxPenaltyCapPct"
            Title = "Max Penalty %"
            DefaultOrder = 10
            DefaultWidthInPixels = Some 120
            DefaultVisible = true
            DefaultPinned = false
            Sortable = true
            Filterable = true
            Groupable = false
            FilterValueKind = Some Number
            UserConfigurable = true
            RenderHint = None
            Alignment = Right
            CellType = NumberType
            IsKey = false
            FormatString = Some "0.0%" }
          { ColumnId = "EffectiveFrom"
            PropertyName = Some "EffectiveFrom"
            Title = "Effective From"
            DefaultOrder = 11
            DefaultWidthInPixels = Some 140
            DefaultVisible = true
            DefaultPinned = false
            Sortable = true
            Filterable = true
            Groupable = false
            FilterValueKind = Some FilterValueKind.DateTimeOffset
            UserConfigurable = true
            RenderHint = None
            Alignment = Left
            CellType = DateTimeType
            IsKey = false
            FormatString = Some "yyyy-MM-dd" }
          { ColumnId = "LifecycleState"
            PropertyName = Some "LifecycleState"
            Title = "Lifecycle State"
            DefaultOrder = 12
            DefaultWidthInPixels = Some 120
            DefaultVisible = true
            DefaultPinned = false
            Sortable = true
            Filterable = true
            Groupable = true
            FilterValueKind = Some Text
            UserConfigurable = true
            RenderHint = Some "StatusBadge"
            Alignment = Center
            CellType = TextType
            IsKey = false
            FormatString = None }
          { ColumnId = "DecisionTraceability"
            PropertyName = Some "DecisionTraceability"
            Title = "Decision Trace"
            DefaultOrder = 13
            DefaultWidthInPixels = Some 140
            DefaultVisible = true
            DefaultPinned = false
            Sortable = true
            Filterable = true
            Groupable = false
            FilterValueKind = Some Text
            UserConfigurable = true
            RenderHint = None
            Alignment = Left
            CellType = TextType
            IsKey = false
            FormatString = None } ]
      DefaultSort =
        Some
            { Field = "CustomerId"
              Direction = Ascending
              Priority = 1 }
      DefaultFilters = [] }

let definition: SchemaDefinition = schema

let adapter: ProjectionAdapter<CustomerPromiseProfileDto> =
    match ProjectionCompiler.compile<CustomerPromiseProfileDto> schema with
    | Valid a -> a
    | Invalid errs ->
        let msg = errs |> String.concat "; "
        invalidOp $"CustomerProfileGrid compilation failed: {msg}"

let componentDefinition: ComponentDefinition =
    { Id = definitionId
      DisplayName = "Customer Promise Profiles"
      Icon = "badge"
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


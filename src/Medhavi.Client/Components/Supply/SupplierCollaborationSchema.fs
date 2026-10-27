namespace Medhavi.Client.Components.Supply

open System
open Medhavi.Contracts.Supply
open Medhavi.Client.ViewComposition.Identity
open Medhavi.Client.ViewComposition.Context
open Medhavi.Client.ViewComposition.Component
open Medhavi.Client.ViewComposition.Schema
open Medhavi.Client.ViewComposition.Data
open Medhavi.Client.Components

module SupplierCollaborationActions =
    let acceptCommitments =
        { Id = ActionDefId.literal "act-accept-commitments"
          DisplayName = "Accept Commitments"
          Icon = "check_circle"
          Scope = SingleEntityScope
          ExecutionMode = RequiresConfirmation "Accept supplier commitments as firm supply constraints?"
          RequiredRoles = [ "Buyer"; "Planner" ]
          IsPrimary = true }

    let requestAlternative =
        { Id = ActionDefId.literal "act-request-alternative"
          DisplayName = "Request Alternative"
          Icon = "swap_horiz"
          Scope = SingleEntityScope
          ExecutionMode = RequiresConfirmation "Request alternative delivery split or buffer date from supplier?"
          RequiredRoles = [ "Buyer"; "Planner" ]
          IsPrimary = false }

    let escalateProcurement =
        { Id = ActionDefId.literal "act-escalate-procurement"
          DisplayName = "Escalate Procurement"
          Icon = "report_problem"
          Scope = SingleEntityScope
          ExecutionMode = RequiresConfirmation "Escalate critical supplier commitment failure to executive procurement?"
          RequiredRoles = [ "SupplyChainLead"; "Buyer" ]
          IsPrimary = false }

    let refreshHealth =
        { Id = ActionDefId.literal "act-refresh-supplier-health"
          DisplayName = "Refresh Health"
          Icon = "refresh"
          Scope = GlobalScope
          ExecutionMode = Direct
          RequiredRoles = [ "Planner"; "Buyer" ]
          IsPrimary = false }

    let all = [ acceptCommitments; requestAlternative; escalateProcurement; refreshHealth ]

module SupplierCollaborationSchema =

    let schemaId = SchemaDefId.literal "schema-supplier-collaboration"

    let columns: SchemaColumnDefinition list =
        [ { ColumnId = "SupplierId"
            PropertyName = Some "SupplierId"
            Title = "Supplier ID"
            DefaultOrder = 1
            DefaultWidthInPixels = Some 160
            DefaultVisible = true
            DefaultPinned = true
            Sortable = true
            Filterable = true
            Groupable = false
            FilterValueKind = Some Text
            UserConfigurable = false
            RenderHint = None
            Alignment = TextAlignment.Left
            CellType = CellType.TextType
            IsKey = true
            FormatString = None }

          { ColumnId = "LifecycleState"
            PropertyName = Some "LifecycleState"
            Title = "State"
            DefaultOrder = 2
            DefaultWidthInPixels = Some 110
            DefaultVisible = true
            DefaultPinned = false
            Sortable = true
            Filterable = true
            Groupable = true
            FilterValueKind = Some (Enum [ "Active"; "Archived" ])
            UserConfigurable = true
            RenderHint = Some "StatusBadge"
            Alignment = TextAlignment.Center
            CellType = CellType.TextType
            IsKey = false
            FormatString = None }

          { ColumnId = "AuthoritativeHealthState"
            PropertyName = Some "AuthoritativeHealthState"
            Title = "Health State"
            DefaultOrder = 3
            DefaultWidthInPixels = Some 140
            DefaultVisible = true
            DefaultPinned = false
            Sortable = true
            Filterable = true
            Groupable = true
            FilterValueKind = Some (Enum [ "Healthy"; "Monitor"; "AtRisk"; "Critical" ])
            UserConfigurable = true
            RenderHint = Some "StatusBadge"
            Alignment = TextAlignment.Center
            CellType = CellType.TextType
            IsKey = false
            FormatString = None }

          { ColumnId = "OverallConfidence"
            PropertyName = Some "OverallConfidence"
            Title = "Confidence"
            DefaultOrder = 4
            DefaultWidthInPixels = Some 120
            DefaultVisible = true
            DefaultPinned = false
            Sortable = true
            Filterable = true
            Groupable = false
            FilterValueKind = Some Text
            UserConfigurable = true
            RenderHint = None
            Alignment = TextAlignment.Left
            CellType = CellType.TextType
            IsKey = false
            FormatString = None }

          { ColumnId = "OnTimeDeliveryRate"
            PropertyName = Some "OnTimeDeliveryRate"
            Title = "OTIF Delivery"
            DefaultOrder = 5
            DefaultWidthInPixels = Some 130
            DefaultVisible = true
            DefaultPinned = false
            Sortable = true
            Filterable = true
            Groupable = false
            FilterValueKind = Some Number
            UserConfigurable = true
            RenderHint = None
            Alignment = TextAlignment.Right
            CellType = CellType.NumberType
            IsKey = false
            FormatString = Some "P1" }

          { ColumnId = "QuantityFulfillmentRate"
            PropertyName = Some "QuantityFulfillmentRate"
            Title = "Qty Fulfillment"
            DefaultOrder = 6
            DefaultWidthInPixels = Some 140
            DefaultVisible = true
            DefaultPinned = false
            Sortable = true
            Filterable = true
            Groupable = false
            FilterValueKind = Some Number
            UserConfigurable = true
            RenderHint = None
            Alignment = TextAlignment.Right
            CellType = CellType.NumberType
            IsKey = false
            FormatString = Some "P1" }

          { ColumnId = "AverageAcknowledgementDurationHours"
            PropertyName = Some "AverageAcknowledgementDurationHours"
            Title = "Avg Ack Time (h)"
            DefaultOrder = 7
            DefaultWidthInPixels = Some 140
            DefaultVisible = true
            DefaultPinned = false
            Sortable = true
            Filterable = true
            Groupable = false
            FilterValueKind = Some Number
            UserConfigurable = true
            RenderHint = None
            Alignment = TextAlignment.Right
            CellType = CellType.NumberType
            IsKey = false
            FormatString = Some "N1" }

          { ColumnId = "AverageCommitmentDurationHours"
            PropertyName = Some "AverageCommitmentDurationHours"
            Title = "Avg Commit Time (h)"
            DefaultOrder = 8
            DefaultWidthInPixels = Some 150
            DefaultVisible = true
            DefaultPinned = false
            Sortable = true
            Filterable = true
            Groupable = false
            FilterValueKind = Some Number
            UserConfigurable = true
            RenderHint = None
            Alignment = TextAlignment.Right
            CellType = CellType.NumberType
            IsKey = false
            FormatString = Some "N1" }

          { ColumnId = "ActiveRiskSignals"
            PropertyName = Some "ActiveRiskSignals"
            Title = "Active Risk Signals"
            DefaultOrder = 9
            DefaultWidthInPixels = Some 220
            DefaultVisible = true
            DefaultPinned = false
            Sortable = false
            Filterable = true
            Groupable = false
            FilterValueKind = Some Text
            UserConfigurable = true
            RenderHint = None
            Alignment = TextAlignment.Left
            CellType = CellType.TextType
            IsKey = false
            FormatString = None }

          { ColumnId = "AssessmentTimestamp"
            PropertyName = Some "AssessmentTimestamp"
            Title = "Assessed At"
            DefaultOrder = 10
            DefaultWidthInPixels = Some 170
            DefaultVisible = true
            DefaultPinned = false
            Sortable = true
            Filterable = true
            Groupable = false
            FilterValueKind = Some DateTimeOffset
            UserConfigurable = true
            RenderHint = None
            Alignment = TextAlignment.Left
            CellType = CellType.DateTimeType
            IsKey = false
            FormatString = Some "yyyy-MM-dd HH:mm" } ]

    let definition: SchemaDefinition =
        { Id = schemaId
          DisplayName = "Supplier Collaboration Assessment (CA-S-005)"
          Capabilities = [ "Tabular"; "Collaboration" ]
          SupportedActions = SupplierCollaborationActions.all |> List.map (fun a -> a.Id)
          Actions = SupplierCollaborationActions.all
          Columns = columns
          DefaultSort = Some { Field = "OnTimeDeliveryRate"; Direction = Ascending; Priority = 1 }
          DefaultFilters = [] }

    let adapter: ProjectionAdapter<SupplierCommitmentAssessmentDto> =
        { ToUniversal =
            fun (dto: SupplierCommitmentAssessmentDto) ->
                let riskSignalsStr =
                    if List.isEmpty dto.ActiveRiskSignals then "None"
                    else String.Join(", ", dto.ActiveRiskSignals)

                { Key = dto.SupplierId
                  Cells =
                    Map.ofList
                        [ "SupplierId", TextValue dto.SupplierId
                          "LifecycleState", TextValue dto.LifecycleState
                          "AuthoritativeHealthState", TextValue dto.AuthoritativeHealthState
                          "OverallConfidence", TextValue dto.OverallConfidence
                          "OnTimeDeliveryRate", NumberValue dto.OnTimeDeliveryRate
                          "QuantityFulfillmentRate", NumberValue dto.QuantityFulfillmentRate
                          "AverageAcknowledgementDurationHours", NumberValue dto.AverageAcknowledgementDurationHours
                          "AverageCommitmentDurationHours", NumberValue dto.AverageCommitmentDurationHours
                          "ActiveRiskSignals", TextValue riskSignalsStr
                          "AssessmentTimestamp", DateTimeValue dto.AssessmentTimestamp ] } }

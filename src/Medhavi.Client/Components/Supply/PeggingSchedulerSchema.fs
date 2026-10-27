namespace Medhavi.Client.Components.Supply

open System
open Medhavi.Client.ViewComposition.Identity
open Medhavi.Client.ViewComposition.Schema
open Medhavi.Client.ViewComposition.Context

module PeggingSchedulerSchema =

    let schemaId = SchemaDefId.literal "schema-pegging-scheduler"

    let definition: SchemaDefinition =
        { Id = schemaId
          DisplayName = "Demand-Supply Pegging Horizon"
          Capabilities = [ "TimeSeries"; "Tabular" ]
          SupportedActions = []
          Actions = []
          Columns =
            [ { ColumnId = "Id"
                PropertyName = Some "Id"
                Title = "Pegging ID"
                DefaultOrder = 0
                DefaultWidthInPixels = Some 140
                DefaultVisible = true
                DefaultPinned = true
                Sortable = true
                FilterValueKind = Some Text
                Filterable = true
                Groupable = false
                RenderHint = None
                Alignment = Left
                CellType = TextType
                IsKey = true
                FormatString = None
                UserConfigurable = false }

              { ColumnId = "Text"
                PropertyName = Some "Text"
                Title = "Description"
                DefaultOrder = 1
                DefaultWidthInPixels = Some 260
                DefaultVisible = true
                DefaultPinned = false
                Sortable = true
                FilterValueKind = Some Text
                Filterable = true
                Groupable = false
                RenderHint = None
                Alignment = Left
                CellType = TextType
                IsKey = false
                FormatString = None
                UserConfigurable = true }

              { ColumnId = "Start"
                PropertyName = Some "Start"
                Title = "Start Date"
                DefaultOrder = 2
                DefaultWidthInPixels = Some 130
                DefaultVisible = true
                DefaultPinned = false
                Sortable = true
                FilterValueKind = Some DateTimeOffset
                Filterable = true
                Groupable = false
                RenderHint = None
                Alignment = Left
                CellType = DateTimeType
                IsKey = false
                FormatString = Some "yyyy-MM-dd"
                UserConfigurable = true }

              { ColumnId = "End"
                PropertyName = Some "End"
                Title = "End Date"
                DefaultOrder = 3
                DefaultWidthInPixels = Some 130
                DefaultVisible = true
                DefaultPinned = false
                Sortable = true
                FilterValueKind = Some DateTimeOffset
                Filterable = true
                Groupable = false
                RenderHint = None
                Alignment = Left
                CellType = DateTimeType
                IsKey = false
                FormatString = Some "yyyy-MM-dd"
                UserConfigurable = true }

              { ColumnId = "ItemType"
                PropertyName = Some "ItemType"
                Title = "Type"
                DefaultOrder = 4
                DefaultWidthInPixels = Some 120
                DefaultVisible = true
                DefaultPinned = false
                Sortable = true
                FilterValueKind = Some Text
                Filterable = true
                Groupable = true
                RenderHint = Some "StatusBadge"
                Alignment = Left
                CellType = TextType
                IsKey = false
                FormatString = None
                UserConfigurable = true }

              { ColumnId = "Quantity"
                PropertyName = Some "Quantity"
                Title = "Quantity"
                DefaultOrder = 5
                DefaultWidthInPixels = Some 110
                DefaultVisible = true
                DefaultPinned = false
                Sortable = true
                FilterValueKind = Some Number
                Filterable = true
                Groupable = false
                RenderHint = None
                Alignment = Right
                CellType = NumberType
                IsKey = false
                FormatString = Some "N0"
                UserConfigurable = true }

              { ColumnId = "EntityRef"
                PropertyName = Some "EntityRef"
                Title = "Entity Reference"
                DefaultOrder = 6
                DefaultWidthInPixels = Some 140
                DefaultVisible = true
                DefaultPinned = false
                Sortable = true
                FilterValueKind = Some Text
                Filterable = true
                Groupable = false
                RenderHint = None
                Alignment = Left
                CellType = TextType
                IsKey = false
                FormatString = None
                UserConfigurable = true }

              { ColumnId = "Status"
                PropertyName = Some "Status"
                Title = "Status"
                DefaultOrder = 7
                DefaultWidthInPixels = Some 110
                DefaultVisible = true
                DefaultPinned = false
                Sortable = true
                FilterValueKind = Some Text
                Filterable = true
                Groupable = true
                RenderHint = Some "StatusBadge"
                Alignment = Left
                CellType = TextType
                IsKey = false
                FormatString = None
                UserConfigurable = true } ]
          DefaultSort = None
          DefaultFilters = [] }

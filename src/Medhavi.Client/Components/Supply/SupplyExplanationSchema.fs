module Medhavi.Client.Components.Supply.SupplyExplanationSchema

open Medhavi.Client.ViewComposition.Identity
open Medhavi.Client.ViewComposition.Schema
open Medhavi.Client.ViewComposition.Context

let schemaId = SchemaDefId.literal "schema-supply-explanation"

let definition: SchemaDefinition =
    {
        Id = schemaId
        DisplayName = "Supply Explanation Schema"
        Capabilities = [ "Detail"; "Tabular" ]
        SupportedActions = []
        Actions = []
        Columns =
            [
                {
                    ColumnId = "ExplanationId"
                    PropertyName = Some "ExplanationId"
                    Title = "Explanation ID"
                    DefaultOrder = 0
                    DefaultWidthInPixels = Some 150
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
                    UserConfigurable = false
                }

                {
                    ColumnId = "ExplainedArtifactType"
                    PropertyName = Some "ExplainedArtifactType"
                    Title = "Artifact Type"
                    DefaultOrder = 1
                    DefaultWidthInPixels = Some 140
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
                    UserConfigurable = true
                }

                {
                    ColumnId = "ExplainedArtifactId"
                    PropertyName = Some "ExplainedArtifactId"
                    Title = "Artifact Ref"
                    DefaultOrder = 2
                    DefaultWidthInPixels = Some 150
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
                    UserConfigurable = true
                }

                {
                    ColumnId = "ExplainedArtifactVersion"
                    PropertyName = Some "ExplainedArtifactVersion"
                    Title = "Version"
                    DefaultOrder = 3
                    DefaultWidthInPixels = Some 90
                    DefaultVisible = true
                    DefaultPinned = false
                    Sortable = true
                    FilterValueKind = Some Number
                    Filterable = false
                    Groupable = false
                    RenderHint = None
                    Alignment = Right
                    CellType = NumberType
                    IsKey = false
                    FormatString = Some "N0"
                    UserConfigurable = true
                }

                {
                    ColumnId = "Confidence"
                    PropertyName = Some "Confidence"
                    Title = "Confidence"
                    DefaultOrder = 4
                    DefaultWidthInPixels = Some 120
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
                    FormatString = Some "P0"
                    UserConfigurable = true
                }
            ]
        DefaultSort = None
        DefaultFilters = []
    }

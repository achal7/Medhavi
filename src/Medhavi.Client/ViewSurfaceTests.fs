module Medhavi.Client.ViewSurfaceTests

open System.Globalization
open Medhavi.Client.Registries
open Medhavi.Client.Rendering
open Medhavi.Client.ViewComposition.Identity
open Medhavi.Client.Shells
open Medhavi.Client.ViewComposition.Component
open Medhavi.Client.ViewComposition.Schema
open Medhavi.Client.ViewComposition.Geometry
open Medhavi.Client.ViewComposition.Session
open Medhavi.Client.ViewComposition.Layout


    // ==================================================================
    // Fixtures
    // ==================================================================
    let private defId = ComponentDefId.literal "demand-grid"
    let private schemaId = SchemaDefId.literal "demand-schema"
    let private storeId = DataStoreId.literal "demand-store"
    let private culture = CultureInfo.InvariantCulture

    let private hints =
        { DashboardRenderHints.RowHeight = 80
          Gap = 8 }

    let private noopDispatch (_: ViewSurfaceMsg) = ()
    let private noopAdapterError (_: LayoutAdapterError) = ()

    let private testDefinition: ComponentDefinition =
        { Id = defId
          DisplayName = "Demand Grid"
          Icon = "grid"
          Category = DataDisplay
          RequiredSchemaCapabilities = [ "Tabular" ]
          DefaultGridSpan = GridSpan.literal 4 4
          DefaultSchemaId = Some schemaId
          DefaultStoreId = Some storeId
          IsResizable = true
          IsDraggable = true
          SelectionMode = SelectionMode.SingleSelection }

    let private testSchema: SchemaDefinition =
        { Id = schemaId
          DisplayName = "Demand Schema"
          Capabilities = [ "Tabular" ]
          SupportedActions = []
          Actions = []
          Columns = []
          DefaultSort = None
          DefaultFilters = [] }

    let private incompatibleSchema: SchemaDefinition =
        { testSchema with
            Capabilities = [ "TimeSeries" ] }

    let private testRuntime: ComponentRuntime = { Render = fun _ _ -> Bolero.Html.text "rendered" }

    let private testRegistration: ComponentRegistration =
        { Definition = testDefinition
          Runtime = testRuntime }

    let private testVC: ViewComponent =
        { ComponentDefId = defId
          Binding =
            { StoreId = storeId
              SchemaDefId = schemaId }
          Configuration =
            { LocalFilters = []
              Parameters = Map.empty }
          Title = None
          Icon = None }

    let private componentRegistry =
        match ComponentRegistry.ofList [ testRegistration ] with
        | Ok r -> r
        | Error _ -> failwith "reg"

    let private schemaRegistry =
        match SchemaRegistry.ofList [ testSchema ] with
        | Ok r -> r
        | Error _ -> failwith "schema"

    let private incompatibleSchemaRegistry =
        match SchemaRegistry.ofList [ incompatibleSchema ] with
        | Ok r -> r
        | Error _ -> failwith "inc"

    // ==================================================================
    // R2.2 Resolution Tests
    // ==================================================================
    let private testR1MissingComponentDef () =
        let unknownVC =
            { testVC with
                ComponentDefId = ComponentDefId.literal "missing" }

        match ComponentResolver.resolve componentRegistry schemaRegistry unknownVC with
        | Error(ComponentResolver.ComponentNotRegistered _) -> true
        | _ -> false

    let private testR2MissingSchemaDef () =
        let unknownVC =
            { testVC with
                Binding =
                    { testVC.Binding with
                        SchemaDefId = SchemaDefId.literal "missing" } }

        match ComponentResolver.resolve componentRegistry schemaRegistry unknownVC with
        | Error(ComponentResolver.SchemaNotRegistered _) -> true
        | _ -> false

    let private testR3IncompatibleBinding () =
        match ComponentResolver.resolve componentRegistry incompatibleSchemaRegistry testVC with
        | Error(ComponentResolver.IncompatibleBinding errs) ->
            errs
            |> List.exists (function
                | MissingCapability("Tabular", _) -> true
                | _ -> false)
        | _ -> false

    let private testR4ValidResolution () =
        match ComponentResolver.resolve componentRegistry schemaRegistry testVC with
        | Ok(reg, schema) -> reg.Definition.Id = defId && schema.Id = schemaId
        | _ -> false

    let private testR5SchemaExtensionMonotonicity () =
        let extSchema =
            { testSchema with
                Capabilities = [ "Tabular"; "FutureCapability" ] }

        let extReg =
            match SchemaRegistry.ofList [ extSchema ] with
            | Ok r -> r
            | _ -> failwith "ext"

        match ComponentResolver.resolve componentRegistry extReg testVC with
        | Ok _ -> true
        | _ -> false

    // ==================================================================
    // R2.3 Context Construction Tests
    // ==================================================================
    let private testC5MissingDataStateDefaultsToIdle () =
        let id = ViewComponentId.literal "A"

        let renderState: ViewRenderState =
            { State =
                { Title = "T"
                  Components = Map.ofList [ (id, testVC) ]
                  Layout = Single id
                  SharedContext = None }
              DataStates = Map.empty
              Selections = Map.empty }

        let ctx = ComponentResolver.buildContext id testVC testDefinition testSchema renderState Map.empty culture
        ctx.DataState = Idle && ctx.Selection = None

    // ==================================================================
    // R2.1 Topology Tests
    // Note: Container topologies (Dashboard, Split, Tabbed) defer child
    // evaluation into Blazor RenderFragment delegates. Pure unit tests
    // cannot observe side-effects inside deferred delegates. We verify
    // that the interpreter successfully produces a Node description
    // without throwing, respecting the presentation boundary.
    // ==================================================================

    let private recordingLeaf () : (ViewComponentId -> Bolero.Node) * (unit -> ViewComponentId list) =
        let mutable called: ViewComponentId list = []

        let leaf id =
            called <- id :: called
            Bolero.Html.empty()

        let read () = List.rev called
        leaf, read

    // L1: Empty layout does not invoke leaf (no deferred children)
    let private testL1EmptyDoesNotInvokeLeaf () =
        let leaf, read = recordingLeaf()
        let _ = LayoutInterpreter.render hints false leaf noopDispatch noopAdapterError Empty
        read() = []

    // L2: Single layout invokes leaf eagerly (no comp<T> container)
    let private testL2SingleInvokesLeaf () =
        let leaf, read = recordingLeaf()
        let id = ViewComponentId.literal "A"
        let _ = LayoutInterpreter.render hints false leaf noopDispatch noopAdapterError (LayoutDefinition.Single id)
        read() = [ id ]

    // L3: Split successfully interprets both branches to a presentation description
    let private testL3SplitInterpretsSuccessfully () =
        let leaf _ = Bolero.Html.empty()
        let idA = ViewComponentId.literal "A"
        let idB = ViewComponentId.literal "B"
        let layout = Split(SplitDirection.Horizontal, Single idA, Single idB, SplitRatio.literal 0.5m)

        try
            let _ = LayoutInterpreter.render hints false leaf noopDispatch noopAdapterError layout
            true
        with _ ->
            false

    // L4: Dashboard successfully interprets all tiles to a presentation description
    let private testL4DashboardInterpretsSuccessfully () =
        let leaf _ = Bolero.Html.empty()
        let idA = ViewComponentId.literal "A"
        let idB = ViewComponentId.literal "B"
        let dims = GridDimensions.literal 12 12
        let tiles = Map.ofList [ idA, TilePosition.literal 1 1 4 4; idB, TilePosition.literal 5 1 4 4 ]

        try
            let _ = LayoutInterpreter.render hints false leaf noopDispatch noopAdapterError (Dashboard(dims, tiles))
            true
        with _ ->
            false

    // L5: Tabbed successfully interprets all tabs to a presentation description
    let private testL5TabbedInterpretsSuccessfully () =
        let leaf _ = Bolero.Html.empty()
        let idA = ViewComponentId.literal "A"
        let idB = ViewComponentId.literal "B"

        let layout =
            Tabbed
                [ { Id = TabId.literal "T1"
                    Label = "t1"
                    Content = Single idA }
                  { Id = TabId.literal "T2"
                    Label = "t2"
                    Content = Single idB } ]

        try
            let _ = LayoutInterpreter.render hints false leaf noopDispatch noopAdapterError layout
            true
        with _ ->
            false

    // ==================================================================
    // Supply Foundation Workbench Validation
    // ==================================================================
    let private testInventoryWorkbenchResolves () =
        let vc: ViewComponent =
            { ComponentDefId = Components.Supply.InventoryGrid.definitionId
              Binding =
                { StoreId = Components.Supply.InventoryGrid.storeId
                  SchemaDefId = Components.Supply.InventorySchema.schemaId }
              Configuration = { LocalFilters = []; Parameters = Map.empty }
              Title = None
              Icon = None }
        match ComponentResolver.resolve Components.ComponentCatalog.components Components.ComponentCatalog.schemas vc with
        | Ok _ -> true
        | Error _ -> false

    let private testCapacityWorkbenchResolves () =
        let vc: ViewComponent =
            { ComponentDefId = Components.Supply.CapacityGrid.definitionId
              Binding =
                { StoreId = Components.Supply.CapacityGrid.storeId
                  SchemaDefId = Components.Supply.CapacitySchema.schemaId }
              Configuration = { LocalFilters = []; Parameters = Map.empty }
              Title = None
              Icon = None }
        match ComponentResolver.resolve Components.ComponentCatalog.components Components.ComponentCatalog.schemas vc with
        | Ok _ -> true
        | Error _ -> false

    let private testSupplyWorkbenchResolves () =
        let vc: ViewComponent =
            { ComponentDefId = Components.Supply.SupplyGrid.definitionId
              Binding =
                { StoreId = Components.Supply.SupplyGrid.storeId
                  SchemaDefId = Components.Supply.SupplySchema.schemaId }
              Configuration = { LocalFilters = []; Parameters = Map.empty }
              Title = None
              Icon = None }
        match ComponentResolver.resolve Components.ComponentCatalog.components Components.ComponentCatalog.schemas vc with
        | Ok _ -> true
        | Error _ -> false

    let private testSupplyUnderstandingResolves () =
        let vc: ViewComponent =
            { ComponentDefId = Components.Supply.SupplyUnderstandingGrid.definitionId
              Binding =
                { StoreId = Components.Supply.SupplyUnderstandingGrid.storeId
                  SchemaDefId = Components.Supply.SupplyUnderstandingSchema.schemaId }
              Configuration = { LocalFilters = []; Parameters = Map.empty }
              Title = None
              Icon = None }
        match ComponentResolver.resolve Components.ComponentCatalog.components Components.ComponentCatalog.schemas vc with
        | Ok _ -> true
        | Error _ -> false

    // ==================================================================
    // Runner
    // ==================================================================
    let run () : (string * bool) list =
        [ "R2.2: missing component def -> error", testR1MissingComponentDef()
          "R2.2: missing schema def -> error", testR2MissingSchemaDef()
          "R2.2: incompatible binding -> error", testR3IncompatibleBinding()
          "R2.2: valid resolution succeeds", testR4ValidResolution()
          "R2.2: schema extension monotonicity", testR5SchemaExtensionMonotonicity()
          "R2.3: missing DataState defaults to Idle", testC5MissingDataStateDefaultsToIdle()
          "R2.1: Empty layout does not invoke leaf", testL1EmptyDoesNotInvokeLeaf()
          "R2.1: Single layout invokes leaf eagerly", testL2SingleInvokesLeaf()
          "R2.1: Split interprets successfully", testL3SplitInterpretsSuccessfully()
          "R2.1: Dashboard interprets successfully", testL4DashboardInterpretsSuccessfully()
          "R2.1: Tabbed interprets successfully", testL5TabbedInterpretsSuccessfully()
          "Supply: CA-S-003 Inventory Workbench resolves", testInventoryWorkbenchResolves()
          "Supply: CA-S-004 Capacity Workbench resolves", testCapacityWorkbenchResolves()
          "Supply: CA-S-002 Planned Order Schedule resolves", testSupplyWorkbenchResolves()
          "Supply: CA-S-001 Sensory Stream resolves", testSupplyUnderstandingResolves() ]

    let report () : unit =
        let results = run()
        results |> List.iter(fun (name, passed) -> printfn "[%s] %s" (if passed then "PASS" else "FAIL") name)
        let failed = results |> List.filter(not << snd) |> List.length
        printfn "ViewSurface contracts: %d/%d checks hold" (List.length results - failed) (List.length results)

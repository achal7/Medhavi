namespace Medhavi.Client.SemanticModel

open System

module Tests =
    // ==================================================================
    // Fixtures
    // ==================================================================
    let private idA = ViewComponentId.literal "A"
    let private idB = ViewComponentId.literal "B"
    let private idC = ViewComponentId.literal "C"
    let private span44 = GridSpan.literal 4 4
    let private dimensions = GridDimensions.literal 12 12
    let private position col row = TilePosition.literal col row 4 4

    let private componentDefinitionId = ComponentDefId.literal "demand-grid"
    let private schemaId = SchemaDefId.literal "demand-schema"
    let private storeId = DataStoreId.literal "demand-store"

    let private sampleComponent: ViewComponent =
        { ComponentDefId = componentDefinitionId
          Binding =
            { StoreId = storeId
              SchemaDefId = schemaId }
          Configuration =
            { LocalFilters = []
              Parameters = Map.empty } }

    let private sampleComponentDefinition: ComponentDefinition =
        { Id = componentDefinitionId
          DisplayName = "Demand Grid"
          Icon = "grid"
          Category = DataDisplay
          RequiredSchemaCapabilities = [ "Tabular" ]
          DefaultGridSpan = span44
          DefaultSchemaId = Some schemaId
          DefaultStoreId = Some storeId
          IsResizable = true
          IsDraggable = true }

    let private sampleSchemaDefinition: SchemaDefinition =
        { Id = schemaId
          DisplayName = "Demand Schema"
          Capabilities = [ "Tabular" ]
          SupportedActions = []
          Columns = []
          DefaultSort = None
          DefaultFilters = [] }

    let private sampleBinding: ComponentBinding =
        match
            ComponentBinding.create
                sampleComponentDefinition
                sampleSchemaDefinition
                storeId
                sampleComponent.Configuration
        with
        | Ok b -> b
        | Error _ -> failwith "Sample binding must be valid for tests"

    let private emptyDashboardState: ViewState =
        { Title = "Test"
          Components = Map.empty
          Layout = Dashboard(dimensions, Map.empty)
          SharedContext = None }

    let private sampleMetadata: ViewMetadata =
        { Category = "Test"
          Icon = "test"
          Roles = []
          IsDefault = false
          IsTemplate = false
          CreatedAt = DateTimeOffset.UtcNow
          CreatedBy = "test"
          ModifiedAt = DateTimeOffset.UtcNow
          ModifiedBy = "test"
          Permissions =
            { Owner = "test"
              Visibility = ViewVisibility.Private
              EditableBy = []
              Shareable = false } }

    let private sampleViewDefinition: ViewDefinition =
        { Id = ViewDefId.literal "test-view"
          Name = "Test View"
          Version = 1
          State = emptyDashboardState
          Metadata = sampleMetadata }

    // ==================================================================
    // Helpers
    // ==================================================================
    let private isError (expected: 'error) (actual: Result<'value, 'error>) : bool =
        match actual with
        | Error error when error = expected -> true
        | _ -> false

    let private sequenceResults (xs: Result<'a, 'e> list) : Result<'a list, 'e> =
        xs
        |> List.fold
            (fun acc r ->
                match acc, r with
                | Ok list, Ok v -> Ok(v :: list)
                | Error e, _ -> Error e
                | _, Error e -> Error e)
            (Ok [])
        |> Result.map List.rev

    let private addA: Result<ViewState, ViewError> =
        ViewAlgebra.addComponent PlacementPolicy.proposePosition span44 idA sampleBinding emptyDashboardState

    let private addAB: Result<ViewState, ViewError> =
        addA
        |> Result.bind(fun state ->
            ViewAlgebra.addComponent PlacementPolicy.proposePosition span44 idB sampleBinding state)

    // ==================================================================
    // Semantic invariant checks (20)
    // ==================================================================
    let private testAddIsValid () = addA |> Result.isOk

    let private testAddDuplicateIsRejected () =
        addA
        |> Result.bind(fun state ->
            ViewAlgebra.addComponent PlacementPolicy.proposePosition span44 idA sampleBinding state)
        |> isError(DuplicateComponentId idA)

    let private testRemoveYieldsEmptyDashboard () =
        addA
        |> Result.bind(fun state -> ViewAlgebra.removeComponent idA state)
        |> function
            | Ok result -> result.Components.IsEmpty && result.Layout = Dashboard(dimensions, Map.empty)
            | Error _ -> false

    let private testRemoveMissingIsRejected () =
        ViewAlgebra.removeComponent idA emptyDashboardState |> isError(ComponentNotFound idA)

    let private testMovePreservesIdentityAndConfiguration () =
        addAB
        |> Result.bind(fun before ->
            ViewAlgebra.adjustDashboardPlacement idA (position 1 5) before |> Result.map(fun after -> (before, after)))
        |> function
            | Ok(before, after) ->
                Map.tryFind idA before.Components = Map.tryFind idA after.Components
                && (Map.keys after.Components |> Set.ofSeq) = Set.ofList [ idA; idB ]
                && match after.Layout with
                   | Dashboard(_, tiles) -> Map.tryFind idA tiles = Some(position 1 5) && Map.containsKey idB tiles
                   | _ -> false
            | Error _ -> false

    let private testDuplicateOccurrenceIsInvalid () =
        ViewAlgebra.validate
            { emptyDashboardState with
                Components = Map.ofList [ (idA, sampleComponent) ]
                Layout =
                    Split(
                        SplitDirection.Horizontal,
                        LayoutDefinition.Single idA,
                        LayoutDefinition.Single idA,
                        SplitRatio.literal 0.5m
                    ) }
        |> isError(DuplicateLayoutOccurrence idA)

    let private testOrphanReferenceIsInvalid () =
        ViewAlgebra.validate
            { emptyDashboardState with
                Components = Map.ofList [ (idA, sampleComponent) ]
                Layout = LayoutDefinition.Single idB }
        |> isError(UnresolvedLayoutReference idB)

    let private testUnreferencedComponentIsInvalid () =
        ViewAlgebra.validate
            { emptyDashboardState with
                Components = Map.ofList [ (idA, sampleComponent); (idB, sampleComponent) ]
                Layout = LayoutDefinition.Single idA }
        |> isError(UnreferencedComponent idB)

    let private testDuplicatePlacementIsRejected () =
        match addA with
        | Error _ -> false
        | Ok state ->
            LayoutAlgebra.placeInDashboard idA (position 9 9) state.Layout
            |> isError(LayoutError.DuplicateLayoutOccurrence idA)

    let private testAdjustMissingIsRejected () =
        LayoutAlgebra.adjustDashboardPlacement idB (position 1 1) (Dashboard(dimensions, Map.empty))
        |> isError(ComponentNotFoundInLayout idB)

    let private testSplitRatioZeroIsRejected () = SplitRatio.create 0.0m |> isError(InvalidSplitRatioValue 0.0m)
    let private testSplitRatioOneIsRejected () = SplitRatio.create 1.0m |> isError(InvalidSplitRatioValue 1.0m)

    let private testOverlapIsRejected () =
        LayoutAlgebra.validateLayout(Dashboard(dimensions, Map.ofList [ (idA, position 1 1); (idB, position 2 2) ]))
        |> function
            | Error(OverlappingTiles _) -> true
            | _ -> false

    let private testOutOfBoundsIsRejected () =
        LayoutAlgebra.validateLayout(Dashboard(dimensions, Map.ofList [ (idA, TilePosition.literal 10 10 4 4) ]))
        |> function
            | Error(InvalidTileBounds _) -> true
            | _ -> false

    let private testEmptyTabContentIsRejected () =
        LayoutAlgebra.validateLayout(
            Tabbed
                [ { Id = TabId.literal "X"
                    Label = "x"
                    Content = Empty } ]
        )
        |> function
            | Error(EmptyTabContent _) -> true
            | _ -> false

    let private testTabIdDuplicateAcrossViewIsRejected () =
        LayoutAlgebra.validateLayout(
            Split(
                SplitDirection.Horizontal,
                Tabbed
                    [ { Id = TabId.literal "X"
                        Label = "a"
                        Content = LayoutDefinition.Single idA } ],
                Tabbed
                    [ { Id = TabId.literal "X"
                        Label = "b"
                        Content = LayoutDefinition.Single idB } ],
                SplitRatio.literal 0.5m
            )
        )
        |> isError(DuplicateTabId(TabId.literal "X"))

    let private testExtractCollapsesSplit () =
        LayoutAlgebra.extract
            idA
            (Split(
                SplitDirection.Horizontal,
                LayoutDefinition.Single idA,
                LayoutDefinition.Single idB,
                SplitRatio.literal 0.5m
            )) = Some(LayoutDefinition.Single idB)

    let private testExtractDropsEmptiedTab () =
        LayoutAlgebra.extract
            idA
            (Tabbed
                [ { Id = TabId.literal "X"
                    Label = "a"
                    Content = LayoutDefinition.Single idA }
                  { Id = TabId.literal "Y"
                    Label = "b"
                    Content = LayoutDefinition.Single idB } ]) = Some(
                                                                     Tabbed
                                                                         [ { Id = TabId.literal "Y"
                                                                             Label = "b"
                                                                             Content = LayoutDefinition.Single idB } ]
                                                                 )

    let private testOpenSessionDerivesSourceMetadata () =
        let session = ViewLifecycle.openSession (ViewSessionId.literal "session") sampleViewDefinition

        session.SourceViewDefId = Some sampleViewDefinition.Id
        && session.SourceVersion = Some sampleViewDefinition.Version
        && session.State = sampleViewDefinition.State
        && not session.IsDirty
        && Map.isEmpty session.DataStates
        && Map.isEmpty session.Selections

    let private testApplyOperationPrunesStaleRuntimeState () =
        match addAB with
        | Error _ -> false
        | Ok state ->
            let session: ViewSession =
                { SessionId = ViewSessionId.literal "session"
                  SourceViewDefId = None
                  SourceVersion = None
                  State = state
                  IsDirty = false
                  DataStates = Map.ofList [ (idA, Idle); (idB, Idle) ]
                  Selections = Map.ofList [ (idA, Some "row"); (idB, Some "row") ] }

            match ViewLifecycle.applyOperation (ViewAlgebra.removeComponent idB) session with
            | Ok updated ->
                updated.IsDirty
                && Map.containsKey idA updated.DataStates
                && not(Map.containsKey idB updated.DataStates)
                && not(Map.containsKey idB updated.Selections)
            | Error _ -> false

    // ==================================================================
    // Placement-policy contract checks (5)
    // ==================================================================
    let private testPolicyProposesOriginOnEmptyGrid () =
        match GridSpan.create 4 4, TilePosition.create 1 1 4 4 with
        | Ok span, Ok expected ->
            PlacementPolicy.proposePosition
                { Dimensions = dimensions
                  Occupied = []
                  Span = span } = Ok expected
        | _ -> false

    let private testPolicyIsHoleAwareNotCountBased () =
        match
            GridSpan.create 4 4, TilePosition.create 1 1 4 4, TilePosition.create 5 5 4 4, TilePosition.create 5 1 4 4
        with
        | Ok span, Ok occA, Ok occB, Ok expected ->
            PlacementPolicy.proposePosition
                { Dimensions = dimensions
                  Occupied = [ occA; occB ]
                  Span = span } = Ok expected
        | _ -> false

    let private testPolicyRejectsOversizedSpan () =
        match GridSpan.create 16 4 with
        | Ok span ->
            PlacementPolicy.proposePosition
                { Dimensions = dimensions
                  Occupied = []
                  Span = span } = Error(Policy(SpanExceedsGridDimensions(span, dimensions)))
        | Error _ -> false

    let private testPolicyRejectsFullGrid () =
        let coords =
            [ for r in 1..4..9 do
                  for c in 1..4..9 do
                      (c, r) ]

        match GridSpan.create 4 4, sequenceResults(coords |> List.map(fun (c, r) -> TilePosition.create c r 4 4)) with
        | Ok span, Ok full ->
            PlacementPolicy.proposePosition
                { Dimensions = dimensions
                  Occupied = full
                  Span = span } = Error(Policy(NoAvailablePosition(span, dimensions)))
        | _ -> false

    let private testFailedInsertionProducesNoStateTransition () =
        match addAB with
        | Ok original ->
            match GridSpan.create 16 4 with
            | Ok oversized ->
                ViewAlgebra.addComponent PlacementPolicy.proposePosition oversized idC sampleBinding original = Error(
                                                                                                                    InvalidPlacement(
                                                                                                                        SpanExceedsGridDimensions(
                                                                                                                            oversized,
                                                                                                                            dimensions
                                                                                                                        )
                                                                                                                    )
                                                                                                                )
            | Error _ -> false
        | Error _ -> false

    // ==================================================================
    // Binding contract checks (6)
    // ==================================================================
    let private testBc1UnknownRequired () =
        let badComp =
            { sampleComponentDefinition with
                RequiredSchemaCapabilities = [ "Tabular"; "Magic" ] }

        match ComponentBinding.create badComp sampleSchemaDefinition storeId sampleComponent.Configuration with
        | Error errs -> errs |> List.contains(UnknownRequiredCapability "Magic")
        | _ -> false

    let private testBc2SchemaExtensionDoesNotInvalidateBinding () =
        let futureSchema =
            { sampleSchemaDefinition with
                Capabilities = [ "Tabular"; "FutureCapability" ] }

        match ComponentBinding.create sampleComponentDefinition futureSchema storeId sampleComponent.Configuration with
        | Ok _ -> true
        | Error _ -> false

    let private testBc3MissingKnown () =
        let comp =
            { sampleComponentDefinition with
                RequiredSchemaCapabilities = [ "Tabular"; "TimeSeries" ] }

        match ComponentBinding.create comp sampleSchemaDefinition storeId sampleComponent.Configuration with
        | Error errs -> errs |> List.contains(MissingCapability("TimeSeries", [ "Tabular" ]))
        | _ -> false

    let private testBc4AccumulatedDiagnostics () =
        let comp =
            { sampleComponentDefinition with
                RequiredSchemaCapabilities = [ "Tabular"; "MagicReq"; "TimeSeries" ] }

        let schema =
            { sampleSchemaDefinition with
                Capabilities = [ "Tabular"; "MagicDecl" ] }

        match ComponentBinding.create comp schema storeId sampleComponent.Configuration with
        | Error errs ->
            errs |> List.contains(UnknownRequiredCapability "MagicReq")
            && errs |> List.contains(MissingCapability("TimeSeries", [ "Tabular"; "MagicDecl" ]))
            && List.length errs = 2
        | _ -> false

    let private testBc5KnownCapabilityExtension () =
        let schemaV2 =
            { sampleSchemaDefinition with
                Capabilities = [ "Tabular"; "TimeSeries" ] }

        match ComponentBinding.create sampleComponentDefinition schemaV2 storeId sampleComponent.Configuration with
        | Ok _ -> true
        | Error _ -> false

    let private testBc6KnownRequiredMissingDespiteUnknownDeclaration () =
        let schema =
            { sampleSchemaDefinition with
                Capabilities = [ "FutureCapability" ] }

        match ComponentBinding.create sampleComponentDefinition schema storeId sampleComponent.Configuration with
        | Error errs -> errs |> List.contains(MissingCapability("Tabular", [ "FutureCapability" ]))
        | _ -> false

    // ==================================================================
    // Runner
    // ==================================================================
    let run () : (string * bool) list =
        [ "add A -> valid", testAddIsValid()
          "add A again -> DuplicateComponentId", testAddDuplicateIsRejected()
          "remove A -> valid empty dashboard", testRemoveYieldsEmptyDashboard()
          "remove missing A -> ComponentNotFound", testRemoveMissingIsRejected()
          "move A -> identity/configuration preserved", testMovePreservesIdentityAndConfiguration()
          "duplicate occurrence in layout -> invalid", testDuplicateOccurrenceIsInvalid()
          "orphan reference in layout -> invalid", testOrphanReferenceIsInvalid()
          "unreferenced component -> invalid", testUnreferencedComponentIsInvalid()
          "duplicate dashboard placement -> rejected", testDuplicatePlacementIsRejected()
          "adjust missing -> ComponentNotFoundInLayout", testAdjustMissingIsRejected()
          "SplitRatio 0 -> rejected", testSplitRatioZeroIsRejected()
          "SplitRatio 1 -> rejected", testSplitRatioOneIsRejected()
          "overlap -> OverlappingTiles", testOverlapIsRejected()
          "out of bounds -> InvalidTileBounds", testOutOfBoundsIsRejected()
          "empty tab content -> EmptyTabContent", testEmptyTabContentIsRejected()
          "TabId duplicated across View -> DuplicateTabId", testTabIdDuplicateAcrossViewIsRejected()
          "extract collapses degenerate split", testExtractCollapsesSplit()
          "extract drops emptied tab", testExtractDropsEmptiedTab()
          "openSession derives source metadata", testOpenSessionDerivesSourceMetadata()
          "applyOperation prunes stale runtime state", testApplyOperationPrunesStaleRuntimeState()
          "policy: origin on empty grid", testPolicyProposesOriginOnEmptyGrid()
          "policy: hole-aware, not count-based", testPolicyIsHoleAwareNotCountBased()
          "policy: oversized span -> Policy error", testPolicyRejectsOversizedSpan()
          "policy: full grid -> NoAvailablePosition", testPolicyRejectsFullGrid()
          "policy: failed insertion -> no state transition", testFailedInsertionProducesNoStateTransition()
          "binding: unknown required -> UnknownRequiredCapability", testBc1UnknownRequired()
          "binding: schema extension does not invalidate", testBc2SchemaExtensionDoesNotInvalidateBinding()
          "binding: missing known -> MissingCapability", testBc3MissingKnown()
          "binding: accumulated diagnostics", testBc4AccumulatedDiagnostics()
          "binding: known capability extension", testBc5KnownCapabilityExtension()
          "binding: known required missing despite unknown declaration",
          testBc6KnownRequiredMissingDespiteUnknownDeclaration() ]

    let report () : unit =
        let results = run()
        results |> List.iter(fun (name, passed) -> printfn "[%s] %s" (if passed then "PASS" else "FAIL") name)
        let failed = results |> List.filter(not << snd) |> List.length
        printfn "Semantic core: %d/%d checks hold" (List.length results - failed) (List.length results)

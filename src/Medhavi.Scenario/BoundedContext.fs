namespace Medhavi.Scenario

open System
open System.Threading.Tasks
open Medhavi.SharedKernel
open Medhavi.SharedKernel.BoundedContexts
open Medhavi.Infrastructure.Stores.InMemRepository
open Medhavi.Scenario.Domain
open Medhavi.Scenario.Domain.ScenarioAgg
open Medhavi.Scenario.Domain.ScenarioConfigurationAgg
open Medhavi.Scenario.Domain.ScenarioOverlaySetAgg
open Medhavi.SharedKernel.ScenarioContracts

module BoundedContext =

    let create
        (scenarioRepo: Repository<Scenario, string, ScenarioEvent>)
        (configRepo: Repository<ScenarioConfiguration, string, ScenarioConfigurationEvent>)
        (overlayRepo: Repository<ScenarioOverlaySet, string, ScenarioOverlayEvent>)
        =

        let createScenario (scenarioId: string, name: string, scenarioType: ScenarioType) =
            task {
                match ScenarioId.create scenarioId with
                | Error e -> return Error e
                | Ok scenId ->
                    // Create Scenario Aggregate
                    let scenCmd = ScenarioCommand.Create(scenId, name, scenarioType)
                    let scenDecRes = ScenarioAgg.handle scenCmd None

                    match scenDecRes with
                    | Error e -> return Error e
                    | Ok scenDec ->
                        let! saveScen = scenarioRepo.Save(scenarioId, scenDec.NewState, scenDec.Events)

                        match saveScen with
                        | Error e -> return Error(DomainError.invariant (sprintf "%A" e))
                        | Ok() ->
                            // Create Configuration Aggregate
                            let configId = ScenarioConfigurationId.create ()

                            let defaultPolicy =
                                { AllowBacklogging = false
                                  AllowLateDelivery = true
                                  SafetyStockMultiplier = 1.0m
                                  MinimumOrderQuantityOverride = Map.empty
                                  ChurnPenaltyCoefficient = 0.1m
                                  FrozenOrderBehavior = Lock }

                            let configCmd =
                                ScenarioConfigurationCommand.Create(
                                    configId,
                                    scenId,
                                    None,
                                    Lexicographic [],
                                    [],
                                    defaultPolicy
                                )

                            let configDecRes = ScenarioConfigurationAgg.handle configCmd None

                            match configDecRes with
                            | Error e -> return Error e
                            | Ok configDec ->
                                let! saveConfig =
                                    configRepo.Save(
                                        ScenarioConfigurationId.value configId |> string,
                                        configDec.NewState,
                                        configDec.Events
                                    )

                                match saveConfig with
                                | Error e -> return Error(DomainError.invariant (sprintf "%A" e))
                                | Ok() ->
                                    // Associate config id to scenario
                                    let assocCmd = ScenarioCommand.Configure(configId)
                                    let assocDecRes = ScenarioAgg.handle assocCmd (Some scenDec.NewState)

                                    match assocDecRes with
                                    | Error e -> return Error e
                                    | Ok assocDec ->
                                        let! saveAssoc =
                                            scenarioRepo.Save(scenarioId, assocDec.NewState, assocDec.Events)

                                        match saveAssoc with
                                        | Error e -> return Error(DomainError.invariant (sprintf "%A" e))
                                        | Ok() ->
                                            // Create OverlaySet Aggregate
                                            let overlayId = ScenarioOverlaySetId.create ()

                                            let overlayCmd =
                                                ScenarioOverlayCommand.CreateOverlaySet(overlayId, scenId, scenarioType)

                                            let overlayDecRes = ScenarioOverlaySetAgg.handle overlayCmd None

                                            match overlayDecRes with
                                            | Error e -> return Error e
                                            | Ok overlayDec ->
                                                let! saveOverlay =
                                                    overlayRepo.Save(
                                                        ScenarioOverlaySetId.value overlayId |> string,
                                                        overlayDec.NewState,
                                                        overlayDec.Events
                                                    )

                                                match saveOverlay with
                                                | Error e -> return Error(DomainError.invariant (sprintf "%A" e))
                                                | Ok() -> return Ok()
            }

        let addOverride (scenarioId: string, ov: ScenarioDataOverride) =
            task {
                match ScenarioId.create scenarioId with
                | Error e -> return Error e
                | Ok scenId ->
                    let! scenRes = scenarioRepo.Get(scenarioId)

                    match scenRes with
                    | Error e -> return Error(DomainError.notFound (sprintf "%A" e))
                    | Ok None -> return Error(DomainError.notFound (sprintf "Scenario %s not found" scenarioId))
                    | Ok(Some scenario) ->
                        let! overlaysRes = overlayRepo.GetAll()

                        match overlaysRes with
                        | Error e -> return Error(DomainError.invariant (sprintf "%A" e))
                        | Ok overlays ->
                            let overlayOpt =
                                overlays
                                |> List.tryFind (fun o -> o.ScenarioId = scenId)

                            match overlayOpt with
                            | None ->
                                return
                                    Error(
                                        DomainError.notFound (sprintf "OverlaySet not found for scenario %s" scenarioId)
                                    )
                            | Some overlay ->
                                let cmd = ScenarioOverlayCommand.AddOverride(ov)
                                let decRes = ScenarioOverlaySetAgg.handle cmd (Some overlay)

                                match decRes with
                                | Error e -> return Error e
                                | Ok dec ->
                                    let! saveRes =
                                        overlayRepo.Save(
                                            ScenarioOverlaySetId.value overlay.Id |> string,
                                            dec.NewState,
                                            dec.Events
                                        )

                                    match saveRes with
                                    | Error e -> return Error(DomainError.invariant (sprintf "%A" e))
                                    | Ok() ->
                                        let dirtyCmd =
                                            ScenarioCommand.MarkDirtyWith(
                                                DirtyReason.OverlayChanged(0, dec.NewState.Version)
                                            )

                                        let dirtyDecRes = ScenarioAgg.handle dirtyCmd (Some scenario)

                                        match dirtyDecRes with
                                        | Error e -> return Error e
                                        | Ok dirtyDec ->
                                            let! _ = scenarioRepo.Save(scenarioId, dirtyDec.NewState, dirtyDec.Events)
                                            return Ok()
            }

        let removeOverride (scenarioId: string, ov: ScenarioDataOverride) =
            task {
                match ScenarioId.create scenarioId with
                | Error e -> return Error e
                | Ok scenId ->
                    let! scenRes = scenarioRepo.Get(scenarioId)

                    match scenRes with
                    | Error e -> return Error(DomainError.notFound (sprintf "%A" e))
                    | Ok None -> return Error(DomainError.notFound (sprintf "Scenario %s not found" scenarioId))
                    | Ok(Some scenario) ->
                        let! overlaysRes = overlayRepo.GetAll()

                        match overlaysRes with
                        | Error e -> return Error(DomainError.invariant (sprintf "%A" e))
                        | Ok overlays ->
                            let overlayOpt =
                                overlays
                                |> List.tryFind (fun o -> o.ScenarioId = scenId)

                            match overlayOpt with
                            | None ->
                                return
                                    Error(
                                        DomainError.notFound (sprintf "OverlaySet not found for scenario %s" scenarioId)
                                    )
                            | Some overlay ->
                                let targetHash = ScenarioDataOverride.contentHash ov
                                let cmd = ScenarioOverlayCommand.RemoveOverride(targetHash)
                                let decRes = ScenarioOverlaySetAgg.handle cmd (Some overlay)

                                match decRes with
                                | Error e -> return Error e
                                | Ok dec ->
                                    let! saveRes =
                                        overlayRepo.Save(
                                            ScenarioOverlaySetId.value overlay.Id |> string,
                                            dec.NewState,
                                            dec.Events
                                        )

                                    match saveRes with
                                    | Error e -> return Error(DomainError.invariant (sprintf "%A" e))
                                    | Ok() ->
                                        let dirtyCmd =
                                            ScenarioCommand.MarkDirtyWith(
                                                DirtyReason.OverlayChanged(0, dec.NewState.Version)
                                            )

                                        let dirtyDecRes = ScenarioAgg.handle dirtyCmd (Some scenario)

                                        match dirtyDecRes with
                                        | Error e -> return Error e
                                        | Ok dirtyDec ->
                                            let! _ = scenarioRepo.Save(scenarioId, dirtyDec.NewState, dirtyDec.Events)
                                            return Ok()
            }

        let commands: ScenarioCommands =
            { Create = createScenario
              AddOverride = addOverride
              RemoveOverride = removeOverride }

        // 3. Queries Implementation (CQRS read model projection)
        let getById (scenarioId: string) =
            task {
                match ScenarioId.create scenarioId with
                | Error _ -> return None
                | Ok scenId ->
                    let! scenRes = scenarioRepo.Get(scenarioId)

                    match scenRes with
                    | Error _
                    | Ok None -> return None
                    | Ok(Some s) ->
                        let! overlaysRes = overlayRepo.GetAll()

                        let overrides =
                            match overlaysRes with
                            | Error _ -> []
                            | Ok list ->
                                list
                                |> List.tryFind (fun o -> o.ScenarioId = scenId)
                                |> Option.map (fun o -> o.Overrides)
                                |> Option.defaultValue []

                        let model =
                            { ScenarioId = scenarioId
                              Name = s.Name
                              BaseScenarioId = s.ParentScenarioId |> Option.map ScenarioId.value
                              Version = Version.value s.Version
                              CreatedAt = DateTimeOffset.UtcNow
                              IsActive =
                                (s.Status = ScenarioStatus.Approved
                                 || s.Status = ScenarioStatus.Ready
                                 || s.Status = ScenarioStatus.PlanningComplete)
                              Overrides = overrides }

                        return Some model
            }

        let getAll () =
            task {
                let! scenRes = scenarioRepo.GetAll()

                match scenRes with
                | Error _ -> return []
                | Ok list ->
                    let! overlaysRes = overlayRepo.GetAll()

                    let overlays =
                        match overlaysRes with
                        | Error _ -> []
                        | Ok oList -> oList

                    let mapped =
                        list
                        |> List.map (fun s ->
                            let ovs =
                                overlays
                                |> List.tryFind (fun o -> o.ScenarioId = s.Id)
                                |> Option.map (fun o -> o.Overrides)
                                |> Option.defaultValue []

                            { ScenarioId = ScenarioId.value s.Id
                              Name = s.Name
                              BaseScenarioId = s.ParentScenarioId |> Option.map ScenarioId.value
                              Version = Version.value s.Version
                              CreatedAt = DateTimeOffset.UtcNow
                              IsActive =
                                (s.Status = ScenarioStatus.Approved
                                 || s.Status = ScenarioStatus.Ready
                                 || s.Status = ScenarioStatus.PlanningComplete)
                              Overrides = ovs })

                    return mapped
            }

        let queries: ScenarioQueries = { GetById = getById; GetAll = getAll }

        // 4. Initialize
        let initialize () =
            task {
                let! baselineOpt = getById "BASELINE"

                match baselineOpt with
                | None ->
                    let! _ = createScenario ("BASELINE", "Live Baseline Plan", ScenarioType.Baseline)
                    ()
                | Some _ -> ()
            }

        let dispose () = ()

        { Commands = commands
          Queries = queries
          Initialize = initialize
          Dispose = dispose }

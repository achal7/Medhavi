namespace Medhavi.Scenario

open System
open System.Threading.Tasks
open Medhavi.SharedKernel
open Medhavi.SharedKernel.BoundedContexts
open Medhavi.Infrastructure.Stores.InMemRepository

module BoundedContext =

    let create () =
        // 1. Repository
        let scenarioRepo = createInMemoryRepository<Scenario, string, obj> ()

        // 2. Commands Implementation
        let createScenario (scenario: Scenario) =
            task {
                let! res = scenarioRepo.Save(scenario.ScenarioId, scenario, [])
                match res with
                | Ok () -> return Ok ()
                | Error e -> return Error (DomainError.validation (sprintf "%A" e))
            }

        let addOverride (scenarioId: string) (ov: ScenarioDataOverride) =
            task {
                let! scenOptRes = scenarioRepo.Get(scenarioId)
                match scenOptRes with
                | Error e -> return Error (DomainError.validation (sprintf "%A" e))
                | Ok None -> return Error (DomainError.validation (sprintf "Scenario %s not found" scenarioId))
                | Ok (Some scenario) ->
                    // Filter out any existing override with the same content hash before adding the new one
                    let targetHash = ScenarioDataOverride.contentHash ov
                    let filtered = scenario.Overrides |> List.filter (fun existing -> 
                        ScenarioDataOverride.contentHash existing <> targetHash)
                    let updatedScenario = { scenario with Overrides = ov :: filtered }
                    let! saveRes = scenarioRepo.Save(scenarioId, updatedScenario, [])
                    match saveRes with
                    | Ok () -> return Ok ()
                    | Error e -> return Error (DomainError.validation (sprintf "%A" e))
            }

        let removeOverride (scenarioId: string) (ov: ScenarioDataOverride) =
            task {
                let! scenOptRes = scenarioRepo.Get(scenarioId)
                match scenOptRes with
                | Error e -> return Error (DomainError.validation (sprintf "%A" e))
                | Ok None -> return Error (DomainError.validation (sprintf "Scenario %s not found" scenarioId))
                | Ok (Some scenario) ->
                    let targetHash = ScenarioDataOverride.contentHash ov
                    let updatedOverrides = scenario.Overrides |> List.filter (fun existing -> 
                        ScenarioDataOverride.contentHash existing <> targetHash)
                    let updatedScenario = { scenario with Overrides = updatedOverrides }
                    let! saveRes = scenarioRepo.Save(scenarioId, updatedScenario, [])
                    match saveRes with
                    | Ok () -> return Ok ()
                    | Error e -> return Error (DomainError.validation (sprintf "%A" e))
            }

        let commands : ScenarioCommands =
            { Create = createScenario
              AddOverride = addOverride
              RemoveOverride = removeOverride }

        // 3. Queries Implementation
        let getById (scenarioId: string) =
            task {
                let! res = scenarioRepo.Get(scenarioId)
                match res with
                | Ok opt -> return opt
                | Error _ -> return None
            }

        let getAll () =
            task {
                let! res = scenarioRepo.GetAll()
                match res with
                | Ok list -> return list
                | Error _ -> return []
            }

        let queries : ScenarioQueries =
            { GetById = getById
              GetAll = getAll }

        // 4. Initialize
        let initialize () =
            task {
                let! baselineOpt = getById "BASELINE"
                match baselineOpt with
                | None ->
                    let baseline =
                        { ScenarioId = "BASELINE"
                          Name = "Live Baseline Plan"
                          BaseScenarioId = None
                          Version = 1
                          CreatedAt = DateTimeOffset.UtcNow
                          IsActive = true
                          Overrides = [] }
                    let! _ = createScenario baseline
                    ()
                | Some _ -> ()
            }

        // 5. Dispose
        let dispose () = ()

        { Commands = commands
          Queries = queries
          Initialize = initialize
          Dispose = dispose }

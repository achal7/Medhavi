namespace Medhavi.Scheduler.Planning.Application

open System
open Medhavi.Contracts.Scenario
open Medhavi.SharedKernel
open Medhavi.Scheduler.Planning.Domain

type StartRunCommand =
    { RunId: PlanningRunId
      ScenarioId: ScenarioId
      Status: ScenarioStatus
      Fingerprint: InputFingerprint
      Horizon: PlanRunHorizon
      Overrides: ScenarioDataOverride list }

type RunCompleted =
    { RunId: PlanningRunId
      PlanVersionId: PlanVersionId
      KpiSummary: PlanKpiSummary
      StorageKey: string
      CompletedAt: DateTimeOffset }

type RunFailed =
    { RunId: PlanningRunId
      Reason: string
      FailedAt: DateTimeOffset }

type OrchestratorDependencies =
    { LoadInputData: ScenarioId -> DateTimeOffset -> Async<PlanningInputData>
      Solver: SolvePlan
      Store: PlanVersionStore
      Now: unit -> DateTimeOffset }

module PlanningOrchestrator =

    let applyOverlays (overrides: ScenarioDataOverride list) (input: PlanningInputData) : PlanningInputData =
        let appliedDemands =
            input.Demands
            |> List.map (fun d ->
                overrides
                |> List.tryPick (function
                    | DemandOverride(id, qty, _) when id = d.DemandId -> Some { d with Quantity = Quantity.clampToZero qty }
                    | _ -> None)
                |> Option.defaultValue d)

        let appliedSupplies =
            input.SupplyBuckets
            |> List.map (fun s ->
                overrides
                |> List.tryPick (function
                    | InventoryOverride(sku, sp, qty) when sku = SkuId.value s.SkuId && sp = StockingPointId.value s.StockingPointId && s.Origin = Inventory ->
                        Some { s with Quantity = Quantity.clampToZero qty }
                    | _ -> None)
                |> Option.defaultValue s)

        let appliedCapacities =
            input.Capacities
            |> List.map (fun c ->
                overrides
                |> List.tryPick (function
                    | CapacityOverride(resId, date, qty) when resId = c.ResourceId && Timestamp.create date = c.Period ->
                        Some { c with Available = Quantity.clampToZero qty }
                    | _ -> None)
                |> Option.defaultValue c)

        { input with
            Demands = appliedDemands
            SupplyBuckets = appliedSupplies
            Capacities = appliedCapacities }

    let execute (deps: OrchestratorDependencies) (cmd: StartRunCommand) : Async<Result<RunCompleted, RunFailed>> =
        async {
            let now = deps.Now()
            let versionId = InputFingerprint.toPlanVersionId cmd.Fingerprint

            // 1. Idempotence / cache check
            let! exists = deps.Store.Exists versionId
            let! cachedOpt =
                if exists then
                    deps.Store.Load versionId
                else
                    async { return None }

            match cachedOpt with
            | Some existing ->
                return Ok
                    { RunId = cmd.RunId
                      PlanVersionId = versionId
                      KpiSummary = existing.KpiSummary
                      StorageKey = sprintf "plan-versions/%s" (PlanVersionId.value versionId)
                      CompletedAt = now }
            | None ->
                // 2. Pre-flight Validation
                // Load base input data
                let! rawInput = deps.LoadInputData cmd.ScenarioId now

                // Apply overlays before checking readiness (so overrides to zero demands, etc. are validated)
                let overlaidInput = applyOverlays cmd.Overrides rawInput

                match ScenarioReadinessValidator.validateResult cmd.Status cmd.Horizon overlaidInput.Demands overlaidInput.SupplyBuckets with
                | Error issues ->
                    let errorMsg =
                        issues
                        |> List.map (fun i -> sprintf "[%s] %s" i.Code i.Message)
                        |> String.concat "; "
                    return Error
                        { RunId = cmd.RunId
                          Reason = sprintf "Readiness validation failed: %s" errorMsg
                          FailedAt = now }
                | Ok () ->
                    // 3. Solve Phase
                    let! solveResult = deps.Solver cmd.ScenarioId cmd.Fingerprint.Mode overlaidInput cmd.Horizon

                    match solveResult with
                    | Error domainErrors ->
                        let errorMsg =
                            domainErrors
                            |> List.map (fun e -> sprintf "%A" e)
                            |> String.concat "; "
                        return Error
                            { RunId = cmd.RunId
                              Reason = sprintf "Solver failed: %s" errorMsg
                              FailedAt = now }
                    | Ok result ->
                        // 4. Persistence
                        let! saveResult = deps.Store.Save result
                        match saveResult with
                        | Error storeError ->
                            return Error
                                { RunId = cmd.RunId
                                  Reason = sprintf "PlanVersion storage failed: %s" storeError
                                  FailedAt = now }
                        | Ok () ->
                            let storageKey = sprintf "plan-versions/%s" (PlanVersionId.value versionId)
                            return Ok
                                { RunId = cmd.RunId
                                  PlanVersionId = versionId
                                  KpiSummary = result.KpiSummary
                                  StorageKey = storageKey
                                  CompletedAt = now }
        }

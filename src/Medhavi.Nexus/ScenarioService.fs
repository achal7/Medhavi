module Medhavi.Nexus.ScenarioService

open System
open System.Threading.Tasks
open Medhavi.Contracts.Scenario
open Medhavi.Scenario
open Medhavi.Contracts.Scenario
open Medhavi.SharedKernel.InMemRepository
open Medhavi.Scenario.Domain

type Service = {
    Context: ScenarioContext
}

let create() : Service =
    let scenarioRepo = createInMemoryRepository<Scenario, string, ScenarioEvent>()
    let configRepo = createInMemoryRepository<ScenarioConfiguration, string, ScenarioConfigurationEvent>()
    let overlayRepo = createInMemoryRepository<ScenarioOverlaySet, string, ScenarioOverlayEvent>()
    let context = BoundedContext.create scenarioRepo configRepo overlayRepo
    { Context = context }
(*        //ScenarioApi = createService ScenarioContext
let getScenarios (scenarioQueries: ScenarioQueries) : Task<ScenarioReadModel list> = task { return! scenarioContext.Queries.GetAll() }

let createScenario (commands: ScenarioCommands) (name: string, scenarioType: ScenarioType, parentId: string option) : Task<Result<unit, string>> =
    task {
        let scenarioId = $"SCENARIO-{Guid.NewGuid().ToString().Substring(0, 8).ToUpper()}"
        let! res = commands.Create(scenarioId, name, scenarioType, parentId)
        match res with
        | Ok () -> return Ok ()
        | Error err -> return Error (sprintf "%A" err)
    }

let addOverride (commands: ScenarioCommands) (scenarioId: string, ov: ScenarioDataOverride) : Task<Result<unit, string>> =
    task {
        let! res = commands.AddOverride(scenarioId, ov)
        match res with
        | Ok () -> return Ok ()
        | Error err -> return Error (sprintf "%A" err)
    }

let removeOverride (commands: ScenarioCommands) (scenarioId: string, ov: ScenarioDataOverride) : Task<Result<unit, string>> =
    task {
        let! res = commands.RemoveOverride(scenarioId, ov)
        match res with
        | Ok () -> return Ok ()
        | Error err -> return Error (sprintf "%A" err)
    }

let submitScenarioForApproval (commands: ScenarioCommands) (scenarioId: string) : Task<Result<unit, string>> =
    task {
        let! res = commands.SubmitForApproval(scenarioId)
        match res with
        | Ok () -> return Ok ()
        | Error err -> return Error (sprintf "%A" err)
    }

let approveScenario (commands: ScenarioCommands) (scenarioId: string) : Task<Result<unit, string>> =
    task {
        let! res = commands.Approve(scenarioId)
        match res with
        | Ok () -> return Ok ()
        | Error err -> return Error (sprintf "%A" err)
    }

let rejectScenario (commands: ScenarioCommands) (scenarioId: string, reason: string) : Task<Result<unit, string>> =
    task {
        let! res = commands.Reject(scenarioId, reason)
        match res with
        | Ok () -> return Ok ()
        | Error err -> return Error (sprintf "%A" err)
    }

member this.PublishScenario(scenarioId: string, reason: string option) : Task<Result<string, string>> =
    task {
        let! scOpt = scenarioContext.Queries.GetById scenarioId
        match scOpt with
        | None -> return Error $"Scenario %s{scenarioId} not found"
        | Some sc ->
            let publishId = $"PUB-{Guid.NewGuid().ToString().Substring(0, 8).ToUpper()}"
            let rollbackId = $"RLB-{Guid.NewGuid().ToString().Substring(0, 8).ToUpper()}"

            let mutable publishedChanges = []
            let mutable rollbackChanges = []

            let! baselineDemandsMap = demandContext.DemandAgent.GetStateAsync()
            let! baselineInventoryMap = supplyContext.Queries.Inventory.GetAll()
            let baselineInventoryDict = baselineInventoryMap |> Seq.map (fun i -> i.Id, i) |> Map.ofSeq

            for ov in sc.Overrides do
                match ov with
                | DemandOverride(demandId, qty, overrideReason) ->
                    match Map.tryFind demandId baselineDemandsMap with
                    | Some d ->
                        let oldQty = Quantity.value d.Quantity
                        let change =
                            { EntityId = demandId
                                EntityType = "DemandLine"
                                FieldPath = "Quantity"
                                OldValueJson = System.Text.Json.JsonSerializer.Serialize(oldQty)
                                NewValueJson = System.Text.Json.JsonSerializer.Serialize(qty)
                                ValueType = "Decimal" }
                        publishedChanges <- change :: publishedChanges

                        let restore =
                            { EntityId = demandId
                                EntityType = "DemandLine"
                                FieldPath = "Quantity"
                                RestoreValueJson = System.Text.Json.JsonSerializer.Serialize(oldQty) }
                        rollbackChanges <- restore :: rollbackChanges

                        let req: DemandDefineReq =
                            { DemandLineId = d.DemandLineId
                                DemandOrderId = d.DemandOrderId
                                SkuId = SkuId.value d.SkuId
                                StockingPointId = StockingPointId.value d.StockingPointId
                                CustomerId = d.CustomerId
                                Quantity = qty
                                UnitOfMeasure = d.UnitOfMeasure
                                OrderDate = d.OrderDate
                                EarliestDeliveryDate = d.EarliestDeliveryDate
                                RequestedDeliveryDate = d.RequestedDeliveryDate
                                LatestDeliveryDate = d.LatestDeliveryDate
                                ConfirmedDeliveryDate = d.ConfirmedDeliveryDate
                                ActualDeliveryDate = d.ActualDeliveryDate
                                Priority = d.Priority
                                DemandCategory = d.DemandCategory.ToString()
                                IsFirm = d.IsFirm
                                IsFrozen = d.IsFrozen }
                        let! _ = demandContext.Commands.DemandLine.Define(req)
                        ()
                    | None -> ()

                | InventoryOverride(skuId, stockingPointId, qty) ->
                    let invId = $"INV-{skuId}-{stockingPointId}"
                    match Map.tryFind invId baselineInventoryDict with
                    | Some i ->
                        let oldQty = i.Quantity
                        let change =
                            { EntityId = invId
                                EntityType = "Inventory"
                                FieldPath = "Quantity"
                                OldValueJson = System.Text.Json.JsonSerializer.Serialize(oldQty)
                                NewValueJson = System.Text.Json.JsonSerializer.Serialize(qty)
                                ValueType = "Decimal" }
                        publishedChanges <- change :: publishedChanges

                        let restore =
                            { EntityId = invId
                                EntityType = "Inventory"
                                FieldPath = "Quantity"
                                RestoreValueJson = System.Text.Json.JsonSerializer.Serialize(oldQty) }
                        rollbackChanges <- restore :: rollbackChanges

                        let req: InventoryDefineReq =
                            { Id = invId
                                SkuId = skuId
                                StockingPointId = stockingPointId
                                Quantity = qty
                                UnitOfMeasure = i.UnitOfMeasure }
                        let! _ = supplyContext.Commands.Inventory.Define(req)
                        ()
                    | None -> ()

                | _ -> ()

            let publishRecord =
                { PublishId = publishId
                    ScenarioId = scenarioId
                    BaselineVersionBefore = int64 sc.Version
                    BaselineVersionAfter = None
                    PublishedAt = DateTimeOffset.UtcNow
                    PublishedBy = "Planner"
                    Changes = publishedChanges
                    Reason = reason }

            let rollbackPackage =
                { PublishId = publishId
                    ScenarioId = scenarioId
                    CreatedAt = DateTimeOffset.UtcNow
                    RestoreChanges = rollbackChanges }

            publishLedger.SaveRecord(publishRecord)
            publishLedger.SavePackage(rollbackPackage)

            let! archiveRes = scenarioContext.Commands.Archive(scenarioId, Some publishId, Some rollbackId)
            match archiveRes with
            | Error e -> return Error (sprintf "%A" e)
            | Ok () -> return Ok publishId
    }

member this.RollbackScenario(publishId: string) : Task<Result<unit, string>> =
    task {
        match publishLedger.GetPackage(publishId) with
        | None -> return Error $"Rollback package for publish event %s{publishId} not found."
        | Some pkg ->
            let! baselineDemandsMap = demandContext.DemandAgent.GetStateAsync()
            let! baselineInventoryMap = supplyContext.Queries.Inventory.GetAll()
            let baselineInventoryDict = baselineInventoryMap |> Seq.map (fun i -> i.Id, i) |> Map.ofSeq

            let mutable conflictErrors = []

            for restore in pkg.RestoreChanges do
                match restore.EntityType with
                | "DemandLine" ->
                    match Map.tryFind restore.EntityId baselineDemandsMap with
                    | None ->
                        conflictErrors <- $"Baseline demand {restore.EntityId} was deleted." :: conflictErrors
                    | Some d ->
                        match publishLedger.GetRecord(publishId) with
                        | None ->
                            conflictErrors <- "Associated publish record not found." :: conflictErrors
                        | Some rec' ->
                            let changeOpt = rec'.Changes |> List.tryFind (fun c -> c.EntityId = restore.EntityId && c.FieldPath = restore.FieldPath)
                            match changeOpt with
                            | None -> ()
                            | Some change ->
                                let currentQty = Quantity.value d.Quantity
                                let expectedQty = System.Text.Json.JsonSerializer.Deserialize<decimal>(change.NewValueJson)
                                if currentQty <> expectedQty then
                                    conflictErrors <- $"Baseline demand {restore.EntityId} was modified after publish (Current Qty: {currentQty}, Expected Qty: {expectedQty})." :: conflictErrors

                | "Inventory" ->
                    match Map.tryFind restore.EntityId baselineInventoryDict with
                    | None ->
                        conflictErrors <- $"Baseline inventory {restore.EntityId} was deleted." :: conflictErrors
                    | Some i ->
                        match publishLedger.GetRecord(publishId) with
                        | None ->
                            conflictErrors <- "Associated publish record not found." :: conflictErrors
                        | Some rec' ->
                            let changeOpt = rec'.Changes |> List.tryFind (fun c -> c.EntityId = restore.EntityId && c.FieldPath = restore.FieldPath)
                            match changeOpt with
                            | None -> ()
                            | Some change ->
                                let currentQty = i.Quantity
                                let expectedQty = System.Text.Json.JsonSerializer.Deserialize<decimal>(change.NewValueJson)
                                if currentQty <> expectedQty then
                                    conflictErrors <- $"Baseline inventory {restore.EntityId} was modified after publish (Current Qty: {currentQty}, Expected Qty: {expectedQty})." :: conflictErrors

                | _ -> ()

            if not (List.isEmpty conflictErrors) then
                let errMsg = "Rollback aborted due to divergence conflicts:\n" + String.concat "\n" conflictErrors
                return Error errMsg
            else
                for restore in pkg.RestoreChanges do
                    match restore.EntityType with
                    | "DemandLine" ->
                        match Map.tryFind restore.EntityId baselineDemandsMap with
                        | Some d ->
                            let restoreQty = System.Text.Json.JsonSerializer.Deserialize<decimal>(restore.RestoreValueJson)
                            let req: DemandDefineReq =
                                { DemandLineId = d.DemandLineId
                                    DemandOrderId = d.DemandOrderId
                                    SkuId = SkuId.value d.SkuId
                                    StockingPointId = StockingPointId.value d.StockingPointId
                                    CustomerId = d.CustomerId
                                    Quantity = restoreQty
                                    UnitOfMeasure = d.UnitOfMeasure
                                    OrderDate = d.OrderDate
                                    EarliestDeliveryDate = d.EarliestDeliveryDate
                                    RequestedDeliveryDate = d.RequestedDeliveryDate
                                    LatestDeliveryDate = d.LatestDeliveryDate
                                    ConfirmedDeliveryDate = d.ConfirmedDeliveryDate
                                    ActualDeliveryDate = d.ActualDeliveryDate
                                    Priority = d.Priority
                                    DemandCategory = d.DemandCategory.ToString()
                                    IsFirm = d.IsFirm
                                    IsFrozen = d.IsFrozen }
                            let! _ = demandContext.Commands.DemandLine.Define(req)
                            ()
                        | None -> ()

                    | "Inventory" ->
                        match Map.tryFind restore.EntityId baselineInventoryDict with
                        | Some i ->
                            let restoreQty = System.Text.Json.JsonSerializer.Deserialize<decimal>(restore.RestoreValueJson)
                            let req: InventoryDefineReq =
                                { Id = restore.EntityId
                                    SkuId = i.SkuId
                                    StockingPointId = i.StockingPointId
                                    Quantity = restoreQty
                                    UnitOfMeasure = i.UnitOfMeasure }
                            let! _ = supplyContext.Commands.Inventory.Define(req)
                            ()
                        | None -> ()

                    | _ -> ()

                return Ok ()
    }
*)

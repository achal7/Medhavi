module Medhavi.MasterData.Application.StockingPoint

open Medhavi.Common.Validation
open Medhavi.Common.Patterns
open Medhavi.SharedKernel
open Medhavi.MasterData.Domain.StockingPointAgg
open Medhavi.Contracts.Integration
open Medhavi.SharedKernel.Aggregate

module ACL =
    let parseStockingPointType (t: string) : Result<StockingPointType, DomainError> =
        match t.Trim().ToLowerInvariant() with
        | "plant" -> Ok StockingPointType.Plant
        | "distributioncenter" | "distribution_center" | "dc" -> Ok DistributionCenter
        | "warehouse" | "wh" -> Ok Warehouse
        | _ -> Error (DomainError.validation $"Unknown StockingPoint type: {t}")

    let toDefineCommand (req: StockingPointDefineReq) : Result<DefineStockingPointCmd, DomainError> =
        let make (spId: StockingPointId) (plantId: PlantId) (spType: StockingPointType) : DefineStockingPointCmd =
            { Id = spId
              PlantId = plantId
              Code = req.Code
              Name = req.Name
              Type = spType
              Location = req.Location
              Level = req.Level
              PlanningLevel = req.PlanningLevel
              SupplyCanBeSplit = req.SupplyCanBeSplit }

        make
        <!> (StockingPointId.create req.Id |> fromResult)
        <*> (PlantId.create req.PlantId |> fromResult)
        <*> (parseStockingPointType req.Type |> fromResult)
        |> toResult
        |> Result.mapError DomainError.combineValidationErrors

    let toRenameCommand (req: StockingPointRenameReq) : Result<RenameStockingPointCmd, DomainError> =
        StockingPointId.create req.Id
        |> Result.map (fun id -> { Id = id; NewName = req.NewName } : RenameStockingPointCmd)

    let toRetireCommand (req: StockingPointRetireReq) : Result<StockingPointId, DomainError> =
        StockingPointId.create req.Id

type StockingPointCapabilities =
    { Define: StockingPointDefineReq -> TaskResult<StockingPointEvent list, ApplicationError>
      Rename: StockingPointRenameReq -> TaskResult<StockingPointEvent list, ApplicationError>
      Retire: StockingPointRetireReq -> TaskResult<StockingPointEvent list, ApplicationError> }

let createCapabilities (repo: Repository<StockingPoint, string, StockingPointEvent>) =
    { Define =
        liftCmdResult ACL.toDefineCommand
        >=> handleCommand (fun (c: DefineStockingPointCmd) -> StockingPointId.value c.Id) repo DefineStockingPoint decide
      Rename =
        liftCmdResult ACL.toRenameCommand
        >=> handleCommand (fun (c: RenameStockingPointCmd) -> StockingPointId.value c.Id) repo RenameStockingPoint decide
      Retire =
        liftCmdResult ACL.toRetireCommand
        >=> handleCommand (fun id -> StockingPointId.value id) repo RetireStockingPoint decide }

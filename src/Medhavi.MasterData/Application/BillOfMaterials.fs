module Medhavi.MasterData.Application.BillOfMaterials

open Medhavi.Common.Validation
open Medhavi.Common.Patterns
open Medhavi.SharedKernel
open Medhavi.MasterData.Domain.UomAgg
open Medhavi.MasterData.Domain.BoMAgg
open Medhavi.Contracts.Integration
open Medhavi.SharedKernel.Aggregate

module ACL =
    let toBomItem (req: BomItemReq) : Validation<DefineBomItemCmd, DomainError> =
        let make (compSkuId: SkuId) (uomId: UomId) : DefineBomItemCmd =
            { ComponentSkuId = compSkuId
              Quantity = Qty.create req.Quantity
              UnitOfMeasureId = uomId
              Sequence = req.Sequence }

        make
        <!> (SkuId.create req.ComponentSkuId |> fromResult)
        <*> (UomId.create req.UnitOfMeasureId |> fromResult)

    let toDefineCommand (req: BomDefineReq) : Result<DefineBillOfMaterialCmd, DomainError> =
        let make (bomId: BillOfMaterialId) (skuId: SkuId) (items: DefineBomItemCmd list) : DefineBillOfMaterialCmd =
            { Id = bomId
              SkuId = skuId
              Items = items }

        let itemsVal =
            req.Items
            |> List.map toBomItem
            |> sequence

        make
        <!> (BillOfMaterialId.create req.Id |> fromResult)
        <*> (SkuId.create req.SkuId |> fromResult)
        <*> itemsVal
        |> toResult
        |> Result.mapError DomainError.combineValidationErrors

    let toActivateCommand (req: BomActivateReq) : Result<BillOfMaterialId, DomainError> =
        BillOfMaterialId.create req.Id

    let toDeactivateCommand (req: BomDeactivateReq) : Result<BillOfMaterialId, DomainError> =
        BillOfMaterialId.create req.Id

type BomCapabilities =
    { Define: BomDefineReq -> TaskResult<BomEvent list, ApplicationError>
      Activate: BomActivateReq -> TaskResult<BomEvent list, ApplicationError>
      Deactivate: BomDeactivateReq -> TaskResult<BomEvent list, ApplicationError> }

let createCapabilities (repo: Repository<BillOfMaterial, string, BomEvent>) =
    { Define =
        liftCmdResult ACL.toDefineCommand
        >=> handleCommand (fun (c: DefineBillOfMaterialCmd) -> BillOfMaterialId.value c.Id) repo DefineBom decide
      Activate =
        liftCmdResult ACL.toActivateCommand
        >=> handleCommand (fun id -> BillOfMaterialId.value id) repo ActivateBom decide
      Deactivate =
        liftCmdResult ACL.toDeactivateCommand
        >=> handleCommand (fun id -> BillOfMaterialId.value id) repo DeactivateBom decide }

module Medhavi.MasterData.Application.BillOfMaterials

open Medhavi.Common.Validation
open Medhavi.Common.Patterns
open Medhavi.Contracts.Integration
open Medhavi.Infrastructure
open Medhavi.Infrastructure.Projections
open Medhavi.SharedKernel
open Medhavi.SharedKernel.Aggregate
open Medhavi.MasterData.Domain.UomAgg
open Medhavi.MasterData.Domain.BoMAgg

module ACL =
    let toBomItem (req: BomItemReq) : Validation<DefineBomItemCmd, DomainError> =
        let make (compSkuId: SkuId) (uomId: UomId) (qty: Quantity) : DefineBomItemCmd =
            { ComponentSkuId = compSkuId
              Quantity = qty
              UnitOfMeasureId = uomId
              Sequence = req.Sequence }

        make
        <!> (SkuId.create req.ComponentSkuId |> fromResult)
        <*> (UomId.create req.UnitOfMeasureId |> fromResult)
        <*> (Quantity.create req.Quantity |> fromResult)

    let toDefineCommand (req: BomDefineReq) : Result<DefineBillOfMaterialCmd, DomainError> =
        let make (bomId: BillOfMaterialId) (skuId: SkuId) (items: DefineBomItemCmd list) : DefineBillOfMaterialCmd =
            { Id = bomId
              SkuId = skuId
              Items = items }

        let itemsVal = req.Items |> List.map toBomItem |> sequence

        make
        <!> (BillOfMaterialId.create req.Id |> fromResult)
        <*> (SkuId.create req.SkuId |> fromResult)
        <*> itemsVal
        |> toResult
        |> Result.mapError DomainError.combineValidationErrors

    let toActivateCommand (req: BomActivateReq) : Result<BillOfMaterialId, DomainError> = BillOfMaterialId.create req.Id

    let toDeactivateCommand (req: BomDeactivateReq) : Result<BillOfMaterialId, DomainError> =
        BillOfMaterialId.create req.Id

type Decision = Decision<BillOfMaterial, BomEvent>

type BomCapabilities =
    { Define: BomDefineReq -> TaskResult<Decision, ApplicationError>
      Activate: BomActivateReq -> TaskResult<Decision, ApplicationError>
      Deactivate: BomDeactivateReq -> TaskResult<Decision, ApplicationError> }

let createCapabilities (repo: Repository<BillOfMaterial, string, BomEvent>) =
    { Define =
        liftCmdResult ACL.toDefineCommand
        >=> handleCommand (fun c -> BillOfMaterialId.value c.Id) repo DefineBom decide
      Activate =
        liftCmdResult ACL.toActivateCommand
        >=> handleCommand BillOfMaterialId.value repo ActivateBom decide
      Deactivate =
        liftCmdResult ACL.toDeactivateCommand
        >=> handleCommand BillOfMaterialId.value repo DeactivateBom decide }

let mapBomDto (b: BillOfMaterial) : Medhavi.Contracts.Domain.Bom =
    let lines =
        b.Items
        |> List.map (fun i ->
            let item: Medhavi.Contracts.Domain.BomItem =
                { ComponentSkuId = SkuId.value i.ComponentSkuId
                  Quantity = (Quantity.value i.Quantity)
                  Sequence = i.Sequence }

            item)

    { Id = BillOfMaterialId.value b.Id
      SkuId = SkuId.value b.SkuId
      Items = lines
      Status = b.Status.ToBool() }

let evolveProjection (state: Map<string, Medhavi.Contracts.Domain.Bom>) (evt: BomEvent) =
    match evt with
    | BomDefined b -> Map.add (BillOfMaterialId.value b.Id) (mapBomDto b) state
    | BomActivated(id, _) ->
        let key = BillOfMaterialId.value id

        match Map.tryFind key state with
        | Some existing -> Map.add key { existing with Status = true } state
        | None -> state
    | BomDeactivated(id, _) ->
        let key = BillOfMaterialId.value id

        match Map.tryFind key state with
        | Some existing -> Map.add key { existing with Status = false } state
        | None -> state

let createProjectionAgent () =
    ProjectionAgent<Map<string, Medhavi.Contracts.Domain.Bom>, BomEvent>(evolveProjection, Map.empty, "BomReadModel")

let createQueryService agent = QueryServiceBase.getQueryService agent id

open Medhavi.SharedKernel.API

let createBomApi (capabilities: BomCapabilities) agent =
    { Define =
        fun req ->
            capabilities.Define req
            |> TaskResult.map (fun d -> d.NewState)
            |> TaskResult.map mapBomDto
      DefineBulk =
        fun reqs ->
            reqs
            |> List.map capabilities.Define
            |> TaskResult.sequence
            |> TaskResult.map (fun decisions ->
                decisions
                |> List.map (fun d -> d.NewState)
                |> List.map mapBomDto)
      Activate =
        fun req ->
            capabilities.Activate req
            |> TaskResult.map (fun d -> d.NewState)
            |> TaskResult.map mapBomDto
      Deactivate =
        fun req ->
            capabilities.Deactivate req
            |> TaskResult.map (fun d -> d.NewState)
            |> TaskResult.map mapBomDto
      QueryService = QueryServiceBase.getQueryService agent id }
    : BomApi

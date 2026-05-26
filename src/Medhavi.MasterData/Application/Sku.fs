module Medhavi.MasterData.Application.Sku

open Medhavi.Common.Patterns
open Medhavi.SharedKernel
open Medhavi.MasterData.Domain.SkuAgg
open Medhavi.Contracts.Integration
open Medhavi.SharedKernel.Aggregate

module ACL =
    let toDefineCommand (req: SkuDefineReq) =
        Ok { Id = req.Id
             Code = req.Code
             Name = req.Name
             Group = req.Group
             CreatedAt = Timestamp.create req.Created }

    let toRenameCommand (req: SkuRenameReq) =
        SkuId.create req.Id
        |> Result.map (fun id -> (id, req.NewName))

    let toRetireCommand (req: SkuRetireReq) =
        SkuId.create req.Id

type SkuCapabilities =
    { Define: SkuDefineReq -> TaskResult<SkuEvent list, ApplicationError>
      Rename: SkuRenameReq -> TaskResult<SkuEvent list, ApplicationError>
      Retire: SkuRetireReq -> TaskResult<SkuEvent list, ApplicationError> }

let createCapabilities (repo: Repository<Sku, string, SkuEvent>) =
    { Define =
        liftCmdResult ACL.toDefineCommand
        >=> handleCommand (fun c -> c.Id) repo DefineSku decide
      Rename =
        liftCmdResult ACL.toRenameCommand
        >=> handleCommand (fun (id, _) -> SkuId.value id) repo RenameSku decide
      Retire =
        liftCmdResult ACL.toRetireCommand
        >=> handleCommand (fun id -> SkuId.value id) repo RetireSku decide }

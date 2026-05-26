module Medhavi.MasterData.Application.Plant

open Medhavi.Common.Patterns
open Medhavi.SharedKernel
open Medhavi.MasterData.Domain.PlantAgg
open Medhavi.Contracts.Integration
open Medhavi.SharedKernel.Aggregate

module ACL =
    let toDefineCommand (req: PlantDefineReq) : Result<DefinePlantCmd, DomainError> =
        Ok { Id = req.Id
             Code = req.Code
             Name = req.Name }

    let toRenameCommand (req: PlantRenameReq) : Result<RenamePlantCmd, DomainError> =
        PlantId.create req.Id
        |> Result.map (fun id -> { Id = id; NewName = req.NewName } : RenamePlantCmd)

    let toRetireCommand (req: PlantRetireReq) : Result<RetirePlantCmd, DomainError> =
        PlantId.create req.Id
        |> Result.map (fun id -> { Id = id } : RetirePlantCmd)

type PlantCapabilities =
    { Define: PlantDefineReq -> TaskResult<PlantEvent list, ApplicationError>
      Rename: PlantRenameReq -> TaskResult<PlantEvent list, ApplicationError>
      Retire: PlantRetireReq -> TaskResult<PlantEvent list, ApplicationError> }

let createCapabilities (repo: Repository<Plant, string, PlantEvent>) =
    { Define =
        liftCmdResult ACL.toDefineCommand
        >=> handleCommand (fun (c: DefinePlantCmd) -> c.Id) repo DefinePlant decide
      Rename =
        liftCmdResult ACL.toRenameCommand
        >=> handleCommand (fun (c: RenamePlantCmd) -> PlantId.value c.Id) repo RenamePlant decide
      Retire =
        liftCmdResult ACL.toRetireCommand
        >=> handleCommand (fun (c: RetirePlantCmd) -> PlantId.value c.Id) repo RetirePlant decide }

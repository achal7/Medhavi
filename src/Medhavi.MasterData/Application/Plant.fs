module Medhavi.MasterData.Application.Plant

open Medhavi
open Medhavi.Common.Patterns
open Medhavi.Contracts.Integration
open Medhavi.Infrastructure
open Medhavi.SharedKernel
open Medhavi.SharedKernel.Aggregate
open Medhavi.MasterData.Domain.PlantAgg
open Medhavi.Infrastructure.Projections

module ACL =
    let toDefineCommand (req: PlantDefineReq) : Result<DefinePlantCmd, DomainError> =
        Ok
            { Id = req.Id
              Code = req.Code
              Name = req.Name }

    let toRenameCommand (req: PlantRenameReq) : Result<RenamePlantCmd, DomainError> =
        PlantId.create req.Id
        |> Result.map (fun id -> { Id = id; NewName = req.NewName }: RenamePlantCmd)

    let toRetireCommand (req: PlantRetireReq) : Result<RetirePlantCmd, DomainError> =
        PlantId.create req.Id
        |> Result.map (fun id -> { Id = id }: RetirePlantCmd)

type Decision = Decision<Plant, PlantEvent>

type PlantCapabilities =
    { Define: PlantDefineReq -> TaskResult<Decision, ApplicationError>
      Rename: PlantRenameReq -> TaskResult<Decision, ApplicationError>
      Retire: PlantRetireReq -> TaskResult<Decision, ApplicationError> }

let createCapabilities (repo: Repository<Plant, string, PlantEvent>) =
    { Define =
        liftCmdResult ACL.toDefineCommand
        >=> handleCommand (fun c -> c.Id) repo DefinePlant decide
      Rename =
        liftCmdResult ACL.toRenameCommand
        >=> handleCommand (fun c -> PlantId.value c.Id) repo RenamePlant decide
      Retire =
        liftCmdResult ACL.toRetireCommand
        >=> handleCommand (fun c -> PlantId.value c.Id) repo RetirePlant decide }

let mapPlantDto (p: Plant) : Contracts.Domain.Plant =
    { Id = PlantId.value p.Id
      Code = p.Code
      Name = p.Name
      Status =
        match p.Status with
        | Active -> true
        | Retired -> false }

let evolveProjection (state: Map<string, Contracts.Domain.Plant>) (evt: PlantEvent) =
    match evt with
    | PlantDefined e ->
        let dto: Contracts.Domain.Plant =
            { Id = PlantId.value e.Id
              Code = e.Code
              Name = e.Name
              Status = true }

        Map.add dto.Id dto state
    | PlantRenamed e ->
        let key = PlantId.value e.Id

        match Map.tryFind key state with
        | Some existing -> Map.add key { existing with Name = e.NewName } state
        | None -> state
    | PlantRetired e ->
        let key = PlantId.value e.Id

        match Map.tryFind key state with
        | Some existing -> Map.add key { existing with Status = false } state
        | None -> state

let createProjectionAgent () =
    ProjectionAgent<Map<string, Contracts.Domain.Plant>, PlantEvent>(evolveProjection, Map.empty, "PlantReadModel")

open Medhavi.SharedKernel.API
open Medhavi.Infrastructure.Projections

let createPlantApi (capabilities: PlantCapabilities) agent =
    { Define =
        fun req ->
            capabilities.Define req
            |> TaskResult.map (fun d -> d.NewState)
            |> TaskResult.map mapPlantDto
      Rename =
        fun req ->
            capabilities.Rename req
            |> TaskResult.map (fun d -> d.NewState)
            |> TaskResult.map mapPlantDto
      Retire =
        fun req ->
            capabilities.Retire req
            |> TaskResult.map (fun d -> d.NewState)
            |> TaskResult.map mapPlantDto
      QueryService = QueryServiceBase.getQueryService agent id }
    : PlantApi

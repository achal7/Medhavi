module Medhavi.MasterData.Application.Sku

open Medhavi
open Medhavi.Common.Patterns
open Medhavi.Contracts.Integration
open Medhavi.Infrastructure
open Medhavi.SharedKernel
open Medhavi.SharedKernel.API
open Medhavi.SharedKernel.Aggregate
open Medhavi.MasterData.Domain.SkuAgg
open Medhavi.Infrastructure.Projections

module ACL =
    let toDefineCommand (req: SkuDefineReq) =
        Ok
            { Id = req.Id
              Code = req.Code
              Name = req.Name
              Group = req.Group
              CreatedAt = Timestamp.create req.Created }

    let toRenameCommand (req: SkuRenameReq) =
        SkuId.create req.Id
        |> Result.map (fun id -> (id, req.NewName))

    let toRetireCommand (req: SkuRetireReq) = SkuId.create req.Id

type Decision = Decision<Sku, SkuEvent>

type SkuCapabilities =
    { Define: SkuDefineReq -> TaskResult<Decision, ApplicationError>
      Rename: SkuRenameReq -> TaskResult<Decision, ApplicationError>
      Retire: SkuRetireReq -> TaskResult<Decision, ApplicationError> }

let createCapabilities (repo: Repository<Sku, string, SkuEvent>) =
    { Define =
        liftCmdResult ACL.toDefineCommand
        >=> handleCommand (fun c -> c.Id) repo DefineSku decide
      Rename =
        liftCmdResult ACL.toRenameCommand
        >=> handleCommand (fun (id, _) -> SkuId.value id) repo RenameSku decide
      Retire =
        liftCmdResult ACL.toRetireCommand
        >=> handleCommand SkuId.value repo RetireSku decide }

let mapSkuDto (s: Sku) : Contracts.Domain.Sku =
    { Id = SkuId.value s.Id
      Code = s.Code
      Name = s.Name
      Group = s.Group
      Status = s.Status.ToBool() }

let evolveProjection (state: Map<string, Contracts.Domain.Sku>) (evt: SkuEvent) =
    match evt with
    | SkuDefined s -> Map.add (SkuId.value s.Id) (mapSkuDto s) state
    | SkuRenamed(id, name, _) ->
        let key = SkuId.value id

        match Map.tryFind key state with
        | Some existing -> Map.add key { existing with Name = name } state
        | None -> state
    | SkuRetired(id, _) ->
        let key = SkuId.value id

        match Map.tryFind key state with
        | Some existing -> Map.add key { existing with Status = false } state
        | None -> state

let createProjection () =
    ProjectionAgent<Map<string, Contracts.Domain.Sku>, SkuEvent>(evolveProjection, Map.empty, "SkuReadModel")

let createQueryService agent = QueryServiceBase.getQueryService agent id

let createSkuApi (capabilities: SkuCapabilities) agent =
    { Define =
        fun req ->
            capabilities.Define req
            |> TaskResult.map (fun d -> d.NewState)
            |> TaskResult.map mapSkuDto
      DefineBulk =
        fun reqs ->
            reqs
            |> List.map capabilities.Define
            |> TaskResult.sequence
            |> TaskResult.map (fun decisions ->
                decisions
                |> List.map (fun d -> d.NewState)
                |> List.map mapSkuDto)
      Rename =
        fun req ->
            capabilities.Rename req
            |> TaskResult.map (fun d -> d.NewState)
            |> TaskResult.map mapSkuDto
      Retire =
        fun req ->
            capabilities.Retire req
            |> TaskResult.map (fun d -> d.NewState)
            |> TaskResult.map mapSkuDto }
    : SkuApi

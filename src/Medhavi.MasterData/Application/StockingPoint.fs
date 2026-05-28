module Medhavi.MasterData.Application.StockingPoint

open Medhavi
open Medhavi.Common.Validation
open Medhavi.Common.Patterns
open Medhavi.Contracts.Integration
open Medhavi.Infrastructure
open Medhavi.SharedKernel
open Medhavi.SharedKernel.Aggregate
open Medhavi.MasterData.Domain.StockingPointAgg
open Medhavi.Infrastructure.Projections

module ACL =
    let parseStockingPointType (t: string) : Result<StockingPointType, DomainError> =
        match t.Trim().ToLowerInvariant() with
        | "plant" -> Ok StockingPointType.Plant
        | "distributioncenter"
        | "distribution_center"
        | "dc" -> Ok DistributionCenter
        | "warehouse"
        | "wh" -> Ok Warehouse
        | _ -> Error(DomainError.validation $"Unknown StockingPoint type: {t}")

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
        |> Result.map (fun id -> { Id = id; NewName = req.NewName }: RenameStockingPointCmd)

    let toRetireCommand (req: StockingPointRetireReq) : Result<StockingPointId, DomainError> =
        StockingPointId.create req.Id

type Decision = Decision<StockingPoint, StockingPointEvent>

type StockingPointCapabilities =
    { Define: StockingPointDefineReq -> TaskResult<Decision, ApplicationError>
      Rename: StockingPointRenameReq -> TaskResult<Decision, ApplicationError>
      Retire: StockingPointRetireReq -> TaskResult<Decision, ApplicationError> }

let createCapabilities (repo: Repository<StockingPoint, string, StockingPointEvent>) =
    { Define =
        liftCmdResult ACL.toDefineCommand
        >=> handleCommand (fun c -> StockingPointId.value c.Id) repo DefineStockingPoint decide
      Rename =
        liftCmdResult ACL.toRenameCommand
        >=> handleCommand (fun c -> StockingPointId.value c.Id) repo RenameStockingPoint decide
      Retire =
        liftCmdResult ACL.toRetireCommand
        >=> handleCommand StockingPointId.value repo RetireStockingPoint decide }

let mapStockingPointDto (s: StockingPoint) : Contracts.Domain.StockingPoint =
    let tStr =
        match s.Type with
        | StockingPointType.Plant -> "Plant"
        | StockingPointType.DistributionCenter -> "DistributionCenter"
        | StockingPointType.Warehouse -> "Warehouse"

    { Id = StockingPointId.value s.Id
      PlantId = PlantId.value s.PlantId
      Code = s.Code
      Name = s.Name
      Type = tStr
      Status = s.Status.ToBool() }

let evolveProjection (state: Map<string, Contracts.Domain.StockingPoint>) (evt: StockingPointEvent) =
    match evt with
    | StockingPointDefined s -> Map.add (StockingPointId.value s.Id) (mapStockingPointDto s) state
    | StockingPointRenamed e ->
        let key = StockingPointId.value e.Id

        match Map.tryFind key state with
        | Some existing -> Map.add key { existing with Name = e.NewName } state
        | None -> state
    | StockingPointRetired e ->
        let key = StockingPointId.value e.Id

        match Map.tryFind key state with
        | Some existing -> Map.add key { existing with Status = false } state
        | None -> state

let createProjectionAgent () =
    ProjectionAgent<Map<string, Contracts.Domain.StockingPoint>, StockingPointEvent>(
        evolveProjection,
        Map.empty,
        "StockingPointReadModel"
    )

let createQueryService agent = QueryServiceBase.getQueryService agent id

open Medhavi.SharedKernel.API

let createStockingPointApi (capabilities: StockingPointCapabilities) agent =
    { Define =
        fun req ->
            capabilities.Define req
            |> TaskResult.map (fun d -> d.NewState)
            |> TaskResult.map mapStockingPointDto
      DefineBulk =
        fun reqs ->
            reqs
            |> List.map capabilities.Define
            |> TaskResult.sequence
            |> TaskResult.map (fun decisions ->
                decisions
                |> List.map (fun d -> d.NewState)
                |> List.map mapStockingPointDto)
      Rename =
        fun req ->
            capabilities.Rename req
            |> TaskResult.map (fun d -> d.NewState)
            |> TaskResult.map mapStockingPointDto
      Retire =
        fun req ->
            capabilities.Retire req
            |> TaskResult.map (fun d -> d.NewState)
            |> TaskResult.map mapStockingPointDto
      QueryService = QueryServiceBase.getQueryService agent id }
    : StockingPointApi

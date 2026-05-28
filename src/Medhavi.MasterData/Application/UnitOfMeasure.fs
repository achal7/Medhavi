module Medhavi.MasterData.Application.Uom

open Medhavi
open Medhavi.Common.Patterns
open Medhavi.Contracts.Integration
open Medhavi.Infrastructure
open Medhavi.Infrastructure.Projections
open Medhavi.MasterData.Domain.UomAgg
open Medhavi.SharedKernel
open Medhavi.SharedKernel.Aggregate
open Medhavi.SharedKernel.API

module ACL =
    let toDefineCommand (req: UomDefineReq) =
        UomId.create (req.Code)
        |> Result.map (fun id ->
            { Code = req.Code
              Name = req.Name
              ToBaseFactor = req.ToBaseFactor
              Created = Timestamp.create req.Created
              IsBase = req.IsBase })

    let toRetireCommand (id: string) = UomId.create id

    let toActivateCommand (id: string) = UomId.create id

    let toChangeConversionFactorCommand (req: UomChangeConversionFactorReq) =
        UomId.create (req.Id)
        |> Result.map (fun id ->
            { Id = id
              NewFactor = req.NewFactor
              NewIsBase = req.IsBase })

type Decision = Decision<UnitOfMeasure, UnitOfMeasureEvent>

type UomCapabilities =
    { Define: UomDefineReq -> TaskResult<Decision, ApplicationError>
      ChangeConversionFactor: UomChangeConversionFactorReq -> TaskResult<Decision, ApplicationError>
      Retire: string -> TaskResult<Decision, ApplicationError>
      Activate: string -> TaskResult<Decision, ApplicationError> }

let createCapabilities (repo: Repository<UnitOfMeasure, string, UnitOfMeasureEvent>) =
    { Define =
        liftCmdResult ACL.toDefineCommand
        >=> handleCommand (fun c -> c.Code) repo Define decide
      ChangeConversionFactor =
        liftCmdResult ACL.toChangeConversionFactorCommand
        >=> handleCommand (fun c -> UomId.value c.Id) repo ChangeConversionFactor decide
      Retire =
        liftCmdResult ACL.toRetireCommand
        >=> handleCommand UomId.value repo Retire decide
      Activate =
        liftCmdResult ACL.toActivateCommand
        >=> handleCommand UomId.value repo Activate decide }

let mapToUomDto (uom: UnitOfMeasure) : Contracts.Domain.UnitOfMeasure =
    let isBase, factorVal =
        match uom.ConversionFactor with
        | Base factor -> true, PositiveDecimal.value factor
        | Derived factor -> false, PositiveDecimal.value factor

    { Id = UomId.value uom.Id
      Code = uom.Code
      Name = uom.Name
      IsBase = isBase
      ConversionFactor = factorVal
      Status =
        match uom.Status with
        | Active -> true
        | Retired -> false }

let evolveProjection (state: Map<string, Contracts.Domain.UnitOfMeasure>) (evt: UnitOfMeasureEvent) =
    match evt with
    | UnitOfMeasureDefined uom -> Map.add (UomId.value uom.Id) (mapToUomDto uom) state
    | ConversionFactorChanged e ->
        let key = UomId.value e.Id

        match Map.tryFind key state with
        | Some existing ->
            let isBase, factorVal =
                match e.NewFactor with
                | Base factor -> true, PositiveDecimal.value factor
                | Derived factor -> false, PositiveDecimal.value factor

            let updated =
                { existing with
                    IsBase = isBase
                    ConversionFactor = factorVal }

            Map.add key updated state
        | None -> state
    | UnitOfMeasureRetired e ->
        let key = UomId.value e.Id

        match Map.tryFind key state with
        | Some existing -> Map.add key { existing with Status = false } state
        | None -> state
    | UnitOfMeasureActivated e ->
        let key = UomId.value e.Id

        match Map.tryFind key state with
        | Some existing -> Map.add key { existing with Status = true } state
        | None -> state

let createProjectionAgent () =
    ProjectionAgent<Map<string, Contracts.Domain.UnitOfMeasure>, UnitOfMeasureEvent>(
        evolveProjection,
        Map.empty,
        "UomReadModel"
    )

let createUomApi
    (capabilities: UomCapabilities)
    (agent: ProjectionAgent<Map<string, Contracts.Domain.UnitOfMeasure>, UnitOfMeasureEvent>)
    =
    { Define =
        fun req ->
            capabilities.Define req
            |> TaskResult.map (fun d -> d.NewState)
            |> TaskResult.map mapToUomDto
      DefineBulk =
        fun reqs ->
            reqs
            |> List.map capabilities.Define
            |> TaskResult.sequence
            |> TaskResult.map (fun decisions ->
                decisions
                |> List.map (fun d -> d.NewState)
                |> List.map mapToUomDto)
      Retire =
        fun req ->
            capabilities.Retire req
            |> TaskResult.map (fun d -> d.NewState)
            |> TaskResult.map mapToUomDto
      Activate =
        fun req ->
            capabilities.Activate req
            |> TaskResult.map (fun d -> d.NewState)
            |> TaskResult.map mapToUomDto
      ChangeConversionFactor =
        fun req ->
            capabilities.ChangeConversionFactor req
            |> TaskResult.map (fun d -> d.NewState)
            |> TaskResult.map mapToUomDto
      QueryService = QueryServiceBase.getQueryService agent id }
    : UomApi

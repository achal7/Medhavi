module Medhavi.MasterData.Application.UoMConversion

open Medhavi
open Medhavi.Contracts.Integration
open Medhavi.Common.Validation
open Medhavi.Common.Patterns
open Medhavi.Infrastructure.Projections
open Medhavi.MasterData.Domain.UnitConversionAgg
open Medhavi.SharedKernel
open Medhavi.SharedKernel.Aggregate

module ACL =
    let toDefineCommand (req: UnitConversionDefineReq) : Result<DefineUnitConversionCmd, DomainError> =
        let make (fromUom: UomId) (toUom: UomId) : DefineUnitConversionCmd =
            let ucId = $"UC-{UomId.value fromUom}-{UomId.value toUom}"

            { Id = ucId
              ProductId = None
              FromUom = fromUom
              ToUom = toUom
              Ratio = req.ConversionFactor
              Created = Timestamp.create req.Created }

        make
        <!> (UomId.create req.SourceUom |> fromResult)
        <*> (UomId.create req.TargetUom |> fromResult)
        |> toResult
        |> Result.mapError DomainError.combineValidationErrors

    let toUpdateRatioCommand (req: UnitConversionUpdateReq) : Result<UpdateUnitConversionCmd, DomainError> =
        UnitConversionId.create req.Id
        |> Result.map (fun id -> { Id = id; Ratio = req.Ratio }: UpdateUnitConversionCmd)

    let toRetireCommand (req: UnitConversionRetireReq) : Result<UnitConversionId * Status, DomainError> =
        UnitConversionId.create req.Id
        |> Result.map (fun id -> (id, Inactive))

type Decision = Decision<UnitConversion, UnitConversionEvent>

type UnitConversionCapabilities =
    { Define: UnitConversionDefineReq -> TaskResult<Decision, ApplicationError>
      UpdateRatio: UnitConversionUpdateReq -> TaskResult<Decision, ApplicationError>
      Retire: UnitConversionRetireReq -> TaskResult<Decision, ApplicationError> }

let createCapabilities (repo: Repository<UnitConversion, string, UnitConversionEvent>) =
    { Define =
        liftCmdResult ACL.toDefineCommand
        >=> handleCommand (fun c -> c.Id) repo DefineUnitConversion decide
      UpdateRatio =
        liftCmdResult ACL.toUpdateRatioCommand
        >=> handleCommand (fun c -> UnitConversionId.value c.Id) repo UpdateRatio decide
      Retire =
        liftCmdResult ACL.toRetireCommand
        >=> handleCommand (fun (id, _) -> UnitConversionId.value id) repo UpdateStatus decide }

let mapUnitConversionDto (uc: UnitConversion) : Contracts.Domain.UnitConversion =
    { Id = UnitConversionId.value uc.Id
      ProductId = uc.ProductId |> Option.map SkuId.value
      FromUnitCode = UomId.value uc.FromUom
      ToUnitCode = UomId.value uc.ToUom
      Ratio = PositiveDecimal.value uc.Ratio
      Status = uc.Status.IsActive }

let evolveProjection (state: Map<string, Contracts.Domain.UnitConversion>) (evt: UnitConversionEvent) =
    match evt with
    | UnitConversionDefined uc ->
        let dto = mapUnitConversionDto uc
        Map.add dto.Id dto state
    | RatioUpdated e ->
        let key = UnitConversionId.value e.Id

        match Map.tryFind key state with
        | Some existing ->
            Map.add
                key
                { existing with
                    Ratio = PositiveDecimal.value e.NewRatio }
                state
        | None -> state
    | StatusUpdated e ->
        let key = UnitConversionId.value e.Id

        match Map.tryFind key state with
        | Some existing ->
            let newStatus =
                match e.NewStatus with
                | Active -> true
                | Inactive -> false

            Map.add key { existing with Status = newStatus } state
        | None -> state
    | UnitConversionRetired e ->
        let key = UnitConversionId.value e.Id

        match Map.tryFind key state with
        | Some existing -> Map.add key { existing with Status = false } state
        | None -> state

let createProjectionAgent () =
    ProjectionAgent<Map<string, Contracts.Domain.UnitConversion>, UnitConversionEvent>(
        evolveProjection,
        Map.empty,
        "UnitConversionReadModel"
    )

let createQueryService agent = QueryServiceBase.getQueryService agent id

open Medhavi.SharedKernel.API

let createUnitConversionApi (capabilities: UnitConversionCapabilities) agent =
    { Define =
        fun req ->
            capabilities.Define req
            |> TaskResult.map (fun d -> d.NewState)
            |> TaskResult.map mapUnitConversionDto
      DefineBulk =
        fun reqs ->
            reqs
            |> List.map capabilities.Define
            |> TaskResult.sequence
            |> TaskResult.map (fun decisions ->
                decisions
                |> List.map (fun d -> d.NewState)
                |> List.map mapUnitConversionDto)
      UpdateRatio =
        fun req ->
            capabilities.UpdateRatio req
            |> TaskResult.map (fun d -> d.NewState)
            |> TaskResult.map mapUnitConversionDto
      Retire =
        fun req ->
            capabilities.Retire req
            |> TaskResult.map (fun d -> d.NewState)
            |> TaskResult.map mapUnitConversionDto }
    : UnitConversionApi

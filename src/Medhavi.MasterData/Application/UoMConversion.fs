module Medhavi.MasterData.Application.UoMConversion

open Medhavi.Common.Validation
open Medhavi.Common.Patterns
open Medhavi.SharedKernel
open Medhavi.MasterData.Domain.UomAgg
open Medhavi.MasterData.Domain.UnitConversionAgg
open Medhavi.Contracts.Integration
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
        |> Result.map (fun id ->
            { Id = id
              Ratio = req.Ratio } : UpdateUnitConversionCmd)

    let toRetireCommand (req: UnitConversionRetireReq) : Result<UnitConversionId * UnitConversionStatus, DomainError> =
        UnitConversionId.create req.Id
        |> Result.map (fun id -> (id, Retired))

type UnitConversionCapabilities =
    { Define: UnitConversionDefineReq -> TaskResult<UnitConversionEvent list, ApplicationError>
      UpdateRatio: UnitConversionUpdateReq -> TaskResult<UnitConversionEvent list, ApplicationError>
      Retire: UnitConversionRetireReq -> TaskResult<UnitConversionEvent list, ApplicationError> }

let createCapabilities (repo: Repository<UnitConversion, string, UnitConversionEvent>) =
    { Define =
        liftCmdResult ACL.toDefineCommand
        >=> handleCommand (fun (c: DefineUnitConversionCmd) -> c.Id) repo DefineUnitConversion decide
      UpdateRatio =
        liftCmdResult ACL.toUpdateRatioCommand
        >=> handleCommand (fun (c: UpdateUnitConversionCmd) -> UnitConversionId.value c.Id) repo UpdateRatio decide
      Retire =
        liftCmdResult ACL.toRetireCommand
        >=> handleCommand (fun (id, _) -> UnitConversionId.value id) repo UpdateStatus decide }

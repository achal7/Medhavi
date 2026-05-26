module Medhavi.MasterData.Application.Uom

open Medhavi.Common.Patterns
open Medhavi.SharedKernel
open Medhavi.MasterData.Domain.UomAgg
open Medhavi.Contracts.Integration
open Medhavi.Common.Validation
open Medhavi.SharedKernel.Aggregate

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

type UomCapabilities =
    { Define: UomDefineReq -> TaskResult<UnitOfMeasureEvent list, ApplicationError>
      ChangeConversionFactor: UomChangeConversionFactorReq -> TaskResult<UnitOfMeasureEvent list, ApplicationError>
      Retire: string -> TaskResult<UnitOfMeasureEvent list, ApplicationError>
      Activate: string -> TaskResult<UnitOfMeasureEvent list, ApplicationError> }

let createCapabilities (repo: Repository<UnitOfMeasure, string, UnitOfMeasureEvent>) =
    { Define =
        liftCmdResult ACL.toDefineCommand
        >=> handleCommand (fun c -> c.Code) repo Define decide
      ChangeConversionFactor =
        liftCmdResult ACL.toChangeConversionFactorCommand
        >=> handleCommand (fun c -> UomId.value c.Id) repo ChangeConversionFactor decide
      Retire =
        liftCmdResult ACL.toRetireCommand
        >=> handleCommand (fun id -> UomId.value id) repo Retire decide
      Activate =
        liftCmdResult ACL.toActivateCommand
        >=> handleCommand (fun id -> UomId.value id) repo Activate decide }

namespace Medhavi.Contracts.MasterData.Uom

open System
open System.Threading.Tasks
open Medhavi.Contracts

type UnitOfMeasure =
    { Id: string
      Code: string
      Name: string
      Status: bool
      ConversionFactor: decimal
      IsBase: bool }

type UnitConversion =
    { Id: string
      ProductId: string option
      FromUnitCode: string
      ToUnitCode: string
      Ratio: decimal
      Status: bool }

type UomDefineReq =
    { Id: string
      Code: string
      Name: string
      IsBase: bool
      ToBaseFactor: decimal
      Created: DateTimeOffset }

type UomStatusChangeReq = { Id: string; NewStatus: bool }

type UomChangeConversionFactorReq =
    { Id: string
      NewFactor: decimal
      IsBase: bool }

type UnitConversionDefineReq =
    { SourceUom: string
      TargetUom: string
      ConversionFactor: decimal
      Created: DateTimeOffset }

type UnitConversionUpdateReq = { Id: string; Ratio: decimal }

type UnitConversionRetireReq = { Id: string }

type UomApi =
    { Define: UomDefineReq -> Task<Result<UnitOfMeasure, ApiError>>
      DefineBulk: UomDefineReq list -> Task<Result<UnitOfMeasure list, ApiError>>
      ChangeConversionFactor: UomChangeConversionFactorReq -> Task<Result<UnitOfMeasure, ApiError>>
      Retire: string -> Task<Result<UnitOfMeasure, ApiError>>
      Activate: string -> Task<Result<UnitOfMeasure, ApiError>> }

type UomQueryService = QueryService<UnitOfMeasure, string>

type UnitConversionApi =
    { Define: UnitConversionDefineReq -> Task<Result<UnitConversion, ApiError>>
      DefineBulk: UnitConversionDefineReq list -> Task<Result<UnitConversion list, ApiError>>
      UpdateRatio: UnitConversionUpdateReq -> Task<Result<UnitConversion, ApiError>>
      Retire: UnitConversionRetireReq -> Task<Result<UnitConversion, ApiError>> }

type UnitConversionQueryService = QueryService<UnitConversion, string>

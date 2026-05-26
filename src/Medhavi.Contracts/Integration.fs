namespace Medhavi.Contracts.Integration

open System

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

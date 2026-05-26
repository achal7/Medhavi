module Medhavi.MasterData.Application.UoMConversion

open Medhavi.Common.Validation
open Medhavi.Common.Patterns
open Medhavi.SharedKernel
open Medhavi.MasterData.Domain.UomAgg
open Medhavi.MasterData.Domain.UnitConversionAgg
open Medhavi.Contracts.Integration

module ACL =
    let toDefineCommand (req: UnitConversionDefineReq) =
        let make (fromUom: UomId) (toUom: UomId) =
            let ucId = $"UC-{id}-{UomId.value fromUom}-{UomId.value toUom}"

            { Id = ucId
              ProductId = None
              FromUom = fromUom
              ToUom = toUom
              Ratio = req.ConversionFactor
              Created = Timestamp.create req.Created }
            : DefineUnitConversionCmd

        make
        <!> (UomId.create req.SourceUom |> fromResult)
        <*> (UomId.create req.TargetUom |> fromResult)

namespace Medhavi.Integration.Adapters

open System
open Medhavi.Contracts.Integration
open Medhavi.Integration

module BomAdapter =
    let parse (csvText: string) : Result<BomDefineReq list, string> =
        try
            let bomLines = InboundAdapter.parseBomLineCsv csvText |> Result.defaultWith (fun e -> failwith e)
            let groupedBoms = bomLines |> List.groupBy (fun b -> b.ParentSkuId)
            groupedBoms
            |> List.map (fun (parentSkuId, lines) ->
                let items = 
                    lines 
                    |> List.mapi (fun idx b -> 
                        { ComponentSkuId = b.ComponentSkuId
                          Quantity = b.QuantityRequired
                          UnitOfMeasureId = "UOM-DEFAULT"
                          Sequence = (idx + 1) * 10 })
                { Id = $"BOM-{parentSkuId}"; SkuId = parentSkuId; Items = items })
            |> Ok
        with ex ->
            Error ex.Message

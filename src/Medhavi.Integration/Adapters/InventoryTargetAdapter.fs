module Medhavi.Integration.Adapters.InventoryTarget

open System
open Medhavi.Contracts.Integration
open Medhavi.Integration

module ACL =
    let parseInventoryTargetJson json = InboundAdapter.parseJsonList<InventoryTargetDefineReq> json

    let parseInventoryTargetCsv csv =
        let rows = CsvHelper.parseCsv csv

        let parseRow (row: CsvHelper.CsvRow) =
            let sku = row.Get "SkuId" |> Option.defaultValue ""

            let sp =
                row.Get "StockingPointId"
                |> Option.defaultValue ""

            let safety = row.GetDecimal "SafetyStockQty"
            let minQty = row.GetDecimal "MinQty"
            let maxQty = row.GetDecimal "MaxQty"
            let serviceLevel = row.GetDecimal "TargetServiceLevel"
            let coverDays = row.GetDecimal "CoverDays"
            let active = row.GetBool "IsActive" |> Option.defaultValue true

            { SkuId = sku
              StockingPointId = sp
              ReplenishmentPolicy = None
              SafetyStockQty = safety
              MinQty = minQty
              MaxQty = maxQty
              TargetServiceLevel = serviceLevel
              CoverDays = coverDays
              SeasonalAdjustments = []
              EffectiveStart = None
              EffectiveEnd = None
              IsActive = active }

        rows |> Array.toList |> List.map parseRow |> Ok

    let parse (csvText: string) : Result<InventoryTargetDefineReq list, string> =
        try
            parseInventoryTargetCsv csvText
        with ex ->
            Error ex.Message

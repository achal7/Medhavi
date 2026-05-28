module Medhavi.Integration.Adapters.SupplierOffer

open System
open Medhavi.Contracts.Integration
open Medhavi.Integration

module ACL =
    // SupplierOffer Parser
    let parseSupplierOfferJson json = InboundAdapter.parseJsonList<SupplierOfferDefineReq> json

    let parseSupplierOfferCsv csv =
        let rows = CsvHelper.parseCsv csv

        let parseRow (row: CsvHelper.CsvRow) =
            let id = row.Get "Id" |> Option.defaultValue ""
            let supplier = row.Get "SupplierId" |> Option.defaultValue ""
            let sku = row.Get "SkuId" |> Option.defaultValue ""
            let sp = row.Get "StockingPointId"
            let moq = row.GetDecimal "Moq"
            let lot = row.GetDecimal "LotSize"
            let ltP50 = row.GetDecimal "LeadTimeP50Minutes"
            let ltP95 = row.GetDecimal "LeadTimeP95Minutes"
            let rel = row.GetDecimal "Reliability"
            let incoterm = row.Get "Incoterm"

            let defaultPriceTier: PriceTierReq =
                { TierNumber = 1
                  MinQuantity = 0.0m
                  MaxQuantity = None
                  PricePerUnit = 1.0m
                  Currency = "USD" }

            { Id = id
              SupplierId = supplier
              SkuId = sku
              StockingPointId = sp
              Moq = moq
              LotSize = lot
              LeadTimeP50Minutes = ltP50
              LeadTimeP95Minutes = ltP95
              PriceTiers = [ defaultPriceTier ]
              Reliability = rel
              Incoterm = incoterm
              CapacityWindows = []
              CreatedDate = DateTimeOffset.UtcNow }

        rows |> Array.toList |> List.map parseRow |> Ok

    let parse (csvText: string) : Result<SupplierOfferDefineReq list, string> =
        try
            parseSupplierOfferCsv csvText
        with ex ->
            Error ex.Message

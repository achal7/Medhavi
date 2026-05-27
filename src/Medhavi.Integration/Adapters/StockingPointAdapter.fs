namespace Medhavi.Integration.Adapters

open System
open Medhavi.Contracts.Integration
open Medhavi.Integration

module StockingPointAdapter =
    let parse (csvText: string) : Result<(StockingPointDefineReq list * NodeDefineReq list), string> =
        try
            let sps = InboundAdapter.parseStockingPointCsv csvText |> Result.defaultWith (fun e -> failwith e)
            let spReqs =
                sps
                |> List.map (fun sp ->
                    { Id = sp.StockingPointId
                      PlantId = "PLANT-DEFAULT"
                      Code = sp.StockingPointId
                      Name = sp.Name
                      Type = "Warehouse"
                      Location = None
                      Level = None
                      PlanningLevel = None
                      SupplyCanBeSplit = false })
            let nodeReqs =
                sps
                |> List.map (fun sp ->
                    { Id = sp.StockingPointId
                      Code = sp.StockingPointId
                      Name = sp.Name
                      Type = "StockingPoint"
                      Attributes = { LocationCode = None; PlanningLevel = None; StockingPointRef = Some sp.StockingPointId }
                      Created = DateTimeOffset.UtcNow })
            Ok (spReqs, nodeReqs)
        with ex ->
            Error ex.Message

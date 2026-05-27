namespace Medhavi.Integration.Adapters

open System
open Medhavi.Contracts.Integration
open Medhavi.Integration

module PlantAdapter =
    let parse (stockingPointsCsv: string) : Result<PlantDefineReq list, string> =
        try
            let sps = InboundAdapter.parseStockingPointCsv stockingPointsCsv |> Result.defaultWith (fun e -> failwith e)
            
            // Stocking points are currently mapped to PLANT-DEFAULT.
            // We map unique plant IDs from them.
            let plantIds = 
                sps
                |> List.map (fun _ -> "PLANT-DEFAULT")
                |> List.distinct

            let plantsList = 
                plantIds
                |> List.map (fun plantId ->
                    { Id = plantId
                      Code = plantId.Replace("PLANT-", "").Replace("-DEFAULT", "DEF")
                      Name = "Plant: " + plantId })
            Ok plantsList
        with ex ->
            Error ex.Message

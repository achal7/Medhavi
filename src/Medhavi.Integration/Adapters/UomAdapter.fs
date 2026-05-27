namespace Medhavi.Integration.Adapters

open System
open Medhavi.Contracts.Integration
open Medhavi.Integration

module UomAdapter =
    let parse (productsCsv: string) (transportLegsCsv: string) : Result<UomDefineReq list, string> =
        try
            let products = InboundAdapter.parseProductCsv productsCsv |> Result.defaultWith (fun e -> failwith e)
            let legs = InboundAdapter.parseTransportLegCsv transportLegsCsv |> Result.defaultWith (fun e -> failwith e)

            // Collect unique UOM ids from products and legs capacity unit
            let uomIds = 
                (products |> List.map (fun p -> p.UoM)) @ (legs |> List.choose (fun l -> l.CapacityUnit))
                |> List.distinct
            
            let uomsList = 
                uomIds
                |> List.map (fun uomId ->
                    let code = uomId.Replace("UOM-", "")
                    { Id = uomId
                      Code = code
                      Name = code
                      IsBase = true
                      ToBaseFactor = 1.0m
                      Created = DateTimeOffset.UtcNow })

            // Include a mock/derived unit for conversions testing
            let derivedUoms = [
                { Id = "UOM-BOX"
                  Code = "BOX"
                  Name = "Box of 10"
                  IsBase = false
                  ToBaseFactor = 10.0m
                  Created = DateTimeOffset.UtcNow }
            ]
            
            Ok (uomsList @ derivedUoms)
        with ex ->
            Error ex.Message

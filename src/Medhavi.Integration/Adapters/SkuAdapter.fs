namespace Medhavi.Integration.Adapters

open System
open Medhavi.Contracts.Integration
open Medhavi.Integration

module SkuAdapter =
    let parse (csvText: string) : Result<SkuDefineReq list, string> =
        try
            let products = InboundAdapter.parseProductCsv csvText |> Result.defaultWith (fun e -> failwith e)
            products
            |> List.map (fun p -> 
                { Id = p.SkuId
                  Code = p.SkuId
                  Name = p.Name
                  Group = "Simulation"
                  Created = DateTimeOffset.UtcNow })
            |> Ok
        with ex ->
            Error ex.Message

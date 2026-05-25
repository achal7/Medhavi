namespace Medhavi.Terminal

open System
open Medhavi.Contracts

module Program =
    [<EntryPoint>]
    let main argv =
        printfn "Welcome to the Medhāvī Terminal!"
        printfn "Connecting to Hub Gateway..."

        let sampleRequest =
            { OrderId = "ORD-TEST-01"
              SkuId = "SKU-TEST-ABC"
              NodeId = "NODE-TEST-XYZ"
              Quantity = 150.0m
              RequestedDate = DateTimeOffset.UtcNow.AddDays(7.0) }

        printfn "Initialized sample request for: %s (Qty: %f)" sampleRequest.OrderId (float sampleRequest.Quantity)
        0

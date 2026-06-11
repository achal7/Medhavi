module Medhavi.Terminal.Transport

open System
open Medhavi.Terminal
open Medhavi.Transport

let runDemo transportContext printer =
    printer.PrintLine PrinterColor.Bold "\n--- [TRANSPORT ATP DEMO — K-SHORTEST PATHS] ---"

    let fromNode = "SP-FACTORY"
    let toNode = "SP-CUSTOMER"
    let needDate = DateTimeOffset.UtcNow.AddDays(3.0)
    let qty = 50.0m

    printfn
        "Finding transport routes: %s → %s | NeedBy: %s | Qty: %M"
        fromNode
        toNode
        (needDate.ToString("yyyy-MM-dd"))
        qty

    let req: GetTransportOptionsReq =
        { FromNode = fromNode
          ToNode = toNode
          SkuId = Some "SKU-FRAME"
          RequiredQuantity = Some qty
          NeedByDate = needDate
          MaxHops = Some 4
          MaxItineraries = Some 5 }

    let result =
        transportContext.Atp.GetOptions req
        |> Async.RunSynchronously

    match result with
    | Error err -> printer.PrintLine PrinterColor.Red (sprintf "Transport ATP failed: %s" err)
    | Ok options ->
        printer.PrintLine PrinterColor.Green (sprintf "\nFound %d feasible transport itineraries:" options.Length)

        for i, opt in options |> List.indexed do
            printer.PrintLine
                Cyan
                (sprintf "\n  Route #%d %s" (i + 1) (if opt.IsPreferred then "★ PREFERRED" else ""))

            printfn "    Hops:         %d" opt.Itinerary.HopCount
            printfn "    Lead Time:    %.1f hours" (float opt.Itinerary.TotalLeadTimeMinutes / 60.0)
            printfn "    Est. Cost:    %M" opt.EstimatedCost
            printfn "    Reliability:  %.1f%%" (float opt.ReliabilityScore * 100.0)
            printfn "    Earliest Dep: %s" (opt.EarliestDeparture.ToString("yyyy-MM-dd HH:mm"))
            printfn "    Earliest Arr: %s" (opt.EarliestArrival.ToString("yyyy-MM-dd HH:mm"))

            opt.CO2Estimate
            |> Option.iter (fun co2 -> printfn "    CO₂ Estimate: %M kg" co2)

            printfn "    Hops detail:"

            for hop in opt.Itinerary.Hops do
                printfn
                    "      [%s] %s → %s  (%.0f min)"
                    hop.Mode
                    hop.Origin
                    hop.Destination
                    (float hop.LeadTimeMinutes)

module Medhavi.Terminal.MasterData

open Medhavi.Common.Patterns
open Medhavi.MasterData
open Medhavi.Integration
open Medhavi.SharedKernel

let loadAndValidateCsv (integrationCaps: IntegrationCapabilities) (printer: Printer) =
    printer.PrintLine PrinterColor.Bold "\n--- [STEP 1: TRIGGER CSV INGESTION & PUBLISH via IntegrationService] ---"
    let ingestTask = integrationCaps.IngestAndPublishMasterData()

    match ingestTask.Result with
    | Ok _ -> printer.PrintLine PrinterColor.Green "   [ OK ] Master data successfully published."
    | Error(Medhavi.Integration.IntegrationError.ValidationError errors) ->
        printer.PrintLine PrinterColor.Red "   [ ERR ] Validation failed with the following errors:\n"

        for err in errors do
            printer.PrintLine PrinterColor.Red (sprintf "     - %s" err)
    | Error(Medhavi.Integration.IntegrationError.IngestionError err) ->
        printer.PrintLine PrinterColor.Red (sprintf "   [ ERR ] Ingestion failed: %s" err)

// Transport context: legs are loaded from MasterData's projection on demand
let getTransportLegs masterDataContext =
    fun () ->
        async {
            let! legs =
                masterDataContext.Queries.TransportLeg.GetAll()
                |> Async.AwaitTask

            return
                legs
                |> List.filter (fun l -> l.Status)
                |> List.map (fun l ->
                    { LegId = l.Id
                      Origin = l.Origin
                      Destination = l.Destination
                      Mode = l.Mode
                      LeadTimeMinutes = l.LeadTimeMinutes
                      Capacity = l.Capacity
                      CapacityUnit = l.CapacityUnit
                      Reliability = None // enrichable from full domain leg
                      CO2PerUnit = None
                      FixedCost = 0.0m
                      VariableCostPerUnit = None
                      Status = l.Status }
                    : Medhavi.Transport.TransportLegRef)
        }

let printData masterDataContext printer =
    // 1. SKUs Table
    let skus = masterDataContext.Queries.Sku.GetAll().Result

    let skuRows =
        skus
        |> List.map (fun s -> [| s.Id; s.Code; s.Name |])
        |> List.toArray

    Printer.printTable printer "SKUs IN DATABASE" [| "SKU ID"; "CODE"; "NAME" |] skuRows

    // 2. Stocking Points Table
    let sps =
        masterDataContext.Queries.StockingPoint
            .GetAll()
            .Result

    let spRows =
        sps
        |> List.map (fun s -> [| s.Id; s.PlantId; s.Name; s.Type |])
        |> List.toArray

    Printer.printTable
        printer
        "STOCKING POINTS IN DATABASE"
        [| "STOCKING POINT ID"; "PLANT ID"; "NAME"; "TYPE" |]
        spRows

    // 3. BOM Table
    let boms = masterDataContext.Queries.Bom.GetAll().Result

    let bomRows =
        boms
        |> List.collect (fun b ->
            b.Items
            |> List.map (fun i -> [| b.Id; b.SkuId; i.ComponentSkuId; i.Quantity.ToString() |]))
        |> List.toArray

    Printer.printTable
        printer
        "BILL OF MATERIALS (BOM) RELATIONSHIPS"
        [| "BOM ID"; "PARENT SKU ID"; "COMPONENT SKU ID"; "QTY REQUIRED" |]
        bomRows

    // 4. Routings Table
    let routings = masterDataContext.Queries.Routing.GetAll().Result

    let routingRows =
        routings
        |> List.collect (fun r ->
            match r.Details with
            | Medhavi.Contracts.Domain.RoutingDetails.Work work ->
                work.Steps
                |> List.map (fun s ->
                    let resIdStr =
                        s.ResourceRequirements
                        |> List.tryHead
                        |> Option.map (fun req ->
                            req.Options
                            |> List.tryHead
                            |> Option.map (fun o -> o.ResourceGroupId)
                            |> Option.defaultValue req.RequirementId)
                        |> Option.defaultValue ""

                    let yieldStr =
                        match s.YieldPolicy with
                        | Medhavi.Contracts.Domain.StepYieldPolicy.NoYieldLoss -> "1.0"
                        | Medhavi.Contracts.Domain.StepYieldPolicy.ExpectedYield y -> y.ToString()

                    [| r.Id
                       $"WORK: {work.ProductId}"
                       r.Applicability.StockingPointId
                       |> Option.defaultValue "All"
                       s.StepId
                       s.Sequence.ToString()
                       resIdStr
                       yieldStr |])
            | Medhavi.Contracts.Domain.RoutingDetails.Transport trans ->
                [ [| r.Id
                     $"TRANSPORT: {trans.SkuId}"
                     "Move"
                     "-"
                     $"{trans.FromNodeId} -> {trans.ToNodeId}"
                     $"Lead: {trans.TransitLeadTime}m" |] ]
            | Medhavi.Contracts.Domain.RoutingDetails.Purchase pur ->
                [ [| r.Id
                     $"PURCHASE: {pur.SkuId}"
                     "Buy"
                     "-"
                     $"Supplier: {pur.SupplierId}"
                     $"Lead: {pur.SupplierLeadTime}m" |] ])
        |> List.toArray

    Printer.printTable
        printer
        "ROUTINGS AND PRODUCTION/LOGISTICS PATHS"
        [| "ROUTING ID"
           "TYPE/SKU"
           "Stocking Point"
           "STEP/OP"
           "SEQ"
           "RESOURCE GROUP / PATH"
           "YIELD / LEAD" |]
        routingRows

    // 5. Transport Legs Table
    let legs =
        masterDataContext.Queries.TransportLeg
            .GetAll()
            .Result

    let legRows =
        legs
        |> List.map (fun l ->
            let capStr =
                l.Capacity
                |> Option.map (fun c -> (float c).ToString())
                |> Option.defaultValue "Uncapped"

            [| l.Id
               l.Origin
               l.Destination
               l.Mode
               l.LeadTimeMinutes.ToString() + "m"
               capStr |])
        |> List.toArray

    Printer.printTable
        printer
        "TRANSPORT LEGS (LOGISTICS LANES)"
        [| "LEG ID"; "ORIGIN SP"; "DESTINATION SP"; "MODE"; "LEAD TIME"; "CAPACITY" |]
        legRows

    // 6. Plants Table
    let plants = masterDataContext.Queries.Plant.GetAll().Result

    let plantRows =
        plants
        |> List.map (fun p -> [| p.Id; p.Code; p.Name; (if p.Status then "Active" else "Inactive") |])
        |> List.toArray

    Printer.printTable printer "PLANTS IN DATABASE" [| "PLANT ID"; "CODE"; "NAME"; "STATUS" |] plantRows

    // 7. Unit Conversions Table
    let conversions =
        masterDataContext.Queries.UnitConversion
            .GetAll()
            .Result

    let convRows =
        conversions
        |> List.map (fun c ->
            [| c.Id
               c.FromUnitCode
               c.ToUnitCode
               c.Ratio.ToString()
               (if c.Status then "Active" else "Inactive") |])
        |> List.toArray

    Printer.printTable
        printer
        "UNIT CONVERSIONS IN DATABASE"
        [| "CONVERSION ID"; "FROM UNIT"; "TO UNIT"; "RATIO"; "STATUS" |]
        convRows

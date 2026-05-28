namespace Medhavi.Integration

open System
open Medhavi.Contracts
open Medhavi.Contracts.Integration
open Medhavi.Contracts.Domain
open Medhavi.Common.Validation
open Medhavi.Common.Serialization

module CsvHelper =
    type CsvRow =
        { Headers: string[]
          Values: string[] }

        member this.Get(columnName: string) : string option =
            let idx =
                Array.tryFindIndex
                    (fun (h: string) ->
                        h
                            .Trim()
                            .Equals(columnName, StringComparison.OrdinalIgnoreCase))
                    this.Headers

            match idx with
            | Some i when i < this.Values.Length ->
                let v = this.Values.[i].Trim()
                if String.IsNullOrEmpty(v) then None else Some v
            | _ -> None

        member this.GetDecimal(columnName: string) : decimal option =
            this.Get columnName
            |> Option.bind (fun v ->
                match System.Decimal.TryParse(v) with
                | true, d -> Some d
                | _ -> None)

        member this.GetFloat(columnName: string) : float option =
            this.Get columnName
            |> Option.bind (fun v ->
                match System.Double.TryParse(v) with
                | true, f -> Some f
                | _ -> None)

        member this.GetInt(columnName: string) : int option =
            this.Get columnName
            |> Option.bind (fun v ->
                match System.Int32.TryParse(v) with
                | true, i -> Some i
                | _ -> None)

        member this.GetBool(columnName: string) : bool option =
            this.Get columnName
            |> Option.bind (fun v ->
                match System.Boolean.TryParse(v) with
                | true, b -> Some b
                | _ ->
                    match v.ToLowerInvariant() with
                    | "1"
                    | "yes"
                    | "true" -> Some true
                    | "0"
                    | "no"
                    | "false" -> Some false
                    | _ -> None)

        member this.GetDateTimeOffset(columnName: string) : DateTimeOffset option =
            this.Get columnName
            |> Option.bind (fun v ->
                match System.DateTimeOffset.TryParse(v) with
                | true, dto -> Some dto
                | _ -> None)

    let splitCsvLine (line: string) : string[] =
        let matches =
            System.Text.RegularExpressions.Regex.Matches(line, "(?<=^|,)(\"(?:[^\"]|\"\")*\"|[^,]*)")

        [| for m in matches -> m.Value.Trim().Trim('"').Replace("\"\"", "\"") |]

    let parseCsv (csvText: string) : CsvRow[] =
        let rawLines =
            csvText.Split([| '\r'; '\n' |], StringSplitOptions.RemoveEmptyEntries)

        if rawLines.Length <= 1 then
            [||]
        else
            let headers =
                rawLines.[0].Split([| ',' |])
                |> Array.map (fun s -> s.Trim().Trim('"'))

            rawLines.[1..]
            |> Array.filter (fun line -> not (String.IsNullOrWhiteSpace(line)))
            |> Array.map (fun line ->
                { Headers = headers
                  Values = splitCsvLine line })

/// Anti-Corruption Layer (ACL) normalizer to transform external systems data
module InboundAdapter =

    /// Normalizes raw ERP unit conversion payloads to unit conversion DTO
    let normalizeUnitConversion
        (rawId: string)
        (rawProd: string)
        (rawFrom: string)
        (rawTo: string)
        (rawRatio: decimal)
        =
        let cleanProdId =
            if String.IsNullOrWhiteSpace(rawProd) then
                None
            else
                Some(rawProd.Trim())

        { Id = rawId.Trim()
          ProductId = cleanProdId
          FromUnitCode = rawFrom.Trim().ToUpper()
          ToUnitCode = rawTo.Trim().ToUpper()
          Ratio = rawRatio
          Status = true }

    // Helpers to parse JSON payloads generic
    let parseJsonList<'T> (json: string) : Result<'T list, string> =
        match deserialize<'T list> json with
        | Ok value -> Ok value
        | Error err ->
            match deserialize<'T> json with
            | Ok singleVal -> Ok [ singleVal ]
            | Error _ -> Error(sprintf "JSON parsing error: %A" err)

    // UOM Parser
    let parseUomJson json = parseJsonList<UomDefineReq> json

    let parseUomCsv csv =
        let rows = CsvHelper.parseCsv csv

        let parseRow (row: CsvHelper.CsvRow) =
            let id = row.Get "Id" |> Option.defaultValue ""
            let code = row.Get "Code" |> Option.defaultValue ""
            let name = row.Get "Name" |> Option.defaultValue ""
            let isBase = row.GetBool "IsBase" |> Option.defaultValue false

            let factor =
                row.GetDecimal "ToBaseFactor"
                |> Option.defaultValue 1.0m

            let created =
                row.GetDateTimeOffset "Created"
                |> Option.defaultValue DateTimeOffset.UtcNow

            { Id = id
              Code = code
              Name = name
              IsBase = isBase
              ToBaseFactor = factor
              Created = created }

        rows |> Array.toList |> List.map parseRow |> Ok

    // UnitConversion Parser
    let parseUnitConversionJson json = parseJsonList<UnitConversionDefineReq> json

    let parseUnitConversionCsv csv =
        let rows = CsvHelper.parseCsv csv

        let parseRow (row: CsvHelper.CsvRow) =
            let src = row.Get "SourceUom" |> Option.defaultValue ""
            let target = row.Get "TargetUom" |> Option.defaultValue ""

            let factor =
                row.GetDecimal "ConversionFactor"
                |> Option.defaultValue 1.0m

            let created =
                row.GetDateTimeOffset "Created"
                |> Option.defaultValue DateTimeOffset.UtcNow

            { SourceUom = src
              TargetUom = target
              ConversionFactor = factor
              Created = created }

        rows |> Array.toList |> List.map parseRow |> Ok

    // Product Parser
    let parseProductJson json = parseJsonList<ProductImportedPayload> json

    let parseProductCsv csv =
        let rows = CsvHelper.parseCsv csv

        let parseRow (row: CsvHelper.CsvRow) =
            let id = row.Get "SkuId" |> Option.defaultValue ""
            let name = row.Get "Name" |> Option.defaultValue ""
            let uom = row.Get "UoM" |> Option.defaultValue ""
            let active = row.GetBool "IsActive" |> Option.defaultValue true

            { SkuId = id
              Name = name
              UoM = uom
              IsActive = active }

        rows |> Array.toList |> List.map parseRow |> Ok

    // BomLine Parser
    let parseBomLineJson json = parseJsonList<BomLineImportedPayload> json

    let parseBomLineCsv csv =
        let rows = CsvHelper.parseCsv csv

        let parseRow (row: CsvHelper.CsvRow) =
            let parent = row.Get "ParentSkuId" |> Option.defaultValue ""
            let comp = row.Get "ComponentSkuId" |> Option.defaultValue ""

            let qty =
                row.GetDecimal "QuantityRequired"
                |> Option.defaultValue 0.0m

            { ParentSkuId = parent
              ComponentSkuId = comp
              QuantityRequired = qty }

        rows |> Array.toList |> List.map parseRow |> Ok

    // StockingPoint Parser
    let parseStockingPointJson json = parseJsonList<StockingPointImportedPayload> json

    let parseStockingPointCsv csv =
        let rows = CsvHelper.parseCsv csv

        let parseRow (row: CsvHelper.CsvRow) =
            let id =
                row.Get "StockingPointId"
                |> Option.defaultValue ""

            let name = row.Get "Name" |> Option.defaultValue ""
            let active = row.GetBool "IsActive" |> Option.defaultValue true

            { StockingPointId = id
              Name = name
              IsActive = active }

        rows |> Array.toList |> List.map parseRow |> Ok

    // Resource Parser
    let parseResourceJson json = parseJsonList<ResourceImportedPayload> json

    let parseResourceCsv csv =
        let rows = CsvHelper.parseCsv csv

        let parseRow (row: CsvHelper.CsvRow) =
            let id = row.Get "ResourceId" |> Option.defaultValue ""
            let name = row.Get "Name" |> Option.defaultValue ""
            let nodeId = row.Get "NodeId" |> Option.defaultValue ""
            let active = row.GetBool "IsActive" |> Option.defaultValue true

            { ResourceId = id
              Name = name
              NodeId = nodeId
              IsActive = active }

        rows |> Array.toList |> List.map parseRow |> Ok

    // Routing & Steps Parser
    let parseRoutingJson json = parseJsonList<RoutingImportedPayload> json

    let parseRoutingCsv csv =
        let rows = CsvHelper.parseCsv csv

        let parseRow (row: CsvHelper.CsvRow) =
            let skuId = row.Get "SkuId" |> Option.defaultValue ""
            let seq = row.GetInt "Sequence" |> Option.defaultValue 0
            let resId = row.Get "ResourceId" |> Option.defaultValue ""

            let setup =
                row.GetFloat "SetupHours"
                |> Option.defaultValue 0.0

            let run =
                row.GetFloat "RunHoursPerUnit"
                |> Option.defaultValue 0.0

            (skuId,
             { Sequence = seq
               ResourceId = resId
               SetupHours = setup
               RunHoursPerUnit = run })

        // Group by SkuId to form RoutingImportedPayload
        let flat = rows |> Array.toList |> List.map parseRow

        flat
        |> List.groupBy fst
        |> List.map (fun (skuId, items) ->
            { SkuId = skuId
              Steps = items |> List.map snd })
        |> Ok

    // Supplier Parser
    let parseSupplierJson json = parseJsonList<SupplierImportedPayload> json

    let parseSupplierCsv csv =
        let rows = CsvHelper.parseCsv csv

        let parseRow (row: CsvHelper.CsvRow) =
            let id = row.Get "SupplierId" |> Option.defaultValue ""
            let name = row.Get "Name" |> Option.defaultValue ""
            let active = row.GetBool "IsActive" |> Option.defaultValue true

            { SupplierId = id
              Name = name
              IsActive = active }

        rows |> Array.toList |> List.map parseRow |> Ok

    // TransportLeg Parser
    let parseTransportLegJson json = parseJsonList<TransportLegDefineReq> json

    let parseTransportLegCsv csv =
        let rows = CsvHelper.parseCsv csv

        let parseRow (row: CsvHelper.CsvRow) =
            let id = row.Get "Id" |> Option.defaultValue ""
            let origin = row.Get "Origin" |> Option.defaultValue ""
            let dest = row.Get "Destination" |> Option.defaultValue ""
            let mode = row.Get "Mode" |> Option.defaultValue ""
            let schedule = row.Get "Schedule" |> Option.defaultValue ""

            let lt =
                row.GetDecimal "LeadTimeMinutes"
                |> Option.defaultValue 0.0m

            let cap = row.GetDecimal "Capacity"
            let capUnit = row.Get "CapacityUnit"
            let cutoff = row.GetDecimal "CutoffMinutes"

            let constraints =
                match row.Get "Constraints" with
                | None -> []
                | Some s ->
                    s.Split([| '|'; ';' |], StringSplitOptions.RemoveEmptyEntries)
                    |> Array.toList
                    |> List.map (fun x -> x.Trim())

            let rel = row.GetDecimal "Reliability"
            let co2 = row.GetDecimal "CO2PerUnit"

            let start =
                row.GetDateTimeOffset "EffectiveStart"
                |> Option.defaultValue DateTimeOffset.UtcNow

            let end' = row.GetDateTimeOffset "EffectiveEnd"

            let created =
                row.GetDateTimeOffset "Created"
                |> Option.defaultValue DateTimeOffset.UtcNow

            { Id = id
              Origin = origin
              Destination = dest
              Mode = mode
              Schedule = schedule
              LeadTimeMinutes = lt
              Capacity = cap
              CapacityUnit = capUnit
              CutoffMinutes = cutoff
              Constraints = constraints
              Reliability = rel
              CO2PerUnit = co2
              EffectiveStart = start
              EffectiveEnd = end'
              Created = created }

        rows |> Array.toList |> List.map parseRow |> Ok

    // CustomerOrder Parser
    let parseCustomerOrderJson json = parseJsonList<CustomerOrderReceivedPayload> json

    let parseCustomerOrderCsv csv =
        let rows = CsvHelper.parseCsv csv

        let parseRow (row: CsvHelper.CsvRow) =
            let orderId = row.Get "OrderId" |> Option.defaultValue ""
            let skuId = row.Get "SkuId" |> Option.defaultValue ""
            let nodeId = row.Get "NodeId" |> Option.defaultValue ""

            let qty =
                row.GetDecimal "Quantity"
                |> Option.defaultValue 0.0m

            let dt =
                row.GetDateTimeOffset "RequestedDateUtc"
                |> Option.defaultValue DateTimeOffset.UtcNow

            { OrderId = orderId
              SkuId = skuId
              NodeId = nodeId
              Quantity = qty
              RequestedDateUtc = dt }

        rows |> Array.toList |> List.map parseRow |> Ok

    // Forecast Parser
    let parseForecastJson json = parseJsonList<ForecastReceivedPayload> json

    let parseForecastCsv csv =
        let rows = CsvHelper.parseCsv csv

        let parseRow (row: CsvHelper.CsvRow) =
            let fcId = row.Get "ForecastId" |> Option.defaultValue ""
            let skuId = row.Get "SkuId" |> Option.defaultValue ""
            let nodeId = row.Get "NodeId" |> Option.defaultValue ""

            let qty =
                row.GetDecimal "Quantity"
                |> Option.defaultValue 0.0m

            let dt =
                row.GetDateTimeOffset "ForecastDateUtc"
                |> Option.defaultValue DateTimeOffset.UtcNow

            { ForecastId = fcId
              SkuId = skuId
              NodeId = nodeId
              Quantity = qty
              ForecastDateUtc = dt }

        rows |> Array.toList |> List.map parseRow |> Ok

    // InventoryPosition Parser
    let parseInventoryPositionJson json = parseJsonList<InventoryPositionPayload> json

    let parseInventoryPositionCsv csv =
        let rows = CsvHelper.parseCsv csv

        let parseRow (row: CsvHelper.CsvRow) =
            let prod = row.Get "ProductId" |> Option.defaultValue ""

            let sp =
                row.Get "StockingPointId"
                |> Option.defaultValue ""

            let qty =
                row.GetDecimal "Quantity"
                |> Option.defaultValue 0.0m

            let dt =
                row.GetDateTimeOffset "AsOfUtc"
                |> Option.defaultValue DateTimeOffset.UtcNow

            { ProductId = prod
              StockingPointId = sp
              Quantity = qty
              AsOfUtc = dt }

        rows |> Array.toList |> List.map parseRow |> Ok

    // SupplyOrderStatus Parser
    let parseSupplyOrderStatusJson json = parseJsonList<SupplyOrderStatusPayload> json

    let parseSupplyOrderStatusCsv csv =
        let rows = CsvHelper.parseCsv csv

        let parseRow (row: CsvHelper.CsvRow) =
            let id = row.Get "SupplyOrderId" |> Option.defaultValue ""
            let prod = row.Get "ProductId" |> Option.defaultValue ""

            let sp =
                row.Get "StockingPointId"
                |> Option.defaultValue ""

            let qty =
                row.GetDecimal "Quantity"
                |> Option.defaultValue 0.0m

            let dt =
                row.GetDateTimeOffset "ExpectedDeliveryUtc"
                |> Option.defaultValue DateTimeOffset.UtcNow

            let status = row.Get "Status" |> Option.defaultValue ""

            { SupplyOrderId = id
              ProductId = prod
              StockingPointId = sp
              Quantity = qty
              ExpectedDeliveryUtc = dt
              Status = status }

        rows |> Array.toList |> List.map parseRow |> Ok

    // ResourceCalendar Parser
    let parseResourceCalendarJson json = parseJsonList<ResourceCalendarPayload> json

    let parseResourceCalendarCsv csv =
        let rows = CsvHelper.parseCsv csv

        let parseRow (row: CsvHelper.CsvRow) =
            let resId = row.Get "ResourceId" |> Option.defaultValue ""

            let start =
                row.GetDateTimeOffset "StartUtc"
                |> Option.defaultValue DateTimeOffset.UtcNow

            let end' =
                row.GetDateTimeOffset "EndUtc"
                |> Option.defaultValue DateTimeOffset.UtcNow

            let factor =
                row.GetFloat "CapacityFactor"
                |> Option.defaultValue 1.0

            let reason = row.Get "Reason"

            { ResourceId = resId
              StartUtc = start
              EndUtc = end'
              CapacityFactor = factor
              Reason = reason }

        rows |> Array.toList |> List.map parseRow |> Ok

    // WorkOrderCompleted Parser
    let parseWorkOrderCompletedJson json = parseJsonList<WorkOrderCompletedPayload> json

    let parseWorkOrderCompletedCsv csv =
        let rows = CsvHelper.parseCsv csv

        let parseRow (row: CsvHelper.CsvRow) =
            let id = row.Get "WorkOrderId" |> Option.defaultValue ""
            let routing = row.Get "RoutingId" |> Option.defaultValue ""

            let qty =
                row.GetDecimal "QuantityCompleted"
                |> Option.defaultValue 0.0m

            let dt =
                row.GetDateTimeOffset "CompletedAtUtc"
                |> Option.defaultValue DateTimeOffset.UtcNow

            { WorkOrderId = id
              RoutingId = routing
              QuantityCompleted = qty
              CompletedAtUtc = dt }

        rows |> Array.toList |> List.map parseRow |> Ok

    // MaterialReceived Parser
    let parseMaterialReceivedJson json = parseJsonList<MaterialReceivedPayload> json

    let parseMaterialReceivedCsv csv =
        let rows = CsvHelper.parseCsv csv

        let parseRow (row: CsvHelper.CsvRow) =
            let id = row.Get "ReceiptId" |> Option.defaultValue ""
            let prod = row.Get "ProductId" |> Option.defaultValue ""

            let sp =
                row.Get "StockingPointId"
                |> Option.defaultValue ""

            let qty =
                row.GetDecimal "QuantityReceived"
                |> Option.defaultValue 0.0m

            let dt =
                row.GetDateTimeOffset "ReceivedAtUtc"
                |> Option.defaultValue DateTimeOffset.UtcNow

            { ReceiptId = id
              ProductId = prod
              StockingPointId = sp
              QuantityReceived = qty
              ReceivedAtUtc = dt }

        rows |> Array.toList |> List.map parseRow |> Ok

    // ResourceDowntime Parser
    let parseResourceDowntimeJson json = parseJsonList<ResourceDowntimePayload> json

    let parseResourceDowntimeCsv csv =
        let rows = CsvHelper.parseCsv csv

        let parseRow (row: CsvHelper.CsvRow) =
            let resId = row.Get "ResourceId" |> Option.defaultValue ""

            let start =
                row.GetDateTimeOffset "StartUtc"
                |> Option.defaultValue DateTimeOffset.UtcNow

            let end' =
                row.GetDateTimeOffset "EndUtc"
                |> Option.defaultValue DateTimeOffset.UtcNow

            let reason = row.Get "Reason" |> Option.defaultValue ""

            { ResourceId = resId
              StartUtc = start
              EndUtc = end'
              Reason = reason }

        rows |> Array.toList |> List.map parseRow |> Ok

    // TransportDelay Parser
    let parseTransportDelayJson json = parseJsonList<TransportDelayPayload> json

    let parseTransportDelayCsv csv =
        let rows = CsvHelper.parseCsv csv

        let parseRow (row: CsvHelper.CsvRow) =
            let id = row.Get "TransportLegId" |> Option.defaultValue ""

            let lt =
                row.GetFloat "EstimatedDelayMinutes"
                |> Option.defaultValue 0.0

            let dt =
                row.GetDateTimeOffset "NewArrivalUtc"
                |> Option.defaultValue DateTimeOffset.UtcNow

            let reason = row.Get "Reason" |> Option.defaultValue ""

            { TransportLegId = id
              EstimatedDelayMinutes = lt
              NewArrivalUtc = dt
              Reason = reason }

        rows |> Array.toList |> List.map parseRow |> Ok

module MasterDataValidator =

    /// Validates cross-references and format constraints of MasterDataPayload using Validation applicative functor
    let validate (payload: MasterDataPayload) : Validation<MasterDataPayload, string> =

        let products = payload.Products
        let stockingPoints = payload.StockingPoints
        let resources = payload.Resources
        let boms = payload.Boms
        let routings = payload.Routings
        let suppliers = payload.Suppliers

        let productIds =
            products
            |> List.map (fun p -> p.SkuId)
            |> Set.ofList

        let stockingPointIds =
            stockingPoints
            |> List.map (fun s -> s.StockingPointId)
            |> Set.ofList

        let resourceIds =
            resources
            |> List.map (fun r -> r.ResourceId)
            |> Set.ofList

        // Validation 1: Product IDs are unique
        let validateUniqueProductIds =
            let duplicates =
                products
                |> List.groupBy (fun p -> p.SkuId)
                |> List.filter (fun (_, g) -> g.Length > 1)
                |> List.map fst

            if List.isEmpty duplicates then
                Valid()
            else
                Invalid [ sprintf "Duplicate Product IDs found: %A" duplicates ]

        // Validation 2: StockingPoint IDs are unique
        let validateUniqueStockingPointIds =
            let duplicates =
                stockingPoints
                |> List.groupBy (fun s -> s.StockingPointId)
                |> List.filter (fun (_, g) -> g.Length > 1)
                |> List.map fst

            if List.isEmpty duplicates then
                Valid()
            else
                Invalid [ sprintf "Duplicate Stocking Point IDs found: %A" duplicates ]

        // Validation 3: BOM Parent and Component SkuIds must exist in Products
        let validateBomReferences =
            let invalidBoms =
                boms
                |> List.filter (fun b ->
                    not (productIds.Contains(b.ParentSkuId))
                    || not (productIds.Contains(b.ComponentSkuId)))

            if List.isEmpty invalidBoms then
                Valid()
            else
                let details =
                    invalidBoms
                    |> List.map (fun b -> sprintf "Parent: %s, Component: %s" b.ParentSkuId b.ComponentSkuId)

                Invalid [ sprintf "BOM Lines refer to missing Product IDs: %A" details ]

        // Validation 4: Resources refer to valid NodeIds (Stocking Points)
        let validateResourceReferences =
            let invalidResources =
                resources
                |> List.filter (fun r -> not (stockingPointIds.Contains(r.NodeId)))

            if List.isEmpty invalidResources then
                Valid()
            else
                let details =
                    invalidResources
                    |> List.map (fun r -> sprintf "Resource: %s, Missing Node: %s" r.ResourceId r.NodeId)

                Invalid [ sprintf "Resources refer to missing Node/StockingPoint IDs: %A" details ]

        // Validation 5: Routings steps refer to valid ResourceIds and SkuId exists
        let validateRoutingReferences =
            let invalidRoutings =
                routings
                |> List.filter (fun r -> not (productIds.Contains(r.SkuId)))

            let invalidSteps =
                routings
                |> List.collect (fun r ->
                    r.Steps
                    |> List.filter (fun s -> not (resourceIds.Contains(s.ResourceId)))
                    |> List.map (fun s -> sprintf "Routing Sku: %s, Missing Resource: %s" r.SkuId s.ResourceId))

            match List.isEmpty invalidRoutings, List.isEmpty invalidSteps with
            | true, true -> Valid()
            | false, _ ->
                let details = invalidRoutings |> List.map (fun r -> r.SkuId)
                Invalid [ sprintf "Routings refer to missing Sku IDs: %A" details ]
            | _, false -> Invalid [ sprintf "Routing steps refer to missing Resource IDs: %A" invalidSteps ]

        // Combine all validations using Applicative <*>
        let rebuildPayload () () () () () = payload

        rebuildPayload <!> validateUniqueProductIds
        <*> validateUniqueStockingPointIds
        <*> validateBomReferences
        <*> validateResourceReferences
        <*> validateRoutingReferences

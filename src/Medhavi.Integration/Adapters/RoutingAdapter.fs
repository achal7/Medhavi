module Medhavi.Integration.Adapters.Routing

open System
open System.Threading
open Medhavi.Common.Patterns
open Medhavi.Contracts.MasterData.Routing
open Medhavi.Integration
open Medhavi.Infrastructure.IO
open Medhavi.Infrastructure.Stores.EnvelopeStore
open Medhavi.Infrastructure

module ACL =
    let parse (routingCsv: string) (routingStepsCsv: string) (resourceCsv: string) (bomLinesCsv: string) : Result<RoutingDefineReq list, string> =
        try
            // Parse resourceCsv
            let resourceRows = CsvHelper.parseCsv resourceCsv
            let resources =
                resourceRows
                |> Array.toList
                |> List.map (fun row ->
                    let id = row.Get "ResourceId" |> Option.defaultValue ""
                    let name = row.Get "Name" |> Option.defaultValue ""
                    let nodeId = row.Get "NodeId" |> Option.defaultValue ""
                    let active = row.GetBool "IsActive" |> Option.defaultValue true
                    {| ResourceId = id; Name = name; NodeId = nodeId; IsActive = active |})

            // Parse bomLinesCsv
            let bomRows = CsvHelper.parseCsv bomLinesCsv
            let bomLines =
                bomRows
                |> Array.toList
                |> List.map (fun row ->
                    let parent = row.Get "ParentSkuId" |> Option.defaultValue ""
                    let comp = row.Get "ComponentSkuId" |> Option.defaultValue ""
                    let qty = row.GetDecimal "QuantityRequired" |> Option.defaultValue 0.0m
                    {| ParentSkuId = parent; ComponentSkuId = comp; QuantityRequired = qty |})

            // Parse routing_steps.csv
            let stepRows = CsvHelper.parseCsv routingStepsCsv
            let stepLines =
                stepRows
                |> Array.toList
                |> List.map (fun row ->
                    {| RoutingId = row.Get "RoutingId" |> Option.defaultValue ""
                       StepId = row.Get "StepId" |> Option.defaultValue ""
                       Sequence = row.GetInt "Sequence" |> Option.defaultValue 10
                       OperationCode = row.Get "OperationCode" |> Option.defaultValue ""
                       Name = row.Get "Name" |> Option.defaultValue ""
                       Description = row.Get "Description"
                       Kind = row.Get "Kind" |> Option.defaultValue "Standard"
                       YieldPercentage = row.GetDecimal "YieldPercentage"
                       ReworkStepId = row.Get "ReworkStepId"
                       ReworkRate = row.GetDecimal "ReworkRate"
                       OverlapPolicyType = row.Get "OverlapPolicyType" |> Option.defaultValue "NoOverlap"
                       OverlapPolicyValue = row.GetDecimal "OverlapPolicyValue"
                       EffectiveStart = row.GetDateTimeOffset "EffectiveStart"
                       EffectiveEnd = row.GetDateTimeOffset "EffectiveEnd"
                       ResourceRequirementId = row.Get "ResourceRequirementId"
                       ResourceKind = row.Get "ResourceKind" |> Option.defaultValue "WorkCenter"
                       ResourceLoadBasis = row.Get "ResourceLoadBasis" |> Option.defaultValue "PerUnit"
                       ResourceRequiredUnits = row.GetDecimal "ResourceRequiredUnits" |> Option.defaultValue 1.0m
                       ResourceSelectionRule = row.Get "ResourceSelectionRule" |> Option.defaultValue "AnyAllowed"
                       OptionId = row.Get "OptionId"
                       ResourceGroupId = row.Get "ResourceGroupId"
                       ResourceId = row.Get "ResourceId"
                       WorkCenterId = row.Get "WorkCenterId"
                       ResourceUsage = row.Get "ResourceUsage" |> Option.defaultValue "Primary"
                       ResourcePriority = row.GetInt "ResourcePriority"
                       SetupTimeMinutes = row.GetDecimal "SetupTimeMinutes"
                       RunTimePerBaseQuantityMinutes = row.GetDecimal "RunTimePerBaseQuantityMinutes"
                       TeardownTimeMinutes = row.GetDecimal "TeardownTimeMinutes"
                       CoolingTimeMinutes = row.GetDecimal "CoolingTimeMinutes"
                       MinLeadTimeMinutes = row.GetDecimal "MinLeadTimeMinutes"
                       CostPerMinute = row.GetDecimal "CostPerMinute"
                       EfficiencyFactor = row.GetDecimal "EfficiencyFactor"
                       SetupTimeFixed = row.GetDecimal "SetupTimeFixed"
                       CoolingTimeFixed = row.GetDecimal "CoolingTimeFixed"
                       OptionEffectiveStart = row.GetDateTimeOffset "OptionEffectiveStart"
                       OptionEffectiveEnd = row.GetDateTimeOffset "OptionEffectiveEnd" |})

            // Parse routingCsv
            let routingRows = CsvHelper.parseCsv routingCsv
            let reqs =
                routingRows
                |> Array.toList
                |> List.map (fun row ->
                    let routingId = row.Get "RoutingId" |> Option.defaultValue ""
                    let name = row.Get "Name" |> Option.defaultValue ""
                    let description = row.Get "Description"
                    let rType = row.Get "RoutingType" |> Option.defaultValue "Work"
                    let stockingPointId = row.Get "StockingPointId"
                    let effectiveStart = row.GetDateTimeOffset "EffectiveStart" |> Option.defaultValue DateTimeOffset.UtcNow
                    let effectiveEnd = row.GetDateTimeOffset "EffectiveEnd"
                    let preferencePriority = row.GetInt "PreferencePriority" |> Option.defaultValue 1
                    let isPreferred = row.GetBool "IsPreferred" |> Option.defaultValue false
                    let minQty = row.GetDecimal "MinQuantity"
                    let maxQty = row.GetDecimal "MaxQuantity"
                    let lotSize = row.GetDecimal "LotSize"
                    let orderMultiple = row.GetDecimal "OrderMultiple"
                    let costPolicyType = row.Get "CostPolicyType" |> Option.defaultValue "NoRoutingCost"
                    let costPolicyValue = row.GetDecimal "CostPolicyValue"

                    let details =
                        match rType.Trim().ToLowerInvariant() with
                        | "work" ->
                            let skuId = row.Get "SkuId" |> Option.defaultValue ""
                            // Filter and group steps for this routing
                            let matchingSteps = stepLines |> List.filter (fun s -> s.RoutingId = routingId)

                            // Map matching steps to RoutingStepReq
                            let steps =
                                matchingSteps
                                |> List.groupBy (fun s -> s.StepId)
                                |> List.map (fun (stepId, stepRows) ->
                                    let firstRow = stepRows |> List.head

                                    // Build resource requirements for this step
                                    let resourceRequirements =
                                        stepRows
                                        |> List.groupBy (fun s -> s.ResourceRequirementId |> Option.defaultValue $"REQ-{stepId}")
                                        |> List.map (fun (reqId, reqRows) ->
                                            let reqFirstRow = reqRows |> List.head

                                            // Build resource options
                                            let options =
                                                reqRows
                                                |> List.choose (fun rRow ->
                                                    match rRow.OptionId with
                                                    | None -> None
                                                    | Some optId ->
                                                        let rgId = rRow.ResourceGroupId |> Option.defaultValue "RG-DEFAULT"
                                                        Some { OptionId = optId
                                                               ResourceGroupId = rgId
                                                               ResourceId = rRow.ResourceId
                                                               WorkCenterId = rRow.WorkCenterId
                                                               Usage = rRow.ResourceUsage
                                                               Priority = rRow.ResourcePriority
                                                               SetupTimeMinutes = rRow.SetupTimeMinutes
                                                               RunTimePerBaseQuantityMinutes = rRow.RunTimePerBaseQuantityMinutes
                                                               TeardownTimeMinutes = rRow.TeardownTimeMinutes
                                                               CoolingTimeMinutes = rRow.CoolingTimeMinutes
                                                               MinLeadTimeMinutes = rRow.MinLeadTimeMinutes
                                                               CostPerMinute = rRow.CostPerMinute
                                                               EfficiencyFactor = rRow.EfficiencyFactor
                                                               SetupTimeFixed = rRow.SetupTimeFixed
                                                               CoolingTimeFixed = rRow.CoolingTimeFixed
                                                               EffectiveStart = rRow.OptionEffectiveStart
                                                               EffectiveEnd = rRow.OptionEffectiveEnd })

                                            { RequirementId = reqId
                                              ResourceKind = reqFirstRow.ResourceKind
                                              LoadBasis = reqFirstRow.ResourceLoadBasis
                                              RequiredUnits = reqFirstRow.ResourceRequiredUnits
                                              SelectionRule = reqFirstRow.ResourceSelectionRule
                                              SelectionRuleGroupId = None
                                              Options = options })

                                    { StepId = stepId
                                      Sequence = firstRow.Sequence
                                      OperationCode = firstRow.OperationCode
                                      Name = firstRow.Name
                                      Description = firstRow.Description
                                      Kind = firstRow.Kind
                                      Inputs = []
                                      Outputs = []
                                      ResourceRequirements = resourceRequirements
                                      TimingProfile =
                                          { FixedLeadTime = None
                                            QueueTime = None
                                            WaitTime = None
                                            MoveTime = None }
                                      YieldPercentage = firstRow.YieldPercentage
                                      ReworkStepId = firstRow.ReworkStepId
                                      ReworkRate = firstRow.ReworkRate
                                      OverlapPolicyType = firstRow.OverlapPolicyType
                                      OverlapPolicyValue = firstRow.OverlapPolicyValue
                                      EffectiveStart = firstRow.EffectiveStart
                                      EffectiveEnd = firstRow.EffectiveEnd })
                                |> List.sortBy (fun s -> s.Sequence)

                            let firstStepOpt = steps |> List.tryHead
                            let lastStepOpt = steps |> List.tryLast
                            let firstStepId =
                                match firstStepOpt with
                                | Some s -> s.StepId
                                | None -> $"STEP-{skuId}-10"
                            let lastStepId =
                                match lastStepOpt with
                                | Some s -> s.StepId
                                | None -> $"STEP-{skuId}-10"

                            let getStepNodeId (stepOpt: RoutingStepReq option) =
                                match stepOpt with
                                | None -> "SP-FACTORY"
                                | Some s ->
                                    let resIdOpt =
                                        s.ResourceRequirements
                                        |> List.collect (fun r -> r.Options)
                                        |> List.tryPick (fun o -> o.ResourceId)
                                    match resIdOpt with
                                    | None -> "SP-FACTORY"
                                    | Some rId ->
                                        resources
                                        |> List.tryFind (fun res -> res.ResourceId = rId)
                                        |> Option.map (fun res -> res.NodeId)
                                        |> Option.defaultValue "SP-FACTORY"

                            let firstStepNodeId = getStepNodeId firstStepOpt
                            let lastStepNodeId = getStepNodeId lastStepOpt

                            let relatedBoms =
                                bomLines
                                |> List.filter (fun b -> b.ParentSkuId = skuId)

                            let stepInputs =
                                if List.isEmpty relatedBoms then
                                    [ { SkuId = "SKU-FRAME"
                                        FromNodeId = Some firstStepNodeId
                                        QuantityPerBaseOutput = Some 1.0m
                                        Timing = StepInputTimingReq.AtStepStart
                                        IsConsumed = true
                                        IsOptional = false } ]
                                else
                                    relatedBoms
                                    |> List.map (fun b ->
                                        { SkuId = b.ComponentSkuId
                                          FromNodeId = Some firstStepNodeId
                                          QuantityPerBaseOutput = Some b.QuantityRequired
                                          Timing = StepInputTimingReq.AtStepStart
                                          IsConsumed = true
                                          IsOptional = false })

                            let stepOutputs =
                                [ { SkuId = skuId
                                    ToNodeId = Some lastStepNodeId
                                    QuantityRatioToPrimaryOutput = Some 1.0m
                                    Role = RoutingOutputRoleReq.PrimaryOutput
                                    Timing = StepOutputTimingReq.AtStepEnd } ]

                            let stepsWithInputsAndOutputs =
                                steps
                                |> List.map (fun s ->
                                    if s.StepId = firstStepId && s.StepId = lastStepId then
                                        { s with Inputs = stepInputs; Outputs = stepOutputs }
                                    elif s.StepId = firstStepId then
                                        { s with Inputs = stepInputs }
                                    elif s.StepId = lastStepId then
                                        { s with Outputs = stepOutputs }
                                    else
                                        s)

                            WorkDetails
                                { ProductId = skuId
                                  PrimaryOutputSkuId = skuId
                                  BaseOutputQuantity = 1.0m
                                  Steps = stepsWithInputsAndOutputs }

                        | "transport" ->
                            let skuId = row.Get "SkuId" |> Option.defaultValue ""
                            let fromNode = row.Get "TransportFromNodeId" |> Option.defaultValue "SP-FACTORY"
                            let toNode = row.Get "TransportToNodeId" |> Option.defaultValue "SP-FACTORY"
                            let mode = row.Get "TransportMode" |> Option.defaultValue "Road"
                            let leadTime = row.GetDecimal "TransitLeadTime" |> Option.defaultValue 0.0m
                            let loss = row.GetDecimal "LossFactor"
                            let selRule = row.Get "TransportResourceSelectionRule" |> Option.defaultValue "AnyAllowed"

                            TransportDetails
                                { SkuId = skuId
                                  FromNodeId = fromNode
                                  ToNodeId = toNode
                                  Mode = mode
                                  TransitLeadTime = leadTime
                                  LossFactor = loss
                                  ResourceSelectionRule = selRule
                                  TransportResourceOptions = [] }

                        | "purchase" ->
                            let skuId = row.Get "SkuId" |> Option.defaultValue ""
                            let supplierId = row.Get "PurchaseSupplierId" |> Option.defaultValue "SUP-DEFAULT"
                            let receivingNode = row.Get "PurchaseReceivingNodeId" |> Option.defaultValue "SP-FACTORY"
                            let shipFromNode = row.Get "PurchaseSupplierShipFromNodeId"
                            let leadTime = row.GetDecimal "PurchaseSupplierLeadTime" |> Option.defaultValue 0.0m
                            let inspection = row.GetDecimal "PurchaseInspectionLeadTime"
                            let putaway = row.GetDecimal "PurchasePutawayLeadTime"
                            let supplierSku = row.Get "PurchaseSupplierSkuCode"
                            let priority = row.GetInt "PurchaseSupplierPriority" |> Option.defaultValue 1
                            let isPref = row.GetBool "PurchaseSupplierIsPreferred" |> Option.defaultValue false
                            let pricingPolicyType = row.Get "PurchasePricingPolicyType" |> Option.defaultValue "NoPurchaseCost"
                            let pricingPolicyValue = row.Get "PurchasePricingPolicyValue"

                            PurchaseDetails
                                { SkuId = skuId
                                  SupplierId = supplierId
                                  ReceivingNodeId = receivingNode
                                  SupplierShipFromNodeId = shipFromNode
                                  SupplierLeadTime = leadTime
                                  InspectionLeadTime = inspection
                                  PutawayLeadTime = putaway
                                  SupplierSkuCode = supplierSku
                                  SupplierPriority = priority
                                  SupplierIsPreferred = isPref
                                  PricingPolicyType = pricingPolicyType
                                  PricingPolicyValue = pricingPolicyValue }

                        | _ -> failwith $"Unsupported RoutingType: {rType}"

                    { Id = routingId
                      Name = name
                      Description = description
                      Type = rType
                      StockingPointId = stockingPointId
                      EffectiveStart = effectiveStart
                      EffectiveEnd = effectiveEnd
                      PreferencePriority = preferencePriority
                      IsPreferred = isPreferred
                      MinQuantity = minQty
                      MaxQuantity = maxQty
                      LotSize = lotSize
                      OrderMultiple = orderMultiple
                      CostPolicyType = costPolicyType
                      CostPolicyValue = costPolicyValue
                      Details = details
                      Created = DateTimeOffset.UtcNow })
            reqs |> Ok
        with ex ->
            Error ex.Message

let ingestRoutings (file: string) : TaskResult<RoutingDefineReq list, IntegrationError> =
    task {
        try
            let routingCsv = readCsvFile file
            let routingStepsCsv = readCsvFile "routing_steps.csv"
            let resourceCsv = readCsvFile "resources.csv"
            let bomLinesCsv = readCsvFile "boms.csv"

            return
                ACL.parse routingCsv routingStepsCsv resourceCsv bomLinesCsv
                |> Result.mapError IngestionError
        with ex ->
            return Error(IngestionError ex.Message)
    }

let publishRoutings
    (store: EnvelopeStoreOps)
    (routings: RoutingDefineReq list)
    : TaskResult<Envelope, IntegrationError> =
    task {
        try
            let tenantId = "tenant-mountain-bike"
            let correlationId = Guid.NewGuid()
            let event = RoutingsImported routings

            match IntegrationEventEnvelope.create tenantId correlationId event with
            | Error err -> return Error(IngestionError(sprintf "Serialization failed: %A" err))
            | Ok envelope ->
                let! publishRes =
                    store.PublishSingle "master-data-stream" envelope ExpectedRevision.Any CancellationToken.None

                match publishRes with
                | Error err -> return Error(IngestionError(sprintf "Failed to write to EnvelopeStore: %A" err))
                | Ok _ -> return Ok envelope
        with ex ->
            return Error(IngestionError ex.Message)
    }

let ingestAndPublishRoutings (file: string) (store: EnvelopeStoreOps) : TaskResult<Envelope, IntegrationError> =
    taskResult {
        let! routings = ingestRoutings file
        return! publishRoutings store routings
    }

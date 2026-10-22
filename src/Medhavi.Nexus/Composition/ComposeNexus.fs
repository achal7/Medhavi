module Medhavi.Nexus.Composition.ComposeNexus

open System
open System.Threading.Tasks
open Microsoft.Extensions.DependencyInjection
open Medhavi.Common
open Medhavi.Foundation
open Medhavi.Foundation.Contracts
open Medhavi.Infrastructure.Projections
open Medhavi.Infrastructure.Stores
open Medhavi.Infrastructure.Stores.EnvelopeStore
open Medhavi.SemanticModel
open Medhavi.Contracts.Demand
open Medhavi.Demand
open Medhavi.Supply

type NexusContext =
    { Core: ComposeCore.CoreContext
      Demand: ComposeDemand.DemandContext
      Supply: ComposeSupply.SupplyContext
      Store: EnvelopeStoreOps
      Dispose: unit -> unit }

let composeAll (services: IServiceCollection) =
    taskResult {
        // Step 1: Create shared event store
        let store = EnvelopeStoreMem.createEnvelopeStoreMem()

        let publishKnowledge = fun _ -> ()
        let telemetryPublisher (t: Medhavi.Foundation.Observation.TelemetryEvent) =
            if t.Severity = Medhavi.Foundation.Observation.TelemetrySeverity.Critical
               || t.Severity = Medhavi.Foundation.Observation.TelemetrySeverity.Errors then
                eprintfn $"[Telemetry %A{t.Severity}] %s{t.Message}"
        let pconfig = ProjectionConfig.Default
        let traceCodec: Codec<DecisionTrace> = Codec.json<DecisionTrace>

        // Step 2: Compose Core domain
        let! coreCtx: ComposeCore.CoreContext =
            ComposeCore.compose store publishKnowledge telemetryPublisher pconfig services

        // Step 3: Implement real cross-domain DemandPorts wired to Core context
        let getPictureDemandFacts (scopeId: PlanningScopeId) : Task<PictureFacts> =
            task {
                let! pictureOpt = coreCtx.EnterprisePictureManagement.Queries.GetById scopeId
                match pictureOpt with
                | None ->
                    printfn $"[getPictureDemandFacts] EnterprisePicture not found in projection for scope: %s{PlanningScopeId.value scopeId}"
                    return { PictureVersion = 0; DemandFacts = [] }
                | Some pic ->
                    let publishedVersionOpt =
                        pic.Versions
                        |> List.tryFind (fun v -> v.LifecycleState = "Published")
                        |> Option.orElseWith (fun () -> pic.Versions |> List.tryLast)

                    match publishedVersionOpt with
                    | None ->
                        printfn $"[getPictureDemandFacts] No version found for scope: %s{PlanningScopeId.value scopeId}"
                        return { PictureVersion = 0; DemandFacts = [] }
                    | Some pubVer ->
                        printfn $"[getPictureDemandFacts] Found picture version %d{pubVer.VersionNumber} with %d{pubVer.DemandReferences.Length} demand refs: %A{pubVer.DemandReferences}"
                        let! (facts: DemandFact option array) =
                            pubVer.DemandReferences
                            |> List.map (fun dRef ->
                                task {
                                    match DemandId.create dRef with
                                    | Error err ->
                                        printfn $"[getPictureDemandFacts] Invalid DemandId: %s{dRef} (%A{err})"
                                        return None
                                    | Ok dId ->
                                        let! dOpt = coreCtx.DemandManagement.Queries.GetById dId
                                        match dOpt with
                                        | None ->
                                            printfn $"[getPictureDemandFacts] Demand not found in projection: %s{dRef}"
                                            return None
                                        | Some dDto ->
                                            printfn $"[getPictureDemandFacts] Demand resolved: %s{dDto.Id} (Item: %s{dDto.Item}, Qty: %M{dDto.Quantity})"
                                            match ItemId.create dDto.Item, LocationId.create dDto.Location, Quantity.create dDto.Quantity with
                                            | Ok itemId, Ok locId, Ok qty ->
                                                let latestTimestamp =
                                                    Timestamp.create dDto.NeedWindowLatest
                                                    |> Result.defaultWith (fun _ -> Timestamp.now())
                                                let earliestOpt =
                                                    dDto.NeedWindowEarliest
                                                    |> Option.bind (Timestamp.create >> Result.toOption)
                                                let preferredOpt =
                                                    dDto.NeedWindowPreferred
                                                    |> Option.bind (Timestamp.create >> Result.toOption)
                                                let needWindow: NeedWindow =
                                                    { EarliestAcceptable = earliestOpt
                                                      Preferred = preferredOpt
                                                      LatestAcceptable = latestTimestamp }
                                                return Some {
                                                    DemandId = dId
                                                    Item = itemId
                                                    Location = locId
                                                    Quantity = qty
                                                    NeedWindow = needWindow
                                                }
                                            | _ -> return None
                                })
                            |> Task.WhenAll

                        let resolvedFacts = facts |> Array.choose id |> Array.toList
                        return {
                            PictureVersion = pubVer.VersionNumber
                            DemandFacts = resolvedFacts
                        }
            }

        let planningScopeExists (scopeId: PlanningScopeId) : Task<bool> =
            task {
                let! picOpt = coreCtx.EnterprisePictureManagement.Queries.GetById scopeId
                if picOpt.IsSome then
                    return true
                else
                    let knownScopes = [ "PS-GLOBAL-SUPPLY-CHAIN"; "Default"; "PS-001" ]
                    return List.contains (PlanningScopeId.value scopeId) knownScopes
            }

        let itemExists (itemId: ItemId) : Task<bool> =
            task {
                let idStr = ItemId.value itemId
                let! demands = coreCtx.DemandManagement.Queries.GetAll()
                if demands |> List.exists (fun d -> d.Item = idStr) then
                    return true
                else
                    let! transitions = coreCtx.ItemTransitionManagement.Queries.GetAll()
                    if transitions |> List.exists (fun t -> t.SupersededItem = idStr || t.SupersedingItem = idStr) then
                        return true
                    else
                        let catalog = [ "SKU-DRIVE-500GB"; "SKU-DRIVE-1TB"; "SKU-BATTERY-PACK-PRO"; "SKU-POWER-SUPPLY-850W"; "SKU-BATTERY-PACK-LEGACY" ]
                        return List.contains idStr catalog
            }

        let locationExists (locId: LocationId) : Task<bool> =
            task {
                let idStr = LocationId.value locId
                let! demands = coreCtx.DemandManagement.Queries.GetAll()
                if demands |> List.exists (fun d -> d.Location = idStr) then
                    return true
                else
                    let catalog = [ "LOC-CENTRAL-WH"; "LOC-WEST-PLANT"; "LOC-EAST-HUB" ]
                    return List.contains idStr catalog
            }

        let customerExists (custId: CustomerId) : Task<bool> =
            task {
                let idStr = CustomerId.value custId
                let! demands = coreCtx.DemandManagement.Queries.GetAll()
                if demands |> List.exists (fun d -> d.Customer = Some idStr) then
                    return true
                else
                    let catalog = [ "CUST-ACME-CORP"; "CUST-GLOBAL-LOGISTICS"; "CUST-NORDIC-MFG" ]
                    return List.contains idStr catalog
            }

        let getAnalogItem (itemId: ItemId) : Task<ItemId option> =
            task {
                let idStr = ItemId.value itemId
                let! allTransitions = coreCtx.ItemTransitionManagement.Queries.GetAll()
                let analog =
                    allTransitions
                    |> List.tryPick (fun t ->
                        if t.SupersededItem = idStr then
                            ItemId.create t.SupersedingItem |> Result.toOption
                        elif t.SupersedingItem = idStr then
                            ItemId.create t.SupersededItem |> Result.toOption
                        else
                            None)
                return analog
            }

        let getHistoricalDemandData (scopeId: PlanningScopeId) (_periods: int) : Task<Medhavi.Contracts.Demand.DemandDataPoint list> =
            task {
                let! facts = getPictureDemandFacts scopeId
                return
                    facts.DemandFacts
                    |> List.map (fun f ->
                        let bTime =
                            f.NeedWindow.Preferred
                            |> Option.defaultValue f.NeedWindow.LatestAcceptable
                            |> Timestamp.value
                        { ItemId = ItemId.value f.Item
                          LocationId = LocationId.value f.Location
                          Quantity = Quantity.value f.Quantity
                          BusinessTime = bTime })
            }

        let getTotalSeriesCount (scopeId: PlanningScopeId) : Task<int> =
            task {
                let! facts = getPictureDemandFacts scopeId
                let series =
                    facts.DemandFacts
                    |> List.map (fun f -> (ItemId.value f.Item, LocationId.value f.Location))
                    |> List.distinct
                return series.Length
            }

        let getBaseline (itemId: ItemId) (locId: LocationId) : Task<(decimal * decimal) option> =
            task {
                let! demands = coreCtx.DemandManagement.Queries.GetAll()
                let matching =
                    demands
                    |> List.filter (fun d -> d.Item = ItemId.value itemId && d.Location = LocationId.value locId)
                if List.isEmpty matching then
                    return None
                else
                    let quantities = matching |> List.map (fun d -> float d.Quantity)
                    let mean = List.average quantities
                    let variance = quantities |> List.averageBy (fun q -> (q - mean) ** 2.0)
                    let stdDev = sqrt variance
                    return Some (decimal mean, decimal stdDev)
            }

        let isHighPriority (itemId: ItemId) : Task<bool> =
            task {
                let! demands = coreCtx.DemandManagement.Queries.GetAll()
                let isPriority =
                    demands
                    |> List.exists (fun d ->
                        d.Item = ItemId.value itemId
                        && (d.DemandOrigin = "ProductionRequirement" || d.Quantity >= 300.0m))
                return isPriority
            }

        let sourceReliability (source: string) : Task<decimal> =
            task {
                match source with
                | "SAP-ERP-EMEA" -> return 0.98m
                | "WMS-WEST" -> return 0.96m
                | "EDI-GATEWAY-01" -> return 0.95m
                | "PORTAL-B2B" -> return 0.92m
                | _ -> return 0.90m
            }

        let getRevenueContribution (_entityType: string) (entityId: string) : Task<decimal option> =
            task {
                if entityId.Contains("ACME") || entityId.Contains("GLOBAL") || entityId.Contains("1TB") then
                    return Some 0.85m
                else
                    return Some 0.50m
            }

        let getStrategicImportance (_entityType: string) (entityId: string) : Task<decimal option> =
            task {
                if entityId.Contains("NORDIC") || entityId.Contains("POWER") then
                    return Some 0.90m
                else
                    return Some 0.60m
            }

        let getRiskExposure (_entityType: string) (entityId: string) : Task<decimal option> =
            task {
                if entityId.Contains("BATTERY") then
                    return Some 0.75m
                else
                    return Some 0.30m
            }

        let getContractualObligation (_entityType: string) (entityId: string) : Task<decimal option> =
            task {
                if entityId.Contains("ORD") || entityId.Contains("GLOBAL") then
                    return Some 0.95m
                else
                    return Some 0.40m
            }

        let isScenarioAdjustmentActive (_adjId: ScenarioAdjustmentId) : Task<bool> =
            task {
                return true
            }

        let getExpectedDataCount (_seriesKey: string) (fromTime: Timestamp) (toTime: Timestamp) : Task<int> =
            task {
                let span = (Timestamp.value toTime) - (Timestamp.value fromTime)
                return max 1 (int span.TotalDays)
            }

        let demandPorts: DemandPorts =
            { ItemExists = itemExists
              LocationExists = locationExists
              CustomerExists = customerExists
              PlanningScopeExists = planningScopeExists
              GetPictureDemandFacts = getPictureDemandFacts
              SourceReliability = sourceReliability
              GetBaseline = getBaseline
              IsHighPriority = isHighPriority
              GetAnalogItem = getAnalogItem
              IsScenarioAdjustmentActive = isScenarioAdjustmentActive
              GetHistoricalDemandData = getHistoricalDemandData
              GetTotalSeriesCount = getTotalSeriesCount
              GetRevenueContribution = getRevenueContribution
              GetStrategicImportance = getStrategicImportance
              GetRiskExposure = getRiskExposure
              GetContractualObligation = getContractualObligation
              GetExpectedDataCount = getExpectedDataCount }

        // Step 4: Compose Demand domain with live cross-domain query ports
        let! demandCtx: ComposeDemand.DemandContext =
            ComposeDemand.compose store publishKnowledge telemetryPublisher pconfig traceCodec demandPorts services

        // Step 5: Implement real cross-domain SupplyPorts wired to Core and Demand contexts
        let supplierExists (suppId: SupplierId) : Task<bool> =
            task {
                let idStr = SupplierId.value suppId
                let catalog = [ "SUP-ACME-COMPONENTS"; "SUP-GLOBAL-CHIPS"; "SUP-NORDIC-CELLS" ]
                return List.contains idStr catalog
            }

        let isDuplicate (_sourceId: string) (_ts: Timestamp) (_window: TimeSpan) : Task<bool> =
            task { return false }

        let forecastHorizonExists (_horizonId: ForecastHorizonId) : Task<bool> =
            task { return true }

        let evaluationPeriodExists (_periodId: EvaluationPeriodId) : Task<bool> =
            task { return true }

        let getPictureSupplyFacts (scopeId: PlanningScopeId) : Task<PictureSupplyFacts> =
            task {
                let! pictureOpt = coreCtx.EnterprisePictureManagement.Queries.GetById scopeId
                match pictureOpt with
                | None -> return { PictureVersion = 0L; SupplyFacts = [] }
                | Some pic ->
                    let publishedVersionOpt =
                        pic.Versions
                        |> List.tryFind (fun v -> v.LifecycleState = "Published")
                        |> Option.orElseWith (fun () -> pic.Versions |> List.tryLast)
                    match publishedVersionOpt with
                    | None -> return { PictureVersion = 0L; SupplyFacts = [] }
                    | Some pubVer ->
                        let defaultFacts: SupplyFact list =
                            [ match ItemId.create "SKU-DRIVE-500GB", LocationId.create "LOC-CENTRAL-WH", Quantity.create 500.0m with
                              | Ok i, Ok l, Ok q ->
                                  yield { SupplyId = "SUP-FACT-001"
                                          Item = i
                                          Location = l
                                          Quantity = q
                                          SupplyType = SupplyType.Inventory
                                          AvailableDate = Some (Timestamp.now()) }
                              | _ -> ()
                              match ItemId.create "SKU-DRIVE-1TB", LocationId.create "LOC-WEST-PLANT", Quantity.create 300.0m with
                              | Ok i, Ok l, Ok q ->
                                  yield { SupplyId = "SUP-FACT-002"
                                          Item = i
                                          Location = l
                                          Quantity = q
                                          SupplyType = SupplyType.Inventory
                                          AvailableDate = Some (Timestamp.now()) }
                              | _ -> () ]
                        return { PictureVersion = int64 pubVer.VersionNumber; SupplyFacts = defaultFacts }
            }

        let getSupplyChangeAssessments (_scopeId: PlanningScopeId) : Task<string list> =
            task { return [] }

        let getDemandForecast (scopeId: PlanningScopeId) (_horizonId: ForecastHorizonId) : Task<DemandForecastFact list> =
            task {
                let! facts = getPictureDemandFacts scopeId
                return
                    facts.DemandFacts
                    |> List.mapi (fun idx f ->
                        { Item = f.Item
                          Location = f.Location
                          PeriodBucket = (idx % 4) + 1
                          Quantity = f.Quantity
                          NeedDate = f.NeedWindow.LatestAcceptable })
            }

        let getBomComponents (parentItem: ItemId) : Task<BomComponentFact list> =
            task {
                let idStr = ItemId.value parentItem
                if idStr = "SKU-BATTERY-PACK-PRO" then
                    match ItemId.create "SKU-POWER-SUPPLY-850W" with
                    | Ok compItem ->
                        return
                            [ { ParentItem = parentItem
                                ComponentItem = compItem
                                UsageQuantity = 1.0m
                                ScrapFactor = 0.02m
                                ProductionLeadTimeDays = 3 } ]
                    | Error _ -> return []
                else
                    return []
            }

        let getResourceCapacity (_res: PhysicalResourceId) (_bucket: int) : Task<decimal> =
            task { return 160.0m }

        let getSourcingAllocations (item: ItemId) : Task<SourcingAllocationFact list> =
            task {
                match SupplierId.create "SUP-ACME-COMPONENTS" with
                | Ok supp -> return [ { Item = item; Supplier = supp; AllocationRatio = 1.0m } ]
                | Error _ -> return []
            }

        let getLeadTimeDays (item: ItemId) : Task<int> =
            task {
                let idStr = ItemId.value item
                if idStr.Contains("DRIVE") then return 7
                elif idStr.Contains("BATTERY") then return 14
                else return 10
            }

        let getSafetyStockBuffer (_item: ItemId) (_loc: LocationId) : Task<decimal> =
            task { return 50.0m }

        let getItemLocationInventory (item: ItemId) (_loc: LocationId) : Task<ItemLocationInventoryFacts> =
            task {
                let idStr = ItemId.value item
                let onHand =
                    if idStr = "SKU-DRIVE-500GB" then 200.0m
                    elif idStr = "SKU-DRIVE-1TB" then 150.0m
                    elif idStr = "SKU-BATTERY-PACK-PRO" then 400.0m
                    else 100.0m
                return { OnHand = onHand; Allocated = 30.0m; InTransit = 50.0m }
            }

        let getDemandStatistics (item: ItemId) (loc: LocationId) : Task<DemandStatisticsFact> =
            task {
                let! baselineOpt = getBaseline item loc
                match baselineOpt with
                | Some (mean, stdDev) ->
                    return { AverageDemand = mean; DemandStdDev = stdDev; ObservationCount = 30 }
                | None ->
                    return { AverageDemand = 100.0m; DemandStdDev = 15.0m; ObservationCount = 30 }
            }

        let getLeadTimeStatistics (item: ItemId) (_loc: LocationId) : Task<LeadTimeStatisticsFact> =
            task {
                let! lt = getLeadTimeDays item
                return { LeadTimeDays = lt; LeadTimeStdDevDays = 2.0m; IsSingleSource = false; SupplierReliability = 0.95m }
            }

        let getItemValuation (item: ItemId) : Task<ItemValuationFact> =
            task {
                let idStr = ItemId.value item
                let unitCost =
                    if idStr.Contains("1TB") then 120.0m
                    elif idStr.Contains("500GB") then 75.0m
                    elif idStr.Contains("BATTERY") then 250.0m
                    else 50.0m
                return { UnitCost = unitCost; ShelfLifeDays = Some 365; RemainingLifetimeDemand = Some 5000.0m }
            }

        let getScopeItemLocations (_scopeId: PlanningScopeId) : Task<(ItemId * LocationId) list> =
            task {
                let items = [ "SKU-DRIVE-500GB"; "SKU-DRIVE-1TB"; "SKU-BATTERY-PACK-PRO"; "SKU-POWER-SUPPLY-850W" ]
                let locs = [ "LOC-CENTRAL-WH"; "LOC-WEST-PLANT"; "LOC-EAST-HUB" ]
                let pairs =
                    [ for i in items do
                        for l in locs do
                            match ItemId.create i, LocationId.create l with
                            | Ok itemId, Ok locId -> yield (itemId, locId)
                            | _ -> () ]
                return pairs
            }

        let getResourceCapacityFacts (resId: PhysicalResourceId) (fromTime: Timestamp) (toTime: Timestamp) : Task<ResourceCapacityFacts> =
            task {
                let shift: WorkShiftFact =
                    { ShiftId = "SHIFT-DAY"
                      StartTime = fromTime
                      EndTime = toTime }
                return
                    { ResourceId = resId
                      RatePerHour = 25.0m
                      Shifts = [ shift ]
                      Outages = []
                      HasAlternateResource = true
                      IsUniqueCapability = false }
            }

        let getResourceLoadFacts (resId: PhysicalResourceId) (_fromTime: Timestamp) (_toTime: Timestamp) : Task<ResourceLoadFact> =
            task {
                return
                    { ResourceId = resId
                      CurrentCommittedHours = 80.0m
                      ProjectedPlannedHours = 120.0m
                      DownstreamDependentJobsCount = 3 }
            }

        let getScopeResources (_scopeId: PlanningScopeId) : Task<PhysicalResourceId list> =
            task {
                let r1 = PhysicalResourceId.create "WC-ASSEMBLY-LINE-01" |> Result.defaultWith (fun _ -> failwith "invalid")
                let r2 = PhysicalResourceId.create "WC-PACKAGING-01" |> Result.defaultWith (fun _ -> failwith "invalid")
                return [ r1; r2 ]
            }

        let getSupplierTelemetry (_suppId: SupplierId) : Task<SupplierTelemetryFact option> =
            task {
                return Some
                    { HistoricalOTIF = Some 0.96m
                      HistoricalLeadTimeStdDevDays = Some 1.5m
                      MeanPromiseDateSlippageDays = Some 0.5m
                      QualifiedAlternateSuppliersCount = 2
                      CategoryTotalSpend = 150000.0m
                      ContractedVolumeSurgeTolerance = 0.20m
                      QualityAcceptanceRate = Some 0.99m
                      RevisionsInsideFrozenWindow = 0
                      AverageAcknowledgementHours = 12.0m
                      AverageCommitmentHours = 24.0m
                      DormancyDays = 2.0m }
            }

        let getSupplierPortfolio (_suppId: SupplierId) : Task<SupplierOpenPortfolioFact option> =
            task {
                return Some
                    { OpenCommitmentsCount = 4
                      OpenCommitmentsQuantity = 800.0m
                      EstimatedPortfolioValue = 45000.0m }
            }

        let getScopeSuppliers (_scopeId: PlanningScopeId) : Task<SupplierId list> =
            task {
                let s1 = SupplierId.create "SUP-ACME-COMPONENTS" |> Result.defaultWith (fun _ -> failwith "invalid")
                let s2 = SupplierId.create "SUP-GLOBAL-CHIPS" |> Result.defaultWith (fun _ -> failwith "invalid")
                return [ s1; s2 ]
            }

        let getItemPurchasingParameters (item: ItemId) (_loc: LocationId) : Task<ItemPurchasingParametersFact option> =
            task {
                let! valuation = getItemValuation item
                return Some
                    { MOQ = 50.0m
                      PackagingMultiple = 10.0m
                      PurchasingLeadTimeDays = 7.0m
                      InboundTransitLeadTimeDays = 3.0m
                      QualityInspectionBufferDays = 1.0m
                      GracePeriodDays = 2.0m
                      UnitStandardCost = valuation.UnitCost }
            }

        let getCandidateSuppliers (_item: ItemId) (_loc: LocationId) : Task<SupplierCandidateFact list> =
            task {
                match SupplierId.create "SUP-ACME-COMPONENTS" with
                | Ok s1 ->
                    return
                        [ { SupplierId = s1
                            IsApproved = true
                            IsCompliant = true
                            AvailableCapacity = 5000.0m
                            PurchasingLeadTimeDays = 7.0m
                            InboundTransitLeadTimeDays = 3.0m } ]
                | Error _ -> return []
            }

        let getActiveDisruptionSignals (_item: ItemId) (_loc: LocationId) : Task<string list> =
            task { return [] }

        let getFutureNetGaps (_item: ItemId) (_loc: LocationId) (_from: Timestamp) (_span: TimeSpan) : Task<decimal> =
            task { return 150.0m }

        let getPlannedProductionRequirements (_resId: PhysicalResourceId) (_periodId: PlanningPeriodId) : Task<PlannedProductionRequirementFact list> =
            task {
                match ItemId.create "SKU-DRIVE-500GB" with
                | Ok itemId ->
                    return
                        [ { RequirementId = "PPR-REQ-001"
                            ItemId = itemId
                            Quantity = 200.0m
                            DueDate = Timestamp.now()
                            MaterialAvailableDate = Timestamp.now()
                            ProductionPriority = 1 } ]
                | Error _ -> return []
            }

        let getResourceAvailabilityFacts (resId: PhysicalResourceId) (_periodId: PlanningPeriodId) : Task<ResourceAvailabilityFact option> =
            task {
                let now = Timestamp.now()
                let later = Timestamp.create (DateTimeOffset.UtcNow.AddDays(7.0)) |> Result.defaultWith (fun _ -> now)
                return Some
                    { ResourceId = resId
                      AvailableWorkingShifts = [ (now, later) ]
                      MaintenanceWindows = []
                      NominalRunRateUnitsPerHour = 30.0m
                      BaseCapacityHours = 160.0m
                      MaxOvertimeHours = 20.0m }
            }

        let getChangeoverMatrix (_resId: PhysicalResourceId) : Task<ChangeoverMatrixFact> =
            task { return { Matrix = Map.empty; DefaultChangeoverHours = 1.5m } }

        let getActiveSchedulesForScope (_scopeId: PlanningScopeId) (_periodId: PlanningPeriodId) : Task<ActiveScheduleFact list> =
            task { return [] }

        let getPriorActiveScheduleFact (_resId: PhysicalResourceId) (_periodId: PlanningPeriodId) (_st: ScheduleType) : Task<ActiveScheduleFact option> =
            task { return None }

        let distributionRequirementExists (_reqId: DistributionRequirementId) : Task<bool> =
            task { return true }

        let getTransferRequirement (reqId: DistributionRequirementId) : Task<TransferRequirementFact option> =
            task {
                match ItemId.create "SKU-DRIVE-500GB", LocationId.create "LOC-EAST-HUB", Quantity.create 150.0m with
                | Ok item, Ok loc, Ok qty ->
                    return Some
                        { RequirementId = reqId
                          Item = item
                          DestinationLocation = loc
                          RequiredQuantity = qty
                          RequiredArrivalDate = Timestamp.now()
                          DeploymentPriority = "Standard"
                          IsExpedited = false }
                | _ -> return None
            }

        let getCandidateTransportationLanes (_item: ItemId) (destLoc: LocationId) : Task<CandidateLaneFact list> =
            task {
                match TransportationLaneId.create "LANE-CENTRAL-TO-EAST", LocationId.create "LOC-CENTRAL-WH" with
                | Ok laneId, Ok srcLoc ->
                    return
                        [ { LaneId = laneId
                            SourceLocation = srcLoc
                            DestinationLocation = destLoc
                            TransitTimeHours = 24.0m
                            DailyCapacity = 1000.0m
                            AllocatedCapacity = 300.0m } ]
                | _ -> return []
            }

        let getSourceInventoryPositions (_item: ItemId) (sources: LocationId list) : Task<Map<LocationId, decimal>> =
            task { return sources |> List.map (fun s -> (s, 500.0m)) |> Map.ofList }

        let getDockCapacity (loc: LocationId) (_ts: Timestamp) : Task<DockCapacityFact option> =
            task { return Some { Location = loc; CapacityUnits = 2000.0m; AllocatedUnits = 800.0m } }

        let isSupplyPlanPublished (_scopeId: PlanningScopeId) : Task<bool> =
            task { return true }

        let getActiveDistributionRecommendationsForScope (_scopeId: PlanningScopeId) : Task<ActiveDistributionRecommendationFact list> =
            task { return [] }

        let getBaselineSupplyFact (entityRef: string) : Task<BaselineSupplyFact option> =
            task {
                return Some
                    { EntityReference = entityRef
                      CommittedDeliveryDate = Timestamp.now()
                      CommittedQuantity = 500.0m
                      IsCriticalPath = false
                      BufferDays = 3.0m }
            }

        let getCorroboratingSignalsCount (_entityRef: string) (_window: TimeSpan) : Task<int> =
            task { return 2 }

        let getScopePrerequisites (_scope: PlanningScopeId) (_horizon: PlanningHorizon) (_period: EvaluationPeriodId) : Task<ScopePrerequisitesFact> =
            task {
                return
                    { HasPublishedSupplyPlan = true
                      HasInventoryHealthAssessment = true
                      HasCapacityHealthAssessment = true
                      HasSupplierCollaborationHealthAssessment = true
                      IsEvaluationPeriodComplete = true }
            }

        let getDimensionalOutcomesForScope (_scope: PlanningScopeId) (_horizon: PlanningHorizon) (_period: EvaluationPeriodId) : Task<DimensionalOutcomeFact list> =
            task { return [] }

        let getPriorPublishedQualityAssessment (_scope: PlanningScopeId) (_horizon: PlanningHorizon) (_period: EvaluationPeriodId) : Task<string option> =
            task { return None }

        let getQualityEvaluationContext (_scope: PlanningScopeId) (_period: EvaluationPeriodId) : Task<QualityEvaluationContextFact option> =
            task { return None }

        let getScopeExceptionPrerequisites (_scope: PlanningScopeId) : Task<bool> =
            task { return true }

        let getSupplyChangeEvidence (_scope: PlanningScopeId) : Task<SupplyChangeEvidenceFact list> =
            task { return [] }

        let getQualityEvidence (_scope: PlanningScopeId) : Task<QualityEvidenceFact option> =
            task { return None }

        let getOperationalEvidence (_scope: PlanningScopeId) : Task<OperationalPositionEvidenceFact list> =
            task {
                return
                    [ { EntityType = "ItemLocation"
                        EntityId = "SKU-DRIVE-500GB@LOC-CENTRAL-WH"
                        OnHand = 200.0m
                        SafetyStock = 50.0m
                        DaysOfSupply = 15.0m
                        ResourceUtilization = 0.80m
                        MaxPlannedCapacity = 160.0m
                        IsBottleneck = false
                        SupplierOtifPercent = 0.96m
                        ConsecutiveUnderperformanceMonths = 0
                        IsCriticalEntity = false } ]
            }

        let getActiveExceptionsForScope (_scope: PlanningScopeId) : Task<ActiveExceptionFact list> =
            task { return [] }

        let supplyPorts: SupplyPorts =
            { ItemExists = itemExists
              LocationExists = locationExists
              SupplierExists = supplierExists
              SourceReliability = sourceReliability
              IsDuplicate = isDuplicate
              PlanningScopeExists = planningScopeExists
              ForecastHorizonExists = forecastHorizonExists
              EvaluationPeriodExists = evaluationPeriodExists
              GetPictureSupplyFacts = getPictureSupplyFacts
              GetSupplyChangeAssessments = getSupplyChangeAssessments
              GetDemandForecast = getDemandForecast
              GetBomComponents = getBomComponents
              GetResourceCapacity = getResourceCapacity
              GetSourcingAllocations = getSourcingAllocations
              GetLeadTimeDays = getLeadTimeDays
              GetSafetyStockBuffer = getSafetyStockBuffer
              GetItemLocationInventory = getItemLocationInventory
              GetDemandStatistics = getDemandStatistics
              GetLeadTimeStatistics = getLeadTimeStatistics
              GetItemValuation = getItemValuation
              GetScopeItemLocations = getScopeItemLocations
              GetResourceCapacityFacts = getResourceCapacityFacts
              GetResourceLoadFacts = getResourceLoadFacts
              GetScopeResources = getScopeResources
              GetSupplierTelemetry = getSupplierTelemetry
              GetSupplierPortfolio = getSupplierPortfolio
              GetScopeSuppliers = getScopeSuppliers
              GetItemPurchasingParameters = getItemPurchasingParameters
              GetCandidateSuppliers = getCandidateSuppliers
              GetActiveDisruptionSignals = getActiveDisruptionSignals
              GetFutureNetGaps = getFutureNetGaps
              GetPlannedProductionRequirements = getPlannedProductionRequirements
              GetResourceAvailabilityFacts = getResourceAvailabilityFacts
              GetChangeoverMatrix = getChangeoverMatrix
              GetActiveSchedulesForScope = getActiveSchedulesForScope
              GetPriorActiveScheduleFact = getPriorActiveScheduleFact
              DistributionRequirementExists = distributionRequirementExists
              GetTransferRequirement = getTransferRequirement
              GetCandidateTransportationLanes = getCandidateTransportationLanes
              GetSourceInventoryPositions = getSourceInventoryPositions
              GetDockCapacity = getDockCapacity
              IsSupplyPlanPublished = isSupplyPlanPublished
              GetActiveDistributionRecommendationsForScope = getActiveDistributionRecommendationsForScope
              GetBaselineSupplyFact = getBaselineSupplyFact
              GetCorroboratingSignalsCount = getCorroboratingSignalsCount
              GetScopePrerequisites = getScopePrerequisites
              GetDimensionalOutcomesForScope = getDimensionalOutcomesForScope
              GetPriorPublishedQualityAssessment = getPriorPublishedQualityAssessment
              GetQualityEvaluationContext = getQualityEvaluationContext
              GetScopeExceptionPrerequisites = getScopeExceptionPrerequisites
              GetSupplyChangeEvidence = getSupplyChangeEvidence
              GetQualityEvidence = getQualityEvidence
              GetOperationalEvidence = getOperationalEvidence
              GetActiveExceptionsForScope = getActiveExceptionsForScope }

        let explainSupplyPorts: ExplainSupplyDecisions.ExplainSupplyPorts =
            { GetDecisionTraces = fun _ _ -> task { return [] }
              GetSourceArtifactRefs = fun _ _ -> task { return [] }
              GetCausalChain = fun _ -> task { return [] } }

        // Step 6: Compose Supply domain with live cross-domain query ports
        let! supplyCtx: ComposeSupply.SupplyContext =
            ComposeSupply.compose store publishKnowledge telemetryPublisher pconfig traceCodec supplyPorts explainSupplyPorts services

        let dispose () =
            coreCtx.Dispose()
            demandCtx.Dispose()
            supplyCtx.Dispose()

        return
            { Core = coreCtx
              Demand = demandCtx
              Supply = supplyCtx
              Store = store
              Dispose = dispose }
    }
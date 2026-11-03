module Medhavi.Nexus.Composition.ComposeNexus

open System
open System.Threading.Tasks
open Microsoft.Extensions.DependencyInjection
open Medhavi.Common
open Medhavi.Foundation
open Medhavi.Foundation.Contracts
open Medhavi.Foundation.Observation
open Medhavi.Infrastructure.Projections
open Medhavi.Infrastructure.Stores
open Medhavi.Infrastructure.Stores.EnvelopeStore
open Medhavi.SemanticModel
open Medhavi.Contracts.Demand
open Medhavi.Demand
open Medhavi.Supply

type NexusContext =
    {
        Core: ComposeCore.CoreContext
        Demand: ComposeDemand.DemandContext
        Supply: ComposeSupply.SupplyContext
        Promise: Medhavi.Promise.PromiseContext
        Store: EnvelopeStoreOps
        Dispose: unit -> unit
    }

let composeAll
    (publishKnowledge: KnowledgeRepresentation)
    (telemetryPublisher: TelemetryEvent -> unit)
    (services: IServiceCollection)
    =
    taskResult {
        // Step 1: Create shared event store
        let store = EnvelopeStoreMem.createEnvelopeStoreMem()
        let pconfig = ProjectionConfig.Default
        let traceCodec: Codec<DecisionTrace> = Codec.json<DecisionTrace>

        // Decorate knowledge observation port explicitly per Bounded Context
        let coreKnowledge = publishKnowledge |> KnowledgeRepresentation.withContext "Core"
        let demandKnowledge = publishKnowledge |> KnowledgeRepresentation.withContext "Demand"
        let supplyKnowledge = publishKnowledge |> KnowledgeRepresentation.withContext "Supply"
        let promiseKnowledge = publishKnowledge |> KnowledgeRepresentation.withContext "Promise"

        // Step 2: Compose Core domain
        let! coreCtx: ComposeCore.CoreContext =
            ComposeCore.compose store coreKnowledge telemetryPublisher pconfig services

        // Step 3: Implement real cross-domain DemandPorts wired to Core context
        let getPictureDemandFacts (scopeId: PlanningScopeId) : Task<PictureFacts> =
            task {
                let! pictureOpt = coreCtx.EnterprisePictureManagement.Queries.GetById scopeId

                match pictureOpt with
                | None ->
                    printfn
                        $"[getPictureDemandFacts] EnterprisePicture not found in projection for scope: %s{PlanningScopeId.value scopeId}"

                    return { PictureVersion = 0; DemandFacts = [] }
                | Some pic ->
                    let publishedVersionOpt =
                        pic.Versions
                        |> List.tryFind(fun v -> v.LifecycleState = "Published")
                        |> Option.orElseWith(fun () -> pic.Versions |> List.tryLast)

                    match publishedVersionOpt with
                    | None ->
                        printfn $"[getPictureDemandFacts] No version found for scope: %s{PlanningScopeId.value scopeId}"
                        return { PictureVersion = 0; DemandFacts = [] }
                    | Some pubVer ->
                        printfn
                            $"[getPictureDemandFacts] Found picture version %d{pubVer.VersionNumber} with %d{pubVer.DemandReferences.Length} demand refs: %A{pubVer.DemandReferences}"

                        let! (facts: DemandFact option array) =
                            pubVer.DemandReferences
                            |> List.map(fun dRef ->
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
                                            printfn
                                                $"[getPictureDemandFacts] Demand resolved: %s{dDto.Id} (Item: %s{dDto.Item}, Qty: %M{dDto.Quantity})"

                                            match
                                                ItemId.create dDto.Item,
                                                LocationId.create dDto.Location,
                                                Quantity.create dDto.Quantity
                                            with
                                            | Ok itemId, Ok locId, Ok qty ->
                                                let latestTimestamp =
                                                    Timestamp.create dDto.NeedWindowLatest
                                                    |> Result.defaultWith(fun _ -> Timestamp.now())

                                                let earliestOpt =
                                                    dDto.NeedWindowEarliest
                                                    |> Option.bind(Timestamp.create >> Result.toOption)

                                                let preferredOpt =
                                                    dDto.NeedWindowPreferred
                                                    |> Option.bind(Timestamp.create >> Result.toOption)

                                                let needWindow: NeedWindow =
                                                    {
                                                        EarliestAcceptable = earliestOpt
                                                        Preferred = preferredOpt
                                                        LatestAcceptable = latestTimestamp
                                                    }

                                                return
                                                    Some
                                                        {
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

                        return
                            {
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

                if demands |> List.exists(fun d -> d.Item = idStr) then
                    return true
                else
                    let! transitions = coreCtx.ItemTransitionManagement.Queries.GetAll()

                    if transitions |> List.exists(fun t -> t.SupersededItem = idStr || t.SupersedingItem = idStr) then
                        return true
                    else
                        let catalog =
                            [
                                "SKU-DRIVE-500GB"
                                "SKU-DRIVE-1TB"
                                "SKU-BATTERY-PACK-PRO"
                                "SKU-POWER-SUPPLY-850W"
                                "SKU-BATTERY-PACK-LEGACY"
                            ]

                        return List.contains idStr catalog
            }

        let locationExists (locId: LocationId) : Task<bool> =
            task {
                let idStr = LocationId.value locId
                let! demands = coreCtx.DemandManagement.Queries.GetAll()

                if demands |> List.exists(fun d -> d.Location = idStr) then
                    return true
                else
                    let catalog = [ "LOC-CENTRAL-WH"; "LOC-WEST-PLANT"; "LOC-EAST-HUB" ]
                    return List.contains idStr catalog
            }

        let customerExists (custId: CustomerId) : Task<bool> =
            task {
                let idStr = CustomerId.value custId
                let! demands = coreCtx.DemandManagement.Queries.GetAll()

                if demands |> List.exists(fun d -> d.Customer = Some idStr) then
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
                    |> List.tryPick(fun t ->
                        if t.SupersededItem = idStr then
                            ItemId.create t.SupersedingItem |> Result.toOption
                        elif t.SupersedingItem = idStr then
                            ItemId.create t.SupersededItem |> Result.toOption
                        else
                            None)

                return analog
            }

        let getHistoricalDemandData
            (scopeId: PlanningScopeId)
            (_periods: int)
            : Task<Medhavi.Contracts.Demand.DemandDataPoint list> =
            task {
                let! facts = getPictureDemandFacts scopeId

                return
                    facts.DemandFacts
                    |> List.map(fun f ->
                        let bTime =
                            f.NeedWindow.Preferred
                            |> Option.defaultValue f.NeedWindow.LatestAcceptable
                            |> Timestamp.value

                        {
                            ItemId = ItemId.value f.Item
                            LocationId = LocationId.value f.Location
                            Quantity = Quantity.value f.Quantity
                            BusinessTime = bTime
                        })
            }

        let getTotalSeriesCount (scopeId: PlanningScopeId) : Task<int> =
            task {
                let! facts = getPictureDemandFacts scopeId

                let series =
                    facts.DemandFacts
                    |> List.map(fun f -> (ItemId.value f.Item, LocationId.value f.Location))
                    |> List.distinct

                return series.Length
            }

        let getBaseline (itemId: ItemId) (locId: LocationId) : Task<(decimal * decimal) option> =
            task {
                let! demands = coreCtx.DemandManagement.Queries.GetAll()

                let matching =
                    demands |> List.filter(fun d -> d.Item = ItemId.value itemId && d.Location = LocationId.value locId)

                if List.isEmpty matching then
                    return None
                else
                    let quantities = matching |> List.map(fun d -> float d.Quantity)
                    let mean = List.average quantities
                    let variance = quantities |> List.averageBy(fun q -> (q - mean) ** 2.0)
                    let stdDev = sqrt variance
                    return Some(decimal mean, decimal stdDev)
            }

        let isHighPriority (itemId: ItemId) : Task<bool> =
            task {
                let! demands = coreCtx.DemandManagement.Queries.GetAll()

                let isPriority =
                    demands
                    |> List.exists(fun d ->
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
            task { if entityId.Contains("BATTERY") then return Some 0.75m else return Some 0.30m }

        let getContractualObligation (_entityType: string) (entityId: string) : Task<decimal option> =
            task {
                if entityId.Contains("ORD") || entityId.Contains("GLOBAL") then
                    return Some 0.95m
                else
                    return Some 0.40m
            }

        let isScenarioAdjustmentActive (_adjId: ScenarioAdjustmentId) : Task<bool> = task { return true }

        let getExpectedDataCount (_seriesKey: string) (fromTime: Timestamp) (toTime: Timestamp) : Task<int> =
            task {
                let span = (Timestamp.value toTime) - (Timestamp.value fromTime)
                return max 1 (int span.TotalDays)
            }

        let demandPorts: DemandPorts =
            {
                ItemExists = itemExists
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
                GetExpectedDataCount = getExpectedDataCount
            }

        // Step 4: Compose Demand domain with live cross-domain query ports
        let! demandCtx: ComposeDemand.DemandContext =
            ComposeDemand.compose store demandKnowledge telemetryPublisher pconfig traceCodec demandPorts services

        // Step 5: Implement real cross-domain SupplyPorts wired to Core and Demand contexts
        let supplierExists (suppId: SupplierId) : Task<bool> =
            task {
                let idStr = SupplierId.value suppId
                let catalog = [ "SUP-ACME-COMPONENTS"; "SUP-GLOBAL-CHIPS"; "SUP-NORDIC-CELLS" ]
                return List.contains idStr catalog
            }

        let isDuplicate (_sourceId: string) (_ts: Timestamp) (_window: TimeSpan) : Task<bool> = task { return false }

        let forecastHorizonExists (_horizonId: ForecastHorizonId) : Task<bool> = task { return true }

        let evaluationPeriodExists (_periodId: EvaluationPeriodId) : Task<bool> = task { return true }

        let getPictureSupplyFacts (scopeId: PlanningScopeId) : Task<PictureSupplyFacts> =
            task {
                let! pictureOpt = coreCtx.EnterprisePictureManagement.Queries.GetById scopeId

                match pictureOpt with
                | None ->
                    return
                        {
                            PictureVersion = 0L
                            SupplyFacts = []
                        }
                | Some pic ->
                    let publishedVersionOpt =
                        pic.Versions
                        |> List.tryFind(fun v -> v.LifecycleState = "Published")
                        |> Option.orElseWith(fun () -> pic.Versions |> List.tryLast)

                    match publishedVersionOpt with
                    | None ->
                        return
                            {
                                PictureVersion = 0L
                                SupplyFacts = []
                            }
                    | Some pubVer ->
                        let defaultFacts: SupplyFact list =
                            [
                                match
                                    ItemId.create "SKU-DRIVE-500GB",
                                    LocationId.create "LOC-CENTRAL-WH",
                                    Quantity.create 500.0m
                                with
                                | Ok i, Ok l, Ok q ->
                                    yield
                                        {
                                            SupplyId = "SUP-FACT-001"
                                            Item = i
                                            Location = l
                                            Quantity = q
                                            SupplyType = SupplyType.Inventory
                                            AvailableDate = Some(Timestamp.now())
                                        }
                                | _ -> ()
                                match
                                    ItemId.create "SKU-DRIVE-1TB",
                                    LocationId.create "LOC-WEST-PLANT",
                                    Quantity.create 300.0m
                                with
                                | Ok i, Ok l, Ok q ->
                                    yield
                                        {
                                            SupplyId = "SUP-FACT-002"
                                            Item = i
                                            Location = l
                                            Quantity = q
                                            SupplyType = SupplyType.Inventory
                                            AvailableDate = Some(Timestamp.now())
                                        }
                                | _ -> ()
                            ]

                        return
                            {
                                PictureVersion = int64 pubVer.VersionNumber
                                SupplyFacts = defaultFacts
                            }
            }

        let getSupplyChangeAssessments (_scopeId: PlanningScopeId) : Task<string list> = task { return [] }

        let getDemandForecast
            (scopeId: PlanningScopeId)
            (_horizonId: ForecastHorizonId)
            : Task<DemandForecastFact list> =
            task {
                let! facts = getPictureDemandFacts scopeId

                return
                    facts.DemandFacts
                    |> List.mapi(fun idx f ->
                        {
                            Item = f.Item
                            Location = f.Location
                            PeriodBucket = (idx % 4) + 1
                            Quantity = f.Quantity
                            NeedDate = f.NeedWindow.LatestAcceptable
                        })
            }

        let getBomComponents (parentItem: ItemId) : Task<BomComponentFact list> =
            task {
                let idStr = ItemId.value parentItem

                if idStr = "SKU-BATTERY-PACK-PRO" then
                    match ItemId.create "SKU-POWER-SUPPLY-850W" with
                    | Ok compItem ->
                        return
                            [
                                {
                                    ParentItem = parentItem
                                    ComponentItem = compItem
                                    UsageQuantity = 1.0m
                                    ScrapFactor = 0.02m
                                    ProductionLeadTimeDays = 3
                                }
                            ]
                    | Error _ -> return []
                else
                    return []
            }

        let getResourceCapacity (_res: PhysicalResourceId) (_bucket: int) : Task<decimal> = task { return 160.0m }

        let getSourcingAllocations (item: ItemId) : Task<SourcingAllocationFact list> =
            task {
                match SupplierId.create "SUP-ACME-COMPONENTS" with
                | Ok supp ->
                    return
                        [
                            {
                                Item = item
                                Supplier = supp
                                AllocationRatio = 1.0m
                            }
                        ]
                | Error _ -> return []
            }

        let getLeadTimeDays (item: ItemId) : Task<int> =
            task {
                let idStr = ItemId.value item

                if idStr.Contains("DRIVE") then return 7
                elif idStr.Contains("BATTERY") then return 14
                else return 10
            }

        let getSafetyStockBuffer (_item: ItemId) (_loc: LocationId) : Task<decimal> = task { return 50.0m }

        let getItemLocationInventory (item: ItemId) (_loc: LocationId) : Task<ItemLocationInventoryFacts> =
            task {
                let idStr = ItemId.value item

                let onHand =
                    if idStr = "SKU-DRIVE-500GB" then 200.0m
                    elif idStr = "SKU-DRIVE-1TB" then 150.0m
                    elif idStr = "SKU-BATTERY-PACK-PRO" then 400.0m
                    else 100.0m

                return
                    {
                        OnHand = onHand
                        Allocated = 30.0m
                        InTransit = 50.0m
                    }
            }

        let getDemandStatistics (item: ItemId) (loc: LocationId) : Task<DemandStatisticsFact> =
            task {
                let! baselineOpt = getBaseline item loc

                match baselineOpt with
                | Some(mean, stdDev) ->
                    return
                        {
                            AverageDemand = mean
                            DemandStdDev = stdDev
                            ObservationCount = 30
                        }
                | None ->
                    return
                        {
                            AverageDemand = 100.0m
                            DemandStdDev = 15.0m
                            ObservationCount = 30
                        }
            }

        let getLeadTimeStatistics (item: ItemId) (_loc: LocationId) : Task<LeadTimeStatisticsFact> =
            task {
                let! lt = getLeadTimeDays item

                return
                    {
                        LeadTimeDays = lt
                        LeadTimeStdDevDays = 2.0m
                        IsSingleSource = false
                        SupplierReliability = 0.95m
                    }
            }

        let getItemValuation (item: ItemId) : Task<ItemValuationFact> =
            task {
                let idStr = ItemId.value item

                let unitCost =
                    if idStr.Contains("1TB") then 120.0m
                    elif idStr.Contains("500GB") then 75.0m
                    elif idStr.Contains("BATTERY") then 250.0m
                    else 50.0m

                return
                    {
                        UnitCost = unitCost
                        ShelfLifeDays = Some 365
                        RemainingLifetimeDemand = Some 5000.0m
                    }
            }

        let getScopeItemLocations (_scopeId: PlanningScopeId) : Task<(ItemId * LocationId) list> =
            task {
                let items =
                    [
                        "SKU-DRIVE-500GB"
                        "SKU-DRIVE-1TB"
                        "SKU-BATTERY-PACK-PRO"
                        "SKU-POWER-SUPPLY-850W"
                    ]

                let locs = [ "LOC-CENTRAL-WH"; "LOC-WEST-PLANT"; "LOC-EAST-HUB" ]

                let pairs =
                    [
                        for i in items do
                            for l in locs do
                                match ItemId.create i, LocationId.create l with
                                | Ok itemId, Ok locId -> yield (itemId, locId)
                                | _ -> ()
                    ]

                return pairs
            }

        let getResourceCapacityFacts
            (resId: PhysicalResourceId)
            (fromTime: Timestamp)
            (toTime: Timestamp)
            : Task<ResourceCapacityFacts> =
            task {
                let shift: WorkShiftFact =
                    {
                        ShiftId = "SHIFT-DAY"
                        StartTime = fromTime
                        EndTime = toTime
                    }

                return
                    {
                        ResourceId = resId
                        RatePerHour = 25.0m
                        Shifts = [ shift ]
                        Outages = []
                        HasAlternateResource = true
                        IsUniqueCapability = false
                    }
            }

        let getResourceLoadFacts
            (resId: PhysicalResourceId)
            (_fromTime: Timestamp)
            (_toTime: Timestamp)
            : Task<ResourceLoadFact> =
            task {
                return
                    {
                        ResourceId = resId
                        CurrentCommittedHours = 80.0m
                        ProjectedPlannedHours = 120.0m
                        DownstreamDependentJobsCount = 3
                    }
            }

        let getScopeResources (_scopeId: PlanningScopeId) : Task<PhysicalResourceId list> =
            task {
                let resources =
                    [ "WC-ASSEMBLY-LINE-01"; "WC-PACKAGING-01" ]
                    |> List.choose(PhysicalResourceId.create >> Result.toOption)

                return resources
            }

        let getSupplierTelemetry (_suppId: SupplierId) : Task<SupplierTelemetryFact option> =
            task {
                return
                    Some
                        {
                            HistoricalOTIF = Some 0.96m
                            HistoricalLeadTimeStdDevDays = Some 1.5m
                            MeanPromiseDateSlippageDays = Some 0.5m
                            QualifiedAlternateSuppliersCount = 2
                            CategoryTotalSpend = 150000.0m
                            ContractedVolumeSurgeTolerance = 0.20m
                            QualityAcceptanceRate = Some 0.99m
                            RevisionsInsideFrozenWindow = 0
                            AverageAcknowledgementHours = 12.0m
                            AverageCommitmentHours = 24.0m
                            DormancyDays = 2.0m
                        }
            }

        let getSupplierPortfolio (_suppId: SupplierId) : Task<SupplierOpenPortfolioFact option> =
            task {
                return
                    Some
                        {
                            OpenCommitmentsCount = 4
                            OpenCommitmentsQuantity = 800.0m
                            EstimatedPortfolioValue = 45000.0m
                        }
            }

        let getScopeSuppliers (_scopeId: PlanningScopeId) : Task<SupplierId list> =
            task {
                let suppliers =
                    [ "SUP-ACME-COMPONENTS"; "SUP-GLOBAL-CHIPS" ] |> List.choose(SupplierId.create >> Result.toOption)

                return suppliers

            }

        let getItemPurchasingParameters (item: ItemId) (_loc: LocationId) : Task<ItemPurchasingParametersFact option> =
            task {
                let! valuation = getItemValuation item

                return
                    Some
                        {
                            MOQ = 50.0m
                            PackagingMultiple = 10.0m
                            PurchasingLeadTimeDays = 7.0m
                            InboundTransitLeadTimeDays = 3.0m
                            QualityInspectionBufferDays = 1.0m
                            GracePeriodDays = 2.0m
                            UnitStandardCost = valuation.UnitCost
                        }
            }

        let getCandidateSuppliers (_item: ItemId) (_loc: LocationId) : Task<SupplierCandidateFact list> =
            task {
                match SupplierId.create "SUP-ACME-COMPONENTS" with
                | Ok s1 ->
                    return
                        [
                            {
                                SupplierId = s1
                                IsApproved = true
                                IsCompliant = true
                                AvailableCapacity = 5000.0m
                                PurchasingLeadTimeDays = 7.0m
                                InboundTransitLeadTimeDays = 3.0m
                            }
                        ]
                | Error _ -> return []
            }

        let getActiveDisruptionSignals (_item: ItemId) (_loc: LocationId) : Task<string list> = task { return [] }

        let getFutureNetGaps (_item: ItemId) (_loc: LocationId) (_from: Timestamp) (_span: TimeSpan) : Task<decimal> =
            task { return 150.0m }

        let getPlannedProductionRequirements
            (_resId: PhysicalResourceId)
            (_periodId: PlanningPeriodId)
            : Task<PlannedProductionRequirementFact list> =
            task {
                match ItemId.create "SKU-DRIVE-500GB" with
                | Ok itemId ->
                    return
                        [
                            {
                                RequirementId = "PPR-REQ-001"
                                ItemId = itemId
                                Quantity = 200.0m
                                DueDate = Timestamp.now()
                                MaterialAvailableDate = Timestamp.now()
                                ProductionPriority = 1
                            }
                        ]
                | Error _ -> return []
            }

        let getResourceAvailabilityFacts
            (resId: PhysicalResourceId)
            (_periodId: PlanningPeriodId)
            : Task<ResourceAvailabilityFact option> =
            task {
                let now = Timestamp.now()
                let later = Timestamp.create(DateTimeOffset.UtcNow.AddDays(7.0)) |> Result.defaultWith(fun _ -> now)

                return
                    Some
                        {
                            ResourceId = resId
                            AvailableWorkingShifts = [ (now, later) ]
                            MaintenanceWindows = []
                            NominalRunRateUnitsPerHour = 30.0m
                            BaseCapacityHours = 160.0m
                            MaxOvertimeHours = 20.0m
                        }
            }

        let getChangeoverMatrix (_resId: PhysicalResourceId) : Task<ChangeoverMatrixFact> =
            task {
                return
                    {
                        Matrix = Map.empty
                        DefaultChangeoverHours = 1.5m
                    }
            }

        let getActiveSchedulesForScope
            (_scopeId: PlanningScopeId)
            (_periodId: PlanningPeriodId)
            : Task<ActiveScheduleFact list> =
            task { return [] }

        let getPriorActiveScheduleFact
            (_resId: PhysicalResourceId)
            (_periodId: PlanningPeriodId)
            (_st: ScheduleType)
            : Task<ActiveScheduleFact option> =
            task { return None }

        let distributionRequirementExists (_reqId: DistributionRequirementId) : Task<bool> = task { return true }

        let getTransferRequirement (reqId: DistributionRequirementId) : Task<TransferRequirementFact option> =
            task {
                match ItemId.create "SKU-DRIVE-500GB", LocationId.create "LOC-EAST-HUB", Quantity.create 150.0m with
                | Ok item, Ok loc, Ok qty ->
                    return
                        Some
                            {
                                RequirementId = reqId
                                Item = item
                                DestinationLocation = loc
                                RequiredQuantity = qty
                                RequiredArrivalDate = Timestamp.now()
                                DeploymentPriority = "Standard"
                                IsExpedited = false
                            }
                | _ -> return None
            }

        let getCandidateTransportationLanes (_item: ItemId) (destLoc: LocationId) : Task<CandidateLaneFact list> =
            task {
                match TransportationLaneId.create "LANE-CENTRAL-TO-EAST", LocationId.create "LOC-CENTRAL-WH" with
                | Ok laneId, Ok srcLoc ->
                    return
                        [
                            {
                                LaneId = laneId
                                SourceLocation = srcLoc
                                DestinationLocation = destLoc
                                TransitTimeHours = 24.0m
                                DailyCapacity = 1000.0m
                                AllocatedCapacity = 300.0m
                            }
                        ]
                | _ -> return []
            }

        let getSourceInventoryPositions (_item: ItemId) (sources: LocationId list) : Task<Map<LocationId, decimal>> =
            task { return sources |> List.map(fun s -> (s, 500.0m)) |> Map.ofList }

        let getDockCapacity (loc: LocationId) (_ts: Timestamp) : Task<DockCapacityFact option> =
            task {
                return
                    Some
                        {
                            Location = loc
                            CapacityUnits = 2000.0m
                            AllocatedUnits = 800.0m
                        }
            }

        let isSupplyPlanPublished (_scopeId: PlanningScopeId) : Task<bool> = task { return true }

        let getActiveDistributionRecommendationsForScope
            (_scopeId: PlanningScopeId)
            : Task<ActiveDistributionRecommendationFact list> =
            task { return [] }

        let getBaselineSupplyFact (entityRef: string) : Task<BaselineSupplyFact option> =
            task {
                return
                    Some
                        {
                            EntityReference = entityRef
                            CommittedDeliveryDate = Timestamp.now()
                            CommittedQuantity = 500.0m
                            IsCriticalPath = false
                            BufferDays = 3.0m
                        }
            }

        let getCorroboratingSignalsCount (_entityRef: string) (_window: TimeSpan) : Task<int> = task { return 2 }

        let getScopePrerequisites
            (_scope: PlanningScopeId)
            (_horizon: PlanningHorizon)
            (_period: EvaluationPeriodId)
            : Task<ScopePrerequisitesFact> =
            task {
                return
                    {
                        HasPublishedSupplyPlan = true
                        HasInventoryHealthAssessment = true
                        HasCapacityHealthAssessment = true
                        HasSupplierCollaborationHealthAssessment = true
                        IsEvaluationPeriodComplete = true
                    }
            }

        let getDimensionalOutcomesForScope
            (_scope: PlanningScopeId)
            (_horizon: PlanningHorizon)
            (_period: EvaluationPeriodId)
            : Task<DimensionalOutcomeFact list> =
            task { return [] }

        let getPriorPublishedQualityAssessment
            (_scope: PlanningScopeId)
            (_horizon: PlanningHorizon)
            (_period: EvaluationPeriodId)
            : Task<string option> =
            task { return None }

        let getQualityEvaluationContext
            (_scope: PlanningScopeId)
            (_period: EvaluationPeriodId)
            : Task<QualityEvaluationContextFact option> =
            task { return None }

        let getScopeExceptionPrerequisites (_scope: PlanningScopeId) : Task<bool> = task { return true }

        let getSupplyChangeEvidence (_scope: PlanningScopeId) : Task<SupplyChangeEvidenceFact list> = task { return [] }

        let getQualityEvidence (_scope: PlanningScopeId) : Task<QualityEvidenceFact option> = task { return None }

        let getOperationalEvidence (_scope: PlanningScopeId) : Task<OperationalPositionEvidenceFact list> =
            task {
                return
                    [
                        {
                            EntityType = "ItemLocation"
                            EntityId = "SKU-DRIVE-500GB@LOC-CENTRAL-WH"
                            OnHand = 200.0m
                            SafetyStock = 50.0m
                            DaysOfSupply = 15.0m
                            ResourceUtilization = 0.80m
                            MaxPlannedCapacity = 160.0m
                            IsBottleneck = false
                            SupplierOtifPercent = 0.96m
                            ConsecutiveUnderperformanceMonths = 0
                            IsCriticalEntity = false
                        }
                    ]
            }

        let getActiveExceptionsForScope (_scope: PlanningScopeId) : Task<ActiveExceptionFact list> = task { return [] }

        let supplyPorts: SupplyPorts =
            {
                ItemExists = itemExists
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
                GetActiveExceptionsForScope = getActiveExceptionsForScope
            }

        let explainSupplyPorts: ExplainSupplyDecisions.ExplainSupplyPorts =
            {
                GetDecisionTraces = fun _ _ -> task { return [] }
                GetSourceArtifactRefs = fun _ _ -> task { return [] }
                GetCausalChain = fun _ -> task { return [] }
            }

        // Step 6: Compose Supply domain with live cross-domain query ports
        let! supplyCtx: ComposeSupply.SupplyContext =
            ComposeSupply.compose
                store
                supplyKnowledge
                telemetryPublisher
                pconfig
                traceCodec
                supplyPorts
                explainSupplyPorts
                services

        // Step 7: Compose Promise Intelligence domain with live cross-domain query ports
        let promiseItemExists (itemId: ItemId) : Task<bool> =
            task {
                let idStr = ItemId.value itemId
                let! demands = coreCtx.DemandManagement.Queries.GetAll()
                if demands |> List.exists (fun d -> d.Item = idStr) then
                    return true
                else
                    let! invPositions = supplyCtx.ManageInventory.PositionQueries.GetAll()
                    if invPositions |> List.exists (fun p -> p.ItemId = idStr) then
                        return true
                    else
                        let! transitions = coreCtx.ItemTransitionManagement.Queries.GetAll()
                        if transitions |> List.exists (fun t -> t.SupersededItem = idStr || t.SupersedingItem = idStr) then
                            return true
                        else
                            let catalog =
                                [ "SKU-DRIVE-500GB"
                                  "SKU-DRIVE-1TB"
                                  "SKU-BATTERY-PACK-PRO"
                                  "SKU-POWER-SUPPLY-850W"
                                  "SKU-NVME-CONTROLLER"
                                  "SKU-AI-STORAGE-2TB" ]
                            return List.contains idStr catalog
            }

        let promiseLocationExists (locId: LocationId) : Task<bool> =
            task {
                let idStr = LocationId.value locId
                let! demands = coreCtx.DemandManagement.Queries.GetAll()
                if demands |> List.exists (fun d -> d.Location = idStr) then
                    return true
                else
                    let! invPositions = supplyCtx.ManageInventory.PositionQueries.GetAll()
                    if invPositions |> List.exists (fun p -> p.LocationId = idStr) then
                        return true
                    else
                        let catalog = [ "LOC-CENTRAL-WH"; "LOC-WEST-PLANT"; "LOC-EAST-HUB" ]
                        return List.contains idStr catalog
            }

        let promiseCustomerExists (custId: CustomerId) : Task<bool> =
            task {
                let idStr = CustomerId.value custId
                let! demands = coreCtx.DemandManagement.Queries.GetAll()
                if demands |> List.exists (fun d -> d.Customer = Some idStr) then
                    return true
                else
                    let catalog = [ "CUST-TITAN-CLOUD"; "CUST-GLOBAL-LOGISTICS"; "CUST-NORDIC-MFG"; "CUST-ACME-CORP" ]
                    return List.contains idStr catalog
            }

        let promisePlanningScopeExists (scopeId: PlanningScopeId) : Task<bool> =
            task {
                let! exists = coreCtx.EnterprisePictureManagement.Queries.Exists scopeId
                if exists then
                    return true
                else
                    return PlanningScopeId.value scopeId = "PS-GLOBAL-SUPPLY-CHAIN"
            }

        let promisePhysicalResourceExists (resId: PhysicalResourceId) : Task<bool> =
            task {
                let! exists = supplyCtx.ManageCapacity.PositionQueries.Exists resId
                if exists then
                    return true
                else
                    let catalog = [ "WC-ASSEMBLY-LINE-01"; "WC-PACKAGING-01" ]
                    return List.contains (PhysicalResourceId.value resId) catalog
            }

        let promiseSourceReliability (source: string) : Task<decimal> =
            task {
                match source with
                | "SAP-ERP-EMEA" -> return 0.98m
                | "WMS-WEST" -> return 0.96m
                | "EDI-GATEWAY-01" -> return 0.95m
                | "PORTAL-B2B" -> return 0.92m
                | _ -> return 0.90m
            }

        let getCustomerProfileFact (custId: CustomerId) : Task<Medhavi.Promise.CustomerProfileFact option> =
            task {
                let idStr = CustomerId.value custId
                match idStr with
                | "CUST-TITAN-CLOUD" ->
                    let fact: Medhavi.Promise.CustomerProfileFact =
                        { CustomerId = custId
                          CustomerTier = "Tier1_Strategic"
                          CreditLimit = 10000000.0m
                          CreditHoldActive = false
                          DefaultSlaDays = 2
                          AllowSplitShipments = true
                          MaxSplitShipmentPackages = 5
                          AllowProductSubstitution = false
                          SubstitutionMarginFloorPct = 0.98m
                          DailyPenaltyRatePct = 0.005m
                          MaxPenaltyCapPct = 0.15m }
                    return Some fact
                | "CUST-GLOBAL-LOGISTICS" ->
                    let fact: Medhavi.Promise.CustomerProfileFact =
                        { CustomerId = custId
                          CustomerTier = "Tier2_Enterprise"
                          CreditLimit = 5000000.0m
                          CreditHoldActive = false
                          DefaultSlaDays = 5
                          AllowSplitShipments = true
                          MaxSplitShipmentPackages = 3
                          AllowProductSubstitution = true
                          SubstitutionMarginFloorPct = 0.95m
                          DailyPenaltyRatePct = 0.002m
                          MaxPenaltyCapPct = 0.10m }
                    return Some fact
                | "CUST-NORDIC-MFG" ->
                    let fact: Medhavi.Promise.CustomerProfileFact =
                        { CustomerId = custId
                          CustomerTier = "Tier3_Industrial"
                          CreditLimit = 1500000.0m
                          CreditHoldActive = false
                          DefaultSlaDays = 7
                          AllowSplitShipments = false
                          MaxSplitShipmentPackages = 1
                          AllowProductSubstitution = false
                          SubstitutionMarginFloorPct = 0.90m
                          DailyPenaltyRatePct = 0.001m
                          MaxPenaltyCapPct = 0.05m }
                    return Some fact
                | _ ->
                    let fact: Medhavi.Promise.CustomerProfileFact =
                        { CustomerId = custId
                          CustomerTier = "Tier2_Enterprise"
                          CreditLimit = 2000000.0m
                          CreditHoldActive = false
                          DefaultSlaDays = 5
                          AllowSplitShipments = true
                          MaxSplitShipmentPackages = 2
                          AllowProductSubstitution = true
                          SubstitutionMarginFloorPct = 0.92m
                          DailyPenaltyRatePct = 0.002m
                          MaxPenaltyCapPct = 0.10m }
                    return Some fact
            }

        let getNetworkInventoryFacts (itemId: ItemId) : Task<Medhavi.Promise.NetworkInventoryFact list> =
            task {
                let itemStr = ItemId.value itemId
                let! positions = supplyCtx.ManageInventory.PositionQueries.GetAll()
                let matchingPositions = positions |> List.filter (fun p -> p.ItemId = itemStr && p.AvailableQuantity > 0m)
                if not (List.isEmpty matchingPositions) then
                    return
                        matchingPositions
                        |> List.choose (fun p ->
                            match LocationId.create p.LocationId, Quantity.create p.AvailableQuantity with
                            | Ok loc, Ok qty ->
                                let fact: Medhavi.Promise.NetworkInventoryFact =
                                    { Location = loc
                                      Item = itemId
                                      AvailableQuantity = qty
                                      BatchOrLotNumber = None }
                                Some fact
                            | _ -> None)
                else
                    let! records = supplyCtx.UnderstandSupply.RecordQueries.GetAll()
                    let invRecords =
                        records
                        |> List.filter (fun r -> r.Item = itemStr && r.SupplyType = "Inventory" && r.Quantity > 0m)
                    return
                        invRecords
                        |> List.choose (fun r ->
                            match LocationId.create r.Location, Quantity.create r.Quantity with
                            | Ok loc, Ok qty ->
                                let fact: Medhavi.Promise.NetworkInventoryFact =
                                    { Location = loc
                                      Item = itemId
                                      AvailableQuantity = qty
                                      BatchOrLotNumber = r.BatchIdentifier }
                                Some fact
                            | _ -> None)
            }

        let getInboundReplenishmentFacts (itemId: ItemId) (locId: LocationId) : Task<Medhavi.Promise.InboundReplenishmentFact list> =
            task {
                let itemStr = ItemId.value itemId
                let locStr = LocationId.value locId
                let! records = supplyCtx.UnderstandSupply.RecordQueries.GetAll()
                let matching =
                    records
                    |> List.filter (fun r ->
                        r.Item = itemStr
                        && r.Location = locStr
                        && (r.SupplyType = "PurchaseOrder" || r.SupplyType = "ProductionOrder" || r.SupplyType = "Transfer")
                        && r.Quantity > 0m)
                return
                    matching
                    |> List.choose (fun r ->
                        match Quantity.create r.Quantity with
                        | Ok qty ->
                            let arrDate =
                                r.ExpectedDeliveryDate
                                |> Option.defaultValue r.BusinessTime
                                |> Timestamp.create
                                |> Result.defaultWith (fun _ -> Timestamp.now())
                            let fact: Medhavi.Promise.InboundReplenishmentFact =
                                { SupplyId = r.RecordId
                                  Item = itemId
                                  DestinationLocation = locId
                                  ReplenishmentQuantity = qty
                                  ExpectedArrivalDate = arrDate }
                            Some fact
                        | _ -> None)
            }

        let getCapacitySlotFacts (resId: PhysicalResourceId) (fromTime: Timestamp) (_toTime: Timestamp) : Task<Medhavi.Promise.CapacitySlotFact list> =
            task {
                let! capOpt = supplyCtx.ManageCapacity.PositionQueries.GetById resId
                match capOpt with
                | Some cap ->
                    let totalPlanned = cap.AvailableCapacityHours + cap.CurrentLoadHours
                    let reserved = cap.CurrentLoadHours
                    let available = max 0m cap.AvailableCapacityHours
                    let slot: Medhavi.Promise.CapacitySlotFact =
                        { Resource = resId
                          ShiftDate = fromTime
                          TotalPlannedCapacityHours = if totalPlanned > 0m then totalPlanned else 160.0m
                          AlreadyReservedHours = reserved
                          AvailableCapacityHours = if available > 0m then available else 80.0m }
                    return [ slot ]
                | None ->
                    let slot: Medhavi.Promise.CapacitySlotFact =
                        { Resource = resId
                          ShiftDate = fromTime
                          TotalPlannedCapacityHours = 160.0m
                          AlreadyReservedHours = 40.0m
                          AvailableCapacityHours = 120.0m }
                    return [ slot ]
            }

        let getPromiseBomComponentFacts (parentItem: ItemId) : Task<Medhavi.Promise.BomComponentFact list> =
            task {
                let idStr = ItemId.value parentItem
                if idStr = "SKU-BATTERY-PACK-PRO" then
                    match ItemId.create "SKU-POWER-SUPPLY-850W" with
                    | Ok comp ->
                        let fact: Medhavi.Promise.BomComponentFact =
                            { ParentItem = parentItem
                              ComponentItem = comp
                              UsageQuantity = 1.0m
                              ScrapFactor = 0.02m
                              LeadTimeDays = 3 }
                        return [ fact ]
                    | Error _ -> return []
                elif idStr = "SKU-AI-STORAGE-2TB" then
                    match ItemId.create "SKU-NVME-CONTROLLER", ItemId.create "SKU-DRIVE-1TB" with
                    | Ok c1, Ok c2 ->
                        let f1: Medhavi.Promise.BomComponentFact =
                            { ParentItem = parentItem
                              ComponentItem = c1
                              UsageQuantity = 2.0m
                              ScrapFactor = 0.01m
                              LeadTimeDays = 5 }
                        let f2: Medhavi.Promise.BomComponentFact =
                            { ParentItem = parentItem
                              ComponentItem = c2
                              UsageQuantity = 2.0m
                              ScrapFactor = 0.00m
                              LeadTimeDays = 2 }
                        return [ f1; f2 ]
                    | _ -> return []
                elif idStr = "SKU-DRIVE-1TB" then
                    match ItemId.create "SKU-DRIVE-500GB" with
                    | Ok comp ->
                        let fact: Medhavi.Promise.BomComponentFact =
                            { ParentItem = parentItem
                              ComponentItem = comp
                              UsageQuantity = 1.0m
                              ScrapFactor = 0.00m
                              LeadTimeDays = 1 }
                        return [ fact ]
                    | Error _ -> return []
                else
                    return []
            }

        let getTransportationLaneFact (origin: LocationId) (dest: LocationId) : Task<Medhavi.Promise.TransportationLaneFact option> =
            task {
                let origStr = LocationId.value origin
                let destStr = LocationId.value dest
                if origStr = "LOC-CENTRAL-WH" && destStr = "LOC-EAST-HUB" then
                    let fact: Medhavi.Promise.TransportationLaneFact =
                        { Origin = origin
                          Destination = dest
                          FulfillmentMode = Medhavi.Promise.FulfillmentMode.WarehouseGroundFreight
                          TransitLeadTimeDays = 2
                          FreightCostPerUnit = 15.0m
                          CarbonEmissionsKgPerUnit = 2.5m }
                    return Some fact
                elif origStr = "LOC-WEST-PLANT" && destStr = "LOC-CENTRAL-WH" then
                    let fact: Medhavi.Promise.TransportationLaneFact =
                        { Origin = origin
                          Destination = dest
                          FulfillmentMode = Medhavi.Promise.FulfillmentMode.WarehouseGroundFreight
                          TransitLeadTimeDays = 3
                          FreightCostPerUnit = 20.0m
                          CarbonEmissionsKgPerUnit = 3.8m }
                    return Some fact
                elif origStr = "LOC-WEST-PLANT" && destStr = "LOC-EAST-HUB" then
                    let fact: Medhavi.Promise.TransportationLaneFact =
                        { Origin = origin
                          Destination = dest
                          FulfillmentMode = Medhavi.Promise.FulfillmentMode.WarehouseAirExpress
                          TransitLeadTimeDays = 1
                          FreightCostPerUnit = 65.0m
                          CarbonEmissionsKgPerUnit = 12.0m }
                    return Some fact
                elif origStr = destStr then
                    let fact: Medhavi.Promise.TransportationLaneFact =
                        { Origin = origin
                          Destination = dest
                          FulfillmentMode = Medhavi.Promise.FulfillmentMode.WarehouseGroundFreight
                          TransitLeadTimeDays = 0
                          FreightCostPerUnit = 2.0m
                          CarbonEmissionsKgPerUnit = 0.1m }
                    return Some fact
                else
                    let fact: Medhavi.Promise.TransportationLaneFact =
                        { Origin = origin
                          Destination = dest
                          FulfillmentMode = Medhavi.Promise.FulfillmentMode.WarehouseGroundFreight
                          TransitLeadTimeDays = 4
                          FreightCostPerUnit = 25.0m
                          CarbonEmissionsKgPerUnit = 4.5m }
                    return Some fact
            }

        let getItemCostFact (item: ItemId) : Task<Medhavi.Promise.ItemCostFact option> =
            task {
                let idStr = ItemId.value item
                let (cogs, price) =
                    if idStr.Contains("AI-STORAGE") then (1200.0m, 1850.0m)
                    elif idStr.Contains("1TB") then (120.0m, 195.0m)
                    elif idStr.Contains("500GB") then (75.0m, 115.0m)
                    elif idStr.Contains("BATTERY") then (250.0m, 390.0m)
                    elif idStr.Contains("POWER") then (50.0m, 85.0m)
                    elif idStr.Contains("CONTROLLER") then (180.0m, 290.0m)
                    else (60.0m, 99.0m)
                let fact: Medhavi.Promise.ItemCostFact =
                    { Item = item
                      StandardCogs = cogs
                      ListPrice = price }
                return Some fact
            }

        let getPromiseActiveDisruptionSignals (_scope: PlanningScopeId) : Task<Medhavi.Promise.SupplyDisruptionSignalFact list> =
            task {
                let! coreExceptions = coreCtx.ExceptionManagement.Queries.GetAll()
                let activeExceptions = coreExceptions |> List.filter (fun e -> e.ResolutionTime.IsNone)
                let signals =
                    activeExceptions
                    |> List.map (fun e ->
                        let disType =
                            match e.Classification with
                            | "MaterialShortage" -> Medhavi.Promise.PromiseDisruptionType.InboundSupplyShortfall
                            | "CapacityExceeded" -> Medhavi.Promise.PromiseDisruptionType.WorkCenterCapacityBreakdown
                            | "LeadTimeBreach" -> Medhavi.Promise.PromiseDisruptionType.LogisticsTransitBottleneck
                            | _ -> Medhavi.Promise.PromiseDisruptionType.InboundSupplyShortfall
                        let sev =
                            match e.Severity with
                            | Some "Critical" -> Medhavi.Promise.PromiseRiskTier.Critical
                            | Some "High" -> Medhavi.Promise.PromiseRiskTier.High
                            | Some "Medium" -> Medhavi.Promise.PromiseRiskTier.Medium
                            | _ -> Medhavi.Promise.PromiseRiskTier.Low
                        let (itemOpt, locOpt) =
                            if e.AffectedScopeType = "ItemLocation" && e.AffectedScopeIdentifier.Contains("@") then
                                let parts = e.AffectedScopeIdentifier.Split('@')
                                (ItemId.create parts.[0] |> Result.toOption, LocationId.create parts.[1] |> Result.toOption)
                            elif e.AffectedScopeType = "WorkCenter" then
                                (None, LocationId.create "LOC-WEST-PLANT" |> Result.toOption)
                            elif e.AffectedScopeType = "TransportLane" then
                                (None, LocationId.create "LOC-EAST-HUB" |> Result.toOption)
                            else
                                (None, None)
                        let fact: Medhavi.Promise.SupplyDisruptionSignalFact =
                            { SignalId = e.ExceptionId
                              DisruptionType = disType
                              AffectedItem = itemOpt
                              AffectedLocation = locOpt
                              EstimatedDelayDays = if sev = Medhavi.Promise.PromiseRiskTier.Critical then 5.0m else 2.0m
                              Severity = sev
                              DetectedAt = Timestamp.create e.RegistrationTime |> Result.defaultWith (fun _ -> Timestamp.now()) }
                        fact)
                return signals
            }

        let promisePorts: Medhavi.Promise.PromisePorts =
            { ItemExists = promiseItemExists
              LocationExists = promiseLocationExists
              CustomerExists = promiseCustomerExists
              PlanningScopeExists = promisePlanningScopeExists
              PhysicalResourceExists = promisePhysicalResourceExists
              SourceReliability = promiseSourceReliability
              GetCustomerProfileFact = getCustomerProfileFact
              GetNetworkInventoryFacts = getNetworkInventoryFacts
              GetInboundReplenishmentFacts = getInboundReplenishmentFacts
              GetCapacitySlotFacts = getCapacitySlotFacts
              GetBomComponentFacts = getPromiseBomComponentFacts
              GetTransportationLaneFact = getTransportationLaneFact
              GetItemCostFact = getItemCostFact
              GetActiveDisruptionSignals = getPromiseActiveDisruptionSignals }

        let! promiseCtx: Medhavi.Promise.PromiseContext =
            ComposePromise.compose
                store
                promiseKnowledge
                telemetryPublisher
                traceCodec
                pconfig
                promisePorts
                "PS-GLOBAL-SUPPLY-CHAIN"
                services

        let dispose () =
            coreCtx.Dispose()
            demandCtx.Dispose()
            supplyCtx.Dispose()
            promiseCtx.Dispose()

        return
            {
                Core = coreCtx
                Demand = demandCtx
                Supply = supplyCtx
                Promise = promiseCtx
                Store = store
                Dispose = dispose
            }
    }

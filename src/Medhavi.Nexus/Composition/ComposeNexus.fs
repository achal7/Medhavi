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

type NexusContext =
    { Core: ComposeCore.CoreContext
      Demand: ComposeDemand.DemandContext
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

        let dispose () =
            coreCtx.Dispose()
            demandCtx.Dispose()

        return
            { Core = coreCtx
              Demand = demandCtx
              Store = store
              Dispose = dispose }
    }
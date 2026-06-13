namespace Medhavi.Nexus

open System
open System.Threading.Tasks
open Medhavi.Common
open Medhavi.Common.Patterns
open Medhavi.Scheduler.Mrp.Domain
open Medhavi.Scheduler.Mrp.Application
open Medhavi.Contracts.Integration
open Medhavi.SharedKernel
open Medhavi.Scheduler.Mrp.Domain.Policies
open Medhavi.Scheduler.Mrp.Domain.MrpRunAggregate
open Medhavi.Scheduler.Mrp.Domain.Types
open Medhavi.SharedKernel.BoundedContexts
open Medhavi.Integration
open Medhavi.Demand

type MrpLogger =
    { LogInfo: string -> unit
      LogWarning: string -> unit
      LogError: string -> unit }

module Mrp =

    let mutable latestMrpRun: MrpRunResult option = None

    let runBaselineMrp (deps: MrpDependencies) (demandContext: Medhavi.Demand.DemandContext) (logger: MrpLogger) =
        task {
            logger.LogInfo "\n--- [EXECUTING BASELINE MRP RUN] ---"
            let now = DateTimeOffset.UtcNow

            let sp =
                StockingPointId.create "SP-WAREHOUSE"
                |> Result.get

            let node name = NodeId.create name |> Result.get

            let! demands = demandContext.Queries.DemandLine.GetAllDemandLines()

            let mrpDemands =
                demands
                |> List.map (fun d ->
                    let src =
                        match d.DemandCategory with
                        | Domain.DemandCategory.CustomerOrderDemand -> CustomerOrder(d.DemandOrderId, d.DemandLineId)
                        | Domain.DemandCategory.SalesOrderForecast -> Forecast d.DemandLineId
                        | _ -> Manual d.DemandLineId

                    { MrpDemand.DemandId = d.DemandLineId
                      SkuId = d.SkuId
                      NodeId = node (StockingPointId.value d.StockingPointId)
                      StockingPointId = d.StockingPointId
                      Quantity = d.Quantity
                      RequiredDate = Timestamp.create d.RequestedDeliveryDate
                      Source = src
                      Priority = Some d.Priority })

            let mrpServiceInstance = Medhavi.Scheduler.Mrp.MrpService.create deps

            let! runRes =
                mrpServiceInstance.ExecuteRun
                    "MRP-RUN-BASELINE"
                    (Timestamp.create now)
                    (Timestamp.create (now.AddDays(30.0)))
                    sp
                    { MrpPolicy.defaults with
                        CapacityPolicy = CapacityPolicy.finiteCapacity }
                    mrpDemands
                    []

            match runRes with
            | Error err -> logger.LogError(sprintf "   [ ERR ] Baseline MRP Run failed: %A" err)
            | Ok result ->
                latestMrpRun <- Some result
                Medhavi.Nexus.AnalyticsWiring.latestMrpRunRef <- Some result
                logger.LogInfo "   [ OK ] Baseline MRP Run executed and cached."
                logger.LogInfo(sprintf "   Generated Proposals: %d" (List.length result.Proposals))

                if not (List.isEmpty result.Warnings) then
                    logger.LogWarning "   Warnings:"

                    for w in result.Warnings do
                        logger.LogWarning(sprintf "     - %s" w)

                if not (List.isEmpty result.Errors) then
                    logger.LogError "   Errors:"

                    for e in result.Errors do
                        logger.LogError(sprintf "     - %s" e)

                for p in result.Proposals do
                    logger.LogInfo(
                        sprintf
                            "     - PropId: %s | Sku: %s | Qty: %M | Due: %s | Type: %A"
                            (SupplyProposalId.value p.Id)
                            (SkuId.value p.SkuId)
                            (Quantity.value p.Quantity)
                            ((Timestamp.value p.DueDate)
                                .ToString("yyyy-MM-dd HH:mm"))
                            p.ProposalType
                    )
        }

    let handleRequest
        (mrpDep: MrpDependencies)
        (masterDataContext: Medhavi.MasterData.MasterData)
        (logger: MrpLogger)
        (req: IntegrationEvent)
        =
        task {
            match req with
            | ResourceDowntimes resourceDowntimes ->
                for payload in resourceDowntimes do
                    logger.LogWarning(
                        sprintf
                            ">>> [Disruption Ingest] Resource downtime reported: Resource=%s, Start=%s, End=%s, Reason=%s"
                            payload.ResourceId
                            (payload.StartUtc.ToString("yyyy-MM-dd HH:mm"))
                            (payload.EndUtc.ToString("yyyy-MM-dd HH:mm"))
                            payload.Reason
                    )

                    match latestMrpRun with
                    | None -> logger.LogInfo "   - No baseline MRP run cached. Skipping heuristic reactive repair."
                    | Some baseline ->
                        logger.LogInfo "   - Evaluating blast radius and triggering reactive repair..."

                        let event =
                            Medhavi.Scheduler.Mrp.Domain.ResourceBreakdown(
                                payload.ResourceId,
                                Timestamp.create payload.StartUtc,
                                Timestamp.create payload.EndUtc
                            )

                        let severityMap =
                            Map.ofList [ "fullReplanDurationHrs", 24.0; "ignoreDurationHrs", 1.0 ]

                        let! replanResult = ReplanService.executeReplan mrpDep baseline event severityMap

                        match replanResult with
                        | Error err -> logger.LogError(sprintf "   [ ERR ] Reactive repair failed: %A" err)
                        | Ok newRun ->
                            let delta = Replan.PlanDeltaCalculator.calculate baseline newRun
                            logger.LogInfo "   [ OK ] Reactive repair complete."

                            logger.LogInfo(
                                sprintf "     - Churn (Rescheduled): %d" (List.length delta.RescheduledProposals)
                            )

                            logger.LogInfo(sprintf "     - Added Proposals: %d" (List.length delta.AddedProposals))

                            logger.LogInfo(
                                sprintf "     - Cancelled Proposals: %d" (List.length delta.CancelledProposals)
                            )

                            // Update cache and persist new proposals
                            latestMrpRun <- Some newRun
                            Medhavi.Nexus.AnalyticsWiring.latestMrpRunRef <- Some newRun

                            let! (persistRes: Result<unit, string>) =
                                mrpDep.CreateSupplyOrders newRun.RunId newRun.Proposals
                                |> Async.StartAsTask

                            match persistRes with
                            | Ok _ -> logger.LogInfo "     - Repaired plan successfully persisted to database."
                            | Error err ->
                                logger.LogError(sprintf "     - Failed to persist repaired proposals: %s" err)

                ()
            | TransportDelays transportDelays ->
                for payload in transportDelays do
                    logger.LogWarning(
                        sprintf
                            ">>> [Disruption Ingest] Transport delay reported: Leg=%s, DelayMins=%.1f, NewArrival=%s, Reason=%s"
                            payload.TransportLegId
                            payload.EstimatedDelayMinutes
                            (payload.NewArrivalUtc.ToString("yyyy-MM-dd HH:mm"))
                            payload.Reason
                    )

                    match latestMrpRun with
                    | None -> logger.LogInfo "   - No baseline MRP run cached. Skipping heuristic reactive repair."
                    | Some baseline ->
                        let! legOpt = masterDataContext.Queries.TransportLeg.GetById(payload.TransportLegId)

                        match legOpt with
                        | None ->
                            logger.LogError(
                                sprintf "   - Transport leg %s not found in database. Skipping." payload.TransportLegId
                            )
                        | Some leg ->
                            let matchedPropOpt =
                                baseline.Proposals
                                |> List.filter (fun p -> p.ProposalType = PlannedTransferOrder)
                                |> List.tryFind (fun p ->
                                    (StockingPointId.value p.StockingPointId)
                                        .Equals(leg.Destination, StringComparison.OrdinalIgnoreCase)
                                    && (p.SupplierId
                                        |> Option.map SupplierId.value
                                        |> Option.defaultValue "")
                                        .Equals(leg.Origin, StringComparison.OrdinalIgnoreCase))

                            match matchedPropOpt with
                            | None ->
                                logger.LogWarning(
                                    sprintf
                                        "   - No active transfer order proposal matches leg %s (Origin=%s, Dest=%s). Skipping."
                                        payload.TransportLegId
                                        leg.Origin
                                        leg.Destination
                                )
                            | Some prop ->
                                logger.LogInfo(
                                    sprintf
                                        "   - Found matching Transfer Order proposal %s. Triggering reactive repair..."
                                        (SupplyProposalId.value prop.Id)
                                )

                                let event =
                                    Medhavi.Scheduler.Mrp.Domain.MaterialDelay(
                                        prop.SkuId,
                                        prop.StockingPointId,
                                        Timestamp.create payload.NewArrivalUtc,
                                        SupplyProposalId.value prop.Id
                                    )

                                let severityMap = Map.ofList [ "fullReplanDelayHrs", 48.0; "ignoreDelayHrs", 2.0 ]

                                let! replanResult = ReplanService.executeReplan mrpDep baseline event severityMap

                                match replanResult with
                                | Error err -> logger.LogError(sprintf "   [ ERR ] Reactive repair failed: %A" err)
                                | Ok newRun ->
                                    let delta = Replan.PlanDeltaCalculator.calculate baseline newRun
                                    logger.LogInfo "   [ OK ] Reactive repair complete."

                                    logger.LogInfo(
                                        sprintf
                                            "     - Churn (Rescheduled): %d"
                                            (List.length delta.RescheduledProposals)
                                    )

                                    logger.LogInfo(
                                        sprintf "     - Added Proposals: %d" (List.length delta.AddedProposals)
                                    )

                                    logger.LogInfo(
                                        sprintf "     - Cancelled Proposals: %d" (List.length delta.CancelledProposals)
                                    )

                                    latestMrpRun <- Some newRun
                                    Medhavi.Nexus.AnalyticsWiring.latestMrpRunRef <- Some newRun

                                    let! (persistRes: Result<unit, string>) =
                                        mrpDep.CreateSupplyOrders newRun.RunId newRun.Proposals
                                        |> Async.StartAsTask

                                    match persistRes with
                                    | Ok _ -> logger.LogInfo "     - Repaired plan successfully persisted to database."
                                    | Error err ->
                                        logger.LogError(sprintf "     - Failed to persist repaired proposals: %s" err)

                ()
            | _ -> logger.LogError(sprintf "INVALID REQUEST %A for MRP" req)
        }

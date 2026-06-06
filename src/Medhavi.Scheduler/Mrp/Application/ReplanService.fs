namespace Medhavi.Scheduler.Mrp.Application

open System
open System.Threading.Tasks
open Medhavi.Common.Patterns
open Medhavi.SharedKernel
open Medhavi.Scheduler.Mrp.Domain
open Medhavi.Scheduler.Mrp.Domain.Types
open Medhavi.Scheduler.Mrp.Domain.Errors
open Medhavi.Scheduler.Mrp.Domain.Policies
open Medhavi.Scheduler.Mrp.Domain.MrpRunAggregate
open Medhavi.Scheduler.Mrp.Pipeline
open Medhavi.Scheduler.Mrp.Steps

module ReplanService =

    // Adapt dependencies to inject baseline proposals as virtual firmed inbound
    let private adaptInboundQuery
        (originalInboundQuery: NettingStep.InboundQuery)
        (virtualInbound: (Timestamp * Quantity * bool * string) list)
        : NettingStep.InboundQuery =
        fun skuId spId start endT ->
            task {
                let! dbInbound = originalInboundQuery skuId spId start endT
                let skuPrefix = $"virt-{SkuId.value skuId}-"
                let filteredVirtual =
                    virtualInbound
                    |> List.filter (fun (_, _, _, id) ->
                        id.StartsWith(skuPrefix, StringComparison.OrdinalIgnoreCase))
                return dbInbound @ filteredVirtual
            }

    let executeReplan
        (deps: MrpDependencies)
        (baseline: MrpRunResult)
        (event: DisruptionEvent)
        (severityThresholds: Map<string, float>)
        : TaskResult<MrpRunResult, MrpApplicationError> =
        task {
            // 1. Determine the mode
            let mode = Replan.ReplanDispatcher.determineMode event severityThresholds
            
            match mode with
            | Ignore ->
                return Ok baseline
                
            | FullReplan ->
                // Run full MRP pipeline again
                // Extract all demands from baseline peggings
                let demands =
                    baseline.Peggings
                    |> List.filter (fun p -> p.Status = PegStatus.Active && not (p.Demand.DemandId.StartsWith("comp-")))
                    |> List.map (fun p ->
                        { MrpDemand.DemandId = p.Demand.DemandId
                          SkuId = p.Demand.SkuId
                          NodeId = p.Demand.NodeId
                          StockingPointId = p.Demand.StockingPointId
                          Quantity = p.PeggedQty
                          RequiredDate = p.Demand.NeedDate
                          Source = Manual p.Demand.DemandId
                          Priority = None })
                    |> List.distinctBy (fun d -> d.DemandId)

                let pipeline = Orchestrator.createPipeline deps
                let spId = 
                    baseline.NetRequirements 
                    |> List.tryHead 
                    |> Option.map (fun nr -> nr.StockingPointId) 
                    |> Option.defaultValue (
                        match StockingPointId.create "SP-FACTORY" with
                        | Ok sp -> sp
                        | Error _ -> failwith "Invalid default SP"
                    )

                let! fullResult =
                    Orchestrator.execute
                        pipeline
                        (MrpRunId.value baseline.RunId + "-replan")
                        baseline.StartTime
                        baseline.EndTime
                        spId
                        MrpPolicy.defaults
                        demands
                        []
                
                return fullResult

            | ReactiveRepair ->
                // Heuristic Reactive Repair
                // Component lookup for BOM cycle traversal
                let componentLookup (skuId: SkuId) =
                    match deps.BomLookup skuId DefaultBom with
                    | Some bom ->
                        bom.Components
                        |> List.map (fun c -> c.ComponentSkuId, c.QuantityPer)
                    | None -> []

                // Blast Radius Impact Assessment
                let (affectedDemands, affectedProposals) =
                    Replan.ImpactAssessment.evaluateBlastRadius baseline event componentLookup

                // Unaffected proposals are treated as firmed virtual inbound
                let unaffectedProposals =
                    baseline.Proposals
                    |> List.filter (fun p -> not (List.contains (SupplyProposalId.value p.Id) affectedProposals))

                let virtualInbound =
                    unaffectedProposals
                    |> List.map (fun p ->
                        let pId = SupplyProposalId.value p.Id
                        let virtId = $"virt-{SkuId.value p.SkuId}-{pId}"
                        (p.DueDate, p.Quantity, true, virtId))

                // Adapt inboundQuery dependency
                let adaptedInbound = adaptInboundQuery deps.InboundQuery virtualInbound
                let adaptedDeps = { deps with InboundQuery = adaptedInbound }

                // Identify affected demands to replan
                let demandsToReplan =
                    baseline.Peggings
                    |> List.filter (fun p -> 
                        List.contains p.Demand.DemandId affectedDemands && 
                        not (p.Demand.DemandId.StartsWith("comp-")))
                    |> List.map (fun p ->
                        { MrpDemand.DemandId = p.Demand.DemandId
                          SkuId = p.Demand.SkuId
                          NodeId = p.Demand.NodeId
                          StockingPointId = p.Demand.StockingPointId
                          Quantity = p.PeggedQty
                          RequiredDate = p.Demand.NeedDate
                          Source = Manual p.Demand.DemandId
                          Priority = None })
                    |> List.distinctBy (fun d -> d.DemandId)

                if List.isEmpty demandsToReplan then
                    return Ok baseline
                else
                    let pipeline = Orchestrator.createPipeline adaptedDeps
                    let! repairResult =
                        Orchestrator.execute
                            pipeline
                            (MrpRunId.value baseline.RunId + "-repair")
                            baseline.StartTime
                            baseline.EndTime
                            demandsToReplan.[0].StockingPointId
                            MrpPolicy.defaults
                            demandsToReplan
                            []

                    match repairResult with
                    | Error err -> return Error err
                    | Ok repRun ->
                        // Merge repair result with unaffected baseline proposals and peggings
                        let unaffectedPeggings =
                            baseline.Peggings
                            |> List.filter (fun p -> not (List.contains p.Demand.DemandId affectedDemands))

                        let mergedProposals = unaffectedProposals @ repRun.Proposals
                        let mergedPeggings = unaffectedPeggings @ repRun.Peggings

                        let finalResult =
                            { repRun with
                                Proposals = mergedProposals
                                Peggings = mergedPeggings }

                        // KPI check (Feasibility Gate & Rollback)
                        let baseKpis = Replan.KPIEvaluator.evaluate baseline None
                        let newKpis = Replan.KPIEvaluator.evaluate finalResult (Some baseline)

                        // If the new lateness is worse, rollback!
                        if newKpis.LateOrdersCount > baseKpis.LateOrdersCount then
                            let warnings = "Replan aborted: repair plan caused worse lateness KPIs. Rolling back." :: baseline.Warnings
                            return Ok { baseline with Warnings = warnings }
                        else
                            return Ok finalResult

            | IncrementalInsert ->
                return Ok baseline
        }

    // Specialized Incremental Insert of a new demand
    let executeIncrementalInsert
        (deps: MrpDependencies)
        (baseline: MrpRunResult)
        (newDemands: MrpDemand list)
        : TaskResult<MrpRunResult, MrpApplicationError> =
        task {
            if List.isEmpty newDemands then
                return Ok baseline
            else
                // Treat ALL baseline proposals as virtual firmed inbound
                let virtualInbound =
                    baseline.Proposals
                    |> List.map (fun p ->
                        let pId = SupplyProposalId.value p.Id
                        let virtId = $"virt-{SkuId.value p.SkuId}-{pId}"
                        (p.DueDate, p.Quantity, true, virtId))

                let adaptedInbound = adaptInboundQuery deps.InboundQuery virtualInbound
                let adaptedDeps = { deps with InboundQuery = adaptedInbound }

                let pipeline = Orchestrator.createPipeline adaptedDeps

                // Run MRP for new demands only
                let! insertResult =
                    Orchestrator.execute
                        pipeline
                        (MrpRunId.value baseline.RunId + "-insert")
                        baseline.StartTime
                        baseline.EndTime
                        newDemands.[0].StockingPointId
                        MrpPolicy.defaults
                        newDemands
                        []

                match insertResult with
                | Error err -> return Error err
                | Ok insRun ->
                    // Merge insert run proposals and peggings with baseline
                    let mergedProposals = baseline.Proposals @ insRun.Proposals
                    let mergedPeggings = baseline.Peggings @ insRun.Peggings

                    let finalResult =
                        { insRun with
                            Proposals = mergedProposals
                            Peggings = mergedPeggings }

                    return Ok finalResult
        }

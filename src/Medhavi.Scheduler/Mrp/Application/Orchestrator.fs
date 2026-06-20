module Medhavi.Scheduler.Mrp.Pipeline.Orchestrator

open System
open System.Threading.Tasks
open Medhavi.Common.Patterns
open Medhavi.SharedKernel
open Medhavi.Scheduler.Mrp.Domain.Types
open Medhavi.Scheduler.Mrp.Domain.Errors
open Medhavi.Scheduler.Mrp.Domain.Policies
open Medhavi.Scheduler.Mrp.Domain.MrpRunAggregate
open Medhavi.Scheduler.Mrp.Domain.Algorithms
open Medhavi.Scheduler.Mrp.Steps
open Medhavi.Scheduler.Mrp.Application

type private NodeKey = SkuId * StockingPointId

type private PlannerState =
    { OpenRequirements: Map<NodeKey, MrpDemand list>
      Worklist: NodeKey list
      InQueue: Set<NodeKey>
      InProgress: Set<NodeKey>
      NetRequirements: NetRequirement list
      Proposals: SupplyProposal list
      TentativeLoad: TentativeLoad
      Context: MrpContext }

let private mkKey (skuId: SkuId) (spId: StockingPointId) : NodeKey = skuId, spId

let private emptyState (ctx: MrpContext) (demands: MrpDemand list) : PlannerState =
    let grouped =
        demands
        |> List.groupBy (fun d -> mkKey d.SkuId d.StockingPointId)
        |> Map.ofList

    let initialWorklist =
        grouped
        |> Map.toList
        |> List.sortBy (fun (_, ds) ->
            ds
            |> List.minBy (fun d -> Timestamp.value d.RequiredDate)
            |> fun d -> Timestamp.value d.RequiredDate)
        |> List.map fst

    { OpenRequirements = grouped
      Worklist = initialWorklist
      InQueue = Set.ofList initialWorklist
      InProgress = Set.empty
      NetRequirements = []
      Proposals = []
      TentativeLoad = Map.empty
      Context = ctx }

let private enqueueDemand (demand: MrpDemand) (state: PlannerState) : Result<PlannerState, MrpStepError> =
    let key = mkKey demand.SkuId demand.StockingPointId

    if Set.contains key state.InProgress then
        let msg =
            $"Cycle detected while adding demand for SKU={SkuId.value demand.SkuId}, StockingPoint={StockingPointId.value demand.StockingPointId}"

        Error(MrpStepError.BomExplosion [ BomExplosionError.CycleDetected [ msg ] ])
    else
        let existing =
            state.OpenRequirements
            |> Map.tryFind key
            |> Option.defaultValue []

        let openRequirements =
            state.OpenRequirements
            |> Map.add key (existing @ [ demand ])

        if Set.contains key state.InQueue then
            Ok
                { state with
                    OpenRequirements = openRequirements }
        else
            Ok
                { state with
                    OpenRequirements = openRequirements
                    Worklist = key :: state.Worklist
                    InQueue = Set.add key state.InQueue }

let private enqueueDemands (demands: MrpDemand list) (state: PlannerState) : Result<PlannerState, MrpStepError> =
    let rec loop s remaining =
        match remaining with
        | [] -> Ok s
        | d :: rest ->
            match enqueueDemand d s with
            | Error e -> Error e
            | Ok s' -> loop s' rest

    loop state demands

let private popNext (state: PlannerState) : option<NodeKey * PlannerState> =
    match state.Worklist with
    | [] -> None
    | key :: rest ->
        let nextState =
            { state with
                Worklist = rest
                InQueue = Set.remove key state.InQueue }

        Some(key, nextState)

let private updateContextWithProposal (proposal: SupplyProposal) (ctx: MrpContext) : MrpContext =
    ctx
    |> MrpContext.addEvent (SupplyProposalCreated proposal)

let private createChildDemands
    (deps: MrpDependencies)
    (state: PlannerState)
    (proposal: SupplyProposal)
    (netReq: NetRequirement)
    : Task<Result<MrpDemand list, MrpStepError>> =
    task {
        match proposal.ProposalType with
        | PlannedTransferOrder ->
            let! sourceSpOpt = deps.TransferSourceQuery netReq.SkuId netReq.StockingPointId

            match sourceSpOpt with
            | None ->
                return
                    Error(
                        MrpStepError.SupplyGeneration
                            [ SupplyGenerationError.NoSupplierFound(SkuId.value netReq.SkuId) ]
                    )
            | Some sourceSpId ->
                let childDemand =
                    { DemandId =
                        $"dep-transfer-{MrpRunId.value state.Context.RunId}-{SkuId.value netReq.SkuId}-{SupplyProposalId.value proposal.Id}"
                      SkuId = netReq.SkuId
                      NodeId =
                        NodeId.create (StockingPointId.value sourceSpId)
                        |> Result.defaultWith (fun _ -> failwith "Invalid NodeId for transfer source")
                      StockingPointId = sourceSpId
                      Quantity = proposal.Quantity
                      RequiredDate = proposal.DueDate
                      Source = Dependent(SkuId.value netReq.SkuId)
                      Priority = None }

                return Ok [ childDemand ]

        | PlannedWorkOrder ->
            match deps.BomLookup netReq.SkuId state.Context.Policy.BomSelectionPolicy with
            | None -> return Ok []

            | Some bom ->
                let childDemands =
                    bom.Components
                    |> List.map (fun comp ->
                        let componentQty =
                            proposal.Quantity
                            * (Quantity.value comp.QuantityPer)

                        { DemandId =
                            $"dep-bom-{MrpRunId.value state.Context.RunId}-{SkuId.value comp.ComponentSkuId}-{SupplyProposalId.value proposal.Id}"
                          SkuId = comp.ComponentSkuId
                          NodeId =
                            proposal.StockingPointId
                            |> StockingPointId.value
                            |> NodeId.create
                            |> Result.defaultWith (fun _ -> failwith "Invalid NodeId for component demand")
                          StockingPointId = netReq.StockingPointId
                          Quantity = componentQty
                          RequiredDate = proposal.DueDate
                          Source = Dependent(SkuId.value netReq.SkuId)
                          Priority = None })

                return Ok childDemands

        | PlannedPurchaseOrder -> return Ok []
    }

let private dateToBucketIndex (startDate: Timestamp) (date: Timestamp) : BucketIndex =
    let startVal = Timestamp.value startDate
    let dateVal = Timestamp.value date
    let diff = (dateVal - startVal).TotalDays
    if diff < 0.0 then 0 else int diff

let private bucketIndexToDate (startDate: Timestamp) (bucket: BucketIndex) : Timestamp =
    let startVal = Timestamp.value startDate
    let target = startVal.AddDays(float bucket)
    Timestamp.create target

let private createCapacityFeasibleProposal
    (deps: MrpDependencies)
    (state: PlannerState)
    (netReq: NetRequirement)
    : Task<Result<SupplyProposal * TentativeLoad, MrpStepError>> =
    task {
        let! proposalResult =
            SupplyGenerationStep.generateProposal
                deps.ProductTypeQuery
                deps.SupplierQuery
                deps.RoutingQuery
                deps.TransferSourceQuery
                (MrpRunId.value state.Context.RunId)
                state.Context.Policy
                netReq

        match proposalResult with
        | Error err -> return Error(MrpStepError.SupplyGeneration [ err ])
        | Ok proposal ->
            match proposal.ProposalType with
            | PlannedWorkOrder when state.Context.Policy.CapacityPolicy.Finite ->
                let! routingDetailsOpt =
                    deps.CapacityRoutingQuery
                        proposal.SkuId
                        proposal.StockingPointId
                        proposal.RoutingId
                        proposal.Quantity

                match routingDetailsOpt with
                | None ->
                    return
                        Error(
                            MrpStepError.SupplyGeneration
                                [ SupplyGenerationError.NoRoutingFound(SkuId.value proposal.SkuId) ]
                        )
                | Some details ->
                    let desiredBucket = dateToBucketIndex state.Context.StartDate proposal.DueDate

                    let! promiseResult =
                        deps.CapacityPromiseQuery
                            details.ResourceGroupId
                            desiredBucket
                            details.NeededDuration
                            state.TentativeLoad

                    if promiseResult.IsFeasible then
                        let finalBucket = promiseResult.EarliestFeasibleBucket
                        let finalDate = bucketIndexToDate state.Context.StartDate finalBucket

                        let updatedProposal =
                            { proposal with
                                DueDate = finalDate
                                CapacityCheckedDate = Some finalDate
                                RoutingId = Some details.RoutingId }

                        let currentLoad =
                            state.TentativeLoad
                            |> Map.tryFind (details.ResourceGroupId, finalBucket)
                            |> Option.defaultValue DurationMinutes.zero

                        let sumVal =
                            DurationMinutes.value currentLoad
                            + DurationMinutes.value details.NeededDuration

                        let newLoad =
                            DurationMinutes.create sumVal
                            |> Result.defaultValue DurationMinutes.zero

                        let updatedLoad =
                            state.TentativeLoad
                            |> Map.add (details.ResourceGroupId, finalBucket) newLoad

                        return Ok(updatedProposal, updatedLoad)
                    else
                        // Try alternate routings
                        let! alternates = deps.AlternateRoutingsQuery proposal.SkuId proposal.StockingPointId

                        let otherAlternates =
                            alternates
                            |> List.filter (fun rId -> Some rId <> proposal.RoutingId)
                            |> List.truncate state.Context.Policy.CapacityPolicy.MaxAlternateAttempts

                        let rec tryAlternates alts =
                            task {
                                match alts with
                                | [] -> return None
                                | altRoutingId :: rest ->
                                    let! altDetailsOpt =
                                        deps.CapacityRoutingQuery
                                            proposal.SkuId
                                            proposal.StockingPointId
                                            (Some altRoutingId)
                                            proposal.Quantity

                                    match altDetailsOpt with
                                    | None -> return! tryAlternates rest
                                    | Some altDetails ->
                                        let! altPromise =
                                            deps.CapacityPromiseQuery
                                                altDetails.ResourceGroupId
                                                desiredBucket
                                                altDetails.NeededDuration
                                                state.TentativeLoad

                                        if altPromise.IsFeasible then
                                            return Some(altDetails, altPromise)
                                        else
                                            return! tryAlternates rest
                            }

                        let! alternateSuccessOpt = tryAlternates otherAlternates

                        match alternateSuccessOpt with
                        | Some(altDetails, altPromise) ->
                            let finalBucket = altPromise.EarliestFeasibleBucket
                            let finalDate = bucketIndexToDate state.Context.StartDate finalBucket

                            let updatedProposal =
                                { proposal with
                                    DueDate = finalDate
                                    CapacityCheckedDate = Some finalDate
                                    RoutingId = Some altDetails.RoutingId }

                            let currentLoad =
                                state.TentativeLoad
                                |> Map.tryFind (altDetails.ResourceGroupId, finalBucket)
                                |> Option.defaultValue DurationMinutes.zero

                            let sumVal =
                                DurationMinutes.value currentLoad
                                + DurationMinutes.value altDetails.NeededDuration

                            let newLoad =
                                DurationMinutes.create sumVal
                                |> Result.defaultValue DurationMinutes.zero

                            let updatedLoad =
                                state.TentativeLoad
                                |> Map.add (altDetails.ResourceGroupId, finalBucket) newLoad

                            return Ok(updatedProposal, updatedLoad)
                        | None ->
                            // Fallback to shifting the primary routing to its earliest feasible bucket
                            let finalBucket = promiseResult.EarliestFeasibleBucket
                            let finalDate = bucketIndexToDate state.Context.StartDate finalBucket

                            // Check if the shifted date exceeds the planning horizon
                            if finalDate > state.Context.EndDate then
                                return
                                    Error(
                                        MrpStepError.SupplyGeneration
                                            [ SupplyGenerationError.CapacityInfeasible(
                                                  SkuId.value proposal.SkuId,
                                                  "Capacity unavailable within planning horizon"
                                              ) ]
                                    )
                            else
                                let updatedProposal =
                                    { proposal with
                                        DueDate = finalDate
                                        CapacityCheckedDate = Some finalDate
                                        RoutingId = Some details.RoutingId }

                                let currentLoad =
                                    state.TentativeLoad
                                    |> Map.tryFind (details.ResourceGroupId, finalBucket)
                                    |> Option.defaultValue DurationMinutes.zero

                                let sumVal =
                                    DurationMinutes.value currentLoad
                                    + DurationMinutes.value details.NeededDuration

                                let newLoad =
                                    DurationMinutes.create sumVal
                                    |> Result.defaultValue DurationMinutes.zero

                                let updatedLoad =
                                    state.TentativeLoad
                                    |> Map.add (details.ResourceGroupId, finalBucket) newLoad

                                return Ok(updatedProposal, updatedLoad)
            | _ ->
                // Non-work orders (purchases, transfers) - load remains unchanged
                return Ok(proposal, state.TentativeLoad)
    }

let private planNode
    (deps: MrpDependencies)
    (state: PlannerState)
    (key: NodeKey)
    : Task<Result<PlannerState, MrpStepError>> =
    task {
        let skuId, spId = key

        if Set.contains key state.InProgress then
            let msg =
                $"Cycle detected while planning SKU={SkuId.value skuId}, StockingPoint={StockingPointId.value spId}"

            return Error(MrpStepError.BomExplosion [ BomExplosionError.CycleDetected [ msg ] ])
        else
            let demands =
                state.OpenRequirements
                |> Map.tryFind key
                |> Option.defaultValue []

            let state =
                { state with
                    OpenRequirements = Map.remove key state.OpenRequirements
                    InProgress = Set.add key state.InProgress }

            let nodeId =
                NodeId.create (StockingPointId.value spId)
                |> Result.defaultWith (fun _ -> failwith "Invalid NodeId")

            let! onHand = deps.OnHandQuery skuId spId
            let! inbound = deps.InboundQuery skuId spId state.Context.StartDate state.Context.EndDate
            let! reservations = deps.ReservationsQuery skuId spId state.Context.StartDate state.Context.EndDate
            let! safetyStock = deps.SafetyStockQuery skuId spId

            let adjustedDemands, adjustedInbound, adjustedReservations =
                NettingStep.adjustForFirmedPegs skuId spId demands inbound reservations state.Context.FirmedPegs

            let nettingInbound =
                adjustedInbound
                |> List.map (fun (t, q, f, _) -> (t, q, f))

            let nettingReservations =
                adjustedReservations
                |> List.map (fun (t, q, _) -> (t, q))

            let netReqs, _ =
                Netting.netDemands
                    skuId
                    nodeId
                    spId
                    onHand
                    nettingInbound
                    nettingReservations
                    safetyStock
                    adjustedDemands
                    state.Context.Policy.NettingPolicy

            let activeNetReqs =
                netReqs
                |> List.filter (fun nr -> Quantity.isPositive nr.NetRequirement)

            let state =
                { state with
                    NetRequirements = state.NetRequirements @ activeNetReqs }

            let rec processNetReqs (currentState: PlannerState) (remaining: NetRequirement list) =
                task {
                    match remaining with
                    | [] -> return Ok currentState

                    | netReq :: tail ->
                        let! proposalResult = createCapacityFeasibleProposal deps currentState netReq

                        match proposalResult with
                        | Error err ->
                            let updatedCtx =
                                match err with
                                | MrpStepError.SupplyGeneration [ SupplyGenerationError.NoSupplierFound sku ] ->
                                    MrpContext.addWarning
                                        $"No supplier found for SKU {sku} at {StockingPointId.value spId}"
                                        currentState.Context
                                | MrpStepError.SupplyGeneration [ SupplyGenerationError.NoRoutingFound sku ] ->
                                    MrpContext.addWarning
                                        $"No routing found for SKU {sku} at {StockingPointId.value spId}"
                                        currentState.Context
                                | MrpStepError.SupplyGeneration [ SupplyGenerationError.CapacityInfeasible(sku, reason) ] ->
                                    MrpContext.addWarning
                                        $"Capacity infeasible for SKU {sku}: {reason}"
                                        currentState.Context
                                | _ -> currentState.Context

                            return!
                                processNetReqs
                                    { currentState with
                                        Context = updatedCtx }
                                    tail

                        | Ok(proposal, updatedLoad) ->
                            let currentState =
                                { currentState with
                                    Proposals = proposal :: currentState.Proposals
                                    TentativeLoad = updatedLoad
                                    Context = updateContextWithProposal proposal currentState.Context }

                            let! childDemandsResult = createChildDemands deps currentState proposal netReq

                            match childDemandsResult with
                            | Error e -> return Error e

                            | Ok childDemands ->
                                match enqueueDemands childDemands currentState with
                                | Error e -> return Error e
                                | Ok nextState -> return! processNetReqs nextState tail
                }

            let! plannedStateResult = processNetReqs state activeNetReqs

            match plannedStateResult with
            | Error e -> return Error e

            | Ok plannedState ->
                let finalState =
                    { plannedState with
                        InProgress = Set.remove key plannedState.InProgress }

                return Ok finalState
    }

let private planRecursive
    (deps: MrpDependencies)
    (initialState: PlannerState)
    : Task<Result<PlannerState, MrpStepError>> =
    task {
        let rec loop (state: PlannerState) =
            task {
                match popNext state with
                | None -> return Ok state

                | Some(key, nextState) ->
                    let! result = planNode deps nextState key

                    match result with
                    | Error e -> return Error e
                    | Ok updatedState -> return! loop updatedState
            }

        return! loop initialState
    }

let createPipeline (deps: MrpDependencies) : MrpStepAsync<MrpDemand list, MrpRunResult> =
    let preprocessStep = PreprocessStep.execute
    let peggingStep = PeggingStep.createStep deps.PeggingCreator
    let postprocessStep = PostprocessStep.execute deps.ReservationCreator

    fun demands ctx ->
        task {
            let! step1 = preprocessStep demands ctx

            match step1 with
            | Error e -> return Error e

            | Ok(processedDemands, ctx1) ->
                let startTime = Timestamp.now

                let initialState = emptyState ctx1 processedDemands

                let! solverResult = planRecursive deps initialState

                match solverResult with
                | Error e -> return Error e

                | Ok finalState ->
                    let endTime = Timestamp.now

                    let duration =
                        Timestamp.value endTime
                        - Timestamp.value startTime

                    let proposals = List.rev finalState.Proposals
                    let netReqs = List.rev finalState.NetRequirements

                    let actionMessages =
                        proposals
                        |> List.choose (fun p ->
                            SupplyGenerationStep.checkFrozenHorizon finalState.Context.Policy.FrozenHorizon p startTime)

                    let updatedCtx =
                        finalState.Context
                        |> MrpContext.addEvent (PlanningCompleted(List.length proposals))
                        |> MrpContext.addEvent (CapacityCheckCompleted(List.length proposals))
                        |> MrpContext.addEvent (NettingCompleted(List.length netReqs))
                        |> MrpContext.updateTelemetry (fun t ->
                            { t with
                                NettingDuration = duration
                                ComponentsProcessed = t.ComponentsProcessed + List.length netReqs
                                ProposalsGenerated = t.ProposalsGenerated + List.length proposals })
                        |> (fun c ->
                            actionMessages
                            |> List.fold
                                (fun acc am ->
                                    acc
                                    |> MrpContext.addActionMessage am
                                    |> MrpContext.addEvent (ActionMessageGenerated am))
                                c)

                    let! step6 = peggingStep proposals updatedCtx

                    match step6 with
                    | Error e -> return Error e

                    | Ok(peggedProposals, ctx6) ->
                        let! step7 = postprocessStep peggedProposals ctx6

                        match step7 with
                        | Error e -> return Error e

                        | Ok(runResult, ctx7) ->
                            let finalResult =
                                { runResult with
                                    NetRequirements = netReqs }

                            return Ok(finalResult, ctx7)
        }

// ============================================================================
// EXECUTION LAYER
// ============================================================================

/// Execute the MRP pipeline with context initialization
let execute
    (pipeline: MrpStepAsync<MrpDemand list, MrpRunResult>)
    (runId: string)
    (startDate: Timestamp)
    (endDate: Timestamp)
    (stockingPointId: StockingPointId)
    (policy: MrpPolicy)
    (demands: MrpDemand list)
    (firmedPegs: PeggingLink list)
    : TaskResult<MrpRunResult, MrpApplicationError> =
    task {
        let runIdObj =
            MrpRunId.create runId
            |> Result.defaultWith (fun _ -> failwith "Invalid RunId")

        let ctx =
            { MrpContext.create runIdObj startDate endDate stockingPointId policy with
                Demands = demands
                FirmedPegs = firmedPegs }

        try
            let! result = pipeline demands ctx

            match result with
            | Ok(mrpResult, _) -> return Ok mrpResult
            | Error stepError -> return Error(MrpApplicationError.PipelineError stepError)
        with ex ->
            return Error(MrpApplicationError.UnexpectedError ex)
    }

/// Execute MRP pipeline with timeout safety
let executeWithTimeout
    (timeout: TimeSpan)
    (pipeline: MrpStepAsync<MrpDemand list, MrpRunResult>)
    (runId: string)
    (startDate: Timestamp)
    (endDate: Timestamp)
    (stockingPointId: StockingPointId)
    (policy: MrpPolicy)
    (demands: MrpDemand list)
    (firmedPegs: PeggingLink list)
    : Async<Result<MrpRunResult, MrpApplicationError>> =
    async {
        let runIdObj =
            MrpRunId.create runId
            |> Result.defaultWith (fun _ -> failwith "Invalid RunId")

        let ctx =
            { MrpContext.create runIdObj startDate endDate stockingPointId policy with
                Demands = demands
                FirmedPegs = firmedPegs }

        try
            let! child = Async.StartChild(Async.AwaitTask(pipeline demands ctx), int timeout.TotalMilliseconds)
            let! result = child

            match result with
            | Ok(mrpResult, _) -> return Ok mrpResult
            | Error stepError -> return Error(MrpApplicationError.PipelineError stepError)
        with
        | :? TimeoutException -> return Error(MrpApplicationError.Timeout timeout)
        | ex -> return Error(MrpApplicationError.UnexpectedError ex)
    }

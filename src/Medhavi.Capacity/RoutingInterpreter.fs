namespace Medhavi.Capacity

open System
open Medhavi.Contracts

type SelectionPolicy =
    | Fastest
    | Cheapest
    | Balanced

/// Result of routing capacity calculation with resource-specific breakdown
type RoutingCapacityResult =
    { TotalDuration: decimal
      TotalCost: decimal
      ResourceLoads: Map<LoadTarget, decimal> // Minutes per resource target
      ResourceCosts: Map<LoadTarget, decimal> } // Cost per resource target

module RoutingAcl =
    /// Translates the shared integration contract Routing DTO into the Capacity bounded context's specific RoutingLoadProfile.
    let translate (dto: MasterData.Routing) : RoutingLoadProfile =
        match dto.Details with
        | MasterData.RoutingDetails.Work work ->
            let translateStep (s: MasterData.RoutingStep) : RoutingStepLoadProfile =
                let loads =
                    s.ResourceRequirements
                    |> List.collect (fun req ->
                        let capacityLoadBasis =
                            match req.LoadBasis with
                            | MasterData.ResourceLoadBasis.PerOrder -> CapacityLoadBasis.PerOrder
                            | _ -> CapacityLoadBasis.PerUnit
                        req.Options
                        |> List.map (fun ro ->
                            let runMin = ro.TimingProfile.RunTimePerBaseQuantity |> Option.defaultValue 1.0M
                            let eff =
                                match ro.EfficiencyPolicy with
                                | MasterData.ResourceEfficiencyPolicy.StandardEfficiency -> 1.0M
                                | MasterData.ResourceEfficiencyPolicy.EfficiencyFactor f -> f
                            let target =
                                match ro.WorkCenterId with
                                | Some resId when not (String.IsNullOrWhiteSpace resId) ->
                                    LoadTarget.WorkCenter (resId, CapacityResourceKind.WorkCenter)
                                | _ ->
                                    LoadTarget.Resource (ro.ResourceGroupId, CapacityResourceKind.WorkCenter)
                            { Target = target
                              LoadBasis = capacityLoadBasis
                              UnitsRequired = req.RequiredUnits
                              SetupLoadMinutes = ro.TimingProfile.SetupTime
                              RunLoadPerBaseQuantityMinutes = runMin / eff
                              TeardownLoadMinutes = ro.TimingProfile.TeardownTime
                              CostPerMinute = ro.CostPerMinute }))

                let yieldVal =
                    match s.YieldPolicy with
                    | MasterData.StepYieldPolicy.NoYieldLoss -> None
                    | MasterData.StepYieldPolicy.ExpectedYield y -> Some y

                let reworkStep, reworkRate =
                    match s.ReworkPolicy with
                    | MasterData.ReworkPolicy.NoRework -> None, None
                    | MasterData.ReworkPolicy.ReworkToStep(stepId, rate) -> Some stepId, Some rate

                { RoutingStepId = s.StepId
                  OperationCode = s.OperationCode
                  SequenceNumber = s.Sequence
                  Loads = loads
                  Yield = yieldVal
                  ReworkStepId = reworkStep
                  ReworkRate = reworkRate }

            { RoutingId = dto.Id
              ProductId = work.ProductId
              PreferencePriority = dto.Preference.Priority
              BaseQuantity = work.BaseOutputQuantity
              StepLoads = work.Steps |> List.map translateStep }

        | MasterData.RoutingDetails.Transport trans ->
            { RoutingId = dto.Id
              ProductId = trans.SkuId
              PreferencePriority = dto.Preference.Priority
              BaseQuantity = 1.0M
              StepLoads = [] }

        | MasterData.RoutingDetails.Purchase pur ->
            { RoutingId = dto.Id
              ProductId = pur.SkuId
              PreferencePriority = dto.Preference.Priority
              BaseQuantity = 1.0M
              StepLoads = [] }

module RoutingInterpreter =

    /// Calculates the required quantity to be processed at each step to produce targetQty at the end of the routing,
    /// accounting for yield loss and loop-back rework steps using fixed-point iteration.
    let calculateStepFlows (routing: RoutingLoadProfile) (targetQty: decimal) : Map<string, decimal> =
        let steps =
            routing.StepLoads
            |> List.sortBy (fun s -> s.SequenceNumber)

        let n = steps.Length

        if n = 0 then
            Map.empty
        else
            let getStepYield (s: RoutingStepLoadProfile) = s.Yield |> Option.defaultValue 1.0M
            let getStepReworkRate (s: RoutingStepLoadProfile) = s.ReworkRate |> Option.defaultValue 0.0M

            // Initialize flows with a guess (1.0 for each step) to solve for 1.0 unit of external input
            let mutable currentFlows =
                steps
                |> List.map (fun s -> s.RoutingStepId, 1.0M)
                |> Map.ofList

            let maxIterations = 100
            let tolerance = 0.000001M
            let mutable converged = false
            let mutable iter = 0

            while not converged && iter < maxIterations do
                let nextFlows =
                    steps
                    |> List.mapi (fun idx s ->
                        let prevOut =
                            if idx = 0 then
                                1.0M
                            else
                                let prevStep = steps[idx - 1]
                                let prevFlow = Map.find prevStep.RoutingStepId currentFlows
                                prevFlow * getStepYield prevStep

                        let reworkIn =
                            steps
                            |> List.filter (fun downstream -> downstream.ReworkStepId = Some s.RoutingStepId)
                            |> List.sumBy (fun downstream ->
                                let downFlow = Map.find downstream.RoutingStepId currentFlows
                                let downYield = getStepYield downstream
                                let downReworkRate = getStepReworkRate downstream
                                downFlow * (1.0M - downYield) * downReworkRate)

                        s.RoutingStepId, prevOut + reworkIn)
                    |> Map.ofList

                let maxDiff =
                    steps
                    |> List.map (fun s ->
                        abs (
                            Map.find s.RoutingStepId nextFlows
                            - Map.find s.RoutingStepId currentFlows
                        ))
                    |> List.max

                if maxDiff < tolerance then
                    converged <- true

                currentFlows <- nextFlows
                iter <- iter + 1

                // Log iteration progress for debugging (remove in production or make configurable)
                if
                    iter % 20 = 0
                    && System.Diagnostics.Debugger.IsAttached
                then
                    System.Diagnostics.Debug.WriteLine(
                        $"[RoutingInterpreter] Iter {iter}: maxDiff={maxDiff}, converged={converged}"
                    )

            let finalStep = steps[n - 1]
            let finalFlow = Map.find finalStep.RoutingStepId currentFlows
            let finalYield = getStepYield finalStep
            let unitOutput = finalFlow * finalYield

            if unitOutput <= 0.0M then
                steps
                |> List.map (fun s -> s.RoutingStepId, 0.0M)
                |> Map.ofList
            else
                let scaleFactor = targetQty / unitOutput

                currentFlows
                |> Map.map (fun _ flow -> flow * scaleFactor)

    /// Computes the aggregated duration and cost for a given routing and quantity.
    /// The step duration is modeled as the maximum duration of its concurrent resource loads (critical path),
    /// while the step cost is the sum of costs of all concurrent resource loads.
    /// Returns detailed breakdown including per-resource loads and costs.
    let calculateRoutingMetrics (routing: RoutingLoadProfile) (qty: decimal) : RoutingCapacityResult =
        let stepFlows = calculateStepFlows routing qty

        let baseQty =
            if routing.BaseQuantity <= 0.0M then
                1.0M
            else
                routing.BaseQuantity

        let stepMetrics =
            routing.StepLoads
            |> List.map (fun s ->
                let stepQty =
                    Map.tryFind s.RoutingStepId stepFlows
                    |> Option.defaultValue qty

                if List.isEmpty s.Loads then
                    {| Duration = 0.0M
                       Cost = 0.0M
                       LoadMetrics = [] |}
                else
                    let loadMetrics =
                        s.Loads
                        |> List.map (fun load ->
                            let setup = load.SetupLoadMinutes |> Option.defaultValue 0.0M

                            let teardown =
                                load.TeardownLoadMinutes
                                |> Option.defaultValue 0.0M

                            let scaleFactor =
                                match load.LoadBasis with
                                | PerOrder -> 1.0M
                                | _ -> stepQty / baseQty

                            let runTime = load.RunLoadPerBaseQuantityMinutes * scaleFactor
                            let duration = setup + runTime + teardown
                            let costPerMin = load.CostPerMinute |> Option.defaultValue 0.0M
                            let cost = duration * costPerMin

                            {| Duration = duration
                               Cost = cost
                               Target = load.Target |})

                    // The step duration is the max of its concurrent loads' durations
                    let stepDuration =
                        loadMetrics
                        |> List.map (fun m -> m.Duration)
                        |> List.max
                    // The step cost is the sum of all concurrent loads' costs
                    let stepCost = loadMetrics |> List.sumBy (fun m -> m.Cost)

                    {| Duration = stepDuration
                       Cost = stepCost
                       LoadMetrics = loadMetrics |})

        // Calculate resource-specific loads and costs
        let resourceLoads =
            stepMetrics
            |> List.collect (fun step ->
                step.LoadMetrics
                |> List.map (fun load -> load.Target, load.Duration))
            |> List.groupBy fst
            |> List.map (fun (key, pairs) -> key, List.sumBy snd pairs)
            |> Map.ofList

        let resourceCosts =
            stepMetrics
            |> List.collect (fun step ->
                step.LoadMetrics
                |> List.map (fun load -> load.Target, load.Cost))
            |> List.groupBy fst
            |> List.map (fun (key, pairs) -> key, List.sumBy snd pairs)
            |> Map.ofList

        let totalDuration = stepMetrics |> List.sumBy (fun m -> m.Duration)
        let totalCost = stepMetrics |> List.sumBy (fun m -> m.Cost)

        { TotalDuration = totalDuration
          TotalCost = totalCost
          ResourceLoads = resourceLoads
          ResourceCosts = resourceCosts }

    /// Selects the best routing alternative for a given quantity based on the selection policy and routing priority.
    let selectRouting
        (policy: SelectionPolicy)
        (qty: decimal)
        (routings: RoutingLoadProfile list)
        : RoutingLoadProfile option =
        if List.isEmpty routings then
            None
        else
            let scored =
                routings
                |> List.map (fun r ->
                    let metrics = calculateRoutingMetrics r qty
                    r, metrics)

            match policy with
            | Fastest ->
                scored
                |> List.sortBy (fun (r, m) -> m.TotalDuration, r.PreferencePriority)
                |> List.tryHead
                |> Option.map fst
            | Cheapest ->
                scored
                |> List.sortBy (fun (r, m) -> m.TotalCost, r.PreferencePriority)
                |> List.tryHead
                |> Option.map fst
            | Balanced ->
                if scored.Length = 1 then
                    Some(fst scored[0])
                else
                    let minDuration =
                        scored
                        |> List.map (fun (_, m) -> m.TotalDuration)
                        |> List.min

                    let maxDuration =
                        scored
                        |> List.map (fun (_, m) -> m.TotalDuration)
                        |> List.max

                    let minCost =
                        scored
                        |> List.map (fun (_, m) -> m.TotalCost)
                        |> List.min

                    let maxCost =
                        scored
                        |> List.map (fun (_, m) -> m.TotalCost)
                        |> List.max

                    let normalize val' min' max' =
                        if max' = min' then
                            0.0M
                        else
                            (val' - min') / (max' - min')

                    // Configurable weights (could come from configuration/tenant settings)
                    let durationWeight = 0.6M
                    let costWeight = 0.4M

                    scored
                    |> List.sortBy (fun (r, m) ->
                        let normDuration = normalize m.TotalDuration minDuration maxDuration
                        let normCost = normalize m.TotalCost minCost maxCost

                        let score =
                            (normDuration * durationWeight)
                            + (normCost * costWeight)

                        score, r.PreferencePriority)
                    |> List.tryHead
                    |> Option.map fst

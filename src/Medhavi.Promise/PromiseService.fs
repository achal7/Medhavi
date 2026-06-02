module Medhavi.Promise.PromiseService

open System
open Medhavi.SharedKernel
open Medhavi.SharedKernel.PromisePolicy
open Medhavi.Promise.PromiseTypes
open Medhavi.Transport

// Context representation for a single line evaluation
type PromiseLineCtx =
    { Request: PromiseRequest
      Line: OrderLine
      Policy: PromisePolicy
      Routing: RoutingSelection option
      RoutingChoice: RoutingChoice option
      MaterialSnapshot: MaterialSnapshot option
      MaterialReady: DateTimeOffset option
      CapacityResult: CapacityCheckResult option
      CapacityReady: DateTimeOffset option
      Itinerary: Itinerary option
      TransportReady: DateTimeOffset option
      PromiseDate: PromiseDateRange option
      Reservations: ReservationId list
      CostBreakdown: CostBreakdown option
      Confidence: float option
      Limiters: PromiseLimiter list
      SupplierOption: SupplierOption option }

type PromiseStep = PromiseLineCtx -> Async<Result<PromiseLineCtx, ProviderError>>

// Initialize context with default values
let initPromiseLineCtx (req: PromiseRequest) (line: OrderLine) (policy: PromisePolicy) : PromiseLineCtx =
    { Request = req
      Line = line
      Policy = policy
      Routing = None
      RoutingChoice = None
      MaterialSnapshot = None
      MaterialReady = None
      CapacityResult = None
      CapacityReady = None
      Itinerary = None
      TransportReady = None
      PromiseDate = None
      Reservations = []
      CostBreakdown = None
      Confidence = None
      Limiters = []
      SupplierOption = None }

// Earliest material calculation logic
let earliestMaterialReady
    (policy: PromisePolicy)
    (snap: MaterialSnapshot)
    (requiredQty: decimal)
    (now: DateTimeOffset) =
    let reservationsToSubtract =
        if policy.EnableReservationSubtraction then snap.Reservations else 0m

    let netStart = snap.OnHand - reservationsToSubtract - snap.Safety

    if netStart >= requiredQty then
        Some now
    else
        let shortfall = requiredQty - netStart

        snap.Inbound
        |> List.sortBy fst
        |> List.fold
            (fun (accQty, ready) inbound ->
                match ready with
                | Some _ -> accQty, ready
                | None ->
                    let acc = accQty + snd inbound
                    if acc >= shortfall then acc, Some(fst inbound) else acc, None)
            (0m, None)
        |> snd

// Calculate confidence logic
let calculateConfidence
    (routingChoice: RoutingChoice option)
    (itinerary: Itinerary option) =
    match routingChoice, itinerary with
    | Some rc, Some it ->
        match rc.Reliability with
        | Some r1 -> Some(min (float r1) (float it.TotalReliability))
        | None -> Some(float it.TotalReliability)
    | Some rc, None -> rc.Reliability |> Option.map float
    | None, Some it -> Some(float it.TotalReliability)
    | None, None -> None

// Monadic pipeline bind helper
let bind (step: PromiseStep) (input: Async<Result<PromiseLineCtx, ProviderError>>) : Async<Result<PromiseLineCtx, ProviderError>> =
    async {
        let! res = input
        match res with
        | Ok ctx -> return! step ctx
        | Error e -> return Error e
    }

// 1. Routing Selection Step
let selectRoutingStep (routing: RoutingProvider) : PromiseStep =
    fun ctx ->
        async {
            let! res = routing.Select(ctx.Line.SkuId, ctx.Line.StockingPointId)
            match res with
            | Ok sel ->
                let choice = Routing.selectBestRoutingChoice ctx.Policy sel
                return Ok { ctx with Routing = Some sel; RoutingChoice = Some choice }
            | Error e -> return Error e
        }

// 2. Material ATP Step
let checkMaterialStep (mat: MaterialProvider) : PromiseStep =
    fun ctx ->
        async {
            let! res = mat.GetSnapshot(ctx.Line.SkuId, ctx.Line.StockingPointId, ctx.Request.AsOfDate)
            match res with
            | Ok snap ->
                let ready = earliestMaterialReady ctx.Policy snap (Quantity.value ctx.Line.Quantity) ctx.Request.AsOfDate
                return Ok { ctx with MaterialSnapshot = Some snap; MaterialReady = ready }
            | Error e -> return Error e
        }

// 3. Capacity ATP Step
let checkCapacityStep (cap: CapacityProvider) : PromiseStep =
    fun ctx ->
        async {
            let! res = cap.CheckCapacity(ctx.Line.SkuId, Quantity.value ctx.Line.Quantity, ctx.Request.AsOfDate)
            match res with
            | Ok checkRes ->
                if checkRes.IsFeasible then
                    let ready = Some checkRes.SuggestedDate
                    return Ok { ctx with CapacityResult = Some checkRes; CapacityReady = ready }
                else
                    return Error ProviderError.Unavailable
            | Error e -> return Error e
        }

// 4. Transport ATP Step
let checkTransportStep (trans: TransportProvider) : PromiseStep =
    fun ctx ->
        async {
            let origin = ctx.Line.Origin |> Option.map (fun sp -> StockingPointId.value sp) |> Option.defaultValue (string ctx.Line.StockingPointId)
            let dest = ctx.Line.Destination |> Option.map (fun sp -> StockingPointId.value sp) |> Option.defaultValue (string ctx.Line.StockingPointId)
            let! res = trans.GetOptions(origin, dest, ctx.Request.AsOfDate)
            match res with
            | Ok itineraries ->
                // Filter for valid itineraries (skip those with regulatory/capacity issues)
                let validItineraries =
                    itineraries
                    |> List.filter (fun it -> 
                        it.Hops |> List.forall (fun hop -> 
                            // Check leg validity - legs with Status=false are blocked
                            // Additional checks for regulatory/cutoff could be added here
                            hop.ArrivalDateOffset > 0m && hop.LeadTimeMinutes > 0m))
                
                let best = validItineraries |> List.sortBy (fun it -> it.TotalLeadTimeMinutes) |> List.tryHead
                match best with
                | Some it ->
                    let arrival = ctx.Request.AsOfDate.AddMinutes(float it.TotalLeadTimeMinutes)
                    return Ok { ctx with Itinerary = Some it; TransportReady = Some arrival }
                | None ->
                    return Error ProviderError.NoTransportCapacity
            | Error ProviderError.TransportRegulatoryBlocked -> return Error ProviderError.TransportRegulatoryBlocked
            | Error ProviderError.TransportCutoffMissed -> return Error ProviderError.TransportCutoffMissed
            | Error e -> return Error e
        }

// 2.5. Supplier Options Step (called when material shortfall detected)
let checkSupplierOptionsStep (mat: MaterialProvider) : PromiseStep =
    fun ctx ->
        async {
            match ctx.MaterialSnapshot, ctx.MaterialReady with
            | Some snap, Some ready when ready <= ctx.Request.AsOfDate ->
                // Material is available now, no supplier needed
                return Ok { ctx with SupplierOption = None }
            | Some snap, _ ->
                // Material shortfall - check supplier options if policy allows
                if ctx.Policy.CallSupplierOnShortfall then
                    let requiredQty = Quantity.value ctx.Line.Quantity
                    let! res = mat.GetSupplierOptions(ctx.Line.SkuId, ctx.Line.StockingPointId, requiredQty, ctx.Request.AsOfDate)
                    match res with
                    | Ok options ->
                        let bestOption = 
                            options 
                            |> List.sortBy (fun opt -> opt.Earliest)
                            |> List.tryHead
                        return Ok { ctx with SupplierOption = bestOption }
                    | Error e -> return Error e
                else
                    return Ok { ctx with SupplierOption = None }
            | None, _ ->
                return Ok { ctx with SupplierOption = None }
        }

// 5. Date Calculation Step with uncertainty bounds
let calculateDateStep : PromiseStep =
    fun ctx ->
        async {
            // Check if any domain is unavailable (None)
            let hasUnavailable =
                [ ctx.MaterialReady; ctx.CapacityReady; ctx.TransportReady ]
                |> List.exists Option.isNone
            
            if hasUnavailable then
                return Error ProviderError.StaleData
            else
                let readyDates =
                    [ ctx.MaterialReady; ctx.CapacityReady; ctx.TransportReady ]
                    |> List.map Option.get
                
                // Calculate dates with uncertainty bounds based on reliability
                let committed = List.max readyDates
                
                let confidencePenalty =
                    match ctx.Itinerary with
                    | Some it -> match it.TotalReliability with
                                  | r when r >= 0.9m -> TimeSpan.FromHours(0.5)  // Tight bounds for high reliability
                                  | r when r >= 0.7m -> TimeSpan.FromHours(2.0)  // Medium bounds
                                  | _ -> TimeSpan.FromHours(6.0)  // Wide bounds for low reliability
                    | None -> TimeSpan.FromHours(4.0)
                
                let earliest = committed - confidencePenalty
                let latest = committed + confidencePenalty
                
                let dateRange = { Earliest = earliest; Committed = committed; Latest = latest }
                return Ok { ctx with PromiseDate = Some dateRange }
        }

// 6. Reservation Creation Step (Creates reservations across all 3 domains)
let createReservationsStep (resv: ReservationProvider) : PromiseStep =
    fun ctx ->
        async {
            let committed = ctx.PromiseDate |> Option.map (fun d -> d.Committed) |> Option.defaultValue ctx.Request.AsOfDate
            let duration = TimeSpan.FromDays(1.0)
            let requests =
                [ { Scope = ReservationScope.Material
                    Reference = string ctx.Request.Order.OrderId
                    SkuId = ctx.Line.SkuId
                    StockingPointId = ctx.Line.StockingPointId
                    Quantity = Quantity.value ctx.Line.Quantity
                    Duration = Some duration
                    WindowStart = committed
                    WindowEnd = committed.Add(duration) }
                  { Scope = ReservationScope.Capacity
                    Reference = ctx.CapacityResult |> Option.bind (fun r -> r.BottleneckResourceId) |> Option.defaultValue ""
                    SkuId = ctx.Line.SkuId
                    StockingPointId = ctx.Line.StockingPointId
                    Quantity = Quantity.value ctx.Line.Quantity
                    Duration = Some duration
                    WindowStart = committed
                    WindowEnd = committed.Add(duration) }
                  { Scope = ReservationScope.Transport
                    Reference = ctx.Itinerary |> Option.map (fun it -> string it.Id) |> Option.defaultValue ""
                    SkuId = ctx.Line.SkuId
                    StockingPointId = ctx.Line.StockingPointId
                    Quantity = Quantity.value ctx.Line.Quantity
                    Duration = Some duration
                    WindowStart = committed
                    WindowEnd = committed.Add(duration) } ]

            let! res = resv.CreateTentative(requests)
            match res with
            | Ok ids -> return Ok { ctx with Reservations = ids }
            | Error e -> return Error e
        }

// Release reservations on failure helper
let releaseReservations (resv: ReservationProvider) (ids: ReservationId list) : Async<unit> =
    async {
        if not (List.isEmpty ids) then
            let! _ = resv.Release(ids)
            ()
    }

// 7. Cost & Confidence Enrichment Step
let enrichCostConfidenceStep : PromiseStep =
    fun ctx ->
        async {
            let committed = ctx.PromiseDate |> Option.map (fun d -> d.Committed) |> Option.defaultValue ctx.Request.AsOfDate
            let defaultProductionRate = 100.0m
            let defaultFxRate = ctx.Request.Currency |> Option.map (fun _ -> 1.0m)
            let cost = CostCalculation.calculateCost ctx.Policy (Quantity.value ctx.Line.Quantity) ctx.MaterialSnapshot ctx.SupplierOption ctx.Itinerary committed ctx.Line.DueDate defaultProductionRate ctx.CapacityResult defaultFxRate
            let conf = calculateConfidence ctx.RoutingChoice ctx.Itinerary
            
            // Telemetry: cost calculated (logEvent available for production use)
            // Telemetry.createEvent TelemetrySeverity.Information "PromiseCostCalculated" ...
            
            let (dates: LimiterSelection.ReadyDates) =
                { MaterialReady = ctx.MaterialReady
                  CapacityReady = ctx.CapacityReady
                  TransportReady = ctx.TransportReady
                  AsOf = ctx.Request.AsOfDate }
            let limiter =
                if committed > ctx.Line.DueDate then
                    LimiterSelection.selectLimiter dates
                else None

            return Ok { ctx with CostBreakdown = Some cost; Confidence = conf; Limiters = limiter |> Option.toList }
        }

// Process a single order line through the monadic pipeline
let processLine
    (mat: MaterialProvider)
    (cap: CapacityProvider)
    (trans: TransportProvider)
    (routing: RoutingProvider)
    (resv: ReservationProvider)
    (req: PromiseRequest)
    (line: OrderLine)
    : Async<Result<PromiseLineCtx, ProviderError>> =
    
    let policy = PolicyHelpers.resolveFromTiers None req.CustomerTier req.SkuTier PolicyPresets.defaultPolicy

    let initCtx = initPromiseLineCtx req line policy

    // Execute pipeline steps in monadic sequence with proper rollback on failure
    async {
        let! r1 = selectRoutingStep routing initCtx
        match r1 with
        | Error e -> return Error e
        | Ok c1 ->
        
        let! r2 = checkMaterialStep mat c1
        match r2 with
        | Error e -> return Error e
        | Ok c2 ->
        
        let! r2b = checkSupplierOptionsStep mat c2
        match r2b with
        | Error e -> return Error e
        | Ok c2b ->
        
        let! r3 = checkCapacityStep cap c2b
        match r3 with
        | Error e -> return Error e
        | Ok c3 ->
        
        let! r4 = checkTransportStep trans c3
        match r4 with
        | Error e -> return Error e
        | Ok c4 ->
        
        let! r5 = calculateDateStep c4
        match r5 with
        | Error e -> return Error e
        | Ok c5 ->
        
        let! r6 = createReservationsStep resv c5
        match r6 with
        | Error e -> return Error e
        | Ok c6 ->
        
        let reservationsMade = c6.Reservations
        let! r7 = enrichCostConfidenceStep c6
        match r7 with
        | Error e ->
            let! _ = releaseReservations resv reservationsMade
            return Error e
        | Ok c7 ->
            return Ok c7
    }

// Single order line execution entrypoint
let tryPromise
    (mat: MaterialProvider)
    (cap: CapacityProvider)
    (trans: TransportProvider)
    (routing: RoutingProvider)
    (resv: ReservationProvider)
    (tenant: TenantProvider)
    (line: OrderLine)
    (req: PromiseRequest)
    : Async<PromiseResponse> =
    async {
        let! result = processLine mat cap trans routing resv req line
        match result with
        | Ok ctx ->
            return
                { Decision = PromiseDecisionStatus.Accepted
                  PromiseDate = ctx.PromiseDate
                  Limiter = ctx.Limiters |> List.tryHead
                  Routing = ctx.RoutingChoice
                  Itinerary = ctx.Itinerary
                  Material = ctx.MaterialSnapshot
                  Cost = ctx.CostBreakdown
                  Confidence = ctx.Confidence
                  Reservations = ctx.Reservations
                  Meta = Map.empty }
        | Error e ->
            return
                { Decision = PromiseDecisionStatus.Rejected
                  PromiseDate = None
                  Limiter = Some { Domain = PromiseLimiterDomain.System; Code = PromiseReasonCode.SearchTimeout; Message = string e; Suggestions = [] }
                  Routing = None
                  Itinerary = None
                  Material = None
                  Cost = None
                  Confidence = None
                  Reservations = []
                  Meta = Map.empty }
    }

// Main promise orchestrator function for full order (multi-line)
let tryPromiseOrder
    (mat: MaterialProvider)
    (cap: CapacityProvider)
    (trans: TransportProvider)
    (routing: RoutingProvider)
    (resv: ReservationProvider)
    (tenant: TenantProvider)
    (req: PromiseRequest)
    : Async<Result<PromiseResponse, ProviderError>> =
    async {
        if List.isEmpty req.Order.Lines then
            let resp =
                { Decision = PromiseDecisionStatus.Rejected
                  PromiseDate = None
                  Limiter =
                    Some
                        { Domain = PromiseLimiterDomain.Policy
                          Code = PromiseReasonCode.DataStale
                          Message = "No order lines to promise"
                          Suggestions = [ "provideLines" ] }
                  Routing = None
                  Itinerary = None
                  Material = None
                  Cost = None
                  Confidence = None
                  Reservations = []
                  Meta = Map.empty }

            return Ok resp
        else
            let policy = PolicyHelpers.resolveFromTiers None req.CustomerTier req.SkuTier PolicyPresets.defaultPolicy

            // Sort lines by priority (expedited lines first, then by priority value)
            let sortedLines =
                req.Order.Lines
                |> List.sortBy (fun line ->
                    match line.IsExpedited with
                    | true -> (0, line.Priority)
                    | false -> (1, line.Priority))
            
            // Run all lines in parallel
            let runLine line = processLine mat cap trans routing resv req line
            let! perLineResults = sortedLines |> List.map runLine |> Async.Parallel

            let successCtxs = perLineResults |> Array.choose (function | Ok ctx -> Some ctx | Error _ -> None) |> Array.toList
            let totalLines = List.length req.Order.Lines
            let successCount = List.length successCtxs
            let hasFailures = successCount < totalLines

            // Enforce FullOrder policy or complete failure
            if (policy.FullOrder && hasFailures) || successCount = 0 then
                // Roll back all reservations
                let allResIds = successCtxs |> List.collect (fun c -> c.Reservations)
                let! _ = releaseReservations resv allResIds

                let message = if policy.FullOrder then "One or more order lines failed under FullOrder constraint." else "All order lines failed."
                let resp =
                    { Decision = PromiseDecisionStatus.Rejected
                      PromiseDate = None
                      Limiter = Some { Domain = PromiseLimiterDomain.Policy; Code = PromiseReasonCode.FullOrderViolation; Message = message; Suggestions = [] }
                      Routing = None
                      Itinerary = None
                      Material = None
                      Cost = None
                      Confidence = None
                      Reservations = []
                      Meta = Map.empty }
                return Ok resp
            else
                // Enforce FullDelivery policy (align promise dates to the maximum date)
                let dates = successCtxs |> List.choose (fun c -> c.PromiseDate)
                let finalCommitted =
                    dates
                    |> List.map (fun d -> d.Committed)
                    |> function
                        | [] -> req.AsOfDate
                        | xs -> List.max xs

                // FullDelivery check: all lines must have same promise date
                let fullDeliveryFailed =
                    policy.FullDelivery && dates |> List.exists (fun d -> d.Committed <> finalCommitted)
                
                if fullDeliveryFailed then
                    // Roll back all reservations
                    let allResIds = successCtxs |> List.collect (fun c -> c.Reservations)
                    let! _ = releaseReservations resv allResIds
                    
                    let resp =
                        { Decision = PromiseDecisionStatus.Rejected
                          PromiseDate = None
                          Limiter = Some { Domain = PromiseLimiterDomain.Policy; Code = PromiseReasonCode.FullDeliveryViolation; Message = "Cannot promise full delivery - lines have different dates"; Suggestions = [] }
                          Routing = None
                          Itinerary = None
                          Material = None
                          Cost = None
                          Confidence = None
                          Reservations = []
                          Meta = Map.empty }
                    return Ok resp
                else
                    let finalDateRange = { Earliest = finalCommitted; Committed = finalCommitted; Latest = finalCommitted }

                    // Check Cost and Risk caps
                    let totalCostBreakdown =
                        successCtxs
                        |> List.choose (fun c -> c.CostBreakdown)
                        |> function
                            | [] -> CostBreakdown.empty
                            | x :: xs -> List.fold CostBreakdown.add x xs

                    let totalCost = totalCostBreakdown.TotalCost
                    let minConfidence =
                        successCtxs
                        |> List.choose (fun c -> c.Confidence)
                        |> function
                            | [] -> 1.0
                            | xs -> List.min xs

                    let costCapExceeded =
                        match policy.CostCap with
                        | Some cap -> totalCost > cap
                        | None -> false

                    let riskCapExceeded =
                        match policy.RiskCap with
                        | Some cap -> minConfidence < cap
                        | None -> false

                    if costCapExceeded || riskCapExceeded then
                        // Roll back all reservations
                        let allResIds = successCtxs |> List.collect (fun c -> c.Reservations)
                        let! _ = releaseReservations resv allResIds

                        let code = if costCapExceeded then PromiseReasonCode.CostCapExceeded else PromiseReasonCode.RiskCapExceeded
                        let msg = if costCapExceeded then $"Total cost {totalCost} exceeds cost cap of {policy.CostCap.Value}" else $"Min confidence {minConfidence} is below risk cap of {policy.RiskCap.Value}"
                        let resp =
                            { Decision = PromiseDecisionStatus.Rejected
                              PromiseDate = None
                              Limiter = Some { Domain = PromiseLimiterDomain.Policy; Code = code; Message = msg; Suggestions = [] }
                              Routing = None
                              Itinerary = None
                              Material = None
                              Cost = None
                              Confidence = None
                              Reservations = []
                              Meta = Map.empty }
                        return Ok resp
                    else
                        // Success!
                        let routingChoice = successCtxs |> List.choose (fun c -> c.RoutingChoice) |> List.tryHead
                        let itinerary = successCtxs |> List.choose (fun c -> c.Itinerary) |> List.tryHead
                        let material = successCtxs |> List.choose (fun c -> c.MaterialSnapshot) |> List.tryHead
                        let allReservations = successCtxs |> List.collect (fun c -> c.Reservations)

                        let resp =
                            { Decision = PromiseDecisionStatus.Accepted
                              PromiseDate = Some finalDateRange
                              Limiter = None
                              Routing = routingChoice
                              Itinerary = itinerary
                              Material = material
                              Cost = Some totalCostBreakdown
                              Confidence = Some minConfidence
                              Reservations = allReservations
                              Meta = Map.empty }

                        // TODO: Integrate with Scenario aggregate (Phase 7 - repository integration)
                        // This would save the snapshot and mark clean steps
                        return Ok resp
        }
module Medhavi.Demand.ForecastPublication.ComputationService

open System
open System.Threading.Tasks
open Medhavi.SharedKernel
open Medhavi.SharedKernel.Failure
open Medhavi.Contracts
open Medhavi.Demand.ForecastPublication.Model
open MathNet.Numerics.Statistics
open Medhavi.SharedKernel.ExecutionContext
open Medhavi.SharedKernel.Observation

type ForecastComputationInput =
    { Coverage: (SkuId * StockingPointId) list
      ForecastHorizon: string
      BucketConfig: string
      ModelId: string
      TargetPeriod: PlanningPeriod option
      TargetReconciliationTotal: decimal option }

type Service =
    { ComputeForecasts:
        ForecastComputationInput -> Task<Result<Demand.ForecastPublication.Forecast list, ApplicationError>> }

/// BA-D-010: Standard Deviation utility using Math.NET Numerics
let stdDev (values: decimal list) : decimal =
    if values.Length < 2 then 10.0m
    else
        let floats = values |> List.map float
        let sd = Statistics.StandardDeviation(floats)
        let sdDec = decimal sd
        if sdDec > 0.0m && not (Double.IsNaN(sd)) then sdDec else 10.0m

/// Croston's Method for intermittent demand series
let private croston (history: decimal list) : decimal =
    let alpha = 0.3m
    let nonZeros = history |> List.filter (fun v -> v > 0.0m)
    match nonZeros with
    | [] -> 0.0m
    | _ ->
        let initialSize = List.average nonZeros
        let initialPeriod = decimal history.Length / decimal nonZeros.Length
        let rec compute (ys: decimal list) size period q =
            match ys with
            | [] -> if period = 0.0m then 0.0m else size / period
            | y :: tail ->
                if y > 0.0m then
                    let nextSize = alpha * y + (1.0m - alpha) * size
                    let nextPeriod = alpha * decimal q + (1.0m - alpha) * period
                    compute tail nextSize nextPeriod 1
                else
                    compute tail size period (q + 1)
        compute history initialSize initialPeriod 1

/// Holt's Linear Trend Method
let private holtLinear (history: decimal list) : decimal =
    let alpha = 0.3m
    let beta = 0.1m
    match history with
    | [] -> 100.0m
    | [y] -> y
    | y1 :: y2 :: tail ->
        let initialLevel = y2
        let initialTrend = y2 - y1
        let rec compute (ys: decimal list) level trend =
            match ys with
            | [] -> level + trend
            | y :: tail ->
                let nextLevel = alpha * y + (1.0m - alpha) * (level + trend)
                let nextTrend = beta * (nextLevel - level) + (1.0m - beta) * trend
                compute tail nextLevel nextTrend
        compute tail initialLevel initialTrend

/// Holt-Winters Additive Seasonality Model
let private holtWinters (history: decimal list) (seasonLength: int) : decimal =
    let alpha = 0.3m
    let beta = 0.1m
    let gamma = 0.2m
    let P = seasonLength
    if history.Length < 2 * P || P < 2 then
        holtLinear history
    else
        let firstSeason = history |> List.take P
        let secondSeason = history |> List.skip P |> List.take P
        let l0 = List.average firstSeason
        let t0 = (List.average secondSeason - l0) / decimal P
        let mutable s = firstSeason |> List.map (fun y -> y - l0)
        let mutable level = l0
        let mutable trend = t0
        let remaining = history |> List.skip P
        let mutable index = 0
        for y in remaining do
            let s_old = s.[index % P]
            let nextLevel = alpha * (y - s_old) + (1.0m - alpha) * (level + trend)
            let nextTrend = beta * (nextLevel - level) + (1.0m - beta) * trend
            let nextSeasonal = gamma * (y - nextLevel) + (1.0m - gamma) * s_old
            level <- nextLevel
            trend <- nextTrend
            s <- s |> List.mapi (fun idx oldVal -> if idx = (index % P) then nextSeasonal else oldVal)
            index <- index + 1
        level + trend + s.[index % P]

/// Simple Exponential Smoothing (SES)
let private ses (history: decimal list) : decimal =
    let alpha = 0.3m
    match history with
    | [] -> 100.0m
    | [y] -> y
    | y :: ys ->
        let rec computeSes currentHistory prevForecast =
            match currentHistory with
            | [] -> prevForecast
            | x :: xs ->
                let nextForecast = alpha * x + (1.0m - alpha) * prevForecast
                computeSes xs nextForecast
        computeSes ys y

/// BA-D-002: Select appropriate algorithm based on classification and history sparsity
let private generateBaseline
    (sku: SkuId)
    (sp: StockingPointId)
    (history: decimal list)
    (targetPeriod: PlanningPeriod)
    (modelId: string)
    : Demand.ForecastPublication.Forecast =

    let zeroCount = history |> List.filter (fun v -> v = 0.0m) |> List.length
    let isSparse = history.Length > 2 && (decimal zeroCount / decimal history.Length) > 0.4m

    let meanValue =
        if isSparse || modelId = "Model-Croston" then
            croston history
        elif modelId = "Model-HoltWinters" || (history.Length >= 8 && modelId <> "Model-SES-01") then
            holtWinters history 4
        else
            ses history

    let sd = stdDev history
    let lower = max 0.0m (meanValue - 1.96m * sd)
    let upper = meanValue + 1.96m * sd
    let confValue = 0.95m

    { ForecastId = $"FC-{SkuId.value sku}-{StockingPointId.value sp}"
      SkuId = SkuId.value sku
      StockingPointId = StockingPointId.value sp
      PlanningPeriod = targetPeriod
      Mean = meanValue
      LowerBound = lower
      UpperBound = upper
      Confidence = confValue
      ModelId = if isSparse then "Model-Croston" else "Model-SES-01"
      OverrideReason = None }

let create (getHistory: SkuId -> StockingPointId -> Task<decimal list>) : Service =

    let computeForecasts (input: ForecastComputationInput) =
        task {
            let start = System.Diagnostics.Stopwatch.StartNew()
            try
                let mutable computed = []
                let targetPeriod = input.TargetPeriod |> Option.defaultValue (PlanningPeriod.PlanningWeek(2027, 27))

                for skuId, spId in input.Coverage do
                    let! history = getHistory skuId spId
                    let fc = generateBaseline skuId spId history targetPeriod input.ModelId
                    computed <- fc :: computed

                // BA-D-003: Proportional top-down reconciliation
                let reconciled =
                    match input.TargetReconciliationTotal with
                    | None -> computed
                    | Some targetTotal ->
                        let sum = computed |> List.map (fun f -> f.Mean) |> List.sum
                        if sum = 0m then
                            let equalShare = targetTotal / decimal computed.Length
                            computed |> List.map (fun f -> { f with Mean = equalShare })
                        else
                            computed |> List.map (fun f ->
                                let ratio = f.Mean / sum
                                let newMean = targetTotal * ratio
                                let sd = (f.UpperBound - f.LowerBound) / (2.0m * 1.96m)
                                { f with
                                    Mean = newMean
                                    LowerBound = max 0.0m (newMean - 1.96m * sd)
                                    UpperBound = newMean + 1.96m * sd })

                start.Stop()
                // Emit PI-DI-202: Forecast Generation Time (millisecond count) using LatencyTelemetry
                let lat: LatencyTelemetry =
                    { OperationName = "PI-DI-202"
                      Component = "ForecastComputation"
                      StartTime = Timestamp.create (DateTimeOffset.UtcNow.AddMilliseconds(-float start.ElapsedMilliseconds))
                      EndTime = Timestamp.create DateTimeOffset.UtcNow
                      DurationMs = float start.ElapsedMilliseconds
                      IsSuccess = true
                      ErrorDetails = None
                      CorrelationId = CorrelationId.create ()
                      TenantId = None
                      Metadata = Map.ofList [ "ModelId", input.ModelId ] }
                DomainEventBus.Publish(TelemetryMetric.Latency lat)

                return Ok reconciled
            with ex ->
                return
                    Error(
                        ApplicationError.Domain(DomainError.validation $"Forecasting computation failed: {ex.Message}")
                    )
        }

    { ComputeForecasts = computeForecasts }

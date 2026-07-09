module Medhavi.Demand.ForecastQualityAssessment.Capabilities

open System.Threading.Tasks
open Medhavi.SharedKernel
open Medhavi.SharedKernel.Failure
open Medhavi.Contracts.Demand.ForecastQualityAssessment
open Medhavi.Demand
open Medhavi.Demand.ForecastQualityAssessment.Model
open Medhavi.Demand.ForecastQualityAssessment.ACL
open Medhavi.Demand.ForecastQualityAlgorithms
open Medhavi.SharedKernel.Execution
open Medhavi.Common.Validation

open Medhavi.SharedKernel.Observation
open System

let private publishTelemetryMetric (name: string) (value: decimal) (assessmentId: string) (scopeId: string) =
    let event: TelemetryEvent =
        { EventId = Guid.NewGuid()
          Timestamp = DateTimeOffset.UtcNow
          Severity = TelemetrySeverity.Information
          Message = name
          Properties = Map.ofList [ "Value", box value; "AssessmentId", box assessmentId; "PlanningScopeId", box scopeId ]
          CorrelationId = None
          CausationId = None
          TraceId = None
          SpanId = None }
    DomainEventBus.Publish event

let private publishSuccess (ass: ForecastQualityAssessment) =
    if ass.Status = Published then
        let notification: ForecastQualityAssessmentPublishedNotification =
            { AssessmentId = ForecastQualityAssessmentId.value ass.Id
              PlanningScopeId = PlanningScopeId.value ass.PlanningScopeId
              EvaluationPeriodStart = Timestamp.value ass.EvaluationPeriodStart
              EvaluationPeriodEnd = Timestamp.value ass.EvaluationPeriodEnd
              Version = ass.Version
              KeyMetricsSummary =
                $"WAPE={ass.CoreMetrics.WAPE:P2}, MAPE={ass.CoreMetrics.MAPE:P2}, Bias={ass.CoreMetrics.ForecastBias:F2}, Acc={ass.CoreMetrics.ForecastAccuracy:P2}"
              OverallQualityScore = ass.OverallQualityScore |> Option.map PositiveDecimal.value }

        DomainEventBus.Publish notification

        let scopeIdStr = PlanningScopeId.value ass.PlanningScopeId
        let assIdStr = ForecastQualityAssessmentId.value ass.Id
        publishTelemetryMetric "PI-DI-002" ass.CoreMetrics.ForecastAccuracy assIdStr scopeIdStr
        publishTelemetryMetric "PI-DI-003" ass.CoreMetrics.WAPE assIdStr scopeIdStr
        publishTelemetryMetric "PI-DI-004" ass.CoreMetrics.MAPE assIdStr scopeIdStr
        publishTelemetryMetric "PI-DI-005" ass.CoreMetrics.ForecastBias assIdStr scopeIdStr
        
        ass.OptionalMetrics.FVA 
        |> Option.iter (fun fva -> publishTelemetryMetric "PI-DI-006" fva assIdStr scopeIdStr)
        
        ass.OptionalMetrics.ForecastStability 
        |> Option.iter (fun stab -> publishTelemetryMetric "PI-DI-007" stab assIdStr scopeIdStr)

// Traceability: Implements CA-D-007 (Evaluate Demand Quality) Capabilities API for SE-D-039 (Forecast Quality Assessment)
// Exposes the workflow layer: validates raw requests, fetches historical/naive forecasts, calls CommandHandler, publishes BN-D-025 notifications.

let createCapabilities
    (execute: ForecastQualityAssessmentCommand -> Task<ExecutionOutcome<ForecastQualityAssessment, ApplicationError>>)
    (getActuals: PlanningScopeId -> Timestamp -> Timestamp -> Task<decimal list>)
    (getForecasts: PlanningScopeId -> Timestamp -> Timestamp -> Task<decimal list>)
    (getNaiveForecasts: PlanningScopeId -> Timestamp -> Timestamp -> Task<decimal list option>)
    (getOverrideHistory: PlanningScopeId -> Timestamp -> Timestamp -> Task<(decimal * decimal) list>)
    (getHistoricalForecasts: PlanningScopeId -> int -> Task<decimal list list>)
    (policyThreshold: decimal)
    (policyMinPeriod: int)
    (policyWeights: CoreMetrics)
    : ForecastQualityApi =

    let evaluate (req: EvaluateForecastQualityReq) =
        task {
            // 1. EARLY VALIDATION: Validate raw request at the boundary
            match ACL.validateRequest req with
            | Invalid errors ->
                let apiErr =
                    ApplicationError.Domain(DomainError.combineValidationErrors errors)
                    |> ApplicationError.mapToApiError

                return Error apiErr
            | Valid validatedReq ->
                let scopeId = validatedReq.PlanningScopeId
                let start = validatedReq.EvaluationPeriodStart
                let end_ = validatedReq.EvaluationPeriodEnd

                // 2. FETCH DATA: Fetch using validated domain types
                let! actuals = getActuals scopeId start end_
                let! forecasts = getForecasts scopeId start end_
                let! naiveForecastsOpt = getNaiveForecasts scopeId start end_
                let! overrides = getOverrideHistory scopeId start end_
                let! histForecasts = getHistoricalForecasts scopeId 4

                // 3. PURE CALCULATIONS: Compute math metrics
                let coreMetrics =
                    { WAPE = wape actuals forecasts |> Option.defaultValue 0m
                      MAPE = mape actuals forecasts |> Option.defaultValue 0m
                      ForecastBias = forecastBias actuals forecasts |> Option.defaultValue 0m
                      ForecastAccuracy = forecastAccuracy actuals forecasts |> Option.defaultValue 0m }

                let optionalMetrics =
                    { FVA = naiveForecastsOpt |> Option.bind(fun n -> fva actuals forecasts n)
                      ForecastStability = forecastStability histForecasts
                      OverrideEffectiveness = overrideEffectiveness overrides actuals }

                let overallScore = computeOverallQualityScore coreMetrics policyWeights

                // Compute counts/metrics for the completeness rules
                let actualCount = actuals.Length
                let daysSpan = (Timestamp.value end_ - Timestamp.value start).TotalDays
                // Assume daily buckets for expected count by default
                let expectedCount = int(ceil daysSpan)

                let assessmentId = ForecastQualityAssessmentId.createFromScopeAndPeriod scopeId start end_

                // 4. DOMAIN COMMAND: Construct domain command directly
                let cmd =
                    { PlanningScopeId = scopeId
                      AssessmentId = assessmentId
                      EvaluationPeriodStart = start
                      EvaluationPeriodEnd = end_
                      CoreMetrics = coreMetrics
                      OptionalMetrics = optionalMetrics
                      OverallQualityScore = overallScore |> Option.map PositiveDecimal.createSafe

                      ActualDataCount = actualCount
                      ExpectedDataCount = expectedCount
                      CompletenessThreshold = policyThreshold
                      MinEvaluationPeriodDays = policyMinPeriod

                      SourceForecastPublicationRefs = req.SourceForecastPublicationRefs
                      SourceDemandHistoryRefs = req.SourceDemandHistoryRefs
                      ForecastMeasurementPolicyVersionRef = req.ForecastMeasurementPolicyVersionRef
                      PublicationTime = Timestamp.now }

                // 5. EXECUTE: Run via capabilities Execute corridor
                let! outcome = execute(Evaluate cmd)

                match Helpers.toApiResult outcome with
                | Ok ass ->
                    publishSuccess ass
                    let completeness = if expectedCount <= 0 then 1.0m else decimal actualCount / decimal expectedCount
                    publishTelemetryMetric "PI-DI-205" completeness (ForecastQualityAssessmentId.value ass.Id) (PlanningScopeId.value scopeId)
                    return Ok ass.AssignmentId
                | Error err -> return Error err
        }

    { Evaluate = evaluate }

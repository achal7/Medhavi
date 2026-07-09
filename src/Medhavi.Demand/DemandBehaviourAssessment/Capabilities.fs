module Medhavi.Demand.DemandBehaviourAssessment.Capabilities

open System
open System.Threading.Tasks
open Medhavi.Common.Validation
open Medhavi.Contracts.Demand.SenseDemand
open Medhavi.Contracts.Demand.ForecastPublication
open Medhavi.SharedKernel
open Medhavi.SharedKernel.Execution
open Medhavi.SharedKernel.Failure
open Medhavi.Demand.DemandBehaviourAssessment.ACL
open Medhavi.Demand.DemandBehaviourAssessment.Model

let private parseSkuId (s: string) =
    SkuId.create s |> Result.mapError(fun err -> ApplicationError.Domain err |> ApplicationError.mapToApiError)

let private handleStateChange
    (state: DemandBehaviourAssessment)
    (change: StateChangeEvent)
    (getScopeId: SkuId -> StockingPointId -> Task<string option>)
    (forecastQueries: ForecastPublicationQueries)
    (forecastApi: ForecastPublicationApi)
    =
    task {
        // 4. PUBLISH NOTIFICATION: Publish BN-D-015 Demand Behaviour Changed Notification
        DomainEventBus.Publish(
            { SkuId = SkuId.value state.SkuId
              StockingPointId = StockingPointId.value state.StockingPointId
              PreviousState = change.PreviousState.AsString()
              NewState = change.NewState.AsString()
              Deviation = PositiveDecimal.value change.DeviationMagnitude
              Direction = change.DeviationDirection.ToString()
              Confidence = PositiveDecimal.value change.ConfidenceScore
              Timestamp = Timestamp.value change.Timestamp }
            : DemandBehaviourChangedNotification
        )

        if change.NewState = Critical then
            // 5. CRITICAL STATE NOTIFICATION: Publish BN-D-016 Critical Demand Behaviour Requires Action
            DomainEventBus.Publish(
                { SkuId = SkuId.value state.SkuId
                  StockingPointId = StockingPointId.value state.StockingPointId
                  Deviation = PositiveDecimal.value change.DeviationMagnitude
                  RecommendedAction = "Forecast Refresh"
                  Timestamp = Timestamp.value change.Timestamp }
                : CriticalBehaviourNotification
            )

            // 6. FS-D-015: Trigger out-of-cycle forecast refresh (refresh policy check)
            let! scopeIdOpt = getScopeId state.SkuId state.StockingPointId

            match scopeIdOpt with
            | None -> ()
            | Some scopeId ->
                let! publishedPubs =
                    forecastQueries.Filter(fun p -> p.Status = "Published" && List.contains scopeId p.PlanningScopeIds)

                let latestPubOpt =
                    publishedPubs
                    |> List.sortByDescending(fun p -> p.PublicationTime |> Option.defaultValue DateTimeOffset.MinValue)
                    |> List.tryHead

                match latestPubOpt with
                | None -> ()
                | Some pub ->
                    let pubTime = pub.PublicationTime |> Option.defaultValue DateTimeOffset.UtcNow
                    let ageHours = (DateTimeOffset.UtcNow - pubTime).TotalHours

                    // Apply out-of-cycle refresh policies (refresh if age > 24 hours and threshold is met)
                    let refreshRes = Decisions.triggerForecastRefresh state ageHours 0.08M

                    match refreshRes with
                    | Ok true ->
                        let initiateReq =
                            { PublicationId = $"REFRESH-{Guid.NewGuid():N}".Substring(0, 16)
                              PlanningScopeIds = [ scopeId ]
                              ForecastHorizon = "7.00:00:00"
                              TimeBucketConfig = "Weekly" }

                        let! _ = forecastApi.InitiateCycle initiateReq
                        ()
                    | _ -> ()
    }

// Traceability: Implements CA-D-003 (Sense Demand) Capabilities API for SE-D-037 (Demand Behaviour Assessment aggregate)
// Exposes workflow layer: validates signals, evaluates priority, dispatches to CommandHandler, publishes BN-D-015/016 notifications.

let createCapabilities
    (execute: DemandBehaviourAssessmentCommand -> Task<ExecutionOutcome<DemandBehaviourAssessment, ApplicationError>>)
    (isHighPriority: SkuId -> Task<bool>)
    (forecastQueries: ForecastPublicationQueries)
    (forecastApi: ForecastPublicationApi)
    (getScopeId: SkuId -> StockingPointId -> Task<string option>)
    : SenseDemandApi =

    /// FS-D-003 — Evaluate Demand Signal (Ingests external signals and evaluates state changes)
    let evaluateSignal (req: EvaluateDemandSignalReq) =
        task {
            match parseSkuId req.SkuId with
            | Error err -> return Error err
            | Ok sku ->
                // 1. EVALUATE PRIORITY: Fetch high-priority status for SKU
                let! priority = isHighPriority sku

                // 2. EARLY VALIDATION: Validate signal inputs at aggregate boundary
                match toEvaluateSignalCmd req priority with
                | Invalid errors ->
                    let apiErr =
                        ApplicationError.Domain(DomainError.combineValidationErrors errors)
                        |> ApplicationError.mapToApiError

                    return Error apiErr
                | Valid cmd ->
                    // 3. EXECUTE COMMAND: Dispatch evaluation to CommandHandler execution corridor
                    let! outcome = execute(EvaluateSignal cmd)

                    match Helpers.toApiResult outcome with
                    | Ok state ->
                        match state.LastStateChange with
                        | Some change ->
                            let! _ = handleStateChange state change getScopeId forecastQueries forecastApi
                            ()
                        | None -> ()

                        return Ok state.AssignmentId
                    | Error err -> return Error err
        }

    let handleAckSuccess (_: DemandBehaviourAssessment) = task { return Ok() }

    /// Acknowledge Demand Behaviour Assessment (resets alert state)
    let acknowledge = Helpers.runWorkflow toAcknowledgeCmd (Acknowledge >> execute) handleAckSuccess

    { EvaluateSignal = evaluateSignal
      Acknowledge = acknowledge }

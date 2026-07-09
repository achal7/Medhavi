module Medhavi.Demand.Application.UnderstandDemandWorkflow

open System.Threading.Tasks
open Medhavi.SharedKernel.Failure
open Medhavi.Contracts.Demand.PlanningScope
open Medhavi.Contracts.Demand.DemandObservation
open Medhavi.Contracts.Demand.Edp
open Medhavi.Contracts.Demand.ForecastPublication
open Medhavi.Demand

type UnderstandDemandWorkflow =
    { DetermineScopeAndAssign: AssignScopeReq -> DeterminePlanningScopeReq -> Task<Result<unit, ApplicationError>>
      BuildAndPublishEdp: ReviseEnterpriseDemandPictureReq -> Task<Result<string, ApplicationError>>
      OnForecastPublished: ForecastPublishedNotification -> Task<Result<unit, ApplicationError>> }

let createUnderstandDemandWorkflow
    (observationApi: DemandObservationApi)
    (scopeApi: PlanningScopeApi)
    (edpApi: EnterpriseDemandPictureApi)
    (forecastQueries: ForecastPublicationQueries)
    : UnderstandDemandWorkflow =

    let determineScopeAndAssign (assignReq: AssignScopeReq) (scopeReq: DeterminePlanningScopeReq) =
        task {
            // 1. Determine or get scope
            let! scopeResult = scopeApi.Determine scopeReq // returns Result<string, ApiError>

            match scopeResult with
            | Error apiErr -> return Error(ApplicationError.Domain(DomainError.validation apiErr.Message))
            | Ok scopeId ->
                // 2. Assign scope to observation
                let! assignResult =
                    observationApi.AssignScope
                        { assignReq with
                            PlanningScopeId = scopeId }

                match assignResult with
                | Error apiErr -> return Error(ApplicationError.Domain(DomainError.validation apiErr.Message))
                | Ok _ -> return Ok()
        }

    let buildAndPublishEdp (reviseReq: ReviseEnterpriseDemandPictureReq) =
        task {
            // FS‑D‑004 → FS‑D‑005 → FS‑D‑006
            let! reviseResult = edpApi.Revise reviseReq

            match reviseResult with
            | Error apiErr -> return Error(ApplicationError.Domain(DomainError.validation apiErr.Message))
            | Ok(scopeId, _) ->
                let calcReq: CalculateEnterpriseDemandPictureReq = { PlanningScopeId = scopeId }
                let! calcResult = edpApi.Calculate calcReq

                match calcResult with
                | Error apiErr -> return Error(ApplicationError.Domain(DomainError.validation apiErr.Message))
                | Ok _ ->
                    let pubReq = { PlanningScopeId = scopeId }
                    let! pubResult = edpApi.Publish pubReq

                    match pubResult with
                    | Error apiErr -> return Error(ApplicationError.Domain(DomainError.validation apiErr.Message))
                    | Ok _ -> return Ok scopeId
        }

    let onForecastPublished (notification: ForecastPublishedNotification) =
        task {
            let operation (_ct: System.Threading.CancellationToken) (_attempt: int) =
                task {
                    let! pubOpt = forecastQueries.GetById notification.PublicationId
                    match pubOpt with
                    | Some pub when pub.Status = "Published" -> return Ok pub
                    | Some pub -> return Error(ApplicationError.Domain(DomainError.validation (sprintf "Publication %s status is %s, expected Published" notification.PublicationId pub.Status)))
                    | None -> return Error(ApplicationError.Domain(DomainError.notFound("ForecastPublication", notification.PublicationId)))
                }

            let retryConfig = { Medhavi.Common.Retry.RetryConfig.Default() with MaxAttempts = 100; BaseDelayMs = 50; MaxDelayMs = 50; BackoffMultiplier = 1.0 }
            let! retryResult =
                Medhavi.Common.Retry.executeWithRetry
                    operation
                    Microsoft.Extensions.Logging.Abstractions.NullLogger.Instance
                    (Some retryConfig)
                    System.Threading.CancellationToken.None
                    (fun () -> ApplicationError.Domain(DomainError.validation "Retry cancelled"))

            match retryResult with
            | Error err ->
                printfn "OnForecastPublished failed: %A" err
                return Error err
            | Ok pub ->
                    // 1. Build batch ingestion request
                    let ingestions =
                        pub.Forecasts
                        |> List.map(fun forecast ->
                            { ObservationId = $"FCAST-{notification.PublicationId}-{forecast.ForecastId}"
                              SkuId = forecast.SkuId
                              StockingPointId = forecast.StockingPointId
                              Quantity = forecast.Mean
                              UnitOfMeasure = "EA"
                              ObservationType = ObservationType.Signal
                              BusinessTime = forecast.PlanningPeriod.ToDateTimeOffset()
                              CustomerId = None
                              PromotionRef = None
                              CampaignRef = None
                              ContractRef = None
                              SourceSystem = "Forecast"
                              ExternalRef = forecast.ForecastId
                              MessageId = System.Guid.NewGuid().ToString()
                              Revision = 1 })

                    let batchReq = { Ingestions = ingestions }
                    let! res = observationApi.ReceiveBatch batchReq

                    match res with
                    | Error apiErr ->
                        printfn "OnForecastPublished: ReceiveBatch failed: %A" apiErr
                        return Error(ApplicationError.Domain(DomainError.validation apiErr.Message))
                    | Ok obsIds ->
                        // 2. For each observation, evaluate and push through the pipeline
                        let mutable errors = []

                        for (obsId, forecast) in List.zip obsIds pub.Forecasts do
                            // Evaluate (auto‑accept)
                            let evalReq: EvaluateObservationReq =
                                { ObservationId = obsId
                                  SignalId = None
                                  SignalSource = None
                                  SourceReliability = None
                                  SignalTimestamp = None
                                  SignalValue = None
                                  StatisticalBound = None
                                  RecentBaseline = None }

                            let! evalRes = observationApi.Evaluate evalReq

                            match evalRes with
                            | Error e ->
                                printfn "OnForecastPublished: Evaluate failed for %s: %A" obsId e
                                errors <- e :: errors
                            | Ok _ ->
                                // Determine scope and assign
                                let scopeReq: DeterminePlanningScopeReq =
                                    { SkuId = forecast.SkuId
                                      StockingPointId = forecast.StockingPointId
                                      CustomerId = None
                                      BucketType = "Weekly"
                                      PlanningPeriod = forecast.PlanningPeriod }

                                let assignReq =
                                    { ObservationId = obsId
                                      PlanningScopeId = "" }

                                let! scopeAssignRes = determineScopeAndAssign assignReq scopeReq

                                match scopeAssignRes with
                                | Error e ->
                                    printfn "OnForecastPublished: determineScopeAndAssign failed for %s: %A" obsId e
                                    errors <- (ApplicationError.mapToApiError e) :: errors
                                | Ok _ ->

                                    // Revise → Calculate → Publish EDP for the scope
                                    let scopeIdRes =
                                        PlanningScopeId.create(
                                            forecast.SkuId,
                                            forecast.StockingPointId,
                                            None,
                                            forecast.PlanningPeriod
                                        )

                                    match scopeIdRes with
                                    | Ok scopeId ->
                                        let reviseReq: ReviseEnterpriseDemandPictureReq =
                                            { PlanningScopeId = PlanningScopeId.value scopeId
                                              Period = forecast.PlanningPeriod
                                              Quantity = forecast.Mean
                                              ObservationId = obsId }

                                        let! edpRes = buildAndPublishEdp reviseReq

                                        match edpRes with
                                        | Error e ->
                                            printfn "OnForecastPublished: buildAndPublishEdp failed for %s: %A" (PlanningScopeId.value scopeId) e
                                            errors <- (ApplicationError.mapToApiError e) :: errors
                                        | Ok _ -> ()
                                    | Error e ->
                                        printfn "OnForecastPublished: scopeIdRes creation failed: %A" e
                                        errors <- (ApplicationError.mapToApiError e) :: errors

                        match errors with
                        | [] -> return Ok()
                        | _ ->
                            printfn "OnForecastPublished completed with errors: %A" errors
                            return
                                Error(
                                    ApplicationError.Domain(
                                        DomainError.validation "One or more forecast lines failed integration"
                                    )
                                )
        }

    { DetermineScopeAndAssign = determineScopeAndAssign
      BuildAndPublishEdp = buildAndPublishEdp
      OnForecastPublished = onForecastPublished }

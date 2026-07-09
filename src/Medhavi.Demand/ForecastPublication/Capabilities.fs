module Medhavi.Demand.ForecastPublication.Capabilities

open System.Threading.Tasks
open Medhavi.Contracts
open Medhavi.Common
open Medhavi.Common.Validation
open Medhavi.Contracts.Demand.ForecastPublication
open Medhavi.SharedKernel
open Medhavi.SharedKernel.Execution
open Medhavi.SharedKernel.Failure
open Medhavi.Demand
open Medhavi.Demand.ForecastPublication.ACL
open Medhavi.Demand.ForecastPublication.Model
open Medhavi.SharedKernel.Observation
open System

// Traceability: Implements CA-D-002 (Forecast Demand) Capabilities API for SE-D-035 (Forecast Publication)
// Exposes the workflow layer: validates raw requests, runs computation algorithms, calls CommandHandler, publishes notifications.

let createCapabilities
    (execute: ForecastPublicationCommand -> Task<ExecutionOutcome<ForecastPublication, ApplicationError>>)
    (computationService: ComputationService.Service)
    (queries: ForecastPublicationQueries)
    : ForecastPublicationApi =

    /// FS-D-005 — Initiate Forecast Cycle (Initialises SE-D-035 context)
    let initiateCycle (req: InitiateForecastCycleReq) =
        task {
            // 1. EARLY VALIDATION
            match toInitiateCmd req with
            | Invalid errors ->
                let apiErr =
                    ApplicationError.Domain(DomainError.combineValidationErrors errors)
                    |> ApplicationError.mapToApiError

                return Error apiErr
            | Valid cmd ->
                // 2. EXECUTE COMMAND
                let! outcome = execute(InitiateForecastCycle cmd)

                match outcome with
                | Completed pub ->
                    // 3. SUCCESS NOTIFICATION: Publish BN-D-010 Forecast Cycle Initialised
                    DomainEventBus.Publish(
                        { PublicationId = ForecastPublicationId.value pub.Id
                          PlanningScopeIds = pub.PlanningScopeIds |> List.map PlanningScopeId.value
                          CycleTime = pub.TransactionTime |> Timestamp.value }
                        : ForecastCycleInitialisedNotification
                    )

                    return Ok(ForecastPublicationId.value pub.Id)
                | Failed err -> return Error(ApplicationError.mapToApiError err)
                | Cancelled ->
                    return
                        Error
                            { Code = "CANCELLED"
                              Category = "Infrastructure"
                              Message = "Operation cancelled" }
        }

    /// FS-D-005 — Prepare Forecast Context (record baseline assumptions, boundaries)
    let prepareContext (req: PrepareForecastContextReq) =
        task {
            // 1. EARLY VALIDATION
            match toPrepareContextCmd req with
            | Invalid errors ->
                let apiErr =
                    ApplicationError.Domain(DomainError.combineValidationErrors errors)
                    |> ApplicationError.mapToApiError

                return Error apiErr
            | Valid cmd ->
                // 2. EXECUTE COMMAND
                let! outcome = execute(PrepareForecastContext cmd)

                match outcome with
                | Completed pub -> return Ok(ForecastPublicationId.value pub.Id)
                | Failed err -> return Error(ApplicationError.mapToApiError err)
                | Cancelled ->
                    return
                        Error
                            { Code = "CANCELLED"
                              Category = "Infrastructure"
                              Message = "Operation cancelled" }
        }

    /// FS-D-005 — Select Champion Model (evaluates candidate forecast performance and sets model reference)
    let selectChampion (req: SelectChampionModelReq) =
        task {
            // 1. EARLY VALIDATION
            match toSelectChampionCmd req with
            | Invalid errors ->
                let apiErr =
                    ApplicationError.Domain(DomainError.combineValidationErrors errors)
                    |> ApplicationError.mapToApiError

                return Error apiErr
            | Valid cmd ->
                // 2. EXECUTE COMMAND
                let! outcome = execute(SelectChampionModel cmd)

                match outcome with
                | Completed pub -> return Ok(ForecastPublicationId.value pub.Id)
                | Failed err -> return Error(ApplicationError.mapToApiError err)
                | Cancelled ->
                    return
                        Error
                            { Code = "CANCELLED"
                              Category = "Infrastructure"
                              Message = "Operation cancelled" }
        }

    /// FS-D-005 — Generate Baseline Forecasts (calls external forecast engine computation and updates lines)
    let generateBaseline (req: GenerateBaselineForecastsReq) =
        task {
            // 1. Fetch the published forecast DTO to obtain champion model and horizon details
            let! dtoOpt = queries.GetById req.PublicationId

            match dtoOpt with
            | None ->
                return
                    Error(
                        ApplicationError.Domain(DomainError.notFound("ForecastPublication", req.PublicationId))
                        |> ApplicationError.mapToApiError
                    )
            | Some dto ->
                // 2. Extract unique coverage from the DTO's coverage list
                let coverage =
                    dto.Coverage
                    |> List.map(fun f ->
                        result {
                            let! sku = SkuId.create f.SkuId
                            let! sp = StockingPointId.create f.StockingPointId
                            return (sku, sp)
                        })
                    |> Result.traverse id
                    |> Result.map(List.distinct >> List.ofSeq)

                match coverage with
                | Error err -> return Error(ApplicationError.Domain err |> ApplicationError.mapToApiError)
                | Ok coverageList ->
                    // 3. Build computation input for engine
                    let input: ComputationService.ForecastComputationInput =
                        { Coverage = coverageList
                          ForecastHorizon = dto.ForecastHorizon
                          BucketConfig = ""
                          ModelId = dto.ChampionModelId |> Option.defaultValue "default"
                          TargetPeriod = Some(PlanningPeriod.PlanningWeek(2027, 27))
                          TargetReconciliationTotal = None }

                    // 4. Run external computation service (forecast engine simulator)
                    let! forecastsRes = computationService.ComputeForecasts input

                    match forecastsRes with
                    | Error err -> return Error(err |> ApplicationError.mapToApiError)
                    | Ok forecastDtos ->
                        // 5. Build enriched request containing calculated forecasts and run ACL validation
                        let enrichedReq =
                            { req with
                                Forecasts = Some forecastDtos }

                        match toGenerateBaselineCmd enrichedReq with
                        | Invalid errors ->
                            return
                                Error(
                                    ApplicationError.Domain(DomainError.combineValidationErrors errors)
                                    |> ApplicationError.mapToApiError
                                )
                        | Valid cmd ->
                            let! outcome = execute(GenerateBaselineForecasts cmd)

                            match outcome with
                            | Completed pub -> return Ok(ForecastPublicationId.value pub.Id)
                            | Failed err -> return Error(ApplicationError.mapToApiError err)
                            | Cancelled ->
                                return
                                    Error
                                        { Code = "CANCELLED"
                                          Category = "Infrastructure"
                                          Message = "Operation cancelled" }
        }

    /// FS-D-005 — Record Forecast Override (validates manual adjustments and updates prediction bounds)
    let recordOverride (req: RecordForecastOverrideReq) =
        task {
            // 1. EARLY VALIDATION
            match toRecordOverrideCmd req with
            | Invalid errors ->
                let apiErr =
                    ApplicationError.Domain(DomainError.combineValidationErrors errors)
                    |> ApplicationError.mapToApiError

                return Error apiErr
            | Valid cmd ->
                // 2. EXECUTE COMMAND
                let! outcome = execute(RecordForecastOverride cmd)

                match outcome with
                | Completed pub ->
                    // 3. SUCCESS NOTIFICATION: Publish BN-D-012 Forecast Override Recorded
                    pub.Overrides
                    |> Map.values
                    |> Seq.sortByDescending(fun o -> Timestamp.value o.OverrideTimestamp)
                    |> Seq.tryHead
                    |> Option.iter(fun ov ->
                        DomainEventBus.Publish(
                            { PublicationId = ForecastPublicationId.value pub.Id
                              ForecastId = ForecastId.value ov.ForecastId
                              OverrideValue = ov.OverrideValue
                              PlannerIdentity = ov.PlannerIdentity }
                            : ForecastOverrideRecordedNotification
                        ))

                    return Ok(ForecastPublicationId.value pub.Id)
                | Failed err -> return Error(ApplicationError.mapToApiError err)
                | Cancelled ->
                    return
                        Error
                            { Code = "CANCELLED"
                              Category = "Infrastructure"
                              Message = "Operation cancelled" }
        }

    /// FS-D-005 — Reconcile Forecast Hierarchy
    let reconcile (req: ReconcileForecastHierarchyReq) =
        task {
            // 1. EARLY VALIDATION
            match toReconcileCmd req with
            | Invalid errors ->
                let apiErr =
                    ApplicationError.Domain(DomainError.combineValidationErrors errors)
                    |> ApplicationError.mapToApiError

                return Error apiErr
            | Valid cmd ->
                // 2. EXECUTE COMMAND
                let! outcome = execute(ReconcileForecastHierarchy cmd)

                match outcome with
                | Completed pub -> return Ok(ForecastPublicationId.value pub.Id)
                | Failed err -> return Error(ApplicationError.mapToApiError err)
                | Cancelled ->
                    return
                        Error
                            { Code = "CANCELLED"
                              Category = "Infrastructure"
                              Message = "Operation cancelled" }
        }

    /// FS-D-005 — Publish Forecast Publication (validates completeness, sum, non-negativity invariants)
    let publish (req: PublishForecastPublicationReq) =
        task {
            // 1. EARLY VALIDATION
            match toPublishCmd req with
            | Invalid errors ->
                let apiErr =
                    ApplicationError.Domain(DomainError.combineValidationErrors errors)
                    |> ApplicationError.mapToApiError
                // 3. FAILURE NOTIFICATION: Publish BN-D-013 Forecast Publication Failed
                DomainEventBus.Publish(
                    { PublicationId = req.PublicationId
                      Reason = apiErr.Message }
                    : ForecastPublicationFailedNotification
                )

                return Error apiErr
            | Valid cmd ->
                // 2. EXECUTE COMMAND
                let! outcome = execute(PublishForecastPublication cmd)

                match outcome with
                | Completed pub ->
                    // 3. SUCCESS NOTIFICATION: Publish BN-D-011 Forecast Published
                    DomainEventBus.Publish(
                        { PublicationId = ForecastPublicationId.value pub.Id
                          Version = pub.Version
                          PublicationTime =
                            pub.PublicationTime
                            |> Option.map Timestamp.value
                            |> Option.defaultValue System.DateTimeOffset.MinValue }
                        : ForecastPublishedNotification
                    )

                    // Emit PI-DI-103: Forecast Confidence Index
                    let conf = pub.OverallConfidenceIndex |> Option.defaultValue 1.0m
                    let event: TelemetryEvent =
                        { EventId = Guid.NewGuid()
                          Timestamp = DateTimeOffset.UtcNow
                          Severity = TelemetrySeverity.Information
                          Message = "PI-DI-103"
                          Properties = Map.ofList [ "Value", box conf; "ForecastPublicationId", box (ForecastPublicationId.value pub.Id) ]
                          CorrelationId = None
                          CausationId = None
                          TraceId = None
                          SpanId = None }
                    DomainEventBus.Publish event

                    return Ok(ForecastPublicationId.value pub.Id)
                | Failed err ->
                    // 3. FAILURE NOTIFICATION: Publish BN-D-013 Forecast Publication Failed with validation rules context (BR-D-050/051/052/053)
                    DomainEventBus.Publish(
                        { PublicationId = req.PublicationId
                          Reason = err.Message }
                        : ForecastPublicationFailedNotification
                    )

                    return Error(ApplicationError.mapToApiError err)
                | Cancelled ->
                    DomainEventBus.Publish(
                        { PublicationId = req.PublicationId
                          Reason = "Operation cancelled during publication corridor." }
                        : ForecastPublicationFailedNotification
                    )

                    return
                        Error
                            { Code = "CANCELLED"
                              Category = "Infrastructure"
                              Message = "Operation cancelled" }
        }

    { InitiateCycle = initiateCycle
      PrepareContext = prepareContext
      SelectChampion = selectChampion
      GenerateBaseline = generateBaseline
      RecordOverride = recordOverride
      Reconcile = reconcile
      Publish = publish }

namespace Medhavi.Foundation.Configuration

open System

type EventStoreSettings =
    { ConnectionString: string
      MaxRetryCount: int
      RetryDelayMs: int
      CommandTimeoutSeconds: int }

type CircuitBreakerSettings =
    { FailureThreshold: int
      RecoveryTimeoutSeconds: int
      MaxRecoveryTimeoutSeconds: int
      BackoffFactor: float
      SuccessThreshold: int }

type PlanningEngineSettings =
    { SolverTimeLimitSeconds: int
      MaxIterations: int
      OptimalityGap: float
      ParallelWorkers: int }

type ObservabilitySettings =
    { LogLevel: string
      MetricsEnabled: bool
      TracingEnabled: bool
      PrometheusPort: int option }

type AppSettings =
    { EventStore: EventStoreSettings
      CircuitBreaker: CircuitBreakerSettings
      PlanningEngine: PlanningEngineSettings
      Observability: ObservabilitySettings
      FeatureFlags: FeatureFlags }

module AppSettings =

    let private defaultSettings =
        { EventStore =
            { ConnectionString = "Host=localhost;Database=medhavi"
              MaxRetryCount = 3
              RetryDelayMs = 100
              CommandTimeoutSeconds = 30 }
          CircuitBreaker =
            { FailureThreshold = 5
              RecoveryTimeoutSeconds = 30
              MaxRecoveryTimeoutSeconds = 60
              BackoffFactor = 2.0
              SuccessThreshold = 3 }
          PlanningEngine =
            { SolverTimeLimitSeconds = 300
              MaxIterations = 1000
              OptimalityGap = 0.01
              ParallelWorkers = 4 }
          Observability =
            { LogLevel = "Information"
              MetricsEnabled = true
              TracingEnabled = true
              PrometheusPort = Some 9090 }
          FeatureFlags = FeatureFlags.loadFromEnvironment() }

    let private getEnv key = Environment.GetEnvironmentVariable(key)

    /// Load settings from a dictionary (e.g. parsed from JSON or env vars).
    /// Override defaults with provided values, then validate.
    let load (config: Map<string, string>) : Result<AppSettings, string> =
        let tryGet key = Map.tryFind key config

        let eventStore =
            { defaultSettings.EventStore with
                ConnectionString =
                    tryGet "EventStore:ConnectionString"
                    |> Option.defaultValue defaultSettings.EventStore.ConnectionString
                MaxRetryCount =
                    tryGet "EventStore:MaxRetryCount"
                    |> Option.bind(fun s ->
                        match Int32.TryParse s with
                        | true, v -> Some v
                        | _ -> None)
                    |> Option.defaultValue defaultSettings.EventStore.MaxRetryCount
                RetryDelayMs =
                    tryGet "EventStore:RetryDelayMs"
                    |> Option.bind(fun s ->
                        match Int32.TryParse s with
                        | true, v -> Some v
                        | _ -> None)
                    |> Option.defaultValue defaultSettings.EventStore.RetryDelayMs
                CommandTimeoutSeconds =
                    tryGet "EventStore:CommandTimeoutSeconds"
                    |> Option.bind(fun s ->
                        match Int32.TryParse s with
                        | true, v -> Some v
                        | _ -> None)
                    |> Option.defaultValue defaultSettings.EventStore.CommandTimeoutSeconds }

        let circuitBreaker =
            { defaultSettings.CircuitBreaker with
                FailureThreshold =
                    tryGet "CircuitBreaker:FailureThreshold"
                    |> Option.bind(fun s ->
                        match Int32.TryParse s with
                        | true, v -> Some v
                        | _ -> None)
                    |> Option.defaultValue defaultSettings.CircuitBreaker.FailureThreshold
                RecoveryTimeoutSeconds =
                    tryGet "CircuitBreaker:RecoveryTimeoutSeconds"
                    |> Option.bind(fun s ->
                        match Int32.TryParse s with
                        | true, v -> Some v
                        | _ -> None)
                    |> Option.defaultValue defaultSettings.CircuitBreaker.RecoveryTimeoutSeconds
                MaxRecoveryTimeoutSeconds =
                    tryGet "CircuitBreaker:MaxRecoveryTimeoutSeconds"
                    |> Option.bind(fun s ->
                        match Int32.TryParse s with
                        | true, v -> Some v
                        | _ -> None)
                    |> Option.defaultValue defaultSettings.CircuitBreaker.MaxRecoveryTimeoutSeconds
                BackoffFactor =
                    tryGet "CircuitBreaker:BackoffFactor"
                    |> Option.bind(fun s ->
                        match Double.TryParse s with
                        | true, v -> Some v
                        | _ -> None)
                    |> Option.defaultValue defaultSettings.CircuitBreaker.BackoffFactor
                SuccessThreshold =
                    tryGet "CircuitBreaker:SuccessThreshold"
                    |> Option.bind(fun s ->
                        match Int32.TryParse s with
                        | true, v -> Some v
                        | _ -> None)
                    |> Option.defaultValue defaultSettings.CircuitBreaker.SuccessThreshold }

        let planningEngine =
            { defaultSettings.PlanningEngine with
                SolverTimeLimitSeconds =
                    tryGet "PlanningEngine:SolverTimeLimitSeconds"
                    |> Option.bind(fun s ->
                        match Int32.TryParse s with
                        | true, v -> Some v
                        | _ -> None)
                    |> Option.defaultValue defaultSettings.PlanningEngine.SolverTimeLimitSeconds
                MaxIterations =
                    tryGet "PlanningEngine:MaxIterations"
                    |> Option.bind(fun s ->
                        match Int32.TryParse s with
                        | true, v -> Some v
                        | _ -> None)
                    |> Option.defaultValue defaultSettings.PlanningEngine.MaxIterations
                OptimalityGap =
                    tryGet "PlanningEngine:OptimalityGap"
                    |> Option.bind(fun s ->
                        match Double.TryParse s with
                        | true, v -> Some v
                        | _ -> None)
                    |> Option.defaultValue defaultSettings.PlanningEngine.OptimalityGap
                ParallelWorkers =
                    tryGet "PlanningEngine:ParallelWorkers"
                    |> Option.bind(fun s ->
                        match Int32.TryParse s with
                        | true, v -> Some v
                        | _ -> None)
                    |> Option.defaultValue defaultSettings.PlanningEngine.ParallelWorkers }

        let observability =
            { defaultSettings.Observability with
                LogLevel = tryGet "Observability:LogLevel" |> Option.defaultValue defaultSettings.Observability.LogLevel
                MetricsEnabled =
                    tryGet "Observability:MetricsEnabled"
                    |> Option.bind(fun s ->
                        match Boolean.TryParse s with
                        | true, v -> Some v
                        | _ -> None)
                    |> Option.defaultValue defaultSettings.Observability.MetricsEnabled
                TracingEnabled =
                    tryGet "Observability:TracingEnabled"
                    |> Option.bind(fun s ->
                        match Boolean.TryParse s with
                        | true, v -> Some v
                        | _ -> None)
                    |> Option.defaultValue defaultSettings.Observability.TracingEnabled
                PrometheusPort =
                    tryGet "Observability:PrometheusPort"
                    |> Option.bind(fun s ->
                        match Int32.TryParse s with
                        | true, v -> Some v
                        | _ -> None) }

        let featureFlags = FeatureFlags.loadFromEnvironment()

        let settings =
            { EventStore = eventStore
              CircuitBreaker = circuitBreaker
              PlanningEngine = planningEngine
              Observability = observability
              FeatureFlags = featureFlags }

        // Validation
        let errors = ResizeArray<string>()

        if String.IsNullOrWhiteSpace settings.EventStore.ConnectionString then
            errors.Add "EventStore.ConnectionString is required"

        if settings.EventStore.MaxRetryCount < 0 then
            errors.Add "EventStore.MaxRetryCount must be >= 0"

        if settings.CircuitBreaker.FailureThreshold < 1 then
            errors.Add "CircuitBreaker.FailureThreshold must be >= 1"

        if settings.CircuitBreaker.RecoveryTimeoutSeconds < 5 then
            errors.Add "CircuitBreaker.RecoveryTimeoutSeconds must be >= 5"

        if settings.PlanningEngine.SolverTimeLimitSeconds <= 0 then
            errors.Add "PlanningEngine.SolverTimeLimitSeconds must be > 0"

        if settings.PlanningEngine.MaxIterations <= 0 then
            errors.Add "PlanningEngine.MaxIterations must be > 0"

        let featureFlagErrors = FeatureFlags.validate settings.FeatureFlags
        errors.AddRange featureFlagErrors

        if errors.Count > 0 then Error(String.concat "; " errors) else Ok settings

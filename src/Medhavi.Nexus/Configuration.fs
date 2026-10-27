module Medhavi.Nexus.Configuration

open System
open Microsoft.Extensions.Configuration
open Medhavi.Common
open Medhavi.Common.Optics
open Medhavi.Common.Validation

type RadzenConfig = { Theme: string }

type ServerConfig = { Port: int; LogMinimumLevel: string }

type LoggingConfig =
    { ConsoleEnabled: bool
      FileEnabled: bool
      FilePath: string
      MinimumLevel: string
      UseMailboxLogger: bool
      MailboxBatchSize: int
      MailboxTimeoutMs: int }

type TelemetryConfig =
    { ConsoleEnabled: bool
      FileEnabled: bool
      FilePath: string
      MinimumSeverity: string
      BatchSize: int
      FlushIntervalMs: int }

type NexusConfig =
    { Server: ServerConfig
      Radzen: RadzenConfig
      Logging: LoggingConfig
      Telemetry: TelemetryConfig }

type ConfigurationFailure =
    | MissingValue of Key: string
    | InvalidInteger of Key: string * Value: string
    | InvalidValue of Key: string * Reason: string
    | DevelopmentAdapterForbidden

module ConfigLenses =
    let serverPortLens: Lens<ServerConfig, int> =
        lens (fun s -> s.Port) (fun v s -> { s with Port = v })

    let loggingFilePathLens: Lens<LoggingConfig, string> =
        lens (fun l -> l.FilePath) (fun v l -> { l with FilePath = v })

    let telemetryBatchSizeLens: Lens<TelemetryConfig, int> =
        lens (fun t -> t.BatchSize) (fun v t -> { t with BatchSize = v })

    let nexusLoggingLens: Lens<NexusConfig, LoggingConfig> =
        lens (fun n -> n.Logging) (fun v n -> { n with Logging = v })

    let nexusTelemetryLens: Lens<NexusConfig, TelemetryConfig> =
        lens (fun n -> n.Telemetry) (fun v n -> { n with Telemetry = v })

module NexusConfig =

    let private readString (config: IConfiguration) (primaryKey: string) (fallbackKey: string option) (defaultVal: string) : string =
        match config.GetValue<string>(primaryKey) with
        | null | "" ->
            match fallbackKey with
            | Some fb ->
                match config.GetValue<string>(fb) with
                | null | "" -> defaultVal
                | s -> s
            | None -> defaultVal
        | s -> s

    let private readBool (config: IConfiguration) (primaryKey: string) (fallbackKey: string option) (defaultVal: bool) : bool =
        match config.GetValue<Nullable<bool>>(primaryKey) with
        | v when v.HasValue -> v.Value
        | _ ->
            match fallbackKey with
            | Some fb ->
                match config.GetValue<Nullable<bool>>(fb) with
                | v when v.HasValue -> v.Value
                | _ -> defaultVal
            | None -> defaultVal

    let private readInt (config: IConfiguration) (primaryKey: string) (fallbackKey: string option) (defaultVal: int) : int =
        match config.GetValue<Nullable<int>>(primaryKey) with
        | v when v.HasValue -> v.Value
        | _ ->
            match fallbackKey with
            | Some fb ->
                match config.GetValue<Nullable<int>>(fb) with
                | v when v.HasValue -> v.Value
                | _ -> defaultVal
            | None -> defaultVal

    let private portValidation (port: int) : Validation<int, ConfigurationFailure> =
        if port > 0 && port <= 65535 then Valid port
        else Invalid [ InvalidValue("Server:Port", $"Port must be between 1 and 65535, got {port}") ]

    let private minLogLevelValidation (level: string) : Validation<string, ConfigurationFailure> =
        match level.ToLowerInvariant() with
        | "trace" | "debug" | "information" | "warning" | "error" | "critical" | "none" -> Valid level
        | other -> Invalid [ InvalidValue("Logging:MinimumLevel", $"Unknown log level '{other}'") ]

    let private logFilePathValidation (path: string) : Validation<string, ConfigurationFailure> =
        if String.IsNullOrWhiteSpace(path) then Invalid [ MissingValue("Logging:File:Path") ]
        else Valid path

    let private positiveIntValidation (key: string) (v: int) : Validation<int, ConfigurationFailure> =
        if v > 0 then Valid v
        else Invalid [ InvalidValue(key, $"Value must be strictly positive (> 0), got {v}") ]

    let private serverConfigValidation (config: IConfiguration) : Validation<ServerConfig, ConfigurationFailure> =
        let rawPort = readInt config "Server:Port" None 5000
        let rawLogLevel = readString config "Server:LogMinimumLevel" None "Information"
        lift2 (fun p l -> { Port = p; LogMinimumLevel = l })
              (portValidation rawPort)
              (minLogLevelValidation rawLogLevel)

    let private radzenConfigValidation (config: IConfiguration) : Validation<RadzenConfig, ConfigurationFailure> =
        let theme = readString config "Radzen:Theme" None "Material"
        Valid { Theme = theme }

    let private loggingConfigValidation (config: IConfiguration) : Validation<LoggingConfig, ConfigurationFailure> =
        let consoleEnabled = readBool config "Logging:Console:Enabled" None true
        let fileEnabled = readBool config "Logging:File:Enabled" None true
        let rawPath = readString config "Logging:File:Path" (Some "Logging:FilePath") "logs/medhavi-nexus-{yyyyMMdd}.log"
        let rawMinLevel = readString config "Logging:File:MinimumLevel" (Some "Logging:LogLevel:Default") "Information"
        let useMailbox = readBool config "Logging:File:UseMailboxLogger" (Some "Logging:UseMailboxLogger") true
        let rawBatchSize = readInt config "Logging:File:BatchSize" (Some "Logging:MailboxBatchSize") 50
        let rawTimeoutMs = readInt config "Logging:File:FlushIntervalMs" (Some "Logging:MailboxTimeoutMs") 500

        lift5 (fun p l b t c ->
            { ConsoleEnabled = c
              FileEnabled = fileEnabled
              FilePath = p
              MinimumLevel = l
              UseMailboxLogger = useMailbox
              MailboxBatchSize = b
              MailboxTimeoutMs = t })
            (logFilePathValidation rawPath)
            (minLogLevelValidation rawMinLevel)
            (positiveIntValidation "Logging:File:BatchSize" rawBatchSize)
            (positiveIntValidation "Logging:File:FlushIntervalMs" rawTimeoutMs)
            (Valid consoleEnabled)

    let private telemetryConfigValidation (config: IConfiguration) : Validation<TelemetryConfig, ConfigurationFailure> =
        let consoleEnabled = readBool config "Telemetry:Console:Enabled" None true
        let fileEnabled = readBool config "Telemetry:File:Enabled" None true
        let rawPath = readString config "Telemetry:File:Path" None "logs/telemetry-{yyyyMMdd}.jsonl"
        let rawSeverity = readString config "Telemetry:MinimumSeverity" None "Information"
        let rawBatchSize = readInt config "Telemetry:BatchSize" None 25
        let rawFlushInterval = readInt config "Telemetry:FlushIntervalMs" None 500

        lift4 (fun p s b f ->
            { ConsoleEnabled = consoleEnabled
              FileEnabled = fileEnabled
              FilePath = p
              MinimumSeverity = s
              BatchSize = b
              FlushIntervalMs = f })
            (if String.IsNullOrWhiteSpace rawPath then Invalid [ MissingValue "Telemetry:File:Path" ] else Valid rawPath)
            (minLogLevelValidation rawSeverity)
            (positiveIntValidation "Telemetry:BatchSize" rawBatchSize)
            (positiveIntValidation "Telemetry:FlushIntervalMs" rawFlushInterval)

    let nexusConfigValidation (config: IConfiguration) : Validation<NexusConfig, ConfigurationFailure> =
        lift4 (fun s r l t ->
            { Server = s
              Radzen = r
              Logging = l
              Telemetry = t })
            (serverConfigValidation config)
            (radzenConfigValidation config)
            (loggingConfigValidation config)
            (telemetryConfigValidation config)

    let load (configuration: IConfiguration) : NexusConfig =
        match nexusConfigValidation configuration with
        | Valid cfg -> cfg
        | Invalid errors ->
            let formattedErrors =
                errors
                |> List.map (function
                    | MissingValue k -> $"Missing configuration key: '{k}'"
                    | InvalidInteger(k, v) -> $"Invalid integer for '{k}': '{v}'"
                    | InvalidValue(k, r) -> $"Invalid value for '{k}': {r}"
                    | DevelopmentAdapterForbidden -> "Development adapter forbidden in non-development environment")
                |> String.concat "; "
            failwith $"[NexusConfig] Host configuration validation failed with {errors.Length} error(s): {formattedErrors}"

let clientConfigValidation
    (environmentName: string)
    (configuration: IConfiguration)
    : Validation<Medhavi.Contracts.Client.ClientConfiguration, ConfigurationFailure> =
    let requiredValidation key =
        match configuration[key] with
        | null
        | "" -> Invalid [ MissingValue key ]
        | value -> Valid value

    let integerValidation key =
        match requiredValidation key with
        | Invalid e -> Invalid e
        | Valid value ->
            match Int32.TryParse value with
            | true, parsed -> Valid parsed
            | false, _ -> Invalid [ InvalidInteger(key, value) ]

    let developmentEnabled = configuration.GetValue<bool>("Adapters:Development:Enabled")
    let devEnvValidation =
        if developmentEnabled && not (String.Equals(environmentName, "Development", StringComparison.Ordinal)) then
            Invalid [ DevelopmentAdapterForbidden ]
        else
            Valid developmentEnabled

    lift4 (fun name envLabel devEnabled maxRisk ->
        { Medhavi.Contracts.Client.ClientConfiguration.ProductName = name
          EnvironmentLabel = envLabel
          DevelopmentAdapterEnabled = devEnabled
          MaximumRiskRows = maxRisk })
        (requiredValidation "Product:Name")
        (requiredValidation "Product:EnvironmentLabel")
        devEnvValidation
        (integerValidation "Queries:MaximumRiskRows")

let loadClientConfiguration
    (environmentName: string)
    (configuration: IConfiguration)
    : Result<Medhavi.Contracts.Client.ClientConfiguration, ConfigurationFailure list> =
    clientConfigValidation environmentName configuration
    |> Validation.toResult

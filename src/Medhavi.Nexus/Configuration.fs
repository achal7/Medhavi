module Medhavi.Nexus.Configuration

open System
open Microsoft.Extensions.Configuration

type RadzenConfig = { Theme: string }

type ServerConfig = { Port: int; LogMinimumLevel: string }

type NexusConfig =
    { Server: ServerConfig
      Radzen: RadzenConfig }

module NexusConfig =
    let load (configuration: IConfiguration) : NexusConfig =
        { Server =
            { Port = configuration.GetValue("Server:Port", 5000)
              LogMinimumLevel = configuration.GetValue("Server:LogMinimumLevel", "Information") }
          Radzen = { Theme = configuration.GetValue("Radzen:Theme", "Material") } }

type ConfigurationFailure =
    | MissingValue of Key: string
    | InvalidInteger of Key: string * Value: string
    | DevelopmentAdapterForbidden

let loadClientConfiguration
    (environmentName: string)
    (configuration: IConfiguration)
    : Result<Medhavi.Contracts.Client.ClientConfiguration, ConfigurationFailure list> =
    let required key =
        match configuration[key] with
        | null
        | "" -> Error(MissingValue key)
        | value -> Ok value

    let integer key =
        required key
        |> Result.bind(fun value ->
            match Int32.TryParse value with
            | true, parsed -> Ok parsed
            | false, _ -> Error(InvalidInteger(key, value)))

    let developmentEnabled = configuration.GetValue<bool>("Adapters:Development:Enabled")

    let errors =
        [ required "Product:Name" |> Result.map ignore
          required "Product:EnvironmentLabel" |> Result.map ignore
          integer "Queries:MaximumRiskRows" |> Result.map ignore
          if developmentEnabled && not(String.Equals(environmentName, "Development", StringComparison.Ordinal)) then
              Error DevelopmentAdapterForbidden
          else
              Ok() ]
        |> List.choose (function
            | Error error -> Some error
            | Ok() -> None)

    match errors with
    | _ :: _ -> Error errors
    | [] ->
        Ok
            { ProductName = configuration["Product:Name"]
              EnvironmentLabel = configuration["Product:EnvironmentLabel"]
              DevelopmentAdapterEnabled = developmentEnabled
              MaximumRiskRows = configuration.GetValue<int>("Queries:MaximumRiskRows") }

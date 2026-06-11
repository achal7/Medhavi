namespace Medhavi.Scenario

open System
open System.Threading.Tasks
open Medhavi.SharedKernel
open Medhavi.SharedKernel.ScenarioContracts

type ScenarioReadModel =
    { ScenarioId: string
      Name: string
      BaseScenarioId: string option
      Version: int
      CreatedAt: DateTimeOffset
      IsActive: bool
      Overrides: ScenarioDataOverride list }

type ScenarioQueries =
    { GetById: string -> Task<ScenarioReadModel option>
      GetAll: unit -> Task<ScenarioReadModel list> }

type ScenarioCommands =
    { Create: string * string * ScenarioType -> Task<Result<unit, DomainError>>
      AddOverride: string * ScenarioDataOverride -> Task<Result<unit, DomainError>>
      RemoveOverride: string * ScenarioDataOverride -> Task<Result<unit, DomainError>> }

type ScenarioContext =
    { Commands: ScenarioCommands
      Queries: ScenarioQueries
      Initialize: unit -> Task<unit>
      Dispose: unit -> unit }
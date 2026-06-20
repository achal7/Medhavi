namespace Medhavi.Scenario

open System.Threading.Tasks
open Medhavi.SharedKernel
open Medhavi.Contracts.Scenario

type ScenarioQueries =
    { GetById: string -> Task<Scenario option>
      GetAll: unit -> Task<Scenario list> }

type ScenarioCommands =
    { Create: string * string * ScenarioType * string option -> Task<Result<unit, DomainError>>
      AddOverride: string * ScenarioDataOverride -> Task<Result<unit, DomainError>>
      RemoveOverride: string * ScenarioDataOverride -> Task<Result<unit, DomainError>>
      SubmitForApproval: string -> Task<Result<unit, DomainError>>
      Approve: string -> Task<Result<unit, DomainError>>
      Reject: string * string -> Task<Result<unit, DomainError>>
      Archive: string * string option * string option -> Task<Result<unit, DomainError>> }

type ScenarioContext =
    { Commands: ScenarioCommands
      Queries: ScenarioQueries
      Initialize: unit -> Task<unit>
      Dispose: unit -> unit }

namespace Medhavi.Scenario

open System
open System.Threading.Tasks
open Medhavi.SharedKernel

/// Pegging Link connecting a demand requirements to supply orders
type PeggingLink = {
    PegId: string
    DemandOrderId: OrderId
    DemandLineId: string
    SupplyRefId: string // E.g. PlannedOrderId or purchase order id
    PeggedQty: Quantity
    IsFixed: bool
}

/// Scenario metadata and sandboxing state
type Scenario = {
    ScenarioId: string
    Name: string
    BaseScenarioId: string option
    Version: int
    CreatedAt: DateTimeOffset
    IsActive: bool
    Overrides: ScenarioDataOverride list
}

type ScenarioQueries =
    { GetById: string -> Task<Scenario option>
      GetAll: unit -> Task<Scenario list> }

type ScenarioCommands =
    { Create: Scenario -> Task<Result<unit, DomainError>>
      AddOverride: string -> ScenarioDataOverride -> Task<Result<unit, DomainError>>
      RemoveOverride: string -> ScenarioDataOverride -> Task<Result<unit, DomainError>> }

type ScenarioContext =
    { Commands: ScenarioCommands
      Queries: ScenarioQueries
      Initialize: unit -> Task<unit>
      Dispose: unit -> unit }
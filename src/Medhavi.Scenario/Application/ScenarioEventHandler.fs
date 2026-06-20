namespace Medhavi.Scenario.Application

open Medhavi.SharedKernel
open Medhavi.Contracts.Scenario
open Medhavi.Scenario.Domain

type BcIntegrationEvent =
    | DemandAggregateVersionChanged of scenarioId: ScenarioId * newVersion: int * changedDemandIds: string list
    | CapacityAggregateVersionChanged of scenarioId: ScenarioId * newVersion: int * changedResourceIds: string list
    | InventoryAggregateVersionChanged of scenarioId: ScenarioId * newVersion: int
    | BomVersionChanged of scenarioId: ScenarioId * newVersion: int
    | RoutingVersionChanged of scenarioId: ScenarioId * newVersion: int

module ScenarioEventHandler =
    let toDirtyCommand (event: BcIntegrationEvent) : ScenarioId * ScenarioCommand =
        match event with
        | DemandAggregateVersionChanged(scenarioId, newVersion, changedIds) ->
            scenarioId, MarkDirtyWith(DemandDataChanged(0, newVersion, changedIds))
        | CapacityAggregateVersionChanged(scenarioId, newVersion, changedIds) ->
            scenarioId, MarkDirtyWith(CapacityDataChanged(0, newVersion, changedIds))
        | InventoryAggregateVersionChanged(scenarioId, newVersion) ->
            scenarioId, MarkDirtyWith(InventoryDataChanged(0, newVersion))
        | BomVersionChanged(scenarioId, newVersion) -> scenarioId, MarkDirtyWith(BomOrRoutingChanged(0, newVersion))
        | RoutingVersionChanged(scenarioId, newVersion) -> scenarioId, MarkDirtyWith(BomOrRoutingChanged(0, newVersion))

    let handle (store: ScenarioStore) (event: BcIntegrationEvent) : Async<Result<unit, DomainError>> =
        async {
            let scenarioId, command = toDirtyCommand event
            let! stateOpt = store.Load scenarioId

            match ScenarioAgg.handle command stateOpt with
            | Ok dec ->
                do! store.Save dec.NewState dec.Events
                return Ok()
            | Error err -> return Error err
        }

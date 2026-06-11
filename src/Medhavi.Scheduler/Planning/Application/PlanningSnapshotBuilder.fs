namespace Medhavi.Scheduler.Planning.Application

open System
open Medhavi.SharedKernel
open Medhavi.Scheduler.Planning.Domain


type ISupplyPort =
    abstract member GetSupply: ScenarioId -> DateTimeOffset -> Async<SupplyBucket list>

type IDemandPort =
    abstract member GetDemand: ScenarioId -> DateTimeOffset -> Async<DemandBucket list>

type ICapacityPort =
    abstract member GetCapacity: ScenarioId -> DateTimeOffset -> Async<ResourceCapacity list>

type IMasterDataPort =
    abstract member GetBomEdges: DateTimeOffset -> Async<BomEdge list>
    abstract member GetRoutings: DateTimeOffset -> Async<Routing list>

type SnapshotBuilderDependencies =
    { Supply: ISupplyPort
      Demand: IDemandPort
      Capacity: ICapacityPort
      MasterData: IMasterDataPort }

module PlanningSnapshotBuilder =
    let load
        (deps: SnapshotBuilderDependencies)
        (scenarioId: ScenarioId)
        (asOf: DateTimeOffset)
        : Async<PlanningInputData> =
        async {
            let! supplies = deps.Supply.GetSupply scenarioId asOf
            let! demands = deps.Demand.GetDemand scenarioId asOf
            let! capacities = deps.Capacity.GetCapacity scenarioId asOf
            let! bomEdges = deps.MasterData.GetBomEdges asOf
            let! routings = deps.MasterData.GetRoutings asOf

            return
                { ScenarioId = scenarioId
                  AsOf = asOf
                  SupplyBuckets = supplies
                  Demands = demands
                  Capacities = capacities
                  BomEdges = bomEdges
                  Routings = routings }
        }

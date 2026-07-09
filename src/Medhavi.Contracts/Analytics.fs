namespace Medhavi.Contracts.Analytics

open System
open Medhavi.Contracts
open Medhavi.Contracts.MasterData.Transport

type PlanningHorizonQueryDto =
    { PlantId: string
      StartDate: DateOnly
      EndDate: DateOnly
      Granularity: string // "Day" | "Week" | "Month" | "Quarter"
      Context: string // "Live" | "Scenario:scenarioId"
      SkuFilter: string list option
      ResourceFilter: string list option }

type KpiQueryDto =
    { PlantId: string
      Periods: string list
      Context: string
      SkuFilter: string list option }

type PlanContext =
    | Live
    | Scenario of scenarioId: string

// Aggregated transport view for a single PlanningPeriod on a transport leg.
type TransportPeriodView =
    { Period: PlanningPeriod
      TransportLegId: string
      FromPlantId: string
      ToPlantId: string
      TotalOutboundQty: decimal
      TotalInboundQty: decimal
      LegCapacity: decimal option
      CapacityUtilizPct: decimal option
      EstimatedCost: decimal option
      Shipments: ShipmentView list }

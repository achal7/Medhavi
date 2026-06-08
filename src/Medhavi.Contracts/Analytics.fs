namespace Medhavi.Contracts.Analytics

open System

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

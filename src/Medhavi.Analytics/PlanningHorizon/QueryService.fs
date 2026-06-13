namespace Medhavi.Analytics.PlanningHorizon

open System
open System.Threading.Tasks
open Medhavi.Contracts.Analytics
open Medhavi.Contracts.Capacity
open Medhavi.Contracts.Capacity
open Medhavi.Contracts.Demand
open Medhavi.Contracts.Supply
open Medhavi.Contracts.Transport

// =============================================================================
// Planning Horizon Query Service — the main entry point for projections
// =============================================================================

/// Request parameters for a planning horizon query
type PlanningHorizonRequest =
    { PlantId: string
      StartDate: DateOnly
      EndDate: DateOnly
      Granularity: PlanningGranularity
      Context: PlanContext
      SkuFilter: string list option // None = all SKUs
      ResourceFilter: string list option } // None = all resource groups

/// Full planning horizon response — all five planes
type PlanningHorizonResponse =
    { Request: PlanningHorizonRequest
      Periods: PlanningPeriod list
      Demand: DemandPeriodView list
      Material: MaterialPeriodView list
      Capacity: CapacityPeriodView list
      Transport: TransportPeriodView list
      GanttGrid: GanttGrid
      GeneratedAt: DateTimeOffset }

/// Query service record
type PlanningHorizonQueryService =
    {
        /// Get the full planning horizon across all five planes
        GetPlanningHorizon: PlanningHorizonRequest -> Task<PlanningHorizonResponse>
        /// Get only the demand plane
        GetDemandPlane: PlanningHorizonRequest -> Task<DemandPeriodView list>
        /// Get only the material plane
        GetMaterialPlane: PlanningHorizonRequest -> Task<MaterialPeriodView list>
        /// Get only the capacity/Gantt plane
        GetCapacityPlane: PlanningHorizonRequest -> Task<GanttGrid>
        /// Get only the transport plane
        GetTransportPlane: PlanningHorizonRequest -> Task<TransportPeriodView list>
    }

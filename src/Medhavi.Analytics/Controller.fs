namespace Medhavi.Analytics

open System
open System.Threading.Tasks
open Medhavi.Analytics.PlanningHorizon
open Medhavi.Analytics.KPI

module Controller =

    type AnalyticsController =
        { GetPlanningHorizon : PlanningHorizonRequest -> Task<PlanningHorizonResponse>
          GetKpis            : KpiQueryRequest -> Task<KpiPeriodView list> }

    let createController
        (horizonService: PlanningHorizonQueryService)
        (kpiService: KpiQueryService)
        : AnalyticsController =
        { GetPlanningHorizon = horizonService.GetPlanningHorizon
          GetKpis            = kpiService.GetKpiPeriodViews }

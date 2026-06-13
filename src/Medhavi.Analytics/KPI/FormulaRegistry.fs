namespace Medhavi.Analytics.KPI

open Medhavi.Contracts.Capacity
open Medhavi.Contracts.Demand
open Medhavi.Contracts.Supply

type KpiFormula<'T> =
    { KpiId: string
      KpiClass: KpiClass
      HigherIsBetter: bool
      Calculate: 'T -> decimal }

module FormulaRegistry =

    // --- Class 1: Plan-Run-Dependent ---
    let OTD : KpiFormula<DemandPeriodView> =
        { KpiId = "OTD"
          KpiClass = PlanRunDependent
          HigherIsBetter = true
          Calculate = fun v ->
              let n = decimal v.DemandLines.Length
              if n = 0m then 100m
              else v.DemandLines |> List.filter (fun l -> l.LatenessRisk = OnTrack) |> List.length |> decimal |> fun x -> x / n * 100m }

    let OTIF : KpiFormula<DemandPeriodView> =
        { KpiId = "OTIF"
          KpiClass = PlanRunDependent
          HigherIsBetter = true
          Calculate = fun v -> if v.TotalDemandQty = 0m then 100m else v.ConfirmedQty / v.TotalDemandQty * 100m }

    // --- Class 2: Operational State ---
    let Utilization : KpiFormula<CapacityPeriodView> =
        { KpiId = "Utilization"
          KpiClass = OperationalState
          HigherIsBetter = false
          Calculate = fun v -> v.UtilizationPct }

    let DaysOfSupply : KpiFormula<MaterialPeriodView> =
        { KpiId = "DaysOfSupply"
          KpiClass = OperationalState
          HigherIsBetter = true
          Calculate = fun v ->
              let avgDaily = v.DemandConsumption / 7m
              if avgDaily = 0m then 999m else v.ProjectedStock / avgDaily }

    let SafetyStockCoverage : KpiFormula<MaterialPeriodView> =
        { KpiId = "SafetyStockCoverage"
          KpiClass = OperationalState
          HigherIsBetter = true
          Calculate = fun v -> if v.SafetyStockQty = 0m then 100m else min 100m (v.ProjectedStock / v.SafetyStockQty * 100m) }

    // --- Class 3: Execution / Real-Time ---
    let ScheduleAdherence : KpiFormula<CapacityPeriodView> =
        { KpiId = "ScheduleAdherence"
          KpiClass = ExecutionRealTime
          HigherIsBetter = true
          Calculate = fun v ->
              let n = decimal v.Operations.Length
              if n = 0m then 100m
              else v.Operations |> List.filter (fun o -> o.Status = OperationStatus.Completed) |> List.length |> decimal |> fun x -> x / n * 100m }

/// Baseline Composition Algorithms for SE-S-071
/// Traces to: Supply Intelligence Specification §4.3.1 (AB-S-061)
module Medhavi.Supply.ScheduleProduction.PublishedProductionSchedule.Algorithms

open System
open Medhavi.SemanticModel
open Medhavi.Supply
open Model
open Policies

type BaselineCompositionResult =
    { ActiveScheduleIds: string list
      Metrics: AggregateScheduleMetrics
      Feasibility: FeasibilitySummary
      Stability: StabilitySummary
      Risks: RiskSignals
      Recovery: RecoveryOpportunities }

/// Deterministically composes active schedules into aggregate baseline metrics and risk summaries
let composeBaseline
    (activeSchedules: ActiveScheduleFact list)
    (policy: PublicationPolicyConfig)
    : BaselineCompositionResult =

    if activeSchedules.IsEmpty then
        { ActiveScheduleIds = []
          Metrics =
            { TotalChangeoverHours = 0.0M
              ResourceUtilization = 0.0M
              OvertimeRequiredHours = 0.0M
              ScheduleAdherenceProjection = 0.0M }
          Feasibility =
            { FeasibleCount = 0
              ConditionallyFeasibleCount = 0
              InfeasibleCount = 0
              TopConstraints = [] }
          Stability =
            { AverageStabilityScore = 100.0M
              HighVolatilityResources = [] }
          Risks =
            { ResourcesAtRiskOfDelay = []
              MaterialsAtRiskOfUnavailability = []
              BottleneckResources = []
              OvertimeDependentSchedules = [] }
          Recovery =
            { RecoverableResources = []
              EstimatedRecoverableHours = 0.0M
              RecoveryStrategy = "No active schedules in scope" } }
    else
        let schedIds = activeSchedules |> List.map (fun s -> s.ScheduleId)
        let totalChangeover = activeSchedules |> List.sumBy (fun s -> s.TotalChangeoverHours)
        let totalOvertime = activeSchedules |> List.sumBy (fun s -> s.OvertimeHours)

        let avgUtilization =
            let count = decimal activeSchedules.Length
            activeSchedules |> List.sumBy (fun s -> s.ResourceUtilization) |> fun sum -> Math.Round(sum / count, 4)

        let avgStability =
            let count = decimal activeSchedules.Length
            activeSchedules |> List.sumBy (fun s -> s.StabilityScore) |> fun sum -> Math.Round(sum / count, 2)

        let (feasibleCount, conditionallyCount, infeasibleCount, topConstraints) =
            activeSchedules
            |> List.fold (fun (f, c, i, tc) s ->
                match s.Feasibility with
                | ScheduleFeasibility.FullyFeasible -> (f + 1, c, i, tc)
                | ScheduleFeasibility.ConditionallyFeasible conds -> (f, c + 1, i, conds @ tc)
                | ScheduleFeasibility.Infeasible reasons -> (f, c, i + 1, reasons @ tc)
            ) (0, 0, 0, [])

        let adherenceProjection =
            if infeasibleCount > 0 then 60.0M
            elif conditionallyCount > 0 then 85.0M
            else 98.0M

        let highVolatility =
            activeSchedules
            |> List.filter (fun s -> s.StabilityScore < policy.StabilityScoreThreshold)
            |> List.map (fun s -> s.ResourceId)

        let delayRisks =
            activeSchedules
            |> List.filter (fun s -> s.HasDelayRisk)
            |> List.map (fun s -> s.ResourceId)

        let materialRisks =
            activeSchedules
            |> List.filter (fun s -> s.HasMaterialRisk)
            |> List.map (fun s -> s.ResourceId)

        let bottlenecks =
            activeSchedules
            |> List.filter (fun s -> s.IsBottleneck)
            |> List.map (fun s -> s.ResourceId)

        let overtimeDependent =
            activeSchedules
            |> List.filter (fun s -> s.IsOvertimeDependent)
            |> List.map (fun s -> s.ResourceId)

        let recoverable =
            activeSchedules
            |> List.filter (fun s -> s.HasDelayRisk && not s.IsBottleneck)
            |> List.map (fun s -> s.ResourceId)

        let recoverableHours =
            activeSchedules
            |> List.filter (fun s -> s.HasDelayRisk && not s.IsBottleneck)
            |> List.sumBy (fun s -> s.OvertimeHours)

        let recoveryStrategy =
            if recoverable.IsEmpty then
                "No offload recovery required; baseline within normal operational margins"
            else
                $"Rebalance non-bottleneck production load across {recoverable.Length} candidate resources"

        { ActiveScheduleIds = schedIds
          Metrics =
            { TotalChangeoverHours = totalChangeover
              ResourceUtilization = avgUtilization
              OvertimeRequiredHours = totalOvertime
              ScheduleAdherenceProjection = adherenceProjection }
          Feasibility =
            { FeasibleCount = feasibleCount
              ConditionallyFeasibleCount = conditionallyCount
              InfeasibleCount = infeasibleCount
              TopConstraints = topConstraints |> List.distinct |> List.truncate 5 }
          Stability =
            { AverageStabilityScore = avgStability
              HighVolatilityResources = highVolatility }
          Risks =
            { ResourcesAtRiskOfDelay = delayRisks
              MaterialsAtRiskOfUnavailability = materialRisks
              BottleneckResources = bottlenecks
              OvertimeDependentSchedules = overtimeDependent }
          Recovery =
            { RecoverableResources = recoverable
              EstimatedRecoverableHours = recoverableHours
              RecoveryStrategy = recoveryStrategy } }

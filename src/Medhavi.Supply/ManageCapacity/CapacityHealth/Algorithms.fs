/// BA-S-013 — Determine Capacity Health Algorithm
/// Traces to: Supply Intelligence Specification §10 (BA-S-013)
module Medhavi.Supply.ManageCapacity.CapacityHealth.Algorithms

open System
open Medhavi.Common
open Medhavi.SemanticModel
open Medhavi.Supply
open Model
open Policies

/// Computes square root using Newton-Raphson for decimal precision
let sqrtDecimal (x: decimal) : decimal =
    if x <= 0.0m then 0.0m
    else
        let rec iterate (guess: decimal) (iter: int) =
            if iter >= 15 then guess
            else
                let next = 0.5m * (guess + (x / guess))
                if abs (next - guess) < 0.00001m then next
                else iterate next (iter + 1)
        iterate (x / 2.0m) 0

/// BA-S-013: Determine Capacity Health across scope
let determineHealth
    (scopeId: PlanningScopeId)
    (periodId: EvaluationPeriodId)
    (positions: CapacityPositionFact list)
    (timestamp: Timestamp)
    (policy: CapacityHealthPolicy)
    (versionNumber: int)
    : CapacityHealthAssessmentVersion =

    let totalCount = positions.Length
    if totalCount = 0 then
        {   VersionNumber = versionNumber
            State = HealthVersionLifecycleState.Published
            AssessmentScopeId = scopeId
            EvaluationPeriodId = periodId
            OverallCapacityHealthState = CapacityHealthState.Unassessed
            ClassificationRationale = "No active capacity position assessments found within planning scope"
            AssessmentConfidence = AssessmentConfidence.Low
            AverageUtilizationPercentage = 0.0m
            PeakUtilizationPercentage = 0.0m
            UnderutilizedResourceCount = 0
            OverloadedResourceCount = 0
            CapacityStability = "Unassessed"
            CapacityVolatility = 0.0m
            BindingConstraints = []
            BottleneckResources = []
            OverloadRiskExposureHours = 0.0m
            UnderloadCostExposureHours = 0.0m
            BottleneckImpactSummary = "None"
            ChronicOverloadResources = []
            ChronicUnderutilizedResources = []
            RecurringBottlenecks = []
            LoadBalancingOpportunities = []
            FlexibilityImprovementOpportunities = []
            EvidenceReferences = []
            TransactionTimestamp = timestamp
            PublicationTimestamp = Some timestamp }
    else
        let utilizations = positions |> List.map (fun p -> p.EffectiveUtilization)
        let avgUtil = List.average utilizations
        let peakUtil = List.max utilizations

        // Standard deviation and Coefficient of Variation
        let variance =
            utilizations
            |> List.map (fun u -> (u - avgUtil) * (u - avgUtil))
            |> List.average
        let stdDev = sqrtDecimal variance
        let cv = if avgUtil > 0.001m then stdDev / avgUtil else 0.0m

        let overloaded = positions |> List.filter (fun p -> p.UtilizationStatus = CapacityUtilizationStatus.Overloaded)
        let nearCapacity = positions |> List.filter (fun p -> p.UtilizationStatus = CapacityUtilizationStatus.NearCapacity)
        let underutilized = positions |> List.filter (fun p -> p.UtilizationStatus = CapacityUtilizationStatus.Underutilized)
        let activeConstraints = positions |> List.filter (fun p -> p.ConstraintStatus = CapacityConstraintStatus.ActiveConstraint)
        let bottlenecks = positions |> List.filter (fun p -> p.BottleneckStatus = BottleneckAssessmentStatus.CurrentBottleneck)
        let potentialBottlenecks = positions |> List.filter (fun p -> p.BottleneckStatus = BottleneckAssessmentStatus.PotentialBottleneck)
        let spofs = positions |> List.filter (fun p -> p.HasSinglePointOfFailureRisk)

        // Stability: total change events
        let totalChangeEvents = positions |> List.sumBy (fun p -> p.ChangeEventsCount)
        let stabilityDesc =
            if totalChangeEvents > policy.HighChangeEventsThreshold then "Unstable"
            elif totalChangeEvents > 2 then "Moderately Stable"
            else "Highly Stable"

        // Exposure hours
        let overloadHours =
            overloaded
            |> List.sumBy (fun p -> max 0.0m (p.CurrentCommittedHours + p.ProjectedPlannedHours - p.AvailableCapacityHours))

        let underloadHours =
            underutilized
            |> List.sumBy (fun p -> max 0.0m (p.AvailableCapacityHours - (p.CurrentCommittedHours + p.ProjectedPlannedHours)))

        // Precedence cascade (PO-S-050)
        let overallHealth, rationale =
            if overloaded.Length > 0 || bottlenecks.Length > 0 then
                CapacityHealthState.CriticallyConstrained,
                $"Critical capacity constraint detected: {overloaded.Length} overloaded resources and {bottlenecks.Length} current bottlenecks."
            elif activeConstraints.Length > 0 || spofs.Length > 0 then
                CapacityHealthState.Constrained,
                $"Active capacity constraints detected: {activeConstraints.Length} binding constraints, {spofs.Length} single point of failure risks."
            elif nearCapacity.Length > 0 || potentialBottlenecks.Length > 0 || cv > policy.HighVolatilityCvThreshold then
                CapacityHealthState.AtRisk,
                $"Elevated capacity risk: {nearCapacity.Length} resources near capacity, {potentialBottlenecks.Length} potential bottlenecks, CV={Math.Round(cv, 2)}."
            elif (decimal underutilized.Length / decimal totalCount) >= policy.UnderutilizedScopeRatioThreshold then
                CapacityHealthState.Underutilized,
                $"Pervasive underutilization: {underutilized.Length} of {totalCount} resources operating below minimum economic envelope."
            else
                CapacityHealthState.Balanced,
                $"Capacity posture is balanced across {totalCount} resources with average utilization of {Math.Round(avgUtil * 100.0m, 1)} percent."

        // Signals
        let chronicOverload = overloaded |> List.map (fun p -> p.ResourceId)
        let chronicUnder = underutilized |> List.map (fun p -> p.ResourceId)
        let recurringBottlenecks = bottlenecks |> List.map (fun p -> p.ResourceId)

        let loadBalancingOpportunities =
            if overloaded.Length > 0 && underutilized.Length > 0 then
                [ $"Opportunity to rebalance load from overloaded resources ({overloaded.Length}) to underutilized resources ({underutilized.Length})" ]
            else []

        let flexibilityRecommendations =
            spofs
            |> List.map (fun p -> $"Cross-train or qualify alternate resource for unique capability on {PhysicalResourceId.value p.ResourceId}")

        let bottleneckSummary =
            if bottlenecks.IsEmpty then "No active bottlenecks affecting dependent supply schedules"
            else $"{bottlenecks.Length} active bottlenecks: " + String.concat ", " (bottlenecks |> List.map (fun b -> PhysicalResourceId.value b.ResourceId))

        {   VersionNumber = versionNumber
            State = HealthVersionLifecycleState.Published
            AssessmentScopeId = scopeId
            EvaluationPeriodId = periodId
            OverallCapacityHealthState = overallHealth
            ClassificationRationale = rationale
            AssessmentConfidence = AssessmentConfidence.High
            AverageUtilizationPercentage = avgUtil * 100.0m
            PeakUtilizationPercentage = peakUtil * 100.0m
            UnderutilizedResourceCount = underutilized.Length
            OverloadedResourceCount = overloaded.Length
            CapacityStability = stabilityDesc
            CapacityVolatility = cv
            BindingConstraints = activeConstraints |> List.map (fun p -> p.ResourceId)
            BottleneckResources = bottlenecks |> List.map (fun p -> p.ResourceId)
            OverloadRiskExposureHours = overloadHours
            UnderloadCostExposureHours = underloadHours
            BottleneckImpactSummary = bottleneckSummary
            ChronicOverloadResources = chronicOverload
            ChronicUnderutilizedResources = chronicUnder
            RecurringBottlenecks = recurringBottlenecks
            LoadBalancingOpportunities = loadBalancingOpportunities
            FlexibilityImprovementOpportunities = flexibilityRecommendations
            EvidenceReferences = [ $"SE-S-040 x {totalCount}" ]
            TransactionTimestamp = timestamp
            PublicationTimestamp = Some timestamp }

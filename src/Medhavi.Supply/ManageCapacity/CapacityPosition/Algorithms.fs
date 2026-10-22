/// BA-S-010, BA-S-011, BA-S-012 — Business Algorithms for Capacity Position Understanding
/// Traces to: Supply Intelligence Specification §10 (BA-S-010, BA-S-011, BA-S-012)
module Medhavi.Supply.ManageCapacity.CapacityPosition.Algorithms

open System
open Medhavi.Supply
open Model
open Policies

/// Geometric interval union to combine overlapping or adjacent downtime windows without double counting
let mergeIntervals (intervals: (DateTimeOffset * DateTimeOffset) list) : (DateTimeOffset * DateTimeOffset) list =
    let sorted =
        intervals
        |> List.filter (fun (s, e) -> e > s)
        |> List.sortBy fst

    match sorted with
    | [] -> []
    | head :: tail ->
        let folder acc (currStart, currEnd) =
            match acc with
            | [] -> [(currStart, currEnd)]
            | (prevStart, prevEnd) :: rest ->
                if currStart <= prevEnd then
                    let mergedEnd = if currEnd > prevEnd then currEnd else prevEnd
                    (prevStart, mergedEnd) :: rest
                else
                    (currStart, currEnd) :: acc

        List.fold folder [head] tail
        |> List.rev

/// Total hours across disjoint time intervals
let calculateTotalHours (intervals: (DateTimeOffset * DateTimeOffset) list) : decimal =
    intervals
    |> List.sumBy (fun (s, e) -> decimal (e - s).TotalHours)

/// BA-S-010: Compute Available Capacity
/// Computes net physical available capacity by subtracting non-overlapping downtime from scheduled operating time.
let computeAvailableCapacity
    (scheduledHours: decimal)
    (outageIntervals: (DateTimeOffset * DateTimeOffset) list)
    (rcr: decimal)
    : {| AvailableHours: decimal; TotalDowntimeHours: decimal; OverDeductionHours: decimal |} =
    let mergedOutages = mergeIntervals outageIntervals
    let totalDowntime = calculateTotalHours mergedOutages
    let netHours = scheduledHours - totalDowntime
    let overDeduction = if netHours < 0.0m then -netHours else 0.0m
    let effectiveNetHours = if netHours < 0.0m then 0.0m else netHours
    let availableCapacity = effectiveNetHours * rcr
    {|  AvailableHours = availableCapacity
        TotalDowntimeHours = totalDowntime
        OverDeductionHours = overDeduction |}

/// BA-S-011: Compute Capacity Operating Envelope
/// Computes the 6-tier operating envelope from available capacity
let computeOperatingEnvelope
    (availableCapacity: decimal)
    (policy: CapacityManagementPolicy)
    : CapacityOperatingEnvelope =
    {   MinEconomicCapacity = availableCapacity * policy.MinEconomicRatio
        TargetLowCapacity = availableCapacity * policy.TargetLowUtilization
        TargetHighCapacity = availableCapacity * policy.TargetHighUtilization
        OvertimeCapacity = availableCapacity * policy.OvertimeRatio
        MaxPlannedCapacity = availableCapacity * policy.MaxPlannedRatio
        AbsolutePhysicalCapacity = availableCapacity * policy.AbsoluteRatio }

/// BA-S-012: Evaluate Capacity Exposure
/// Evaluates exposure across 6 orthogonal dimensions (Overload, Constraint, Bottleneck, Flexibility, SPOF, Policy Conformance)
let evaluateCapacityExposure
    (availableHours: decimal)
    (totalPlannedLoadHours: decimal)
    (downstreamJobsCount: int)
    (hasAlternateResource: bool)
    (isUniqueCapability: bool)
    (envelope: CapacityOperatingEnvelope)
    (policy: CapacityManagementPolicy)
    : {|    EffectiveUtilization: decimal
            UtilizationStatus: CapacityUtilizationStatus
            ConstraintStatus: CapacityConstraintStatus
            BottleneckStatus: BottleneckAssessmentStatus
            RiskLevel: CapacityRiskLevel
            PolicyCompliance: PolicyComplianceStatus
            HasSinglePointOfFailure: bool |} =
    
    let safeAvailable = if availableHours <= 0.0m then 0.0001m else availableHours
    let utilization = totalPlannedLoadHours / safeAvailable

    // Dimension 1: Overload & Utilization
    let utilizationStatus =
        if utilization > policy.OverloadThreshold then
            CapacityUtilizationStatus.Overloaded
        elif utilization >= policy.NearCapacityThreshold then
            CapacityUtilizationStatus.NearCapacity
        elif utilization < policy.TargetLowUtilization then
            CapacityUtilizationStatus.Underutilized
        else
            CapacityUtilizationStatus.Optimal

    // Dimension 2: Constraint Status
    let constraintStatus =
        if utilization >= policy.ActiveConstraintThreshold then
            CapacityConstraintStatus.ActiveConstraint
        elif utilization >= policy.NearConstraintThreshold then
            CapacityConstraintStatus.NearConstraint
        else
            CapacityConstraintStatus.NonConstraint

    // Dimension 3: Bottleneck Status (requires both high load & dependent downstream jobs)
    let bottleneckStatus =
        if utilization >= policy.BottleneckThreshold && downstreamJobsCount > 0 then
            BottleneckAssessmentStatus.CurrentBottleneck
        elif utilization >= policy.NearConstraintThreshold && downstreamJobsCount > 0 then
            BottleneckAssessmentStatus.PotentialBottleneck
        else
            BottleneckAssessmentStatus.NotABottleneck

    // Dimension 5: Single Point of Failure (SPOF)
    let hasSpof = (not hasAlternateResource) && isUniqueCapability && utilization >= policy.NearConstraintThreshold

    // Dimension 6: Policy Conformance
    let policyCompliance =
        if totalPlannedLoadHours > envelope.MaxPlannedCapacity then
            PolicyComplianceStatus.NonConforming ["Planned load exceeds governed Maximum Planned Capacity envelope"]
        else
            PolicyComplianceStatus.Conforming

    // Synthesize Multi-Dimensional Risk Level
    let riskLevel =
        if utilizationStatus = CapacityUtilizationStatus.Overloaded 
            || bottleneckStatus = BottleneckAssessmentStatus.CurrentBottleneck
            || (hasSpof && constraintStatus = CapacityConstraintStatus.ActiveConstraint) then
            CapacityRiskLevel.Critical
        elif constraintStatus = CapacityConstraintStatus.ActiveConstraint
                || bottleneckStatus = BottleneckAssessmentStatus.PotentialBottleneck
                || hasSpof then
            CapacityRiskLevel.High
        elif utilizationStatus = CapacityUtilizationStatus.NearCapacity
                || constraintStatus = CapacityConstraintStatus.NearConstraint
                || utilizationStatus = CapacityUtilizationStatus.Underutilized then
            CapacityRiskLevel.Medium
        else
            CapacityRiskLevel.Low

    {|  EffectiveUtilization = utilization
        UtilizationStatus = utilizationStatus
        ConstraintStatus = constraintStatus
        BottleneckStatus = bottleneckStatus
        RiskLevel = riskLevel
        PolicyCompliance = policyCompliance
        HasSinglePointOfFailure = hasSpof |}

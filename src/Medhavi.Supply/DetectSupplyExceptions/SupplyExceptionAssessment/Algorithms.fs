module Medhavi.Supply.DetectSupplyExceptions.SupplyExceptionAssessment.Algorithms

open System
open Medhavi.Foundation.Failure
open Medhavi.SemanticModel
open Medhavi.Supply
open Model
open Policies

// =============================================================================
// BA-S-080 — Evaluate Supply Exception
// Stage: State, Output Type: State, Explainability: Full
// Evaluates operational signals, change assessments, and quality evaluations
// against governed criteria to determine exception existence, severity, horizon,
// and contextual correlation. Pure deterministic calculation.
// =============================================================================

[<RequireQualifiedAccess>]
type ExceptionDetermination =
    | ExceptionExists of Type: ExceptionType * PrimaryReason: string
    | NoException of Reason: string

type ExceptionAssessmentResult =
    { Determination: ExceptionDetermination
      Severity: SeverityAssessment
      Horizon: ExceptionHorizon
      Confidence: ExceptionConfidence
      ReasonCodes: string list
      EvidenceLineage: EvidenceReference list
      CorrelatedExceptionIds: SupplyExceptionId list
      Timestamp: Timestamp }

/// Stage 1: Filter and bind evidence to evaluated scope
let filterEvidenceForScope (scope: ExceptionScope) (changes: SupplyChangeEvidenceFact list) (positions: OperationalPositionEvidenceFact list) =
    let (_, entityId) = ExceptionScope.entityTypeAndId scope
    let matchingChanges = changes |> List.filter (fun c -> c.EntityId = entityId || entityId = "Global")
    let matchingPositions = positions |> List.filter (fun p -> p.EntityId = entityId || entityId = "Global")
    (matchingChanges, matchingPositions)

/// Stage 2: Evaluate Triggering Predicates per PO-S-120
let evaluatePredicates
    (policy: ExceptionDetectionPolicy)
    (changes: SupplyChangeEvidenceFact list)
    (qualityOpt: QualityEvidenceFact option)
    (positions: OperationalPositionEvidenceFact list)
    : (ExceptionType * string * decimal) list =
    let results = ResizeArray<ExceptionType * string * decimal>()

    // 1. Supply Disruption Predicate
    for c in changes do
        if c.StoppageConfirmed then
            results.Add((ExceptionType.SupplyDisruption, "SupplierStoppageConfirmed", c.Confidence))
        elif c.Materiality = "MaterialToPlanning" && c.DaysVariance >= policy.SupplyDisruptionCriteria.StoppageDaysThreshold then
            results.Add((ExceptionType.SupplyDisruption, "CriticalPathDeliveryDelayed", c.Confidence))

    // 2. Service Policy Violation Predicate
    match qualityOpt with
    | Some q ->
        if q.ConsecutiveBreachCount >= policy.ServiceLevelCriteria.ConsecutivePeriodBreachThreshold then
            results.Add((ExceptionType.ServicePolicyViolation, "ServicePolicyViolationSustained", 0.90m))
        elif q.LowestServiceLevel < policy.ServiceLevelCriteria.CriticalSinglePeriodServiceLevelPercent then
            results.Add((ExceptionType.ServicePolicyViolation, "ServicePolicyViolationCritical", 0.85m))
    | None -> ()

    // 3. Operational Positions: Inventory, Capacity, Supplier
    for pos in positions do
        // Capacity Policy Violation
        if pos.ResourceUtilization > policy.CapacityCriteria.UtilizationCapThreshold
           && pos.ConsecutiveUnderperformanceMonths >= policy.CapacityCriteria.ConsecutiveOverloadPeriodsThreshold then
            results.Add((ExceptionType.CapacityPolicyViolation, "CapacityOperatingEnvelopeBreached", 0.85m))

        // Inventory Policy Violation
        if pos.DaysOfSupply <= 0.0m || pos.OnHand < pos.SafetyStock then
            results.Add((ExceptionType.InventoryPolicyViolation, "ProjectedStockoutMaterial", 0.90m))

        // Supplier Collaboration Breach
        if pos.SupplierOtifPercent < policy.SupplierCollaborationCriteria.OtifThresholdPercent
           && pos.ConsecutiveUnderperformanceMonths >= policy.SupplierCollaborationCriteria.ConsecutiveUnderperformingMonthsThreshold then
            results.Add((ExceptionType.SupplierCollaborationBreach, "SupplierOTIFBreachSustained", 0.85m))

    // Precedence sorting: Disruption > ExternalEvent > ServicePolicyViolation > Inventory > Capacity > Supplier > EmergingRisk
    let typePrecedence (t: ExceptionType) =
        match t with
        | ExceptionType.SupplyDisruption -> 1
        | ExceptionType.ExternalEvent -> 2
        | ExceptionType.ServicePolicyViolation -> 3
        | ExceptionType.InventoryPolicyViolation -> 4
        | ExceptionType.CapacityPolicyViolation -> 5
        | ExceptionType.SupplierCollaborationBreach -> 6
        | ExceptionType.EmergingRisk -> 7

    results
    |> Seq.toList
    |> List.sortBy (fun (t, _, _) -> typePrecedence t)

/// Stage 3: Multi-Dimensional Severity Vector Assessment
let evaluateSeverity
    (scope: ExceptionScope)
    (exType: ExceptionType)
    (changes: SupplyChangeEvidenceFact list)
    (positions: OperationalPositionEvidenceFact list)
    : SeverityAssessment =
    // Business Impact
    let businessImpact =
        let isCritical = positions |> List.exists (fun p -> p.IsCriticalEntity)
        let hasStoppage = changes |> List.exists (fun c -> c.StoppageConfirmed)
        if hasStoppage && isCritical then SeverityBusinessImpact.Severe
        elif hasStoppage || isCritical then SeverityBusinessImpact.Major
        elif exType = ExceptionType.ServicePolicyViolation then SeverityBusinessImpact.Moderate
        elif exType = ExceptionType.InventoryPolicyViolation then SeverityBusinessImpact.Moderate
        else SeverityBusinessImpact.Minor

    // Urgency
    let urgency =
        let hasZeroBuffer = positions |> List.exists (fun p -> p.DaysOfSupply <= 0.0m)
        let maxDelayDays = changes |> List.map (fun c -> c.DaysVariance) |> List.fold max 0.0m
        if hasZeroBuffer || maxDelayDays >= 7.0m then SeverityUrgency.Immediate
        elif maxDelayDays >= 3.0m then SeverityUrgency.ShortTerm
        else SeverityUrgency.MediumTerm

    // Scope
    let severityScope =
        match scope with
        | ExceptionScope.ItemScope _ -> SeverityScope.SingleItem
        | ExceptionScope.LocationScope _ -> SeverityScope.Location
        | ExceptionScope.SupplierScope _ -> SeverityScope.Supplier
        | ExceptionScope.ResourceScope _ -> SeverityScope.Resource
        | ExceptionScope.PlanningScope _ -> SeverityScope.BusinessUnit
        | ExceptionScope.EnterpriseScope -> SeverityScope.Enterprise

    // Potential Impact
    let potentialImpact =
        match businessImpact with
        | SeverityBusinessImpact.Severe -> SeverityPotentialImpact.Severe
        | SeverityBusinessImpact.Major -> SeverityPotentialImpact.Major
        | SeverityBusinessImpact.Moderate -> SeverityPotentialImpact.Moderate
        | SeverityBusinessImpact.Minor -> SeverityPotentialImpact.Minor
        | SeverityBusinessImpact.Negligible -> SeverityPotentialImpact.Negligible

    // Current Impact
    let currentImpact =
        let hasRealizedDisruption = changes |> List.exists (fun c -> c.DaysVariance > 0.0m)
        if hasRealizedDisruption then SeverityCurrentImpact.PartiallyMaterialized
        else SeverityCurrentImpact.Buffered

    { BusinessImpact = businessImpact
      Urgency = urgency
      Scope = severityScope
      PotentialImpact = potentialImpact
      CurrentImpact = currentImpact }

/// Stage 4: Horizon Classification & Correlation Clustering
let evaluateHorizonAndCorrelations
    (policy: ExceptionDetectionPolicy)
    (scope: ExceptionScope)
    (changes: SupplyChangeEvidenceFact list)
    (activeExceptions: ActiveExceptionFact list)
    : (ExceptionHorizon * SupplyExceptionId list) =
    let minDelay = changes |> List.map (fun c -> c.DaysVariance) |> List.fold min 999.0m
    let horizon =
        if minDelay <= (policy.HorizonThresholds.ImmediateMaxHours / 24.0m) then ExceptionHorizon.Immediate
        elif minDelay <= policy.HorizonThresholds.ShortTermMaxDays then ExceptionHorizon.ShortTerm
        elif minDelay <= policy.HorizonThresholds.MediumTermMaxDays then ExceptionHorizon.MediumTerm
        elif minDelay < 999.0m then ExceptionHorizon.Future
        else ExceptionHorizon.Uncertain

    let (_, entityId) = ExceptionScope.entityTypeAndId scope
    let correlatedIds =
        activeExceptions
        |> List.filter (fun ae -> ae.ScopeEntityId = entityId || ae.ScopeEntityType = "PlanningScope")
        |> List.choose (fun ae ->
            match SupplyExceptionId.create ae.ExceptionId with
            | Ok id -> Some id
            | Error _ -> None)

    (horizon, correlatedIds)

/// Stage 5: Confidence Derivation, Reason Coding & Packaging
let evaluateSupplyException
    (policy: ExceptionDetectionPolicy)
    (scope: ExceptionScope)
    (changes: SupplyChangeEvidenceFact list)
    (qualityOpt: QualityEvidenceFact option)
    (positions: OperationalPositionEvidenceFact list)
    (activeExceptions: ActiveExceptionFact list)
    (timestamp: Timestamp)
    : ExceptionAssessmentResult =
    let (boundChanges, boundPositions) = filterEvidenceForScope scope changes positions
    let candidateViolations = evaluatePredicates policy boundChanges qualityOpt boundPositions

    match candidateViolations with
    | [] ->
        let zeroSeverity =
            { BusinessImpact = SeverityBusinessImpact.Negligible
              Urgency = SeverityUrgency.Low
              Scope = SeverityScope.SingleItem
              PotentialImpact = SeverityPotentialImpact.Negligible
              CurrentImpact = SeverityCurrentImpact.None }
        { Determination = ExceptionDetermination.NoException "InsufficientEvidenceForException"
          Severity = zeroSeverity
          Horizon = ExceptionHorizon.Uncertain
          Confidence = ExceptionConfidence.Low
          ReasonCodes = [ "InsufficientEvidenceForException" ]
          EvidenceLineage = []
          CorrelatedExceptionIds = []
          Timestamp = timestamp }
    | (primaryType, primaryReason, avgConfidence) :: _ ->
        let severity = evaluateSeverity scope primaryType boundChanges boundPositions
        let (horizon, correlations) = evaluateHorizonAndCorrelations policy scope boundChanges activeExceptions

        let confidenceLevel =
            if avgConfidence >= 0.85m && (boundChanges.Length + boundPositions.Length) >= 2 then
                ExceptionConfidence.High
            elif avgConfidence >= 0.60m then
                ExceptionConfidence.Medium
            else
                ExceptionConfidence.Low

        let evidenceReferences =
            let changeRefs =
                boundChanges
                |> List.map (fun c ->
                    { AssessmentId = c.AssessmentId
                      AssessmentType = "SE-S-090 SupplyChangeAssessment"
                      Description = sprintf "%s variance %M days" c.ChangeType c.DaysVariance
                      Confidence = c.Confidence
                      Timestamp = c.OccurrenceTime })
            let qualityRefs =
                match qualityOpt with
                | Some q ->
                    [ { AssessmentId = q.AssessmentId
                        AssessmentType = "SE-S-100 SupplyQualityAssessment"
                        Description = sprintf "Quality score %M, consecutive breaches: %d" q.OverallScore q.ConsecutiveBreachCount
                        Confidence = 0.90m
                        Timestamp = timestamp } ]
                | None -> []
            changeRefs @ qualityRefs

        let reasonCodes =
            candidateViolations
            |> List.map (fun (_, reason, _) -> reason)
            |> List.distinct

        { Determination = ExceptionDetermination.ExceptionExists(primaryType, primaryReason)
          Severity = severity
          Horizon = horizon
          Confidence = confidenceLevel
          ReasonCodes = reasonCodes
          EvidenceLineage = evidenceReferences
          CorrelatedExceptionIds = correlations
          Timestamp = timestamp }

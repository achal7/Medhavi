module Medhavi.Supply.ProcureMaterials.ProcurementRecommendation.Algorithms

open System
open System.Security.Cryptography
open System.Text
open Medhavi.SemanticModel
open Medhavi.Supply
open Model

/// BA-S-030 — Evaluate Procurement Requirement
/// Transforms net gap into commercially grounded procurement requirement with MOQ, packaging multiples, and back-scheduling
let evaluateProcurementRequirement
    (requirementId: ProcurementRequirementId)
    (itemId: ItemId)
    (locationId: LocationId)
    (netGapQuantity: decimal)
    (requiredDeliveryDate: Timestamp)
    (evaluationTimestamp: Timestamp)
    (purchasingParams: ItemPurchasingParametersFact)
    : EvaluatedProcurementRequirement =

    let moq = max 0.0m purchasingParams.MOQ

    let batchMultiple =
        if purchasingParams.PackagingMultiple > 0.0m then
            purchasingParams.PackagingMultiple
        else
            1.0m

    let qBase = max moq netGapQuantity

    let orderBatches = Math.Ceiling(qBase / batchMultiple)
    let qOrd = orderBatches * batchMultiple
    let qExcess = max 0.0m (qOrd - netGapQuantity)
    let rhoExcess = if qOrd > 0.0m then qExcess / qOrd else 0.0m

    let totalLeadTimeDays =
        purchasingParams.PurchasingLeadTimeDays
        + purchasingParams.InboundTransitLeadTimeDays
        + purchasingParams.QualityInspectionBufferDays

    let releaseDateDt = (Timestamp.value requiredDeliveryDate).AddDays(-float totalLeadTimeDays)
    let calculatedReleaseDate = Timestamp.ofUtc releaseDateDt

    let slackDuration = releaseDateDt - (Timestamp.value evaluationTimestamp)
    let slackDays = decimal slackDuration.TotalDays

    let leadTimeDeficitDays = if slackDays < 0.0m then Math.Abs(slackDays) else 0.0m

    let graceDays = purchasingParams.GracePeriodDays

    let priority =
        if
            slackDays < -graceDays || (Timestamp.value requiredDeliveryDate) <= (Timestamp.value evaluationTimestamp)
        then
            RequirementPriority.CriticalShortage
        elif slackDays < 0.0m then
            RequirementPriority.Expedited
        else
            RequirementPriority.Standard

    let reasonCodes =
        [
            yield "NetShortfallReplenishment"
            if qExcess > 0.0m then
                yield "MOQInducedExcessStock"
            if batchMultiple > 1.0m && qOrd > qBase then
                yield "PackagingMultipleRounding"
            if slackDays < 0.0m then
                yield "LeadTimeCompressionRequired"
            else
                yield "SourcingLeadTimeFeasible"
            if priority = RequirementPriority.CriticalShortage then
                yield "CriticalShortagePriorityAssigned"
        ]

    {
        RequirementId = requirementId
        ItemId = itemId
        LocationId = locationId
        NetGapQuantity = netGapQuantity
        RecommendedOrderQuantity = qOrd
        InducedExcessQuantity = qExcess
        InducedExcessRatio = rhoExcess
        RequiredDeliveryDate = requiredDeliveryDate
        CalculatedReleaseDate = calculatedReleaseDate
        LeadTimeDeficitDays = leadTimeDeficitDays
        SlackDays = slackDays
        Priority = priority
        ReasonCodes = reasonCodes
        EvaluationTimestamp = evaluationTimestamp
    }

/// BA-S-031 — Assess Sourcing Situation
/// Evaluates sourcing context across 4 sequential dimensions: Supply Ecosystem, Enterprise Feasibility, Risk Landscape, Opportunity Landscape
let assessSourcingSituation
    (evaluatedReq: EvaluatedProcurementRequirement)
    (candidates: SupplierCandidateFact list)
    (healthStates: Map<SupplierId, SupplierCollaborationHealthState>)
    (activeDisruptions: string list)
    (futureNetGapsQuantity: decimal)
    (policy: ProcurementPolicyConfig)
    (isDualSourcingMandatory: bool)
    : SourcingSituationAssessment =

    // Dimension 1: Supply Ecosystem
    let approvedSuppliers = candidates |> List.filter(fun c -> c.IsApproved && c.IsCompliant)

    let supplierCount = approvedSuppliers.Length
    let aggregateCapacity = approvedSuppliers |> List.sumBy(fun c -> c.AvailableCapacity)

    let getHealthCount (predicate: SupplierCollaborationHealthState -> bool) =
        approvedSuppliers
        |> List.filter(fun s ->
            match Map.tryFind s.SupplierId healthStates with
            | Some st -> predicate st
            | None -> false)
        |> List.length

    let healthyCount = getHealthCount((=) SupplierCollaborationHealthState.Healthy)
    let atRiskCount = getHealthCount((=) SupplierCollaborationHealthState.AtRisk)
    let underperfCount = getHealthCount((=) SupplierCollaborationHealthState.Underperforming)
    let critCount = getHealthCount((=) SupplierCollaborationHealthState.CriticallyDependent)
    let unassessedCount = supplierCount - (healthyCount + atRiskCount + underperfCount + critCount)

    let toRatio count = if supplierCount > 0 then decimal count / decimal supplierCount else 0.0m

    let hHealthy = toRatio healthyCount
    let hAtRisk = toRatio atRiskCount
    let hUnderperf = toRatio underperfCount
    let hCritical = toRatio critCount
    let hUnassessed = toRatio unassessedCount

    let hhi =
        if aggregateCapacity > 0.0m && supplierCount > 0 then
            approvedSuppliers
            |> List.sumBy(fun s ->
                let share = (s.AvailableCapacity / aggregateCapacity) * 100.0m
                share * share)
            |> int
        else
            10000

    let marketConcentration =
        if hhi < 1500 && supplierCount >= 3 then "Diversified"
        elif hhi <= 2500 || supplierCount = 2 then "ModeratelyConcentrated"
        else "HighlyConcentrated"

    let ecosystemSummary =
        {
            ApprovedSupplierCount = supplierCount
            AggregateCapacity = aggregateCapacity
            HealthyRatio = hHealthy
            AtRiskRatio = hAtRisk
            UnderperformingRatio = hUnderperf
            CriticalRatio = hCritical
            UnassessedRatio = hUnassessed
            HerfindahlIndex = hhi
            MarketConcentration = marketConcentration
        }

    // Dimension 2: Enterprise Feasibility
    let phiCap = aggregateCapacity >= evaluatedReq.RecommendedOrderQuantity

    let isLeadTimeFeasible (s: SupplierCandidateFact) =
        let totalLead = s.PurchasingLeadTimeDays + s.InboundTransitLeadTimeDays
        let earliestDeliveryDt = (Timestamp.value evaluatedReq.EvaluationTimestamp).AddDays(float totalLead)
        earliestDeliveryDt <= (Timestamp.value evaluatedReq.RequiredDeliveryDate)

    let leadTimeFeasibleSuppliers = approvedSuppliers |> List.filter isLeadTimeFeasible

    let phiTime = not(List.isEmpty leadTimeFeasibleSuppliers)
    let phiDual = not isDualSourcingMandatory || supplierCount >= 2

    let feasibility =
        if supplierCount = 0 then
            ProcurementFeasibility.Infeasible "Zero approved suppliers available"
        elif not phiCap then
            ProcurementFeasibility.Infeasible
                $"Aggregate capacity insufficient: {aggregateCapacity} < {evaluatedReq.RecommendedOrderQuantity}"
        elif not phiTime && evaluatedReq.SlackDays < -policy.GracePeriodDays then
            ProcurementFeasibility.Infeasible "Lead time infeasible and expedition tolerance exceeded"
        elif not phiDual && phiCap then
            ProcurementFeasibility.ConditionallyFeasible [ "Dual-sourcing policy violated; single vendor only" ]
        elif not phiTime then
            ProcurementFeasibility.ConditionallyFeasible [ "Lead time compressed; requires expedited freight" ]
        else
            ProcurementFeasibility.Feasible

    // Dimension 3: Risk Landscape
    let rDisrupt = min 1.0m (decimal activeDisruptions.Length * 0.50m)

    let rHealth = 1.0m * hCritical + 0.8m * hUnderperf + 0.4m * hAtRisk + 0.5m * hUnassessed

    let rSingle =
        if supplierCount <= 1 then 1.0m
        elif supplierCount = 2 then 0.5m
        else 0.0m

    let compositeRiskScore = 100.0m * (0.40m * rHealth + 0.35m * rDisrupt + 0.25m * rSingle)

    let identifiedRisks =
        [
            if supplierCount <= 1 then
                "Single approved supplier risk"
            if hCritical > 0.0m || hUnderperf > 0.0m then
                "Key supplier performance degraded"
            if not (List.isEmpty activeDisruptions) then
                $"Active supply disruptions detected ({activeDisruptions.Length})"
            if not phiTime then
                "Lead time compression required"
        ]

    // Dimension 4: Opportunity Landscape
    let totalBundleQty = evaluatedReq.RecommendedOrderQuantity + futureNetGapsQuantity

    let bundlingAdvantage =
        totalBundleQty >= policy.TierBreakQuantity && totalBundleQty > (1.5m * evaluatedReq.RecommendedOrderQuantity)

    let highBuyerLeverage = aggregateCapacity >= (2.5m * evaluatedReq.RecommendedOrderQuantity) && supplierCount >= 2

    let identifiedOpps =
        [
            if bundlingAdvantage then
                "Volume discount tier break opportunity via order bundling"
            if highBuyerLeverage then
                "Favorable buyer negotiation leverage due to excess market capacity"
        ]

    let reasonCodes =
        [
            if supplierCount >= 3 && hhi < 1500 then
                yield "DiverseSupplyBase"
            elif supplierCount = 2 then
                yield "NarrowSupplyBase"
            elif supplierCount = 1 then
                yield "SingleSourceOnly"
            match feasibility with
            | ProcurementFeasibility.Feasible -> yield "FeasibleSolutionExists"
            | ProcurementFeasibility.ConditionallyFeasible _ -> yield "FeasibilityConstrained"
            | ProcurementFeasibility.Infeasible _ -> yield "CapacityInsufficient"
            if rHealth >= 0.5m then
                yield "KeySupplierAtRisk"
            if activeDisruptions.Length > 0 then
                yield "DisruptionSignalActive"
            if bundlingAdvantage then
                yield "VolumeBundlingOpportunity"
        ]

    let confidence =
        if compositeRiskScore <= policy.MaxRiskScoreForAutonomous && supplierCount >= 2 && unassessedCount = 0 then
            RecommendationConfidence.High
        elif compositeRiskScore <= 60.0m && supplierCount >= 1 then
            RecommendationConfidence.Medium
        else
            RecommendationConfidence.Low

    {
        RequirementId = evaluatedReq.RequirementId
        Ecosystem = ecosystemSummary
        Feasibility = feasibility
        CompositeRiskScore = compositeRiskScore
        DisruptionRiskIntensity = rDisrupt
        SingleSourceRisk = rSingle
        VolumeBundlingAdvantage = bundlingAdvantage
        HighBuyerLeverage = highBuyerLeverage
        IdentifiedRisks = identifiedRisks
        IdentifiedOpportunities = identifiedOpps
        ReasonCodes = reasonCodes
        OverallConfidence = confidence
        EvaluationTimestamp = evaluatedReq.EvaluationTimestamp
    }

/// BA-S-032 — Determine Enterprise Sourcing Determination
/// Derives canonical governance quadruple (N, I, G, D) based on situation assessment and policy
let determineEnterpriseSourcingDetermination
    (evaluatedReq: EvaluatedProcurementRequirement)
    (situation: SourcingSituationAssessment)
    (unitCost: decimal)
    (policy: ProcurementPolicyConfig)
    (isDualSourcingMandatory: bool)
    : EnterpriseSourcingDetermination =

    let totalSpend = evaluatedReq.RecommendedOrderQuantity * unitCost

    // 1. Sourcing Necessity (N)
    let necessity =
        match evaluatedReq.Priority with
        | RequirementPriority.CriticalShortage
        | RequirementPriority.Expedited -> SourcingNecessity.MandatoryShortfall
        | RequirementPriority.Standard ->
            if evaluatedReq.InducedExcessRatio > 0.5m then
                SourcingNecessity.DiscretionaryOptimization
            else
                SourcingNecessity.MandatoryShortfall

    // 2. Sourcing Intent (I)
    let intent =
        if
            evaluatedReq.Priority = RequirementPriority.CriticalShortage
            || evaluatedReq.Priority = RequirementPriority.Expedited
            || evaluatedReq.LeadTimeDeficitDays > 0.0m
        then
            SourcingIntent.ExpeditedIntervention
        elif
            isDualSourcingMandatory
            && situation.Ecosystem.ApprovedSupplierCount >= 2
            && (situation.CompositeRiskScore >= 40.0m || situation.DisruptionRiskIntensity > 0.0m)
        then
            SourcingIntent.DualSourceRiskMitigation
        elif situation.Ecosystem.ApprovedSupplierCount = 1 then
            SourcingIntent.SoleSourcePreservation
        else
            SourcingIntent.StandardReplenishment

    // 3. Governance Posture (G)
    let isInfeasible =
        match situation.Feasibility with
        | ProcurementFeasibility.Infeasible _ -> true
        | _ -> false

    let isConstrained =
        match situation.Feasibility with
        | ProcurementFeasibility.ConditionallyFeasible _ -> true
        | _ -> false

    let governance =
        if
            isInfeasible || totalSpend >= policy.ExecutiveEscalationSpendLimit || situation.CompositeRiskScore >= 80.0m
        then
            GovernancePosture.ExecutiveEscalationRequired
        elif
            isConstrained
            || totalSpend > policy.AutoApprovalSpendLimit
            || situation.CompositeRiskScore > policy.MaxRiskScoreForAutonomous
            || evaluatedReq.InducedExcessRatio > policy.MaxInducedExcessRatioForAutonomous
            || intent = SourcingIntent.ExpeditedIntervention
            || situation.DisruptionRiskIntensity > 0.0m
        then
            GovernancePosture.PlannerApprovalMandatory
        else
            GovernancePosture.AutonomousExecutionPermitted

    // 4. Sourcing Directive (D)
    let directive =
        if governance = GovernancePosture.ExecutiveEscalationRequired || isInfeasible then
            let reason =
                match situation.Feasibility with
                | ProcurementFeasibility.Infeasible r -> r
                | _ ->
                    if totalSpend >= policy.ExecutiveEscalationSpendLimit then
                        "Spend exceeds executive limit"
                    else
                        "Composite risk exceeds allowable bounds"

            SourcingDirective.HaltAndEscalate reason
        elif
            governance <> GovernancePosture.ExecutiveEscalationRequired
            && situation.VolumeBundlingAdvantage
            && evaluatedReq.Priority = RequirementPriority.Standard
            && evaluatedReq.SlackDays >= policy.HoldingHorizonDays
        then
            SourcingDirective.HoldForConsolidation
        else
            SourcingDirective.ExecuteProcurementRecommendation

    let rawKey =
        $"{ProcurementRequirementId.value evaluatedReq.RequirementId}#{Timestamp.value evaluatedReq.EvaluationTimestamp:O}"

    use sha = SHA256.Create()
    let hashBytes = sha.ComputeHash(Encoding.UTF8.GetBytes(rawKey))
    let hash64 = Convert.ToHexString(hashBytes).Substring(0, 16)
    let determinationId = $"ESD-{hash64}"

    let rationale =
        sprintf
            "Necessity: %A, Intent: %A, Governance: %A, Directive: %A. Spend: %M, CompositeRisk: %M, ExcessRatio: %M"
            necessity
            intent
            governance
            directive
            totalSpend
            situation.CompositeRiskScore
            evaluatedReq.InducedExcessRatio

    let triggeringRule =
        match governance with
        | GovernancePosture.ExecutiveEscalationRequired -> "PO-S-070-R1-ExecutiveEscalation"
        | GovernancePosture.PlannerApprovalMandatory -> "PO-S-070-R2-PlannerApproval"
        | GovernancePosture.AutonomousExecutionPermitted -> "PO-S-070-R3-AutonomousExecution"

    {
        DeterminationId = determinationId
        Necessity = necessity
        Intent = intent
        Governance = governance
        Directive = directive
        TriggeringPolicyRuleId = triggeringRule
        Rationale = rationale
        ReasonCodes = situation.ReasonCodes
        EffectiveTimestamp = evaluatedReq.EvaluationTimestamp
    }

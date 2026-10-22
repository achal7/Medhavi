/// AB-S-051: Publish Procurement Plan
/// Traces to: Supply Intelligence Specification §4.3.1 (AB-S-051, EV-S-051)
module Medhavi.Supply.ProcureMaterials.ProcurementPlan.Behaviors

open Medhavi.Common
open Medhavi.Foundation.Contracts
open Medhavi.Foundation.Contracts.Decision
open Medhavi.Foundation.Failure
open Medhavi.SemanticModel
open Medhavi.Supply
open Model
open Decisions

type ProcurementPlanFactsProvider = PlanningScopeId -> RecommendationSummaryFact list * int

let publishPlan
    (factsProvider: ProcurementPlanFactsProvider)
    (policy: PlanPublicationPolicyConfig)
    : Decide<ProcurementPlan, PublishProcurementPlanCmd, ProcurementPlanEvent> =
    fun (cmd: PublishProcurementPlanCmd) (existingPlan: ProcurementPlan option) ->
        result {
            // Step 1: Validate Identity
            do! Rules.validateIdentity cmd.PlanningScopeId cmd.PlanningCycleId

            // Step 2: Fetch Active Recommendations
            let recommendations, totalActiveReqs = factsProvider cmd.PlanningScopeId

            // Step 3: Validate Publication Eligibility
            do!
                Rules.validatePublicationEligibility
                    recommendations.Length
                    cmd.PublicationAuthority
                    cmd.PublicationTimestamp

            // Step 4: Check Completeness
            let coveredReqsCount = recommendations.Length

            let isComplete =
                match Rules.validateCompleteness totalActiveReqs coveredReqsCount policy with
                | Ok c -> c
                | Error _ -> false

            // Step 5: Compose Line Items from Recommendations
            let lineItems =
                recommendations
                |> List.collect(fun r ->
                    r.Allocations
                    |> List.map(fun a ->
                        let cost = (Quantity.value a.Quantity) * (defaultArg a.EstimatedUnitCost 1.0m)

                        {
                            RequirementId = r.RequirementId
                            ItemId = r.ItemId
                            LocationId = r.LocationId
                            SupplierId = a.SupplierId
                            DeliveryDate = a.ExpectedDeliveryDate
                            OrderReleaseDate = a.OrderReleaseDate
                            OrderQuantity = a.Quantity
                            EstimatedCost = cost
                            Confidence = r.OverallConfidence
                        }))

            // Step 6: Compute Aggregate Metrics
            let totalSpend = lineItems |> List.sumBy(fun l -> l.EstimatedCost)

            let supplierSpend =
                lineItems
                |> List.groupBy(fun l -> l.SupplierId)
                |> List.map(fun (sup, items) ->
                    let spend = items |> List.sumBy(fun l -> l.EstimatedCost)
                    let pct = if totalSpend > 0.0m then (spend / totalSpend) * 100.0m else 0.0m
                    sup, pct)

            let confidenceDist =
                recommendations
                |> List.groupBy(fun r -> r.OverallConfidence)
                |> List.map(fun (conf, items) -> conf, items.Length)

            let feasibilitySummary =
                recommendations |> List.groupBy(fun r -> r.Feasibility) |> List.map(fun (f, items) -> f, items.Length)

            let metrics =
                {
                    TotalSpend = totalSpend
                    SupplierAllocationPercentages = supplierSpend
                    ConfidenceDistribution = confidenceDist
                    FeasibilitySummary = feasibilitySummary
                }

            // Step 7: Compute Risk Signals
            let topSupplierConcentration = supplierSpend |> List.sortByDescending snd |> List.truncate 5

            let singleSourceItems =
                recommendations
                |> List.filter(fun r -> r.Strategy = SourcingStrategy.SingleSource)
                |> List.map(fun r -> r.ItemId)
                |> List.distinct

            let geoConcentration =
                lineItems
                |> List.groupBy(fun l -> l.LocationId)
                |> List.map(fun (loc, items) ->
                    let spend = items |> List.sumBy(fun l -> l.EstimatedCost)
                    let pct = if totalSpend > 0.0m then (spend / totalSpend) * 100.0m else 0.0m
                    loc, pct)

            let riskSignals =
                {
                    SupplierConcentration = topSupplierConcentration
                    SingleSourceExposure = singleSourceItems
                    GeographicConcentration = geoConcentration
                    ProcurementConfidenceDistribution = confidenceDist
                }

            // Step 8: Compute Recommendation Signals
            let itemsRequiringReview =
                recommendations |> List.filter(fun r -> r.IsEscalated) |> List.map(fun r -> r.ItemId) |> List.distinct

            let consolidationOpps =
                recommendations
                |> List.filter(fun r -> r.IsHeldForConsolidation)
                |> List.map(fun r ->
                    $"Requirement {ProcurementRequirementId.value r.RequirementId} for {ItemId.value r.ItemId} flagged for volume consolidation")

            let riskConcentrationAlerts =
                topSupplierConcentration
                |> List.filter(fun (_, pct) -> pct > 50.0m)
                |> List.map(fun (sup, pct) ->
                    $"Supplier {SupplierId.value sup} holds {pct:F1}%% of total spend (exceeds 50%% threshold)")

            let unresolvedFeasIssues =
                recommendations
                |> List.choose(fun r ->
                    match r.Feasibility with
                    | ProcurementFeasibility.Infeasible reason ->
                        Some $"Item {ItemId.value r.ItemId}: Infeasible - {reason}"
                    | ProcurementFeasibility.ConditionallyFeasible constraints ->
                        let constrStr = String.concat "; " constraints
                        Some $"Item {ItemId.value r.ItemId}: Constrained - {constrStr}"
                    | ProcurementFeasibility.Feasible -> None)

            let recommendationSignals =
                {
                    ItemsRequiringPlannerReview = itemsRequiringReview
                    SupplierConsolidationOpportunities = consolidationOpps
                    RiskConcentrationAlerts = riskConcentrationAlerts
                    UnresolvedFeasibilityIssues = unresolvedFeasIssues
                }

            // Step 9: Evaluate DE-S-052 Publish Procurement Plan
            let confidenceScore = Rules.evaluateConfidenceScore confidenceDist

            let! decisionOutcome =
                Decisions.evaluatePublication isComplete confidenceScore unresolvedFeasIssues.Length policy

            match decisionOutcome with
            | PlanPublicationDecisionOutcome.Hold reason ->
                return! Error(ruleViolationWithDetail ArsIdentifiers.Rules.procurementPlanPublicationAuthorization $"Procurement plan publication held: {reason}")
            | PlanPublicationDecisionOutcome.Publish
            | PlanPublicationDecisionOutcome.PublishWithFlags _ ->

                let newVersion =
                    match existingPlan with
                    | None -> 1
                    | Some p -> p.VersionNumber + 1

                let planId = ProcurementPlanId.ofScopeAndPeriod cmd.PlanningScopeId cmd.PlanningCycleId

                let plan =
                    {
                        PlanId = planId
                        PlanningScopeId = cmd.PlanningScopeId
                        PlanningCycleId = cmd.PlanningCycleId
                        VersionNumber = newVersion
                        LifecycleState = ProcurementPlanLifecycleState.Published
                        LineItems = lineItems
                        ActiveRecommendationIds = recommendations |> List.map(fun r -> r.RequirementId)
                        AggregateMetrics = metrics
                        RiskSignals = riskSignals
                        RecommendationSignals = recommendationSignals
                        PublicationTimestamp = cmd.PublicationTimestamp
                        PublicationAuthority = cmd.PublicationAuthority
                    }

                let events =
                    [
                        ProcurementPlanEvent.PlanPublished(
                            newVersion,
                            cmd.PublicationTimestamp,
                            cmd.PublicationAuthority,
                            plan
                        )
                    ]

                let traceOutcome = { Outcome = plan; Evaluations = [] }

                return
                    buildDecisionWithTrace
                        Model.evolve
                        existingPlan
                        events
                        ArsIdentifiers.Decisions.publishProcurementPlanDecision.Id
                        []
                        ArsIdentifiers.Capabilities.procureMaterials.Id
                        traceOutcome
                        (Some ArsIdentifiers.Policies.procurementPolicy.Id)
                        (Some 1)
                        [ ArsIdentifiers.SemanticObjects.procurementPlan.Id ]
                        (Some
                            $"Procurement plan published for scope {PlanningScopeId.value cmd.PlanningScopeId} cycle {cmd.PlanningCycleId}: Version {newVersion}")
        }

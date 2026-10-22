/// AB-S-050: Create Procurement Recommendation
/// Traces to: Supply Intelligence Specification §4.3.1 (AB-S-050, EV-S-050)
module Medhavi.Supply.ProcureMaterials.ProcurementRecommendation.Behaviors

open System
open Medhavi.Common
open Medhavi.Foundation.Contracts
open Medhavi.Foundation.Contracts.Decision
open Medhavi.Foundation.Failure
open Medhavi.SemanticModel
open Medhavi.Supply
open Model
open Decisions

type ProcurementFactsProvider =
    ItemId
        -> LocationId
        -> (ItemPurchasingParametersFact option *
        SupplierCandidateFact list *
        Map<SupplierId, SupplierCollaborationHealthState> *
        string list *
        decimal)

let createRecommendation
    (factsProvider: ProcurementFactsProvider)
    (policy: ProcurementPolicyConfig)
    : Decide<ProcurementRecommendation, CreateProcurementRecommendationCmd, ProcurementRecommendationEvent> =
    fun (cmd: CreateProcurementRecommendationCmd) (existingState: ProcurementRecommendation option) ->
        result {
            do! Rules.validateIdentity cmd.RequirementId

            do!
                Rules.validateCreationEligibility
                    (Quantity.value cmd.RequiredQuantity)
                    cmd.RequiredDate
                    cmd.EvaluationTimestamp

            do! Rules.validateActiveStateTransition existingState

            let paramsOpt, candidates, healthStates, activeDisruptions, futureNetGaps =
                factsProvider cmd.ItemId cmd.LocationId

            let! purchasingParams =
                match paramsOpt with
                | Some p -> Ok p
                | None ->
                    Error(
                        ruleViolationWithDetail
                            ArsIdentifiers.Rules.procurementRecommendationCreationEligibility
                            "Purchasing master data missing for item-location"
                    )

            // Step 1: BA-S-030 Evaluate Procurement Requirement
            let evaluatedReq =
                Algorithms.evaluateProcurementRequirement
                    cmd.RequirementId
                    cmd.ItemId
                    cmd.LocationId
                    (Quantity.value cmd.RequiredQuantity)
                    cmd.RequiredDate
                    cmd.EvaluationTimestamp
                    purchasingParams

            let isDualSourcingMandatory = policy.DualSourcingMandatoryItemCategories.Contains(ItemId.value cmd.ItemId)

            // Step 2: BA-S-031 Assess Sourcing Situation
            let situation =
                Algorithms.assessSourcingSituation
                    evaluatedReq
                    candidates
                    healthStates
                    activeDisruptions
                    futureNetGaps
                    policy
                    isDualSourcingMandatory

            // Step 3: BA-S-032 Determine Enterprise Sourcing Determination
            let determination =
                Algorithms.determineEnterpriseSourcingDetermination
                    evaluatedReq
                    situation
                    purchasingParams.UnitStandardCost
                    policy
                    isDualSourcingMandatory

            // Step 4: DE-S-051 Determine Enterprise Sourcing Determination
            let! decisionOutcome = Decisions.evaluateSourcingDetermination determination situation evaluatedReq policy

            match decisionOutcome with
            | SourcingDecisionOutcome.Reject reason ->
                return! Error(ruleViolationWithDetail ArsIdentifiers.Rules.sourcingDeterminationGovernance $"Sourcing determination rejected: {reason}")
            | SourcingDecisionOutcome.Approve ->

                let approvedCandidates = candidates |> List.filter(fun c -> c.IsApproved && c.IsCompliant)
                let defaultUom = UnitOfMeasureId.defaultUoM()

                let strategy, allocations =
                    match determination.Intent with
                    | SourcingIntent.DualSourceRiskMitigation when approvedCandidates.Length >= 2 ->
                        let s1 = approvedCandidates.[0]
                        let s2 = approvedCandidates.[1]
                        let qTotal = evaluatedReq.RecommendedOrderQuantity
                        let q1 = Math.Round(qTotal * 0.60m, 4)
                        let q2 = qTotal - q1
                        let rel1 = evaluatedReq.CalculatedReleaseDate
                        let rel2 = evaluatedReq.CalculatedReleaseDate
                        let del1 = evaluatedReq.RequiredDeliveryDate
                        let del2 = evaluatedReq.RequiredDeliveryDate

                        let a1 =
                            {
                                SupplierId = s1.SupplierId
                                Quantity = Quantity.ofDecimal q1 defaultUom
                                OrderReleaseDate = rel1
                                ExpectedDeliveryDate = del1
                                EstimatedUnitCost = Some purchasingParams.UnitStandardCost
                            }

                        let a2 =
                            {
                                SupplierId = s2.SupplierId
                                Quantity = Quantity.ofDecimal q2 defaultUom
                                OrderReleaseDate = rel2
                                ExpectedDeliveryDate = del2
                                EstimatedUnitCost = Some purchasingParams.UnitStandardCost
                            }

                        SourcingStrategy.DualSource, [ a1; a2 ]

                    | SourcingIntent.DualSourceRiskMitigation ->
                        let allocs =
                            match approvedCandidates with
                            | s :: _ ->
                                [
                                    {
                                        SupplierId = s.SupplierId
                                        Quantity = Quantity.ofDecimal evaluatedReq.RecommendedOrderQuantity defaultUom
                                        OrderReleaseDate = evaluatedReq.CalculatedReleaseDate
                                        ExpectedDeliveryDate = evaluatedReq.RequiredDeliveryDate
                                        EstimatedUnitCost = Some purchasingParams.UnitStandardCost
                                    }
                                ]
                            | [] -> []

                        SourcingStrategy.SingleSource, allocs

                    | SourcingIntent.SoleSourcePreservation ->
                        let allocs =
                            match approvedCandidates with
                            | s :: _ ->
                                [
                                    {
                                        SupplierId = s.SupplierId
                                        Quantity = Quantity.ofDecimal evaluatedReq.RecommendedOrderQuantity defaultUom
                                        OrderReleaseDate = evaluatedReq.CalculatedReleaseDate
                                        ExpectedDeliveryDate = evaluatedReq.RequiredDeliveryDate
                                        EstimatedUnitCost = Some purchasingParams.UnitStandardCost
                                    }
                                ]
                            | [] -> []

                        SourcingStrategy.SingleSource, allocs

                    | SourcingIntent.ExpeditedIntervention ->
                        let allocs =
                            match approvedCandidates with
                            | s :: _ ->
                                [
                                    {
                                        SupplierId = s.SupplierId
                                        Quantity = Quantity.ofDecimal evaluatedReq.RecommendedOrderQuantity defaultUom
                                        OrderReleaseDate = evaluatedReq.CalculatedReleaseDate
                                        ExpectedDeliveryDate = evaluatedReq.RequiredDeliveryDate
                                        EstimatedUnitCost = Some purchasingParams.UnitStandardCost
                                    }
                                ]
                            | [] -> []

                        SourcingStrategy.Competitive, allocs

                    | SourcingIntent.StandardReplenishment ->
                        let allocs =
                            match approvedCandidates with
                            | s :: _ ->
                                [
                                    {
                                        SupplierId = s.SupplierId
                                        Quantity = Quantity.ofDecimal evaluatedReq.RecommendedOrderQuantity defaultUom
                                        OrderReleaseDate = evaluatedReq.CalculatedReleaseDate
                                        ExpectedDeliveryDate = evaluatedReq.RequiredDeliveryDate
                                        EstimatedUnitCost = Some purchasingParams.UnitStandardCost
                                    }
                                ]
                            | [] -> []

                        SourcingStrategy.SingleSource, allocs

                let governanceStatus =
                    match determination.Governance with
                    | GovernancePosture.AutonomousExecutionPermitted -> ProcurementGovernanceStatus.Authorized
                    | GovernancePosture.PlannerApprovalMandatory ->
                        ProcurementGovernanceStatus.Escalated "Planner approval mandatory per governance posture"
                    | GovernancePosture.ExecutiveEscalationRequired ->
                        ProcurementGovernanceStatus.Escalated
                            "Executive escalation required per policy spend and risk limits"

                let newVersion =
                    match existingState with
                    | None -> 1
                    | Some r -> r.VersionNumber + 1

                let recommendation =
                    {
                        RequirementId = cmd.RequirementId
                        VersionNumber = newVersion
                        LifecycleState = ProcurementRecommendationLifecycleState.Active
                        ItemId = cmd.ItemId
                        LocationId = cmd.LocationId
                        RequiredDate = cmd.RequiredDate
                        RequiredQuantity = cmd.RequiredQuantity
                        Feasibility = situation.Feasibility
                        Allocations = allocations
                        Strategy = strategy
                        Determination = determination
                        Rationale = determination.Rationale
                        EvidenceReferences = [ cmd.SourceSupplyPlanId; determination.DeterminationId ]
                        OverallConfidence = situation.OverallConfidence
                        SupplierConfidence = situation.OverallConfidence
                        DemandConfidence = RecommendationConfidence.High
                        CapacityConfidence =
                            if situation.Ecosystem.AggregateCapacity >= evaluatedReq.RecommendedOrderQuantity then
                                RecommendationConfidence.High
                            else
                                RecommendationConfidence.Low
                        InventoryRiskFactor = evaluatedReq.InducedExcessRatio
                        SourcingComplexityFactor = decimal situation.ReasonCodes.Length * 0.1m
                        GovernanceStatus = governanceStatus
                        Assumptions =
                            [
                                $"MOQ={purchasingParams.MOQ}"
                                $"BatchMultiple={purchasingParams.PackagingMultiple}"
                            ]
                        SourceSupplyPlanId = cmd.SourceSupplyPlanId
                        LastUpdatedTimestamp = cmd.EvaluationTimestamp
                    }

                let events = [ ProcurementRecommendationEvent.RecommendationCreated recommendation ]

                let traceOutcome =
                    {
                        Outcome = recommendation
                        Evaluations = []
                    }

                return
                    buildDecisionWithTrace
                        Model.evolve
                        existingState
                        events
                        ArsIdentifiers.Decisions.determineEnterpriseSourcingDetermination.Id
                        []
                        ArsIdentifiers.Capabilities.procureMaterials.Id
                        traceOutcome
                        (Some ArsIdentifiers.Policies.procurementPolicy.Id)
                        (Some 1)
                        [ ArsIdentifiers.SemanticObjects.procurementRecommendation.Id ]
                        (Some
                            $"Procurement recommendation created for requirement {ProcurementRequirementId.value cmd.RequirementId}: Version {newVersion}")
        }

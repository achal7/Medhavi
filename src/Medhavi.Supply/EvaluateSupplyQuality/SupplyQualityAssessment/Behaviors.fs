/// SE-S-100 — Supply Quality Assessment Aggregate Behaviors
/// Traces to: AB-S-090 (Publish Supply Quality Assessment)
module Medhavi.Supply.EvaluateSupplyQuality.SupplyQualityAssessment.Behaviors

open System
open Medhavi.Common
open Medhavi.Foundation.Contracts
open Medhavi.Foundation.Contracts.Decision
open Medhavi.Foundation.Failure
open Medhavi.SemanticModel
open Medhavi.Supply
open Model
open Policies
open Algorithms
open Rules
open Decisions

/// AB-S-090 — Publish Supply Quality Assessment
let evaluateAndPublish
    (policy: SupplyQualityPolicy)
    : Decide<SupplyQualityAssessment, EvaluateSupplyQualityCmd, SupplyQualityAssessmentEvent> =
    fun (cmd: EvaluateSupplyQualityCmd) (stateOpt: SupplyQualityAssessment option) ->
        result {
            // 1. Verify Prerequisites per BR-S-260
            do! Rules.validateEligibility
                    cmd.Prerequisites.HasPublishedSupplyPlan
                    cmd.Prerequisites.HasInventoryHealthAssessment
                    cmd.Prerequisites.HasCapacityHealthAssessment
                    cmd.Prerequisites.HasSupplierCollaborationHealthAssessment
                    cmd.Prerequisites.IsEvaluationPeriodComplete

            // 2. Validate Identity per BR-S-100
            let! assessmentId = Rules.validateIdentity cmd.PlanningScopeId cmd.PlanningHorizon cmd.EvaluationPeriodId
            let! _ = Rules.validatePolicyActive policy

            // 3. Determine current aggregate and published version
            let currentAggregate =
                stateOpt
                |> Option.defaultValue
                    { AssessmentId = assessmentId
                      PlanningScopeId = cmd.PlanningScopeId
                      PlanningHorizon = cmd.PlanningHorizon
                      EvaluationPeriodId = cmd.EvaluationPeriodId
                      Versions = []
                      CurrentPublishedVersion = None }

            let currentPublished =
                currentAggregate.Versions
                |> List.tryFind (fun v -> v.LifecycleState = SupplyQualityLifecycleState.Published)

            let highestVersion =
                currentAggregate.Versions
                |> List.map (fun v -> v.VersionNumber)
                |> List.fold max 0

            let nextVersionNumber = highestVersion + 1
            do! Rules.validateNextVersion highestVersion nextVersionNumber

            // 4. Map dimensional outcomes to metrics & evaluations via BA-S-070 and DE-S-091
            let dimensionalEvaluations =
                policy.Dimensions
                |> List.map (fun dimConfig ->
                    let outcomeOpt =
                        cmd.DimensionalOutcomes
                        |> List.tryFind (fun o ->
                            o.DimensionName.Trim().ToLowerInvariant() = (QualityDimensionType.name dimConfig.Dimension).Trim().ToLowerInvariant())

                    match outcomeOpt with
                    | Some outcome ->
                        let actuals = outcome.Metrics |> List.map (fun m -> m.MetricName, m.ActualValue) |> Map.ofList
                        let priors = outcome.Metrics |> List.choose (fun m -> m.PriorPeriodValue |> Option.map (fun pv -> m.MetricName, pv)) |> Map.ofList
                        let priorClassifications =
                            currentPublished
                            |> Option.map (fun p -> p.DimensionalEvaluations |> List.map (fun d -> d.Dimension, d.AuthoritativeClassification) |> Map.ofList)
                            |> Option.defaultValue Map.empty

                        let initialEval =
                            Algorithms.EvaluateSupplyQualityDimensions.evaluateDimension
                                dimConfig
                                actuals
                                priors
                                priorClassifications
                                outcome.EvidenceCompleteness
                                outcome.EvidenceTimeliness
                                outcome.SourceConsistency
                                outcome.EvidenceReferences
                                policy.TrendDeadbandPercent

                        let decCtx : Decisions.EvaluateSupplyPerformance.DecisionContext =
                            { ForceMajeureDeclared = cmd.ForceMajeureDeclared
                              StrategicContextNote = cmd.StrategicInvestmentNotes }

                        let dec = Decisions.EvaluateSupplyPerformance.decide initialEval decCtx policy
                        { initialEval with
                            AuthoritativeClassification = dec.AuthoritativeClassification
                            Trend = dec.AuthoritativeTrend }

                    | None ->
                        // Missing dimension evidence
                        { Dimension = dimConfig.Dimension
                          DimensionName = QualityDimensionType.name dimConfig.Dimension
                          IsMandatory = dimConfig.IsMandatory
                          EvaluatedMetrics = []
                          EffectivenessClassification = QualityClassification.Unassessed
                          EfficiencyClassification = QualityClassification.Unassessed
                          RecommendedClassification = QualityClassification.Unassessed
                          AuthoritativeClassification = QualityClassification.Unassessed
                          Trend = TrendTrajectory.Unassessed
                          Confidence = QualityConfidenceLevel.Low
                          DataCompleteness = 0.0m
                          Score = 0.0m
                          IssueCount = 1
                          EvidenceReferences = []
                          ReasonCodes = [ "MandatoryEvidenceIncomplete" ]
                          AffectedObjectives = [ dimConfig.ObjectiveDescription ] }
                )

            // 5. Synthesize Overall via BA-S-071
            let overallQuality =
                Algorithms.DeriveOverallSupplyQualityAssessment.synthesizeOverall
                    policy
                    dimensionalEvaluations
                    cmd.ForceMajeureDeclared
                    cmd.StrategicInvestmentNotes

            let comparators : AssessmentContextComparator =
                { HistoricalComparator = currentPublished |> Option.map (fun p -> $"Version {p.VersionNumber} ({p.OverallQuality.OverallClassification})")
                  CrossScopeComparator = None
                  TargetVarianceSummary = "Evaluated against PO-S-110 baseline quality thresholds" }

            // 6. Evaluate DE-S-092 Publication Decision
            let pubInput : PublicationRuleInput =
                { OverallQuality = overallQuality
                  DimensionalEvaluations = dimensionalEvaluations
                  Policy = policy }

            let! pubDecision = Decisions.PublishSupplyQualityAssessment.evaluatePublication Rules.publicationRules pubInput

            let publicationFlags =
                pubDecision.Evaluations
                |> List.filter (fun e -> not e.Passed)
                |> List.map (fun e ->
                    let evidenceStr = String.concat "; " e.Evidence
                    sprintf "%s: %s" e.RuleId evidenceStr)

            match pubDecision.Outcome with
            | PublicationDecisionOutcome.Publish
            | PublicationDecisionOutcome.PublishWithFlags ->
                // Draft transitions to Published
                let newPublishedVersion : SupplyQualityAssessmentVersion =
                    { VersionNumber = nextVersionNumber
                      DimensionalEvaluations = dimensionalEvaluations
                      OverallQuality = overallQuality
                      Comparators = comparators
                      PublicationFlags = publicationFlags
                      PolicyVersion = policy.PolicyVersion
                      CreatedTimestamp = cmd.Timestamp
                      PublicationTimestamp = Some cmd.Timestamp
                      LifecycleState = SupplyQualityLifecycleState.Published }

                // Supersede previous published version
                let updatedVersions =
                    currentAggregate.Versions
                    |> List.map (fun v ->
                        if v.LifecycleState = SupplyQualityLifecycleState.Published then
                            { v with LifecycleState = SupplyQualityLifecycleState.Superseded }
                        else v)
                    |> fun vs -> newPublishedVersion :: vs

                let updatedAggregate =
                    { currentAggregate with
                        Versions = updatedVersions
                        CurrentPublishedVersion = Some nextVersionNumber }

                let events =
                    [ SupplyQualityAssessmentPublished(updatedAggregate, nextVersionNumber, currentAggregate.CurrentPublishedVersion, cmd.Timestamp) ]

                let traceId = Guid.NewGuid().ToString("N")

                return
                    buildDecisionWithTrace
                        evolve
                        stateOpt
                        events
                        traceId
                        []
                        ArsIdentifiers.Capabilities.evaluateSupplyQuality.Id
                        pubDecision
                        (Some ArsIdentifiers.Policies.supplyQualityPolicy.Id)
                        (Some nextVersionNumber)
                        [ ArsIdentifiers.SemanticObjects.supplyQualityAssessment.Id ]
                        (Some (sprintf "Supply Quality Assessment published as version %d" nextVersionNumber))

            | PublicationDecisionOutcome.Hold ->
                // Stays in Draft
                let newDraftVersion : SupplyQualityAssessmentVersion =
                    { VersionNumber = nextVersionNumber
                      DimensionalEvaluations = dimensionalEvaluations
                      OverallQuality = overallQuality
                      Comparators = comparators
                      PublicationFlags = publicationFlags
                      PolicyVersion = policy.PolicyVersion
                      CreatedTimestamp = cmd.Timestamp
                      PublicationTimestamp = None
                      LifecycleState = SupplyQualityLifecycleState.Draft }

                let updatedVersions = newDraftVersion :: currentAggregate.Versions
                let updatedAggregate =
                    { currentAggregate with Versions = updatedVersions }

                let events =
                    [ SupplyQualityAssessmentDraftCreated(updatedAggregate, nextVersionNumber) ]

                let traceId = Guid.NewGuid().ToString("N")

                return
                    buildDecisionWithTrace
                        evolve
                        stateOpt
                        events
                        traceId
                        []
                        ArsIdentifiers.Capabilities.evaluateSupplyQuality.Id
                        pubDecision
                        (Some ArsIdentifiers.Policies.supplyQualityPolicy.Id)
                        (Some nextVersionNumber)
                        [ ArsIdentifiers.SemanticObjects.supplyQualityAssessment.Id ]
                        (Some "Supply Quality Assessment draft composed but held from publication")
        }

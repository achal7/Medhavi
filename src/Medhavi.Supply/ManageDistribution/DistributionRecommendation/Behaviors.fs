/// AB-S-070 — Create Distribution Recommendation Aggregate Behavior
/// Traces to: Supply Intelligence Specification §4.3.1 (AB-S-070, EV-S-070)
module Medhavi.Supply.ManageDistribution.DistributionRecommendation.Behaviors

open System
open Medhavi.Common
open Medhavi.Foundation.Contracts
open Medhavi.Foundation.Contracts.Decision
open Medhavi.Foundation.Failure
open Medhavi.SemanticModel
open Medhavi.Supply
open Medhavi.Supply.ArsIdentifiers
open Model
open Policies
open Algorithms
open Rules
open Decisions

let private parseDeploymentPriority (priorityStr: string) : DeploymentPriority =
    match priorityStr with
    | "PromotionSupport" -> DeploymentPriority.PromotionSupport
    | "NewProductLaunch" -> DeploymentPriority.NewProductLaunch
    | "SeasonalPositioning" -> DeploymentPriority.SeasonalPositioning
    | "InventoryBalancing" -> DeploymentPriority.InventoryBalancing
    | _ -> DeploymentPriority.ServiceProtection

/// AB-S-070 — Execute pure functional recommendation creation
let create
    (policy: DistributionPolicyParameters)
    (candidateLanes: CandidateLaneFact list)
    (inventoryPositions: Map<LocationId, decimal>)
    (dockCap: DockCapacityFact option)
    (isSupplyPlanPublished: bool)
    (reqFact: TransferRequirementFact)
    : Decide<DistributionRecommendationAggregate, CreateRecommendationCmd, DistributionRecommendationEvent> =
    fun (cmd: CreateRecommendationCmd) (stateOpt: DistributionRecommendationAggregate option) ->
        result {
            // 1. Build Construction Input
            let constructionInput: ConstructionInput =
                {
                    Requirement = reqFact
                    CandidateLanes = candidateLanes
                    SourceInventoryPositions = inventoryPositions
                    DockCapacity = dockCap
                    Policy = policy
                    EvaluationTimestamp = cmd.Timestamp
                    PreviousRecommendation = stateOpt
                }

            // 2. Execute BA-S-050
            let constructionRes = Algorithms.construct constructionInput

            // 3. Evaluate Decision DE-S-071
            let sourcingInput: SourcingInput =
                {
                    Construction = constructionInput
                    Result = constructionRes
                    IsSupplyPlanPublished = isSupplyPlanPublished
                    IsPolicyCurrent = true
                }

            let! decision = evaluateSourcing sourcingRules sourcingInput

            let res =
                match decision.Outcome with
                | SourcingDecisionOutcome.PreferredSourceSourcing r -> r
                | SourcingDecisionOutcome.AlternateSourceSourcing r -> r
                | SourcingDecisionOutcome.DeferMovement(_, r) -> r
                | SourcingDecisionOutcome.CrossDockSourcing r -> r

            // 4. Determine next version number (monotonicity)
            let versionNumber =
                match stateOpt with
                | Some prev -> prev.VersionNumber + 1
                | None -> 1

            let newAggregate: DistributionRecommendationAggregate =
                {
                    RequirementReference = reqFact.RequirementId
                    VersionNumber = versionNumber
                    LifecycleState = DistributionRecommendationLifecycleState.Active
                    Item = reqFact.Item
                    RequiredQuantity = reqFact.RequiredQuantity
                    DestinationLocation = reqFact.DestinationLocation
                    RequiredDeliveryDate = reqFact.RequiredArrivalDate
                    SourceLocations = res.SourceLocations
                    LanesUsed = res.LanesUsed
                    DepartureTiming = res.DepartureTiming
                    ExpectedArrivalTiming = res.ExpectedArrivalTiming
                    DestinationReceivingWindow = None
                    Strategy = res.Strategy
                    AlternativeStrategy = res.AlternativeStrategy
                    AllocationPolicyUsed = res.AllocationPolicyUsed
                    AllocationOutcome = res.AllocationOutcome
                    DeploymentPriority = parseDeploymentPriority reqFact.DeploymentPriority
                    Feasibility = res.Feasibility
                    FeasibilityConstraints = res.FeasibilityConstraints
                    Confidence = res.Confidence
                    Stability = res.Stability
                    RiskSignals = res.RiskSignals
                    ReasonCodes = res.ReasonCodes
                    DecisionRationale = res.Rationale
                    DecisionEvidenceReferences = res.EvidenceReferences
                    CreatedTimestamp = cmd.Timestamp
                    SupersededTimestamp = None
                    SupersededByVersion = None
                }

            let supersededVersion = stateOpt |> Option.map(fun prev -> prev.VersionNumber)
            let events = [ DistributionRecommendationCreated(newAggregate, supersededVersion) ]
            let traceId = Guid.NewGuid().ToString()

            let summary =
                match decision.Outcome with
                | SourcingDecisionOutcome.PreferredSourceSourcing _ ->
                    "Distribution Recommendation created (Preferred Source)"
                | SourcingDecisionOutcome.AlternateSourceSourcing _ ->
                    "Distribution Recommendation created (Alternate Source)"
                | SourcingDecisionOutcome.CrossDockSourcing _ -> "Distribution Recommendation created (Cross Dock)"
                | SourcingDecisionOutcome.DeferMovement(reason, _) ->
                    sprintf "Distribution Recommendation created (Deferred: %s)" reason

            return
                buildDecisionWithTrace
                    evolve
                    stateOpt
                    events
                    traceId
                    []
                    ArsIdentifiers.Capabilities.manageDistribution.Id
                    decision
                    (Some ArsIdentifiers.Policies.distributionPolicy.Id)
                    (Some policy.Version)
                    [ ArsIdentifiers.SemanticObjects.distributionRecommendation.Id ]
                    (Some summary)
        }

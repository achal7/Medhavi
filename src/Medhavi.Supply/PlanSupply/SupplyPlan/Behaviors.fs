/// SE-S-020 — Supply Planning Behaviors
module Medhavi.Supply.PlanSupply.SupplyPlan.Behaviors

open Medhavi.Common
open Medhavi.Foundation.Contracts
open Medhavi.Foundation.Contracts.Decision
open Medhavi.Foundation.Failure
open Medhavi.SemanticModel
open Medhavi.Supply
open Model
open Policies
open Rules
open Decisions
open Algorithms


type SupplyPlanDecisionOutcome =
    | Generated
    | Evaluated of PlanEvaluationOutcome
    | Published of version: int * superseded: int option

/// Pure catamorphic evolution function for SupplyPlan aggregate root
let evolve (stateOpt: SupplyPlan option) (event: SupplyPlanEvent) : SupplyPlan option =
    match event with
    | SupplyPlanGenerated(plan, _) -> Some plan
    | SupplyPlanEvaluated(plan, _, _) -> Some plan
    | SupplyPlanPublished(plan, _, _) -> Some plan

/// Replay a sequence of enterprise events into the current aggregate state
let replay (events: SupplyPlanEvent seq) : SupplyPlan option =
    Seq.fold evolve None events

let private buildTrace policy decisionId (decisionOutcome: DecisionOutcome<SupplyPlanDecisionOutcome>) state events summary =
    let policyId = policy |> Option.map (fun _ -> ArsIdentifiers.Policies.supplyPlanAcceptancePolicy.Id)
    let policyVersion = policy |> Option.map (fun _ -> 1)

    buildDecisionWithTrace
        evolve
        state
        events
        decisionId
        []
        ArsIdentifiers.Capabilities.planSupply.Id
        decisionOutcome
        policyId
        policyVersion
        [ ArsIdentifiers.SemanticObjects.supplyPlan.Id ]
        (Some summary)

/// AB-S-010: Generate Supply Plan
let generate
    (relaxationPolicy: ConstraintRelaxationPolicy)
    (balancingInputProvider: PlanningScopeId -> ForecastHorizonId -> BalancingInput)
    : Decide<SupplyPlan, GeneratePlanCmd, SupplyPlanEvent> =
    fun (cmd: GeneratePlanCmd) (stateOpt: SupplyPlan option) ->
        result {
            do! validateAggregateIdentity cmd.PlanningScopeId cmd.ForecastHorizonId
            do! validateDerivationSource cmd.SupplyUnderstandingVersion cmd.ForecastPublicationVersion cmd.EnterprisePictureVersion

            let currentPlan =
                match stateOpt with
                | Some p -> p
                | None ->
                    { PlanningScopeId = cmd.PlanningScopeId
                      ForecastHorizonId = cmd.ForecastHorizonId
                      Versions = [] }

            let previousPublishedOpt =
                currentPlan.Versions
                |> List.tryFind (fun v -> v.State = VersionState.Published)

            let input = balancingInputProvider cmd.PlanningScopeId cmd.ForecastHorizonId
            let balancingResult = balanceSupplyAndDemand relaxationPolicy previousPublishedOpt input

            let nextVersionNumber =
                let maxVer =
                    currentPlan.Versions
                    |> List.map (fun v -> v.VersionNumber)
                    |> List.fold max 0
                maxVer + 1

            let versionId =
                SupplyPlanVersionId.create (sprintf "SPV-%s-%s-%d" (PlanningScopeId.value cmd.PlanningScopeId) (ForecastHorizonId.value cmd.ForecastHorizonId) nextVersionNumber)
                |> Result.defaultValue SupplyPlanVersionId.defaultId

            let newDraftVersion: SupplyPlanVersion =
                { VersionId = versionId
                  VersionNumber = nextVersionNumber
                  State = VersionState.Draft
                  PlannedOrders = balancingResult.PlannedOrders
                  ProjectedShortfalls = balancingResult.ProjectedShortfalls
                  ConstraintViolations = balancingResult.ConstraintViolations
                  FeasibilityStatus = balancingResult.FeasibilityStatus
                  ServiceLevelAchieved = balancingResult.ServiceLevelAchieved
                  MaxCapacityUtilization = balancingResult.MaxCapacityUtilization
                  PlanStabilityScore = balancingResult.PlanStabilityScore
                  PlanConfidenceScore = balancingResult.PlanConfidenceScore
                  SourceSupplyUnderstandingVersion = cmd.SupplyUnderstandingVersion
                  SourceForecastPublicationVersion = cmd.ForecastPublicationVersion
                  SourceEnterprisePictureVersion = cmd.EnterprisePictureVersion
                  GenerationTime = cmd.Timestamp
                  EvaluationOutcome = None
                  EvaluationTime = None
                  PublicationTime = None }

            let updatedVersions = newDraftVersion :: currentPlan.Versions
            let updatedPlan = { currentPlan with Versions = updatedVersions }

            let events = [ SupplyPlanGenerated(updatedPlan, nextVersionNumber) ]
            let decision = { Outcome = Generated; Evaluations = [] }

            return buildTrace (Some relaxationPolicy) ArsIdentifiers.Capabilities.planSupply.Id decision stateOpt events "Supply plan draft generated"
        }

/// AB-S-011: Evaluate Supply Plan
let evaluate
    (acceptancePolicy: SupplyPlanAcceptancePolicy)
    : Decide<SupplyPlan, EvaluatePlanCmd, SupplyPlanEvent> =
    fun (cmd: EvaluatePlanCmd) (stateOpt: SupplyPlan option) ->
        result {
            let! plan =
                match stateOpt with
                | Some p -> Ok p
                | None ->
                    Error(DomainError.notFound ("SupplyPlan", sprintf "%s#%s" (PlanningScopeId.value cmd.PlanningScopeId) (ForecastHorizonId.value cmd.ForecastHorizonId)))

            let targetVersionOpt =
                plan.Versions |> List.tryFind (fun v -> v.VersionNumber = cmd.VersionNumber)

            let! targetVersion = validateEvaluationEligibility targetVersionOpt

            if targetVersion.EvaluationOutcome.IsSome then
                return! Error(DomainError.conflict $"Supply Plan Version %d{cmd.VersionNumber} has already been evaluated")

            let isFirstPublication =
                not (plan.Versions |> List.exists (fun v -> v.State = VersionState.Published || v.State = VersionState.Superseded))

            let outcome = acceptSupplyPlan acceptancePolicy isFirstPublication targetVersion

            let updatedVersion =
                { targetVersion with
                    EvaluationOutcome = Some outcome
                    EvaluationTime = Some cmd.Timestamp }

            let updatedVersions =
                plan.Versions
                |> List.map (fun v -> if v.VersionNumber = cmd.VersionNumber then updatedVersion else v)

            let updatedPlan = { plan with Versions = updatedVersions }

            let events = [ SupplyPlanEvaluated(updatedPlan, cmd.VersionNumber, outcome) ]
            let decision = { Outcome = Evaluated outcome; Evaluations = [] }

            return buildTrace (Some acceptancePolicy) ArsIdentifiers.Decisions.acceptSupplyPlan.Id decision stateOpt events "Supply plan evaluated"
        }

/// AB-S-012: Publish Supply Plan
let publish
    (cadencePolicy: SupplyPlanPublicationCadencePolicy)
    : Decide<SupplyPlan, PublishPlanCmd, SupplyPlanEvent> =
    fun (cmd: PublishPlanCmd) (stateOpt: SupplyPlan option) ->
        result {
            let! plan =
                match stateOpt with
                | Some p -> Ok p
                | None ->
                    Error(DomainError.notFound ("SupplyPlan", sprintf "%s#%s" (PlanningScopeId.value cmd.PlanningScopeId) (ForecastHorizonId.value cmd.ForecastHorizonId)))

            let! (publishedVer, supersededOpt) = publishSupplyPlan cadencePolicy plan cmd.VersionNumber

            let updatedVersions =
                plan.Versions
                |> List.map (fun v ->
                    if v.VersionNumber = publishedVer then
                        { v with State = VersionState.Published; PublicationTime = Some cmd.Timestamp }
                    elif supersededOpt = Some v.VersionNumber then
                        { v with State = VersionState.Superseded }
                    else
                        v)

            let updatedPlan = { plan with Versions = updatedVersions }

            let events = [ SupplyPlanPublished(updatedPlan, publishedVer, supersededOpt) ]
            let decision = { Outcome = Published(publishedVer, supersededOpt); Evaluations = [] }

            return buildTrace (Some cadencePolicy) ArsIdentifiers.Decisions.publishSupplyPlanDecision.Id decision stateOpt events "Supply plan published"
        }

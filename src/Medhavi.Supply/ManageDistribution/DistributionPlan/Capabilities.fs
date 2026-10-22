/// SE-S-081 — Distribution Plan Aggregate Capabilities
/// Traces to: Supply Intelligence Specification §4.3.1 (SE-S-081, AB-S-071)
module Medhavi.Supply.ManageDistribution.DistributionPlan.Capabilities

open System.Threading.Tasks
open Medhavi.Common
open Medhavi.Foundation.Contracts
open Medhavi.Foundation.Execution
open Medhavi.Foundation.Execution.CommandCapabilities
open Medhavi.Foundation.Failure
open Medhavi.Foundation.Observation
open Medhavi.SemanticModel
open Medhavi.Contracts.Supply
open Medhavi.Supply
open Model
open Policies

type DistributionPlanAggregateApi =
    { Publish: PublishDistributionPlanReq -> Task<Result<DistributionPlanAggregate, ApplicationError>> }

/// Pure factory for SE-S-081 Distribution Plan Aggregate API (Fully isolated from other aggregates)
let create
    (planRepo: Repository<DistributionPlanAggregate, DistributionPlanId, DistributionPlanEvent>)
    (publishKnowledge: KnowledgeRepresentation)
    (policy: DistributionPlanPublicationPolicyParameters)
    (deps: AggregateStages.EnvelopeStoreDependencies<DistributionPlanEvent>)
    (ports: SupplyPorts)
    : DistributionPlanAggregateApi =

    let handlePublish (req: PublishDistributionPlanReq) : Task<Result<DistributionPlanAggregate, ApplicationError>> =
        taskResult {
            let! cmd: PublishDistributionPlanCmd = liftValidation (ACL.toPublishCmd req)

            do! requireEntityExists ports.PlanningScopeExists cmd.PlanningScopeId "PlanningScope" PlanningScopeId.value

            // Retrieve active distribution recommendation facts via decoupled port
            let! (activeRecFacts: ActiveDistributionRecommendationFact list) =
                protect (ports.GetActiveDistributionRecommendationsForScope cmd.PlanningScopeId)

            let decideFn = Behaviors.publish policy activeRecFacts
            let pipeline = CommandPipeline.create planRepo (fun (c: PublishDistributionPlanCmd) -> c.PlanId) decideFn deps

            return! runPipeline pipeline publishKnowledge id cmd
        }

    { Publish = handlePublish }

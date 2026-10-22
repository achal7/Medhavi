namespace Medhavi.Supply.ProcureMaterials.ProcurementPlan

open System.Threading.Tasks
open Medhavi.Common
open Medhavi.Foundation.Contracts
open Medhavi.Foundation.Execution
open Medhavi.Foundation.Execution.CommandCapabilities
open Medhavi.Foundation.Failure
open Medhavi.Foundation.Observation
open Medhavi.Contracts.Supply
open Medhavi.Supply
open Model
open Behaviors

type ProcurementPlanAggregateApi =
    { Publish: PublishProcurementPlanReq -> Task<Result<ProcurementPlan, ApplicationError>> }

module Capabilities =

    let create
        (repo: Repository<ProcurementPlan, ProcurementPlanId, ProcurementPlanEvent>)
        (publishKnowledge: KnowledgeRepresentation)
        (deps: AggregateStages.EnvelopeStoreDependencies<ProcurementPlanEvent>)
        (factsProvider: ProcurementPlanFactsProvider)
        (policy: PlanPublicationPolicyConfig)
        : ProcurementPlanAggregateApi =

        let publish (req: PublishProcurementPlanReq) : Task<Result<ProcurementPlan, ApplicationError>> =
            taskResult {
                let! cmd = liftValidation (ACL.toPublishPlanCmd req)

                let pipeline =
                    CommandPipeline.create
                        repo
                        (fun (c: PublishProcurementPlanCmd) -> ProcurementPlanId.ofScopeAndPeriod c.PlanningScopeId c.PlanningCycleId)
                        (publishPlan factsProvider policy)
                        deps

                return! runPipeline pipeline publishKnowledge id cmd
            }

        { Publish = publish }

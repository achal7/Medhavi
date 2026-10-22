/// SE-S-071 — Published Production Schedule Capability Handler
/// Traces to: Supply Intelligence Specification §4.3.1 (SE-S-071, AB-S-061)
module Medhavi.Supply.ScheduleProduction.PublishedProductionSchedule.Capabilities

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

type PublishedProductionScheduleAggregateApi =
    {
        Publish: PublishProductionScheduleReq -> Task<Result<PublishedProductionScheduleAggregate, ApplicationError>>
    }

let create
    (repo: Repository<PublishedProductionScheduleAggregate, string, PublishedProductionScheduleEvent>)
    (publishKnowledge: KnowledgeRepresentation)
    (policy: PublicationPolicyConfig)
    (deps: AggregateStages.EnvelopeStoreDependencies<PublishedProductionScheduleEvent>)
    (ports: SupplyPorts)
    : PublishedProductionScheduleAggregateApi =

    let handlePublish
        (req: PublishProductionScheduleReq)
        : Task<Result<PublishedProductionScheduleAggregate, ApplicationError>> =
        taskResult {
            let! cmd: PublishProductionScheduleCmd = liftValidation(ACL.toPublishCmd req)

            let! (activeSchedules: ActiveScheduleFact list) =
                protect(ports.GetActiveSchedulesForScope cmd.PlanningScopeId cmd.PlanningPeriodId)

            let decider = Behaviors.decide activeSchedules policy

            let getId (c: PublishProductionScheduleCmd) =
                $"PUB-{PlanningScopeId.value c.PlanningScopeId}-{PlanningPeriodId.value c.PlanningPeriodId}"

            let pipeline = CommandPipeline.create repo getId decider deps
            return! runPipeline pipeline publishKnowledge id cmd
        }

    { Publish = handlePublish }

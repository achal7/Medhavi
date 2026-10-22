/// SE-S-070 — Production Schedule Capability Handler
/// Traces to: Supply Intelligence Specification §4.3.1 (SE-S-070, AB-S-060)
module Medhavi.Supply.ScheduleProduction.ProductionSchedule.Capabilities

open System
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
open Behaviors
open ACL

type ProductionScheduleAggregateApi =
    { Create: CreateProductionScheduleReq -> Task<Result<ProductionScheduleAggregate, ApplicationError>> }

let create
    (repo: Repository<ProductionScheduleAggregate, string, ProductionScheduleEvent>)
    (publishKnowledge: KnowledgeRepresentation)
    (policy: SchedulingPolicyConfig)
    (deps: AggregateStages.EnvelopeStoreDependencies<ProductionScheduleEvent>)
    (ports: SupplyPorts)
    : ProductionScheduleAggregateApi =

    let handleCreate (req: CreateProductionScheduleReq) : Task<Result<ProductionScheduleAggregate, ApplicationError>> =
        taskResult {
            let! cmd: CreateProductionScheduleCmd = liftValidation (ACL.toCreateCmd req)

            let! (reqs: PlannedProductionRequirementFact list) = protect (ports.GetPlannedProductionRequirements cmd.ResourceId cmd.PlanningPeriodId)
            let! (availOpt: ResourceAvailabilityFact option) = protect (ports.GetResourceAvailabilityFacts cmd.ResourceId cmd.PlanningPeriodId)
            let! (chgMatrix: ChangeoverMatrixFact) = protect (ports.GetChangeoverMatrix cmd.ResourceId)
            let! (priorOpt: ActiveScheduleFact option) = protect (ports.GetPriorActiveScheduleFact cmd.ResourceId cmd.PlanningPeriodId cmd.ScheduleType)

            match availOpt with
            | None ->
                return! TaskResult.fail (Domain (ruleViolationWithDetail ArsIdentifiers.Rules.productionScheduleCreationEligibility $"Resource availability facts not found for {PhysicalResourceId.value cmd.ResourceId}"))
            | Some avail ->
                let decider = Behaviors.decide reqs avail chgMatrix priorOpt policy
                let getId (c: CreateProductionScheduleCmd) =
                    $"SCHED-{PhysicalResourceId.value c.ResourceId}-{PlanningPeriodId.value c.PlanningPeriodId}-{c.ScheduleType}"
                let pipeline = CommandPipeline.create repo getId decider deps
                return! runPipeline pipeline publishKnowledge id cmd
        }

    { Create = handleCreate }

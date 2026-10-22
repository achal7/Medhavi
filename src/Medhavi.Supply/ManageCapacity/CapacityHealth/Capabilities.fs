/// SE-S-041 — Capacity Health Command Capabilities
/// Traces to: FS-S-031 (Assess Capacity Health)
module Medhavi.Supply.ManageCapacity.CapacityHealth.Capabilities

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
open Medhavi.Supply.ManageCapacity.CapacityHealth
open Policies
open Model

type CapacityHealthAggregateApi =
    { Publish: PublishCapacityHealthReq -> Task<Result<CapacityHealthAssessment, ApplicationError>> }

type ScopePositionsTaskProvider = PlanningScopeId -> Task<CapacityPositionFact list>

let create
    (repo: Repository<CapacityHealthAssessment, CapacityHealthAssessmentId, CapacityHealthEvent>)
    (publishKnowledge: KnowledgeRepresentation)
    (policy: CapacityHealthPolicy)
    (deps: AggregateStages.EnvelopeStoreDependencies<CapacityHealthEvent>)
    (ports: SupplyPorts)
    (positionsProvider: ScopePositionsTaskProvider)
    : CapacityHealthAggregateApi =

    let publish (req: PublishCapacityHealthReq) : Task<Result<CapacityHealthAssessment, ApplicationError>> =
        taskResult {
            let! cmd: PublishCapacityHealthCmd = liftValidation (ACL.toPublishCmd req)

            do! requireEntityExists ports.PlanningScopeExists cmd.AssessmentScopeId "PlanningScope" PlanningScopeId.value
            do! requireEntityExists ports.EvaluationPeriodExists cmd.EvaluationPeriodId "EvaluationPeriod" EvaluationPeriodId.value

            let! positions = protect (positionsProvider cmd.AssessmentScopeId)

            let decide = Behaviors.publish (fun _ -> positions) policy

            let pipeline =
                CommandPipeline.create
                    repo
                    (fun (c: PublishCapacityHealthCmd) -> CapacityHealthAssessmentId.ofScopeAndPeriod c.AssessmentScopeId c.EvaluationPeriodId)
                    decide
                    deps

            return! runPipeline pipeline publishKnowledge id cmd
        }

    { Publish = publish }

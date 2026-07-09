module internal Medhavi.Demand.DemandBehaviourAssessment.CommandHandler

open System.Threading.Tasks
open Medhavi.Common.Validation
open Medhavi.SharedKernel
open Medhavi.SharedKernel.Execution
open Medhavi.SharedKernel.Failure
open Medhavi.SharedKernel.Observation
open Medhavi.SharedKernel.Contracts.Aggregate
open Medhavi.Demand
open Medhavi.Demand.DemandBehaviourAssessment.ACL
open Medhavi.Demand.DemandBehaviourAssessment.Model

// Traceability: Coordinates command execution pipeline for SE-D-037 (Demand Behaviour Assessment aggregate)
// Exposes execution corridor mapping DomainCommand -> Task<ExecutionOutcome<DemandBehaviourAssessment, ApplicationError>>

let execute
    (repo: Repository<DemandBehaviourAssessment, string, DemandBehaviourAssessmentEvent>)
    (publishKnowledge: ArchitecturalKnowledge -> unit)
    (cmd: DemandBehaviourAssessmentCommand)
    : Task<ExecutionOutcome<DemandBehaviourAssessment, ApplicationError>> =

    // Command Pipeline maps to Decisions.decide state transition loop
    let pipeline = CommandPipeline.create repo (fun (c: DemandBehaviourAssessmentCommand) -> c.AssignmentId) Decisions.decide

    CommandCapabilities.execute pipeline publishKnowledge cmd

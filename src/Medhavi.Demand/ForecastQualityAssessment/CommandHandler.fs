module internal Medhavi.Demand.ForecastQualityAssessment.CommandHandler

open System.Threading.Tasks
open Medhavi.SharedKernel.Execution
open Medhavi.SharedKernel.Failure
open Medhavi.SharedKernel.Observation
open Medhavi.SharedKernel.Contracts.Aggregate
open Medhavi.Demand.ForecastQualityAssessment.Model

let execute
    (repo: Repository<ForecastQualityAssessment, string, ForecastQualityAssessmentEvent>)
    (publishKnowledge: ArchitecturalKnowledge -> unit)
    (cmd: ForecastQualityAssessmentCommand)
    : Task<ExecutionOutcome<ForecastQualityAssessment, ApplicationError>> =

    let pipeline =
        CommandPipeline.create repo (fun (c: ForecastQualityAssessmentCommand) -> c.AssignmentId) Decisions.decide

    CommandCapabilities.execute pipeline publishKnowledge cmd

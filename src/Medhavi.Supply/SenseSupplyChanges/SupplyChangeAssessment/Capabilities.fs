/// SE-S-090 — Supply Change Assessment Command Capabilities
/// Traces to: CR-S-021, FS-S-085
module Medhavi.Supply.SenseSupplyChanges.SupplyChangeAssessment.Capabilities

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

type SupplyChangeAssessmentAggregateApi =
    {
        Assess: AssessSupplyChangeReq -> Task<Result<SupplyChangeAssessmentAggregate, ApplicationError>>
        Archive: ArchiveSupplyChangeReq -> Task<Result<SupplyChangeAssessmentAggregate, ApplicationError>>
    }

/// Factory for aggregate-level command capabilities
let create
    (repo: Repository<SupplyChangeAssessmentAggregate, SupplyChangeAssessmentId, SupplyChangeEvent>)
    (publishKnowledge: KnowledgeRepresentation)
    (policy: SupplyChangePolicyConfiguration)
    (deps: AggregateStages.EnvelopeStoreDependencies<SupplyChangeEvent>)
    (ports: SupplyPorts)
    : SupplyChangeAssessmentAggregateApi =

    let assess (req: AssessSupplyChangeReq) : Task<Result<SupplyChangeAssessmentAggregate, ApplicationError>> =
        taskResult {
            let! baselineOpt: BaselineSupplyFact option = protect(ports.GetBaselineSupplyFact req.MonitoredEntityId)

            let! baseFact: BaselineSupplyFact =
                match baselineOpt with
                | Some f -> Ok f
                | None -> Error(Domain(DomainError.notFound("BaselineSupplyFact", req.MonitoredEntityId)))
                |> TaskResult.ofResult

            let window = TimeSpan.FromHours(float policy.CorroborationTimeWindowHours)
            let! corrobCount = protect(ports.GetCorroboratingSignalsCount req.MonitoredEntityId window)

            let baseline: BaselineEntityFact =
                {
                    EntityReference = baseFact.EntityReference
                    CommittedDeliveryDate = baseFact.CommittedDeliveryDate
                    CommittedQuantity = baseFact.CommittedQuantity
                    IsCriticalPath = baseFact.IsCriticalPath
                    BufferDays = baseFact.BufferDays
                }

            let! cmd = liftValidation(ACL.toAssessCmd req baseline corrobCount)
            let decideFn = Behaviors.assess Rules.defaultRules policy

            let pipeline = CommandPipeline.create repo (fun (c: AssessSupplyChangeCmd) -> c.AssessmentId) decideFn deps

            return! runPipeline pipeline publishKnowledge id cmd
        }

    let archive (req: ArchiveSupplyChangeReq) : Task<Result<SupplyChangeAssessmentAggregate, ApplicationError>> =
        taskResult {
            let! cmd: ArchiveSupplyChangeCmd = liftValidation(ACL.toArchiveCmd req)

            let pipeline =
                CommandPipeline.create repo (fun (c: ArchiveSupplyChangeCmd) -> c.AssessmentId) Behaviors.archive deps

            return! runPipeline pipeline publishKnowledge id cmd
        }

    { Assess = assess; Archive = archive }

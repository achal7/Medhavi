module Medhavi.Supply.UnderstandSupply.SupplyUnderstanding.Capabilities

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

type SupplyUnderstandingAggregateApi =
    { Revise: ReviseSupplyUnderstandingReq -> Task<Result<SupplyUnderstanding, ApplicationError>>
      Publish: PublishSupplyUnderstandingReq -> Task<Result<SupplyUnderstanding, ApplicationError>> }

/// CA-S-001 — Supply Understanding command capabilities (FS-S-003 Revise, FS-S-004 Publish).
let create
    (repo: Repository<SupplyUnderstanding, PlanningScopeId, SupplyUnderstandingEvent>)
    (publishKnowledge: KnowledgeRepresentation)
    (materialityPolicy: MaterialityPolicy)
    (cadencePolicy: CadencePolicy)
    (deps: AggregateStages.EnvelopeStoreDependencies<SupplyUnderstandingEvent>)
    (ports: SupplyPorts)
    : SupplyUnderstandingAggregateApi =

    /// FS-S-003 — Revise Supply Understanding.
    let revise (req: ReviseSupplyUnderstandingReq) : Task<Result<SupplyUnderstanding, ApplicationError>> =
        taskResult {
            let! cmd: ReviseCmd = liftValidation (ACL.toReviseCmd req)

            do! requireEntityExists ports.PlanningScopeExists cmd.PlanningScopeId "PlanningScope" PlanningScopeId.value

            let! facts: PictureSupplyFacts = protect (ports.GetPictureSupplyFacts cmd.PlanningScopeId)
            let! changes: string list = protect (ports.GetSupplyChangeAssessments cmd.PlanningScopeId)

            let enrichedCmd: ReviseCmd =
                { cmd with
                    PictureSupplyFacts = facts
                    SupplyChangeAssessments = changes }

            let pipeline =
                CommandPipeline.create
                    repo
                    (fun (c: ReviseCmd) -> c.PlanningScopeId)
                    Behaviors.revise
                    deps

            return! runPipeline pipeline publishKnowledge id enrichedCmd
        }

    /// FS-S-004 — Publish Supply Understanding.
    let publish (req: PublishSupplyUnderstandingReq) : Task<Result<SupplyUnderstanding, ApplicationError>> =
        taskResult {
            let! cmd: PublishCmd = liftValidation (ACL.toPublishCmd req)

            let pipeline =
                CommandPipeline.create
                    repo
                    (fun (c: PublishCmd) -> c.PlanningScopeId)
                    (Behaviors.publish materialityPolicy cadencePolicy)
                    deps

            return! runPipeline pipeline publishKnowledge id cmd
        }

    { Revise = revise
      Publish = publish }

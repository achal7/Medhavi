module Medhavi.Demand.UnderstandDemand.DemandUnderstanding.Capabilities

open System.Threading.Tasks
open Medhavi.Common
open Medhavi.Foundation.Contracts
open Medhavi.Foundation.Execution
open Medhavi.Foundation.Execution.CommandCapabilities
open Medhavi.Foundation.Failure
open Medhavi.Foundation.Observation
open Medhavi.SemanticModel
open Medhavi.Contracts.Demand
open Medhavi.Demand
open Medhavi.Demand.UnderstandDemand.DemandUnderstanding
open Model
open Policies

type DemandUnderstandingAggregateApi =
    { Revise: ReviseDemandUnderstandingReq -> Task<Result<DemandUnderstanding, ApplicationError>>
      Publish: PublishDemandUnderstandingReq -> Task<Result<DemandUnderstanding, ApplicationError>> }

/// CA-D-001 — Demand Understanding command capabilities (FS-D-003 Revise, FS-D-004 Publish).
let create
    (repo: Repository<DemandUnderstanding, PlanningScopeId, DemandUnderstandingEvent>)
    (publishKnowledge: KnowledgeRepresentation)
    (materialityPolicy: MaterialityPolicy)
    (cadencePolicy: CadencePolicy)
    (deps: AggregateStages.EnvelopeStoreDependencies<DemandUnderstandingEvent>)
    (ports: DemandPorts)
    : DemandUnderstandingAggregateApi =

    /// FS-D-003 — Revise Demand Understanding.
    let revise (req: ReviseDemandUnderstandingReq) : Task<Result<DemandUnderstanding, ApplicationError>> =
        taskResult {
            // Step 1: Lift validation
            let! cmd: ReviseCmd = liftValidation (ACL.toReviseCmd req)

            // Step 2: Ensure the Planning Scope exists (BR-D-002)
            do! requireEntityExists ports.PlanningScopeExists cmd.PlanningScopeId "PlanningScope" PlanningScopeId.value

            // Step 3: Load the latest Published Enterprise Picture demand facts (BR-D-400, FS-D-003)
            let! facts: PictureFacts = protect (ports.GetPictureDemandFacts cmd.PlanningScopeId)

            let enrichedCmd: ReviseCmd = { cmd with PictureFacts = facts }

            // Step 4: Execute the pipeline
            let pipeline =
                CommandPipeline.create
                    repo
                    (fun (c: ReviseCmd) -> c.PlanningScopeId)
                    Behaviors.revise
                    deps

            return! runPipeline pipeline publishKnowledge id enrichedCmd
        }

    /// FS-D-004 — Publish Demand Understanding.
    let publish (req: PublishDemandUnderstandingReq) : Task<Result<DemandUnderstanding, ApplicationError>> =
        taskResult {
            // Step 1: Lift validation
            let! cmd: PublishCmd = liftValidation (ACL.toPublishCmd req)

            // Step 2: Execute the pipeline
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

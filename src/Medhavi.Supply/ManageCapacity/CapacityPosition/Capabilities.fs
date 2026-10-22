/// SE-S-040 — Capacity Position Command Capabilities
/// Traces to: FS-S-030 (Maintain Capacity Position Understanding)
module Medhavi.Supply.ManageCapacity.CapacityPosition.Capabilities

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
open Medhavi.Supply.ManageCapacity.CapacityPosition
open Model
open Policies

type CapacityPositionAggregateApi =
    { Update: UpdateCapacityPositionReq -> Task<Result<CapacityPositionAssessment, ApplicationError>> }

let create
    (repo: Repository<CapacityPositionAssessment, PhysicalResourceId, CapacityPositionEvent>)
    (publishKnowledge: KnowledgeRepresentation)
    (deps: AggregateStages.EnvelopeStoreDependencies<CapacityPositionEvent>)
    (ports: SupplyPorts)
    (policy: CapacityManagementPolicy)
    : CapacityPositionAggregateApi =

    let update (req: UpdateCapacityPositionReq) : Task<Result<CapacityPositionAssessment, ApplicationError>> =
        taskResult {
            let! cmd: UpdateCapacityPositionCmd = liftValidation (ACL.toUpdateCmd req)

            // Fetch capacity and load facts asynchronously
            let startTime = cmd.Timestamp
            let endTime = Timestamp.addDays 7.0 cmd.Timestamp
            let! capFacts: ResourceCapacityFacts = protect (ports.GetResourceCapacityFacts cmd.ResourceId startTime endTime)
            let! loadFact: ResourceLoadFact = protect (ports.GetResourceLoadFacts cmd.ResourceId startTime endTime)

            let factsProvider _ : ResourceCapacityFacts * ResourceLoadFact =
                (capFacts, loadFact)

            let decide = Behaviors.update factsProvider policy

            let pipeline =
                CommandPipeline.create
                    repo
                    (fun (c: UpdateCapacityPositionCmd) -> c.ResourceId)
                    decide
                    deps

            return! runPipeline pipeline publishKnowledge id cmd
        }

    { Update = update }

/// SE-S-080 — Distribution Recommendation Aggregate Capabilities
/// Traces to: Supply Intelligence Specification §4.3.1 (SE-S-080, AB-S-070)
module Medhavi.Supply.ManageDistribution.DistributionRecommendation.Capabilities

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

type DistributionRecommendationAggregateApi =
    { Create: CreateDistributionRecommendationReq -> Task<Result<DistributionRecommendationAggregate, ApplicationError>> }

/// Pure factory for SE-S-080 Distribution Recommendation Aggregate API
let create
    (repo: Repository<DistributionRecommendationAggregate, DistributionRequirementId, DistributionRecommendationEvent>)
    (publishKnowledge: KnowledgeRepresentation)
    (policy: DistributionPolicyParameters)
    (deps: AggregateStages.EnvelopeStoreDependencies<DistributionRecommendationEvent>)
    (ports: SupplyPorts)
    : DistributionRecommendationAggregateApi =

    let handleCreate (req: CreateDistributionRecommendationReq) : Task<Result<DistributionRecommendationAggregate, ApplicationError>> =
        taskResult {
            let! cmd: CreateRecommendationCmd = liftValidation (ACL.toCreateCmd req)

            do! requireEntityExists ports.DistributionRequirementExists cmd.RequirementId "DistributionRequirement" DistributionRequirementId.value

            let! (reqFactOpt: TransferRequirementFact option) = protect (ports.GetTransferRequirement cmd.RequirementId)

            match reqFactOpt with
            | None ->
                return! TaskResult.fail (Domain(DomainError.notFound("TransferRequirement", DistributionRequirementId.value cmd.RequirementId)))
            | Some reqFact ->
                let! (candidateLanes: CandidateLaneFact list) = protect (ports.GetCandidateTransportationLanes reqFact.Item reqFact.DestinationLocation)
                let sourceLocations = candidateLanes |> List.map (fun l -> l.SourceLocation) |> List.distinct
                let! (inventoryPositions: Map<LocationId, decimal>) = protect (ports.GetSourceInventoryPositions reqFact.Item sourceLocations)
                let! (dockCap: DockCapacityFact option) = protect (ports.GetDockCapacity reqFact.DestinationLocation reqFact.RequiredArrivalDate)

                let! scopeId = liftValidation (validatePlanningScopeId "ENTERPRISE")

                let! (isPlanPublished: bool) = protect (ports.IsSupplyPlanPublished scopeId)

                let decideFn = Behaviors.create policy candidateLanes inventoryPositions dockCap isPlanPublished reqFact
                let pipeline = CommandPipeline.create repo (fun (c: CreateRecommendationCmd) -> c.RequirementId) decideFn deps

                return! runPipeline pipeline publishKnowledge id cmd
        }

    { Create = handleCreate }

namespace Medhavi.Supply.ProcureMaterials.ProcurementRecommendation

open System.Threading.Tasks
open Medhavi.Common
open Medhavi.Foundation.Contracts
open Medhavi.Foundation.Execution
open Medhavi.Foundation.Execution.CommandCapabilities
open Medhavi.Foundation.Failure
open Medhavi.Foundation.Observation
open Medhavi.Contracts.Supply
open Medhavi.Supply
open Model
open Behaviors

type ProcurementRecommendationAggregateApi =
    {
        Create: CreateProcurementRecommendationReq -> Task<Result<ProcurementRecommendation, ApplicationError>>
    }

module Capabilities =

    let create
        (repo: Repository<ProcurementRecommendation, ProcurementRequirementId, ProcurementRecommendationEvent>)
        (publishKnowledge: KnowledgeRepresentation)
        (deps: AggregateStages.EnvelopeStoreDependencies<ProcurementRecommendationEvent>)
        (factsProvider: ProcurementFactsProvider)
        (policy: ProcurementPolicyConfig)
        : ProcurementRecommendationAggregateApi =

        let createRec
            (req: CreateProcurementRecommendationReq)
            : Task<Result<ProcurementRecommendation, ApplicationError>> =
            taskResult {
                let! cmd = liftValidation(ACL.toCreateRecommendationCmd req)

                let pipeline =
                    CommandPipeline.create
                        repo
                        (fun (c: CreateProcurementRecommendationCmd) -> c.RequirementId)
                        (createRecommendation factsProvider policy)
                        deps

                return! runPipeline pipeline publishKnowledge id cmd
            }

        { Create = createRec }

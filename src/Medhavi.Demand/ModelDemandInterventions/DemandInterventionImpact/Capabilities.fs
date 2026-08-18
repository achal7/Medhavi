/// SE-D-018 Demand Intervention Impact Child Aggregate Capabilities API
/// Operates strictly within ApplicationError domain (no ApiError).
module Medhavi.Demand.ModelDemandInterventions.DemandInterventionImpact.Capabilities

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
open Model
open Policies

type AggregateApi =
    { AssessImpact: AssessInterventionImpactReq -> Task<Result<DemandInterventionImpact, ApplicationError>>
      PublishImpact: PublishInterventionImpactReq -> Task<Result<DemandInterventionImpact, ApplicationError>>
      GetById: DemandInterventionImpactId -> Task<Result<DemandInterventionImpact option, ApplicationError>> }

let create
    (repo: Repository<DemandInterventionImpact, DemandInterventionImpactId, DemandInterventionImpactEvent>)
    (publishKnowledge: KnowledgeRepresentation)
    (policy: InterventionModelingGovernancePolicy)
    (deps: AggregateStages.EnvelopeStoreDependencies<DemandInterventionImpactEvent>)
    (ports: DemandPorts)
    : AggregateApi =

    let assessImpact (req: AssessInterventionImpactReq) : Task<Result<DemandInterventionImpact, ApplicationError>> =
        taskResult {
            // Step 1: Lift applicative validation
            let! cmd: AssessInterventionImpactCmd = liftValidation (ACL.toAssessImpactCmd req)

            // Step 2: Ensure Item, Location exist and Intervention is active
            do! requireEntityExists ports.ItemExists cmd.Item "Item" ItemId.value
            do! requireEntityExists ports.LocationExists cmd.Location "Location" LocationId.value

            let! isActive = protect (ports.IsScenarioAdjustmentActive cmd.InterventionReference)
            if not isActive then
                return!
                    TaskResult.fail(
                        ApplicationError.fromDomainError(
                            DomainError.rule(
                                "Scenario adjustment '" + (ScenarioAdjustmentId.value cmd.InterventionReference) + "' is not active per BR-D-415",
                                rule = ArsIdentifiers.Rules.interventionReferenceValidity.Id
                            )
                        )
                    )
            else
                // Step 3: Execute pipeline
                let pipeline =
                    CommandPipeline.create
                        repo
                        (fun (c: AssessInterventionImpactCmd) -> c.ImpactId)
                        (Behaviors.assessInterventionImpact policy)
                        deps

                return! runPipeline pipeline publishKnowledge id cmd
        }

    let publishImpact (req: PublishInterventionImpactReq) : Task<Result<DemandInterventionImpact, ApplicationError>> =
        taskResult {
            // Step 1: Lift applicative validation
            let! cmd: PublishInterventionImpactCmd = liftValidation (ACL.toPublishImpactCmd req)

            // Step 2: Retrieve existing Draft
            let! (existingOpt: DemandInterventionImpact option) =
                repo.Get cmd.ImpactId
                |> TaskResult.mapError Repository.mapRepositoryErrorToApplicationError

            match existingOpt with
            | None ->
                return!
                    TaskResult.fail(
                        ApplicationError.fromDomainError(
                            DomainError.notFound(
                                "DemandInterventionImpact",
                                DemandInterventionImpactId.value cmd.ImpactId
                            )
                        )
                    )
            | Some draftImpact ->
                let! isActive = protect (ports.IsScenarioAdjustmentActive draftImpact.InterventionReference)

                // Search for any existing published version to supersede
                let! (allImpacts: DemandInterventionImpact list) =
                    repo.GetAll ()
                    |> TaskResult.mapError Repository.mapRepositoryErrorToApplicationError

                let previousPublishedOpt =
                    allImpacts
                    |> List.tryFind (fun (imp: DemandInterventionImpact) ->
                        imp.InterventionReference = draftImpact.InterventionReference
                        && imp.Item = draftImpact.Item
                        && imp.Location = draftImpact.Location
                        && imp.LifecycleState = Published
                        && imp.ImpactId <> draftImpact.ImpactId)

                let prevIdOpt = previousPublishedOpt |> Option.map (fun imp -> imp.ImpactId)

                let pipeline =
                    CommandPipeline.create
                        repo
                        (fun (c: PublishInterventionImpactCmd) -> c.ImpactId)
                        (Behaviors.publishInterventionImpact policy isActive prevIdOpt)
                        deps

                return! runPipeline pipeline publishKnowledge id cmd
        }

    let getById (id: DemandInterventionImpactId) : Task<Result<DemandInterventionImpact option, ApplicationError>> =
        repo.Get id
        |> TaskResult.mapError Repository.mapRepositoryErrorToApplicationError

    { AssessImpact = assessImpact
      PublishImpact = publishImpact
      GetById = getById }

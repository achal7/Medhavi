/// SE-D-003 Forecast Publication Child Aggregate Capabilities API
/// Operates strictly within ApplicationError domain (no ApiError).
module Medhavi.Demand.ForecastDemand.ForecastPublication.Capabilities

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
    { InitiateCycle: InitiateForecastCycleReq -> Task<Result<ForecastPublication, ApplicationError>>
      SelectChampionModel: SelectChampionModelReq -> Task<Result<ForecastPublication, ApplicationError>>
      ProduceProjection: ProduceForecastProjectionReq -> Task<Result<ForecastPublication, ApplicationError>>
      ApplyOverride: ApplyPlannerOverrideReq -> Task<Result<ForecastPublication, ApplicationError>>
      Publish: PublishForecastPublicationReq -> Task<Result<ForecastPublication, ApplicationError>> }

let create
    (repo: Repository<ForecastPublication, ForecastPublicationId, ForecastPublicationEvent>)
    (publishKnowledge: KnowledgeRepresentation)
    (modelGovernancePolicy: ForecastModelGovernancePolicy)
    (unforecastablePolicy: UnforecastableSeriesPolicy)
    (publicationGovernancePolicy: ForecastPublicationGovernancePolicy)
    (overridePolicy: ForecastOverrideAuthorizationPolicy)
    (modelParamsPolicy: ForecastModelParametersPolicy)
    (deps: AggregateStages.EnvelopeStoreDependencies<ForecastPublicationEvent>)
    (ports: DemandPorts)
    : AggregateApi =

    let initiateCycle (req: InitiateForecastCycleReq) : Task<Result<ForecastPublication, ApplicationError>> =
        taskResult {
            // Step 1: Lift applicative validation
            let! cmd: InitiateForecastCycleCmd = liftValidation (ACL.toInitiateCycleCmd req)

            // Step 2: Ensure Planning Scope exists
            do! requireEntityExists ports.PlanningScopeExists cmd.PlanningScope "PlanningScope" PlanningScopeId.value

            // Step 3: Execute pipeline
            let pipeline =
                CommandPipeline.create
                    repo
                    (fun (c: InitiateForecastCycleCmd) -> c.PublicationId)
                    Behaviors.initiateCycle
                    deps

            return! runPipeline pipeline publishKnowledge id cmd
        }

    let selectChampionModel (req: SelectChampionModelReq) : Task<Result<ForecastPublication, ApplicationError>> =
        taskResult {
            let! cmd: SelectChampionModelCmd = liftValidation (ACL.toSelectChampionCmd req)

            let pipeline =
                CommandPipeline.create
                    repo
                    (fun (c: SelectChampionModelCmd) -> c.PublicationId)
                    (Behaviors.selectChampionModel modelGovernancePolicy)
                    deps

            return! runPipeline pipeline publishKnowledge id cmd
        }

    let produceProjection (req: ProduceForecastProjectionReq) : Task<Result<ForecastPublication, ApplicationError>> =
        let decider = Behaviors.produceProjection unforecastablePolicy modelParamsPolicy

        let pipeline =
            CommandPipeline.create
                repo
                (fun (c: ProduceForecastProjectionCmd) -> c.PublicationId)
                decider
                deps
        taskResult {
            let! pubId =
                ForecastPublicationId.create req.PublicationId
                |> Result.mapError ApplicationError.fromDomainError

            // Fetch aggregate root to extract scope and horizon
            let! pub:ForecastPublication = 
                repo.Get pubId 
                |> TaskResult.mapError Repository.mapRepositoryErrorToApplicationError
                |> TaskResult.ofOption (
                    ApplicationError.fromDomainError(
                        DomainError.notFound("ForecastPublication", req.PublicationId)
                    )
                )

            // Build projection command with historical data
            let! cmd:ProduceForecastProjectionCmd = ACL.toProduceProjectionCmd ports pub req
            return! runPipeline pipeline publishKnowledge id cmd
        }

    let applyOverride (req: ApplyPlannerOverrideReq) : Task<Result<ForecastPublication, ApplicationError>> =
        taskResult {
            let! cmd: ApplyPlannerOverrideCmd = liftValidation (ACL.toApplyOverrideCmd req)

            do! requireEntityExists ports.ItemExists cmd.Item "Item" ItemId.value
            do! requireEntityExists ports.LocationExists cmd.Location "Location" LocationId.value

            let pipeline =
                CommandPipeline.create
                    repo
                    (fun (c: ApplyPlannerOverrideCmd) -> c.PublicationId)
                    (Behaviors.applyPlannerOverride overridePolicy)
                    deps

            return! runPipeline pipeline publishKnowledge id cmd
        }

    let publish (req: PublishForecastPublicationReq) : Task<Result<ForecastPublication, ApplicationError>> =
        taskResult {
            let! cmd: PublishForecastPublicationCmd = liftValidation (ACL.toPublishCmd req)

            let pipeline =
                CommandPipeline.create
                    repo
                    (fun (c: PublishForecastPublicationCmd) -> c.PublicationId)
                    (Behaviors.publishPublication publicationGovernancePolicy)
                    deps

            return! runPipeline pipeline publishKnowledge id cmd
        }

    { InitiateCycle = initiateCycle
      SelectChampionModel = selectChampionModel
      ProduceProjection = produceProjection
      ApplyOverride = applyOverride
      Publish = publish }

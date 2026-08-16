module Medhavi.Core.EnterprisePictureManagement.Context

open System.Threading
open Medhavi
open Medhavi.SemanticModel
open Medhavi.Common
open Medhavi.Contracts.Core
open Medhavi.Foundation.Contracts
open Medhavi.Foundation.Execution.AggregateStages
open Medhavi.Foundation.Failure
open Medhavi.Foundation.Observation
open Medhavi.Core.ArsIdentifiers
open Medhavi.Core.EnterprisePictureManagement.Workflows
open Model
open Projection

type EnterprisePictureContext =
    { Commands: EnterprisePictureApi
      Queries: QueryService<EnterprisePictureDto, SemanticModel.PlanningScopeId>
      Dispose: unit -> unit }

type CrossDomainQueryPorts =
    { GetActiveDemandReferences: PlanningScopeId -> System.Threading.Tasks.Task<DemandId list>
      GetAvailableSupplyReferences: PlanningScopeId -> System.Threading.Tasks.Task<SupplyId list>
      GetCurrentInventoryReferences: PlanningScopeId -> System.Threading.Tasks.Task<InventoryIdentity list> }

let create
    (repo: Repository<SemanticModel.EnterprisePicture, SemanticModel.PlanningScopeId, EnterprisePictureEvent>)
    (deps: EnvelopeStoreDependencies<EnterprisePictureEvent>)
    (createQueryService: CreateQueryService<EnterprisePictureEvent, Projection.EnterprisePictureDto, SemanticModel.PlanningScopeId>)
    (publishKnowledge: KnowledgeRepresentation)
    (policy: EnterprisePicturePolicy)
    : TaskResult<EnterprisePictureContext, ApplicationError> =
    taskResult {

        // 1. Create capabilities
        let commands = Capabilities.create repo publishKnowledge policy deps

        // 2. Projection
        let! aggregates =
            repo.GetAll()
            |> TaskResult.ofTask
            |> TaskResult.mapError(fun repoErr -> Infrastructure(Database(sprintf "%A" repoErr)))

        let state = seedFromAggregates aggregates

        let validEventTypes =
            [ EnterpriseEvents.pictureVersionComposed.Id
              EnterpriseEvents.pictureVersionPublished.Id ]
            |> EnvelopeFilter.EventTypes

        let! queryCtx:ProjectionContext<EnterprisePictureDto, PlanningScopeId> = createQueryService Projection.evolveProjection validEventTypes state "EnterprisePicture"

        // 3. Wire FS-C-002: Picture Publication Workflow (materiality gate)
        let publicationDeps : PicturePublication.PublicationDependencies =
            { EnterprisePictureApi = commands
              Subscribe = deps.Subscribe
              Codec = Foundation.Codec.json<SharedKernel.BusinessNotifications.PictureVersionComposedNotification> }

        let! publicationSubscription:System.IDisposable =
            PicturePublication.create publicationDeps CancellationToken.None
            |> TaskResult.ofTaskValue
            |> TaskResult.mapError (fun err ->
                Infrastructure (EventStore ($"Failed to create picture publication workflow: {err}")))

        let dispose () =
            queryCtx.Dispose()
            publicationSubscription.Dispose()

        return
            { Commands = commands
              Queries = queryCtx.QueryService
              Dispose = dispose }
    }

module Medhavi.Core.EnterprisePictureManagement.Context

open System
open System.Threading
open System.Threading.Tasks
open Medhavi
open Medhavi.SemanticModel
open Medhavi.Common
open Medhavi.Contracts.Core
open Medhavi.Foundation.Contracts
open Medhavi.Foundation.Execution.AggregateStages
open Medhavi.Foundation.Failure
open Medhavi.Foundation.Observation
open Medhavi.Core.ArsIdentifiers
open Medhavi.Core.EnterprisePictureManagement.Workflows.PictureRecomposition
open Model
open Projection

type EnterprisePictureContext =
    { Commands: EnterprisePictureApi
      Queries: QueryService<EnterprisePictureDto, SemanticModel.PlanningScopeId>
      Dispose: unit -> unit }

type CrossDomainQueryPorts =
    { GetActiveDemandReferences: PlanningScopeId -> Task<DemandId list>
      GetAvailableSupplyReferences: PlanningScopeId -> Task<SupplyId list>
      GetCurrentInventoryReferences: PlanningScopeId -> Task<InventoryIdentity list> }

let create
    (repo: Repository<SemanticModel.EnterprisePicture, SemanticModel.PlanningScopeId, EnterprisePictureEvent>)
    (deps: EnvelopeStoreDependencies<EnterprisePictureEvent>)
    (createQueryService: CreateQueryService<EnterprisePictureEvent, Projection.EnterprisePictureDto, SemanticModel.PlanningScopeId>)
    (publishKnowledge: KnowledgeRepresentation)
    (policy: EnterprisePicturePolicy)
    (planningScopeId: PlanningScopeId)
    (crossDomainPorts: CrossDomainQueryPorts)
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
              EnterpriseEvents.pictureVersionPublished.Id
              EnterpriseEvents.pictureVersionSuperseded.Id ]
            |> EnvelopeFilter.EventTypes

        let! queryCtx:ProjectionContext<EnterprisePictureDto, PlanningScopeId> = createQueryService Projection.evolveProjection validEventTypes state "EnterprisePicture"

        // 3. Wire FS-C-019: Picture Recomposition Workflow
        let recompositionDeps : WorkflowDependencies =
            { Subscribe = deps.Subscribe
              EnterprisePictureApi = commands
              DebounceWindow = TimeSpan.FromSeconds (float policy.DebounceWindowSeconds)
              PlanningScopeId = planningScopeId
              GetCurrentTime = fun () -> Timestamp.now()
              GetActiveDemandReferences = crossDomainPorts.GetActiveDemandReferences
              GetAvailableSupplyReferences = crossDomainPorts.GetAvailableSupplyReferences
              GetCurrentInventoryReferences = crossDomainPorts.GetCurrentInventoryReferences
              Codec = Foundation.Codec.json<SharedKernel.BusinessNotifications.DemandUnderstandingPublishedNotification> }

        // 6. Create the recomposition workflow
        let! recompositionSubscription:System.IDisposable =
            create recompositionDeps CancellationToken.None
            |> TaskResult.ofTaskValue
            |> TaskResult.mapError (fun err ->
                Infrastructure (EventStore ($"Failed to create picture recomposition workflow: {err}")))

        let dispose () =
            recompositionSubscription.Dispose()

        return
            { Commands = commands
              Queries = queryCtx.QueryService
              Dispose = dispose }
    }

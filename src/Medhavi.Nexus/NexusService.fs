namespace Medhavi.Nexus

open Medhavi.Infrastructure
open Medhavi.Infrastructure.Stores.EnvelopeStoreMem
open Medhavi.SharedKernel
open Medhavi.Common.Patterns

type NexusService =
    { DemandService: DemandService.Service
      ScenarioService: ScenarioService.Service
      SupplyService: SupplyService.Service
      MasterDataService: MasterDataService.Service
      Integration: Medhavi.Integration.IntegrationCapabilities
      EnvelopeStore: Stores.EnvelopeStore.EnvelopeStoreOps }

module Bootstrap =
    let setupBoundedContexts (envelopeStore: Stores.EnvelopeStore.EnvelopeStoreOps) =
        taskResult {
            let extractEnvelope = IntegrationService.extractEnvelope
            let! (masterDataService: MasterDataService.Service) = MasterDataService.create envelopeStore extractEnvelope
            let! (demandService: DemandService.Service) = DemandService.create envelopeStore extractEnvelope

            let! (supplyService: SupplyService.Service) =
                SupplyService.create envelopeStore extractEnvelope masterDataService.Context

            let scenarioService: ScenarioService.Service = ScenarioService.create()

            let service =
                { MasterDataService = masterDataService
                  DemandService = demandService
                  ScenarioService = scenarioService
                  SupplyService = supplyService
                  Integration = IntegrationService.create envelopeStore
                  EnvelopeStore = envelopeStore }

            return service
        }

    let create () =
        let envelopeStore = createEnvelopeStoreMem()
        setupBoundedContexts envelopeStore

    let start () : TaskResult<NexusService, ApplicationError> =
        taskResult {
            let! service = create()
            printfn "[ OK ] Started Nexus Service..."

            let! (_: Envelope list) =
                service.Integration.IngestAndPublishMasterData()
                |> TaskResult.mapError(fun err -> ApplicationError.Unknown(err.ToString()))

            [ service.MasterDataService.Context.Initialize()
              service.SupplyService.Context.Initialize()
              service.DemandService.Context.Initialize()
              service.ScenarioService.Context.Initialize() ]
            |> System.Threading.Tasks.Task.WhenAll
            |> Async.AwaitTask
            |> ignore

            DemandService.startSimulator service.DemandService.Context

            return service
        }

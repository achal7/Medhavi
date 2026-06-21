module Medhavi.Nexus.MasterDataService

open System.Threading
open System.Threading.Tasks
open Medhavi.Common.Patterns
open Medhavi.Contracts
open Medhavi.Contracts.MasterData.Uom
open Medhavi.Contracts.MasterData.Network
open Medhavi.Contracts.MasterData.Sku
open Medhavi.Contracts.MasterData.Bom
open Medhavi.Contracts.MasterData.Resource
open Medhavi.Contracts.MasterData.Routing
open Medhavi.Contracts.MasterData.Transport
open Medhavi.Infrastructure.Stores.EnvelopeStore
open Medhavi.MasterData
open Medhavi.SharedKernel
open Medhavi.Transport
open Medhavi.Integration

type Service =
    { Context: MasterDataContext
      IntegrationHandler: SubscriptionHandle }

let mapApi (tr: TaskResult<'T, ApiError>) : TaskResult<'T, ApplicationError> =
    tr |> TaskResult.mapError(fun err -> ApplicationError.External(err.Code, err.Message, Map.empty))

let handler (context: MasterDataContext) (event: IntegrationEvent) : TaskResult<unit, ApplicationError> =
    taskResult {
        match event with
        | UomImported uoms ->
            printfn "[MasterData] Processing UomImported (%d items)..." uoms.Length
            let! (_: UnitOfMeasure list) = mapApi(context.Commands.Uom.DefineBulk(uoms))
            return ()
        | UnitConversionsImported unitConversions ->
            printfn "[MasterData] Processing UnitConversionsImported (%d items)..." unitConversions.Length
            let! (_: UnitConversion list) = mapApi(context.Commands.UnitConversion.DefineBulk(unitConversions))
            return ()
        | TransportLegsImported transportLegs ->
            printfn "[MasterData] Processing TransportLegsImported (%d items)..." transportLegs.Length
            let! (_: TransportLeg list) = mapApi(context.Commands.TransportLeg.DefineBulk(transportLegs))
            return ()
        | RoutingsImported routings ->
            printfn "[MasterData] Processing RoutingsImported (%d items)..." routings.Length
            let! (_: Routing list) = mapApi(context.Commands.Routing.DefineBulk(routings))
            return ()
        | BomImported boms ->
            printfn "[MasterData] Processing BomImported (%d items)..." boms.Length
            let! (_: Bom list) = mapApi(context.Commands.Bom.DefineBulk(boms))
            return ()
        | SkusImported skus ->
            printfn "[MasterData] Processing SkusImported (%d items)..." skus.Length
            let! (_: Sku list) = mapApi(context.Commands.Sku.DefineBulk(skus))
            return ()
        | StockingPointsImported stockingPoints ->
            printfn "[MasterData] Processing StockingPointsImported (%d items)..." stockingPoints.Length
            let! (_: StockingPoint list) = mapApi(context.Commands.StockingPoint.DefineBulk(stockingPoints))
            return ()
        | PlantsImported plants ->
            printfn "[MasterData] Processing PlantsImported (%d items)..." plants.Length
            let! (_: Plant list) = mapApi(context.Commands.Plant.DefineBulk(plants))
            return ()
        | ResourceGroupsImported groups ->
            printfn "[MasterData] Processing ResourceGroupsImported (%d items)..." groups.Length
            let! (_: ResourceGroup list) = mapApi(context.Commands.ResourceGroup.DefineBulk(groups))
            return ()
        | StandardResourcesImported reqs ->
            printfn "[MasterData] Processing StandardResourcesImported (%d items)..." reqs.Length
            let! (_: StandardResource list) = mapApi(context.Commands.StandardResource.DefineBulk(reqs))
            return ()
        | PhysicalResourcesImported reqs ->
            printfn "[MasterData] Processing PhysicalResourcesImported (%d items)..." reqs.Length
            let! (_: PhysicalResource list) = mapApi(context.Commands.PhysicalResource.DefineBulk(reqs))
            return ()
        | _ -> return ()
    }

let create
    (integrationStore: EnvelopeStoreOps)
    (extractEnvelope: IntegrationService.ExtractEnvelope)
    : TaskResult<Service, ApplicationError> =
    taskResult {
        let context = Medhavi.MasterData.BoundedContext.create()

        let handleEvents (envelopedEvent: EnvelopedEvent) : Task<unit> =
            task {
                match extractEnvelope envelopedEvent with
                | Error e -> printfn $"[ MasterData ] Error while deserializing envelope: {e.ToString()}"
                | Ok event ->
                    let! res = handler context event

                    match res with
                    | Ok() -> ()
                    | Error err -> printfn $"[ MasterData ] Error while processing event: Event={event.GetType().Name}, Code={err.Code}, Message={err.Message} ({err.ToString()})"
            }

        let! integrationHandler =
            integrationStore.Subscribe SubscriptionMode.All None handleEvents CancellationToken.None
            |> TaskResult.mapError(fun e -> ApplicationError.Unknown $"{e.ToString()}")

        return
            { Context = context
              IntegrationHandler = integrationHandler }
    }

// Transport context: legs are loaded from MasterData's projection on demand
let getTransportLegs (context: MasterDataContext) =
    task {
        let! legs = context.Queries.TransportLeg.GetAll() |> Async.AwaitTask

        return
            legs
            |> List.filter(fun l -> l.Status)
            |> List.map(fun l ->
                { LegId = l.Id
                  Origin = l.Origin
                  Destination = l.Destination
                  Mode = l.Mode
                  LeadTimeMinutes = l.LeadTimeMinutes
                  Capacity = l.Capacity
                  CapacityUnit = l.CapacityUnit
                  Reliability = None // enrichable from full domain leg
                  CO2PerUnit = None
                  FixedCost = 0.0m
                  VariableCostPerUnit = None
                  Status = l.Status }
                : TransportLegRef)

    }

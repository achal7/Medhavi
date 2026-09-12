namespace Medhavi.Demand

open System.Threading.Tasks
open Medhavi.Contracts.Core
open Medhavi.Contracts.Demand
open Medhavi.Demand.ForecastDemand.Ports
open Medhavi.Foundation
open Medhavi.Foundation.Contracts.Aggregate
open Medhavi.Foundation.Observation
open Medhavi.Demand.ClassifyDemand
open Medhavi.Demand.DetectDemandExceptions
open Medhavi.Demand.EvaluateDemandQuality
open Medhavi.Demand.ExplainDemand
open Medhavi.Demand.ForecastDemand
open Medhavi.Demand.LearnFromDemand
open Medhavi.Demand.PrioritizeDemand
open Medhavi.Demand.SegmentDemand
open Medhavi.Demand.SenseDemand
open Medhavi.Demand.UnderstandDemand


type DemandContext = {
    UnderstandDemand: UnderstandDemandContext
    ForecastDemand: ForecastDemandContext
    SenseDemand: SenseDemandContext
    // SegmentDemand: SegmentDemandContext
    // ClassifyDemand: ClassifyDemandContext
    // PrioritizeDemand: PrioritizeDemandContext
    // EvaluateDemand: EvaluateDemandQualityContext
    // DetectDemandException: DetectDemandExceptionsContext
    // ExplainDemand: ExplainDemandContext
    // LearnFromDemand: LearnFromDemandContext
}

type DemandPolicies = {
    Observation: UnderstandDemand.DemandObservation.Policies
    Materiality: UnderstandDemand.DemandUnderstanding.Policies
    Cadence: UnderstandDemand.DemandUnderstanding.Policies
    ForecastGovernance: ForecastDemand.Policies
    OverrideAuthorization: ForecastDemand.Policies
    UnforecastableSeries: ForecastDemand.Policies
    ModelParameters: ForecastDemand.Policies
    Sensing: SenseDemand.Policies
    Segmentation: SegmentDemand.Policies
    Classification: ClassifyDemand.Policies
    Prioritization: PrioritizeDemand.Policies
    ForecastMeasurement: EvaluateDemandQuality.Policies
    Detection: DetectDemandExceptions.Policies
    Learning: LearnFromDemand.Policies
}

let composeObservation
    (store: EnvelopeStoreOps)
    publishKnowledge
    (telemetryPublisher: TelemetryEvent -> unit)
    (pconfig: ProjectionConfig) = 

    let repo = InMemRepository.create<DemandObservation.Model.DemandObservation, string, DemandObservation.Model.DemandObservationEvent>()
    let codec = Codec.json<DemandObservation.Model.DemandObservationEvent>
    let serializeEvent = codec.Encode >> Result.mapError(fun c -> c.ToString())
    let deps = EnvelopeStoreDeps.create store serializeEvent traceCodec

    let itemTransitionProjectionDeps =
        { Config = pconfig
          EnvelopeStore = store
          EventCodec = itemTransitionCodec
          TelemetryPublisher = telemetryPublisher }

    let itemTransitionProjection =
        createProjectionAgent<ItemTransitionEvent, Medhavi.Contracts.Core.ItemTransition.ItemTransitionDto, TransitionId> itemTransitionProjectionDeps

    let getItemState: (ItemId -> System.Threading.Tasks.Task<option<ReferenceLifecycleState>>) = fun _ -> task { return None }

    Medhavi.Demand.UnderstandDemand.DemandObservation.Context.create itemTransitionRepo itemTransitionDeps publishKnowledge itemTransitionProjection ItemTransitionPolicy.defaultPolicy getItemState



module DemandContext =
    let create
        (publishKnowledge: KnowledgeRepresentation)
        (publishNotification: NotificationPublisher)
        (ports: DemandPorts)
        (policies: DemandPolicies) =

        let observationRepo = InMemRepository.create<DemandObservation.Model.DemandObservation, string, DemandObservation.Model.DemandObservationEvent>()
        let understandRepo = InMemRepository.create<DemandUnderstanding.Model.DemandUnderstanding, string, DemandUnderstanding.Model.DemandUnderstandingEvent>()
        let forecastRepo = InMemRepository.create<ForecastDemand.ForecastPublication.Model.ForecastPublication, string, ForecastDemand.ForecastPublication.Model.ForecastPublicationEvent>()

        let understandCtx = UnderstandDemand.Context.create ports observationRepo understandRepo obsPolicy matPolicy cadencePolicy publishKnowledge publishNotification
        let forecastPorts:ForecastDemandPorts = {
            GetHistoricalDemand = ports.GetHistoricalDemandData
            GetChampionModel = ports
            GetModelConfidence: GetModelConfidencePort
            GetSignalQuality: GetSignalQualityPort
            GetTotalSeriesCount: GetTotalSeriesCountPort
        }
        let forecastCtx = ForecastDemand.Context.create forecastPorts forecastRepo publishKnowledge govPolicy overridePol unforecastPol modelParamsPol publishNotification
        let forecastSub =
            DomainEventBus.Subscribe<ForecastPublicationCon.ForecastPublishedNotification>(fun notif ->
                task {
                    for line in notif.Lines do
                        let! _ = Medhavi.Core.Context.acceptDemand coreCtx DemandOrigin.Forecast
                } |> ignore)

        {
            UnderstandDemand = understandCtx
            ForecastDemand = forecastCtx
        }

    let handleEnterprisePicturePublished
        (ctx : DemandContext)
        (pic : EnterprisePicturePublishedNotification)
        : Task<unit> =
        Context.handleEnterprisePicturePublished
            ctx.UnderstandDemand.Commands.Understanding
            pic
            ctx.UnderstandDemand.CadencePolicy

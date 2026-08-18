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
    // SenseDemand: SenseDemandContext
    // SegmentDemand: SegmentDemandContext
    // ClassifyDemand: ClassifyDemandContext
    // PrioritizeDemand: PrioritizeDemandContext
    // EvaluateDemand: EvaluateDemandQualityContext
    // DetectDemandException: DetectDemandExceptionsContext
    // ExplainDemand: ExplainDemandContext
    // LearnFromDemand: LearnFromDemandContext
}

module DemandContext =
    let create
        (ports: DemandPorts) // Ports required by Medhavi.Demand to run it's capabilities
        (publishKnowledge: KnowledgeRepresentation)
        (publishNotification: NotificationPublisher) =

        let observationRepo = InMemRepository.create<DemandObservation.Model.DemandObservation, string, DemandObservation.Model.DemandObservationEvent>()
        let understandRepo = InMemRepository.create<DemandUnderstanding.Model.DemandUnderstanding, string, DemandUnderstanding.Model.DemandUnderstandingEvent>()
        let forecastRepo = InMemRepository.create<ForecastDemand.ForecastPublication.Model.ForecastPublication, string, ForecastDemand.ForecastPublication.Model.ForecastPublicationEvent>()

        // ---- Policies (defaults; overridable via parameters) ----
        let obsPolicy       = UnderstandDemand.DemandObservation.Policies.defaultPolicy
        let matPolicy       = UnderstandDemand.DemandUnderstanding.Policies.defaultMaterialityPolicy
        let cadencePolicy   = UnderstandDemand.DemandUnderstanding.Policies.defaultCadencePolicy
        let govPolicy       = ForecastDemand.Policies.defaultPublicationGovernancePolicy
        let overridePol     = ForecastDemand.Policies.defaultOverrideAuthorizationPolicy
        let unforecastPol   = ForecastDemand.Policies.defaultUnforecastableSeriesPolicy
        let modelParamsPol  = ForecastDemand.Policies.defaultModelParametersPolicy
        // let sensingPol      = SenseDemand.Policies.defaultSensingPolicy
        // let segPol          = SegmentDemand.Policies.defaultSegmentationPolicy
        // let classPol        = ClassifyDemand.Policies.defaultClassificationPolicy
        // let prioPol         = PrioritizeDemand.Policies.defaultPrioritizationPolicy
        // let measPol         = EvaluateDemandQuality.Policies.defaultForecastMeasurementPolicy
        // let detectPol       = DetectDemandExceptions.Policies.defaultDetectionPolicy
        // let learningPol     = LearnFromDemand.Policies.defaultLearningAnalysisPolicy

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

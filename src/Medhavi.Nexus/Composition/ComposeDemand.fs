module Medhavi.Nexus.Composition.ComposeDemand

open Medhavi.Common
open Medhavi.Infrastructure
open Medhavi.Infrastructure.Stores
open Medhavi.Demand
open Medhavi.Demand.ArsIdentifiers
open Medhavi.Demand.UnderstandDemand
open Medhavi.Foundation.Contracts
open Medhavi.Foundation
open Medhavi.Foundation.Observation
open Medhavi.SemanticModel
open Microsoft.Extensions.DependencyInjection
open Medhavi.Infrastructure.Stores.EnvelopeStore
open Medhavi.Foundation.Failure
open Medhavi.Infrastructure.Projections
open Medhavi.Demand.UnderstandDemand.DemandObservation.Model
open Medhavi.Demand.UnderstandDemand.Context
open Medhavi.Contracts.Demand
open Medhavi.Demand.UnderstandDemand.DemandObservation.Policies
open Medhavi.Demand.UnderstandDemand.DemandUnderstanding.Model

type DemandContext =
    { UnderstandDemandContext: UnderstandDemandContext
      SenseDemand: SenseDemand.Context.SenseDemandContext
      ForecastDemand: ForecastDemand.Context.ForecastDemandContext
      SegmentDemand: SegmentDemand.Context.PlanningClassificationContext
      ClassifyDemand: ClassifyDemand.Context.DemandBehaviorClassificationContext
      PrioritizeDemand: PrioritizeDemand.Context.PlanningPriorityContext
      EvaluateDemandQuality: EvaluateDemandQuality.Context.ForecastQualityContext
      DetectDemandExceptions: DetectDemandExceptions.Context.DemandExceptionContext
      ExplainDemand: ExplainDemand.Context.DemandExplanationContext
      LearnFromDemand: LearnFromDemand.Context.DemandLearningContext
      ModelDemandInterventions: ModelDemandInterventions.Context.DemandInterventionContext
      Dispose: unit -> unit }

    member this.UnderstandDemand = this.UnderstandDemandContext

let composeUnderstandDemand
    (store: EnvelopeStoreOps)
    publishKnowledge
    (telemetryPublisher: TelemetryEvent -> unit)
    (traceCodec: Codec<DecisionTrace>)
    (pconfig: ProjectionConfig)
    (demandPorts: DemandPorts) =

    let repo = InMemRepository.create<DemandObservation, DemandObservationId, ObservationEvent>()
    let codec = Codec.json<ObservationEvent>
    let serializeEvent = codec.Encode >> Result.mapError(fun c -> c.ToString())
    let deps = EnvelopeStoreDeps.create store DemandObservation.ACL.toArsEventId serializeEvent traceCodec
    let obsPolicy = DemandDataAcceptancePolicy.defaultPolicy

    let projectionDeps =
        { Config = pconfig
          EnvelopeStore = store
          EventCodec = codec
          TelemetryPublisher = telemetryPublisher }

    let projectionAgent =
        createProjectionAgent<ObservationEvent, DemandObservationDto, DemandObservationId> projectionDeps

    let repoUnderstand = InMemRepository.create<DemandUnderstanding, PlanningScopeId, DemandUnderstandingEvent>()
    let codecUnderstand = Codec.json<DemandUnderstandingEvent>
    let serializeEventUnderstand = codecUnderstand.Encode >> Result.mapError(fun c -> c.ToString())
    let depsUnderstand = EnvelopeStoreDeps.create store DemandUnderstanding.ACL.toArsEventId serializeEventUnderstand traceCodec

    let projectionDepsUnderstand =
        { Config = pconfig
          EnvelopeStore = store
          EventCodec = codecUnderstand
          TelemetryPublisher = telemetryPublisher }

    let projectionAgentUnderstand =
        createProjectionAgent<DemandUnderstandingEvent, DemandUnderstandingDto, PlanningScopeId>
            projectionDepsUnderstand

    UnderstandDemand.Context.create
        repo
        deps
        projectionAgent
        obsPolicy
        repoUnderstand
        depsUnderstand
        projectionAgentUnderstand
        DemandUnderstanding.Policies.defaultMaterialityPolicy
        DemandUnderstanding.Policies.defaultCadencePolicy
        publishKnowledge
        demandPorts
        "PS-GLOBAL-SUPPLY-CHAIN"

let composeSenseDemand
    (store: EnvelopeStoreOps)
    publishKnowledge
    (telemetryPublisher: TelemetryEvent -> unit)
    (traceCodec: Codec<DecisionTrace>)
    (pconfig: ProjectionConfig)
    (demandPorts: DemandPorts) =

    let repo = InMemRepository.create<SenseDemand.DemandBehaviorAssessment.Model.DemandBehaviorAssessment, DemandBehaviorAssessmentId, SenseDemand.DemandBehaviorAssessment.Model.DemandBehaviorEvent>()
    let codec = Codec.json<SenseDemand.DemandBehaviorAssessment.Model.DemandBehaviorEvent>
    let serializeEvent = codec.Encode >> Result.mapError(fun c -> c.ToString())
    let deps = EnvelopeStoreDeps.create store SenseDemand.DemandBehaviorAssessment.ACL.toArsEventId serializeEvent traceCodec

    let projectionDeps =
        { Config = pconfig
          EnvelopeStore = store
          EventCodec = codec
          TelemetryPublisher = telemetryPublisher }

    let createQry =
        createProjectionAgent<SenseDemand.DemandBehaviorAssessment.Model.DemandBehaviorEvent, DemandBehaviorAssessmentDto, DemandBehaviorAssessmentId> projectionDeps

    SenseDemand.Context.create
        repo
        deps
        publishKnowledge
        createQry
        SenseDemand.DemandBehaviorAssessment.Policies.DemandSensingPolicy.defaultPolicy
        SenseDemand.DemandBehaviorAssessment.Policies.ForecastRefreshTriggerPolicy.defaultPolicy
        demandPorts

let composeForecastDemand
    (store: EnvelopeStoreOps)
    publishKnowledge
    (telemetryPublisher: TelemetryEvent -> unit)
    (traceCodec: Codec<DecisionTrace>)
    (pconfig: ProjectionConfig)
    (demandPorts: DemandPorts) =

    let repo = InMemRepository.create<ForecastDemand.ForecastPublication.Model.ForecastPublication, ForecastPublicationId, ForecastDemand.ForecastPublication.Model.ForecastPublicationEvent>()
    let codec = Codec.json<ForecastDemand.ForecastPublication.Model.ForecastPublicationEvent>
    let serializeEvent = codec.Encode >> Result.mapError(fun c -> c.ToString())
    let deps = EnvelopeStoreDeps.create store ForecastDemand.ForecastPublication.ACL.toArsEventId serializeEvent traceCodec

    let projectionDeps =
        { Config = pconfig
          EnvelopeStore = store
          EventCodec = codec
          TelemetryPublisher = telemetryPublisher }

    let createQry =
        createProjectionAgent<ForecastDemand.ForecastPublication.Model.ForecastPublicationEvent, ForecastPublicationDto, ForecastPublicationId> projectionDeps

    ForecastDemand.Context.create
        repo
        deps
        publishKnowledge
        createQry
        ForecastDemand.ForecastPublication.Policies.ForecastModelGovernancePolicy.defaultPolicy
        ForecastDemand.ForecastPublication.Policies.UnforecastableSeriesPolicy.defaultPolicy
        ForecastDemand.ForecastPublication.Policies.ForecastPublicationGovernancePolicy.defaultPolicy
        ForecastDemand.ForecastPublication.Policies.ForecastOverrideAuthorizationPolicy.defaultPolicy
        ForecastDemand.ForecastPublication.Policies.ForecastModelParametersPolicy.defaultPolicy
        demandPorts
        "PS-GLOBAL-SUPPLY-CHAIN"

let composeSegmentDemand
    (store: EnvelopeStoreOps)
    publishKnowledge
    (telemetryPublisher: TelemetryEvent -> unit)
    (traceCodec: Codec<DecisionTrace>)
    (pconfig: ProjectionConfig)
    (demandPorts: DemandPorts) =

    let repo = InMemRepository.create<SegmentDemand.PlanningClassificationAssignment.Model.PlanningClassificationAssignment, PlanningClassificationAssignmentId, SegmentDemand.PlanningClassificationAssignment.Model.PlanningClassificationEvent>()
    let codec = Codec.json<SegmentDemand.PlanningClassificationAssignment.Model.PlanningClassificationEvent>
    let serializeEvent = codec.Encode >> Result.mapError(fun c -> c.ToString())
    let deps = EnvelopeStoreDeps.create store SegmentDemand.PlanningClassificationAssignment.ACL.toArsEventId serializeEvent traceCodec

    let projectionDeps =
        { Config = pconfig
          EnvelopeStore = store
          EventCodec = codec
          TelemetryPublisher = telemetryPublisher }

    let createQry =
        createProjectionAgent<SegmentDemand.PlanningClassificationAssignment.Model.PlanningClassificationEvent, PlanningClassificationDto, PlanningClassificationAssignmentId> projectionDeps

    SegmentDemand.Context.create
        repo
        deps
        publishKnowledge
        createQry
        SegmentDemand.PlanningClassificationAssignment.Policies.SegmentationPolicy.defaultPolicy
        SegmentDemand.PlanningClassificationAssignment.Policies.SegmentationOverridePolicy.defaultPolicy
        demandPorts

let composeClassifyDemand
    (store: EnvelopeStoreOps)
    publishKnowledge
    (telemetryPublisher: TelemetryEvent -> unit)
    (traceCodec: Codec<DecisionTrace>)
    (pconfig: ProjectionConfig) =

    let repo = InMemRepository.create<ClassifyDemand.DemandBehaviorAssignment.Model.DemandBehaviorAssignment, DemandBehaviorAssignmentId, ClassifyDemand.DemandBehaviorAssignment.Model.DemandBehaviorEvent>()
    let codec = Codec.json<ClassifyDemand.DemandBehaviorAssignment.Model.DemandBehaviorEvent>
    let serializeEvent = codec.Encode >> Result.mapError(fun c -> c.ToString())
    let deps = EnvelopeStoreDeps.create store ClassifyDemand.DemandBehaviorAssignment.ACL.toArsEventId serializeEvent traceCodec

    let projectionDeps =
        { Config = pconfig
          EnvelopeStore = store
          EventCodec = codec
          TelemetryPublisher = telemetryPublisher }

    let createQry =
        createProjectionAgent<ClassifyDemand.DemandBehaviorAssignment.Model.DemandBehaviorEvent, DemandBehaviorAssignmentDto, DemandBehaviorAssignmentId> projectionDeps

    ClassifyDemand.Context.create
        repo
        deps
        publishKnowledge
        createQry
        ClassifyDemand.DemandBehaviorAssignment.Policies.ClassificationPolicy.defaultPolicy
        ClassifyDemand.DemandBehaviorAssignment.Policies.ClassificationOverridePolicy.defaultPolicy
        "LOC-CENTRAL-WH"

let composePrioritizeDemand
    (store: EnvelopeStoreOps)
    publishKnowledge
    (telemetryPublisher: TelemetryEvent -> unit)
    (traceCodec: Codec<DecisionTrace>)
    (pconfig: ProjectionConfig)
    (demandPorts: DemandPorts) =

    let repo = InMemRepository.create<PrioritizeDemand.PlanningPriorityAssignment.Model.PlanningPriorityAssignment, PlanningPriorityAssignmentId, PrioritizeDemand.PlanningPriorityAssignment.Model.PlanningPriorityEvent>()
    let codec = Codec.json<PrioritizeDemand.PlanningPriorityAssignment.Model.PlanningPriorityEvent>
    let serializeEvent = codec.Encode >> Result.mapError(fun c -> c.ToString())
    let deps = EnvelopeStoreDeps.create store PrioritizeDemand.PlanningPriorityAssignment.ACL.toArsEventId serializeEvent traceCodec

    let projectionDeps =
        { Config = pconfig
          EnvelopeStore = store
          EventCodec = codec
          TelemetryPublisher = telemetryPublisher }

    let createQry =
        createProjectionAgent<PrioritizeDemand.PlanningPriorityAssignment.Model.PlanningPriorityEvent, PlanningPriorityDto, PlanningPriorityAssignmentId> projectionDeps

    PrioritizeDemand.Context.create
        repo
        deps
        publishKnowledge
        createQry
        PrioritizeDemand.PlanningPriorityAssignment.Policies.PrioritizationPolicy.defaultPolicy
        PrioritizeDemand.PlanningPriorityAssignment.Policies.PrioritizationOverridePolicy.defaultPolicy
        demandPorts

let composeEvaluateDemandQuality
    (store: EnvelopeStoreOps)
    publishKnowledge
    (telemetryPublisher: TelemetryEvent -> unit)
    (traceCodec: Codec<DecisionTrace>)
    (pconfig: ProjectionConfig) =

    let repo = InMemRepository.create<EvaluateDemandQuality.ForecastQualityAssessment.Model.ForecastQualityAssessment, ForecastQualityAssessmentId, EvaluateDemandQuality.ForecastQualityAssessment.Model.ForecastQualityEvent>()
    let codec = Codec.json<EvaluateDemandQuality.ForecastQualityAssessment.Model.ForecastQualityEvent>
    let serializeEvent = codec.Encode >> Result.mapError(fun c -> c.ToString())
    let deps = EnvelopeStoreDeps.create store EvaluateDemandQuality.ForecastQualityAssessment.ACL.toArsEventId serializeEvent traceCodec

    let projectionDeps =
        { Config = pconfig
          EnvelopeStore = store
          EventCodec = codec
          TelemetryPublisher = telemetryPublisher }

    let createQry =
        createProjectionAgent<EvaluateDemandQuality.ForecastQualityAssessment.Model.ForecastQualityEvent, ForecastQualityAssessmentDto, ForecastQualityAssessmentId> projectionDeps

    EvaluateDemandQuality.Context.create
        repo
        deps
        publishKnowledge
        createQry
        EvaluateDemandQuality.ForecastQualityAssessment.Policies.ForecastMeasurementPolicy.defaultPolicy

let composeDetectDemandExceptions
    (store: EnvelopeStoreOps)
    publishKnowledge
    (telemetryPublisher: TelemetryEvent -> unit)
    (traceCodec: Codec<DecisionTrace>)
    (pconfig: ProjectionConfig) =

    let repo = InMemRepository.create<DetectDemandExceptions.DemandExceptionEvidence.Model.DemandExceptionEvidenceAggregate, DemandExceptionEvidenceId, DetectDemandExceptions.DemandExceptionEvidence.Model.DemandExceptionEvent>()
    let codec = Codec.json<DetectDemandExceptions.DemandExceptionEvidence.Model.DemandExceptionEvent>
    let serializeEvent = codec.Encode >> Result.mapError(fun c -> c.ToString())
    let deps = EnvelopeStoreDeps.create store DetectDemandExceptions.DemandExceptionEvidence.ACL.toArsEventId serializeEvent traceCodec

    let projectionDeps =
        { Config = pconfig
          EnvelopeStore = store
          EventCodec = codec
          TelemetryPublisher = telemetryPublisher }

    let createQry =
        createProjectionAgent<DetectDemandExceptions.DemandExceptionEvidence.Model.DemandExceptionEvent, DemandExceptionEvidenceDto, DemandExceptionEvidenceId> projectionDeps

    DetectDemandExceptions.Context.create
        repo
        deps
        publishKnowledge
        createQry
        DetectDemandExceptions.DemandExceptionEvidence.Policies.DemandExceptionEvidencePolicy.defaultPolicy

let composeExplainDemand
    (store: EnvelopeStoreOps)
    publishKnowledge
    (telemetryPublisher: TelemetryEvent -> unit)
    (traceCodec: Codec<DecisionTrace>)
    (pconfig: ProjectionConfig) =

    let repo = InMemRepository.create<ExplainDemand.DemandExplanation.Model.DemandExplanation, DemandExplanationId, ExplainDemand.DemandExplanation.Model.DemandExplanationEvent>()
    let codec = Codec.json<ExplainDemand.DemandExplanation.Model.DemandExplanationEvent>
    let serializeEvent = codec.Encode >> Result.mapError(fun c -> c.ToString())
    let deps = EnvelopeStoreDeps.create store ExplainDemand.DemandExplanation.ACL.toArsEventId serializeEvent traceCodec

    let projectionDeps =
        { Config = pconfig
          EnvelopeStore = store
          EventCodec = codec
          TelemetryPublisher = telemetryPublisher }

    let createQry =
        createProjectionAgent<ExplainDemand.DemandExplanation.Model.DemandExplanationEvent, DemandExplanationDto, DemandExplanationId> projectionDeps

    ExplainDemand.Context.create
        repo
        deps
        publishKnowledge
        createQry
        ExplainDemand.DemandExplanation.Policies.ExplanationGovernancePolicy.defaultPolicy

let composeLearnFromDemand
    (store: EnvelopeStoreOps)
    publishKnowledge
    (telemetryPublisher: TelemetryEvent -> unit)
    (traceCodec: Codec<DecisionTrace>)
    (pconfig: ProjectionConfig)
    (demandPorts: DemandPorts) =

    let repo = InMemRepository.create<LearnFromDemand.DemandLearning.Model.DemandLearning, DemandLearningId, LearnFromDemand.DemandLearning.Model.DemandLearningEvent>()
    let codec = Codec.json<LearnFromDemand.DemandLearning.Model.DemandLearningEvent>
    let serializeEvent = codec.Encode >> Result.mapError(fun c -> c.ToString())
    let deps = EnvelopeStoreDeps.create store LearnFromDemand.DemandLearning.ACL.toArsEventId serializeEvent traceCodec

    let projectionDeps =
        { Config = pconfig
          EnvelopeStore = store
          EventCodec = codec
          TelemetryPublisher = telemetryPublisher }

    let createQry =
        createProjectionAgent<LearnFromDemand.DemandLearning.Model.DemandLearningEvent, DemandLearningDto, DemandLearningId> projectionDeps

    LearnFromDemand.Context.create
        repo
        deps
        publishKnowledge
        createQry
        LearnFromDemand.DemandLearning.Policies.LearningAnalysisPolicy.defaultPolicy
        demandPorts

let composeModelDemandInterventions
    (store: EnvelopeStoreOps)
    publishKnowledge
    (telemetryPublisher: TelemetryEvent -> unit)
    (traceCodec: Codec<DecisionTrace>)
    (pconfig: ProjectionConfig)
    (demandPorts: DemandPorts) =

    let repo = InMemRepository.create<ModelDemandInterventions.DemandInterventionImpact.Model.DemandInterventionImpact, DemandInterventionImpactId, ModelDemandInterventions.DemandInterventionImpact.Model.DemandInterventionImpactEvent>()
    let codec = Codec.json<ModelDemandInterventions.DemandInterventionImpact.Model.DemandInterventionImpactEvent>
    let serializeEvent = codec.Encode >> Result.mapError(fun c -> c.ToString())
    let deps = EnvelopeStoreDeps.create store ModelDemandInterventions.DemandInterventionImpact.ACL.toArsEventId serializeEvent traceCodec

    let projectionDeps =
        { Config = pconfig
          EnvelopeStore = store
          EventCodec = codec
          TelemetryPublisher = telemetryPublisher }

    let createQry =
        createProjectionAgent<ModelDemandInterventions.DemandInterventionImpact.Model.DemandInterventionImpactEvent, DemandInterventionImpactDto, DemandInterventionImpactId> projectionDeps

    ModelDemandInterventions.Context.create
        repo
        deps
        publishKnowledge
        createQry
        ModelDemandInterventions.DemandInterventionImpact.Policies.InterventionModelingGovernancePolicy.defaultPolicy
        demandPorts
        "SKU-DRIVE-500GB"
        "LOC-CENTRAL-WH"

let compose
    (store: EnvelopeStoreOps)
    publishKnowledge
    (telemetryPublisher: TelemetryEvent -> unit)
    (pconfig: ProjectionConfig)
    (traceCodec: Codec<DecisionTrace>)
    (demandPorts: DemandPorts)
    (services: IServiceCollection)
    : TaskResult<DemandContext, ApplicationError> =
    taskResult {
        // 1. Understand Demand (CA-D-001)
        let! understandCtx:UnderstandDemandContext = composeUnderstandDemand store publishKnowledge telemetryPublisher traceCodec pconfig demandPorts
        // 2. Sense Demand (CA-D-003)
        let! senseCtx:SenseDemand.Context.SenseDemandContext = composeSenseDemand store publishKnowledge telemetryPublisher traceCodec pconfig demandPorts
        // 3. Forecast Demand (CA-D-002)
        let! forecastCtx:ForecastDemand.Context.ForecastDemandContext = composeForecastDemand store publishKnowledge telemetryPublisher traceCodec pconfig demandPorts
        // 4. Segment Demand (CA-D-004)
        let! segmentCtx:SegmentDemand.Context.PlanningClassificationContext = composeSegmentDemand store publishKnowledge telemetryPublisher traceCodec pconfig demandPorts
        // 5. Classify Demand (CA-D-005)
        let! classifyCtx:ClassifyDemand.Context.DemandBehaviorClassificationContext = composeClassifyDemand store publishKnowledge telemetryPublisher traceCodec pconfig
        // 6. Prioritize Demand (CA-D-006)
        let! prioritizeCtx:PrioritizeDemand.Context.PlanningPriorityContext = composePrioritizeDemand store publishKnowledge telemetryPublisher traceCodec pconfig demandPorts
        // 7. Evaluate Demand Quality (CA-D-007)
        let! evaluateCtx:EvaluateDemandQuality.Context.ForecastQualityContext = composeEvaluateDemandQuality store publishKnowledge telemetryPublisher traceCodec pconfig
        // 8. Detect Demand Exceptions (CA-D-008)
        let! detectCtx:DetectDemandExceptions.Context.DemandExceptionContext = composeDetectDemandExceptions store publishKnowledge telemetryPublisher traceCodec pconfig
        // 9. Explain Demand (CA-D-009)
        let! explainCtx:ExplainDemand.Context.DemandExplanationContext = composeExplainDemand store publishKnowledge telemetryPublisher traceCodec pconfig
        // 10. Learn From Demand (CA-D-010)
        let! learnCtx:LearnFromDemand.Context.DemandLearningContext = composeLearnFromDemand store publishKnowledge telemetryPublisher traceCodec pconfig demandPorts
        // 11. Model Demand Interventions (CA-D-011)
        let! interventionCtx:ModelDemandInterventions.Context.DemandInterventionContext = composeModelDemandInterventions store publishKnowledge telemetryPublisher traceCodec pconfig demandPorts

        // Register all contexts in DI for UI and API consumption
        services.AddSingleton<UnderstandDemandContext>(understandCtx) |> ignore
        services.AddSingleton<SenseDemand.Context.SenseDemandContext>(senseCtx) |> ignore
        services.AddSingleton<ForecastDemand.Context.ForecastDemandContext>(forecastCtx) |> ignore
        services.AddSingleton<SegmentDemand.Context.PlanningClassificationContext>(segmentCtx) |> ignore
        services.AddSingleton<ClassifyDemand.Context.DemandBehaviorClassificationContext>(classifyCtx) |> ignore
        services.AddSingleton<PrioritizeDemand.Context.PlanningPriorityContext>(prioritizeCtx) |> ignore
        services.AddSingleton<EvaluateDemandQuality.Context.ForecastQualityContext>(evaluateCtx) |> ignore
        services.AddSingleton<DetectDemandExceptions.Context.DemandExceptionContext>(detectCtx) |> ignore
        services.AddSingleton<ExplainDemand.Context.DemandExplanationContext>(explainCtx) |> ignore
        services.AddSingleton<LearnFromDemand.Context.DemandLearningContext>(learnCtx) |> ignore
        services.AddSingleton<ModelDemandInterventions.Context.DemandInterventionContext>(interventionCtx) |> ignore

        let dispose () =
            understandCtx.Dispose()
            senseCtx.Dispose()
            forecastCtx.Dispose()
            segmentCtx.Dispose()
            classifyCtx.Dispose()
            prioritizeCtx.Dispose()
            evaluateCtx.Dispose()
            detectCtx.Dispose()
            explainCtx.Dispose()
            learnCtx.Dispose()
            interventionCtx.Dispose()

        return
            { UnderstandDemandContext = understandCtx
              SenseDemand = senseCtx
              ForecastDemand = forecastCtx
              SegmentDemand = segmentCtx
              ClassifyDemand = classifyCtx
              PrioritizeDemand = prioritizeCtx
              EvaluateDemandQuality = evaluateCtx
              DetectDemandExceptions = detectCtx
              ExplainDemand = explainCtx
              LearnFromDemand = learnCtx
              ModelDemandInterventions = interventionCtx
              Dispose = dispose }
    }
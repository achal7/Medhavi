module Medhavi.Nexus.Composition.ComposePromise

open System
open System.Threading
open Microsoft.Extensions.DependencyInjection
open Medhavi.Common
open Medhavi.Foundation
open Medhavi.Foundation.Contracts
open Medhavi.Foundation.Execution.AggregateStages
open Medhavi.Foundation.Failure
open Medhavi.Foundation.Observation
open Medhavi.Infrastructure
open Medhavi.Infrastructure.Projections
open Medhavi.Infrastructure.Stores
open Medhavi.Infrastructure.Stores.EnvelopeStore
open Medhavi.SemanticModel
open Medhavi.Contracts.Promise
open Medhavi.Promise

let composeUnderstandOrders
    (store: EnvelopeStoreOps)
    (publishKnowledge: KnowledgeRepresentation)
    (telemetryPublisher: TelemetryEvent -> unit)
    (traceCodec: Codec<DecisionTrace>)
    (pconfig: ProjectionConfig)
    (ports: PromisePorts)
    (defaultScopeId: string)
    : TaskResult<UnderstandOrders.Context.UnderstandOrdersContext, ApplicationError> =
    let orderBookRepo = InMemRepository.create<UnderstandOrders.OrderBookSnapshot.Model.OrderBookSnapshot, OrderBookSnapshotId, UnderstandOrders.OrderBookSnapshot.Model.OrderBookEvent>()
    let orderBookCodec = Codec.json<UnderstandOrders.OrderBookSnapshot.Model.OrderBookEvent>
    let orderBookSerialize = orderBookCodec.Encode >> Result.mapError (fun c -> c.ToString())
    let orderBookDeps = EnvelopeStoreDeps.create store UnderstandOrders.OrderBookSnapshot.ACL.toArsEventId orderBookSerialize traceCodec
    let orderBookProjDeps = { Config = pconfig; EnvelopeStore = store; EventCodec = orderBookCodec; TelemetryPublisher = telemetryPublisher }
    let orderBookQueryService = createProjectionAgent<UnderstandOrders.OrderBookSnapshot.Model.OrderBookEvent, OrderBookSnapshotDto, OrderBookSnapshotId> orderBookProjDeps

    let custProfileRepo = InMemRepository.create<UnderstandOrders.CustomerPromiseProfile.Model.CustomerPromiseProfile, CustomerId, UnderstandOrders.CustomerPromiseProfile.Model.CustomerProfileEvent>()
    let custProfileCodec = Codec.json<UnderstandOrders.CustomerPromiseProfile.Model.CustomerProfileEvent>
    let custProfileSerialize = custProfileCodec.Encode >> Result.mapError (fun c -> c.ToString())
    let custProfileDeps = EnvelopeStoreDeps.create store UnderstandOrders.CustomerPromiseProfile.ACL.toArsEventId custProfileSerialize traceCodec
    let custProfileProjDeps = { Config = pconfig; EnvelopeStore = store; EventCodec = custProfileCodec; TelemetryPublisher = telemetryPublisher }
    let custProfileQueryService = createProjectionAgent<UnderstandOrders.CustomerPromiseProfile.Model.CustomerProfileEvent, CustomerPromiseProfileDto, CustomerId> custProfileProjDeps

    UnderstandOrders.Context.create
        orderBookRepo
        orderBookDeps
        orderBookQueryService
        UnderstandOrders.OrderBookSnapshot.Policies.OrderValidationPolicy.defaultPolicy
        UnderstandOrders.OrderBookSnapshot.Policies.OrderLifecyclePolicy.defaultPolicy
        custProfileRepo
        custProfileDeps
        custProfileQueryService
        UnderstandOrders.CustomerPromiseProfile.Policies.CustomerProfilePolicy.defaultPolicy
        publishKnowledge
        ports
        defaultScopeId

let composeAllocateSupply
    (store: EnvelopeStoreOps)
    (publishKnowledge: KnowledgeRepresentation)
    (telemetryPublisher: TelemetryEvent -> unit)
    (traceCodec: Codec<DecisionTrace>)
    (pconfig: ProjectionConfig)
    (ports: PromisePorts)
    (defaultScopeId: string)
    : TaskResult<AllocateSupply.Context.AllocateSupplyContext, ApplicationError> =
    let allocRepo = InMemRepository.create<AllocateSupply.AllocationPool.Model.AllocationPool, AllocationPoolId, AllocateSupply.AllocationPool.Model.AllocationPoolEvent>()
    let allocCodec = Codec.json<AllocateSupply.AllocationPool.Model.AllocationPoolEvent>
    let allocSerialize = allocCodec.Encode >> Result.mapError (fun c -> c.ToString())
    let allocDeps = EnvelopeStoreDeps.create store AllocateSupply.AllocationPool.ACL.toArsEventId allocSerialize traceCodec
    let allocProjDeps = { Config = pconfig; EnvelopeStore = store; EventCodec = allocCodec; TelemetryPublisher = telemetryPublisher }
    let allocQueryService = createProjectionAgent<AllocateSupply.AllocationPool.Model.AllocationPoolEvent, AllocationPoolDto, AllocationPoolId> allocProjDeps

    AllocateSupply.Context.create
        allocRepo
        allocDeps
        allocQueryService
        AllocateSupply.AllocationPool.Policies.defaultAllocationGovernancePolicy
        AllocateSupply.AllocationPool.Policies.defaultChannelOverdrawPolicy
        AllocateSupply.AllocationPool.Policies.defaultDynamicPriorityPolicy
        publishKnowledge
        ports
        defaultScopeId

let composeReserveInventory
    (store: EnvelopeStoreOps)
    (publishKnowledge: KnowledgeRepresentation)
    (telemetryPublisher: TelemetryEvent -> unit)
    (traceCodec: Codec<DecisionTrace>)
    (pconfig: ProjectionConfig)
    (ports: PromisePorts)
    (defaultScopeId: string)
    : TaskResult<ReserveInventory.Context.ReserveInventoryContext, ApplicationError> =
    let resRepo = InMemRepository.create<ReserveInventory.TemporaryReservation.Model.TemporaryReservation, TemporaryReservationId, ReserveInventory.TemporaryReservation.Model.TemporaryReservationEvent>()
    let resCodec = Codec.json<ReserveInventory.TemporaryReservation.Model.TemporaryReservationEvent>
    let resSerialize = resCodec.Encode >> Result.mapError (fun c -> c.ToString())
    let resDeps = EnvelopeStoreDeps.create store ReserveInventory.TemporaryReservation.ACL.toArsEventId resSerialize traceCodec
    let resProjDeps = { Config = pconfig; EnvelopeStore = store; EventCodec = resCodec; TelemetryPublisher = telemetryPublisher }
    let resQueryService = createProjectionAgent<ReserveInventory.TemporaryReservation.Model.TemporaryReservationEvent, TemporaryReservationDto, TemporaryReservationId> resProjDeps

    ReserveInventory.Context.create
        resRepo
        resDeps
        resQueryService
        ReserveInventory.TemporaryReservation.Policies.defaultTemporaryReservationPolicy
        publishKnowledge
        ports
        defaultScopeId

let composeEvaluateAtp
    (store: EnvelopeStoreOps)
    (traceCodec: Codec<DecisionTrace>)
    (ports: PromisePorts)
    (defaultScopeId: string)
    : TaskResult<EvaluateATP.Context.EvaluateATPContext, ApplicationError> =
    let atpCodec = Codec.json<EvaluateATP.Model.PromiseEvaluationEvent>
    let atpSerialize = atpCodec.Encode >> Result.mapError (fun c -> c.ToString())
    let atpDeps = EnvelopeStoreDeps.create store EvaluateATP.ACL.toArsEventId atpSerialize traceCodec

    EvaluateATP.Context.create
        atpDeps
        EvaluateATP.Policies.defaultMultiOptionPolicy
        ports
        defaultScopeId

let composeEvaluateCtp
    (store: EnvelopeStoreOps)
    (traceCodec: Codec<DecisionTrace>)
    (ports: PromisePorts)
    (defaultScopeId: string)
    : TaskResult<EvaluateCTP.Context.EvaluateCTPContext, ApplicationError> =
    let ctpCodec = Codec.json<EvaluateCTP.Model.CTPEvent>
    let ctpSerialize = ctpCodec.Encode >> Result.mapError (fun c -> c.ToString())
    let ctpDeps = EnvelopeStoreDeps.create store EvaluateCTP.ACL.toArsEventId ctpSerialize traceCodec

    EvaluateCTP.Context.create
        ctpDeps
        EvaluateCTP.Policies.defaultCTPPolicy
        EvaluateATP.Policies.defaultMultiOptionPolicy
        ports
        defaultScopeId

let composeEstablishCommitments
    (store: EnvelopeStoreOps)
    (publishKnowledge: KnowledgeRepresentation)
    (telemetryPublisher: TelemetryEvent -> unit)
    (traceCodec: Codec<DecisionTrace>)
    (pconfig: ProjectionConfig)
    (ports: PromisePorts)
    (defaultScopeId: string)
    (commRepo: Repository<EstablishCommitments.OrderCommitment.Model.OrderCommitment, OrderCommitmentId, EstablishCommitments.OrderCommitment.Model.OrderCommitmentEvent>)
    : TaskResult<EstablishCommitments.Context.EstablishCommitmentsContext, ApplicationError> =
    let commCodec = Codec.json<EstablishCommitments.OrderCommitment.Model.OrderCommitmentEvent>
    let commSerialize = commCodec.Encode >> Result.mapError (fun c -> c.ToString())
    let commDeps = EnvelopeStoreDeps.create store EstablishCommitments.OrderCommitment.ACL.toArsEventId commSerialize traceCodec
    let commProjDeps = { Config = pconfig; EnvelopeStore = store; EventCodec = commCodec; TelemetryPublisher = telemetryPublisher }
    let commQueryService = createProjectionAgent<EstablishCommitments.OrderCommitment.Model.OrderCommitmentEvent, OrderCommitmentDto, OrderCommitmentId> commProjDeps

    let explRepo = InMemRepository.create<EstablishCommitments.PromiseExplanation.Model.PromiseExplanation, PromiseExplanationId, EstablishCommitments.PromiseExplanation.Model.PromiseExplanationEvent>()
    let explCodec = Codec.json<EstablishCommitments.PromiseExplanation.Model.PromiseExplanationEvent>
    let explSerialize = explCodec.Encode >> Result.mapError (fun c -> c.ToString())
    let explDeps = EnvelopeStoreDeps.create store EstablishCommitments.PromiseExplanation.ACL.toArsEventId explSerialize traceCodec
    let explProjDeps = { Config = pconfig; EnvelopeStore = store; EventCodec = explCodec; TelemetryPublisher = telemetryPublisher }
    let explQueryService = createProjectionAgent<EstablishCommitments.PromiseExplanation.Model.PromiseExplanationEvent, PromiseExplanationDto, PromiseExplanationId> explProjDeps

    EstablishCommitments.Context.create
        commRepo
        commDeps
        commQueryService
        EstablishCommitments.OrderCommitment.Policies.defaultCommitmentPolicy
        explRepo
        explDeps
        explQueryService
        publishKnowledge
        ports
        defaultScopeId

let composeAssessPromiseRisk
    (store: EnvelopeStoreOps)
    (publishKnowledge: KnowledgeRepresentation)
    (telemetryPublisher: TelemetryEvent -> unit)
    (traceCodec: Codec<DecisionTrace>)
    (pconfig: ProjectionConfig)
    (ports: PromisePorts)
    (commRepo: Repository<EstablishCommitments.OrderCommitment.Model.OrderCommitment, OrderCommitmentId, EstablishCommitments.OrderCommitment.Model.OrderCommitmentEvent>)
    (scopeId: PlanningScopeId)
    : TaskResult<AssessPromiseRisk.Context.AssessPromiseRiskContext, ApplicationError> =
    let riskRepo = InMemRepository.create<AssessPromiseRisk.PromiseRiskAssessment.Model.PromiseRiskAssessment, PromiseRiskAssessmentId, AssessPromiseRisk.PromiseRiskAssessment.Model.PromiseRiskEvent>()
    let riskCodec = Codec.json<AssessPromiseRisk.PromiseRiskAssessment.Model.PromiseRiskEvent>
    let riskSerialize = riskCodec.Encode >> Result.mapError (fun c -> c.ToString())
    let riskDeps = EnvelopeStoreDeps.create store AssessPromiseRisk.PromiseRiskAssessment.ACL.toArsEventId riskSerialize traceCodec
    let riskProjDeps = { Config = pconfig; EnvelopeStore = store; EventCodec = riskCodec; TelemetryPublisher = telemetryPublisher }
    let riskQueryService = createProjectionAgent<AssessPromiseRisk.PromiseRiskAssessment.Model.PromiseRiskEvent, PromiseRiskAssessmentDto, PromiseRiskAssessmentId> riskProjDeps

    AssessPromiseRisk.Context.create
        riskRepo
        riskDeps
        riskQueryService
        AssessPromiseRisk.PromiseRiskAssessment.Policies.defaultPolicy
        publishKnowledge
        ports
        commRepo
        scopeId

let composeManageBackorders
    (store: EnvelopeStoreOps)
    (publishKnowledge: KnowledgeRepresentation)
    (telemetryPublisher: TelemetryEvent -> unit)
    (traceCodec: Codec<DecisionTrace>)
    (pconfig: ProjectionConfig)
    (ports: PromisePorts)
    (scopeId: PlanningScopeId)
    : TaskResult<ManageBackorders.Context.ManageBackordersContext, ApplicationError> =
    let boRepo = InMemRepository.create<ManageBackorders.BackorderRecord.Model.BackorderRecord, BackorderId, ManageBackorders.BackorderRecord.Model.BackorderEvent>()
    let boCodec = Codec.json<ManageBackorders.BackorderRecord.Model.BackorderEvent>
    let boSerialize = boCodec.Encode >> Result.mapError (fun c -> c.ToString())
    let boDeps = EnvelopeStoreDeps.create store ManageBackorders.BackorderRecord.ACL.toArsEventId boSerialize traceCodec
    let boProjDeps = { Config = pconfig; EnvelopeStore = store; EventCodec = boCodec; TelemetryPublisher = telemetryPublisher }
    let boQueryService = createProjectionAgent<ManageBackorders.BackorderRecord.Model.BackorderEvent, BackorderRecordDto, BackorderId> boProjDeps

    ManageBackorders.Context.create
        boRepo
        boDeps
        boQueryService
        ManageBackorders.BackorderRecord.Policies.defaultPolicy
        publishKnowledge
        ports
        scopeId

let composeResolveExceptions
    (store: EnvelopeStoreOps)
    (publishKnowledge: KnowledgeRepresentation)
    (telemetryPublisher: TelemetryEvent -> unit)
    (traceCodec: Codec<DecisionTrace>)
    (pconfig: ProjectionConfig)
    (commRepo: Repository<EstablishCommitments.OrderCommitment.Model.OrderCommitment, OrderCommitmentId, EstablishCommitments.OrderCommitment.Model.OrderCommitmentEvent>)
    (scopeId: PlanningScopeId)
    : TaskResult<ResolveExceptions.Context.ResolveExceptionsContext, ApplicationError> =
    let excRepo = InMemRepository.create<ResolveExceptions.PromiseExceptionAssessment.Model.PromiseExceptionAssessment, PromiseExceptionId, ResolveExceptions.PromiseExceptionAssessment.Model.PromiseExceptionEvent>()
    let excCodec = Codec.json<ResolveExceptions.PromiseExceptionAssessment.Model.PromiseExceptionEvent>
    let excSerialize = excCodec.Encode >> Result.mapError (fun c -> c.ToString())
    let excDeps = EnvelopeStoreDeps.create store ResolveExceptions.PromiseExceptionAssessment.ACL.toArsEventId excSerialize traceCodec
    let excProjDeps = { Config = pconfig; EnvelopeStore = store; EventCodec = excCodec; TelemetryPublisher = telemetryPublisher }
    let excQueryService = createProjectionAgent<ResolveExceptions.PromiseExceptionAssessment.Model.PromiseExceptionEvent, PromiseExceptionAssessmentDto, PromiseExceptionId> excProjDeps

    ResolveExceptions.Context.create
        excRepo
        excDeps
        excQueryService
        ResolveExceptions.PromiseExceptionAssessment.Policies.defaultPolicy
        publishKnowledge
        commRepo
        scopeId

let composeMeasurePerformance
    (store: EnvelopeStoreOps)
    (publishKnowledge: KnowledgeRepresentation)
    (telemetryPublisher: TelemetryEvent -> unit)
    (traceCodec: Codec<DecisionTrace>)
    (pconfig: ProjectionConfig)
    : TaskResult<MeasurePerformance.Context.MeasurePerformanceContext, ApplicationError> =
    let qaRepo = InMemRepository.create<MeasurePerformance.PromiseQualityAssessment.Model.PromiseQualityAssessment, PromiseQualityAssessmentId, MeasurePerformance.PromiseQualityAssessment.Model.CommitmentQualityEvent>()
    let qaCodec = Codec.json<MeasurePerformance.PromiseQualityAssessment.Model.CommitmentQualityEvent>
    let qaSerialize = qaCodec.Encode >> Result.mapError (fun c -> c.ToString())
    let qaDeps = EnvelopeStoreDeps.create store MeasurePerformance.PromiseQualityAssessment.ACL.toArsEventId qaSerialize traceCodec
    let qaProjDeps = { Config = pconfig; EnvelopeStore = store; EventCodec = qaCodec; TelemetryPublisher = telemetryPublisher }
    let qaQueryService = createProjectionAgent<MeasurePerformance.PromiseQualityAssessment.Model.CommitmentQualityEvent, PromiseQualityAssessmentDto, PromiseQualityAssessmentId> qaProjDeps

    MeasurePerformance.Context.create
        qaRepo
        qaDeps
        qaQueryService
        MeasurePerformance.PromiseQualityAssessment.Policies.SlaMeasurementPolicy.defaultPolicy
        publishKnowledge

let composeLearnFromPromises
    (store: EnvelopeStoreOps)
    (publishKnowledge: KnowledgeRepresentation)
    (telemetryPublisher: TelemetryEvent -> unit)
    (traceCodec: Codec<DecisionTrace>)
    (pconfig: ProjectionConfig)
    : TaskResult<LearnFromPromises.Context.LearnFromPromisesContext, ApplicationError> =
    let learnRepo = InMemRepository.create<LearnFromPromises.PromiseCalibrationModel.Model.PromiseCalibrationModel, PromiseLearningId, LearnFromPromises.PromiseCalibrationModel.Model.PromiseCalibrationEvent>()
    let learnCodec = Codec.json<LearnFromPromises.PromiseCalibrationModel.Model.PromiseCalibrationEvent>
    let learnSerialize = learnCodec.Encode >> Result.mapError (fun c -> c.ToString())
    let learnDeps = EnvelopeStoreDeps.create store LearnFromPromises.PromiseCalibrationModel.ACL.toArsEventId learnSerialize traceCodec
    let learnProjDeps = { Config = pconfig; EnvelopeStore = store; EventCodec = learnCodec; TelemetryPublisher = telemetryPublisher }
    let learnQueryService = createProjectionAgent<LearnFromPromises.PromiseCalibrationModel.Model.PromiseCalibrationEvent, PromiseCalibrationModelDto, PromiseLearningId> learnProjDeps

    LearnFromPromises.Context.create
        learnRepo
        learnDeps
        learnQueryService
        LearnFromPromises.PromiseCalibrationModel.Policies.PromisingModelCalibrationPolicy.defaultPolicy
        publishKnowledge

let compose
    (store: EnvelopeStoreOps)
    (publishKnowledge: KnowledgeRepresentation)
    (telemetryPublisher: TelemetryEvent -> unit)
    (traceCodec: Codec<DecisionTrace>)
    (pconfig: ProjectionConfig)
    (ports: PromisePorts)
    (defaultScopeId: string)
    (services: IServiceCollection)
    : TaskResult<PromiseContext, ApplicationError> =
    taskResult {
        let scopeId = PlanningScopeId.create defaultScopeId |> Result.defaultWith (fun _ -> failwith "invalid scope")
        let commRepo = InMemRepository.create<EstablishCommitments.OrderCommitment.Model.OrderCommitment, OrderCommitmentId, EstablishCommitments.OrderCommitment.Model.OrderCommitmentEvent>()

        let! (understandOrdersCtx: UnderstandOrders.Context.UnderstandOrdersContext) =
            composeUnderstandOrders store publishKnowledge telemetryPublisher traceCodec pconfig ports defaultScopeId

        let! (allocateSupplyCtx: AllocateSupply.Context.AllocateSupplyContext) =
            composeAllocateSupply store publishKnowledge telemetryPublisher traceCodec pconfig ports defaultScopeId

        let! (reserveInventoryCtx: ReserveInventory.Context.ReserveInventoryContext) =
            composeReserveInventory store publishKnowledge telemetryPublisher traceCodec pconfig ports defaultScopeId

        let! (evaluateAtpCtx: EvaluateATP.Context.EvaluateATPContext) =
            composeEvaluateAtp store traceCodec ports defaultScopeId

        let! (evaluateCtpCtx: EvaluateCTP.Context.EvaluateCTPContext) =
            composeEvaluateCtp store traceCodec ports defaultScopeId

        let! (establishCommitmentsCtx: EstablishCommitments.Context.EstablishCommitmentsContext) =
            composeEstablishCommitments store publishKnowledge telemetryPublisher traceCodec pconfig ports defaultScopeId commRepo

        let! (assessPromiseRiskCtx: AssessPromiseRisk.Context.AssessPromiseRiskContext) =
            composeAssessPromiseRisk store publishKnowledge telemetryPublisher traceCodec pconfig ports commRepo scopeId

        let! (manageBackordersCtx: ManageBackorders.Context.ManageBackordersContext) =
            composeManageBackorders store publishKnowledge telemetryPublisher traceCodec pconfig ports scopeId

        let! (resolveExceptionsCtx: ResolveExceptions.Context.ResolveExceptionsContext) =
            composeResolveExceptions store publishKnowledge telemetryPublisher traceCodec pconfig commRepo scopeId

        let! (measurePerformanceCtx: MeasurePerformance.Context.MeasurePerformanceContext) =
            composeMeasurePerformance store publishKnowledge telemetryPublisher traceCodec pconfig

        let! (learnFromPromisesCtx: LearnFromPromises.Context.LearnFromPromisesContext) =
            composeLearnFromPromises store publishKnowledge telemetryPublisher traceCodec pconfig

        // Register all Promise capability contexts into DI for UI and API consumption
        services.AddSingleton<UnderstandOrders.Context.UnderstandOrdersContext>(understandOrdersCtx) |> ignore
        services.AddSingleton<AllocateSupply.Context.AllocateSupplyContext>(allocateSupplyCtx) |> ignore
        services.AddSingleton<ReserveInventory.Context.ReserveInventoryContext>(reserveInventoryCtx) |> ignore
        services.AddSingleton<EvaluateATP.Context.EvaluateATPContext>(evaluateAtpCtx) |> ignore
        services.AddSingleton<EvaluateCTP.Context.EvaluateCTPContext>(evaluateCtpCtx) |> ignore
        services.AddSingleton<EstablishCommitments.Context.EstablishCommitmentsContext>(establishCommitmentsCtx) |> ignore
        services.AddSingleton<AssessPromiseRisk.Context.AssessPromiseRiskContext>(assessPromiseRiskCtx) |> ignore
        services.AddSingleton<ManageBackorders.Context.ManageBackordersContext>(manageBackordersCtx) |> ignore
        services.AddSingleton<ResolveExceptions.Context.ResolveExceptionsContext>(resolveExceptionsCtx) |> ignore
        services.AddSingleton<MeasurePerformance.Context.MeasurePerformanceContext>(measurePerformanceCtx) |> ignore
        services.AddSingleton<LearnFromPromises.Context.LearnFromPromisesContext>(learnFromPromisesCtx) |> ignore

        let dispose () =
            understandOrdersCtx.Dispose ()
            allocateSupplyCtx.Dispose ()
            reserveInventoryCtx.Dispose ()
            evaluateAtpCtx.Dispose ()
            evaluateCtpCtx.Dispose ()
            establishCommitmentsCtx.Dispose ()
            assessPromiseRiskCtx.Dispose ()
            manageBackordersCtx.Dispose ()
            resolveExceptionsCtx.Dispose ()
            measurePerformanceCtx.Dispose ()
            learnFromPromisesCtx.Dispose ()

        let promiseContext: PromiseContext =
            { UnderstandOrders = understandOrdersCtx
              AllocateSupply = allocateSupplyCtx
              ReserveInventory = reserveInventoryCtx
              EvaluateATP = evaluateAtpCtx
              EvaluateCTP = evaluateCtpCtx
              EstablishCommitments = establishCommitmentsCtx
              AssessPromiseRisk = assessPromiseRiskCtx
              ManageBackorders = manageBackordersCtx
              ResolveExceptions = resolveExceptionsCtx
              MeasurePerformance = measurePerformanceCtx
              LearnFromPromises = learnFromPromisesCtx
              Dispose = dispose }

        services.AddSingleton<PromiseContext>(promiseContext) |> ignore
        return promiseContext
    }

module Medhavi.Nexus.Composition.ComposeSupply

open System
open System.Threading
open System.Threading.Tasks
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
open Medhavi.Contracts.Supply
open Medhavi.Supply
open Medhavi.Supply.ArsIdentifiers

type SupplyContext =
    { UnderstandSupply: UnderstandSupply.Context.UnderstandSupplyContext
      PlanSupply: PlanSupply.Context.PlanSupplyContext
      ManageInventory: ManageInventory.Context.ManageInventoryContext
      ManageCapacity: ManageCapacity.ManageCapacityContext
      CollaborateWithSuppliers: CollaborateWithSuppliers.CollaborateWithSuppliersContext
      ProcureMaterials: ProcureMaterials.ProcureMaterialsContext
      ScheduleProduction: ScheduleProduction.ScheduleProductionContext
      ManageDistribution: ManageDistribution.Context.ManageDistributionContext
      SenseSupplyChanges: SenseSupplyChanges.Context.SenseSupplyChangesContext
      EvaluateSupplyQuality: EvaluateSupplyQuality.Context.EvaluateSupplyQualityContext
      DetectSupplyExceptions: DetectSupplyExceptions.Context.DetectSupplyExceptionsContext
      ExplainSupplyDecisions: ExplainSupplyDecisions.Context.ExplainSupplyDecisionsContext
      LearnFromSupply: LearnFromSupply.SupplyLearningContext
      Dispose: unit -> unit }

let composeUnderstandSupply
    (store: EnvelopeStoreOps)
    publishKnowledge
    (telemetryPublisher: TelemetryEvent -> unit)
    (traceCodec: Codec<DecisionTrace>)
    (pconfig: ProjectionConfig)
    (ports: SupplyPorts) =

    let recRepo = InMemRepository.create<UnderstandSupply.SupplyDataRecord.Model.SupplyDataRecord, SupplyDataRecordId, UnderstandSupply.SupplyDataRecord.Model.RecordEvent>()
    let recCodec = Codec.json<UnderstandSupply.SupplyDataRecord.Model.RecordEvent>
    let recSerialize = recCodec.Encode >> Result.mapError (fun c -> c.ToString())
    let recDeps = EnvelopeStoreDeps.create store UnderstandSupply.SupplyDataRecord.ACL.toArsEventId recSerialize traceCodec
    let recPolicy = UnderstandSupply.SupplyDataRecord.Policies.SupplyDataAcceptancePolicy.defaultPolicy

    let recProjDeps =
        { Config = pconfig
          EnvelopeStore = store
          EventCodec = recCodec
          TelemetryPublisher = telemetryPublisher }

    let recQueryService =
        createProjectionAgent<UnderstandSupply.SupplyDataRecord.Model.RecordEvent, SupplyDataRecordDto, SupplyDataRecordId> recProjDeps

    let undRepo = InMemRepository.create<UnderstandSupply.SupplyUnderstanding.Model.SupplyUnderstanding, PlanningScopeId, UnderstandSupply.SupplyUnderstanding.Model.SupplyUnderstandingEvent>()
    let undCodec = Codec.json<UnderstandSupply.SupplyUnderstanding.Model.SupplyUnderstandingEvent>
    let undSerialize = undCodec.Encode >> Result.mapError (fun c -> c.ToString())
    let undDeps = EnvelopeStoreDeps.create store UnderstandSupply.SupplyUnderstanding.ACL.toArsEventId undSerialize traceCodec

    let undProjDeps =
        { Config = pconfig
          EnvelopeStore = store
          EventCodec = undCodec
          TelemetryPublisher = telemetryPublisher }

    let undQueryService =
        createProjectionAgent<UnderstandSupply.SupplyUnderstanding.Model.SupplyUnderstandingEvent, SupplyUnderstandingDto, PlanningScopeId> undProjDeps

    UnderstandSupply.Context.create
        recRepo
        recDeps
        recQueryService
        recPolicy
        undRepo
        undDeps
        undQueryService
        UnderstandSupply.SupplyUnderstanding.Policies.MaterialityPolicy.defaultPolicy
        UnderstandSupply.SupplyUnderstanding.Policies.CadencePolicy.defaultPolicy
        publishKnowledge
        ports
        "PS-GLOBAL-SUPPLY-CHAIN"

let composePlanSupply
    (store: EnvelopeStoreOps)
    publishKnowledge
    (telemetryPublisher: TelemetryEvent -> unit)
    (traceCodec: Codec<DecisionTrace>)
    (pconfig: ProjectionConfig)
    (ports: SupplyPorts) =

    let repo = InMemRepository.create<PlanSupply.SupplyPlan.Model.SupplyPlan, SupplyPlanId, PlanSupply.SupplyPlan.Model.SupplyPlanEvent>()
    let codec = Codec.json<PlanSupply.SupplyPlan.Model.SupplyPlanEvent>
    let serializeEvent = codec.Encode >> Result.mapError (fun c -> c.ToString())
    let deps = EnvelopeStoreDeps.create store PlanSupply.SupplyPlan.ACL.toArsEventId serializeEvent traceCodec

    let projDeps =
        { Config = pconfig
          EnvelopeStore = store
          EventCodec = codec
          TelemetryPublisher = telemetryPublisher }

    let queryService =
        createProjectionAgent<PlanSupply.SupplyPlan.Model.SupplyPlanEvent, SupplyPlanDto, SupplyPlanId> projDeps

    PlanSupply.Context.create
        repo
        deps
        queryService
        PlanSupply.SupplyPlan.Policies.ConstraintRelaxationPolicy.defaultPolicy
        PlanSupply.SupplyPlan.Policies.SupplyPlanAcceptancePolicy.defaultPolicy
        PlanSupply.SupplyPlan.Policies.SupplyPlanPublicationCadencePolicy.defaultPolicy
        publishKnowledge
        ports
        "PS-GLOBAL-SUPPLY-CHAIN"
        "HORIZON-90D"

let composeManageInventory
    (store: EnvelopeStoreOps)
    publishKnowledge
    (telemetryPublisher: TelemetryEvent -> unit)
    (traceCodec: Codec<DecisionTrace>)
    (pconfig: ProjectionConfig)
    (ports: SupplyPorts) =

    let posRepo = InMemRepository.create<ManageInventory.InventoryPosition.Model.InventoryPositionAssessment, ItemLocationId, ManageInventory.InventoryPosition.Model.InventoryPositionEvent>()
    let posCodec = Codec.json<ManageInventory.InventoryPosition.Model.InventoryPositionEvent>
    let posSerialize = posCodec.Encode >> Result.mapError (fun c -> c.ToString())
    let posToArsId = function
        | ManageInventory.InventoryPosition.Model.InventoryPositionAssessmentChanged _ -> ArsIdentifiers.EnterpriseEvents.inventoryPositionAssessmentChanged.Id
    let posDeps = EnvelopeStoreDeps.create store posToArsId posSerialize traceCodec
    let posProjDeps = { Config = pconfig; EnvelopeStore = store; EventCodec = posCodec; TelemetryPublisher = telemetryPublisher }
    let posQueryService = createProjectionAgent<ManageInventory.InventoryPosition.Model.InventoryPositionEvent, InventoryPositionAssessmentDto, ItemLocationId> posProjDeps

    let polRepo = InMemRepository.create<ManageInventory.InventoryPolicy.Model.InventoryPolicyAssignment, ItemLocationId, ManageInventory.InventoryPolicy.Model.InventoryPolicyEvent>()
    let polCodec = Codec.json<ManageInventory.InventoryPolicy.Model.InventoryPolicyEvent>
    let polSerialize = polCodec.Encode >> Result.mapError (fun c -> c.ToString())
    let polToArsId = function
        | ManageInventory.InventoryPolicy.Model.InventoryPolicyAssignmentUpdated _ -> ArsIdentifiers.EnterpriseEvents.inventoryPolicyAssignmentUpdated.Id
    let polDeps = EnvelopeStoreDeps.create store polToArsId polSerialize traceCodec
    let polProjDeps = { Config = pconfig; EnvelopeStore = store; EventCodec = polCodec; TelemetryPublisher = telemetryPublisher }
    let polQueryService = createProjectionAgent<ManageInventory.InventoryPolicy.Model.InventoryPolicyEvent, InventoryPolicyAssignmentDto, ItemLocationId> polProjDeps

    let healthRepo = InMemRepository.create<ManageInventory.InventoryHealth.Model.InventoryHealthAssessment, InventoryHealthAssessmentId, ManageInventory.InventoryHealth.Model.InventoryHealthEvent>()
    let healthCodec = Codec.json<ManageInventory.InventoryHealth.Model.InventoryHealthEvent>
    let healthSerialize = healthCodec.Encode >> Result.mapError (fun c -> c.ToString())
    let healthToArsId = function
        | ManageInventory.InventoryHealth.Model.InventoryHealthAssessmentPublished _ -> ArsIdentifiers.EnterpriseEvents.inventoryHealthAssessmentPublished.Id
    let healthDeps = EnvelopeStoreDeps.create store healthToArsId healthSerialize traceCodec
    let healthProjDeps = { Config = pconfig; EnvelopeStore = store; EventCodec = healthCodec; TelemetryPublisher = telemetryPublisher }
    let healthQueryService = createProjectionAgent<ManageInventory.InventoryHealth.Model.InventoryHealthEvent, InventoryHealthAssessmentDto, InventoryHealthAssessmentId> healthProjDeps

    ManageInventory.Context.create
        posRepo
        posDeps
        posQueryService
        polRepo
        polDeps
        polQueryService
        healthRepo
        healthDeps
        healthQueryService
        ManageInventory.InventoryPolicy.Policies.InventoryPolicyGovernancePolicy.defaultPolicy
        ManageInventory.InventoryHealth.Policies.InventoryHealthAssessmentPolicy.defaultPolicy
        publishKnowledge
        ports
        "PS-GLOBAL-SUPPLY-CHAIN"
        "P-2026-09"
        "LOC-CENTRAL-WH"

let composeManageCapacity
    (store: EnvelopeStoreOps)
    publishKnowledge
    (telemetryPublisher: TelemetryEvent -> unit)
    (traceCodec: Codec<DecisionTrace>)
    (pconfig: ProjectionConfig)
    (ports: SupplyPorts) =

    let posRepo = InMemRepository.create<ManageCapacity.CapacityPosition.Model.CapacityPositionAssessment, PhysicalResourceId, ManageCapacity.CapacityPosition.Model.CapacityPositionEvent>()
    let posCodec = Codec.json<ManageCapacity.CapacityPosition.Model.CapacityPositionEvent>
    let posSerialize = posCodec.Encode >> Result.mapError (fun c -> c.ToString())
    let posToArsId = function
        | ManageCapacity.CapacityPosition.Model.CapacityPositionAssessmentChanged _ -> ArsIdentifiers.EnterpriseEvents.capacityPositionAssessmentChanged.Id
    let posDeps = EnvelopeStoreDeps.create store posToArsId posSerialize traceCodec
    let posProjDeps = { Config = pconfig; EnvelopeStore = store; EventCodec = posCodec; TelemetryPublisher = telemetryPublisher }
    let posQueryService = createProjectionAgent<ManageCapacity.CapacityPosition.Model.CapacityPositionEvent, CapacityPositionAssessmentDto, PhysicalResourceId> posProjDeps

    let healthRepo = InMemRepository.create<ManageCapacity.CapacityHealth.Model.CapacityHealthAssessment, CapacityHealthAssessmentId, ManageCapacity.CapacityHealth.Model.CapacityHealthEvent>()
    let healthCodec = Codec.json<ManageCapacity.CapacityHealth.Model.CapacityHealthEvent>
    let healthSerialize = healthCodec.Encode >> Result.mapError (fun c -> c.ToString())
    let healthToArsId = function
        | ManageCapacity.CapacityHealth.Model.CapacityHealthAssessmentPublished _ -> ArsIdentifiers.EnterpriseEvents.capacityHealthAssessmentPublished.Id
    let healthDeps = EnvelopeStoreDeps.create store healthToArsId healthSerialize traceCodec
    let healthProjDeps = { Config = pconfig; EnvelopeStore = store; EventCodec = healthCodec; TelemetryPublisher = telemetryPublisher }
    let healthQueryService = createProjectionAgent<ManageCapacity.CapacityHealth.Model.CapacityHealthEvent, CapacityHealthAssessmentDto, CapacityHealthAssessmentId> healthProjDeps

    ManageCapacity.Context.create
        posRepo
        posDeps
        posQueryService
        healthRepo
        healthDeps
        healthQueryService
        ManageCapacity.CapacityPosition.Policies.defaultPolicy
        ManageCapacity.CapacityHealth.Policies.defaultPolicy
        publishKnowledge
        ports
        "WC-ASSEMBLY-LINE-01"
        "PS-GLOBAL-SUPPLY-CHAIN"
        "P-2026-09"

let composeCollaborateWithSuppliers
    (store: EnvelopeStoreOps)
    publishKnowledge
    (telemetryPublisher: TelemetryEvent -> unit)
    (traceCodec: Codec<DecisionTrace>)
    (pconfig: ProjectionConfig)
    (ports: SupplyPorts) =

    let commitRepo = InMemRepository.create<CollaborateWithSuppliers.SupplierCommitment.Model.SupplierCommitment, SupplierCommitmentId, CollaborateWithSuppliers.SupplierCommitment.Model.SupplierCommitmentEvent>()
    let commitCodec = Codec.json<CollaborateWithSuppliers.SupplierCommitment.Model.SupplierCommitmentEvent>
    let commitSerialize = commitCodec.Encode >> Result.mapError (fun c -> c.ToString())
    let commitToArsId = function
        | CollaborateWithSuppliers.SupplierCommitment.Model.SupplierCommitmentChanged _ -> ArsIdentifiers.EnterpriseEvents.supplierCommitmentChanged.Id
    let commitDeps = EnvelopeStoreDeps.create store commitToArsId commitSerialize traceCodec
    let commitProjDeps = { Config = pconfig; EnvelopeStore = store; EventCodec = commitCodec; TelemetryPublisher = telemetryPublisher }
    let commitQueryService = createProjectionAgent<CollaborateWithSuppliers.SupplierCommitment.Model.SupplierCommitmentEvent, SupplierCommitmentDto, SupplierCommitmentId> commitProjDeps

    let assessRepo = InMemRepository.create<CollaborateWithSuppliers.SupplierCommitmentAssessment.Model.SupplierCommitmentAssessment, SupplierId, CollaborateWithSuppliers.SupplierCommitmentAssessment.Model.SupplierCommitmentAssessmentEvent>()
    let assessCodec = Codec.json<CollaborateWithSuppliers.SupplierCommitmentAssessment.Model.SupplierCommitmentAssessmentEvent>
    let assessSerialize = assessCodec.Encode >> Result.mapError (fun c -> c.ToString())
    let assessToArsId = function
        | CollaborateWithSuppliers.SupplierCommitmentAssessment.Model.SupplierCommitmentAssessmentChanged _ -> ArsIdentifiers.EnterpriseEvents.supplierCommitmentAssessmentChanged.Id
    let assessDeps = EnvelopeStoreDeps.create store assessToArsId assessSerialize traceCodec
    let assessProjDeps = { Config = pconfig; EnvelopeStore = store; EventCodec = assessCodec; TelemetryPublisher = telemetryPublisher }
    let assessQueryService = createProjectionAgent<CollaborateWithSuppliers.SupplierCommitmentAssessment.Model.SupplierCommitmentAssessmentEvent, SupplierCommitmentAssessmentDto, SupplierId> assessProjDeps

    let healthRepo = InMemRepository.create<CollaborateWithSuppliers.SupplierCollaborationHealth.Model.SupplierCollaborationHealthAssessment, SupplierCollaborationHealthAssessmentId, CollaborateWithSuppliers.SupplierCollaborationHealth.Model.SupplierCollaborationHealthEvent>()
    let healthCodec = Codec.json<CollaborateWithSuppliers.SupplierCollaborationHealth.Model.SupplierCollaborationHealthEvent>
    let healthSerialize = healthCodec.Encode >> Result.mapError (fun c -> c.ToString())
    let healthToArsId = function
        | CollaborateWithSuppliers.SupplierCollaborationHealth.Model.SupplierCollaborationHealthAssessmentPublished _ -> ArsIdentifiers.EnterpriseEvents.supplierCollaborationHealthAssessmentPublished.Id
    let healthDeps = EnvelopeStoreDeps.create store healthToArsId healthSerialize traceCodec
    let healthProjDeps = { Config = pconfig; EnvelopeStore = store; EventCodec = healthCodec; TelemetryPublisher = telemetryPublisher }
    let healthQueryService = createProjectionAgent<CollaborateWithSuppliers.SupplierCollaborationHealth.Model.SupplierCollaborationHealthEvent, SupplierCollaborationHealthAssessmentDto, SupplierCollaborationHealthAssessmentId> healthProjDeps

    CollaborateWithSuppliers.Context.create
        commitRepo
        commitDeps
        commitQueryService
        assessRepo
        assessDeps
        assessQueryService
        healthRepo
        healthDeps
        healthQueryService
        CollaborateWithSuppliers.SupplierCommitmentAssessment.Policies.defaultPolicy
        CollaborateWithSuppliers.SupplierCollaborationHealth.Policies.defaultPolicy
        publishKnowledge
        ports
        "SUP-ACME-COMPONENTS"
        "SKU-DRIVE-500GB"
        "PS-GLOBAL-SUPPLY-CHAIN"
        "P-2026-09"

let composeProcureMaterials
    (store: EnvelopeStoreOps)
    publishKnowledge
    (telemetryPublisher: TelemetryEvent -> unit)
    (traceCodec: Codec<DecisionTrace>)
    (pconfig: ProjectionConfig)
    (ports: SupplyPorts) =

    let recRepo = InMemRepository.create<ProcureMaterials.ProcurementRecommendation.Model.ProcurementRecommendation, ProcurementRequirementId, ProcureMaterials.ProcurementRecommendation.Model.ProcurementRecommendationEvent>()
    let recCodec = Codec.json<ProcureMaterials.ProcurementRecommendation.Model.ProcurementRecommendationEvent>
    let recSerialize = recCodec.Encode >> Result.mapError (fun c -> c.ToString())
    let recToArsId = function
        | ProcureMaterials.ProcurementRecommendation.Model.ProcurementRecommendationEvent.RecommendationCreated _
        | ProcureMaterials.ProcurementRecommendation.Model.ProcurementRecommendationEvent.RecommendationSuperseded _
        | ProcureMaterials.ProcurementRecommendation.Model.ProcurementRecommendationEvent.RecommendationArchived _ -> ArsIdentifiers.EnterpriseEvents.procurementRecommendationCreated.Id
    let recDeps = EnvelopeStoreDeps.create store recToArsId recSerialize traceCodec
    let recProjDeps = { Config = pconfig; EnvelopeStore = store; EventCodec = recCodec; TelemetryPublisher = telemetryPublisher }
    let recQueryService = createProjectionAgent<ProcureMaterials.ProcurementRecommendation.Model.ProcurementRecommendationEvent, ProcurementRecommendationDto, ProcurementRequirementId> recProjDeps

    let planRepo = InMemRepository.create<ProcureMaterials.ProcurementPlan.Model.ProcurementPlan, ProcurementPlanId, ProcureMaterials.ProcurementPlan.Model.ProcurementPlanEvent>()
    let planCodec = Codec.json<ProcureMaterials.ProcurementPlan.Model.ProcurementPlanEvent>
    let planSerialize = planCodec.Encode >> Result.mapError (fun c -> c.ToString())
    let planToArsId = function
        | ProcureMaterials.ProcurementPlan.Model.ProcurementPlanEvent.PlanDraftComposed _
        | ProcureMaterials.ProcurementPlan.Model.ProcurementPlanEvent.PlanPublished _
        | ProcureMaterials.ProcurementPlan.Model.ProcurementPlanEvent.PlanSuperseded _ -> ArsIdentifiers.EnterpriseEvents.procurementPlanPublished.Id
    let planDeps = EnvelopeStoreDeps.create store planToArsId planSerialize traceCodec
    let planProjDeps = { Config = pconfig; EnvelopeStore = store; EventCodec = planCodec; TelemetryPublisher = telemetryPublisher }
    let planQueryService = createProjectionAgent<ProcureMaterials.ProcurementPlan.Model.ProcurementPlanEvent, ProcurementPlanDto, ProcurementPlanId> planProjDeps

    ProcureMaterials.Context.create
        recRepo
        recDeps
        recQueryService
        planRepo
        planDeps
        planQueryService
        ProcureMaterials.ProcurementRecommendation.Policies.defaultConfig
        ProcureMaterials.ProcurementPlan.Policies.defaultConfig
        publishKnowledge
        ports
        "PR-REQ-001"
        "PP-PLAN-001"
        "SKU-DRIVE-500GB"
        "LOC-CENTRAL-WH"
        "PS-GLOBAL-SUPPLY-CHAIN"
        "CYCLE-2026-W37"
        "AUTOMATED-APS"

let composeScheduleProduction
    (store: EnvelopeStoreOps)
    publishKnowledge
    (telemetryPublisher: TelemetryEvent -> unit)
    (traceCodec: Codec<DecisionTrace>)
    (pconfig: ProjectionConfig)
    (ports: SupplyPorts) =

    let schedRepo = InMemRepository.create<ScheduleProduction.ProductionSchedule.Model.ProductionScheduleAggregate, string, ScheduleProduction.ProductionSchedule.Model.ProductionScheduleEvent>()
    let schedCodec = Codec.json<ScheduleProduction.ProductionSchedule.Model.ProductionScheduleEvent>
    let schedSerialize = schedCodec.Encode >> Result.mapError (fun c -> c.ToString())
    let schedDeps = EnvelopeStoreDeps.create store ScheduleProduction.ProductionSchedule.ACL.toArsEventId schedSerialize traceCodec
    let schedProjDeps = { Config = pconfig; EnvelopeStore = store; EventCodec = schedCodec; TelemetryPublisher = telemetryPublisher }
    let schedQueryService = createProjectionAgent<ScheduleProduction.ProductionSchedule.Model.ProductionScheduleEvent, ProductionScheduleDto, string> schedProjDeps

    let pubRepo = InMemRepository.create<ScheduleProduction.PublishedProductionSchedule.Model.PublishedProductionScheduleAggregate, string, ScheduleProduction.PublishedProductionSchedule.Model.PublishedProductionScheduleEvent>()
    let pubCodec = Codec.json<ScheduleProduction.PublishedProductionSchedule.Model.PublishedProductionScheduleEvent>
    let pubSerialize = pubCodec.Encode >> Result.mapError (fun c -> c.ToString())
    let pubDeps = EnvelopeStoreDeps.create store ScheduleProduction.PublishedProductionSchedule.ACL.toArsEventId pubSerialize traceCodec
    let pubProjDeps = { Config = pconfig; EnvelopeStore = store; EventCodec = pubCodec; TelemetryPublisher = telemetryPublisher }
    let pubQueryService = createProjectionAgent<ScheduleProduction.PublishedProductionSchedule.Model.PublishedProductionScheduleEvent, PublishedProductionScheduleDto, string> pubProjDeps

    ScheduleProduction.Context.create
        schedRepo
        schedDeps
        schedQueryService
        pubRepo
        pubDeps
        pubQueryService
        publishKnowledge
        ports
        "WC-ASSEMBLY-LINE-01"
        "P-2026-09"
        "PS-GLOBAL-SUPPLY-CHAIN"


let composeManageDistribution
    (store: EnvelopeStoreOps)
    publishKnowledge
    (telemetryPublisher: TelemetryEvent -> unit)
    (traceCodec: Codec<DecisionTrace>)
    (pconfig: ProjectionConfig)
    (ports: SupplyPorts) =

    let recRepo = InMemRepository.create<ManageDistribution.DistributionRecommendation.Model.DistributionRecommendationAggregate, DistributionRequirementId, ManageDistribution.DistributionRecommendation.Model.DistributionRecommendationEvent>()
    let recCodec = Codec.json<ManageDistribution.DistributionRecommendation.Model.DistributionRecommendationEvent>
    let recSerialize = recCodec.Encode >> Result.mapError (fun c -> c.ToString())
    let recDeps = EnvelopeStoreDeps.create store ManageDistribution.DistributionRecommendation.ACL.toArsEventId recSerialize traceCodec
    let recProjDeps = { Config = pconfig; EnvelopeStore = store; EventCodec = recCodec; TelemetryPublisher = telemetryPublisher }
    let recQueryService = createProjectionAgent<ManageDistribution.DistributionRecommendation.Model.DistributionRecommendationEvent, DistributionRecommendationDto, DistributionRequirementId> recProjDeps

    let planRepo = InMemRepository.create<ManageDistribution.DistributionPlan.Model.DistributionPlanAggregate, DistributionPlanId, ManageDistribution.DistributionPlan.Model.DistributionPlanEvent>()
    let planCodec = Codec.json<ManageDistribution.DistributionPlan.Model.DistributionPlanEvent>
    let planSerialize = planCodec.Encode >> Result.mapError (fun c -> c.ToString())
    let planDeps = EnvelopeStoreDeps.create store ManageDistribution.DistributionPlan.ACL.toArsEventId planSerialize traceCodec
    let planProjDeps = { Config = pconfig; EnvelopeStore = store; EventCodec = planCodec; TelemetryPublisher = telemetryPublisher }
    let planQueryService = createProjectionAgent<ManageDistribution.DistributionPlan.Model.DistributionPlanEvent, DistributionPlanDto, DistributionPlanId> planProjDeps

    ManageDistribution.Context.create
        recRepo
        recDeps
        recQueryService
        ManageDistribution.DistributionRecommendation.Policies.defaultDistributionPolicy
        planRepo
        planDeps
        planQueryService
        ManageDistribution.DistributionPlan.Policies.defaultDistributionPlanPublicationPolicy
        publishKnowledge
        ports
        "PS-GLOBAL-SUPPLY-CHAIN"
        "CYCLE-2026-W37"

let composeSenseSupplyChanges
    (store: EnvelopeStoreOps)
    publishKnowledge
    (telemetryPublisher: TelemetryEvent -> unit)
    (traceCodec: Codec<DecisionTrace>)
    (pconfig: ProjectionConfig)
    (ports: SupplyPorts) =

    let repo = InMemRepository.create<SenseSupplyChanges.SupplyChangeAssessment.Model.SupplyChangeAssessmentAggregate, SupplyChangeAssessmentId, SenseSupplyChanges.SupplyChangeAssessment.Model.SupplyChangeEvent>()
    let codec = Codec.json<SenseSupplyChanges.SupplyChangeAssessment.Model.SupplyChangeEvent>
    let serializeEvent = codec.Encode >> Result.mapError (fun c -> c.ToString())
    let deps = EnvelopeStoreDeps.create store SenseSupplyChanges.SupplyChangeAssessment.ACL.toArsEventId serializeEvent traceCodec

    let projDeps =
        { Config = pconfig
          EnvelopeStore = store
          EventCodec = codec
          TelemetryPublisher = telemetryPublisher }

    let queryService =
        createProjectionAgent<SenseSupplyChanges.SupplyChangeAssessment.Model.SupplyChangeEvent, SupplyChangeAssessmentDto, SupplyChangeAssessmentId> projDeps

    SenseSupplyChanges.Context.create
        repo
        deps
        queryService
        SenseSupplyChanges.SupplyChangeAssessment.Policies.defaultPolicy
        publishKnowledge
        ports

let composeEvaluateSupplyQuality
    (store: EnvelopeStoreOps)
    publishKnowledge
    (telemetryPublisher: TelemetryEvent -> unit)
    (traceCodec: Codec<DecisionTrace>)
    (pconfig: ProjectionConfig)
    (ports: SupplyPorts) =

    let repo = InMemRepository.create<EvaluateSupplyQuality.SupplyQualityAssessment.Model.SupplyQualityAssessment, SupplyQualityAssessmentId, EvaluateSupplyQuality.SupplyQualityAssessment.Model.SupplyQualityAssessmentEvent>()
    let codec = Codec.json<EvaluateSupplyQuality.SupplyQualityAssessment.Model.SupplyQualityAssessmentEvent>
    let serializeEvent = codec.Encode >> Result.mapError (fun c -> c.ToString())
    let deps = EnvelopeStoreDeps.create store EvaluateSupplyQuality.SupplyQualityAssessment.ACL.toArsEventId serializeEvent traceCodec

    let projDeps =
        { Config = pconfig
          EnvelopeStore = store
          EventCodec = codec
          TelemetryPublisher = telemetryPublisher }

    let queryService =
        createProjectionAgent<EvaluateSupplyQuality.SupplyQualityAssessment.Model.SupplyQualityAssessmentEvent, SupplyQualityAssessmentDto, SupplyQualityAssessmentId> projDeps

    EvaluateSupplyQuality.Context.create
        repo
        deps
        queryService
        EvaluateSupplyQuality.SupplyQualityAssessment.Policies.defaultPolicy
        publishKnowledge
        ports

let composeDetectSupplyExceptions
    (store: EnvelopeStoreOps)
    publishKnowledge
    (telemetryPublisher: TelemetryEvent -> unit)
    (traceCodec: Codec<DecisionTrace>)
    (pconfig: ProjectionConfig)
    (ports: SupplyPorts) =

    let repo = InMemRepository.create<DetectSupplyExceptions.SupplyExceptionAssessment.Model.SupplyExceptionAssessment, SupplyExceptionId, DetectSupplyExceptions.SupplyExceptionAssessment.Model.SupplyExceptionEvent>()
    let codec = Codec.json<DetectSupplyExceptions.SupplyExceptionAssessment.Model.SupplyExceptionEvent>
    let serializeEvent = codec.Encode >> Result.mapError (fun c -> c.ToString())
    let deps = EnvelopeStoreDeps.create store DetectSupplyExceptions.SupplyExceptionAssessment.ACL.toArsEventId serializeEvent traceCodec

    let projDeps =
        { Config = pconfig
          EnvelopeStore = store
          EventCodec = codec
          TelemetryPublisher = telemetryPublisher }

    let queryService =
        createProjectionAgent<DetectSupplyExceptions.SupplyExceptionAssessment.Model.SupplyExceptionEvent, SupplyExceptionAssessmentDto, SupplyExceptionId> projDeps

    DetectSupplyExceptions.Context.create
        repo
        deps
        queryService
        DetectSupplyExceptions.SupplyExceptionAssessment.Policies.defaultPolicy
        publishKnowledge
        ports
        "PS-GLOBAL-SUPPLY-CHAIN"

let composeExplainSupplyDecisions
    (store: EnvelopeStoreOps)
    publishKnowledge
    (telemetryPublisher: TelemetryEvent -> unit)
    (traceCodec: Codec<DecisionTrace>)
    (pconfig: ProjectionConfig)
    (explainPorts: ExplainSupplyDecisions.ExplainSupplyPorts) =

    let repo = InMemRepository.create<ExplainSupplyDecisions.SupplyExplanation.Model.SupplyExplanation, SupplyExplanationId, ExplainSupplyDecisions.SupplyExplanation.Model.SupplyExplanationEvent>()
    let codec = Codec.json<ExplainSupplyDecisions.SupplyExplanation.Model.SupplyExplanationEvent>
    let serializeEvent = codec.Encode >> Result.mapError (fun c -> c.ToString())
    let deps = EnvelopeStoreDeps.create store ExplainSupplyDecisions.SupplyExplanation.ACL.toArsEventId serializeEvent traceCodec

    let projDeps =
        { Config = pconfig
          EnvelopeStore = store
          EventCodec = codec
          TelemetryPublisher = telemetryPublisher }

    let queryService =
        createProjectionAgent<ExplainSupplyDecisions.SupplyExplanation.Model.SupplyExplanationEvent, SupplyExplanationDto, SupplyExplanationId> projDeps

    let dispatchNotification (notif: SupplyExplanationNotification) : Task<unit> =
        task {
            let notifCodec = Codec.json<SupplyExplanationNotification>
            match notifCodec.Encode notif with
            | Ok json ->
                let envelope =
                    Envelope.Create(
                        eventType = "BN-S-110",
                        domainCode = "S",
                        aggregateType = "SupplyExplanationNotification",
                        aggregateId = notif.ExplanationId,
                        occurrenceNumber = 1L,
                        capabilityId = "CA-S-012",
                        dataJson = json
                    )
                let! _ = store.PublishSingle "category-SupplyExplanationNotification" envelope ExpectedRevision.Any CancellationToken.None
                return ()
            | Error _ -> return ()
        }

    ExplainSupplyDecisions.Context.create
        repo
        deps
        publishKnowledge
        queryService
        ExplainSupplyDecisions.SupplyExplanation.Policies.defaultExplanationPolicy
        explainPorts
        dispatchNotification

let composeLearnFromSupply
    (store: EnvelopeStoreOps)
    publishKnowledge
    (telemetryPublisher: TelemetryEvent -> unit)
    (traceCodec: Codec<DecisionTrace>)
    (pconfig: ProjectionConfig) =

    let repo = InMemRepository.create<LearnFromSupply.SupplyLearning.Model.SupplyLearning, SupplyLearningId, LearnFromSupply.SupplyLearning.Model.SupplyLearningEvent>()
    let codec = Codec.json<LearnFromSupply.SupplyLearning.Model.SupplyLearningEvent>
    let serializeEvent = codec.Encode >> Result.mapError (fun c -> c.ToString())
    let toArsId = function
        | LearnFromSupply.SupplyLearning.Model.SupplyLearningRecorded _ -> ArsIdentifiers.EnterpriseEvents.supplyLearningRecorded.Id
        | LearnFromSupply.SupplyLearning.Model.SupplyLearningOutcomeRecorded _ -> ArsIdentifiers.EnterpriseEvents.supplyLearningOutcomeRecorded.Id
        | LearnFromSupply.SupplyLearning.Model.SupplyLearningSuperseded _ -> ArsIdentifiers.EnterpriseEvents.supplyLearningRecorded.Id
    let deps = EnvelopeStoreDeps.create store toArsId serializeEvent traceCodec

    let projDeps =
        { Config = pconfig
          EnvelopeStore = store
          EventCodec = codec
          TelemetryPublisher = telemetryPublisher }

    let queryService =
        createProjectionAgent<LearnFromSupply.SupplyLearning.Model.SupplyLearningEvent, SupplyLearningDto, SupplyLearningId> projDeps

    LearnFromSupply.Context.create
        repo
        deps
        publishKnowledge
        queryService
        LearnFromSupply.SupplyLearning.Policies.defaultLearningPolicy

let compose
    (store: EnvelopeStoreOps)
    publishKnowledge
    (telemetryPublisher: TelemetryEvent -> unit)
    (pconfig: ProjectionConfig)
    (traceCodec: Codec<DecisionTrace>)
    (ports: SupplyPorts)
    (explainPorts: ExplainSupplyDecisions.ExplainSupplyPorts)
    (services: IServiceCollection)
    : TaskResult<SupplyContext, ApplicationError> =
    taskResult {
        // 1. Understand Supply (CA-S-001)
        let! understandCtx: UnderstandSupply.Context.UnderstandSupplyContext =
            composeUnderstandSupply store publishKnowledge telemetryPublisher traceCodec pconfig ports
        // 2. Plan Supply (CA-S-002)
        let! planCtx: PlanSupply.Context.PlanSupplyContext =
            composePlanSupply store publishKnowledge telemetryPublisher traceCodec pconfig ports
        // 3. Manage Inventory (CA-S-003)
        let! inventoryCtx: ManageInventory.Context.ManageInventoryContext =
            composeManageInventory store publishKnowledge telemetryPublisher traceCodec pconfig ports
        // 4. Manage Capacity (CA-S-004)
        let! capacityCtx: ManageCapacity.ManageCapacityContext =
            composeManageCapacity store publishKnowledge telemetryPublisher traceCodec pconfig ports
        // 5. Collaborate with Suppliers (CA-S-005)
        let! supplierCtx: CollaborateWithSuppliers.CollaborateWithSuppliersContext =
            composeCollaborateWithSuppliers store publishKnowledge telemetryPublisher traceCodec pconfig ports
        // 6. Procure Materials (CA-S-006)
        let! procureCtx: ProcureMaterials.ProcureMaterialsContext =
            composeProcureMaterials store publishKnowledge telemetryPublisher traceCodec pconfig ports
        // 7. Schedule Production (CA-S-007)
        let! scheduleCtx: ScheduleProduction.ScheduleProductionContext =
            composeScheduleProduction store publishKnowledge telemetryPublisher traceCodec pconfig ports
        // 8. Manage Distribution (CA-S-008)
        let! distributionCtx: ManageDistribution.Context.ManageDistributionContext =
            composeManageDistribution store publishKnowledge telemetryPublisher traceCodec pconfig ports
        // 9. Sense Supply Changes (CA-S-009)
        let! senseCtx: SenseSupplyChanges.Context.SenseSupplyChangesContext =
            composeSenseSupplyChanges store publishKnowledge telemetryPublisher traceCodec pconfig ports
        // 10. Evaluate Supply Quality (CA-S-010)
        let! qualityCtx: EvaluateSupplyQuality.Context.EvaluateSupplyQualityContext =
            composeEvaluateSupplyQuality store publishKnowledge telemetryPublisher traceCodec pconfig ports
        // 11. Detect Supply Exceptions (CA-S-011)
        let! exceptionCtx: DetectSupplyExceptions.Context.DetectSupplyExceptionsContext =
            composeDetectSupplyExceptions store publishKnowledge telemetryPublisher traceCodec pconfig ports
        // 12. Explain Supply Decisions (CA-S-012)
        let! explainCtx: ExplainSupplyDecisions.Context.ExplainSupplyDecisionsContext =
            composeExplainSupplyDecisions store publishKnowledge telemetryPublisher traceCodec pconfig explainPorts
        // 13. Learn From Supply (CA-S-013)
        let! learnCtx: LearnFromSupply.SupplyLearningContext =
            composeLearnFromSupply store publishKnowledge telemetryPublisher traceCodec pconfig

        // Register all capability contexts into DI for UI and API consumption
        services.AddSingleton<UnderstandSupply.Context.UnderstandSupplyContext>(understandCtx) |> ignore
        services.AddSingleton<PlanSupply.Context.PlanSupplyContext>(planCtx) |> ignore
        services.AddSingleton<ManageInventory.Context.ManageInventoryContext>(inventoryCtx) |> ignore
        services.AddSingleton<ManageCapacity.ManageCapacityContext>(capacityCtx) |> ignore
        services.AddSingleton<CollaborateWithSuppliers.CollaborateWithSuppliersContext>(supplierCtx) |> ignore
        services.AddSingleton<ProcureMaterials.ProcureMaterialsContext>(procureCtx) |> ignore
        services.AddSingleton<ScheduleProduction.ScheduleProductionContext>(scheduleCtx) |> ignore
        services.AddSingleton<ManageDistribution.Context.ManageDistributionContext>(distributionCtx) |> ignore
        services.AddSingleton<SenseSupplyChanges.Context.SenseSupplyChangesContext>(senseCtx) |> ignore
        services.AddSingleton<EvaluateSupplyQuality.Context.EvaluateSupplyQualityContext>(qualityCtx) |> ignore
        services.AddSingleton<DetectSupplyExceptions.Context.DetectSupplyExceptionsContext>(exceptionCtx) |> ignore
        services.AddSingleton<ExplainSupplyDecisions.Context.ExplainSupplyDecisionsContext>(explainCtx) |> ignore
        services.AddSingleton<LearnFromSupply.SupplyLearningContext>(learnCtx) |> ignore

        let dispose () =
            understandCtx.Dispose()
            planCtx.Dispose()
            inventoryCtx.Dispose()
            capacityCtx.Dispose()
            supplierCtx.Dispose()
            procureCtx.Dispose()
            scheduleCtx.Dispose()
            distributionCtx.Dispose()
            senseCtx.Dispose()
            qualityCtx.Dispose()
            exceptionCtx.Dispose()
            explainCtx.Dispose()
            learnCtx.Dispose()

        return
            { UnderstandSupply = understandCtx
              PlanSupply = planCtx
              ManageInventory = inventoryCtx
              ManageCapacity = capacityCtx
              CollaborateWithSuppliers = supplierCtx
              ProcureMaterials = procureCtx
              ScheduleProduction = scheduleCtx
              ManageDistribution = distributionCtx
              SenseSupplyChanges = senseCtx
              EvaluateSupplyQuality = qualityCtx
              DetectSupplyExceptions = exceptionCtx
              ExplainSupplyDecisions = explainCtx
              LearnFromSupply = learnCtx
              Dispose = dispose }
    }

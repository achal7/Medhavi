module Medhavi.Supply.ArsIdentifiers

open Medhavi.Foundation.Contracts.ArsArtifact

let domain = "S"

module Capabilities =
    let understandSupply =
        ofCapability
            "CA-S-001"
            "Interprets the latest Enterprise Picture and supply change signals into an authoritative Supply Understanding"

    let planSupply =
        ofCapability
            "CA-S-002"
            "Generates, evaluates, and publishes authoritative, versioned supply plans balancing demand and supply"

    let manageInventory =
        ofCapability
            "CA-S-003"
            "Maintains governed inventory positions, buffer targets, and stockout/excess exposure assessments"

    let manageCapacity =
        ofCapability
            "CA-S-004"
            "Evaluates and manages capacity utilization, constraint thresholds, and bottleneck resources"

    let collaborateWithSuppliers =
        ofCapability
            "CA-S-005"
            "Maintains supplier commitments, collaboration posture, and bidirectional confirmation history"

    let procureMaterials =
        ofCapability
            "CA-S-006"
            "Generates and governs procurement recommendations and material baselines"

    let scheduleProduction =
        ofCapability
            "CA-S-007"
            "Creates sequenced, feasible production schedules for constrained resources"

    let manageDistribution =
        ofCapability
            "CA-S-008"
            "Generates governed distribution and replenishment recommendations across the network"

    let senseSupplyChanges =
        ofCapability
            "CA-S-009"
            "Continuously detects, correlates, and corroborates supply disruption and change signals"

    let evaluateSupplyQuality =
        ofCapability
            "CA-S-010"
            "Measures supply execution performance, supplier adherence, and delivery timeliness"

    let detectSupplyExceptions =
        ofCapability
            "CA-S-011"
            "Detects and classifies supply exceptions across inventory, capacity, orders, and commitments"

    let explainSupply =
        ofCapability
            "CA-S-012"
            "Composes structured, deterministic explanations for supply intelligence conclusions"

    let learnFromSupply =
        ofCapability
            "CA-S-013"
            "Discovers recurring supply patterns and records immutable enterprise learnings"

module Responsibilities =
    let receiveSupplyData =
        ofResponsibility
            "CR-S-001"
            "Establishes an immutable Supply Data Record from a received transaction"

    let evaluateSupplyData =
        ofResponsibility
            "CR-S-002"
            "Evaluates a Supply Data Record against governed acceptance criteria"

    let reviseSupplyUnderstanding =
        ofResponsibility
            "CR-S-003"
            "Interprets Enterprise Picture and change signals into a Draft Supply Understanding"

    let publishSupplyUnderstanding =
        ofResponsibility
            "CR-S-004"
            "Evaluates materiality and data completeness to publish an authoritative Supply Understanding"

    let balanceSupplyAndDemand =
        ofResponsibility
            "CR-S-005"
            "Executes supply-demand balancing algorithm and creates draft supply plan"

    let evaluateSupplyPlan =
        ofResponsibility
            "CR-S-006"
            "Evaluates a draft supply plan against quality thresholds and stability criteria"

    let publishSupplyPlan =
        ofResponsibility
            "CR-S-007"
            "Publishes the accepted supply plan, superseding the previous published version"

    let maintainInventoryPositionUnderstanding =
        ofResponsibility
            "CR-S-008"
            "Maintain authoritative understanding of inventory position and risk per item-location"

    let maintainInventoryPolicies =
        ofResponsibility
            "CR-S-009"
            "Maintain governed inventory policy parameters per item-location"

    let assessInventoryHealth =
        ofResponsibility
            "CR-S-010"
            "Periodically assess inventory health for management oversight"

    let maintainCapacityPositionUnderstanding =
        ofResponsibility
            "CR-S-011"
            "Maintain current interpreted capacity position understanding for each active resource"

    let assessCapacityHealth =
        ofResponsibility
            "CR-S-012"
            "Assess and publish periodic capacity health across resources for an assessment scope and evaluation period"

    let maintainSupplierCommitments =
        ofResponsibility
            "CR-S-013"
            "Maintain supplier commitments and continuous commitment assessments"

    let assessSupplierCollaborationHealth =
        ofResponsibility
            "CR-S-014"
            "Assess and publish periodic supplier collaboration health"

    let createProcurementRecommendations =
        ofResponsibility
            "CR-S-015"
            "Create governed procurement recommendations for each supply plan requirement"

    let publishProcurementPlan =
        ofResponsibility
            "CR-S-016"
            "Publish the authoritative procurement baseline for the planning cycle"

    let createProductionSchedules =
        ofResponsibility
            "CR-S-017"
            "Sequence and time production activities on each constrained resource to satisfy the Supply Plan while respecting all finite constraints"

    let publishProductionScheduleBaseline =
        ofResponsibility
            "CR-S-018"
            "Compose the current Active schedules into the authoritative scheduling baseline for the planning period"

    let createDistributionRecommendations =
        ofResponsibility
            "CR-S-019"
            "Create governed distribution recommendations for each distribution requirement"

    let publishDistributionPlan =
        ofResponsibility
            "CR-S-020"
            "Publish the authoritative distribution baseline for the planning cycle"

    let assessSupplyChanges =
        ofResponsibility
            "CR-S-021"
            "Continuously assess supply changes across all monitored entities and change types"

    let evaluateSupplyQuality =
        ofResponsibility
            "CR-S-022"
            "Evaluate supply quality across all governed dimensions and publish authoritative assessment"

    let evaluateSupplyExceptions =
        ofResponsibility
            "CR-S-023"
            "Evaluate whether correlated evidence violates governed supply policies and maintain exception lifecycle"

    let recordSupplyExplanation =
        ofResponsibility
            "CR-S-024"
            "Record immutable, auditable, deterministic enterprise explanations of the reasoning behind supply intelligence conclusions"

    let recordSupplyLearning =
        ofResponsibility
            "CR-S-025"
            "Discover recurring patterns from historical evidence, conclude what they mean, and record the immutable enterprise learning with its outcome"

    let recordLearningOutcome =
        ofResponsibility
            "CR-S-026"
            "Record the governance decision on a Supply Learning"



module SemanticObjects =
    let supplyUnderstanding =
        ofSemanticObject
            "SE-S-001"
            "Authoritative interpretation of current supply health, risk, and confidence for a Planning Scope"

    let supplyDataRecord =
        ofSemanticObject
            "SE-S-010"
            "Immutable enterprise record of a received supply transaction"

    let supplyPlan =
        ofSemanticObject
            "SE-S-020"
            "Authoritative, versioned supply planning baseline for a Planning Scope and Forecast Horizon"

    let supplyPlanVersion =
        ofSemanticObject
            "SE-S-021"
            "A single version entity within the Supply Plan aggregate root"

    let supplyChangeAssessment =
        ofSemanticObject
            "SE-S-090"
            "Corroborated supply change signal informing interpretation"

    let inventoryPositionAssessment =
        ofSemanticObject
            "SE-S-030"
            "Continuous interpretation of inventory position, coverage, risk, and compliance for an active item-location"

    let inventoryPolicyAssignment =
        ofSemanticObject
            "SE-S-031"
            "Governed inventory policy parameters and replenishment strategy for an active item-location"

    let inventoryHealthAssessment =
        ofSemanticObject
            "SE-S-032"
            "Authoritative periodic management assessment of inventory health for an assessment scope and evaluation period"

    let capacityPositionAssessment =
        ofSemanticObject
            "SE-S-040"
            "Continuous interpretation of capacity position, utilization, constraint status, and bottleneck risk for an active physical resource"

    let capacityHealthAssessment =
        ofSemanticObject
            "SE-S-041"
            "Periodic management assessment of capacity health, stability, volatility, constraint pressure, and bottleneck impact for an assessment scope and evaluation period"

    let supplierCommitment =
        ofSemanticObject
            "SE-S-050"
            "Authoritative enterprise record of a planning collaboration agreement between enterprise and supplier"

    let supplierCommitmentAssessment =
        ofSemanticObject
            "SE-S-051"
            "Continuously maintained assessment of supplier commitment posture, reliability, responsiveness, and risk exposure"

    let supplierCollaborationHealthAssessment =
        ofSemanticObject
            "SE-S-052"
            "Periodically published management assessment of supplier collaboration health for an assessment scope and evaluation period"

    let procurementRecommendation =
        ofSemanticObject
            "SE-S-060"
            "Authoritative, continuously maintained sourcing decision for a procurement requirement"

    let procurementPlan =
        ofSemanticObject
            "SE-S-061"
            "Authoritative, versioned procurement baseline composed from active procurement recommendations"

    let productionSchedule =
        ofSemanticObject
            "SE-S-070"
            "Authoritative, continuously maintained, resource-specific production sequence for a planning period"

    let publishedProductionSchedule =
        ofSemanticObject
            "SE-S-071"
            "Authoritative, periodically published, immutable scheduling baseline composed from active production schedules"

    let distributionRecommendation =
        ofSemanticObject
            "SE-S-080"
            "Authoritative, continuously maintained network positioning decision for a distribution requirement"

    let distributionPlan =
        ofSemanticObject
            "SE-S-081"
            "Authoritative, versioned distribution baseline composed from active distribution recommendations"

    let supplyQualityAssessment =
        ofSemanticObject
            "SE-S-100"
            "Authoritative, versioned supply quality assessment baseline for an assessment scope, horizon, and evaluation period"

    let supplyExceptionAssessment =
        ofSemanticObject
            "SE-S-110"
            "The authoritative enterprise record of a governed policy violation in the supply ecosystem"

    let supplyExplanation =
        ofSemanticObject
            "SE-S-120"
            "The immutable, auditable, deterministic enterprise explanation of the reasoning behind any supply intelligence conclusion"

    let supplyLearning =
        ofSemanticObject
            "SE-S-130"
            "The authoritative, immutable enterprise learning discovering recurring supply phenomena across multiple planning cycles"



module Decisions =
    let acceptSupplyData =
        ofDecision
            "DE-S-001"
            "Evaluates incoming supply data against governed eligibility criteria and assigns an acceptance outcome"

    let publishSupplyUnderstanding =
        ofDecision
            "DE-S-002"
            "Evaluates whether a Draft Supply Understanding should transition to Published state"

    let acceptSupplyPlan =
        ofDecision
            "DE-S-011"
            "Determines whether a draft supply plan meets quality and stability criteria for acceptance"

    let publishSupplyPlanDecision =
        ofDecision
            "DE-S-012"
            "Determines whether an accepted draft supply plan is authorized for publication"

    let determineInventoryPositionStatus =
        ofDecision
            "DE-S-021"
            "Determines coverage status, risk level, and policy conformance for an item-location"

    let determineInventoryPolicy =
        ofDecision
            "DE-S-022"
            "Selects calculation methodology and determines replenishment parameters"

    let classifyInventoryHealth =
        ofDecision
            "DE-S-023"
            "Classifies enterprise inventory health state across assessment scope and evaluation period"

    let determineCapacityPositionStatus =
        ofDecision
            "DE-S-031"
            "Determine the overall capacity position status for a specific resource and planning period"

    let classifyCapacityHealth =
        ofDecision
            "DE-S-032"
            "Classify the overall capacity health state for an assessment scope and evaluation period"

    let acceptSupplierResponse =
        ofDecision
            "DE-S-041"
            "Determines whether a supplier response is acceptable as a firm commitment, with buffer, requires escalation, or must be rejected"

    let classifyCollaborationHealth =
        ofDecision
            "DE-S-042"
            "Classifies collaboration health across suppliers and determines publication authorization"

    let determineEnterpriseSourcingDetermination =
        ofDecision
            "DE-S-051"
            "Evaluates completeness, consistency, and governance alignment of computed enterprise sourcing determination"

    let publishProcurementPlanDecision =
        ofDecision
            "DE-S-052"
            "Evaluates completeness, confidence, and feasibility of active procurement recommendations to authorize procurement plan publication"

    let determineProductionSequence =
        ofDecision
            "DE-S-061"
            "Determines the optimal production sequence considering changeover minimization, due date adherence, and constraints"

    let assessScheduleFeasibility =
        ofDecision
            "DE-S-062"
            "Assesses proposed production schedule feasibility across capacity, material, tool, labor, maintenance, and frozen horizon"

    let publishProductionScheduleDecision =
        ofDecision
            "DE-S-063"
            "Determines whether the proposed production schedule meets publication criteria and should be published"

    let determineDistributionSourcing =
        ofDecision
            "DE-S-071"
            "Determines the governed sourcing strategy and allocation outcome for a distribution requirement"

    let publishDistributionPlanDecision =
        ofDecision
            "DE-S-072"
            "Evaluates completeness, confidence, and feasibility of distribution plan to authorize publication"

    let evaluateSupplyPerformance =
        ofDecision
            "DE-S-091"
            "Determines authoritative quality classification and trend for a governed quality dimension"

    let publishSupplyQualityAssessmentDecision =
        ofDecision
            "DE-S-092"
            "Determines whether the composed Supply Quality Assessment meets publication criteria"

    let evaluateSupplyLearning =
        ofDecision
            "DE-S-121"
            "Evaluates whether a candidate learning meets recurrence, evidence, and confidence thresholds for enterprise recording"


module Rules =
    let supplyDataRecordIdentity =
        ofRule "BR-S-001" "Supply Data Record Identifier is globally unique and immutable"

    let supplyUnderstandingAggregateIdentity =
        ofRule "BR-S-002" "Exactly one Supply Understanding aggregate exists for each Planning Scope"

    let supplyDataReceptionEligibility =
        ofRule "BR-S-003" "Supply data must originate from known source with valid structural payload"

    let supplyPlanAggregateIdentity =
        ofRule "BR-S-020" "Identity of a Supply Plan is the combination of Planning Scope Identifier and Forecast Horizon"

    let supplyPlanGenerationEligibility =
        ofRule "BR-S-021" "Generation requires active Planning Scope and valid Forecast Horizon"

    let supplyPlanDerivationSource =
        ofRule "BR-S-022" "Plan derived exclusively from Published Supply Understanding and Published Forecast"

    let supplyDataTimeliness =
        ofRule "BR-S-010" "Supply data timestamp must be within maximum allowed latency"

    let supplyQuantityRangeValidity =
        ofRule "BR-S-011" "Supply quantities must be positive and within valid range"

    let sourceReliabilityThreshold =
        ofRule "BR-S-012" "Source reliability must meet minimum acceptance threshold"

    let duplicateDataDetection =
        ofRule "BR-S-013" "Duplicate data within duplicate detection window is rejected"

    let materialChangeRequiredForPublication =
        ofRule "BR-S-014" "Publication requires material change or periodic refresh"

    let dataCompletenessThreshold =
        ofRule "BR-S-015" "Data completeness must meet threshold defined in PO-S-011"

    let structuralCompleteness =
        ofRule "BR-S-015a" "All six mandatory interpretation dimensions must be present in the Draft"

    let contentCompleteness =
        ofRule "BR-S-015b" "All evidence references and confidence levels within dimensions must be populated"

    let singlePublishedUnderstanding =
        ofRule "BR-S-016" "Exactly one Published version shall exist per Planning Scope at any moment"

    let publishedUnderstandingImmutability =
        ofRule "BR-S-017" "Published Supply Understanding versions are immutable"

    let supplyDataEvaluationEligibility =
        ofRule "BR-S-101" "Supply data record must be in Received state for evaluation"

    let supplyUnderstandingRevisionEligibility =
        ofRule "BR-S-102" "Published enterprise picture must exist for planning scope"

    let singleEvaluationOfSupplyData =
        ofRule "BR-S-103" "Supply data record shall be evaluated exactly once from Received"

    let singlePublishedSupplyUnderstandingPerPlanningScope =
        ofRule "BR-S-104" "Supersedes previous Published version atomically upon new publication"

    let publishedSupplyUnderstandingImmutabilityExplicit =
        ofRule "BR-S-105" "Published versions cannot be modified"

    let excludedSupplyData =
        ofRule "BR-S-106" "Quarantined and Rejected records do not contribute to Enterprise Picture"

    let supplyDataAccessGovernance =
        ofRule "BR-S-107" "Supply data acceptance is governed by PO-S-010"

    let supplyUnderstandingMaterialityGovernance =
        ofRule "BR-S-108" "Supply understanding materiality is governed by PO-S-011"

    let supplyUnderstandingPublicationAuthorization =
        ofRule "BR-S-109" "Publication requires DE-S-002 authorization"

    let supplyUnderstandingDerivationSource =
        ofRule "BR-S-110" "Supply understanding derived solely from Enterprise Picture and eligible change signals"

    let firstPublicationMateriality =
        ofRule "BR-S-111" "First publication is always material"

    let supplyPlanEvaluationEligibility =
        ofRule "BR-S-121" "Evaluation requires a complete Draft Supply Plan Version"

    let supplyPlanPublicationEligibility =
        ofRule "BR-S-122" "Plan version must be accepted by DE-S-011 before publication authorization"

    let singlePublishedSupplyPlanPerScopeAndHorizon =
        ofRule "BR-S-123" "Exactly one Published Supply Plan Version shall exist per Planning Scope and Forecast Horizon"

    let publishedSupplyPlanImmutability =
        ofRule "BR-S-124" "Published Supply Plan versions are immutable"

    let hardConstraintSatisfactionBeforePublication =
        ofRule "BR-S-125" "All hard constraints must be satisfied or documented as approved relaxations"

    let planVersionMonotonicity =
        ofRule "BR-S-126" "Version numbers within a Supply Plan aggregate are monotonically increasing and never reused"

    let supplyPlanQualityThresholds =
        ofRule "BR-S-127" "Draft plan must meet quality thresholds for service level, utilization, inventory, and cost"

    let planStabilityThresholds =
        ofRule "BR-S-128" "Draft plan must satisfy stability limits relative to previous published baseline"

    let supplyPlanPublicationAuthorization =
        ofRule "BR-S-129" "Publication requires explicit authorization by DE-S-012"

    let firstPublicationAcceptance =
        ofRule "BR-S-131" "Stability thresholds are waived for initial baseline publication"

    let inventoryPositionAssessmentIdentity =
        ofRule "BR-S-030" "Inventory Position Assessment aggregate identity invariant"

    let inventoryPolicyAssignmentIdentity =
        ofRule "BR-S-031" "Inventory Policy Assignment aggregate identity invariant"

    let inventoryHealthAssessmentIdentity =
        ofRule "BR-S-032" "Inventory Health Assessment aggregate identity invariant"

    let inventoryPositionUpdateEligibility =
        ofRule "BR-S-140" "Item and location must be active and valid for position update"

    let inventoryPolicyUpdateEligibility =
        ofRule "BR-S-141" "Item and location must be active and valid for policy update"

    let inventoryHealthAssessmentEligibility =
        ofRule "BR-S-142" "Health assessment requires valid risk assessments and active policy"

    let singleCurrentInventoryPositionAssessment =
        ofRule "BR-S-143" "Exactly one current position assessment per item-location"

    let inventoryPositionAssessmentStateChangeImmutability =
        ofRule "BR-S-144" "Assessment change events are immutable and append-only"

    let singleCurrentInventoryPolicyAssignment =
        ofRule "BR-S-145" "Exactly one active inventory policy assignment per item-location"

    let singlePublishedInventoryHealthAssessment =
        ofRule "BR-S-146" "Exactly one published inventory health assessment per scope and period"

    let publishedInventoryHealthAssessmentImmutability =
        ofRule "BR-S-147" "Published inventory health assessment versions are immutable"

    let permanentRetentionOfHealthAssessments =
        ofRule "BR-S-148" "All published and superseded health assessments are permanently retained"

    let inventoryPositionStatusClassification =
        ofRule "BR-S-149" "Position status classification conforms to PO-S-032"

    let inventoryPolicyDetermination =
        ofRule "BR-S-150" "Policy determination conforms to PO-S-033"

    let inventoryPolicyParameterValidity =
        ofRule "BR-S-046" "Inventory policy parameters shall be valid (Reorder Point >= Safety Stock, Max > Min, Quantities >= 0)"

    let inventoryHealthClassification =
        ofRule "BR-S-151" "Health classification conforms to PO-S-034"

    let inventoryHealthPublicationCompleteness =
        ofRule "BR-S-049" "All item-locations in the Assessment Scope must be evaluated before inventory health assessment publication"

    let safetyStockDerivation =
        ofRule "BR-S-152" "Safety stock derived via BA-S-002"

    let reorderPointDerivation =
        ofRule "BR-S-153" "Reorder point derived via BA-S-003"

    let inventoryRiskAssessmentDerivation =
        ofRule "BR-S-154" "Inventory risk assessment derived via BA-S-004"

    let inventoryHealthStateDerivation =
        ofRule "BR-S-155" "Inventory health state derived via BA-S-005"

    let capacityPositionAssessmentIdentity =
        ofRule "BR-S-040" "Aggregate identity is Physical Resource Identifier (SE-C-007)"

    let capacityHealthAssessmentIdentity =
        ofRule "BR-S-041" "Aggregate identity is Assessment Scope Identifier + Evaluation Period"

    let capacityPositionUpdateEligibility =
        ofRule "BR-S-160" "Capacity Position Assessment update eligibility"

    let capacityHealthAssessmentEligibility =
        ofRule "BR-S-161" "Capacity Health Assessment eligibility"

    let singleCurrentCapacityPositionAssessment =
        ofRule "BR-S-162" "Exactly one active Capacity Position Assessment per physical resource"

    let capacityPositionAssessmentStateChangeImmutability =
        ofRule "BR-S-163" "Capacity Position Assessment state change immutability"

    let singlePublishedCapacityHealthAssessment =
        ofRule "BR-S-164" "Exactly one Published Capacity Health Assessment per scope and period"

    let publishedCapacityHealthAssessmentImmutability =
        ofRule "BR-S-165" "Published Capacity Health Assessment is immutable"

    let permanentRetentionOfCapacityHealthAssessments =
        ofRule "BR-S-166" "Permanent retention of Capacity Health Assessments"

    let capacityPositionAssessmentHistoryImmutability =
        ofRule "BR-S-167" "Capacity Assessment Change Events are immutable and append-only"

    let capacityPositionStatusClassification =
        ofRule "BR-S-168" "Capacity position status classification rules"

    let capacityHealthClassification =
        ofRule "BR-S-169" "Capacity health classification rules"

    let capacityHealthStateClassification =
        ofRule "BR-S-170" "Capacity health state classification rules"

    let capacityOperatingEnvelopeDerivation =
        ofRule "BR-S-171" "Capacity Operating Envelope derivation via BA-S-011"

    let capacityUtilizationDerivation =
        ofRule "BR-S-172" "Capacity utilization derivation"

    let capacityBottleneckAndHealthDerivation =
        ofRule "BR-S-173" "Capacity bottleneck and health state derivation via BA-S-012/013"

    let capacityPositionAssessmentDerivationSource =
        ofRule "BR-S-174" "Capacity Position Assessment derived exclusively from published upstream facts"

    let capacityHealthAssessmentDerivationSource =
        ofRule "BR-S-175" "Capacity Health Assessment derived exclusively from Position Assessments"

    let supplierCommitmentAssessmentIdentity =
        ofRule "BR-S-050" "Supplier Commitment Assessment aggregate identity is Supplier Identifier (SE-C-004)"

    let supplierCommitmentUpdateEligibility =
        ofRule "BR-S-180" "Supplier commitment update requires published supply plan and current policy"

    let supplierCollaborationHealthAssessmentEligibility =
        ofRule "BR-S-181" "Supplier collaboration health assessment eligibility requires published plan, assessments, and policy"

    let singleCurrentSupplierCommitmentAssessment =
        ofRule "BR-S-182" "Exactly one active supplier commitment assessment per supplier"

    let supplierCommitmentAssessmentStateChangeImmutability =
        ofRule "BR-S-183" "State change events within supplier commitment assessment are immutable"

    let singlePublishedSupplierCollaborationHealthAssessment =
        ofRule "BR-S-184" "Exactly one published collaboration health assessment per scope and period"

    let publishedSupplierCollaborationHealthAssessmentImmutability =
        ofRule "BR-S-185" "Published supplier collaboration health assessment is immutable"

    let permanentRetentionOfCollaborationHealthAssessments =
        ofRule "BR-S-186" "All collaboration health assessment versions are retained permanently"

    let supplierCollaborationHealthAssessmentVersionMonotonicity =
        ofRule "BR-S-187" "Version numbers within collaboration health assessment are strictly monotonic"

    let supplierResponseAcceptanceGovernance =
        ofRule "BR-S-188" "Supplier response acceptance criteria are governed exclusively by PO-S-060"

    let collaborationHealthClassificationGovernance =
        ofRule "BR-S-189" "Collaboration health classification taxonomy is governed exclusively by PO-S-060"

    let commitmentExposureAssessmentGovernance =
        ofRule "BR-S-190" "Commitment exposure assessment methodology across 7 dimensions is governed by PO-S-060"

    let commitmentExposureAssessmentDerivation =
        ofRule "BR-S-191" "Commitment exposure assessment derived via BA-S-020 across 7 orthogonal dimensions"

    let collaborationHealthStateDerivation =
        ofRule "BR-S-192" "Collaboration health state derived via BA-S-021 precedence cascade"

    let supplierCommitmentAssessmentDerivationSource =
        ofRule "BR-S-193" "Commitment assessment derived exclusively from supply plan and supplier response data"

    let supplierCollaborationHealthAssessmentDerivationSource =
        ofRule "BR-S-194" "Collaboration health assessment derived exclusively from commitment assessments and policy"

    let procurementRecommendationIdentity =
        ofRule "BR-S-060" "Procurement Recommendation aggregate identity is Supply Plan Procurement Requirement Reference"

    let procurementPlanAggregateIdentity =
        ofRule "BR-S-061" "Procurement Plan aggregate identity is Planning Scope Identifier + Planning Cycle"

    let procurementRecommendationCreationEligibility =
        ofRule "BR-S-200" "Procurement Recommendation creation requires active procurement requirement in published Supply Plan"

    let procurementPlanPublicationEligibility =
        ofRule "BR-S-201" "Procurement Plan publication requires active procurement recommendations for planning scope and cycle"

    let singleActiveProcurementRecommendation =
        ofRule "BR-S-202" "Exactly one Active Procurement Recommendation exists per procurement requirement"

    let procurementRecommendationStateChangeImmutability =
        ofRule "BR-S-203" "State change events within procurement recommendation are immutable and append-only"

    let singlePublishedProcurementPlan =
        ofRule "BR-S-204" "Exactly one Published Procurement Plan exists per planning scope and planning cycle"

    let publishedProcurementPlanImmutability =
        ofRule "BR-S-205" "Published Procurement Plan versions are immutable"

    let permanentRetentionOfProcurementPlans =
        ofRule "BR-S-206" "All published and superseded procurement plan versions are retained permanently"

    let procurementRecommendationHistoryImmutability =
        ofRule "BR-S-207" "All procurement recommendation versions are retained permanently and are immutable"

    let sourcingDeterminationGovernance =
        ofRule "BR-S-208" "All sourcing determinations are governed exclusively by PO-S-070 and Delegation of Authority Policy"

    let procurementPlanPublicationAuthorization =
        ofRule "BR-S-209" "Procurement plan publication requires explicit authorization by DE-S-052"

    let supplierAllocationStrategyGovernance =
        ofRule "BR-S-210" "Supplier allocation strategies conform strictly to PO-S-070"

    let procurementRequirementDerivation =
        ofRule "BR-S-211" "Procurement requirements derived deterministically via BA-S-030"

    let sourcingSituationDerivation =
        ofRule "BR-S-212" "Sourcing situation assessed deterministically via BA-S-031 across 4 sequential dimensions"

    let sourcingDeterminationDerivation =
        ofRule "BR-S-213" "Enterprise sourcing determination derived deterministically via BA-S-032 governance quadruple"

    let procurementPlanCompleteness =
        ofRule "BR-S-111" "The Procurement Plan must cover all active procurement requirements for the planning cycle, or document exclusions"

    let procurementPlanConfidenceThreshold =
        ofRule "BR-S-112" "The Procurement Plan must meet the confidence threshold governed by PO-S-070, or be flagged for review"

    let procurementPlanFeasibilityDocumentation =
        ofRule "BR-S-113" "Unresolved feasibility issues must be documented and flagged in the Procurement Plan"

    let productionScheduleAggregateIdentity =
        ofRule "BR-S-070" "Production Schedule aggregate identity is Resource Identifier (SE-C-007) + Planning Period (SE-C-034) + Schedule Type"

    let publishedProductionScheduleIdentity =
        ofRule "BR-S-071" "Published Production Schedule aggregate identity is Planning Scope Identifier (SE-C-010) + Planning Period (SE-C-034) + Version Number"

    let productionScheduleCreationEligibility =
        ofRule "BR-S-220" "Production schedule creation requires a Published Supply Plan and current Scheduling Policy"

    let productionSchedulePublicationEligibility =
        ofRule "BR-S-221" "Production schedule publication requires an Active Production Schedule and current Scheduling Policy"

    let singleActiveProductionSchedulePerResource =
        ofRule "BR-S-222" "Exactly one Active Production Schedule exists per resource, planning period, and schedule type"

    let productionScheduleStateChangeImmutability =
        ofRule "BR-S-223" "State change events within the Production Schedule are immutable"

    let singlePublishedProductionSchedulePerScope =
        ofRule "BR-S-224" "Exactly one Published Production Schedule exists per Planning Scope and Planning Period"

    let publishedProductionScheduleImmutability =
        ofRule "BR-S-225" "A Published Production Schedule is immutable"

    let permanentRetentionOfProductionSchedules =
        ofRule "BR-S-226" "All Production Schedule versions are retained permanently"

    let productionScheduleVersionMonotonicity =
        ofRule "BR-S-227" "Version Number is monotonically increasing within the Production Schedule aggregate"

    let productionSequenceDeterminationGovernance =
        ofRule "BR-S-228" "Production sequence determination criteria are governed exclusively by PO-S-080"

    let scheduleFeasibilityAssessmentGovernance =
        ofRule "BR-S-229" "Schedule feasibility assessment criteria are governed exclusively by PO-S-080"

    let productionSchedulePublicationGovernance =
        ofRule "BR-S-244" "Production schedule publication criteria are governed exclusively by PO-S-080"

    let productionScheduleDerivationSource =
        ofRule "BR-S-246" "The Production Schedule is derived exclusively from the Published Supply Plan and resource calendar"

    let scheduleStabilityAssessmentDerivation =
        ofRule "BR-S-248" "Schedule stability assessment is derived by BA-S-041 from the Production Schedule and previous Published baseline"

    let sequencingStrategyGovernance =
        ofRule "BR-S-120" "Sequencing shall use the strategy defined by the Scheduling Policy"

    let changeoverMatrixConstraint =
        ofRule "BR-S-121" "Changeover times between consecutive activities shall be respected as hard constraints"

    let dueDateAdherencePriority =
        ofRule "BR-S-122" "Due date adherence shall be prioritized according to governed scheduling rules"

    let priorityBasedSequencing =
        ofRule "BR-S-123" "Priority-based sequencing shall respect governed demand and production priorities"

    let materialAvailabilityConfirmation =
        ofRule "BR-S-124" "Material availability shall be confirmed for each production activity before the schedule is activated"

    let capacityAvailabilityVerification =
        ofRule "BR-S-125" "Capacity availability shall be verified against the finite capacity of the scheduled resource"

    let toolAvailabilityConfirmation =
        ofRule "BR-S-126" "Tool availability shall be confirmed where tool constraints are governed"

    let laborAvailabilityConfirmation =
        ofRule "BR-S-127" "Labor availability shall be confirmed where labor constraints are governed"

    let maintenanceAndCalendarRespect =
        ofRule "BR-S-128" "Maintenance windows and calendar closures shall be respected as hard constraints"

    let schedulePublicationFeasibilityThreshold =
        ofRule "BR-S-133" "The schedule must meet the feasibility threshold governed by PO-S-080"

    let schedulePublicationStabilityThreshold =
        ofRule "BR-S-134" "The schedule must not deviate beyond stability threshold governed by PO-S-080"

    let scheduleCompletenessRequirement =
        ofRule "BR-S-135" "Every schedule must contain all mandatory fields"

    let distributionRecommendationIdentity =
        ofRule "BR-S-080" "The business identity of a Distribution Recommendation is the combination of Distribution Requirement Identifier and Source Location Identifier"

    let distributionPlanAggregateIdentity =
        ofRule "BR-S-081" "The business identity of a Distribution Plan is the combination of Planning Scope Identifier and Plan Horizon"

    let sourceInventoryAvailabilityConfirmed =
        ofRule "BR-S-140" "Source inventory availability shall be confirmed using projected available inventory at departure time"

    let laneCapacityVerified =
        ofRule "BR-S-141" "Lane capacity shall be verified against the finite capacity of the lane for the required transit period"

    let transitTimeFeasibilityConfirmed =
        ofRule "BR-S-142" "Transit time feasibility shall consider the required delivery date, transit lead time, and receiving window"

    let destinationReceivingCapacityConfirmed =
        ofRule "BR-S-143" "Destination receiving capacity shall consider dock capacity, storage availability, and handling capability"

    let deploymentPriorityVsCostBalance =
        ofRule "BR-S-144" "Movement decisions shall balance deployment priority against cost"

    let constrainedSupplyAllocationStrategyApplied =
        ofRule "BR-S-145" "When available deployable inventory is insufficient, the Allocation Strategy governed by PO-S-090 shall be applied, and the allocation outcome recorded"

    let completenessOfDistributionRecommendations =
        ofRule "BR-S-156" "The plan must cover all active distribution requirements for the planning cycle, or document exclusions"

    let distributionPlanConfidenceThreshold =
        ofRule "BR-S-157" "The plan must meet the confidence threshold governed by PO-S-090, or be flagged for review"

    let distributionPlanUnresolvedFeasibilityIssues =
        ofRule "BR-S-158" "Unresolved feasibility issues must be documented and flagged in the plan"

    let distributionPlanAggregatesApprovedRecommendations =
        ofRule "BR-S-160" "Distribution Plan must aggregate all approved Distribution Recommendations"

    let distributionPlanImmutability =
        ofRule "BR-S-161" "Distribution Plan must be immutable once published"

    let distributionPlanVersionMonotonicity =
        ofRule "BR-S-162" "Version Numbers must be monotonically increasing"

    let distributionRecommendationCreationEligibility =
        ofRule "BR-S-230" "Distribution recommendation creation requires a Published Supply Plan and the Distribution Policy to be current"

    let distributionPlanPublicationEligibility =
        ofRule "BR-S-231" "Distribution plan publication requires Active Distribution Recommendations for the Planning Scope and Plan Horizon, and the Distribution Plan Publication Cadence Policy to be current"

    let singleActiveDistributionRecommendation =
        ofRule "BR-S-232" "Exactly one Active Distribution Recommendation exists per distribution requirement per source location at any moment"

    let distributionRecommendationStateChangeImmutability =
        ofRule "BR-S-233" "State change events within the Distribution Recommendation are immutable"

    let singlePublishedDistributionPlan =
        ofRule "BR-S-234" "Exactly one Published Distribution Plan exists per Planning Scope and Plan Horizon at any moment"

    let publishedDistributionPlanImmutability =
        ofRule "BR-S-235" "A Published Distribution Plan is immutable"

    let permanentRetentionOfDistributionPlans =
        ofRule "BR-S-236" "All Distribution Plan versions are retained permanently"

    let distributionPlanVersionMonotonicityRule =
        ofRule "BR-S-237" "Version Number is monotonically increasing within the Distribution Plan aggregate"

    let distributionSourcingDeterminationGovernance =
        ofRule "BR-S-238" "Distribution sourcing determination criteria and methodology are governed exclusively by PO-S-090"

    let distributionPlanPublicationGovernance =
        ofRule "BR-S-239" "Distribution plan publication criteria are governed exclusively by PO-S-091"

    let networkCoherenceGovernance =
        ofRule "BR-S-240" "Distribution recommendations must maintain network coherence as governed by PO-S-090"

    let distributionRecommendationDerivationSource =
        ofRule "BR-S-241" "The Distribution Recommendation is derived exclusively from the Published Supply Plan and network data"

    let distributionPlanStabilityAssessmentDerivation =
        ofRule "BR-S-242" "Distribution plan stability assessment is derived by BA-S-051 from the Distribution Plan and the previous Published Distribution Plan"

    let supplyChangeAssessmentIdentity =
        ofRule "BR-S-090" "Supply Change Assessment aggregate identity is composite of Monitored Entity Type, Monitored Entity Identifier, and Change Type"

    let supplyChangeAssessmentUpdateEligibility =
        ofRule "BR-S-250" "Supply Change Assessment update requires recognized entity, change type, and active policy"

    let singleCurrentSupplyChangeAssessment =
        ofRule "BR-S-251" "Single Current Supply Change Assessment per entity and change type"

    let supplyChangeAssessmentStateChangeImmutability =
        ofRule "BR-S-252" "Supply Change Assessment state change immutability"

    let supplyChangeAssessmentHistoryImmutability =
        ofRule "BR-S-253" "Assessment Change Events are strictly immutable and append-only"

    let changeDetectionGovernance =
        ofRule "BR-S-254" "Change detection governed by pure functional variance quantification"

    let changeMaterialityGovernance =
        ofRule "BR-S-255" "Change materiality governed by business thresholds and planning relevance"

    let changeLifecycleDeterminism =
        ofRule "BR-S-256" "Change lifecycle state transitions are deterministic per PO-S-100"

    let supplyChangeAssessmentDerivationSource =
        ofRule "BR-S-257" "Supply Change Assessment derived exclusively from signals, baseline facts, and policy"

    let changeSignalNormalization =
        ofRule "BR-S-258" "Change signal normalization into canonical representation"

    let supplyQualityAssessmentIdentity =
        ofRule "BR-S-100" "Business identity of a Supply Quality Assessment is the combination of Assessment Scope Identifier, Planning Horizon, and Evaluation Period"

    let supplyQualityAssessmentEligibility =
        ofRule "BR-S-260" "Supply quality assessment requires current position assessments, commitment assessments, Published Supply Plan, and current Supply Quality Policy"

    let singlePublishedSupplyQualityAssessment =
        ofRule "BR-S-261" "Exactly one Published Supply Quality Assessment exists per Assessment Scope, Planning Horizon, and Evaluation Period at any moment"

    let publishedSupplyQualityAssessmentImmutability =
        ofRule "BR-S-262" "A Published Supply Quality Assessment is immutable"

    let permanentRetentionOfSupplyQualityAssessments =
        ofRule "BR-S-263" "All Supply Quality Assessment versions are retained permanently"

    let supplyQualityAssessmentVersionMonotonicity =
        ofRule "BR-S-264" "Version Number is monotonically increasing within the Supply Quality Assessment aggregate"

    let dimensionalQualityEvaluationGovernance =
        ofRule "BR-S-265" "Dimensional quality evaluation criteria and thresholds are governed exclusively by PO-S-110"

    let supplyQualityPublicationGovernance =
        ofRule "BR-S-266" "Supply quality publication criteria are governed exclusively by PO-S-110"

    let dimensionalQualityDerivationSource =
        ofRule "BR-S-267" "Dimensional quality metrics are derived by BA-S-070 from current position assessments, commitment assessments, and Published Supply Plan"

    let overallQualityDerivationSource =
        ofRule "BR-S-268" "Overall supply quality is derived by BA-S-071 from authoritative dimensional quality classifications"

    let supplyExceptionAssessmentIdentity =
        ofRule "BR-S-270" "The business identity of a Supply Exception Assessment is the combination of Governed Policy Reference, Exception Scope, and Exception Type"

    let exceptionDetectionEligibility =
        ofRule "BR-S-271" "Exception detection requires a Published Supply Change Assessment or Supply Quality Assessment and current Exception Detection Policy"

    let singleCurrentSupplyExceptionAssessment =
        ofRule "BR-S-272" "Exactly one current Supply Exception Assessment exists per monitored entity and exception type at any moment"

    let exceptionAssessmentStateChangeImmutability =
        ofRule "BR-S-273" "State change events within the Supply Exception Assessment are immutable"

    let exceptionAssessmentHistoryImmutability =
        ofRule "BR-S-274" "The complete history of exception assessments is permanently preserved"

    let deterministicLifecycleTransitions =
        ofRule "BR-S-275" "Lifecycle transitions are deterministic per PO-S-120; no business decision is invoked"

    let exceptionEvidencePublicationRequirement =
        ofRule "BR-S-276" "Every exception that meets detection thresholds must be published to Core Exception Management"

    let exceptionResolutionEvidenceRequirement =
        ofRule "BR-S-277" "Resolution evidence must be published when underlying data returns to within governed thresholds"

    let noDirectExceptionMutation =
        ofRule "BR-S-278" "Supply Intelligence never directly creates, updates, or resolves SE-C-019 Exception instances"

    let exceptionClassificationByPolicy =
        ofRule "BR-S-279" "Exception classification criteria and thresholds are governed exclusively by PO-S-120"

    let exceptionSeverityAssessmentByPolicy =
        ofRule "BR-S-279a" "Exception severity assessment is governed exclusively by PO-S-120"

    let exceptionEvidenceDerivationSource =
        ofRule "BR-S-279b" "Exception evidence is derived from Supply Change Assessments and Supply Quality Assessments"

    let enterpriseExceptionEscalationCriteria =
        ofRule "BR-S-279c" "Enterprise escalation occurs when exception is Confirmed and severity is Major/Severe or multi-facility"

    let supplyExceptionIdentitySpecification =
        ofRule "BR-S-290" "Guarantees deterministic identity binding across exception type, business scope, and causal entity"

    let exceptionTriggeringCriterionGovernance =
        ofRule "BR-S-291" "Requires exception existence to be established strictly through satisfied policy predicates in PO-S-120"

    let multiDimensionalSeverityIndependence =
        ofRule "BR-S-292" "Enforces independent evaluation of impact, urgency, scope, potential, and current severity"

    let temporalHorizonClassificationRule =
        ofRule "BR-S-293" "Enforces strict threshold gating for temporal horizon classification based on impact onset delta"

    let contextualCorrelationLinkageRule =
        ofRule "BR-S-294" "Requires related disruptions to be linked as context without spawning redundant standalone artifacts"

    let statelessAssessmentBoundary =
        ofRule "BR-S-295" "Prohibits lifecycle state transitions or action recommendations within algorithm execution"

    let evidenceSufficiencyGuard =
        ofRule "BR-S-296" "Prevents exception generation when supporting evidence fails minimum reliability thresholds"

    let supplyExplanationIdentity =
        ofRule "BR-S-120" "Composite key (ExplainedArtifactType, ExplainedArtifactId, Version) uniquely identifies an immutable Supply Explanation"

    let supplyExplanationCompositionEligibility =
        ofRule "BR-S-280" "Requires explained artifact to exist, all contributing evidence at historical versions, and PO-S-130 to be current"

    let supplyExplanationImmutability =
        ofRule "BR-S-281" "A Supply Explanation is immutable from the moment of creation; lifecycle is Created (terminal)"

    let historicalVersionReferencing =
        ofRule "BR-S-282" "Every evidence reference must point to the historical version in effect when the artifact was produced, not current version"

    let noInventedReasoning =
        ofRule "BR-S-283" "A Supply Explanation composes existing enterprise knowledge and never invents new reasoning"

    let explanationCompositionDeterminism =
        ofRule "BR-S-284" "Explanation composition is deterministic; identical inputs produce isomorphic reasoning graphs with zero decision branching"

    let explanationConfidenceAssessment =
        ofRule "BR-S-285" "Explanation confidence is computed from inherited evidence confidence, completeness, and consistency governed by PO-S-130"

    let explanationDerivationSource =
        ofRule "BR-S-286" "Supply Explanation is derived strictly from preserved enterprise knowledge available when the artifact was produced"

    let supplyLearningIdentity =
        ofRule "BR-S-130" "The business identity of a Supply Learning is the Learning Identifier, assigned at creation and globally unique"

    let supplyLearningDiscoveryEligibility =
        ofRule "BR-S-300" "Supply learning discovery requires historical supply evidence spanning the minimum number of periods governed by PO-S-140"

    let supplyLearningEvaluationEligibility =
        ofRule "BR-S-301" "Supply learning evaluation requires a candidate learning produced by BA-S-100 and current Supply Learning Policy"

    let supplyLearningImmutability =
        ofRule "BR-S-302" "A Supply Learning is immutable once created, except for the Learning Outcome which may be set exactly once"

    let permanentRetentionOfSupplyLearnings =
        ofRule "BR-S-303" "All Supply Learnings are retained permanently, regardless of their Learning Outcome"

    let singleLearningOutcomePerSupplyLearning =
        ofRule "BR-S-304" "The Learning Outcome of a Supply Learning is set exactly once and cannot be changed or reverted"

    let supplyLearningEvaluationGovernance =
        ofRule "BR-S-305" "Supply learning evaluation criteria and confidence thresholds are governed exclusively by PO-S-140"

    let candidateLearningDerivationSource =
        ofRule "BR-S-306" "Candidate learnings are derived by BA-S-100 exclusively from historical supply evidence spanning at least two capabilities"

    let confidenceAssessmentDerivation =
        ofRule "BR-S-307" "Pattern Confidence and Intervention Confidence are derived independently by BA-S-100 governed by PO-S-140"

    let supplyLearningIdentitySpecification =
        ofRule "BR-S-310" "Guarantees deterministic identity binding across pattern key, analysis scope, and policy version"

    let multiPeriodRecurrenceGating =
        ofRule "BR-S-311" "Forbids deriving learning from events that fail minimum recurrence frequency Theta_rec across periods"

    let independenceOfPatternAndInterventionConfidence =
        ofRule "BR-S-312" "Requires Pattern Confidence and Intervention Confidence to be derived via separate mathematical formulations"

    let zeroPrescriptiveAdoptionBoundary =
        ofRule "BR-S-313" "Prohibits the algorithm from adopting or mutating planning policies; output is strictly candidate"

    let crossCapabilityEvidenceCorroboration =
        ofRule "BR-S-314" "Rewards multi-source corroboration in Pattern Confidence derivation and caps single-source confidence at Low"

    let aiProposedPatternEquivalenceRule =
        ofRule "BR-S-315" "Requires candidate patterns proposed by external AI models to undergo identical empirical recurrence testing"

    let analogueValidatedInterventionSizing =
        ofRule "BR-S-316" "Requires proposed parameter changes to be bounded by historical analogue success distributions"



module Policies =
    let supplyDataAcceptancePolicy =
        ofPolicy "PO-S-010" "Governs criteria for accepting, quarantining, or rejecting incoming supply data"

    let supplyUnderstandingMaterialityPolicy =
        ofPolicy "PO-S-011" "Governs materiality thresholds per interpretation dimension and data completeness"

    let publicationCadencePolicy =
        ofPolicy "PO-S-012" "Governs maximum allowed staleness interval and periodic refresh for Supply Understanding"

    let constraintRelaxationPolicy =
        ofPolicy "PO-S-021" "Governs approval and documentation of constraint relaxations"

    let supplyPlanAcceptancePolicy =
        ofPolicy "PO-S-022" "Governs quality thresholds and stability criteria for plan acceptance"

    let supplyPlanPublicationCadencePolicy =
        ofPolicy "PO-S-023" "Governs publication cadence and out-of-cycle publication rules"

    let inventoryHealthPolicy =
        ofPolicy "PO-S-032" "Governs coverage thresholds, risk assessment criteria, and obsolescence criteria"

    let inventoryPolicyGovernance =
        ofPolicy "PO-S-033" "Governs calculation methodology, service level targets, and cost parameters"

    let inventoryHealthAssessmentPolicy =
        ofPolicy "PO-S-034" "Governs health classification thresholds, assessment frequency, and publication criteria"

    let capacityManagementPolicy =
        ofPolicy "PO-S-050" "Governs capacity operating envelopes, bottleneck criteria, health classification, and assessment frequency"

    let supplierCollaborationPolicy =
        ofPolicy "PO-S-060" "Governs commitment state transitions, confidence thresholds, responsiveness thresholds, reliability criteria, risk signal definitions, and health taxonomy"

    let procurementPolicy =
        ofPolicy "PO-S-070" "Governs sourcing rules, supplier allocation preferences, confidence thresholds, feasibility criteria, publication cadence, and review requirements"

    let schedulingPolicy =
        ofPolicy "PO-S-080" "Governs sequencing rules, changeover matrices, overtime limits, feasibility criteria, stability thresholds, confidence thresholds, frozen horizon definition, campaign scheduling rules, and publication cadence"

    let distributionPolicy =
        ofPolicy "PO-S-090" "Governs sourcing rules, lane preferences, allocation strategies, feasibility thresholds, stability thresholds, and confidence thresholds"

    let distributionPlanPublicationCadencePolicy =
        ofPolicy "PO-S-091" "Governs publication cadence, completeness requirements, and out-of-cycle publication rules for Distribution Plans"

    let supplyChangePolicy =
        ofPolicy "PO-S-100" "Governs detection thresholds, corroboration requirements, materiality criteria, confidence assessment rules, signal normalization rules, and lifecycle management for supply change assessments"

    let supplyQualityPolicy =
        ofPolicy "PO-S-110" "Governs performance dimensions, quality objectives, judgment thresholds, trend classification rules, overall composition, confidence computation, and publication cadence"

    let exceptionDetectionPolicy =
        ofPolicy "PO-S-120" "Governs exception types, triggering criteria, severity classification rules, horizon rules, lifecycle management, and evidence sufficiency thresholds"

    let explanationPolicy =
        ofPolicy "PO-S-130" "Governs automatic and on-demand explanation triggers, template catalog, revision/withdrawal rules, and confidence thresholds"

    let learningPolicy =
        ofPolicy "PO-S-140" "Governs discovery cadence, recurrence thresholds, evidence span, cross-capability corroboration, and confidence computation"



module BusinessAlgorithms =
    let evaluateUnderstandingChangeMateriality =
        ofBusinessAlgorithm "BA-S-001" "Compares draft and published supply interpretations across six dimensions"

    let balanceSupplyAndDemand =
        ofBusinessAlgorithm "BA-S-015" "Reconciles demand forecast with active supply understanding, BOMs, and capacity envelopes"

    let computeSafetyStock =
        ofBusinessAlgorithm "BA-S-002" "Calculates demand-variability safety stock buffer quantity"

    let computeReorderPoint =
        ofBusinessAlgorithm "BA-S-003" "Calculates replenishment trigger threshold"

    let evaluateInventoryRisk =
        ofBusinessAlgorithm "BA-S-004" "Evaluates multi-dimensional inventory risk across 5 orthogonal dimensions"

    let determineInventoryHealth =
        ofBusinessAlgorithm "BA-S-005" "Determines authoritative inventory health state via strict precedence cascade"

    let computeAvailableCapacity =
        ofBusinessAlgorithm "BA-S-010" "Computes net usable physical capacity from work shifts and merged downtime intervals"

    let computeCapacityOperatingEnvelope =
        ofBusinessAlgorithm "BA-S-011" "Derives multi-tier capacity operating boundaries and utilization guardrails"

    let evaluateCapacityExposure =
        ofBusinessAlgorithm "BA-S-012" "Evaluates multi-dimensional capacity exposure across six orthogonal dimensions"

    let determineCapacityHealth =
        ofBusinessAlgorithm "BA-S-013" "Determines authoritative enterprise capacity health state via governed precedence cascade"

    let evaluateCommitmentExposure =
        ofBusinessAlgorithm "BA-S-020" "Evaluates multi-dimensional commitment exposure across 7 orthogonal dimensions"

    let determineSupplierCollaborationHealth =
        ofBusinessAlgorithm "BA-S-021" "Determines authoritative enterprise collaboration health state via strict precedence cascade"

    let evaluateProcurementRequirement =
        ofBusinessAlgorithm "BA-S-030" "Evaluates net procurement gaps against lot-sizing, MOQs, packaging multiples, and lead-time horizons"

    let assessSourcingSituation =
        ofBusinessAlgorithm "BA-S-031" "Assesses sourcing situation across 4 sequential dimensions: Ecosystem, Feasibility, Risk, and Opportunity"

    let determineEnterpriseSourcingDetermination =
        ofBusinessAlgorithm "BA-S-032" "Determines enterprise sourcing determination as a governance quadruple (N, I, G, D)"

    let constructProductionSchedule =
        ofBusinessAlgorithm "BA-S-040" "Synthesizes authoritative, conflict-free, sequenced production activities respecting finite capacity, setups, material arrivals, and frozen horizon"

    let assessScheduleStability =
        ofBusinessAlgorithm "BA-S-041" "Computes schedule stability score against prior active schedule across job count, quantity, and timing variance"

    let constructDistributionRecommendation =
        ofBusinessAlgorithm "BA-S-050" "Synthesizes authoritative, operationally feasible Distribution Recommendation for planned transfer requirements respecting stock, lanes, and dock throughput"

    let assessDistributionPlanStability =
        ofBusinessAlgorithm "BA-S-051" "Computes distribution plan stability score and deviation against previously published baseline"

    let assessSupplyChange =
        ofBusinessAlgorithm "BA-S-060" "Evaluates operational signals against baseline facts, calculating variance magnitude, Bayesian evidence confidence, and planning materiality"

    let evaluateSupplyQualityDimensions =
        ofBusinessAlgorithm "BA-S-070" "Evaluates supply quality across each governed dimension against policy objectives, tracking trends, separating effectiveness from efficiency, and computing confidence"

    let deriveOverallSupplyQualityAssessment =
        ofBusinessAlgorithm "BA-S-071" "Synthesizes dimensional quality evaluations into a single enterprise-level quality assessment applying governed composition rules, mandatory vetoes, and capping cascades"

    let evaluateSupplyException =
        ofBusinessAlgorithm "BA-S-080" "Evaluates operational signals, change assessments, and quality evaluations against governed criteria to determine exception existence, severity, horizon, and correlation"

    let composeSupplyExplanation =
        ofBusinessAlgorithm "BA-S-090" "Deterministically composes complete, auditable Structured Reasoning and multi-level renderings behind supply intelligence conclusions"

    let deriveSupplyLearning =
        ofBusinessAlgorithm "BA-S-100" "Systematically analyzes multi-period cross-capability evidence to discover recurring empirical supply patterns and candidate improvements"



module BusinessWorkflows =
    let receiveSupplyData =
        ofBusinessWorkflow "BW-S-001" "Ensure every received supply transaction becomes one traceable enterprise record"

    let evaluateSupplyData =
        ofBusinessWorkflow "BW-S-002" "Ensure every received supply record receives one governed evaluation outcome"

    let reviseSupplyUnderstanding =
        ofBusinessWorkflow "BW-S-003" "Ensure latest Enterprise Picture and change signals are interpreted into draft"

    let publishSupplyUnderstanding =
        ofBusinessWorkflow "BW-S-004" "Ensure only materially changed and complete Supply Understanding is published"

    let balanceSupplyAndDemand =
        ofBusinessWorkflow "BW-S-010" "Orchestrates creation of draft supply plan via multi-echelon balancing"

    let evaluateSupplyPlan =
        ofBusinessWorkflow "BW-S-011" "Orchestrates evaluation of draft supply plan against acceptance criteria"

    let publishSupplyPlan =
        ofBusinessWorkflow "BW-S-012" "Orchestrates publication of accepted supply plan and notification dispatch"

    let maintainInventoryPositionUnderstanding =
        ofBusinessWorkflow "BW-S-020" "Maintain continuous inventory position understanding"

    let maintainInventoryPolicies =
        ofBusinessWorkflow "BW-S-021" "Maintain governed inventory policies"

    let assessInventoryHealth =
        ofBusinessWorkflow "BW-S-022" "Assess and publish periodic inventory health"

    let maintainCapacityPositionUnderstanding =
        ofBusinessWorkflow "BW-S-030" "Maintain continuous capacity position understanding for each active resource"

    let assessCapacityHealth =
        ofBusinessWorkflow "BW-S-031" "Assess and publish periodic capacity health across resources"

    let maintainSupplierCommitments =
        ofBusinessWorkflow "BW-S-050" "Ensure supplier commitments and commitment assessments are continuously maintained"

    let assessSupplierCollaborationHealth =
        ofBusinessWorkflow "BW-S-051" "Ensure periodic supplier collaboration health assessments are composed and published"

    let createProcurementRecommendations =
        ofBusinessWorkflow "BW-S-060" "Create governed procurement recommendations for each supply plan requirement"

    let publishProcurementPlan =
        ofBusinessWorkflow "BW-S-061" "Publish the authoritative procurement baseline for the planning cycle"

    let createProductionSchedules =
        ofBusinessWorkflow "BW-S-070" "Create sequenced, feasible production schedules for each constrained resource"

    let publishProductionScheduleBaseline =
        ofBusinessWorkflow "BW-S-071" "Publish the authoritative production scheduling baseline for the planning period"

    let createDistributionRecommendations =
        ofBusinessWorkflow "BW-S-080" "Create governed distribution recommendations for each distribution requirement"

    let publishDistributionPlan =
        ofBusinessWorkflow "BW-S-081" "Publish the authoritative distribution baseline for the planning cycle"

    let assessSupplyChangesWorkflow =
        ofBusinessWorkflow "BW-S-085" "Continuously assess supply changes across all monitored entities and change types"

    let evaluateSupplyQualityWorkflow =
        ofBusinessWorkflow "BW-S-090" "Ensure authoritative Supply Quality Assessment is composed, evaluated, and published for the evaluation period"

    let evaluateSupplyExceptionsWorkflow =
        ofBusinessWorkflow "BW-S-100" "Evaluate whether correlated evidence violates governed supply policies and maintain exception lifecycle"

    let composeSupplyExplanation =
        ofBusinessWorkflow "BW-S-110" "Ensure every requested or triggered supply outcome receives one authoritative, immutable explanation"

    let discoverAndRecordSupplyLearning =
        ofBusinessWorkflow "BW-S-120" "Discover recurring supply patterns from historical evidence and record immutable enterprise learnings"

    let recordLearningOutcomeWorkflow =
        ofBusinessWorkflow "BW-S-121" "Record the authorized governance outcome on a candidate Supply Learning"



module FunctionalSpecifications =
    let receiveSupplyData =
        ofFunctionalSpecification "FS-S-001" "Orchestrates receipt of raw supply transaction into SE-S-010"

    let evaluateSupplyData =
        ofFunctionalSpecification "FS-S-002" "Orchestrates evaluation of SE-S-010 and notification dispatch"

    let reviseSupplyUnderstanding =
        ofFunctionalSpecification "FS-S-003" "Orchestrates revision of SE-S-001 draft version"

    let publishSupplyUnderstanding =
        ofFunctionalSpecification "FS-S-004" "Orchestrates evaluation of materiality and publication of SE-S-001"

    let balanceSupplyAndDemand =
        ofFunctionalSpecification "FS-S-010" "Orchestrates execution of BA-S-015 to produce draft SE-S-020"

    let evaluateSupplyPlan =
        ofFunctionalSpecification "FS-S-011" "Orchestrates execution of DE-S-011 to evaluate draft SE-S-020"

    let publishSupplyPlan =
        ofFunctionalSpecification "FS-S-012" "Orchestrates execution of DE-S-012 to publish SE-S-020"

    let maintainInventoryPositionUnderstanding =
        ofFunctionalSpecification "FS-S-020" "Orchestrates maintenance of SE-S-030"

    let maintainInventoryPolicies =
        ofFunctionalSpecification "FS-S-021" "Orchestrates maintenance of SE-S-031"

    let assessInventoryHealth =
        ofFunctionalSpecification "FS-S-022" "Orchestrates publication of SE-S-032"

    let maintainCapacityPositionUnderstanding =
        ofFunctionalSpecification "FS-S-030" "Orchestrates maintenance of SE-S-040"

    let assessCapacityHealth =
        ofFunctionalSpecification "FS-S-031" "Orchestrates publication of SE-S-041"

    let maintainSupplierCommitments =
        ofFunctionalSpecification "FS-S-050" "Orchestrates maintenance of SE-S-050 and SE-S-051"

    let assessSupplierCollaborationHealth =
        ofFunctionalSpecification "FS-S-051" "Orchestrates publication of SE-S-052"

    let createProcurementRecommendations =
        ofFunctionalSpecification "FS-S-060" "Orchestrates maintenance and generation of SE-S-060"

    let publishProcurementPlan =
        ofFunctionalSpecification "FS-S-061" "Orchestrates composition and publication of SE-S-061"

    let createProductionSchedules =
        ofFunctionalSpecification "FS-S-070" "Orchestrates creation of sequenced, feasible production schedules via AB-S-060"

    let publishProductionScheduleBaseline =
        ofFunctionalSpecification "FS-S-071" "Orchestrates composition and publication of production scheduling baseline via AB-S-061"

    let createDistributionRecommendations =
        ofFunctionalSpecification "FS-S-080" "Orchestrates creation of governed distribution recommendations via AB-S-070"

    let publishDistributionPlan =
        ofFunctionalSpecification "FS-S-081" "Orchestrates composition and publication of distribution plan baseline via AB-S-071"

    let assessSupplyChangesFunctionalSpecification =
        ofFunctionalSpecification "FS-S-085" "Orchestrates sensing, normalization, assessment, and lifecycle management of supply changes via AB-S-080"

    let evaluateSupplyPerformanceFunctionalSpecification =
        ofFunctionalSpecification "FS-S-090" "Orchestrates dimensional evaluation, overall synthesis, and publication of SE-S-100 via AB-S-090"

    let evaluateSupplyExceptions =
        ofFunctionalSpecification "FS-S-100" "Orchestrates evidence gathering, evaluation via AB-S-100, and notification dispatch for supply exceptions"

    let recordSupplyExplanation =
        ofFunctionalSpecification "FS-S-110" "Orchestrates execution of BA-S-090, persistence of SE-S-120 via AB-S-110, and dispatch of BN-S-110"

    let recordSupplyLearningFS =
        ofFunctionalSpecification "FS-S-120" "Orchestrates execution of BA-S-100, persistence of SE-S-130 via AB-S-120, and dispatch of BN-S-120"

    let recordLearningOutcomeFS =
        ofFunctionalSpecification "FS-S-121" "Orchestrates recording of governance decision on SE-S-130 via AB-S-121 and dispatch of BN-S-121"



module EnterpriseEvents =
    let supplyDataRecordReceived =
        ofEnterpriseEvent "EV-S-001" "Published by AB-S-001 when a supply data record is received"

    let supplyDataRecordEvaluated =
        ofEnterpriseEvent "EV-S-002" "Published by AB-S-002 when a supply data record is evaluated"

    let supplyUnderstandingRevised =
        ofEnterpriseEvent "EV-S-003" "Published by AB-S-003 when a draft Supply Understanding is created or updated"

    let supplyUnderstandingPublished =
        ofEnterpriseEvent "EV-S-004" "Published by AB-S-005 when a Supply Understanding version is published"

    let supplyPlanGenerated =
        ofEnterpriseEvent "EV-S-010" "Published by AB-S-010 when a draft Supply Plan Version is created"

    let supplyPlanEvaluated =
        ofEnterpriseEvent "EV-S-011" "Published by AB-S-011 when a draft Supply Plan Version is evaluated"

    let supplyPlanPublished =
        ofEnterpriseEvent "EV-S-012" "Published by AB-S-012 when a draft Supply Plan Version is published"

    let inventoryPositionAssessmentChanged =
        ofEnterpriseEvent "EV-S-020" "Published by AB-S-020 when inventory position assessment changes"

    let inventoryPolicyAssignmentUpdated =
        ofEnterpriseEvent "EV-S-021" "Published by AB-S-021 when inventory policy parameters are updated"

    let inventoryHealthAssessmentPublished =
        ofEnterpriseEvent "EV-S-022" "Published by AB-S-022 when inventory health assessment is published"

    let capacityPositionAssessmentChanged =
        ofEnterpriseEvent "EV-S-030" "Published by AB-S-030 when capacity position interpretation changes"

    let capacityHealthAssessmentPublished =
        ofEnterpriseEvent "EV-S-031" "Published by AB-S-031 when capacity health assessment is published"

    let supplierCommitmentChanged =
        ofEnterpriseEvent "EV-S-040" "Published by AB-S-040 when a supplier commitment state changes"

    let supplierCommitmentAssessmentChanged =
        ofEnterpriseEvent "EV-S-041" "Published by AB-S-041 when a supplier commitment assessment changes"

    let supplierCollaborationHealthAssessmentPublished =
        ofEnterpriseEvent "EV-S-042" "Published by AB-S-042 when a collaboration health assessment version is published"

    let procurementRecommendationCreated =
        ofEnterpriseEvent "EV-S-050" "Published by AB-S-050 when an active procurement recommendation is created or superseded"

    let procurementPlanPublished =
        ofEnterpriseEvent "EV-S-051" "Published by AB-S-051 when a procurement plan version is published"

    let productionScheduleActivated =
        ofEnterpriseEvent "EV-S-060" "Published by AB-S-060 when a new Production Schedule version becomes Active"

    let productionSchedulePublished =
        ofEnterpriseEvent "EV-S-061" "Published by AB-S-061 when a production schedule baseline is published"

    let distributionRecommendationCreated =
        ofEnterpriseEvent "EV-S-070" "Published by AB-S-070 when an active distribution recommendation is created or superseded"

    let distributionPlanPublished =
        ofEnterpriseEvent "EV-S-071" "Published by AB-S-071 when a distribution plan version is published"

    let supplyChangeAssessmentUpdated =
        ofEnterpriseEvent "EV-S-080" "Published by AB-S-080 when a supply change assessment lifecycle state transitions or is materially updated"

    let supplyQualityAssessmentPublished =
        ofEnterpriseEvent "EV-S-090" "Published by AB-S-090 when a Supply Quality Assessment version is published"

    let supplyExceptionAssessmentUpdated =
        ofEnterpriseEvent "EV-S-100" "Published by AB-S-100 when a Supply Exception Assessment is created, updated, or transitions lifecycle state"

    let supplyExceptionDetectionEvidenceProduced =
        ofEnterpriseEvent "EV-S-101" "Published by AB-S-100 when a confirmed exception satisfies enterprise escalation criteria per BR-S-279c"

    let supplyExceptionResolutionEvidenceProduced =
        ofEnterpriseEvent "EV-S-102" "Published by AB-S-100 when an enterprise-escalated exception transitions to Resolved"

    let supplyExplanationRecorded =
        ofEnterpriseEvent "EV-S-110" "Published by AB-S-110 when a new Supply Explanation version is created and persisted as SE-S-120"

    let supplyLearningRecorded =
        ofEnterpriseEvent "EV-S-120" "Published by AB-S-120 when a new Supply Learning is discovered and recorded in Pending Outcome state"

    let supplyLearningOutcomeRecorded =
        ofEnterpriseEvent "EV-S-121" "Published by AB-S-121 when an authorized governance decision outcome is recorded on a Supply Learning"



module BusinessNotifications =
    let supplyUnderstandingPublished =
        ofBusinessNotification "BN-S-001" "Notifies downstream capabilities of new authoritative supply interpretation"

    let supplyDataQuarantined =
        ofBusinessNotification "BN-S-002" "Notifies Supply Data Steward that a record requires manual review"

    let supplyDataRejected =
        ofBusinessNotification "BN-S-003" "Notifies governance that a supply record was permanently excluded"

    let supplyPlanPublished =
        ofBusinessNotification "BN-S-010" "Notifies downstream capabilities of new authoritative Supply Plan baseline"

    let inventoryPositionAssessmentChanged =
        ofBusinessNotification "BN-S-020" "Notifies downstream capabilities of inventory position change"

    let inventoryPolicyAssignmentUpdated =
        ofBusinessNotification "BN-S-021" "Notifies downstream capabilities of inventory policy update"

    let inventoryHealthAssessmentPublished =
        ofBusinessNotification "BN-S-022" "Notifies management and governance of published inventory health assessment"

    let forecastPublished =
        ofBusinessNotification "BN-D-011" "External notification from Demand domain when a forecast is published"

    let capacityPositionAssessmentChanged =
        ofBusinessNotification "BN-S-030" "Notifies downstream capabilities of capacity position change"

    let capacityHealthAssessmentPublished =
        ofBusinessNotification "BN-S-031" "Notifies management and planning of published capacity health assessment"

    let supplierCommitmentChanged =
        ofBusinessNotification "BN-S-040" "Notifies downstream capabilities that a supplier commitment state has changed"

    let supplierCommitmentAssessmentChanged =
        ofBusinessNotification "BN-S-041" "Notifies downstream capabilities that a supplier commitment assessment has changed"

    let supplierCollaborationHealthAssessmentPublished =
        ofBusinessNotification "BN-S-042" "Notifies downstream capabilities and governance of published collaboration health assessment"

    let procurementRecommendationCreated =
        ofBusinessNotification "BN-S-050" "Notifies downstream capabilities of new active procurement recommendation"

    let procurementPlanPublished =
        ofBusinessNotification "BN-S-051" "Notifies procurement execution and downstream capabilities of published procurement plan"

    let productionScheduleActivated =
        ofBusinessNotification "BN-S-060" "Notifies downstream capabilities of new or revised Active Production Schedule"

    let productionSchedulePublished =
        ofBusinessNotification "BN-S-061" "Notifies downstream capabilities and execution systems of published production schedule baseline"

    let distributionRecommendationCreated =
        ofBusinessNotification "BN-S-070" "Notifies downstream capabilities of new or revised Active Distribution Recommendation"

    let distributionPlanPublished =
        ofBusinessNotification "BN-S-071" "Notifies distribution execution and downstream capabilities of published distribution plan baseline"

    let supplyChangeAssessmentUpdatedNotification =
        ofBusinessNotification "BN-S-080" "Notifies downstream capabilities that a supply change assessment has been updated"

    let supplyQualityAssessmentPublishedNotification =
        ofBusinessNotification "BN-S-090" "Notifies downstream capabilities and governance of published supply quality assessment"

    let supplyExceptionAssessmentUpdatedNotification =
        ofBusinessNotification "BN-S-100" "Notifies downstream capabilities that a Supply Exception Assessment has been created, updated, or transitioned"

    let supplyExceptionDetectionEvidenceNotification =
        ofBusinessNotification "BN-S-101" "Transmits authoritative detection evidence to Core Exception Management to instantiate or update enterprise-level exception"

    let supplyExceptionResolutionEvidenceNotification =
        ofBusinessNotification "BN-S-102" "Transmits authoritative resolution evidence to Core Exception Management to complete lifecycle transition of enterprise exception"

    let supplyExplanationRecorded =
        ofBusinessNotification "BN-S-110" "Published by BW-S-110 Notification Node notifying downstream consumers that a new Supply Explanation has been recorded"

    let supplyLearningRecordedNotification =
        ofBusinessNotification "BN-S-120" "Notifies downstream capabilities and governance that a new Supply Learning has been recorded"

    let supplyLearningOutcomeRecordedNotification =
        ofBusinessNotification "BN-S-121" "Notifies downstream capabilities and governance that a Supply Learning outcome has been recorded"

module ExternalNotifications =
    let enterprisePicturePublished = ofBusinessNotification "BN-C-001" "Notifies that Enterprise Picture is published"
    let pictureVersionPublished = ofEnterpriseEvent "EV-C-002" "Fired when enterprise picture version is published"
    let enterpriseExceptionActive = ofBusinessNotification "BN-C-002" "Published when an enterprise exception is registered or updated"
    let enterpriseExceptionResolved = ofBusinessNotification "BN-C-003" "Published when an enterprise exception is resolved"





module Medhavi.Demand.ArsIdentifiers

open Medhavi.Foundation.Contracts.ArsArtifact

let domain = "D"

module Capabilities =
    let understandDemand =
        ofCapability
            "CA-D-001"
            "Ingests, evaluates, and interprets demand signals into an authoritative Demand Understanding"

    let forecastDemand =
        ofCapability
            "CA-D-002"
            "Generates, evaluates, and publishes authoritative, versioned demand projections and prediction intervals"

    let senseDemand =
        ofCapability
            "CA-D-003"
            "Continuously evaluates incoming demand signals against expected behavior baseline and detects meaningful deviations"

    let segmentDemand =
        ofCapability "CA-D-004" "Maintains governed planning classifications (ABC/XYZ) for planning entities"

    let classifyDemand =
        ofCapability
            "CA-D-005"
            "Analyzes statistical features of demand series and assigns behavior classifications to govern model selection"

    let prioritizeDemand =
        ofCapability
            "CA-D-006"
            "Calculates multi-dimensional priority scores and assigns planning priority levels to establish execution precedence"

    let evaluateDemandQuality =
        ofCapability
            "CA-D-007"
            "Measures forecast accuracy, bias, FVA, and tracking signals against materialized actuals to publish authoritative quality assessments"

    let detectDemandExceptions =
        ofCapability
            "CA-D-008"
            "Evaluates demand exception evidence (Bias, Accuracy, Completeness, Volatility, Tracking Signal, FVA) and publishes evidence to Core Exception Management"

    let explainDemand =
        ofCapability
            "CA-D-009"
            "Composes structured, deterministic, and auditable explanations for any demand intelligence conclusion from preserved historical evidence and policies"

    let learnFromDemand =
        ofCapability
            "CA-D-010"
            "Continuously discovers and records what the enterprise has learned about demand patterns and forecasting performance to improve future planning cycles"

    let modelDemandInterventions =
        ofCapability
            "CA-D-011"
            "Assesses the demand impact of planned commercial interventions and publishes deterministic impact assessments for consumption by the forecast capability"

module Responsibilities =
    let receiveDemandObservation = ofResponsibility "CR-D-001" "Establishes a Demand Observation with full provenance"

    let evaluateDemandObservation =
        ofResponsibility "CR-D-002" "Evaluates a Demand Observation against acceptance criteria"

    let reviseDemandUnderstanding =
        ofResponsibility "CR-D-003" "Creates or updates a Draft Demand Understanding based on new enterprise facts"

    let publishDemandUnderstanding =
        ofResponsibility "CR-D-004" "Evaluates materiality and publishes a Draft Demand Understanding when warranted"

    let establishForecastCycle =
        ofResponsibility "CR-D-005" "Establishes a new forecast cycle identity and initial workflow state"

    let produceForecastProjection =
        ofResponsibility
            "CR-D-006"
            "Generates statistical forecast lines and prediction intervals for a draft publication"

    let governForecastOverrides =
        ofResponsibility "CR-D-007" "Processes and records planner overrides while preserving original system forecasts"

    let publishForecastPublication =
        ofResponsibility
            "CR-D-008"
            "Releases a draft forecast publication as the single authoritative enterprise projection"

    let maintainDemandBehaviorUnderstanding =
        ofResponsibility "CR-D-009" "Maintains authoritative demand behavior state per monitored Item-Location"

    let escalateCriticalDemandBehavior =
        ofResponsibility "CR-D-010" "Evaluates critical demand behavior state changes for out-of-cycle forecast refresh"

    let classifyPlanningEntity =
        ofResponsibility "CR-D-011" "Evaluates and maintains authoritative planning classification for an entity"

    let classifyDemandBehavior =
        ofResponsibility
            "CR-D-012"
            "Analyzes statistical demand patterns (Continuous, Intermittent, Seasonal, Lumpy, Trend) for an Item-Location"

    let calculatePlanningPriority =
        ofResponsibility
            "CR-D-013"
            "Computes priority scores from business dimensions and assigns priority levels per PO-D-039"

    let evaluateForecastQuality =
        ofResponsibility
            "CR-D-014"
            "Calculates quality metrics against actual outcomes and governs publication of quality assessments"

    let detectDemandExceptions =
        ofResponsibility
            "CR-D-015"
            "Calculates demand exception evidence against governed detection criteria and publishes evidence to Core Exception Management"

    let composeDemandExplanation =
        ofResponsibility
            "CR-D-016"
            "Composes canonical Structured Reasoning Graphs from preserved historical evidence and policies"

    let establishDemandLearning =
        ofResponsibility
            "CR-D-017"
            "Derives and records immutable enterprise learnings from multi-period historical evidence"

    let assessDemandInterventionImpact =
        ofResponsibility
            "CR-D-018"
            "Produces a deterministic impact assessment for a planned commercial intervention"

    let publishDemandInterventionImpact =
        ofResponsibility
            "CR-D-019"
            "Publishes the Draft Demand Intervention Impact, making it authoritative and superseding any prior version"

module Decisions =
    let acceptDemandObservation =
        ofDecision "DE-D-001" "Determines whether a Demand Observation is Accepted, Quarantined, or Rejected"

    let approveDemandUnderstandingPublication =
        ofDecision
            "DE-D-002"
            "Determines whether a Draft Demand Understanding meets materiality thresholds for publication"

    let generateForecastForSeries =
        ofDecision
            "DE-D-003"
            "Determines whether a demand series is forecastable or requires a governed fallback method"

    let approveForecastPublication =
        ofDecision
            "DE-D-004"
            "Determines whether a Draft Forecast Publication meets completeness and governance criteria for release"

    let evaluateForecastOverride =
        ofDecision
            "DE-D-005"
            "Determines whether a planner forecast override is authorized under governed deviation limits"

    let evaluateDemandSignalForStateChange =
        ofDecision
            "DE-D-006"
            "Determines whether an incoming demand signal represents a meaningful change in demand behavior warranting a state transition"

    let triggerForecastRefreshOnCriticalState =
        ofDecision
            "DE-D-007"
            "Determines whether a Critical demand behavior state change warrants an immediate out-of-cycle forecast refresh evaluation"

    let determinePlanningClassification =
        ofDecision
            "DE-D-008"
            "Determines the planning classification for an entity under a governed scheme per PO-D-035"

    let determineBehaviorClassification =
        ofDecision
            "DE-D-009"
            "Determines the behavioral classification for a demand series under a governed dimension per PO-D-037"

    let determinePlanningPriority =
        ofDecision "DE-D-010" "Determines the planning priority level and score for a planning entity per PO-D-039"

    let publishForecastQualityAssessment =
        ofDecision
            "DE-D-011"
            "Determines whether a Forecast Quality Assessment meets completeness and length criteria for publication per PO-D-041"

    let evaluateDemandExceptionEvidence =
        ofDecision
            "DE-D-012"
            "Determines whether demand exception evidence or resolution evidence exists and assigns severity level per PO-D-044"

    let selectChampionModel =
        ofDecision
            "DE-D-013"
            "Selects the champion forecasting model for a forecast cycle based on statistical accuracy and governance criteria"

    let approveDemandExplanation =
        ofDecision
            "DE-D-014"
            "Evaluates demand explanation graph completeness and governs immutable persistence per PO-D-047"

    let approveDemandLearning =
        ofDecision
            "DE-D-015"
            "Determines whether a candidate demand learning meets policy recurrence and confidence criteria for permanent recording per PO-D-048"

    let approveInterventionImpactPublication =
        ofDecision
            "DE-D-014"
            "Determines whether a Demand Intervention Impact meets confidence and completeness criteria for authoritative publication per PO-D-050"

module Rules =
    let demandSignalTimeliness = ofRule "BR-D-200" "Demand signal latency must not exceed the governed maximum"
    let demandQuantityRangeValidity = ofRule "BR-D-201" "Demand quantity must be non-negative"
    let sourceReliabilityThreshold = ofRule "BR-D-202" "Source system reliability must meet the governed minimum"
    let duplicateDataDetection = ofRule "BR-D-203" "Identical observations within the detection window are rejected"
    let receivedStatePrerequisite = ofRule "BR-D-210" "Observation must be in Received state to be evaluated"
    let observationExistencePrerequisite = ofRule "BR-D-211" "Observation must exist to be evaluated"

    let demandUnderstandingAggregateIdentity =
        ofRule
            "BR-D-002"
            "The business identity of a Demand Understanding is the Planning Scope; exactly one aggregate per Planning Scope"

    let singlePublishedDemandUnderstanding =
        ofRule "BR-D-103" "Exactly one Published Demand Understanding version exists per Planning Scope at any moment"

    let publishedDemandUnderstandingImmutability =
        ofRule "BR-D-104" "A Published Demand Understanding version is immutable"

    let materialChangeRequiredForPublication =
        ofRule
            "BR-D-204"
            "Publication requires material change in at least one interpretation dimension, or a periodic refresh due per PO-D-012"

    let interpretationCompletenessThreshold =
        ofRule "BR-D-205" "Interpretation completeness must meet the threshold defined in PO-D-011"

    let demandUnderstandingDerivationSource =
        ofRule
            "BR-D-400"
            "The Demand Understanding is derived exclusively from the latest Published Enterprise Picture and, if available, the most recent Published Forecast Publication"

    let demandBehaviorAssessmentIdentity =
        ofRule
            "BR-D-004"
            "The business identity of a Demand Behavior Assessment is Item + Location; exactly one aggregate per monitored Item-Location"

    let singleCurrentBehaviorState =
        ofRule
            "BR-D-110"
            "Exactly one Demand Behavior State (Normal, Elevated, Depressed, Critical) is active at any moment"

    let immutableStateChangeEvents =
        ofRule "BR-D-111" "State Change Events recorded in Demand Behavior Assessment are immutable"

    let deviationThresholds = ofRule "BR-D-300" "Deviation thresholds for state change are governed by PO-D-031"

    let corroborationRequirement =
        ofRule "BR-D-301" "Critical state requires corroboration by at least two independent sources"

    let highPrioritySensitivity =
        ofRule "BR-D-302" "High-priority items use a lowered Significant threshold per PO-D-031"

    let noiseSuppression = ofRule "BR-D-303" "Signals below noise threshold are suppressed"

    let forecastRefreshEvaluation =
        ofRule "BR-D-304" "Critical state changes are evaluated for forecast refresh trigger per PO-D-032"

    let classificationDeterminedBySegmentationPolicy =
        ofRule "BR-D-305" "Classification must be determined by the rules defined in the current Segmentation Policy"

    let minimumEvidenceForClassification =
        ofRule "BR-D-306" "An entity shall be classified as Unclassified if minimum evidence requirements are not met"

    let behaviorClassificationDeterminedByPolicy =
        ofRule
            "BR-D-307"
            "Behavior classification must be determined by the rules defined in the current Classification Policy"

    let minimumEvidenceForBehaviorClassification =
        ofRule "BR-D-308" "An entity shall be classified as Unclassified if minimum evidence requirements are not met"

    let prioritizationDeterminedByPolicy =
        ofRule
            "BR-D-309"
            "Priority must be determined using the scoring methodology and level thresholds defined in the current Prioritization Policy"

    let minimumEvidenceForPrioritization =
        ofRule
            "BR-D-310"
            "An entity shall be assigned Unclassified priority if mandatory business evidence is not available"

    let qualityAssessmentCompletenessRequirement =
        ofRule
            "BR-D-212"
            "A Forecast Quality Assessment shall only be published if actual demand data covers the full evaluation period and meets completeness threshold"

    let qualityAssessmentEvaluationPeriodMinimum =
        ofRule
            "BR-D-213"
            "The evaluation period shall meet the minimum length defined in the Forecast Measurement Policy"

    let demandExceptionEvidenceRequirement =
        ofRule
            "BR-D-311"
            "Demand exception evidence shall only be published if the detection evidence meets the thresholds in PO-D-044"

    let explanationImmutability =
        ofRule "BR-D-124" "Demand explanations are immutable once recorded and preserved permanently"

    let learningIdentity =
        ofRule "BR-D-011" "A demand learning shall be identified by a unique enterprise learning identifier"

    let learningImmutability =
        ofRule "BR-D-125" "A demand learning is immutable once recorded and permanently preserved"

    let learningMinimumRecurrence =
        ofRule
            "BR-D-411"
            "A learning shall only be derived when evidence demonstrates recurrence across minimum governed periods"

    let learningMinimumHorizonWindow =
        ofRule
            "BR-D-412"
            "A learning shall only be evaluated over an evidence horizon meeting the minimum window defined in PO-D-048"

    let learningPatternConfidenceCriteria =
        ofRule
            "BR-D-413"
            "Pattern confidence must satisfy governed high/medium/low thresholds based on recurrence and significance"

    let forecastMethodSelectionMatrix =
        ofRule
            "BR-D-200"
            "Forecasting method is selected per governed matrix based on series classification and sparsity"

    let forecastabilityMinimumData =
        ofRule "BR-D-201" "Series must meet minimum historical data point threshold to be statistically forecastable"

    let overrideAuthorizationThresholds =
        ofRule
            "BR-D-202"
            "Planner overrides must not exceed maximum percentage deviation without elevated authorization"

    let overrideReasonCodeMandatory =
        ofRule "BR-D-203" "All planner overrides must carry a non-empty business justification and valid reason code"

    let horizonCoverageValidation =
        ofRule "BR-D-204" "Forecast publication must cover all planning periods in the defined horizon"

    let unforecastableSeriesHandling =
        ofRule "BR-D-205" "Unforecastable series must receive a designated fallback forecast per PO-D-019"

    let publicationCompletenessGovernance =
        ofRule "BR-D-206" "Forecast publication completeness score must meet minimum threshold defined in PO-D-020"

    let authorisedForecastingStrategy =
        ofRule "BR-D-401" "Champion model selection must conform to PO-D-017 model governance criteria"

    let interventionImpactNonNegativity =
        ofRule "BR-D-414" "Assessed Demand Lift must be non-negative"

    let interventionReferenceValidity =
        ofRule
            "BR-D-415"
            "The Intervention Reference in a Demand Intervention Impact must point to an active Scenario Adjustment"

module Policies =
    let demandDataAcceptance =
        ofPolicy
            "PO-D-001"
            "Governs the criteria and actions for accepting, quarantining, or rejecting demand observations"

    let demandUnderstandingMateriality =
        ofPolicy "PO-D-011" "Governs the materiality thresholds for publishing Demand Understanding revisions"

    let demandUnderstandingPublicationCadence =
        ofPolicy "PO-D-012" "Governs the maximum allowed interval between Demand Understanding publications"

    let forecastModelGovernance =
        ofPolicy "PO-D-017" "Governs champion model selection and challenger evaluation criteria"

    let unforecastableSeries =
        ofPolicy "PO-D-019" "Governs criteria and fallback methods for unforecastable demand series"

    let forecastPublicationGovernance =
        ofPolicy
            "PO-D-020"
            "Governs the release criteria, completeness thresholds, and authority of Forecast Publications"

    let forecastOverrideAuthorization =
        ofPolicy "PO-D-022" "Governs planner override bounds, justification requirements, and authorization levels"

    let forecastModelParameters =
        ofPolicy
            "PO-D-023"
            "Governs smoothing constants, seasonality lengths, and prediction interval confidence levels"

    let forecastGenerationTimeliness =
        ofPolicy "PO-D-024" "Governs the maximum permitted duration of a forecast generation cycle"

    let demandSensing =
        ofPolicy
            "PO-D-031"
            "Governs the deviation thresholds, corroboration requirements, and state transition rules for demand behavior assessment"

    let forecastRefreshTrigger =
        ofPolicy
            "PO-D-032"
            "Governs when a Critical demand behavior state triggers an automatic evaluation for an out-of-cycle forecast refresh"

    let forecastRefreshExecution =
        ofPolicy
            "PO-D-034"
            "Governs whether a Critical demand behavior state triggers a partial or full forecast refresh"

    let segmentationPolicy =
        ofPolicy
            "PO-D-035"
            "Governs ABC Pareto thresholds, XYZ coefficient of variation cutoffs, and analog item inheritance"

    let segmentationOverridePolicy =
        ofPolicy "PO-D-036" "Governs manual planner classification overrides, mandatory justification, and audit review"

    let classificationPolicy =
        ofPolicy
            "PO-D-037"
            "Governs ADI, CV², seasonal autocorrelation, and trend p-value thresholds for demand behavior classification"

    let classificationOverridePolicy =
        ofPolicy "PO-D-038" "Governs manual planner overrides of demand behavior classifications"

    let prioritizationPolicy =
        ofPolicy
            "PO-D-039"
            "Governs scoring methodology, dimension weights, and level cutoffs for planning priority calculation"

    let prioritizationOverridePolicy =
        ofPolicy "PO-D-040" "Governs manual planner overrides of planning priority assignments"

    let forecastMeasurement =
        ofPolicy
            "PO-D-041"
            "Governs WAPE, Bias, Tracking Signal, and Completeness thresholds for forecast quality measurement and publication"

    let demandExceptionEvidence =
        ofPolicy
            "PO-D-044"
            "Governs exception types, detection thresholds, severity rules, and resolution criteria for demand exception evidence"

    let explanationGovernance =
        ofPolicy
            "PO-D-047"
            "Governs completeness, determinism, required template elements, and evidence preservation for demand explanations"

    let learningAnalysis =
        ofPolicy
            "PO-D-048"
            "Governs minimum recurrence periods, sample sizes, statistical significance, and confidence criteria for demand learning derivation"

    let interventionModelingGovernance =
        ofPolicy
            "PO-D-050"
            "Governs modeling approach selection, confidence thresholds, and publication criteria for demand intervention impact assessments"

module SemanticObjects =
    let demandObservation =
        ofSemanticObject "SE-D-001" "The enterprise fact representing a single observed demand signal"

    let demandUnderstanding =
        ofSemanticObject "SE-D-002" "The enterprise interpretation of current demand patterns, health, and volatility"

    let forecastPublication =
        ofSemanticObject "SE-D-003" "The authoritative, versioned demand projection for a Planning Scope and horizon"

    let demandBehaviorAssessment =
        ofSemanticObject
            "SE-D-004"
            "The enterprise assessment of demand behavior and state transitions for a monitored Item-Location"

    let planningClassificationAssignment =
        ofSemanticObject
            "SE-D-005"
            "The enterprise's authoritative planning classification for an entity under a governed scheme"

    let demandBehaviorAssignment =
        ofSemanticObject
            "SE-D-006"
            "The enterprise's authoritative behavioral classification for a SKU-Location and dimension"

    let planningPriorityAssignment =
        ofSemanticObject "SE-D-007" "The enterprise's authoritative planning priority score and level for an entity"

    let forecastQualityAssessment =
        ofSemanticObject
            "SE-D-008"
            "The enterprise's authoritative assessment of forecast accuracy, bias, and stability for a Planning Scope and Evaluation Period"

    let forecastQualityAssessmentVersion =
        ofSemanticObject "SE-D-014" "A single immutable version of a Forecast Quality Assessment"

    let demandExplanation =
        ofSemanticObject
            "SE-D-010"
            "The immutable enterprise record maintaining the canonical Structured Reasoning Graph explaining a demand conclusion"

    let demandLearning =
        ofSemanticObject
            "SE-D-011"
            "The enterprise's authoritative discovery regarding recurring demand patterns or performance"

    let demandInterventionImpact =
        ofSemanticObject
            "SE-D-018"
            "The enterprise-recognised assessment of how a planned commercial intervention affects demand for a specific item-location"

module EnterpriseEvents =
    let demandObservationReceived =
        ofEnterpriseEvent "EV-D-001" "Fired when a new Demand Observation is established in Received state"

    let demandObservationEvaluated =
        ofEnterpriseEvent "EV-D-002" "Fired when a Demand Observation transitions to Accepted, Quarantined, or Rejected"

    let demandUnderstandingRevised =
        ofEnterpriseEvent "EV-D-003" "Fired when a Draft Demand Understanding is created or updated"

    let demandUnderstandingPublished =
        ofEnterpriseEvent "EV-D-004" "Fired when a Demand Understanding is published and previous version superseded"

    let forecastCycleEstablished = ofEnterpriseEvent "EV-D-010" "Fired when a new Forecast Cycle is established"

    let forecastProjectionProduced =
        ofEnterpriseEvent "EV-D-011" "Fired when statistical forecast lines and prediction intervals are produced"

    let forecastOverrideRecorded =
        ofEnterpriseEvent "EV-D-012" "Fired when a planner override is recorded on a forecast line"

    let forecastPublicationPublished =
        ofEnterpriseEvent "EV-D-013" "Fired when a Forecast Publication is published as authoritative"

    let championModelSelected =
        ofEnterpriseEvent "EV-D-014" "Fired when a champion forecasting model is selected for a cycle"

    let demandBehaviorChanged =
        ofEnterpriseEvent "EV-D-015" "Fired when demand behavior assessment transitions to a new state"

    let criticalDemandBehaviorDetected =
        ofEnterpriseEvent "EV-D-016" "Fired when demand behavior assessment transitions to Critical state"

    let planningClassificationChanged =
        ofEnterpriseEvent "EV-D-017" "Fired when an entity's planning classification is assigned or updated"

    let demandBehaviorClassificationChanged =
        ofEnterpriseEvent "EV-D-019" "Fired when a SKU-Location's demand behavior classification is assigned or updated"

    let planningPriorityChanged =
        ofEnterpriseEvent "EV-D-020" "Fired when an entity's planning priority level or score is assigned or updated"

    let forecastQualityAssessmentPublished =
        ofEnterpriseEvent "EV-D-021" "Fired when a Forecast Quality Assessment version is published as authoritative"

    let demandExceptionEvidenceEvaluated =
        ofEnterpriseEvent
            "EV-D-022"
            "Fired when demand exception evidence has been evaluated and prepared for publication to Core Exception Management"

    let demandExplanationEstablished =
        ofEnterpriseEvent "EV-D-024" "Fired when an immutable Demand Explanation is composed and persisted"

    let demandLearningEstablished =
        ofEnterpriseEvent "EV-D-025" "Fired when an immutable Demand Learning is derived and permanently recorded"

    let demandInterventionImpactPublished =
        ofEnterpriseEvent "EV-D-023" "Fired when an authoritative Demand Intervention Impact is published"

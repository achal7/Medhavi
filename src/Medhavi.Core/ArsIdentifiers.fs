module Medhavi.Core.ArsIdentifiers

open Medhavi.Foundation.Contracts.ArsArtifact

let domain = "C"

module Capabilities =
    let enterprisePictureManagement =
        ofCapability
            "CA-C-019"
            "Composes and publishes the unified planning scope view by aggregating Demand, Supply, and Inventory facts"

    let coreExceptionManagement =
        ofCapability "CA-C-020" "Central registry and lifecycle manager for enterprise exceptions"

    let manageItemTransitions =
        ofCapability
            "CA-C-021"
            "Governs the lifecycle of item succession relationships ensuring all planning capabilities operate from a single authoritative transition definition"

    let manageEnterpriseDemand =
        ofCapability "CA-C-022" "Records and governs the enterprise fact ledger of historical demand signals"

module Responsibilities =
    let composeEnterprisePictureVersion =
        ofResponsibility
            "CR-C-001"
            "Creates a Draft version of the Enterprise Picture referencing specific domain facts"

    let publishEnterprisePictureVersion =
        ofResponsibility "CR-C-002" "Evaluates materiality and publishes a Draft version when warranted"

    let processExceptionDetectionEvidence =
        ofResponsibility "CR-C-003" "Registers or updates an exception in the Core registry based on detection evidence"

    let processExceptionResolutionEvidence =
        ofResponsibility "CR-C-004" "Transitions an Active exception to Resolved with resolution evidence"

    let recognizeItemTransition =
        ofResponsibility "CR-C-005" "Recognizes a new item succession relationship from governed stewardship"

    let suspendItemTransition =
        ofResponsibility "CR-C-006" "Suspends an active item succession relationship temporarily"

    let reinstateItemTransition =
        ofResponsibility "CR-C-007" "Reinstates a suspended item succession relationship to active"

    let retireItemTransition = ofResponsibility "CR-C-008" "Retires an item succession relationship permanently"
    let recordDemand = ofResponsibility "CR-C-009" "Records a new demand fact from an accepted demand observation"
    let satisfyDemand = ofResponsibility "CR-C-010" "Transitions an Active demand fact to Satisfied"
    let cancelDemand = ofResponsibility "CR-C-011" "Transitions an Active demand fact to Cancelled"

module Decisions =
    let assessPictureMateriality =
        ofDecision "DE-C-001" "Determines whether a composed picture differs materially from the last published version"

    let evaluateExceptionEvidence =
        ofDecision
            "DE-C-002"
            "Determines whether exception detection evidence registers a new exception or updates an existing one"

    let evaluateExceptionResolution =
        ofDecision "DE-C-003" "Determines whether exception resolution evidence resolves an existing exception"

    let validateItemTransitionRecognition =
        ofDecision
            "DE-C-005"
            "Validates that an item transition meets all recognition criteria before becoming authoritative"

    let evaluateDemandRecording = ofDecision "DE-C-006" "Determines whether a demand fact can be recorded"
    let evaluateDemandSatisfaction = ofDecision "DE-C-007" "Determines whether a demand fact can be satisfied"
    let evaluateDemandCancellation = ofDecision "DE-C-008" "Determines whether a demand fact can be cancelled"

module Rules =
    let compositionRequiresReferences =
        ofRule
            "BR-C-001"
            "An Enterprise Picture version must contain at least one Demand, Supply, or Inventory reference"

    let demandReferencesMustBeUnique =
        ofRule "BR-C-002" "Demand references within a composition must not contain duplicates"

    let supplyReferencesMustBeUnique =
        ofRule "BR-C-003" "Supply references within a composition must not contain duplicates"

    let inventoryReferencesMustBeUnique =
        ofRule "BR-C-004" "Inventory references within a composition must not contain duplicates"

    let versionMustExist = ofRule "BR-C-005" "The specified version number must exist before publication"
    let onlyDraftVersionsCanBePublished = ofRule "BR-C-006" "Only Draft versions are eligible for publication"

    let exceptionBusinessIdentity =
        ofRule
            "BR-C-007"
            "Exception business identity is Constraint Reference + Affected Scope Type + Affected Scope Identifier"

    let constraintReferenceRequired = ofRule "BR-C-008" "Every exception must have a non-empty constraint reference"

    let affectedScopeIdentifierRequired =
        ofRule "BR-C-009" "Every exception must identify the specific affected scope instance"

    let evidenceReferenceRequired =
        ofRule "BR-C-010" "When policy demands evidence, a non-empty evidence reference must be provided"

    let exceptionMustExist = ofRule "BR-C-011" "An exception must already exist before resolution"
    let exceptionMustBeActive = ofRule "BR-C-012" "Only Active exceptions can be transitioned to Resolved"

    let transitionIdentityUnique =
        ofRule
            "BR-C-013"
            "Each Item Transition shall have a globally unique Transition Identifier assigned at recognition"

    let supersededItemValidity =
        ofRule "BR-C-014" "The Superseded Item must be in Active or Inactive state. A Retired item cannot be superseded"

    let supersedingItemValidity = ofRule "BR-C-015" "The Superseding Item must be in Active state at recognition"

    let singleActiveTransitionPerItem =
        ofRule "BR-C-016" "At most one Active transition exists per Superseded Item at any moment"

    let noSelfSupersession = ofRule "BR-C-017" "Superseded Item and Superseding Item must reference distinct items"

    let retiredTransitionNotReferenced =
        ofRule "BR-C-018" "A Retired Item Transition cannot be referenced by new planning activities"

    let demandMustNotExist = ofRule "BR-C-019" "A demand fact with the same identity must not already exist"
    let demandMustBeActiveForSatisfaction = ofRule "BR-C-020" "Only Active demand can transition to Satisfied"
    let demandMustBeActiveForCancellation = ofRule "BR-C-021" "Only Active demand can transition to Cancelled"
    let demandMustExistForSatisfaction = ofRule "BR-C-022" "Demand must exist before satisfaction"
    let demandMustExistForCancellation = ofRule "BR-C-023" "Demand must exist before cancellation"

module Policies =
    let enterprisePicturePolicy =
        ofPolicy
            "PO-C-001"
            "Governs composition cadence, materiality thresholds, and publication behavior of Enterprise Pictures"

    let exceptionManagementPolicy =
        ofPolicy
            "PO-C-002"
            "Governs exception classification, severity assessment, deduplication, and resolution criteria"

    let itemTransitionGovernance =
        ofPolicy "PO-C-003" "Governs item transition validation criteria, recognition rules, and lifecycle governance"

    let demandManagementPolicy =
        ofPolicy "PO-C-004" "Governs demand fact recording, satisfaction, and cancellation criteria"

module SemanticObjects =
    let enterprisePicture =
        ofSemanticObject
            "SE-C-021"
            "The aggregate root representing the unified, point-in-time view of a Planning Scope"

    let exceptionObject = ofSemanticObject "SE-C-019" "The enterprise fact representing a detected constraint violation"
    let demandObject = ofSemanticObject "SE-C-013" "The enterprise fact representing a single confirmed demand event"

// Ratified Core Spec events: EV-C-001 .. EV-C-005
module EnterpriseEvents =
    let pictureVersionComposed =
        ofEnterpriseEvent "EV-C-001" "Fired when a new Draft version of an Enterprise Picture is composed"

    let pictureVersionPublished = ofEnterpriseEvent "EV-C-002" "Fired when an Enterprise Picture version is published"
    let exceptionActivated = ofEnterpriseEvent "EV-C-003" "Fired when a new exception is registered"
    let exceptionUpdated = ofEnterpriseEvent "EV-C-004" "Fired when an existing exception receives updated evidence"
    let exceptionResolved = ofEnterpriseEvent "EV-C-005" "Fired when an exception is resolved"

    let itemTransitionRecognized =
        ofEnterpriseEvent "EV-C-006" "Fired when a new item transition is recognized and becomes authoritative"

    let itemTransitionSuspended =
        ofEnterpriseEvent "EV-C-007" "Fired when an active item transition is temporarily suspended"

    let itemTransitionReinstated =
        ofEnterpriseEvent "EV-C-008" "Fired when a suspended item transition is reinstated to active"

    let itemTransitionRetired = ofEnterpriseEvent "EV-C-009" "Fired when an item transition is permanently retired"

    let demandRecorded =
        ofEnterpriseEvent "EV-C-010" "Fired when a new demand fact is recorded in the enterprise ledger"

    let demandSatisfied = ofEnterpriseEvent "EV-C-011" "Fired when a demand fact transitions to Satisfied"
    let demandCancelled = ofEnterpriseEvent "EV-C-012" "Fired when a demand fact transitions to Cancelled"

module BusinessNotifications =
    open Medhavi.Contracts

    let enterprisePicturePublished =
        ofBusinessNotification
            "BN-C-001"
            "Published when a new Enterprise Picture version becomes the active published truth"

    let enterpriseExceptionActive =
        ofBusinessNotification "BN-C-002" "Published when an enterprise exception is registered or updated"

    let enterpriseExceptionResolved =
        ofBusinessNotification "BN-C-003" "Published when an enterprise exception is resolved"
    // Cross-domain evidence consumed by Core
    let demandExceptionEvidence =
        ofBusinessNotification CrossDomainEvents.demandExceptionEvidence "Demand-side exception detection evidence"

    let supplyExceptionEvidence =
        ofBusinessNotification CrossDomainEvents.supplyExceptionEvidence "Supply-side exception detection evidence"

    let inventoryExceptionEvidence =
        ofBusinessNotification CrossDomainEvents.inventoryExceptionEvidence "Inventory exception detection evidence"

    let itemTransitionRecognizedNotification =
        ofBusinessNotification
            "BN-C-004"
            "Published when an item transition is recognized. Consumed by Demand, Supply, Promise, Scenario Intelligence"

    let itemTransitionSuspendedNotification =
        ofBusinessNotification
            "BN-C-005"
            "Published when an item transition is suspended. Consumed by Demand, Supply, Promise, Scenario Intelligence"

    let itemTransitionReinstatedNotification =
        ofBusinessNotification
            "BN-C-006"
            "Published when an item transition is reinstated. Consumed by Demand, Supply, Promise, Scenario Intelligence"

    let itemTransitionRetiredNotification =
        ofBusinessNotification
            "BN-C-007"
            "Published when an item transition is retired. Consumed by Demand, Supply, Promise, Scenario Intelligence"

module BusinessAlgorithms =
    let evaluatePictureMateriality =
        ofBusinessAlgorithm "BA-C-001" "Deterministic assessment of the delta between a Draft and the Published picture"

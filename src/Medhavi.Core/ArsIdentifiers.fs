module Medhavi.Core.ArsIdentifiers

open Medhavi.Foundation.Contracts.ArsArtifact

let domain = "C"

module Capabilities =
    let enterprisePictureManagement =
        ofCapability "CA-C-019" "Composes and publishes the unified planning scope view by aggregating Demand, Supply, and Inventory facts"
    let coreExceptionManagement =
        ofCapability "CA-C-020" "Central registry and lifecycle manager for enterprise exceptions"

module Responsibilities =
    let composeEnterprisePictureVersion =
        ofResponsibility "CR-C-001" "Creates a Draft version of the Enterprise Picture referencing specific domain facts"
    let publishEnterprisePictureVersion =
        ofResponsibility "CR-C-002" "Evaluates materiality and publishes a Draft version when warranted"
    let processExceptionDetectionEvidence =
        ofResponsibility "CR-C-003" "Registers or updates an exception in the Core registry based on detection evidence"
    let processExceptionResolutionEvidence =
        ofResponsibility "CR-C-004" "Transitions an Active exception to Resolved with resolution evidence"

module Decisions =
    let assessPictureMateriality =
        ofDecision "DE-C-001" "Determines whether a composed picture differs materially from the last published version"
    let evaluateExceptionEvidence =
        ofDecision "DE-C-002" "Determines whether exception detection evidence registers a new exception or updates an existing one"
    let evaluateExceptionResolution =
        ofDecision "DE-C-003" "Determines whether exception resolution evidence resolves an existing exception"

module Rules =
    let compositionRequiresReferences = ofRule "BR-C-001" "An Enterprise Picture version must contain at least one Demand, Supply, or Inventory reference"
    let demandReferencesMustBeUnique  = ofRule "BR-C-002" "Demand references within a composition must not contain duplicates"
    let supplyReferencesMustBeUnique  = ofRule "BR-C-003" "Supply references within a composition must not contain duplicates"
    let inventoryReferencesMustBeUnique = ofRule "BR-C-004" "Inventory references within a composition must not contain duplicates"
    let versionMustExist              = ofRule "BR-C-005" "The specified version number must exist before publication"
    let onlyDraftVersionsCanBePublished = ofRule "BR-C-006" "Only Draft versions are eligible for publication"
    let exceptionBusinessIdentity     = ofRule "BR-C-007" "Exception business identity is Constraint Reference + Affected Scope Type + Affected Scope Identifier"
    let constraintReferenceRequired   = ofRule "BR-C-008" "Every exception must have a non-empty constraint reference"
    let affectedScopeIdentifierRequired = ofRule "BR-C-009" "Every exception must identify the specific affected scope instance"
    let evidenceReferenceRequired     = ofRule "BR-C-010" "When policy demands evidence, a non-empty evidence reference must be provided"
    let exceptionMustExist            = ofRule "BR-C-011" "An exception must already exist before resolution"
    let exceptionMustBeActive         = ofRule "BR-C-012" "Only Active exceptions can be transitioned to Resolved"

module Policies =
    let enterprisePicturePolicy =
        ofPolicy "PO-C-001" "Governs composition cadence, materiality thresholds, and publication behavior of Enterprise Pictures"
    let exceptionManagementPolicy =
        ofPolicy "PO-C-002" "Governs exception classification, severity assessment, deduplication, and resolution criteria"

module SemanticObjects =
    let enterprisePicture = ofSemanticObject "SE-C-021" "The aggregate root representing the unified, point-in-time view of a Planning Scope"
    let exceptionObject   = ofSemanticObject "SE-C-019" "The enterprise fact representing a detected constraint violation"

// Ratified Core Spec events: EV-C-001 .. EV-C-005
module EnterpriseEvents =
    let pictureVersionComposed = ofEnterpriseEvent "EV-C-001" "Fired when a new Draft version of an Enterprise Picture is composed"
    let pictureVersionPublished = ofEnterpriseEvent "EV-C-002" "Fired when an Enterprise Picture version is published"
    let exceptionActivated     = ofEnterpriseEvent "EV-C-003" "Fired when a new exception is registered"
    let exceptionUpdated       = ofEnterpriseEvent "EV-C-004" "Fired when an existing exception receives updated evidence"
    let exceptionResolved      = ofEnterpriseEvent "EV-C-005" "Fired when an exception is resolved"

module BusinessNotifications =
    open Medhavi.Contracts
    let enterprisePicturePublished =
        ofBusinessNotification "BN-C-001" "Published when a new Enterprise Picture version becomes the active published truth"
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

module BusinessAlgorithms =
    let evaluatePictureMateriality =
        ofBusinessAlgorithm "BA-C-001" "Deterministic assessment of the delta between a Draft and the Published picture"

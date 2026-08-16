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

module Responsibilities =
    let composeEnterprisePictureVersion =
        ofResponsibility
            "CR-C-019a"
            "Creates a Draft version of the Enterprise Picture referencing specific domain facts"

    let publishEnterprisePictureVersion =
        ofResponsibility
            "CR-C-019b"
            "Transitions a Draft version to Published, superseding any existing Published version"

    let registerException =
        ofResponsibility "CR-C-020a" "Registers a new exception in the Core registry based on evidence"

    let resolveException =
        ofResponsibility "CR-C-020b" "Transitions an Active exception to Resolved with resolution evidence"

module Decisions =
    let decideComposition =
        ofDecision
            "DE-C-019a"
            "Evaluates composition rules against the input to determine if a new Picture Version can be created"

    let decidePublication =
        ofDecision "DE-C-019b" "Evaluates publication rules to determine if a Draft Picture Version can be published"

    let decideRegistration =
        ofDecision "DE-C-020a" "Evaluates registration rules to determine if an exception can be registered"

    let decideResolution =
        ofDecision "DE-C-020b" "Evaluates resolution rules to determine if an Active exception can be resolved"

module Rules =
    let compositionRequiresReferences =
        ofRule
            "BR-C-1901"
            "An Enterprise Picture version must contain at least one Demand, Supply, or Inventory reference to be meaningful"

    let demandReferencesMustBeUnique =
        ofRule
            "BR-C-1902"
            "Demand references within a single Enterprise Picture composition must not contain duplicates"

    let supplyReferencesMustBeUnique =
        ofRule
            "BR-C-1903"
            "Supply references within a single Enterprise Picture composition must not contain duplicates"

    let inventoryReferencesMustBeUnique =
        ofRule
            "BR-C-1904"
            "Inventory references within a single Enterprise Picture composition must not contain duplicates"

    let versionMustExist =
        ofRule
            "BR-C-1905"
            "The specified version number must already exist in the Enterprise Picture aggregate before it can be published"

    let onlyDraftVersionsCanBePublished =
        ofRule
            "BR-C-1906"
            "Only Enterprise Picture versions currently in the Draft lifecycle state are eligible for publication"

    let exceptionMustNotAlreadyExist =
        ofRule "BR-C-2001" "An exception must not already exist unless policy allows duplicates"

    let constraintReferenceRequired =
        ofRule "BR-C-2002" "Every registered exception must have a non-empty constraint reference"

    let affectedScopeIdentifierRequired =
        ofRule "BR-C-2003" "Every registered exception must identify the specific affected scope instance"

    let evidenceReferenceRequired =
        ofRule "BR-C-2004" "When policy demands evidence, a non-empty evidence reference must be provided"

    let exceptionMustExist =
        ofRule "BR-C-2005" "An exception must already exist before a resolution command can be executed"

    let exceptionMustBeActive =
        ofRule "BR-C-2006" "Only exceptions in the Active lifecycle state can be transitioned to Resolved"

module Policies =
    let enterprisePicturePolicy =
        ofPolicy
            "PO-C-019"
            "Governs the composition and publication behavior of Enterprise Pictures, including version retention and auto-supersede rules"

    let exceptionManagementPolicy =
        ofPolicy "PO-C-020" "Governs exception registration thresholds and evidence requirements"

module SemanticObjects =
    let enterprisePicture =
        ofSemanticObject
            "SE-C-021"
            "The aggregate root representing the unified, point-in-time view of a Planning Scope"

    let exceptionObject = ofSemanticObject "SE-C-019" "The enterprise fact representing a detected constraint violation"

module EnterpriseEvents =
    let pictureVersionComposed =
        ofEnterpriseEvent "EV-C-019a" "Fired when a new Draft version of an Enterprise Picture is successfully composed"

    let pictureVersionPublished =
        ofEnterpriseEvent
            "EV-C-019b"
            "Fired when an Enterprise Picture version is successfully transitioned to Published"

    let pictureVersionSuperseded =
        ofEnterpriseEvent
            "EV-C-019c"
            "Fired when a previously Published Enterprise Picture version is superseded by a new publication"

    let exceptionRegistered = ofEnterpriseEvent "EV-C-020a" "Fired when a new exception is successfully registered"

    let exceptionResolved =
        ofEnterpriseEvent "EV-C-020b" "Fired when an Active exception is successfully transitioned to Resolved"

module BusinessNotifications =
    open Medhavi.Contracts

    let enterprisePicturePublished =
        ofBusinessNotification
            "BN-C-019a"
            "Published to other domains when a new Enterprise Picture version becomes the active published truth"
    // Core-published notifications
    let exceptionRegisteredNotification =
        ofBusinessNotification
            "BN-C-020a"
            "Published to notify downstream systems that a new enterprise exception has been registered"

    let exceptionResolvedNotification =
        ofBusinessNotification
            "BN-C-020b"
            "Published to notify downstream systems that an enterprise exception has been resolved"

    // Cross-domain notifications consumed by Core (referenced from Contracts)
    let demandExceptionEvidence =
        ofBusinessNotification
            CrossDomainEvents.demandExceptionEvidence
            "Evidence of demand-side exceptions detected by the Demand Intelligence domain"

    let supplyExceptionEvidence =
        ofBusinessNotification
            CrossDomainEvents.supplyExceptionEvidence
            "Evidence of supply-side exceptions detected by the Supply Intelligence domain"

    let inventoryExceptionEvidence =
        ofBusinessNotification
            CrossDomainEvents.inventoryExceptionEvidence
            "Evidence of inventory exceptions detected by the Inventory Intelligence domain"

    let demandUnderstandingPublished =
        ofBusinessNotification
            CrossDomainEvents.demandUnderstandingPublished
            "Demand Understanding version published - signals material change for picture recomposition"

    let supplyUnderstandingPublished =
        ofBusinessNotification
            CrossDomainEvents.supplyUnderstandingPublished
            "Supply Understanding version published - signals material change for picture recomposition"

    let exceptionSlaWarning =
        ofBusinessNotification "BN-C-020c" "Published when an exception approaches its SLA deadline"

    let exceptionSlaEscalation =
        ofBusinessNotification "BN-C-020d" "Published when an exception breaches its SLA deadline"

    let inventorySnapshotPublished =
        ofBusinessNotification
            CrossDomainEvents.inventorySnapshotPublished
            "Inventory Snapshot version published - signals material change for picture recomposition"

module BusinessAlgorithms =
    let calculateNextVersionNumber =
        ofBusinessAlgorithm
            "BA-C-019"
            "Deterministic algorithm to calculate the next sequential version number for an Enterprise Picture"

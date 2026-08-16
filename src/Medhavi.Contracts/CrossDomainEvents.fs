/// Cross-domain event type constants for envelope routing
/// These strings are used in Envelope.EventType for inter-domain communication
module Medhavi.Contracts.CrossDomainEvents

// Demand domain notifications consumed by Core
let demandExceptionEvidence = "BN-D-022"
let demandUnderstandingPublished = "BN-D-010"
let demandObservationQuarantined = "BN-D-002"
let demandObservationRejected = "BN-D-003"

// Supply domain notifications consumed by Core
let supplyExceptionEvidence = "BN-S-022"
let supplyUnderstandingPublished = "BN-S-010"

// Inventory domain notifications consumed by Core
let inventoryExceptionEvidence = "BN-I-022"
let inventorySnapshotPublished = "BN-I-010"

// Core domain notifications published to other domains
let exceptionRegistered = "BN-C-020a"
let exceptionResolved = "BN-C-020b"
let enterprisePicturePublished = "BN-C-019a"

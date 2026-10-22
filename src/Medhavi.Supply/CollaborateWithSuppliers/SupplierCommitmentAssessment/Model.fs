/// SE-S-051 — Supplier Commitment Assessment Domain Model
/// Traces to: Supply Intelligence Specification §4.3.1 (SE-S-051, AB-S-041, EV-S-041)
module Medhavi.Supply.CollaborateWithSuppliers.SupplierCommitmentAssessment.Model

open Medhavi.SemanticModel
open Medhavi.Supply

type AssessmentLifecycleState =
    | Active
    | Archived

type ExposureLevel =
    | Nominal
    | Low
    | Medium
    | High
    | Critical

type DimensionalExposure =
    {
        DimensionName: string
        Level: ExposureLevel
        QuantityAtRisk: decimal
        ValuationAtRisk: decimal
        ReasonCodes: string list
        ThreatenedObjectives: string list
    }

type CommitmentExposureAssessment =
    {
        SupplierId: SupplierId
        EvaluationTimestamp: Timestamp
        OverallConfidence: string
        Exposures: DimensionalExposure list
        ReasonCodes: string list
    }

type SupplierCommitmentAssessmentChangeEvent =
    {
        SequenceNumber: int
        ChangeTimestamp: Timestamp
        PreviousHealthState: SupplierCollaborationHealthState option
        NewHealthState: SupplierCollaborationHealthState
        ChangeSummary: string
    }

type SupplierCommitmentAssessment =
    {
        SupplierId: SupplierId
        State: AssessmentLifecycleState
        AssessmentTimestamp: Timestamp
        OverallConfidence: string
        Exposures: DimensionalExposure list
        OnTimeDeliveryRate: decimal
        QuantityFulfillmentRate: decimal
        AverageAcknowledgementHours: decimal
        AverageCommitmentHours: decimal
        ActiveRiskSignals: string list
        AuthoritativeHealthState: SupplierCollaborationHealthState
        ChangeHistory: SupplierCommitmentAssessmentChangeEvent list
    }

type UpdateSupplierCommitmentAssessmentCmd =
    {
        SupplierId: SupplierId
        AssessmentTimestamp: Timestamp
    }

type SupplierCommitmentAssessmentEvent =
    | SupplierCommitmentAssessmentChanged of
        assessment: SupplierCommitmentAssessment *
        changeEventOpt: SupplierCommitmentAssessmentChangeEvent option

let evolve
    (stateOpt: SupplierCommitmentAssessment option)
    (event: SupplierCommitmentAssessmentEvent)
    : SupplierCommitmentAssessment option =
    match event with
    | SupplierCommitmentAssessmentChanged(assessment, _) -> Some assessment

let replay (events: SupplierCommitmentAssessmentEvent seq) : SupplierCommitmentAssessment option =
    Seq.fold evolve None events

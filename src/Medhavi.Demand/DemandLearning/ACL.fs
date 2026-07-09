module Medhavi.Demand.DemandLearning.ACL

open Medhavi.Common.Validation
open Medhavi.Contracts.Demand.DemandLearning
open Medhavi.SharedKernel
open Medhavi.SharedKernel.Failure
open Medhavi.Demand
open Medhavi.Demand.DemandLearning.Model

let private notEmpty field =
    validate (fun s -> not (System.String.IsNullOrWhiteSpace s)) (DomainError.validation(field, $"{field} cannot be empty"))

let toRecordCmd (req: RecordDemandLearningReq) : Validation<RecordDemandLearningCmd, DomainError> =
    let make learningId scopeIdOpt learningType learningStatement evidenceStrength sourceRef =
        { LearningId          = learningId
          PlanningScopeId     = scopeIdOpt
          LearningType        = learningType
          LearningStatement   = learningStatement
          SupportingEvidence  = req.SupportingEvidence
          EvidenceStrength    = evidenceStrength
          SourceAnalysisRef   = sourceRef
          BusinessTime        = Timestamp.create req.BusinessTime
          TransactionTime     = Timestamp.now }

    let learningIdVal = DemandLearningId.create req.LearningId |> fromResult
    let scopeIdVal =
        if System.String.IsNullOrWhiteSpace req.PlanningScopeId then
            Valid None
        else
            PlanningScopeId.fromString req.PlanningScopeId |> Result.map Some |> fromResult

    make <!> learningIdVal
    <*> scopeIdVal
    <*> notEmpty "LearningType" req.LearningType
    <*> notEmpty "LearningStatement" req.LearningStatement
    <*> notEmpty "EvidenceStrength" req.EvidenceStrength
    <*> notEmpty "SourceAnalysisRef" req.SourceAnalysisRef

module Medhavi.Demand.DemandLearning.Decisions

open Medhavi.Common
open Medhavi.SharedKernel
open Medhavi.SharedKernel.Contracts.Aggregate
open Medhavi.SharedKernel.Contracts.DecisionTrace
open Medhavi.SharedKernel.Failure
open Medhavi.Demand.DemandLearning.Model
open Medhavi.Demand.DemandLearning.Rules

let record
    (cmd: RecordDemandLearningCmd)
    (stateOpt: DemandLearning option)
    : Result<DemandLearningEvent list, DomainError> =
    result {
        let! _ = learningImmutable stateOpt
        let! _ = evidenceRequired cmd.SupportingEvidence

        let learning: DemandLearning =
            { Id = cmd.LearningId
              PlanningScopeId = cmd.PlanningScopeId
              LearningType = cmd.LearningType
              LearningStatement = cmd.LearningStatement
              SupportingEvidence = cmd.SupportingEvidence
              EvidenceStrength = cmd.EvidenceStrength
              SourceAnalysisRef = cmd.SourceAnalysisRef
              BusinessTime = cmd.BusinessTime
              TransactionTime = cmd.TransactionTime }

        return [ DemandLearningRecorded learning ]
    }

let decide
    (cmd: DemandLearningCommand)
    (stateOpt: DemandLearning option)
    : Result<Decision<DemandLearning, DemandLearningEvent>, DomainError> =
    match cmd, stateOpt with
    | Record cmd, None ->
        record cmd stateOpt
        |> Result.map(fun events ->
            let derivedState = events |> List.fold (fun acc e -> evolve e acc) stateOpt

            let rationale =
                match derivedState with
                | Some learning ->
                    { Summary = $"Demand learning recorded: {learning.LearningType} — {learning.LearningStatement}"
                      Evidence = learning.SupportingEvidence
                      Alternatives = [] }
                | None ->
                    { Summary = "Demand learning recorded."
                      Evidence = []
                      Alternatives = [] }

            let trace =
                { DecisionId = ArsIdentifiers.Demand.Decisions.recordDemandLearning
                  CapabilityId = ArsIdentifiers.Demand.Capabilities.learnFromDemand
                  RulesEvaluated =
                    [ (ArsIdentifiers.Demand.Rules.learningEvidenceRequired, 1)
                      (ArsIdentifiers.Demand.Rules.learningImmutable, 1) ]
                  PolicyId = Some ArsIdentifiers.Demand.Policies.learningAnalysisPolicy
                  PolicyVersion = Some 1
                  SemanticObjectIds = [ ArsIdentifiers.Demand.SemanticObjects.demandLearning ]
                  Rationale = rationale }

            buildDecision evolve stateOpt events (Some trace))
    | _ -> Error(DomainError.validation "Command invalid for current state")

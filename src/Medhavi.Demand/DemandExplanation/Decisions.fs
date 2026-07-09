module Medhavi.Demand.DemandExplanation.Decisions

open Medhavi.Common
open Medhavi.SharedKernel
open Medhavi.SharedKernel.Contracts.Aggregate
open Medhavi.SharedKernel.Contracts.DecisionTrace
open Medhavi.SharedKernel.Failure
open Medhavi.Demand.DemandExplanation.Model
open Medhavi.Demand.DemandExplanation.Rules

let record (cmd: RecordDemandExplanationCmd) (stateOpt: DemandExplanation option) : Result<DemandExplanationEvent list, DomainError> =
    result {
        let! _ = explanationImmutable stateOpt
        let! _ = sourceArtifactVersionsPresent cmd.SourceArtifactRefs
        let! _ = graphDeterministic cmd.StructuredReasoningGraph

        let explanation: DemandExplanation =
            { Id = cmd.ExplanationId
              ExplainedArtifactType = cmd.ExplainedArtifactType
              ExplainedArtifactId = cmd.ExplainedArtifactId
              StructuredReasoningGraph = cmd.StructuredReasoningGraph
              NaturalLanguageExplanation = cmd.NaturalLanguageExplanation
              SourceArtifactRefs = cmd.SourceArtifactRefs
              ExplanationGenerationTimestamp = cmd.TransactionTime
              TemplateVersionRef = cmd.TemplateVersionRef
              BusinessTime = cmd.BusinessTime
              TransactionTime = cmd.TransactionTime }

        return [ DemandExplanationRecorded explanation ]
    }

let decide
    (cmd: DemandExplanationCommand)
    (stateOpt: DemandExplanation option)
    : Result<Decision<DemandExplanation, DemandExplanationEvent>, DomainError> =
    match cmd, stateOpt with
    | RecordDemandExplanation cmd, None ->
        record cmd stateOpt
        |> Result.map(fun events ->
            let derivedState = events |> List.fold (fun acc e -> evolve e acc) stateOpt

            let rationale =
                match derivedState with
                | Some exp ->
                    { Summary = $"Explanation recorded for {exp.ExplainedArtifactType} {exp.ExplainedArtifactId}."
                      Evidence =
                        [ $"Nodes: {exp.StructuredReasoningGraph.Nodes.Length}"
                          $"Edges: {exp.StructuredReasoningGraph.Edges.Length}" ]
                      Alternatives = [] }
                | None ->
                    { Summary = "Explanation recorded."
                      Evidence = []
                      Alternatives = [] }

            let trace =
                { DecisionId = ArsIdentifiers.Demand.Decisions.recordDemandExplanation
                  CapabilityId = ArsIdentifiers.Demand.Capabilities.explainDemand
                  RulesEvaluated =
                    [ (ArsIdentifiers.Demand.Rules.explanationImmutable, 1)
                      (ArsIdentifiers.Demand.Rules.structuredReasoningVersions, 1)
                      (ArsIdentifiers.Demand.Rules.explanationDeterministic, 1) ]
                  PolicyId = Some ArsIdentifiers.Demand.Policies.explanationTemplateGoverned
                  PolicyVersion = Some 1
                  SemanticObjectIds = [ ArsIdentifiers.Demand.SemanticObjects.demandExplanation ]
                  Rationale = rationale }

            buildDecision evolve stateOpt events (Some trace))
    | _ -> Error(DomainError.validation "Command invalid for current state")

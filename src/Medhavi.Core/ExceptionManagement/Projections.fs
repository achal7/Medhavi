/// CA-C-020 Projections
module Medhavi.Core.ExceptionManagement.Projections

open Medhavi.SemanticModel
open Medhavi.SemanticModel.Identities
open Medhavi.Contracts.Core
open Medhavi.Core.ExceptionManagement.Model

/// Builds the read-model DTO. Severity is sourced from events (not aggregate state).
let mapToDto (aggregate: CoreException) (severity: VocabularyEntryId option) : Exception.ExceptionDto =
    { ExceptionId = exceptionIdValue aggregate.ExceptionIdentifier
      ConstraintReference = aggregate.ConstraintReference
      Classification = vocabularyEntryIdValue aggregate.Classification
      AffectedScopeType = vocabularyEntryIdValue aggregate.AffectedScopeType
      AffectedScopeIdentifier = aggregate.AffectedScopeIdentifier
      EvidenceReference = aggregate.EvidenceReference
      Severity = severity |> Option.map vocabularyEntryIdValue
      RegistrationTime = Timestamp.value(Timestamp.now())
      ResolutionTime = None
      ResolutionEvidence = None
      LifecycleState = aggregate.LifecycleState.ToString() }

/// Projection state carries severity alongside the DTO (severity lives in the read-model).
type State = Map<ExceptionId, Exception.ExceptionDto>
let initial: State = Map.empty

let apply (state: State) (event: ExceptionEvent) : State =
    match event with
    | ExceptionActivated(exceptionObject, severity) ->
        let dto = mapToDto exceptionObject severity
        state |> Map.add exceptionObject.ExceptionIdentifier dto

    | ExceptionUpdated(exceptionId, newEvidence, newSeverity, _) ->
        state
        |> Map.change
            exceptionId
            (Option.map(fun existing ->
                { existing with
                    EvidenceReference = newEvidence |> Option.orElse existing.EvidenceReference
                    Severity = newSeverity |> Option.map vocabularyEntryIdValue |> Option.orElse existing.Severity }))

    | ExceptionResolved(exceptionId, resolutionTime, resolutionEvidence) ->
        state
        |> Map.change
            exceptionId
            (Option.map(fun existing ->
                { existing with
                    LifecycleState = ExceptionLifecycleState.Resolved.ToString()
                    ResolutionTime = Some(Timestamp.value resolutionTime)
                    ResolutionEvidence = Some resolutionEvidence }))

let seedFromAggregates (aggregates: CoreException list) : State =
    aggregates
    |> List.fold
        (fun state agg ->
            let dto = mapToDto agg None
            Map.add agg.ExceptionIdentifier dto state)
        initial

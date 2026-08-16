/// CA-C-020 Exception Management Projections
module Medhavi.Core.ExceptionManagement.Projections

open Medhavi.SemanticModel
open Medhavi.Contracts.Core
open Medhavi.Core.ExceptionManagement.Model

let mapToDto (aggregate: CoreException) : Exception.ExceptionDto =
    { ExceptionId = Identities.exceptionIdValue aggregate.ExceptionIdentifier
      ConstraintReference = aggregate.ConstraintReference
      Classification = Identities.vocabularyEntryIdValue aggregate.Classification
      AffectedScopeType = Identities.vocabularyEntryIdValue aggregate.AffectedScopeType
      AffectedScopeIdentifier = aggregate.AffectedScopeIdentifier
      EvidenceReference = aggregate.EvidenceReference
      Severity = aggregate.Severity |> Option.map Identities.vocabularyEntryIdValue
      RegistrationTime = Timestamp.value <| Timestamp.now()
      ResolutionTime = None
      ResolutionEvidence = None
      LifecycleState = aggregate.LifecycleState.ToString() }

type State = Map<ExceptionId, Exception.ExceptionDto>
let initial: State = Map.empty

/// Pure projection fold
let apply (state: State) (event: ExceptionEvent) : State =
    match event with
    | ExceptionRegistered exceptionObject ->
        state |> Map.add exceptionObject.ExceptionIdentifier (mapToDto exceptionObject)
    | ExceptionResolved(exceptionId, resolutionTime, resolutionEvidence) ->
        state
        |> Map.change
            exceptionId
            (Option.map(fun existing ->
                { existing with
                    LifecycleState = ExceptionLifecycleState.Resolved.ToString()
                    ResolutionTime = Some(Timestamp.value resolutionTime)
                    ResolutionEvidence = Some resolutionEvidence }))

/// Seed the projection from existing aggregates
let seedFromAggregates (aggregates: CoreException list) : Map<ExceptionId, Exception.ExceptionDto> =
    aggregates
    |> List.fold
        (fun state agg ->
            let dto = mapToDto agg
            Map.add agg.ExceptionIdentifier dto state)
        initial

/// CA-C-020 Exception Management Model
module Medhavi.Core.ExceptionManagement.Model

open Medhavi.SemanticModel

type CoreException = Medhavi.SemanticModel.Exception

/// AB-C-003 input. ExceptionId is DERIVED by the ACL from business identity (BR-C-007).
/// Severity is evidence metadata, NOT aggregate state (ESM SE-C-019).
type ProcessExceptionEvidenceCmd =
    { ExceptionId: ExceptionId
      ConstraintReference: string
      Classification: VocabularyEntryId
      AffectedScopeType: VocabularyEntryId
      AffectedScopeIdentifier: string
      EvidenceReference: string option
      Severity: VocabularyEntryId option
      EvidenceTime: Timestamp }

/// AB-C-004 input
type ResolveExceptionCmd =
    { ExceptionId: ExceptionId
      ResolutionTime: Timestamp
      ResolutionEvidence: string }

type ExceptionCmd =
    | ProcessEvidence of ProcessExceptionEvidenceCmd
    | Resolve of ResolveExceptionCmd

/// EV-C-003 (registered), EV-C-004 (updated), EV-C-005 (resolved).
/// Severity rides in the event payload for projection; never in aggregate state.
type ExceptionEvent =
    | ExceptionActivated of CoreException * Severity: VocabularyEntryId option
    | ExceptionUpdated of ExceptionId * NewEvidenceReference: string option * NewSeverity: VocabularyEntryId option * EvidenceTime: Timestamp
    | ExceptionResolved of ExceptionId * ResolutionTime: Timestamp * ResolutionEvidence: string

/// Pure evolution. Severity is NOT mutated into CoreException (ESM-compliant).
let evolve (state: CoreException option) (event: ExceptionEvent) : CoreException option =
    match event with
    | ExceptionActivated (exceptionObject, _) -> Some exceptionObject
    | ExceptionUpdated (exceptionId, newEvidence, _, _) ->
        state |> Option.map (fun e ->
            if e.ExceptionIdentifier = exceptionId then
                { e with EvidenceReference = newEvidence }
            else e)
    | ExceptionResolved (exceptionId, _, _) ->
        state |> Option.map (fun e ->
            if e.ExceptionIdentifier = exceptionId then
                { e with LifecycleState = ExceptionLifecycleState.Resolved }
            else e)

let replay (events: ExceptionEvent seq) : CoreException option = Seq.fold evolve None events

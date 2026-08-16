/// CA-C-020 Exception Management Model
module Medhavi.Core.ExceptionManagement.Model

open Medhavi.SemanticModel

type CoreException = Medhavi.SemanticModel.Exception

/// AB-C-020a: Register a new exception
type RegisterExceptionCmd =
    { ExceptionId: ExceptionId
      ConstraintReference: string
      Classification: VocabularyEntryId
      AffectedScopeType: VocabularyEntryId
      AffectedScopeIdentifier: string
      EvidenceReference: string
      Severity: VocabularyEntryId option
      RegistrationTime: Timestamp }

/// AB-C-020b: Resolve an existing exception
type ResolveExceptionCmd =
    { ExceptionId: ExceptionId
      ResolutionTime: Timestamp
      ResolutionEvidence: string }

type ExceptionCmd =
    | Register of RegisterExceptionCmd
    | Resolve of ResolveExceptionCmd

type ExceptionEvent =
    | ExceptionRegistered of CoreException
    | ExceptionResolved of ExceptionId * ResolutionTime: Timestamp * ResolutionEvidence: string

/// Layer E: Pure state evolution (Catamorphism)
let evolve (state: CoreException option) (event: ExceptionEvent) : CoreException option =
    match event with
    | ExceptionRegistered exceptionObject -> Some exceptionObject
    | ExceptionResolved(exceptionId, _, _) ->
        state
        |> Option.map(fun e ->
            if e.ExceptionIdentifier = exceptionId then
                { e with
                    LifecycleState = ExceptionLifecycleState.Resolved }
            else
                e)

let replay (events: ExceptionEvent seq) : CoreException option = Seq.fold evolve None events

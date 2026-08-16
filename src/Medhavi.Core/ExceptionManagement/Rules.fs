/// CA-C-020 Exception Management Rules
module Medhavi.Core.ExceptionManagement.Rules

open System
open Medhavi.SemanticModel
open Medhavi.Foundation.Contracts
open Medhavi.Core.ArsIdentifiers
open Model
open Policies

type ProcessInput =
    { Cmd: ProcessExceptionEvidenceCmd
      CurrentState: CoreException option
      CurrentSeverity: VocabularyEntryId option
      Policy: ExceptionManagementPolicy }

type ResolveInput =
    { Cmd: ResolveExceptionCmd
      CurrentState: CoreException option }

let constraintReferenceRequired: Rule<ProcessInput> =
    Rule.create
        Rules.constraintReferenceRequired.Id
        Rules.constraintReferenceRequired.Explanation
        (fun input -> not(String.IsNullOrWhiteSpace input.Cmd.ConstraintReference))
        (fun input -> sprintf "ConstraintReference: '%s'" input.Cmd.ConstraintReference)

let affectedScopeIdentifierRequired: Rule<ProcessInput> =
    Rule.create
        Rules.affectedScopeIdentifierRequired.Id
        Rules.affectedScopeIdentifierRequired.Explanation
        (fun input -> not(String.IsNullOrWhiteSpace input.Cmd.AffectedScopeIdentifier))
        (fun input -> sprintf "AffectedScope: '%s'" input.Cmd.AffectedScopeIdentifier)

let evidenceReferenceRequired: Rule<ProcessInput> =
    Rule.create
        Rules.evidenceReferenceRequired.Id
        Rules.evidenceReferenceRequired.Explanation
        (fun input ->
            not input.Policy.RequireEvidenceReference
            || (input.Cmd.EvidenceReference |> Option.exists (not << String.IsNullOrWhiteSpace)))
        (fun input -> sprintf "EvidenceReference: '%A'" input.Cmd.EvidenceReference)

let exceptionMustExist: Rule<ResolveInput> =
    Rule.create
        Rules.exceptionMustExist.Id
        Rules.exceptionMustExist.Explanation
        (fun input -> input.CurrentState.IsSome)
        (fun input -> sprintf "ExceptionId: %A" input.Cmd.ExceptionId)

let exceptionMustBeActive: Rule<ResolveInput> =
    Rule.create
        Rules.exceptionMustBeActive.Id
        Rules.exceptionMustBeActive.Explanation
        (fun input ->
            input.CurrentState
            |> Option.map(fun e -> e.LifecycleState = ExceptionLifecycleState.Active)
            |> Option.defaultValue false)
        (fun input -> sprintf "ExceptionId: %A" input.Cmd.ExceptionId)

let registrationRules: Rule<ProcessInput> list =
    [ constraintReferenceRequired
      affectedScopeIdentifierRequired
      evidenceReferenceRequired ]

let resolutionRules: Rule<ResolveInput> list = [ exceptionMustExist; exceptionMustBeActive ]

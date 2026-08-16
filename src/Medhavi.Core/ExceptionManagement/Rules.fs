/// CA-C-020 Exception Management Rules
module Medhavi.Core.ExceptionManagement.Rules

open System
open Medhavi.SemanticModel
open Medhavi.Foundation.Contracts
open Medhavi.Core.ArsIdentifiers
open Model
open Policies

type RegisterInput =
    { Cmd: RegisterExceptionCmd
      CurrentState: CoreException option
      Policy: ExceptionManagementPolicy }

type ResolveInput =
    { Cmd: ResolveExceptionCmd
      CurrentState: CoreException option }

let exceptionMustNotAlreadyExist: Rule<RegisterInput> =
    Rule.create
        Rules.exceptionMustNotAlreadyExist.Id
        Rules.exceptionMustNotAlreadyExist.Explanation
        (fun input -> input.Policy.AllowDuplicateRegistration || input.CurrentState.IsNone)
        (fun input -> sprintf "Exists: %b" input.CurrentState.IsSome)

let constraintReferenceRequired: Rule<RegisterInput> =
    Rule.create
        Rules.constraintReferenceRequired.Id
        Rules.constraintReferenceRequired.Explanation
        (fun input -> not(String.IsNullOrWhiteSpace input.Cmd.ConstraintReference))
        (fun input -> sprintf "ConstraintReference: '%s'" input.Cmd.ConstraintReference)

let affectedScopeIdentifierRequired: Rule<RegisterInput> =
    Rule.create
        Rules.affectedScopeIdentifierRequired.Id
        Rules.affectedScopeIdentifierRequired.Explanation
        (fun input -> not(String.IsNullOrWhiteSpace input.Cmd.AffectedScopeIdentifier))
        (fun input -> sprintf "AffectedScope: '%s'" input.Cmd.AffectedScopeIdentifier)

let evidenceReferenceRequired: Rule<RegisterInput> =
    Rule.create
        Rules.evidenceReferenceRequired.Id
        Rules.evidenceReferenceRequired.Explanation
        (fun input ->
            not input.Policy.RequireEvidenceReference || not(String.IsNullOrWhiteSpace input.Cmd.EvidenceReference))
        (fun input -> sprintf "EvidenceReference: '%s'" input.Cmd.EvidenceReference)

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

let registrationRules: Rule<RegisterInput> list =
    [ exceptionMustNotAlreadyExist
      constraintReferenceRequired
      affectedScopeIdentifierRequired
      evidenceReferenceRequired ]

let resolutionRules: Rule<ResolveInput> list = [ exceptionMustExist; exceptionMustBeActive ]

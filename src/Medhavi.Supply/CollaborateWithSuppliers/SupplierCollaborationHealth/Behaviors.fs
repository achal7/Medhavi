/// AB-S-042 — Publish Supplier Collaboration Health Assessment Aggregate Behavior
/// Traces to: Supply Intelligence Specification §4.3.1 (AB-S-042, EV-S-042)
module Medhavi.Supply.CollaborateWithSuppliers.SupplierCollaborationHealth.Behaviors

open Medhavi.Common
open Medhavi.Foundation.Contracts
open Medhavi.Foundation.Contracts.Decision
open Medhavi.SemanticModel
open Medhavi.Supply
open Model
open Policies
open Rules
open Decisions

type HealthPublicationOutcome =
    | Published of version: int * supersededOpt: int option

type ScopeAssessmentsProvider = PlanningScopeId -> SupplierCommitmentAssessmentFact list

/// AB-S-042: Execute Publish Supplier Collaboration Health Assessment
let publish
    (assessmentsProvider: ScopeAssessmentsProvider)
    (policy: SupplierCollaborationHealthPolicy)
    : Decide<SupplierCollaborationHealthAssessment, PublishSupplierCollaborationHealthCmd, SupplierCollaborationHealthEvent> =
    fun (cmd: PublishSupplierCollaborationHealthCmd) (stateOpt: SupplierCollaborationHealthAssessment option) ->
        result {
            do! validateIdentity cmd.AssessmentScopeId cmd.EvaluationPeriodId

            let nextVersion = getNextVersionNumber stateOpt
            do! validateVersionNotPublished stateOpt nextVersion

            let assessments = assessmentsProvider cmd.AssessmentScopeId

            let updatedAssessment, newVersion, supersededOpt =
                classifyScopeHealth
                    cmd.AssessmentScopeId
                    cmd.EvaluationPeriodId
                    assessments
                    stateOpt
                    cmd.PublicationTimestamp
                    policy

            let events = [ SupplierCollaborationHealthAssessmentPublished(updatedAssessment, newVersion.VersionNumber, supersededOpt) ]
            let outcome = Published(newVersion.VersionNumber, supersededOpt)
            let traceOutcome = { Outcome = outcome; Evaluations = [] }

            return
                buildDecisionWithTrace
                    Model.evolve
                    stateOpt
                    events
                    ArsIdentifiers.Decisions.classifyCollaborationHealth.Id
                    []
                    ArsIdentifiers.Capabilities.collaborateWithSuppliers.Id
                    traceOutcome
                    (Some ArsIdentifiers.Policies.supplierCollaborationPolicy.Id)
                    (Some 1)
                    [ ArsIdentifiers.SemanticObjects.supplierCollaborationHealthAssessment.Id ]
                    (Some $"Supplier collaboration health assessment published for scope {PlanningScopeId.value cmd.AssessmentScopeId}: Version {newVersion.VersionNumber}")
        }

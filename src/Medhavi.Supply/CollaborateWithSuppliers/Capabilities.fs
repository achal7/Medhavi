/// High-level Capability Composition and Notification Dispatch for Collaborate with Suppliers
/// Traces to: Supply Intelligence Specification §4.3.2 (CA-S-005, BW-S-050, BW-S-051, BN-S-040, BN-S-041, BN-S-042)
module Medhavi.Supply.CollaborateWithSuppliers.Capabilities

open System.Threading.Tasks
open Medhavi.Common
open Medhavi.Contracts
open Medhavi.Contracts.Supply
open Medhavi.Supply.AclHelpers

type CollaborateWithSuppliersApisDependencies =
    {
        CommitmentAggregate: SupplierCommitment.Capabilities.SupplierCommitmentAggregateApi
        AssessmentAggregate: SupplierCommitmentAssessment.Capabilities.SupplierCommitmentAssessmentAggregateApi
        HealthAggregate: SupplierCollaborationHealth.Capabilities.SupplierCollaborationHealthAggregateApi
    }

let create (deps: CollaborateWithSuppliersApisDependencies) : CollaborateWithSuppliersApis =

    let commitmentApi: SupplierCommitmentApi =
        {
            RecordEnterpriseRequest =
                fun req ->
                    deps.CommitmentAggregate.RecordRequest req
                    |> TaskResult.map SupplierCommitment.Projections.mapCommitmentToDto
                    |> TaskResult.mapError mapAppErrorToApiError

            RecordSupplierResponse =
                fun req ->
                    deps.CommitmentAggregate.RecordResponse req
                    |> TaskResult.map SupplierCommitment.Projections.mapCommitmentToDto
                    |> TaskResult.mapError mapAppErrorToApiError
        }

    let assessmentApi: SupplierCommitmentAssessmentApi =
        {
            Update =
                fun req ->
                    deps.AssessmentAggregate.Update req
                    |> TaskResult.map SupplierCommitmentAssessment.Projections.mapAssessmentToDto
                    |> TaskResult.mapError mapAppErrorToApiError
        }

    let healthApi: SupplierCollaborationHealthApi =
        {
            Publish =
                fun req ->
                    deps.HealthAggregate.Publish req
                    |> TaskResult.map (fun health ->
                        match health.Versions |> List.sortByDescending (fun v -> v.VersionNumber) |> List.tryHead with
                        | Some latestVersion ->
                            SupplierCollaborationHealth.Projections.mapVersionToDto latestVersion
                        | None ->
                            { AssessmentScopeId = req.AssessmentScopeId
                              EvaluationPeriod = req.EvaluationPeriod
                              VersionNumber = 1
                              AssessmentVersionId = $"{req.AssessmentScopeId}#{req.EvaluationPeriod}#v1"
                              LifecycleState = "Draft"
                              PublicationTimestamp = req.PublicationTimestamp
                              OverallPublicationOutcome = "Published"
                              SupplierClassifications = []
                              ResponsivenessTrends = []
                              CommitmentAccuracyTrends = []
                              RecommendationSignals = [] })
                    |> TaskResult.mapError mapAppErrorToApiError
        }

    {
        Commitment = commitmentApi
        Assessment = assessmentApi
        Health = healthApi
    }

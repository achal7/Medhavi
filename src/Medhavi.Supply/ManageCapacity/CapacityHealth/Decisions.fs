/// DE-S-032 — Classify Capacity Health Decision
/// Traces to: Supply Intelligence Specification §6 (DE-S-032)
module Medhavi.Supply.ManageCapacity.CapacityHealth.Decisions

open Medhavi.SemanticModel
open Medhavi.Supply
open Model
open Policies

/// DE-S-032: Classify Capacity Health across scope and generate published version
let classifyHealth
    (scopeId: PlanningScopeId)
    (periodId: EvaluationPeriodId)
    (positions: CapacityPositionFact list)
    (existingOpt: CapacityHealthAssessment option)
        (timestamp: Timestamp)
        (policy: CapacityHealthPolicy)
        : CapacityHealthAssessment * CapacityHealthAssessmentVersion * int option =

        let nextVersion = Rules.getNextVersionNumber existingOpt

        let newVersion =
            Algorithms.determineHealth scopeId periodId positions timestamp policy nextVersion

        let supersededVersionOpt =
            match existingOpt with
            | Some existing ->
                existing.Versions
                |> List.tryFind (fun v -> v.State = HealthVersionLifecycleState.Published)
                |> Option.map (fun v -> v.VersionNumber)
            | None -> None

        // Mark previously published versions as Superseded (BR-S-166)
        let updatedVersions =
            match existingOpt with
            | None -> [newVersion]
            | Some existing ->
                let archived =
                    existing.Versions
                    |> List.map (fun v ->
                        if v.State = HealthVersionLifecycleState.Published then
                            { v with State = HealthVersionLifecycleState.Superseded }
                        else v)
                newVersion :: archived

        let updatedAssessment : CapacityHealthAssessment =
            { AssessmentScopeId = scopeId
              EvaluationPeriodId = periodId
              Versions = updatedVersions }

        updatedAssessment, newVersion, supersededVersionOpt

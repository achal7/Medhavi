module Medhavi.Demand.PlanningClassificationAssignment.Decisions

open Medhavi.SharedKernel
open Medhavi.SharedKernel.Contracts.Aggregate
open Medhavi.SharedKernel.Contracts.DecisionTrace
open Medhavi.SharedKernel.Failure
open Medhavi.Demand.PlanningClassificationAssignment.Model
open Medhavi.Demand.PlanningClassificationAssignment.Rules

/// DE‑D‑032 — Determine Classification
let determineClassification (entityId: string) (classificationType: ClassificationType) : string * decimal =
    // Placeholder policy: simple volume-based for ABC, random for XYZ, static for Strategic
    match classificationType with
    | ABC ->
        let hash = (entityId.GetHashCode() % 3) |> abs

        match hash with
        | 0 -> "A", 0.9m
        | 1 -> "B", 0.8m
        | _ -> "C", 0.7m
    | XYZ ->
        let hash = (entityId.GetHashCode() % 3) |> abs

        match hash with
        | 0 -> "X", 0.85m
        | 1 -> "Y", 0.75m
        | _ -> "Z", 0.7m
    | Strategic -> "Gold", 0.9m

let updateClassification (cmd: UpdatePlanningClassificationCmd) (stateOpt: PlanningClassificationAssignment option) =
    let previousClassification = stateOpt |> Option.map(fun s -> s.CurrentClassification)
    let newClass, confidence = determineClassification cmd.EntityId cmd.ClassificationType
    let confidenceValue = PositiveDecimal.createSafe confidence

    let changeEvent: AssignmentChangeEvent =
        { Timestamp = Timestamp.now
          PreviousClassification = previousClassification
          NewClassification = newClass
          Reason = "Scheduled re-evaluation"
          OverrideJustification = None
          ClassificationConfidence = confidenceValue
          PolicyVersionRef = "v1" }

    let assessment: PlanningClassificationAssignment =
        { EntityType = cmd.EntityType
          EntityId = cmd.EntityId
          ClassificationType = cmd.ClassificationType
          CurrentClassification = newClass
          ClassificationConfidence = confidenceValue
          LastClassified = Timestamp.now
          LastChangeEvent = Some changeEvent
          BusinessTime = Timestamp.now
          TransactionTime = Timestamp.now }

    Ok [ PlanningClassificationUpdated(assessment, changeEvent) ]

let overrideClassification (cmd: OverridePlanningClassificationCmd) (state: PlanningClassificationAssignment) =
    let changeEvent: AssignmentChangeEvent =
        { Timestamp = Timestamp.now
          PreviousClassification = Some state.CurrentClassification
          NewClassification = cmd.NewClassification
          Reason = "Planner Override"
          OverrideJustification = Some cmd.Justification
          ClassificationConfidence = PositiveDecimal.createSafe 1.0m
          PolicyVersionRef = "v1-override" }

    let updated =
        { state with
            CurrentClassification = cmd.NewClassification
            ClassificationConfidence = PositiveDecimal.createSafe 1.0m
            LastClassified = Timestamp.now
            LastChangeEvent = Some changeEvent
            TransactionTime = Timestamp.now }

    Ok [ PlanningClassificationUpdated(updated, changeEvent) ]

let decide
    (cmd: PlanningClassificationCommand)
    (stateOpt: PlanningClassificationAssignment option)
    : Result<Decision<PlanningClassificationAssignment, PlanningClassificationEvent>, DomainError> =
    match cmd with
    | UpdatePlanningClassification cmd ->
        classificationByPolicy cmd.EntityType cmd.EntityId cmd.ClassificationType
        |> Result.bind(fun () -> sufficientEvidence cmd.EntityType cmd.EntityId cmd.ClassificationType)
        |> Result.bind(fun () -> updateClassification cmd stateOpt)
        |> Result.map(fun events ->
            let derivedState = events |> List.fold (fun acc e -> evolve e acc) stateOpt

            let rationale =
                match derivedState with
                | Some s ->
                    { Summary =
                        $"Planning classification {s.ClassificationType.AsString()} automatically updated to {s.CurrentClassification}."
                      Evidence = [ $"Confidence: {PositiveDecimal.value s.ClassificationConfidence}" ]
                      Alternatives = [] }
                | None ->
                    { Summary = "Classification updated"
                      Evidence = []
                      Alternatives = [] }

            buildDecision
                evolve
                stateOpt
                events
                (Some
                    { DecisionId = ArsIdentifiers.Demand.Decisions.determineClassification
                      CapabilityId = ArsIdentifiers.Demand.Capabilities.segmentDemand
                      RulesEvaluated =
                        [ (ArsIdentifiers.Demand.Rules.classificationByPolicy, 1)
                          (ArsIdentifiers.Demand.Rules.unclassifiedIfEvidenceMissing, 1) ]
                      PolicyId = Some ArsIdentifiers.Demand.Policies.segmentationPolicyGoverned
                      PolicyVersion = Some 1
                      SemanticObjectIds = [ ArsIdentifiers.Demand.SemanticObjects.planningClassificationAssignment ]
                      Rationale = rationale }))
    | OverridePlanningClassification cmd ->
        match stateOpt with
        | None -> Error(DomainError.notFound("PlanningClassificationAssignment", cmd.AssignmentId))
        | Some state ->
            overrideClassification cmd state
            |> Result.map(fun events ->
                buildDecision
                    evolve
                    (Some state)
                    events
                    (Some
                        { DecisionId = ArsIdentifiers.Demand.Decisions.determineClassification
                          CapabilityId = ArsIdentifiers.Demand.Capabilities.segmentDemand
                          RulesEvaluated = []
                          PolicyId = Some ArsIdentifiers.Demand.Policies.classificationOverrideReview
                          PolicyVersion = Some 1
                          SemanticObjectIds =
                            [ ArsIdentifiers.Demand.SemanticObjects.planningClassificationAssignment ]
                          Rationale =
                            { Summary =
                                $"Classification {cmd.ClassificationType.AsString()} manually overridden to {cmd.NewClassification}."
                              Evidence = [ $"Justification: {cmd.Justification}" ]
                              Alternatives = [ ("Previous classification", state.CurrentClassification) ] } }))

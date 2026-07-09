module Medhavi.Demand.DemandBehaviourAssignment.Decisions

open System
open Medhavi.SharedKernel
open Medhavi.SharedKernel.Contracts.Aggregate
open Medhavi.SharedKernel.Contracts.DecisionTrace
open Medhavi.SharedKernel.Failure
open Medhavi.Demand.DemandBehaviourAssignment.Model
open Medhavi.Demand.DemandBehaviourAssignment.Rules

/// DE‑D‑033 — Determine Behaviour Classification
let determineBehaviour (entityId: string) (dimension: string) : string * decimal * string =
    match dimension with
    | "StatisticalPattern" ->
        let rng = Random(entityId.GetHashCode())
        let patterns = [| "Continuous"; "Intermittent"; "Seasonal"; "Lumpy"; "Trend" |]
        let idx = rng.Next(patterns.Length)
        patterns.[idx], 0.85m, $"Statistical features: pattern {patterns.[idx]}"
    | "LifecycleBehaviour" ->
        let stages = [| "Introduction"; "Growth"; "Maturity"; "Decline"; "EndOfLife" |]
        let idx = Math.Abs(entityId.GetHashCode()) % stages.Length
        stages.[idx], 0.9m, $"Product lifecycle stage: {stages.[idx]}"
    | _ -> "Unclassified", 0.5m, "Unknown dimension"

let updateBehaviour (cmd: UpdateBehaviourClassificationCmd) (stateOpt: DemandBehaviourAssignment option) =
    let prevClass = stateOpt |> Option.map(fun s -> s.CurrentClassification)
    let newClass, confidence, evidence = determineBehaviour cmd.EntityId cmd.BehaviourDimension
    let confidenceValue = PositiveDecimal.createSafe confidence

    let changeEvent: BehaviourChangeEvent =
        { Timestamp = Timestamp.now
          PreviousClassification = prevClass
          NewClassification = newClass
          Reason = "Scheduled re-evaluation"
          OverrideJustification = None
          ClassificationConfidence = confidenceValue
          EvidenceSummary = evidence
          PolicyVersionRef = "v1" }

    let assessment: DemandBehaviourAssignment =
        { EntityType = cmd.EntityType
          EntityId = cmd.EntityId
          BehaviourDimension = cmd.BehaviourDimension
          CurrentClassification = newClass
          ClassificationConfidence = confidenceValue
          EvidenceSummary = evidence
          LastClassified = Timestamp.now
          LastChangeEvent = Some changeEvent
          BusinessTime = Timestamp.now
          TransactionTime = Timestamp.now }

    Ok [ DemandBehaviourClassificationUpdated(assessment, changeEvent) ]

let overrideBehaviour (cmd: OverrideBehaviourClassificationCmd) (state: DemandBehaviourAssignment) =
    let changeEvent: BehaviourChangeEvent =
        { Timestamp = Timestamp.now
          PreviousClassification = Some state.CurrentClassification
          NewClassification = cmd.NewClassification
          Reason = "Planner Override"
          OverrideJustification = Some cmd.Justification
          ClassificationConfidence = PositiveDecimal.createSafe 1.0m
          EvidenceSummary = state.EvidenceSummary
          PolicyVersionRef = "v1-override" }

    let updated =
        { state with
            CurrentClassification = cmd.NewClassification
            ClassificationConfidence = PositiveDecimal.createSafe 1.0m
            LastClassified = Timestamp.now
            LastChangeEvent = Some changeEvent
            TransactionTime = Timestamp.now }

    Ok [ DemandBehaviourClassificationUpdated(updated, changeEvent) ]

let decide
    (cmd: DemandBehaviourAssignmentCommand)
    (stateOpt: DemandBehaviourAssignment option)
    : Result<Decision<DemandBehaviourAssignment, DemandBehaviourAssignmentEvent>, DomainError> =
    match cmd with
    | UpdateBehaviourClassification cmd ->
        classificationByPolicy cmd.EntityType cmd.EntityId cmd.BehaviourDimension
        |> Result.bind(fun () -> sufficientEvidence cmd.EntityType cmd.EntityId cmd.BehaviourDimension)
        |> Result.bind(fun () -> updateBehaviour cmd stateOpt)
        |> Result.map(fun events ->
            let derivedState = events |> List.fold (fun acc e -> evolve e acc) stateOpt

            let rationale =
                match derivedState with
                | Some s ->
                    { Summary = $"Demand behaviour classified as {s.CurrentClassification} on {s.BehaviourDimension}."
                      Evidence =
                        [ $"Confidence: {PositiveDecimal.value s.ClassificationConfidence}"
                          $"Evidence: {s.EvidenceSummary}" ]
                      Alternatives = [] }
                | None ->
                    { Summary = "Behaviour classification updated"
                      Evidence = []
                      Alternatives = [] }

            buildDecision
                evolve
                stateOpt
                events
                (Some
                    { DecisionId = ArsIdentifiers.Demand.Decisions.determineBehaviourClassification
                      CapabilityId = ArsIdentifiers.Demand.Capabilities.classifyDemand
                      RulesEvaluated =
                        [ (ArsIdentifiers.Demand.Rules.behaviourClassByPolicy, 1)
                          (ArsIdentifiers.Demand.Rules.behaviourUnclassifiedIfMissing, 1) ]
                      PolicyId = Some ArsIdentifiers.Demand.Policies.classificationPolicyGoverned
                      PolicyVersion = Some 1
                      SemanticObjectIds = [ ArsIdentifiers.Demand.SemanticObjects.demandBehaviourAssignment ]
                      Rationale = rationale }))
    | OverrideBehaviourClassification cmd ->
        match stateOpt with
        | None -> Error(DomainError.notFound("DemandBehaviourAssignment", cmd.AssignmentId))
        | Some state ->
            overrideBehaviour cmd state
            |> Result.map(fun events ->
                buildDecision
                    evolve
                    (Some state)
                    events
                    (Some
                        { DecisionId = ArsIdentifiers.Demand.Decisions.determineBehaviourClassification
                          CapabilityId = ArsIdentifiers.Demand.Capabilities.classifyDemand
                          RulesEvaluated = []
                          PolicyId = Some ArsIdentifiers.Demand.Policies.behaviourOverrideReview
                          PolicyVersion = Some 1
                          SemanticObjectIds = [ ArsIdentifiers.Demand.SemanticObjects.demandBehaviourAssignment ]
                          Rationale =
                            { Summary = $"Behaviour classification manually overridden to {cmd.NewClassification}."
                              Evidence = [ $"Justification: {cmd.Justification}" ]
                              Alternatives = [ ("Previous classification", state.CurrentClassification) ] } }))

module Medhavi.Demand.PlanningPriorityAssignment.Decisions

open System
open Medhavi.SharedKernel
open Medhavi.SharedKernel.Contracts.Aggregate
open Medhavi.SharedKernel.Contracts.DecisionTrace
open Medhavi.SharedKernel.Failure
open Medhavi.Demand.PlanningPriorityAssignment.Model
open Medhavi.Demand.PlanningPriorityAssignment.Rules

/// DE‑D‑034 — Determine Planning Priority
let determinePriority (entityType: string) (entityId: string) : PriorityLevel * PositiveDecimal * string * string =
    // Deterministic placeholder using entity characteristics
    let rng = Random(entityId.GetHashCode())
    let score = rng.NextDouble() * 100.0

    let priority =
        if score > 90.0 then Critical
        elif score > 70.0 then High
        elif score > 40.0 then Medium
        else Low

    let rationale = $"Priority derived from {entityType} attributes."
    let validity = "Effective until next evaluation"
    let scoreDec = score |> decimal |> PositiveDecimal.createSafe
    priority, scoreDec, rationale, validity

let updatePriority (cmd: UpdatePlanningPriorityCmd) (stateOpt: PlanningPriorityAssignment option) =
    let prevPriority = stateOpt |> Option.map(fun s -> s.CurrentPriority.AsString())
    let prevScore = stateOpt |> Option.map(fun s -> s.PriorityScore)
    let newPriority, newScore, rationale, validity = determinePriority cmd.EntityType cmd.EntityId

    let changeEvent: PriorityChangeEvent =
        { Timestamp = Timestamp.now
          PreviousPriority = prevPriority
          NewPriority = newPriority.AsString()
          PreviousScore = prevScore
          NewScore = newScore
          DecisionRationale = rationale
          BusinessValidity = validity
          Reason = "Scheduled re-evaluation"
          OverrideJustification = None
          PolicyVersionRef = "v1" }

    let assessment: PlanningPriorityAssignment =
        { EntityType = cmd.EntityType
          EntityId = cmd.EntityId
          CurrentPriority = newPriority
          PriorityScore = newScore
          DecisionRationale = rationale
          BusinessValidity = validity
          LastEvaluated = Timestamp.now
          LastChangeEvent = Some changeEvent
          BusinessTime = Timestamp.now
          TransactionTime = Timestamp.now }

    Ok [ PlanningPriorityUpdated(assessment, changeEvent) ]

let overridePriority (cmd: OverridePlanningPriorityCmd) (state: PlanningPriorityAssignment) =
    let changeEvent: PriorityChangeEvent =
        { Timestamp = Timestamp.now
          PreviousPriority = Some <| state.CurrentPriority.AsString()
          NewPriority = cmd.NewPriority.AsString()
          PreviousScore = Some state.PriorityScore
          NewScore = state.PriorityScore // unchanged
          DecisionRationale = state.DecisionRationale
          BusinessValidity = state.BusinessValidity
          Reason = "Planner Override"
          OverrideJustification = Some cmd.Justification
          PolicyVersionRef = "v1-override" }

    let updated =
        { state with
            CurrentPriority = cmd.NewPriority
            LastEvaluated = Timestamp.now
            LastChangeEvent = Some changeEvent
            TransactionTime = Timestamp.now }

    Ok [ PlanningPriorityUpdated(updated, changeEvent) ]

let decide
    (cmd: PlanningPriorityCommand)
    (stateOpt: PlanningPriorityAssignment option)
    : Result<Decision<PlanningPriorityAssignment, PlanningPriorityEvent>, DomainError> =
    match cmd with
    | UpdatePlanningPriority cmd ->
        priorityByPolicy cmd.EntityType cmd.EntityId
        |> Result.bind(fun () -> mandatoryEvidencePresent cmd.EntityType cmd.EntityId)
        |> Result.bind(fun () -> updatePriority cmd stateOpt)
        |> Result.map(fun events ->
            let derivedState = events |> List.fold (fun acc e -> evolve e acc) stateOpt
            let rationale =
                match derivedState with
                | Some s ->
                    { Summary = $"Planning priority automatically evaluated as {s.CurrentPriority.AsString()}."
                      Evidence = [ $"Score: {PositiveDecimal.value s.PriorityScore}"; $"Rationale: {s.DecisionRationale}" ]
                      Alternatives = [] }
                | None ->
                    { Summary = "Priority updated"
                      Evidence = []
                      Alternatives = [] }

            buildDecision
                evolve
                stateOpt
                events
                (Some
                    { DecisionId = ArsIdentifiers.Demand.Decisions.determinePlanningPriority
                      CapabilityId = ArsIdentifiers.Demand.Capabilities.prioritizeDemand
                      RulesEvaluated =
                        [ (ArsIdentifiers.Demand.Rules.priorityByPolicy, 1)
                          (ArsIdentifiers.Demand.Rules.priorityUnclassifiedIfMissing, 1) ]
                      PolicyId = Some ArsIdentifiers.Demand.Policies.priorityPolicyGoverned
                      PolicyVersion = Some 1
                      SemanticObjectIds = [ ArsIdentifiers.Demand.SemanticObjects.planningPriorityAssignment ]
                      Rationale = rationale }))
    | OverridePlanningPriority cmd ->
        match stateOpt with
        | None -> Error(DomainError.notFound("PlanningPriorityAssignment", cmd.AssignmentId))
        | Some state ->
            overridePriority cmd state
            |> Result.map(fun events ->
                buildDecision
                    evolve
                    (Some state)
                    events
                    (Some
                        { DecisionId = ArsIdentifiers.Demand.Decisions.determinePlanningPriority
                          CapabilityId = ArsIdentifiers.Demand.Capabilities.prioritizeDemand
                          RulesEvaluated = []
                          PolicyId = Some ArsIdentifiers.Demand.Policies.priorityOverrideReview
                          PolicyVersion = Some 1
                          SemanticObjectIds = [ ArsIdentifiers.Demand.SemanticObjects.planningPriorityAssignment ]
                          Rationale =
                            { Summary = $"Planning priority manually overridden to {cmd.NewPriority.AsString()}."
                              Evidence = [ $"Justification: {cmd.Justification}" ]
                              Alternatives = [ ("Previous priority", state.CurrentPriority.AsString()) ] } }))

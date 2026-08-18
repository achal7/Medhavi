namespace Medhavi.SemanticModel

type PlanId = private PlanId of string

module PlanId =
    let create (id: string) = Invariants.createStringId PlanId "PlanId" id
    let value (PlanId id) = id

/// Lifecycle states for Plan
type PlanLifecycleState =
    | Draft
    | Approved
    | Superseded
    | Archived

module PlanLifecycleState =
    let validateTransition
        (fromState: PlanLifecycleState)
        (toState: PlanLifecycleState)
        : Result<unit, SemanticValidationError> =
        match fromState, toState with
        | PlanLifecycleState.Draft, PlanLifecycleState.Approved
        | PlanLifecycleState.Draft, PlanLifecycleState.Archived
        | PlanLifecycleState.Approved, PlanLifecycleState.Superseded
        | PlanLifecycleState.Approved, PlanLifecycleState.Archived -> Ok()
        | _ -> Error(InvalidLifecycleTransition(sprintf "%A -> %A is not a permitted lifecycle transition." fromState toState))

/// SE-C-012 Plan
type Plan =
    { PlanIdentifier: PlanId
      PlanName: string
      PlanningScope: PlanningScopeId
      PlanningHorizon: PlanningHorizon
      Scenario: ScenarioId
      LifecycleState: PlanLifecycleState }

module Plan =
    let validate (plan: Plan) : Result<unit, SemanticValidationError> =
        Invariants.firstError
            [ Invariants.nonEmptyIdentifier "PlanId" (PlanId.value plan.PlanIdentifier)
              Invariants.nonEmptyField "Plan" "PlanName" plan.PlanName
              Invariants.nonEmptyIdentifier "Plan.PlanningScope" (PlanningScopeId.value plan.PlanningScope)
              Invariants.nonEmptyIdentifier "Plan.Scenario" (ScenarioId.value plan.Scenario)
              PlanningHorizon.validatePlanningHorizon plan.PlanningHorizon ]

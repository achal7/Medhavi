namespace Medhavi.SemanticModel

type PlanningScopeId = private PlanningScopeId of string

module PlanningScopeId =
    let create (id: string) = Invariants.createStringId PlanningScopeId "PlanningScopeId" id
    let value (PlanningScopeId id) = id

/// SE-C-010 Planning Scope
type PlanningScope =
    { PlanningScopeIdentifier: PlanningScopeId
      ScopeName: string
      BoundaryStatement: string option
      BoundaryRules: ScopeBoundaryRule list
      LifecycleState: ReferenceLifecycleState }

module PlanningScope =
    let validate (scope: PlanningScope) : Result<unit, SemanticValidationError> =
        let boundaryRuleChecks = scope.BoundaryRules |> List.map ScopeBoundaryRule.validate

        Invariants.firstError(
            [ Invariants.nonEmptyIdentifier "PlanningScopeId" (PlanningScopeId.value scope.PlanningScopeIdentifier)
              Invariants.nonEmptyField "PlanningScope" "ScopeName" scope.ScopeName ]
            @ boundaryRuleChecks
        )

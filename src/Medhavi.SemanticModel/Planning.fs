namespace Medhavi.SemanticModel

/// SE-C-010 Planning Scope
type PlanningScope =
    { PlanningScopeIdentifier: PlanningScopeId
      ScopeName: string
      BoundaryStatement: string option
      BoundaryRules: ScopeBoundaryRule list
      LifecycleState: ReferenceLifecycleState }

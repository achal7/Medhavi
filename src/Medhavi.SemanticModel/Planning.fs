namespace Medhavi.SemanticModel

/// SE-C-010 Planning Scope
/// Planning Scope is the boundary object used by Enterprise Picture composition.
type PlanningScope =
    { PlanningScopeIdentifier: PlanningScopeId
      ScopeName: string
      BoundaryRules: ScopeBoundaryRule list
      LifecycleState: ReferenceLifecycleState }

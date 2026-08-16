namespace Medhavi.SemanticModel

/// SE-C-011 Scenario
type Scenario =
    { ScenarioIdentifier: ScenarioId
      ScenarioName: string
      AssumptionStatement: string option
      Adjustments: ScenarioAdjustment list
      LifecycleState: ScenarioLifecycleState }

/// SE-C-012 Plan
type Plan =
    { PlanIdentifier: PlanId
      PlanName: string
      PlanningScope: PlanningScopeId
      PlanningHorizon: PlanningHorizon
      Scenario: ScenarioId
      LifecycleState: PlanLifecycleState }

/// SE-C-033 Calendar
type Calendar =
    { CalendarIdentifier: CalendarId
      CalendarName: string
      TimeZone: TimeZoneId
      CalendarDefinitionReference: string
      VersionNumber: int
      AdoptionState: CalendarAdoptionState }

/// SE-C-034 Planning Period
type PlanningPeriod =
    { PlanningPeriodIdentifier: PlanningPeriodId
      DisplayName: string
      AdoptionState: AdoptionState }

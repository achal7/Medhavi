namespace Medhavi.SemanticModel

/// SE-C-011 Scenario
type Scenario =
    { ScenarioIdentifier: ScenarioId
      ScenarioName: string
      BaseScenario: ScenarioId option
      Adjustments: ScenarioAdjustment list
      LifecycleState: PlanningLifecycleState }

/// SE-C-012 Plan
type Plan =
    { PlanIdentifier: PlanId
      PlanName: string
      PlanningScope: PlanningScopeId
      PlanningHorizon: PlanningHorizon
      LifecycleState: PlanningLifecycleState }

/// SE-C-033 Calendar
type Calendar =
    { CalendarIdentifier: CalendarId
      CalendarName: string
      TimeZone: TimeZoneId
      LifecycleState: ReferenceLifecycleState }

/// SE-C-034 Planning Period
type PlanningPeriod =
    { PlanningPeriodIdentifier: PlanningPeriodId
      Calendar: CalendarId
      PeriodType: PeriodType
      Start: Timestamp
      End: Timestamp }

module PlanningPeriod =
    let create
        (planningPeriodIdentifier: PlanningPeriodId)
        (calendar: CalendarId)
        (periodType: PeriodType)
        (start: Timestamp)
        (endTimestamp: Timestamp)
        : Result<PlanningPeriod, SemanticValidationError> =

        if Timestamp.isAfter start endTimestamp then
            Error(InvalidWindow "PlanningPeriod.Start must not be after PlanningPeriod.End.")
        else
            Ok
                { PlanningPeriodIdentifier = planningPeriodIdentifier
                  Calendar = calendar
                  PeriodType = periodType
                  Start = start
                  End = endTimestamp }

namespace Medhavi.SemanticModel

type PlanningPeriodId = private PlanningPeriodId of string

module PlanningPeriodId =
    let create (id: string) = Invariants.createStringId PlanningPeriodId "PlanningPeriodId" id
    let value (PlanningPeriodId id) = id

/// SE-C-034 Planning Period
type PlanningPeriod =
    { PlanningPeriodIdentifier: PlanningPeriodId
      DisplayName: string
      AdoptionState: AdoptionState }

module PlanningPeriod =
    let validate (period: PlanningPeriod) : Result<unit, SemanticValidationError> =
        Invariants.firstError
            [ Invariants.nonEmptyIdentifier "PlanningPeriodId" (PlanningPeriodId.value period.PlanningPeriodIdentifier)
              Invariants.nonEmptyField "PlanningPeriod" "DisplayName" period.DisplayName ]

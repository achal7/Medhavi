namespace Medhavi.SemanticModel

/// SE-C-027 Planning Horizon
type PlanningHorizon = { Start: Timestamp; End: Timestamp }

module PlanningHorizon =
    let create (start: Timestamp) (endTimestamp: Timestamp) : Result<PlanningHorizon, SemanticValidationError> =
        if Timestamp.isAfter start endTimestamp then
            Error(InvalidWindow "PlanningHorizon.Start must not be after PlanningHorizon.End.")
        else
            Ok { Start = start; End = endTimestamp }

    let start (horizon: PlanningHorizon) = horizon.Start
    let endTimestamp (horizon: PlanningHorizon) = horizon.End
    let duration (horizon: PlanningHorizon) : System.TimeSpan = Timestamp.diff horizon.End horizon.Start

    let validatePlanningHorizon (horizon: PlanningHorizon) : Result<unit, SemanticValidationError> =
        if Timestamp.isAfter horizon.Start horizon.End then
            Error(InvalidWindow "PlanningHorizon.Start must not be after PlanningHorizon.End.")
        else
            Ok()

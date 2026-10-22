namespace Medhavi.SemanticModel

/// SE-C-027 Planning Horizon
type PlanningHorizon = { Start: Timestamp; End: Timestamp }

module PlanningHorizon =
    open System

    let create (start: Timestamp) (endTimestamp: Timestamp) : Result<PlanningHorizon, SemanticValidationError> =
        if Timestamp.isAfter start endTimestamp then
            Error(InvalidWindow "PlanningHorizon.Start must not be after PlanningHorizon.End.")
        else
            Ok { Start = start; End = endTimestamp }

    let createFromString (raw: string) : Result<PlanningHorizon, SemanticValidationError> =
        if String.IsNullOrWhiteSpace raw then
            Error(
                InvalidWindow
                    "PlanningHorizon cannot be empty. Expected ISO-8601 interval (e.g. '2026-01-01T00:00:00Z/2026-12-31T23:59:59Z')."
            )
        else
            let parts =
                if raw.Contains("/") then raw.Split('/')
                elif raw.Contains("..") then raw.Split([| ".." |], StringSplitOptions.None)
                else [||]

            if parts.Length = 2 then
                match DateTimeOffset.TryParse(parts.[0].Trim()), DateTimeOffset.TryParse(parts.[1].Trim()) with
                | (true, startDto), (true, endDto) ->
                    match Timestamp.create startDto, Timestamp.create endDto with
                    | Ok s, Ok e -> create s e
                    | Error err, _
                    | _, Error err ->
                        Error(InvalidWindow "PlanningHorizon.Start must not be after PlanningHorizon.End.")
                | _ ->
                    Error(
                        InvalidWindow
                            $"PlanningHorizon '{raw}' has invalid timestamp formats. Expected ISO-8601 timestamps."
                    )
            else
                Error(
                    InvalidWindow
                        $"PlanningHorizon '{raw}' must be an interval with start and end timestamps separated by '/' or '..' (e.g. '2026-01-01T00:00:00Z/2026-12-31T23:59:59Z')."
                )

    let start (horizon: PlanningHorizon) = horizon.Start
    let endTimestamp (horizon: PlanningHorizon) = horizon.End
    let duration (horizon: PlanningHorizon) : System.TimeSpan = Timestamp.diff horizon.End horizon.Start

    let validatePlanningHorizon (horizon: PlanningHorizon) : Result<unit, SemanticValidationError> =
        if Timestamp.isAfter horizon.Start horizon.End then
            Error(InvalidWindow "PlanningHorizon.Start must not be after PlanningHorizon.End.")
        else
            Ok()

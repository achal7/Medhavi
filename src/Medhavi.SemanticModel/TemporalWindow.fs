namespace Medhavi.SemanticModel

/// SE-C-028 Temporal Window
type TemporalWindow =
    { Earliest: Timestamp option
      Latest: Timestamp }

module TemporalWindow =
    let isValid (window: TemporalWindow) =
        match window.Earliest with
        | Some earliest -> Timestamp.isBefore earliest window.Latest || Timestamp.isEqual earliest window.Latest
        | None -> true

    let validateTemporalWindow (window: TemporalWindow) : Result<unit, SemanticValidationError> =
        isValid window
        |> function
            | true -> Ok()
            | false -> Error(InvalidWindow "TemporalWindow.Earliest must not be after TemporalWindow.Latest.")

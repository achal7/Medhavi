namespace Medhavi.SemanticModel

open Medhavi.SemanticModel.Invariants

/// SE-C-029 Need Window
type NeedWindow =
    { EarliestAcceptable: Timestamp option
      Preferred: Timestamp option
      LatestAcceptable: Timestamp }

module NeedWindow =
    let validateNeedWindow (window: NeedWindow) : Result<unit, SemanticValidationError> =
        let earliestCheck =
            match window.EarliestAcceptable with
            | Some earliest when Timestamp.isAfter earliest window.LatestAcceptable ->
                Error(InvalidWindow "NeedWindow.EarliestAcceptable must not be after NeedWindow.LatestAcceptable.")
            | _ -> Ok()

        let preferredCheck =
            match window.Preferred with
            | Some preferred ->
                let earliestValid =
                    match window.EarliestAcceptable with
                    | Some earliest when Timestamp.isAfter earliest preferred ->
                        Error(InvalidWindow "NeedWindow.Preferred must not be before NeedWindow.EarliestAcceptable.")
                    | _ -> Ok()

                let latestValid =
                    if Timestamp.isAfter preferred window.LatestAcceptable then
                        Error(InvalidWindow "NeedWindow.Preferred must not be after NeedWindow.LatestAcceptable.")
                    else
                        Ok()

                firstError [ earliestValid; latestValid ]
            | None -> Ok()

        firstError [ earliestCheck; preferredCheck ]

    let create
        (latest: System.DateTimeOffset)
        (earliest: System.DateTimeOffset option)
        (preferred: System.DateTimeOffset option)
        : Result<NeedWindow, SemanticValidationError> =

        // Helper to convert optional DateTimeOffset to Result<Timestamp option, SemanticValidationError>
        let createOptionalTimestamp
            (dto: System.DateTimeOffset option)
            : Result<Timestamp option, SemanticValidationError> =
            match dto with
            | None -> Ok None
            | Some d -> Timestamp.create d |> Result.map Some

        // Create all three timestamps
        match Timestamp.create latest, createOptionalTimestamp earliest, createOptionalTimestamp preferred with
        | Ok latestTs, Ok earliestTs, Ok preferredTs ->
            // All timestamps created successfully, build the window
            let win =
                { EarliestAcceptable = earliestTs
                  Preferred = preferredTs
                  LatestAcceptable = latestTs }

            // Validate and return
            validateNeedWindow win |> Result.map(fun () -> win)
        | Error e, _, _ -> Error e
        | _, Error e, _ -> Error e
        | _, _, Error e -> Error e

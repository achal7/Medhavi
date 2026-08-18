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

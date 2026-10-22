/// Decisions for Supplier Commitment Assessment
module Medhavi.Supply.CollaborateWithSuppliers.SupplierCommitmentAssessment.Decisions

open Medhavi.Supply
open Model

/// Detects material change between previous assessment state and new health state/exposures
let isMaterialChange
    (prevOpt: SupplierCommitmentAssessment option)
    (newHealthState: SupplierCollaborationHealthState)
    (newExposures: DimensionalExposure list)
    : bool =
    match prevOpt with
    | None -> true
    | Some prev ->
        prev.AuthoritativeHealthState <> newHealthState
        || (prev.Exposures |> List.map(fun e -> e.Level)) <> (newExposures |> List.map(fun e -> e.Level))

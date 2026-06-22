namespace Medhavi.Demand.Domain

open System
open Medhavi.SharedKernel
open Medhavi.Common.Validation

/// Customer Order aggregate (historical/read model reference)
type CustomerOrder =
    { OrderId: OrderId
      LineId: string
      SkuId: SkuId
      NodeId: NodeId
      Quantity: Quantity
      DueDate: Timestamp
      Priority: int
      IsExpedited: bool }

/// Demand Forecast aggregate (historical/read model reference)
type Forecast =
    { ForecastId: string
      SkuId: SkuId
      NodeId: NodeId
      Quantity: Quantity
      PeriodStart: Timestamp
      PeriodEnd: Timestamp }

// =============================================================================
// APS Demand Types — Full demand model with standard APS date semantics
// =============================================================================

/// Classification of where a demand signal originated
type DemandCategory =
    | CustomerOrderDemand // hard demand from a confirmed sales order
    | SalesOrderForecast // statistical forecast before order confirmation
    | InterplantTransfer // demand from another plant/node in the network
    | ServicePart // spare parts / aftermarket demand
    | InternalConsumption // production self-consumption (e.g., components)
    | DependentDemand // exploded demand from MRP/BOM explosion

/// Rich, state-aware planning and execution lifecycle
type DemandStatus =
    | Active
    | Cancelled
    | Fulfilled

/// Rich demand priority classification
type DemandPriority =
    | Critical
    | High
    | Normal
    | Low

module DemandPriority =

    let weight =
        function
        | Critical -> 1
        | High -> 2
        | Normal -> 3
        | Low -> 4

/// Source integration metadata for replayability and audit traces
type Provenance =
    { SourceSystem: string
      ExternalRef: string
      MessageId: string
      Revision: Revision
      ScenarioId: ScenarioId option }

/// Allocation tracker for consumed forecasts
type ForecastAllocation =
    { ForecastId: string
      ConsumedQuantity: Quantity }

/// Local domain record for pegging links between demand and supply.
type PeggedSupply =
    { SupplyOrderId: string
      SupplyType: string
      Quantity: Quantity
      PlannedDate: DateTimeOffset }

type DemandLine =
    { DemandLineId: string
      DemandOrderId: string
      SkuId: SkuId
      StockingPointId: StockingPointId
      CustomerId: string
      Quantity: Quantity
      UnitOfMeasure: string
      // --- APS date fields ---
      OrderDate: Timestamp
      EarliestDeliveryDate: Timestamp option
      RequestedDeliveryDate: Timestamp // primary target / projection bucket key
      LatestDeliveryDate: Timestamp option // hard upper bound; after this = penalty
      ConfirmedDeliveryDate: Timestamp option // APS promise result from planning
      ActualDeliveryDate: Timestamp option // execution reality (for KPI retrospective)

      // --- APS planning attributes ---
      ConfirmedQty: Quantity
      Priority: DemandPriority
      DemandCategory: DemandCategory
      IsFirm: bool // firm demand: APS cannot defer or cancel
      IsFrozen: bool // frozen period: can adjust qty but not cancel
      FrozenUntilUtc: Timestamp option
      IsOnHold: bool
      OnHoldReason: string option
      CancelReason: string option
      CancelledAtUtc: Timestamp option
      // --- Provenance & Scenario ---
      Provenance: Provenance
      // --- Fulfillment state ---
      OpenQuantity: Quantity
      FulfilledQuantity: Quantity
      Status: DemandStatus }

module DemandLineAgg =
    type IngestDemandLineCmd =
        { DemandLineId: string
          DemandOrderId: string
          SkuId: SkuId
          StockingPointId: StockingPointId
          CustomerId: string
          Quantity: Quantity
          UnitOfMeasure: string
          OrderDate: Timestamp
          EarliestDeliveryDate: Timestamp option
          RequestedDeliveryDate: Timestamp
          LatestDeliveryDate: Timestamp option
          Priority: DemandPriority
          DemandCategory: DemandCategory
          IsFirm: bool
          IsFrozen: bool
          Provenance: Provenance }

    type ReviseDemandLineCmd =
        { DemandLineId: string
          Quantity: Quantity option
          RequestedDeliveryDate: Timestamp option
          EarliestDeliveryDate: Timestamp option
          LatestDeliveryDate: Timestamp option
          Priority: DemandPriority option
          IsFirm: bool option
          IsFrozen: bool option
          ProvenanceRevision: Revision }

    type PromiseDemandLineCmd =
        { DemandLineId: string
          PromisedDate: Timestamp
          ConfirmedQty: Quantity }

    type FreezeDemandLineCmd =
        { DemandLineId: string
          FrozenUntilUtc: Timestamp }

    type ReleaseDemandLineCmd =
        { DemandLineId: string
          ReleaseFromHold: bool
          Unfreeze: bool }

    type CancelDemandLineCmd =
        { DemandLineId: string
          Reason: string
          CancelledAtUtc: Timestamp
          ForceOverride: bool }

    type RecordExecutionFulfillmentCmd =
        { DemandLineId: string
          Quantity: Quantity
          ActualDeliveryDate: Timestamp }

    type HoldDemandLineCmd =
        { DemandLineId: string; Reason: string }

    type DemandLineCommand =
        | IngestDemandLine of IngestDemandLineCmd
        | ReviseDemandLine of ReviseDemandLineCmd
        | PromiseDemandLine of PromiseDemandLineCmd
        | FreezeDemandLine of FreezeDemandLineCmd
        | HoldDemandLine of HoldDemandLineCmd
        | ReleaseDemandLine of ReleaseDemandLineCmd
        | CancelDemandLine of CancelDemandLineCmd
        | RecordExecutionFulfillment of RecordExecutionFulfillmentCmd

    type DemandPromised =
        { DemandLineId: string
          PromisedDate: Timestamp
          ConfirmedQty: Quantity }

    type DemandLineConfirmed =
        { DemandLineId: string
          ConfirmedDate: Timestamp
          ConfirmedQty: Quantity }

    type DemandLineOnHold =
        { DemandLineId: string; Reason: string }

    type DemandLineFrozen =
        { DemandLineId: string
          FrozenUntilUtc: Timestamp }

    type DemandLineReleased =
        { DemandLineId: string
          ReleaseFromHold: bool
          Unfreeze: bool }

    type DemandPlacedOnHold =
        { DemandLineId: string; Reason: string }

    type DemandLineCancelled =
        { DemandLineId: string
          Reason: string
          CancelledAtUtc: Timestamp }

    type DemandLineFulfillmentRecorded =
        { DemandLineId: string
          Quantity: Quantity
          ActualDeliveryDate: Timestamp }

    type DemandLineRevised =
        { DemandLineId: string
          EarliestDeliveryDate: option<Timestamp>
          IsFirm: option<bool>
          IsFrozen: option<bool>
          LatestDeliveryDate: option<Timestamp>
          Priority: option<DemandPriority>
          ProvenanceRevision: Revision
          Quantity: option<Quantity>
          RequestedDeliveryDate: option<Timestamp> }

    type DemandLineEvent =
        | DemandLineIngested of DemandLine
        | DemandLineRevised of DemandLineRevised
        | DemandLinePromised of DemandPromised
        | DemandLineConfirmed of DemandLineConfirmed
        | DemandLineFrozen of DemandLineFrozen
        | DemandPlacedOnHold of DemandPlacedOnHold
        | DemandLineReleased of DemandLineReleased
        | DemandLineCancelled of DemandLineCancelled
        | DemandLineFulfillmentRecorded of DemandLineFulfillmentRecorded

    type DecideDemandLine = Decide<DemandLine, DemandLineCommand, DemandLineEvent>
    type EvolveDemandLine = Evolve<DemandLine, DemandLineEvent>

    let makeDemandLine (cmd: IngestDemandLineCmd) : DemandLine =
        { DemandLineId = cmd.DemandLineId
          DemandOrderId = cmd.DemandOrderId
          SkuId = cmd.SkuId
          StockingPointId = cmd.StockingPointId
          CustomerId = cmd.CustomerId
          Quantity = cmd.Quantity
          UnitOfMeasure = cmd.UnitOfMeasure
          OrderDate = cmd.OrderDate
          EarliestDeliveryDate = cmd.EarliestDeliveryDate
          RequestedDeliveryDate = cmd.RequestedDeliveryDate
          LatestDeliveryDate = cmd.LatestDeliveryDate
          ConfirmedDeliveryDate = None
          ActualDeliveryDate = None
          ConfirmedQty = Quantity.Zero
          Priority = cmd.Priority
          DemandCategory = cmd.DemandCategory
          IsFirm = cmd.IsFirm
          IsFrozen = cmd.IsFrozen
          FrozenUntilUtc = None
          IsOnHold = false
          OnHoldReason = None
          CancelReason = None
          CancelledAtUtc = None
          Provenance = cmd.Provenance
          OpenQuantity = cmd.Quantity
          FulfilledQuantity = Quantity.Zero
          Status = DemandStatus.Active }

    let evolve: EvolveDemandLine =
        fun event stateOpt ->
            match event with
            | DemandLineIngested cmd ->
                Some
                    { DemandLineId = cmd.DemandLineId
                      DemandOrderId = cmd.DemandOrderId
                      SkuId = cmd.SkuId
                      StockingPointId = cmd.StockingPointId
                      CustomerId = cmd.CustomerId
                      Quantity = cmd.Quantity
                      UnitOfMeasure = cmd.UnitOfMeasure
                      OrderDate = cmd.OrderDate
                      EarliestDeliveryDate = cmd.EarliestDeliveryDate
                      RequestedDeliveryDate = cmd.RequestedDeliveryDate
                      LatestDeliveryDate = cmd.LatestDeliveryDate
                      ConfirmedDeliveryDate = None
                      ActualDeliveryDate = None
                      ConfirmedQty = Quantity.Zero
                      Priority = cmd.Priority
                      DemandCategory = cmd.DemandCategory
                      IsFirm = cmd.IsFirm
                      IsFrozen = cmd.IsFrozen
                      FrozenUntilUtc = None
                      IsOnHold = false
                      OnHoldReason = None
                      CancelReason = None
                      CancelledAtUtc = None
                      Provenance = cmd.Provenance
                      OpenQuantity = cmd.Quantity
                      FulfilledQuantity = Quantity.Zero
                      Status = Active }

            | DemandLineRevised cmd ->
                stateOpt
                |> Option.map(fun state ->
                    let updatedProvenance =
                        { state.Provenance with
                            Revision = cmd.ProvenanceRevision }

                    let nextFrozen = cmd.IsFrozen |> Option.defaultValue state.IsFrozen
                    let nextFrozenUntil = if nextFrozen then state.FrozenUntilUtc else None

                    { state with
                        Quantity = cmd.Quantity |> Option.defaultValue state.Quantity
                        RequestedDeliveryDate = cmd.RequestedDeliveryDate |> Option.defaultValue state.RequestedDeliveryDate
                        EarliestDeliveryDate = cmd.EarliestDeliveryDate
                        LatestDeliveryDate = cmd.LatestDeliveryDate
                        Priority = cmd.Priority |> Option.defaultValue state.Priority
                        IsFirm = cmd.IsFirm |> Option.defaultValue state.IsFirm
                        IsFrozen = nextFrozen
                        FrozenUntilUtc = nextFrozenUntil
                        Provenance = updatedProvenance
                        OpenQuantity =
                            match cmd.Quantity with
                            | Some newQty -> newQty - state.FulfilledQuantity
                            | None -> state.OpenQuantity })

            | DemandLinePromised cmd ->
                stateOpt
                |> Option.map(fun state ->
                    { state with
                        ConfirmedDeliveryDate = Some cmd.PromisedDate
                        ConfirmedQty = cmd.ConfirmedQty })

            | DemandLineFrozen cmd ->
                stateOpt
                |> Option.map(fun state ->
                    { state with
                        IsFrozen = true
                        FrozenUntilUtc = Some cmd.FrozenUntilUtc })

            | DemandLineReleased cmd ->
                stateOpt
                |> Option.map(fun state ->
                    let nextFrozen = if cmd.Unfreeze then false else state.IsFrozen
                    let nextFrozenUntil = if nextFrozen then state.FrozenUntilUtc else None
                    let nextIsOnHold = if cmd.ReleaseFromHold then false else state.IsOnHold
                    let nextOnHoldReason = if nextIsOnHold then state.OnHoldReason else None

                    { state with
                        IsFrozen = nextFrozen
                        FrozenUntilUtc = nextFrozenUntil
                        IsOnHold = nextIsOnHold
                        OnHoldReason = nextOnHoldReason })

            | DemandLineCancelled cmd ->
                stateOpt
                |> Option.map(fun state ->
                    { state with
                        OpenQuantity = Quantity.Zero
                        CancelReason = Some cmd.Reason
                        CancelledAtUtc = Some cmd.CancelledAtUtc
                        Status = Cancelled })

            | DemandLineFulfillmentRecorded cmd ->
                stateOpt
                |> Option.map(fun state ->
                    let newFulfilled = state.FulfilledQuantity + cmd.Quantity
                    let finalOpen = state.OpenQuantity - cmd.Quantity
                    let nextStatus = if finalOpen.IsZero then Fulfilled else Active

                    { state with
                        OpenQuantity = finalOpen
                        FulfilledQuantity = newFulfilled
                        ActualDeliveryDate = Some cmd.ActualDeliveryDate
                        Status = nextStatus })
            | DemandLineConfirmed cmd ->
                stateOpt
                |> Option.map(fun state ->
                    { state with
                        ConfirmedDeliveryDate = Some cmd.ConfirmedDate
                        ConfirmedQty = cmd.ConfirmedQty })
            | DemandPlacedOnHold evt ->
                stateOpt
                |> Option.map(fun state ->
                    { state with
                        IsOnHold = true
                        OnHoldReason = Some evt.Reason })

    let decide: DecideDemandLine =
        fun command stateOpt ->
            match command, stateOpt with
            | IngestDemandLine cmd, None ->
                match stateOpt with
                | Some _ -> Error(DomainError.validation "Demand line already exists.")
                | None ->
                    let newState = makeDemandLine cmd

                    Ok
                        { NewState = newState
                          Events = [ DemandLineIngested newState ] }
            | ReviseDemandLine cmd, Some state ->
                match state.Status with
                | Cancelled -> Error(DomainError.validation "Cannot revise a cancelled demand line.")
                | Fulfilled -> Error(DomainError.validation "Cannot revise a fully fulfilled demand line.")
                | Active ->
                    if state.IsFrozen then
                        Error(DomainError.validation "Demand line is frozen.")
                    else
                        match cmd.Quantity with
                        | Some qty when qty < state.FulfilledQuantity ->
                            Error(
                                DomainError.validation
                                    "Demand quantity cannot be revised below already fulfilled quantity."
                            )
                        | _ ->
                            let revised =
                                { DemandLineId = state.DemandLineId
                                  EarliestDeliveryDate = cmd.EarliestDeliveryDate
                                  IsFirm = cmd.IsFirm
                                  IsFrozen = cmd.IsFrozen
                                  LatestDeliveryDate = cmd.LatestDeliveryDate
                                  Priority = cmd.Priority
                                  Quantity = cmd.Quantity
                                  ProvenanceRevision = cmd.ProvenanceRevision
                                  RequestedDeliveryDate = cmd.RequestedDeliveryDate }

                            match evolve (DemandLineRevised revised) (Some state) with
                            | Some newState ->
                                Ok
                                    { NewState = newState
                                      Events = [ DemandLineRevised revised ] }
                            | None -> Error(DomainError.validation "Failed to evolve Revised state.")

            | PromiseDemandLine cmd, Some state ->
                match state.Status with
                | Cancelled -> Error(DomainError.validation "Cannot promise a cancelled demand line.")
                | Fulfilled -> Error(DomainError.validation "Cannot promise a fully fulfilled demand line.")
                | Active ->
                    if cmd.ConfirmedQty > state.OpenQuantity then
                        Error(DomainError.validation "Confirmed quantity exceeds remaining open quantity.")
                    else
                        let promised =
                            { ConfirmedQty = cmd.ConfirmedQty
                              DemandLineId = cmd.DemandLineId
                              PromisedDate = cmd.PromisedDate }
                            |> DemandLinePromised

                        match evolve promised (Some state) with
                        | Some newState ->
                            Ok
                                { NewState = newState
                                  Events = [ promised ] }
                        | None -> Error(DomainError.validation "Failed to evolve Promised state.")

            | FreezeDemandLine cmd, Some state ->
                match state.Status with
                | Cancelled -> Error(DomainError.validation "Cannot freeze a cancelled demand line.")
                | Fulfilled -> Error(DomainError.validation "Cannot freeze a fully fulfilled demand line.")
                | Active ->
                    let frozen =
                        { FrozenUntilUtc = cmd.FrozenUntilUtc
                          DemandLineId = cmd.DemandLineId }
                        |> DemandLineFrozen

                    match evolve frozen (Some state) with
                    | Some newState ->
                        Ok
                            { NewState = newState
                              Events = [ frozen ] }
                    | None -> Error(DomainError.validation "Failed to evolve Frozen state.")

            | ReleaseDemandLine cmd, Some state ->
                if not state.IsFrozen && not state.IsOnHold then
                    Error(DomainError.validation "Demand line is neither frozen nor on hold.")
                else
                    let released =
                        { DemandLineId = cmd.DemandLineId
                          Unfreeze = cmd.Unfreeze
                          ReleaseFromHold = cmd.ReleaseFromHold }
                        |> DemandLineReleased

                    match evolve released (Some state) with
                    | Some newState ->
                        Ok
                            { NewState = newState
                              Events = [ released ] }
                    | None -> Error(DomainError.validation "Failed to evolve Released state.")

            | CancelDemandLine cmd, Some state ->
                if state.IsFrozen && not cmd.ForceOverride then
                    Error(DomainError.validation "Cannot cancel a frozen demand line without policy override.")
                elif state.IsFirm && not cmd.ForceOverride then
                    Error(DomainError.validation "Cannot cancel a firm demand line contract without policy override.")
                else
                    let cancelled =
                        { CancelledAtUtc = cmd.CancelledAtUtc
                          DemandLineId = cmd.DemandLineId
                          Reason = cmd.Reason }
                        |> DemandLineCancelled

                    match evolve cancelled (Some state) with
                    | Some newState ->
                        Ok
                            { NewState = newState
                              Events = [ cancelled ] }
                    | None -> Error(DomainError.validation "Failed to evolve Cancelled state.")

            | RecordExecutionFulfillment cmd, Some state ->
                match state.Status with
                | Cancelled ->
                    Error(DomainError.validation "Cannot record fulfillment against a cancelled demand line.")
                | Active when state.IsOnHold ->
                    Error(
                        DomainError.validation
                            "Cannot record fulfillment against a demand line that is currently on hold."
                    )
                | _ ->
                    if cmd.Quantity > state.OpenQuantity then
                        Error(
                            DomainError.validation
                                "Over-fulfillment is prohibited. Fulfill quantity exceeds remaining open quantity."
                        )
                    else
                        let fulfillmentRecorded =
                            { DemandLineId = cmd.DemandLineId
                              Quantity = cmd.Quantity
                              ActualDeliveryDate = cmd.ActualDeliveryDate }
                            |> DemandLineFulfillmentRecorded

                        match evolve fulfillmentRecorded (Some state) with
                        | Some newState ->
                            Ok
                                { NewState = newState
                                  Events = [ fulfillmentRecorded ] }
                        | None -> Error(DomainError.validation "Failed to evolve FulfillmentRecorded state.")

            | _, _ -> Error(DomainError.validation "Command invalid for current aggregate state.")

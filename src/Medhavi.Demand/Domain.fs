namespace Medhavi.Demand

open System
open Medhavi.SharedKernel
open Medhavi.Common.Validation

/// Customer Order aggregate
type CustomerOrder =
    { OrderId: OrderId
      LineId: string
      SkuId: SkuId
      NodeId: NodeId
      Quantity: Quantity
      DueDate: Timestamp
      Priority: int // E.g., Gold = 1, Silver = 2, Bronze = 3
      IsExpedited: bool }

/// Demand Forecast aggregate
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

/// Demand fulfillment lifecycle
type DemandStatus =
    | Open
    | PartiallyFulfilled
    | Fulfilled
    | Cancelled
    | OnHold

type DemandLine =
    { DemandLineId: string
      DemandOrderId: string
      SkuId: SkuId
      StockingPointId: StockingPointId
      CustomerId: string
      Quantity: Quantity
      UnitOfMeasure: string
      // --- APS date fields ---
      OrderDate: DateTimeOffset
      EarliestDeliveryDate: DateTimeOffset option // customer's earliest acceptable date
      RequestedDeliveryDate: DateTimeOffset // primary target / projection bucket key
      LatestDeliveryDate: DateTimeOffset option // hard upper bound; after this = penalty
      ConfirmedDeliveryDate: DateTimeOffset option // APS promise result from planning
      ActualDeliveryDate: DateTimeOffset option // execution reality (for KPI retrospective)
      // --- Classification ---
      Priority: int // 1 = highest (drives SLA tier, optimizer priority)
      DemandCategory: DemandCategory
      IsFirm: bool // firm demand: APS cannot defer or cancel
      IsFrozen: bool // frozen period: can adjust qty but not cancel
      // --- Fulfillment state ---
      OpenQuantity: Quantity
      FulfilledQuantity: Quantity
      Status: DemandStatus }

type DemandLineDefineReq =
    { DemandLineId: string
      DemandOrderId: string
      SkuId: string
      StockingPointId: string
      CustomerId: string
      Quantity: decimal
      UnitOfMeasure: string
      OrderDate: DateTimeOffset
      EarliestDeliveryDate: DateTimeOffset option
      RequestedDeliveryDate: DateTimeOffset
      LatestDeliveryDate: DateTimeOffset option
      ConfirmedDeliveryDate: DateTimeOffset option
      ActualDeliveryDate: DateTimeOffset option
      Priority: int
      DemandCategory: DemandCategory
      IsFirm: bool
      IsFrozen: bool }

type FulfillDemandLineReq =
    { DemandLineId: string
      Quantity: decimal }

module Domain =

    module DemandLineAgg =

        open Medhavi.SharedKernel.Aggregate

        type DefineDemandLineCmd =
            { DemandLineId: string
              DemandOrderId: string
              SkuId: SkuId
              StockingPointId: StockingPointId
              CustomerId: string
              Quantity: Quantity
              UnitOfMeasure: string
              OrderDate: DateTimeOffset
              EarliestDeliveryDate: DateTimeOffset option
              RequestedDeliveryDate: DateTimeOffset
              LatestDeliveryDate: DateTimeOffset option
              ConfirmedDeliveryDate: DateTimeOffset option
              ActualDeliveryDate: DateTimeOffset option
              Priority: int
              DemandCategory: DemandCategory
              IsFirm: bool
              IsFrozen: bool }

        type FulfillDemandLineCmd =
            { DemandLineId: string
              Quantity: Quantity }

        type DemandLineCommand =
            | Create of DefineDemandLineCmd
            | Fulfill of FulfillDemandLineCmd

        type DemandLineCreatedEvt = DemandLine

        type DemandLineFulfilledEvt =
            { DemandLineId: string
              Quantity: Quantity }

        type DemandLineEvent =
            | DemandLineCreated of DemandLineCreatedEvt
            | DemandLineFulfilled of DemandLineFulfilledEvt

        type DecideDemandLine = Decide<DemandLine, DemandLineCommand, DemandLineEvent>
        type EvolveDemandLine = Evolve<DemandLine, DemandLineEvent>

        let validateAndDefineDemandLine now (cmd: DefineDemandLineCmd) : Validation<DemandLine, DomainError> =
            let makeDemandLine now (cmd: DefineDemandLineCmd) =
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
                  ConfirmedDeliveryDate = cmd.ConfirmedDeliveryDate
                  ActualDeliveryDate = cmd.ActualDeliveryDate
                  Priority = cmd.Priority
                  DemandCategory = cmd.DemandCategory
                  IsFirm = cmd.IsFirm
                  IsFrozen = cmd.IsFrozen
                  OpenQuantity = cmd.Quantity
                  FulfilledQuantity = Quantity.Zero
                  Status = DemandStatus.Open }

            Valid(makeDemandLine now cmd)

        let applyFulfilled (state: DemandLine) (evt: DemandLineFulfilledEvt) : DemandLine =
            let newFulfilled = state.FulfilledQuantity + evt.Quantity
            let finalOpen = state.OpenQuantity - evt.Quantity

            let finalStatus =
                if finalOpen.IsZero then
                    DemandStatus.Fulfilled
                else
                    DemandStatus.PartiallyFulfilled

            { state with
                OpenQuantity = finalOpen
                FulfilledQuantity = newFulfilled
                Status = finalStatus }

        let decide: DecideDemandLine =
            fun command stateOpt ->
                match command, stateOpt with
                | Create cmd, None ->
                    createAggregate (validateAndDefineDemandLine Timestamp.now) (fun dl -> [ DemandLineCreated dl ]) cmd
                | Fulfill cmd, Some state ->
                    let newFulfilled = state.FulfilledQuantity + cmd.Quantity
                    let finalOpen = state.OpenQuantity - cmd.Quantity

                    let finalStatus =
                        if finalOpen.IsZero then
                            DemandStatus.Fulfilled
                        else
                            DemandStatus.PartiallyFulfilled

                    Ok
                        { NewState =
                            { state with
                                OpenQuantity = finalOpen
                                FulfilledQuantity = newFulfilled
                                Status = finalStatus }
                          Events =
                            [ DemandLineFulfilled
                                  { DemandLineId = cmd.DemandLineId
                                    Quantity = cmd.Quantity } ] }
                | _, _ -> Error(DomainError.validation "Not Implemented or state mismatch")

        let applyCreated (evt: DemandLineCreatedEvt) : DemandLine = evt

        let evolve (event: DemandLineEvent) (state: DemandLine option) : DemandLine option =
            match event with
            | DemandLineCreated e -> Some(applyCreated e)
            | DemandLineFulfilled e -> state |> Option.map (fun s -> applyFulfilled s e)

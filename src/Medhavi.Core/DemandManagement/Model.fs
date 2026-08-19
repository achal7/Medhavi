module Medhavi.Core.DemandManagement.Model

open Medhavi.SemanticModel

// ---------------------------------------------------------------------
// Commands
// ---------------------------------------------------------------------

type RecordDemandCmd =
    { DemandId: DemandId
      Item: ItemId
      Location: LocationId
      Customer: CustomerId option
      Quantity: Quantity
      NeedWindow: NeedWindow
      DemandOrigin: DemandOrigin
      ParentDemand: DemandId option }

type SatisfyDemandCmd =
    { DemandId: DemandId
      SatisfactionTime: Timestamp }

type CancelDemandCmd =
    { DemandId: DemandId
      CancellationTime: Timestamp
      Reason: string }

type DemandCmd =
    | Record of RecordDemandCmd
    | Satisfy of SatisfyDemandCmd
    | Cancel of CancelDemandCmd

// ---------------------------------------------------------------------
// Events
// ---------------------------------------------------------------------

type DemandEvent =
    | DemandRecorded of Demand
    | DemandSatisfied of DemandId * SatisfactionTime: Timestamp
    | DemandCancelled of DemandId * CancellationTime: Timestamp * Reason: string

// ---------------------------------------------------------------------
// Pure Evolution (Catamorphism)
// ---------------------------------------------------------------------

let evolve (state: Demand option) (event: DemandEvent) : Demand option =
    match event with
    | DemandRecorded demand -> Some demand
    | DemandSatisfied(demandId, _) ->
        state |> Option.map(fun d -> if d.Id = demandId then { d with LifecycleState = Satisfied } else d)
    | DemandCancelled(demandId, _, _) ->
        state |> Option.map(fun d -> if d.Id = demandId then { d with LifecycleState = Cancelled } else d)

let replay (events: DemandEvent seq) : Demand option = Seq.fold evolve None events

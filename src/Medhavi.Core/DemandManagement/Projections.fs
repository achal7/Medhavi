module Medhavi.Core.DemandManagement.Projections

open Medhavi.SemanticModel
open Medhavi.Contracts.Core.Demand
open Model

let mapToDto (d: Demand) : DemandDto =
    { Id = DemandId.value d.Id
      Item = ItemId.value d.Item
      Location = LocationId.value d.Location
      Customer = d.Customer |> Option.map CustomerId.value
      Quantity = Quantity.value d.Quantity
      NeedWindowLatest = Timestamp.value d.NeedWindow.LatestAcceptable
      NeedWindowEarliest = d.NeedWindow.EarliestAcceptable |> Option.map Timestamp.value
      NeedWindowPreferred = d.NeedWindow.Preferred |> Option.map Timestamp.value
      DemandOrigin = sprintf "%A" d.DemandOrigin
      ParentDemand = d.ParentDemand |> Option.map DemandId.value
      LifecycleState = sprintf "%A" d.LifecycleState }

type State = Map<DemandId, DemandDto>
let initial: State = Map.empty

let apply (state: State) (event: DemandEvent) : State =
    match event with
    | DemandRecorded d -> state |> Map.add d.Id (mapToDto d)
    | DemandSatisfied(demandId, _) ->
        state
        |> Map.change
            demandId
            (Option.map(fun dto ->
                { dto with
                    LifecycleState = "Satisfied" }))
    | DemandCancelled(demandId, _, _) ->
        state
        |> Map.change
            demandId
            (Option.map(fun dto ->
                { dto with
                    LifecycleState = "Cancelled" }))

let seedFromAggregates (aggregates: Demand list) : State =
    aggregates |> List.fold (fun s a -> Map.add a.Id (mapToDto a) s) initial

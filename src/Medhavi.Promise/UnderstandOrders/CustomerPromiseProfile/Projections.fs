/// CA-P-001 Customer Promise Profile Projections
module Medhavi.Promise.UnderstandOrders.CustomerPromiseProfile.Projections

open Medhavi.SemanticModel
open Medhavi.Contracts.Promise
open Medhavi.Promise
open Model

let mapToDto (profile: CustomerPromiseProfile) : CustomerPromiseProfileDto =
    { CustomerId = CustomerId.value profile.CustomerId
      CustomerTier = profile.CustomerTier
      CreditLimit = profile.CreditLimit
      CreditHoldActive = profile.CreditHoldActive
      DefaultSlaDays = profile.DefaultSlaDays
      AllowSplitShipments = profile.AllowSplitShipments
      MaxSplitShipmentPackages = profile.MaxSplitShipmentPackages
      AllowProductSubstitution = profile.AllowProductSubstitution
      SubstitutionMarginFloorPct = profile.SubstitutionMarginFloorPct
      DailyPenaltyRatePct = profile.DailyPenaltyRatePct
      MaxPenaltyCapPct = profile.MaxPenaltyCapPct
      EffectiveFrom = Timestamp.value profile.EffectiveFrom
      EffectiveTo = profile.EffectiveTo |> Option.map Timestamp.value
      LifecycleState =
        match profile.LifecycleState with
        | Active -> "Active"
        | Suspended -> "Suspended"
        | Deprecated -> "Deprecated"
      DecisionTraceability = profile.DecisionTraceability }

type State = Map<CustomerId, CustomerPromiseProfileDto>

let initial: State = Map.empty

let apply (state: State) (event: CustomerProfileEvent) : State =
    match event with
    | CustomerPromiseProfileConfigured profile ->
        state |> Map.add profile.CustomerId (mapToDto profile)

    | CustomerServiceTermsUpdated(custId, _, _, traceId) ->
        state
        |> Map.change custId (Option.map (fun dto ->
            { dto with DecisionTraceability = Some traceId }))

let seedFromAggregates (aggregates: CustomerPromiseProfile list) : State =
    aggregates
    |> List.fold (fun state agg -> Map.add agg.CustomerId (mapToDto agg) state) initial


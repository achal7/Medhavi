/// CA-P-001 Customer Promise Profile Model
/// Traces to: SE-P-050 Customer Promise Profile
module Medhavi.Promise.UnderstandOrders.CustomerPromiseProfile.Model

open System
open Medhavi.SemanticModel
open Medhavi.Promise

type ProfileLifecycleState =
    | Active
    | Suspended
    | Deprecated

/// SE-P-050 Customer Promise Profile Aggregate Root
type CustomerPromiseProfile =
    { CustomerId: CustomerId
      CustomerTier: string
      CreditLimit: decimal
      CreditHoldActive: bool
      DefaultSlaDays: int
      AllowSplitShipments: bool
      MaxSplitShipmentPackages: int
      AllowProductSubstitution: bool
      SubstitutionMarginFloorPct: decimal
      DailyPenaltyRatePct: decimal
      MaxPenaltyCapPct: decimal
      EffectiveFrom: Timestamp
      EffectiveTo: Timestamp option
      LifecycleState: ProfileLifecycleState
      DecisionTraceability: string option }

/// AB-P-017 Configure Customer Promise Profile Command payload
type ConfigureCustomerPromiseProfileCmd =
    { CustomerId: CustomerId
      CustomerTier: string
      CreditLimit: decimal
      CreditHoldActive: bool
      DefaultSlaDays: int
      AllowSplitShipments: bool
      MaxSplitShipmentPackages: int
      AllowProductSubstitution: bool
      SubstitutionMarginFloorPct: decimal
      DailyPenaltyRatePct: decimal
      MaxPenaltyCapPct: decimal
      EffectiveFrom: Timestamp
      EffectiveTo: Timestamp option }

/// AB-P-018 Update Customer Service Terms Command payload
type UpdateCustomerServiceTermsCmd =
    { CustomerId: CustomerId
      DefaultSlaDays: int option
      AllowSplitShipments: bool option
      AllowProductSubstitution: bool option
      DailyPenaltyRatePct: decimal option
      CreditHoldActive: bool option
      UpdateTime: Timestamp }

type CustomerProfileCmd =
    | Configure of ConfigureCustomerPromiseProfileCmd
    | UpdateTerms of UpdateCustomerServiceTermsCmd

    static member GetId(cmd: CustomerProfileCmd) =
        match cmd with
        | Configure c -> c.CustomerId
        | UpdateTerms c -> c.CustomerId

/// Domain Events emitted by Customer Promise Profile
type CustomerProfileEvent =
    | CustomerPromiseProfileConfigured of CustomerPromiseProfile
    | CustomerServiceTermsUpdated of CustomerId: CustomerId * UpdatedTermsSummary: string * UpdateTime: Timestamp * TraceId: string

/// Pure state evolution (catamorphism)
let evolve: Medhavi.Foundation.Contracts.Evolve<CustomerPromiseProfile, CustomerProfileEvent> =
    fun (state: CustomerPromiseProfile option) (event: CustomerProfileEvent) ->
        match event with
        | CustomerPromiseProfileConfigured profile -> Some profile

        | CustomerServiceTermsUpdated(custId, summary, _, traceId) ->
            state
            |> Option.map(fun p ->
                if p.CustomerId = custId then
                    { p with DecisionTraceability = Some traceId }
                else
                    p)


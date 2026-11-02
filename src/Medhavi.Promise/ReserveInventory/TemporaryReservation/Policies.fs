/// CA-P-003 Reserve Inventory and Capacity Temporarily Policies
/// Traces to: PO-P-007
module Medhavi.Promise.ReserveInventory.TemporaryReservation.Policies

type TemporaryReservationPolicy =
    { DefaultLeaseDurationMinutes: int
      MaxLeaseDurationMinutes: int
      AllowExtension: bool
      MaxExtensions: int
      EnforceCreditCheckBeforeHold: bool }

let defaultTemporaryReservationPolicy: TemporaryReservationPolicy =
    { DefaultLeaseDurationMinutes = 15
      MaxLeaseDurationMinutes = 60
      AllowExtension = true
      MaxExtensions = 2
      EnforceCreditCheckBeforeHold = true }


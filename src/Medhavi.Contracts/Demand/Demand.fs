namespace Medhavi.Contracts.Demand

open System

/// Represents a single historical demand fact
/// Traces to: SE‑D‑002, AB‑D‑003 input contract
type DemandDataPoint =
    { ItemId: string
      LocationId: string
      Quantity: decimal
      BusinessTime: DateTimeOffset }

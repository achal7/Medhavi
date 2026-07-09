module Medhavi.Demand.Domain.ObservationRules

open System
open Medhavi.SharedKernel.Failure
open Medhavi.Demand

/// BR-D-010 — Signal Timeliness Rule
let signalTimeliness (signal: DemandSignal) (maxLatency: TimeSpan) =
    if DateTimeOffset.UtcNow - signal.Timestamp > maxLatency then
        Error(DomainError.validation $"Signal timestamp exceeds max latency of {maxLatency.TotalMinutes}min")
    else
        Ok()

/// BR-D-011 — Signal Quantity Bound Rule
let signalRange (signal: DemandSignal) =
    if abs(signal.Value - signal.RecentBaseline) > signal.StatisticalBound then
        Error(DomainError.validation $"Signal value {signal.Value} outside {signal.StatisticalBound} bound")
    else
        Ok()

/// BR-D-012 — Signal Source Reliability Rule
let signalSourceReliability (signal: DemandSignal) (minReliability: float) =
    if signal.SourceReliability < minReliability then
        Error(DomainError.validation $"Source reliability {signal.SourceReliability}%% below {minReliability}%%")
    else
        Ok()

/// BR-D-014 — Observation must be evaluated only once from Received
let evaluateOnlyFromReceived (status: ObservationStatus) =
    if status <> ObservationStatus.Received then
        Error(DomainError.validation "Observation can only be evaluated from Received status")
    else
        Ok()

/// BR-D-016 — Decision must produce exactly one outcome (enforced by design)

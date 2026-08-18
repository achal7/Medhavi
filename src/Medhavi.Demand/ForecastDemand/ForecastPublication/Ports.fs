module Medhavi.Demand.ForecastDemand.Ports

open System
open System.Threading.Tasks
open Medhavi.Core
open Medhavi.Demand
open Medhavi.Demand.ForecastDemand.ForecastPublication.Model

// =============================================================================
// DemandDataPoint – value object for historical demand facts used by ForecastDemand
// Traces to: BA‑D‑002 input contract (SE‑C‑013 Demand, SE‑C‑023 Quantity)
// =============================================================================

// Historical demand series for model training
type GetHistoricalDemandForSeriesPort =
    PlanningScopeId -> DateTimeOffset -> DateTimeOffset -> Task<Map<string, DemandDataPoint list>>

// Retrieve current champion model identifier (governed by PO‑D‑017)
type GetChampionModelPort = unit -> Task<string>

// Forecast publication repository (read side) to load existing publications
type LoadForecastPublicationPort = ForecastPublicationId -> Task<ForecastPublication option>

type GetModelConfidencePort = unit -> Task<decimal>
type GetSignalQualityPort = PlanningScopeId -> Task<decimal>

type ForecastDemandPorts =
    { GetHistoricalDemand: GetHistoricalDemandForSeriesPort
      GetChampionModel: GetChampionModelPort
      GetModelConfidence: GetModelConfidencePort
      GetSignalQuality: GetSignalQualityPort
      GetTotalSeriesCount: GetTotalSeriesCountPort }

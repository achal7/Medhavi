namespace Medhavi.Demand

open System.Threading.Tasks
open Medhavi.SemanticModel

type ItemExistsPort = ItemId -> Task<bool>
type LocationExistsPort = LocationId -> Task<bool>
type CustomerExistsPort = CustomerId -> Task<bool>
type SourceReliabilityPort = string -> Task<decimal>
type GetTotalSeriesCountPort = PlanningScopeId -> Task<int>
type GetRevenueContributionPort = string -> string -> Task<decimal option>
type GetStrategicImportancePort = string -> string -> Task<decimal option>
type GetRiskExposurePort = string -> string -> Task<decimal option>
type GetContractualObligationPort = string -> string -> Task<decimal option>
type GetExpectedDataCountPort = string -> Timestamp -> Timestamp -> Task<int>
type GetHistoricalDemandDataPort = PlanningScopeId -> int -> Task<Medhavi.Contracts.Demand.DemandDataPoint list>

/// SE-C-013 demand fact referenced by a Published Enterprise Picture version (BR-D-400).
type DemandFact =
    { DemandId: DemandId
      Item: ItemId
      Location: LocationId
      Quantity: Quantity
      NeedWindow: NeedWindow }

/// The demand facts of the latest Published Enterprise Picture for a Planning Scope.
type PictureFacts =
    { PictureVersion: int
      DemandFacts: DemandFact list }

type GetPictureDemandFactsPort = PlanningScopeId -> Task<PictureFacts>
type PlanningScopeExistsPort = PlanningScopeId -> Task<bool>
type GetBaselinePort = ItemId -> LocationId -> Task<(decimal * decimal) option>
type IsHighPriorityPort = ItemId -> Task<bool>
type GetAnalogItemPort = ItemId -> Task<ItemId option>
type IsScenarioAdjustmentActivePort = ScenarioAdjustmentId -> Task<bool>

/// Clean Demand Domain Query Ports (Zero Notification Ports; all events dispatched via Envelope Store)
type DemandPorts =
    { ItemExists: ItemExistsPort
      LocationExists: LocationExistsPort
      CustomerExists: CustomerExistsPort
      SourceReliability: SourceReliabilityPort
      GetHistoricalDemandData: GetHistoricalDemandDataPort
      GetTotalSeriesCount: GetTotalSeriesCountPort
      GetRevenueContribution: GetRevenueContributionPort
      GetStrategicImportance: GetStrategicImportancePort
      GetRiskExposure: GetRiskExposurePort
      GetContractualObligation: GetContractualObligationPort
      GetExpectedDataCount: GetExpectedDataCountPort
      PlanningScopeExists: PlanningScopeExistsPort
      GetPictureDemandFacts: GetPictureDemandFactsPort
      GetBaseline: GetBaselinePort
      IsHighPriority: IsHighPriorityPort
      GetAnalogItem: GetAnalogItemPort
      IsScenarioAdjustmentActive: IsScenarioAdjustmentActivePort }

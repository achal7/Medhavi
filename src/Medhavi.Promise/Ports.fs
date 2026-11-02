namespace Medhavi.Promise

open System
open System.Threading.Tasks
open Medhavi.SemanticModel

type ItemExistsPort = ItemId -> Task<bool>
type LocationExistsPort = LocationId -> Task<bool>
type CustomerExistsPort = CustomerId -> Task<bool>
type PlanningScopeExistsPort = PlanningScopeId -> Task<bool>
type PhysicalResourceExistsPort = PhysicalResourceId -> Task<bool>
type SourceReliabilityPort = string -> Task<decimal>

/// Customer commercial profile fact queried from Core/Customer domain
type CustomerProfileFact =
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
      MaxPenaltyCapPct: decimal }

/// Network inventory availability fact across distribution centers
type NetworkInventoryFact =
    { Location: LocationId
      Item: ItemId
      AvailableQuantity: Quantity
      BatchOrLotNumber: string option }

/// Scheduled inbound replenishment purchase order or transfer
type InboundReplenishmentFact =
    { SupplyId: string
      Item: ItemId
      DestinationLocation: LocationId
      ReplenishmentQuantity: Quantity
      ExpectedArrivalDate: Timestamp }

/// Work-center capacity slot fact for manufacturing CTP evaluation
type CapacitySlotFact =
    { Resource: PhysicalResourceId
      ShiftDate: Timestamp
      TotalPlannedCapacityHours: decimal
      AlreadyReservedHours: decimal
      AvailableCapacityHours: decimal }

/// Bill of Materials component fact for CTP assembly explosion
type BomComponentFact =
    { ParentItem: ItemId
      ComponentItem: ItemId
      UsageQuantity: decimal
      ScrapFactor: decimal
      LeadTimeDays: int }

/// Certified transportation lane fact with transit lead time, tariffs, and carbon emissions
type TransportationLaneFact =
    { Origin: LocationId
      Destination: LocationId
      FulfillmentMode: FulfillmentMode
      TransitLeadTimeDays: int
      FreightCostPerUnit: decimal
      CarbonEmissionsKgPerUnit: decimal }

/// Item cost and margin baselines
type ItemCostFact =
    { Item: ItemId
      StandardCogs: decimal
      ListPrice: decimal }

/// Supply disruption signal fact streamed from Nexus/Supply
type SupplyDisruptionSignalFact =
    { SignalId: string
      DisruptionType: PromiseDisruptionType
      AffectedItem: ItemId option
      AffectedLocation: LocationId option
      EstimatedDelayDays: decimal
      Severity: PromiseRiskTier
      DetectedAt: Timestamp }

type GetCustomerProfileFactPort = CustomerId -> Task<CustomerProfileFact option>
type GetNetworkInventoryFactsPort = ItemId -> Task<NetworkInventoryFact list>
type GetInboundReplenishmentFactsPort = ItemId -> LocationId -> Task<InboundReplenishmentFact list>
type GetCapacitySlotFactsPort = PhysicalResourceId -> Timestamp -> Timestamp -> Task<CapacitySlotFact list>
type GetBomComponentFactsPort = ItemId -> Task<BomComponentFact list>
type GetTransportationLaneFactPort = LocationId -> LocationId -> Task<TransportationLaneFact option>
type GetItemCostFactPort = ItemId -> Task<ItemCostFact option>
type GetActiveDisruptionSignalsPort = PlanningScopeId -> Task<SupplyDisruptionSignalFact list>

/// Aggregated functional ports for Promise Intelligence
type PromisePorts =
    { ItemExists: ItemExistsPort
      LocationExists: LocationExistsPort
      CustomerExists: CustomerExistsPort
      PlanningScopeExists: PlanningScopeExistsPort
      PhysicalResourceExists: PhysicalResourceExistsPort
      SourceReliability: SourceReliabilityPort
      GetCustomerProfileFact: GetCustomerProfileFactPort
      GetNetworkInventoryFacts: GetNetworkInventoryFactsPort
      GetInboundReplenishmentFacts: GetInboundReplenishmentFactsPort
      GetCapacitySlotFacts: GetCapacitySlotFactsPort
      GetBomComponentFacts: GetBomComponentFactsPort
      GetTransportationLaneFact: GetTransportationLaneFactPort
      GetItemCostFact: GetItemCostFactPort
      GetActiveDisruptionSignals: GetActiveDisruptionSignalsPort }


namespace Medhavi.Contracts.Promise

open System
open System.Threading.Tasks
open Medhavi.Contracts

/// SE-P-050 Customer Promise Profile Data Transfer Object
type CustomerPromiseProfileDto =
    { CustomerId: string
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
      EffectiveFrom: DateTimeOffset
      EffectiveTo: DateTimeOffset option
      LifecycleState: string
      DecisionTraceability: string option }

/// External request to configure a customer promise profile
type ConfigureCustomerPromiseProfileReq =
    { CustomerId: string
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
      EffectiveFrom: DateTimeOffset
      EffectiveTo: DateTimeOffset option }

/// External request to update customer service terms
type UpdateCustomerServiceTermsReq =
    { CustomerId: string
      DefaultSlaDays: int option
      AllowSplitShipments: bool option
      AllowProductSubstitution: bool option
      DailyPenaltyRatePct: decimal option
      CreditHoldActive: bool option
      UpdateTime: DateTimeOffset }

// ---------- Business Notifications ----------

/// BN-P-012: Customer Profile Updated Notification
type CustomerProfileUpdatedNotification =
    { CustomerId: string
      CustomerTier: string
      CreditHoldActive: bool
      UpdatedAttributes: string list
      NotificationTime: DateTimeOffset }

/// BN-P-013: Customer SLA Penalty Accrual Warning
type CustomerSlaPenaltyWarningNotification =
    { CustomerId: string
      OrderLineId: string
      ProjectedLateDays: int
      AccruedPenaltyAmount: decimal
      NotificationTime: DateTimeOffset }

/// Public API for Customer Promise Profile (CA-P-001 / SE-P-050)
type CustomerPromiseProfileApi =
    { Configure: ConfigureCustomerPromiseProfileReq -> Task<Result<CustomerPromiseProfileDto, ApiError>>
      UpdateTerms: UpdateCustomerServiceTermsReq -> Task<Result<CustomerPromiseProfileDto, ApiError>> }

/// Query service alias for Customer Promise Profiles
type CustomerPromiseProfileQueries = QueryService<CustomerPromiseProfileDto, string>


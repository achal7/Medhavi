/// CA-P-001 Ingest and Understand Orders Policies
module Medhavi.Promise.UnderstandOrders.OrderBookSnapshot.Policies

open System

/// PO-P-001 Order Ingestion Policy
type OrderIngestionPolicy =
    { MaxLinesPerSnapshot: int
      DuplicateRequestDetectionWindow: TimeSpan
      AllowedChannels: string list }

module OrderIngestionPolicy =
    let defaultPolicy =
        { MaxLinesPerSnapshot = 500
          DuplicateRequestDetectionWindow = TimeSpan.FromHours 24.0
          AllowedChannels = [ "EDI"; "B2B_PORTAL"; "CRM"; "ERP_SAP"; "ECOMMERCE" ] }

/// PO-P-002 Order Validation Policy
type OrderValidationPolicy =
    { MaxAdvanceOrderWindowDays: int
      MinOrderQuantity: decimal
      MaxOrderQuantity: decimal
      EnforceFutureDeliveryDate: bool }

module OrderValidationPolicy =
    let defaultPolicy =
        { MaxAdvanceOrderWindowDays = 365
          MinOrderQuantity = 1.0m
          MaxOrderQuantity = 1000000.0m
          EnforceFutureDeliveryDate = true }

/// PO-P-003 Order Lifecycle & Cancellation Policy
type OrderLifecyclePolicy =
    { CancellationTimeFenceHours: int
      AllowPartialCancellation: bool }

module OrderLifecyclePolicy =
    let defaultPolicy =
        { CancellationTimeFenceHours = 24
          AllowPartialCancellation = true }


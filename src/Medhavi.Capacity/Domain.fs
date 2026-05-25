namespace Medhavi.Capacity

open System
open Medhavi.SharedKernel
open Medhavi.MasterData

/// Capacity Bucket representing resource availability for a specific time period
type CapacityBucket =
    { ResourceId: string
      Period: Timestamp
      TotalHours: float
      AllocatedHours: float }

/// Resource Capacity Allocation record
type CapacityAllocation =
    { AllocationId: string
      ResourceId: string
      Period: Timestamp
      AllocatedHours: float
      OrderId: OrderId option }

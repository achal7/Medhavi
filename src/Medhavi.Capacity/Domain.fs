namespace Medhavi.Capacity

open System
open Medhavi.SharedKernel
open Medhavi.MasterData

/// Capacity Bucket representing resource availability for a specific time period
type CapacityBucket = {
    ResourceId: string
    Period: Period
    TotalHours: float
    AllocatedHours: float
}

/// Resource Capacity Allocation record
type CapacityAllocation = {
    AllocationId: string
    ResourceId: string
    Period: Period
    AllocatedHours: float
    OrderId: OrderId option
}

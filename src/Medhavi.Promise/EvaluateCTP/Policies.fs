/// CA-P-005 Evaluate Capable-to-Promise Policies
/// Traces to: PO-P-008
module Medhavi.Promise.EvaluateCTP.Policies

type CapableToPromisePolicy =
    { MaxBomDepth: int
      CapacityBufferPct: decimal
      MinShiftHoursPerJob: decimal
      StandardAssemblyHoursPerUnit: decimal }

let defaultCTPPolicy: CapableToPromisePolicy =
    { MaxBomDepth = 5
      CapacityBufferPct = 10.0m
      MinShiftHoursPerJob = 2.0m
      StandardAssemblyHoursPerUnit = 0.50m }


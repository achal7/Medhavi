module Medhavi.Demand.Tests.Edp.Builders

open Medhavi.Demand.EnterpriseDemandPicture.Model
open Medhavi.Demand.Tests.Builders
open Medhavi.SharedKernel

let defaultEdp: EnterpriseDemandPicture =
    { PlanningScopeId = scopeId "SCOPE-001"
      Version = 1
      Status = AwaitingPlanningDemandCalculation
      OperationalDemand = Map.ofList [ Medhavi.Contracts.PlanningWeek(2027, 27), quantity 100m ]
      PlanningDemand = Map.empty
      TransactionTime = Timestamp.now
      PublicationTime = None
      SupersededVersionId = None }

let withStatus s edp = { edp with Status = s }
let withVersion v edp = { edp with Version = v }
let withOperationalDemand m (edp: EnterpriseDemandPicture) = { edp with OperationalDemand = m }
let withPlanningDemand m edp = { edp with PlanningDemand = m }

let defaultReviseCmd: ReviseEdpCmd =
    { PlanningScopeId = scopeId "SCOPE-001"
      Period = Medhavi.Contracts.PlanningWeek(2027, 27)
      Quantity = quantity 50m
      ObservationId = obsId "OBS-001" }

let defaultCalculateCmd: CalculateEdpCmd = { PlanningScopeId = scopeId "SCOPE-001"; Adjustments = Map.empty; Overrides = Map.empty }

let defaultPublishCmd: PublishEdpCmd = { PlanningScopeId = scopeId "SCOPE-001" }

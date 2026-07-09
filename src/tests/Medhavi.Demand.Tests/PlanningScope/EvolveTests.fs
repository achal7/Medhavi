module Medhavi.Demand.Tests.PlanningScope.EvolveTests

open Expecto
open Medhavi.Demand.PlanningScope
open Medhavi.Demand.PlanningScope.Model
open Medhavi.Contracts
open Medhavi.Demand.Tests.Builders
open Medhavi.SharedKernel

let defaultScope: PlanningScope =
    { Id = scopeId "SCOPE-001"
      SkuId = skuId "SKU-001"
      StockingPointId = stockingPointId "SP-001"
      CustomerId = None
      PlanningPeriod = PlanningWeek(2027, 27)
      Status = Model.Active
      TransactionTime = Timestamp.now }

let withStatus status scope = { scope with Status = status }

[<Tests>]
let tests =
    testList
        "PlanningScope Evolve"
        [

          testCase "ScopeDetermined creates new Active scope"
          <| fun _ ->
              let scope = defaultScope
              let event = ScopeDetermined scope
              let result = evolve event None
              Expect.isSome result "State should be created"
              Expect.equal result.Value.Status Model.Active "Status should be Active"
              Expect.equal result.Value.Id scope.Id "Id should match"

          testCase "ScopeArchived transitions to Archived"
          <| fun _ ->
              let state = defaultScope
              let event = ScopeArchived state.Id
              let result = evolve event (Some state)
              Expect.equal result.Value.Status Archived "Status should be Archived" ]

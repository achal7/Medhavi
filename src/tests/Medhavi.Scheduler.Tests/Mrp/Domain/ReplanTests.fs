namespace Medhavi.Scheduler.Tests.Mrp.Domain

open System
open Expecto
open Swensen.Unquote
open Medhavi.SharedKernel
open Medhavi.Scheduler.Mrp.Domain
open Medhavi.Scheduler.Mrp.Domain.Types
open Medhavi.Scheduler.Mrp.Domain.MrpRunAggregate
open Medhavi.Scheduler.Mrp.Domain.Policies
open Medhavi.Scheduler.Mrp.Steps
open Medhavi.Scheduler.Mrp.Steps.SupplyGenerationStep
open Medhavi.Scheduler.Mrp.Application
open Medhavi.Scheduler.Tests.TestCommon

module ReplanTests =

    let t0 = DateTimeOffset.UtcNow
    let tPlusDays d = createTimestamp (t0.AddDays(d))

    // Helper builders mapping to TestCommon
    let mockDemand refId sku node sp due qty =
        { DemandRef.DemandId = refId
          SkuId = sku
          NodeId = node
          StockingPointId = sp
          NeedDate = due
          Quantity = qty }

    let mockSupply refId pType sku node sp del qty =
        { SupplyRef.SupplyId = refId
          ProposalType = pType
          SkuId = sku
          NodeId = node
          StockingPointId = sp
          DeliveryDate = del
          Quantity = qty }

    let mockPegging linkId demand target qty status isLocked =
        { PeggingLink.Id = PeggingId.create linkId |> getOk
          Demand = demand
          Target = target
          PeggedQty = qty
          Status = status
          IsLocked = isLocked
          Created = t0
          Modified = t0 }

    let mockProposal id pType sku node sp due qty routing =
        { SupplyProposal.Id = SupplyProposalId.create id |> getOk
          ProposalType = pType
          SkuId = sku
          NodeId = node
          StockingPointId = sp
          Quantity = qty
          DueDate = due
          StartDate = Some(Timestamp.add due (TimeSpan.FromDays(-2.0)))
          RoutingId = routing
          SupplierId = None
          Priority = 5
          IsExpedite = false
          Status = Planned
          PeggingRefs = []
          CapacityCheckedDate = None
          CreatedAt = Timestamp.now }

    let mockComponentLookup (sku: SkuId) =
        if sku = skuFG then
            [ (skuRM, createQty 1.0m) ]
        else
            []

    let buildBaseline () =
        let demandBike =
            mockDemand "D-BIKE-1" skuFG nodeWarehouse spWarehouse (tPlusDays 10.0) (createQty 10.0m)

        let demandFrame =
            mockDemand "comp-D-FRAME-1" skuRM nodeFactory spFactory (tPlusDays 8.0) (createQty 10.0m)

        let supplyBike =
            mockSupply "WO-BIKE-1" PlannedWorkOrder skuFG nodeWarehouse spWarehouse (tPlusDays 10.0) (createQty 10.0m)

        let supplyFrame =
            mockSupply "WO-FRAME-1" PlannedWorkOrder skuRM nodeFactory spFactory (tPlusDays 8.0) (createQty 10.0m)

        let pegBike =
            mockPegging "PEG-1" demandBike (Supply supplyBike) (createQty 10.0m) Active false

        let pegFrame =
            mockPegging "PEG-2" demandFrame (Supply supplyFrame) (createQty 10.0m) Active false

        let proposalBike =
            mockProposal
                "WO-BIKE-1"
                PlannedWorkOrder
                skuFG
                nodeWarehouse
                spWarehouse
                (tPlusDays 10.0)
                (createQty 10.0m)
                (Some(RoutingId.create "ROUTING-BIKE" |> getOk))

        let proposalFrame =
            mockProposal
                "WO-FRAME-1"
                PlannedWorkOrder
                skuRM
                nodeFactory
                spFactory
                (tPlusDays 8.0)
                (createQty 10.0m)
                (Some(RoutingId.create "ROUTING-FRAME" |> getOk))

        { RunId = MrpRunId.create "RUN-BASE-1" |> getOk
          StartTime = createTimestamp t0
          EndTime = tPlusDays 30.0
          Status = MrpRunStatus.Completed
          BomExplosionCount = 2
          NetRequirements = []
          Proposals = [ proposalBike; proposalFrame ]
          ActionMessages = []
          Peggings = [ pegBike; pegFrame ]
          Errors = []
          Warnings = [] }

    [<Tests>]
    let tests =
        testList
            "MRP Domain - Heuristic Replanning (Disruption Handling) Tests"
            [ testCase "Scenario: Blast Radius Evaluation - should evaluate downstream blast radius correctly" (fun () ->
                  let baseline = buildBaseline ()
                  // Break down resource "ROUTING-FRAME" between T+6 and T+9 days (overlaps WO-FRAME-1 which is due at T+8)
                  let event = ResourceBreakdown("ROUTING-FRAME", tPlusDays 6.0, tPlusDays 9.0)

                  let (affectedDemands, affectedProposals) =
                      Replan.ImpactAssessment.evaluateBlastRadius baseline event mockComponentLookup

                  // WO-FRAME-1 is affected. Since BOM maps Frame -> Bike, the parent proposal WO-BIKE-1 and demand D-BIKE-1 should also be affected.
                  test <@ List.contains "WO-FRAME-1" affectedProposals @>

                  test
                      <@
                          List.contains "comp-D-FRAME-1" affectedDemands
                          || List.contains "D-BIKE-1" affectedDemands
                      @>)

              testCase "Scenario: Replan Mode Dispatcher - should determine PlanningMode based on severity" (fun () ->
                  let severityMap =
                      Map.ofList [ "fullReplanDurationHrs", 24.0; "ignoreDurationHrs", 1.0 ]

                  // Disruption duration: 30 mins (0.5 hrs) -> Ignore
                  let eventIgnore =
                      ResourceBreakdown(
                          "ROUTING-FRAME",
                          tPlusDays 1.0,
                          Timestamp.add (tPlusDays 1.0) (TimeSpan.FromMinutes(30.0))
                      )

                  test <@ Replan.ReplanDispatcher.determineMode eventIgnore severityMap = Ignore @>

                  // Disruption duration: 6 hrs -> ReactiveRepair
                  let eventRepair =
                      ResourceBreakdown(
                          "ROUTING-FRAME",
                          tPlusDays 1.0,
                          Timestamp.add (tPlusDays 1.0) (TimeSpan.FromHours(6.0))
                      )

                  test <@ Replan.ReplanDispatcher.determineMode eventRepair severityMap = ReactiveRepair @>

                  // Disruption duration: 36 hrs -> FullReplan
                  let eventFull =
                      ResourceBreakdown(
                          "ROUTING-FRAME",
                          tPlusDays 1.0,
                          Timestamp.add (tPlusDays 1.0) (TimeSpan.FromHours(36.0))
                      )

                  test <@ Replan.ReplanDispatcher.determineMode eventFull severityMap = FullReplan @>)

              testCase "Scenario: KPI Evaluator - should calculate lateness and churn correctly" (fun () ->
                  let baseline = buildBaseline ()
                  let kpis = Replan.KPIEvaluator.evaluate baseline None
                  test <@ kpis.TotalLatenessMinutes = 0.0 @>
                  test <@ kpis.LateOrdersCount = 0 @>

                  let delayedProposal =
                      { baseline.Proposals.[0] with
                          DueDate = tPlusDays 12.0 }

                  let delayedSupply =
                      { (baseline.Peggings.[0].Target
                         |> function
                             | Supply s -> s
                             | _ -> failwith "") with
                          DeliveryDate = tPlusDays 12.0 }

                  let delayedPeg =
                      { baseline.Peggings.[0] with
                          Target = Supply delayedSupply }

                  let delayedRun =
                      { baseline with
                          Proposals = [ delayedProposal; baseline.Proposals.[1] ]
                          Peggings = [ delayedPeg; baseline.Peggings.[1] ] }

                  let lateKpis = Replan.KPIEvaluator.evaluate delayedRun (Some baseline)

                  test
                      <@
                          lateKpis.TotalLatenessMinutes >= 2870.0
                          && lateKpis.TotalLatenessMinutes <= 2890.0
                      @>

                  test <@ lateKpis.LateOrdersCount = 1 @>
                  test <@ lateKpis.ScheduleChurnCount = 1 @>)

              testCase "Scenario: Plan Delta Calculation - should report added, rescheduled, and cancelled proposals" (fun () ->
                  let before = buildBaseline ()

                  let pRescheduled =
                      { before.Proposals.[0] with
                          DueDate = tPlusDays 11.0 }

                  let pAdded =
                      mockProposal
                          "WO-BIKE-2"
                          PlannedWorkOrder
                          skuFG
                          nodeWarehouse
                          spWarehouse
                          (tPlusDays 15.0)
                          (createQty 5.0m)
                          None

                  let after =
                      { before with
                          Proposals = [ pRescheduled; pAdded ]
                          Peggings = [] }

                  let delta = Replan.PlanDeltaCalculator.calculate before after

                  test <@ List.length delta.AddedProposals = 1 @>
                  test <@ delta.AddedProposals.[0].Id = (SupplyProposalId.create "WO-BIKE-2" |> getOk) @>
                  test <@ List.length delta.RescheduledProposals = 1 @>
                  test <@ fst delta.RescheduledProposals.[0] = "WO-BIKE-1" @>
                  test <@ List.contains "WO-FRAME-1" delta.CancelledProposals @>)

              testCaseAsync
                  "Scenario: ReplanService - should execute Reactive Repair successfully"
                  (async {
                      let baseline = buildBaseline ()
                      let event = ResourceBreakdown("ROUTING-FRAME", tPlusDays 6.0, tPlusDays 9.0)

                      let severityMap =
                          Map.ofList [ "fullReplanDurationHrs", 24.0; "ignoreDurationHrs", 1.0 ]

                      let deps =
                          { defaultDeps with
                              BomLookup =
                                  fun sku _ ->
                                      if sku = skuFG then
                                          Some
                                              { BomId = "BOM-BIKE"
                                                ParentSkuId = skuFG
                                                Components =
                                                  [ { ComponentSkuId = skuRM
                                                      QuantityPer = createQty 1.0m
                                                      UnitOfMeasureId = uomPc
                                                      Sequence = 1
                                                      IsPhantom = false } ]
                                                IsActive = true }
                                      else
                                          None
                              ProductTypeQuery =
                                  fun sku ->
                                      task {
                                          if sku = skuFG || sku = skuRM then
                                              return Manufactured
                                          else
                                              return Purchased
                                      }
                              RoutingQuery =
                                  fun sku _ ->
                                      task {
                                          if sku = skuFG then
                                              return Some(RoutingId.create "ROUTING-BIKE" |> getOk)
                                          elif sku = skuRM then
                                              return Some(RoutingId.create "ROUTING-FRAME" |> getOk)
                                          else
                                              return None
                                      } }

                      let! result =
                          ReplanService.executeReplan deps baseline event severityMap
                          |> Async.AwaitTask

                      match result with
                      | Ok runResult -> test <@ List.length runResult.Proposals >= 1 @>
                      | Error err -> failwithf "Reactive Repair failed: %A" err
                  })

              testCaseAsync
                  "Scenario: ReplanService - should execute Incremental Insert successfully"
                  (async {
                      let baseline = buildBaseline ()

                      let newDemand =
                          { MrpDemand.DemandId = "NEW-DEMAND-1"
                            SkuId = skuFG
                            NodeId = nodeWarehouse
                            StockingPointId = spWarehouse
                            Quantity = createQty 5.0m
                            RequiredDate = tPlusDays 15.0
                            Source = CustomerOrder("NEW-ORDER", "1")
                            Priority = Some 2 }

                      let! result =
                          ReplanService.executeIncrementalInsert defaultDeps baseline [ newDemand ]
                          |> Async.AwaitTask

                      match result with
                      | Ok runResult ->
                          test
                              <@
                                  List.exists
                                      (fun (p: SupplyProposal) -> SupplyProposalId.value p.Id = "WO-BIKE-1")
                                      runResult.Proposals
                              @>
                      | Error err -> failwithf "Incremental Insert failed: %A" err
                  }) ]

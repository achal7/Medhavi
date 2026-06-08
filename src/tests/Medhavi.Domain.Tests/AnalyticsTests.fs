namespace Medhavi.Domain.Tests

open System
open Expecto
open Swensen.Unquote
open Medhavi.SharedKernel
open Medhavi.Scenario
open Medhavi.Analytics
open Medhavi.Analytics.PlanningHorizon
open Medhavi.Analytics.KPI

module AnalyticsWiringTests =

    [<Tests>]
    let tests =
        testList "Analytics Bounded Context Tests" [
            testCase "toScenarioOverlay should translate ScenarioDataOverrides into ScenarioOverlay correctly" (fun () ->
                let now = DateTimeOffset.UtcNow
                let overrides = [
                    DemandOverride("DEM-001", 50.0m, "Increase qty")
                    InventoryOverride("SKU-1", "SP-1", 100.0m)
                    CapacityOverride("RES-1", now, 120.0m)
                ]

                let overlay = ScenarioAdapter.toScenarioOverlay "SCEN-TEST" overrides

                test <@ overlay.ScenarioId = "SCEN-TEST" @>
                test <@ List.length overlay.DemandOverrides = 1 @>
                test <@ (List.head overlay.DemandOverrides).DemandLineId = "DEM-001" @>
                test <@ (List.head overlay.DemandOverrides).NewQuantity = Some 50.0m @>

                test <@ List.length overlay.InventoryOverrides = 1 @>
                test <@ (List.head overlay.InventoryOverrides).SkuId = "SKU-1" @>
                test <@ (List.head overlay.InventoryOverrides).OnHandOverride = 100.0m @>

                test <@ List.length overlay.CapacityOverrides = 1 @>
                test <@ (List.head overlay.CapacityOverrides).ResourceGroupId = "RES-1" @>
                test <@ (List.head overlay.CapacityOverrides).AvailableHoursOverride = 120.0m @>
            )

            testCase "OTD and OTIF KPI Formulas should evaluate correctly" (fun () ->
                let period = PlanningPeriod.PlanningDay(DateOnly.FromDateTime(DateTime.UtcNow))
                
                let view =
                    { Period = period
                      PlantId = "PLANT-1"
                      SkuId = None
                      TotalDemandQty = 100m
                      FirmDemandQty = 100m
                      ForecastDemandQty = 0m
                      ConfirmedQty = 80m
                      OpenShortfallQty = 20m
                      DemandLines = [
                          { DemandLineId = "L1"; DemandOrderId = "O1"
                            SkuId = "SKU"; SkuCode = "SKU"; SkuName = "SKU"
                            CustomerId = "C1"; CustomerName = "C1"; Priority = 1; DemandCategory = "CustomerOrder"; IsFirm = true
                            EarliestDeliveryDate = None; RequestedDeliveryDate = DateOnly.FromDateTime(DateTime.UtcNow); LatestDeliveryDate = None; ConfirmedDeliveryDate = Some (DateOnly.FromDateTime(DateTime.UtcNow))
                            RequestedQty = 60m; OpenQty = 60m; FulfilledQty = 0m; ConfirmedQty = 60m; ShortfallQty = 0m
                            LatenessRisk = OnTrack; PeggedSupply = [] }
                          { DemandLineId = "L2"; DemandOrderId = "O2"
                            SkuId = "SKU"; SkuCode = "SKU"; SkuName = "SKU"
                            CustomerId = "C1"; CustomerName = "C1"; Priority = 2; DemandCategory = "CustomerOrder"; IsFirm = true
                            EarliestDeliveryDate = None; RequestedDeliveryDate = DateOnly.FromDateTime(DateTime.UtcNow); LatestDeliveryDate = None; ConfirmedDeliveryDate = Some (DateOnly.FromDateTime(DateTime.UtcNow).AddDays(1))
                            RequestedQty = 40m; OpenQty = 40m; FulfilledQty = 0m; ConfirmedQty = 20m; ShortfallQty = 20m
                            LatenessRisk = AtRisk 1; PeggedSupply = [] }
                      ]
                      EarliestPossibleQty = 100m
                      LatestAcceptableQty = 100m
                      AtRiskDemandCount = 1
                      CriticalDemandCount = 0 }

                let otd = FormulaRegistry.OTD.Calculate view
                let otif = FormulaRegistry.OTIF.Calculate view

                test <@ otd = 50.0m @>
                test <@ otif = 80.0m @>
            )
        ]

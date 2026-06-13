namespace Medhavi.Domain.Tests

open System
open System.Text.Json
open Expecto
open Swensen.Unquote
open Medhavi.SharedKernel
open Medhavi.Scenario

module ScenarioTests =

    [<Tests>]
    let tests =
        testList
            "Scenario Domain Tests"
            [ testCase "should construct Scenario with default active state" (fun () ->
                  let scenario =
                      { ScenarioId = "scen-001"
                        Name = "Base Plan"
                        BaseScenarioId = None
                        Version = 1
                        CreatedAt = DateTimeOffset.UtcNow
                        IsActive = true
                        Overrides = []
                        KpiSummary = None
                        PublishId = None
                        RollbackPackageId = None
                        Status = ScenarioStatus.Draft }

                  test <@ scenario.IsActive = true @>
                  test <@ scenario.Version = 1 @>) ]

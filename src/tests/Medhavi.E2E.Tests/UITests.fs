namespace Medhavi.E2E.Tests

open System
open Expecto
open Swensen.Unquote
open Elmish
open Medhavi.Web
open Medhavi.Web.Pages
open Medhavi.Web.AppShell
open Medhavi.Scenario
open Medhavi.SharedKernel.ScenarioContracts
open Medhavi.Web.DemandWorkbench

module UITests =

    module StateHelpers =
        let initModel : Medhavi.Web.AppShell.Model = Medhavi.Web.AppShell.initModel
        
        let dummyDemandStore =
            { GetSnapshot = fun () -> []
              Refresh = fun () -> System.Threading.Tasks.Task.CompletedTask
              Subscribe = fun _ -> { new IDisposable with member _.Dispose() = () }
              SetScope = fun _ -> System.Threading.Tasks.Task.CompletedTask }
              
        let dummyStores =
            { Demand = dummyDemandStore
              Supply = Unchecked.defaultof<Medhavi.Web.Stores.SupplyStore>
              Capacity = Unchecked.defaultof<Medhavi.Web.Stores.CapacityStore>
              Scenario = Unchecked.defaultof<Medhavi.Web.Stores.ScenarioStore>
              Activity = Unchecked.defaultof<Medhavi.Web.Stores.ActivityStore> }

        let update (msg: Medhavi.Web.AppShell.Message) (model: Medhavi.Web.AppShell.Model) : Medhavi.Web.AppShell.Model * Cmd<Medhavi.Web.AppShell.Message> =
            let dummyPlanning = Unchecked.defaultof<Medhavi.Web.Services.PlanningService.PlanningCommandService>
            let dummyWorkspace = Unchecked.defaultof<Medhavi.Web.Services.WorkspaceContextService>
            Medhavi.Web.AppShell.update dummyStores dummyPlanning dummyWorkspace msg model

    [<Tests>]
    let uiTests =
        testList "UI Elmish State and Page Logic Tests" [
            
            testCase "1. User Role Authorization Cycles and Dashboard Actions" (fun () ->
                let model = StateHelpers.initModel
                
                // Planner should start by default and not be authorized to import
                test <@ model.CurrentUser.IsSome @>
                test <@ model.CurrentUser.Value.Role = Role.Planner @>
                
                let canImport m =
                    match m.CurrentUser with
                    | Some u ->
                        match u.Role with
                        | Role.Supervisor | Role.Manager | Role.Administrator -> true
                        | Role.Planner -> false
                        | _ -> false
                    | None -> false

                test <@ canImport model = false @>

                // Cycle to Supervisor
                let model1, _ = StateHelpers.update CycleUserRole model
                test <@ model1.CurrentUser.Value.Role = Role.Supervisor @>
                test <@ canImport model1 = true @>

                // Cycle to Manager
                let model2, _ = StateHelpers.update CycleUserRole model1
                test <@ model2.CurrentUser.Value.Role = Role.Manager @>
                test <@ canImport model2 = true @>

                // Cycle to Administrator
                let model3, _ = StateHelpers.update CycleUserRole model2
                test <@ model3.CurrentUser.Value.Role = Role.Administrator @>
                test <@ canImport model3 = true @>

                // Cycle back to Planner
                let model4, _ = StateHelpers.update CycleUserRole model3
                test <@ model4.CurrentUser.Value.Role = Role.Planner @>
                test <@ canImport model4 = false @>
            )

            testCase "2. Scenario Comparison Selectors and Overrides Delta Check" (fun () ->
                let model = StateHelpers.initModel
                
                // Let's create mock scenarios to test loading and comparing
                let mockScenarios = [
                    { ScenarioId = "sc-1"
                      Name = "Baseline"
                      BaseScenarioId = None
                      Version = 1
                      CreatedAt = DateTimeOffset.Now
                      IsActive = true
                      Overrides = [] }
                    { ScenarioId = "sc-2"
                      Name = "What-If Capacity Boost"
                      BaseScenarioId = Some "sc-1"
                      Version = 2
                      CreatedAt = DateTimeOffset.Now
                      IsActive = false
                      Overrides = [
                          CapacityOverride ("CNC-01", DateTimeOffset.Now, 150.0m)
                          DemandOverride ("ORD-99", 50.0m, "Priority override")
                      ] }
                ]

                // Load scenarios into ScenarioWorkbench
                let loadedWorkbench = 
                    Pages.ScenarioWorkbench.update (Pages.ScenarioWorkbench.LoadScenarios mockScenarios) model.ScenarioWorkbench
                
                test <@ loadedWorkbench.Scenarios.Length = 2 @>
                test <@ loadedWorkbench.IsLoading = false @>
                test <@ loadedWorkbench.ErrorMessage.IsNone @>

                // Select Active and Compare scenarios
                let step1 = Pages.ScenarioWorkbench.update (Pages.ScenarioWorkbench.SelectActiveScenario (Some "sc-1")) loadedWorkbench
                let step2 = Pages.ScenarioWorkbench.update (Pages.ScenarioWorkbench.SelectCompareScenario (Some "sc-2")) step1

                test <@ step2.ActiveScenarioId = Some "sc-1" @>
                test <@ step2.CompareScenarioId = Some "sc-2" @>

                // Select the same scenario in active and compare
                let step3 = Pages.ScenarioWorkbench.update (Pages.ScenarioWorkbench.SelectCompareScenario (Some "sc-1")) step2
                test <@ step3.ActiveScenarioId = Some "sc-1" @>
                test <@ step3.CompareScenarioId = Some "sc-1" @>
            )

            testCase "3. Demand Workbench Grid virtualized loading and debounced search" (fun () ->
                let model = StateHelpers.initModel
                
                let mockRow =
                    { DemandLineId = "dl-1"
                      DemandOrderId = "ORD-001"
                      SkuId = "SKU-A"
                      StockingPointId = "LOC-1"
                      Quantity = 100.0m
                      UnitOfMeasure = "PCS"
                      OrderDate = DateTimeOffset.Now
                      RequestedDeliveryDate = DateTimeOffset.Now
                      Priority = 1
                      DemandCategory = "Standard"
                      OpenQuantity = 100.0m
                      FulfilledQuantity = 0.0m
                      Status = "Tentative" }

                // Type search text "ORD-001" which schedules a 300ms delayed TriggerSearch message
                let step1, searchCmd = DemandWorkbench.Update.update StateHelpers.dummyDemandStore (SearchTextChanged "ORD-001") model.DemandWorkbench
                
                test <@ step1.PendingSearchText = "ORD-001" @>
                test <@ step1.SearchText = "" @> // SearchText should not update immediately (debounced)
                
                // Trigger the search directly as the debouncer would
                let step2, _ = DemandWorkbench.Update.update StateHelpers.dummyDemandStore (TriggerSearch "ORD-001") step1
                test <@ step2.SearchText = "ORD-001" @>

                // If TriggerSearch is sent with outdated text, it should be ignored
                let step3, _ = DemandWorkbench.Update.update StateHelpers.dummyDemandStore (TriggerSearch "ORD-old") step1
                test <@ step3.SearchText = "" @> // Ignored because PendingSearchText is ORD-001

                // Select a row for details lazy loading
                let step4, detailCmd = DemandWorkbench.Update.update StateHelpers.dummyDemandStore (RowSelected mockRow) step2
                test <@ step4.SelectedDemand = Some mockRow @>
                test <@ step4.IsLoadingDetails = true @>
                test <@ step4.DetailsText.IsNone @>

                // Loaded details should populate
                let step5, _ = DemandWorkbench.Update.update StateHelpers.dummyDemandStore (DetailsLoaded "Details text populated.") step4
                test <@ step5.IsLoadingDetails = false @>
                test <@ step5.DetailsText = Some "Details text populated." @>

                // Dismiss details selection
                let step6, _ = DemandWorkbench.Update.update StateHelpers.dummyDemandStore CloseDetails step5
                test <@ step6.SelectedDemand.IsNone @>
                test <@ step6.IsLoadingDetails = false @>
                test <@ step6.DetailsText.IsNone @>
            )

            testCase "4. Background Operations and Activity Feed Monitor" (fun () ->
                let model = StateHelpers.initModel
                let opId = Guid.NewGuid()
                
                // Start a background MRP operation
                let step1, _ = StateHelpers.update (StartOperation (opId, "MRP Baseline Run")) model
                test <@ step1.ActiveOperations.Length = 1 @>
                test <@ step1.ActiveOperations.[0].Id = opId @>
                test <@ step1.ActiveOperations.[0].Name = "MRP Baseline Run" @>
                
                match step1.ActiveOperations.[0].State with
                | Running (progress, stage) -> 
                    test <@ progress = 0 @>
                    test <@ stage = "Initializing" @>
                | _ -> failwith "Expected Running state"

                // Progress update
                let step2, _ = StateHelpers.update (UpdateOperationProgress (opId, 50, "Netting requirements")) step1
                match step2.ActiveOperations.[0].State with
                | Running (progress, stage) -> 
                    test <@ progress = 50 @>
                    test <@ stage = "Netting requirements" @>
                | _ -> failwith "Expected Running state"

                // Complete operation
                let step3, _ = StateHelpers.update (CompleteOperation opId) step2
                match step3.ActiveOperations.[0].State with
                | Completed () -> ()
                | _ -> failwith "Expected Completed state"

                // Dismiss operation
                let step4, _ = StateHelpers.update (DismissOperation opId) step3
                test <@ step4.ActiveOperations.IsEmpty @>

                // Open/Close Activity Feed drawer
                test <@ model.ActivityFeedOpen = false @>
                let step5, _ = StateHelpers.update ToggleActivityFeed model
                test <@ step5.ActivityFeedOpen = true @>
                let step6, _ = StateHelpers.update ToggleActivityFeed step5
                test <@ step6.ActivityFeedOpen = false @>

                // Prevent memory leaks: limit notifications history to 50 items
                let mutable currentModel = model
                for i in 1 .. 60 do
                    let notif = {
                        Id = Guid.NewGuid()
                        Title = sprintf "Alert %d" i
                        Message = "Test alert payload"
                        Timestamp = DateTime.Now
                        IsRead = false
                    }
                    let updated, _ = StateHelpers.update (ReceiveNotification notif) currentModel
                    currentModel <- updated
                
                // Length must be capped at 50
                test <@ currentModel.Notifications.Length = 50 @>
            )
        ]

namespace Medhavi.Domain.Tests

open System
open Expecto
open Swensen.Unquote
open Medhavi.SharedKernel
open Medhavi.Common
open Medhavi.Common.Validation
open Medhavi.Contracts.Domain
open Medhavi.MasterData.Domain.RoutingAgg
open Medhavi.Capacity

module RoutingTests =
    open Medhavi.MasterData.Domain.RoutingAgg.Commands

    let createId helper creator valStr =
        match creator valStr with
        | Ok id -> id
        | Error err -> failwithf "Failed to create ID: %A" err

    let getSomeId helper creator valStr = createId helper creator valStr

    let getPercent v = Percent.create v |> function Ok p -> p | Error e -> failwithf "Invalid percent %M: %A" v e

    let defaultCommand () : DefineRoutingCmd =
        let rId = getSomeId "RoutingId" RoutingId.create "ROUTING-TEST"
        let now = Timestamp.now

        let timing =
            { FixedLeadTime = None
              QueueTime = None
              WaitTime = None
              MoveTime = None }

        let step1 =
            { StepId = "STEP-10"
              Sequence = 10
              OperationCode = "OP-10"
              Name = "Welding"
              Description = None
              Kind = RoutingStepKind.Standard
              Inputs =
                [ { SkuId = "SKU-RAW"
                    FromNodeId = Some "NODE-PLANT"
                    QuantityPerBaseOutput = Some 1.0m
                    Timing = StepInputTiming.AtStepStart
                    IsConsumed = true
                    IsOptional = false } ]
              Outputs = []
              ResourceRequirements =
                [ { RequirementId = "REQ-10"
                    ResourceKind = RoutingResourceKind.WorkCenter
                    LoadBasis = ResourceLoadBasis.PerUnit
                    RequiredUnits = 1.0m
                    SelectionRule = ResourceSelectionRule.AnyAllowed
                    Options =
                      [ { OptionId = "OPT-10"
                          ResourceGroupId = "RG-LINE1"
                          ResourceId = Some "RES-LINE1-A"
                          WorkCenterId = None
                          Usage = ResourceUsage.Primary
                          Priority = Some 1
                          SetupTime = Some 15.0m
                          RunTimePerBaseQuantity = Some 1.5m
                          TeardownTime = None
                          CoolingTime = None
                          MinLeadTime = None
                          CostPerMinute = Some 2.0m
                          EfficiencyPolicy = None
                          SetupPolicy = SetupPolicy.NoSetup
                          CoolingPolicy = CoolingPolicy.NoCooling
                          EffectiveStart = None
                          EffectiveEnd = None } ] } ]
              TimingProfile = timing
              YieldPolicy = StepYieldPolicy.NoYieldLoss
              ReworkPolicy = ReworkPolicy.NoRework
              OverlapPolicy = StepOverlapPolicy.NoOverlap
              EffectiveStart = None
              EffectiveEnd = None }

        let step2 =
            { StepId = "STEP-20"
              Sequence = 20
              OperationCode = "OP-20"
              Name = "Paint"
              Description = None
              Kind = RoutingStepKind.Standard
              Inputs = []
              Outputs =
                [ { SkuId = "SKU-FINISHED"
                    ToNodeId = Some "NODE-PLANT"
                    QuantityRatioToPrimaryOutput = Some 1.0m
                    Role = RoutingOutputRole.PrimaryOutput
                    Timing = StepOutputTiming.AtStepEnd } ]
              ResourceRequirements =
                [ { RequirementId = "REQ-20"
                    ResourceKind = RoutingResourceKind.WorkCenter
                    LoadBasis = ResourceLoadBasis.PerUnit
                    RequiredUnits = 1.0m
                    SelectionRule = ResourceSelectionRule.AnyAllowed
                    Options =
                      [ { OptionId = "OPT-20"
                          ResourceGroupId = "RG-LINE2"
                          ResourceId = Some "RES-LINE2-A"
                          WorkCenterId = None
                          Usage = ResourceUsage.Primary
                          Priority = Some 1
                          SetupTime = Some 30.0m
                          RunTimePerBaseQuantity = Some 2.0m
                          TeardownTime = None
                          CoolingTime = None
                          MinLeadTime = None
                          CostPerMinute = Some 3.0m
                          EfficiencyPolicy = None
                          SetupPolicy = SetupPolicy.NoSetup
                          CoolingPolicy = CoolingPolicy.NoCooling
                          EffectiveStart = None
                          EffectiveEnd = None } ] } ]
              TimingProfile = timing
              YieldPolicy = StepYieldPolicy.ExpectedYield (getPercent 0.95m)
              ReworkPolicy = ReworkPolicy.NoRework
              OverlapPolicy = StepOverlapPolicy.NoOverlap
              EffectiveStart = None
              EffectiveEnd = None }

        { Id = rId
          Name = "Test Assembly Routing"
          Description = Some "Test description"
          Applicability =
            { StockingPointId = Some "SP-01"
              EffectiveStart = now
              EffectiveEnd = None }
          Priority = 1
          IsPreffered = true
          QuantityRule =
            { MinQuantity = None
              MaxQuantity = None
              LotSize = None
              OrderMultiple = None }
          CostPolicy = DefineRoutingCostPolicy.NoRoutingCost
          Details =
            DefineRoutingDetails.Work
              { ProductId = "SKU-FINISHED"
                PrimaryOutputSkuId = "SKU-FINISHED"
                BaseOutputQuantity = 1.0m
                Steps = [ step1; step2 ] }
          CreatedAt = now
          ModifiedAt = now }

    [<Tests>]
    let tests =
        testList
            "Routing Domain & Interpreter Tests"
            [

              testCase "should define routing with valid input parameters" (fun () ->
                  let cmd = defaultCommand ()
                  let decRes = decide (DefineRouting cmd) None

                  match decRes with
                  | Error err -> failwithf "Failed to define routing: %A" err
                  | Ok dec ->
                      let state = dec.NewState
                      test <@ state.Name = "Test Assembly Routing" @>
                      match state.Details with
                      | RoutingDetails.Work work ->
                          test <@ work.Steps.Length = 2 @>
                      | _ -> failwith "Expected Work details"
                      test <@ state.Preference.Priority = 1 @>)

              testCase "should fail validation if sequence numbers are not positive" (fun () ->
                  let cmd = defaultCommand ()
                  match cmd.Details with
                  | DefineRoutingDetails.Work work ->
                      let steps = work.Steps |> List.map (fun s -> { s with Sequence = 0 })
                      let cmd = { cmd with Details = DefineRoutingDetails.Work { work with Steps = steps } }
                      let decRes = decide (DefineRouting cmd) None
                      test <@ Result.isError decRes @>
                  | _ -> failwith "Expected Work details")

              testCase "should fail validation if step sequence numbers are duplicate" (fun () ->
                  let cmd = defaultCommand ()
                  match cmd.Details with
                  | DefineRoutingDetails.Work work ->
                      let steps = work.Steps |> List.map (fun s -> { s with Sequence = 10 })
                      let cmd = { cmd with Details = DefineRoutingDetails.Work { work with Steps = steps } }
                      let decRes = decide (DefineRouting cmd) None
                      test <@ Result.isError decRes @>
                  | _ -> failwith "Expected Work details")

              testCase "should fail validation if step IDs are duplicate" (fun () ->
                  let cmd = defaultCommand ()
                  match cmd.Details with
                  | DefineRoutingDetails.Work work ->
                      let steps = work.Steps |> List.map (fun s -> { s with StepId = "STEP-10" })
                      let cmd = { cmd with Details = DefineRoutingDetails.Work { work with Steps = steps } }
                      let decRes = decide (DefineRouting cmd) None
                      test <@ Result.isError decRes @>
                  | _ -> failwith "Expected Work details")

              testCase "should fail validation if ReworkStepId references non-existent step" (fun () ->
                  let cmd = defaultCommand ()
                  match cmd.Details with
                  | DefineRoutingDetails.Work work ->
                      let steps =
                          work.Steps
                          |> List.map (fun s ->
                              if s.StepId = "STEP-10" then
                                  { s with ReworkPolicy = ReworkPolicy.ReworkToStep(RoutingStepId "STEP-NONEXISTENT", getPercent 0.5m) }
                              else
                                  s)
                      let cmd = { cmd with Details = DefineRoutingDetails.Work { work with Steps = steps } }
                      let decRes = decide (DefineRouting cmd) None
                      test <@ Result.isError decRes @>
                  | _ -> failwith "Expected Work details")

              testCase "should fail validation if ReworkRate is outside [0, 1] range" (fun () ->
                  let res =
                      let details : Medhavi.Contracts.Integration.WorkRoutingDetailsReq =
                          { ProductId = "SKU-FINISHED"
                            PrimaryOutputSkuId = "SKU-FINISHED"
                            BaseOutputQuantity = 1.0m
                            Steps = [
                                { StepId = "STEP-10"
                                  Sequence = 10
                                  OperationCode = "OP-10"
                                  Name = "Welding"
                                  Description = None
                                  Kind = "Standard"
                                  Inputs = []
                                  Outputs = []
                                  ResourceRequirements = []
                                  TimingProfile = { FixedLeadTime = None; QueueTime = None; WaitTime = None; MoveTime = None }
                                  YieldPercentage = None
                                  ReworkStepId = Some "STEP-10"
                                  ReworkRate = Some 1.1m
                                  OverlapPolicyType = "NoOverlap"
                                  OverlapPolicyValue = None
                                  EffectiveStart = None
                                  EffectiveEnd = None }
                            ] }
                      let req : Medhavi.Contracts.Integration.RoutingDefineReq =
                          { Id = "ROUTING-TEST"
                            Name = "Test Assembly Routing"
                            Description = Some "Test description"
                            Type = "Work"
                            StockingPointId = Some "SP-01"
                            EffectiveStart = DateTimeOffset.UtcNow
                            EffectiveEnd = None
                            PreferencePriority = 1
                            IsPreferred = true
                            MinQuantity = None
                            MaxQuantity = None
                            LotSize = None
                            OrderMultiple = None
                            CostPolicyType = "NoRoutingCost"
                            CostPolicyValue = None
                            Details = Medhavi.Contracts.Integration.WorkDetails details
                            Created = DateTimeOffset.UtcNow }
                      Medhavi.MasterData.Application.Routing.ACL.toDefineCommand req
                  test <@ match res with Invalid _ -> true | _ -> false @>
              )

              testCase "should fail validation if EfficiencyFactor is non-positive" (fun () ->
                  let cmd = defaultCommand ()
                  match cmd.Details with
                  | DefineRoutingDetails.Work work ->
                      let steps =
                          work.Steps
                          |> List.map (fun s ->
                              if s.StepId = "STEP-10" then
                                  let reqs =
                                      s.ResourceRequirements
                                      |> List.map (fun r ->
                                          let opts =
                                              r.Options
                                              |> List.map (fun o ->
                                                   { o with EfficiencyPolicy = Some -1.0m })
                                          { r with Options = opts })
                                  { s with ResourceRequirements = reqs }
                              else
                                  s)
                      let cmd = { cmd with Details = DefineRoutingDetails.Work { work with Steps = steps } }
                      let decRes = decide (DefineRouting cmd) None
                      test <@ Result.isError decRes @>
                  | _ -> failwith "Expected Work details")

              testCase "should calculate step flows for linear routing correctly" (fun () ->
                  let cmd = defaultCommand ()

                  let decRes =
                      decide (DefineRouting cmd) None
                      |> function
                          | Ok x -> x
                          | Error e -> failwithf "%A" e

                  let routing = decRes.NewState

                  let mapped = Medhavi.MasterData.Application.Routing.mapRoutingDto routing
                  let capacityRouting = RoutingAcl.translate mapped

                  let flows = RoutingInterpreter.calculateStepFlows capacityRouting 100.0m
                  test <@ Map.find "STEP-10" flows = 100.0m / 0.95m @>
                  test <@ Map.find "STEP-20" flows = 100.0m / 0.95m @>)

              testCase "should calculate step flows with feedback rework loops correctly" (fun () ->
                  let rId = getSomeId "RoutingId" RoutingId.create "ROUTING-REWORK"
                  let now = Timestamp.now

                  let cmd = defaultCommand ()
                  let cmd = { cmd with Id = rId }
                  match cmd.Details with
                  | DefineRoutingDetails.Work work ->
                      let step1 = { work.Steps.[0] with StepId = "STEP-10"; Sequence = 10; YieldPolicy = StepYieldPolicy.NoYieldLoss; ReworkPolicy = ReworkPolicy.NoRework }
                      let step2 = { work.Steps.[1] with StepId = "STEP-20"; Sequence = 20; YieldPolicy = StepYieldPolicy.ExpectedYield (getPercent 0.9m); ReworkPolicy = ReworkPolicy.ReworkToStep(RoutingStepId "STEP-10", getPercent 0.8m) }
                      let cmd = { cmd with Details = DefineRoutingDetails.Work { work with Steps = [ step1; step2 ] } }

                      let decRes =
                          decide (DefineRouting cmd) None
                          |> function
                              | Ok x -> x
                              | Error e -> failwithf "%A" e

                      let mapped = Medhavi.MasterData.Application.Routing.mapRoutingDto decRes.NewState
                      let capacityRouting = RoutingAcl.translate mapped

                      let flows = RoutingInterpreter.calculateStepFlows capacityRouting 90.0m
                      let f1 = Map.find "STEP-10" flows
                      let f2 = Map.find "STEP-20" flows

                      test <@ abs (f2 - 100.0m) < 0.001m @>
                      test <@ abs (f1 - f2) < 0.0001m @>
                  | _ -> failwith "Expected Work details")

              testCase "should calculate duration and cost correctly with setup and efficiency factors" (fun () ->
                  let rId = getSomeId "RoutingId" RoutingId.create "ROUTING-METRICS"
                  let now = Timestamp.now

                  let cmd = defaultCommand ()
                  let cmd = { cmd with Id = rId }
                  match cmd.Details with
                  | DefineRoutingDetails.Work work ->
                      let steps =
                          [ { work.Steps.[0] with
                                ResourceRequirements =
                                  [ { work.Steps.[0].ResourceRequirements.[0] with
                                        Options =
                                          [ { work.Steps.[0].ResourceRequirements.[0].Options.[0] with
                                                SetupTime = Some 10.0m
                                                RunTimePerBaseQuantity = Some 2.0m
                                                CostPerMinute = Some 5.0m
                                                EfficiencyPolicy = Some 2.0m } ] } ] } ]
                      let cmd = { cmd with Details = DefineRoutingDetails.Work { work with Steps = steps } }

                      let decRes =
                          decide (DefineRouting cmd) None
                          |> function
                              | Ok x -> x
                              | Error e -> failwithf "%A" e

                      let mapped = Medhavi.MasterData.Application.Routing.mapRoutingDto decRes.NewState
                      let capacityRouting = RoutingAcl.translate mapped

                      let metrics = RoutingInterpreter.calculateRoutingMetrics capacityRouting 10.0m
                      test <@ metrics.TotalDuration = 20.0m @>
                      test <@ metrics.TotalCost = 100.0m @>
                  | _ -> failwith "Expected Work details")

              testCase "should fallback to defaults when rates and times are missing" (fun () ->
                  let rId = getSomeId "RoutingId" RoutingId.create "ROUTING-FALLBACK"
                  let now = Timestamp.now

                  let cmd = defaultCommand ()
                  let cmd = { cmd with Id = rId }
                  match cmd.Details with
                  | DefineRoutingDetails.Work work ->
                      let steps =
                          [ { work.Steps.[0] with
                                ResourceRequirements =
                                  [ { work.Steps.[0].ResourceRequirements.[0] with
                                        Options =
                                          [ { work.Steps.[0].ResourceRequirements.[0].Options.[0] with
                                                SetupTime = None
                                                RunTimePerBaseQuantity = None
                                                CostPerMinute = None
                                                EfficiencyPolicy = None } ] } ] } ]
                      let cmd = { cmd with Details = DefineRoutingDetails.Work { work with Steps = steps } }

                      let decRes =
                          decide (DefineRouting cmd) None
                          |> function
                              | Ok x -> x
                              | Error e -> failwithf "%A" e

                      let mapped = Medhavi.MasterData.Application.Routing.mapRoutingDto decRes.NewState
                      let capacityRouting = RoutingAcl.translate mapped

                      let metrics = RoutingInterpreter.calculateRoutingMetrics capacityRouting 10.0m
                      test <@ metrics.TotalDuration = 10.0m @>
                      test <@ metrics.TotalCost = 0.0m @>
                  | _ -> failwith "Expected Work details")

              testCase "should select best routing based on Fastest/Cheapest/Balanced policies" (fun () ->
                  let now = Timestamp.now

                  let buildRouting id name priority duration setup cost =
                      let rId = getSomeId "RoutingId" RoutingId.create id
                      let cmd = defaultCommand ()
                      let cmd = { cmd with Id = rId; Name = name; Priority = priority; IsPreffered = false }
                      match cmd.Details with
                      | DefineRoutingDetails.Work work ->
                          let steps =
                              [ { work.Steps.[0] with
                                    ResourceRequirements =
                                      [ { work.Steps.[0].ResourceRequirements.[0] with
                                            Options =
                                              [ { work.Steps.[0].ResourceRequirements.[0].Options.[0] with
                                                    SetupTime = Some setup
                                                    RunTimePerBaseQuantity = Some duration
                                                    CostPerMinute = Some cost
                                                    EfficiencyPolicy = None } ] } ] } ]
                          let cmd = { cmd with Details = DefineRoutingDetails.Work { work with Steps = steps } }
                          match decide (DefineRouting cmd) None with
                          | Ok dec -> Medhavi.MasterData.Application.Routing.mapRoutingDto dec.NewState
                          | Error err -> failwithf "Failed to build routing: %A" err
                      | _ -> failwith "Expected Work details"

                  let r1 = buildRouting "R-1" "Fast Line" 1 1.0m 10.0m 10.0m
                  let r2 = buildRouting "R-2" "Cheap Line" 2 5.0m 0.0m 1.0m

                  let candidates = [ r1; r2 ] |> List.map RoutingAcl.translate

                  let selectedFastest = RoutingInterpreter.selectRouting Fastest 10.0m candidates

                  test
                      <@
                          selectedFastest
                          |> Option.map (fun r -> r.RoutingId) = Some "R-1"
                      @>

                  let selectedCheapest = RoutingInterpreter.selectRouting Cheapest 10.0m candidates

                  test
                      <@
                          selectedCheapest
                          |> Option.map (fun r -> r.RoutingId) = Some "R-2"
                      @>

                  let selectedBalanced = RoutingInterpreter.selectRouting Balanced 10.0m candidates

                  test
                      <@
                          selectedBalanced
                          |> Option.map (fun r -> r.RoutingId) = Some "R-1"
                      @>) ]

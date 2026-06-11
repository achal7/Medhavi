namespace Medhavi.Scheduler.Tests.Mrp.Domain

open System
open Expecto
open Swensen.Unquote
open Medhavi.SharedKernel
open Medhavi.Scheduler.Mrp.Domain.Types
open Medhavi.Scheduler.Mrp.Domain.Policies
open Medhavi.Scheduler.Mrp.Domain.MrpRunAggregate
open Medhavi.Scheduler.Tests.TestCommon

module MrpRunAggregateTests =
    let startCmd =
        StartMrpRun
            { RunId = "run-id-1"
              StartDate = createTimestampYmd 2026 6 1
              EndDate = createTimestampYmd 2026 6 10
              StockingPointId = spWarehouse
              Policy = MrpPolicy.defaults
              StartedAt = createTimestampYmd 2026 6 1 }

    let updateCmd =
        UpdateMrpRunProgress
            { RunId = "run-id-1"
              Progress = 45
              UpdatedAt = createTimestampYmd 2026 6 2 }

    [<Tests>]
    let tests =
        testList
            "MRP Domain - MrpRunAggregate State Transitions"
            [

              testCase
                  "Given pending/empty run, when starting run, then transition to Running state and emit MrpRunStarted"
                  (fun () ->

                      let result = decide startCmd None |> getOk

                      test <@ result.Events.Length = 1 @>

                      match result.Events.[0] with
                      | MrpRunStarted evt ->
                          test <@ evt.RunId = "run-id-1" @>
                          test <@ evt.StockingPointId = spWarehouse @>
                      | _ -> failwith "Expected MrpRunStarted event"

                      match result.NewState with
                      | Running data -> test <@ data.Progress = 0 @>
                      | _ -> failwith "Expected Running state")

              testCase "Given a running run, when starting run again, then return conflict error" (fun () ->
                  let runningState =
                      Running
                          { StartTime = createTimestampYmd 2026 6 1
                            Progress = 10 }

                  let result = decide startCmd (Some runningState)

                  test <@ Result.isError result @>)

              testCase
                  "Given a running run, when updating progress, then succeed and update running progress"
                  (fun () ->

                      let runningState =
                          Running
                              { StartTime = createTimestampYmd 2026 6 1
                                Progress = 10 }

                      let result = decide updateCmd (Some runningState) |> getOk

                      test <@ result.Events.Length = 1 @>

                      match result.Events.[0] with
                      | MrpRunProgressUpdated evt -> test <@ evt.Progress = 45 @>
                      | _ -> failwith "Expected MrpRunProgressUpdated event"

                      match result.NewState with
                      | Running data -> test <@ data.Progress = 45 @>
                      | _ -> failwith "Expected Running state with progress 45")

              testCase
                  "Given a running run, when updating progress to out-of-bounds, then return validation error"
                  (fun () ->
                      let updateCmd =
                          UpdateMrpRunProgress
                              { RunId = "run-id-1"
                                Progress = 150
                                UpdatedAt = createTimestampYmd 2026 6 2 }

                      let runningState =
                          Running
                              { StartTime = createTimestampYmd 2026 6 1
                                Progress = 10 }

                      let result = decide updateCmd (Some runningState)

                      test <@ Result.isError result @>)

              testCase
                  "Given running run, when completing, then transition to Completed and emit MrpRunCompleted"
                  (fun () ->
                      let runResult =
                          { RunId = MrpRunId.create "run-id-1" |> getOk
                            StartTime = createTimestampYmd 2026 6 1
                            EndTime = createTimestampYmd 2026 6 2
                            Status = MrpRunStatus.Completed
                            BomExplosionCount = 0
                            NetRequirements = []
                            Proposals = []
                            ActionMessages = []
                            Peggings = []
                            Errors = []
                            Warnings = [] }

                      let completeCmd =
                          CompleteMrpRun
                              { RunId = "run-id-1"
                                Result = runResult
                                CompletedAt = createTimestampYmd 2026 6 2 }

                      let runningState =
                          Running
                              { StartTime = createTimestampYmd 2026 6 1
                                Progress = 80 }

                      let result = decide completeCmd (Some runningState) |> getOk

                      test <@ result.Events.Length = 1 @>

                      match result.Events.[0] with
                      | MrpRunCompleted evt -> test <@ evt.Result.RunId = runResult.RunId @>
                      | _ -> failwith "Expected MrpRunCompleted event"

                      match result.NewState with
                      | Completed completedData -> test <@ completedData.Result.RunId = runResult.RunId @>
                      | _ -> failwith "Expected Completed state")

              testCase "Given running run, when failing, then transition to Failed and emit MrpRunFailed" (fun () ->
                  let failCmd =
                      FailMrpRun
                          { RunId = "run-id-1"
                            Error = "Critical db connection error"
                            FailedAt = createTimestampYmd 2026 6 2 }

                  let runningState =
                      Running
                          { StartTime = createTimestampYmd 2026 6 1
                            Progress = 50 }

                  let result = decide failCmd (Some runningState) |> getOk

                  test <@ result.Events.Length = 1 @>

                  match result.Events.[0] with
                  | MrpRunFailed evt -> test <@ evt.Error = "Critical db connection error" @>
                  | _ -> failwith "Expected MrpRunFailed event"

                  match result.NewState with
                  | Failed failedData -> test <@ failedData.Error = "Critical db connection error" @>
                  | _ -> failwith "Expected Failed state")

              testCase "Given MrpRunStarted event, when evolving None, then transition state to Running" (fun () ->
                  let startEvt =
                      MrpRunStarted
                          { RunId = "run-id-1"
                            StartDate = createTimestampYmd 2026 6 1
                            EndDate = createTimestampYmd 2026 6 10
                            StockingPointId = spWarehouse
                            Policy = MrpPolicy.defaults
                            StartedAt = createTimestampYmd 2026 6 1 }

                  let stateOpt = evolve startEvt None

                  test <@ Option.isSome stateOpt @>

                  match Option.get stateOpt with
                  | Running data -> test <@ data.Progress = 0 @>
                  | _ -> failwith "Expected Running state after evolution") ]

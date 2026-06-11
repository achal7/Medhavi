module Medhavi.Domain.Tests.SharedKernelTests

open System
open System.Text.Json
open Expecto
open Swensen.Unquote
open Medhavi.SharedKernel
open Medhavi.Scenario
open Medhavi.Common.Validation
open Medhavi.Common.Patterns.Optics
open Medhavi.Common.Patterns
open Medhavi.Common.Patterns.StateMonad
open Medhavi.Common.Patterns.Writer

type CounterState = { Count: int }

type CounterCommand =
    | Increment of int
    | Decrement of int

type CounterEvent =
    | Incremented of int
    | Decremented of int

type MockCommand = {
    CommandId: string
    Quantity: decimal
    SetupHours: float
}

type MockState = {
    CommandId: string
    Quantity: decimal
    SetupHours: float
}

type MockEvent =
    | CommandProcessed of string * decimal * float

type PlanningState = {
    Iteration: int
    ActiveLimiters: string list
}

let roundTrip<'T> (value: 'T) : 'T =
    let json = JsonSerializer.Serialize(value)
    JsonSerializer.Deserialize<'T>(json)

[<Tests>]
let tests =
    testList
        "SharedKernel Primitives Tests"
        [ testCase "SkuId equality and validation" (fun () ->
              let sku1 = SkuId.create "SKU-001"
              let sku2 = SkuId.create "SKU-001"
              let sku3 = SkuId.create "SKU-002"
              let skuEmpty = SkuId.create "  "

              let isEq = (sku1 = sku2)
              let isNotEq = (sku1 <> sku3)
              let isEmptyError = skuEmpty.IsError

              test <@ isEq @>
              test <@ isNotEq @>
              test <@ isEmptyError @>)

          testCase "Quantity ratio and trySubtract validation" (fun () ->
              let q1 = Quantity.create 10m |> Result.defaultWith (fun e -> failwith e.Message)
              let q2 = Quantity.create 5m |> Result.defaultWith (fun e -> failwith e.Message)
              let qZero = Quantity.Zero

              let ratioOk = Quantity.ratio q1 q2
              let ratioError = Quantity.ratio q1 qZero

              let isRatioOk = (ratioOk = Ok 2m)
              let isRatioError = ratioError.IsError

              test <@ isRatioOk @>
              test <@ isRatioError @>

              let subOk = Quantity.trySubtract q1 q2
              let subError = Quantity.trySubtract q2 q1

              let isSubOk = (subOk = Ok q2)
              let isSubError = subError.IsError

              test <@ isSubOk @>
              test <@ isSubError @>)

          testCase "Quantity type safety validation" (fun () ->
              let nnOk = Quantity.create 0m
              let nnError = Quantity.create -1m
              let pOk = Quantity.create 1m

              let isNnOk = nnOk.IsOk
              let isNnError = nnError.IsError
              let isPOk = pOk.IsOk

              test <@ isNnOk @>
              test <@ isNnError @>
              test <@ isPOk @>)

          testCase "Window validation and invariants" (fun () ->
              let t1 = Timestamp(DateTimeOffset.UtcNow)
              let t2 = Timestamp(DateTimeOffset.UtcNow.AddHours(1.0))

              let winOk = Window.create t1 t2
              let winError = Window.create t2 t1

              let isWinOk = winOk.IsOk
              let isWinError = winError.IsError

              test <@ isWinOk @>
              test <@ isWinError @>

              match winOk with
              | Ok w ->
                  let isStartEqual = (w.Start = t1)
                  let isEndEqual = (w.End = t2)
                  test <@ isStartEqual @>
                  test <@ isEndEqual @>
              | Error e -> failwith e.Message)

          testCase "JSON serialization round-trip" (fun () ->
              let sku =
                  SkuId.create "SKU-999"
                  |> function
                      | Ok x -> x
                      | Error _ -> failwith "invalid"

              let qty = Quantity.create 123.45m |> Result.defaultWith (fun e -> failwith e.Message)
              let money = { Amount = 100.50m; Currency = "USD" }

              let roundTripSku = roundTrip sku
              let roundTripQty = roundTrip qty
              let roundTripMoney = roundTrip money

              let isSkuRtOk = (SkuId.value roundTripSku = "SKU-999")
              let isQtyRtOk = (Quantity.value roundTripQty = 123.45m)
              let isMoneyRtOk = (roundTripMoney = money)

              test <@ isSkuRtOk @>
              test <@ isQtyRtOk @>
              test <@ isMoneyRtOk @>)

          testCase "Aggregate API and pattern match compile verification" (fun () ->
              let decide
                  (cmd: CounterCommand)
                  (stateOpt: CounterState option)
                  : Result<Decision<CounterState, CounterEvent>, DomainError> =
                  let state = stateOpt |> Option.defaultValue { Count = 0 }

                  match cmd with
                  | Increment value -> Ok { NewState = { Count = state.Count + value }; Events = [ Incremented value ] }
                  | Decrement value -> Ok { NewState = { Count = state.Count - value }; Events = [ Decremented value ] }

              let evolve: Evolve<CounterState, CounterEvent> =
                  fun ev stateOpt ->
                      let state = stateOpt |> Option.defaultValue { Count = 0 }

                      match ev with
                      | Incremented value -> Some { Count = state.Count + value }
                      | Decremented value -> Some { Count = state.Count - value }

              let result = Aggregate.handleCommandFromHistory decide evolve (Increment 5) [ Incremented 10 ]

              match result with
              | Ok decision ->
                  let isCountFifteen = (decision.NewState.Count = 15)
                  let isEventsOk = (decision.Events = [ Incremented 5 ])
                  test <@ isCountFifteen @>
                  test <@ isEventsOk @>
              | Error _ -> failwith "Decision failed")

          testCase "Timestamp and Window enforce UTC normalization" (fun () ->
              // Local time: 2026-05-25 23:00:00 +05:30 -> UTC: 17:30:00
              let localDto = DateTimeOffset(2026, 5, 25, 23, 0, 0, TimeSpan.FromHours(5.5))
              let ts = Timestamp.create localDto
              let innerVal = Timestamp.value ts
              let offset = innerVal.Offset
              let hour = innerVal.Hour
              let minute = innerVal.Minute
              test <@ offset = TimeSpan.Zero @>
              test <@ hour = 17 @>
              test <@ minute = 30 @>

              let localStart = DateTimeOffset(2026, 5, 25, 23, 0, 0, TimeSpan.FromHours(5.5))
              let localEnd = DateTimeOffset(2026, 5, 26, 2, 0, 0, TimeSpan.FromHours(5.5))
              let winResult = Window.createFromTime localStart localEnd

              match winResult with
              | Ok w ->
                  let startVal = Timestamp.value w.Start
                  let endVal = Timestamp.value w.End
                  let startOffset = startVal.Offset
                  let endOffset = endVal.Offset
                  let startHour = startVal.Hour
                  let endHour = endVal.Hour
                  test <@ startOffset = TimeSpan.Zero @>
                  test <@ endOffset = TimeSpan.Zero @>
                  test <@ startHour = 17 @>
                  test <@ endHour = 20 @>
              | Error e -> failwith e.Message)

          testCase "ContextWrapper explicit propagation" (fun () ->
              let ctx =
                  ExecutionContext.create ()
                  |> ExecutionContext.withTenantId "tenant-99"

              let wrapper =
                  { Context = ctx
                    Payload = "test-payload" }

              test <@ wrapper.Payload = "test-payload" @>
              test <@ wrapper.Context.TenantId = Some "tenant-99" @>)

          testCase "ExecutionContextHolder implicit AsyncLocal propagation" (fun () ->
              let ctx =
                  ExecutionContext.create ()
                  |> ExecutionContext.withTenantId "tenant-async"

              ExecutionContextHolder.Set ctx

              let currentCtx = ExecutionContextHolder.TryGet()
              test <@ currentCtx.IsSome @>
              test <@ currentCtx.Value.TenantId = Some "tenant-async" @>

              // Verify inside an async/task boundary
              let taskResult =
                  task {
                      do! System.Threading.Tasks.Task.Delay(10)
                      let innerCtx = ExecutionContextHolder.TryGet()
                      return innerCtx
                  }
                  |> Async.AwaitTask
                  |> Async.RunSynchronously

              test <@ taskResult.IsSome @>
              test <@ taskResult.Value.TenantId = Some "tenant-async" @>

              ExecutionContextHolder.Clear()
              test <@ (ExecutionContextHolder.TryGet()).IsNone @>)

          testCase "ExecutionContextValidation requireTenant helper" (fun () ->
              let ctxWithTenant =
                  ExecutionContext.create ()
                  |> ExecutionContext.withTenantId "tenant-valid"

              let resultOk = ExecutionContextValidation.requireTenant ctxWithTenant
              test <@ resultOk = Ok "tenant-valid" @>

              let ctxNoTenant = ExecutionContext.create ()
              let resultError = ExecutionContextValidation.requireTenant ctxNoTenant
              let isError = resultError.IsError
              test <@ isError @>

              let ctxEmptyTenant =
                  ExecutionContext.create ()
                  |> ExecutionContext.withTenantId "   "

              let resultError2 = ExecutionContextValidation.requireTenant ctxEmptyTenant
              let isError2 = resultError2.IsError
              test <@ isError2 @>)

          testCase "Telemetry metric construction and Context mapping" (fun () ->
              let ctx = ExecutionContext.create() |> ExecutionContext.withTenantId "tenant-metrics"
              let tags = Map.ofList [ ("module", "Promise"); ("env", "test") ]
              let startTime = DateTimeOffset.UtcNow
              let duration = TimeSpan.FromMilliseconds(124.5)

              let latency = Telemetry.createLatency "PromiseCheck" "PromiseEngine" startTime duration true None ctx tags
              test <@ latency.OperationName = "PromiseCheck" @>
              test <@ latency.Component = "PromiseEngine" @>
              let durationMs = latency.DurationMs
              test <@ durationMs = 124.5 @>
              test <@ latency.IsSuccess = true @>
              test <@ latency.ErrorDetails = None @>
              test <@ latency.CorrelationId = ctx.CorrelationId @>
              test <@ latency.TenantId = Some "tenant-metrics" @>
              test <@ Map.tryFind "module" latency.Metadata = Some "Promise" @>

              let limiter = Telemetry.createLimiterFrequency "MaterialLimit" "PromiseEngine" (Some 10.0) (Some 100.0) (Some 0.1) 1L 10L true ctx tags
              test <@ limiter.LimiterName = "MaterialLimit" @>
              test <@ limiter.Component = "PromiseEngine" @>
              test <@ limiter.CurrentRate = Some 10.0 @>
              test <@ limiter.ConfiguredLimitRate = Some 100.0 @>
              test <@ limiter.Utilization = Some 0.1 @>
              test <@ limiter.ThrottledCount = 1L @>
              test <@ limiter.TotalEvaluatedCount = 10L @>
              test <@ limiter.IsActive = true @>
              test <@ limiter.CorrelationId = ctx.CorrelationId @>
              test <@ limiter.TenantId = Some "tenant-metrics" @>

              let kpis = Telemetry.createPlanningKpis "scen-abc" 1500m 200m (Map.ofList [("WC-1", 0.85)]) 0.95 ctx
              test <@ kpis.ScenarioId = "scen-abc" @>
              test <@ kpis.TotalCost = 1500m @>
              test <@ kpis.LatenessPenalty = 200m @>
              test <@ kpis.ServiceLevel = 0.95 @>
              test <@ kpis.TenantId = Some "tenant-metrics" @>

              let err = Telemetry.createError "Integrator" "ING-01" "Database timeout" ctx
              test <@ err.Component = "Integrator" @>
              test <@ err.ErrorCode = "ING-01" @>
              test <@ err.ErrorMessage = "Database timeout" @>
              test <@ err.TenantId = Some "tenant-metrics" @>)

          testCase "Telemetry JSON serialization roundtrip" (fun () ->
              let ctx = ExecutionContext.create() |> ExecutionContext.withTenantId "tenant-metrics"
              let tags = Map.ofList [ ("module", "Promise") ]
              let startTime = DateTimeOffset.UtcNow
              let duration = TimeSpan.FromMilliseconds(124.5)
              let latency = Telemetry.createLatency "PromiseCheck" "PromiseEngine" startTime duration true None ctx tags

              let metric = Latency latency
              let serializedResult = roundTrip metric

              match serializedResult with
              | Latency lat ->
                  test <@ lat.OperationName = "PromiseCheck" @>
                  test <@ lat.Component = "PromiseEngine" @>
                  let latDuration = lat.DurationMs
                  test <@ latDuration = 124.5 @>
                  test <@ lat.CorrelationId = ctx.CorrelationId @>
                  test <@ lat.TenantId = Some "tenant-metrics" @>
                  test <@ Map.tryFind "module" lat.Metadata = Some "Promise" @>
              | _ -> failwith "Expected Latency case after roundtrip"

              let limiterMetric = Telemetry.createLimiterFrequency "CapacityLimit" "Scheduler" (Some 8.0) (Some 8.0) (Some 1.0) 5L 20L true ctx tags
              let metric2 = LimiterFrequency limiterMetric
              let serializedResult2 = roundTrip metric2

              match serializedResult2 with
              | LimiterFrequency lim ->
                  test <@ lim.LimiterName = "CapacityLimit" @>
                  test <@ lim.Component = "Scheduler" @>
                  test <@ lim.CurrentRate = Some 8.0 @>
                  test <@ lim.ConfiguredLimitRate = Some 8.0 @>
                  test <@ lim.Utilization = Some 1.0 @>
                  test <@ lim.ThrottledCount = 5L @>
                  test <@ lim.TotalEvaluatedCount = 20L @>
                  test <@ lim.IsActive = true @>
                  test <@ lim.CorrelationId = ctx.CorrelationId @>
                  test <@ lim.TenantId = Some "tenant-metrics" @>
              | _ -> failwith "Expected LimiterFrequency case after roundtrip")
                
          testCase "Applicative command validation pipeline verification" (fun () ->
              // Define custom validation rules
              let validateCommandId (cmdId: string) =
                  if String.IsNullOrWhiteSpace cmdId then
                      Invalid [ DomainError.validation "CommandId cannot be empty" ]
                  else
                      Valid cmdId

              let validateQty (qty: decimal) =
                  if qty <= 0m then
                      Invalid [ DomainError.validation "Quantity must be positive" ]
                  else
                      Valid qty

              let validateHours (hours: float) =
                  if hours <= 0.0 then
                      Invalid [ DomainError.validation "SetupHours must be positive" ]
                  else
                      Valid hours

              // Combined validator using fully qualified applicative properties to avoid operator collision
              let validateMockCommand cmd =
                  let c id q h = { CommandId = id; Quantity = q; SetupHours = h }
                  let vId = validateCommandId cmd.CommandId
                  let vQty = validateQty cmd.Quantity
                  let vHours = validateHours cmd.SetupHours
                  Medhavi.Common.Validation.apply (Medhavi.Common.Validation.apply (Medhavi.Common.Validation.map c vId) vQty) vHours

              // Handlers
              let decide cmd _ : Result<Decision<MockState, MockEvent>, DomainError> =
                  Ok { NewState = { CommandId = cmd.CommandId; Quantity = cmd.Quantity; SetupHours = cmd.SetupHours }
                       Events = [ CommandProcessed (cmd.CommandId, cmd.Quantity, cmd.SetupHours) ] }

              let evolve: Evolve<MockState, MockEvent> =
                  fun ev stateOpt ->
                      match ev with
                      | CommandProcessed (id, q, h) ->
                          Some { CommandId = id; Quantity = q; SetupHours = h }

              let handleCommandWithValidation validator decide evolve command history =
                  validator command
                  |> toResult
                  |> Result.mapError DomainError.combineValidationErrors
                  |> Result.bind (fun cmd -> Aggregate.handleCommandFromHistory decide evolve cmd history)
                  |> Result.map (fun decision -> (decision.NewState, decision.Events))

              // 1. Success Case
              let validCmd = { CommandId = "CMD-001"; Quantity = 10.5m; SetupHours = 2.5 }
              let successResult = handleCommandWithValidation validateMockCommand decide evolve validCmd Seq.empty
              
              match successResult with
              | Ok (state, events) ->
                  test <@ state.CommandId = "CMD-001" @>
                  test <@ state.Quantity = 10.5m @>
                  test <@ state.SetupHours = 2.5 @>
                  test <@ events.Length = 1 @>
              | Error _ -> failwith "Expected valid command to succeed"

              // 2. Failure Case with error accumulation
              let invalidCmd = { CommandId = "   "; Quantity = -5.0m; SetupHours = -1.0 }
              let failureResult = handleCommandWithValidation validateMockCommand decide evolve invalidCmd Seq.empty

              match failureResult with
              | Error (ValidationError (code, msg, data)) ->
                  test <@ code = ErrorCodes.ValidationFailed @>
                  let hasError0 = Map.containsKey "error_0" data
                  let hasError1 = Map.containsKey "error_1" data
                  let hasError2 = Map.containsKey "error_2" data
                  test <@ hasError0 && hasError1 && hasError2 @>
                  
                  // Messages must be concatenated in the summary
                  test <@ msg.Contains("CommandId cannot be empty") @>
                  test <@ msg.Contains("Quantity must be positive") @>
                  test <@ msg.Contains("SetupHours must be positive") @>
              | Error _ -> failwith "Expected ValidationError case"
              | Ok _ -> failwith "Expected invalid command to fail validation")
                
          testCase "Optics Lens and Optional verification in domain context" (fun () ->
              // Verify Lens
              let versionLens : Lens<ScenarioReadModel, int> =
                  lens (fun s -> s.Version) (fun v s -> { s with Version = v })

              let initialScenario = {
                  ScenarioId = "SCEN-001"
                  Name = "Test Scenario"
                  BaseScenarioId = None
                  Version = 1
                  CreatedAt = DateTimeOffset.UtcNow
                  IsActive = true
                  Overrides = []
              }

              // Get
              let ver = get versionLens initialScenario
              test <@ ver = 1 @>

              // Set
              let updatedScenario = set versionLens 2 initialScenario
              test <@ updatedScenario.Version = 2 @>
              // Immutability check
              test <@ initialScenario.Version = 1 @>

              // Over
              let mappedScenario = over versionLens (fun v -> v + 5) initialScenario
              test <@ mappedScenario.Version = 6 @>

              // Verify Optional
              let baseScenarioIdOptional : Optional<ScenarioReadModel, string> =
                  optional
                      (fun s -> s.BaseScenarioId)
                      (fun opt s -> { s with BaseScenarioId = opt })

              // TryGet None
              let baseOpt1 = baseScenarioIdOptional.TryGet initialScenario
              test <@ baseOpt1 = None @>

              // Set
              let scenarioWithBase = baseScenarioIdOptional.Set (Some "SCEN-BASE") initialScenario
              test <@ scenarioWithBase.BaseScenarioId = Some "SCEN-BASE" @>

              // TryGet Some
              let baseOpt2 = baseScenarioIdOptional.TryGet scenarioWithBase
              test <@ baseOpt2 = Some "SCEN-BASE" @>

              // OverOptional
              let mappedBaseScenario = overOptional baseScenarioIdOptional (fun id -> id.ToLower()) scenarioWithBase
              test <@ mappedBaseScenario.BaseScenarioId = Some "scen-base" @>)

          testCase "StateMonad transitions, builders and Kleisli verification" (fun () ->
              let state = StateBuilder()

              // State transitions
              let incrementIteration : State<PlanningState, int> =
                  state {
                      let! s = StateMonad.getState
                      let nextIt = s.Iteration + 1
                      do! StateMonad.putState { s with Iteration = nextIt }
                      return nextIt
                  }

              let recordLimiter (name: string) : State<PlanningState, unit> =
                  state {
                      let! s = StateMonad.getState
                      do! StateMonad.putState { s with ActiveLimiters = name :: s.ActiveLimiters }
                  }

              // 1. Run basic transitions
              let initialState = { Iteration = 0; ActiveLimiters = [] }
              let (resultVal, finalState) = StateMonad.runState incrementIteration initialState
              test <@ resultVal = 1 @>
              test <@ finalState.Iteration = 1 @>

              // 2. Run computation expression with loops
              let loopComputation : State<PlanningState, unit> =
                  state {
                      for name in [ "Limiter-A"; "Limiter-B" ] do
                          do! recordLimiter name
                          let! _ = incrementIteration
                          return ()
                  }

              let (_, loopedState) = StateMonad.runState loopComputation initialState
              test <@ loopedState.Iteration = 2 @>
              test <@ loopedState.ActiveLimiters = [ "Limiter-B"; "Limiter-A" ] @>

              // 3. Kleisli composition using >=> (fish operator)
              let step1 (x: int) : State<PlanningState, int> =
                  state {
                      do! recordLimiter $"Step1-{x}"
                      return x * 2
                  }
              let step2 (y: int) : State<PlanningState, int> =
                  state {
                      do! recordLimiter $"Step2-{y}"
                      let! _ = incrementIteration
                      return y + 5
                  }

              let composed = step1 >=> step2
              let (composedVal, composedState) = StateMonad.runState (composed 10) initialState
              test <@ composedVal = 25 @> // 10 * 2 = 20, then 20 + 5 = 25
              test <@ composedState.Iteration = 1 @>
              test <@ composedState.ActiveLimiters = [ "Step2-20"; "Step1-10" ] @>)

          testCase "WriterMonad execution and logging verification" (fun () ->
              let listMonoid : Monoid<string list> = {
                  Empty = []
                  Append = (@)
              }

              let writer = WriterBuilder<string list>(listMonoid)

              // Writer monad computation
              let allocateCapacity (resource: string) (requiredHours: float) =
                  writer {
                      do! Writer.tell [ $"Checking capacity for resource: {resource}" ]
                      let isSufficient = requiredHours <= 8.0
                      do! Writer.tell [ $"Capacity check completed. Sufficient: {isSufficient}" ]
                      return isSufficient
                  }

              let (result, logs) = Writer.run (allocateCapacity "WorkCenter-1" 5.5)
              test <@ result = true @>
              test <@ logs = [ "Checking capacity for resource: WorkCenter-1"; "Capacity check completed. Sufficient: True" ] @>

              let (resultFail, logsFail) = Writer.run (allocateCapacity "WorkCenter-2" 12.0)
              test <@ resultFail = false @>
              test <@ logsFail = [ "Checking capacity for resource: WorkCenter-2"; "Capacity check completed. Sufficient: False" ] @>) ]

module Medhavi.Demand.Tests.DemandBehaviourAssessment.WorkflowTests

open System
open System.Threading.Tasks
open Expecto
open Medhavi.SharedKernel
open Medhavi.SharedKernel.Contracts.Aggregate
open Medhavi.Contracts.Demand.SenseDemand
open Medhavi.Demand
open Medhavi.Demand.DemandBehaviourAssessment.Model
open Medhavi.Demand.ForecastPublication.Model
open Medhavi.Demand.ForecastPublication.Projection
open Medhavi.Demand.ForecastPublication.Context
open Medhavi.Demand.Tests.Builders

[<Tests>]
let tests =
    testList
        "DemandBehaviourAssessment Workflow"
        [ testCaseTask "Critical state triggers forecast refresh"
          <| fun () ->
              task {
                  // ----- in‑memory repos -----
                  let assessmentRepo =
                      InMemRepository.createInMemoryRepository<
                          DemandBehaviourAssessment,
                          string,
                          DemandBehaviourAssessmentEvent
                       >()

                  let forecastRepo =
                      InMemRepository.createInMemoryRepository<ForecastPublication, string, ForecastPublicationEvent>()

                  // ----- dependencies -----
                  let isHighPriority _ = Task.FromResult false

                  let getScopeId (sku: SkuId) (sp: StockingPointId) =
                      task { return Some $"scope-{SkuId.value sku}-{StockingPointId.value sp}" }

                  // ----- seed a published forecast that meets refresh criteria -----
                  let publishedPubId = "PUB-OLD"

                  let publishedForecast: ForecastPublication =
                      { Id = ForecastPublicationId publishedPubId
                        PlanningScopeIds = [ PlanningScopeId.fromString $"scope-SKU-001-SP-001" |> okOrFail ]
                        ForecastHorizon = TimeSpan.FromDays 30.0
                        TimeBucketConfig = "Week"
                        Status = Published
                        Version = 1
                        ChampionModelId = None
                        OverallConfidenceIndex = None
                        Forecasts = Map.empty
                        Assumptions = Map.empty
                        Overrides = Map.empty
                        Coverage = []
                        TransactionTime = Timestamp.now - TimeSpan.FromHours(48.0)
                        PublicationTime = Some(Timestamp.now - TimeSpan.FromHours(48.0))
                        SupersededPublicationId = None }

                  let! save = forecastRepo.Save(publishedPubId, publishedForecast, [])
                  save |> okOrFail

                  // ----- seed an existing assessment with corroboration (two active sources) -----
                  let existingAssessment : DemandBehaviourAssessment = {
                        SkuId                    = skuId "SKU-001"
                        StockingPointId          = stockingPointId "SP-001"
                        CurrentState             = Elevated
                        LastUpdated              = Timestamp.now
                        CurrentDeviation         = Some (PositiveDecimal.createSafe 3.5m)
                        Confidence               = PositiveDecimal.createSafe 0.85m
                        CorroboratingSignalCount = 2
                        BaselineReference        = "Baseline-1"
                        ActiveSources            = ["POS"; "WEB"]
                        LastSignalTime           = Some (Timestamp.now - TimeSpan.FromHours 1.0)
                        LastStateChange          = None
                        BusinessTime             = Timestamp.now - TimeSpan.FromHours 1.0
                        TransactionTime          = Timestamp.now - TimeSpan.FromHours 1.0
                    }
                  let key = "SKU-001-SP-001"
                  let! _ = assessmentRepo.Save(key, existingAssessment, [])

                  // ----- forecast queries backed by in‑memory projection -----
                  let forecastAgent = createProjectionAgent()
                  seedProjections forecastAgent [ publishedForecast ]
                  let forecastQueries = createQueryService forecastAgent

                  // ----- forecast context -----
                  let forecastCompService = ForecastPublication.ComputationService.create(fun _ _ -> task { return [] })
                  let forecastCtx = create forecastRepo forecastCompService (fun _ -> ())
                  let forecastApi = forecastCtx.Commands

                  // ----- assessment context -----
                  let assessmentCtx = DemandBehaviourAssessment.Context.create assessmentRepo isHighPriority forecastQueries forecastApi getScopeId (fun _ -> ())
                  let workflow = assessmentCtx.Commands

                  // ----- signal that should trigger Critical -----
                  let req: EvaluateDemandSignalReq =
                      { SkuId = "SKU-001"
                        StockingPointId = "SP-001"
                        SignalId = "sig-critical"
                        Source = "POS"
                        SourceReliability = 90m
                        Timestamp = DateTimeOffset.UtcNow
                        Value = 150m
                        StatisticalBound = 10m
                        RecentBaseline = 100m }

                  let! result = workflow.EvaluateSignal req
                  Expect.isOk result "EvaluateSignal should succeed"

                  // ----- verify that a forecast cycle was initiated -----
                  // ----- verify the assessment state is Critical -----
                  let! assessmentState = assessmentRepo.Get(key)
                  match assessmentState with
                  | Ok(Some ass) -> Expect.equal ass.CurrentState Critical "Assessment should be in Critical state"
                  | _ -> failwith "Assessment not found"

                  // ----- verify that a forecast cycle was initiated with the expected scope -----
                  let expectedScope = $"scope-SKU-001-SP-001"
                  let! initiatedEvents =
                      forecastRepo.GetEventsByType (function
                          | ForecastCycleInitiated(pub, _, _) ->
                              pub.PlanningScopeIds
                              |> List.map PlanningScopeId.value
                              |> List.contains expectedScope
                          | _ -> false)

                  match initiatedEvents with
                  | Ok evts -> Expect.isNonEmpty evts "A new forecast cycle should have been initiated for the correct scope"
                  | _ -> failwith "Failed to query forecast events"
                  // let! forecastEvents = forecastRepo.GetEvents publishedPubId
                  //
                  // let initiated =
                  //     match forecastEvents with
                  //     | Ok evts ->
                  //         evts
                  //         |> List.exists (function
                  //             | ForecastCycleInitiated _ -> true
                  //             | _ -> false)
                  //     | _ -> false
                  //
                  // Expect.isTrue initiated "A new forecast cycle should have been initiated"
              } ]

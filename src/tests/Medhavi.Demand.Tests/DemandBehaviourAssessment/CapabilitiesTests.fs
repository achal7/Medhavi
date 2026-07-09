module Medhavi.Demand.Tests.DemandBehaviourAssessment.CapabilitiesTests

open System
open System.Threading.Tasks
open Expecto
open Medhavi.Demand.DemandBehaviourAssessment.Context
open Medhavi.SharedKernel.Contracts.Aggregate
open Medhavi.SharedKernel
open Medhavi.Demand
open Medhavi.Contracts.Demand.ForecastPublication
open Medhavi.Demand.DemandBehaviourAssessment.Model
open Medhavi.Demand.DemandBehaviourAssessment.Projection
open Medhavi.Demand.DemandBehaviourAssessment.Capabilities
open Medhavi.Demand.Tests.Builders
open Medhavi.SharedKernel.Contracts.DecisionTrace
open Medhavi.SharedKernel.Observation
open Medhavi.SharedKernel.Execution

let private getAssessmentId (cmd: DemandBehaviourAssessmentCommand) : string =
    match cmd with
    | EvaluateSignal c -> $"{SkuId.value c.SkuId}-{StockingPointId.value c.StockingPointId}"
    | Acknowledge c -> $"{SkuId.value c.SkuId}-{StockingPointId.value c.StockingPointId}"

let defaultSignal: DemandSignal =
    { SignalId = "sig-int-1"
      Source = "POS"
      SourceReliability = 90m
      Timestamp = DateTimeOffset.UtcNow
      Value = 130m
      StatisticalBound = 10m
      RecentBaseline = 100m }

let defaultEvaluateCmd: EvaluateSignalCmd =
    { Signal = defaultSignal
      SkuId = skuId "SKU-001"
      StockingPointId = stockingPointId "SP-001"
      IsHighPriority = false }

let private create repo isHighPriority publishKnowledge =
    let forecastQueries: ForecastPublicationQueries =
        { GetAll = fun () -> Task.FromResult []
          GetById = fun _ -> Task.FromResult None
          Exists = fun _ -> Task.FromResult false
          Filter = fun _ -> Task.FromResult []
          SubscribeApiEvents = fun _ -> { new IDisposable with member _.Dispose() = () } }
    let forecastApi: ForecastPublicationApi =
        { InitiateCycle = fun _ -> Task.FromResult (Ok "")
          PrepareContext = fun _ -> Task.FromResult (Ok "")
          SelectChampion = fun _ -> Task.FromResult (Ok "")
          GenerateBaseline = fun _ -> Task.FromResult (Ok "")
          RecordOverride = fun _ -> Task.FromResult (Ok "")
          Reconcile = fun _ -> Task.FromResult (Ok "")
          Publish = fun _ -> Task.FromResult (Ok "") }
    let getScopeId _ _ = Task.FromResult None
    let ctx = Medhavi.Demand.DemandBehaviourAssessment.Context.create repo isHighPriority forecastQueries forecastApi getScopeId publishKnowledge
    ctx.Commands

[<Tests>]
let tests =
    testList
        "DemandBehaviourAssessment Pipeline"
        [ testCaseTask "EvaluateSignal: no state → Elevated state → projection updated"
          <| fun () ->
              task {
                  let repo =
                      InMemRepository.createInMemoryRepository<
                          DemandBehaviourAssessment,
                          string,
                          DemandBehaviourAssessmentEvent
                       >()

                  let _ = DemandBehaviourAssessmentCommand.EvaluateSignal defaultEvaluateCmd
                  let isHighPriority _ = task { return false }
                  let knowledge = ResizeArray<ArchitecturalKnowledge>()
                  let caps = create repo isHighPriority (fun k -> knowledge.Add k)

                  // Execute via capability
                  let! outcome =
                      caps.EvaluateSignal
                          { SkuId = "SKU-001"
                            StockingPointId = "SP-001"
                            SignalId = "sig-int-1"
                            Source = "POS"
                            SourceReliability = 90m
                            Timestamp = DateTimeOffset.UtcNow
                            Value = 130m
                            StatisticalBound = 10m
                            RecentBaseline = 100m }

                  match outcome with
                  | Error err -> failwithf $"EvaluateSignal failed: %A{err}"
                  | Ok id ->
                      // Feed events into projection manually
                      let! events = repo.GetEvents id

                      let projState = events |> okOrFail |> List.fold evolveProjection Map.empty
                      Expect.isTrue (projState.ContainsKey id) "Projection should contain assessment"
                      Expect.equal projState[id].CurrentState "Elevated" "Projection state should be Elevated"
              }

          testCaseTask "EvaluateSignal: noise signal → no state change"
          <| fun () ->
              task {
                  let repo =
                      InMemRepository.createInMemoryRepository<
                          DemandBehaviourAssessment,
                          string,
                          DemandBehaviourAssessmentEvent
                       >()

                  let isHighPriority _ = task { return false }
                  let knowledge = ResizeArray<ArchitecturalKnowledge>()
                  let caps = create repo isHighPriority (fun k -> knowledge.Add k)

                  let! outcome =
                      caps.EvaluateSignal
                          { SkuId = "SKU-002"
                            StockingPointId = "SP-001"
                            SignalId = "sig-noise"
                            Source = "POS"
                            SourceReliability = 90m
                            Timestamp = DateTimeOffset.UtcNow
                            Value = 105m
                            StatisticalBound = 10m
                            RecentBaseline = 100m }

                  match outcome with
                  | Error err -> failwithf $"EvaluateSignal failed: %A{err}"
                  | Ok id ->
                      let! stateRes = repo.Get id
                      match stateRes with
                      | Error err -> failwithf "Failed to load state: %A" err
                      | Ok None -> failwith "State not found"
                      | Ok (Some state) ->
                          Expect.equal state.CurrentState Normal "Should remain Normal"

                          let decisionKnowledgeOpt =
                              knowledge
                              |> Seq.tryFind(fun k ->
                                  k.Name = "DecisionEvaluated" && k.Attributes.ContainsKey "DecisionTrace")

                          Expect.isSome decisionKnowledgeOpt "Decision trace should be published"
                          match decisionKnowledgeOpt with
                          | None -> failwith "Decision trace should be published"
                          | Some k ->
                              let traceOpt = k.Attributes.["DecisionTrace"] :?> DecisionTrace option
                              Expect.isSome traceOpt "DecisionTrace should be present"
                              match traceOpt with
                              | None -> failwith "Trace should be present"
                              | Some trace ->
                                  Expect.contains
                                      (trace.RulesEvaluated |> List.map fst)
                                      (ArsIdentifiers.Demand.Rules.noiseFilter)
                                      "Noise filter rule should be evaluated"
              } ]

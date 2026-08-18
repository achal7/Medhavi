/// SE-D-002 — Demand Understanding Aggregate Behaviors
/// Traces to: AB-D-003 (Revise), AB-D-004 (Publish)
module Medhavi.Demand.UnderstandDemand.DemandUnderstanding.Behaviors

open System
open Medhavi.Common
open Medhavi.Foundation.Contracts
open Medhavi.Foundation.Contracts.Decision
open Medhavi.Foundation.Failure
open Medhavi.SemanticModel
open Medhavi.Demand
open Medhavi.Demand.ArsIdentifiers
open Model
open Rules
open Decisions
open Policies
open Algorithms

/// Demand time anchor: the preferred need time, falling back to the latest acceptable need time.
let private timeAnchor (fact: DemandFact) : DateTimeOffset =
    match fact.NeedWindow.Preferred with
    | Some preferred -> Timestamp.value preferred
    | None -> Timestamp.value fact.NeedWindow.LatestAcceptable

/// Daily total demand series derived from the picture's demand facts.
let private dailySeries (facts: DemandFact list) : (DateTime * decimal) list =
    facts
    |> List.groupBy(fun f -> (timeAnchor f).Date)
    |> List.map(fun (day, dayFacts) -> day, dayFacts |> List.sumBy(fun f -> Quantity.value f.Quantity))
    |> List.sortBy fst

/// OLS simple linear regression; returns (slope, R2) with R2 clamped to [0..1].
let private fitTrend (points: (float * float) list) : float * float =
    if List.length points < 2 then
        0.0, 0.0
    else
        let xs = points |> List.map fst
        let ys = points |> List.map snd
        let xbar = List.average xs
        let ybar = List.average ys
        let sxx = xs |> List.sumBy(fun x -> (x - xbar) * (x - xbar))
        let sxy = List.zip xs ys |> List.sumBy(fun (x, y) -> (x - xbar) * (y - ybar))
        let slope = if sxx = 0.0 then 0.0 else sxy / sxx
        let intercept = ybar - slope * xbar
        let sst = ys |> List.sumBy(fun y -> (y - ybar) * (y - ybar))
        let sse = List.zip xs ys |> List.sumBy(fun (x, y) -> (y - (intercept + slope * x)) ** 2.0)
        let r2 = if sst = 0.0 then 0.0 else 1.0 - (sse / sst)
        slope, max 0.0 r2

/// Pearson correlation coefficient.
let private pearson (a: float list) (b: float list) : float =
    if List.length a < 2 then
        0.0
    else
        let abar = List.average a
        let bbar = List.average b
        let sxy = List.zip a b |> List.sumBy(fun (x, y) -> (x - abar) * (y - bbar))
        let sxx = a |> List.sumBy(fun x -> (x - abar) ** 2.0)
        let syy = b |> List.sumBy(fun y -> (y - bbar) ** 2.0)
        if sxx = 0.0 || syy = 0.0 then 0.0 else sxy / (sqrt sxx * sqrt syy)

/// Coefficient of variation.
let private coefficientOfVariation (values: float list) : float =
    if List.isEmpty values then
        0.0
    else
        let mean = List.average values

        if mean = 0.0 then
            0.0
        else
            sqrt(values |> List.averageBy(fun v -> (v - mean) ** 2.0)) / mean

/// Autocorrelation at a fixed lag over a daily series.
let private autocorrelation (series: float list) (lag: int) : float =
    let n = List.length series

    if n <= lag then
        0.0
    else
        pearson series.[0 .. n - lag - 1] series.[lag .. n - 1]

/// Step-change detection: relative mean shift between the first and second halves of the daily series.
let private hasStepChange (series: float list) : bool =
    let n = List.length series

    if n < 4 then
        false
    else
        let half = n / 2
        let firstMean = List.average series.[0 .. half - 1]
        let secondMean = List.average series.[half .. n - 1]
        let combinedMean = (firstMean + secondMean) / 2.0
        combinedMean <> 0.0 && abs(secondMean - firstMean) / abs combinedMean >= 0.5

/// Data completeness: fraction of days with any demand within the trailing 30-day window
/// ending at the picture's demand horizon (pure; no wall-clock dependency).
let private completenessRatio (days: DateTime list) : float =
    if List.isEmpty days then
        0.0
    else
        let horizonEnd = days |> List.max
        let windowStart = horizonEnd.AddDays(-29.0)
        let covered = days |> List.filter(fun d -> d >= windowStart) |> Set.ofList
        let totalDays = [ 0..29 ] |> List.map(fun i -> windowStart.AddDays(float i))
        float(totalDays |> List.filter(fun d -> Set.contains d covered) |> List.length) / 30.0

/// AB-D-003 — Interpret the demand facts referenced by the latest Published Enterprise Picture
/// into the four interpretation dimensions (BR-D-400).
let private interpret (facts: DemandFact list) : Interpretation =
    if List.isEmpty facts then
        { Continuity = Incomplete "NoDemandFactsInPicture"
          ContinuityDrivers = []
          Pattern = Incomplete "NoDemandFactsInPicture"
          PatternConfidence = Incomplete "NoDemandFactsInPicture"
          Health = Incomplete "NoDemandFactsInPicture"
          HealthConcerns = []
          Volatility = Incomplete "NoDemandFactsInPicture"
          VolatilityDrivers = []
          ReasonCodes = [ "NoDemandFactsInPicture" ] }
    else
        let daily = dailySeries facts
        let days = daily |> List.map fst
        let quantities = daily |> List.map snd |> List.map float
        let slope, r2 = daily |> List.map(fun (d, q) -> float(d.Ticks / TimeSpan.TicksPerDay), float q) |> fitTrend
        let cv = coefficientOfVariation quantities
        let seasonal = autocorrelation quantities 7 > 0.3
        let stepChange = hasStepChange quantities
        let completeness = completenessRatio days

        let continuityStatus, continuityDrivers =
            if cv > 1.5 then
                Volatile, [ "High coefficient of variation" ]
            elif r2 < 0.5 then
                Stable, []
            elif slope > 0.0 then
                Increasing, [ sprintf "Positive trend (slope %.2f, R2 %.2f)" slope r2 ]
            elif slope < 0.0 then
                Declining, [ sprintf "Negative trend (slope %.2f, R2 %.2f)" slope r2 ]
            else
                Stable, []

        let patternStatus =
            if stepChange then StepChange
            elif seasonal then Seasonal
            elif cv > 1.0 then Irregular
            else Normal

        let patternConfidence =
            if r2 >= 0.8 then ConfidenceLevel.High
            elif r2 >= 0.5 then ConfidenceLevel.Medium
            else ConfidenceLevel.Low

        let healthStatus, healthConcerns =
            if completeness < 0.5 then Critical, [ "Data completeness below 50%" ]
            elif completeness < 0.8 then AtRisk, [ "Data completeness below 80%" ]
            else Healthy, []

        let volatilityLevel, volatilityDrivers =
            if cv > 1.5 then VolatilityLevel.High, [ "High coefficient of variation" ]
            elif cv > 0.5 then VolatilityLevel.Medium, [ "Moderate variability" ]
            else VolatilityLevel.Low, []

        let reasonCodes =
            [ if r2 < 0.5 then
                  yield "LowTrendConfidence"
              if cv > 0.5 then
                  yield "HighVariability"
              if completeness < 0.8 then
                  yield "DataCompletenessLow"
              if seasonal then
                  yield "SeasonalityDetected"
              if stepChange then
                  yield "StepChangeDetected" ]

        { Continuity = Known continuityStatus
          ContinuityDrivers = continuityDrivers
          Pattern = Known patternStatus
          PatternConfidence = Known patternConfidence
          Health = Known healthStatus
          HealthConcerns = healthConcerns
          Volatility = Known volatilityLevel
          VolatilityDrivers = volatilityDrivers
          ReasonCodes = reasonCodes }

/// Records the DecisionTrace for an Aggregate Behaviour lifecycle outcome.
let buildTrace policy decisionId decisionOutcome state events summary =
    let policyId = policy |> Option.map(fun p -> p.PolicyId)
    let policyVersion = policy |> Option.map(fun p -> p.Version)

    buildDecisionWithTrace
        evolve
        state
        events
        decisionId
        []
        ArsIdentifiers.Capabilities.understandDemand.Id
        decisionOutcome
        policyId
        policyVersion
        [ ArsIdentifiers.SemanticObjects.demandUnderstanding.Id ]
        (Some summary)

/// AB-D-003 — Revise Demand Understanding
/// BR-D-002 (aggregate identity), BR-D-400 (derivation from the latest Published Enterprise Picture)
/// Publishes EV-D-003. Invokes no Decision.
let revise: Decide<DemandUnderstanding, ReviseCmd, DemandUnderstandingEvent> =
    fun (cmd: ReviseCmd) (stateOpt: DemandUnderstanding option) ->
        result {
            let interpretation = interpret cmd.PictureFacts.DemandFacts
            let previousPublished = stateOpt |> Option.bind(fun du -> du.CurrentPublishedVersion)

            match stateOpt with
            | None ->
                let draftVersion: DemandUnderstandingVersion =
                    { VersionNumber = 1
                      Interpretation = interpretation
                      EvidencePictureVersion = cmd.PictureFacts.PictureVersion
                      TransactionTime = cmd.TransactionTime
                      PublicationTime = None
                      State = Draft }

                let du: DemandUnderstanding =
                    { PlanningScopeId = cmd.PlanningScopeId
                      Versions = [ draftVersion ]
                      CurrentPublishedVersion = None }

                let events = [ DemandUnderstandingRevised(du, None) ]
                let decision: DecisionOutcome<RevisionOutcome> = { Outcome = Revised; Evaluations = [] }

                return
                    buildTrace
                        None
                        (ArsIdentifiers.Capabilities.understandDemand.Id)
                        decision
                        None
                        events
                        "Demand Understanding revised"
            | Some du ->
                let existingDraft = du.Versions |> List.tryFind(fun v -> v.State = Draft)

                match existingDraft with
                | Some draft when
                    draft.EvidencePictureVersion = cmd.PictureFacts.PictureVersion
                    && draft.Interpretation = interpretation
                    ->
                    // FS-D-003: re-execution with the same Enterprise Picture version produces the same Draft.
                    return buildDecision evolve (Some du) [] None
                | _ ->
                    let updatedDraft =
                        match existingDraft with
                        | Some draft ->
                            { draft with
                                Interpretation = interpretation
                                EvidencePictureVersion = cmd.PictureFacts.PictureVersion
                                TransactionTime = cmd.TransactionTime }
                        | None ->
                            let nextVersionNumber = (du.Versions |> List.map(fun v -> v.VersionNumber) |> List.max) + 1

                            { VersionNumber = nextVersionNumber
                              Interpretation = interpretation
                              EvidencePictureVersion = cmd.PictureFacts.PictureVersion
                              TransactionTime = cmd.TransactionTime
                              PublicationTime = None
                              State = Draft }

                    let updatedVersions =
                        match existingDraft with
                        | Some draft ->
                            du.Versions
                            |> List.map(fun v -> if v.VersionNumber = draft.VersionNumber then updatedDraft else v)
                        | None -> updatedDraft :: du.Versions

                    let newDU = { du with Versions = updatedVersions }
                    let events = [ DemandUnderstandingRevised(newDU, previousPublished) ]
                    let decision: DecisionOutcome<RevisionOutcome> = { Outcome = Revised; Evaluations = [] }

                    return
                        buildTrace
                            None
                            (ArsIdentifiers.Capabilities.understandDemand.Id)
                            decision
                            None
                            events
                            "Demand Understanding revised"
        }

/// AB-D-004 — Publish Demand Understanding
/// BR-D-103 (single Published version), BR-D-104 (Published immutability)
/// Invokes DE-D-002 and BA-D-001; publishes EV-D-004 when the decision is Publish.
let publish
    (materialityPolicy: MaterialityPolicy)
    (cadencePolicy: CadencePolicy)
    : Decide<DemandUnderstanding, PublishCmd, DemandUnderstandingEvent> =
    fun (cmd: PublishCmd) (stateOpt: DemandUnderstanding option) ->
        result {
            match stateOpt with
            | None ->
                return! Error(DomainError.notFound("DemandUnderstanding", PlanningScopeId.value cmd.PlanningScopeId))
            | Some du ->
                match du.Versions |> List.tryFind(fun v -> v.State = Draft) with
                | None -> return! Error(DomainError.validation "No Draft version to publish")
                | Some draft ->
                    let published = du.Versions |> List.tryFind(fun v -> v.State = Published)

                    let lastPublicationTime =
                        published |> Option.bind(fun v -> v.PublicationTime) |> Option.defaultValue cmd.PublicationTime

                    let periodicRefreshDue =
                        cmd.IsPeriodicRefresh
                        || (Timestamp.value cmd.PublicationTime) - (Timestamp.value lastPublicationTime)
                           >= cadencePolicy.MaxPublicationInterval

                    let input: PublicationInput =
                        { Assessment =
                            Algorithms.evaluateMateriality
                                materialityPolicy
                                { Draft = draft.Interpretation
                                  Published = published |> Option.map(fun v -> v.Interpretation)
                                  ContinuityChangeMagnitudePercent = None }
                          PeriodicRefreshDue = periodicRefreshDue
                          Interpretation = draft.Interpretation
                          CompletenessThreshold = materialityPolicy.InterpretationCompletenessThreshold }

                    let! decisionOutcome = Decisions.evaluatePublication Rules.publicationRules input

                    match decisionOutcome.Outcome with
                    | DoNotPublish ->
                        let reason =
                            decisionOutcome.Evaluations
                            |> List.filter(fun e -> not e.Passed)
                            |> List.map(fun e -> e.ReasonCode |> Option.defaultValue e.RuleId)
                            |> String.concat "; "

                        return! Error(DomainError.validation $"Publication criteria not met: {reason}")
                    | Publish ->
                        let now = cmd.PublicationTime

                        let publishedVersion =
                            { draft with
                                State = Published
                                PublicationTime = Some now }

                        let supersededPrevious = published |> Option.map(fun v -> { v with State = Superseded })

                        let updatedVersions =
                            du.Versions
                            |> List.map(fun v ->
                                if v.VersionNumber = draft.VersionNumber then
                                    publishedVersion
                                elif
                                    supersededPrevious.IsSome
                                    && v.VersionNumber = supersededPrevious.Value.VersionNumber
                                then
                                    supersededPrevious.Value
                                else
                                    v)

                        let newDU =
                            { du with
                                Versions = updatedVersions
                                CurrentPublishedVersion = Some draft.VersionNumber }

                        let events =
                            [ DemandUnderstandingPublished(
                                  newDU,
                                  published |> Option.map(fun v -> v.VersionNumber),
                                  now
                              ) ]

                        let traceId = Guid.NewGuid().ToString()

                        return
                            buildDecisionWithTrace
                                evolve
                                (Some du)
                                events
                                traceId
                                []
                                ArsIdentifiers.Capabilities.understandDemand.Id
                                decisionOutcome
                                (Some materialityPolicy.PolicyId)
                                (Some materialityPolicy.Version)
                                [ ArsIdentifiers.SemanticObjects.demandUnderstanding.Id ]
                                (Some "Demand Understanding published")
        }

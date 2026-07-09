module Medhavi.Demand.ForecastPublication.ACL

open System
open Medhavi.Common
open Medhavi.Common.Validation
open Medhavi.Contracts.Demand.ForecastPublication
open Medhavi.SharedKernel
open Medhavi.SharedKernel.Failure
open Medhavi.Demand
open Medhavi.Demand.ForecastPublication.Model

let private notEmpty field =
    validate (fun s -> not(String.IsNullOrWhiteSpace s)) (DomainError.validation(field, $"{field} cannot be empty"))

let toInitiateCmd (req: InitiateForecastCycleReq) : Validation<InitiateForecastCycleCmd, DomainError> =
    let make pubId scopeIds timeBucket =
        { PublicationId = pubId
          PlanningScopeIds = scopeIds
          ForecastHorizon = TimeSpan.Parse req.ForecastHorizon
          TimeBucketConfig = timeBucket }

    make <!> (ForecastPublicationId.create req.PublicationId |> fromResult)
    <*> (req.PlanningScopeIds |> traverse(PlanningScopeId.fromString >> fromResult))
    <*> notEmpty "TimeBucketConfig" req.TimeBucketConfig

let toPrepareContextCmd (req: PrepareForecastContextReq) : Validation<PrepareForecastContextCmd, DomainError> =
    let make pubId assumptions coverage =
        { PublicationId = pubId
          Assumptions = assumptions
          Coverage = coverage }

    let assumptionMap =
        req.Assumptions
        |> List.map(fun a ->
            { AssumptionId = a.AssumptionId
              Statement = a.Statement
              DeclaredBy = a.DeclaredBy
              LifecycleState =
                match a.LifecycleState with
                | "Declared" -> Declared
                | "Validated" -> Validated
                | "Approved" -> Approved
                | "Withdrawn" -> Withdrawn
                | _ -> Declared
              LinkedDriverRef = a.LinkedDriverRef
              Timestamp = Timestamp.now })

    let coverageMap =
        req.Coverage
        |> traverse(fun c ->
            let skuV = SkuId.create c.SkuId |> fromResult
            let spV = StockingPointId.create c.StockingPointId |> fromResult
            (fun sku sp -> (sku, sp)) <!> skuV <*> spV)

    make <!> (ForecastPublicationId.create req.PublicationId |> fromResult) <*> (Valid assumptionMap) <*> coverageMap

let toSelectChampionCmd (req: SelectChampionModelReq) : Validation<SelectChampionModelCmd, DomainError> =
    let make pubId candidateModelId =
        { PublicationId = pubId
          CandidateModelId = candidateModelId
          EvaluationWindowStart = Timestamp.create req.EvaluationWindowStart
          EvaluationWindowEnd = Timestamp.create req.EvaluationWindowEnd }

    make <!> (ForecastPublicationId.create req.PublicationId |> fromResult)
    <*> notEmpty "CandidateModelId" req.CandidateModelId

let toGenerateBaselineCmd (req: GenerateBaselineForecastsReq) : Validation<GenerateBaselineForecastsCmd, DomainError> =
    let pubIdV = ForecastPublicationId.create req.PublicationId |> fromResult

    let forecastsV =
        match req.Forecasts with
        | None -> Invalid [ DomainError.validation "Forecasts must be provided" ]
        | Some dtos ->
            dtos
            |> List.map(fun dto ->
                result {
                    let! fId = ForecastId.create dto.ForecastId
                    let! sku = SkuId.create dto.SkuId
                    let! sp = StockingPointId.create dto.StockingPointId
                    let! lower = PositiveDecimal.create dto.LowerBound
                    let! upper = PositiveDecimal.create dto.UpperBound
                    let! confLevel = PositiveDecimal.create dto.Confidence
                    let! confidence = PositiveDecimal.create dto.Confidence

                    return
                        { ForecastId = fId
                          SkuId = sku
                          StockingPointId = sp
                          PlanningPeriod = dto.PlanningPeriod
                          Mean = dto.Mean
                          PredictionInterval =
                            { LowerBound = lower
                              UpperBound = upper
                              ConfidenceLevel = confLevel }
                          Confidence = confidence
                          ModelId = dto.ModelId
                          GeneratedAt = Timestamp.now
                          OverrideReason = dto.OverrideReason }
                })
            |> List.map fromResult
            |> sequence
            |> map id

    (fun pubId forecasts ->
        { PublicationId = pubId
          Forecasts = forecasts })
    <!> pubIdV
    <*> forecastsV

let toRecordOverrideCmd (req: RecordForecastOverrideReq) : Validation<RecordForecastOverrideCmd, DomainError> =
    let make pubId fId newValue justification plannerId =
        { PublicationId = pubId
          ForecastId = fId
          NewValue = newValue
          Justification = justification
          PlannerIdentity = plannerId }

    make <!> (ForecastPublicationId.create req.PublicationId |> fromResult)
    <*> (ForecastId.create req.ForecastId |> fromResult)
    <*> (PositiveDecimal.create req.NewValue |> fromResult)
    <*> notEmpty "Justification" req.Justification
    <*> notEmpty "PlannerIdentity" req.PlannerIdentity

let toReconcileCmd (req: ReconcileForecastHierarchyReq) : Validation<ReconcileForecastHierarchyCmd, DomainError> =
    let make pubId =
        { PublicationId = pubId
          TargetTotal = req.TargetTotal }
    make <!> (ForecastPublicationId.create req.PublicationId |> fromResult)

let toPublishCmd (req: PublishForecastPublicationReq) : Validation<PublishForecastPublicationCmd, DomainError> =
    let make pubId = { PublicationId = pubId }
    make <!> (ForecastPublicationId.create req.PublicationId |> fromResult)

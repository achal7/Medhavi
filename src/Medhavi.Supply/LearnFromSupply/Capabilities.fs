// Traceability: CA-S-013 Public Capability API
module Medhavi.Supply.LearnFromSupply.Capabilities

open System.Threading.Tasks
open Medhavi.Contracts
open Medhavi.Contracts.Supply
open Medhavi.SemanticModel
open Medhavi.Supply
open Medhavi.Supply.LearnFromSupply.SupplyLearning

let create (aggregateApi: SupplyLearning.AggregateApi) : SupplyLearningApi =

    let deriveLearnings (req: DeriveSupplyLearningsReq) : Task<Result<CandidateSupplyLearningDto list, ApiError>> =
        task {
            let! result = aggregateApi.DeriveAndRecordAll req

            match result with
            | Ok learnings ->
                let dtos =
                    learnings
                    |> List.map(fun l ->
                        {
                            PatternReference = l.PatternDescription
                            SubjectType = l.SubjectType
                            SubjectId = l.SubjectId
                            LearningType = l.LearningType.AsString
                            LearningScope = l.Scope.AsString
                            TimeHorizonStart =
                                (l.TimeHorizon.Earliest |> Option.defaultValue l.TimeHorizon.Latest |> Timestamp.value)
                            TimeHorizonEnd = Timestamp.value l.TimeHorizon.Latest
                            LearningStatement = l.LearningStatement
                            PatternConfidence = l.PatternConfidence.AsString
                            PatternConfidenceScore = l.PatternConfidenceScore
                            RecurrenceCount = l.RecurrenceCount
                            RecurrenceFrequency = l.RecurrenceFrequency
                            StatisticalSignificance = l.StatisticalSignificance
                            SupportingEvidence = l.SupportingEvidence |> List.map Projections.mapEvidenceRefToDto
                            ImprovementOpportunities =
                                l.ImprovementOpportunities |> List.map Projections.mapOpportunityToDto
                            DerivedTimestamp = Timestamp.value l.GenerationTimestamp
                        })

                return Ok dtos
            | Error appErr -> return Error(ApiError.businessLogic(sprintf "%A" appErr))
        }

    let recordLearning (req: RecordSupplyLearningReq) : Task<Result<SupplyLearningDto, ApiError>> =
        task {
            let! result = aggregateApi.RecordLearning req

            match result with
            | Ok learning -> return Ok(Projections.mapToDto learning)
            | Error appErr -> return Error(ApiError.businessLogic(sprintf "%A" appErr))
        }

    let recordOutcome (req: RecordLearningOutcomeReq) : Task<Result<SupplyLearningDto, ApiError>> =
        task {
            let! result = aggregateApi.RecordOutcome req

            match result with
            | Ok learning -> return Ok(Projections.mapToDto learning)
            | Error appErr -> return Error(ApiError.businessLogic(sprintf "%A" appErr))
        }

    let getById (idStr: string) : Task<Result<SupplyLearningDto option, ApiError>> =
        task {
            match SupplyLearningId.create idStr with
            | Ok learningId ->
                let! result = aggregateApi.GetById learningId

                match result with
                | Ok learningOpt -> return Ok(learningOpt |> Option.map Projections.mapToDto)
                | Error appErr -> return Error(ApiError.businessLogic(sprintf "%A" appErr))
            | Error domainErr -> return Error(ApiError.validation(sprintf "%A" domainErr))
        }

    {
        DeriveLearnings = deriveLearnings
        RecordLearning = recordLearning
        RecordOutcome = recordOutcome
        GetById = getById
    }

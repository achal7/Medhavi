// Traceability: SE-S-130 Aggregate Execution Pipeline & Capabilities
namespace Medhavi.Supply.LearnFromSupply.SupplyLearning

open System.Threading.Tasks
open Medhavi.Common
open Medhavi.Foundation.Contracts
open Medhavi.Foundation.Execution
open Medhavi.Foundation.Execution.CommandCapabilities
open Medhavi.Foundation.Failure
open Medhavi.Foundation.Observation
open Medhavi.SemanticModel
open Medhavi.Contracts
open Medhavi.Contracts.Supply
open Medhavi.Supply
open Model
open Policies

type AggregateApi =
    {
        RecordLearning: RecordSupplyLearningReq -> Task<Result<SupplyLearning, ApplicationError>>
        RecordOutcome: RecordLearningOutcomeReq -> Task<Result<SupplyLearning, ApplicationError>>
        DeriveAndRecordAll: DeriveSupplyLearningsReq -> Task<Result<SupplyLearning list, ApplicationError>>
        GetById: SupplyLearningId -> Task<Result<SupplyLearning option, ApplicationError>>
    }

module Capabilities =

    let create
        (repo: Repository<SupplyLearning, SupplyLearningId, SupplyLearningEvent>)
        (publishKnowledge: KnowledgeRepresentation)
        (policy: LearningPolicy)
        (deps: AggregateStages.EnvelopeStoreDependencies<SupplyLearningEvent>)
        : AggregateApi =

        let recordLearning (req: RecordSupplyLearningReq) : Task<Result<SupplyLearning, ApplicationError>> =
            taskResult {
                let! cmd: RecordSupplyLearningCmd = liftValidation(ACL.toRecordCmd req)

                let pipeline =
                    CommandPipeline.create
                        repo
                        (fun (c: RecordSupplyLearningCmd) -> c.LearningId)
                        (Behaviors.recordLearning policy)
                        deps

                return! runPipeline pipeline publishKnowledge id cmd
            }

        let recordOutcome (req: RecordLearningOutcomeReq) : Task<Result<SupplyLearning, ApplicationError>> =
            taskResult {
                let! cmd: RecordLearningOutcomeCmd = liftValidation(ACL.toOutcomeCmd req)

                let pipeline =
                    CommandPipeline.create
                        repo
                        (fun (c: RecordLearningOutcomeCmd) -> c.LearningId)
                        (Behaviors.recordOutcome policy)
                        deps

                return! runPipeline pipeline publishKnowledge id cmd
            }

        let deriveAndRecordAll (req: DeriveSupplyLearningsReq) : Task<Result<SupplyLearning list, ApplicationError>> =
            taskResult {
                let! corpus = liftValidation(ACL.toEvidenceCorpus req)
                let now = Timestamp.now()
                let candidates = Algorithms.deriveSupplyLearnings corpus policy now

                let rec recordIter
                    (acc: SupplyLearning list)
                    (remaining: Algorithms.CandidateSupplyLearning list)
                    : Task<Result<SupplyLearning list, ApplicationError>> =
                    taskResult {
                        match remaining with
                        | [] -> return List.rev acc
                        | cand :: tail ->
                            let learningId =
                                SupplyLearningId.ofComponents
                                    cand.SubjectType
                                    cand.SubjectId
                                    cand.LearningType.AsString
                                    1L

                            let recordReq: RecordSupplyLearningReq =
                                {
                                    LearningId = SupplyLearningId.value learningId
                                    SubjectType = cand.SubjectType
                                    SubjectId = cand.SubjectId
                                    LearningType = cand.LearningType.AsString
                                    Scope = cand.Scope.AsString
                                    TimeHorizonStart =
                                        (cand.TimeHorizon.Earliest
                                         |> Option.defaultValue cand.TimeHorizon.Latest
                                         |> Timestamp.value)
                                    TimeHorizonEnd = Timestamp.value cand.TimeHorizon.Latest
                                    PatternDescription = cand.LearningStatement
                                    RecurrenceCount = cand.RecurrenceCount
                                    RecurrenceFrequency = cand.RecurrenceFrequency
                                    StatisticalSignificance = cand.StatisticalSignificance
                                    LearningStatement = cand.LearningStatement
                                    PatternConfidence = cand.PatternConfidence.AsString
                                    PatternConfidenceScore = cand.PatternConfidenceScore
                                    ImprovementOpportunities =
                                        cand.ImprovementOpportunities |> List.map Projections.mapOpportunityToDto
                                    SupportingEvidence =
                                        cand.SupportingEvidence |> List.map Projections.mapEvidenceRefToDto
                                    SourceEvidenceVersionSet = []
                                    PolicyVersion = policy.PolicyId
                                }

                            let! recorded = recordLearning recordReq
                            return! recordIter (recorded :: acc) tail
                    }

                return! recordIter [] candidates
            }

        let getById (id: SupplyLearningId) : Task<Result<SupplyLearning option, ApplicationError>> =
            repo.Get id
            |> TaskResult.ofTask
            |> TaskResult.mapError(fun repoErr -> Infrastructure(Database(sprintf "%A" repoErr)))

        {
            RecordLearning = recordLearning
            RecordOutcome = recordOutcome
            DeriveAndRecordAll = deriveAndRecordAll
            GetById = getById
        }

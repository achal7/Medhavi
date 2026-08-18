/// SE-D-011 Demand Learning Child Aggregate Capabilities API
module Medhavi.Demand.LearnFromDemand.DemandLearning.Capabilities

open System.Threading.Tasks
open Medhavi.Common
open Medhavi.Foundation.Contracts
open Medhavi.Foundation.Execution
open Medhavi.Foundation.Execution.CommandCapabilities
open Medhavi.Foundation.Failure
open Medhavi.Foundation.Observation
open Medhavi.SemanticModel
open Medhavi.Contracts.Demand
open Medhavi.Demand
open Model
open Policies

type AggregateApi =
    { Establish: RecordDemandLearningReq -> Task<Result<DemandLearning, ApplicationError>>
      DeriveAndEstablishAll: DeriveDemandLearningsReq -> Task<Result<DemandLearning list, ApplicationError>>
      GetById: DemandLearningId -> Task<Result<DemandLearning option, ApplicationError>> }

let create
    (repo: Repository<DemandLearning, DemandLearningId, DemandLearningEvent>)
    (publishKnowledge: KnowledgeRepresentation)
    (policy: LearningAnalysisPolicy)
    (deps: AggregateStages.EnvelopeStoreDependencies<DemandLearningEvent>)
    (ports: DemandPorts)
    : AggregateApi =

    let establish (req: RecordDemandLearningReq) : Task<Result<DemandLearning, ApplicationError>> =
        taskResult {
            // Step 1: Lift applicative validation
            let! cmd: EstablishLearningCmd = liftValidation (ACL.toRecordCmd req)

            // Step 2: Ensure Planning Scope exists
            do! requireEntityExists ports.PlanningScopeExists cmd.Scope "PlanningScope" PlanningScopeId.value

            // Step 3: Execute pipeline
            let pipeline =
                CommandPipeline.create
                    repo
                    (fun (c: EstablishLearningCmd) -> c.LearningId)
                    (Behaviors.establishLearning policy)
                    deps

            return! runPipeline pipeline publishKnowledge id cmd
        }

    let deriveAndEstablishAll (req: DeriveDemandLearningsReq) : Task<Result<DemandLearning list, ApplicationError>> =
        taskResult {
            let! bundle: HistoricalDemandEvidenceBundle = liftValidation (ACL.toEvidenceBundle req)
            do! requireEntityExists ports.PlanningScopeExists bundle.Scope "PlanningScope" PlanningScopeId.value

            let now = Timestamp.now ()
            let derivedLearnings = Algorithms.deriveDemandLearnings bundle policy now

            let rec recordAll (acc: DemandLearning list) (remaining: DemandLearning list) : Task<Result<DemandLearning list, ApplicationError>> =
                taskResult {
                    match remaining with
                    | [] -> return List.rev acc
                    | learning :: tail ->
                        let recordReq: RecordDemandLearningReq =
                            { LearningId = DemandLearningId.value learning.Id
                              Scope = PlanningScopeId.value learning.Scope
                              LearningType = learning.LearningType.AsString
                              LearningStatement = learning.LearningStatement
                              PatternConfidence = learning.PatternConfidence
                              InterventionConfidence = learning.InterventionConfidence
                              SupportingEvidence = learning.SupportingEvidence |> List.map Projections.mapEvidenceRefToDto
                              ImprovementOpportunities = learning.ImprovementOpportunities |> List.map Projections.mapOpportunityToDto
                              PolicyVersion = learning.PolicyVersion }

                        let! recorded = establish recordReq
                        return! recordAll (recorded :: acc) tail
                }

            return! recordAll [] derivedLearnings
        }

    let getById (id: DemandLearningId) : Task<Result<DemandLearning option, ApplicationError>> =
        repo.Get id
        |> TaskResult.ofTask
        |> TaskResult.mapError (fun repoErr -> Infrastructure(Database(sprintf "%A" repoErr)))

    { Establish = establish
      DeriveAndEstablishAll = deriveAndEstablishAll
      GetById = getById }

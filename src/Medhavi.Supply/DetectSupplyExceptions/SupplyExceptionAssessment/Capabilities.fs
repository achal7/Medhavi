/// CA-S-011 Detect Supply Exceptions — Aggregate Capabilities
/// Traces to: CR-S-023, AB-S-100, FS-S-100
module Medhavi.Supply.DetectSupplyExceptions.SupplyExceptionAssessment.Capabilities

open System
open System.Threading.Tasks
open Medhavi.Common
open Medhavi.Foundation.Contracts
open Medhavi.Foundation.Execution
open Medhavi.Foundation.Execution.CommandCapabilities
open Medhavi.Foundation.Failure
open Medhavi.Foundation.Observation
open Medhavi.SemanticModel
open Medhavi.Contracts.Supply
open Medhavi.Supply
open Model
open Policies
open ACL

type SupplyExceptionAssessmentAggregateApi =
    { Evaluate: EvaluateSupplyExceptionReq -> Task<Result<SupplyExceptionAssessment, ApplicationError>>
      Resolve: SupplyExceptionId * string * DateTimeOffset -> Task<Result<SupplyExceptionAssessment, ApplicationError>>
      Archive: SupplyExceptionId * string * DateTimeOffset -> Task<Result<SupplyExceptionAssessment, ApplicationError>> }

/// Instantiates aggregate API wired with governed command pipeline
let create
    (repo: Repository<SupplyExceptionAssessment, SupplyExceptionId, SupplyExceptionEvent>)
    (publishKnowledge: KnowledgeRepresentation)
    (policy: ExceptionDetectionPolicy)
    (deps: AggregateStages.EnvelopeStoreDependencies<SupplyExceptionEvent>)
    (ports: SupplyPorts)
    : SupplyExceptionAssessmentAggregateApi =

    let pipeline =
        CommandPipeline.create
            repo
            (fun (c: SupplyExceptionCommand) ->
                match c with
                | SupplyExceptionCommand.DetectException d -> d.Id
                | SupplyExceptionCommand.CorroborateException d -> d.Id
                | SupplyExceptionCommand.UpdateExceptionEvidence d -> d.Id
                | SupplyExceptionCommand.ResolveException d -> d.Id
                | SupplyExceptionCommand.ArchiveException d -> d.Id)
            (Behaviors.decide policy)
            deps

    /// FS-S-100: Evaluate Scope for Supply Exceptions
    let evaluate (rawReq: EvaluateSupplyExceptionReq) : Task<Result<SupplyExceptionAssessment, ApplicationError>> =
        taskResult {
            let! req: ValidatedExceptionEvaluationReq = liftValidation (ACL.toEvaluationReq rawReq)

            do! requireEntityExists ports.PlanningScopeExists req.PlanningScopeId "PlanningScope" PlanningScopeId.value

            let! prereqsOk: bool = protect (ports.GetScopeExceptionPrerequisites req.PlanningScopeId)
            let! changes: SupplyChangeEvidenceFact list = protect (ports.GetSupplyChangeEvidence req.PlanningScopeId)
            let! qualityOpt: QualityEvidenceFact option = protect (ports.GetQualityEvidence req.PlanningScopeId)
            let! positions: OperationalPositionEvidenceFact list = protect (ports.GetOperationalEvidence req.PlanningScopeId)
            let! activeExceptions: ActiveExceptionFact list = protect (ports.GetActiveExceptionsForScope req.PlanningScopeId)

            let assessmentResult =
                Algorithms.evaluateSupplyException
                    policy
                    req.Scope
                    changes
                    qualityOpt
                    positions
                    activeExceptions
                    req.EvaluationTimestamp

            let scopeKey = ExceptionScope.toKey req.Scope
            let exceptionId = SupplyExceptionId.ofBusinessIdentity req.GovernedPolicyReference scopeKey (req.ExceptionType.ToString())

            let! existingOpt: SupplyExceptionAssessment option =
                repo.Get exceptionId
                |> TaskResult.ofTask
                |> TaskResult.mapError (fun e -> Infrastructure(Database(sprintf "%A" e)))

            match existingOpt with
            | None ->
                match assessmentResult.Determination with
                | Algorithms.ExceptionDetermination.ExceptionExists(exType, reason) ->
                    let cmd =
                        SupplyExceptionCommand.DetectException {|
                            Id = exceptionId
                            GovernedPolicyReference = req.GovernedPolicyReference
                            ViolatedRuleId = "PO-S-120"
                            ExceptionType = exType
                            ExceptionScope = req.Scope
                            PlanningScopeId = req.PlanningScopeId
                            Severity = assessmentResult.Severity
                            Horizon = assessmentResult.Horizon
                            Evidence = assessmentResult.EvidenceLineage
                            Confidence = assessmentResult.Confidence
                            ReasonCodes = assessmentResult.ReasonCodes
                            CorrelatedExceptionIds = assessmentResult.CorrelatedExceptionIds
                            Rationale = reason
                            Timestamp = req.EvaluationTimestamp
                        |}
                    return! runPipeline pipeline publishKnowledge id cmd
                | Algorithms.ExceptionDetermination.NoException _ ->
                    return! TaskResult.fail (Domain(DomainError.notFound("SupplyExceptionAssessment", SupplyExceptionId.value exceptionId)))

            | Some current ->
                match current.LifecycleState with
                | SupplyExceptionLifecycleState.Detected ->
                    match assessmentResult.Determination with
                    | Algorithms.ExceptionDetermination.ExceptionExists(_, _) ->
                        let cmd =
                            SupplyExceptionCommand.CorroborateException {|
                                Id = exceptionId
                                CorroboratingEvidence = assessmentResult.EvidenceLineage
                                Severity = assessmentResult.Severity
                                Horizon = assessmentResult.Horizon
                                Confidence = assessmentResult.Confidence
                                ReasonCodes = assessmentResult.ReasonCodes
                                Rationale = "Corroborated by continuous evaluation"
                                Timestamp = req.EvaluationTimestamp
                            |}
                        return! runPipeline pipeline publishKnowledge id cmd
                    | Algorithms.ExceptionDetermination.NoException reason ->
                        let resolutionEvidence: EvidenceReference list =
                            [ { AssessmentId = Guid.NewGuid().ToString()
                                AssessmentType = "OperationalMetrics"
                                Description = sprintf "Return to compliance: %s" reason
                                Confidence = 1.0m
                                Timestamp = req.EvaluationTimestamp } ]
                        let cmd =
                            SupplyExceptionCommand.ResolveException {|
                                Id = exceptionId
                                ResolutionEvidence = resolutionEvidence
                                Rationale = "Underlying conditions returned to compliance"
                                Timestamp = req.EvaluationTimestamp
                            |}
                        return! runPipeline pipeline publishKnowledge id cmd

                | SupplyExceptionLifecycleState.Confirmed ->
                    match assessmentResult.Determination with
                    | Algorithms.ExceptionDetermination.ExceptionExists(_, _) ->
                        let cmd =
                            SupplyExceptionCommand.UpdateExceptionEvidence {|
                                Id = exceptionId
                                UpdatedEvidence = assessmentResult.EvidenceLineage
                                Severity = assessmentResult.Severity
                                Horizon = assessmentResult.Horizon
                                Confidence = assessmentResult.Confidence
                                ReasonCodes = assessmentResult.ReasonCodes
                                Rationale = "Evidence and severity updated"
                                Timestamp = req.EvaluationTimestamp
                            |}
                        return! runPipeline pipeline publishKnowledge id cmd
                    | Algorithms.ExceptionDetermination.NoException reason ->
                        let resolutionEvidence: EvidenceReference list =
                            [ { AssessmentId = Guid.NewGuid().ToString()
                                AssessmentType = "OperationalMetrics"
                                Description = sprintf "Return to compliance: %s" reason
                                Confidence = 1.0m
                                Timestamp = req.EvaluationTimestamp } ]
                        let cmd =
                            SupplyExceptionCommand.ResolveException {|
                                Id = exceptionId
                                ResolutionEvidence = resolutionEvidence
                                Rationale = "Underlying conditions returned to compliance"
                                Timestamp = req.EvaluationTimestamp
                            |}
                        return! runPipeline pipeline publishKnowledge id cmd

                | SupplyExceptionLifecycleState.Resolved ->
                    match assessmentResult.Determination with
                    | Algorithms.ExceptionDetermination.ExceptionExists(exType, reason) ->
                        // Archive previously resolved exception before re-detecting
                        let archiveCmd =
                            SupplyExceptionCommand.ArchiveException {|
                                Id = exceptionId
                                Rationale = "Superseded by new occurrence"
                                Timestamp = req.EvaluationTimestamp
                            |}
                        let! (_: SupplyExceptionAssessment) = runPipeline pipeline publishKnowledge id archiveCmd

                        let detectCmd =
                            SupplyExceptionCommand.DetectException {|
                                Id = exceptionId
                                GovernedPolicyReference = req.GovernedPolicyReference
                                ViolatedRuleId = "PO-S-120"
                                ExceptionType = exType
                                ExceptionScope = req.Scope
                                PlanningScopeId = req.PlanningScopeId
                                Severity = assessmentResult.Severity
                                Horizon = assessmentResult.Horizon
                                Evidence = assessmentResult.EvidenceLineage
                                Confidence = assessmentResult.Confidence
                                ReasonCodes = assessmentResult.ReasonCodes
                                CorrelatedExceptionIds = assessmentResult.CorrelatedExceptionIds
                                Rationale = reason
                                Timestamp = req.EvaluationTimestamp
                            |}
                        return! runPipeline pipeline publishKnowledge id detectCmd
                    | Algorithms.ExceptionDetermination.NoException _ ->
                        return current

                | SupplyExceptionLifecycleState.Archived ->
                    return! TaskResult.fail (Domain(DomainError.validation "Archived exceptions are terminal and immutable"))
        }

    let resolve (exId: SupplyExceptionId, rationale: string, timestamp: DateTimeOffset) : Task<Result<SupplyExceptionAssessment, ApplicationError>> =
        taskResult {
            let ts = Timestamp.create timestamp |> Result.defaultValue (Timestamp.now())
            let resolutionEvidence: EvidenceReference list =
                [ { AssessmentId = Guid.NewGuid().ToString()
                    AssessmentType = "ResolutionEvidence"
                    Description = rationale
                    Confidence = 1.0m
                    Timestamp = ts } ]

            let cmd =
                SupplyExceptionCommand.ResolveException {|
                    Id = exId
                    ResolutionEvidence = resolutionEvidence
                    Rationale = rationale
                    Timestamp = ts
                |}
            return! runPipeline pipeline publishKnowledge id cmd
        }

    let archive (exId: SupplyExceptionId, rationale: string, timestamp: DateTimeOffset) : Task<Result<SupplyExceptionAssessment, ApplicationError>> =
        taskResult {
            let ts = Timestamp.create timestamp |> Result.defaultValue (Timestamp.now())
            let cmd =
                SupplyExceptionCommand.ArchiveException {|
                    Id = exId
                    Rationale = rationale
                    Timestamp = ts
                |}
            return! runPipeline pipeline publishKnowledge id cmd
        }

    { Evaluate = evaluate
      Resolve = resolve
      Archive = archive }

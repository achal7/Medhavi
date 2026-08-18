/// SE-D-004 Demand Behavior Assessment Child Aggregate Capabilities API
/// Operates strictly within ApplicationError domain (no ApiError).
module Medhavi.Demand.SenseDemand.DemandBehaviorAssessment.Capabilities

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
    { InitializeBaseline: InitializeBaselineReq -> Task<Result<DemandBehaviorAssessment, ApplicationError>>
      EvaluateSignal: EvaluateDemandSignalReq -> Task<Result<DemandBehaviorAssessment, ApplicationError>>
      EvaluateForecastRefresh: EvaluateForecastRefreshReq -> Task<Result<ForecastRefreshDecisionDto, ApplicationError>> }

let create
    (repo: Repository<DemandBehaviorAssessment, DemandBehaviorAssessmentId, DemandBehaviorEvent>)
    (publishKnowledge: KnowledgeRepresentation)
    (sensingPolicy: DemandSensingPolicy)
    (triggerPolicy: ForecastRefreshTriggerPolicy)
    (deps: AggregateStages.EnvelopeStoreDependencies<DemandBehaviorEvent>)
    (ports: DemandPorts)
    : AggregateApi =

    let initializeBaseline (req: InitializeBaselineReq) : Task<Result<DemandBehaviorAssessment, ApplicationError>> =
        taskResult {
            // Step 1: Lift applicative validation
            let! cmd: InitializeBaselineCmd = liftValidation(ACL.toInitializeBaselineCmd req)

            // Step 2: Ensure Item and Location exist
            do! requireEntityExists ports.ItemExists cmd.Item "Item" ItemId.value
            do! requireEntityExists ports.LocationExists cmd.Location "Location" LocationId.value

            // Step 3: Execute pipeline
            let pipeline =
                CommandPipeline.create
                    repo
                    (fun (c: InitializeBaselineCmd) -> c.AssessmentId)
                    Behaviors.initializeBaseline
                    deps

            return! runPipeline pipeline publishKnowledge id cmd
        }

    let evaluateSignal (req: EvaluateDemandSignalReq) : Task<Result<DemandBehaviorAssessment, ApplicationError>> =
        taskResult {
            // Step 1: Lift applicative validation
            let! cmd: EvaluateSignalCmd = liftValidation(ACL.toEvaluateSignalCmd req)

            // Step 2: Ensure Item and Location exist
            do! requireEntityExists ports.ItemExists cmd.Item "Item" ItemId.value
            do! requireEntityExists ports.LocationExists cmd.Location "Location" LocationId.value

            // Step 3: Ensure aggregate baseline is established (FS-D-009 Step 1 & PO-D-031)
            let! existingOpt: DemandBehaviorAssessment option =
                repo.Get cmd.AssessmentId
                |> TaskResult.mapError Repository.mapRepositoryErrorToApplicationError

            do!
                match existingOpt with
                | Some _ -> TaskResult.return'()
                | None ->
                    taskResult {
                        let! baselineOpt = protect(ports.GetBaseline cmd.Item cmd.Location)

                        match baselineOpt with
                        | Some(mean, stdDev) ->
                            let initCmd: InitializeBaselineCmd =
                                { AssessmentId = cmd.AssessmentId
                                  Item = cmd.Item
                                  Location = cmd.Location
                                  BaselineMean = mean
                                  BaselineStdDev = stdDev }

                            let initPipeline =
                                CommandPipeline.create
                                    repo
                                    (fun (c: InitializeBaselineCmd) -> c.AssessmentId)
                                    Behaviors.initializeBaseline
                                    deps

                            let! _:DemandBehaviorAssessmentDto = runPipeline initPipeline publishKnowledge Projections.mapToDto initCmd
                            return ()
                        | None ->
                            return!
                                TaskResult.fail(
                                    ApplicationError.fromDomainError(
                                        DomainError.notFound(
                                            "Baseline",
                                            (DemandBehaviorAssessmentId.value cmd.AssessmentId)
                                        )
                                    )
                                )
                    }

            // Step 4: Enrich high-priority flag from port if needed
            let! isPriority = protect(ports.IsHighPriority cmd.Item)

            let enrichedCmd =
                { cmd with
                    IsHighPriority = cmd.IsHighPriority || isPriority }

            // Step 5: Execute evaluate pipeline
            let pipeline =
                CommandPipeline.create
                    repo
                    (fun (c: EvaluateSignalCmd) -> c.AssessmentId)
                    (Behaviors.evaluateSignal sensingPolicy)
                    deps

            return! runPipeline pipeline publishKnowledge id enrichedCmd
        }

    let evaluateForecastRefresh (req: EvaluateForecastRefreshReq) : Task<Result<ForecastRefreshDecisionDto, ApplicationError>> =
        taskResult {
            // Step 1: Lift applicative validation
            let! cmd: EvaluateForecastRefreshCmd = liftValidation(ACL.toEvaluateForecastRefreshCmd req)

            // Step 2: Ensure Item and Location exist
            do! requireEntityExists ports.ItemExists cmd.Item "Item" ItemId.value
            do! requireEntityExists ports.LocationExists cmd.Location "Location" LocationId.value

            // Step 3: Fetch existing aggregate
            let! existingOpt =
                repo.Get cmd.AssessmentId
                |> TaskResult.mapError Repository.mapRepositoryErrorToApplicationError

            match existingOpt with
            | None ->
                return!
                    TaskResult.fail(
                        ApplicationError.fromDomainError(
                            DomainError.notFound(
                                "DemandBehaviorAssessment",
                                (DemandBehaviorAssessmentId.value cmd.AssessmentId)
                            )
                        )
                    )
            | Some current ->
                let! (decision: Decision<DemandBehaviorAssessment, DemandBehaviorEvent>) =
                    Behaviors.evaluateForecastRefresh triggerPolicy cmd (Some current)
                    |> Result.mapError ApplicationError.fromDomainError
                    |> TaskResult.ofResult

                decision.Trace
                |> Option.iter(fun trace -> publishKnowledge(ArchitecturalKnowledge.ofDecisionTrace(trace.DecisionId, trace)))

                let selected =
                    decision.Trace
                    |> Option.map(fun t -> t.Outcome)
                    |> Option.defaultValue "DeferToNextScheduledCycle"

                let rationale =
                    decision.Trace
                    |> Option.map(fun t -> t.Rationale.Summary)
                    |> Option.defaultValue ""

                let traceId =
                    decision.Trace
                    |> Option.map(fun t -> t.DecisionId)
                    |> Option.defaultValue ""

                let dto: ForecastRefreshDecisionDto =
                    { SelectedAlternative = selected
                      Rationale = rationale
                      DecisionTraceId = traceId }

                return dto
        }

    { InitializeBaseline = initializeBaseline
      EvaluateSignal = evaluateSignal
      EvaluateForecastRefresh = evaluateForecastRefresh }

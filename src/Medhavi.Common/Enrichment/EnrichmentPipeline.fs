namespace Medhavi.Common.Enrichment

open System
open System.Threading.Tasks

// ==========================================
// ENRICHMENT PIPELINE - Advanced Orchestration
// ==========================================

/// Advanced pipeline orchestration with Category Theory
module EnrichmentPipeline =

    // Pipeline stage with metadata
    type PipelineStage<'input, 'output> =
        { Name: string
          Description: string
          Validate: 'input -> EnrichmentResult<'input>
          Enrich: EnrichmentContext -> 'input -> Async<Result<'output, string>>
          Metadata: Map<string, obj> }

    // Pipeline execution result
    type PipelineResult<'T> =
        { StageResults: (string * EnrichmentResult<'T>) list
          ExecutionTime: TimeSpan
          TotalStages: int
          SuccessfulStages: int
          FailedStages: int }

    // Create a pipeline stage
    let createStage name description validate enrich metadata =
        { Name = name
          Description = description
          Validate = validate
          Enrich = enrich
          Metadata = metadata }
    // Execute a single stage
    let executeStage (stage: PipelineStage<'input, 'output>) ctx input =
        async {
            let startTime = DateTimeOffset.UtcNow

            match stage.Validate input with
            | Success validInput ->
                let! result = stage.Enrich ctx validInput
                let executionTime = DateTimeOffset.UtcNow - startTime
                return Success result, executionTime
            | ValidationError err ->
                let executionTime = DateTimeOffset.UtcNow - startTime
                return ValidationError err, executionTime
            | EnrichmentError err ->
                let executionTime = DateTimeOffset.UtcNow - startTime
                return EnrichmentError err, executionTime
            | NotFound msg ->
                let executionTime = DateTimeOffset.UtcNow - startTime
                return NotFound msg, executionTime
        }

    // Sequential pipeline execution (Monad-based)
    let executeSequential (stages: PipelineStage<'a, 'b> list) ctx initialInput =
        async {
            let startTime = DateTimeOffset.UtcNow
            let mutable currentInput = initialInput
            let mutable results = []
            let mutable successfulStages = 0
            let mutable failedStages = 0

            for stage in stages do
                let! result, executionTime = executeStage stage ctx currentInput

                results <- (stage.Name, result) :: results

                match result with
                | Success output ->
                    currentInput <- output
                    successfulStages <- successfulStages + 1
                | _ ->
                    failedStages <- failedStages + 1
                    // Stop on first failure
                    ()

            let totalTime = DateTimeOffset.UtcNow - startTime

            return
                { StageResults = List.rev results
                  ExecutionTime = totalTime
                  TotalStages = stages.Length
                  SuccessfulStages = successfulStages
                  FailedStages = failedStages }
        }

    // Parallel pipeline execution (Applicative-based)
    let executeParallel (stages: PipelineStage<'input, 'output> list) ctx input =
        async {
            let startTime = DateTimeOffset.UtcNow

            // Execute all stages in parallel
            let stageTasks = stages |> List.map (fun stage -> executeStage stage ctx input)

            let! stageResults = Async.Parallel stageTasks

            let totalTime = DateTimeOffset.UtcNow - startTime

            let successfulStages =
                stageResults |> Array.filter (fst >> EnrichmentResult.isSuccess) |> Array.length

            let failedStages = stageResults.Length - successfulStages

            let stageResultsList =
                List.zip stages (Array.toList stageResults)
                |> List.map (fun (stage, (result, _)) -> stage.Name, result)

            return
                { StageResults = stageResultsList
                  ExecutionTime = totalTime
                  TotalStages = stages.Length
                  SuccessfulStages = successfulStages
                  FailedStages = failedStages }
        }

    // Conditional pipeline execution
    let executeConditional
        predicate
        (successPipeline: PipelineStage<'a, 'b> list)
        (failurePipeline: PipelineStage<'a, 'b> list)
        ctx
        input
        =
        async {
            if predicate input then
                executeSequential successPipeline ctx input
            else
                executeSequential failurePipeline ctx input
        }

    // Pipeline composition using Kleisli arrows
    let composeStagesKleisli (stage1: PipelineStage<'a, 'b>) (stage2: PipelineStage<'b, 'c>) : PipelineStage<'a, 'c> =
        { Name = $"{stage1.Name} >> {stage2.Name}"
          Description = $"Composed pipeline: {stage1.Description} then {stage2.Description}"
          Validate =
            fun input ->
                match stage1.Validate input with
                | Success valid1 -> stage2.Validate valid1
                | error -> error
          Enrich =
            fun ctx input ->
                async {
                    match stage1.Validate input with
                    | Success valid1 ->
                        let! result1 = stage1.Enrich ctx valid1

                        match result1 with
                        | Ok output1 ->
                            let! result2 = stage2.Enrich ctx output1
                            return result2
                        | Error e -> return Error e
                    | ValidationError err -> return Error err
                    | EnrichmentError err -> return Error err
                    | NotFound msg -> return Error msg
                }
          Metadata =
            Map.ofList
                [ "composed", true :> obj
                  "stage1", stage1.Name :> obj
                  "stage2", stage2.Name :> obj ] }

    // Pipeline builder pattern
    type PipelineBuilder<'input, 'output>() =
        let mutable stages: PipelineStage<'input, 'output> list = []

        member this.AddStage(stage: PipelineStage<'input, 'output>) =
            stages <- stages @ [ stage ]
            this

        member this.AddStage(name, description, validate, enrich, ?metadata) =
            let stage =
                createStage name description validate enrich (defaultArg metadata Map.empty)

            this.AddStage(stage)

        member this.BuildSequential() = stages
        member this.BuildParallel() = stages

    // Create pipeline builder
    let pipelineBuilder<'input, 'output> () = PipelineBuilder<'input, 'output>()

    // Pipeline monitoring and metrics
    module PipelineMetrics =

        type PipelineMetrics =
            { TotalExecutions: int64
              SuccessfulExecutions: int64
              FailedExecutions: int64
              AverageExecutionTime: TimeSpan
              StageMetrics: Map<string, StageMetrics> }

        and StageMetrics =
            { Executions: int64
              Successes: int64
              Failures: int64
              AverageTime: TimeSpan
              LastExecution: DateTimeOffset option }

        let createEmptyMetrics () =
            { TotalExecutions = 0L
              SuccessfulExecutions = 0L
              FailedExecutions = 0L
              AverageExecutionTime = TimeSpan.Zero
              StageMetrics = Map.empty }

        let updateMetrics (metrics: PipelineMetrics) (result: PipelineResult<'T>) =
            let newTotal = metrics.TotalExecutions + 1L

            let newSuccessful =
                if result.FailedStages = 0 then
                    metrics.SuccessfulExecutions + 1L
                else
                    metrics.SuccessfulExecutions

            let newFailed =
                if result.FailedStages > 0 then
                    metrics.FailedExecutions + 1L
                else
                    metrics.FailedExecutions

            let newAvgTime =
                let totalTime =
                    metrics.AverageExecutionTime.TotalMilliseconds * float metrics.TotalExecutions
                    + result.ExecutionTime.TotalMilliseconds

                TimeSpan.FromMilliseconds(totalTime / float newTotal)

            // Update stage metrics
            let updatedStageMetrics =
                result.StageResults
                |> List.fold
                    (fun acc (stageName, stageResult) ->
                        let currentMetrics =
                            Map.tryFind stageName acc
                            |> Option.defaultValue
                                { Executions = 0L
                                  Successes = 0L
                                  Failures = 0L
                                  AverageTime = TimeSpan.Zero
                                  LastExecution = None }

                        let updated =
                            { Executions = currentMetrics.Executions + 1L
                              Successes =
                                if EnrichmentResult.isSuccess stageResult then
                                    currentMetrics.Successes + 1L
                                else
                                    currentMetrics.Successes
                              Failures =
                                if not (EnrichmentResult.isSuccess stageResult) then
                                    currentMetrics.Failures + 1L
                                else
                                    currentMetrics.Failures
                              AverageTime = TimeSpan.Zero // Would need to track individual stage times
                              LastExecution = Some DateTimeOffset.UtcNow }

                        Map.add stageName updated acc)
                    metrics.StageMetrics

            { TotalExecutions = newTotal
              SuccessfulExecutions = newSuccessful
              FailedExecutions = newFailed
              AverageExecutionTime = newAvgTime
              StageMetrics = updatedStageMetrics }

namespace Medhavi.SharedKernel.Execution

open System.Threading.Tasks
open Medhavi.SharedKernel.Observation

type ExecutionPipeline<'Execution, 'Outcome> =
    private ExecutionPipeline of ExecutionStage<'Execution, 'Outcome> list

module ExecutionPipeline =

    let empty = ExecutionPipeline []

    let stage (executionStage : ExecutionStage<'Execution, 'Outcome>)
               (ExecutionPipeline stages) =
        ExecutionPipeline (stages @ [ executionStage ])

    /// Returns the underlying list of stages.
    let stages (ExecutionPipeline s) = s

    /// Creates a pipeline from a list of stages.
    let ofList (stageList: ExecutionStage<'Execution, 'Outcome> list) =
        ExecutionPipeline stageList

    let run
        (pipeline: ExecutionPipeline<'Exec, 'Outcome>)
        (initial: 'Exec)
        : Task<StageResult<'Exec, 'Outcome> * ArchitecturalKnowledge list> =

        let folder (currentTask: Task<StageResult<'Exec, 'Outcome> * ArchitecturalKnowledge list>)
                    (stage: ExecutionStage<'Exec, 'Outcome>) =
            task {
                let! currentResult, totalKnowledge = currentTask
                match currentResult with
                | Complete _ ->
                    return (currentResult, totalKnowledge)
                | Continue (exec, _) ->
                    let sw = System.Diagnostics.Stopwatch.StartNew()
                    let! stageResult = stage exec
                    sw.Stop()
                    let perfKnowledge = {
                        Name = "StageCompleted"
                        Timestamp = System.DateTimeOffset.UtcNow
                        Attributes = Map.ofList [
                            "Stage",      box (stage.GetType().Name)
                            "DurationMs", box sw.Elapsed.TotalMilliseconds
                        ]
                    }
                    let stageKnowledge =
                        match stageResult with
                        | Continue (_, k) -> k
                        | Complete (_, k) -> k
                    return (stageResult, totalKnowledge @ [perfKnowledge] @ stageKnowledge)
            }

        let initialTask = Task.FromResult (Continue (initial, []), [])
        List.fold folder initialTask (stages pipeline)

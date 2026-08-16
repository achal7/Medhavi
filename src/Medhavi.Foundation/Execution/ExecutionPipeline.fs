namespace Medhavi.Foundation.Execution

namespace Medhavi.Foundation.Execution

open System.Threading.Tasks
open Medhavi.Foundation.Observation

/// ============================================================================
/// EXECUTION PIPELINE (The Monoid of Stages)
/// ============================================================================
/// A pipeline is a monoid under stage concatenation.
/// - Identity element: `empty` (a pipeline with zero stages).
/// - Binary operation: `append` (combine two pipelines).
type ExecutionPipeline<'Execution, 'Outcome> = private ExecutionPipeline of ExecutionStage<'Execution, 'Outcome> list

module ExecutionPipeline =
    /// MONOIDAL IDENTITY
    let empty = ExecutionPipeline []

    /// MONOIDAL APPEND (Combine two pipelines)
    let append (ExecutionPipeline p1) (ExecutionPipeline p2) = ExecutionPipeline(p1 @ p2)

    /// Alias for append
    let combine = append

    let stage (executionStage: ExecutionStage<'Execution, 'Outcome>) (ExecutionPipeline stages) =
        ExecutionPipeline(stages @ [ executionStage ])

    let stages (ExecutionPipeline s) = s

    let ofList (stageList: ExecutionStage<'Execution, 'Outcome> list) = ExecutionPipeline stageList

    /// CATAMORPHISM (Fold)
    /// Executes the pipeline by folding over the Kleisli morphisms.
    /// Includes cross-cutting observability (Stopwatch) per CN-006 Traceability.
    let run
        (pipeline: ExecutionPipeline<'Exec, 'Outcome>)
        (initial: 'Exec)
        : Task<StageResult<'Exec, 'Outcome> * ArchitecturalKnowledge list> =

        let folder
            (currentTask: Task<StageResult<'Exec, 'Outcome> * ArchitecturalKnowledge list>)
            (stage: ExecutionStage<'Exec, 'Outcome>)
            =
            task {
                let! currentResult, totalKnowledge = currentTask

                match currentResult with
                | Complete _ -> return (currentResult, totalKnowledge) // Short-circuit
                | Continue(exec, _) ->
                    let sw = System.Diagnostics.Stopwatch.StartNew()
                    let! stageResult = stage exec
                    sw.Stop()

                    // Inject performance knowledge (Cross-cutting concern)
                    let perfKnowledge = ArchitecturalKnowledge.ofPerformance (stage.GetType().Name) sw.Elapsed true

                    let stageKnowledge =
                        match stageResult with
                        | Continue(_, k) -> k
                        | Complete(_, k) -> k

                    return (stageResult, totalKnowledge @ [ perfKnowledge ] @ stageKnowledge)
            }

        let initialTask = Task.FromResult(Continue(initial, []), [])
        List.fold folder initialTask (stages pipeline)

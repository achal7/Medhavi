namespace Medhavi.Foundation.Execution

open System.Threading.Tasks
open Medhavi.Common
open Medhavi.Foundation.Observation

/// Named execution stage preserving semantic morphism identity.
type NamedStage<'Execution, 'Outcome> =
    { Name: string
      Stage: ExecutionStage<'Execution, 'Outcome> }

/// ============================================================================
/// EXECUTION PIPELINE (The Monoid of Stages)
/// ============================================================================
/// A pipeline is a monoid under stage concatenation.
/// - Identity element: `empty` (a pipeline with zero stages).
/// - Binary operation: `append` (combine two pipelines).
type ExecutionPipeline<'Execution, 'Outcome> = private ExecutionPipeline of NamedStage<'Execution, 'Outcome> list

module ExecutionPipeline =
    /// MONOIDAL COMBINATORS
    let stageTimingMonoid: Monoid<(string * float) list> = Monoid.list
    let knowledgeListMonoid: Monoid<ArchitecturalKnowledge list> = Monoid.list

    /// MONOIDAL IDENTITY
    let empty = ExecutionPipeline []

    /// MONOIDAL APPEND (Combine two pipelines)
    let append (ExecutionPipeline p1) (ExecutionPipeline p2) = ExecutionPipeline(p1 @ p2)

    /// Alias for append
    let combine = append

    let pipelineMonoid<'Exec, 'Outcome> : Monoid<ExecutionPipeline<'Exec, 'Outcome>> =
        { Empty = empty; Combine = append }

    let namedStage (name: string) (executionStage: ExecutionStage<'Execution, 'Outcome>) : NamedStage<'Execution, 'Outcome> =
        { Name = name; Stage = executionStage }

    let anonymousStage (executionStage: ExecutionStage<'Execution, 'Outcome>) : NamedStage<'Execution, 'Outcome> =
        { Name = executionStage.GetType().Name; Stage = executionStage }

    let stage (executionStage: ExecutionStage<'Execution, 'Outcome>) (ExecutionPipeline stages) =
        ExecutionPipeline(stages @ [ anonymousStage executionStage ])

    let addNamed (name: string) (executionStage: ExecutionStage<'Execution, 'Outcome>) (ExecutionPipeline stages) =
        ExecutionPipeline(stages @ [ namedStage name executionStage ])

    let stages (ExecutionPipeline s) = s

    let ofNamedList (stageList: NamedStage<'Execution, 'Outcome> list) = ExecutionPipeline stageList

    let ofList (stageList: ExecutionStage<'Execution, 'Outcome> list) =
        stageList |> List.map anonymousStage |> ExecutionPipeline

    /// CATAMORPHISM (Fold)
    /// Executes the pipeline by folding over the Kleisli morphisms using Monoids for timings and knowledge.
    /// Preserves cross-cutting performance observability consolidated into CommandCompleted.
    let run
        (pipeline: ExecutionPipeline<'Exec, 'Outcome>)
        (initial: 'Exec)
        : Task<StageResult<'Exec, 'Outcome> * ArchitecturalKnowledge list> =

        let overallSw = System.Diagnostics.Stopwatch.StartNew()

        let stageExecutionCatamorphism
            (currentTask: Task<StageResult<'Exec, 'Outcome> * ArchitecturalKnowledge list * (string * float) list>)
            (namedStage: NamedStage<'Exec, 'Outcome>)
            =
            task {
                let! currentResult, totalKnowledge, timings = currentTask

                match currentResult with
                | Complete _ -> return (currentResult, totalKnowledge, timings) // Short-circuit
                | Continue(exec, _) ->
                    let sw = System.Diagnostics.Stopwatch.StartNew()
                    let! stageResult = namedStage.Stage exec
                    sw.Stop()

                    let stageMs = sw.Elapsed.TotalMilliseconds
                    let newTimings = Monoid.append stageTimingMonoid timings [ (namedStage.Name, stageMs) ]

                    let stageKnowledge =
                        match stageResult with
                        | Continue(_, k) -> k
                        | Complete(_, k) -> k

                    let newKnowledge = Monoid.append knowledgeListMonoid totalKnowledge stageKnowledge
                    return (stageResult, newKnowledge, newTimings)
            }

        let initialTask = Task.FromResult((Continue(initial, []), Monoid.empty knowledgeListMonoid, Monoid.empty stageTimingMonoid))

        task {
            let! finalResult, accumulatedKnowledge, timings =
                List.fold stageExecutionCatamorphism initialTask (stages pipeline)
            overallSw.Stop()

            let totalMs = overallSw.Elapsed.TotalMilliseconds
            let timingSummary =
                timings
                |> List.map (fun (name, ms) -> $"{name}={ms:F2}ms")
                |> String.concat ", "

            let enrichedKnowledge =
                accumulatedKnowledge
                |> List.map (fun k ->
                    if k.Name = "CommandCompleted" then
                        let updatedAttrs =
                            k.Attributes
                            |> Map.add "DurationTotalMs" (box totalMs)
                            |> Map.add "StageTimings" (box timingSummary)
                        { k with Attributes = updatedAttrs }
                    else
                        k)

            return (finalResult, enrichedKnowledge)
        }

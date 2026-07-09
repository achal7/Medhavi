namespace Medhavi.SharedKernel.Execution

open System.Threading.Tasks
open Medhavi.SharedKernel.Observation

type StageResult<'Execution, 'Outcome> =
    | Continue of 'Execution * ArchitecturalKnowledge list
    | Complete of 'Outcome * ArchitecturalKnowledge list

type ExecutionStage<'Execution, 'Outcome> = 'Execution -> Task<StageResult<'Execution, 'Outcome>>

module StageResult =
    let continueWith
        (execution: 'Execution)
        (knowledge: ArchitecturalKnowledge list)
        : StageResult<'Execution, 'Outcome> =
        Continue(execution, knowledge)

    let complete (outcome: 'Outcome) (knowledge: ArchitecturalKnowledge list) : StageResult<'Execution, 'Outcome> =
        Complete(outcome, knowledge)

// No map function. The execution type appears in both input and Continue output,
// making a simple map impossible. When needed, we will introduce dimap or invmap.
// For MVP, stages are written directly for their specific execution type.

(*

type StageDisposition =
    | Continue
    | Complete
    | Suspend
    | Reject
    | Cancel
    
type StageResult<'Subject> =
    {
        Execution : Execution<'Subject>

        Disposition : StageDisposition

        Knowledge : ArchitecturalKnowledge list
    }

module StageResult =

    let continueWith execution knowledge =
        {
            Execution = execution
            Disposition = Continue
            Knowledge = knowledge
        }

    let complete execution knowledge =
        {
            Execution =
                execution
                |> Execution.complete

            Disposition = Complete

            Knowledge = knowledge
        }

    let suspend execution knowledge =
        {
            Execution =
                execution
                |> Execution.suspend

            Disposition = Suspend

            Knowledge = knowledge
        }

    let reject execution knowledge =
        {
            Execution =
                execution
                |> Execution.reject

            Disposition = Reject

            Knowledge = knowledge
        }

    let cancel execution knowledge =
        {
            Execution =
                execution
                |> Execution.cancel

            Disposition = Cancel

            Knowledge = knowledge
        }

// ============================================================================
// Execution Stage
// ============================================================================

type ExecutionStage<'Subject> = Execution<'Subject> -> StageResult<'Subject>

module ExecutionStage =

    let map mapper stage =

        fun execution ->

            let result =
                stage execution

            {
                result with
                    Execution =
                        Execution.map
                            mapper
                            result.Execution
            }
*)

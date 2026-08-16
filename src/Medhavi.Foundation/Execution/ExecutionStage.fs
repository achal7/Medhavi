namespace Medhavi.Foundation.Execution

namespace Medhavi.Foundation.Execution

open System.Threading.Tasks
open Medhavi.Foundation.Observation

/// ============================================================================
/// STAGE RESULT (The Object in our Pipeline Category)
/// ============================================================================
/// Mathematically: StageResult ≅ Writer<Knowledge list> (Either<Outcome, Execution>)
/// - Continue = Right (success, keep going, accumulate knowledge)
/// - Complete = Left (terminal outcome, short-circuit, accumulate knowledge)
type StageResult<'Execution, 'Outcome> =
    | Continue of 'Execution * ArchitecturalKnowledge list
    | Complete of 'Outcome * ArchitecturalKnowledge list

/// ============================================================================
/// EXECUTION STAGE (The Morphism in our Pipeline Category)
/// ============================================================================
/// A stage is a Kleisli morphism: Execution -> Task<StageResult>
type ExecutionStage<'Execution, 'Outcome> = 'Execution -> Task<StageResult<'Execution, 'Outcome>>

module StageResult =
    let continueWith
        (execution: 'Execution)
        (knowledge: ArchitecturalKnowledge list)
        : StageResult<'Execution, 'Outcome> =
        Continue(execution, knowledge)

    let complete (outcome: 'Outcome) (knowledge: ArchitecturalKnowledge list) : StageResult<'Execution, 'Outcome> =
        Complete(outcome, knowledge)

module ExecutionStage =
    /// KLEISLI COMPOSITION (>=>)
    /// Composes two morphisms f and g into a single morphism.
    /// Mathematically: (f >=> g)(x) = f(x) >>= g
    /// Knowledge lists are combined using the List Monoid (append).
    let compose (f: ExecutionStage<'E, 'O>) (g: ExecutionStage<'E, 'O>) : ExecutionStage<'E, 'O> =
        fun exec ->
            task {
                let! result = f exec

                match result with
                | Complete(outcome, k1) -> return Complete(outcome, k1) // Short-circuit
                | Continue(exec', k1) ->
                    let! result' = g exec'

                    match result' with
                    | Complete(outcome, k2) -> return Complete(outcome, k1 @ k2)
                    | Continue(exec'', k2) -> return Continue(exec'', k1 @ k2)
            }

    /// Fish operator for Kleisli composition
    let (>=>) = compose

    /// KLEISLI IDENTITY MORPHISM
    /// A stage that does nothing and passes the execution state through unchanged.
    let identity<'E, 'O> : ExecutionStage<'E, 'O> = fun exec -> Task.FromResult(Continue(exec, []))

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
*)

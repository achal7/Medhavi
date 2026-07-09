namespace Medhavi.SharedKernel.Execution

open System
open System.Threading
open System.Threading.Tasks
open Medhavi.SharedKernel.Observation

module ExecutionRunner =

    let execute
        (strategy: ExecutionStrategy<'TOk, 'TError>)
        (pipeline: ExecutionPipeline<Execution<'State, 'Context>, ExecutionOutcome<'TOk, 'TError>>)
        (initialModel: Execution<'State, 'Context>)
        (publishKnowledge: ArchitecturalKnowledge -> unit)
        (maxRetries: int)
        (retryDelay: TimeSpan)
        (ct: CancellationToken)
        : Task<ExecutionOutcome<'TOk, 'TError>> =

        let rec loop attempt =
            task {
                ct.ThrowIfCancellationRequested()

                let! stageResult, knowledge = ExecutionPipeline.run pipeline initialModel
                knowledge |> List.iter publishKnowledge

                match stageResult with
                | Continue _ ->
                    return failwith "Pipeline returned Continue; expected Complete."

                | Complete (executionOutcome, _) ->
                    if attempt < maxRetries then
                        let action = strategy executionOutcome
                        match action with
                        | Retry ->
                            do! Task.Delay(retryDelay, ct)
                            return! loop (attempt + 1)
                        | _ ->
                            return executionOutcome
                    else
                        return executionOutcome
            }

        loop 0

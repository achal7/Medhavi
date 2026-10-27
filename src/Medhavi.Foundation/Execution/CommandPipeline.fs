module Medhavi.Foundation.Execution.CommandPipeline

open System
open Medhavi.Foundation.Contracts
open Medhavi.Foundation.Failure
open AggregateStages

let private resolveBoundedContext (t: Type) : string =
    let fullName = if isNull t.FullName then t.Name else t.FullName
    if fullName.Contains(".Demand") || fullName.Contains("Demand") then "Demand"
    elif fullName.Contains(".Supply") || fullName.Contains("Supply") then "Supply"
    elif fullName.Contains(".Core") || fullName.Contains("Core") || fullName.Contains("Exception") || fullName.Contains("Picture") then "Core"
    else "Core"

let create
    (repo: Repository<'Agg, 'Id, 'Event>)
    (getId: 'Cmd -> 'Id)
    (decideFn: Decide<'Agg, 'Cmd, 'Event>)
    (deps: EnvelopeStoreDependencies<'Event>)
    : ExecutionPipeline<
          Execution<'Agg option, CommandContext<'Cmd, 'Agg, 'Event>>,
          ExecutionOutcome<'Agg, ApplicationError>
       >
    =

    let boundedContext = resolveBoundedContext typeof<'Agg>

    let enricher: AggregateStages.KnowledgeEnricher<'Cmd> =
        fun cmd knowledge ->
            { knowledge with
                Attributes =
                    knowledge.Attributes
                    |> Map.add "BoundedContext" (box boundedContext)
                    |> Map.add "AggregateType" (box typeof<'Agg>.Name)
                    |> Map.add "AggregateId" (box (getId cmd))
                    |> Map.add "CommandType" (box(cmd.GetType().Name)) }

    ExecutionPipeline.ofNamedList
        [ ExecutionPipeline.namedStage "Load" (AggregateStages.load repo getId enricher)
          ExecutionPipeline.namedStage "Decide" (AggregateStages.decide decideFn enricher)
          ExecutionPipeline.namedStage "Persist" (AggregateStages.persist repo getId deps enricher)
          ExecutionPipeline.namedStage "PublishKnowledge" (AggregateStages.publishKnowledge enricher) ]

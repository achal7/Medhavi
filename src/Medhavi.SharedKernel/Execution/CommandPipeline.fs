module Medhavi.SharedKernel.Execution.CommandPipeline

open Medhavi.SharedKernel.Contracts.Aggregate
open Medhavi.SharedKernel.Failure

let create
    (repo: Repository<'Agg, 'Id, 'Event>)
    (getId: 'Cmd -> 'Id)
    (decideFn: Decide<'Agg, 'Cmd, 'Event>)
    : ExecutionPipeline<
          Execution<'Agg option, CommandContext<'Cmd, 'Agg, 'Event>>,
          ExecutionOutcome<'Agg, ApplicationError>
       >
    =

    let enricher: AggregateStages.KnowledgeEnricher<'Cmd> =
        fun cmd knowledge ->
            { knowledge with
                Attributes =
                    knowledge.Attributes
                    |> Map.add "AggregateType" (box typeof<'Agg>.Name)
                    |> Map.add "CommandType" (box(cmd.GetType().Name)) }

    ExecutionPipeline.ofList
        [ AggregateStages.load repo getId enricher
          AggregateStages.decide decideFn enricher
          AggregateStages.persist repo getId enricher
          AggregateStages.publishKnowledge enricher ]

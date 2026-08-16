module Medhavi.Foundation.Execution.CommandPipeline

open Medhavi.Foundation.Contracts
open Medhavi.Foundation.Failure
open AggregateStages

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
          AggregateStages.persist repo getId deps enricher
          AggregateStages.publishKnowledge enricher ]

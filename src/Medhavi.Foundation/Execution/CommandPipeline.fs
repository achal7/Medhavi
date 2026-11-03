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
                    |> Map.add "AggregateId" (box (getId cmd))
                    |> Map.add "CommandType" (box(cmd.GetType().Name)) }

    ExecutionPipeline.ofNamedList
        [ ExecutionPipeline.namedStage "Load" (AggregateStages.load repo getId enricher)
          ExecutionPipeline.namedStage "Decide" (AggregateStages.decide decideFn enricher)
          ExecutionPipeline.namedStage "Persist" (AggregateStages.persist repo getId deps enricher)
          ExecutionPipeline.namedStage "PublishKnowledge" (AggregateStages.publishKnowledge enricher) ]

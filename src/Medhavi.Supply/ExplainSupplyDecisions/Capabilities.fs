/// CA-S-012 Explain Supply Decisions Public Capability API
module Medhavi.Supply.ExplainSupplyDecisions.Capabilities

open System.Threading.Tasks
open Medhavi.Contracts
open Medhavi.Contracts.Supply

type ExplainSupplyDecisionsApis = { Explanation: SupplyExplanationApi }

let create
    (aggregateApi: SupplyExplanation.Capabilities.AggregateApi)
    (ports: ExplainSupplyPorts)
    : ExplainSupplyDecisionsApis =

    let recordExplanation (req: RecordSupplyExplanationReq) : Task<Result<SupplyExplanationDto, ApiError>> =
        task {
            // Retrieve historical decision traces via decoupled port
            let! traces = ports.GetDecisionTraces req.ExplainedArtifactType req.ExplainedArtifactId

            let! outcome = aggregateApi.Record traces req

            match outcome with
            | Ok exp ->
                let dto = SupplyExplanation.Projections.toDto exp
                return Ok dto
            | Error appErr -> return Error(ApiError.validation $"{appErr}")
        }

    let explanationApi: SupplyExplanationApi =
        {
            RecordExplanation = recordExplanation
        }

    { Explanation = explanationApi }

module Medhavi.Client.Supply.ChangeSensing.SupplyChangeSensingActions

open System
open System.Threading.Tasks
open Medhavi.Contracts.Supply
open Medhavi.Client.ViewComposition.Identity
open Medhavi.Client.ActionExecution
open Medhavi.Client

let corroborate: ActionHandler =
    fun ctx ->
        task {
            match ctx.TargetKey with
            | Some assessmentId ->
                let api = ctx.Domain.Supply.SenseSupplyChanges.SupplyChangeApi
                let queries = ctx.Domain.Supply.SenseSupplyChanges.SupplyChangeQueries
                let! dtoOpt = queries.GetById assessmentId
                match dtoOpt with
                | Some dto ->
                    let req: AssessSupplyChangeReq =
                        { AssessmentId = dto.AssessmentId
                          MonitoredEntityType = dto.MonitoredEntityType
                          MonitoredEntityId = dto.MonitoredEntityId
                          ChangeType = dto.ChangeType
                          SignalId = dto.SignalSummary.SignalId
                          SourceId = dto.SignalSummary.SourceId
                          SourceType = dto.SignalSummary.SourceType
                          SourceReliabilityPrior = dto.SignalSummary.SourceReliabilityPrior
                          ObservedQuantity = dto.SignalSummary.ObservedQuantity
                          ObservedDate = dto.SignalSummary.ObservedDate
                          SignalGenerationTime = dto.SignalSummary.SignalGenerationTime
                          Classification = dto.ChangeClassification
                          ExpectedDuration = dto.ExpectedDuration
                          CorrelatedChangeIds = dto.CorrelatedChangeIds
                          Timestamp = DateTimeOffset.UtcNow }

                    match! api.Assess req with
                    | Ok (assessedDto: SupplyChangeAssessmentDto) ->
                        return
                            Ok(
                                ActionExecutionOutcome.Completed(
                                    $"Change signal '{assessedDto.AssessmentId}' corroborated with secondary telemetry. Epistemic state: {assessedDto.CurrentEpistemicState}.",
                                    [ SupplyChangeSensingGrid.storeId
                                      DataStoreId.literal "supply-exception-store" ]
                                )
                            )
                    | Error err -> return Error err.Message
                | None -> return Error $"Change signal '{assessmentId}' not found in query projection."
            | None -> return Error "No change signal selected to corroborate."
        }

let confirm: ActionHandler =
    fun ctx ->
        task {
            match ctx.TargetKey with
            | Some assessmentId ->
                let api = ctx.Domain.Supply.SenseSupplyChanges.SupplyChangeApi
                let queries = ctx.Domain.Supply.SenseSupplyChanges.SupplyChangeQueries
                let! dtoOpt = queries.GetById assessmentId
                match dtoOpt with
                | Some dto ->
                    let req: ArchiveSupplyChangeReq =
                        { AssessmentId = dto.AssessmentId
                          Reason = "Confirmed as operational reality by planner."
                          Timestamp = DateTimeOffset.UtcNow }

                    match! api.Archive req with
                    | Ok (assessedDto: SupplyChangeAssessmentDto) ->
                        return
                            Ok(
                                ActionExecutionOutcome.Completed(
                                    $"Change signal '{assessedDto.AssessmentId}' confirmed as operational reality. Epistemic state: {assessedDto.CurrentEpistemicState}.",
                                    [ SupplyChangeSensingGrid.storeId
                                      DataStoreId.literal "supply-store"
                                      DataStoreId.literal "supply-exception-store" ]
                                )
                            )
                    | Error err -> return Error err.Message
                | None -> return Error $"Change signal '{assessmentId}' not found in query projection."
            | None -> return Error "No change signal selected to confirm."
        }

let dismiss: ActionHandler =
    fun ctx ->
        task {
            match ctx.TargetKey with
            | Some assessmentId ->
                let api = ctx.Domain.Supply.SenseSupplyChanges.SupplyChangeApi
                let queries = ctx.Domain.Supply.SenseSupplyChanges.SupplyChangeQueries
                let! dtoOpt = queries.GetById assessmentId
                match dtoOpt with
                | Some dto ->
                    let req: ArchiveSupplyChangeReq =
                        { AssessmentId = dto.AssessmentId
                          Reason = "Dismissed as unverified noise by planner."
                          Timestamp = DateTimeOffset.UtcNow }

                    match! api.Archive req with
                    | Ok (assessedDto: SupplyChangeAssessmentDto) ->
                        return
                            Ok(
                                ActionExecutionOutcome.Completed(
                                    $"Change signal '{assessedDto.AssessmentId}' dismissed. Epistemic state: {assessedDto.CurrentEpistemicState}.",
                                    [ SupplyChangeSensingGrid.storeId ]
                                )
                            )
                    | Error err -> return Error err.Message
                | None -> return Error $"Change signal '{assessmentId}' not found in query projection."
            | None -> return Error "No change signal selected to dismiss."
        }

let all: (ActionDefId * ActionHandler) list =
    [ SupplyChangeSensingGrid.SupplyChangeSensingActions.corroborate.Id, corroborate
      SupplyChangeSensingGrid.SupplyChangeSensingActions.confirm.Id, confirm
      SupplyChangeSensingGrid.SupplyChangeSensingActions.dismiss.Id, dismiss ]


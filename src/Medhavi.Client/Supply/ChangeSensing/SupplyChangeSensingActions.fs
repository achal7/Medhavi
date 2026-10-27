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
                let apiOpt =
                    match ctx.Resolve<SupplyChangeApi>() with
                    | Some api -> Some api
                    | None ->
                        ctx.Resolve<DomainServices>()
                        |> Option.map (fun ds -> ds.Supply.SenseSupplyChanges.SupplyChangeApi)

                match apiOpt with
                | Some api ->
                    let queriesOpt =
                        match ctx.Resolve<SupplyChangeQueries>() with
                        | Some q -> Some q
                        | None ->
                            ctx.Resolve<DomainServices>()
                            |> Option.map (fun ds -> ds.Supply.SenseSupplyChanges.SupplyChangeQueries)

                    match queriesOpt with
                    | Some queries ->
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
                            | Ok assessedDto ->
                                return
                                    Ok(
                                        ActionExecutionOutcome.Completed(
                                            $"Change signal '{assessedDto.AssessmentId}' corroborated with secondary telemetry. Epistemic state: {assessedDto.CurrentEpistemicState}.",
                                            [ SupplyChangeSensingStore.storeId
                                              DataStoreId.literal "supply-exception-store" ]
                                        )
                                    )
                            | Error err -> return Error err.Message
                        | None -> return Error $"Change signal '{assessmentId}' not found in query projection."
                    | None -> return Error "SupplyChangeQueries is not connected."
                | None -> return Error "SupplyChangeApi capability is not connected."
            | None -> return Error "No change signal selected to corroborate."
        }

let confirm: ActionHandler =
    fun ctx ->
        task {
            match ctx.TargetKey with
            | Some assessmentId ->
                let apiOpt =
                    match ctx.Resolve<SupplyChangeApi>() with
                    | Some api -> Some api
                    | None ->
                        ctx.Resolve<DomainServices>()
                        |> Option.map (fun ds -> ds.Supply.SenseSupplyChanges.SupplyChangeApi)

                match apiOpt with
                | Some api ->
                    let queriesOpt =
                        match ctx.Resolve<SupplyChangeQueries>() with
                        | Some q -> Some q
                        | None ->
                            ctx.Resolve<DomainServices>()
                            |> Option.map (fun ds -> ds.Supply.SenseSupplyChanges.SupplyChangeQueries)

                    match queriesOpt with
                    | Some queries ->
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
                            | Ok assessedDto ->
                                return
                                    Ok(
                                        ActionExecutionOutcome.Completed(
                                            $"Change signal '{assessedDto.AssessmentId}' confirmed as verified reality. Propagated to planning model.",
                                            [ SupplyChangeSensingStore.storeId
                                              DataStoreId.literal "supply-store"
                                              DataStoreId.literal "supply-exception-store" ]
                                        )
                                    )
                            | Error err -> return Error err.Message
                        | None -> return Error $"Change signal '{assessmentId}' not found in query projection."
                    | None -> return Error "SupplyChangeQueries is not connected."
                | None -> return Error "SupplyChangeApi capability is not connected."
            | None -> return Error "No change signal selected to confirm."
        }

let dismiss: ActionHandler =
    fun ctx ->
        task {
            match ctx.TargetKey with
            | Some assessmentId ->
                let apiOpt =
                    match ctx.Resolve<SupplyChangeApi>() with
                    | Some api -> Some api
                    | None ->
                        ctx.Resolve<DomainServices>()
                        |> Option.map (fun ds -> ds.Supply.SenseSupplyChanges.SupplyChangeApi)

                match apiOpt with
                | Some api ->
                    let req: ArchiveSupplyChangeReq =
                        { AssessmentId = assessmentId
                          Reason = "Dismissed by planner"
                          Timestamp = DateTimeOffset.UtcNow }

                    match! api.Archive req with
                    | Ok _ ->
                        return
                            Ok(
                                ActionExecutionOutcome.Completed(
                                    $"Change signal '{assessmentId}' dismissed as unverified telemetry noise.",
                                    [ SupplyChangeSensingStore.storeId ]
                                )
                            )
                    | Error err -> return Error err.Message
                | None -> return Error "SupplyChangeApi capability is not connected."
            | None -> return Error "No change signal selected to dismiss."
        }

let all: (ActionDefId * ActionHandler) list =
    [ SupplyChangeSensingGridSchema.corroborate.Id, corroborate
      SupplyChangeSensingGridSchema.confirm.Id, confirm
      SupplyChangeSensingGridSchema.dismiss.Id, dismiss ]

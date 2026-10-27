module Medhavi.Client.Supply.Exceptions.SupplyExceptionActions

open System
open System.Threading.Tasks
open Medhavi.Contracts.Core.Exception
open Medhavi.Contracts.Supply
open Medhavi.Client.ViewComposition.Identity
open Medhavi.Client.ActionExecution
open Medhavi.Client

let explainRootCause: ActionHandler =
    fun ctx ->
        task {
            match ctx.TargetKey with
            | Some exceptionId ->
                return Ok(ActionExecutionOutcome.ExplainRequested(exceptionId, "SupplyException"))
            | None -> return Error "No supply exception selected to explain."
        }

let confirm: ActionHandler =
    fun ctx ->
        task {
            match ctx.TargetKey with
            | Some exceptionId ->
                let apiOpt =
                    match ctx.Resolve<SupplyExceptionApi>() with
                    | Some api -> Some api
                    | None ->
                        ctx.Resolve<DomainServices>()
                        |> Option.map (fun ds -> ds.Supply.DetectSupplyExceptions.SupplyExceptionApi)

                match apiOpt with
                | Some api ->
                    let queriesOpt =
                        match ctx.Resolve<SupplyExceptionQueries>() with
                        | Some q -> Some q
                        | None ->
                            ctx.Resolve<DomainServices>()
                            |> Option.map (fun ds -> ds.Supply.DetectSupplyExceptions.SupplyExceptionQueries)

                    match queriesOpt with
                    | Some queries ->
                        let! dtoOpt = queries.GetById exceptionId
                        match dtoOpt with
                        | Some dto ->
                            let req: EvaluateSupplyExceptionReq =
                                { GovernedPolicyReference = dto.GovernedPolicyReference
                                  ScopeEntityType = dto.ScopeEntityType
                                  ScopeEntityId = dto.ScopeEntityId
                                  PlanningScopeId = dto.PlanningScopeId
                                  ExceptionType = dto.ExceptionType
                                  TriggeringEvidence = dto.EvidenceReferences
                                  EvaluationTimestamp = DateTimeOffset.UtcNow }

                            match! api.EvaluateException req with
                            | Ok assessedDto ->
                                return
                                    Ok(
                                        ActionExecutionOutcome.Completed(
                                            $"Supply exception '{assessedDto.AssessmentId}' confirmed for active mitigation.",
                                            [ SupplyExceptionStore.storeId
                                              DataStoreId.literal "core-exception-store" ]
                                        )
                                    )
                            | Error err -> return Error err.Message
                        | None -> return Error $"Supply exception '{exceptionId}' not found in query projection."
                    | None -> return Error "SupplyExceptionQueries is not connected."
                | None -> return Error "SupplyExceptionApi capability is not connected."
            | None -> return Error "No supply exception selected to confirm."
        }

let resolve: ActionHandler =
    fun ctx ->
        task {
            match ctx.TargetKey with
            | Some exceptionId ->
                let apiOpt =
                    match ctx.Resolve<ExceptionApi>() with
                    | Some api -> Some api
                    | None ->
                        ctx.Resolve<DomainServices>()
                        |> Option.map (fun ds -> ds.Core.ExceptionManagement.Commands)

                match apiOpt with
                | Some api ->
                    let queriesOpt =
                        match ctx.Resolve<SupplyExceptionQueries>() with
                        | Some q -> Some q
                        | None ->
                            ctx.Resolve<DomainServices>()
                            |> Option.map (fun ds -> ds.Supply.DetectSupplyExceptions.SupplyExceptionQueries)

                    match queriesOpt with
                    | Some queries ->
                        let! dtoOpt = queries.GetById exceptionId
                        match dtoOpt with
                        | Some dto ->
                            let req: ResolveExceptionReq =
                                { ConstraintReference = dto.GovernedPolicyReference
                                  AffectedScopeType = dto.ScopeEntityType
                                  AffectedScopeIdentifier = dto.ScopeEntityId
                                  ResolutionTime = DateTimeOffset.UtcNow
                                  ResolutionEvidence =
                                    $"Resolved by planner via Supply Exceptions Workbench for assessment {exceptionId}." }

                            match! api.Resolve req with
                            | Ok resDto ->
                                return
                                    Ok(
                                        ActionExecutionOutcome.Completed(
                                            $"Supply exception '{exceptionId}' (Core Exception '{resDto.ExceptionId}') marked as resolved. Buffers and operational plan rebalanced.",
                                            [ SupplyExceptionStore.storeId
                                              DataStoreId.literal "core-exception-store"
                                              DataStoreId.literal "supply-store" ]
                                        )
                                    )
                            | Error err -> return Error err.Message
                        | None -> return Error $"Supply exception '{exceptionId}' not found in query projection."
                    | None -> return Error "SupplyExceptionQueries is not connected."
                | None -> return Error "Core ExceptionApi capability is not connected."
            | None -> return Error "No supply exception selected to resolve."
        }

let escalate: ActionHandler =
    fun ctx ->
        task {
            match ctx.TargetKey with
            | Some exceptionId ->
                let apiOpt =
                    match ctx.Resolve<ExceptionApi>() with
                    | Some api -> Some api
                    | None ->
                        ctx.Resolve<DomainServices>()
                        |> Option.map (fun ds -> ds.Core.ExceptionManagement.Commands)

                match apiOpt with
                | Some api ->
                    let queriesOpt =
                        match ctx.Resolve<SupplyExceptionQueries>() with
                        | Some q -> Some q
                        | None ->
                            ctx.Resolve<DomainServices>()
                            |> Option.map (fun ds -> ds.Supply.DetectSupplyExceptions.SupplyExceptionQueries)

                    match queriesOpt with
                    | Some queries ->
                        let! dtoOpt = queries.GetById exceptionId
                        match dtoOpt with
                        | Some dto ->
                            let req: ExceptionEvidenceReq =
                                { ConstraintReference = dto.GovernedPolicyReference
                                  Classification = dto.ExceptionType
                                  AffectedScopeType = dto.ScopeEntityType
                                  AffectedScopeIdentifier = dto.ScopeEntityId
                                  EvidenceReference =
                                    dto.EvidenceReferences
                                    |> List.tryHead
                                    |> Option.defaultValue exceptionId
                                    |> Some
                                  Severity = Some dto.BusinessImpact
                                  EvidenceTime = DateTimeOffset.UtcNow }

                            match! api.ProcessEvidence req with
                            | Ok coreDto ->
                                return
                                    Ok(
                                        ActionExecutionOutcome.Completed(
                                            $"Supply exception '{exceptionId}' escalated to Core central exception management (CA-C-020, Core Exception '{coreDto.ExceptionId}').",
                                            [ SupplyExceptionStore.storeId
                                              DataStoreId.literal "core-exception-store" ]
                                        )
                                    )
                            | Error err -> return Error err.Message
                        | None -> return Error $"Supply exception '{exceptionId}' not found in query projection."
                    | None -> return Error "SupplyExceptionQueries is not connected."
                | None -> return Error "Core ExceptionApi capability is not connected."
            | None -> return Error "No supply exception selected to escalate."
        }

let all: (ActionDefId * ActionHandler) list =
    [ SupplyExceptionGridSchema.explainRootCause.Id, explainRootCause
      SupplyExceptionGridSchema.confirm.Id, confirm
      SupplyExceptionGridSchema.resolve.Id, resolve
      SupplyExceptionGridSchema.escalate.Id, escalate ]

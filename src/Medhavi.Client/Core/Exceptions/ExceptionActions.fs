module Medhavi.Client.Core.Exceptions.ExceptionActions

open System
open System.Threading.Tasks
open Medhavi.Contracts.Core.Exception
open Medhavi.Client.ViewComposition.Identity
open Medhavi.Client.ActionExecution
open Medhavi.Client
open Medhavi.Client.Core.Exceptions

let private getApi (ctx: ActionExecutionContext) : ExceptionApi option =
    match ctx.Resolve<ExceptionApi>() with
    | Some api -> Some api
    | None ->
        ctx.Resolve<DomainServices>()
        |> Option.map (fun ds -> ds.Core.ExceptionManagement.Commands)

let private getQueries (ctx: ActionExecutionContext) : ExceptionQueries option =
    match ctx.Resolve<ExceptionQueries>() with
    | Some q -> Some q
    | None ->
        ctx.Resolve<DomainServices>()
        |> Option.map (fun ds -> ds.Core.ExceptionManagement.Queries)

let private getExceptionDto (ctx: ActionExecutionContext) (exceptionId: string) : Task<ExceptionDto option> =
    task {
        match getQueries ctx with
        | Some q -> return! q.GetById exceptionId
        | None -> return None
    }

/// Processes evidence and acknowledges an active exception.
let acknowledgeException: ActionHandler =
    fun ctx ->
        task {
            match getApi ctx with
            | Some api ->
                match ctx.TargetKey with
                | Some exceptionId ->
                    let! dtoOpt = getExceptionDto ctx exceptionId
                    match dtoOpt with
                    | Some dto ->
                        let req: ExceptionEvidenceReq =
                            { ConstraintReference = dto.ConstraintReference
                              Classification = dto.Classification
                              AffectedScopeType = dto.AffectedScopeType
                              AffectedScopeIdentifier = dto.AffectedScopeIdentifier
                              EvidenceReference = Some (sprintf "Acknowledged by planner at %s UTC" (DateTimeOffset.Now.ToString("yyyy-MM-dd HH:mm:ss")))
                              Severity = dto.Severity
                              EvidenceTime = DateTimeOffset.Now }

                        match! api.ProcessEvidence req with
                        | Ok resDto ->
                            return
                                Ok(
                                    ActionExecutionOutcome.Completed(
                                        $"Exception '{resDto.ExceptionId}' acknowledged in enterprise ledger.",
                                        [ ExceptionStore.storeId ]
                                    )
                                )
                        | Error err -> return Error err.Message
                    | None -> return Error $"Exception '{exceptionId}' not found in query projection."
                | None -> return Error "No exception selected to acknowledge."
            | None -> return Error "ExceptionApi capability is not connected."
        }

/// Resolves an existing exception with governed resolution evidence.
let resolveException: ActionHandler =
    fun ctx ->
        task {
            match getApi ctx with
            | Some api ->
                match ctx.TargetKey with
                | Some exceptionId ->
                    let! dtoOpt = getExceptionDto ctx exceptionId
                    match dtoOpt with
                    | Some dto ->
                        let req: ResolveExceptionReq =
                            { ConstraintReference = dto.ConstraintReference
                              AffectedScopeType = dto.AffectedScopeType
                              AffectedScopeIdentifier = dto.AffectedScopeIdentifier
                              ResolutionTime = DateTimeOffset.Now
                              ResolutionEvidence = "Resolved by planner via exception workbench." }

                        match! api.Resolve req with
                        | Ok resDto ->
                            return
                                Ok(
                                    ActionExecutionOutcome.Completed(
                                        $"Exception '{resDto.ExceptionId}' ({resDto.ConstraintReference}) resolved successfully.",
                                        [ ExceptionStore.storeId; DataStoreId.literal "demo-demand-store" ]
                                    )
                                )
                        | Error err -> return Error err.Message
                    | None -> return Error $"Exception '{exceptionId}' not found in query projection."
                | None -> return Error "No exception selected to resolve."
            | None -> return Error "ExceptionApi capability is not connected."
        }

/// Overrides a constraint rule for an exception under supervisor authority.
let overrideRule: ActionHandler =
    fun ctx ->
        task {
            match getApi ctx with
            | Some api ->
                match ctx.TargetKey with
                | Some exceptionId ->
                    let! dtoOpt = getExceptionDto ctx exceptionId
                    match dtoOpt with
                    | Some dto ->
                        let req: ResolveExceptionReq =
                            { ConstraintReference = dto.ConstraintReference
                              AffectedScopeType = dto.AffectedScopeType
                              AffectedScopeIdentifier = dto.AffectedScopeIdentifier
                              ResolutionTime = DateTimeOffset.Now
                              ResolutionEvidence = "Constraint rule overridden by supervisor authority." }

                        match! api.Resolve req with
                        | Ok resDto ->
                            return
                                Ok(
                                    ActionExecutionOutcome.Completed(
                                        $"Constraint rule '{resDto.ConstraintReference}' overridden and exception '{resDto.ExceptionId}' resolved.",
                                        [ ExceptionStore.storeId; DataStoreId.literal "demo-demand-store" ]
                                    )
                                )
                        | Error err -> return Error err.Message
                    | None -> return Error $"Exception '{exceptionId}' not found in query projection."
                | None -> return Error "No exception selected to override."
            | None -> return Error "ExceptionApi capability is not connected."
        }

/// Evaluates and investigates an exception causal chain.
let evaluateException: ActionHandler =
    fun ctx ->
        task {
            match ctx.TargetKey with
            | Some exceptionId ->
                return Ok(ActionExecutionOutcome.ExplainRequested(exceptionId, "Exception"))
            | None ->
                return Error "No exception selected for evaluation."
        }

let all: (ActionDefId * ActionHandler) list =
    [ ExceptionGridSchema.acknowledge.Id, acknowledgeException
      ExceptionGridSchema.resolve.Id, resolveException
      ExceptionGridSchema.overrideRule.Id, overrideRule
      ActionDefId.literal "exception.evaluate", evaluateException
      ActionDefId.literal "exception.processEvidence", acknowledgeException ]

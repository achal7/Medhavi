module Medhavi.Client.Core.Exceptions.ExceptionActions

open System
open System.Threading.Tasks
open Medhavi.Contracts.Core.Exception
open Medhavi.Client.ViewComposition.Identity
open Medhavi.Client.ActionExecution
open Medhavi.Client
open Medhavi.Client.Core.Exceptions

let private getExceptionDto (ctx: ActionExecutionContext) (exceptionId: string) : Task<ExceptionDto option> =
    ctx.Domain.Core.ExceptionManagement.Queries.GetById exceptionId

/// Processes evidence and acknowledges an active exception.
let acknowledgeException: ActionHandler =
    fun ctx ->
        task {
            let api = ctx.Domain.Core.ExceptionManagement.Commands
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
                                    [ ExceptionGrid.storeId ]
                                )
                            )
                    | Error err -> return Error err.Message
                | None -> return Error $"Exception '{exceptionId}' not found in query projection."
            | None -> return Error "No exception selected to acknowledge."
        }

/// Resolves an existing exception with governed resolution evidence.
let resolveException: ActionHandler =
    fun ctx ->
        task {
            let api = ctx.Domain.Core.ExceptionManagement.Commands
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
                                    [ ExceptionGrid.storeId; DataStoreId.literal "demo-demand-store" ]
                                )
                            )
                    | Error err -> return Error err.Message
                | None -> return Error $"Exception '{exceptionId}' not found in query projection."
            | None -> return Error "No exception selected to resolve."
        }

/// Overrides a constraint rule for an exception under supervisor authority.
let overrideRule: ActionHandler =
    fun ctx ->
        task {
            let api = ctx.Domain.Core.ExceptionManagement.Commands
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
                                    [ ExceptionGrid.storeId; DataStoreId.literal "demo-demand-store" ]
                                )
                            )
                    | Error err -> return Error err.Message
                | None -> return Error $"Exception '{exceptionId}' not found in query projection."
            | None -> return Error "No exception selected to override."
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
    [ ExceptionGrid.ExceptionActions.acknowledge.Id, acknowledgeException
      ExceptionGrid.ExceptionActions.resolve.Id, resolveException
      ExceptionGrid.ExceptionActions.overrideRule.Id, overrideRule
      ActionDefId.literal "exception.evaluate", evaluateException
      ActionDefId.literal "exception.processEvidence", acknowledgeException ]

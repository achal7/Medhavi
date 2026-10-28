module Medhavi.Client.Supply.Collaboration.SupplierCollaborationActions

open System
open System.Threading.Tasks
open Medhavi.Contracts.Core.Exception
open Medhavi.Contracts.Supply
open Medhavi.Client.ViewComposition.Identity
open Medhavi.Client.ActionExecution
open Medhavi.Client

let acceptCommitments: ActionHandler =
    fun ctx ->
        task {
            match ctx.TargetKey with
            | Some supplierId ->
                let api = ctx.Domain.Supply.CollaborateWithSuppliers.CollaborateWithSuppliersApis.Assessment
                let req: UpdateSupplierCommitmentAssessmentReq =
                    { SupplierId = supplierId
                      AssessmentTimestamp = DateTimeOffset.UtcNow }

                match! api.Update req with
                | Ok _ ->
                    return
                        Ok(
                            ActionExecutionOutcome.Completed(
                                $"Supplier commitments for '{supplierId}' accepted as firm planning constraints.",
                                [ SupplierCollaborationGrid.storeId
                                  DataStoreId.literal "supply-store"
                                  DataStoreId.literal "inventory-store" ]
                            )
                        )
                | Error err -> return Error err.Message
            | None -> return Error "No supplier selected to accept commitments."
        }

let requestAlternative: ActionHandler =
    fun ctx ->
        task {
            match ctx.TargetKey with
            | Some supplierId ->
                let api = ctx.Domain.Supply.CollaborateWithSuppliers.CollaborateWithSuppliersApis.Commitment
                let queries = ctx.Domain.Supply.CollaborateWithSuppliers.SupplierCommitmentQueries
                let! commitments = queries.Filter (fun c -> c.SupplierId = supplierId)

                match commitments |> List.tryHead with
                | Some c ->
                    let req: RecordSupplierResponseReq =
                        { SupplierId = supplierId
                          ItemId = c.ItemId
                          RequiredDate = c.RequiredDate
                          ResponseEventType = CollaborationEventType.SupplierAlternative
                          CommittedQuantity = c.CommittedQuantity
                          CommittedDate = c.CommittedDate
                          AlternativeDescription = Some "Alternative delivery schedule & split requested by planner"
                          AlternativeQuantity = c.CommittedQuantity |> Option.map (fun q -> q * 0.5m)
                          AlternativeDate = c.CommittedDate |> Option.map (fun d -> d.AddDays 3.0)
                          SupplierAssumptions = [ "Split delivery proposed across multiple batches" ]
                          ResponseTimestamp = DateTimeOffset.UtcNow }

                    match! api.RecordSupplierResponse req with
                    | Ok _ ->
                        return
                            Ok(
                                ActionExecutionOutcome.Completed(
                                    $"Alternative response logged for supplier '{supplierId}'. Updated delivery split requested.",
                                    [ SupplierCollaborationGrid.storeId
                                      DataStoreId.literal "supply-store" ]
                                )
                            )
                    | Error err -> return Error err.Message
                | None ->
                    return
                        Error
                            $"No active commitments found for supplier '{supplierId}' to request alternative schedule."
            | None -> return Error "No supplier selected to request alternative schedule."
        }

let escalateProcurement: ActionHandler =
    fun ctx ->
        task {
            match ctx.TargetKey with
            | Some supplierId ->
                let api = ctx.Domain.Core.ExceptionManagement.Commands
                let queries = ctx.Domain.Supply.CollaborateWithSuppliers.SupplierAssessmentQueries
                let! dtoOpt = queries.GetById supplierId
                match dtoOpt with
                | Some dto ->
                    let req: ExceptionEvidenceReq =
                        { ConstraintReference = $"POL-SUPPLIER-SLA-{supplierId}"
                          Classification = "SupplierCommitmentBreach"
                          AffectedScopeType = "Supplier"
                          AffectedScopeIdentifier = supplierId
                          EvidenceReference = Some $"ASSESSMENT-{supplierId}-{DateTimeOffset.UtcNow.ToUnixTimeSeconds()}"
                          Severity = Some "High"
                          EvidenceTime = DateTimeOffset.UtcNow }

                    match! api.ProcessEvidence req with
                    | Ok coreDto ->
                        return
                            Ok(
                                ActionExecutionOutcome.Completed(
                                    $"Critical supplier commitment failure for '{supplierId}' escalated to Core exception governance (Core Exception '{coreDto.ExceptionId}').",
                                    [ SupplierCollaborationGrid.storeId
                                      DataStoreId.literal "core-exception-store" ]
                                )
                            )
                    | Error err -> return Error err.Message
                | None -> return Error $"Supplier assessment for '{supplierId}' not found in query projection."
            | None -> return Error "No supplier selected to escalate procurement."
        }

let refreshHealth: ActionHandler =
    fun ctx ->
        task {
            let api = ctx.Domain.Supply.CollaborateWithSuppliers.CollaborateWithSuppliersApis.Assessment
            let queries = ctx.Domain.Supply.CollaborateWithSuppliers.SupplierAssessmentQueries
            let! assessments = queries.GetAll()

            let mutable updatedCount = 0
            for a in assessments do
                let req: UpdateSupplierCommitmentAssessmentReq =
                    { SupplierId = a.SupplierId
                      AssessmentTimestamp = DateTimeOffset.UtcNow }
                let! res = api.Update req
                if Result.isOk res then
                    updatedCount <- updatedCount + 1

            return
                Ok(
                    ActionExecutionOutcome.Completed(
                        $"Supplier commitment health refreshed for {updatedCount} supplier(s).",
                        [ SupplierCollaborationGrid.storeId ]
                    )
                )
        }

let all: (ActionDefId * ActionHandler) list =
    [ SupplierCollaborationGrid.SupplierCollaborationActions.acceptCommitments.Id, acceptCommitments
      SupplierCollaborationGrid.SupplierCollaborationActions.requestAlternative.Id, requestAlternative
      SupplierCollaborationGrid.SupplierCollaborationActions.escalateProcurement.Id, escalateProcurement
      SupplierCollaborationGrid.SupplierCollaborationActions.refreshHealth.Id, refreshHealth ]


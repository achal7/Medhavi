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
                let apiOpt =
                    match ctx.Resolve<SupplierCommitmentAssessmentApi>() with
                    | Some api -> Some api
                    | None ->
                        ctx.Resolve<DomainServices>()
                        |> Option.map (fun ds -> ds.Supply.CollaborateWithSuppliers.CollaborateWithSuppliersApis.Assessment)

                match apiOpt with
                | Some api ->
                    let req: UpdateSupplierCommitmentAssessmentReq =
                        { SupplierId = supplierId
                          AssessmentTimestamp = DateTimeOffset.UtcNow }

                    match! api.Update req with
                    | Ok _ ->
                        return
                            Ok(
                                ActionExecutionOutcome.Completed(
                                    $"Supplier commitments for '{supplierId}' accepted as firm planning constraints.",
                                    [ SupplierCollaborationStore.storeId
                                      DataStoreId.literal "supply-store"
                                      DataStoreId.literal "inventory-store" ]
                                )
                            )
                    | Error err -> return Error err.Message
                | None -> return Error "SupplierCommitmentAssessmentApi capability is not connected."
            | None -> return Error "No supplier selected to accept commitments."
        }

let requestAlternative: ActionHandler =
    fun ctx ->
        task {
            match ctx.TargetKey with
            | Some supplierId ->
                let apiOpt =
                    match ctx.Resolve<SupplierCommitmentApi>() with
                    | Some api -> Some api
                    | None ->
                        match ctx.Resolve<CollaborateWithSuppliersApis>() with
                        | Some apis -> Some apis.Commitment
                        | None ->
                            ctx.Resolve<DomainServices>()
                            |> Option.map (fun ds -> ds.Supply.CollaborateWithSuppliers.CollaborateWithSuppliersApis.Commitment)

                match apiOpt with
                | Some api ->
                    let queriesOpt =
                        match ctx.Resolve<SupplierCommitmentQueries>() with
                        | Some q -> Some q
                        | None ->
                            ctx.Resolve<DomainServices>()
                            |> Option.map (fun ds -> ds.Supply.CollaborateWithSuppliers.SupplierCommitmentQueries)

                    let! commitments =
                        match queriesOpt with
                        | Some q -> q.Filter (fun c -> c.SupplierId = supplierId)
                        | None -> Task.FromResult []

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
                        | Ok dto ->
                            return
                                Ok(
                                    ActionExecutionOutcome.Completed(
                                        $"Alternative delivery schedule & split requested from supplier '{supplierId}' (Item: {dto.ItemId}).",
                                        [ SupplierCollaborationStore.storeId
                                          DataStoreId.literal "supply-store" ]
                                    )
                                )
                        | Error err -> return Error err.Message
                    | None ->
                        let req: RecordEnterpriseRequestReq =
                            { SupplierId = supplierId
                              ItemId = "SKU-POWER-SUPPLY-850W"
                              RequiredDate = DateTimeOffset.UtcNow.AddDays 14.0
                              RequestedQuantity = 100m
                              RequestedDate = DateTimeOffset.UtcNow.AddDays 14.0
                              RequestTimestamp = DateTimeOffset.UtcNow }

                        match! api.RecordEnterpriseRequest req with
                        | Ok dto ->
                            return
                                Ok(
                                    ActionExecutionOutcome.Completed(
                                        $"Alternative procurement request initiated for supplier '{supplierId}' (Item: {dto.ItemId}, Qty: {dto.RequestedQuantity}).",
                                        [ SupplierCollaborationStore.storeId
                                          DataStoreId.literal "supply-store" ]
                                    )
                                )
                        | Error err -> return Error err.Message
                | None -> return Error "SupplierCommitmentApi capability is not connected."
            | None -> return Error "No supplier selected to request alternative schedule."
        }

let escalateProcurement: ActionHandler =
    fun ctx ->
        task {
            match ctx.TargetKey with
            | Some supplierId ->
                let exceptionApiOpt =
                    match ctx.Resolve<ExceptionApi>() with
                    | Some api -> Some api
                    | None ->
                        ctx.Resolve<DomainServices>()
                        |> Option.map (fun ds -> ds.Core.ExceptionManagement.Commands)

                match exceptionApiOpt with
                | Some exceptionApi ->
                    let assessmentQueriesOpt =
                        match ctx.Resolve<SupplierCommitmentAssessmentQueries>() with
                        | Some q -> Some q
                        | None ->
                            ctx.Resolve<DomainServices>()
                            |> Option.map (fun ds -> ds.Supply.CollaborateWithSuppliers.SupplierAssessmentQueries)

                    let! dtoOpt =
                        match assessmentQueriesOpt with
                        | Some q -> q.GetById supplierId
                        | None -> Task.FromResult None

                    let evidenceRef =
                        dtoOpt
                        |> Option.bind (fun d -> d.ActiveRiskSignals |> List.tryHead)
                        |> Option.defaultValue ($"Supplier risk state: " + (dtoOpt |> Option.map (fun d -> d.AuthoritativeHealthState) |> Option.defaultValue "AtRisk"))

                    let severity =
                        match dtoOpt |> Option.map (fun d -> d.AuthoritativeHealthState) with
                        | Some "Breached" | Some "Critical" -> "Severe"
                        | _ -> "Major"

                    let req: ExceptionEvidenceReq =
                        { ConstraintReference = "POL-SUP-COLLAB-03"
                          Classification = "LeadTimeBreach"
                          AffectedScopeType = "Supplier"
                          AffectedScopeIdentifier = supplierId
                          EvidenceReference = Some evidenceRef
                          Severity = Some severity
                          EvidenceTime = DateTimeOffset.UtcNow }

                    match! exceptionApi.ProcessEvidence req with
                    | Ok coreDto ->
                        return
                            Ok(
                                ActionExecutionOutcome.Completed(
                                    $"Critical commitment variance for supplier '{supplierId}' escalated to procurement leadership and Core exception governance (CA-C-020, Exception '{coreDto.ExceptionId}').",
                                    [ SupplierCollaborationStore.storeId
                                      DataStoreId.literal "core-exception-store" ]
                                )
                            )
                    | Error err -> return Error err.Message
                | None -> return Error "Core ExceptionApi capability is not connected."
            | None -> return Error "No supplier selected for procurement escalation."
        }

let refreshHealth: ActionHandler =
    fun ctx ->
        task {
            let healthApiOpt =
                match ctx.Resolve<SupplierCollaborationHealthApi>() with
                | Some api -> Some api
                | None ->
                    ctx.Resolve<DomainServices>()
                    |> Option.map (fun ds -> ds.Supply.CollaborateWithSuppliers.CollaborateWithSuppliersApis.Health)

            match healthApiOpt with
            | Some api ->
                let req: PublishSupplierCollaborationHealthAssessmentReq =
                    { AssessmentScopeId = "GLOBAL"
                      EvaluationPeriod = "CURRENT"
                      PublicationTimestamp = DateTimeOffset.UtcNow }

                match! api.Publish req with
                | Ok _ ->
                    return
                        Ok(
                            ActionExecutionOutcome.Completed(
                                "Supplier collaboration health assessment refreshed across all strategic vendors.",
                                [ SupplierCollaborationStore.storeId
                                  DataStoreId.literal "demo-domain-health-store" ]
                            )
                        )
                | Error err -> return Error err.Message
            | None -> return Error "SupplierCollaborationHealthApi capability is not connected."
        }

let all: (ActionDefId * ActionHandler) list =
    [ SupplierCollaborationGridSchema.acceptCommitments.Id, acceptCommitments
      SupplierCollaborationGridSchema.requestAlternative.Id, requestAlternative
      SupplierCollaborationGridSchema.escalateProcurement.Id, escalateProcurement
      SupplierCollaborationGridSchema.refreshHealth.Id, refreshHealth ]

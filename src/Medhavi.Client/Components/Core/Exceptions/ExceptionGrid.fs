namespace Medhavi.Client.Components.Core.Exceptions

open System
open Medhavi.Contracts.Core.Exception
open Medhavi.Client.ViewComposition.Identity
open Medhavi.Client.ViewComposition.Geometry
open Medhavi.Client.ViewComposition.Component
open Medhavi.Client.ViewComposition.Data
open Medhavi.Client.Rendering
open Medhavi.Client.Registries
open Medhavi.Client.Controls
open Medhavi.Client

module ExceptionGrid =

    let definitionId = ComponentDefId.literal "exception-grid"
    let storeId = DataStoreId.literal "core-exception-store"
    let schemaId = ExceptionSchema.schemaId

    let definition: ComponentDefinition =
        { Id = definitionId
          DisplayName = "Core Exceptions Workbench"
          Icon = "warning"
          Category = ComponentCategory.DataDisplay
          RequiredSchemaCapabilities = [ "Tabular" ]
          DefaultGridSpan = GridSpan.literal 12 6
          DefaultSchemaId = Some schemaId
          DefaultStoreId = Some storeId
          IsResizable = true
          IsDraggable = true
          SelectionMode = SingleSelection }

    let private fallbackDtos: ExceptionDto list =
        [ { ExceptionId = "EX-001"
            ConstraintReference = "LeadTimeViolation"
            Classification = "Temporal"
            AffectedScopeType = "Demand"
            AffectedScopeIdentifier = "D-1002"
            EvidenceReference = Some "PO-8802 delayed by 4 days"
            Severity = Some "High"
            RegistrationTime = DateTimeOffset.UtcNow.AddHours(-2.0)
            ResolutionTime = None
            ResolutionEvidence = None
            LifecycleState = "Active" }
          { ExceptionId = "EX-002"
            ConstraintReference = "CapacityOverload"
            Classification = "Resource"
            AffectedScopeType = "WorkCenter"
            AffectedScopeIdentifier = "WC-SMT-01"
            EvidenceReference = Some "Utilization 118% in Week 37"
            Severity = Some "Medium"
            RegistrationTime = DateTimeOffset.UtcNow.AddHours(-5.0)
            ResolutionTime = None
            ResolutionEvidence = None
            LifecycleState = "Active" } ]

    let store: DataStore =
        { Id = storeId
          Fetch =
            fun (ctx: ApplicationContext) ->
                async {
                    match ctx.Resolve<ExceptionQueries>() with
                    | Some queries ->
                        let! dtos = queries.GetAll() |> Async.AwaitTask
                        let rows = dtos |> List.map ExceptionSchema.adapter.ToUniversal
                        return Tabular rows
                    | None ->
                        match ctx.DomainServices with
                        | Some ds ->
                            let! dtos = ds.Core.ExceptionManagement.Queries.GetAll() |> Async.AwaitTask
                            let rows = dtos |> List.map ExceptionSchema.adapter.ToUniversal
                            return Tabular rows
                        | None ->
                            let rows = fallbackDtos |> List.map ExceptionSchema.adapter.ToUniversal
                            return Tabular rows
                } }

    let runtime: ComponentRuntime = { Render = UniversalEnterpriseGrid.render }

    let registration: ComponentRegistration =
        { Definition = definition
          Runtime = runtime }

    module ActionHandlers =
        open System.Threading.Tasks
        open Medhavi.Client.ActionExecution

        let resolveException : ActionHandler =
            fun ctx -> task {
                match ctx.Resolve<ExceptionApi>() with
                | Some api ->
                    match ctx.TargetKey with
                    | Some exceptionId ->
                        let! dtoOpt =
                            match ctx.Resolve<ExceptionQueries>() with
                            | Some q -> q.GetById exceptionId
                            | None -> Task.FromResult None

                        match dtoOpt with
                        | Some dto ->
                            let req : ResolveExceptionReq =
                                { ConstraintReference = dto.ConstraintReference
                                  AffectedScopeType = dto.AffectedScopeType
                                  AffectedScopeIdentifier = dto.AffectedScopeIdentifier
                                  ResolutionTime = DateTimeOffset.Now
                                  ResolutionEvidence = "Resolved by planner via exception workbench." }
                            match! api.Resolve(req) with
                            | Ok resDto ->
                                return Ok (ActionExecutionOutcome.Completed(
                                    $"Exception '{resDto.ExceptionId}' ({resDto.ConstraintReference}) resolved successfully.",
                                    [ storeId; DataStoreId.literal "demo-demand-store" ]
                                ))
                            | Error err ->
                                return Error err.Message
                        | None ->
                            return Error $"Exception '{exceptionId}' not found in query projection."
                    | None ->
                        return Error "No exception selected to resolve."
                | None ->
                    return Error "ExceptionApi capability is not connected."
            }

        let investigate : ActionHandler =
            fun ctx -> task {
                match ctx.TargetKey with
                | Some exceptionId ->
                    return Ok (ActionExecutionOutcome.ExplainRequested(exceptionId, "Exception"))
                | None ->
                    return Error "No exception selected to investigate."
            }

        let all : (ActionDefId * ActionHandler) list =
            [ ExceptionActions.resolve.Id, resolveException
              ExceptionActions.investigate.Id, investigate ]

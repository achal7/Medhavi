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

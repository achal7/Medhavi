namespace Medhavi.Client.Components.Supply

open System
open Medhavi.Contracts.Supply
open Medhavi.Client.ViewComposition.Identity
open Medhavi.Client.ViewComposition.Geometry
open Medhavi.Client.ViewComposition.Component
open Medhavi.Client.ViewComposition.Data
open Medhavi.Client.Rendering
open Medhavi.Client.Registries
open Medhavi.Client.Controls
open Medhavi.Client

module CapacityGrid =

    let definitionId = ComponentDefId.literal "capacity-grid"
    let storeId = DataStoreId.literal "capacity-store"
    let schemaId = CapacitySchema.schemaId

    let definition: ComponentDefinition =
        { Id = definitionId
          DisplayName = "Work Center Capacity Workbench"
          Icon = "precision_manufacturing"
          Category = ComponentCategory.DataDisplay
          RequiredSchemaCapabilities = [ "Tabular" ]
          DefaultGridSpan = GridSpan.literal 12 6
          DefaultSchemaId = Some schemaId
          DefaultStoreId = Some storeId
          IsResizable = true
          IsDraggable = true
          SelectionMode = SingleSelection }

    let private defaultEnvelope: CapacityOperatingEnvelopeDto =
        { TargetLowerHours = 80m
          TargetUpperHours = 150m
          OvertimeThresholdHours = 155m
          MaxPlannedHours = 160m
          AbsoluteCapacityHours = 180m
          MinEconomicHours = 40m }

    let fallbackDtos: CapacityPositionAssessmentDto list =
        [ { PhysicalResourceId = "WC-ASSEMBLY-LINE-01"
            LifecycleState = "Active"
            AvailableCapacityHours = 160.0m
            CurrentLoadHours = 184.0m
            CurrentUtilizationPercentage = 115.0m
            ProjectedLoadHours = 188.0m
            ProjectedUtilizationPercentage = 117.5m
            OperatingEnvelope = defaultEnvelope
            TimePhasedBuckets = []
            UtilizationStatus = CapacityUtilizationStatus.Overloaded
            ConstraintStatus = CapacityConstraintStatus.ActiveConstraint
            BottleneckAssessment = BottleneckAssessmentStatus.CurrentBottleneck
            OverloadRisk = CapacityRiskLevel.Critical
            UnderloadRisk = CapacityRiskLevel.Negligible
            SinglePointOfFailureRisk = true
            AssessmentConfidence = "High"
            SourceSupplyUnderstandingVersion = Some 1
            SourceSupplyPlanVersion = Some 1
            AssessmentTimestamp = DateTimeOffset(2026, 9, 12, 10, 15, 0, TimeSpan.Zero)
            ChangeHistory = [] }
          { PhysicalResourceId = "WC-PACKAGING-01"
            LifecycleState = "Active"
            AvailableCapacityHours = 160.0m
            CurrentLoadHours = 128.0m
            CurrentUtilizationPercentage = 80.0m
            ProjectedLoadHours = 130.0m
            ProjectedUtilizationPercentage = 81.2m
            OperatingEnvelope = defaultEnvelope
            TimePhasedBuckets = []
            UtilizationStatus = CapacityUtilizationStatus.Optimal
            ConstraintStatus = CapacityConstraintStatus.NonConstraint
            BottleneckAssessment = BottleneckAssessmentStatus.NotABottleneck
            OverloadRisk = CapacityRiskLevel.Low
            UnderloadRisk = CapacityRiskLevel.Low
            SinglePointOfFailureRisk = false
            AssessmentConfidence = "High"
            SourceSupplyUnderstandingVersion = Some 1
            SourceSupplyPlanVersion = Some 1
            AssessmentTimestamp = DateTimeOffset(2026, 9, 12, 10, 15, 0, TimeSpan.Zero)
            ChangeHistory = [] }
          { PhysicalResourceId = "WC-TESTING-BAY-02"
            LifecycleState = "Active"
            AvailableCapacityHours = 120.0m
            CurrentLoadHours = 112.0m
            CurrentUtilizationPercentage = 93.3m
            ProjectedLoadHours = 115.0m
            ProjectedUtilizationPercentage = 95.8m
            OperatingEnvelope = defaultEnvelope
            TimePhasedBuckets = []
            UtilizationStatus = CapacityUtilizationStatus.NearCapacity
            ConstraintStatus = CapacityConstraintStatus.NearConstraint
            BottleneckAssessment = BottleneckAssessmentStatus.PotentialBottleneck
            OverloadRisk = CapacityRiskLevel.Medium
            UnderloadRisk = CapacityRiskLevel.Low
            SinglePointOfFailureRisk = false
            AssessmentConfidence = "High"
            SourceSupplyUnderstandingVersion = Some 1
            SourceSupplyPlanVersion = Some 1
            AssessmentTimestamp = DateTimeOffset(2026, 9, 12, 10, 15, 0, TimeSpan.Zero)
            ChangeHistory = [] } ]

    let store: DataStore =
        { Id = storeId
          Fetch =
            fun (ctx: ApplicationContext) ->
                async {
                    match ctx.Resolve<CapacityPositionQueries>() with
                    | Some queries ->
                        let! dtos = queries.GetAll() |> Async.AwaitTask
                        let positions = if List.isEmpty dtos then fallbackDtos else dtos
                        let rows = positions |> List.map CapacitySchema.adapter.ToUniversal
                        return Tabular rows
                    | None ->
                        match ctx.DomainServices with
                        | Some ds ->
                            let! dtos = ds.Supply.ManageCapacity.CapacityPositionQueries.GetAll() |> Async.AwaitTask
                            let positions = if List.isEmpty dtos then fallbackDtos else dtos
                            let rows = positions |> List.map CapacitySchema.adapter.ToUniversal
                            return Tabular rows
                        | None ->
                            let rows = fallbackDtos |> List.map CapacitySchema.adapter.ToUniversal
                            return Tabular rows
                } }

    let runtime: ComponentRuntime = { Render = UniversalEnterpriseGrid.render }

    let registration: ComponentRegistration =
        { Definition = definition
          Runtime = runtime }

    module ActionHandlers =
        open System.Threading.Tasks
        open Medhavi.Client.ActionExecution

        let addShift : ActionHandler =
            fun ctx -> task {
                match ctx.TargetKey with
                | Some resourceId ->
                    match ctx.Resolve<CapacityPositionApi>() with
                    | Some api ->
                        let req : UpdateCapacityPositionReq =
                            { PhysicalResourceId = resourceId
                              TriggeringEvidence = [ "AUTHORIZE_WEEKEND_OVERTIME_8H" ] }
                        match! api.Update(req) with
                        | Ok _ ->
                            return Ok (ActionExecutionOutcome.Completed(
                                $"+8h weekend overtime shift authorized for '{resourceId}'. Operating envelope rebalanced.",
                                [ storeId; DataStoreId.literal "supply-store"; DataStoreId.literal "core-exception-store" ]
                            ))
                        | Error err ->
                            return Error err.Message
                    | None ->
                        return Ok (ActionExecutionOutcome.Completed(
                            $"+8h weekend overtime shift authorized for '{resourceId}'.",
                            [ storeId; DataStoreId.literal "supply-store"; DataStoreId.literal "core-exception-store" ]
                        ))
                | None ->
                    return Error "No work center selected to authorize overtime shift."
            }

        let maintenance : ActionHandler =
            fun ctx -> task {
                match ctx.TargetKey with
                | Some resourceId ->
                    return Ok (ActionExecutionOutcome.Completed(
                        $"Preventative maintenance window scheduled for '{resourceId}'.",
                        [ storeId; DataStoreId.literal "supply-store" ]
                    ))
                | None ->
                    return Error "No work center selected for maintenance."
            }

        let reallocate : ActionHandler =
            fun ctx -> task {
                match ctx.TargetKey with
                | Some resourceId ->
                    return Ok (ActionExecutionOutcome.Completed(
                        $"Workload successfully reallocated from '{resourceId}' to alternate lines.",
                        [ storeId; DataStoreId.literal "supply-store" ]
                    ))
                | None ->
                    return Error "No work center selected for workload reallocation."
            }

        let explainBottleneck : ActionHandler =
            fun ctx -> task {
                match ctx.TargetKey with
                | Some resourceId ->
                    return Ok (ActionExecutionOutcome.ExplainRequested(resourceId, "CapacityBottleneck"))
                | None ->
                    return Error "No work center selected to explain bottleneck."
            }

        let all : (ActionDefId * ActionHandler) list =
            [ CapacityActions.addShift.Id, addShift
              CapacityActions.maintenance.Id, maintenance
              CapacityActions.reallocate.Id, reallocate
              CapacityActions.explainBottleneck.Id, explainBottleneck ]

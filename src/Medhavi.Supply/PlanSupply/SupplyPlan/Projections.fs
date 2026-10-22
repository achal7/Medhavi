module Medhavi.Supply.PlanSupply.SupplyPlan.Projections

open Medhavi.SemanticModel
open Medhavi.Contracts.Supply
open Medhavi.Supply
open Model

let private mapSupplyType = function
    | Model.PlannedSupplyType.Production -> Medhavi.Contracts.Supply.PlannedSupplyType.Production
    | Model.PlannedSupplyType.Procurement -> Medhavi.Contracts.Supply.PlannedSupplyType.Procurement
    | Model.PlannedSupplyType.Transfer -> Medhavi.Contracts.Supply.PlannedSupplyType.Transfer

let private mapFeasibility = function
    | Model.PlanFeasibilityStatus.FullyFeasible -> Medhavi.Contracts.Supply.PlanFeasibilityStatus.FullyFeasible
    | Model.PlanFeasibilityStatus.ConstrainedInfeasible -> Medhavi.Contracts.Supply.PlanFeasibilityStatus.ConstrainedInfeasible

let private mapOutcome = function
    | Model.PlanEvaluationOutcome.Accepted -> Medhavi.Contracts.Supply.PlanEvaluationOutcome.Accepted
    | Model.PlanEvaluationOutcome.AcceptedWithWarnings -> Medhavi.Contracts.Supply.PlanEvaluationOutcome.AcceptedWithWarnings
    | Model.PlanEvaluationOutcome.Rejected -> Medhavi.Contracts.Supply.PlanEvaluationOutcome.Rejected

let private mapPlannedOrder (o: Model.PlannedSupplyOrder) : PlannedOrderDto =
    { OrderId = PlannedSupplyOrderId.value o.OrderId
      Item = ItemId.value o.Item
      Location = LocationId.value o.Location
      SupplyType = mapSupplyType o.SupplyType
      Quantity = Quantity.value o.Quantity
      UnitOfMeasure = UnitOfMeasureId.value o.UnitOfMeasure
      ReleaseDate = Timestamp.value o.ReleaseDate
      DueDate = Timestamp.value o.DueDate
      AssignedResource = o.AssignedResource |> Option.map PhysicalResourceId.value
      AssignedSupplier = o.AssignedSupplier |> Option.map SupplierId.value
      SourceLocation = o.SourceLocation |> Option.map LocationId.value }

let private mapShortfall (s: Model.ShortfallRecord) : ProjectedShortfallDto =
    { Item = ItemId.value s.Item
      Location = LocationId.value s.Location
      PeriodBucket = s.PeriodBucket
      ShortfallQuantity = Quantity.value s.ShortfallQuantity
      UnitOfMeasure = UnitOfMeasureId.value s.UnitOfMeasure
      Reason = s.Reason }

let private mapConstraintViolation (c: Model.ConstraintViolationRecord) : ConstraintViolationDto =
    { Resource = PhysicalResourceId.value c.Resource
      PeriodBucket = c.PeriodBucket
      RequiredLoad = c.RequiredLoad
      MaxPlannedCapacity = c.MaxPlannedCapacity
      OverloadQuantity = c.OverloadQuantity
      UnitOfMeasure = UnitOfMeasureId.value c.UnitOfMeasure
      IsRelaxationApproved = c.IsRelaxationApproved
      RelaxationApprovalReference = c.RelaxationApprovalReference }

let private mapVersion (v: Model.SupplyPlanVersion) : SupplyPlanVersionDto =
    { VersionId = SupplyPlanVersionId.value v.VersionId
      VersionNumber = v.VersionNumber
      LifecycleState = v.State.ToString()
      PlannedOrders = v.PlannedOrders |> List.map mapPlannedOrder
      ProjectedShortfalls = v.ProjectedShortfalls |> List.map mapShortfall
      ConstraintViolations = v.ConstraintViolations |> List.map mapConstraintViolation
      FeasibilityStatus = mapFeasibility v.FeasibilityStatus
      ServiceLevelAchieved = v.ServiceLevelAchieved
      MaxCapacityUtilization = v.MaxCapacityUtilization
      PlanStabilityScore = v.PlanStabilityScore
      PlanConfidenceScore = v.PlanConfidenceScore
      SourceSupplyUnderstandingVersion = v.SourceSupplyUnderstandingVersion
      SourceForecastPublicationVersion = v.SourceForecastPublicationVersion
      SourceEnterprisePictureVersion = v.SourceEnterprisePictureVersion
      GenerationTime = Timestamp.value v.GenerationTime
      EvaluationOutcome = v.EvaluationOutcome |> Option.map mapOutcome
      EvaluationTime = v.EvaluationTime |> Option.map Timestamp.value
      PublicationTime = v.PublicationTime |> Option.map Timestamp.value }

/// Map domain aggregate state to DTO
let mapToDto (plan: Model.SupplyPlan) : SupplyPlanDto =
    let currentVer =
        plan.Versions
        |> List.tryFind (fun v -> v.State = Model.VersionState.Published)
        |> Option.orElseWith (fun () -> plan.Versions |> List.sortByDescending (fun v -> v.VersionNumber) |> List.tryHead)
        |> Option.map mapVersion

    let allVers =
        plan.Versions
        |> List.sortByDescending (fun v -> v.VersionNumber)
        |> List.map mapVersion

    { PlanningScopeId = PlanningScopeId.value plan.PlanningScopeId
      ForecastHorizonId = ForecastHorizonId.value plan.ForecastHorizonId
      CurrentVersion = currentVer
      AllVersions = allVers }

/// Projection state: Map of composite key (PlanningScopeId, ForecastHorizonId) to DTO
type State = Map<SupplyPlanId, SupplyPlanDto>

let initial: State = Map.empty

/// Pure projection fold (catamorphism)
let apply (state: State) (event: Model.SupplyPlanEvent) : State =
    let plan =
        match event with
        | Model.SupplyPlanGenerated(p, _)
        | Model.SupplyPlanEvaluated(p, _, _)
        | Model.SupplyPlanPublished(p, _, _) -> p

    let key = SupplyPlanId.ofScopeAndHorizon plan.PlanningScopeId plan.ForecastHorizonId
    state |> Map.add key (mapToDto plan)

/// Seed projection from existing aggregates
let seedFromAggregates (aggregates: Model.SupplyPlan list) : State =
    aggregates
    |> List.fold (fun state p ->
        let key = SupplyPlanId.ofScopeAndHorizon p.PlanningScopeId p.ForecastHorizonId
        Map.add key (mapToDto p) state) initial

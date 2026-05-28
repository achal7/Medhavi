module Medhavi.MasterData.Application.TransportLeg

open System
open Medhavi.Common.Validation
open Medhavi.Common.Patterns
open Medhavi.SharedKernel
open Medhavi.SharedKernel.Aggregate
open Medhavi.MasterData.Domain.UomAgg
open Medhavi.MasterData.Domain.TransportAgg
open Medhavi.Contracts.Integration
open Medhavi.Infrastructure
open Medhavi.Infrastructure.Projections

module ACL =
    let parseTransportMode (t: string) : Result<TransportMode, DomainError> =
        match t.Trim().ToLowerInvariant() with
        | "air" -> Ok TransportMode.Air
        | "road" -> Ok TransportMode.Road
        | "rail" -> Ok TransportMode.Rail
        | "sea" -> Ok TransportMode.Sea
        | "pipeline" -> Ok TransportMode.Pipeline
        | "" -> Error(DomainError.validation "TransportMode cannot be empty")
        | s -> Ok(TransportMode.Other s)

    let parseTransportSchedule (s: string) : Result<TransportSchedule, DomainError> =
        match s.Trim().ToLowerInvariant() with
        | "daily" -> Ok TransportSchedule.Daily
        | "ondemand"
        | "on-demand" -> Ok OnDemand
        | _ when s.StartsWith("weekly:") ->
            let value = s.Substring("weekly:".Length)

            match System.Int32.TryParse value with
            | true, d when d >= 0 && d <= 6 -> Ok(Weekly d)
            | _ -> Error(DomainError.validation "Weekly schedule requires day-of-week 0-6")
        | _ when s.StartsWith("monthly:") ->
            let value = s.Substring("monthly:".Length)

            match System.Int32.TryParse value with
            | true, d when d >= 1 && d <= 31 -> Ok(Monthly d)
            | _ -> Error(DomainError.validation "Monthly schedule requires day-of-month 1-31")
        | _ when s.StartsWith("custom:") -> Ok(TransportSchedule.Custom(s.Substring("custom:".Length)))
        | _ -> Error(DomainError.validation $"Unknown transport schedule: {s}")

    let parseConstraints (items: string list) : Result<TransportConstraint list, DomainError> =
        let parseOne (c: string) =
            match c.Trim().ToLowerInvariant() with
            | "hazmat" -> Ok TransportConstraint.Hazmat
            | "temperaturecontrolled"
            | "temperature-controlled" -> Ok TemperatureControlled
            | "refrigerated" -> Ok Refrigerated
            | "fragile" -> Ok Fragile
            | "oversized" -> Ok Oversized
            | s when s.StartsWith("regulatory:") -> Ok(Regulatory(s.Substring("regulatory:".Length)))
            | s when s.StartsWith("custom:") -> Ok(TransportConstraint.Custom(s.Substring("custom:".Length)))
            | _ -> Error(DomainError.validation $"Unknown transport constraint: {c}")

        items
        |> List.map (parseOne >> fromResult)
        |> sequence
        |> toResult
        |> Result.mapError DomainError.combineValidationErrors

    let toDefineCommand (req: TransportLegDefineReq) : Result<DefineTransportLegCmd, DomainError> =
        let constraintsVal = parseConstraints req.Constraints

        let capacity =
            match req.Capacity with
            | None -> Ok None
            | Some v -> PositiveDecimal.create v |> Result.map Some

        let capacityUnit =
            match req.CapacityUnit with
            | None -> Ok None
            | Some v -> UomId.create v |> Result.map Some

        let reliability =
            match req.Reliability with
            | None -> Ok None
            | Some v -> Percent.create (decimal v) |> Result.map Some

        let co2 =
            match req.CO2PerUnit with
            | None -> Ok None
            | Some v -> PositiveDecimal.create v |> Result.map Some

        let makeCmd
            (legId: TransportLegId)
            (origin: StockingPointId)
            (dest: StockingPointId)
            (mode: TransportMode)
            (schedule: TransportSchedule)
            constrs
            cap
            capUnit
            rel
            co2
            : DefineTransportLegCmd =
            { Id = legId
              Origin = origin
              Destination = dest
              Mode = mode
              Schedule = schedule
              LeadTime = TimeSpan.FromMinutes(float req.LeadTimeMinutes)
              Capacity = cap
              CapacityUnit = capUnit
              Cutoff =
                req.CutoffMinutes
                |> Option.map (float >> TimeSpan.FromMinutes)
              Constraints = constrs
              Reliability = rel
              CO2PerUnit = co2
              EffectiveStart = Timestamp.create req.EffectiveStart
              EffectiveEnd = req.EffectiveEnd |> Option.map Timestamp.create
              Created = Timestamp.create req.Created }

        makeCmd
        <!> (TransportLegId.create req.Id |> fromResult)
        <*> (StockingPointId.create req.Origin |> fromResult)
        <*> (StockingPointId.create req.Destination
             |> fromResult)
        <*> (parseTransportMode req.Mode |> fromResult)
        <*> (parseTransportSchedule req.Schedule |> fromResult)
        <*> (constraintsVal |> fromResult)
        <*> (capacity |> fromResult)
        <*> (capacityUnit |> fromResult)
        <*> (reliability |> fromResult)
        <*> (co2 |> fromResult)
        |> toResult
        |> Result.mapError DomainError.combineValidationErrors

    let toUpdateCommand (req: TransportLegUpdateReq) : Result<UpdateTransportLegCmd, DomainError> =
        let mode =
            match req.Mode with
            | None -> Ok None
            | Some m -> parseTransportMode m |> Result.map Some

        let schedule =
            match req.Schedule with
            | None -> Ok None
            | Some s -> parseTransportSchedule s |> Result.map Some

        let constraints =
            match req.Constraints with
            | None -> Ok None
            | Some c -> parseConstraints c |> Result.map Some

        let reliability =
            match req.Reliability with
            | None -> Ok None
            | Some r -> Percent.create (decimal r) |> Result.map Some

        let capacityUnitVal =
            match req.CapacityUnit with
            | None -> Ok None
            | Some v -> UomId.create v |> Result.map Some

        let makeCmd (legId: TransportLegId) m s constrs rel capUnit : UpdateTransportLegCmd =
            { Id = legId
              Mode = m
              Schedule = s
              LeadTime =
                req.LeadTimeMinutes
                |> Option.map (float >> TimeSpan.FromMinutes)
              Capacity = req.Capacity
              CapacityUnit = capUnit
              Cutoff =
                req.CutoffMinutes
                |> Option.map (float >> TimeSpan.FromMinutes)
              Constraints = constrs
              Reliability =
                rel
                |> Option.map Percent.value
                |> Option.map float
              CO2PerUnit = req.CO2PerUnit
              EffectiveEnd = req.EffectiveEnd |> Option.map Timestamp.create
              Modified = Timestamp.create req.Modified }

        makeCmd
        <!> (TransportLegId.create req.Id |> fromResult)
        <*> (mode |> fromResult)
        <*> (schedule |> fromResult)
        <*> (constraints |> fromResult)
        <*> (reliability |> fromResult)
        <*> (capacityUnitVal |> fromResult)
        |> toResult
        |> Result.mapError DomainError.combineValidationErrors

    let toDeactivateCommand (req: TransportLegDeactivateReq) : Result<DeactivateTransportLegCmd, DomainError> =
        TransportLegId.create req.Id
        |> Result.map (fun id ->
            { Id = id
              DeactivatedAt = Timestamp.create req.DeactivatedAt }
            : DeactivateTransportLegCmd)

type Decision = Decision<TransportLeg, TransportLegEvent>

type TransportLegCapabilities =
    { Define: TransportLegDefineReq -> TaskResult<Decision, ApplicationError>
      Update: TransportLegUpdateReq -> TaskResult<Decision, ApplicationError>
      Deactivate: TransportLegDeactivateReq -> TaskResult<Decision, ApplicationError> }

let createCapabilities (repo: Repository<TransportLeg, string, TransportLegEvent>) =
    { Define =
        liftCmdResult ACL.toDefineCommand
        >=> handleCommand (fun c -> TransportLegId.value c.Id) repo DefineTransportLeg decide
      Update =
        liftCmdResult ACL.toUpdateCommand
        >=> handleCommand (fun c -> TransportLegId.value c.Id) repo UpdateTransportLeg decide
      Deactivate =
        liftCmdResult ACL.toDeactivateCommand
        >=> handleCommand (fun c -> TransportLegId.value c.Id) repo DeactivateTransportLeg decide }

let mapTransportLegDto (l: TransportLeg) : Medhavi.Contracts.Domain.TransportLeg =
    let modeStr =
        match l.Mode with
        | TransportMode.Air -> "Air"
        | TransportMode.Road -> "Road"
        | TransportMode.Rail -> "Rail"
        | TransportMode.Sea -> "Sea"
        | TransportMode.Pipeline -> "Pipeline"
        | TransportMode.Other s -> s

    { Id = TransportLegId.value l.Id
      Origin = StockingPointId.value l.Origin
      Destination = StockingPointId.value l.Destination
      Mode = modeStr
      LeadTimeMinutes = decimal l.LeadTime.TotalMinutes
      Capacity =
        l.Capacity
        |> Option.map (fun c -> PositiveDecimal.value c)
      CapacityUnit = l.CapacityUnit |> Option.map UomId.value
      Status = l.Status.ToBool() }

let evolveProjection (state: Map<string, Medhavi.Contracts.Domain.TransportLeg>) (evt: TransportLegEvent) =
    match evt with
    | TransportLegDefined e ->
        let modeStr =
            match e.Mode with
            | TransportMode.Air -> "Air"
            | TransportMode.Road -> "Road"
            | TransportMode.Rail -> "Rail"
            | TransportMode.Sea -> "Sea"
            | TransportMode.Pipeline -> "Pipeline"
            | TransportMode.Other s -> s

        let dto: Medhavi.Contracts.Domain.TransportLeg =
            { Id = TransportLegId.value e.Id
              Origin = StockingPointId.value e.Origin
              Destination = StockingPointId.value e.Destination
              Mode = modeStr
              LeadTimeMinutes = decimal e.LeadTime.TotalMinutes
              Capacity =
                e.Capacity
                |> Option.map (fun c -> PositiveDecimal.value c)
              CapacityUnit = e.CapacityUnit |> Option.map UomId.value
              Status = true }

        Map.add dto.Id dto state
    | TransportLegUpdated e ->
        let key = TransportLegId.value e.Id

        match Map.tryFind key state with
        | Some existing ->
            let modeStrOpt =
                e.Mode
                |> Option.map (fun m ->
                    match m with
                    | TransportMode.Air -> "Air"
                    | TransportMode.Road -> "Road"
                    | TransportMode.Rail -> "Rail"
                    | TransportMode.Sea -> "Sea"
                    | TransportMode.Pipeline -> "Pipeline"
                    | TransportMode.Other s -> s)

            let updated =
                { existing with
                    Mode = defaultArg modeStrOpt existing.Mode
                    LeadTimeMinutes =
                        match e.LeadTime with
                        | Some lt -> decimal lt.TotalMinutes
                        | None -> existing.LeadTimeMinutes
                    Capacity =
                        match e.Capacity with
                        | Some cap -> Some(PositiveDecimal.value cap)
                        | None -> existing.Capacity
                    CapacityUnit =
                        match e.CapacityUnit with
                        | Some cu -> Some(UomId.value cu)
                        | None -> existing.CapacityUnit }

            Map.add key updated state
        | None -> state
    | TransportLegDeactivated e ->
        let key = TransportLegId.value e.Id

        match Map.tryFind key state with
        | Some existing -> Map.add key { existing with Status = false } state
        | None -> state

let createProjectionAgent () =
    ProjectionAgent<Map<string, Medhavi.Contracts.Domain.TransportLeg>, TransportLegEvent>(
        evolveProjection,
        Map.empty,
        "TransportLegReadModel"
    )

let createQueryService agent = QueryServiceBase.getQueryService agent id

open Medhavi.SharedKernel.API

let createTransportLegApi (capabilities: TransportLegCapabilities) agent =
    { Define =
        fun req ->
            capabilities.Define req
            |> TaskResult.map (fun d -> d.NewState)
            |> TaskResult.map mapTransportLegDto
      DefineBulk =
        fun reqs ->
            reqs
            |> List.map capabilities.Define
            |> TaskResult.sequence
            |> TaskResult.map (fun decisions ->
                decisions
                |> List.map (fun d -> d.NewState)
                |> List.map mapTransportLegDto)
      Update =
        fun req ->
            capabilities.Update req
            |> TaskResult.map (fun d -> d.NewState)
            |> TaskResult.map mapTransportLegDto
      Deactivate =
        fun req ->
            capabilities.Deactivate req
            |> TaskResult.map (fun d -> d.NewState)
            |> TaskResult.map mapTransportLegDto
      QueryService = QueryServiceBase.getQueryService agent id }
    : TransportLegApi

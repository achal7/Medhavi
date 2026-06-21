module Medhavi.Supply.Application.MaterialReservation

open System
open Medhavi.Common.Patterns
open Medhavi.Common.Validation
open Medhavi.Contracts.Supply
open Medhavi.Infrastructure.Projections
open Medhavi.SharedKernel
open Medhavi.SharedKernel.Aggregate
open Medhavi.Supply.Domain.MaterialReservationAgg

module ACL =
    let toCreateTentativeCommand (req: MaterialReservationCreateReq) : Validation<CreateTentativeCmd, DomainError> =
        let make skuId spId =
            { Id = req.Id
              IdempotencyKey = req.IdempotencyKey
              SkuId = skuId
              StockingPointId = spId
              Quantity = req.Quantity
              RequiredDate = req.RequiredDate
              ExpiryTime = req.ExpiryTime }

        make <!> (SkuId.create req.SkuId |> fromResult) <*> (StockingPointId.create req.StockingPointId |> fromResult)

    let toConfirmCommand (req: MaterialReservationConfirmReq) : Result<ConfirmCmd, DomainError> = Ok { Id = req.Id }

    let toReleaseCommand (req: MaterialReservationReleaseReq) : Result<ReleaseCmd, DomainError> = Ok { Id = req.Id }

    let toReduceCommand (req: MaterialReservationReduceReq) : Result<ReduceCmd, DomainError> =
        Ok
            { Id = req.Id
              NewQuantity = req.NewQuantity }

    let toExpireCommand (req: MaterialReservationExpireReq) : Result<ExpireCmd, DomainError> = Ok { Id = req.Id }

    let toContract (res: MaterialReservation) : Medhavi.Contracts.Supply.MaterialReservation =
        { Id = res.Id
          IdempotencyKey = res.IdempotencyKey
          SkuId = SkuId.value res.SkuId
          StockingPointId = StockingPointId.value res.StockingPointId
          Quantity = Quantity.value res.Quantity
          State = res.State
          RequiredDate = res.RequiredDate
          ExpiryTime = res.ExpiryTime
          Created = Timestamp.value res.Created
          Modified = Timestamp.value res.Modified }

type Decision = Decision<MaterialReservation, MaterialReservationEvent>

type MaterialReservationCapabilities =
    { CreateTentative: MaterialReservationCreateReq -> TaskResult<Decision, ApplicationError>
      Confirm: MaterialReservationConfirmReq -> TaskResult<Decision, ApplicationError>
      Release: MaterialReservationReleaseReq -> TaskResult<Decision, ApplicationError>
      Reduce: MaterialReservationReduceReq -> TaskResult<Decision, ApplicationError>
      Expire: MaterialReservationExpireReq -> TaskResult<Decision, ApplicationError> }

let createCapabilities
    (repo: Repository<MaterialReservation, string, MaterialReservationEvent>)
    =
    { CreateTentative =
        liftCmdValidation ACL.toCreateTentativeCommand >=> handleCommand (fun cmd -> cmd.Id) repo CreateTentative decide

      Confirm = liftCmdResult ACL.toConfirmCommand >=> handleCommand (fun cmd -> cmd.Id) repo Confirm decide

      Release = liftCmdResult ACL.toReleaseCommand >=> handleCommand (fun cmd -> cmd.Id) repo Release decide

      Reduce = liftCmdResult ACL.toReduceCommand >=> handleCommand (fun cmd -> cmd.Id) repo Reduce decide

      Expire = liftCmdResult ACL.toExpireCommand >=> handleCommand (fun cmd -> cmd.Id) repo Expire decide }

let evolveProjection
    (state: Map<string, Medhavi.Contracts.Supply.MaterialReservation>)
    (evt: MaterialReservationEvent)
    =
    match evt with
    | ReservationCreated res -> Map.add res.Id (ACL.toContract res) state
    | ReservationConfirmed e ->
        match Map.tryFind e.Id state with
        | Some s ->
            Map.add
                e.Id
                { s with
                    State = "Confirmed"
                    Modified = DateTimeOffset.UtcNow }
                state
        | None -> state
    | ReservationReleased e ->
        match Map.tryFind e.Id state with
        | Some s ->
            Map.add
                e.Id
                { s with
                    State = "Released"
                    Modified = DateTimeOffset.UtcNow }
                state
        | None -> state
    | ReservationReduced e ->
        match Map.tryFind e.Id state with
        | Some s ->
            Map.add
                e.Id
                { s with
                    Quantity = e.NewQuantity
                    State = "Reduced"
                    Modified = DateTimeOffset.UtcNow }
                state
        | None -> state
    | ReservationExpired e ->
        match Map.tryFind e.Id state with
        | Some s ->
            Map.add
                e.Id
                { s with
                    State = "Expired"
                    Modified = DateTimeOffset.UtcNow }
                state
        | None -> state

let createProjectionAgent () =
    ProjectionAgent<Map<string, Medhavi.Contracts.Supply.MaterialReservation>, MaterialReservationEvent>(
        evolveProjection,
        Map.empty,
        "MaterialReservationReadModel"
    )

let createQueryService agent = QueryServiceBase.getQueryService agent id

let createMaterialReservationApi (capabilities: MaterialReservationCapabilities) =
    { CreateTentative =
        fun req ->
            capabilities.CreateTentative req |> TaskResult.map(fun d -> d.NewState) |> TaskResult.map ACL.toContract |> TaskResult.mapError ApplicationError.mapToApiError
      Confirm =
        fun req -> capabilities.Confirm req |> TaskResult.map(fun d -> d.NewState) |> TaskResult.map ACL.toContract |> TaskResult.mapError ApplicationError.mapToApiError
      Release =
        fun req -> capabilities.Release req |> TaskResult.map(fun d -> d.NewState) |> TaskResult.map ACL.toContract |> TaskResult.mapError ApplicationError.mapToApiError
      Reduce =
        fun req -> capabilities.Reduce req |> TaskResult.map(fun d -> d.NewState) |> TaskResult.map ACL.toContract |> TaskResult.mapError ApplicationError.mapToApiError
      Expire =
        fun req -> capabilities.Expire req |> TaskResult.map(fun d -> d.NewState) |> TaskResult.map ACL.toContract |> TaskResult.mapError ApplicationError.mapToApiError }
    : MaterialReservationApi

module Medhavi.Supply.Application.SupplierOffer

open Medhavi
open Medhavi.Common.Patterns
open Medhavi.Common.Validation
open Medhavi.Contracts.Domain
open Medhavi.Contracts.Integration
open Medhavi.Infrastructure.Projections
open Medhavi.SharedKernel
open Medhavi.SharedKernel.API
open Medhavi.SharedKernel.Aggregate
open Medhavi.Supply.Domain.SupplierOfferAgg

module ACL =
    open System

    let sequenceOpt (opt: Result<'T, 'Err> option) : Result<'T option, 'Err> =
        match opt with
        | None -> Ok None
        | Some(Ok x) -> Ok(Some x)
        | Some(Error e) -> Error e

    let mapPriceTierReq (t: PriceTierReq) : PriceTierCmd =
        { MinQuantity = t.MinQuantity
          MaxQuantity = t.MaxQuantity
          PricePerUnit = t.PricePerUnit
          Currency = t.Currency }

    let mapCapacityWindowReq (w: CapacityWindowReq) : CapacityWindowCmd =
        { WindowId = w.WindowId
          StartDate = Timestamp.create w.StartDate
          EndDate = Timestamp.create w.EndDate
          MaxQuantity = w.MaxQuantity
          AvailableQuantity = w.AvailableQuantity }

    let toDefineCommand (req: SupplierOfferDefineReq) : Validation<DefineSupplierOfferCmd, DomainError> =
        let make (suppId: SupplierId) (skuId: SkuId) (spId: StockingPointId option) : DefineSupplierOfferCmd =
            { Id = req.Id
              SupplierId = suppId
              SkuId = skuId
              StockingPointId = spId
              Moq = req.Moq
              LotSize = req.LotSize
              LeadTimeP50 =
                req.LeadTimeP50Minutes
                |> Option.map (float >> TimeSpan.FromMinutes)
              LeadTimeP95 =
                req.LeadTimeP95Minutes
                |> Option.map (float >> TimeSpan.FromMinutes)
              PriceTiers = req.PriceTiers |> List.map mapPriceTierReq
              Reliability = req.Reliability
              Incoterm = req.Incoterm |> Option.map (Incoterm.parse)
              CapacityWindows =
                req.CapacityWindows
                |> List.map mapCapacityWindowReq
              CreatedDate = Timestamp.create req.CreatedDate }

        make
        <!> (SupplierId.create req.SupplierId |> fromResult)
        <*> (SkuId.create req.SkuId |> fromResult)
        <*> (req.StockingPointId
             |> Option.map StockingPointId.create
             |> sequenceOpt
             |> fromResult)

    let toUpdateCommand (req: SupplierOfferUpdateReq) : Validation<UpdateSupplierOfferCmd, DomainError> =
        let make (offerId: SupplierOfferId) : UpdateSupplierOfferCmd =

            { Id = offerId
              Moq = req.Moq
              LotSize = req.LotSize
              LeadTimeP50 =
                req.LeadTimeP50Minutes
                |> Option.map (float >> TimeSpan.FromMinutes)
              LeadTimeP95 =
                req.LeadTimeP95Minutes
                |> Option.map (float >> TimeSpan.FromMinutes)
              PriceTiers =
                req.PriceTiers
                |> Option.map (List.map mapPriceTierReq)
              Reliability = req.Reliability
              Incoterm = req.Incoterm |> Option.map Incoterm.parse
              CapacityWindows =
                req.CapacityWindows
                |> Option.map (List.map mapCapacityWindowReq)
              ModifiedDate = Timestamp.create req.ModifiedDate }

        make
        <!> (SupplierOfferId.create req.Id |> fromResult)

    let toRevokeCommand (offerIdStr: string) : Result<RevokeSupplierOfferCmd, DomainError> =
        SupplierOfferId.create offerIdStr
        |> Result.map (fun id -> { Id = id; DeletedDate = Timestamp.now })

    let toChangeStatusCommand (req: SupplierOfferChangeStatusReq) : Result<ChangeSupplierOfferStatusCmd, DomainError> =
        SupplierOfferId.create req.Id
        |> Result.map (fun id ->
            { Id = id
              IsActive = req.IsActive
              ModifiedDate = Timestamp.create req.ModifiedDate })

    let toContractPriceTier (t: PriceTier) : Contracts.Domain.PriceTier =
        { TierNumber = t.TierNumber
          MinQuantity = Quantity.value t.MinQuantity
          MaxQuantity = t.MaxQuantity |> Option.map Quantity.value
          PricePerUnit = t.PricePerUnit
          Currency = t.Currency }

    let toContractCapacityWindow (w: SupplierCapacityWindow) : Contracts.Domain.SupplierCapacityWindow =
        { WindowId = w.WindowId
          StartDate = Timestamp.value w.StartDate
          EndDate = Timestamp.value w.EndDate
          MaxQuantity = Quantity.value w.MaxQuantity
          AvailableQuantity = Quantity.value w.AvailableQuantity }

    let toContract (offer: SupplierOffer) : Contracts.Domain.SupplierOffer =
        let incotermStr =
            offer.Incoterm
            |> Option.map (function
                | FOB -> "FOB"
                | CIF -> "CIF"
                | EXW -> "EXW"
                | DDP -> "DDP"
                | Other s -> s)

        { Id = SupplierOfferId.value offer.Id
          SupplierId = SupplierId.value offer.SupplierId
          SkuId = SkuId.value offer.SkuId
          StockingPointId =
            offer.StockingPointId
            |> Option.map StockingPointId.value
          Moq = offer.Moq
          LotSize = offer.LotSize |> Option.map Quantity.value
          LeadTimeP50Minutes =
            offer.LeadTimeP50
            |> Option.map (fun t -> t.TotalMinutes)
          LeadTimeP95Minutes =
            offer.LeadTimeP95
            |> Option.map (fun t -> t.TotalMinutes)
          PriceTiers = offer.PriceTiers |> List.map toContractPriceTier
          Reliability = offer.Reliability |> Option.map Percent.value
          Incoterm = incotermStr
          CapacityWindows =
            offer.CapacityWindows
            |> List.map toContractCapacityWindow
          IsActive = offer.IsActive }

type Decision = Decision<SupplierOffer, SupplierOfferEvent>

type SupplierOfferCapabilities =
    { Define: SupplierOfferDefineReq -> TaskResult<Decision, ApplicationError>
      Update: SupplierOfferUpdateReq -> TaskResult<Decision, ApplicationError>
      Revoke: string -> TaskResult<Decision, ApplicationError>
      ChangeStatus: SupplierOfferChangeStatusReq -> TaskResult<Decision, ApplicationError> }

let createCapabilities (repo: Repository<SupplierOffer, string, SupplierOfferEvent>) =
    { Define =
        liftCmdValidation ACL.toDefineCommand
        >=> handleCommand (fun cmd -> cmd.Id) repo DefineSupplierOffer decide

      Update =
        liftCmdValidation ACL.toUpdateCommand
        >=> handleCommand (fun cmd -> SupplierOfferId.value cmd.Id) repo UpdateSupplierOffer decide

      Revoke =
        liftCmdResult ACL.toRevokeCommand
        >=> handleCommand (fun cmd -> SupplierOfferId.value cmd.Id) repo RevokeSupplierOffer decide

      ChangeStatus =
        liftCmdResult ACL.toChangeStatusCommand
        >=> handleCommand (fun cmd -> SupplierOfferId.value cmd.Id) repo ChangeSupplierOfferStatus decide }

let evolveProjection (state: Map<string, Contracts.Domain.SupplierOffer>) (evt: SupplierOfferEvent) =
    match evt with
    | SupplierOfferDefined e -> Map.add (SupplierOfferId.value e.Id) (ACL.toContract (applyDefined e)) state
    | SupplierOfferUpdated e ->
        let key = SupplierOfferId.value e.Id

        match Map.tryFind key state with
        | Some existing ->
            let priceTiers =
                e.PriceTiers
                |> Option.map (List.map ACL.toContractPriceTier)

            let capacityWindows =
                e.CapacityWindows
                |> Option.map (List.map ACL.toContractCapacityWindow)

            let incotermStr =
                e.Incoterm
                |> Option.map (function
                    | FOB -> "FOB"
                    | CIF -> "CIF"
                    | EXW -> "EXW"
                    | DDP -> "DDP"
                    | Other s -> s)

            let updated =
                { existing with
                    Moq = e.Moq |> Option.orElse existing.Moq
                    LotSize =
                        e.LotSize
                        |> Option.map Quantity.value
                        |> Option.orElse existing.LotSize
                    LeadTimeP50Minutes =
                        e.LeadTimeP50
                        |> Option.map (fun t -> t.TotalMinutes)
                        |> Option.orElse existing.LeadTimeP50Minutes
                    LeadTimeP95Minutes =
                        e.LeadTimeP95
                        |> Option.map (fun t -> t.TotalMinutes)
                        |> Option.orElse existing.LeadTimeP95Minutes
                    PriceTiers =
                        priceTiers
                        |> Option.defaultValue existing.PriceTiers
                    Reliability =
                        e.Reliability
                        |> Option.map Percent.value
                        |> Option.orElse existing.Reliability
                    Incoterm = incotermStr |> Option.orElse existing.Incoterm
                    CapacityWindows =
                        capacityWindows
                        |> Option.defaultValue existing.CapacityWindows
                    IsActive = true }

            Map.add key updated state
        | None -> state
    | SupplierOfferRevoked e ->
        let key = SupplierOfferId.value e.Id

        match Map.tryFind key state with
        | Some existing -> Map.add key { existing with IsActive = false } state
        | None -> state
    | SupplierOfferStatusChanged e ->
        let key = SupplierOfferId.value e.Id

        match Map.tryFind key state with
        | Some existing -> Map.add key { existing with IsActive = e.IsActive } state
        | None -> state

let createProjectionAgent () =
    ProjectionAgent<Map<string, Contracts.Domain.SupplierOffer>, SupplierOfferEvent>(
        evolveProjection,
        Map.empty,
        "SupplierOfferReadModel"
    )

let createSupplierOfferApi (capabilities: SupplierOfferCapabilities) agent =
    { Define =
        fun req ->
            capabilities.Define req
            |> TaskResult.map (fun d -> d.NewState)
            |> TaskResult.map ACL.toContract
      DefineBulk =
        fun reqs ->
            reqs
            |> List.map capabilities.Define
            |> TaskResult.sequence
            |> TaskResult.map (fun decisions ->
                decisions
                |> List.map (fun d -> d.NewState)
                |> List.map ACL.toContract)
      Update =
        fun req ->
            capabilities.Update req
            |> TaskResult.map (fun d -> d.NewState)
            |> TaskResult.map ACL.toContract
      Revoke =
        fun reqId ->
            capabilities.Revoke reqId
            |> TaskResult.map (fun d -> d.NewState)
            |> TaskResult.map ACL.toContract
      ChangeStatus =
        fun req ->
            capabilities.ChangeStatus req
            |> TaskResult.map (fun d -> d.NewState)
            |> TaskResult.map ACL.toContract }
    : SupplierOfferApi

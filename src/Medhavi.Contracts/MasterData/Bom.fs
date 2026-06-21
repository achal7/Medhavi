namespace Medhavi.Contracts.MasterData.Bom

open System.Threading.Tasks
open Medhavi.Contracts

type BomItem =
    { ComponentSkuId: string
      Quantity: decimal
      Sequence: int }

type Bom =
    { Id: string
      SkuId: string
      Items: BomItem list
      Status: bool }

type BomItemReq =
    { ComponentSkuId: string
      Quantity: decimal
      UnitOfMeasureId: string
      Sequence: int }

type BomDefineReq =
    { Id: string
      SkuId: string
      Items: BomItemReq list }

type BomActivateReq = { Id: string }

type BomDeactivateReq = { Id: string }

type BomApi =
    { Define: BomDefineReq -> Task<Result<Bom, ApiError>>
      DefineBulk: BomDefineReq list -> Task<Result<Bom list, ApiError>>
      Activate: BomActivateReq -> Task<Result<Bom, ApiError>>
      Deactivate: BomDeactivateReq -> Task<Result<Bom, ApiError>> }

type BomQueryService = QueryService<Bom, string>

namespace Medhavi.Contracts.MasterData.Sku

open System
open System.Threading.Tasks
open Medhavi.Contracts

type Sku =
    { Id: string
      Code: string
      Name: string
      Group: string
      Status: bool }

type SkuDefineReq =
    { Id: string
      Code: string
      Name: string
      Group: string
      Created: DateTimeOffset }

type SkuRenameReq = { Id: string; NewName: string }

type SkuRetireReq = { Id: string }

type SkuApi =
    { Define: SkuDefineReq -> Task<Result<Sku, ApiError>>
      DefineBulk: SkuDefineReq list -> Task<Result<Sku list, ApiError>>
      Rename: SkuRenameReq -> Task<Result<Sku, ApiError>>
      Retire: SkuRetireReq -> Task<Result<Sku, ApiError>> }

type SkuQueryService = QueryService<Sku, string>

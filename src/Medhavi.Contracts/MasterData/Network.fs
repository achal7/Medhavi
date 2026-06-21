namespace Medhavi.Contracts.MasterData.Network

open System
open System.Threading.Tasks
open Medhavi.Contracts

type Plant =
    { Id: string
      Code: string
      Name: string
      Status: bool }

type PlantDefineReq =
    { Id: string
      Code: string
      Name: string }

type PlantRenameReq = { Id: string; NewName: string }

type PlantRetireReq = { Id: string }

type StockingPoint =
    { Id: string
      PlantId: string
      Code: string
      Name: string
      Type: string
      Status: bool }

type StockingPointDefineReq =
    { Id: string
      PlantId: string
      Code: string
      Name: string
      Type: string // Plant, DistributionCenter, Warehouse
      Location: string option
      Level: int option
      PlanningLevel: int option
      SupplyCanBeSplit: bool }

type StockingPointRenameReq = { Id: string; NewName: string }

type StockingPointRetireReq = { Id: string }

type NodeAttributesReq =
    { LocationCode: string option
      PlanningLevel: int option
      StockingPointRef: string option }

type NodeDefineReq =
    { Id: string
      Code: string
      Name: string
      Type: string // Plant, DistributionCenter, etc.
      Attributes: NodeAttributesReq
      Created: DateTimeOffset }

type NodeRetireReq = { Id: string }

type PlantApi =
    { Define: PlantDefineReq -> Task<Result<Plant, ApiError>>
      DefineBulk: PlantDefineReq list -> Task<Result<Plant list, ApiError>>
      Rename: PlantRenameReq -> Task<Result<Plant, ApiError>>
      Retire: PlantRetireReq -> Task<Result<Plant, ApiError>> }

type PlantQueryService = QueryService<Plant, string>

type StockingPointApi =
    { Define: StockingPointDefineReq -> Task<Result<StockingPoint, ApiError>>
      DefineBulk: StockingPointDefineReq list -> Task<Result<StockingPoint list, ApiError>>
      Rename: StockingPointRenameReq -> Task<Result<StockingPoint, ApiError>>
      Retire: StockingPointRetireReq -> Task<Result<StockingPoint, ApiError>> }

type StockingPointQueryService = QueryService<StockingPoint, string>

namespace Medhavi.Web.PromiseWorkbench

open System
open Medhavi.Web.WorkspaceEngine
open Medhavi.Web.Panels
open Medhavi.Nexus
open Medhavi.Contracts.Promise

type PromiseInput = {
    SkuId: string
    StockingPointId: string
    Quantity: decimal
    DueDate: DateTimeOffset
    CustomerTier: string
    SkuTier: string
    Currency: string
}

type Model =
    { WorkspaceId: WorkspaceId
      Context: WorkspaceContext
      Input: PromiseInput
      EvaluationResult: RemoteData<PromiseEvaluationResponse> }

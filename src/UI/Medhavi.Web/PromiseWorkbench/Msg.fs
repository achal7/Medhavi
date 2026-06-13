namespace Medhavi.Web.PromiseWorkbench

open System
open Medhavi.Nexus
open Medhavi.Contracts.Promise

type Msg =
    | UpdateSkuId of string
    | UpdateStockingPointId of string
    | UpdateQuantity of decimal
    | UpdateDueDate of DateTimeOffset
    | UpdateCustomerTier of string
    | UpdateSkuTier of string
    | UpdateCurrency of string
    | TriggerEvaluation
    | EvaluationCompleted of PromiseEvaluationResponse
    | EvaluationFailed of string
    | ResetInput

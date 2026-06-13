namespace Medhavi.Web.PromiseWorkbench

open System
open Elmish
open Medhavi.Web.WorkspaceEngine
open Medhavi.Web.Panels
open Medhavi.Web.Stores
open Medhavi.Contracts.Promise

module Update =

    let defaultInput () =
        { SkuId = "SKU-A"
          StockingPointId = "SP-1"
          Quantity = 10m
          DueDate = DateTimeOffset.Now.AddDays(7.0)
          CustomerTier = "silver"
          SkuTier = "tier-2"
          Currency = "USD" }

    let init (workspaceId: WorkspaceId) (context: WorkspaceContext) : Model * Cmd<Msg> =
        { WorkspaceId = workspaceId
          Context = context
          Input = defaultInput ()
          EvaluationResult = NotRequested },
        Cmd.none

    let update (promiseStore: PromiseStore) (msg: Msg) (model: Model) : Model * Cmd<Msg> =
        match msg with
        | UpdateSkuId sku ->
            { model with
                Input = { model.Input with SkuId = sku } },
            Cmd.none

        | UpdateStockingPointId sp ->
            { model with
                Input =
                    { model.Input with
                        StockingPointId = sp } },
            Cmd.none

        | UpdateQuantity qty ->
            { model with
                Input = { model.Input with Quantity = qty } },
            Cmd.none

        | UpdateDueDate due ->
            { model with
                Input = { model.Input with DueDate = due } },
            Cmd.none

        | UpdateCustomerTier tier ->
            { model with
                Input = { model.Input with CustomerTier = tier } },
            Cmd.none

        | UpdateSkuTier tier ->
            { model with
                Input = { model.Input with SkuTier = tier } },
            Cmd.none

        | UpdateCurrency curr ->
            { model with
                Input = { model.Input with Currency = curr } },
            Cmd.none

        | TriggerEvaluation ->
            let evalAsync () =
                async {
                    try
                        let skuId = model.Input.SkuId
                        let spId = model.Input.StockingPointId

                        let line =
                            { LineId = "sim-line-1"
                              SkuId = skuId
                              StockingPointId = spId
                              Quantity = model.Input.Quantity
                              DueDate = model.Input.DueDate
                              Priority = 1
                              IsExpedited = false
                              Origin = Some spId
                              Destination = Some spId }

                        let order =
                            { OrderId = "SIM-ORDER"
                              Lines = [ line ]
                              CustomerId = Some "SIM-CUSTOMER"
                              RequestDate = DateTimeOffset.Now }

                        let req: PromiseRequest =
                            { Order = order
                              AsOfDate = DateTimeOffset.Now
                              CustomerTier = Some model.Input.CustomerTier
                              SkuTier = Some model.Input.SkuTier
                              Currency = Some model.Input.Currency }

                        let! res =
                            promiseStore.EvaluatePromise req
                            |> Async.AwaitTask

                        return res
                    with ex ->
                        return Error ex.Message
                }

            let cmd =
                Cmd.OfAsync.either
                    evalAsync
                    ()
                    (function
                    | Ok resp -> EvaluationCompleted resp
                    | Error err -> EvaluationFailed err)
                    (fun ex -> EvaluationFailed ex.Message)

            { model with
                EvaluationResult = Loading },
            cmd

        | EvaluationCompleted resp ->
            { model with
                EvaluationResult = Loaded resp },
            Cmd.none

        | EvaluationFailed err ->
            { model with
                EvaluationResult = Failed err },
            Cmd.none

        | ResetInput ->
            { model with
                Input = defaultInput ()
                EvaluationResult = NotRequested },
            Cmd.none

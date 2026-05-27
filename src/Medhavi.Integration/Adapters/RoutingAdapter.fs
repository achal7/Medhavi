namespace Medhavi.Integration.Adapters

open System
open Medhavi.Contracts
open Medhavi.Contracts.Integration
open Medhavi.Integration

module RoutingAdapter =
    let parse (routingCsv: string) (resourceCsv: string) (bomLines: BomLineImportedPayload list) : Result<RoutingDefineReq list, string> =
        try
            let routings = InboundAdapter.parseRoutingCsv routingCsv |> Result.defaultWith (fun e -> failwith e)
            let resources = InboundAdapter.parseResourceCsv resourceCsv |> Result.defaultWith (fun e -> failwith e)

            routings
            |> List.map (fun r ->
                let steps = 
                    r.Steps 
                    |> List.map (fun s -> 
                        { StepId = $"STEP-{r.SkuId}-{s.Sequence}"
                          Sequence = s.Sequence
                          ResourceGroupId = Some s.ResourceId
                          Yield = None })

                let stepResources =
                    r.Steps
                    |> List.map (fun s ->
                        { StepId = $"STEP-{r.SkuId}-{s.Sequence}"
                          ResourceId = s.ResourceId
                          IsAllowed = true
                          Sequence = s.Sequence
                          DurationPerUnitMinutes = Some (decimal (s.RunHoursPerUnit * 60.0)) })

                let firstStepOpt = r.Steps |> List.sortBy (fun s -> s.Sequence) |> List.tryHead
                let lastStepOpt = r.Steps |> List.sortBy (fun s -> s.Sequence) |> List.tryLast

                let firstStepId = 
                    match firstStepOpt with
                    | Some s -> $"STEP-{r.SkuId}-{s.Sequence}"
                    | None -> $"STEP-{r.SkuId}-10"
                let lastStepId = 
                    match lastStepOpt with
                    | Some s -> $"STEP-{r.SkuId}-{s.Sequence}"
                    | None -> $"STEP-{r.SkuId}-10"

                let getStepNodeId (stepOpt: RoutingStepImportedPayload option) =
                    match stepOpt with
                    | None -> "SP-FACTORY"
                    | Some s ->
                        resources 
                        |> List.tryFind (fun res -> res.ResourceId = s.ResourceId)
                        |> Option.map (fun res -> res.NodeId)
                        |> Option.defaultValue "SP-FACTORY"

                let firstStepNodeId = getStepNodeId firstStepOpt
                let lastStepNodeId = getStepNodeId lastStepOpt

                let relatedBoms = bomLines |> List.filter (fun b -> b.ParentSkuId = r.SkuId)

                let inputs =
                    if List.isEmpty relatedBoms then
                        [ { StepId = firstStepId
                            SkuId = "SKU-FRAME"
                            NodeId = firstStepNodeId
                            ConversionRate = Some 1.0m } ]
                    else
                        relatedBoms |> List.map (fun b ->
                            { StepId = firstStepId
                              SkuId = b.ComponentSkuId
                              NodeId = firstStepNodeId
                              ConversionRate = Some b.QuantityRequired })

                let outputs =
                    [ { StepId = lastStepId
                        SkuId = r.SkuId
                        NodeId = lastStepNodeId
                        ConversionRate = Some 1.0m
                        IsCoSku = false } ]

                { Id = $"ROUTING-{r.SkuId}"
                  Name = $"Routing for {r.SkuId}"
                  Type = "Work"
                  EffectiveStart = DateTimeOffset.UtcNow
                  EffectiveEnd = None
                  Steps = steps
                  Inputs = inputs
                  Outputs = outputs
                  StepResources = stepResources
                  Created = DateTimeOffset.UtcNow })
            |> Ok
        with ex ->
            Error ex.Message

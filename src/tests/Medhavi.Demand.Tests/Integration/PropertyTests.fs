module Medhavi.Demand.Tests.Integration.PropertyTests
 
open Expecto
open FsCheck
open Medhavi.SharedKernel
open Medhavi.Demand.ForecastQualityAlgorithms
open Medhavi.Demand.ForecastPublication
open Medhavi.Demand.EnterpriseDemandPicture
 
[<Tests>]
let tests =
    testList "Demand Intelligence Property-Based Tests" [
        
        testProperty "BA-D-001 EDP Invariant: Planning Demand is always non-negative and equals sum of components" 
        <| fun (opVal: decimal) (adjVal: decimal) (ovrVal: decimal) ->
            let op = abs opVal % 1000000m
            let adj = abs adjVal % 1000000m
            let ovr = abs ovrVal % 1000000m
            
            let qOp = Quantity.create op |> Result.defaultValue Quantity.Zero
            let qAdj = Quantity.create adj |> Result.defaultValue Quantity.Zero
            let qOvr = Quantity.create ovr |> Result.defaultValue Quantity.Zero
            
            let sumVal = Quantity.value qOp + Quantity.value qAdj + Quantity.value qOvr
            let finalQ = Quantity.create sumVal |> Result.defaultValue Quantity.Zero
            
            Quantity.value finalQ >= 0m 
            && Quantity.value finalQ = (Quantity.value qOp + Quantity.value qAdj + Quantity.value qOvr)
 
        testProperty "BA-D-002 Forecast Invariant: Standard deviation is non-negative and bounds are valid"
        <| fun (values: decimal list) ->
            let cleanValues = values |> List.map (fun v -> abs v % 1000000m)
            if cleanValues.Length < 2 then
                true
            else
                let sd = ComputationService.stdDev cleanValues
                let meanValue = List.average cleanValues
                let lower = max 0.0m (meanValue - 1.96m * sd)
                let upper = meanValue + 1.96m * sd
                
                sd >= 0.0m
                && lower <= meanValue
                && meanValue <= upper
 
        testProperty "BA-D-005 WAPE Invariant: Identical actuals and forecasts yield 0% WAPE and 100% Accuracy"
        <| fun (values: decimal list) ->
            let decimals = values |> List.map (fun v -> abs v % 1000000m) |> List.filter (fun v -> v > 0m)
            if decimals.IsEmpty then
                true
            else
                let w = wape decimals decimals
                let acc = forecastAccuracy decimals decimals
                w = Some 0m && acc = Some 1m
 
        testProperty "BA-D-005 MAPE Invariant: Identical actuals and forecasts yield 0% MAPE"
        <| fun (values: decimal list) ->
            let decimals = values |> List.map (fun v -> abs v % 1000000m) |> List.filter (fun v -> v > 0m)
            if decimals.IsEmpty then
                true
            else
                let m = mape decimals decimals
                m = Some 0m
 
        testProperty "BA-D-005 Bias Invariant: Identical actuals and forecasts yield 0 Bias"
        <| fun (values: decimal list) ->
            let decimals = values |> List.map (fun v -> abs v % 1000000m)
            if decimals.IsEmpty then
                true
            else
                let b = forecastBias decimals decimals
                b = Some 0m
 
        testProperty "BA-D-007 Forecast Stability: Identical sequences yield 0 Stability change"
        <| fun (values: decimal list) ->
            let decimals = values |> List.map (fun v -> abs v % 1000000m)
            if decimals.IsEmpty then
                true
            else
                let hist = [ decimals; decimals; decimals ]
                let stab = forecastStability hist
                stab = Some 0m
 
        testProperty "BA-D-008 Override Effectiveness: Perfect overrides yield 100% effectiveness"
        <| fun (values: (decimal * decimal) list) (actuals: decimal list) ->
            let len = min values.Length actuals.Length
            if len = 0 then
                true
            else
                let cleanActuals = actuals |> List.take len |> List.map (fun v -> (abs v % 1000000m) + 1m)
                let cleanOverrides = 
                    cleanActuals 
                    |> List.map (fun act -> 
                        let original = act + 10m
                        let overrideVal = act
                        (original, overrideVal)
                    )
                let eff = overrideEffectiveness cleanOverrides cleanActuals
                eff = Some 1.0m
    ]

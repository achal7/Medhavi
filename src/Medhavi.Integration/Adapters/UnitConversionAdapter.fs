namespace Medhavi.Integration.Adapters

open System
open Medhavi.Contracts.Integration
open Medhavi.Integration

module UnitConversionAdapter =
    let parse (csvText: string) : Result<UnitConversionDefineReq list, string> =
        try
            if String.IsNullOrWhiteSpace(csvText) then
                [ { SourceUom = "UOM-BOX"
                    TargetUom = "UOM-PCS"
                    ConversionFactor = 10.0m
                    Created = DateTimeOffset.UtcNow } ]
                |> Ok
            else
                let conversions = InboundAdapter.parseUnitConversionCsv csvText |> Result.defaultWith (fun e -> failwith e)
                if List.isEmpty conversions then
                    [ { SourceUom = "UOM-BOX"
                        TargetUom = "UOM-PCS"
                        ConversionFactor = 10.0m
                        Created = DateTimeOffset.UtcNow } ]
                        |> Ok
                else
                    Ok conversions
        with ex ->
            Error ex.Message

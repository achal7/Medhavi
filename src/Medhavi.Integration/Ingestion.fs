namespace Medhavi.Integration

open System
open Medhavi.SharedKernel
open Medhavi.Contracts

/// Anti-Corruption Layer (ACL) normalizer to transform external systems data
module InboundAdapter =

    /// Normalizes raw ERP unit conversion payloads to unit conversion DTO
    let normalizeUnitConversion (rawId: string) (rawProd: string) (rawFrom: string) (rawTo: string) (rawRatio: float) =
        let cleanProdId = 
            if String.IsNullOrWhiteSpace(rawProd) then None 
            else Some (rawProd.Trim())
        {
            Id = rawId.Trim()
            ProductId = cleanProdId
            FromUnitCode = rawFrom.Trim().ToUpper()
            ToUnitCode = rawTo.Trim().ToUpper()
            Ratio = rawRatio
            IsActive = true
        }

namespace Medhavi.Integration.Adapters

open System
open Medhavi.Contracts.Integration
open Medhavi.Integration

module TransportLegAdapter =
    let parse (csvText: string) : Result<TransportLegDefineReq list, string> =
        InboundAdapter.parseTransportLegCsv csvText

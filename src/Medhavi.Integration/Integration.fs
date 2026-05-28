namespace Medhavi.Integration

open System
open Medhavi.Infrastructure
open Medhavi.Contracts
open Medhavi.Common.Serialization
open Medhavi.Contracts.Integration

type IntegrationSuccess =
    { EnvelopeId: EventId
      CorrelationId: CorrelationId option }

type IntegrationError =
    | ValidationError of string list
    | IngestionError of string

/// Discriminated Union representing all Integration Events.
type IntegrationEvent =
    | UomImported of UomDefineReq list
    | UnitConversionsImported of UnitConversionDefineReq list
    | TransportLegsImported of TransportLegDefineReq list
    | RoutingsImported of RoutingDefineReq list
    | BomImported of BomDefineReq list
    | SkusImported of SkuDefineReq list
    | StockingPointsImported of StockingPointDefineReq list
    | PlantsImported of PlantDefineReq list
    | ResourcesImported of ResourceImportedPayload list
    | SupplyOffersImported of SupplierOfferDefineReq list
    | DemandSignalsImported of DemandSignalsPayload
    | InventoryPositionsImported of InventoryDefineReq list
    | InventoryTargetsImported of InventoryTargetDefineReq list
    | SupplyOrdersImported of SupplyOrderStatusPayload list
    | ResourceCalendarsImported of ResourceCalendarPayload list
    | WorkOrdersCompleted of WorkOrderCompletedPayload list
    | MaterialsReceived of MaterialReceivedPayload list
    | ResourceDowntimes of ResourceDowntimePayload list
    | TransportDelays of TransportDelayPayload list

[<RequireQualifiedAccess>]
module IntegrationEventEnvelope =

    /// Helper to create a generic Envelope containing a serialized IntegrationEvent.
    let create
        (tenantId: string)
        (correlationId: Guid)
        (payload: IntegrationEvent)
        : Result<Envelope, SerializationError> =
        serialize payload
        |> Result.map (fun dataJson ->
            Envelope.createEnvelope "IntegrationEvent" dataJson 1
            |> Envelope.withTenantId tenantId
            |> Envelope.withCorrelationId (correlationId.ToString()))

    /// Helper to parse the typed IntegrationEvent out of a generic Envelope.
    let tryGetPayload (env: Envelope) : Result<IntegrationEvent, SerializationError> =
        Envelope.deserialize<IntegrationEvent> env


module CsvHelper =
    type CsvRow =
        { Headers: string[]
          Values: string[] }

        member this.Get(columnName: string) : string option =
            let idx =
                Array.tryFindIndex
                    (fun (h: string) ->
                        h
                            .Trim()
                            .Equals(columnName, StringComparison.OrdinalIgnoreCase))
                    this.Headers

            match idx with
            | Some i when i < this.Values.Length ->
                let v = this.Values.[i].Trim()
                if String.IsNullOrEmpty(v) then None else Some v
            | _ -> None

        member this.GetDecimal(columnName: string) : decimal option =
            this.Get columnName
            |> Option.bind (fun v ->
                match System.Decimal.TryParse(v) with
                | true, d -> Some d
                | _ -> None)

        member this.GetFloat(columnName: string) : float option =
            this.Get columnName
            |> Option.bind (fun v ->
                match System.Double.TryParse(v) with
                | true, f -> Some f
                | _ -> None)

        member this.GetInt(columnName: string) : int option =
            this.Get columnName
            |> Option.bind (fun v ->
                match System.Int32.TryParse(v) with
                | true, i -> Some i
                | _ -> None)

        member this.GetBool(columnName: string) : bool option =
            this.Get columnName
            |> Option.bind (fun v ->
                match System.Boolean.TryParse(v) with
                | true, b -> Some b
                | _ ->
                    match v.ToLowerInvariant() with
                    | "1"
                    | "yes"
                    | "true" -> Some true
                    | "0"
                    | "no"
                    | "false" -> Some false
                    | _ -> None)

        member this.GetDateTimeOffset(columnName: string) : DateTimeOffset option =
            this.Get columnName
            |> Option.bind (fun v ->
                match System.DateTimeOffset.TryParse(v) with
                | true, dto -> Some dto
                | _ -> None)

    let splitCsvLine (line: string) : string[] =
        let matches =
            System.Text.RegularExpressions.Regex.Matches(line, "(?<=^|,)(\"(?:[^\"]|\"\")*\"|[^,]*)")

        [| for m in matches -> m.Value.Trim().Trim('"').Replace("\"\"", "\"") |]

    let parseCsv (csvText: string) : CsvRow[] =
        let rawLines =
            csvText.Split([| '\r'; '\n' |], StringSplitOptions.RemoveEmptyEntries)

        if rawLines.Length <= 1 then
            [||]
        else
            let headers =
                rawLines.[0].Split([| ',' |])
                |> Array.map (fun s -> s.Trim().Trim('"'))

            rawLines.[1..]
            |> Array.filter (fun line -> not (String.IsNullOrWhiteSpace(line)))
            |> Array.map (fun line ->
                { Headers = headers
                  Values = splitCsvLine line })
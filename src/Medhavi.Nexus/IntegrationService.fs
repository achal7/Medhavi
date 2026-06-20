module Medhavi.Nexus.IntegrationService

open System.Threading.Tasks
open Medhavi.Common

open Medhavi.Infrastructure.Stores.EnvelopeStore
open Medhavi.Infrastructure.Stores.EnvelopeStoreMem
open Medhavi.Integration

type ExtractEnvelope = EnvelopedEvent -> Result<IntegrationEvent,Serialization.SerializationError>

let extractEnvelope : ExtractEnvelope =
    fun (envelopedEvent: EnvelopedEvent) ->
        let envelope = envelopedEvent.Envelope
        IntegrationEventEnvelope.tryGetPayload envelope


let seedInitialData(integration: IntegrationCapabilities) =
    task{
        let! bootstrapRes = integration.IngestAndPublishMasterData()
        match bootstrapRes with
        | Error err -> printfn $"[ ERR ] Failed to bootstrap master data from CSV: %A{err}"
        | Ok _ -> printfn "[ OK ] System successfully bootstrapped with CSV master data."
    }

let create(envelopeStore) =
    IntegrationService.createCapabilities envelopeStore

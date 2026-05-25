module Medhavi.SharedKernel.IdsFactory

open System
open System.Security.Cryptography
open System.Text
open Medhavi.SharedKernel

/// External system identifier (e.g., from ERP, MES, etc.)
type ExternalSystemId =
    { SystemName: string // e.g., "ERP", "MES", "SAP"
      ExternalId: string } // The ID from the external system

/// ID generation strategy
type IdGenerationStrategy =
    | Deterministic of ExternalSystemId // Use external system ID to generate deterministic internal ID
    | Random // Generate random ID (for internal-only entities)
    | Explicit of string // Use explicitly provided ID

/// Deterministic ID helpers for idempotency (pure, side-effect free).
module DeterministicIds =
    let private hashParts (parts: string list) =
        use sha = SHA256.Create()
        let payload = String.concat "|" parts |> Encoding.UTF8.GetBytes
        let bytes = sha.ComputeHash payload

        bytes
        |> Array.take 12
        |> Array.fold (fun acc b -> acc + b.ToString("x2")) ""

    /// Build a deterministic ID for reservations (material/capacity/transport) given a natural key.
    let reservationId (scope: string) (reference: string) (windowStart: DateTimeOffset) (windowEnd: DateTimeOffset) =
        hashParts [ "resv"; scope; reference; windowStart.ToString("O"); windowEnd.ToString("O") ]

    /// Deterministic itinerary/transport option ID (origin/destination + legs signature).
    let itineraryId (origin: string) (destination: string) (legsSignature: string) =
        hashParts [ "itin"; origin; destination; legsSignature ]

    /// Deterministic capacity allocation/assignment ID.
    let allocationId (resourceRef: string) (windowStart: DateTimeOffset) (windowEnd: DateTimeOffset) =
        hashParts [ "alloc"; resourceRef; windowStart.ToString("O"); windowEnd.ToString("O") ]

    /// Deterministic peg (demand↔supply) ID.
    let pegId (demandId: string) (supplyId: string) = hashParts [ "peg"; demandId; supplyId ]

    /// Deterministic proposal/recommendation ID (e.g., supply order proposals).
    let proposalId (proposalType: string) (anchorId: string) (timestamp: DateTimeOffset) =
        hashParts [ "proposal"; proposalType; anchorId; timestamp.ToString("O") ]

/// Common ID factory for creating deterministic IDs from external system identifiers
/// This ensures idempotent ingestion: same external ID always produces same internal ID
/// Normalize external system name (lowercase, trimmed)
let private normalizeSystemName (name: string) : string = name.Trim().ToLowerInvariant().Replace(" ", "_")

/// Normalize external ID (trimmed, but preserve case for uniqueness)
let private normalizeExternalId (id: string) : string = id.Trim()

/// Create a deterministic internal ID from external system identifier
/// Uses SHA256 hash to ensure same external ID always produces same internal ID
/// Format: "{systemName}:{hash}" where hash is first 16 chars of SHA256
let createDeterministicId (externalSystemId: ExternalSystemId) (aggregateType: string) : string =
    let normalizedSystem = normalizeSystemName externalSystemId.SystemName
    let normalizedExternalId = normalizeExternalId externalSystemId.ExternalId
    let combined = $"{normalizedSystem}:{aggregateType}:{normalizedExternalId}"

    // Use SHA256 to create deterministic hash
    use sha256 = SHA256.Create()
    let hashBytes = sha256.ComputeHash(Encoding.UTF8.GetBytes(combined))

    let hashString =
        BitConverter
            .ToString(hashBytes)
            .Replace("-", "")
            .ToLowerInvariant()

    // Use first 16 characters of hash + last 4 of original ID for readability
    let shortHash = hashString.Substring(0, 16)

    let idSuffix =
        if normalizedExternalId.Length >= 4 then
            normalizedExternalId.Substring(Math.Max(0, normalizedExternalId.Length - 4))
        else
            normalizedExternalId

    $"{normalizedSystem}-{aggregateType}-{shortHash}-{idSuffix}"

/// Create deterministic ProductId from external system identifier
let generateId (strategy: IdGenerationStrategy) (name: string) : Result<string, DomainError> =
    match strategy with
    | Deterministic extId -> Ok(createDeterministicId extId name)
    | Random -> Ok(Guid.NewGuid().ToString("N"))
    | Explicit id ->
        if String.IsNullOrWhiteSpace id then
            Error(DomainError.validation ($"{name} cannot be empty"))
        else
            Ok(id)

let createId (strategy: IdGenerationStrategy) (ctor: string -> 'id) (name: string) =
    generateId strategy name
    |> Result.bind (fun id -> Ok(ctor id))

let createExplicitId (ctor: string -> 'id) (name: string) (value: string) = createId (Explicit value) ctor name

namespace Medhavi.Scheduler.Planning.Domain

open System
open System.Security.Cryptography
open System.Text
open System.Text.Json
open Medhavi.SharedKernel
open Medhavi.SharedKernel.ScenarioContracts

/// A deterministic, content-addressed identity for a planning run.
/// Two runs with identical InputFingerprints will always produce identical output.
/// This guarantees idempotent run creation and deterministic plan replay.
type InputFingerprint =
    {
        /// Version of the scenario configuration at time of run.
        ScenarioVersion: Version
        /// SHA-256 of the full planning horizon specification.
        HorizonHash: string
        /// Version of the demand dataset consumed.
        DemandVersion: Version
        /// Version of the inventory snapshot consumed.
        InventoryVersion: Version
        /// Version of the capacity data consumed.
        CapacityVersion: Version
        /// Version of the BOM dataset consumed.
        BomVersion: Version
        /// Version of the routing dataset consumed.
        RoutingVersion: Version
        /// Version of the planning policy (objectives, constraints).
        PolicyVersion: Version
        /// Semantic version of the solver engine (e.g. "2.1.0").
        SolverVersion: string
        /// The planning mode for this run.
        Mode: PlanningMode
        /// Wall-clock instant at which the fingerprint was computed.
        /// Part of the fingerprint so archived plans can be replayed exactly.
        AsOf: DateTimeOffset
    }

module InputFingerprint =

    let private canonicalJson (fp: InputFingerprint) : string =
        // Produce canonical JSON with sorted keys for determinism.
        JsonSerializer.Serialize(fp, JsonSerializerOptions(WriteIndented = false))

    /// Compute the SHA-256 hash of the fingerprint and return it as a hex string.
    let computeHash (fp: InputFingerprint) : string =
        let json = canonicalJson fp
        let bytes = Encoding.UTF8.GetBytes(json)
        use sha = SHA256.Create()
        let hashBytes = sha.ComputeHash(bytes)
        Convert.ToHexString(hashBytes).ToLowerInvariant()

    /// Derive the PlanVersionId for this fingerprint (content-addressed identifier).
    let toPlanVersionId (fp: InputFingerprint) : PlanVersionId = PlanVersionId.create (computeHash fp)

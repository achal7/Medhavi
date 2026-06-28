namespace Medhavi.DecisionCore

open System
open System.Security.Cryptography
open System.Text

type SnapshotFingerprint = SnapshotFingerprint of string
type PolicyFingerprint = PolicyFingerprint of string
type PlanFingerprint = PlanFingerprint of string
type GraphFingerprint = GraphFingerprint of string

module Fingerprint =

    let private sha256 (input: string) =
        use sha = SHA256.Create()
        let bytes = Encoding.UTF8.GetBytes input
        let hash = sha.ComputeHash bytes
        Convert.ToHexStringLower hash

    let ofSnapshot (json: string) = json |> sha256 |> SnapshotFingerprint
    let ofPolicy (json: string)   = json |> sha256 |> PolicyFingerprint
    let ofPlan (json: string)     = json |> sha256 |> PlanFingerprint
    let ofGraph (json: string)    = json |> sha256 |> GraphFingerprint

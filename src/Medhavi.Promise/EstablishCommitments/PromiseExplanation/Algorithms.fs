/// CA-P-006 Establish Commitments — Promise Explanation Cryptographic Algorithms
module Medhavi.Promise.EstablishCommitments.PromiseExplanation.Algorithms

open System
open System.Security.Cryptography
open System.Text
open Model

let computeLineageDigest (nodes: LineageNode list) : string =
    let payload =
        nodes
        |> List.sortBy (fun n -> n.StepIndex)
        |> List.map (fun n -> $"{n.StepIndex}:{n.StageName}:{n.EntityReference}:{n.ActionTaken}:{n.EvidenceSummary}")
        |> String.concat "|"

    use sha = SHA256.Create()
    let bytes = Encoding.UTF8.GetBytes(payload)
    let hash = sha.ComputeHash(bytes)
    BitConverter.ToString(hash).Replace("-", "").ToLowerInvariant()


module Medhavi.Domain.Tests.TransportTests

open System
open Expecto
open Medhavi.Transport
open Medhavi.Transport.Domain.TransportGraphAgg
open Medhavi.Transport.Domain.TransportReservationAgg

// ─── Helpers ─────────────────────────────────────────────────────────────────

let makeLeg legId origin dest mode leadTime =
    { LegId              = legId
      Origin             = origin
      Destination        = dest
      Mode               = mode
      LeadTimeMinutes    = leadTime
      Capacity           = Some 500.0m
      CapacityUnit       = Some "PCS"
      Reliability        = Some 0.98m
      CO2PerUnit         = Some 0.02m
      FixedCost          = 100.0m
      VariableCostPerUnit = Some 0.5m
      Status             = true }

/// Build test network:
/// FACTORY -> WAREHOUSE (120 min, Road)
/// WAREHOUSE -> DIST (180 min, Road)
/// DIST -> CUSTOMER (60 min, Road)
/// FACTORY -> DIST (240 min, Rail) -- faster but rail
/// WAREHOUSE -> CUSTOMER (90 min, Road) -- bypass dist
let buildTestGraph () =
    let legs =
        [ makeLeg "L1" "FACTORY" "WAREHOUSE" "Road" 120.0m
          makeLeg "L2" "WAREHOUSE" "DIST" "Road" 180.0m
          makeLeg "L3" "DIST" "CUSTOMER" "Road" 60.0m
          makeLeg "L4" "FACTORY" "DIST" "Rail" 240.0m
          makeLeg "L5" "WAREHOUSE" "CUSTOMER" "Road" 90.0m ]
    buildGraph legs

// ─── Graph Tests ─────────────────────────────────────────────────────────────

[<Tests>]
let graphTests =
    testList "TransportGraph" [

        test "buildGraph creates all nodes" {
            let graph = buildTestGraph ()
            Expect.contains graph.Nodes "FACTORY" "FACTORY should be a node"
            Expect.contains graph.Nodes "WAREHOUSE" "WAREHOUSE should be a node"
            Expect.contains graph.Nodes "DIST" "DIST should be a node"
            Expect.contains graph.Nodes "CUSTOMER" "CUSTOMER should be a node"
        }

        test "outgoingLegs returns correct legs from FACTORY" {
            let graph = buildTestGraph ()
            let outgoing = outgoingLegs graph "FACTORY"
            Expect.equal outgoing.Length 2 "FACTORY has 2 outgoing legs"
            let dests = outgoing |> List.map (fun l -> l.Destination) |> Set.ofList
            Expect.contains dests "WAREHOUSE" "One goes to WAREHOUSE"
            Expect.contains dests "DIST" "One goes to DIST"
        }

        test "kShortestPaths finds direct 1-hop route WAREHOUSE -> CUSTOMER" {
            let graph = buildTestGraph ()
            let paths = kShortestPaths graph "WAREHOUSE" "CUSTOMER" 3 4
            Expect.isNonEmpty paths "Should find at least one path"
            let shortest = paths |> List.head
            Expect.equal shortest.Legs.Length 1 "Shortest path should be direct (1 hop)"
            Expect.equal shortest.Legs.[0].LegId "L5" "Should use leg L5 (direct)"
        }

        test "kShortestPaths finds 2-hop route WAREHOUSE -> DIST -> CUSTOMER" {
            let graph = buildTestGraph ()
            let paths = kShortestPaths graph "WAREHOUSE" "CUSTOMER" 3 4
            Expect.isTrue (paths.Length >= 2) "Should find at least 2 paths"
            let twoHopPath = paths |> List.tryFind (fun p -> p.Legs.Length = 2)
            Expect.isSome twoHopPath "Should find a 2-hop path via DIST"
        }

        test "kShortestPaths finds route FACTORY -> CUSTOMER (multi-hop)" {
            let graph = buildTestGraph ()
            let paths = kShortestPaths graph "FACTORY" "CUSTOMER" 5 4
            Expect.isNonEmpty paths "Should find at least one path"
            // Shortest path: FACTORY -> WAREHOUSE -> CUSTOMER (120+90=210)
            let shortest = paths |> List.head
            let totalLeadTime = Path.totalLeadTimeMinutes shortest
            Expect.isTrue (totalLeadTime > 0.0m) "Lead time should be positive"
        }

        test "kShortestPaths returns empty for nonexistent route" {
            let graph = buildTestGraph ()
            let paths = kShortestPaths graph "CUSTOMER" "FACTORY" 5 4
            Expect.isEmpty paths "Should return empty list for reverse (no return legs)"
        }

        test "kShortestPaths respects maxHops constraint" {
            let graph = buildTestGraph ()
            // With maxHops=1, only 1-hop paths are allowed
            let paths = kShortestPaths graph "FACTORY" "CUSTOMER" 5 1
            // There's no direct 1-hop FACTORY -> CUSTOMER leg
            Expect.isEmpty paths "Should return empty when maxHops=1 and no direct route"
        }

        test "Path.toItinerary builds correct hop offsets" {
            let graph = buildTestGraph ()
            let paths = kShortestPaths graph "FACTORY" "CUSTOMER" 1 4
            Expect.isNonEmpty paths "Should find a path"
            let path = paths |> List.head
            let id = ItineraryId.generate ()
            let itin = Path.toItinerary id (Some "SKU-001") (Some 10.0m) path
            Expect.equal itin.FromNode "FACTORY" "From should be FACTORY"
            Expect.equal itin.ToNode "CUSTOMER" "To should be CUSTOMER"
            Expect.isTrue (itin.TotalLeadTimeMinutes > 0.0m) "Total lead time positive"
            Expect.isTrue (itin.HopCount > 0) "Hop count positive"
        }

        test "Path.totalReliability is product of all hop reliabilities" {
            let legs =
                [ makeLeg "L1" "A" "B" "Road" 60.0m
                  makeLeg "L2" "B" "C" "Road" 60.0m ]
            let path = { Legs = legs }
            let reliability = Path.totalReliability path
            // 0.98 * 0.98 = 0.9604
            Expect.isTrue (reliability < 0.97m) "Combined reliability should be less than single leg"
            Expect.isTrue (reliability > 0.95m) "Combined reliability should be > 0.95"
        }
    ]

// ─── TransportReservation Aggregate Tests ─────────────────────────────────────

let makeResId () = TransportReservationId.generate()
let makeItinId () = ItineraryId.generate()

let makeCreateCmd () : CreateTransportReservationCmd =
    { Id              = makeResId()
      IdempotencyKey  = Guid.NewGuid().ToString()
      ItineraryId     = makeItinId()
      SkuId           = "SKU-FRAME"
      FromNode        = "FACTORY"
      ToNode          = "CUSTOMER"
      Quantity        = 50.0m
      EarliestDeparture = DateTimeOffset.UtcNow.AddHours(1.0)
      EarliestArrival   = DateTimeOffset.UtcNow.AddHours(3.0)
      ExpiryTime      = DateTimeOffset.UtcNow.AddHours(2.0) }

[<Tests>]
let reservationTests =
    testList "TransportReservation" [

        test "Create reservation from None succeeds" {
            let cmd = makeCreateCmd ()
            let result = Reservation.decide (CreateTransportReservation cmd) None
            Expect.isOk result "Should succeed"
            match result with
            | Error _ -> failtest "Expected Ok"
            | Ok decision ->
                Expect.equal decision.NewState.Status Tentative "Should be Tentative"
                Expect.equal decision.NewState.FromNode "FACTORY" "From should match"
                Expect.equal decision.NewState.ToNode "CUSTOMER" "To should match"
        }

        test "Create reservation from Some state fails (already exists)" {
            let cmd = makeCreateCmd ()
            match Reservation.decide (CreateTransportReservation cmd) None with
            | Error _ -> failtest "Should have succeeded"
            | Ok decision ->
                let result = Reservation.decide (CreateTransportReservation cmd) (Some decision.NewState)
                Expect.isError result "Should fail when already exists"
        }

        test "Create reservation with zero quantity fails" {
            let cmd = { makeCreateCmd () with Quantity = 0.0m }
            let result = Reservation.decide (CreateTransportReservation cmd) None
            Expect.isError result "Should fail with zero quantity"
        }

        test "Create reservation with past expiry fails" {
            let cmd = { makeCreateCmd () with ExpiryTime = DateTimeOffset.UtcNow.AddHours(-1.0) }
            let result = Reservation.decide (CreateTransportReservation cmd) None
            Expect.isError result "Should fail with past expiry"
        }

        test "Confirm Tentative reservation succeeds" {
            let createCmd = makeCreateCmd ()
            match Reservation.decide (CreateTransportReservation createCmd) None with
            | Error _ -> failtest "Create should succeed"
            | Ok createDecision ->
                let state = createDecision.NewState
                let confirmCmd : ConfirmTransportReservationCmd = { Id = createCmd.Id }
                let result = Reservation.decide (ConfirmTransportReservation confirmCmd) (Some state)
                Expect.isOk result "Confirm should succeed"
                match result with
                | Error _ -> failtest "Expected Ok for confirm"
                | Ok decision ->
                    Expect.equal decision.NewState.Status Confirmed "Should be Confirmed"
        }

        test "Release Tentative reservation succeeds" {
            let createCmd = makeCreateCmd ()
            match Reservation.decide (CreateTransportReservation createCmd) None with
            | Error _ -> failtest "Create should succeed"
            | Ok createDecision ->
                let state = createDecision.NewState
                let releaseCmd : ReleaseTransportReservationCmd =
                    { Id = createCmd.Id; ReleasedAt = DateTimeOffset.UtcNow }
                let result = Reservation.decide (ReleaseTransportReservation releaseCmd) (Some state)
                Expect.isOk result "Release should succeed"
                match result with
                | Error _ -> failtest "Expected Ok for release"
                | Ok decision ->
                    Expect.equal decision.NewState.Status Released "Should be Released"
        }

        test "Release already Released reservation fails" {
            let createCmd = makeCreateCmd ()
            match Reservation.decide (CreateTransportReservation createCmd) None with
            | Error _ -> failtest "Create should succeed"
            | Ok createDecision ->
                let state = createDecision.NewState
                let releaseCmd : ReleaseTransportReservationCmd =
                    { Id = createCmd.Id; ReleasedAt = DateTimeOffset.UtcNow }
                match Reservation.decide (ReleaseTransportReservation releaseCmd) (Some state) with
                | Error _ -> failtest "First release should succeed"
                | Ok released ->
                    let result = Reservation.decide (ReleaseTransportReservation releaseCmd) (Some released.NewState)
                    Expect.isError result "Double release should fail"
        }

        test "Evolve Created event returns Tentative state" {
            let createCmd = makeCreateCmd ()
            match Reservation.decide (CreateTransportReservation createCmd) None with
            | Error _ -> failtest "Create should succeed"
            | Ok decision ->
                let evt = decision.Events |> List.head
                let state = Reservation.evolve evt None
                Expect.isSome state "Should return Some state"
                Expect.equal state.Value.Status Tentative "Should be Tentative after Create event"
        }
    ]

namespace Medhavi.Nexus

open Medhavi.MasterData
open Medhavi.Transport

module MasterData =

    // Transport context: legs are loaded from MasterData's projection on demand
    let getTransportLegs (masterDataContext: Medhavi.MasterData.MasterData) =
        fun () ->
            async {
                let! legs =
                    masterDataContext.Queries.TransportLeg.GetAll()
                    |> Async.AwaitTask

                return
                    legs
                    |> List.filter (fun l -> l.Status)
                    |> List.map (fun l ->
                        { LegId = l.Id
                          Origin = l.Origin
                          Destination = l.Destination
                          Mode = l.Mode
                          LeadTimeMinutes = l.LeadTimeMinutes
                          Capacity = l.Capacity
                          CapacityUnit = l.CapacityUnit
                          Reliability = None // enrichable from full domain leg
                          CO2PerUnit = None
                          FixedCost = 0.0m
                          VariableCostPerUnit = None
                          Status = l.Status }
                        : Medhavi.Transport.TransportLegRef)
            }

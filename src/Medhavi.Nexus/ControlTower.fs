namespace Medhavi.Nexus

open System
open Medhavi.SharedKernel

/// Core Control Tower orchestrations
module DigitalTwin =

    /// Evaluates if an anomaly is detected based on raw telemetry values
    let detectAnomaly (sensorId: string) (reading: float) (threshold: float) =
        if reading > threshold then
            Some (sprintf "Sensor %s exceeded threshold of %f with value %f" sensorId threshold reading)
        else
            None

namespace Medhavi.SharedKernel.Observation

module Metrics =
    let recordCounter (name: string) (value: float) (tags: Map<string, string>) =
        Telemetry.createMetric name MetricType.Counter value tags

    let recordGauge (name: string) (value: float) (tags: Map<string, string>) =
        Telemetry.createMetric name MetricType.Gauge value tags

    let recordHistogram (name: string) (value: float) (tags: Map<string, string>) =
        Telemetry.createMetric name MetricType.Histogram value tags

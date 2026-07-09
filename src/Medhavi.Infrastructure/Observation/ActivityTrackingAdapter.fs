namespace Medhavi.Infrastructure.Observation

open System
open System.Diagnostics
open System.Threading.Tasks
open Medhavi.SharedKernel.Observation

/// Distributed tracing adapter using System.Diagnostics.Activity
/// Provides activity start/stop and a KnowledgeRepresentation
module ActivityTrackingAdapter =

    /// Start a new activity span
    let startActivity (activityName: string) (tags: (string * string) list) : Activity =
        let activity = new Activity(activityName)

        for key, value in tags do
            activity.SetTag(key, value) |> ignore

        activity.Start()

    /// Stop an activity and produce ArchitecturalKnowledge from it.
    let stopActivity (activity: Activity) : ArchitecturalKnowledge =
        activity.Stop()

        let properties =
            activity.Tags
            |> Seq.map(fun tag -> (tag.Key, box tag.Value))
            |> Map.ofSeq
            |> Map.add "ActivityName" (box activity.OperationName)
            |> Map.add "DurationMs" (box activity.Duration.TotalMilliseconds)
            |> Map.add "TraceId" (box activity.TraceId)
            |> Map.add "SpanId" (box activity.SpanId)

        { Name = activity.OperationName
          Timestamp = DateTimeOffset.UtcNow
          Attributes = properties }

    /// Execute an operation within an activity span, returning the result and the span knowledge.
    let withActivity
        (activityName: string)
        (tags: (string * string) list)
        (operation: unit -> 'T)
        : 'T * ArchitecturalKnowledge =
        let activity = startActivity activityName tags

        try
            let result = operation()
            let knowledge = stopActivity activity
            (result, knowledge)
        with ex ->
            activity.SetTag("error", "true") |> ignore
            activity.SetTag("error.message", ex.Message) |> ignore
            let knowledge = stopActivity activity
            reraise()

    /// Async version.
    let withActivityAsync
        (activityName: string)
        (tags: (string * string) list)
        (operation: unit -> Task<'T>)
        : Task<'T * ArchitecturalKnowledge> =
        task {
            let activity = startActivity activityName tags

            try
                let! result = operation()
                let knowledge = stopActivity activity
                return (result, knowledge)
            with ex ->
                activity.SetTag("error", "true") |> ignore
                activity.SetTag("error.message", ex.Message) |> ignore
                let knowledge = stopActivity activity
                return raise ex
        }

    /// Create a KnowledgeRepresentation that logs completed activity spans.
    /// This is a no‑op in production; traces are exported via OpenTelemetry, not the event bus.
    let toKnowledgeRepresentation () : KnowledgeRepresentation =
        fun knowledge ->
            // Activity knowledge is exported via OpenTelemetry automatically
            // when Activity.Current is set. This representation is a placeholder.
            ()

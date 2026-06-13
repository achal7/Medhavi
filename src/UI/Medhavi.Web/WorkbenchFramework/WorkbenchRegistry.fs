namespace Medhavi.Web.WorkbenchFramework

open System.Collections.Concurrent
open Medhavi.Web.WorkspaceEngine

module WorkbenchRegistry =
    let private specs = ConcurrentDictionary<WorkspaceKind, WorkbenchSpec>()

    let register (spec: WorkbenchSpec) =
        specs.[spec.Kind] <- spec

    let tryGet (kind: WorkspaceKind) =
        match specs.TryGetValue(kind) with
        | true, spec -> Some spec
        | _ -> None

    let getAllSpecs () =
        specs.Values |> Seq.toList

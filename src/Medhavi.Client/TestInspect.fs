module Medhavi.Client.TestInspect

open System
open Bolero
open Bolero.Html

let test () =
    let a = attr.empty()
    for m in a.GetType().GetMethods() do
        printfn "Attr method: %s" m.Name
    for t in typeof<Node>.Assembly.GetTypes() do
        if t.Name.Contains("Attr") || t.Name.Contains("attr") then
            printfn "Type: %s" t.FullName
            for m in t.GetMethods() do
                if m.IsPublic then printfn "  -> %s" m.Name


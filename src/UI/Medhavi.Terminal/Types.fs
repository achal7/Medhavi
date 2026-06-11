namespace Medhavi.Terminal

[<AutoOpen>]
module ResultExtensions =
    module Result =
        let get =
            function
            | Ok x -> x
            | Error e -> failwithf "Expected Ok, got Error: %A" e

type PrinterColor =
    | Green
    | Cyan
    | Yellow
    | Red
    | Bold
    | Reset

type Printer =
    { Print: PrinterColor -> string -> unit
      PrintLine: PrinterColor -> string -> unit }

module Printer =

    let printColor color text =
        let code =
            match color with
            | PrinterColor.Green -> "\u001b[32m"
            | PrinterColor.Cyan -> "\u001b[36m"
            | PrinterColor.Yellow -> "\u001b[33m"
            | PrinterColor.Red -> "\u001b[31m"
            | PrinterColor.Bold -> "\u001b[1m"
            | PrinterColor.Reset -> "\u001b[0m"

        printf "%s%s\u001b[0m" code text

    let create () =
        { Print = printColor
          PrintLine =
            fun color text ->
                printColor color text
                printfn "" }

    let printTable printer (title: string) (headers: string[]) (rows: string[][]) =
        let colCount = headers.Length

        let colWidths =
            Array.init colCount (fun i ->
                let headerWidth = headers.[i].Length

                let rowMax =
                    if rows.Length = 0 then
                        0
                    else
                        rows |> Seq.map (fun r -> r.[i].Length) |> Seq.max

                max headerWidth rowMax + 2)

        let totalTableWidth = max 40 (1 + (colWidths |> Array.sumBy (fun w -> w + 2)))

        printer.PrintLine Cyan ("\n┌" + String.replicate (totalTableWidth - 2) "─" + "┐")
        printfn "│ %-*s │" (totalTableWidth - 4) title
        printer.PrintLine Cyan ("├" + String.replicate (totalTableWidth - 2) "─" + "┤")

        // Render headers
        printf "│"

        for i in 0 .. colCount - 1 do
            printf " %-*s │" (colWidths.[i] - 1) headers.[i]

        printfn ""

        // Render header separator
        printf "├"

        for i in 0 .. colCount - 1 do
            printf "%s%s" (String.replicate (colWidths.[i] + 1) "─") (if i = colCount - 1 then "┤" else "┼")

        printfn ""

        // Render rows
        if rows.Length = 0 then
            printfn "│ %-*s │" (totalTableWidth - 4) "No data found."
        else
            for row in rows do
                printf "│"

                for i in 0 .. colCount - 1 do
                    printf " %-*s │" (colWidths.[i] - 1) row.[i]

                printfn ""

        // Render bottom border
        printf "└"

        for i in 0 .. colCount - 1 do
            printf "%s%s" (String.replicate (colWidths.[i] + 1) "─") (if i = colCount - 1 then "┘" else "┴")

        printfn ""

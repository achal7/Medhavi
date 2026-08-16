module Medhavi.Infrastructure.IO

open System

let getCsvPath fileName = System.IO.Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "csv", fileName)

let readCsvFile fileName : Result<string, string> =
    let path = getCsvPath fileName

    if System.IO.File.Exists(path) then
        Ok(System.IO.File.ReadAllText(path))
    else
        let fallbackPath = System.IO.Path.Combine("src", "Medhavi.Integration", "csv", fileName)

        if System.IO.File.Exists(fallbackPath) then
            Ok(System.IO.File.ReadAllText(fallbackPath))
        else
            let upFallbackPath = System.IO.Path.Combine("..", fallbackPath)

            if System.IO.File.Exists(upFallbackPath) then
                Ok(System.IO.File.ReadAllText(upFallbackPath))
            else
                let doubleUpFallbackPath = System.IO.Path.Combine("..", "..", fallbackPath)

                if System.IO.File.Exists(doubleUpFallbackPath) then
                    Ok(System.IO.File.ReadAllText(doubleUpFallbackPath))
                else
                    Error(sprintf "CSV file not found: %s (Checked %s and fallbacks)" fileName path)

namespace Medhavi.E2E.Tests

open Expecto

module Program =
    [<EntryPoint>]
    let main argv =
        Tests.runTestsInAssemblyWithCLIArgs [] argv

namespace Medhavi.Planning.Tests

open Expecto

module Program =
    [<EntryPoint>]
    let main argv =
        Tests.runTestsInAssemblyWithCLIArgs [] argv

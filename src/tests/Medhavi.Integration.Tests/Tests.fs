namespace Medhavi.Integration.Tests

open Expecto
open Swensen.Unquote
open Medhavi.Infrastructure

module RepositoryTests =

    [<Tests>]
    let tests =
        testList "Repository Schema Tests" [
            testCase "should initialize scenario repository without errors" (fun () ->
                let repo = ScenarioRepository()
                // Just a basic check that it initializes
                test <@ box repo <> null @>
            )
        ]

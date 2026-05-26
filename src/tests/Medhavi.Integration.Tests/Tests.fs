namespace Medhavi.Integration.Tests

open Expecto
open Swensen.Unquote
open Medhavi.SharedKernel
open Medhavi.Infrastructure
open Medhavi.Infrastructure.Stores.InMemRepository

type MockEntity = { Id: string; Value: int }

module RepositoryTests =

    [<Tests>]
    let tests =
        testList
            "Repository Schema Tests"
            [ testCase "should initialize scenario repository without errors" (fun () ->
                  let repo = ScenarioRepository()
                  // Just a basic check that it initializes
                  test <@ box repo <> null @>)

              testCase "InMemoryRepository CRUD operations validation" (fun () ->
                  let repo = createInMemoryRepository<MockEntity, string, string> ()

                  let entity = { Id = "entity-01"; Value = 42 }

                  let saveResult =
                      (repo.Save("entity-01", entity, [ "CreatedEvent" ])).Result

                  let findResult = (repo.Get "entity-01").Result

                  let isSaveOk =
                      match saveResult with
                      | Ok _ -> true
                      | Error _ -> false

                  let isFoundValueOk =
                      match findResult with
                      | Ok(Some found) -> found.Value = 42
                      | _ -> false

                  test <@ isSaveOk @>
                  test <@ isFoundValueOk @>

                  let deleteResult = (repo.Delete "entity-01").Result
                  let findAfterDeleteResult = (repo.Get "entity-01").Result

                  let isDeleteOk =
                      match deleteResult with
                      | Ok _ -> true
                      | Error _ -> false

                  let isNotFoundOk =
                      match findAfterDeleteResult with
                      | Ok None -> true
                      | _ -> false

                  test <@ isDeleteOk @>
                  test <@ isNotFoundOk @>) ]

module Medhavi.Common.Task

open System.Threading.Tasks

type Task with
    static member Ignore(t: Task) = t.ContinueWith(fun _ -> ()) |> ignore

    static member Map (f: 'a -> 'b) (t: Task<Result<'a, 'e>>) : Task<Result<'b, 'e>> =
        task {
            let! r = t
            return Result.map f r
        }

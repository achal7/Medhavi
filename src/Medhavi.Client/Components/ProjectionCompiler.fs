namespace Medhavi.Client.Components.Shared

open System
open Microsoft.FSharp.Reflection
open Medhavi.Client.ViewComposition.Schema
open Medhavi.Client.ViewComposition.Data
open Medhavi.Common.Validation

module ProjectionCompiler =

    let rec private convertValue (value: obj) : CellValue =
        match value with
        | null -> EmptyValue
        | :? string as s -> TextValue s
        | :? int as i -> NumberValue(decimal i)
        | :? int16 as i -> NumberValue(decimal i)
        | :? int64 as i -> NumberValue(decimal i)
        | :? uint32 as i -> NumberValue(decimal i)
        | :? byte as b -> NumberValue(decimal b)
        | :? decimal as d -> NumberValue d
        | :? float as f -> NumberValue(decimal f)
        | :? float32 as f -> NumberValue(decimal f)
        | :? bool as b -> BooleanValue b
        | :? DateTime as dt -> DateTimeValue(DateTimeOffset(dt))
        | :? DateTimeOffset as dto -> DateTimeValue dto
        | other ->
            let t = other.GetType()
            if t.IsGenericType && t.GetGenericTypeDefinition() = typedefof<option<_>> then
                let _, fields = FSharpValue.GetUnionFields(other, t)
                if fields.Length = 0 then EmptyValue
                else convertValue fields.[0]
            else
                TextValue(other.ToString())

    let validateSchemaMatchesRecord<'T> (schema: SchemaDefinition) : Validation<unit, string> =
        let recordType = typeof<'T>

        if not(FSharpType.IsRecord recordType) then
            Invalid [ $"Type {recordType.Name} is not an F# record" ]
        else
            let recordFields = FSharpType.GetRecordFields recordType |> Array.map(fun pi -> pi.Name) |> Set.ofArray
            let schemaProperties = schema.Columns |> List.choose(fun c -> c.PropertyName) |> Set.ofList
            let missing = Set.difference schemaProperties recordFields

            let errors =
                [ if not(Set.isEmpty missing) then
                      yield! missing |> Set.toList |> List.map(sprintf "Schema references missing field: %s") ]

            match errors with
            | [] -> Valid()
            | errs -> Invalid errs

    let compile<'T when 'T: not null> (schema: SchemaDefinition) : Validation<ProjectionAdapter<'T>, string> =
        match validateSchemaMatchesRecord<'T> schema with
        | Invalid errors -> Invalid errors
        | Valid() ->
            let recordType = typeof<'T>

            let fieldReaders =
                FSharpType.GetRecordFields recordType
                |> Array.map(fun pi ->
                    let reader = FSharpValue.PreComputeRecordFieldReader pi
                    pi.Name, reader)

            let keyColumn = schema.Columns |> List.tryFind(fun c -> c.IsKey)

            let toUniversal (record: 'T) : UniversalRow =
                let cells =
                    schema.Columns
                    |> List.choose(fun col ->
                        col.PropertyName
                        |> Option.bind(fun propName ->
                            fieldReaders
                            |> Array.tryFind(fun (name, _) -> name = propName)
                            |> Option.map(fun (_, reader) ->
                                let value = reader record
                                col.ColumnId, convertValue value)))
                    |> Map.ofList

                let key =
                    keyColumn
                    |> Option.bind(fun kc -> kc.PropertyName)
                    |> Option.bind(fun propName ->
                        fieldReaders
                        |> Array.tryFind(fun (name, _) -> name = propName)
                        |> Option.map(fun (_, reader) -> reader record |> string))
                    |> Option.defaultValue(Guid.NewGuid().ToString())

                { Key = key; Cells = cells }

            Valid { ToUniversal = toUniversal }

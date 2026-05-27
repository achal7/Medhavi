module Medhavi.MasterData.Domain.SkuAgg

open Medhavi.Common
open Medhavi.Common.Validation
open Medhavi.SharedKernel
open Medhavi.SharedKernel.Validations
open Medhavi.SharedKernel.Aggregate

type Sku =
    { Id: SkuId
      Code: string
      Name: string
      Group: string
      Status: Status
      CreatedAt: Timestamp
      ModifiedAt: Timestamp }

type DefineSkuCmd =
    { Id: string
      Code: string
      Name: string
      Group: string
      CreatedAt: Timestamp }

type SkuCommand =
    | DefineSku of DefineSkuCmd
    | RenameSku of id: SkuId * name: string
    | RetireSku of id: SkuId

type SkuEvent =
    | SkuDefined of Sku
    | SkuRenamed of SkuId * string * Timestamp
    | SkuRetired of SkuId * Timestamp

type DecideSku = Decide<Sku, SkuCommand, SkuEvent>
type EvolveSku = Evolve<Sku, SkuEvent>

let createSku id code name group =
    { Id = id
      Code = code
      Name = name
      Group = group
      Status = Active
      CreatedAt = Timestamp.now
      ModifiedAt = Timestamp.minValue }

let validateDefineSkuCmd now (cmd: DefineSkuCmd) =
    createSku <!> (SkuId.create cmd.Id |> fromResult)
    <*> required "Sku code" cmd.Code
    <*> required "Sku name" cmd.Name
    <*> required "Sku group" cmd.Group

let decide: DecideSku =
    fun command stateOpt ->
        match command, stateOpt with
        | DefineSku cmd, None ->
            createAggregate (validateDefineSkuCmd Timestamp.now) (fun Sku -> [ SkuDefined Sku ]) cmd

        | DefineSku _, Some _ -> Error(DomainError.validation "Sku already exists")

        | RenameSku(id, name), Some state when state.Id = id ->
            match state.Status with
            | Inactive -> Error(DomainError.invariant "Cannot update an inactive Sku")
            | Active ->
                required "Sku name" name
                |> toResult
                |> Result.mapError DomainError.combineValidationErrors
                |> Result.map (fun updatedSku ->
                    let updatedSku =
                        { state with
                            Name = name.Trim()
                            ModifiedAt = Timestamp.now }

                    { NewState = updatedSku
                      Events = [ SkuRenamed(id, updatedSku.Name, updatedSku.ModifiedAt) ] })

        | RenameSku _, Some _ -> Error(DomainError.validation "Sku not found")

        | RetireSku id, Some state when state.Id = id ->
            match state.Status with
            | Inactive -> Error(DomainError.invariant "Sku is already inactive")
            | Active ->
                let updated =
                    { state with
                        Status = Inactive
                        ModifiedAt = Timestamp.now }

                Ok
                    { NewState = updated
                      Events = [ SkuRetired(id, updated.ModifiedAt) ] }

        | RetireSku _, Some _ -> Error(DomainError.validation "Sku not found")

        | _, None -> Error(DomainError.validation "Sku not found")

let evolve: EvolveSku =
    fun event stateOpt ->
        match event, stateOpt with
        | SkuDefined state, None -> Some state
        | SkuRenamed(id, newName, modifiedAt), Some state when state.Id = id ->
            Some
                { state with
                    Name = newName
                    ModifiedAt = modifiedAt }
        | SkuRetired(id, modifiedAt), Some state when state.Id = id ->
            Some
                { state with
                    Status = Inactive
                    ModifiedAt = modifiedAt }
        | SkuDefined _, Some state -> Some state
        | _, current -> current

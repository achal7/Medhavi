namespace Medhavi.Web

open Elmish

[<AutoOpen>]
module Utility =
    let updateChild getChild setChild mapMsg childUpdate handleOutput childMsg parentModel =
        let childModel, childCmd, output = childUpdate childMsg (getChild parentModel)
        let parentModel = setChild childModel parentModel
        let mappedCmd = Cmd.map mapMsg childCmd

        match output with
        | Some output ->
            let parentModel, parentCmd = handleOutput output parentModel
            parentModel, Cmd.batch [ mappedCmd; parentCmd ]
        | None -> parentModel, mappedCmd

    let updateChildWithOutput getChild setChild mapMsg childUpdate handleOutput childMsg parentModel =
        let childModel, childCmd, output = childUpdate childMsg (getChild parentModel)
        let parentModel = setChild childModel parentModel
        let mappedCmd = Cmd.map mapMsg childCmd

        match output with
        | Some output ->
            let parentModel, parentCmd, parentOutput = handleOutput output parentModel
            parentModel, Cmd.batch [ mappedCmd; parentCmd ], parentOutput
        | None -> parentModel, mappedCmd, None

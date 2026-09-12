namespace Medhavi.Client

open Elmish

[<AutoOpen>]
module Utility =

    let updateChild
        (getChild: 'parentModel -> 'childModel)
        (setChild: 'childModel -> 'parentModel -> 'parentModel)
        (mapMsg: 'childMsg -> 'parentMsg)
        (childUpdate: 'childMsg -> 'childModel -> 'childModel * Cmd<'childMsg>)
        (childMsg: 'childMsg)
        (parentModel: 'parentModel)
        : 'parentModel * Cmd<'parentMsg> =

        let childModel, childCmd = childUpdate childMsg (getChild parentModel)

        let parentModel = setChild childModel parentModel

        parentModel, Cmd.map mapMsg childCmd

    let updateChildWithOutput
        (getChild: 'parentModel -> 'childModel)
        (setChild: 'childModel -> 'parentModel -> 'parentModel)
        (mapMsg: 'childMsg -> 'parentMsg)
        (childUpdate: 'childMsg -> 'childModel -> 'childModel * Cmd<'childMsg> * 'output option)
        (handleOutput: 'output -> 'parentModel -> 'parentModel * Cmd<'parentMsg> * 'parentOutput option)
        (childMsg: 'childMsg)
        (parentModel: 'parentModel)
        : 'parentModel * Cmd<'parentMsg> * 'parentOutput option =

        let childModel, childCmd, childOutput = childUpdate childMsg (getChild parentModel)

        let parentModel = setChild childModel parentModel

        let mappedChildCmd = Cmd.map mapMsg childCmd

        match childOutput with
        | None -> parentModel, mappedChildCmd, None

        | Some output ->
            let parentModel, parentCmd, parentOutput = handleOutput output parentModel

            parentModel, Cmd.batch [ mappedChildCmd; parentCmd ], parentOutput

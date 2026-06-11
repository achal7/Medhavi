namespace Medhavi.Web

open System
open Elmish
open Bolero
open Bolero.Html
open Microsoft.AspNetCore.Components.Web
open Radzen
open Radzen.Blazor

module LoginShell =

    type Model = {
        ErrorMessage: string option
        IsSubmitting: bool
    }

    type Msg =
        | SubmitLogin of username: string * password: string
        | LoginFailed of string
        | LoginSuccess of User

    let init () = {
        ErrorMessage = None
        IsSubmitting = false
    }

    let update msg model =
        match msg with
        | SubmitLogin (u, p) ->
            { model with IsSubmitting = true; ErrorMessage = None }, Cmd.none
        | LoginFailed err ->
            { model with ErrorMessage = Some err; IsSubmitting = false }, Cmd.none
        | LoginSuccess _ ->
            { model with IsSubmitting = false }, Cmd.none

    let view (model: Model) (dispatch: Msg -> unit) =
        div {
            attr.``class`` "login-wrapper"
            attr.style "display: flex; align-items: center; justify-content: center; height: 100vh; background: radial-gradient(circle, #1e293b 0%, #0f172a 100%); font-family: var(--rz-font-family);"
            
            comp<RadzenRow> {
                "Gap" => "0"
                attr.``class`` "rz-my-12 rz-mx-auto rz-border-radius-6 rz-shadow-10"
                "Style" => "width: 100%; max-width: 800px; overflow: hidden; border: 1px solid rgba(255,255,255,0.05);"
                
                // Left Column: Welcome graphics panel
                comp<RadzenColumn> {
                    "Size" => 12
                    "SizeMD" => 6
                    
                    comp<RadzenCard> {
                        attr.``class`` "rz-shadow-0 rz-border-radius-0 rz-text-align-center rz-p-12"
                        "Style" => "height: 100%; background: var(--rz-primary-light) no-repeat 100% 70% fixed url('data:image/svg+xml;base64,PHN2ZyB3aWR0aD0iMTIwNCIgaGVpZ2h0PSIxNDU4IiB2aWV3Qm94PSIwIDAgMTIwNCAxNDU4IiBmaWxsPSJub25lIiB4bWxucz0iaHR0cDovL3d3dy53My5vcmcvMjAwMC9zdmciPgo8ZyBvcGFjaXR5PSIwLjUiIGZpbHRlcj0idXJsKCNmaWx0ZXIwX2ZfNDkzXzEwMTM0KSI+CjxjaXJjbGUgY3g9IjcyMi4xMjgiIGN5PSI4MzkuMDIiIHI9IjQ4MS40MTkiIGZpbGw9InVybCgjcGFpbnQwX3JhZGlhbF80OTNfMTAxMzQpIi8+CjwvZz4KPGcgb3BhY2l0eT0iMC41IiBmaWx0ZXI9InVybCgjZmlsdGVyMV9mXzQ5M18xMDEzNCkiPgo8Y2lyY2xlIGN4PSI0NzAuMzMzIiBjeT0iNTcwLjMzMyIgcj0iNDcwLjMzMyIgZmlsbD0idXJsKCNwYWludDFfcmFkaWFsXzQ5M18xMDEzNCkiLz4KPC9nPgo8ZyBvcGFjaXR5PSIwLjUiIGZpbHRlcj0idXJsKCNmaWx0ZXIyX2ZfNDkzXzEwMTM0KSI+CjxjaXJjbGUgY3g9IjY5MS41MTEiIGN5PSI1MjIuMjk3IiByPSIzMzEuNTAzIiBmaWxsPSJ1cmwoI3BhaW50Ml9yYWRpYWxfNDkzXzEwMTM0KSIvPgo8L2c+CjxnIG9wYWNpdHk9IjAuNSIgZmlsdGVyPSJ1cmwoI2ZpbHRlcjNfZl80OTNfMTAxMzQpIj4KPGNpcmNsZSBjeD0iNjA4LjI0NCIgY3k9IjEwNzkuOTciIHI9IjMzMS41MDMiIHRyYW5zZm9ybT0icm90YXRlKC04MS4yMjQ0IDYwOC4yNDQgMTA3OS45NykiIGZpbGw9InVybCgjcGFpbnQzX3JhZGlhbF80OTNfMTAxMzQpIi8+CjwvZz4KPGRlZnM+CjxmaWx0ZXIgaWQ9ImZpbHRlcjBfZl80OTNfMTAxMzQiIHg9IjE0MC43MDkiIHk9IjI1Ny42MDEiIHdpZHRoPSIxMTYyLjg0IiBoZWlnaHQ9IjExNjIuODQiIGZpbHRlclVuaXRzPSJ1c2VyU3BhY2VPblVzZSIgY29sb3ItaW50ZXJwb2xhdGlvbi1maWx0ZXJzPSJzUkdCIj4KPGZlRmxvb2QgZmxvb2Qtb3BhY2l0eT0iMCIgcmVzdWx0PSJCYWNrZ3JvdW5kSW1hZ2VGaXgiLz4KPGZlQmxlbmQgbW9kZT0ibm9ybWFsIiBpbj0iU291cmNlR3JhcGhpYyIgaW4yPSJCYWNrZ3JvdW5kSW1hZ2VGaXgiIHJlc3VsdD0ic2hhcGUiLz4KPGZlR2F1c3NpYW5CbHVyIHN0ZERldmlhdGlvbj0iNTAiIHJlc3VsdD0iZWZmZWN0MV9mb3JlZ3JvdW5kQmx1cl80OTNfMTAxMzQiLz4KPC9maWx0ZXI+CjxmaWx0ZXIgaWQ9ImZpbHRlcjFfZl80OTNfMTAxMzQiIHg9Ii0xMDAiIHk9IjAiIHdpZHRoPSIxMTQwLjY3IiBoZWlnaHQ9IjExNDAuNjciIGZpbHRlclVuaXRzPSJ1c2VyU3BhY2VPblVzZSIgY29sb3ItaW50ZXJwb2xhdGlvbi1maWx0ZXJzPSJzUkdCIj4KPGZlRmxvb2QgZmxvb2Qtb3BhY2l0eT0iMCIgcmVzdWx0PSJCYWNrZ3JvdW5kSW1hZ2VGaXgiLz4KPGZlQmxlbmQgbW9kZT0ibm9ybWFsIiBpbj0iU291cmNlR3JhcGhpYyIgaW4yPSJCYWNrZ3JvdW5kSW1hZ2VGaXgiIHJlc3VsdD0ic2hhcGUiLz4KPGZlR2F1c3NpYW5CbHVyIHN0ZERldmlhdGlvbj0iNTAiIHJlc3VsdD0iZWZmZWN0MV9mb3JlZ3JvdW5kQmx1cl80OTNfMTAxMzQiLz4KPC9maWx0ZXI+CjxmaWx0ZXIgaWQ9ImZpbHRlcjJfZl80OTNfMTAxMzQiIHg9IjblockMC4wMDgiIHk9IjkwLjc5MzkiIHdpZHRoPSI4NjMuMDA2IiBoZWlnaHQ9Ijg2My4wMDYiIGZpbHRlclVuaXRzPSJ1c2VyU3BhY2VPblVzZSIgY29sb3ItaW50ZXJwb2xhdGlvbi1maWx0ZXJzPSJzUkdCIj4KPGZlRmxvb2QgZmxvb2Qtb3BhY2l0eT0iMCIgcmVzdWx0PSJCYWNrZ3JvdW5kSW1hZ2VGaXgiLz4KPGZlQmxlbmQgbW9kZT0ibm9ybWFsIiBpbj0iU291cmNlR3JhcGhpYyIgaW4yPSJCYWNrZ3JvdW5kSW1hZ2VGaXgiIHJlc3VsdD0ic2hhcGUiLz4KPGZlR2F1c3NpYW5CbHVyIHN0ZERldmlhdGlvbj0iNTAiIHJlc3VsdD0iZWZmZWN0MV9mb3JlZ3JvdW5kQmx1cl80OTNfMTAxMzQiLz4KPC9maWx0ZXI+CjxmaWx0ZXIgaWQ9ImZpbHRlcjNfZl80OTNfMTAxMzQiIHg9IjE3Ni42OTQiIHk9IjY0OC40MjMiIHdpZHRoPSI4NjMuMSIgaGVpZ2h0PSI4NjMuMSIgZmlsdGVyVW5pdHM9InVzZXJTcGFjZU9uVXNlIiBjb2xvci1pbnRlcnBvbGF0aW9uLWZpbHRlcnM9InNSR0IiPgo8ZmVGbG9vZCBmbG9vZC1vcGFjaXR5PSIwIiByZXN1bHQ9IkJhY2tncm91bmRJbWFnZUZpeCIvPgo8ZmVCbGVuZCBtb2RlPSJub3JtYWwiIGluPSJTb3VyY2VHcmFwaGljIiBpbjI9IkJhY2tncm91bmRJbWFnZUZpeCIgcmVzdWx0PSJzaGFwZSIvPgo8ZmVHYXVzc2lhbkJsdXIgc3RkRGV2aWF0aW9uPSI1MCIgcmVzdWx0PSJlZmZlY3QxX2ZvcmVncm91bmRCbHVyXzQ5M18xMDEzNCIvPgo8L2ZpbHRlcj4KPHJhZGlhbEdyYWRpZW50IGlkPSJwYWludDBfcmFkaWFsXzQ5M18xMDEzNCIgY3g9IjAiIGN5PSIwIiByPSIxIiBncmFheaderVW5pdHM9InVzZXJTcGFjZU9uVXNlIiBncmFkaWVudFRyYW5zZm9ybT0idHJhbnNsYXRlKDcyMi4xMjggODM5LjwmiSByb3RhdGUoOTApIHNjYWxlKDQ4MS40MTkpIj4KPHN0b3Agc3RvcC1jb2xvcj0iI0ZGMUE2QyIvPgo8c3RvcCBvZmZzZXQ9IjEiIHN0b3AtY29sb3I9IiNGRjFBNkMiIHN0b3Atb3BhY2l0eT0iMCIvPgo8L3JhZGlhbEdyYWRpZW50Pgo8cmFkaWFsR3JhZGllbnQgaWQ9InBhaW50MV9yYWRpYWxfNDkzXzEwMTM0IiBjeD0iMCIgY3k9IjAiIHI9IjEiIGdyYWRpZW50VW5pdHM9InVzZXJTcGFjZU9uVXNlIiBncmFkaWVudFRyYW5zZm9ybT0idHJhbnNsYXRlKDQ3MC4zMzMgNTcwLjMzMykgcm90YXRlKDkwKSBzY2FsZSg0NzAuMzMzKSI+CjxzdG9wIHN0b3AtY29sb3I9IiMzQUFDRkYiLz4KPHN0b3Agb2Zmc2V0PSIxIiBzdG9wLWNvbG9yPSIjM0E5NUZGIiBzdG9wLW9wYWNpdHk9IjAiLz4KPC9yYWRpYWxHcmFkaWVudD4KPHJhZGlhbEdyYWRpZW50IGlkPSJwYWludFpfcmFkaWFsXzQ5M18xMDEzNCIgY3g9IjAiIGN5PSIwIiByPSIxIiBncmFkaWVudFVuaXRzPSJ1c2VyU3BhY2VPblVzZSIgZ3JhZGllbnRUcmFuc2Zvcm09InRyYW5zbGF0ZSg2OTEuNTExIDUyMi4yOTcpIHJvdGF0ZSg5MCkgc2NhbGUoMzMxLjUwMykiPgo8c3RvcCBzdG9wLWNvbG9yPSIjNDgzQUZGIi8+CjxzdG9wIG9mZnNldD0iMSIgc3RvcC1jb2xvcj0iIzQ4M0FGRiIgc3RvcC1vcGFjaXR5PSIwIi8+CjwvcmFkaWFsR3JhZGllbnQ+CjxyYWRpYWxHcmFkaWVudCBpZD0icGFpbnQzX3JhZGlhbF80OTNfMTAxMzQiIGN4PSIwIiBjeT0iMCIgcj0iMSIgZ3JhZGllbnRVbml0cz0idXNlclNwYWNlT25Vc2UiIGdyYWRpZW50VHJhbnNmb3JtPSJ0cmFuc2xhdGUoNjA4LjI0NCAxMDc5Ljk3KSByb3RhdGUoOTApIHNjYWxlKDMzMS41MDMpIj4KPHN0b3Agc3RvcC1jb2xvcj0iI0ZGQzgzQSIvPgo8c3RvcCBvZmZzZXQ9IjEiIHN0b3AtY29sb3I9IiNGRkM4M0EiIHN0b3Atb3BhY2l0eT0iMCIvPgo8L3JhZGlhbEdyYWRpZW50Pgo8L2RlZnM+Cjwvc3ZnPgo=') ; display: flex; flex-direction: column; justify-content: center; align-items: center;"
                        
                        comp<RadzenStack> {
                            "Orientation" => Orientation.Vertical
                            "AlignItems" => AlignItems.Center
                            "Gap" => "12px"
                            
                            comp<RadzenIcon> {
                                "Icon" => "bubble_chart"
                                "Style" => "font-size: 56px; color: #ffffff;"
                            }
                            comp<RadzenText> {
                                "TextStyle" => TextStyle.DisplayH3
                                "TagName" => TagName.H2
                                attr.``class`` "rz-color-white rz-m-0"
                                "Text" => "Welcome!"
                            }
                            comp<RadzenText> {
                                "TextStyle" => TextStyle.H6
                                attr.``class`` "rz-color-white rz-m-0"
                                "Text" => "Medhavi APS Client Portal"
                            }
                            comp<RadzenText> {
                                "TextStyle" => TextStyle.Body2
                                attr.``class`` "rz-color-white rz-opacity-75 rz-text-align-center"
                                "Text" => "Sign in to access advanced planning, scheduling workbenches, and optimizer actions."
                            }
                        }
                    }
                }
                
                // Right Column: Form fields Panel
                comp<RadzenColumn> {
                    "Size" => 12
                    "SizeMD" => 6
                    
                    comp<RadzenCard> {
                        attr.``class`` "rz-shadow-0 rz-border-radius-0 rz-p-12"
                        "Style" => "height: 100%; display: flex; flex-direction: column; justify-content: center;"
                        
                        comp<RadzenText> {
                            "TextStyle" => TextStyle.H5
                            "TagName" => TagName.H2
                            attr.``class`` "rz-mb-6"
                            "Text" => "Login"
                        }
                        
                        match model.ErrorMessage with
                        | Some err ->
                            comp<RadzenAlert> {
                                "Text" => err
                                "AlertStyle" => AlertStyle.Danger
                                "Variant" => Variant.Flat
                                "Shade" => Shade.Dark
                            }
                        | None -> empty()
                        
                        comp<RadzenLogin> {
                            "Username" => "admin"
                            "Password" => "password"
                            "AllowRegister" => false
                            "AllowResetPassword" => false
                            "AllowRememberMe" => false
                            attr.callback "Login" (fun (args: LoginArgs) ->
                                dispatch (SubmitLogin (args.Username, args.Password))
                            )
                        }
                    }
                }
            }
        }

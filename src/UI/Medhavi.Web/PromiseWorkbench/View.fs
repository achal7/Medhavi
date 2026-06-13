namespace Medhavi.Web.PromiseWorkbench

open Bolero
open Bolero.Html
open Radzen
open Radzen.Blazor
open Medhavi.Web.Components
open Medhavi.Web.Panels
open Microsoft.AspNetCore.Components
open System
open Medhavi.Contracts.Promise
open Medhavi.Nexus

module View =

    let renderResult (resp: PromiseEvaluationResponse) =

        let isAccepted, promiseDateOpt, limiterOpt =
            match resp.Decision with
            | PromiseDecisionStatus.Accepted dateRangeOpt -> true, dateRangeOpt, None
            | PromiseDecisionStatus.Rejected limiter -> false, None, Some limiter

        let badgeStyle =
            if isAccepted then "background-color: var(--rz-success-color); color: white; padding: 6px 12px; border-radius: 4px; font-weight: bold;"
            else "background-color: var(--rz-danger-color); color: white; padding: 6px 12px; border-radius: 4px; font-weight: bold;"

        div {
            attr.style "display: flex; flex-direction: column; gap: 20px;"

            // Header Decision status
            comp<RadzenCard> {
                "Style"
                => "padding: 16px; border-radius: 8px; border: 1px solid var(--rz-border-color);"

                Rz.stack (
                    [ div {
                          attr.style "display: flex; justify-content: space-between; align-items: center;"

                          h4 {
                              attr.style "font-weight: bold; margin: 0; font-family: var(--rz-font-family);"
                              "Evaluation Decision"
                          }

                          span {
                              attr.style badgeStyle
                              if isAccepted then "ACCEPTED" else "REJECTED"
                          }
                      }
                      if not isAccepted then
                          match limiterOpt with
                          | Some lim ->
                              div {
                                  attr.style
                                      "background-color: rgba(244, 67, 54, 0.05); padding: 12px; border-radius: 6px; border-left: 4px solid var(--rz-danger-color); font-family: var(--rz-font-family);"

                                  Rz.stack (
                                      [ div {
                                            attr.style "font-weight: bold; font-size: 13px;"
                                            sprintf "Limiter: %A | Reason: %A" lim.Domain lim.Code
                                        }
                                        div {
                                            attr.style
                                                "font-size: 12px; color: var(--rz-color-text-secondary); margin-top: 4px;"

                                            lim.Message
                                        }
                                        if not lim.Suggestions.IsEmpty then
                                            div {
                                                attr.style "font-size: 11px; margin-top: 8px;"

                                                span {
                                                    attr.style "font-weight: bold;"
                                                    "Remediation suggestions: "
                                                }

                                                ul {
                                                    attr.style "margin: 4px 0 0 16px; padding: 0;"

                                                    for sug in lim.Suggestions do
                                                        li { sug }
                                                }
                                            } ],
                                      gap = "4px"
                                  )
                              }
                          | None -> empty () ],
                    gap = "12px"
                )
            }

            // Date calculations
            match promiseDateOpt with
            | Some d ->
                comp<RadzenCard> {
                    "Style"
                    => "padding: 16px; border-radius: 8px; border: 1px solid var(--rz-border-color);"

                    Rz.stack (
                        [ h4 {
                              attr.style "font-weight: bold; margin: 0; font-family: var(--rz-font-family);"
                              "Committed Dates"
                          }
                          div {
                              attr.style
                                  "display: grid; grid-template-columns: 1fr 1fr 1fr; gap: 12px; font-family: var(--rz-font-family);"

                              div {
                                  attr.style
                                      "text-align: center; padding: 8px; background-color: rgba(255,255,255,0.02); border-radius: 4px;"

                                  span {
                                      attr.style
                                          "display: block; font-size: 11px; color: var(--rz-color-text-secondary);"

                                      "Earliest Arrival"
                                  }

                                  span {
                                      attr.style "font-weight: bold; font-size: 13px;"
                                      d.Earliest.ToString("yyyy-MM-dd")
                                  }
                              }

                              div {
                                  attr.style
                                      "text-align: center; padding: 8px; background-color: rgba(33, 150, 243, 0.05); border-radius: 4px; border: 1px solid rgba(33, 150, 243, 0.2);"

                                  span {
                                      attr.style "display: block; font-size: 11px; color: var(--rz-info-color);"
                                      "Committed Date (P50)"
                                  }

                                  span {
                                      attr.style "font-weight: bold; font-size: 13px; color: var(--rz-info-color);"
                                      d.Committed.ToString("yyyy-MM-dd")
                                  }
                              }

                              div {
                                  attr.style
                                      "text-align: center; padding: 8px; background-color: rgba(255,255,255,0.02); border-radius: 4px;"

                                  span {
                                      attr.style
                                          "display: block; font-size: 11px; color: var(--rz-color-text-secondary);"

                                      "Latest Arrival (P95)"
                                  }

                                  span {
                                      attr.style "font-weight: bold; font-size: 13px;"
                                      d.Latest.ToString("yyyy-MM-dd")
                                  }
                              }
                          } ],
                        gap = "12px"
                    )
                }
            | None -> empty ()

            // Confidence score
            match resp.Confidence with
            | Some conf ->
                comp<RadzenCard> {
                    "Style"
                    => "padding: 16px; border-radius: 8px; border: 1px solid var(--rz-border-color);"

                    Rz.stack (
                        [ h4 {
                              attr.style "font-weight: bold; margin: 0; font-family: var(--rz-font-family);"
                              "Simulation Confidence"
                          }
                          div {
                              attr.style "font-family: var(--rz-font-family);"
                              Rz.progressBar (double (conf * 100.0))

                              span {
                                  attr.style
                                      "font-size: 11px; color: var(--rz-color-text-secondary); margin-top: 4px; display: block;"

                                  sprintf "Plan reliability score: %.1f%%" (conf * 100.0)
                              }
                          } ],
                        gap = "12px"
                    )
                }
            | None -> empty ()

            // Cost breakdowns
            match resp.Cost with
            | Some c ->
                comp<RadzenCard> {
                    "Style"
                    => "padding: 16px; border-radius: 8px; border: 1px solid var(--rz-border-color);"

                    Rz.stack (
                        [ h4 {
                              attr.style "font-weight: bold; margin: 0; font-family: var(--rz-font-family);"
                              "Cost Breakdown"
                          }
                          div {
                              attr.style
                                  "display: flex; flex-direction: column; gap: 8px; font-family: var(--rz-font-family); font-size: 12px;"

                              div {
                                  attr.style "display: flex; justify-content: space-between;"
                                  span { "Material Cost" }

                                  span {
                                      attr.style "font-weight: 500;"
                                      sprintf "$%M" c.MaterialCost
                                  }
                              }

                              div {
                                  attr.style "display: flex; justify-content: space-between;"
                                  span { "Production Cost" }

                                  span {
                                      attr.style "font-weight: 500;"
                                      sprintf "$%M" c.ProductionCost
                                  }
                              }

                              div {
                                  attr.style "display: flex; justify-content: space-between;"
                                  span { "Transport Cost" }

                                  span {
                                      attr.style "font-weight: 500;"
                                      sprintf "$%M" c.TransportCost
                                  }
                              }

                              div {
                                  attr.style "display: flex; justify-content: space-between;"
                                  span { "Holding Cost" }

                                  span {
                                      attr.style "font-weight: 500;"
                                      sprintf "$%M" c.HoldingCost
                                  }
                              }

                              div {
                                  attr.style "display: flex; justify-content: space-between;"
                                  span { "Lateness Penalty" }

                                  span {
                                      attr.style "font-weight: 500;"
                                      sprintf "$%M" c.LatenessPenalty
                                  }
                              }

                              div {
                                  attr.style
                                      "display: flex; justify-content: space-between; border-top: 1px solid var(--rz-border-color); padding-top: 8px; font-weight: bold;"

                                  span { "Total Computed Cost" }
                                  span { sprintf "$%M" c.TotalCost }
                              }
                          } ],
                        gap = "12px"
                    )
                }
            | None -> empty ()

            // Routing details
            match resp.Routing with
            | Some route ->
                comp<RadzenCard> {
                    "Style"
                    => "padding: 16px; border-radius: 8px; border: 1px solid var(--rz-border-color);"

                    Rz.stack (
                        [ h4 {
                              attr.style "font-weight: bold; margin: 0; font-family: var(--rz-font-family);"
                              "Selected Routing"
                          }
                          div {
                              attr.style
                                  "font-family: var(--rz-font-family); font-size: 12px; display: flex; flex-direction: column; gap: 6px;"

                              div {
                                  span { "Routing ID: " }

                                  span {
                                      attr.style "font-weight: bold;"
                                      string route.RoutingId
                                  }
                              }

                              div {
                                  span { "Alternate used: " }

                                  span {
                                      attr.style "font-weight: bold;"
                                      if route.AlternateUsed then "Yes" else "No"
                                  }
                              }

                              match route.EstimatedDuration with
                              | Some dur ->
                                  div {
                                      span { "Estimated cycle time: " }

                                      span {
                                          attr.style "font-weight: bold;"
                                          sprintf "%.1f hours" dur.TotalHours
                                      }
                                  }
                              | None -> empty ()
                          } ],
                        gap = "12px"
                    )
                }
            | None -> empty ()

            // Reservations
            if not resp.Reservations.IsEmpty then
                comp<RadzenCard> {
                    "Style"
                    => "padding: 16px; border-radius: 8px; border: 1px solid var(--rz-border-color);"

                    Rz.stack (
                        [ h4 {
                              attr.style "font-weight: bold; margin: 0; font-family: var(--rz-font-family);"
                              "Tentative Reservations Allocated"
                          }
                          div {
                              attr.style
                                  "font-family: monospace; font-size: 11px; max-height: 120px; overflow-y: auto; display: flex; flex-direction: column; gap: 4px;"

                              for res in resp.Reservations do
                                  div {
                                      attr.style
                                          "background-color: rgba(255,255,255,0.01); padding: 4px 8px; border-radius: 4px; border: 1px solid rgba(255,255,255,0.03);"

                                      res
                                  }
                          } ],
                        gap = "12px"
                    )
                }
        }

    let render (model: Model) (dispatch: Msg -> unit) =
        div {
            attr.``class`` "p-4"

            h3 {
                attr.``class`` "rz-text-h4 rz-mb-4"
                "Promise Workbench"
            }

            div {
                attr.style "display: grid; grid-template-columns: 1fr 1fr; gap: 24px; align-items: start;"

                // Left column: promising form card
                comp<RadzenCard> {
                    "Style"
                    => "padding: 24px; border-radius: 8px; border: 1px solid var(--rz-border-color);"

                    Rz.stack (
                        [ h4 {
                              attr.style "font-weight: bold; margin-bottom: 8px; font-family: var(--rz-font-family);"
                              "Order Promising Simulator"
                          }

                          // Sku ID textbox
                          div {
                              Rz.label (
                                  "SKU ID",
                                  style = "font-size: 12px; font-weight: bold; display: block; margin-bottom: 4px;"
                              )

                              input {
                                  attr.``class`` "rz-textbox"
                                  attr.style "width: 100%;"
                                  attr.value model.Input.SkuId
                                  on.change (fun (e: ChangeEventArgs) -> dispatch (UpdateSkuId(string e.Value)))
                              }
                          }

                          // Stocking Point ID textbox
                          div {
                              Rz.label (
                                  "Stocking Point ID (Location)",
                                  style = "font-size: 12px; font-weight: bold; display: block; margin-bottom: 4px;"
                              )

                              input {
                                  attr.``class`` "rz-textbox"
                                  attr.style "width: 100%;"
                                  attr.value model.Input.StockingPointId

                                  on.change (fun (e: ChangeEventArgs) ->
                                      dispatch (UpdateStockingPointId(string e.Value)))
                              }
                          }

                          // Quantity input
                          div {
                              Rz.label (
                                  "Required Quantity",
                                  style = "font-size: 12px; font-weight: bold; display: block; margin-bottom: 4px;"
                              )

                              input {
                                  attr.``class`` "rz-textbox"
                                  attr.``type`` "number"
                                  attr.style "width: 100%;"
                                  attr.value (string model.Input.Quantity)

                                  on.change (fun (e: ChangeEventArgs) ->
                                      dispatch (UpdateQuantity(Decimal.Parse(string e.Value))))
                              }
                          }

                          // Due Date Picker
                          div {
                              Rz.label (
                                  "Requested Due Date",
                                  style = "font-size: 12px; font-weight: bold; display: block; margin-bottom: 4px;"
                              )

                              comp<RadzenDatePicker<DateTimeOffset>> {
                                  "Value" => model.Input.DueDate
                                  "Style" => "width: 100%;"
                                  "DateFormat" => "yyyy-MM-dd"

                                  attr.callback "Change" (fun (d: System.Nullable<DateTime>) ->
                                      let date =
                                          if d.HasValue then
                                              DateTimeOffset(d.Value)
                                          else
                                              DateTimeOffset.Now.AddDays(7.0)

                                      dispatch (UpdateDueDate date))
                              }
                          }

                          // Customer Tier dropdown
                          div {
                              Rz.label (
                                  "Customer Service Tier",
                                  style = "font-size: 12px; font-weight: bold; display: block; margin-bottom: 4px;"
                              )

                              Rz.dropDown (
                                  data = [ "gold"; "silver"; "bronze" ],
                                  value = model.Input.CustomerTier,
                                  style = "width: 100%;",
                                  onChange = (fun (s: obj) -> dispatch (UpdateCustomerTier (string s)))
                              )
                          }

                          // Sku Tier dropdown
                          div {
                              Rz.label (
                                  "SKU Priority Tier",
                                  style = "font-size: 12px; font-weight: bold; display: block; margin-bottom: 4px;"
                              )

                              Rz.dropDown (
                                  data = [ "tier-1"; "tier-2"; "tier-3" ],
                                  value = model.Input.SkuTier,
                                  style = "width: 100%;",
                                  onChange = (fun (s: obj) -> dispatch (UpdateSkuTier (string s)))
                              )
                          }

                          // Currency dropdown
                          div {
                              Rz.label (
                                  "Preferred Currency",
                                  style = "font-size: 12px; font-weight: bold; display: block; margin-bottom: 4px;"
                              )

                              Rz.dropDown (
                                  data = [ "USD"; "EUR"; "GBP"; "INR" ],
                                  value = model.Input.Currency,
                                  style = "width: 100%;",
                                  onChange = (fun (s: obj) -> dispatch (UpdateCurrency (string s)))
                              )
                          }

                          // Action buttons
                          Rz.stack (
                              [ Rz.button (
                                    text = "Evaluate ATP/CTP",
                                    icon = "flash_on",
                                    style = ButtonStyle.Primary,
                                    onClick = (fun _ -> dispatch TriggerEvaluation)
                                )
                                Rz.button (
                                    text = "Reset",
                                    style = ButtonStyle.Secondary,
                                    onClick = (fun _ -> dispatch ResetInput)
                                ) ],
                              orientation = Orientation.Horizontal,
                              gap = "12px",
                              class' = "rz-mt-4"
                          ) ],
                        gap = "16px"
                    )
                }

                // Right column: detailed results panel
                div {
                    comp<RemoteState<PromiseEvaluationResponse>> {
                        "Data" => model.EvaluationResult

                        "EmptyMessage"
                        => "Provide simulator parameters on the left and trigger the promising evaluation."

                        "Template" => (fun res -> renderResult res)
                    }
                }
            }
        }

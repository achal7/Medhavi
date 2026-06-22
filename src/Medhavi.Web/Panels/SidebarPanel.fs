namespace Medhavi.Web.Panels

open System
open Microsoft.AspNetCore.Components.Web
open Bolero
open Bolero.Html
open Medhavi.Web
open Medhavi.Web.Controls
open Radzen
open Radzen.Blazor

type SidebarPanel =
    static member private activityLogTab(history: CommandTrace list) : Node =
        div {
            attr.``class`` "sidebar-tab-content"

            if history.IsEmpty then
                div {
                    attr.``class`` "rz-text-align-center rz-p-8 rz-text-secondary"
                    Rz.icon("history", style = "font-size: 48px; opacity: 0.5; margin-bottom: 8px;")

                    p {
                        attr.``class`` "rz-m-0 rz-text-body2"
                        attr.style "color: var(--rz-text-secondary-color);"
                        "No actions executed yet."
                    }
                }
            else
                Rz.stack(
                    [ for trace in history ->
                          comp<RadzenCard> {
                              attr.``class`` "rz-p-3 rz-mb-2"

                              div {
                                  attr.``class``
                                      "rz-display-flex rz-align-items-center rz-justify-content-between rz-mb-2"

                                  Rz.stack(
                                      [ let iconName, iconColor =
                                            match trace.Origin with
                                            | CommandOrigin.Human _ -> "person", "var(--rz-primary-color)"
                                            | CommandOrigin.Ai -> "smart_toy", "var(--rz-success-color)"
                                            | CommandOrigin.System -> "settings", "var(--rz-text-secondary-color)"

                                        Rz.icon(iconName, style = sprintf "font-size: 16px; color: %s;" iconColor)

                                        span {
                                            attr.``class`` "rz-text-caption rz-font-weight-bold rz-text-secondary"
                                            attr.style "text-transform: uppercase;"

                                            match trace.Origin with
                                            | CommandOrigin.Human name -> name
                                            | CommandOrigin.Ai -> "AI"
                                            | CommandOrigin.System -> "SYSTEM"
                                        } ],
                                      orientation = Orientation.Horizontal,
                                      alignItems = AlignItems.Center,
                                      gap = "4px"
                                  )

                                  span {
                                      attr.``class`` "rz-text-caption rz-text-secondary"
                                      trace.TimestampUtc.ToLocalTime().ToString("HH:mm:ss")
                                  }
                              }

                              div {
                                  attr.``class`` "rz-text-subtitle2 rz-font-weight-bold"
                                  trace.ActionText
                              }

                              if not(String.IsNullOrEmpty trace.RawText) then
                                  div {
                                      attr.``class`` "rz-p-1 rz-background-color-background"

                                      attr.style
                                          "font-family: monospace; font-size: 11px; border-radius: 4px; color: var(--rz-text-secondary-color); word-break: break-all;"

                                      trace.RawText
                                  }

                              div {
                                  attr.``class``
                                      "rz-display-flex rz-align-items-center rz-justify-content-between rz-mt-2"

                                  let statusText, badgeStyle =
                                      match trace.Status with
                                      | CommandStatus.Queued -> "Queued", BadgeStyle.Warning
                                      | CommandStatus.Succeeded -> "Succeeded", BadgeStyle.Success
                                      | CommandStatus.Failed -> "Failed", BadgeStyle.Danger

                                  comp<RadzenBadge> {
                                      "Text" => statusText
                                      "BadgeStyle" => badgeStyle
                                  }

                                  match trace.Notes with
                                  | Some notes ->
                                      span {
                                          attr.``class`` "rz-text-caption rz-text-danger"
                                          attr.style "word-break: break-all; max-width: 180px; text-align: right;"
                                          notes
                                      }
                                  | None -> empty()
                              }
                          } ],
                    orientation = Orientation.Vertical,
                    gap = "10px"
                )
        }

    static member private notificationsTab
        (notifications: Notification list)
        (onClear: unit -> unit)
        (onMarkAllRead: unit -> unit)
        : Node =
        div {
            attr.``class`` "sidebar-tab-content"

            Rz.stack(
                [ if not notifications.IsEmpty then
                      yield
                          Rz.stack(
                              [ Rz.actionButton("done_all", "Mark all read", (fun _ -> onMarkAllRead()))
                                Rz.actionButton("delete_sweep", "Clear all", (fun _ -> onClear())) ],
                              orientation = Orientation.Horizontal,
                              gap = "8px",
                              style = "margin-bottom: 8px;"
                          )

                  if notifications.IsEmpty then
                      yield
                          div {
                              attr.``class`` "rz-text-align-center rz-p-8 rz-text-secondary"
                              Rz.icon("notifications_off", style = "font-size: 48px; opacity: 0.5; margin-bottom: 8px;")

                              p {
                                  attr.``class`` "rz-m-0 rz-text-body2"
                                  attr.style "color: var(--rz-text-secondary-color);"
                                  "No notifications."
                              }
                          }
                  else
                      for n in notifications do
                          yield
                              comp<RadzenCard> {
                                  attr.``class`` "rz-p-3 rz-mb-2"

                                  attr.style(
                                      sprintf
                                          "border-left: 3px solid %s;"
                                          (if n.IsRead then "var(--rz-border-color)" else "var(--rz-primary-color)")
                                  )

                                  div {
                                      attr.``class``
                                          "rz-display-flex rz-justify-content-between rz-align-items-center rz-mb-1"

                                      Rz.stack(
                                          [ comp<RadzenBadge> {
                                                "Text" => n.Category
                                                "BadgeStyle" => BadgeStyle.Info
                                                "IsPill" => true
                                            }
                                            span {
                                                attr.``class`` "rz-text-subtitle2 rz-font-weight-bold"
                                                n.Title
                                            } ],
                                          orientation = Orientation.Horizontal,
                                          alignItems = AlignItems.Center,
                                          gap = "6px"
                                      )

                                      span {
                                          attr.``class`` "rz-text-caption rz-text-secondary"
                                          n.Timestamp.ToString("HH:mm")
                                      }
                                  }

                                  div {
                                      attr.``class`` "rz-text-body2 rz-text-secondary"
                                      n.Message
                                  }
                              } ],
                orientation = Orientation.Vertical,
                gap = "10px"
            )
        }

    static member private operationsTab (operations: Operation list) (onDismiss: Guid -> unit) : Node =
        div {
            attr.``class`` "sidebar-tab-content"

            if operations.IsEmpty then
                div {
                    attr.``class`` "rz-text-align-center rz-p-8 rz-text-secondary"
                    Rz.icon("task_alt", style = "font-size: 48px; opacity: 0.5; margin-bottom: 8px;")

                    p {
                        attr.``class`` "rz-m-0 rz-text-body2"
                        attr.style "color: var(--rz-text-secondary-color);"
                        "No active operations."
                    }
                }
            else
                Rz.stack(
                    [ for op in operations ->
                          comp<RadzenCard> {
                              attr.``class`` "rz-p-3 rz-mb-2"

                              div {
                                  attr.``class``
                                      "rz-display-flex rz-justify-content-between rz-align-items-center rz-mb-2"

                                  span {
                                      attr.``class`` "rz-text-subtitle2 rz-font-weight-bold"
                                      op.Name
                                  }

                                  match op.State with
                                  | OperationState.Completed _ ->
                                      Rz.actionButton(
                                          "close",
                                          "Dismiss",
                                          (fun _ -> onDismiss op.Id),
                                          class' = "dismiss-btn"
                                      )
                                  | OperationState.Failed _ ->
                                      Rz.actionButton(
                                          "close",
                                          "Dismiss",
                                          (fun _ -> onDismiss op.Id),
                                          class' = "dismiss-btn"
                                      )
                                  | _ -> empty()
                              }

                              match op.State with
                              | OperationState.Pending -> Rz.progressBar(0.0, mode = ProgressBarMode.Indeterminate)
                              | OperationState.Running(progress, stage) ->
                                  Rz.stack(
                                      [ Rz.progressBar(double progress)
                                        div {
                                            attr.``class``
                                                "rz-display-flex rz-justify-content-between rz-text-caption rz-text-secondary rz-mt-1"

                                            span { stage }
                                            span { sprintf "%d%%" progress }
                                        } ],
                                      orientation = Orientation.Vertical
                                  )
                              | OperationState.Completed _ ->
                                  Rz.stack(
                                      [ Rz.progressBar(100.0)
                                        div {
                                            attr.``class`` "rz-text-caption rz-text-success rz-font-weight-bold rz-mt-1"
                                            "Finished successfully"
                                        } ],
                                      orientation = Orientation.Vertical
                                  )
                              | OperationState.Failed err ->
                                  Rz.stack(
                                      [ Rz.progressBar(100.0, class' = "progress-failed")
                                        div {
                                            attr.``class`` "rz-text-caption rz-text-danger rz-font-weight-bold rz-mt-1"
                                            attr.style "word-break: break-all;"
                                            sprintf "Failed: %s" err
                                        } ],
                                      orientation = Orientation.Vertical
                                  )
                              | OperationState.Cancelled ->
                                  div {
                                      attr.``class`` "rz-text-caption rz-text-secondary rz-font-weight-bold"
                                      "Operation cancelled"
                                  }
                          } ],
                    orientation = Orientation.Vertical,
                    gap = "10px"
                )
        }

    static member view2
        (
            activeTab: int,
            notifications: Notification list,
            operations: Operation list,
            commandHistory: CommandTrace list,
            onClearNotifications: unit -> unit,
            onMarkAllRead: unit -> unit,
            onDismissOperation: Guid -> unit,
            onClose: unit -> unit
        ) : Node =

        div {
            attr.``class`` "rz-display-flex rz-flex-column"

            attr.style
                "height: 100%; width: 100%; background-color: var(--rz-background-color, #131a22); color: var(--rz-text-color); font-family: var(--rz-font-family);"

            // Sidebar Header
            div {
                attr.style
                    "display: flex; align-items: center; justify-content: space-between; padding: 16px; border-bottom: 1px solid var(--rz-border-color); background-color: rgba(255,255,255,0.02);"

                let title, icon =
                    match activeTab with
                    | 0 -> "Command History", "history"
                    | 1 -> "System Notifications", "notifications"
                    | 2 -> "Active Operations", "build"
                    | _ -> "System Control Center", "settings"

                div {
                    attr.style "display: flex; align-items: center; gap: 8px;"
                    Rz.icon(icon, style = "font-size: 20px; color: var(--rz-primary-color);")

                    h3 {
                        attr.``class`` "rz-m-0 rz-text-h6 rz-font-weight-bold"
                        attr.style "font-size: 15px; color: var(--rz-text-color);"
                        title
                    }
                }

                comp<RadzenButton> {
                    "Icon" => "close"
                    "ButtonStyle" => ButtonStyle.Light
                    "Size" => ButtonSize.ExtraSmall
                    attr.callback "Click" (fun (args: MouseEventArgs) -> onClose())
                }
            }

            // Scrollable Content
            div {
                attr.``class`` "rz-flex-1 rz-overflow-y-auto"
                attr.style "padding: 16px;"

                let content =
                    match activeTab with
                    | 0 -> SidebarPanel.activityLogTab commandHistory
                    | 1 -> SidebarPanel.notificationsTab notifications onClearNotifications onMarkAllRead
                    | 2 -> SidebarPanel.operationsTab operations onDismissOperation
                    | _ -> div { "No content" }

                content
            }
        }

    static member view
        (
            activeTab: int,
            notifications: Notification list,
            operations: Operation list,
            commandHistory: CommandTrace list,
            onClearNotifications: unit -> unit,
            onMarkAllRead: unit -> unit,
            onDismissOperation: Guid -> unit,
            onClose: unit -> unit
        ) : Node =
        let title, icon, content =
            match activeTab with
            | 0 -> "Command History", "history", SidebarPanel.activityLogTab commandHistory
            | 1 ->
                "System Notifications",
                "notifications",
                SidebarPanel.notificationsTab notifications onClearNotifications onMarkAllRead
            | 2 -> "Active Operations", "build", SidebarPanel.operationsTab operations onDismissOperation
            | _ -> "System Control Center", "settings", div { "No content" }

        comp<RadzenPanelMenu> {

            div {
                h3 {
                    attr.``class`` "rz-m-0 rz-text-h6 rz-font-weight-bold"
                    attr.style "font-size: 15px; color: var(--rz-text-color);"
                    title
                }

                content
            }
        }

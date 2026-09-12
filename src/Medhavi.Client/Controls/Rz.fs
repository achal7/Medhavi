namespace Medhavi.Client.Controls

open System
open System.Threading.Tasks
open Microsoft.AspNetCore.Components
open Microsoft.AspNetCore.Components.Web
open Bolero
open Bolero.Html
open Radzen
open Radzen.Blazor


/// Configuration for a dropdown.
type DropDownConfig<'T when 'T : not null> =
    {   Items: 'T list
        Selected: 'T
        SelectedChanged: 'T -> unit
        Display: 'T -> string
        Placeholder: string option
        Disabled: bool option
        Class: string option }

/// Configuration for a tab.
type TabConfig =
    {   Title: string
        Content: Node
        Disabled: bool option }

/// Configuration for a dialog.
type DialogConfig =
    {   Title: string
        Width: string option
        ShowClose: bool option }

/// Configuration for a notification.
type NotificationConfig =
    {   Severity: NotificationSeverity
        Summary: string
        Detail: string option
        Duration: int option }

type Rz =

    // =========================================================================
    // Section 1: Buttons & Actions
    // =========================================================================

    /// Creates a button with optional parameters.
    /// 
    /// Example:
    /// <code>
    /// Rz.button("Save", onClick = handleSave, style = ButtonStyle.Primary, icon = "save")
    /// </code>
    static member button
        (
            text: string,
            ?onClick: MouseEventArgs -> unit,
            ?style: ButtonStyle,
            ?icon: string,
            ?isBusy: bool,
            ?disabled: bool,
            ?class': string
        ) : Node =
        comp<RadzenButton> {
            "Text" => text
            if onClick.IsSome then attr.callback "Click" onClick.Value else attr.empty()
            if style.IsSome then "ButtonStyle" => style.Value else attr.empty()
            if icon.IsSome then "Icon" => icon.Value else attr.empty()
            if isBusy.IsSome then "IsBusy" => isBusy.Value else attr.empty()
            if disabled.IsSome then "Disabled" => disabled.Value else attr.empty()
            if class'.IsSome then attr.``class`` class'.Value else attr.empty()
        }

    /// Convenience: Primary button (most common action).
    static member primaryButton (text: string, onClick: MouseEventArgs -> unit) : Node =
        Rz.button(text, onClick = onClick, style = ButtonStyle.Primary)

    /// Convenience: Secondary button (less important actions).
    static member secondaryButton (text: string, onClick: MouseEventArgs -> unit) : Node =
        Rz.button(text, onClick = onClick, style = ButtonStyle.Secondary)

    /// Convenience: Light button (tertiary actions).
    static member lightButton (text: string, onClick: MouseEventArgs -> unit) : Node =
        Rz.button(text, onClick = onClick, style = ButtonStyle.Light)

    /// Convenience: Icon-only button (toolbar buttons, action buttons).
    static member iconButton
        (
            icon: string,
            onClick: MouseEventArgs -> unit,
            ?tooltip: string,
            ?style: ButtonStyle,
            ?disabled: bool
        ) : Node =
        Rz.button(
            "",
            onClick = onClick,
            icon = icon,
            style = defaultArg style ButtonStyle.Light,
            class' = "rz-button-icon-only",
            disabled = (disabled |> Option.defaultValue false)
        )

    // =========================================================================
    // Section 2: Typography & Icons
    // =========================================================================

    /// Creates a text element with optional styling.
    /// 
    /// Example:
    /// <code>
    /// Rz.text("Hello World", style = TextStyle.H5, tagName = TagName.H2)
    /// </code>
    static member text
        (
            content: string,
            ?textStyle: TextStyle,
            ?tagName: TagName,
            ?class': string
        ) : Node =
        comp<RadzenText> {
            "Text" => content
            if textStyle.IsSome then "TextStyle" => textStyle.Value else attr.empty()
            if tagName.IsSome then "TagName" => tagName.Value else attr.empty()
            if class'.IsSome then attr.``class`` class'.Value else attr.empty()
        }

    /// Convenience: Heading levels (H1-H6).
    static member heading (level: int, content: string, ?class': string) : Node =
        let textStyle =
            match level with
            | 1 -> TextStyle.H1
            | 2 -> TextStyle.H2
            | 3 -> TextStyle.H3
            | 4 -> TextStyle.H4
            | 5 -> TextStyle.H5
            | 6 -> TextStyle.H6
            | _ -> TextStyle.H3
        if class'.IsSome then
            Rz.text(content, textStyle = textStyle, class' = class'.Value)
        else
            Rz.text(content, textStyle = textStyle)

    /// Creates a Material icon.
    /// 
    /// Example:
    /// <code>
    /// Rz.icon("home", class' = "rz-text-primary")
    /// </code>
    static member icon (name: string, ?class': string, ?style: string) : Node =
        comp<RadzenIcon> {
            "Icon" => name
            if class'.IsSome then attr.``class`` class'.Value else attr.empty()
            if style.IsSome then "Style" => style.Value else attr.empty()
        }

    // =========================================================================
    // Section 3: Feedback (Alerts, Progress)
    // =========================================================================

    /// Creates an alert with optional styling.
    /// 
    /// Example:
    /// <code>
    /// Rz.alert("Operation failed", style = AlertStyle.Danger, variant = Variant.Filled)
    /// </code>
    static member alert
        (
            message: string,
            ?alertStyle: AlertStyle,
            ?variant: Variant,
            ?shade: Shade,
            ?icon: string,
            ?class': string
        ) : Node =
        comp<RadzenAlert> {
            "Text" => message
            if alertStyle.IsSome then "AlertStyle" => alertStyle.Value else attr.empty()
            if variant.IsSome then "Variant" => variant.Value else attr.empty()
            if shade.IsSome then "Shade" => shade.Value else attr.empty()
            if icon.IsSome then "Icon" => icon.Value else attr.empty()
            if class'.IsSome then attr.``class`` class'.Value else attr.empty()
        }

    /// Convenience: Success alert.
    static member successAlert (message: string) : Node =
        Rz.alert(message, alertStyle = AlertStyle.Success, icon = "check_circle")

    /// Convenience: Error alert.
    static member errorAlert (message: string) : Node =
        Rz.alert(message, alertStyle = AlertStyle.Danger, icon = "error")

    /// Convenience: Warning alert.
    static member warningAlert (message: string) : Node =
        Rz.alert(message, alertStyle = AlertStyle.Warning, icon = "warning")

    /// Convenience: Info alert.
    static member infoAlert (message: string) : Node =
        Rz.alert(message, alertStyle = AlertStyle.Info, icon = "info")

    /// Creates a progress bar.
    /// 
    /// Example:
    /// <code>
    /// Rz.progressBar(75.0, mode = ProgressBarMode.Determinate)
    /// </code>
    static member progressBar
        (
            value: double,
            ?mode: ProgressBarMode,
            ?showValue: bool,
            ?class': string
        ) : Node =
        comp<RadzenProgressBar> {
            "Value" => value
            if mode.IsSome then "Mode" => mode.Value else attr.empty()
            if showValue.IsSome then "ShowValue" => showValue.Value else attr.empty()
            if class'.IsSome then attr.``class`` class'.Value else attr.empty()
        }

    /// Convenience: Indeterminate progress bar (loading spinner).
    static member loadingProgress () : Node =
        Rz.progressBar(100.0, mode = ProgressBarMode.Indeterminate)

    // =========================================================================
    // Section 4: Basic Inputs (Two-Way Binding)
    // =========================================================================

    /// Creates a text input with two-way binding.
    /// 
    /// Example:
    /// <code>
    /// Rz.textBox(model.Name, valueChanged = fun v -> dispatch (SetName v), placeholder = "Enter name")
    /// </code>
    static member textBox
        (
            value: string,
            valueChanged: string -> unit,
            ?placeholder: string,
            ?disabled: bool,
            ?class': string
        ) : Node =
        comp<RadzenTextBox> {
            "Value" => value
            attr.callback "ValueChanged" valueChanged
            if placeholder.IsSome then "Placeholder" => placeholder.Value else attr.empty()
            if disabled.IsSome then "Disabled" => disabled.Value else attr.empty()
            if class'.IsSome then attr.``class`` class'.Value else attr.empty()
        }

    /// Creates a password input with two-way binding.
    static member password
        (
            value: string,
            valueChanged: string -> unit,
            ?placeholder: string,
            ?disabled: bool,
            ?class': string
        ) : Node =
        comp<RadzenPassword> {
            "Value" => value
            attr.callback "ValueChanged" valueChanged
            if placeholder.IsSome then "Placeholder" => placeholder.Value else attr.empty()
            if disabled.IsSome then "Disabled" => disabled.Value else attr.empty()
            if class'.IsSome then attr.``class`` class'.Value else attr.empty()
        }

    /// Creates a numeric input (int) with two-way binding.
    /// 
    /// Example:
    /// <code>
    /// Rz.numeric(model.Quantity, valueChanged = fun v -> dispatch (SetQuantity v), min = 0, max = 100)
    /// </code>
    static member numeric
        (
            value: int,
            valueChanged: int -> unit,
            ?min: int,
            ?max: int,
            ?step: int,
            ?disabled: bool,
            ?class': string
        ) : Node =
        comp<RadzenNumeric<int>> {
            "Value" => value
            attr.callback "ValueChanged" valueChanged
            if min.IsSome then "Min" => min.Value else attr.empty()
            if max.IsSome then "Max" => max.Value else attr.empty()
            if step.IsSome then "Step" => step.Value else attr.empty()
            if disabled.IsSome then "Disabled" => disabled.Value else attr.empty()
            if class'.IsSome then attr.``class`` class'.Value else attr.empty()
        }

    /// Creates a date picker with two-way binding.
    static member datePicker
        (
            value: DateTime,
            valueChanged: DateTime -> unit,
            ?min: DateTime,
            ?max: DateTime,
            ?disabled: bool,
            ?class': string
        ) : Node =
        comp<RadzenDatePicker<DateTime>> {
            "Value" => value
            attr.callback "ValueChanged" valueChanged
            if min.IsSome then "Min" => min.Value else attr.empty()
            if max.IsSome then "Max" => max.Value else attr.empty()
            if disabled.IsSome then "Disabled" => disabled.Value else attr.empty()
            if class'.IsSome then attr.``class`` class'.Value else attr.empty()
        }

    /// Creates a checkbox with two-way binding.
    static member checkBox
        (
            value: bool,
            valueChanged: bool -> unit,
            ?text: string,
            ?disabled: bool,
            ?class': string
        ) : Node =
        comp<RadzenCheckBox<bool>> {
            "Value" => value
            attr.callback "ValueChanged" valueChanged
            if text.IsSome then "Text" => text.Value else attr.empty()
            if disabled.IsSome then "Disabled" => disabled.Value else attr.empty()
            if class'.IsSome then attr.``class`` class'.Value else attr.empty()
        }

    // =========================================================================
    // Section 5: Dropdowns (Generic)
    // =========================================================================


    /// Creates a generic dropdown with type-safe selection.
    /// 
    /// Example:
    /// <code>
    /// static member config = {
    ///     Items = [OptionA; OptionB; OptionC]
    ///     Selected = model.SelectedOption
    ///     SelectedChanged = fun v -> dispatch (SetOption v)
    ///     Display = fun opt -> opt.ToString()
    ///     Placeholder = Some "Select an option"
    ///     Disabled = None
    ///     Class = None
    /// }
    /// Rz.dropDown(config)
    /// </code>
    static member dropDown<'T when 'T : not null> (config: DropDownConfig<'T>) : Node =
        let data = config.Items |> List.map config.Display
        let value = config.Display config.Selected

        comp<RadzenDropDown<string>> {
            "Data" => data
            "Value" => value
            if config.Placeholder.IsSome then "Placeholder" => config.Placeholder.Value else attr.empty()
            if config.Disabled.IsSome then "Disabled" => config.Disabled.Value else attr.empty()
            if config.Class.IsSome then attr.``class`` config.Class.Value else attr.empty()
            attr.callback "Change" (fun (obj: obj) ->
                let selectedStr = obj :?> string
                match config.Items |> List.tryFind (fun x -> config.Display x = selectedStr) with
                | Some item -> config.SelectedChanged item
                | None -> ())
        }

    /// Convenience: String dropdown (simplest case).
    static member stringDropDown
        (
            items: string list,
            selected: string,
            selectedChanged: string -> unit,
            ?placeholder: string,
            ?disabled: bool,
            ?class': string
        ) : Node =
        Rz.dropDown<string> {
            Items = items
            Selected = selected
            SelectedChanged = selectedChanged
            Display = id
            Placeholder = placeholder
            Disabled = disabled
            Class = class'
        }

    // =========================================================================
    // Section 6: Layout (Splitter, Tabs, Panel)
    // =========================================================================

    /// Creates a splitter layout with two panes.
    /// 
    /// Example:
    /// <code>
    /// Rz.splitter(
    ///     orientation = Orientation.Horizontal,
    ///     pane1 = Rz.pane(leftContent, size = "60%"),
    ///     pane2 = Rz.pane(rightContent, size = "40%")
    /// )
    /// </code>
    static member splitter
        (
            orientation: Orientation,
            pane1: Node,
            pane2: Node,
            ?class': string
        ) : Node =
        comp<RadzenSplitter> {
            "Orientation" => orientation
            if class'.IsSome then attr.``class`` class'.Value else attr.empty()
            pane1
            pane2
        }

    /// Creates a splitter pane with optional size.
    static member pane
        (
            content: Node,
            ?size: string,
            ?resizable: bool,
            ?collapsible: bool
        ) : Node =
        comp<RadzenSplitterPane> {
            if size.IsSome then "Size" => size.Value else attr.empty()
            if resizable.IsSome then "Resizable" => resizable.Value else attr.empty()
            if collapsible.IsSome then "Collapsible" => collapsible.Value else attr.empty()
            content
        }

    /// Creates a tabbed interface.
    /// 
    /// Example:
    /// <code>
    /// static member tabs = [
    ///     { Title = "Overview"; Content = overviewContent; Disabled = None }
    ///     { Title = "Details"; Content = detailsContent; Disabled = None }
    /// ]
    /// Rz.tabs(tabs, selectedIndex = 0, selectedIndexChanged = fun i -> dispatch (SelectTab i))
    /// </code>
    static member tabs
        (
            tabs: TabConfig list,
            ?selectedIndex: int,
            ?selectedIndexChanged: int -> unit,
            ?class': string
        ) : Node =
        comp<RadzenTabs> {
            if selectedIndex.IsSome then "SelectedIndex" => selectedIndex.Value else attr.empty()
            if selectedIndexChanged.IsSome then attr.callback "SelectedIndexChanged" selectedIndexChanged.Value else attr.empty()
            if class'.IsSome then attr.``class`` class'.Value else attr.empty()
            attr.fragment "Tabs" (
                forEach tabs (fun tab ->
                    comp<RadzenTabsItem> {
                        "Text" => tab.Title
                        if tab.Disabled.IsSome then "Disabled" => tab.Disabled.Value else attr.empty()
                        tab.Content
                    })
            )
        }

    /// Creates a panel container with optional title.
    /// 
    /// Example:
    /// <code>
    /// Rz.panel("Settings", [
    ///     Rz.textBox(model.Name, valueChanged = fun v -> dispatch (SetName v))
    ///     Rz.button("Save", onClick = handleSave)
    /// ])
    /// </code>
    static member panel
        (
            ?title: string,
            ?children: Node list,
            ?class': string
        ) : Node =
        comp<RadzenPanel> {
            if title.IsSome then "Text" => title.Value else attr.empty()
            if class'.IsSome then attr.``class`` class'.Value else attr.empty()
            if children.IsSome then
                attr.fragment "ChildContent" (forEach children.Value id)
            else attr.empty()
        }

    /// Creates a card container.
    static member card (children: Node list, ?class': string) : Node =
        comp<RadzenCard> {
            if class'.IsSome then attr.``class`` class'.Value else attr.empty()
            for child in children do child
        }

    // =========================================================================
    // Section 7: Dialog & Notification Services
    // =========================================================================


    /// Shows a modal dialog (functional wrapper around DialogService).
    /// 
    /// Example:
    /// <code>
    /// static member config = {
    ///     Title = "Confirm Delete"
    ///     Width = Some "400px"
    ///     ShowClose = Some true
    /// }
    /// do! Rz.showDialog(dialogService, config, confirmDialogContent)
    /// </code>
    static member showDialog
        (
            dialogService: DialogService,
            config: DialogConfig,
            content: Node
        ) : Task =
        task {
            let options = DialogOptions(
                Width = defaultArg config.Width "500px",
                ShowClose = defaultArg config.ShowClose true
            )
            let childContent = RenderFragment<DialogService>(fun _ ->
                RenderFragment(fun builder ->
                    content.Invoke(null, builder, 0) |> ignore
                )
            )
            let! _ = dialogService.OpenAsync(config.Title, childContent, options)
            return ()
        }


    /// Shows a notification (functional wrapper around NotificationService).
    /// 
    /// Example:
    /// <code>
    /// static member config = {
    ///     Severity = NotificationSeverity.Success
    ///     Summary = "Saved successfully"
    ///     Detail = None
    ///     Duration = Some 3000
    /// }
    /// Rz.notify(notificationService, config)
    /// </code>
    static member notify (notificationService: NotificationService, config: NotificationConfig) : unit =
        let msg = NotificationMessage(
            Severity = config.Severity,
            Summary = config.Summary,
            Detail = defaultArg config.Detail "",
            Duration = Nullable(float (defaultArg config.Duration 4000))
        )
        notificationService.Notify(msg)

    /// Convenience: Success notification.
    static member notifySuccess (notificationService: NotificationService, summary: string, ?duration: int) : unit =
        Rz.notify(notificationService, {
            Severity = NotificationSeverity.Success
            Summary = summary
            Detail = None
            Duration = duration
        })

    /// Convenience: Error notification.
    static member notifyError (notificationService: NotificationService, summary: string, ?duration: int) : unit =
        Rz.notify(notificationService, {
            Severity = NotificationSeverity.Error
            Summary = summary
            Detail = None
            Duration = duration
        })

    /// Convenience: Warning notification.
    static member notifyWarning (notificationService: NotificationService, summary: string, ?duration: int) : unit =
        Rz.notify(notificationService, {
            Severity = NotificationSeverity.Warning
            Summary = summary
            Detail = None
            Duration = duration
        })

    /// Convenience: Info notification.
    static member notifyInfo (notificationService: NotificationService, summary: string, ?duration: int) : unit =
        Rz.notify(notificationService, {
            Severity = NotificationSeverity.Info
            Summary = summary
            Detail = None
            Duration = duration
        })

    // =========================================================================
    // Section 9: Helpers
    // =========================================================================

    /// Converts a list of nodes into a single fragment node.
    static member fragment (nodes: Node list) : Node =
        forEach nodes id

    /// Creates a render fragment from a list of nodes.
    static member toRenderFragment (nodes: Node list) : RenderFragment =
        RenderFragment(fun builder ->
            for node in nodes do
                node.Invoke(null, builder, 0) |> ignore
        )

    /// Applies CSS classes conditionally.
    static member classes (classMap: (string * bool) list) : string =
        classMap
        |> List.choose (fun (cls, cond) -> if cond then Some cls else None)
        |> String.concat " "
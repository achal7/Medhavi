// Rz.fs - Type-safe Radzen Blazor components for Bolero
namespace Medhavi.Web.Controls

open System
open Microsoft.AspNetCore.Components
open Microsoft.AspNetCore.Components.Web
open Bolero
open Bolero.Html
open Radzen
open Radzen.Blazor

type Rz =

    // ========================================
    // Helpers
    // ========================================

    /// Converts a list of nodes into a single fragment node.
    static member private fragment(nodes: Node list) : Node = forEach nodes id

    /// For components that require a named render fragment.
    static member private namedFragment name (nodes: Node list) : Attr = attr.fragment name (forEach nodes id)

    // ========================================
    // Basic Inputs (with two-way binding support)
    // ========================================

    /// Text box with two-way binding.
    static member textBox
        (
            value: string,
            valueChanged: string -> unit,
            ?placeholder: string,
            ?disabled: bool,
            ?style: string,
            ?class': string
        ) : Node =
        comp<RadzenTextBox> {
            "Value" => value
            attr.callback "ValueChanged" valueChanged

            if placeholder.IsSome then "Placeholder" => placeholder.Value else attr.empty()
            if disabled.IsSome then "Disabled" => disabled.Value else attr.empty()
            if style.IsSome then "Style" => style.Value else attr.empty()
            if class'.IsSome then attr.``class`` class'.Value else attr.empty()
        }

    /// Password input with two-way binding.
    static member password
        (
            value: string,
            valueChanged: string -> unit,
            ?placeholder: string,
            ?disabled: bool,
            ?style: string,
            ?class': string
        ) : Node =
        comp<RadzenPassword> {
            "Value" => value
            attr.callback "ValueChanged" valueChanged

            if placeholder.IsSome then "Placeholder" => placeholder.Value else attr.empty()
            if disabled.IsSome then "Disabled" => disabled.Value else attr.empty()
            if style.IsSome then "Style" => style.Value else attr.empty()
            if class'.IsSome then attr.``class`` class'.Value else attr.empty()
        }

    /// Numeric input (int).
    static member numeric(value: int, valueChanged: int -> unit, ?min: int, ?max: int, ?step: int, ?disabled: bool) : Node =
        comp<RadzenNumeric<int>> {
            "Value" => value
            attr.callback "ValueChanged" valueChanged

            if min.IsSome then "Min" => min.Value else attr.empty()
            if max.IsSome then "Max" => max.Value else attr.empty()
            if step.IsSome then "Step" => step.Value else attr.empty()
            if disabled.IsSome then "Disabled" => disabled.Value else attr.empty()
        }

    /// Date picker.
    static member datePicker(value: DateTime, valueChanged: DateTime -> unit, ?min: DateTime, ?max: DateTime, ?disabled: bool) : Node =
        comp<RadzenDatePicker<DateTime>> {
            "Value" => value
            attr.callback "ValueChanged" valueChanged

            if min.IsSome then "Min" => min.Value else attr.empty()
            if max.IsSome then "Max" => max.Value else attr.empty()
            if disabled.IsSome then "Disabled" => disabled.Value else attr.empty()
        }

    /// Checkbox.
    static member checkBox(value: bool, valueChanged: bool -> unit, ?text: string, ?disabled: bool) : Node =
        comp<RadzenCheckBox<bool>> {
            "Value" => value
            attr.callback "ValueChanged" valueChanged

            if text.IsSome then "Text" => text.Value else attr.empty()
            if disabled.IsSome then "Disabled" => disabled.Value else attr.empty()
        }

    /// Radio button list (from a list of items with string label and 'T value).
    static member radioButtonList<'T when 'T: equality>(items: (string * 'T) list, selected: 'T, selectedChanged: 'T -> unit, ?disabled: bool) : Node =
        comp<RadzenRadioButtonList<'T>> {
            "Data" => (items |> List.map fst)
            "Value" => selected
            attr.callback "ValueChanged" selectedChanged

            if disabled.IsSome then "Disabled" => disabled.Value else attr.empty()
        }

    // ========================================
    // Dropdown (generic with display expression)
    // ========================================

    /// Generic dropdown with display text selector.
    static member dropDown<'T when 'T: not null>
        (
            items: 'T list,
            selected: 'T,
            selectedChanged: 'T -> unit,
            ?display: 'T -> string,
            ?style: string,
            ?class': string,
            ?disabled: bool
        ) : Node =
        let displayFunc = defaultArg display (fun x -> x.ToString())
        let data = items |> List.map displayFunc
        let value = displayFunc selected

        comp<RadzenDropDown<string>> {
            "Data" => data
            "Value" => value

            attr.callback "Change" (fun (obj: obj) ->
                let selectedStr = obj :?> string
                match items |> List.tryFindIndex (fun x -> displayFunc x = selectedStr) with
                | Some idx -> selectedChanged items.[idx]
                | None -> ())

            if style.IsSome then "Style" => style.Value else attr.empty()
            if class'.IsSome then attr.``class`` class'.Value else attr.empty()
            if disabled.IsSome then "Disabled" => disabled.Value else attr.empty()
        }

    /// String dropdown with Change event handler (matches UI conventions).
    static member dropDownStr(data: string list, value: string, ?style: string, ?onChange: obj -> unit) : Node =
        comp<RadzenDropDown<string>> {
            "Data" => data
            "Value" => value
            if style.IsSome then "Style" => style.Value else attr.empty()
            if onChange.IsSome then attr.callback "Change" onChange.Value else attr.empty()
        }

    // ========================================
    // Buttons & Actions
    // ========================================

    static member button
        (
            text: string,
            ?onClick: MouseEventArgs -> unit,
            ?style: ButtonStyle,
            ?icon: string,
            ?isBusy: bool,
            ?class': string
        ) : Node =
        comp<RadzenButton> {
            "Text" => text

            if onClick.IsSome then attr.callback "Click" onClick.Value else attr.empty()
            if style.IsSome then "ButtonStyle" => style.Value else attr.empty()
            if icon.IsSome then "Icon" => icon.Value else attr.empty()
            if isBusy.IsSome then "IsBusy" => isBusy.Value else attr.empty()
            if class'.IsSome then attr.``class`` class'.Value else attr.empty()
        }

    static member login(onLogin: string * string -> unit, ?allowRegister: bool, ?allowReset: bool, ?allowRememberMe: bool, ?class': string) : Node =
        comp<RadzenLogin> {
            "AllowRegister" => defaultArg allowRegister false
            "AllowResetPassword" => defaultArg allowReset false
            "AllowRememberMe" => defaultArg allowRememberMe false
            attr.callback "Login" (fun (args: Radzen.LoginArgs) -> onLogin(args.Username, args.Password))

            if class'.IsSome then attr.``class`` class'.Value else attr.empty()
        }

    static member icon(name: string, ?style: string, ?class': string) : Node =
        comp<RadzenIcon> {
            "Icon" => name

            if style.IsSome then "Style" => style.Value else attr.empty()
            if class'.IsSome then attr.``class`` class'.Value else attr.empty()
        }

    /// Toggle button specifically for collapsing sidebar.
    static member sidebarToggle(?click: EventArgs -> unit, ?style: string) : Node =
        comp<RadzenSidebarToggle> {
            if click.IsSome then attr.callback "Click" click.Value else attr.empty()
            if style.IsSome then "Style" => style.Value else attr.empty()
        }

    /// Basic label component.
    static member label(text: string, ?style: string, ?class': string) : Node =
        comp<RadzenLabel> {
            "Text" => text
            if style.IsSome then "Style" => style.Value else attr.empty()
            if class'.IsSome then attr.``class`` class'.Value else attr.empty()
        }

    // ========================================
    // Layout & Containers
    // ========================================

    static member layout(children: Node list, ?class': string) : Node =
        comp<RadzenLayout> {
            if class'.IsSome then attr.``class`` class'.Value else attr.empty()
            for child in children do child
        }

    static member rzLayout(items: Node list, ?style: string) : Node =
        comp<RadzenLayout> {
            if style.IsSome then "Style" => style.Value else attr.empty()
            for item in items do item
        }

    static member header(children: Node list, ?class': string) : Node =
        comp<RadzenHeader> {
            if class'.IsSome then attr.``class`` class'.Value else attr.empty()
            for child in children do child
        }

    static member rzHeader(items: Node list, ?style: string) : Node =
        comp<RadzenHeader> {
            if style.IsSome then "Style" => style.Value else attr.empty()
            for item in items do item
        }

    static member sidebar(expanded: bool, expandedChanged: bool -> unit, children: Node list, ?fullHeight: bool, ?responsive: bool, ?class': string) : Node =
        comp<RadzenSidebar> {
            "Expanded" => expanded
            attr.callback "ExpandedChanged" expandedChanged

            if fullHeight.IsSome then "FullHeight" => fullHeight.Value else attr.empty()
            if responsive.IsSome then "Responsive" => responsive.Value else attr.empty()
            if class'.IsSome then attr.``class`` class'.Value else attr.empty()
            for child in children do child
        }

    static member rzSidebar(items: Node list, ?expanded: bool, ?expandedChanged: bool -> unit, ?style: string, ?fullHeight: bool, ?responsive: bool, ?position: SidebarPosition, ?displayStyle: MenuItemDisplayStyle) : Node =
        comp<RadzenSidebar> {
            if expanded.IsSome then "Expanded" => expanded.Value else attr.empty()
            if displayStyle.IsSome then "DisplayStyle" => displayStyle.Value else attr.empty()
            if expandedChanged.IsSome then attr.callback "ExpandedChanged" expandedChanged.Value else attr.empty()
            if fullHeight.IsSome then "FullHeight" => fullHeight.Value else attr.empty()
            if responsive.IsSome then "Responsive" => responsive.Value else attr.empty()
            if position.IsSome then "Position" => position.Value else attr.empty()
            if style.IsSome then "Style" => style.Value else attr.empty()
            for item in items do item
        }

    static member body(children: Node list, ?class': string) : Node =
        comp<RadzenBody> {
            if class'.IsSome then attr.``class`` class'.Value else attr.empty()
            for child in children do child
        }

    static member rzBody(items: Node list, ?style: string) : Node =
        comp<RadzenBody> {
            if style.IsSome then "Style" => style.Value else attr.empty()
            for item in items do item
        }

    static member card(children: Node list, ?class': string) : Node =
        comp<RadzenCard> {
            if class'.IsSome then attr.``class`` class'.Value else attr.empty()
            for child in children do child
        }

    static member stack
        (
            children: Node list,
            ?orientation: Orientation,
            ?alignItems: AlignItems,
            ?justifyContent: JustifyContent,
            ?gap: string,
            ?style: string,
            ?class': string
        ) : Node =
        comp<RadzenStack> {
            if orientation.IsSome then "Orientation" => orientation.Value else attr.empty()
            if alignItems.IsSome then "AlignItems" => alignItems.Value else attr.empty()
            if justifyContent.IsSome then "JustifyContent" => justifyContent.Value else attr.empty()
            if gap.IsSome then "Gap" => gap.Value else attr.empty()
            if style.IsSome then "Style" => style.Value else attr.empty()
            if class'.IsSome then attr.``class`` class'.Value else attr.empty()
            for child in children do child
        }

    // ========================================
    // Navigation (Breadcrumb, PanelMenu)
    // ========================================

    static member breadcrumb(items: (string * string option) list, ?class': string) : Node =
        comp<RadzenBreadCrumb> {
            if class'.IsSome then attr.``class`` class'.Value else attr.empty()
            Rz.namedFragment "Items" [
                for (text, icon) in items ->
                    comp<RadzenBreadCrumbItem> {
                        "Text" => text
                        if icon.IsSome then "Icon" => icon.Value else attr.empty()
                    }
            ]
        }

    static member breadCrumbItem(text: string, ?icon: string) : Node =
        comp<RadzenBreadCrumbItem> {
            "Text" => text
            if icon.IsSome then "Icon" => icon.Value else attr.empty()
        }

    static member breadCrumb(items: Node list) : Node =
        comp<RadzenBreadCrumb> {
            for item in items do item
        }

    static member panelMenu(items: (string * string option * string option) list, ?class': string) : Node =
        comp<RadzenPanelMenu> {
            if class'.IsSome then attr.``class`` class'.Value else attr.empty()
            Rz.namedFragment "MenuItems" [
                for (text, icon, path) in items ->
                    comp<RadzenPanelMenuItem> {
                        "Text" => text
                        if icon.IsSome then "Icon" => icon.Value else attr.empty()
                        if path.IsSome then "Path" => path.Value else attr.empty()
                    }
            ]
        }

    static member panelMenuItem(text: string, ?icon: string, ?path: string, ?style: string, ?onClick: MenuItemEventArgs -> unit) : Node =
        comp<RadzenPanelMenuItem> {
            "Text" => text
            if icon.IsSome then "Icon" => icon.Value else attr.empty()
            if path.IsSome then "Path" => path.Value else attr.empty()
            if style.IsSome then "Style" => style.Value else attr.empty()
            if onClick.IsSome then attr.callback "Click" onClick.Value else attr.empty()
        }

    static member panelMenu(items: Node list, ?style: string) : Node =
        comp<RadzenPanelMenu> {
            if style.IsSome then "Style" => style.Value else attr.empty()
            for item in items do item
        }

    // ========================================
    // DataGrid (with compile-time property access)
    // ========================================

    static member private propName<'T, 'P>(getter: 'T -> 'P) : string = ""

    static member dataGridColumn<'T when 'T: not null>(property: string, title: string, ?template: 'T -> Node, ?width: string, ?formatString: string, ?sortable: bool, ?filterable: bool, ?filterMode: Radzen.FilterMode, ?footer: Node, ?isFrozen: bool, ?frozenPosition: FrozenColumnPosition) : Node =
        comp<RadzenDataGridColumn<'T>> {
            "Property" => property
            "Title" => title
            if width.IsSome then "Width" => width.Value else attr.empty()
            if formatString.IsSome then "FormatString" => formatString.Value else attr.empty()
            if sortable.IsSome then "Sortable" => sortable.Value else attr.empty()
            if filterable.IsSome then "Filterable" => filterable.Value else attr.empty()
            if filterMode.IsSome then "FilterMode" => filterMode.Value else attr.empty()
            if footer.IsSome then attr.fragment "FooterTemplate" footer.Value else attr.empty()
            if isFrozen.IsSome then "Frozen" => isFrozen.Value else attr.empty()
            if frozenPosition.IsSome then "FrozenPosition" => frozenPosition.Value else attr.empty()
            match template with
            | Some t -> attr.fragmentWith "Template" t 
            | None -> attr.empty()
        }

    static member dataGrid<'T when 'T: not null>
        (
            data: seq<'T>,
            columns: Node list,
            ?headerTemplate: Node,
            ?allowFiltering: bool,
            ?allowSorting: bool,
            ?allowResize: bool,
            ?showFooter:bool,
            ?allowPaging: bool,
            ?pageSize: int,
            ?allowVirtualization: bool,
            ?height: string,
            ?rowClick: 'T -> unit,
            ?allowGrouping: bool,
            ?allowColumnPicking: bool,
            ?class': string
        ) : Node =
        comp<RadzenDataGrid<'T>> {
            "Data" => data
            if showFooter.IsSome then "ShowFooter" => showFooter.Value else attr.empty()
            if allowResize.IsSome then "AllowColumnResize" => allowResize.Value else attr.empty()
            if allowFiltering.IsSome then "AllowFiltering" => allowFiltering.Value else attr.empty()
            if allowSorting.IsSome then 
                "AllowSorting" => allowSorting.Value
                "AllowMultiColumnSorting" => allowSorting.Value
                "ShowMultiColumnSortingIndex" => allowSorting.Value
            else 
                attr.empty()
            if allowPaging.IsSome then "AllowPaging" => allowPaging.Value else attr.empty()
            if pageSize.IsSome then "PageSize" => pageSize.Value else attr.empty()
            if allowVirtualization.IsSome then "AllowVirtualization" => allowVirtualization.Value else attr.empty()
            if height.IsSome then "Height" => height.Value else attr.empty()
            if rowClick.IsSome then attr.callback "RowClick" rowClick.Value else attr.empty()
            if class'.IsSome then attr.``class`` class'.Value else attr.empty()
            if allowGrouping.IsSome then "AllowGrouping" => allowGrouping.Value else attr.empty()
            "HideGroupedColumn" => true
            if allowColumnPicking.IsSome then
                "ColumnsPickerAllowFiltering" => allowColumnPicking.Value
                //"QueryOnlyVisibleColumns" => allowColumnPicking.Value
                "AllowColumnPicking" => allowColumnPicking.Value
            else
                attr.empty()
            match headerTemplate with
            | Some t -> attr.fragment "HeaderTemplate" t 
            | None -> attr.empty()
            Rz.namedFragment "Columns" columns
        }

    // ========================================
    // Feedback (Dialog, Notification)
    // ========================================

    /// Show a modal dialog with custom content.
    static member showDialog(dialogService: Radzen.DialogService, title: string, content: Node, ?width: string) : unit =
        let options = Radzen.DialogOptions()
        options.Width <- defaultArg width "500px"
        options.Style <- "border-radius: 8px;"
        let childContent = RenderFragment<Radzen.DialogService>(fun _ ->
            RenderFragment(fun builder ->
                content.Invoke(null, builder, 0) |> ignore
            )
        )
        dialogService.OpenAsync(title, childContent, options) |> ignore

    static member notifySuccess(notificationService: Radzen.NotificationService, message: string, ?duration: int) : unit =
        let msg = Radzen.NotificationMessage()
        msg.Duration <- Nullable(double (defaultArg duration 4000))
        msg.Severity <- NotificationSeverity.Success
        msg.Summary <- message
        notificationService.Notify(msg)

    static member notifyError(notificationService: Radzen.NotificationService, message: string, ?duration: int) : unit =
        let msg = Radzen.NotificationMessage()
        msg.Duration <- Nullable(double (defaultArg duration 4000))
        msg.Severity <- NotificationSeverity.Error
        msg.Summary <- message
        notificationService.Notify(msg)

    // ========================================
    // Progress & Visuals
    // ========================================

    static member progressBar(value: double, ?mode: ProgressBarMode, ?class': string) : Node =
        comp<RadzenProgressBar> {
            "Value" => value
            if mode.IsSome then "Mode" => mode.Value else attr.empty()
            if class'.IsSome then attr.``class`` class'.Value else attr.empty()
        }

    // ========================================
    // Tabs & Accordion
    // ========================================

    static member tabs(tabs: (string * Node list) list, ?selectedIndex: int, ?selectedIndexChanged: int -> unit, ?class': string) : Node =
        comp<RadzenTabs> {
            if selectedIndex.IsSome then "SelectedIndex" => selectedIndex.Value else attr.empty()
            if selectedIndexChanged.IsSome then attr.callback "SelectedIndexChanged" selectedIndexChanged.Value else attr.empty()
            if class'.IsSome then attr.``class`` class'.Value else attr.empty()
            attr.fragment "Tabs" (
                forEach tabs (fun (header, content) ->
                    comp<RadzenTabsItem> {
                        "Text" => header
                        for child in content do child
                    }
                )
            )
        }

    static member accordion(items: (string * Node list) list, ?class': string) : Node =
        comp<RadzenAccordion> {
            if class'.IsSome then attr.``class`` class'.Value else attr.empty()
            attr.fragment "Items" (
                forEach items (fun (title, content) ->
                    comp<RadzenAccordionItem> {
                        "Text" => title
                        for child in content do child
                    }
                )
            )
        }

    // ========================================
    // Form Layout Helpers (theme-aware, no inline styles)
    // ========================================

    static member formField(labelText: string, control: Node, ?required: bool, ?class': string) : Node =
        div {
            attr.``class``(defaultArg class' "rz-form-field")
            label {
                attr.``class`` "rz-form-label"
                if required = Some true then
                    span {
                        attr.``class`` "rz-required-marker"
                        "*"
                    }
                labelText
            }
            div {
                attr.``class`` "rz-form-control"
                control
            }
        }

    static member formField(labelText: string, control: Node) : Node =
        div {
            attr.style "display: flex; flex-direction: column; gap: 6px;"
            label { attr.style "font-size: 13px; font-weight: 500; color: var(--rz-text-secondary-color); font-family: var(--rz-font-family);"; labelText }
            control
        }

    static member templateForm<'T> (data: 'T, children: Node list, ?submit: 'T -> unit, ?class': string) : Node =
        comp<RadzenTemplateForm<'T>> {
            "Data" => data
            if submit.IsSome then attr.callback "Submit" submit.Value else attr.empty()
            if class'.IsSome then attr.``class`` class'.Value else attr.empty()
            attr.fragmentWith "ChildContent" (fun (editContext: Microsoft.AspNetCore.Components.Forms.EditContext) -> Rz.fragment children)
        }

    static member row(columns: (int * Node list) list, ?class': string) : Node =
        comp<RadzenRow> {
            if class'.IsSome then attr.``class`` class'.Value else attr.empty()
            for (span, content) in columns do
                comp<RadzenColumn> {
                    "Size" => span
                    for child in content do child
                }
        }

    static member text(content: string, ?textStyle: TextStyle, ?tagName: TagName, ?class': string) : Node =
        comp<RadzenText> {
            "Text" => content
            if textStyle.IsSome then "TextStyle" => textStyle.Value else attr.empty()
            if tagName.IsSome then "TagName" => tagName.Value else attr.empty()
            if class'.IsSome then attr.``class`` class'.Value else attr.empty()
        }

    static member alert(message: string, ?alertStyle: AlertStyle, ?variant: Variant, ?shade: Shade, ?class': string) : Node =
        comp<RadzenAlert> {
            "Text" => message
            if alertStyle.IsSome then "AlertStyle" => alertStyle.Value else attr.empty()
            if variant.IsSome then "Variant" => variant.Value else attr.empty()
            if shade.IsSome then "Shade" => shade.Value else attr.empty()
            if class'.IsSome then attr.``class`` class'.Value else attr.empty()
        }

    /// Stylized KPI Block Card.
    static member kpiCard(labelText: string, valueText: string, borderColor: string) : Node =
        comp<RadzenCard> {
            "Style" => sprintf "padding: 12px; text-align: center; border-left: 4px solid %s;" borderColor
            span { attr.style "font-size: 11px; color: var(--rz-text-secondary-color); font-weight: bold; text-transform: uppercase; font-family: var(--rz-font-family);"; labelText }
            h4 { attr.style (sprintf "margin: 8px 0 0 0; font-weight: bold; color: %s; font-family: var(--rz-font-family);" borderColor); valueText }
        }

    /// Action button helper for custom layouts.
    static member actionButton(iconName: string, labelText: string, onClick: MouseEventArgs -> unit, ?title: string, ?class': string) : Node =
        button {
            attr.``class`` (defaultArg class' "theme-trigger-btn")
            if title.IsSome then attr.title title.Value else attr.empty()
            attr.style "padding: 4px 8px; border-radius: 4px; font-size: 11px; display: flex; align-items: center; gap: 4px; background-color: rgba(255,255,255,0.05); border: 1px solid var(--rz-border-color); color: var(--rz-text-color); cursor: pointer;"
            on.click onClick
            Rz.icon(iconName, style = "font-size: 14px;")
            span { labelText }
        }

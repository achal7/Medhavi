namespace Medhavi.Web.Components

open System
open Microsoft.AspNetCore.Components
open Microsoft.AspNetCore.Components.Web
open Bolero
open Bolero.Html
open Radzen
open Radzen.Blazor

type DropDownString = RadzenDropDown<string>

type Rz =
    // Type-safe wrapper for RadzenButton
    static member button 
        (text: string, 
         ?style: ButtonStyle, 
         ?icon: string, 
         ?isBusy: bool, 
         ?onClick: MouseEventArgs -> unit) =
        comp<RadzenButton> {
            "Text" => text
            if style.IsSome then "ButtonStyle" => style.Value else attr.empty()
            if icon.IsSome then "Icon" => icon.Value else attr.empty()
            if isBusy.IsSome then "IsBusy" => isBusy.Value else attr.empty()
            if onClick.IsSome then attr.callback "Click" onClick.Value else attr.empty()
        }

    // Type-safe wrapper for RadzenProgressBar
    static member progressBar (value: double, ?mode: ProgressBarMode) =
        comp<RadzenProgressBar> {
            "Value" => value
            if mode.IsSome then "Mode" => mode.Value else attr.empty()
        }

    // Type-safe wrapper for RadzenIcon
    static member icon (name: string, ?style: string) =
        comp<RadzenIcon> {
            "Icon" => name
            if style.IsSome then "Style" => style.Value else attr.empty()
        }

    // Type-safe wrapper for RadzenSidebarToggle
    static member sidebarToggle (?click: EventArgs -> unit, ?style: string) =
        comp<RadzenSidebarToggle> {
            if click.IsSome then attr.callback "Click" click.Value else attr.empty()
            if style.IsSome then "Style" => style.Value else attr.empty()
        }

    // Type-safe wrapper for RadzenLabel
    static member label (text: string, ?style: string, ?class': string) =
        comp<RadzenLabel> {
            "Text" => text
            if style.IsSome then "Style" => style.Value else attr.empty()
            if class'.IsSome then attr.``class`` class'.Value else attr.empty()
        }

    // Type-safe wrapper for RadzenStack
    static member stack (items: Node list, ?orientation: Orientation, ?alignItems: AlignItems, ?justifyContent: JustifyContent, ?gap: string, ?style: string, ?class': string) =
        comp<RadzenStack> {
            if orientation.IsSome then "Orientation" => orientation.Value else attr.empty()
            if alignItems.IsSome then "AlignItems" => alignItems.Value else attr.empty()
            if justifyContent.IsSome then "JustifyContent" => justifyContent.Value else attr.empty()
            if gap.IsSome then "Gap" => gap.Value else attr.empty()
            if style.IsSome then "Style" => style.Value else attr.empty()
            if class'.IsSome then attr.``class`` class'.Value else attr.empty()
            for item in items do
                item
        }

    // Type-safe wrapper for RadzenDropDown<string>
    static member dropDown 
        (data: string list, 
         value: string, 
         ?style: string, 
         ?onChange: obj -> unit) =
        comp<DropDownString> {
            "Data" => data
            "Value" => value
            if style.IsSome then "Style" => style.Value else attr.empty()
            if onChange.IsSome then attr.callback "Change" onChange.Value else attr.empty()
        }

    // Type-safe wrappers for Breadcrumbs
    static member breadCrumbItem (text: string, ?icon: string) =
        comp<RadzenBreadCrumbItem> {
            "Text" => text
            if icon.IsSome then "Icon" => icon.Value else attr.empty()
        }

    static member breadCrumb (items: Node list) =
        comp<RadzenBreadCrumb> {
            for item in items do
                item
        }

    // Type-safe wrappers for PanelMenu Navigation
    static member panelMenuItem (text: string, ?icon: string, ?path: string, ?style: string) =
        comp<RadzenPanelMenuItem> {
            "Text" => text
            if icon.IsSome then "Icon" => icon.Value else attr.empty()
            if path.IsSome then "Path" => path.Value else attr.empty()
            if style.IsSome then "Style" => style.Value else attr.empty()
        }

    static member panelMenu (items: Node list, ?style: string) =
        comp<RadzenPanelMenu> {
            if style.IsSome then "Style" => style.Value else attr.empty()
            for item in items do
                item
        }

    // Type-safe wrappers for Layout containers
    static member rzLayout (items: Node list, ?style: string) =
        comp<RadzenLayout> {
            if style.IsSome then "Style" => style.Value else attr.empty()
            for item in items do
                item
        }

    static member rzHeader (items: Node list, ?style: string) =
        comp<RadzenHeader> {
            if style.IsSome then "Style" => style.Value else attr.empty()
            for item in items do
                item
        }

    static member rzSidebar 
        (items: Node list, 
         ?expanded: bool, 
         ?expandedChanged: bool -> unit, 
         ?style: string, 
         ?fullHeight: bool,
         ?responsive: bool) =
        comp<RadzenSidebar> {
            if expanded.IsSome then "Expanded" => expanded.Value else attr.empty()
            if expandedChanged.IsSome then attr.callback "ExpandedChanged" expandedChanged.Value else attr.empty()
            if fullHeight.IsSome then "FullHeight" => fullHeight.Value else attr.empty()
            if responsive.IsSome then "Responsive" => responsive.Value else attr.empty()
            if style.IsSome then "Style" => style.Value else attr.empty()
            for item in items do
                item
        }

    static member rzBody (items: Node list, ?style: string) =
        comp<RadzenBody> {
            if style.IsSome then "Style" => style.Value else attr.empty()
            for item in items do
                item
        }

    // Type-safe wrappers for RadzenDataGrid
    static member dataGrid<'T> 
        (data: 'T list, 
         columns: Node list,
         ?allowFiltering: bool,
         ?allowSorting: bool,
         ?allowPaging: bool,
         ?pageSize: int,
         ?filterMode: FilterMode,
         ?filterCaseSensitivity: FilterCaseSensitivity,
         ?allowVirtualization: bool,
         ?height: string,
         ?style: string) =
        comp<RadzenDataGrid<'T>> {
            "Data" => data
            if allowFiltering.IsSome then "AllowFiltering" => allowFiltering.Value else attr.empty()
            if allowSorting.IsSome then "AllowSorting" => allowSorting.Value else attr.empty()
            if allowPaging.IsSome then "AllowPaging" => allowPaging.Value else attr.empty()
            if pageSize.IsSome then "PageSize" => pageSize.Value else attr.empty()
            if filterMode.IsSome then "FilterMode" => filterMode.Value else attr.empty()
            if filterCaseSensitivity.IsSome then "FilterCaseSensitivity" => filterCaseSensitivity.Value else attr.empty()
            if allowVirtualization.IsSome then "AllowVirtualization" => allowVirtualization.Value else attr.empty()
            if height.IsSome then "Height" => height.Value else attr.empty()
            if style.IsSome then "Style" => style.Value else attr.empty()
            attr.fragment "Columns" (forEach columns id)
        }

    static member dataGridColumn<'T> 
        (property: string, 
         title: string, 
         ?width: string, 
         ?formatString: string) =
        comp<RadzenDataGridColumn<'T>> {
            "Property" => property
            "Title" => title
            if width.IsSome then "Width" => width.Value else attr.empty()
            if formatString.IsSome then "FormatString" => formatString.Value else attr.empty()
        }

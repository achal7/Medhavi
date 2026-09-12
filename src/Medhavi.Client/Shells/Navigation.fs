module Medhavi.Client.Shells.NavigationBar

open Elmish
open Bolero
open Bolero.Html
open Radzen
open Radzen.Blazor
open Medhavi.Client.Registries
open Medhavi.Client.ViewComposition.Identity
open Medhavi.Client.ViewComposition.View

type NavigationSection =
    | Assistant
    | Views
    | Components
    | Tools

type NavigationTarget =
    | OpenView of ViewDefId
    | AddComponent of ComponentDefId
    | Tool of toolId: string

type NavigationLink =
    { LinkId: string
      Label: string
      Icon: string
      Target: NavigationTarget
      Section: NavigationSection
      KeyboardShortcut: string option
      RequiredRoles: string list
      Order: int
      IsPinned: bool }

type NavigationSectionModel =
    { Section: NavigationSection
      Label: string
      Icon: string
      Links: NavigationLink list
      Collapsed: bool }

type Model =
    { Sections: NavigationSectionModel list
      Collapsed: bool
      SearchText: string
      ActiveLinkId: string option }

type Msg =
    | SetExpanded of bool
    | ToggleCollapsed
    | SetSearchText of string
    | LinkClicked of NavigationLink
    | ToggleSection of NavigationSection
    | AddLink of NavigationLink
    | RemoveLink of string
    | ReorderLink of string * int
    | ReplaceViews of ViewDefinition list

[<RequireQualifiedAccess>]
type Output = LinkClicked of NavigationLink

let private sectionMetadata =
    [ Assistant, "AI Intelligence", "auto_awesome"
      Views, "Views", "layout"
      Components, "Components", "grid_view"
      Tools, "Tools", "construction" ]

let fromViewDefinition (view: ViewDefinition) : NavigationLink =
    { LinkId = "view-" + ViewDefId.value view.Id
      Label = view.Name
      Icon = view.Metadata.Icon
      Target = NavigationTarget.OpenView view.Id
      Section = Views
      KeyboardShortcut = None
      RequiredRoles = view.Metadata.Roles
      Order = if view.Metadata.IsDefault then 0 else 1
      IsPinned = view.Metadata.IsDefault }

let private fromComponentRegistration (registration: ComponentRegistration) : NavigationLink =
    let def = registration.Definition

    { LinkId = "comp-" + ComponentDefId.value def.Id
      Label = def.DisplayName
      Icon = def.Icon
      Target = NavigationTarget.AddComponent def.Id
      Section = Components
      KeyboardShortcut = None
      RequiredRoles = []
      Order = 0
      IsPinned = false }

let private buildSections
    (componentRegistry: ComponentRegistry)
    (views: ViewDefinition list)
    : NavigationSectionModel list =
    let assistantLink =
        { LinkId = "nav-medhavi-assistant"
          Label = "Medhavi Assistant"
          Icon = "auto_awesome"
          Target = NavigationTarget.OpenView(ViewDefId.literal "view-medhavi-assistant")
          Section = Assistant
          KeyboardShortcut = Some "Ctrl+Shift+A"
          RequiredRoles = []
          Order = 0
          IsPinned = true }

    let viewLinks =
        views
        |> List.filter(fun v -> v.Id <> ViewDefId.literal "view-medhavi-assistant")
        |> List.map fromViewDefinition
        |> List.sortBy(fun l -> l.Order, l.Label)

    let componentLinks =
        ComponentRegistry.all componentRegistry |> List.map fromComponentRegistration |> List.sortBy(fun l -> l.Label)

    [ { Section = Assistant
        Label = "Medhavi Assistant"
        Icon = "auto_awesome"
        Links = [ assistantLink ]
        Collapsed = false }
      { Section = Views
        Label = "Views"
        Icon = "auto_awesome_mosaic"
        Links = viewLinks
        Collapsed = false }
      { Section = Components
        Label = "Components"
        Icon = "view_list"
        Links = componentLinks
        Collapsed = false }
      { Section = Tools
        Label = "Tools"
        Icon = "construction"
        Links = []
        Collapsed = false } ]

let init (componentRegistry: ComponentRegistry) (views: ViewDefinition list) : Model * Cmd<Msg> =
    { Sections = buildSections componentRegistry views
      Collapsed = false
      SearchText = ""
      ActiveLinkId = None },
    Cmd.none

let private matchesSearch searchText (link: NavigationLink) =
    if System.String.IsNullOrWhiteSpace searchText then
        true
    else
        let query = searchText.Trim()
        link.Label.Contains(query, System.StringComparison.OrdinalIgnoreCase)

let private visibleSections model =
    model.Sections
    |> List.map(fun section ->
        { section with
            Links = section.Links |> List.filter(matchesSearch model.SearchText) })
    |> List.filter(fun section -> not(List.isEmpty section.Links) || System.String.IsNullOrWhiteSpace model.SearchText)

let private replaceViews (views: ViewDefinition list) (model: Model) : Model =
    let newViewLinks =
        views
        |> List.filter(fun v -> v.Id <> ViewDefId.literal "view-medhavi-assistant")
        |> List.map fromViewDefinition
        |> List.sortBy(fun l -> l.Order, l.Label)

    { model with
        Sections = model.Sections |> List.map(fun s -> if s.Section = Views then { s with Links = newViewLinks } else s) }

let private upsertLink (link: NavigationLink) (model: Model) : Model * Cmd<Msg> * Output option =
    let updatedSections =
        model.Sections
        |> List.map(fun section ->
            if section.Section = link.Section then
                let existing = section.Links |> List.filter(fun l -> l.LinkId <> link.LinkId)
                let combined = (link :: existing) |> List.sortBy(fun l -> l.Order, l.Label)
                { section with Links = combined }
            else
                section)

    { model with
        Sections = updatedSections },
    Cmd.none,
    None

let update (msg: Msg) (model: Model) : Model * Cmd<Msg> * Output option =
    match msg with
    | SetExpanded expanded -> { model with Collapsed = not expanded }, Cmd.none, None

    | ToggleCollapsed ->
        { model with
            Collapsed = not model.Collapsed },
        Cmd.none,
        None

    | SetSearchText text -> { model with SearchText = text }, Cmd.none, None

    | LinkClicked link ->
        { model with
            ActiveLinkId = Some link.LinkId },
        Cmd.none,
        Some(Output.LinkClicked link)

    | ToggleSection section ->
        { model with
            Sections =
                model.Sections
                |> List.map(fun s -> if s.Section = section then { s with Collapsed = not s.Collapsed } else s) },
        Cmd.none,
        None

    | AddLink link -> upsertLink link model

    | RemoveLink linkId ->
        { model with
            Sections =
                model.Sections
                |> List.map(fun s ->
                    { s with
                        Links = s.Links |> List.filter(fun l -> l.LinkId <> linkId) }) },
        Cmd.none,
        None

    | ReorderLink(linkId, newOrder) ->
        { model with
            Sections =
                model.Sections
                |> List.map(fun s ->
                    { s with
                        Links =
                            s.Links
                            |> List.map(fun l -> if l.LinkId = linkId then { l with Order = newOrder } else l)
                            |> List.sortBy(fun l -> l.Order, l.Label) }) },
        Cmd.none,
        None

    | ReplaceViews views -> replaceViews views model, Cmd.none, None

let view (model: Model) (dispatch: Msg -> unit) : Node =
    comp<RadzenSidebar> {
        "Position" => SidebarPosition.Start
        "Expanded" => not model.Collapsed
        "Responsive" => true
        attr.callback "ExpandedChanged" (fun (expanded: bool) -> dispatch(SetExpanded expanded))

        comp<RadzenPanelMenu> {
            forEach (visibleSections model) (fun section ->
                comp<RadzenPanelMenuItem> {
                    "Text" => section.Label
                    "Icon" => section.Icon
                    "Expanded" => true

                    forEach section.Links (fun link ->
                        comp<RadzenPanelMenuItem> {
                            "Text" => link.Label
                            "Icon" => link.Icon
                            attr.callback "Click" (fun (_: MenuItemEventArgs) -> dispatch(LinkClicked link))
                        })
                })
        }
    }

type Component() =
    inherit ElmishComponent<Model, Msg>()

    override _.View model dispatch = view model dispatch

namespace Medhavi.Client.Components

open System
open Bolero
open Bolero.Html
open Radzen
open Radzen.Blazor
open Microsoft.AspNetCore.Components.Web
open Medhavi.Contracts.Client
open Medhavi.Client

module SettingsDialog =

    type Model =
        { SelectedTab: int
          Appearance: AppearancePreferences
          Cultural: CulturalPreferences
          DefaultHorizon: string
          DefaultPlant: string
          CausalDepth: string }

    type Msg =
        | SetSelectedTab of int
        | ChangeTheme of VisualTheme
        | ChangeDensity of Density
        | ChangeCultureCode of string
        | ChangeDateFormat of DateFormatPreference
        | ChangeTimeFormat of TimeFormatPreference
        | ChangeCurrencyDisplay of CurrencyDisplay
        | ChangeMeasurementSystem of MeasurementSystem
        | ChangeFirstDayOfWeek of DayOfWeek
        | ChangeDefaultHorizon of string
        | ChangeDefaultPlant of string
        | ChangeCausalDepth of string
        | ToggleReducedMotion of bool
        | CloseRequested

    type Output =
        | ThemeUpdated of VisualTheme
        | CulturalUpdated of CulturalPreferences
        | AppearanceUpdated of AppearancePreferences
        | CloseModal

    let init (appearance: AppearancePreferences) (cultural: CulturalPreferences) : Model =
        { SelectedTab = 0
          Appearance = appearance
          Cultural = cultural
          DefaultHorizon = "90d"
          DefaultPlant = "ALL"
          CausalDepth = "Standard" }

    let update (msg: Msg) (model: Model) : Model * Output option =
        match msg with
        | SetSelectedTab tab ->
            { model with SelectedTab = tab }, None

        | ChangeTheme theme ->
            let newAppearance = { model.Appearance with Theme = theme }
            { model with Appearance = newAppearance }, Some(ThemeUpdated theme)

        | ChangeDensity density ->
            let newAppearance = { model.Appearance with Density = density }
            { model with Appearance = newAppearance }, Some(AppearanceUpdated newAppearance)

        | ChangeCultureCode code ->
            match CultureName.create code, LanguageTag.create code with
            | Ok cName, Ok lTag ->
                let newCultural = { model.Cultural with FormattingCulture = cName; LanguageTag = lTag }
                { model with Cultural = newCultural }, Some(CulturalUpdated newCultural)
            | _ ->
                model, None

        | ChangeDateFormat df ->
            let newCultural = { model.Cultural with DateFormat = df }
            { model with Cultural = newCultural }, Some(CulturalUpdated newCultural)

        | ChangeTimeFormat tf ->
            let newCultural = { model.Cultural with TimeFormat = tf }
            { model with Cultural = newCultural }, Some(CulturalUpdated newCultural)

        | ChangeCurrencyDisplay cd ->
            let newCultural = { model.Cultural with CurrencyDisplay = cd }
            { model with Cultural = newCultural }, Some(CulturalUpdated newCultural)

        | ChangeMeasurementSystem ms ->
            let newCultural = { model.Cultural with MeasurementSystem = ms }
            { model with Cultural = newCultural }, Some(CulturalUpdated newCultural)

        | ChangeFirstDayOfWeek dow ->
            let newCultural = { model.Cultural with FirstDayOfWeek = dow }
            { model with Cultural = newCultural }, Some(CulturalUpdated newCultural)

        | ChangeDefaultHorizon h ->
            { model with DefaultHorizon = h }, None

        | ChangeDefaultPlant p ->
            { model with DefaultPlant = p }, None

        | ChangeCausalDepth d ->
            { model with CausalDepth = d }, None

        | ToggleReducedMotion rm ->
            let newAppearance = { model.Appearance with ReducedMotion = rm }
            { model with Appearance = newAppearance }, Some(AppearanceUpdated newAppearance)

        | CloseRequested ->
            model, Some CloseModal

    let private themes: (VisualTheme * string) list =
        [ VisualTheme.SoftwareDark, "Software Dark (Default APS)"
          VisualTheme.Software, "Software Light"
          VisualTheme.MaterialDark, "Material Dark"
          VisualTheme.Material, "Material Light"
          VisualTheme.StandardDark, "Standard Dark"
          VisualTheme.Standard, "Standard Light"
          VisualTheme.DefaultDark, "Default Dark"
          VisualTheme.Default, "Default Light"
          VisualTheme.HumanisticDark, "Humanistic Dark"
          VisualTheme.Humanistic, "Humanistic Light" ]

    let view (model: Model) (dispatch: Msg -> unit) : Node =
        let preview = Formatting.generatePreview model.Cultural
        let currentCultureCode = CultureName.value model.Cultural.FormattingCulture

        div {
            // Backdrop Overlay
            attr.style
                """
                position: fixed;
                top: 0;
                left: 0;
                width: 100vw;
                height: 100vh;
                background: rgba(0, 0, 0, 0.65);
                backdrop-filter: blur(4px);
                display: flex;
                align-items: center;
                justify-content: center;
                z-index: 100000;
                """

            // Dialog Window Card
            div {
                attr.style
                    """
                    background: var(--rz-base-background-color);
                    border: 1px solid var(--rz-border-color);
                    border-radius: 12px;
                    box-shadow: 0 20px 48px rgba(0, 0, 0, 0.45);
                    width: 780px;
                    max-width: 95vw;
                    max-height: 90vh;
                    display: flex;
                    flex-direction: column;
                    box-sizing: border-box;
                    overflow: hidden;
                    """

                // Dialog Header
                div {
                    attr.style
                        "display: flex; align-items: center; justify-content: space-between; padding: 1rem 1.5rem; border-bottom: 1px solid var(--rz-border-color); background: var(--rz-base-background-color);"

                    div {
                        attr.style "display: flex; align-items: center; gap: 0.6rem;"

                        div {
                            attr.style
                                "width: 32px; height: 32px; border-radius: 8px; background: var(--rz-primary); display: flex; align-items: center; justify-content: center; color: var(--rz-on-primary);"

                            span {
                                attr.``class`` "rzi"
                                attr.style "font-size: 18px;"
                                "settings"
                            }
                        }

                        div {
                            attr.style "display: flex; flex-direction: column;"

                            span {
                                attr.style "font-weight: 700; font-size: 15px; color: var(--rz-text-color);"
                                "Platform Settings & User Preferences"
                            }

                            span {
                                attr.style "font-size: 11px; color: var(--rz-text-secondary-color);"
                                "Appearance, Cultural Formatting & Planning Defaults"
                            }
                        }
                    }

                    button {
                        attr.``type`` "button"
                        attr.style
                            "border: none; background: transparent; color: var(--rz-text-secondary-color); font-size: 18px; cursor: pointer; padding: 4px;"
                        attr.callback "onclick" (fun (_: MouseEventArgs) -> dispatch CloseRequested)
                        "✕"
                    }
                }

                // Tabs Body
                div {
                    attr.style "flex: 1; overflow-y: auto; padding: 1.25rem 1.5rem;"

                    comp<RadzenTabs> {
                        "SelectedIndex" => model.SelectedTab
                        attr.callback "Change" (SetSelectedTab >> dispatch)

                        attr.fragment
                            "Tabs"
                            (concat {
                                // TAB 1: APPEARANCE & CULTURE
                                comp<RadzenTabsItem> {
                                    "Text" => "Appearance & Culture"

                                    div {
                                        attr.style "display: flex; flex-direction: column; gap: 1.25rem; padding-top: 1rem;"

                                        // Section: Visual Appearance
                                        div {
                                            attr.style "display: flex; flex-direction: column; gap: 0.75rem;"

                                            div {
                                                attr.style "font-weight: 700; font-size: 13px; color: var(--rz-text-color); border-bottom: 1px solid var(--rz-border-color); padding-bottom: 4px;"
                                                "Visual Theme & Density"
                                            }

                                            div {
                                                attr.style "display: grid; grid-template-columns: 1fr 1fr; gap: 1rem;"

                                                // Theme DropDown
                                                div {
                                                    attr.style "display: flex; flex-direction: column; gap: 0.25rem;"

                                                    span {
                                                        attr.style "font-size: 11px; font-weight: 600; color: var(--rz-text-secondary-color);"
                                                        "Active Visual Theme"
                                                    }

                                                    comp<RadzenDropDown<string>> {
                                                        "Data" => (themes |> List.map snd)
                                                        "Value" => (themes |> List.tryFind (fun (t, _) -> t = model.Appearance.Theme) |> Option.map snd |> Option.defaultValue "Software Dark (Default APS)")
                                                        "PopupStyle" => "z-index: 100005;"
                                                        "Style" => "width: 100%;"
                                                        attr.callback "Change" (fun (v: obj) ->
                                                            let selectedTitle = string v
                                                            let matchedTheme = themes |> List.tryFind (fun (_, title) -> title = selectedTitle) |> Option.map fst |> Option.defaultValue VisualTheme.SoftwareDark
                                                            dispatch (ChangeTheme matchedTheme))
                                                    }
                                                }

                                                // Density Selection
                                                div {
                                                    attr.style "display: flex; flex-direction: column; gap: 0.25rem;"

                                                    span {
                                                        attr.style "font-size: 11px; font-weight: 600; color: var(--rz-text-secondary-color);"
                                                        "Grid & Layout Density"
                                                    }

                                                    comp<RadzenDropDown<string>> {
                                                        "Data" => [ "Compact (APS High Density)"; "Comfortable" ]
                                                        "Value" => (match model.Appearance.Density with Density.Compact -> "Compact (APS High Density)" | Density.Comfortable -> "Comfortable")
                                                        "PopupStyle" => "z-index: 100005;"
                                                        "Style" => "width: 100%;"
                                                        attr.callback "Change" (fun (v: obj) ->
                                                            let d = if string v = "Comfortable" then Density.Comfortable else Density.Compact
                                                            dispatch (ChangeDensity d))
                                                    }
                                                }
                                            }
                                        }

                                        // Section: Regional Culture & Numbers
                                        div {
                                            attr.style "display: flex; flex-direction: column; gap: 0.75rem;"

                                            div {
                                                attr.style "font-weight: 700; font-size: 13px; color: var(--rz-text-color); border-bottom: 1px solid var(--rz-border-color); padding-bottom: 4px;"
                                                "Regional Culture & Numeric Formatting"
                                            }

                                            div {
                                                attr.style "display: grid; grid-template-columns: 1fr 1fr; gap: 1rem;"

                                                // Culture Selector
                                                div {
                                                    attr.style "display: flex; flex-direction: column; gap: 0.25rem;"

                                                    span {
                                                        attr.style "font-size: 11px; font-weight: 600; color: var(--rz-text-secondary-color);"
                                                        "Primary Formatting Culture"
                                                    }

                                                    comp<RadzenDropDown<string>> {
                                                        "Data" => (Formatting.availableCultures |> List.map (fun c -> c.DisplayName))
                                                        "Value" => (Formatting.availableCultures |> List.tryFind (fun c -> c.Code = currentCultureCode) |> Option.map (fun c -> c.DisplayName) |> Option.defaultValue "English (United States)")
                                                        "PopupStyle" => "z-index: 100005;"
                                                        "Style" => "width: 100%;"
                                                        attr.callback "Change" (fun (v: obj) ->
                                                            let selectedName = string v
                                                            let matched = Formatting.availableCultures |> List.tryFind (fun c -> c.DisplayName = selectedName)
                                                            match matched with
                                                            | Some c -> dispatch (ChangeCultureCode c.Code)
                                                            | None -> ())
                                                    }
                                                }

                                                // Date Format
                                                div {
                                                    attr.style "display: flex; flex-direction: column; gap: 0.25rem;"

                                                    span {
                                                        attr.style "font-size: 11px; font-weight: 600; color: var(--rz-text-secondary-color);"
                                                        "Date Format Preference"
                                                    }

                                                    comp<RadzenDropDown<string>> {
                                                        "Data" => [ "Culture Default"; "DD/MM/YYYY (European)"; "MM/DD/YYYY (US)"; "YYYY-MM-DD (ISO)" ]
                                                        "Value" => (match model.Cultural.DateFormat with
                                                                    | DateFormatPreference.CultureDefault -> "Culture Default"
                                                                    | DateFormatPreference.DayMonthYear -> "DD/MM/YYYY (European)"
                                                                    | DateFormatPreference.MonthDayYear -> "MM/DD/YYYY (US)"
                                                                    | DateFormatPreference.YearMonthDay -> "YYYY-MM-DD (ISO)")
                                                        "PopupStyle" => "z-index: 100005;"
                                                        "Style" => "width: 100%;"
                                                        attr.callback "Change" (fun (v: obj) ->
                                                            let df =
                                                                match string v with
                                                                | "DD/MM/YYYY (European)" -> DateFormatPreference.DayMonthYear
                                                                | "MM/DD/YYYY (US)" -> DateFormatPreference.MonthDayYear
                                                                | "YYYY-MM-DD (ISO)" -> DateFormatPreference.YearMonthDay
                                                                | _ -> DateFormatPreference.CultureDefault
                                                            dispatch (ChangeDateFormat df))
                                                    }
                                                }

                                                // Time Format
                                                div {
                                                    attr.style "display: flex; flex-direction: column; gap: 0.25rem;"

                                                    span {
                                                        attr.style "font-size: 11px; font-weight: 600; color: var(--rz-text-secondary-color);"
                                                        "Time Format"
                                                    }

                                                    comp<RadzenDropDown<string>> {
                                                        "Data" => [ "24-Hour (14:30:00)"; "12-Hour AM/PM (02:30:00 PM)" ]
                                                        "Value" => (match model.Cultural.TimeFormat with
                                                                    | TimeFormatPreference.TwentyFourHour -> "24-Hour (14:30:00)"
                                                                    | TimeFormatPreference.TwelveHour -> "12-Hour AM/PM (02:30:00 PM)")
                                                        "PopupStyle" => "z-index: 100005;"
                                                        "Style" => "width: 100%;"
                                                        attr.callback "Change" (fun (v: obj) ->
                                                            let tf = if (string v).StartsWith("12") then TimeFormatPreference.TwelveHour else TimeFormatPreference.TwentyFourHour
                                                            dispatch (ChangeTimeFormat tf))
                                                    }
                                                }

                                                // Currency Display
                                                div {
                                                    attr.style "display: flex; flex-direction: column; gap: 0.25rem;"

                                                    span {
                                                        attr.style "font-size: 11px; font-weight: 600; color: var(--rz-text-secondary-color);"
                                                        "Currency Display Format"
                                                    }

                                                    comp<RadzenDropDown<string>> {
                                                        "Data" => [ "Symbol (€ / $ / £ / ¥ / ₹)"; "ISO Currency Code (EUR / USD / GBP / JPY)" ]
                                                        "Value" => (match model.Cultural.CurrencyDisplay with
                                                                    | CurrencyDisplay.CurrencySymbol -> "Symbol (€ / $ / £ / ¥ / ₹)"
                                                                    | CurrencyDisplay.CurrencyCode -> "ISO Currency Code (EUR / USD / GBP / JPY)")
                                                        "PopupStyle" => "z-index: 100005;"
                                                        "Style" => "width: 100%;"
                                                        attr.callback "Change" (fun (v: obj) ->
                                                            let cd = if (string v).StartsWith("ISO") then CurrencyDisplay.CurrencyCode else CurrencyDisplay.CurrencySymbol
                                                            dispatch (ChangeCurrencyDisplay cd))
                                                    }
                                                }

                                                // Measurement System
                                                div {
                                                    attr.style "display: flex; flex-direction: column; gap: 0.25rem;"

                                                    span {
                                                        attr.style "font-size: 11px; font-weight: 600; color: var(--rz-text-secondary-color);"
                                                        "Measurement System"
                                                    }

                                                    comp<RadzenDropDown<string>> {
                                                        "Data" => [ "Metric (Metric Tons / kg)"; "Imperial (US Tons / lbs)" ]
                                                        "Value" => (match model.Cultural.MeasurementSystem with
                                                                    | MeasurementSystem.Metric -> "Metric (Metric Tons / kg)"
                                                                    | MeasurementSystem.Imperial -> "Imperial (US Tons / lbs)")
                                                        "PopupStyle" => "z-index: 100005;"
                                                        "Style" => "width: 100%;"
                                                        attr.callback "Change" (fun (v: obj) ->
                                                            let ms = if (string v).StartsWith("Imperial") then MeasurementSystem.Imperial else MeasurementSystem.Metric
                                                            dispatch (ChangeMeasurementSystem ms))
                                                    }
                                                }

                                                // First Day of Week
                                                div {
                                                    attr.style "display: flex; flex-direction: column; gap: 0.25rem;"

                                                    span {
                                                        attr.style "font-size: 11px; font-weight: 600; color: var(--rz-text-secondary-color);"
                                                        "First Day of Planning Week"
                                                    }

                                                    comp<RadzenDropDown<string>> {
                                                        "Data" => [ "Monday"; "Sunday"; "Saturday" ]
                                                        "Value" => (match model.Cultural.FirstDayOfWeek with
                                                                    | DayOfWeek.Sunday -> "Sunday"
                                                                    | DayOfWeek.Saturday -> "Saturday"
                                                                    | _ -> "Monday")
                                                        "PopupStyle" => "z-index: 100005;"
                                                        "Style" => "width: 100%;"
                                                        attr.callback "Change" (fun (v: obj) ->
                                                            let dow =
                                                                match string v with
                                                                | "Sunday" -> DayOfWeek.Sunday
                                                                | "Saturday" -> DayOfWeek.Saturday
                                                                | _ -> DayOfWeek.Monday
                                                            dispatch (ChangeFirstDayOfWeek dow))
                                                    }
                                                }
                                            }
                                        }

                                        // Section: Live Formatting Preview Card
                                        div {
                                            attr.style
                                                "background: var(--rz-secondary-lighter); border: 1px solid var(--rz-border-color); border-radius: 8px; padding: 1rem; display: flex; flex-direction: column; gap: 0.5rem;"

                                            div {
                                                attr.style "display: flex; align-items: center; justify-content: space-between;"

                                                div {
                                                    attr.style "font-weight: 700; font-size: 12px; color: var(--rz-text-color); display: flex; align-items: center; gap: 0.4rem;"

                                                    span {
                                                        attr.``class`` "rzi"
                                                        attr.style "color: var(--rz-primary); font-size: 16px;"
                                                        "preview"
                                                    }
                                                    "Live Formatting Preview"
                                                }

                                                span {
                                                    attr.style "font-size: 10px; color: var(--rz-text-secondary-color);"
                                                    "Calculated immediately for active culture"
                                                }
                                            }

                                            div {
                                                attr.style "display: grid; grid-template-columns: repeat(4, 1fr); gap: 0.75rem; margin-top: 0.25rem;"

                                                div {
                                                    attr.style "background: var(--rz-base-background-color); border: 1px solid var(--rz-border-color); border-radius: 6px; padding: 8px; display: flex; flex-direction: column; gap: 2px;"
                                                    span { attr.style "font-size: 10px; color: var(--rz-text-secondary-color); font-weight: 700; text-transform: uppercase;"; "Currency" }
                                                    span { attr.style "font-size: 12px; font-weight: 700; color: var(--rz-primary);"; preview.CurrencySample }
                                                }

                                                div {
                                                    attr.style "background: var(--rz-base-background-color); border: 1px solid var(--rz-border-color); border-radius: 6px; padding: 8px; display: flex; flex-direction: column; gap: 2px;"
                                                    span { attr.style "font-size: 10px; color: var(--rz-text-secondary-color); font-weight: 700; text-transform: uppercase;"; "Date & Time" }
                                                    span { attr.style "font-size: 12px; font-weight: 700; color: var(--rz-text-color);"; preview.DateTimeSample }
                                                }

                                                div {
                                                    attr.style "background: var(--rz-base-background-color); border: 1px solid var(--rz-border-color); border-radius: 6px; padding: 8px; display: flex; flex-direction: column; gap: 2px;"
                                                    span { attr.style "font-size: 10px; color: var(--rz-text-secondary-color); font-weight: 700; text-transform: uppercase;"; "Order Qty" }
                                                    span { attr.style "font-size: 12px; font-weight: 700; color: var(--rz-text-color);"; preview.QuantitySample }
                                                }

                                                div {
                                                    attr.style "background: var(--rz-base-background-color); border: 1px solid var(--rz-border-color); border-radius: 6px; padding: 8px; display: flex; flex-direction: column; gap: 2px;"
                                                    span { attr.style "font-size: 10px; color: var(--rz-text-secondary-color); font-weight: 700; text-transform: uppercase;"; "Large Number" }
                                                    span { attr.style "font-size: 12px; font-weight: 700; color: var(--rz-text-color);"; preview.NumberSample }
                                                }
                                            }
                                        }
                                    }
                                }

                                // TAB 2: PLANNING DEFAULTS
                                comp<RadzenTabsItem> {
                                    "Text" => "Planning Defaults"

                                    div {
                                        attr.style "display: flex; flex-direction: column; gap: 1rem; padding-top: 1rem;"

                                        div {
                                            attr.style "font-weight: 700; font-size: 13px; color: var(--rz-text-color); border-bottom: 1px solid var(--rz-border-color); padding-bottom: 4px;"
                                            "Default Horizon & Plant Scope"
                                        }

                                        div {
                                            attr.style "display: grid; grid-template-columns: 1fr 1fr; gap: 1rem;"

                                            div {
                                                attr.style "display: flex; flex-direction: column; gap: 0.25rem;"

                                                span {
                                                    attr.style "font-size: 11px; font-weight: 600; color: var(--rz-text-secondary-color);"
                                                    "Default Planning Horizon"
                                                }

                                                comp<RadzenDropDown<string>> {
                                                    "Data" => [ "30 Days"; "90 Days (Standard)"; "180 Days"; "365 Days" ]
                                                    "Value" => "90 Days (Standard)"
                                                    "PopupStyle" => "z-index: 100005;"
                                                    "Style" => "width: 100%;"
                                                    attr.callback "Change" (fun (v: obj) -> dispatch (ChangeDefaultHorizon (string v)))
                                                }
                                            }

                                            div {
                                                attr.style "display: flex; flex-direction: column; gap: 0.25rem;"

                                                span {
                                                    attr.style "font-size: 11px; font-weight: 600; color: var(--rz-text-secondary-color);"
                                                    "Default Plant Filter"
                                                }

                                                comp<RadzenDropDown<string>> {
                                                    "Data" => [ "All Plants"; "Berlin DC"; "Munich DC"; "Hamburg DC"; "Frankfurt DC" ]
                                                    "Value" => "All Plants"
                                                    "PopupStyle" => "z-index: 100005;"
                                                    "Style" => "width: 100%;"
                                                    attr.callback "Change" (fun (v: obj) -> dispatch (ChangeDefaultPlant (string v)))
                                                }
                                            }
                                        }
                                    }
                                }

                                // TAB 3: AI & INTELLIGENCE
                                comp<RadzenTabsItem> {
                                    "Text" => "AI & Intelligence"

                                    div {
                                        attr.style "display: flex; flex-direction: column; gap: 1rem; padding-top: 1rem;"

                                        div {
                                            attr.style "font-weight: 700; font-size: 13px; color: var(--rz-text-color); border-bottom: 1px solid var(--rz-border-color); padding-bottom: 4px;"
                                            "Causal Intelligence Settings"
                                        }

                                        div {
                                            attr.style "display: flex; flex-direction: column; gap: 0.75rem;"

                                            div {
                                                attr.style "display: flex; flex-direction: column; gap: 0.25rem;"

                                                span {
                                                    attr.style "font-size: 11px; font-weight: 600; color: var(--rz-text-secondary-color);"
                                                    "Causal Lineage Explanation Depth"
                                                }

                                                comp<RadzenDropDown<string>> {
                                                    "Data" => [ "Executive Summary"; "Standard Causal Trace"; "Deep Multi-Tier Lineage" ]
                                                    "Value" => "Standard Causal Trace"
                                                    "PopupStyle" => "z-index: 100005;"
                                                    "Style" => "width: 100%;"
                                                    attr.callback "Change" (fun (v: obj) -> dispatch (ChangeCausalDepth (string v)))
                                                }
                                            }
                                        }
                                    }
                                }

                                // TAB 4: ACCESSIBILITY
                                comp<RadzenTabsItem> {
                                    "Text" => "Accessibility"

                                    div {
                                        attr.style "display: flex; flex-direction: column; gap: 1rem; padding-top: 1rem;"

                                        div {
                                            attr.style "font-weight: 700; font-size: 13px; color: var(--rz-text-color); border-bottom: 1px solid var(--rz-border-color); padding-bottom: 4px;"
                                            "Motion & Accessibility"
                                        }

                                        div {
                                            attr.style "display: flex; align-items: center; justify-content: space-between;"

                                            div {
                                                attr.style "display: flex; flex-direction: column; gap: 2px;"
                                                span { attr.style "font-size: 13px; font-weight: 600; color: var(--rz-text-color);"; "Reduced Motion" }
                                                span { attr.style "font-size: 11px; color: var(--rz-text-secondary-color);"; "Minimize UI animations and micro-transitions" }
                                            }

                                            comp<RadzenSwitch> {
                                                "Value" => model.Appearance.ReducedMotion
                                                attr.callback "Change" (fun (v: bool) -> dispatch (ToggleReducedMotion v))
                                            }
                                        }
                                    }
                                }
                            })
                    }
                }

                // Dialog Footer
                div {
                    attr.style
                        "display: flex; align-items: center; justify-content: flex-end; gap: 0.75rem; padding: 1rem 1.5rem; border-top: 1px solid var(--rz-border-color); background: var(--rz-base-background-color);"

                    comp<RadzenButton> {
                        "Text" => "Done"
                        "Icon" => "check"
                        "ButtonStyle" => ButtonStyle.Primary
                        "Size" => ButtonSize.Small
                        attr.callback "Click" (fun (_: MouseEventArgs) -> dispatch CloseRequested)
                    }
                }
            }
        }

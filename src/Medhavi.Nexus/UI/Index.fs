module Medhavi.Nexus.UI

open Bolero
open Bolero.Html
open Bolero.Server.Html
open Microsoft.AspNetCore.Components
open Microsoft.AspNetCore.Components.Web
open Radzen.Blazor
open Medhavi.Contracts.Client
open Medhavi.Client.Components

[<Route "/{*path}">]
type Index() =
    inherit Component()

    override _.Render() =
        doctypeHtml {
            html {
                attr.lang "en"

                head {
                    meta { attr.charset "utf-8" }

                    meta {
                        attr.name "viewport"
                        attr.content "width=device-width, initial-scale=1"
                    }

                    title { text "Medhavi APS" }

                    comp<RadzenTheme> { "Theme" => VisualTheme.cssName VisualTheme.SoftwareDark }

                    style {
                        rawHtml
                            """
                            .rz-dropdown-panel, .rz-autocomplete-panel, .rz-datepicker-popup, .rz-popup, .rz-overlaypanel {
                                z-index: 100005 !important;
                            }
                            .rz-grid-table tbody tr {
                                cursor: pointer;
                            }
                            .rz-grid-table tbody tr.rz-state-highlight,
                            .rz-grid-table tbody tr.rz-state-highlight > td {
                                background-color: var(--rz-primary-lighter) !important;
                                color: var(--rz-primary-dark) !important;
                                font-weight: 600;
                            }
                            """
                    }
                }

                body {
                    comp<Medhavi.Client.Application> { attr.renderMode(InteractiveServerRenderMode(prerender = false)) }

                    boleroScript
                    script { attr.src "_content/Radzen.Blazor/Radzen.Blazor.js" }

                    script {
                        rawHtml
                            """
                            window.addEventListener('keydown', function(e) {
                                if ((e.ctrlKey || e.metaKey) && (e.key === 'k' || e.key === 'K')) {
                                    e.preventDefault();
                                    var btn = document.getElementById('btn-command-palette');
                                    if (btn) { btn.click(); }
                                }
                            });
                            """
                    }
                }

            }
        }

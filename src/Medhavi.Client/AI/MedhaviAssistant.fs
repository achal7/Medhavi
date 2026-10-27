module Medhavi.Client.Components.MedhaviAssistant

open System
open System.Text.Json
open System.Text.Json.Serialization
open Bolero
open Bolero.Html
open Radzen
open Radzen.Blazor
open Microsoft.AspNetCore.Components.Web
open Medhavi.Client.Registries
open Medhavi.Client.Rendering
open Medhavi.Client.Controls
open Medhavi.Client.AI
open Medhavi.Client.Supply.Capacity
open Medhavi.Client.Supply.SupplyOrders

module CapacityActions = CapacityGridSchema
module SupplyActions = SupplyOrderGridSchema
open Medhavi.Client.ViewComposition.Identity
open Medhavi.Client.ViewComposition.Schema
open Medhavi.Client.ViewComposition.Component
open Medhavi.Client.ViewComposition.Geometry

let definitionId = ComponentDefId.literal "medhavi-assistant"
let storeId = DataStoreId.literal "demo-ai-store"
let schemaId = SchemaDefId.literal "medhavi-assistant-schema"

type ProposedAction =
    { Title: string
      ActionId: ActionDefId
      TargetKey: string
      Description: string
      Impact: string
      Icon: string }

let schemaDefinition: SchemaDefinition =
    { Id = schemaId
      DisplayName = "Medhavi Assistant Intelligence Schema"
      Capabilities = [ "AI"; "Intelligence"; "Causal" ]
      DefaultSort = None
      DefaultFilters = []
      SupportedActions =
        [ CapacityActions.addShift.Id
          CapacityActions.reallocate.Id
          SupplyActions.expedite.Id
          SupplyActions.split.Id ]
      Actions = []
      Columns = [] }

let definition: ComponentDefinition =
    { Id = definitionId
      DisplayName = "Medhavi Assistant"
      Icon = "auto_awesome"
      Category = ComponentCategory.Visualization
      RequiredSchemaCapabilities = []
      DefaultGridSpan = GridSpan.literal 12 12
      DefaultSchemaId = Some schemaId
      DefaultStoreId = Some storeId
      IsResizable = true
      IsDraggable = true
      SelectionMode = SelectionMode.NoSelection }

type ChatMessage =
    | UserMessage of timestamp: string * text: string
    | AssistantMessage of
        timestamp: string *
        title: string *
        summary: string *
        rootCause: string *
        financialImpact: string *
        entityKey: string option *
        entityKind: string option *
        actions: ProposedAction list
    | ReceiptMessage of timestamp: string * title: string * details: string

let private quickPrompts =
    [ "🚨 Shortage Causal Trace (D-1002)", "Explain shortage on demand order D-1002"
      "⚡ Berlin Line 3 Bottleneck (WC-BER-03)", "Analyze critical bottleneck on Berlin Line 3"
      "🚢 PO-8802 Supplier Delay", "Explain 4-day supplier delay on PO-8802"
      "🔄 Multi-DC Load Balancing", "Propose autonomous plan rebalancing across European DCs" ]

let private initialMessages: ChatMessage list =
    [ AssistantMessage(
          DateTimeOffset.UtcNow.ToString("HH:mm:ss"),
          "Medhavi Autonomous Planning Copilot",
          "Welcome to Medhavi Assistant — your AI Supply Chain Strategist and Autonomous Planning Intelligence Engine.\n\nI continuously monitor real-time demand variations, supplier lead-time slips, and machine capacity constraints across European distribution centers. Select a quick starter prompt below or type any planning query.",
          "Continuous multi-tier causal monitoring active.",
          "$0.00 At Risk",
          None,
          None,
          []
      ) ]

let private jsonOptions =
    let opt = JsonSerializerOptions()
    opt.Converters.Add(JsonFSharpConverter())
    opt

let private encodeMessages (messages: ChatMessage list) : string =
    try
        JsonSerializer.Serialize(messages, jsonOptions)
    with _ ->
        "[]"

let private decodeMessages (jsonStr: string) : ChatMessage list =
    if String.IsNullOrWhiteSpace jsonStr then
        initialMessages
    else
        try
            let msgs = JsonSerializer.Deserialize<ChatMessage list>(jsonStr, jsonOptions)
            if List.isEmpty msgs then initialMessages else msgs
        with _ ->
            initialMessages

let private generateAssistantResponse (prompt: string) : ChatMessage =
    let p = prompt.ToLowerInvariant()
    let now = DateTimeOffset.UtcNow.ToString("HH:mm:ss")

    if p.Contains("d-1002") || p.Contains("shortage") then
        let actions =
            [ { Title = "Authorize Weekend Overtime (+8h)"
                ActionId = CapacityActions.addShift.Id
                TargetKey = "WC-BER-03"
                Description = "Authorize +8h Saturday shift on Berlin DC Line 3 to recover 26h deficit."
                Impact = "Recovers 550 units on D-1002; protects $342.5k revenue."
                Icon = "more_time" }
              { Title = "Reallocate Workload to Munich DC"
                ActionId = CapacityActions.reallocate.Id
                TargetKey = "WC-BER-03"
                Description = "Reroute 400 units of A100 Power Module assembly to Munich DC Cell 2."
                Impact = "Balances Berlin Line 3 load to 94.2%; OTIF preserved."
                Icon = "alt_route" } ]

        AssistantMessage(
            now,
            "Root Cause Analysis: Demand Order D-1002 (Siemens Health)",
            "• Customer: Siemens Health\n• Item: A100 Power Module (Munich DC)\n• Deficit: Shortage of 550 units ($342.5k revenue at risk).\n• Causal Chain: Upstream supplier customs delay on PO-8802 caused machine starvation on Berlin Line 3, delaying final assembly.",
            "Upstream vendor customs delay on PO-8802 (Texas Instruments) starved Berlin Line 3.",
            "$342,500 Revenue at Risk",
            Some "D-1002",
            Some "Demand Order",
            actions
        )
    elif p.Contains("wc-ber-03") || p.Contains("bottleneck") || p.Contains("line 3") || p.Contains("berlin") then
        let actions =
            [ { Title = "Authorize Weekend Overtime (+8h)"
                ActionId = CapacityActions.addShift.Id
                TargetKey = "WC-BER-03"
                Description = "Add +8h weekend shift on Berlin Final Power QC & Test Line 3."
                Impact = "Reduces Berlin Line 3 utilization from 121.7% to 92.4%."
                Icon = "more_time" }
              { Title = "Reallocate Workload to Munich Cell 2"
                ActionId = CapacityActions.reallocate.Id
                TargetKey = "WC-BER-03"
                Description = "Shift 38h assembly load to Munich DC Cell 2."
                Impact = "Eliminates overtime expense; restores plant balance."
                Icon = "alt_route" } ]

        AssistantMessage(
            now,
            "Capacity Bottleneck Diagnosis: WC-BER-03 (Berlin DC)",
            "• Work Center: Berlin DC · Final Power QC & Test Line 3\n• Utilization: 121.7% (Required: 146.0h vs Available: 120.0h)\n• Overload: +26.0h deficit across the next 14 business days.\n• Downstream Orders Impacted: D-1002 ($342.5k) and D-1004 ($180.0k).",
            "Machine capacity deficit (+26.0h) on Final Power QC line.",
            "$522,500 Overload Exposure",
            Some "WC-BER-03",
            Some "Work Center",
            actions
        )
    elif p.Contains("po-8802") || p.Contains("delay") || p.Contains("supplier") || p.Contains("vendor") then
        let actions =
            [ { Title = "Air Freight Expedite for PO-8802"
                ActionId = SupplyActions.expedite.Id
                TargetKey = "PO-8802"
                Description = "Switch PO-8802 shipment from ocean transit to priority air cargo."
                Impact = "Recovers 3 business days; avoids €12,500/day SLA penalty."
                Icon = "bolt" }
              { Title = "Split Purchase Order"
                ActionId = SupplyActions.split.Id
                TargetKey = "PO-8802"
                Description = "Split PO-8802: 400 units air freight immediately, 550 units ground."
                Impact = "Guarantees Siemens D-1002 assembly schedule on Sept 3."
                Icon = "call_split" } ]

        AssistantMessage(
            now,
            "Upstream Supplier Delay Analysis: PO-8802",
            "• Vendor: Texas Instruments (Dallas, TX)\n• Promised Delivery: 2026-09-08 (4-day slip from 2026-09-04)\n• Impact: Starves Berlin Line 3 SMT assembly, cascading into shortage for Siemens Health.",
            "Customs clearance hold at Frankfurt cargo hub.",
            "$342,500 Downstream Impact",
            Some "PO-8802",
            Some "Purchase Order",
            actions
        )
    else
        AssistantMessage(
            now,
            "Strategic Supply Chain Network Assessment",
            "• Active Planning Horizon: 90 Days (2026-09-01 -> 2026-11-30)\n• Active Scenario: Baseline Plan\n• Projected Network OTIF: 91.2% (Target: 98.5%)\n• Critical Alerts: 2 machine bottlenecks in Berlin and Frankfurt DCs; 3 demand shortage orders requiring planner intervention.",
            "Network load imbalance across Central European plants.",
            "$865,000 Total Value at Risk",
            None,
            None,
            []
        )

let private renderCausalNode (node: CausalNode) (isLast: bool) : Node =
    concat {
        div {
            attr.style
                "display: flex; flex-direction: column; gap: 2px; padding: 6px 10px; border-radius: 6px; background: var(--rz-secondary-lighter); border: 1px solid var(--rz-border-color); min-width: 140px;"

            div {
                attr.style
                    "font-size: 10px; font-weight: 700; color: var(--rz-text-secondary-color); text-transform: uppercase;"

                node.NodeType
            }

            div {
                attr.style "font-size: 12px; font-weight: 700; color: var(--rz-text-color);"
                node.Title
            }

            div {
                attr.style "font-size: 11px; color: var(--rz-warning-dark); font-weight: 600;"
                node.Status
            }
        }

        if not isLast then
            div {
                attr.style "display: flex; align-items: center; justify-content: center; padding: 0 4px;"

                span {
                    attr.``class`` "rzi"
                    attr.style "color: var(--rz-text-secondary-color); font-size: 16px;"
                    "arrow_forward"
                }
            }
    }

let private renderMessage (msg: ChatMessage) (onExecuteAction: ProposedAction -> unit) : Node =
    match msg with
    | UserMessage(ts, text) ->
        div {
            attr.style "display: flex; justify-content: flex-end; margin: 0.5rem 0;"

            div {
                attr.style
                    "background: var(--rz-primary); color: var(--rz-on-primary); padding: 0.75rem 1rem; border-radius: 12px 12px 2px 12px; max-width: 75%; font-size: 13px; box-shadow: 0 2px 8px rgba(0,0,0,0.12);"

                div { text }

                div {
                    attr.style "font-size: 10px; opacity: 0.8; text-align: right; margin-top: 4px;"
                    ts
                }
            }
        }

    | AssistantMessage(ts, title, summary, rootCause, financialImpact, entityKeyOpt, entityKindOpt, actions) ->
        let explOpt =
            match entityKeyOpt, entityKindOpt with
            | Some key, Some kind -> Some(CausalExplainer.explain key kind)
            | _ -> None

        div {
            attr.style "display: flex; gap: 0.75rem; margin: 0.75rem 0; align-items: flex-start;"

            // AI Avatar Icon
            div {
                attr.style
                    "width: 36px; height: 36px; border-radius: 50%; background: linear-gradient(135deg, var(--rz-primary), var(--rz-secondary)); display: flex; align-items: center; justify-content: center; color: var(--rz-on-primary); flex-shrink: 0; box-shadow: 0 2px 8px var(--rz-shadow-1);"

                span {
                    attr.``class`` "rzi"
                    attr.style "font-size: 18px;"
                    "auto_awesome"
                }
            }

            // Assistant Bubble
            div {
                attr.style
                    "background: var(--rz-base-background-color); border: 1px solid var(--rz-border-color); padding: 1rem 1.25rem; border-radius: 2px 12px 12px 12px; max-width: 85%; font-size: 13px; line-height: 1.5; box-shadow: 0 2px 8px rgba(0,0,0,0.05); display: flex; flex-direction: column; gap: 0.6rem;"

                // Header
                div {
                    attr.style
                        "display: flex; align-items: center; justify-content: space-between; border-bottom: 1px solid var(--rz-border-color); padding-bottom: 0.4rem;"

                    span {
                        attr.style "font-weight: 700; font-size: 13px; color: var(--rz-text-color);"
                        title
                    }

                    span {
                        attr.style
                            "font-size: 11px; font-weight: 700; padding: 2px 8px; border-radius: 10px; background: var(--rz-danger-lighter); color: var(--rz-danger); border: 1px solid var(--rz-danger);"

                        financialImpact
                    }
                }

                // Summary Text
                div {
                    attr.style "white-space: pre-wrap; color: var(--rz-text-color);"
                    summary
                }

                // Root Cause Driver
                div {
                    attr.style
                        "background: var(--rz-secondary-lighter); padding: 6px 10px; border-radius: 4px; font-size: 11px; color: var(--rz-text-secondary-color); border-left: 3px solid var(--rz-primary);"

                    "Root Cause: " + rootCause
                }

                // Optional Causal Lineage Graph
                match explOpt with
                | Some expl when not(List.isEmpty expl.CausalPath) ->
                    div {
                        attr.style
                            "background: var(--rz-secondary-lighter); border: 1px solid var(--rz-border-color); border-radius: 6px; padding: 0.75rem; display: flex; flex-direction: column; gap: 0.5rem; margin-top: 0.25rem;"

                        div {
                            attr.style
                                "display: flex; align-items: center; gap: 0.4rem; font-weight: 700; font-size: 11px; color: var(--rz-text-secondary-color); text-transform: uppercase;"

                            span {
                                attr.``class`` "rzi"
                                attr.style "color: var(--rz-primary); font-size: 14px;"
                                "account_tree"
                            }

                            "Causal Lineage"
                        }

                        div {
                            attr.style "display: flex; align-items: center; gap: 0.35rem; flex-wrap: wrap;"

                            for i, node in List.indexed expl.CausalPath do
                                renderCausalNode node (i = expl.CausalPath.Length - 1)
                        }
                    }
                | _ -> empty()

                // Recommended Action Resolution Buttons
                if not(List.isEmpty actions) then
                    div {
                        attr.style "display: flex; flex-direction: column; gap: 0.5rem; margin-top: 0.4rem;"

                        div {
                            attr.style
                                "font-size: 11px; font-weight: 700; color: var(--rz-text-secondary-color); text-transform: uppercase;"

                            "Recommended Autonomous Resolutions"
                        }

                        for act in actions do
                            div {
                                attr.style
                                    "display: flex; align-items: center; justify-content: space-between; gap: 1rem; padding: 0.5rem 0.75rem; border-radius: 6px; background: var(--rz-secondary-lighter); border: 1px solid var(--rz-border-color);"

                                div {
                                    attr.style "display: flex; flex-direction: column; gap: 2px;"

                                    div {
                                        attr.style "font-weight: 700; font-size: 12px; color: var(--rz-text-color);"
                                        act.Title
                                    }

                                    div {
                                        attr.style "font-size: 11px; color: var(--rz-text-secondary-color);"
                                        act.Description
                                    }

                                    div {
                                        attr.style "font-size: 11px; color: var(--rz-success); font-weight: 600;"
                                        act.Impact
                                    }
                                }

                                comp<RadzenButton> {
                                    "Text" => "Execute"
                                    "Icon" => act.Icon
                                    "ButtonStyle" => ButtonStyle.Primary
                                    "Size" => ButtonSize.Small
                                    attr.callback "Click" (fun (_: MouseEventArgs) -> onExecuteAction act)
                                }
                            }
                    }

                div {
                    attr.style "font-size: 10px; color: var(--rz-text-secondary-color); margin-top: 2px;"
                    "Medhavi AI Engine · " + ts
                }
            }
        }

    | ReceiptMessage(ts, title, details) ->
        div {
            attr.style "display: flex; justify-content: center; margin: 0.5rem 0;"

            div {
                attr.style
                    "display: flex; align-items: center; gap: 0.5rem; background: rgba(40, 167, 69, 0.15); border: 1px solid var(--rz-success); color: var(--rz-success); padding: 0.4rem 0.85rem; border-radius: 20px; font-size: 12px; font-weight: 600;"

                span {
                    attr.``class`` "rzi"
                    attr.style "font-size: 16px;"
                    "check_circle"
                }

                span { "Resolution Applied: '" + title + "' — " + details }

                span {
                    attr.style "opacity: 0.7; font-size: 10px;"
                    "(" + ts + ")"
                }
            }
        }

let private render (ctx: ComponentRenderContext) (dispatchEvent: ComponentEvent -> unit) : Node =
    let historyJson =
        ctx.Configuration.Parameters
        |> Map.tryFind "ChatHistory"
        |> Option.bind (function
            | StringValue s -> Some s
            | _ -> None)
        |> Option.defaultValue ""

    let currentDraft =
        ctx.Configuration.Parameters
        |> Map.tryFind "Draft"
        |> Option.bind (function
            | StringValue s -> Some s
            | _ -> None)
        |> Option.defaultValue ""

    let messages = decodeMessages historyJson

    let saveMessages (newMessages: ChatMessage list) (draft: string) =
        let encoded = encodeMessages newMessages

        let updatedParams =
            ctx.Configuration.Parameters
            |> Map.add "ChatHistory" (StringValue encoded)
            |> Map.add "Draft" (StringValue draft)

        let updatedConfig =
            { ctx.Configuration with
                Parameters = updatedParams }

        dispatchEvent(ConfigurationChangeRequested updatedConfig)

    let onSendPrompt (promptText: string) =
        if not(String.IsNullOrWhiteSpace promptText) then
            let userMsg = UserMessage(DateTimeOffset.UtcNow.ToString("HH:mm:ss"), promptText.Trim())
            let aiMsg = generateAssistantResponse(promptText.Trim())
            let updated = messages @ [ userMsg; aiMsg ]
            saveMessages updated ""

    let onExecuteAction (act: ProposedAction) =
        dispatchEvent(ActionRequested(act.ActionId, Some act.TargetKey, NoPayload))

        let receipt =
            ReceiptMessage(
                DateTimeOffset.UtcNow.ToString("HH:mm:ss"),
                act.Title,
                "Applied to " + act.TargetKey + ". Plan synchronized."
            )

        let updated = messages @ [ receipt ]
        saveMessages updated currentDraft

    let onClearConversation () = saveMessages initialMessages ""

    let onUpdateDraft (text: string) =
        let updatedParams = ctx.Configuration.Parameters |> Map.add "Draft" (StringValue text)

        let updatedConfig =
            { ctx.Configuration with
                Parameters = updatedParams }

        dispatchEvent(ConfigurationChangeRequested updatedConfig)

    div {
        attr.style
            "display: flex; flex-direction: column; width: 100%; height: 100%; background: var(--rz-base-background-color); min-height: 0; box-sizing: border-box;"

        // Header Cockpit Bar
        div {
            attr.style
                "display: flex; align-items: center; justify-content: space-between; padding: 0.75rem 1.25rem; border-bottom: 1px solid var(--rz-border-color); background: var(--rz-base-background-color);"

            div {
                attr.style "display: flex; align-items: center; gap: 0.6rem;"

                div {
                    attr.style
                        "width: 34px; height: 34px; border-radius: 8px; background: linear-gradient(135deg, var(--rz-primary), var(--rz-secondary)); display: flex; align-items: center; justify-content: center; color: var(--rz-on-primary); box-shadow: 0 2px 8px var(--rz-shadow-1);"

                    span {
                        attr.``class`` "rzi"
                        attr.style "font-size: 20px;"
                        "auto_awesome"
                    }
                }

                div {
                    attr.style "display: flex; flex-direction: column;"

                    span {
                        attr.style "font-weight: 700; font-size: 15px; color: var(--rz-text-color);"
                        "Medhavi Assistant"
                    }

                    span {
                        attr.style "font-size: 11px; color: var(--rz-text-secondary-color);"
                        "Autonomous Supply Chain Planning & Causal Intelligence Studio"
                    }
                }
            }

            div {
                attr.style "display: flex; align-items: center; gap: 0.75rem;"

                span {
                    attr.style
                        "padding: 3px 10px; border-radius: 12px; background: var(--rz-success-lighter); color: var(--rz-success); border: 1px solid var(--rz-success); font-size: 11px; font-weight: 700;"

                    "● AI Online"
                }

                comp<RadzenButton> {
                    "Text" => "Clear Chat"
                    "Icon" => "delete_sweep"
                    "ButtonStyle" => ButtonStyle.Light
                    "Size" => ButtonSize.Small
                    attr.callback "Click" (fun (_: MouseEventArgs) -> onClearConversation())
                }
            }
        }

        // Quick Scenario Chips Bar
        div {
            attr.style
                "display: flex; align-items: center; gap: 0.5rem; padding: 0.5rem 1.25rem; border-bottom: 1px solid var(--rz-border-color); background: var(--rz-secondary-lighter); overflow-x: auto;"

            span {
                attr.style
                    "font-size: 11px; font-weight: 700; color: var(--rz-text-secondary-color); white-space: nowrap;"

                "Quick Prompts:"
            }

            for label, promptText in quickPrompts do
                button {
                    attr.``type`` "button"

                    attr.style
                        "border: 1px solid var(--rz-border-color); background: var(--rz-base-background-color); border-radius: 16px; padding: 3px 10px; font-size: 11px; font-weight: 600; color: var(--rz-primary); cursor: pointer; white-space: nowrap; transition: all 0.15s;"

                    attr.callback "onclick" (fun (_: MouseEventArgs) -> onSendPrompt promptText)
                    label
                }
        }

        // Chat Message Stream
        div {
            attr.style
                "flex: 1; min-height: 0; overflow-y: auto; padding: 1.25rem; display: flex; flex-direction: column; gap: 0.5rem;"

            for m in messages do
                renderMessage m onExecuteAction
        }

        // Bottom Chat Input Box
        div {
            attr.style
                "padding: 0.75rem 1.25rem; border-top: 1px solid var(--rz-border-color); background: var(--rz-base-background-color); display: flex; align-items: center; gap: 0.75rem;"

            comp<RadzenTextBox> {
                "Value" => currentDraft

                "Placeholder"
                => "Ask Medhavi Assistant anything (e.g. 'Explain shortage on D-1002' or 'Analyze Berlin Line 3 bottleneck')..."

                "Style" => "flex: 1;"
                attr.callback "Change" (fun (v: string) -> onUpdateDraft v)
            }

            comp<RadzenButton> {
                "Text" => "Send"
                "Icon" => "send"
                "ButtonStyle" => ButtonStyle.Primary
                "Disabled" => String.IsNullOrWhiteSpace currentDraft
                attr.callback "Click" (fun (_: MouseEventArgs) -> onSendPrompt currentDraft)
            }
        }
    }

let runtime: ComponentRuntime = { Render = render }

let registration: ComponentRegistration =
    { Definition = definition
      Runtime = runtime }

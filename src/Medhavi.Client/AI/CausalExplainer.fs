namespace Medhavi.Client.AI

open System

type ImpactSeverity =
    | Low
    | Medium
    | High
    | Critical

type CausalNode =
    { NodeId: string
      NodeType: string
      Icon: string
      Title: string
      Subtitle: string
      Status: string
      StatusColor: string
      Metrics: (string * string) list }

type RecommendedResolution =
    { ResolutionId: string
      Title: string
      Description: string
      ConfidencePct: float
      CostEstimate: string option
      RecoveryTime: string option
      ActionCode: string
      ButtonLabel: string
      ButtonIcon: string }

type CausalExplanation =
    { EntityId: string
      EntityType: string
      Title: string
      Summary: string
      RootCauseStatement: string
      FinancialImpact: string
      Severity: ImpactSeverity
      CausalPath: CausalNode list
      Recommendations: RecommendedResolution list
      GeneratedAt: DateTimeOffset }

module CausalExplainer =

    let private explanationForD1002 : CausalExplanation =
        { EntityId = "D-1002"
          EntityType = "Demand Order"
          Title = "Root-Cause Analysis for Demand D-1002"
          Summary = "Demand order D-1002 for Siemens AG has a shortage of 450 units due to an upstream raw material delivery delay on Purchase Order PO-8802."
          RootCauseStatement = "Texas Instruments Dallas Fab experienced a 4-day wafer packaging backlog, delaying delivery of A100 sub-modules to Berlin DC from 2026-09-04 to 2026-09-08."
          FinancialImpact = "$342,500 Revenue at Risk"
          Severity = Critical
          CausalPath =
            [ { NodeId = "D-1002"
                NodeType = "Customer Demand"
                Icon = "person"
                Title = "Siemens AG · A100 Module"
                Subtitle = "Required: 950 Units · Due: 2026-09-04"
                Status = "Shortage (-450 Units)"
                StatusColor = "var(--rz-danger)"
                Metrics = [ "Contract SLA", "Tier 1 Priority"; "Penalty Clause", "€12,500/day" ] }

              { NodeId = "PL-BER"
                NodeType = "Manufacturing Assembly"
                Icon = "precision_manufacturing"
                Title = "Berlin DC · SMT Line 3"
                Subtitle = "Scheduled Build: 2026-09-03"
                Status = "Material Starvation"
                StatusColor = "#b78103"
                Metrics = [ "Current Load", "92.4% (Bottleneck)"; "Buffer Stock", "0 Units available" ] }

              { NodeId = "PO-8802"
                NodeType = "Pegged Supplier PO"
                Icon = "local_shipping"
                Title = "Texas Instruments · PO-8802"
                Subtitle = "Promised: 2026-09-08 (Original: 2026-09-04)"
                Status = "Supplier Delay (+4 Days)"
                StatusColor = "var(--rz-danger)"
                Metrics = [ "Shipment Mode", "Ocean Freight"; "Tracking", "In Transit (Rotterdam Port)" ] } ]
          Recommendations =
            [ { ResolutionId = "RES-EXPEDITE"
                Title = "Air Freight Expedite"
                Description = "Re-route remaining 450 units from Rotterdam via express air charter directly to Berlin DC. Recovers promised delivery to 2026-09-04."
                ConfidencePct = 94.5
                CostEstimate = Some "+€2,400 Freight"
                RecoveryTime = Some "Recovers 4 Days (On-Time)"
                ActionCode = "supply.expedite"
                ButtonLabel = "Apply Expedite"
                ButtonIcon = "flight_takeoff" }

              { ResolutionId = "RES-REALLOCATE"
                Title = "Buffer Stock Reallocation"
                Description = "Transfer 450 uncommitted buffer units from Munich DC inventory. Transit time is 18 hours via regional ground transport."
                ConfidencePct = 88.0
                CostEstimate = Some "+€650 Transfer"
                RecoveryTime = Some "Recovers 3 Days (2026-09-05)"
                ActionCode = "inventory.transfer"
                ButtonLabel = "Reallocate Stock"
                ButtonIcon = "swap_horiz" }

              { ResolutionId = "RES-SPLIT"
                Title = "Split Order & Customer Waiver"
                Description = "Ship 500 available units on 2026-09-04 and split remaining 450 units to arrive on 2026-09-09 under customer agreement."
                ConfidencePct = 82.0
                CostEstimate = Some "€0 Additional Cost"
                RecoveryTime = Some "500 Units On-Time, 450 Delayed"
                ActionCode = "demand.split"
                ButtonLabel = "Split Order"
                ButtonIcon = "call_split" } ]
          GeneratedAt = DateTimeOffset.UtcNow }

    let private explanationForD1003 : CausalExplanation =
        { EntityId = "D-1003"
          EntityType = "Demand Order"
          Title = "Root-Cause Analysis for Demand D-1003"
          Summary = "Demand order D-1003 for ASML Corp is at risk due to a scheduled maintenance window on Berlin Assembly Line 2."
          RootCauseStatement = "Line 2 scheduled calibration downtime on 2026-09-02 reduces available capacity by 16 hours, causing a 1-day production slip."
          FinancialImpact = "$180,000 Revenue at Risk"
          Severity = Medium
          CausalPath =
            [ { NodeId = "D-1003"
                NodeType = "Customer Demand"
                Icon = "person"
                Title = "ASML Corp · B200 Inverter"
                Subtitle = "Required: 300 Units · Due: 2026-09-04"
                Status = "At Risk"
                StatusColor = "#b78103"
                Metrics = [ "Contract SLA", "Tier 2 Priority"; "Margin", "42.5%" ] }

              { NodeId = "PL-BER-L2"
                NodeType = "Capacity Bottleneck"
                Icon = "build"
                Title = "Berlin DC · Inverter Line 2"
                Subtitle = "Scheduled Maintenance: 2026-09-02"
                Status = "Capacity Constrained (94.8%)"
                StatusColor = "#b78103"
                Metrics = [ "Downtime", "16 Hours"; "Available Capacity", "5.2%" ] } ]
          Recommendations =
            [ { ResolutionId = "RES-OVERTIME"
                Title = "Authorize Weekend Overtime Shift"
                Description = "Authorize a 4-hour overtime shift for Line 2 technicians on 2026-09-03 to clear the production backlog."
                ConfidencePct = 96.0
                CostEstimate = Some "+€1,100 Overtime"
                RecoveryTime = Some "100% On-Time (2026-09-04)"
                ActionCode = "capacity.add_shift"
                ButtonLabel = "Authorize Overtime"
                ButtonIcon = "schedule" }

              { ResolutionId = "RES-REROUTE"
                Title = "Reroute Production to Hamburg DC"
                Description = "Shift 300 units production to Hamburg DC Line 1 which has 35% available capacity."
                ConfidencePct = 91.5
                CostEstimate = Some "+€450 Logistics"
                RecoveryTime = Some "100% On-Time (2026-09-04)"
                ActionCode = "production.reroute"
                ButtonLabel = "Reroute to Hamburg"
                ButtonIcon = "alt_route" } ]
          GeneratedAt = DateTimeOffset.UtcNow }

    let private explanationForPO8802 : CausalExplanation =
        { EntityId = "PO-8802"
          EntityType = "Purchase Order"
          Title = "Supplier Delay Analysis for PO-8802"
          Summary = "PO-8802 from Texas Instruments is delayed by +4 days, impacting 2 downstream customer orders (D-1002, D-1005)."
          RootCauseStatement = "Customs processing delay at Rotterdam Port following ocean transit from Dallas, TX."
          FinancialImpact = "$342,500 Downstream Impact"
          Severity = Critical
          CausalPath =
            [ { NodeId = "PO-8802"
                NodeType = "Purchase Order"
                Icon = "local_shipping"
                Title = "Texas Instruments · PO-8802"
                Subtitle = "Quantity: 950 Units · Promised: 2026-09-08"
                Status = "Delayed (+4 Days)"
                StatusColor = "var(--rz-danger)"
                Metrics = [ "Original Date", "2026-09-04"; "Current Location", "Rotterdam Port" ] }

              { NodeId = "IMPACT-DEMAND"
                NodeType = "Impacted Demands"
                Icon = "warning"
                Title = "2 Downstream Demands Impacted"
                Subtitle = "D-1002 (Siemens AG), D-1005 (Continental)"
                Status = "Production Blocked"
                StatusColor = "var(--rz-danger)"
                Metrics = [ "Total Shortage", "750 Units"; "Combined Revenue", "$512,000" ] } ]
          Recommendations =
            [ { ResolutionId = "RES-EXPEDITE-PO"
                Title = "Priority Port Clearance & Dedicated Trucking"
                Description = "Authorize priority customs clearance fast-track and dedicated hotshot freight truck from Rotterdam to Berlin DC."
                ConfidencePct = 93.0
                CostEstimate = Some "+€1,800 Fast-Track"
                RecoveryTime = Some "Recovers 3 Days (2026-09-05)"
                ActionCode = "supply.expedite"
                ButtonLabel = "Authorize Fast-Track"
                ButtonIcon = "local_shipping" } ]
          GeneratedAt = DateTimeOffset.UtcNow }

    let private explanationForWCBER03 : CausalExplanation =
        { EntityId = "WC-BER-03"
          EntityType = "Work Center"
          Title = "Bottleneck Analysis for Berlin Line 3 (WC-BER-03)"
          Summary = "Work Center WC-BER-03 is running at 121.7% utilization due to concentrated customer demand orders for A100 Power Modules."
          RootCauseStatement = "Load of 146.0 hours exceeds rated shift capacity of 120.0 hours by 26.0 hours (21.7% deficit), stalling D-1002 Siemens Health."
          FinancialImpact = "$342,500 Revenue at Risk"
          Severity = Critical
          CausalPath =
            [ { NodeId = "WC-BER-03"
                NodeType = "Production Work Center"
                Icon = "precision_manufacturing"
                Title = "Berlin DC · Final Power QC & Test Line 3"
                Subtitle = "Avail: 120.0h · Required Load: 146.0h (121.7%)"
                Status = "Critical Overload"
                StatusColor = "var(--rz-danger)"
                Metrics = [ "Deficit Load", "+26.0 Hours"; "Current Shift", "2 Shifts (16h)" ] }

              { NodeId = "D-1002"
                NodeType = "Impacted Demand"
                Icon = "person"
                Title = "Siemens Health · D-1002"
                Subtitle = "Required: 950 Units · Due: 2026-09-03"
                Status = "Material & Capacity Starved"
                StatusColor = "var(--rz-danger)"
                Metrics = [ "Contract SLA", "Tier 1 Priority"; "Shortage", "450 Units" ] }

              { NodeId = "WC-MUN-02"
                NodeType = "Alternate Production Line"
                Icon = "alt_route"
                Title = "Munich DC · Sensor Packaging Cell 2"
                Subtitle = "Current Utilization: 90.0% (Surplus: 12.0h)"
                Status = "Reallocation Target Available"
                StatusColor = "var(--rz-success)"
                Metrics = [ "Surplus Capacity", "12.0 Hours"; "Tooling Compatibility", "100% Match" ] } ]
          Recommendations =
            [ { ResolutionId = "RES-ADD-SHIFT"
                Title = "Authorize Weekend Overtime (+8h Shift)"
                Description = "Authorize +8 hours Saturday overtime shift on Line 3. Reduces deficit by 8h and expedites D-1002 completion by 1 day."
                ConfidencePct = 96.0
                CostEstimate = Some "+€1,200 Overtime Pay"
                RecoveryTime = Some "Recovers 8.0 Load Hours"
                ActionCode = "capacity.add_shift"
                ButtonLabel = "Authorize Overtime Shift"
                ButtonIcon = "more_time" }

              { ResolutionId = "RES-REALLOCATE-LINE"
                Title = "Reallocate 200 Units to Munich DC Line 2"
                Description = "Transfer 200 units of A100 sub-assembly to Munich DC Cell 2, which has 10% spare capacity."
                ConfidencePct = 89.0
                CostEstimate = Some "+€450 Transfer"
                RecoveryTime = Some "Relieves 18.0 Load Hours"
                ActionCode = "capacity.reallocate"
                ButtonLabel = "Reallocate to Munich"
                ButtonIcon = "alt_route" } ]
          GeneratedAt = DateTimeOffset.UtcNow }

    let private fallbackExplanation (entityId: string) (entityType: string) : CausalExplanation =
        { EntityId = entityId
          EntityType = entityType
          Title = sprintf "AI Causal Analysis for %s" entityId
          Summary = sprintf "Autonomous causal trace for %s (%s) generated from active planning graph." entityId entityType
          RootCauseStatement = "Constraint satisfaction evaluation shows minor buffer variance within normal operating thresholds."
          FinancialImpact = "Within Tolerance (< $5,000)"
          Severity = Low
          CausalPath =
            [ { NodeId = entityId
                NodeType = entityType
                Icon = "info"
                Title = entityId
                Subtitle = "Active Planning Horizon"
                Status = "Evaluated"
                StatusColor = "var(--rz-success)"
                Metrics = [ "Status", "Nominal"; "Feasibility", "100%" ] } ]
          Recommendations =
            [ { ResolutionId = "RES-MONITOR"
                Title = "Maintain Standard Plan"
                Description = "No corrective intervention required. Continue monitoring standard execution telemetry."
                ConfidencePct = 99.0
                CostEstimate = Some "€0"
                RecoveryTime = Some "Standard Schedule"
                ActionCode = "plan.monitor"
                ButtonLabel = "Acknowledge"
                ButtonIcon = "check" } ]
          GeneratedAt = DateTimeOffset.UtcNow }

    let explain (entityId: string) (entityType: string) : CausalExplanation =
        match entityId.Trim().ToUpperInvariant() with
        | "D-1002" -> explanationForD1002
        | "D-1003" -> explanationForD1003
        | "PO-8802" -> explanationForPO8802
        | "WC-BER-03" -> explanationForWCBER03
        | _ -> fallbackExplanation entityId entityType


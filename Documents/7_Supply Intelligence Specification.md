# Supply Intelligence Specification

# Chapter 1 — Purpose & Scope

## 1.1 Purpose

Supply Intelligence is the authoritative enterprise domain responsible for developing trusted understanding of supply capabilities, constraints, and plans. Every inventory policy, capacity decision, procurement recommendation, production schedule, distribution allocation, and supplier collaboration activity originates from and is governed by this specification.

Supply Intelligence consumes trusted demand understanding from Demand Intelligence and transforms it into feasible, optimized supply plans that balance service, cost, and risk. It provides the supply‑side foundation upon which order promising, scenario analysis, and enterprise learning depend.

This specification defines every business objective, performance indicator, semantic concept, capability, decision, rule, policy, functional behaviour, interface, report, and dashboard that constitutes the Supply Intelligence domain. It is the single source of enterprise truth for supply.

## 1.2 Scope

**Supply Intelligence includes:**

- Supply plan generation across strategic, tactical, and operational horizons
- Inventory policy setting and inventory health management
- Capacity modelling, capacity planning, and bottleneck identification
- Procurement planning and purchase recommendation generation
- Supplier collaboration, commitment management, and performance evaluation
- Production scheduling and sequencing
- Distribution planning and inter‑location inventory allocation
- Supply risk assessment and continuity planning
- Supply change sensing and real‑time disruption monitoring
- Supply quality measurement and performance reporting
- Supply exception detection, prioritization, and resolution
- Supply decision explainability and traceability
- Continuous supply intelligence learning and improvement

**Supply Intelligence excludes:**

- Demand forecasting and demand signal processing (Demand Intelligence)
- Customer order promising and commitment management (Promise Intelligence)
- Transportation execution and carrier management (Transportation domain, out of scope for core APS)
- Strategic network design (periodic, not operational; may be addressed by Scenario Intelligence)
- Warehouse execution and material handling (execution systems)
- Manufacturing execution and shop‑floor control (MES)

## 1.3 Traceability

| Reference | Principle |
|-----------|-----------|
| C‑EP‑001 | Enterprise First |
| C‑AI‑001 | AI Ready by Design |
| C‑TR‑001 | End‑to‑End Traceability |
| C‑EX‑001 | Explainability |
| C‑CO‑001 | Architectural Consistency |
| ARS‑SM‑001 | Semantic Consistency |
| ARS‑CP‑001 | Capability Consistency |
| ARS‑DM‑001 | Decision Consistency |
| ARS‑RP‑001 | Rule & Policy Consistency |

---

# Chapter 2 — Business Objectives

## BO‑SI‑001 — Deliver Trusted Supply Understanding

**Business Motivation**

Effective supply planning begins with a trusted, current, and complete understanding of enterprise supply capabilities. Supply Intelligence shall continuously consolidate inventory positions, open orders, production status, capacity availability, supplier commitments, and in‑transit quantities into an authoritative supply picture that the entire enterprise can depend upon.

**Business Questions**

- What is the current supply position across all products, locations, and stages (on‑hand, on‑order, in‑transit, work‑in‑progress)?
- How complete and trustworthy is our supply understanding at every node in the supply network?
- Which supply information is uncertain, stale, or inconsistent?
- Where do planners and downstream domains require additional supply visibility?

**Success Measures**

| PI | Name |
|----|------|
| PI‑SI‑001 | Supply Intelligence Effectiveness (Reserved) |
| PI‑SI‑010 | Supply Plan Adherence |
| PI‑SI‑101 | Supply Understanding Index |
| PI‑SI‑206 | Data Quality Score (Supply) |

---

## BO‑SI‑002 — Optimize Inventory Performance

**Business Motivation**

Inventory is both a critical enabler of customer service and a significant use of working capital. Supply Intelligence shall continuously optimize inventory levels—setting appropriate safety stocks, reorder points, and lot sizes—to balance service level targets against holding costs, obsolescence risk, and cash flow requirements.

**Business Questions**

- What are the optimal inventory targets for every product–location combination?
- Where is inventory excessive relative to demand variability and lead time?
- Where is inventory insufficient, creating unacceptable stockout risk?
- How effectively is working capital deployed across the inventory portfolio?

**Success Measures**

| PI | Name |
|----|------|
| PI‑SI‑002 | Inventory Turnover |
| PI‑SI‑003 | Days of Supply |
| PI‑SI‑013 | Excess & Obsolete Inventory |
| PI‑SI‑011 | Backorder Rate |
| PI‑SI‑004 | Fill Rate (Supply) |

---

## BO‑SI‑003 — Maximize Capacity Utilization

**Business Motivation**

Capacity—whether production equipment, warehouse space, or labour—represents a fixed or semi‑fixed resource that must be used efficiently. Supply Intelligence shall continuously evaluate capacity availability against demand requirements, identify bottlenecks, and recommend capacity allocation strategies that maximize throughput while respecting constraints.

**Business Questions**

- What is the current capacity utilization across all resources?
- Where are the binding constraints that limit supply output?
- What capacity investments or reallocations would yield the highest return?
- How can capacity be flexed (overtime, outsourcing, alternate routings) to meet demand peaks?

**Success Measures**

| PI | Name |
|----|------|
| PI‑SI‑005 | Capacity Utilization |
| PI‑SI‑006 | Schedule Adherence |
| PI‑SI‑103 | Capacity Forecast Accuracy |

---

## BO‑SI‑004 — Ensure Supply Continuity

**Business Motivation**

Supply disruptions—whether from supplier failures, quality issues, logistics delays, or unexpected demand spikes—directly threaten customer service and revenue. Supply Intelligence shall proactively identify supply risks, assess their potential impact, and recommend mitigation actions to ensure uninterrupted supply.

**Business Questions**

- Which products, suppliers, or locations pose the greatest supply risk?
- Where are single points of failure in the supply network?
- What mitigation options exist (safety stock, alternate suppliers, capacity buffers)?
- How quickly can the supply network recover from a disruption?

**Success Measures**

| PI | Name |
|----|------|
| PI‑SI‑012 | Stockout Frequency |
| PI‑SI‑104 | Supplier Risk Score |
| PI‑SI‑007 | Supplier On‑Time Delivery |

---

## BO‑SI‑005 — Minimize Total Delivered Cost

**Business Motivation**

Supply decisions directly influence the total cost of satisfying customer demand—including procurement cost, production cost, inventory holding cost, transportation cost, and obsolescence cost. Supply Intelligence shall continuously evaluate trade‑offs between cost, service, and risk, recommending supply plans that minimize total delivered cost over the planning horizon.

**Business Questions**

- What is the total delivered cost per unit for each product–customer combination?
- Where do cost reduction opportunities exist without compromising service?
- How do procurement, production, and distribution decisions interact to affect total cost?
- What is the financial impact of current inventory and capacity decisions?

**Success Measures**

| PI | Name |
|----|------|
| PI‑SI‑008 | Total Supply Chain Cost |
| PI‑SI‑015 | Cash‑to‑Cash Cycle Time |

---

## BO‑SI‑006 — Improve Supplier Collaboration

**Business Motivation**

Suppliers are not external black boxes; they are participants in the enterprise supply network. Supply Intelligence shall enable collaborative planning with key suppliers—sharing forecasts, receiving supply commitments, evaluating supplier reliability, and jointly developing improvement plans—to create a more responsive, transparent, and resilient supply base.

**Business Questions**

- Which suppliers should receive forecast shares and at what level of detail?
- How reliable are supplier commitments, and how should that reliability influence planning?
- Where are supplier constraints limiting enterprise supply capability?
- How can supplier performance be improved through data‑driven collaboration?

**Success Measures**

| PI | Name |
|----|------|
| PI‑SI‑007 | Supplier On‑Time Delivery |
| PI‑SI‑104 | Supplier Risk Score |
| PI‑SI‑105 | Supplier Collaboration Index |

---

## BO‑SI‑007 — Increase Planning Automation

**Business Motivation**

Routine supply planning activities—replenishment, order generation, capacity checks—shall be automated wherever possible, allowing supply planners to focus on exceptions, strategic decisions, and supplier collaboration. Automation shall increase planner productivity without increasing supply risk.

**Business Questions**

- Which supply recommendations can be executed automatically?
- Which situations genuinely require planner intervention?
- Which exceptions should be escalated and to whom?
- Which manual supply planning activities provide little value?

**Success Measures**

| PI | Name |
|----|------|
| PI‑SI‑017 | Supply Automation Rate |
| PI‑SI‑018 | Manual Override Rate (Supply) |
| PI‑SI‑019 | Touchless Planning Rate (Supply) |

---

## BO‑SI‑008 — Continuously Improve Supply Intelligence

**Business Motivation**

Supply Intelligence shall continuously evolve by learning from supply outcomes, plan accuracy, supplier performance, and changing supply network conditions. This objective ensures that supply recommendations become progressively more accurate, more automated, and more valuable without requiring architectural redesign.

**Business Questions**

- Are supply recommendations improving over time?
- Which inventory policies are performing best for which demand‑supply patterns?
- Which planning parameters require revision based on recent evidence?
- Where should Supply Intelligence evolve next?

**Success Measures**

| PI | Name |
|----|------|
| PI‑SI‑021 | Supply Plan Accuracy Improvement |
| PI‑SI‑105 | Recommendation Quality Index (Supply) |
| PI‑SI‑106 | Decision Confidence Index (Supply) |
| PI‑SI‑107 | Explainability Score (Supply) |
| PI‑SI‑108 | Learning Effectiveness Index (Supply) |

---

# Chapter 3 — Enterprise Measurement Model

## 3.1 Measurement Architecture

The Enterprise Measurement Model defines every performance indicator used to evaluate Supply Intelligence. Each indicator is a first‑class enterprise object with a unique identifier, complete definition, formula, interpretation, worked example, limitations, and relationships.

**Three measurement tiers:**

| Range | Tier | Purpose |
|-------|------|---------|
| PI‑SI‑001 – PI‑SI‑049 | Business Outcome Measures | Measure business value delivered |
| PI‑SI‑050 – PI‑SI‑099 | Reserved | Future expansion |
| PI‑SI‑100 – PI‑SI‑199 | Intelligence Measures | Measure intelligence quality |
| PI‑SI‑200 – PI‑SI‑299 | Operational Measures | Measure system performance |

**PI‑SI‑001** is reserved for a future composite index—Supply Intelligence Effectiveness—to be derived after all underlying measures are defined.

---

## 3.2 Business Outcome Measures

### PI‑SI‑001 — Supply Intelligence Effectiveness [RESERVED]

This identifier is reserved for a future composite indicator that will aggregate Business Outcome Measures, Intelligence Measures, and Operational Measures into a single executive health score for the Supply Intelligence domain. It cannot be defined until all underlying measures exist and their interactions are understood.

---

### PI‑SI‑002 — Inventory Turnover

**Definition**

Inventory Turnover measures the number of times the average inventory is sold and replaced over a defined period. It reflects how efficiently inventory is being used relative to demand. Higher values generally indicate leaner inventory management, though excessively high turnover may indicate stockout risk.

**Business Objectives**

- BO‑SI‑002 Optimize Inventory Performance
- BO‑SI‑005 Minimize Total Delivered Cost

**Business Interpretation**

| Value (Turns per year) | Interpretation |
|------------------------|----------------|
| > 12 | Excellent — very lean, potential stockout risk if not managed carefully |
| 8 – 12 | Good — efficient inventory management |
| 4 – 8 | Acceptable — moderate inventory levels |
| < 4 | Investigation required — excessive inventory or slow‑moving stock |

Thresholds are industry‑specific and configurable by enterprise policy.

**Formula**

Inventory Turnover = Cost of Goods Sold (COGS) ÷ Average Inventory Value

Where:

- COGS = Total cost of goods sold over the evaluation period (annualized if period is shorter)
- Average Inventory Value = (Beginning Inventory Value + Ending Inventory Value) ÷ 2, or preferably the average of monthly inventory values for better accuracy

**Formula Variables**

| Variable | Type | Definition |
|----------|------|-------------|
| Cost of Goods Sold (COGS) | Currency | Total cost of goods sold during the evaluation period |
| Beginning Inventory Value | Currency | Inventory valuation at the start of the period |
| Ending Inventory Value | Currency | Inventory valuation at the end of the period |
| Average Inventory Value | Currency | Arithmetic mean of inventory valuations over the period |

**Preconditions**

- COGS data shall be available for the evaluation period
- Inventory valuations shall be available at period boundaries
- Both COGS and inventory values shall use the same costing method (e.g., standard cost, actual cost, FIFO)
- The evaluation period shall be long enough to produce a meaningful annualized figure (minimum one month)

**Assumptions**

- COGS and inventory valuations are accurate and consistently calculated
- Annualization for shorter periods assumes demand and supply are relatively stable (no extreme seasonality)
- Inventory includes all categories: raw materials, work‑in‑progress, finished goods, and spare parts, unless a subset is specified
- The measure reflects financial inventory performance, not operational inventory health alone; a high turnover achieved by frequent stockouts is undesirable

**Specification**

| Attribute | Value |
|-----------|-------|
| Inputs | COGS, Inventory valuations at period boundaries |
| Unit | Turns (number per period, typically annualized) |
| Precision | One decimal place |
| Rounding | Round Half Up |
| Aggregation Levels | Product, Product Family, Location, Business Unit, Enterprise |
| Frequency | Monthly, Quarterly, Annually |
| Performance Targets | Target >12, Warning 8–12, Critical <8 (industry‑specific, configurable) |
| Business Owner | Supply Chain Finance / Inventory Management |
| Business Consumers | Supply Planner, Inventory Manager, Finance Manager, Executive Management |
| System Consumers | Reports, Dashboards, Analytics Services |
| Derived From | COGS, Inventory Valuation |
| Related PIs | PI‑SI‑003 Days of Supply, PI‑SI‑013 Excess & Obsolete Inventory |

**Worked Example**

**Annual Data:**

| Month | COGS | Ending Inventory Value |
|-------|------|------------------------|
| Jan | 500,000 | 200,000 |
| Feb | 480,000 | 210,000 |
| Mar | 520,000 | 190,000 |
| Apr | 510,000 | 220,000 |
| May | 530,000 | 230,000 |
| Jun | 490,000 | 240,000 |
| Jul | 550,000 | 250,000 |
| Aug | 560,000 | 260,000 |
| Sep | 540,000 | 270,000 |
| Oct | 570,000 | 280,000 |
| Nov | 580,000 | 290,000 |
| Dec | 600,000 | 300,000 |
| **Total** | **6,430,000** | |

Average Inventory Value = (200,000 + 210,000 + … + 300,000) ÷ 12

Sum of monthly ending inventories = 2,940,000

Average Inventory Value = 2,940,000 ÷ 12 = 245,000

COGS (annual) = 6,430,000

Inventory Turnover = 6,430,000 ÷ 245,000 = **26.24 turns/year**

Business Interpretation: **Excellent** — inventory is turning over very rapidly, but verify service levels and stockout frequency to ensure leanness is not harming customer service.

**Limitations**

- Inventory Turnover can be inflated by stockouts (low inventory due to shortages rather than efficiency); always evaluate alongside Fill Rate and Stockout Frequency
- The measure is sensitive to inventory valuation methods; changes in costing method distort comparability
- Average of beginning and ending inventory may not capture intra‑period fluctuations; monthly averaging improves accuracy
- Industry norms vary widely; comparisons across industries should be done with caution

**Relationships**

| Relationship | Reference |
|--------------|-----------|
| Supports | BO‑SI‑002, BO‑SI‑005 |
| Compared With | PI‑SI‑003 Days of Supply |
| Complemented By | PI‑SI‑004 Fill Rate (Supply), PI‑SI‑012 Stockout Frequency |
| Displayed In | Inventory Health Dashboard, Supply Chain Performance Dashboard |
| Used By | Inventory Optimization, Working Capital Management, S&OP |

---

### PI‑SI‑003 — Days of Supply

**Definition**

Days of Supply measures the number of days that current on‑hand inventory can satisfy average daily demand, assuming no additional replenishment. It is the inverse perspective of Inventory Turnover, providing an intuitive measure of inventory adequacy. Lower values indicate leaner inventory; higher values may indicate excess stock or safety buffers.

**Business Objectives**

- BO‑SI‑002 Optimize Inventory Performance
- BO‑SI‑004 Ensure Supply Continuity

**Business Interpretation**

| Value (Days) | Interpretation |
|--------------|----------------|
| < 15 | Very lean — potential stockout risk for long‑lead‑time items |
| 15 – 30 | Lean — efficient for fast‑moving products |
| 30 – 60 | Adequate — moderate safety coverage |
| 60 – 90 | Elevated — potential excess inventory |
| > 90 | Excessive — investigation required |

Thresholds are configurable by product category, lead time, and demand variability.

**Formula**

Days of Supply = On‑Hand Inventory ÷ Average Daily Demand

Where:

- On‑Hand Inventory = Quantity currently available for sale or use
- Average Daily Demand = Total Demand Quantity over the evaluation period ÷ Number of Days in the period

**Formula Variables**

| Variable | Type | Definition |
|----------|------|-------------|
| On‑Hand Inventory | Decimal | Current inventory quantity (units) available for use |
| Average Daily Demand | Decimal | Average demand per day over the evaluation period |
| Total Demand Quantity | Decimal | Total demand over the evaluation period |
| Number of Days | Integer | Length of the evaluation period in days |

**Preconditions**

- On‑Hand Inventory shall be current and accurate (ideally from a recent cycle count or trusted system)
- Demand quantities shall be available for a representative evaluation period (minimum 30 days)
- The unit of measure for inventory and demand shall be identical

**Assumptions**

- Average Daily Demand is representative of future demand; if demand is highly seasonal, a longer evaluation period or a seasonal adjustment may be appropriate
- On‑Hand Inventory excludes consignment stock, damaged goods, and allocated inventory (unless explicitly included by policy)
- The measure assumes no immediate replenishment; it is a snapshot, not a projection

**Specification**

| Attribute | Value |
|-----------|-------|
| Inputs | On‑Hand Inventory, Demand History, Evaluation Period |
| Unit | Days |
| Precision | One decimal place |
| Rounding | Round Half Up |
| Aggregation Levels | SKU, Product, Product Family, Location, Business Unit, Enterprise |
| Frequency | Daily, Weekly |
| Performance Targets | Target 15–30 days, Warning 30–60, Critical <15 or >90 (configurable by product category) |
| Business Owner | Inventory Management |
| Business Consumers | Supply Planner, Inventory Manager, Demand Planner |
| System Consumers | Reports, Dashboards, Replenishment Services |
| Derived From | On‑Hand Inventory, Demand History |
| Related PIs | PI‑SI‑002 Inventory Turnover, PI‑SI‑012 Stockout Frequency, PI‑SI‑013 Excess & Obsolete Inventory |

**Worked Example**

**Product X, Location L1:**

On‑Hand Inventory = 1,500 units

Monthly Demand (30 days) = 1,200 units

Average Daily Demand = 1,200 ÷ 30 = 40 units/day

Days of Supply = 1,500 ÷ 40 = **37.5 days**

Business Interpretation: **Adequate** — moderate safety coverage; review if this is appropriate given lead time and variability.

**Worked Example — Low Days of Supply:**

On‑Hand Inventory = 200 units

Average Daily Demand = 50 units/day

Days of Supply = 200 ÷ 50 = **4.0 days**

Business Interpretation: **Very lean** — potential stockout risk; review replenishment lead time and safety stock.

**Limitations**

- Days of Supply is a snapshot measure; it does not capture upcoming replenishments or demand spikes
- Average Daily Demand may mask recent trends; using a trailing average or a weighted moving average can improve responsiveness
- The measure does not account for demand variability or lead time; safety stock requirements vary significantly

**Relationships**

| Relationship | Reference |
|--------------|-----------|
| Supports | BO‑SI‑002, BO‑SI‑004 |
| Compared With | PI‑SI‑002 Inventory Turnover |
| Complemented By | PI‑SI‑012 Stockout Frequency |
| Displayed In | Inventory Health Dashboard |
| Used By | Replenishment Planning, Inventory Policy Setting |

---

### PI‑SI‑004 — Fill Rate (Supply)

**Definition**

Fill Rate (Supply) measures the percentage of demand requests that are fulfilled from available on‑hand inventory at the time of request. It is the supply‑side complement to the customer‑facing Service Level (PI‑DI‑010). A high fill rate indicates that inventory is positioned effectively to meet demand; a low fill rate indicates supply shortfalls or planning failures.

**Business Objectives**

- BO‑SI‑002 Optimize Inventory Performance
- BO‑SI‑004 Ensure Supply Continuity

**Business Interpretation**

| Value | Interpretation |
|-------|----------------|
| 98% – 100% | World‑class supply performance |
| 95% – 98% | Excellent supply performance |
| 90% – 95% | Good supply performance |
| 85% – 90% | Acceptable supply performance |
| Below 85% | Supply performance requires investigation |

Thresholds are configurable by product category and segment.

**Formula**

Fill Rate (Supply) (%) = ( Quantity Fulfilled from On‑Hand ÷ Total Quantity Requested ) × 100

Where:

- Quantity Fulfilled from On‑Hand = sum of requested quantities that were immediately satisfied from available inventory at the first attempt
- Total Quantity Requested = sum of all quantities requested during the evaluation period (from production orders, transfer requests, or customer orders if measured at the point of supply)

**Formula Variables**

| Variable | Type | Definition |
|----------|------|-------------|
| Quantity Fulfilled from On‑Hand | Decimal | Quantity immediately supplied from available inventory |
| Total Quantity Requested | Decimal | Total quantity demanded from the supply point during the period |

**Preconditions**

- Every supply request shall be recorded with a timestamp and requested quantity
- Fulfilment status shall be determined at the point of first supply attempt
- The evaluation period shall be long enough to produce statistically meaningful results

**Assumptions**

- Fill Rate measures first‑attempt fulfilment; subsequent backorder fulfilment is not counted as filled for this metric
- The measure is recorded at the supply point (e.g., warehouse, production line), not at the customer delivery point
- Partial fulfilment is typically counted as a miss, unless policy specifies proportional counting

**Specification**

| Attribute | Value |
|-----------|-------|
| Inputs | Supply requests, Fulfilment records |
| Unit | Percentage (%) |
| Precision | One decimal place |
| Rounding | Round Half Up |
| Aggregation Levels | Product, Product Family, Location, Supplier, Business Unit, Enterprise |
| Frequency | Daily, Weekly, Monthly |
| Performance Targets | Target ≥98%, Warning 95–98%, Critical <95% (configurable) |
| Business Owner | Supply Chain / Inventory Management |
| Business Consumers | Supply Planner, Inventory Manager, Production Scheduler |
| System Consumers | Reports, Dashboards, Replenishment Services |
| Derived From | Supply request and fulfilment transactions |
| Related PIs | PI‑SI‑011 Backorder Rate, PI‑SI‑012 Stockout Frequency, PI‑DI‑010 Service Level |

**Worked Example**

| Request ID | Requested Qty | Fulfilled Qty | First Attempt? | Fulfilled from On‑Hand? |
|------------|---------------|---------------|----------------|--------------------------|
| R1 | 100 | 100 | Yes | Yes |
| R2 | 50 | 50 | Yes | Yes |
| R3 | 200 | 150 | Yes | No (partial) |
| R4 | 75 | 75 | Yes | Yes |
| R5 | 30 | 0 | Yes | No |

Quantity Fulfilled from On‑Hand = 100 + 50 + 0 + 75 + 0 = 225

Total Quantity Requested = 100 + 50 + 200 + 75 + 30 = 455

Fill Rate (Supply) = (225 ÷ 455) × 100 = **49.5%**

Business Interpretation: **Supply performance requires investigation** — significantly below 85%, indicating systemic supply issues.

**Limitations**

- Fill Rate can be influenced by demand volatility, not just supply planning quality
- Partial fulfilment counting depends on policy; consistent application is critical
- Fill Rate at one point in the supply chain may not reflect downstream customer experience

**Relationships**

| Relationship | Reference |
|--------------|-----------|
| Supports | BO‑SI‑002, BO‑SI‑004 |
| Compared With | PI‑SI‑011 Backorder Rate, PI‑DI‑010 Service Level |
| Complemented By | PI‑SI‑012 Stockout Frequency |
| Displayed In | Supply Performance Dashboard |
| Used By | Inventory Policy Review, Supply Planning Accuracy Assessment |

---

### PI‑SI‑005 — Capacity Utilization

**Definition**

Capacity Utilization measures the percentage of available capacity (production, storage, labour) that is actually used during a defined period. It reflects how efficiently the enterprise uses its fixed and variable capacity. High utilization indicates efficient use; excessively high utilization may indicate bottleneck risk.

**Business Objectives**

- BO‑SI‑003 Maximize Capacity Utilization
- BO‑SI‑005 Minimize Total Delivered Cost

**Business Interpretation**

| Value | Interpretation |
|-------|----------------|
| 85% – 95% | Optimal — efficient use with reasonable buffer for variability |
| 70% – 85% | Adequate — some unused capacity |
| 50% – 70% | Underutilized — cost opportunity |
| Above 95% | Overutilized — bottleneck risk, no buffer for disruptions |
| Below 50% | Severely underutilized — requires strategic review |

Thresholds are resource‑ and industry‑specific; configurable.

**Formula**

Capacity Utilization (%) = ( Actual Output ÷ Maximum Available Capacity ) × 100

Where:

- Actual Output = total units produced, volume stored, or hours worked during the period
- Maximum Available Capacity = theoretical maximum output for the same period, adjusted for planned downtime (maintenance, holidays) but not unplanned downtime

**Formula Variables**

| Variable | Type | Definition |
|----------|------|-------------|
| Actual Output | Decimal | Measured output in standard units of capacity |
| Maximum Available Capacity | Decimal | Theoretical capacity net of planned downtime |

**Preconditions**

- Capacity shall be defined in standard units (e.g., machine hours, labour hours, units per period)
- Actual output shall be measurable and recorded
- Maximum Available Capacity shall be known for each resource

**Assumptions**

- Maximum Available Capacity is net of planned maintenance and known holidays
- Unplanned downtime is captured in the Actual Output (lower output) rather than adjusting capacity
- For multi‑resource facilities, utilization may be aggregated using weighted averages

**Specification**

| Attribute | Value |
|-----------|-------|
| Inputs | Actual Output, Maximum Available Capacity |
| Unit | Percentage (%) |
| Precision | One decimal place |
| Rounding | Round Half Up |
| Aggregation Levels | Work Center, Resource Group, Plant, Business Unit, Enterprise |
| Frequency | Daily, Weekly, Monthly |
| Performance Targets | Target 85–95%, Warning 70–85% or >95%, Critical <70% or >98% (configurable) |
| Business Owner | Production Planning / Capacity Management |
| Business Consumers | Production Scheduler, Capacity Planner, Plant Manager |
| System Consumers | Dashboards, Planning Services |
| Derived From | Production records, Capacity master data |
| Related PIs | PI‑SI‑006 Schedule Adherence, PI‑SI‑008 Total Supply Chain Cost |

**Worked Example**

**Work Center WC‑100, Weekly:**

Maximum Available Capacity = 120 hours/week (3 shifts × 5 days × 8 hours, net of planned maintenance)

Actual Output = 102 hours of productive work

Capacity Utilization = (102 ÷ 120) × 100 = **85.0%**

Business Interpretation: **Optimal** — efficient use with a 15% buffer for variability.

**Worked Example — Overutilized:**

Maximum Available Capacity = 120 hours

Actual Output = 118 hours (including overtime)

Capacity Utilization = (118 ÷ 120) × 100 = **98.3%**

Business Interpretation: **Overutilized** — at risk of bottleneck and unable to absorb disruptions.

**Limitations**

- Capacity Utilization does not distinguish between productive output and waste (e.g., rework)
- Aggregating utilization across diverse resources may mask bottlenecks
- Theoretical capacity may be difficult to determine for manual or variable‑pace processes

**Relationships**

| Relationship | Reference |
|--------------|-----------|
| Supports | BO‑SI‑003, BO‑SI‑005 |
| Complemented By | PI‑SI‑006 Schedule Adherence |
| Displayed In | Capacity Utilization Dashboard |
| Used By | Capacity Planning, Capital Investment Decisions |

---

### PI‑SI‑006 — Schedule Adherence

**Definition**

Schedule Adherence measures the percentage of production orders or planned activities that are completed on time and in the planned quantity. It reflects the reliability of production execution against the production schedule. High adherence indicates a stable, predictable production process; low adherence indicates disruptions or planning inaccuracies.

**Business Objectives**

- BO‑SI‑003 Maximize Capacity Utilization
- BO‑SI‑004 Ensure Supply Continuity

**Business Interpretation**

| Value | Interpretation |
|-------|----------------|
| 95% – 100% | Excellent — highly reliable production |
| 90% – 95% | Good — minor deviations |
| 80% – 90% | Acceptable — occasional disruptions |
| Below 80% | Investigation required — significant schedule instability |

Thresholds configurable by resource criticality.

**Formula**

Schedule Adherence (%) = ( Number of Orders Completed On Time and In Full ÷ Total Number of Scheduled Orders ) × 100

Where:

- Completed On Time = order finished on or before the scheduled completion date/time
- In Full = produced quantity equals planned quantity (within a tolerance, default ±5%)
- Total Scheduled Orders includes only firm planned orders, not tentative plans

**Formula Variables**

| Variable | Type | Definition |
|----------|------|-------------|
| Number of Orders Completed OTIF | Integer | Count of orders meeting both on‑time and in‑full criteria |
| Total Scheduled Orders | Integer | All firm production orders with scheduled completion dates |

**Preconditions**

- Every production order shall have a scheduled completion date and planned quantity
- Actual completion date and produced quantity shall be recorded

**Assumptions**

- Only firm planned orders are included; tentative plans are excluded
- Early completion (before scheduled date) is counted as on‑time unless it causes inventory or storage issues, per policy
- Tolerance for in‑full (±5%) is configurable

**Specification**

| Attribute | Value |
|-----------|-------|
| Inputs | Production schedule, Completion records |
| Unit | Percentage (%) |
| Precision | One decimal place |
| Rounding | Round Half Up |
| Aggregation Levels | Work Center, Production Line, Plant, Business Unit |
| Frequency | Daily, Weekly |
| Performance Targets | Target ≥95%, Warning 90–95%, Critical <90% (configurable) |
| Business Owner | Production Planning |
| Business Consumers | Production Scheduler, Plant Manager, Supply Planner |
| System Consumers | Dashboards, MES systems |
| Derived From | Schedule and completion data |
| Related PIs | PI‑SI‑005 Capacity Utilization, PI‑SI‑010 Supply Plan Adherence |

**Worked Example**

| Order | Scheduled Completion | Actual Completion | Scheduled Qty | Actual Qty | On Time? | In Full (within 5%)? | Adherent? |
|-------|----------------------|-------------------|---------------|------------|----------|----------------------|-----------|
| WO1 | 10‑Mar | 10‑Mar | 100 | 100 | Yes | Yes | Yes |
| WO2 | 11‑Mar | 12‑Mar | 50 | 50 | No | Yes | No |
| WO3 | 12‑Mar | 12‑Mar | 200 | 195 | Yes | Yes (2.5% deviation) | Yes |
| WO4 | 13‑Mar | 13‑Mar | 75 | 70 | Yes | No (6.7% deviation) | No |

Number of Orders OTIF = 2 (WO1, WO3)

Total Scheduled Orders = 4

Schedule Adherence = (2 ÷ 4) × 100 = **50.0%**

Business Interpretation: **Investigation required** — significant schedule instability.

**Limitations**

- Schedule Adherence does not distinguish between causes of deviation (machine breakdown, material shortage, planning error)
- The measure is sensitive to the quality of the schedule itself; a poor schedule may have high adherence but poor business outcomes
- The tolerance band may mask small but cumulative deviations

**Relationships**

| Relationship | Reference |
|--------------|-----------|
| Supports | BO‑SI‑003, BO‑SI‑004 |
| Complemented By | PI‑SI‑005 Capacity Utilization |
| Displayed In | Production Performance Dashboard |
| Used By | Production Planning, Continuous Improvement |

---

### PI‑SI‑007 — Supplier On‑Time Delivery

**Definition**

Supplier On‑Time Delivery measures the percentage of supplier deliveries (purchase orders or inbound shipments) that are received on or before the agreed delivery date. It is the primary measure of supplier reliability and directly affects the enterprise’s ability to maintain supply continuity.

**Business Objectives**

- BO‑SI‑004 Ensure Supply Continuity
- BO‑SI‑006 Improve Supplier Collaboration

**Business Interpretation**

| Value | Interpretation |
|-------|----------------|
| 95% – 100% | Excellent supplier reliability |
| 90% – 95% | Good reliability |
| 80% – 90% | Acceptable reliability |
| Below 80% | Supplier performance requires investigation |

Thresholds may vary by supplier tier and criticality.

**Formula**

Supplier On‑Time Delivery (%) = ( Number of Deliveries Received On Time ÷ Total Number of Deliveries ) × 100

Where:

- Received On Time = actual receipt date ≤ agreed delivery date (with a configurable grace period, default +0 days)
- Total Number of Deliveries = all purchase order lines or shipments with a committed delivery date

**Formula Variables**

| Variable | Type | Definition |
|----------|------|-------------|
| Number of Deliveries Received On Time | Integer | Count of deliveries meeting the on‑time criterion |
| Total Number of Deliveries | Integer | All deliveries with a committed date during the period |

**Preconditions**

- A committed delivery date shall exist for every evaluated delivery
- Actual receipt date shall be recorded at goods receipt

**Assumptions**

- Early delivery is counted as on‑time unless early delivery is not permitted by policy (e.g., warehouse capacity constraints)
- Partial deliveries are counted as separate deliveries; each line is evaluated independently
- The evaluation period is typically a rolling 3‑month or 6‑month window to smooth supplier volatility

**Specification**

| Attribute | Value |
|-----------|-------|
| Inputs | Purchase order data, Goods receipt data |
| Unit | Percentage (%) |
| Precision | One decimal place |
| Rounding | Round Half Up |
| Aggregation Levels | Supplier, Supplier Group, Product, Location, Business Unit |
| Frequency | Weekly, Monthly |
| Performance Targets | Target ≥95%, Warning 90–95%, Critical <90% (configurable) |
| Business Owner | Procurement |
| Business Consumers | Supply Planner, Procurement Manager, Supplier Manager |
| System Consumers | Supplier Scorecards, Dashboards |
| Derived From | Purchase orders, receipts |
| Related PIs | PI‑SI‑104 Supplier Risk Score, PI‑SI‑012 Stockout Frequency |

**Worked Example**

| Supplier | PO Line | Committed Date | Actual Receipt | On Time? |
|----------|---------|----------------|----------------|----------|
| SupA | PO1‑L1 | 05‑Mar | 04‑Mar | Yes |
| SupA | PO1‑L2 | 06‑Mar | 08‑Mar | No |
| SupB | PO2‑L1 | 07‑Mar | 07‑Mar | Yes |
| SupC | PO3‑L1 | 08‑Mar | 08‑Mar | Yes |
| SupC | PO3‑L2 | 09‑Mar | 12‑Mar | No |

Number of Deliveries On Time = 3

Total Deliveries = 5

Supplier On‑Time Delivery (Overall) = (3 ÷ 5) × 100 = **60.0%**

Business Interpretation: **Supplier performance requires investigation**.

**Limitations**

- On‑time delivery does not capture quality or quantity compliance; a delivery may be on time but incomplete or damaged
- The measure is sensitive to the agreed delivery date; if dates are routinely padded, performance appears better than it is
- Aggregation across suppliers may mask individual poor performers

**Relationships**

| Relationship | Reference |
|--------------|-----------|
| Supports | BO‑SI‑004, BO‑SI‑006 |
| Complemented By | PI‑SI‑104 Supplier Risk Score |
| Displayed In | Supplier Scorecard, Procurement Dashboard |
| Used By | Supplier Selection, Procurement Planning |

---

### PI‑SI‑008 — Total Supply Chain Cost

**Definition**

Total Supply Chain Cost measures the sum of all costs incurred to plan, source, produce, hold, and deliver products to customers over a defined period. It is the primary financial metric for supply chain efficiency. Lower total cost, for a given service level, indicates better supply chain performance.

**Business Objectives**

- BO‑SI‑005 Minimize Total Delivered Cost

**Business Interpretation**

| Value (Trend) | Interpretation |
|---------------|----------------|
| Decreasing | Supply chain efficiency improving |
| Stable | Efficiency maintained |
| Increasing (below revenue growth) | Cost growing slower than business — acceptable |
| Increasing (above revenue growth) | Cost growing faster than business — investigation required |

Absolute values are compared to budget, forecast, and industry benchmarks.

**Formula**

Total Supply Chain Cost = Procurement Cost + Production Cost + Inventory Holding Cost + Distribution Cost + Obsolescence Cost + Planning & Administration Cost

Each cost component is defined by enterprise policy. The recommended standard components are:

- Procurement Cost = total spend on purchased materials and services
- Production Cost = direct labour, overhead, and consumables for production
- Inventory Holding Cost = average inventory value × holding cost rate (typically 15‑30% per annum)
- Distribution Cost = transportation, warehousing, and handling costs
- Obsolescence Cost = write‑offs and provisions for excess and obsolete inventory
- Planning & Administration Cost = supply chain personnel, systems, and overhead

**Formula Variables**

| Variable | Type | Definition |
|----------|------|-------------|
| Procurement Cost | Currency | Total external spend on materials and services |
| Production Cost | Currency | Direct and allocated production costs |
| Inventory Holding Cost | Currency | Cost of capital, storage, insurance, and shrinkage for inventory held |
| Distribution Cost | Currency | Transportation and warehousing costs |
| Obsolescence Cost | Currency | Write‑offs and provisions for excess/obsolete inventory |
| Planning & Administration Cost | Currency | Personnel, systems, and overhead for supply chain management |

**Preconditions**

- All cost components shall be available from financial or operational systems
- Cost allocation rules shall be defined and consistently applied
- The evaluation period shall align with financial reporting periods

**Assumptions**

- Holding cost rate is defined by finance and may vary by product category
- Costs are actuals, not standard costs, for performance measurement
- The measure may be expressed as absolute cost or as cost‑per‑unit for benchmarking

**Specification**

| Attribute | Value |
|-----------|-------|
| Inputs | Financial data, operational cost data |
| Unit | Currency (or currency per unit) |
| Precision | Whole currency unit for absolute; two decimals for per‑unit |
| Rounding | Round Half Up |
| Aggregation Levels | Product Family, Business Unit, Enterprise |
| Frequency | Monthly, Quarterly, Annually |
| Performance Targets | Budget comparison; year‑over‑year improvement target (configurable) |
| Business Owner | Supply Chain Finance |
| Business Consumers | Supply Chain Director, CFO, Executive Management |
| System Consumers | Financial Dashboards, S&OP Reports |
| Derived From | ERP financial modules |
| Related PIs | PI‑SI‑015 Cash‑to‑Cash Cycle Time, PI‑SI‑002 Inventory Turnover |

**Worked Example**

**Quarterly Cost Summary (Q1):**

| Cost Component | Amount ($) |
|----------------|------------|
| Procurement Cost | 2,500,000 |
| Production Cost | 1,800,000 |
| Inventory Holding Cost | 320,000 |
| Distribution Cost | 450,000 |
| Obsolescence Cost | 75,000 |
| Planning & Admin Cost | 155,000 |
| **Total Supply Chain Cost** | **5,300,000** |

Revenue for Q1 = 12,000,000

Cost as % of Revenue = 5,300,000 ÷ 12,000,000 × 100 = **44.2%**

Trend: Q4 prior year was 45.1% → improvement of 0.9 percentage points.

Business Interpretation: **Improving** — cost ratio declining.

**Limitations**

- Cost allocation can significantly influence results; consistent methodology is critical
- Some cost components (e.g., obsolescence) are lumpy and may distort quarterly comparisons
- Total cost does not reflect service trade‑offs; cost reduction that degrades service is not desirable

**Relationships**

| Relationship | Reference |
|--------------|-----------|
| Supports | BO‑SI‑005 |
| Complemented By | PI‑SI‑004 Fill Rate, PI‑SI‑010 Supply Plan Adherence |
| Displayed In | Supply Chain Cost Dashboard |
| Used By | Budgeting, Cost Reduction Initiatives |

---

### PI‑SI‑009 — Perfect Order Fulfillment (Supply Perspective)

**Definition**

Perfect Order Fulfillment (Supply Perspective) measures the percentage of supply requests (production orders, transfer orders, procurement requisitions) that are fulfilled without any error: correct product, correct quantity, on time, to the correct location, with accurate documentation. This is the supply‑side counterpart to the customer‑facing Perfect Order Rate (PI‑DI‑013). It reflects the reliability of internal supply processes.

**Business Objectives**

- BO‑SI‑002 Optimize Inventory Performance
- BO‑SI‑004 Ensure Supply Continuity

**Business Interpretation**

| Value | Interpretation |
|-------|----------------|
| 90% – 100% | Excellent supply reliability |
| 80% – 90% | Good reliability |
| 65% – 80% | Acceptable reliability |
| Below 65% | Supply reliability requires investigation |

**Formula**

Perfect Order Fulfillment (Supply) (%) = ( Number of Perfect Supply Orders ÷ Total Number of Supply Orders ) × 100

Where a Perfect Supply Order satisfies ALL criteria:
- Correct product (no substitutions unless approved)
- Correct quantity (delivered quantity = requested quantity within tolerance ±5%)
- On time (delivery date ≤ requested date)
- Correct location (delivered to requested location)
- Accurate documentation (order confirmation, packing slip correct)
- No damage (goods in acceptable condition)

**Formula Variables**

| Variable | Type | Definition |
|----------|------|-------------|
| Perfect Supply Order | Boolean | True if all criteria are met |
| Total Number of Supply Orders | Integer | Count of all supply orders during the period |

**Preconditions**

- Each perfection criterion shall have a recorded pass/fail indicator
- Orders without complete criterion data shall be excluded or counted as imperfect per policy

**Specification**

| Attribute | Value |
|-----------|-------|
| Inputs | Supply order data, Fulfilment records, Quality records |
| Unit | Percentage (%) |
| Precision | One decimal place |
| Rounding | Round Half Up |
| Aggregation Levels | Location, Business Unit, Enterprise |
| Frequency | Monthly, Quarterly |
| Performance Targets | Target ≥90%, Warning 80–90%, Critical <80% (configurable) |
| Business Owner | Supply Chain |
| Business Consumers | Supply Planner, Operations Manager |
| System Consumers | Dashboards, Performance Reports |
| Derived From | Supply order and fulfilment systems |
| Related PIs | PI‑SI‑004 Fill Rate, PI‑SI‑006 Schedule Adherence |

**Worked Example**

| Order | Correct Item | Correct Qty | On Time | Correct Location | Accurate Docs | No Damage | Perfect? |
|-------|--------------|-------------|---------|------------------|---------------|-----------|----------|
| TO1 | Yes | Yes | Yes | Yes | Yes | Yes | Yes |
| TO2 | Yes | No (short) | Yes | Yes | Yes | Yes | No |
| TO3 | Yes | Yes | No (1 day late) | Yes | Yes | Yes | No |
| TO4 | Yes | Yes | Yes | Yes | Yes | Yes | Yes |

Number of Perfect Orders = 2

Total Orders = 4

Perfect Order Fulfillment = (2 ÷ 4) × 100 = **50.0%**

Business Interpretation: **Supply reliability requires investigation**.

**Limitations**

- Requires detailed tracking across multiple dimensions
- Sensitive to data quality in auxiliary systems

**Relationships**

| Relationship | Reference |
|--------------|-----------|
| Supports | BO‑SI‑002, BO‑SI‑004 |
| Compared With | PI‑SI‑004 Fill Rate |
| Displayed In | Supply Performance Dashboard |
| Used By | Supply Process Improvement |

---

### PI‑SI‑010 — Supply Plan Adherence

**Definition**

Supply Plan Adherence measures the degree to which actual supply execution follows the agreed supply plan. It compares the supply plan quantities (planned production, procurement, transfers) against actual execution. Adherence below 100% indicates deviation from the plan, which may cause downstream planning disruptions.

**Business Objectives**

- BO‑SI‑001 Deliver Trusted Supply Understanding
- BO‑SI‑002 Optimize Inventory Performance

**Business Interpretation**

| Value | Interpretation |
|-------|----------------|
| 95% – 100% | Excellent adherence |
| 85% – 95% | Good adherence |
| 75% – 85% | Acceptable adherence |
| Below 75% | Adherence requires investigation |

**Formula**

Supply Plan Adherence (%) = ( Quantity Executed Per Plan ÷ Total Planned Quantity ) × 100

Where:
- Quantity Executed Per Plan = sum of quantities where actual execution matched the supply plan within a tolerance band (default ±5%)
- Total Planned Quantity = sum of all planned supply quantities

**Specification**

| Attribute | Value |
|-----------|-------|
| Inputs | Supply Plan, Execution records |
| Unit | Percentage (%) |
| Precision | One decimal place |
| Rounding | Round Half Up |
| Aggregation Levels | Product, Product Family, Location, Business Unit |
| Frequency | Weekly, Monthly |
| Performance Targets | Target ≥95%, Warning 85–95%, Critical <85% (configurable) |
| Business Owner | Supply Planning |
| Business Consumers | Supply Planner, Demand Planner, Production Scheduler |
| Derived From | Supply Plan, Execution |
| Related PIs | PI‑SI‑006 Schedule Adherence, PI‑DI‑009 Demand Plan Adherence |

**Worked Example**

| Week | Plan | Actual | Within Tolerance (±5%)? |
|------|------|--------|--------------------------|
| W1 | 1000 | 980 | Yes |
| W2 | 1200 | 1300 | No (8.3%) |
| W3 | 1100 | 1080 | Yes |

Quantity Executed Per Plan = 1000 + 1100 = 2100

Total Planned = 1000 + 1200 + 1100 = 3300

Adherence = (2100 ÷ 3300) × 100 = **63.6%**

Business Interpretation: **Adherence requires investigation**.

**Limitations**

- Does not distinguish between necessary deviations (e.g., response to demand changes) and execution failures
- Tolerance band choice affects results

**Relationships**

| Relationship | Reference |
|--------------|-----------|
| Supports | BO‑SI‑001, BO‑SI‑002 |
| Complemented By | PI‑SI‑006 Schedule Adherence |
| Displayed In | S&OP Dashboard |
| Used By | Supply‑Demand Balancing |

---

### PI‑SI‑011 — Backorder Rate

**Definition**

Backorder Rate measures the percentage of demand requests that cannot be fulfilled immediately and are placed on backorder. It is a direct indicator of supply insufficiency relative to demand. A high backorder rate signals that supply planning or execution is failing to meet demand.

**Business Objectives**

- BO‑SI‑002 Optimize Inventory Performance
- BO‑SI‑004 Ensure Supply Continuity

**Business Interpretation**

| Value | Interpretation |
|-------|----------------|
| 0% – 2% | Excellent — minimal backorders |
| 2% – 5% | Good |
| 5% – 10% | Acceptable |
| Above 10% | Backorder situation requires investigation |

**Formula**

Backorder Rate (%) = ( Quantity Backordered ÷ Total Quantity Requested ) × 100

**Specification**

| Attribute | Value |
|-----------|-------|
| Inputs | Demand requests, Backorder records |
| Unit | Percentage (%) |
| Precision | One decimal place |
| Rounding | Round Half Up |
| Frequency | Daily, Weekly |
| Performance Targets | Target ≤2%, Warning 2–5%, Critical >10% (configurable) |
| Business Owner | Supply Chain |
| Related PIs | PI‑SI‑004 Fill Rate, PI‑SI‑012 Stockout Frequency |

**Worked Example**

| Requested | Fulfilled Immediately | Backordered | Backorder Rate? |
|-----------|----------------------|-------------|-----------------|
| 100 | 80 | 20 | 20% |

Total Requested = 500, Total Backordered = 55

Backorder Rate = (55 ÷ 500) × 100 = **11.0%**

Business Interpretation: **Investigation required**.

---

### PI‑SI‑012 — Stockout Frequency

**Definition**

Stockout Frequency measures the number of times an item is out of stock when demand occurs during the evaluation period. It counts discrete stockout events, not durations.

**Business Objectives**

- BO‑SI‑004 Ensure Supply Continuity

**Business Interpretation**

| Value (Events per item per month) | Interpretation |
|-----------------------------------|----------------|
| 0 | Excellent — no stockouts |
| 1 | Acceptable — isolated incident |
| 2+ | Investigation required |

**Formula**

Stockout Frequency = Count of Stockout Events

A stockout event occurs when on‑hand inventory reaches zero or is insufficient to meet a demand request.

**Specification**

| Attribute | Value |
|-----------|-------|
| Inputs | Inventory levels, Demand transactions |
| Unit | Count |
| Frequency | Monthly |
| Business Owner | Inventory Management |
| Related PIs | PI‑SI‑004 Fill Rate, PI‑SI‑011 Backorder Rate |

---

### PI‑SI‑013 — Excess & Obsolete Inventory

**Definition**

Excess & Obsolete Inventory measures the value of inventory that exceeds reasonable demand coverage or is no longer saleable/usable, expressed as a percentage of total inventory value.

**Business Objectives**

- BO‑SI‑002 Optimize Inventory Performance

**Formula**

E&O (%) = ( Value of Excess & Obsolete Inventory ÷ Total Inventory Value ) × 100

Excess = inventory quantity exceeding the maximum demand coverage period (e.g., >12 months of demand). Obsolete = inventory with no demand for a defined period or reached end‑of‑life.

**Specification**

| Attribute | Value |
|-----------|-------|
| Inputs | Inventory valuation, Demand forecast, Life‑cycle status |
| Unit | Percentage (%) |
| Frequency | Monthly, Quarterly |
| Performance Targets | Target <5%, Warning 5–10%, Critical >10% |
| Business Owner | Inventory Management / Finance |

---

### PI‑SI‑014 — Planning Cycle Time (Supply)

**Definition**

Planning Cycle Time measures the total elapsed time to complete a full supply planning cycle, from demand input receipt to published supply plan.

**Formula**

Planning Cycle Time = Time(Plan Published) − Time(Demand Input Received)

**Specification**

| Attribute | Value |
|-----------|-------|
| Inputs | Cycle timestamps |
| Unit | Hours |
| Frequency | Per cycle |
| Business Owner | Supply Planning |
| Related PIs | PI‑SI‑201 Operational measures |

---

### PI‑SI‑015 — Cash‑to‑Cash Cycle Time

**Definition**

Cash‑to‑Cash Cycle Time measures the number of days between paying for raw materials and receiving payment for finished goods. It reflects how long working capital is tied up in operations.

**Formula**

Cash‑to‑Cash = Days of Inventory Outstanding + Days Sales Outstanding − Days Payable Outstanding

**Specification**

| Attribute | Value |
|-----------|-------|
| Inputs | Financial data |
| Unit | Days |
| Frequency | Monthly |
| Business Owner | Supply Chain Finance |
| Related PIs | PI‑SI‑003 Days of Supply |

---

We will now continue with **Part 3** of the Supply Intelligence Specification, covering the remaining sections of Chapter 3 (Intelligence and Operational Measures), then moving into Chapter 4 — Semantic Foundation.

Before writing, I challenged the structure for any inconsistencies and found a numbering collision in the Business Objectives that needs to be corrected. I’ll fix it as we proceed.

---

## 3.3 Intelligence Measures  

Intelligence Measures evaluate the quality, confidence, explainability, and effectiveness of the Supply Intelligence domain’s internal reasoning. These measures depend on the detailed design of the Capability Realizations (Chapter 5) and are defined as stubs here. Full specifications will be completed once the capabilities are fully specified.

**Note on numbering:** In Chapter 2, `PI‑SI‑105` was referenced by two different objectives (Supplier Collaboration Index and Recommendation Quality Index). That has been corrected. The official mapping is:

| PI | Name | Description |
|----|------|-------------|
| PI‑SI‑101 | Supply Understanding Index | Composite measure of how completely and reliably the enterprise understands its current supply position. Reserved. |
| PI‑SI‑102 | Inventory Optimization Effectiveness | Measures how well inventory policies balance service, cost, and risk. Reserved. |
| PI‑SI‑103 | Capacity Forecast Accuracy | Accuracy of capacity utilization forecasts versus actuals. Reserved. |
| PI‑SI‑104 | Supplier Risk Score | Quantifies the risk of supply disruption from each supplier based on performance, financial health, and external factors. Reserved. |
| PI‑SI‑105 | Recommendation Quality Index (Supply) | Evaluates the business quality of supply recommendations (e.g., replenishment, capacity allocation). Reserved. |
| PI‑SI‑106 | Decision Confidence Index (Supply) | Average confidence score across all supply decisions in a period. Reserved. |
| PI‑SI‑107 | Explainability Score (Supply) | Measures the degree to which supply outputs are accompanied by complete, traceable explanations. Reserved. |
| PI‑SI‑108 | Learning Effectiveness Index (Supply) | Measures the rate at which supply model and policy performance improves due to learning. Reserved. |
| PI‑SI‑109 | Supplier Collaboration Index | Measures the depth and effectiveness of planning collaboration with key suppliers (forecast sharing, commitment reliability, joint improvement). Reserved. |
| PI‑SI‑110 | Supply Exception Detection Accuracy | Precision and recall of detected supply exceptions (shortages, excess, capacity violations). Reserved. |
| PI‑SI‑111 | Supply Plan Confidence Index | Aggregate confidence in the published supply plan. Reserved. |
| PI‑SI‑112 | Supply Intelligence Coverage Index | Percentage of products/locations/suppliers with active supply intelligence. Reserved. |

---

## 3.4 Operational Measures  

Operational Measures evaluate the technical performance of the Supply Intelligence system. These are placeholders pending the detailed software realization (Chapter 5). Full specifications will be added when implementation decisions are finalised.

| PI | Name | Description |
|----|------|-------------|
| PI‑SI‑201 | Supply Planning Cycle Time | Total time to complete a full supply planning cycle. Reserved. |
| PI‑SI‑202 | Plan Generation Time | Time taken to generate the supply plan for a cycle. Reserved. |
| PI‑SI‑203 | Inventory Refresh Latency | Time from inventory transaction to reflection in the supply picture. Reserved. |
| PI‑SI‑204 | Supply Data Freshness | Age of the most recent supply data available. Reserved. |
| PI‑SI‑205 | Supply Data Completeness | Percentage of expected supply data points received. Reserved. |
| PI‑SI‑206 | Supply Data Quality Score | Composite score of supply data quality (completeness, accuracy, timeliness). Reserved. |
| PI‑SI‑207 | Integration Success Rate (Supply) | Percentage of integration events successfully processed. Reserved. |
| PI‑SI‑208 | Event Processing Latency (Supply) | Time from supply event publication to processing completion. Reserved. |
| PI‑SI‑209 | API Response Time (Supply) | 95th percentile API response time. Reserved. |
| PI‑SI‑210 | System Availability (Supply) | Uptime percentage of supply intelligence services. Reserved. |
| PI‑SI‑211 | Planning Throughput (Supply) | Number of planning items processed per unit time. Reserved. |
| PI‑SI‑212 | Exception Processing Time (Supply) | Time from exception detection to alert generation. Reserved. |

---

# Chapter 4 — Semantic Foundation  

The following concepts establish the enterprise meaning upon which all Supply Intelligence capabilities operate. Each concept is a first‑class enterprise object with a unique identifier and a complete definition. This chapter mirrors the structure of the Demand Intelligence Semantic Foundation, specialized for supply.

## 4.1 Core Enterprise Concepts  

| ID | Concept | Definition |
|----|---------|-------------|
| SE‑SI‑001 | Supply | The ability and intention of the enterprise to provide products or materials to meet demand. Supply is expressed as a quantity available at a location and time, including on‑hand, on‑order, in‑transit, and planned production. Supply is the fundamental unit of Supply Intelligence. |
| SE‑SI‑002 | Supply Plan | A time‑phased projection of planned supply quantities—production, procurement, and transfers—designed to satisfy the demand plan while respecting capacity and material constraints. The supply plan is the authoritative output of the supply planning process. |
| SE‑SI‑003 | Inventory | The stock of goods held by the enterprise at a specific location, including raw materials, work‑in‑progress, and finished goods. Inventory serves as a buffer between supply and demand. |
| SE‑SI‑004 | Capacity | The maximum output a resource (machine, labour, warehouse space, transportation lane) can achieve in a given period, expressed in standard units (hours, units, volume). Capacity may be fixed or flexible (overtime, outsourcing). |
| SE‑SI‑005 | Supplier | An external entity that provides materials, components, or services to the enterprise. Suppliers have contractual terms, performance history, and collaboration agreements. |

## 4.2 Supply Plan Concepts  

| ID | Concept | Definition |
|----|---------|-------------|
| SE‑SI‑010 | Planned Supply Quantity | The quantity of a product that the enterprise intends to make available at a location and time, as recorded in the supply plan. It is the sum of planned production, planned procurement, and planned transfers. |
| SE‑SI‑011 | Supply Plan Horizon | The future time span covered by the supply plan, typically aligned with the demand plan horizon. Different horizons (operational, tactical, strategic) may have different levels of aggregation and constraint detail. |
| SE‑SI‑012 | Supply Constraint | Any limitation that restricts the enterprise’s ability to produce, procure, or move goods. Constraints include capacity limits, material shortages, lead times, minimum order quantities, and regulatory restrictions. |
| SE‑SI‑013 | Supply Variability | The uncertainty or fluctuation in actual supply compared to the plan, arising from production yield variation, supplier delivery variability, or transportation delays. Supply variability influences safety stock and buffering decisions. |
| SE‑SI‑014 | Supply Lead Time | The time between the recognition of a supply need (e.g., a planned order release) and the availability of the goods for use. Supply lead time includes procurement lead time, production lead time, and transportation time. |

## 4.3 Inventory Concepts  

| ID | Concept | Definition |
|----|---------|-------------|
| SE‑SI‑020 | Inventory Position | The total effective inventory at a location, calculated as: On‑Hand Inventory + On‑Order Inventory − Allocated Inventory − Backorders. Inventory position is the basis for replenishment decisions. |
| SE‑SI‑021 | Safety Stock | An additional quantity of inventory held to buffer against demand variability and supply variability during the replenishment lead time. Safety stock is calculated based on desired service level, demand standard deviation, and lead time. |
| SE‑SI‑022 | Reorder Point | The inventory position level that triggers a replenishment order. Typically set as: (Average Demand × Lead Time) + Safety Stock. |
| SE‑SI‑023 | Economic Order Quantity (EOQ) | The order quantity that minimizes the sum of ordering cost and holding cost, given a constant demand rate. EOQ is a classic deterministic model; actual lot sizes may be adjusted for constraints. |
| SE‑SI‑024 | Inventory Policy | The set of rules and parameters governing how an item is replenished: review period, target stock level, reorder point, order quantity, and sourcing rules. Inventory policies are set per product–location and may be dynamic. |
| SE‑SI‑025 | Excess Inventory | Inventory quantity that exceeds the level needed to cover demand over a defined excess threshold (e.g., >12 months of average demand). Excess inventory incurs carrying costs and obsolescence risk. |
| SE‑SI‑026 | Obsolete Inventory | Inventory that has no expected future demand (e.g., end‑of‑life product, expired shelf life) and must be written off or scrapped. |

## 4.4 Capacity Concepts  

| ID | Concept | Definition |
|----|---------|-------------|
| SE‑SI‑030 | Capacity Bucket | A defined time period (e.g., day, week) for which capacity availability and utilization are measured and planned. Capacity buckets aggregate resource availability and load. |
| SE‑SI‑031 | Resource | A specific asset that provides capacity: a machine, a labour pool, a warehouse dock, a transportation lane. Resources have a capacity rate and a cost rate. |
| SE‑SI‑032 | Bottleneck | A resource whose capacity is less than the demand placed upon it, causing a constraint that limits overall throughput. Identifying bottlenecks is critical for supply planning. |
| SE‑SI‑033 | Throughput | The actual output rate of a resource or entire supply chain over a period. Throughput may be less than capacity due to downtime, inefficiency, or constraints. |
| SE‑SI‑034 | Capacity Utilization | The ratio of actual output to available capacity, expressed as a percentage. (Measurement defined as PI‑SI‑005.) |
| SE‑SI‑035 | Capacity Strategy | The enterprise’s approach to managing capacity: level (constant output), chase (adjust to demand), or hybrid. Capacity strategy influences plan stability and cost. |

## 4.5 Supplier Concepts  

| ID | Concept | Definition |
|----|---------|-------------|
| SE‑SI‑040 | Supplier Performance | A quantified assessment of a supplier’s reliability, comprising on‑time delivery, quantity accuracy, quality acceptance, and responsiveness. Supplier performance is used to adjust lead time assumptions and supplier selection. |
| SE‑SI‑041 | Supplier Commitment | A confirmed promise from a supplier to deliver a specified quantity by a specified date. Supplier commitments are integrated into the supply plan to improve plan accuracy. |
| SE‑SI‑042 | Supplier Lead Time | The time from placing a purchase order to the receipt of goods, including processing, manufacturing, and transit. Actual lead time may vary from the supplier’s quoted lead time. |
| SE‑SI‑043 | Supplier Capacity | The maximum output a supplier can dedicate to the enterprise over a period, as agreed or estimated. Supplier capacity constraints may limit procurement quantities. |
| SE‑SI‑044 | Supplier Contract | A formal agreement defining terms, pricing, minimum/maximum quantities, lead times, and service level expectations. Contracts influence procurement decisions and cost models. |

## 4.6 Procurement Concepts  

| ID | Concept | Definition |
|----|---------|-------------|
| SE‑SI‑050 | Purchase Requisition | An internal request for the procurement of goods or services. It specifies the item, quantity, required date, and cost center. Requisitions may be generated automatically by the planning system or manually by a planner. |
| SE‑SI‑051 | Purchase Order | A legally binding document issued to a supplier, specifying items, quantities, prices, delivery dates, and terms. Purchase orders are the execution output of procurement decisions. |
| SE‑SI‑052 | Procurement Policy | The rules governing how procurement recommendations are generated and released: approval requirements, minimum order quantities, supplier allocation rules, and spend authorization levels. |
| SE‑SI‑053 | Procurement Lead Time | The total time from requisition creation to goods receipt. It includes internal approval, supplier lead time, and receiving. |

## 4.7 Production Concepts  

| ID | Concept | Definition |
|----|---------|-------------|
| SE‑SI‑060 | Bill of Materials (BOM) | A structured list of components, sub‑assemblies, and raw materials required to produce a finished good. The BOM defines the dependent demand relationship between finished goods and their inputs. |
| SE‑SI‑061 | Routing | The sequence of operations or work centers required to produce a product. Each step has a standard time, setup time, and resource requirement. |
| SE‑SI‑062 | Production Order | A firm instruction to produce a specific quantity of a product by a specific date, following a defined BOM and routing. Production orders consume capacity and materials. |
| SE‑SI‑063 | Production Schedule | A time‑phased sequence of production orders assigned to specific resources, respecting constraints and sequencing rules. The production schedule is the execution output of scheduling decisions. |
| SE‑SI‑064 | Work Center | A physical or logical grouping of resources that perform the same or similar production operations. A work center has a defined capacity and cost rate. |
| SE‑SI‑065 | Changeover | The time and cost required to switch a resource from producing one product to another. Changeovers influence lot sizing and sequencing. |

## 4.8 Distribution Concepts  

| ID | Concept | Definition |
|----|---------|-------------|
| SE‑SI‑070 | Distribution Network | The set of locations (plants, warehouses, distribution centers, cross‑docks) and the transportation lanes connecting them. The network defines the physical scope of supply planning. |
| SE‑SI‑071 | Transfer Order | An internal order to move inventory from one location to another within the enterprise’s distribution network. Transfer orders are a key mechanism for inventory rebalancing. |
| SE‑SI‑072 | Allocation Rule | The logic used to distribute available supply when demand exceeds supply at a network node. Allocation may be proportional, priority‑based, or fair‑share. |
| SE‑SI‑073 | Distribution Lead Time | The time required to move goods from the source location to the destination, including pick, pack, transit, and receiving. |

## 4.9 Supply Relationships  

| ID | Concept | Definition |
|----|---------|-------------|
| SE‑SI‑080 | Supply‑Demand Balancing | The process of reconciling the supply plan with the demand plan, identifying gaps, and making decisions to resolve imbalances. The result is a feasible, constrained supply plan. |
| SE‑SI‑081 | Substitutability (Supply) | The ability to replace one material or component with another in production, or one finished good with another for a customer. Substitutability creates flexibility in supply planning. |
| SE‑SI‑082 | Co‑Products | Two or more products that are produced simultaneously from the same process (e.g., joint production, by‑products). Co‑product relationships complicate capacity and inventory planning. |
| SE‑SI‑083 | Supply Network | The complete graph of nodes (suppliers, plants, warehouses, customers) and links (transportation lanes, information flows) that constitute the enterprise’s extended supply chain. |
| SE‑SI‑084 | Dependency (Supply) | The relationship where the availability of one item (e.g., a component) is a prerequisite for the production of another (e.g., a finished good), as defined by the BOM. Supply dependencies drive material requirements planning. |

## 4.10 Common Enumerations  

**Supply Plan Type**  

| Value | Description |
|-------|-------------|
| Strategic | Long‑term (1–5 years), aggregate capacity and investment planning |
| Tactical | Medium‑term (3–18 months), family‑level supply‑demand balancing |
| Operational | Short‑term (1–12 weeks), detailed production scheduling and replenishment |

**Inventory Policy Type**  

| Value | Description |
|-------|-------------|
| Periodic Review (T,S) | Inventory position reviewed at fixed intervals; order up to target S |
| Continuous Review (s,Q) | Fixed quantity Q ordered when inventory position reaches reorder point s |
| Continuous Review (s,S) | Variable quantity ordered up to target S when position reaches s |
| Min‑Max (s,S) | Simplified version: order up to S if position falls below s |
| Lot‑for‑Lot | Order exactly the demand quantity, no lot sizing |

**Capacity Strategy**  

| Value | Description |
|-------|-------------|
| Level | Maintain constant capacity, absorbing demand variability with inventory or backorders |
| Chase | Adjust capacity to match demand, using flexible labour, overtime, or subcontracting |
| Hybrid | Mix of level and chase, with a stable base and flexible margin |

**Supply Exception Type** (to be used in Chapter 5.11)  

| Value | Description |
|-------|-------------|
| Shortage | Supply insufficient to meet planned demand |
| Excess | Supply exceeds demand coverage target |
| Late Delivery | Supplier or production delivery not on time |
| Capacity Violation | Planned load exceeds available capacity |
| Quality Failure | Material fails quality inspection |
| Data Gap | Missing supply information |

---

# Chapter 5 — Enterprise Capability Specifications  

## 5.1 Understand Supply  

### 5.1.1 Purpose  

Establish a trusted, complete, and current picture of enterprise supply by consolidating inventory positions, open purchase orders, production orders, in‑transit shipments, supplier commitments, and capacity status. Answers: *“What is the enterprise supply position right now, and what is our basis for knowing it?”* The capability serves as the single source of truth for all downstream supply reasoning, providing cleansed, aggregated supply views and flagging data gaps or quality issues.

### 5.1.2 Business Objectives Served  

- BO‑SI‑001 (Deliver Trusted Supply Understanding)  
- BO‑SI‑003 (Maximize Capacity Utilization) — indirectly, by providing the capacity picture  

### 5.1.3 Enterprise Measures  

- PI‑SI‑101 (Supply Understanding Index)  
- PI‑SI‑206 (Supply Data Quality Score)  
- PI‑SI‑204 (Supply Data Freshness)  
- PI‑SI‑205 (Supply Data Completeness)  

### 5.1.4 Semantic Objects  

| ID | Object | Role |
|----|--------|------|
| SE‑SI‑001 | Supply | Core unit |
| SE‑SI‑003 | Inventory | Inventory position |
| SE‑SI‑004 | Capacity | Capacity status |
| SE‑SI‑005 | Supplier | Source of supply |
| SE‑SI‑020 | Inventory Position | Calculated position |
| SE‑SI‑031 | Resource | Capacity owner |
| SE‑SI‑041 | Supplier Commitment | Supplier‑confirmed supply |
| SE‑SI‑051 | Purchase Order | Procurement in‑process |
| SE‑SI‑062 | Production Order | Production in‑process |

### 5.1.5 Primitive Capabilities Composed  

- **Observe** – captures supply transactions, inventory levels, order status  
- **Understand** – interprets and cleanses data into a unified supply picture  
- **Assess** – evaluates data quality and completeness  

### 5.1.6 Enterprise Inputs  

- Inventory transactions (receipts, issues, adjustments) from ERP/WMS  
- Open purchase orders with supplier confirmations  
- Production order status (released, in‑progress, completed)  
- In‑transit shipment data  
- Supplier commitments and delivery dates  
- Capacity calendars and resource availability  
- Product, location, and supplier master data  

### 5.1.7 Enterprise Understanding Produced  

- Unified inventory position (on‑hand, on‑order, allocated, backorder) for every product–location  
- Open supply picture: all outstanding purchase orders, production orders, and transfers with expected delivery dates  
- Current capacity status per resource: available, utilized, overloaded  
- Supply data quality score per source and per product–location (completeness, freshness, accuracy)  
- Supplier commitment reliability tracker (committed vs. actual delivery history)  

### 5.1.8 Enterprise Outputs  

| ID | Output | Description |
|----|--------|-------------|
| OUT‑SI‑001 | Inventory Position Snapshot | Current on‑hand, on‑order, allocated, backorder per product–location |
| OUT‑SI‑002 | Open Supply Orders | All open purchase orders, production orders, and transfer orders with expected dates |
| OUT‑SI‑003 | Capacity Status | Per‑resource capacity availability and utilization |
| OUT‑SI‑004 | Supply Data Quality Score | Completeness, freshness, and accuracy rating per data source |
| OUT‑SI‑005 | Supplier Commitment Tracker | Committed vs. actual deliveries per supplier |

### 5.1.9 Preconditions  

- Source systems (ERP, WMS, MES) provide inventory and order data at least daily  
- Product, location, and supplier master data are available and maintained  
- Units of measure are standardized or mappable  

### 5.1.10 Capability Dependencies  

None. This is the foundational supply capability.

### 5.1.11 Collaborating Capabilities  

- **Demand Intelligence — Understand Demand** — provides demand context for evaluating supply adequacy  
- **Forecast Demand** — downstream consumer of supply understanding for constrained forecasting  

### 5.1.12 Business Decisions  

---

#### DE‑SI‑010 — Accept Supply Data  

**Purpose:** Validate incoming supply data (inventory updates, order status, supplier confirmations) and decide whether it is trustworthy enough to incorporate into the supply picture.

**Required Understanding:** Data source reliability, timestamp, consistency with recent supply patterns.

**Decision Alternatives:**  
- Accept and integrate immediately  
- Accept with flag (low confidence, stale data)  
- Quarantine for manual review  
- Reject (duplicate, out‑of‑range, source unreliable)  

**Decision Criteria:** Source reliability ≥ threshold (default 90%), timestamp within allowed latency (default 24 hours for inventory, 1 hour for critical orders), value within statistical bounds (no negative inventory unless allowed, no impossible lead times).

**Decision Confidence:** Derived from source reliability index and data freshness.

**Decision Rationale:** *Explainability Template:* “Inventory update for Product X at Location L1 accepted: source reliability 98%, timestamp 15 min old (within 1‑hour window), quantity change from 500 to 520 within expected range. Rule BR‑SI‑010 confirmed.”  

---

##### Rules (for DE‑SI‑010)  

| ID | Rule | Category | Description |
|----|------|----------|-------------|
| BR‑SI‑010 | Supply Data Timeliness Rule | Validation Rule | Inventory data must not be older than the maximum allowed latency (default: 24 hours for inventory, 1 hour for order status). Stale data is flagged and may be accepted with reduced confidence if no fresher data exists. |
| BR‑SI‑011 | Supply Data Range Rule | Validation Rule | Inventory quantities must be non‑negative (unless negative quantities are allowed for consignment or returns with proper documentation). Order quantities must be positive and within reasonable bounds. |
| BR‑SI‑012 | Supply Data Source Reliability Rule | Validation Rule | Data from sources with reliability index < 70% shall be quarantined for manual review. Sources with a sudden drop in reliability (>20% decline in 1 month) shall be flagged for investigation. |
| BR‑SI‑013 | Duplicate Detection Rule (Supply) | Validation Rule | A supply transaction is rejected if its fingerprint (source, type, timestamp, quantity) matches an already‑processed transaction within the same 24‑hour window. |

##### Policies (for DE‑SI‑010)  

| ID | Policy | Category | Description |
|----|--------|----------|-------------|
| PO‑SI‑010 | Supply Data Acceptance Automation Policy | Automation Policy | If all validation rules pass and source reliability ≥ 90%, accept automatically. If any rule fails, route to Supply Data Steward. |

---

#### DE‑SI‑011 — Reconcile Inventory Position  

**Purpose:** Reconcile system‑recorded inventory with physical inventory counts, resolve discrepancies, and produce the authoritative inventory position.

**Required Understanding:** System inventory record, latest cycle count or physical inventory results, recent transactions, discrepancy history.

**Decision Alternatives:**  
- Accept system record (no material discrepancy)  
- Adjust to physical count (discrepancy confirmed)  
- Investigate further (unexplained large discrepancy)  
- Flag as exception (systemic issue with this item/location)  

**Decision Criteria:** Discrepancy threshold (default ±2% or ±10 units, whichever is larger), recurrence pattern, impact on planning accuracy.

**Decision Confidence:** Higher when physical count is recent and discrepancy is small.

**Decision Rationale:** “Inventory for Product Y reconciled: system record 1,000 units, cycle count 1,010 units, discrepancy 1.0% within threshold. System record accepted. Rule BR‑SI‑014 applied.”  

---

##### Rules (for DE‑SI‑011)  

| ID | Rule | Category | Description |
|----|------|----------|-------------|
| BR‑SI‑014 | Inventory Reconciliation Threshold Rule | Validation Rule | If the absolute discrepancy between system record and physical count exceeds 2% AND 10 units, an adjustment is required. Otherwise, the system record is accepted. |
| BR‑SI‑015 | Recurring Discrepancy Rule | Consistency Rule | If the same product–location shows a discrepancy >2% in two consecutive reconciliation cycles, the item is flagged as “suspect” and requires an investigation. |
| BR‑SI‑016 | Reconciliation Documentation Rule | Compliance Rule | Every inventory adjustment must record the reason, the authority, and the date of the physical count. |

##### Policies (for DE‑SI‑011)  

| ID | Policy | Category | Description |
|----|--------|----------|-------------|
| PO‑SI‑011 | Reconciliation Approval Policy | Authorization Policy | Adjustments exceeding 5% of the inventory value or a configurable absolute value require Supply Manager approval. |

---

### 5.1.13 Functional Behaviour  

1. **Ingest** supply transactions, inventory updates, and order status changes from all source systems via APIs or event streams.  
2. **Validate** each incoming data point via DE‑SI‑010 (Accept Supply Data) — rules BR‑SI‑010/011/012/013 and policy PO‑SI‑010 determine acceptance.  
3. **Aggregate** accepted data into the unified supply picture: inventory position, open orders, capacity status.  
4. **Reconcile** inventory positions when physical count data is received via DE‑SI‑011 (Reconcile Inventory Position) — rules BR‑SI‑014/015/016 and policy PO‑SI‑011 govern adjustments.  
5. **Compute** supply data quality scores per source and product–location.  
6. **Publish** supply snapshot events for downstream capabilities.  
7. **Raise events:** `SupplyDataAccepted`, `SupplyDataQuarantined`, `InventoryPositionUpdated`, `InventoryReconciled`, `SupplyPictureUpdated`.  

### 5.1.14 Commands  

| Command | Purpose |
|---------|---------|
| `IngestSupplyData` | Accept a batch of supply transactions |
| `ReconcileInventory` | Execute an inventory reconciliation |
| `RefreshSupplyPicture` | Rebuild the current supply snapshot |
| `FlagSupplyDataIssue` | Manually flag a data quality issue |

### 5.1.15 Events  

| Event | Payload Highlights |
|-------|-------------------|
| `SupplyDataAccepted` | Source, timestamp, product, location, quantity, type |
| `SupplyDataQuarantined` | Source, reason, assigned reviewer |
| `InventoryPositionUpdated` | Product, location, on‑hand, on‑order, allocated, backorder |
| `InventoryReconciled` | Product, location, old value, new value, reason |
| `SupplyPictureUpdated` | Snapshot timestamp, coverage %, data quality score |

### 5.1.16 Queries  

| Query | Description |
|-------|-------------|
| `GetInventoryPosition(product, location)` | Current inventory position |
| `GetOpenOrders(filter)` | All open purchase, production, transfer orders |
| `GetCapacityStatus(resource)` | Current capacity availability and utilization |
| `GetSupplyDataQuality(period)` | Data quality scores by source |

### 5.1.17 Reports  

- **Supply Data Quality Report** – completeness, freshness, accuracy by source  
- **Inventory Reconciliation Report** – discrepancies, adjustments, root causes  

### 5.1.18 Dashboards  

- **Supply Health Dashboard** – real‑time inventory positions, open order status, capacity at a glance  
- **Supply Data Quality Monitor** – source reliability trends, stale data alerts  

### 5.1.19 Software Realization  

```
API (REST / gRPC) 
  → Application Service (SupplyData, InventoryAggregate) 
  → Domain Model (InventoryPosition, SupplyOrder, CapacityStatus) 
  → Repository → Event Store → Projections → Read Model
```  
Data validation uses configurable rules. The read model is optimized for real‑time supply queries. Integration adapters connect to ERP, WMS, and MES systems.

---

## 5.2 Plan Supply  

### 5.2.1 Purpose  

Generate a constrained, feasible, and optimized supply plan across all horizons (strategic, tactical, operational) by balancing demand forecasts against inventory, capacity, material availability, and sourcing constraints. Answers: *“What supply actions should we take to meet demand while respecting constraints and minimizing cost?”* The output is a time‑phased plan of recommended production, procurement, and transfer quantities.

### 5.2.2 Business Objectives Served  

- BO‑SI‑001 (Deliver Trusted Supply Understanding)  
- BO‑SI‑002 (Optimize Inventory Performance)  
- BO‑SI‑003 (Maximize Capacity Utilization)  
- BO‑SI‑005 (Minimize Total Delivered Cost)  
- BO‑SI‑007 (Increase Planning Automation)  

### 5.2.3 Enterprise Measures  

- PI‑SI‑010 (Supply Plan Adherence)  
- PI‑SI‑103 (Capacity Forecast Accuracy)  
- PI‑SI‑105 (Recommendation Quality Index — Supply)  
- PI‑SI‑111 (Supply Plan Confidence Index)  
- PI‑SI‑201 (Supply Planning Cycle Time)  

### 5.2.4 Semantic Objects  

| ID | Object | Role |
|----|--------|------|
| SE‑SI‑002 | Supply Plan | Output |
| SE‑SI‑010 | Planned Supply Quantity | Plan line |
| SE‑SI‑012 | Supply Constraint | Input |
| SE‑SI‑014 | Supply Lead Time | Input |
| SE‑SI‑060 | Bill of Materials | Input for material requirements |
| SE‑SI‑080 | Supply‑Demand Balancing | Core process |
| SE‑SI‑084 | Dependency (Supply) | Material dependencies |

### 5.2.5 Primitive Capabilities Composed  

- **Understand** – interprets demand plan, inventory, constraints  
- **Predict** – projects supply outcomes under constraints  
- **Evaluate** – compares alternative supply plans  
- **Learn** – improves plan quality over time  

### 5.2.6 Enterprise Inputs  

- Demand plan (from Demand Intelligence — Forecast Demand capability)  
- Current inventory position (from Understand Supply)  
- Open supply orders (from Understand Supply)  
- Capacity availability (from Understand Supply)  
- Bill of materials and routings (from master data)  
- Supplier lead times and commitments  
- Planning parameters: safety stock targets, lot sizes, frozen period  

### 5.2.7 Enterprise Understanding Produced  

- Time‑phased supply plan: recommended production orders, purchase orders, transfer orders per period  
- Supply‑demand balance summary: gaps, surpluses, constraints  
- Plan confidence score (based on data quality and constraint satisfaction)  
- Constraint utilization report: which constraints are binding, slack, or violated  

### 5.2.8 Enterprise Outputs  

| ID | Output | Description |
|----|--------|-------------|
| OUT‑SI‑010 | Constrained Supply Plan | Time‑phased planned production, procurement, and transfer quantities |
| OUT‑SI‑011 | Supply‑Demand Balance Report | Gap/surplus by period, product, location |
| OUT‑SI‑012 | Constraint Analysis | Binding constraints, violations, slack |
| OUT‑SI‑013 | Plan Confidence Score | Aggregate confidence in plan feasibility |

### 5.2.9 Preconditions  

- Demand plan is available for the full planning horizon  
- Inventory position is current and accurate  
- Capacity and BOM data are available  
- Planning parameters are configured  

### 5.2.10 Capability Dependencies  

- `CA‑SI‑001 Understand Supply` — for inventory and capacity data  
- `CA‑DI‑002 Forecast Demand` — for the demand plan  

### 5.2.11 Collaborating Capabilities  

- **Manage Inventory** — consumes supply plan for inventory policy execution  
- **Manage Capacity** — consumes supply plan for capacity planning feedback  
- **Procure Materials** — consumes supply plan for procurement recommendations  

### 5.2.12 Business Decisions  

---

#### DE‑SI‑020 — Select Supply Planning Model  

**Purpose:** Choose the optimization model (e.g., linear programming, heuristic, simulation) to generate the supply plan for the current cycle.

**Required Understanding:** Planning horizon, constraint type and severity, data quality, computational time budget, required accuracy.

**Decision Alternatives:**  
- Exact optimization (e.g., LP/MIP)  
- Heuristic (e.g., priority‑based sequencing)  
- Hybrid (exact for tactical, heuristic for operational)  
- Maintain current model  

**Decision Criteria:** Solution quality (cost, constraint satisfaction), solve time within cycle window, stability of results.

**Decision Confidence:** Based on historical performance of the selected model.

**Decision Rationale:** “Heuristic model selected for operational horizon (1‑4 weeks) due to tight solve time (<15 min). LP model selected for tactical horizon (1‑6 months). Both models meet historical accuracy targets. Rule BR‑SI‑020 applied.”  

---

##### Rules (for DE‑SI‑020)  

| ID | Rule | Category | Description |
|----|------|----------|-------------|
| BR‑SI‑020 | Planning Model Selection Rule | Derivation Rule | Select the model based on horizon: Operational (1–4 weeks) → Heuristic if solve time budget < 15 min, else LP. Tactical (1–18 months) → LP. Strategic (>18 months) → Aggregate LP. |
| BR‑SI‑021 | Model Performance Monitoring Rule | Model Evaluation Rule | If a model’s plan cost deviates from the optimal (or best achievable) benchmark by >5% over a rolling 3‑month period, trigger a model review. |

##### Policies (for DE‑SI‑020)  

| ID | Policy | Category | Description |
|----|--------|----------|-------------|
| PO‑SI‑020 | Model Selection Override Policy | Authorization Policy | A Supply Planning Manager may override the model selection if a documented business reason exists (e.g., exceptional constraint complexity). |

---

#### DE‑SI‑021 — Generate Supply Plan  

**Purpose:** Execute the selected planning model, apply constraints, and produce the initial draft supply plan.

**Required Understanding:** Demand plan, inventory, open orders, capacity, BOM, lead times, planning parameters.

**Decision Alternatives:** The decision is deterministic in execution, but the outcome is a plan. The choice is whether to accept the plan as generated, or flag for manual adjustment if infeasibilities exist.

**Decision Criteria:** All hard constraints satisfied; soft constraints violated only when necessary and documented.

**Decision Confidence:** Based on data completeness and constraint satisfaction degree.

**Decision Rationale:** “Supply plan generated for horizon W27‑W39. 98% of demand satisfied. 2% backordered due to binding capacity constraint at WC‑100 (documented). All hard constraints satisfied. Plan confidence: 91%. Rule BR‑SI‑022 passed.”  

---

##### Rules (for DE‑SI‑021)  

| ID | Rule | Category | Description |
|----|------|----------|-------------|
| BR‑SI‑022 | Hard Constraint Rule | Validation Rule | The plan must not violate hard constraints (capacity cannot be negative, BOM dependencies must be satisfied, lead times must be respected). Violations cause the plan to be rejected and re‑optimized. |
| BR‑SI‑023 | Soft Constraint Documentation Rule | Compliance Rule | Any violation of a soft constraint (e.g., exceeding target inventory) must be documented with the reason and business impact. |
| BR‑SI‑024 | Plan Completeness Rule | Validation Rule | The plan must cover at least 95% of active product–locations. Below that threshold, the plan is considered incomplete and is not published. |

##### Policies (for DE‑SI‑021)  

| ID | Policy | Category | Description |
|----|--------|----------|-------------|
| PO‑SI‑021 | Infeasibility Escalation Policy | Exception Policy | If the plan is infeasible (hard constraints cannot be satisfied), the Supply Planning Manager is immediately notified and a constraint relaxation review is triggered. |

---

#### DE‑SI‑022 — Evaluate Supply Plan Quality  

**Purpose:** Assess the generated supply plan against business KPIs (cost, service, inventory, capacity utilization) and determine whether it meets quality thresholds for publication.

**Required Understanding:** Draft supply plan, business targets for cost, service level, inventory turns, capacity utilization.

**Decision Alternatives:**  
- Accept and publish  
- Accept with warnings (minor deviations)  
- Reject and re‑plan (major deviations)  

**Decision Criteria:** Plan cost within budget variance (±5%), projected service level ≥ target (≥95%), capacity utilization within target range (85–95%), no critical shortages.

**Decision Confidence:** Aggregate of plan feasibility and data quality.

**Decision Rationale:** “Supply plan evaluated: total cost $5.2M (within budget), projected service level 96%, capacity utilization 89%. All metrics within target. Rule BR‑SI‑025 passed. Plan recommended for publication.”  

---

##### Rules (for DE‑SI‑022)  

| ID | Rule | Category | Description |
|----|------|----------|-------------|
| BR‑SI‑025 | Plan Quality Threshold Rule | Validation Rule | The plan must meet all minimum quality thresholds: service level ≥ 95%, capacity utilization ≤ 98%, total cost within 105% of budget. Failure triggers re‑plan or escalation. |
| BR‑SI‑026 | Plan Stability Rule | Consistency Rule | The new plan must not deviate from the previously published plan by more than 20% in total volume for the first 4 weeks, unless driven by a confirmed demand change. Excessive churn is flagged for review. |

##### Policies (for DE‑SI‑022)  

| ID | Policy | Category | Description |
|----|--------|----------|-------------|
| PO‑SI‑022 | Plan Acceptance Automation Policy | Automation Policy | If all quality thresholds (BR‑SI‑025) are met and plan stability (BR‑SI‑026) is within limits, the plan is published automatically. Otherwise, Supply Planner approval is required. |

---

#### DE‑SI‑023 — Publish Supply Plan  

**Purpose:** Finalize and release the supply plan to downstream capabilities and execution systems.

**Required Understanding:** Approved supply plan, publication metadata.

**Decision Alternatives:**  
- Publish as final  
- Publish with caveats (specific items flagged)  
- Hold publication (unresolved issues)  

**Decision Criteria:** Plan approval status, completeness, all mandatory sign‑offs obtained.

**Decision Confidence:** High if all prior decisions passed.

**Decision Rationale:** “Supply Plan v2026‑W27 published: all quality checks passed, approved by Supply Manager. Published to Procure Materials, Schedule Production, and Manage Distribution capabilities.”  

---

##### Rules (for DE‑SI‑023)  

| ID | Rule | Category | Description |
|----|------|----------|-------------|
| BR‑SI‑027 | Publication Authorization Rule | Validation Rule | The plan may only be published if it has been accepted (DE‑SI‑022) or approved (manual override). |
| BR‑SI‑028 | Versioning Rule | Compliance Rule | Every published plan receives a unique version identifier and is stored immutably for audit. |

##### Policies (for DE‑SI‑023)  

| ID | Policy | Category | Description |
|----|--------|----------|-------------|
| PO‑SI‑023 | Plan Publication Policy | Compliance Policy | The supply plan shall be published by 10:00 AM every Monday for the operational horizon, and by the 5th business day of the month for the tactical horizon. |

---

### 5.2.13 Functional Behaviour  

1. **Trigger:** Scheduled (weekly for operational, monthly for tactical) or event‑driven (demand plan update, major supply disruption).  
2. **Retrieve** demand plan, inventory position, open orders, capacity, BOM, and planning parameters.  
3. **Execute DE‑SI‑020** (Select Planning Model) — rules BR‑SI‑020/021, policy PO‑SI‑020.  
4. **Execute DE‑SI‑021** (Generate Supply Plan) using the selected model — rules BR‑SI‑022/023/024, policy PO‑SI‑021 for infeasibility.  
5. **Execute DE‑SI‑022** (Evaluate Supply Plan Quality) — rules BR‑SI‑025/026, policy PO‑SI‑022.  
6. **If accepted,** execute DE‑SI‑023 (Publish Supply Plan) — rules BR‑SI‑027/028, policy PO‑SI‑023.  
7. **Raise events:** `SupplyPlanGenerated`, `SupplyPlanEvaluated`, `SupplyPlanPublished`, `SupplyPlanInfeasible`.  

### 5.2.14 Commands  

| Command | Purpose |
|---------|---------|
| `StartSupplyPlanningCycle` | Initiates a new planning cycle |
| `SelectPlanningModel` | Select or override the planning model |
| `GenerateSupplyPlan` | Execute plan generation |
| `EvaluateSupplyPlan` | Run quality evaluation |
| `PublishSupplyPlan` | Release the approved plan |

### 5.2.15 Events  

| Event | Payload Highlights |
|-------|-------------------|
| `SupplyPlanGenerated` | Cycle ID, horizon, plan metrics |
| `SupplyPlanEvaluated` | Quality scores, violations |
| `SupplyPlanPublished` | Plan version, published timestamp, scope |
| `SupplyPlanInfeasible` | Cycle ID, violated constraints |

### 5.2.16 Queries  

| Query | Description |
|-------|-------------|
| `GetSupplyPlan(cycleId, product, location)` | Current or historical plan quantities |
| `GetSupplyDemandBalance(cycleId)` | Gap/surplus summary |
| `GetConstraintAnalysis(cycleId)` | Binding constraints and violations |

### 5.2.17 Reports  

- **Supply Plan Accuracy Report** – adherence, deviation analysis  
- **Constraint Utilization Report** – bottleneck analysis  

### 5.2.18 Dashboards  

- **Supply Plan Dashboard** – plan summary, gaps, confidence  
- **Constraint Monitor** – real‑time constraint status  

### 5.2.19 Software Realization  

```
API → Application Service (PlanningCycle, SupplyPlan aggregates)  
→ Optimization Engine (pluggable: LP solver, heuristics)  
→ Domain Model (SupplyPlan, ConstraintSet)  
→ Event Store → Projections → Read Model  
```  
The optimization engine is pluggable, allowing different solvers for different horizons. The domain model enforces constraint satisfaction rules before publication.

---

## 5.3 Manage Inventory  

### 5.3.1 Purpose  

Establish, maintain, and optimize inventory policies (safety stock, reorder points, lot sizes) for every product–location combination. Continuously evaluate inventory health, project future inventory positions, and generate replenishment recommendations that balance service, cost, and risk. Answers: *“What inventory should we hold, and when should we replenish?”*

### 5.3.2 Business Objectives Served  

- BO‑SI‑002 (Optimize Inventory Performance)  
- BO‑SI‑004 (Ensure Supply Continuity)  
- BO‑SI‑005 (Minimize Total Delivered Cost)  
- BO‑SI‑007 (Increase Planning Automation)  

### 5.3.3 Enterprise Measures  

- PI‑SI‑002 (Inventory Turnover)  
- PI‑SI‑003 (Days of Supply)  
- PI‑SI‑013 (Excess & Obsolete Inventory)  
- PI‑SI‑012 (Stockout Frequency)  
- PI‑SI‑102 (Inventory Optimization Effectiveness)  

### 5.3.4 Semantic Objects  

| ID | Object | Role |
|----|--------|------|
| SE‑SI‑020 | Inventory Position | Input |
| SE‑SI‑021 | Safety Stock | Policy output |
| SE‑SI‑022 | Reorder Point | Policy output |
| SE‑SI‑023 | Economic Order Quantity (EOQ) | Lot sizing |
| SE‑SI‑024 | Inventory Policy | Policy definition |
| SE‑SI‑025 | Excess Inventory | Exception |
| SE‑SI‑026 | Obsolete Inventory | Exception |
| SE‑SI‑014 | Supply Lead Time | Input |
| SE‑SI‑012 | Supply Variability | Input |
| SE‑DI‑012 | Demand Variability | Input (from Demand Intelligence) |

### 5.3.5 Primitive Capabilities Composed  

- **Understand** – interprets demand patterns, lead times, cost parameters  
- **Predict** – projects future inventory position  
- **Evaluate** – assesses trade‑offs between cost and service  
- **Learn** – continuously improves policy parameters  

### 5.3.6 Enterprise Inputs  

- Demand forecast and demand variability (from Demand Intelligence)  
- Current inventory position and open supply orders (from Understand Supply)  
- Supply lead times and supply variability (from Understand Supply / Supplier data)  
- Cost parameters: holding cost, ordering cost, stockout cost  
- Service level targets by segment  
- Current inventory policies (safety stock, reorder points, lot sizes)  

### 5.3.7 Enterprise Understanding Produced  

- Recommended inventory policies: safety stock, reorder point, target stock level, lot size per product–location  
- Projected inventory position over the planning horizon, including expected shortages and excesses  
- Inventory health classification: optimal, under‑stocked, over‑stocked, at risk of obsolescence  
- Replenishment recommendations: when and how much to order  

### 5.3.8 Enterprise Outputs  

| ID | Output | Description |
|----|--------|-------------|
| OUT‑SI‑020 | Inventory Policy Set | Safety stock, reorder point, target stock, lot size per product–location |
| OUT‑SI‑021 | Inventory Projection | Time‑phased projected inventory position, including expected shortages/excesses |
| OUT‑SI‑022 | Replenishment Recommendation | Recommended order quantity and timing |
| OUT‑SI‑023 | Inventory Health Status | Classification and alerts for each product–location |

### 5.3.9 Preconditions  

- Demand forecast and variability data are available  
- Inventory position data is current  
- Cost parameters and service level targets are configured  

### 5.3.10 Capability Dependencies  

- `CA‑DI‑002 Forecast Demand` – for demand forecast and variability  
- `CA‑SI‑001 Understand Supply` – for inventory position and lead times  

### 5.3.11 Collaborating Capabilities  

- **Plan Supply** – consumes replenishment recommendations to generate the supply plan  
- **Evaluate Supply Quality** – receives inventory health data for performance measurement  

### 5.3.12 Business Decisions  

---

#### DE‑SI‑030 — Set Inventory Policy  

**Purpose:** Determine the optimal inventory control parameters (safety stock, reorder point, lot size) for each product–location combination based on demand and supply characteristics, cost, and service targets.

**Required Understanding:** Demand forecast, demand variability, supply lead time, supply variability, holding cost, ordering cost, stockout cost, service level target.

**Decision Alternatives:**  
- Apply statistical safety stock formula (e.g., SS = Z × σ × √L)  
- Apply advanced optimization (e.g., simulation, multi‑echelon)  
- Apply manual override (planner‑set)  
- Maintain current policy  

**Decision Criteria:** Minimize total inventory cost (holding + ordering + stockout) subject to service level constraint.

**Discovered Alternatives:** The Learn capability may propose a new policy methodology, validated by an Alternative Validation Rule.

**Decision Confidence:** Based on data quality and model fit.

**Decision Rationale:** *Explainability Template:* “Safety stock for Product X set to 250 units, using Z=1.65 (95% service), demand σ=50/week, lead time=4 weeks. Policy minimizes expected cost while meeting service target. Rule BR‑SI‑030 applied.”  

---

##### Rules (for DE‑SI‑030)  

| ID | Rule | Category | Description |
|----|------|----------|-------------|
| BR‑SI‑030 | Safety Stock Calculation Rule | Calculation Rule | Safety stock = Z × σ × √L, where Z is the service factor for the target service level, σ is the demand standard deviation over lead time, and L is the lead time in periods. For items with high demand variability (CV > 1.0), a simulation‑based safety stock may be used. |
| BR‑SI‑031 | Lot Size Calculation Rule | Calculation Rule | Lot size is determined using EOQ if demand is relatively stable (CV < 0.5); otherwise, periodic review with target stock or a minimum‑maximum policy is applied. |
| BR‑SI‑032 | Policy Consistency Rule | Consistency Rule | Inventory policy parameters shall not change more than ±20% from the previous cycle unless a structural change in demand or supply is confirmed. |

##### Policies (for DE‑SI‑030)  

| ID | Policy | Category | Description |
|----|--------|----------|-------------|
| PO‑SI‑030 | Inventory Policy Override Policy | Authorization Policy | A Supply Planner may override a computed inventory policy with justification. Overrides are tracked and reviewed monthly. |

---

#### DE‑SI‑031 — Generate Replenishment Recommendation  

**Purpose:** Determine when and how much to order for each product–location, based on current inventory position, projected demand, and inventory policy.

**Required Understanding:** Current inventory position, demand forecast, inventory policy (safety stock, reorder point, lot size), open supply orders, supply lead time.

**Decision Alternatives:**  
- Order now (inventory at or below reorder point)  
- Defer order (inventory above reorder point)  
- Expedite (shortage risk)  
- Cancel / defer existing orders (excess inventory)  

**Decision Criteria:** Reorder point rule: if projected inventory position ≤ reorder point within lead time, recommend order. Order quantity determined by lot size policy.

**Decision Confidence:** Based on forecast accuracy and supply lead time certainty.

**Decision Rationale:** “Replenishment for Product Z recommended: current position 120 units, projected to reach reorder point (100) in 2 weeks. Recommend order of 500 units (EOQ). Rule BR‑SI‑033 triggered.”  

---

##### Rules (for DE‑SI‑031)  

| ID | Rule | Category | Description |
|----|------|----------|-------------|
| BR‑SI‑033 | Reorder Point Rule | Derivation Rule | An order is recommended when the projected inventory position at the end of lead time is ≤ reorder point. |
| BR‑SI‑034 | Lot Sizing Rule | Derivation Rule | The recommended order quantity shall be the defined lot size (EOQ, min‑max, or period demand). If expediting, the quantity may be increased to cover the shortage plus a buffer. |
| BR‑SI‑035 | Excess Inventory Action Rule | Validation Rule | If the projected inventory exceeds the maximum target (e.g., 12 months of demand), a recommendation to defer or cancel open orders is generated. |

##### Policies (for DE‑SI‑031)  

| ID | Policy | Category | Description |
|----|--------|----------|-------------|
| PO‑SI‑031 | Replenishment Automation Policy | Automation Policy | Replenishment recommendations for items with low variability (XYZ‑X) and high confidence forecast may be automatically converted to purchase requisitions. All other recommendations require planner approval. |
| PO‑SI‑032 | Expediting Authorization Policy | Authorization Policy | Expediting requests must be approved by the Supply Manager if the additional cost exceeds a defined threshold. |

---

#### DE‑SI‑032 — Assess Inventory Health  

**Purpose:** Classify inventory items by health status (optimal, under‑stocked, over‑stocked, obsolete) and trigger corrective actions.

**Required Understanding:** Projected inventory, demand coverage, life‑cycle status, obsolescence risk.

**Decision Alternatives:**  
- Optimal (no action)  
- Under‑stocked (trigger replenishment or escalation)  
- Over‑stocked (reduce orders, consider transfers)  
- Obsolete (recommend write‑off)  

**Decision Criteria:** Coverage days vs. policy target, demand trends, product life‑cycle.

**Decision Confidence:** High if demand is stable; moderate for intermittent items.

**Decision Rationale:** “Product W classified as over‑stocked: 14 months of demand coverage, policy target 3 months. Recommendation: defer open PO. Rule BR‑SI‑036.”  

---

##### Rules (for DE‑SI‑032)  

| ID | Rule | Category | Description |
|----|------|----------|-------------|
| BR‑SI‑036 | Inventory Health Classification Rule | Derivation Rule | Under‑stocked: Days of Supply < 50% of target. Over‑stocked: Days of Supply > 200% of target. Obsolete: no demand in last 12 months and end‑of‑life flag. |
| BR‑SI‑037 | Obsolete Inventory Review Rule | Compliance Rule | Items flagged as obsolete must be reviewed quarterly by Finance and Supply Chain for write‑off. |

##### Policies (for DE‑SI‑032)  

| ID | Policy | Category | Description |
|----|--------|----------|-------------|
| PO‑SI‑033 | Overstock Disposition Policy | Compliance Policy | Over‑stocked items above a value threshold require a disposition plan within 30 days. |

---

### 5.3.13 Functional Behaviour  

1. **Scheduled trigger:** Daily for replenishment, weekly for policy review, monthly for health assessment.  
2. **Retrieve** demand forecast, variability, inventory position, lead times, cost parameters, current policies.  
3. **Execute DE‑SI‑030** (Set Inventory Policy) — rules BR‑SI‑030/031/032, policy PO‑SI‑030.  
4. **Execute DE‑SI‑031** (Generate Replenishment Recommendation) for each product–location — rules BR‑SI‑033/034/035, policies PO‑SI‑031/032.  
5. **Execute DE‑SI‑032** (Assess Inventory Health) — rules BR‑SI‑036/037, policy PO‑SI‑033.  
6. **Publish** policy updates, replenishment recommendations, and health alerts.  
7. **Raise events:** `InventoryPolicyUpdated`, `ReplenishmentRecommended`, `InventoryHealthAlert`.  

### 5.3.14 Commands  

| Command | Purpose |
|---------|---------|
| `SetInventoryPolicy` | Update policy parameters for a product‑location |
| `GenerateReplenishment` | Run replenishment calculation |
| `AssessInventoryHealth` | Run health classification |

### 5.3.15 Events  

| Event | Payload Highlights |
|-------|-------------------|
| `InventoryPolicyUpdated` | Product, location, new safety stock, reorder point |
| `ReplenishmentRecommended` | Product, location, order quantity, due date |
| `InventoryHealthAlert` | Product, location, health status, reason |

### 5.3.16 Queries  

| Query | Description |
|-------|-------------|
| `GetInventoryPolicy(product, location)` | Current policy parameters |
| `GetReplenishmentRecommendations(filter)` | Active replenishment suggestions |
| `GetInventoryHealth(scope)` | Health status summary |

### 5.3.17 Reports  

- **Inventory Policy Compliance Report** – adherence to recommended vs. overridden policies  
- **Replenishment Action Report** – orders generated, deferred, expedited  
- **Inventory Health Report** – distribution of health status, aging of excess  

### 5.3.18 Dashboards  

- **Inventory Optimization Dashboard** – policy performance, safety stock vs. actual, service vs. cost trade‑off  
- **Replenishment Workbench** – planner actions, overdue orders, recommended orders  

### 5.3.19 Software Realization  

```
API → Application Service → Domain Model (InventoryPolicy, ReplenishmentPlan)  
→ Computation Engine (statistical safety stock, simulation option)  
→ Event Store → Projections → Read Model
```  
The computation engine supports multiple methods, configured per segment. Policies are versioned for audit.

---

## 5.4 Manage Capacity  

### 5.4.1 Purpose  

Model, monitor, and plan the utilization of enterprise resources (machines, labour, warehouse space, transportation lanes) to ensure that the supply plan is feasible and that bottlenecks are identified early. Answers: *“Do we have enough capacity to execute the plan? Where are the constraints, and how can we resolve them?”*

### 5.4.2 Business Objectives Served  

- BO‑SI‑003 (Maximize Capacity Utilization)  
- BO‑SI‑005 (Minimize Total Delivered Cost)  
- BO‑SI‑007 (Increase Planning Automation)  

### 5.4.3 Enterprise Measures  

- PI‑SI‑005 (Capacity Utilization)  
- PI‑SI‑006 (Schedule Adherence)  
- PI‑SI‑103 (Capacity Forecast Accuracy)  

### 5.4.4 Semantic Objects  

| ID | Object | Role |
|----|--------|------|
| SE‑SI‑004 | Capacity | Core concept |
| SE‑SI‑030 | Capacity Bucket | Time bucket |
| SE‑SI‑031 | Resource | Asset |
| SE‑SI‑032 | Bottleneck | Constraint |
| SE‑SI‑033 | Throughput | Output rate |
| SE‑SI‑035 | Capacity Strategy | Policy |
| SE‑SI‑065 | Changeover | Operational factor |

### 5.4.5 Primitive Capabilities Composed  

- **Understand** – models capacity availability and load  
- **Predict** – projects future utilization  
- **Assess** – identifies bottlenecks and evaluates alternatives  
- **Evaluate** – compares capacity options (overtime, outsourcing)  

### 5.4.6 Enterprise Inputs  

- Resource master data (capacity rates, calendars, shifts)  
- Current capacity utilization (from Understand Supply)  
- Supply plan (from Plan Supply)  
- Production routings and changeover times  
- Capacity strategy parameters (overtime limits, outsourcing options)  

### 5.4.7 Enterprise Understanding Produced  

- Time‑phased capacity load vs. availability per resource  
- Bottleneck identification and impact assessment  
- Capacity utilization forecasts  
- Recommended capacity adjustments (overtime, outsourcing, shift changes)  

### 5.4.8 Enterprise Outputs  

| ID | Output | Description |
|----|--------|-------------|
| OUT‑SI‑030 | Capacity Load Profile | Planned load vs. available capacity per resource, per bucket |
| OUT‑SI‑031 | Bottleneck Report | Identified constraints, queue lengths, throughput impact |
| OUT‑SI‑032 | Capacity Adjustment Recommendations | Overtime, outsourcing, or shift changes proposed |

### 5.4.9 Preconditions  

- Resource master and calendars are current  
- Supply plan is published  
- Routing data is available for all products  

### 5.4.10 Capability Dependencies  

- `CA‑SI‑001 Understand Supply` – for current capacity status  
- `CA‑SI‑002 Plan Supply` – for the supply plan to evaluate  

### 5.4.11 Collaborating Capabilities  

- **Plan Supply** – receives feedback to refine the plan  
- **Schedule Production** – consumes capacity availability for detailed scheduling  

### 5.4.12 Business Decisions  

---

#### DE‑SI‑040 — Assess Capacity Feasibility  

**Purpose:** Evaluate whether the current supply plan is capacity‑feasible, identify violations, and recommend resolutions.

**Required Understanding:** Capacity load profile, resource availability, capacity strategy (level/chase/hybrid).

**Decision Alternatives:**  
- Feasible (no action)  
- Overloaded (recommend adjustments)  
- Underloaded (flag opportunity)  

**Decision Criteria:** Load vs. capacity per bucket; overload > 100% requires resolution; underload < 70% may trigger cost review.

**Decision Confidence:** Based on plan stability and demand certainty.

**Decision Rationale:** “Resource WC‑100 overloaded in W30‑W32 (utilization 115%). Recommended: 10% overtime or shift 20% of load to alternate resource WC‑102. Rule BR‑SI‑040 applied.”  

---

##### Rules (for DE‑SI‑040)  

| ID | Rule | Category | Description |
|----|------|----------|-------------|
| BR‑SI‑040 | Capacity Overload Rule | Validation Rule | If capacity utilization exceeds 100% in any bucket, an overload resolution is required: adjust plan, add overtime, or outsource. |
| BR‑SI‑041 | Underload Alert Rule | Validation Rule | If utilization is < 50% for a resource over 4 consecutive weeks, an underload alert is raised for cost review. |
| BR‑SI‑042 | Bottleneck Impact Rule | Derivation Rule | The bottleneck resource with the highest utilization constraining overall throughput is identified and its impact on total output is quantified. |

##### Policies (for DE‑SI‑040)  

| ID | Policy | Category | Description |
|----|--------|----------|-------------|
| PO‑SI‑040 | Overtime Authorization Policy | Authorization Policy | Overtime up to 20% of base capacity is allowed without approval if overload is forecast. Above 20%, Plant Manager approval is required. |
| PO‑SI‑041 | Outsourcing Policy | Authorization Policy | Outsourcing of production requires Supply Chain Director approval and must be reviewed quarterly. |

---

#### DE‑SI‑041 — Publish Capacity Plan  

**Purpose:** Finalize the capacity plan and distribute it to execution and planning systems.

**Decision Alternatives:** Publish, Hold.

**Decision Criteria:** All overloads resolved or documented.

**Decision Confidence:** Derived from plan feasibility.

**Decision Rationale:** “Capacity plan published: 2 overloads resolved, 0 outstanding. Plan feasible.”  

---

##### Rules (for DE‑SI‑041)  

| ID | Rule | Category | Description |
|----|------|----------|-------------|
| BR‑SI‑043 | Capacity Plan Publication Rule | Validation Rule | The capacity plan shall not be published if any resource has an unresolved overload. |

##### Policies (for DE‑SI‑041)  

| ID | Policy | Category | Description |
|----|--------|----------|-------------|
| PO‑SI‑042 | Capacity Plan Publication Policy | Automation Policy | The capacity plan is published automatically with the supply plan if feasible. |

---

### 5.4.13 Functional Behaviour  

1. **Trigger:** After supply plan generation, before publication, or on capacity calendar changes.  
2. **Load** supply plan quantities onto resources using routings and BOM.  
3. **Execute DE‑SI‑040** (Assess Capacity Feasibility) — rules BR‑SI‑040/041/042, policies PO‑SI‑040/041.  
4. **Generate** capacity adjustment recommendations if needed.  
5. **Execute DE‑SI‑041** (Publish Capacity Plan) — rule BR‑SI‑043, policy PO‑SI‑042.  
6. **Raise events:** `CapacityFeasibilityAssessed`, `CapacityPlanPublished`.  

### 5.4.14 Commands  

| Command | Purpose |
|---------|---------|
| `AssessCapacityFeasibility` | Run capacity evaluation |
| `PublishCapacityPlan` | Finalize and distribute |

### 5.4.15 Events  

| Event | Payload Highlights |
|-------|-------------------|
| `CapacityFeasibilityAssessed` | Resource, overload/underload, recommendations |
| `CapacityPlanPublished` | Plan version, timestamp |

### 5.4.16 Queries  

| Query | Description |
|-------|-------------|
| `GetCapacityLoad(resource, period)` | Load vs. capacity |
| `GetBottlenecks()` | Active bottlenecks |

### 5.4.17 Reports  

- **Capacity Utilization Report** – actual vs. planned utilization  
- **Bottleneck Analysis Report** – constraints and throughput impact  

### 5.4.18 Dashboards  

- **Capacity Control Tower** – real‑time load/capacity gauges  
- **Resource Utilization Heatmap** – visual overload/underload  

### 5.4.19 Software Realization  

```
API → Application Service → Domain Model (CapacityPlan, ResourceLoad)  
→ Calculation Engine (load propagation via routings)  
→ Event Store → Projections → Read Model
```  
Capacity calendars are stored in master data. The engine supports what‑if scenarios for capacity adjustments.

---

## 5.5 Collaborate with Suppliers  

### 5.5.1 Purpose  

Enable collaborative planning with suppliers by sharing demand forecasts, receiving and evaluating supplier commitments, assessing supplier reliability, and jointly managing supply risk. Answers: *“What do our suppliers commit to deliver, and how can we work together to improve supply reliability?”*

### 5.5.2 Business Objectives Served  

- BO‑SI‑006 (Improve Supplier Collaboration)  
- BO‑SI‑004 (Ensure Supply Continuity)  

### 5.5.3 Enterprise Measures  

- PI‑SI‑007 (Supplier On‑Time Delivery)  
- PI‑SI‑104 (Supplier Risk Score)  
- PI‑SI‑109 (Supplier Collaboration Index)  

### 5.5.4 Semantic Objects  

| ID | Object | Role |
|----|--------|------|
| SE‑SI‑005 | Supplier | External entity |
| SE‑SI‑040 | Supplier Performance | Evaluation |
| SE‑SI‑041 | Supplier Commitment | Confirmed delivery |
| SE‑SI‑042 | Supplier Lead Time | Planning parameter |
| SE‑SI‑043 | Supplier Capacity | Constraint |
| SE‑SI‑044 | Supplier Contract | Terms |

### 5.5.5 Primitive Capabilities Composed  

- **Observe** – tracks supplier performance  
- **Understand** – evaluates commitments against history  
- **Assess** – determines supplier risk  
- **Evaluate** – compares supplier scenarios  
- **Learn** – improves collaboration strategies  

### 5.5.6 Enterprise Inputs  

- Supplier master data and contracts  
- Supplier commitments (acknowledged delivery dates, quantities)  
- Purchase order history and delivery performance  
- Demand forecast (to share)  
- Supplier capacity information  
- External risk data (financial, geopolitical) if available  

### 5.5.7 Enterprise Understanding Produced  

- Supplier scorecards: on‑time delivery, quality, responsiveness  
- Supplier risk assessments  
- Integrated supplier commitments into supply picture  
- Recommendations for supplier development or escalation  
- Supplier capacity outlook  

### 5.5.8 Enterprise Outputs  

| ID | Output | Description |
|----|--------|-------------|
| OUT‑SI‑040 | Supplier Scorecard | Performance metrics per supplier |
| OUT‑SI‑041 | Supplier Commitment Schedule | Confirmed deliveries integrated into supply plan |
| OUT‑SI‑042 | Supplier Risk Report | Risk scores and mitigation recommendations |
| OUT‑SI‑043 | Supplier Collaboration Plan | Forecast share recommendations, improvement actions |

### 5.5.9 Preconditions  

- Supplier master data is current  
- Purchase order and delivery history is available  
- Demand forecasts are available for sharing (with appropriate aggregation)  

### 5.5.10 Capability Dependencies  

- `CA‑DI‑002 Forecast Demand` – to share forecasts  
- `CA‑SI‑001 Understand Supply` – for current purchase orders  

### 5.5.11 Collaborating Capabilities  

- **Procure Materials** – consumes supplier commitments for procurement decisions  
- **Plan Supply** – consumes commitment‑adjusted supply picture  

### 5.5.12 Business Decisions  

---

#### DE‑SI‑050 — Evaluate Supplier Commitment  

**Purpose:** Assess the reliability of a supplier’s delivery commitment based on historical performance and current risk factors, and decide whether to accept the commitment as firm or apply a confidence factor.

**Required Understanding:** Supplier’s promised delivery date and quantity, historical on‑time delivery rate, current risk score, lead time.

**Decision Alternatives:**  
- Accept as firm (high confidence)  
- Accept with buffer (add safety lead time or partial quantity)  
- Reject commitment (unreliable, trigger escalation)  

**Decision Criteria:** Supplier reliability ≥ 95% → accept; 80–95% → accept with buffer; <80% → escalate or reject.

**Decision Confidence:** Based on historical performance and current risk indicators.

**Decision Rationale:** “Supplier S1 commitment accepted with 5% buffer: historical OTD 88%, current risk moderate. Buffer of 2 days added. Rule BR‑SI‑050.”  

---

##### Rules (for DE‑SI‑050)  

| ID | Rule | Category | Description |
|----|------|----------|-------------|
| BR‑SI‑050 | Commitment Reliability Rule | Validation Rule | If supplier’s 12‑month OTD rate < 80%, commitments are automatically flagged as unreliable and not used as firm supply. |
| BR‑SI‑051 | Buffer Calculation Rule | Derivation Rule | For suppliers with OTD 80–95%, a buffer of (1 – OTD) × lead time is added to the expected delivery date. |

##### Policies (for DE‑SI‑050)  

| ID | Policy | Category | Description |
|----|--------|----------|-------------|
| PO‑SI‑050 | Supplier Escalation Policy | Exception Policy | Suppliers with OTD < 80% for two consecutive months are escalated to Supplier Management for corrective action. |

---

#### DE‑SI‑051 — Share Demand Forecast with Supplier  

**Purpose:** Decide what forecast information to share with each supplier, at what level of aggregation, and how frequently, to enable better supplier planning without exposing sensitive data.

**Required Understanding:** Demand forecast for items sourced from the supplier, supplier relationship type, confidentiality agreements.

**Decision Alternatives:**  
- Share detailed SKU‑level forecast  
- Share aggregated product‑family forecast  
- No sharing (spot buy only)  

**Decision Criteria:** Supplier collaboration agreement level, strategic importance, forecast accuracy.

**Decision Confidence:** Based on forecast quality and agreement status.

**Decision Rationale:** “SKU‑level forecast shared with Supplier S2 (strategic partner, under NDA). Weekly sharing cadence. Rule BR‑SI‑052 applied.”  

---

##### Rules (for DE‑SI‑051)  

| ID | Rule | Category | Description |
|----|------|----------|-------------|
| BR‑SI‑052 | Forecast Sharing Authorization Rule | Validation Rule | Forecast sharing shall only occur if a valid confidentiality agreement exists and the supplier is classified as Strategic or Preferred. |
| BR‑SI‑053 | Forecast Aggregation Rule | Derivation Rule | For non‑strategic suppliers, forecasts are aggregated to product‑family level or higher. |

##### Policies (for DE‑SI‑051)  

| ID | Policy | Category | Description |
|----|--------|----------|-------------|
| PO‑SI‑051 | Forecast Sharing Policy | Compliance Policy | Forecast sharing cadence and level are reviewed annually and upon contract renewal. |

---

#### DE‑SI‑052 — Assess Supplier Risk  

**Purpose:** Compute a supplier risk score based on delivery performance, financial health, geographic risk, and single‑source dependency.

**Decision Alternatives:** Low risk, Medium risk, High risk.

**Decision Criteria:** Weighted risk factors.

**Decision Rationale:** “Supplier S3 risk score: Medium. OTD 90% (good), financial score B (moderate), sole‑source for 2 critical items (high). Rule BR‑SI‑054.”  

---

##### Rules (for DE‑SI‑052)  

| ID | Rule | Category | Description |
|----|------|----------|-------------|
| BR‑SI‑054 | Supplier Risk Scoring Rule | Derivation Rule | Risk score = weighted sum of performance score (30%), financial health (25%), single‑source dependency (30%), geographic risk (15%). Thresholds configurable. |

##### Policies (for DE‑SI‑052)  

| ID | Policy | Category | Description |
|----|--------|----------|-------------|
| PO‑SI‑052 | Risk Mitigation Policy | Compliance Policy | High‑risk suppliers require a documented mitigation plan (e.g., dual sourcing, safety stock) within 60 days. |

---

### 5.5.13 Functional Behaviour  

1. **Scheduled:** Weekly commitment evaluation, monthly risk assessment, quarterly forecast sharing review.  
2. **Retrieve** supplier performance data, open commitments, demand forecasts.  
3. **Execute DE‑SI‑050** (Evaluate Supplier Commitment) for each open commitment — rules BR‑SI‑050/051, policy PO‑SI‑050.  
4. **Execute DE‑SI‑051** (Share Demand Forecast) — rules BR‑SI‑052/053, policy PO‑SI‑051.  
5. **Execute DE‑SI‑052** (Assess Supplier Risk) — rule BR‑SI‑054, policy PO‑SI‑052.  
6. **Update** supply picture with adjusted commitments.  
7. **Raise events:** `SupplierCommitmentEvaluated`, `ForecastSharedWithSupplier`, `SupplierRiskAssessed`.  

### 5.5.14 Commands  

| Command | Purpose |
|---------|---------|
| `EvaluateSupplierCommitments` | Run commitment evaluation for a cycle |
| `ShareForecastWithSupplier` | Trigger forecast share |
| `AssessSupplierRisk` | Run risk assessment |

### 5.5.15 Events  

| Event | Payload Highlights |
|-------|-------------------|
| `SupplierCommitmentEvaluated` | Supplier ID, confidence factor |
| `ForecastSharedWithSupplier` | Supplier ID, aggregation level, timestamp |
| `SupplierRiskAssessed` | Supplier ID, risk score, factors |

### 5.5.16 Queries  

| Query | Description |
|-------|-------------|
| `GetSupplierScorecard(supplierId)` | Performance metrics |
| `GetSupplierCommitments(supplierId)` | Current commitments |
| `GetSupplierRisk(supplierId)` | Risk assessment |

### 5.5.17 Reports  

- **Supplier Scorecard Report** – OTD, quality, responsiveness  
- **Supplier Risk Report** – risk scores and mitigation status  

### 5.5.18 Dashboards  

- **Supplier Collaboration Hub** – commitments, forecasts shared, performance trends  
- **Supplier Risk Dashboard** – heatmap of risk factors  

### 5.5.19 Software Realization  

```
API → Application Service → Domain Model (SupplierCommitment, SupplierScorecard)  
→ Integration Layer (supplier portal, EDI)  
→ Event Store → Projections → Read Model
```  
The system sends forecast shares via API or portal; commitment updates are received similarly.

---

## 5.6 Procure Materials  

### 5.6.1 Purpose  

Generate procurement recommendations that translate the supply plan into actionable purchase requisitions and purchase orders, respecting procurement policies, supplier contracts, lead times, and minimum order quantities. Answers: *“What should we buy, from whom, when, and in what quantity?”*

### 5.6.2 Business Objectives Served  

- BO‑SI‑004 (Ensure Supply Continuity)  
- BO‑SI‑005 (Minimize Total Delivered Cost)  
- BO‑SI‑006 (Improve Supplier Collaboration)  
- BO‑SI‑007 (Increase Planning Automation)  

### 5.6.3 Enterprise Measures  

- PI‑SI‑007 (Supplier On‑Time Delivery) — indirectly, by placing timely orders  
- PI‑SI‑105 (Recommendation Quality Index — Supply)  
- PI‑SI‑019 (Touchless Planning Rate — Supply)  

### 5.6.4 Semantic Objects  

| ID | Object | Role |
|----|--------|------|
| SE‑SI‑050 | Purchase Requisition | Internal request |
| SE‑SI‑051 | Purchase Order | Execution document |
| SE‑SI‑052 | Procurement Policy | Rules |
| SE‑SI‑053 | Procurement Lead Time | Lead time |
| SE‑SI‑005 | Supplier | Source |
| SE‑SI‑044 | Supplier Contract | Terms |
| SE‑SI‑041 | Supplier Commitment | Confirmed supply |

### 5.6.5 Primitive Capabilities Composed  

- **Understand** – interprets supply plan and inventory position  
- **Evaluate** – selects supplier and determines order quantities  
- **Assess** – validates against procurement policies  

### 5.6.6 Enterprise Inputs  

- Supply plan (planned purchase quantities and dates from Plan Supply)  
- Inventory position and open purchase orders (from Understand Supply)  
- Supplier master data, contracts, and lead times  
- Supplier commitments (from Collaborate with Suppliers)  
- Procurement policies: approval thresholds, supplier allocation rules, minimum order quantities  

### 5.6.7 Enterprise Understanding Produced  

- Recommended purchase requisitions: item, quantity, required date, suggested supplier  
- Supplier‑order assignment logic (which supplier gets which order)  
- Order consolidation suggestions (combining multiple planned orders into one PO)  
- Compliance flags: orders requiring approval, policy violations  

### 5.6.8 Enterprise Outputs  

| ID | Output | Description |
|----|--------|-------------|
| OUT‑SI‑050 | Purchase Requisition | Internal request to procure, with recommended supplier and dates |
| OUT‑SI‑051 | Order Consolidation Plan | Consolidated purchase orders for efficiency |
| OUT‑SI‑052 | Procurement Compliance Flags | Items requiring approval or policy review |

### 5.6.9 Preconditions  

- Supply plan is published  
- Supplier master and contracts are current  
- Procurement policies and approval thresholds are configured  

### 5.6.10 Capability Dependencies  

- `CA‑SI‑002 Plan Supply` – for planned procurement quantities  
- `CA‑SI‑001 Understand Supply` – for inventory and open orders  
- `CA‑SI‑005 Collaborate with Suppliers` – for supplier commitments and performance  

### 5.6.11 Collaborating Capabilities  

- **Collaborate with Suppliers** – provides supplier performance and commitment data  
- **Evaluate Supply Quality** – monitors procurement plan adherence  

### 5.6.12 Business Decisions  

---

#### DE‑SI‑060 — Select Supplier for Order  

**Purpose:** Assign each planned procurement to a specific supplier, considering contract terms, supplier performance, total cost, and strategic allocation rules.

**Required Understanding:** Planned item, quantity, required date, eligible suppliers (contract), supplier OTD, lead time, cost, risk score, supplier allocation quota.

**Decision Alternatives:**  
- Assign to primary supplier (if within quota and meets criteria)  
- Assign to alternate supplier (if primary unavailable, over quota, or poor performance)  
- Split order (large quantity across multiple suppliers)  
- Hold for manual assignment  

**Decision Criteria:** Primary supplier has OTD ≥ 95%, risk low/medium, within allocation quota. Cost is secondary within approved variance. Split if quantity exceeds single‑supplier capacity.

**Decision Confidence:** Based on supplier reliability and data completeness.

**Decision Rationale:** *Explainability Template:* “Order for 5,000 units of Material M assigned to Supplier S1 (primary). S1 OTD 97%, risk low, within allocation quota. Rule BR‑SI‑060 applied.”  

---

##### Rules (for DE‑SI‑060)  

| ID | Rule | Category | Description |
|----|------|----------|-------------|
| BR‑SI‑060 | Primary Supplier Assignment Rule | Derivation Rule | If the primary supplier’s OTD ≥ 95%, risk ≤ medium, and the order is within allocation quota, assign to primary. Otherwise, evaluate alternates. |
| BR‑SI‑061 | Supplier Split Rule | Derivation Rule | If the planned quantity exceeds any single supplier’s capacity or allocation limit, split the order proportionally among eligible suppliers. |
| BR‑SI‑062 | Supplier Exclusion Rule | Validation Rule | Suppliers with OTD < 80% or risk score “High” are excluded from automatic assignment and flagged for manual review. |

##### Policies (for DE‑SI‑060)  

| ID | Policy | Category | Description |
|----|--------|----------|-------------|
| PO‑SI‑060 | Supplier Assignment Override Policy | Authorization Policy | A Procurement Manager may manually override the supplier assignment with a documented business justification. |
| PO‑SI‑061 | Allocation Quota Policy | Compliance Policy | Supplier allocation quotas are reviewed quarterly and adjusted based on performance and strategic sourcing decisions. |

---

#### DE‑SI‑061 — Generate Purchase Requisition  

**Purpose:** Convert the supply plan’s procurement recommendations into firm purchase requisitions, applying lot‑sizing, consolidation, and policy checks.

**Required Understanding:** Planned purchase quantity, due date, selected supplier, supplier lead time, minimum order quantity (MOQ), procurement calendar.

**Decision Alternatives:**  
- Create requisition (single line)  
- Consolidate with other planned orders (multi‑line or blanket)  
- Defer (order not yet needed)  

**Decision Criteria:** Order by release date (due date minus lead time). If multiple orders for same supplier fall within a consolidation window (e.g., 3 days), consolidate. Meet MOQ; if planned quantity < MOQ, round up to MOQ.

**Decision Confidence:** Based on lead time accuracy and demand certainty.

**Decision Rationale:** “Requisition REQ‑789 created for 500 units of Material M from Supplier S1, release date 15‑Jul. Consolidated with REQ‑788 for the same supplier. Rule BR‑SI‑063 applied.”  

---

##### Rules (for DE‑SI‑060)  

| ID | Rule | Category | Description |
|----|------|----------|-------------|
| BR‑SI‑063 | Requisition Release Date Rule | Derivation Rule | Release date = Required date − Supplier lead time − Internal processing time (default 2 days). |
| BR‑SI‑064 | Order Consolidation Rule | Derivation Rule | Orders for the same supplier with release dates within 3 business days are consolidated into a single requisition. |
| BR‑SI‑065 | MOQ Compliance Rule | Validation Rule | If the planned quantity is less than the supplier’s MOQ, the requisition quantity is rounded up to MOQ. If the excess exceeds 50% of the planned quantity, the requisition is flagged for review. |

##### Policies (for DE‑SI‑061)  

| ID | Policy | Category | Description |
|----|--------|----------|-------------|
| PO‑SI‑062 | Requisition Automation Policy | Automation Policy | Requisitions below a configurable value threshold and with auto‑selected suppliers are automatically released. Above threshold, Procurement approval is required. |
| PO‑SI‑063 | MOQ Exception Policy | Exception Policy | If MOQ rounding causes inventory excess beyond a defined threshold, the requisition requires Supply Planner approval. |

---

#### DE‑SI‑062 — Release Purchase Order  

**Purpose:** Convert approved purchase requisitions into legally binding purchase orders and transmit them to suppliers.

**Required Understanding:** Approved requisition, supplier contract, current price, terms.

**Decision Alternatives:**  
- Release PO immediately  
- Hold (pending further review)  
- Cancel requisition (no longer needed)  

**Decision Criteria:** Requisition approved, funds available, contract valid.

**Decision Confidence:** High if all criteria satisfied.

**Decision Rationale:** “PO‑4567 created from REQ‑789 and transmitted to Supplier S1. All criteria met. Rule BR‑SI‑066 passed.”  

---

##### Rules (for DE‑SI‑062)  

| ID | Rule | Category | Description |
|----|------|----------|-------------|
| BR‑SI‑066 | PO Release Validation Rule | Validation Rule | A PO may only be released if the requisition is approved, the supplier contract is valid, and the item is active. |

##### Policies (for DE‑SI‑062)  

| ID | Policy | Category | Description |
|----|--------|----------|-------------|
| PO‑SI‑064 | PO Release Approval Policy | Approval Policy | POs above a defined value threshold require Procurement Manager approval before transmission. |
| PO‑SI‑065 | PO Transmission Policy | Compliance Policy | POs are transmitted electronically (EDI, API, portal) within 4 hours of release. |

---

### 5.6.13 Functional Behaviour  

1. **Trigger:** After supply plan publication and on‑demand for expedites.  
2. **Retrieve** supply plan (procurement portion), inventory, open orders, supplier data.  
3. **For each planned procurement**, execute DE‑SI‑060 (Select Supplier) — rules BR‑SI‑060/061/062, policies PO‑SI‑060/061.  
4. **Execute DE‑SI‑061** (Generate Purchase Requisition) — rules BR‑SI‑063/064/065, policies PO‑SI‑062/063.  
5. **Execute DE‑SI‑062** (Release Purchase Order) for approved requisitions — rule BR‑SI‑066, policies PO‑SI‑064/065.  
6. **Transmit** POs to suppliers and update the supply picture.  
7. **Raise events:** `SupplierSelected`, `RequisitionCreated`, `PurchaseOrderReleased`.  

### 5.6.14 Commands  

| Command | Purpose |
|---------|---------|
| `SelectSupplierForOrder` | Assign supplier to a planned procurement |
| `GenerateRequisition` | Create purchase requisition |
| `ReleasePurchaseOrder` | Convert requisition to PO and transmit |

### 5.6.15 Events  

| Event | Payload Highlights |
|-------|-------------------|
| `SupplierSelected` | Planned order ID, selected supplier, reason |
| `RequisitionCreated` | Requisition ID, items, quantities, dates |
| `PurchaseOrderReleased` | PO ID, supplier, items, transmitted timestamp |

### 5.6.16 Queries  

| Query | Description |
|-------|-------------|
| `GetProcurementRecommendations(filter)` | Planned orders awaiting action |
| `GetOpenRequisitions()` | Requisitions pending approval or release |
| `GetSupplierOrderHistory(supplierId)` | PO history and performance |

### 5.6.17 Reports  

- **Procurement Action Report** – requisitions created, POs released, spend summary  
- **Supplier Allocation Report** – actual vs. quota allocation  

### 5.6.18 Dashboards  

- **Procurement Workbench** – planned orders, requisitions, pending approvals  
- **Spend Dashboard** – committed spend by supplier, category, period  

### 5.6.19 Software Realization  

```
API → Application Service → Domain Model (ProcurementPlan, Requisition, PurchaseOrder)  
→ Rule Engine (supplier assignment, consolidation)  
→ Integration Adapter (supplier transmission: EDI, API)  
→ Event Store → Projections → Read Model
```  
Supplier contracts and MOQs are stored in master data. The transmission layer supports multiple protocols.

---

## 5.7 Schedule Production  

### 5.7.1 Purpose  

Generate a detailed, feasible production schedule that sequences production orders on specific resources, respecting constraints, changeovers, and dependencies. Answers: *“What should we produce, on which resource, in what sequence, and when?”*

### 5.7.2 Business Objectives Served  

- BO‑SI‑003 (Maximize Capacity Utilization)  
- BO‑SI‑004 (Ensure Supply Continuity)  
- BO‑SI‑007 (Increase Planning Automation)  

### 5.7.3 Enterprise Measures  

- PI‑SI‑006 (Schedule Adherence)  
- PI‑SI‑005 (Capacity Utilization)  
- PI‑SI‑201 (Supply Planning Cycle Time — scheduling component)  

### 5.7.4 Semantic Objects  

| ID | Object | Role |
|----|--------|------|
| SE‑SI‑062 | Production Order | Execution document |
| SE‑SI‑063 | Production Schedule | Output |
| SE‑SI‑064 | Work Center | Resource |
| SE‑SI‑065 | Changeover | Operational factor |
| SE‑SI‑061 | Routing | Sequence |
| SE‑SI‑060 | Bill of Materials | Material dependency |

### 5.7.5 Primitive Capabilities Composed  

- **Understand** – interprets supply plan, routings, constraints  
- **Predict** – estimates completion times  
- **Evaluate** – optimizes sequence and resource assignment  

### 5.7.6 Enterprise Inputs  

- Supply plan (production portion from Plan Supply)  
- Current production order status and WIP (from Understand Supply)  
- Resource availability and calendars (from Manage Capacity)  
- Routings, BOMs, changeover matrices  
- Production constraints: minimum run length, shelf life, batch sizes  

### 5.7.7 Enterprise Understanding Produced  

- Detailed production schedule: order sequence per resource, start/end times  
- Projected completion times and material requirement dates  
- Schedule risk alerts: orders at risk of being late  

### 5.7.8 Enterprise Outputs  

| ID | Output | Description |
|----|--------|-------------|
| OUT‑SI‑060 | Production Schedule | Time‑phased sequence of production orders per resource |
| OUT‑SI‑061 | Schedule Risk Alerts | Orders at risk of delay |
| OUT‑SI‑062 | Material Requirement Dates | When components are needed at each work center |

### 5.7.9 Preconditions  

- Supply plan is published and capacity‑feasible  
- Resource calendars and routings are current  
- WIP status is accurate  

### 5.7.10 Capability Dependencies  

- `CA‑SI‑002 Plan Supply` – for planned production quantities  
- `CA‑SI‑001 Understand Supply` – for WIP and resource status  
- `CA‑SI‑004 Manage Capacity` – for capacity feasibility  

### 5.7.11 Collaborating Capabilities  

- **Manage Capacity** – provides capacity feedback  
- **Manage Inventory** – consumes schedule for material planning  

### 5.7.12 Business Decisions  

---

#### DE‑SI‑070 — Sequence Production Orders  

**Purpose:** Determine the optimal sequence of production orders on each resource to minimize changeovers, meet due dates, and respect constraints.

**Required Understanding:** Orders due dates, changeover times and costs, resource availability, order priorities.

**Decision Alternatives:**  
- Earliest Due Date (EDD)  
- Minimize Changeover (optimize sequence)  
- Priority‑based (Critical/High items first)  
- Hybrid (optimize within priority windows)  

**Decision Criteria:** Minimize total cost (changeover cost + lateness penalty), subject to hard constraints (capacity, due date for critical orders).

**Decision Confidence:** Based on schedule stability and data accuracy.

**Decision Rationale:** “Resource WC‑100 sequenced: 5 orders, 2 changeovers (3.5 hrs total). All due dates met. Priority order WO‑2001 scheduled first. Rule BR‑SI‑070 applied.”  

---

##### Rules (for DE‑SI‑070)  

| ID | Rule | Category | Description |
|----|------|----------|-------------|
| BR‑SI‑070 | Sequencing Rule | Derivation Rule | Orders are sequenced using a configurable strategy (default: Critical priority first, then minimize changeover within priority group). |
| BR‑SI‑071 | Due Date Constraint Rule | Validation Rule | No order shall be scheduled to complete after its required date unless capacity is infeasible. Late orders are flagged with the delay reason. |
| BR‑SI‑072 | Minimum Run Length Rule | Constraint Rule | Production runs must meet the defined minimum run quantity or time; orders below the minimum are consolidated or deferred per policy. |

##### Policies (for DE‑SI‑070)  

| ID | Policy | Category | Description |
|----|--------|----------|-------------|
| PO‑SI‑070 | Sequencing Override Policy | Authorization Policy | Production Scheduler may manually adjust sequence with justification. Adjustments are tracked. |
| PO‑SI‑071 | Minimum Run Exception Policy | Exception Policy | Orders below minimum run length may be produced with a documented exception from the Production Manager. |

---

#### DE‑SI‑071 — Release Production Orders  

**Purpose:** Convert planned production into firm production orders, check material availability, and release them to the shop floor.

**Required Understanding:** Scheduled production order, BOM, current inventory of components, WIP status.

**Decision Alternatives:**  
- Release (materials available)  
- Hold (material shortage)  
- Partial release (produce what materials allow)  

**Decision Criteria:** All components available or within expected receipt window.

**Decision Confidence:** Based on inventory accuracy and supplier delivery reliability.

**Decision Rationale:** “Production Order WO‑2100 released: all materials confirmed. Start date 18‑Jul, completion 20‑Jul. Rule BR‑SI‑073 passed.”  

---

##### Rules (for DE‑SI‑071)  

| ID | Rule | Category | Description |
|----|------|----------|-------------|
| BR‑SI‑073 | Material Availability Check Rule | Validation Rule | A production order may only be released if all required materials (per BOM) are available or confirmed for delivery before the production start date. |
| BR‑SI‑074 | Order Release Timing Rule | Derivation Rule | Production orders are released to the floor at the scheduled release date (start date minus any staging time). |

##### Policies (for DE‑SI‑071)  

| ID | Policy | Category | Description |
|----|--------|----------|-------------|
| PO‑SI‑072 | Partial Release Policy | Authorization Policy | Partial release is allowed only with Production Manager approval and a documented risk assessment. |

---

#### DE‑SI‑072 — Publish Production Schedule  

**Purpose:** Finalize and distribute the production schedule to execution systems and stakeholders.

**Decision Alternatives:** Publish, Hold.

**Decision Criteria:** All orders sequenced, material checks complete, no unresolved critical late orders.

**Decision Confidence:** High if schedule is feasible.

**Decision Rationale:** “Production schedule vW27 published: 120 orders scheduled across 8 resources, 2 late orders flagged with mitigation plans.”  

---

##### Rules (for DE‑SI‑072)  

| ID | Rule | Category | Description |
|----|------|----------|-------------|
| BR‑SI‑075 | Schedule Publication Rule | Validation Rule | Schedule shall not be published if any critical priority orders are unscheduled or late without a mitigation plan. |

##### Policies (for DE‑SI‑072)  

| ID | Policy | Category | Description |
|----|--------|----------|-------------|
| PO‑SI‑073 | Schedule Publication Cadence Policy | Compliance Policy | The production schedule is published daily by 08:00 for the next 2 weeks. |

---

### 5.7.13 Functional Behaviour  

1. **Trigger:** Daily, or after supply plan update.  
2. **Retrieve** planned production, resource status, routings, changeover data, order priorities.  
3. **Execute DE‑SI‑070** (Sequence Production Orders) — rules BR‑SI‑070/071/072, policies PO‑SI‑070/071.  
4. **Execute DE‑SI‑071** (Release Production Orders) for orders in the release window — rules BR‑SI‑073/074, policy PO‑SI‑072.  
5. **Execute DE‑SI‑072** (Publish Production Schedule) — rule BR‑SI‑075, policy PO‑SI‑073.  
6. **Transmit** schedule to MES / shop‑floor systems.  
7. **Raise events:** `ProductionScheduleGenerated`, `ProductionOrderReleased`, `ProductionSchedulePublished`.  

### 5.7.14 Commands  

| Command | Purpose |
|---------|---------|
| `SequenceOrders` | Run sequencing for a resource or plant |
| `ReleaseProductionOrder` | Release a specific order |
| `PublishSchedule` | Finalize and distribute schedule |

### 5.7.15 Events  

| Event | Payload Highlights |
|-------|-------------------|
| `ProductionScheduleGenerated` | Plant, resource, sequence |
| `ProductionOrderReleased` | Order ID, start date, completion date |
| `ProductionSchedulePublished` | Schedule version, timestamp |

### 5.7.16 Queries  

| Query | Description |
|-------|-------------|
| `GetSchedule(resource, period)` | Current production schedule |
| `GetOrderStatus(orderId)` | WIP status |
| `GetScheduleRisk()` | Late or at‑risk orders |

### 5.7.17 Reports  

- **Schedule Adherence Report** – planned vs. actual completion  
- **Changeover Analysis Report** – changeover frequency and duration  

### 5.7.18 Dashboards  

- **Production Schedule Board** – Gantt view per resource  
- **Schedule Risk Dashboard** – late orders, bottlenecks  

### 5.7.19 Software Realization  

```
API → Application Service → Domain Model (ProductionSchedule, ProductionOrder)  
→ Sequencing Engine (optimizer or heuristic)  
→ Material Check Service (inventory query)  
→ Event Store → Projections → Read Model  
→ MES Adapter (schedule transmission)
```  
The sequencing engine supports configurable strategies. Material check queries Understand Supply for inventory.

---

## 5.8 Manage Distribution  

### 5.8.1 Purpose  

Plan inter‑location inventory movements (transfers, allocations) to balance stock across the distribution network, meet regional demand, and minimize transportation cost. Answers: *“Where should inventory be positioned across the network?”*

### 5.8.2 Business Objectives Served  

- BO‑SI‑002 (Optimize Inventory Performance)  
- BO‑SI‑005 (Minimize Total Delivered Cost)  
- BO‑SI‑004 (Ensure Supply Continuity)  

### 5.8.3 Enterprise Measures  

- PI‑SI‑004 (Fill Rate — Supply)  
- PI‑SI‑008 (Total Supply Chain Cost — distribution component)  
- PI‑SI‑010 (Supply Plan Adherence)  

### 5.8.4 Semantic Objects  

| ID | Object | Role |
|----|--------|------|
| SE‑SI‑070 | Distribution Network | Scope |
| SE‑SI‑071 | Transfer Order | Execution |
| SE‑SI‑072 | Allocation Rule | Logic |
| SE‑SI‑073 | Distribution Lead Time | Lead time |

### 5.8.5 Primitive Capabilities Composed  

- **Understand** – interprets network imbalances  
- **Evaluate** – optimizes allocation and transfers  
- **Predict** – projects inventory positions after transfers  

### 5.8.6 Enterprise Inputs  

- Inventory position at all network nodes (from Understand Supply)  
- Demand forecast by location (from Demand Intelligence)  
- Distribution lead times and transportation costs  
- Allocation rules and policies  
- Open transfer orders  

### 5.8.7 Enterprise Understanding Produced  

- Recommended transfer orders: source, destination, quantity, timing  
- Allocation plans for constrained supply  
- Projected post‑transfer inventory positions  

### 5.8.8 Enterprise Outputs  

| ID | Output | Description |
|----|--------|-------------|
| OUT‑SI‑070 | Transfer Plan | Recommended transfer orders |
| OUT‑SI‑071 | Allocation Plan | Supply allocation when constrained |
| OUT‑SI‑072 | Network Balance Projection | Post‑transfer inventory positions |

### 5.8.9 Preconditions  

- Inventory positions are current at all nodes  
- Distribution network and lead times are defined  
- Demand forecasts are available by location  

### 5.8.10 Capability Dependencies  

- `CA‑SI‑001 Understand Supply` – for inventory positions  
- `CA‑DI‑002 Forecast Demand` – for demand by location  
- `CA‑SI‑002 Plan Supply` – for overall supply availability  

### 5.8.11 Collaborating Capabilities  

- **Manage Inventory** – receives transfer‑adjusted inventory projections  

### 5.8.12 Business Decisions  

---

#### DE‑SI‑080 — Determine Rebalancing Transfers  

**Purpose:** Identify imbalances in the distribution network (surplus at some nodes, deficit at others) and recommend transfer orders to rebalance.

**Required Understanding:** Inventory position vs. demand forecast at each node, lead times, transportation cost, service level priority.

**Decision Alternatives:**  
- Transfer from surplus to deficit node  
- Hold (imbalance within tolerance)  
- External procurement if no internal surplus  

**Decision Criteria:** Net inventory position (on‑hand + on‑order – demand) projected over lead time. Transfer if deficit > safety stock shortfall and surplus exists.

**Decision Confidence:** Based on demand and inventory accuracy.

**Decision Rationale:** “Transfer 200 units from DC‑A (surplus 350) to DC‑B (deficit 180). Cost $500, delivery in 3 days. Rule BR‑SI‑080.”  

---

##### Rules (for DE‑SI‑080)  

| ID | Rule | Category | Description |
|----|------|----------|-------------|
| BR‑SI‑080 | Rebalancing Trigger Rule | Derivation Rule | A transfer is recommended when a node’s projected inventory drops below safety stock and another node has surplus above its maximum target. |
| BR‑SI‑081 | Cost‑Benefit Rule | Validation Rule | The estimated transportation cost of the transfer must not exceed the cost of an external purchase plus the cost of holding surplus. |

##### Policies (for DE‑SI‑080)  

| ID | Policy | Category | Description |
|----|--------|----------|-------------|
| PO‑SI‑080 | Transfer Authorization Policy | Authorization Policy | Transfers below a configurable value threshold are automatically approved. Above threshold, Supply Manager approval is required. |

---

#### DE‑SI‑081 — Allocate Constrained Supply  

**Purpose:** When total supply is insufficient to meet total demand across the network, allocate the available supply according to business rules (fair share, priority, profitability).

**Required Understanding:** Total available supply, demand by node, customer priorities, allocation rules.

**Decision Alternatives:**  
- Fair share (proportional)  
- Priority‑based (strategic segments first)  
- Profitability‑based  

**Decision Criteria:** Defined by policy; default fair share with priority adjustment.

**Decision Confidence:** Based on demand certainty.

**Decision Rationale:** “Supply of Product P constrained (500 available, 700 demanded). Allocated fair share with Gold customers receiving 100% of forecast, remainder shared. Rule BR‑SI‑082.”  

---

##### Rules (for DE‑SI‑081)  

| ID | Rule | Category | Description |
|----|------|----------|-------------|
| BR‑SI‑082 | Allocation Rule | Derivation Rule | Allocation is performed using a configurable rule set: Priority customers receive up to 100% of forecast; remainder is distributed proportionally. |
| BR‑SI‑083 | Allocation Documentation Rule | Compliance Rule | Every allocation event with constrained supply must record the method, the quantities, and the customers affected. |

##### Policies (for DE‑SI‑081)  

| ID | Policy | Category | Description |
|----|--------|----------|-------------|
| PO‑SI‑081 | Allocation Method Policy | Compliance Policy | The default allocation method is reviewed and approved annually by the S&OP Council. |

---

#### DE‑SI‑082 — Release Transfer Orders  

**Purpose:** Convert recommended transfers into firm transfer orders and transmit to execution systems (WMS, TMS).

**Decision Alternatives:** Release, Hold.

**Decision Criteria:** Source inventory confirmed, destination has capacity to receive.

**Decision Confidence:** High if inventory confirmed.

**Decision Rationale:** “Transfer TO‑890 created from DC‑A to DC‑B for 200 units. Released.”  

---

##### Rules (for DE‑SI‑082)  

| ID | Rule | Category | Description |
|----|------|----------|-------------|
| BR‑SI‑084 | Transfer Release Rule | Validation Rule | Transfer orders may only be released if source inventory is confirmed available and the source location is not itself projected to fall below safety stock after the transfer. |

##### Policies (for DE‑SI‑082)  

| ID | Policy | Category | Description |
|----|--------|----------|-------------|
| PO‑SI‑082 | Transfer Automation Policy | Automation Policy | Transfers that meet all rules and are below the value threshold are automatically released. |

---

### 5.8.13 Functional Behaviour  

1. **Trigger:** Daily, after supply plan update, or on inventory position change.  
2. **Retrieve** inventory positions, demand forecasts, network configuration.  
3. **Execute DE‑SI‑080** (Determine Rebalancing Transfers) — rules BR‑SI‑080/081, policy PO‑SI‑080.  
4. **If total supply constrained,** execute DE‑SI‑081 (Allocate Constrained Supply) — rules BR‑SI‑082/083, policy PO‑SI‑081.  
5. **Execute DE‑SI‑082** (Release Transfer Orders) — rule BR‑SI‑084, policy PO‑SI‑082.  
6. **Transmit** transfer orders to execution systems.  
7. **Raise events:** `TransferRecommended`, `SupplyAllocated`, `TransferOrderReleased`.  

### 5.8.14 Commands  

| Command | Purpose |
|---------|---------|
| `DetermineTransfers` | Run rebalancing analysis |
| `AllocateSupply` | Run allocation for constrained items |
| `ReleaseTransfer` | Create and transmit transfer order |

### 5.8.15 Events  

| Event | Payload Highlights |
|-------|-------------------|
| `TransferRecommended` | Source, destination, quantity, timing |
| `SupplyAllocated` | Product, total supply, allocation per node |
| `TransferOrderReleased` | Transfer ID, source, destination, quantity |

### 5.8.16 Queries  

| Query | Description |
|-------|-------------|
| `GetNetworkBalance()` | Inventory surplus/deficit per node |
| `GetTransferPlan()` | Pending and in‑transit transfers |
| `GetAllocationHistory(product)` | Past allocation events |

### 5.8.17 Reports  

- **Network Balance Report** – surplus/deficit by node and product  
- **Transfer Cost Report** – transportation spend for rebalancing  

### 5.8.18 Dashboards  

- **Distribution Network View** – map with inventory levels, transfers in flight  
- **Allocation Dashboard** – constrained items and allocation decisions  

### 5.8.19 Software Realization  

```
API → Application Service → Domain Model (TransferPlan, Allocation)  
→ Network Optimization Engine (rebalancing algorithm)  
→ Integration Adapter (WMS/TMS)  
→ Event Store → Projections → Read Model
```  
The engine supports configurable allocation rules. Transfer orders are transmitted to WMS/TMS via API.

---

## 5.9 Sense Supply Changes  

### 5.9.1 Purpose  

Continuously monitor the supply environment to detect real‑time disruptions, deviations, and emerging risks as early as possible. Answers: *“What is changing in supply right now, and which changes require immediate attention?”* The capability provides early warning of supply issues so that plans can be adjusted before customer service is affected.

### 5.9.2 Business Objectives Served  

- BO‑SI‑004 (Ensure Supply Continuity)  
- BO‑SI‑003 (Maximize Capacity Utilization) — by detecting capacity disruptions  

### 5.9.3 Enterprise Measures  

- PI‑SI‑110 (Supply Exception Detection Accuracy)  
- PI‑SI‑212 (Exception Processing Time — Supply)  
- PI‑SI‑208 (Event Processing Latency — Supply)  

### 5.9.4 Semantic Objects  

| ID | Object | Role |
|----|--------|------|
| SE‑SI‑001 | Supply | Subject of change |
| SE‑SI‑012 | Supply Constraint | Constraint violation |
| SE‑SI‑005 | Supplier | Source of disruption |
| SE‑SI‑040 | Supplier Performance | Deterioration signal |
| SE‑SI‑004 | Capacity | Capacity events |

### 5.9.5 Primitive Capabilities Composed  

- **Observe** – ingests streaming supply events  
- **Understand** – compares against baseline expectations  
- **Assess** – determines severity and significance  

### 5.9.6 Enterprise Inputs  

- Real‑time supply events: supplier delay notices, production stoppages, quality holds, transportation delays, inventory adjustments  
- Current supply plan and open orders (baseline)  
- Supplier performance history  
- Pre‑defined change detection thresholds  

### 5.9.7 Enterprise Understanding Produced  

- Real‑time supply change alerts with magnitude, cause, affected items  
- Supply disruption severity classification (Minor, Significant, Critical)  
- Estimated impact on supply plan and customer service  

### 5.9.8 Enterprise Outputs  

| ID | Output | Description |
|----|--------|-------------|
| OUT‑SI‑090 | Supply Change Alert | Alert with type, severity, affected items, timestamp |
| OUT‑SI‑091 | Disruption Impact Estimate | Projected supply shortfall and affected orders |

### 5.9.9 Preconditions  

- Supply event streams are operational with latency ≤ 15 minutes  
- Baseline supply plan and open order data available  

### 5.9.10 Capability Dependencies  

- `CA‑SI‑001 Understand Supply` – for baseline supply picture  
- `CA‑SI‑002 Plan Supply` – for current supply plan  

### 5.9.11 Collaborating Capabilities  

- **Detect Supply Exceptions** – changes that qualify as exceptions are forwarded  
- **Plan Supply** – may trigger a plan re‑generation  

### 5.9.12 Business Decisions  

---

#### DE‑SI‑090 — Detect Supply Disruption  

**Purpose:** Determine whether an incoming supply event constitutes a meaningful disruption requiring action.

**Required Understanding:** Event details, current supply plan, historical norms, supplier performance.

**Decision Alternatives:**  
- No disruption (within normal variance)  
- Minor disruption (log, no immediate action)  
- Significant disruption (alert, optional plan refresh)  
- Critical disruption (immediate escalation, mandatory plan refresh)  

**Decision Criteria:** Deviation from planned quantity/timing > threshold (default: delay > 1 day or quantity shortfall > 10% for critical items). Supplier OTD history and item criticality modulate threshold.

**Decision Confidence:** Based on event source reliability and corroboration.

**Decision Rationale:** *Explainability Template:* “Critical disruption: Supplier S3 notified 3‑day delay on PO‑8900, affecting critical Product P. Shortfall 500 units. Rule BR‑SI‑090 triggered. Confidence 95%.”  

---

##### Rules (for DE‑SI‑090)  

| ID | Rule | Category | Description |
|----|------|----------|-------------|
| BR‑SI‑090 | Disruption Classification Rule | Derivation Rule | A delay > 1 day or quantity shortfall > 10% for critical items (Gold priority) is Critical. For non‑critical items, >3 days or >20% is Significant. |
| BR‑SI‑091 | Corroboration Rule (Supply) | Consistency Rule | Critical disruptions must be corroborated by supplier confirmation or independent tracking data. Unconfirmed disruptions are flagged as “Unconfirmed”. |

##### Policies (for DE‑SI‑090)  

| ID | Policy | Category | Description |
|----|--------|----------|-------------|
| PO‑SI‑090 | Disruption Escalation Policy | Authorization Policy | Critical disruptions are escalated immediately to Supply Manager and affected planners. |
| PO‑SI‑091 | Automatic Plan Refresh Policy | Automation Policy | Critical disruptions automatically trigger a supply plan re‑generation for affected items. |

---

### 5.9.13 Functional Behaviour  

1. **Ingest** real‑time events from suppliers, production, logistics, quality.  
2. **Correlate** events to planned orders and supply commitments.  
3. **Execute DE‑SI‑090** (Detect Supply Disruption) — rules BR‑SI‑090/091, policies PO‑SI‑090/091.  
4. **Publish** alerts and trigger downstream actions.  
5. **Raise events:** `SupplyDisruptionDetected`, `SupplyAlertEscalated`.  

### 5.9.14 Commands  

| Command | Purpose |
|---------|---------|
| `EvaluateSupplyEvent` | Manually trigger a disruption assessment |
| `AcknowledgeAlert` | Planner acknowledges an alert |

### 5.9.15 Events  

| Event | Payload Highlights |
|-------|-------------------|
| `SupplyDisruptionDetected` | Event ID, type, severity, affected orders |
| `SupplyAlertEscalated` | Alert ID, escalated to |

### 5.9.16 Queries  

| Query | Description |
|-------|-------------|
| `GetActiveSupplyAlerts()` | Current unresolved alerts |
| `GetDisruptionHistory(period)` | Past disruptions |

### 5.9.17 Reports  

- **Supply Disruption Report** – frequency, severity, response time  

### 5.9.18 Dashboards  

- **Supply Disruption Monitor** – live alert feed with severity indicators  

### 5.9.19 Software Realization  

```
Event Bus → Stream Processor → Domain Service (DisruptionDetector)
→ Alert Publisher → Read Model
```  
Stream processing uses windowing to compare events to plan. Alerts are published to a notification service.

---

## 5.10 Evaluate Supply Quality  

### 5.10.1 Purpose  

Continuously measure and assess the quality of supply plans, inventory policies, production schedules, and supplier performance. Answers: *“How good are our supply plans and execution, and where are they failing?”* This capability is the analytical engine behind Business Outcome Measures for supply.

### 5.10.2 Business Objectives Served  

- BO‑SI‑001 (Deliver Trusted Supply Understanding)  
- BO‑SI‑002 (Optimize Inventory Performance)  
- BO‑SI‑006 (Improve Supplier Collaboration)  
- BO‑SI‑008 (Continuously Improve Supply Intelligence)  

### 5.10.3 Enterprise Measures  

- PI‑SI‑002 (Inventory Turnover)  
- PI‑SI‑003 (Days of Supply)  
- PI‑SI‑004 (Fill Rate — Supply)  
- PI‑SI‑005 (Capacity Utilization)  
- PI‑SI‑006 (Schedule Adherence)  
- PI‑SI‑007 (Supplier On‑Time Delivery)  
- PI‑SI‑010 (Supply Plan Adherence)  
- PI‑SI‑013 (Excess & Obsolete Inventory)  
- PI‑SI‑105 (Recommendation Quality Index — Supply)  
- PI‑SI‑106 (Decision Confidence Index — Supply)  

### 5.10.4 Semantic Objects  

| ID | Object | Role |
|----|--------|------|
| SE‑SI‑002 | Supply Plan | Evaluated plan |
| SE‑SI‑003 | Inventory | Evaluated inventory |
| SE‑SI‑063 | Production Schedule | Evaluated schedule |
| SE‑SI‑040 | Supplier Performance | Evaluated performance |

### 5.10.5 Primitive Capabilities Composed  

- **Observe** – collects actual outcomes  
- **Understand** – aligns plans with actuals  
- **Assess** – computes metrics  
- **Evaluate** – compares against targets and trends  

### 5.10.6 Enterprise Inputs  

- Published supply plans, production schedules, procurement plans  
- Actual inventory levels, production output, receipts, deliveries  
- Supplier performance data  
- Performance targets and thresholds  

### 5.10.7 Enterprise Understanding Produced  

- Supply plan accuracy metrics (adherence, bias, stability)  
- Inventory health trends (turns, days of supply, E&O)  
- Schedule adherence and capacity utilization metrics  
- Supplier performance scorecards  

### 5.10.8 Enterprise Outputs  

| ID | Output | Description |
|----|--------|-------------|
| OUT‑SI‑100 | Supply Quality Report | Consolidated accuracy and health metrics |
| OUT‑SI‑101 | Supplier Performance Scorecard | OTD, quality, responsiveness per supplier |
| OUT‑SI‑102 | Inventory Health Trends | Turns, days of supply, E&O over time |

### 5.10.9 Preconditions  

- Actual data for the evaluation period is available  
- Plans and schedules are stored and accessible  

### 5.10.10 Capability Dependencies  

- `CA‑SI‑001 Understand Supply` – for actual supply data  
- `CA‑SI‑002 Plan Supply` – for plan data  
- `CA‑SI‑007 Schedule Production` – for schedule data  
- `CA‑SI‑005 Collaborate with Suppliers` – for supplier commitments  

### 5.10.11 Collaborating Capabilities  

- **Learn From Supply** – consumes quality reports for improvement  

### 5.10.12 Business Decisions  

---

#### DE‑SI‑100 — Compute Supply Metrics  

**Purpose:** Calculate the standard set of supply performance metrics for a given evaluation period.

**Required Understanding:** Plan vs. actual data, calculation formulas.

**Decision Alternatives:** Deterministic.

**Decision Criteria:** Follow formulas defined in Chapter 3.

**Decision Confidence:** Based on data completeness.

**Decision Rationale:** “Supply Plan Adherence for W27: 92%. All metrics computed per standard.”  

---

##### Rules (for DE‑SI‑100)  

| ID | Rule | Category | Description |
|----|------|----------|-------------|
| BR‑SI‑100 | Metric Calculation Standard Rule | Calculation Rule | All metrics shall be calculated per Chapter 3 formulas. |
| BR‑SI‑101 | Data Completeness for Metrics Rule | Validation Rule | Metrics for scopes with <90% actual data availability are flagged as “low confidence”. |

##### Policies (for DE‑SI‑100)  

| ID | Policy | Category | Description |
|----|--------|----------|-------------|
| PO‑SI‑100 | Metric Calculation Frequency Policy | Compliance Policy | Supply metrics are computed weekly (rolling 4 weeks) and monthly (rolling 13 weeks). |

---

#### DE‑SI‑101 — Publish Supply Quality Report  

**Purpose:** Compile and distribute the periodic supply quality report.

**Decision Alternatives:** Publish, Publish with flags, Hold.

**Decision Criteria:** Data completeness ≥ 90%.

**Decision Rationale:** “Weekly supply quality report published.”  

---

##### Rules (for DE‑SI‑101)  

| ID | Rule | Category | Description |
|----|------|----------|-------------|
| BR‑SI‑102 | Report Completeness Rule | Validation Rule | The quality report must contain all mandatory metrics. |

##### Policies (for DE‑SI‑101)  

| ID | Policy | Category | Description |
|----|--------|----------|-------------|
| PO‑SI‑101 | Report Distribution Policy | Compliance Policy | The report is published by 10:00 Monday and distributed to Supply Chain leadership. |

---

### 5.10.13 Functional Behaviour  

1. **Scheduled:** Weekly and monthly.  
2. **Retrieve** plans, actuals, targets.  
3. **Execute DE‑SI‑100** (Compute Metrics) — rules BR‑SI‑100/101, policy PO‑SI‑100.  
4. **Execute DE‑SI‑101** (Publish Report) — rule BR‑SI‑102, policy PO‑SI‑101.  
5. **Raise events:** `SupplyMetricsComputed`, `SupplyQualityReportPublished`.  

### 5.10.14 Commands  

| Command | Purpose |
|---------|---------|
| `ComputeSupplyMetrics` | Run metric calculation |
| `PublishSupplyQualityReport` | Compile and release |

### 5.10.15 Events  

| Event | Payload Highlights |
|-------|-------------------|
| `SupplyMetricsComputed` | Scope, period, metrics |
| `SupplyQualityReportPublished` | Report ID, period |

### 5.10.16 Queries  

| Query | Description |
|-------|-------------|
| `GetSupplyMetrics(scope, period)` | Current metrics |
| `GetQualityReport(period)` | Full report |

### 5.10.17 Reports  

- **Supply Quality Report** – consolidated metrics  
- **Supplier Scorecard** – per supplier  

### 5.10.18 Dashboards  

- **Supply Performance Dashboard** – plan adherence, fill rate, inventory health  
- **Supplier Performance Dashboard** – OTD, quality trends  

### 5.10.19 Software Realization  

```
API → Application Service → Domain Model (SupplyMetrics)  
→ Calculation Engine (pre‑defined formulas)  
→ Event Store → Projections → Read Model
```  

---

## 5.11 Detect Supply Exceptions  

### 5.11.1 Purpose  

Identify, classify, prioritize, and resolve supply exceptions—shortages, excesses, late deliveries, capacity violations, quality failures, data gaps. Answers: *“Where is something wrong in the supply picture?”*

### 5.11.2 Business Objectives Served  

- BO‑SI‑004 (Ensure Supply Continuity)  
- BO‑SI‑007 (Increase Planning Automation)  

### 5.11.3 Enterprise Measures  

- PI‑SI‑110 (Supply Exception Detection Accuracy)  
- PI‑SI‑212 (Exception Processing Time — Supply)  

### 5.11.4 Semantic Objects  

| ID | Object | Role |
|----|--------|------|
| SE‑SI‑012 | Supply Constraint | Violated constraint |
| SE‑SI‑025 | Excess Inventory | Exception |
| SE‑SI‑026 | Obsolete Inventory | Exception |
| (New) | Supply Exception | General exception |

### 5.11.5 Primitive Capabilities Composed  

- **Observe** – scans supply data  
- **Understand** – interprets deviations  
- **Assess** – determines severity  

### 5.11.6 Enterprise Inputs  

- Supply plan, inventory, open orders, supplier commitments  
- Quality metrics from Evaluate Supply Quality  
- Priority data from Prioritize Demand (or internal supply priority)  

### 5.11.7 Enterprise Understanding Produced  

- Exception instances with type, severity, affected items, timestamp  
- Recommended resolution actions  

### 5.11.8 Enterprise Outputs  

| ID | Output | Description |
|----|--------|-------------|
| OUT‑SI‑110 | Supply Exception | Exception record |
| OUT‑SI‑111 | Exception Resolution Recommendation | Suggested action |

### 5.11.9 Preconditions  

- Baseline supply data and thresholds configured  

### 5.11.10 Capability Dependencies  

- `CA‑SI‑001 Understand Supply` – for baseline data  
- `CA‑SI‑002 Plan Supply` – for expected supply  

### 5.11.11 Collaborating Capabilities  

- **Sense Supply Changes** – feeds disruptions  
- **Explain Supply Decisions** – generates exception explanations  

### 5.11.12 Business Decisions  

---

#### DE‑SI‑110 — Classify Supply Exception  

**Purpose:** Determine the type of an anomaly (Shortage, Excess, Late Delivery, Capacity Violation, Quality Failure, Data Gap).

**Required Understanding:** Anomalous data, context.

**Decision Criteria:** Rules‑based classification.

**Decision Rationale:** “Exception EX‑S‑2001 classified as Shortage: inventory projected below safety stock for 3 weeks.”  

---

##### Rules (for DE‑SI‑110)  

| ID | Rule | Category | Description |
|----|------|----------|-------------|
| BR‑SI‑110 | Exception Classification Rule (Supply) | Derivation Rule | Classify based on condition: Shortage if projected inventory < safety stock for ≥ 2 periods; Late Delivery if supplier confirmation date > required date + 1 day; etc. |

##### Policies (for DE‑SI‑110)  

| ID | Policy | Category | Description |
|----|--------|----------|-------------|
| PO‑SI‑110 | False Positive Filtering Policy | Automation Policy | Exceptions that self‑resolve within 24 hours are logged but not presented. |

---

#### DE‑SI‑111 — Prioritize Supply Exception  

**Purpose:** Assign severity based on business impact.

**Decision Criteria:** Exception type × item priority matrix.

**Decision Rationale:** “Exception prioritized as Critical: Shortage on Gold product.”  

---

##### Rules (for DE‑SI‑111)  

| ID | Rule | Category | Description |
|----|------|----------|-------------|
| BR‑SI‑111 | Exception Priority Matrix Rule (Supply) | Derivation Rule | Matrix maps exception type and item priority to severity. (Appendix). |

##### Policies (for DE‑SI‑111)  

| ID | Policy | Category | Description |
|----|--------|----------|-------------|
| PO‑SI‑111 | Escalation Policy (Supply) | Authorization Policy | Critical exceptions escalated to Supply Manager. |

---

#### DE‑SI‑112 — Resolve Supply Exception  

**Purpose:** Determine resolution action (auto‑resolve or manual).

**Decision Criteria:** Based on type, confidence, automation policy.

**Decision Rationale:** “Shortage auto‑resolved: triggered expedite request.”  

---

##### Rules (for DE‑SI‑112)  

| ID | Rule | Category | Description |
|----|------|----------|-------------|
| BR‑SI‑112 | Auto‑Resolution Eligibility Rule (Supply) | Validation Rule | Auto‑resolve if confidence ≥ 90% and severity not Critical. |

##### Policies (for DE‑SI‑112)  

| ID | Policy | Category | Description |
|----|--------|----------|-------------|
| PO‑SI‑112 | Auto‑Resolution Policy (Supply) | Automation Policy | Eligible exceptions are auto‑resolved; others require planner action. |
| PO‑SI‑113 | Resolution SLA Policy (Supply) | Compliance Policy | Critical: 2 hrs, High: 8 hrs, Medium: 48 hrs. |

---

### 5.11.13 Functional Behaviour, Commands, Events, Queries, Reports, Dashboards, Software Realization  

Analogous to Demand Exception detection, adapted for supply.

---

## 5.12 Explain Supply Decisions  

### 5.12.1 Purpose  

Generate clear, traceable explanations for every supply plan, inventory policy, procurement decision, production schedule, and exception. Answers: *“Why did the system recommend this?”*

### 5.12.2–5.12.12 (Structure mirrors Explain Demand capability, using supply‑specific decisions and the same explanation template.)

- **DE‑SI‑120** – Generate Supply Plan Explanation  
- **DE‑SI‑121** – Generate Exception Explanation  
- **DE‑SI‑122** – Generate Decision Explanation  
- Rules: completeness, traceability, natural language.  
- Policies: quality thresholds, logging.  

### 5.12.13 Functional Behaviour  

Triggers on plan publication, exception classification, any decision. Generates structured explanation and publishes event.

---

## 5.13 Learn From Supply  

### 5.13.1 Purpose  

Continuously improve supply intelligence by analyzing outcomes, detecting patterns, and recommending enhancements to models, policies, thresholds, and processes. Answers: *“How can we get better?”*

### 5.13.2–5.13.12 (Structure mirrors Learn From Demand.)

- **DE‑SI‑130** – Recommend Model Improvement (supply planning, inventory, scheduling models)  
- **DE‑SI‑131** – Recommend Threshold Adjustment (disruption, exception thresholds)  
- **DE‑SI‑132** – Propose New Supply Pattern or Exception Type (discovered alternatives)  
- **DE‑SI‑133** – Close the Learning Loop (verify improvements)  

Rules: improvement significance, stability, alternative validation.  
Policies: approval, rollback.  

### 5.13.13 Functional Behaviour  

Scheduled and event‑driven analysis, improvement recommendations, feedback to relevant capabilities.

---

# Chapter 6 — External Interfaces

## 6.1 Purpose

This chapter defines every external interface that the Supply Intelligence domain exposes to other domains, external systems, and users. Each interface is specified with its purpose, contract, authentication, and the capability that owns it. This chapter is derived from the Commands, Queries, and Events defined in Chapter 5.

## 6.2 Enterprise APIs

### 6.2.1 Supply Data Ingestion API

| Attribute | Value |
|-----------|-------|
| Owner | Understand Supply (5.1) |
| Purpose | Accept supply transactions, inventory updates, order status changes, and supplier confirmations from source systems. |
| Protocol | REST (HTTPS) |
| Authentication | OAuth 2.0 (Client Credentials) |
| Rate Limit | 10,000 requests/minute |
| Endpoint | `POST /api/v1/supply/data` |

**Request Body:**
```json
{
  "source": "ERP_System",
  "batchId": "uuid",
  "transactions": [
    {
      "type": "INVENTORY_UPDATE",
      "productId": "SKU123",
      "locationId": "LOC01",
      "quantity": 520,
      "unit": "EA",
      "timestamp": "2026-06-28T08:00:00Z"
    }
  ]
}
```

**Response (202 Accepted):**
```json
{
  "batchId": "uuid",
  "accepted": 1,
  "status": "processing"
}
```

---

### 6.2.2 Supply Position Query API

| Attribute | Value |
|-----------|-------|
| Owner | Understand Supply (5.1) |
| Purpose | Retrieve current inventory position, open orders, and capacity status. |
| Protocol | REST (HTTPS) |
| Endpoint | `GET /api/v1/supply/position` |

**Query Parameters:** `productId`, `locationId`.

**Response (200 OK):**
```json
{
  "productId": "SKU123",
  "locationId": "LOC01",
  "onHand": 500,
  "onOrder": 200,
  "allocated": 50,
  "backorder": 0,
  "lastUpdated": "2026-06-28T08:00:00Z"
}
```

---

### 6.2.3 Supply Plan Query API

| Attribute | Value |
|-----------|-------|
| Owner | Plan Supply (5.2) |
| Purpose | Retrieve the current published supply plan. |
| Protocol | REST (HTTPS) |
| Endpoint | `GET /api/v1/supply/plan` |

**Query Parameters:** `productId`, `locationId`, `startDate`, `endDate`.

**Response:**
```json
{
  "planVersion": "2026-W27",
  "publishedAt": "2026-06-28T06:00:00Z",
  "items": [
    {
      "productId": "SKU123",
      "locationId": "LOC01",
      "period": "2026-07-05",
      "plannedProduction": 300,
      "plannedProcurement": 0,
      "plannedTransfersIn": 0
    }
  ]
}
```

---

### 6.2.4 Inventory Policy API

| Attribute | Value |
|-----------|-------|
| Owner | Manage Inventory (5.3) |
| Purpose | Retrieve and update inventory policies (safety stock, reorder points). |
| Endpoint | `GET /api/v1/inventory/policy/{productId}/{locationId}` |
| Endpoint | `PUT /api/v1/inventory/policy/{productId}/{locationId}` |

---

### 6.2.5 Replenishment Recommendation API

| Attribute | Value |
|-----------|-------|
| Owner | Manage Inventory (5.3) |
| Purpose | Retrieve current replenishment recommendations. |
| Endpoint | `GET /api/v1/inventory/replenishment` |

---

### 6.2.6 Procurement API

| Attribute | Value |
|-----------|-------|
| Owner | Procure Materials (5.6) |
| Purpose | Retrieve procurement recommendations, create requisitions, release POs. |
| Endpoints | `GET /api/v1/procurement/recommendations`, `POST /api/v1/procurement/requisitions`, `POST /api/v1/procurement/purchase-orders` |

---

### 6.2.7 Production Schedule API

| Attribute | Value |
|-----------|-------|
| Owner | Schedule Production (5.7) |
| Purpose | Retrieve production schedule, release orders. |
| Endpoints | `GET /api/v1/production/schedule`, `POST /api/v1/production/orders/release` |

---

### 6.2.8 Distribution API

| Attribute | Value |
|-----------|-------|
| Owner | Manage Distribution (5.8) |
| Purpose | Retrieve transfer recommendations, release transfer orders. |
| Endpoints | `GET /api/v1/distribution/transfers`, `POST /api/v1/distribution/transfer-orders` |

---

### 6.2.9 Supplier Collaboration API

| Attribute | Value |
|-----------|-------|
| Owner | Collaborate with Suppliers (5.5) |
| Purpose | Retrieve supplier scorecards, commitments, risk assessments; share forecasts. |
| Endpoints | `GET /api/v1/suppliers/{id}/scorecard`, `GET /api/v1/suppliers/{id}/commitments`, `POST /api/v1/suppliers/{id}/forecast-share` |

---

### 6.2.10 Exception and Alert API

| Attribute | Value |
|-----------|-------|
| Owner | Detect Supply Exceptions (5.11), Sense Supply Changes (5.9) |
| Purpose | Retrieve active exceptions and supply alerts. |
| Endpoint | `GET /api/v1/supply/exceptions`, `GET /api/v1/supply/alerts` |

---

### 6.2.11 Explanation API

| Attribute | Value |
|-----------|-------|
| Owner | Explain Supply Decisions (5.12) |
| Purpose | Retrieve structured explanation for any supply artifact. |
| Endpoint | `GET /api/v1/supply/explanations/{artifactId}` |

---

## 6.3 Integration Events

Supply Intelligence publishes events to the enterprise event bus (Kafka topic: `supply-intelligence-events`). All events use the CloudEvents v1.0 envelope.

| Event Type | Payload Summary | Publisher Capability | Consumers |
|------------|-----------------|---------------------|-----------|
| `InventoryPositionUpdated` | Product, location, on‑hand, on‑order | Understand Supply | Manage Inventory, Plan Supply, Manage Distribution |
| `SupplyPictureUpdated` | Snapshot timestamp, quality score | Understand Supply | All supply capabilities |
| `SupplyPlanPublished` | Plan version, horizon, scope | Plan Supply | Procure Materials, Schedule Production, Manage Distribution, Manage Inventory, Manage Capacity |
| `InventoryPolicyUpdated` | Product, location, safety stock, ROP | Manage Inventory | Plan Supply, Procure Materials |
| `ReplenishmentRecommended` | Product, location, order qty, due date | Manage Inventory | Procure Materials |
| `CapacityFeasibilityAssessed` | Resource, overload/underload | Manage Capacity | Plan Supply, Schedule Production |
| `CapacityPlanPublished` | Plan version | Manage Capacity | Plan Supply |
| `SupplierCommitmentEvaluated` | Supplier, confidence factor | Collaborate with Suppliers | Plan Supply, Procure Materials |
| `SupplierRiskAssessed` | Supplier, risk score | Collaborate with Suppliers | Procure Materials |
| `RequisitionCreated` | Requisition ID, items | Procure Materials | ERP |
| `PurchaseOrderReleased` | PO ID, supplier, items | Procure Materials | Supplier systems, ERP |
| `ProductionSchedulePublished` | Schedule version, resource | Schedule Production | MES, Manage Inventory (for material) |
| `TransferOrderReleased` | Transfer ID, source, destination | Manage Distribution | WMS, TMS |
| `SupplyDisruptionDetected` | Event ID, severity, affected orders | Sense Supply Changes | Detect Supply Exceptions, Plan Supply |
| `SupplyExceptionDetected` | Exception ID, type, severity | Detect Supply Exceptions | Explain Supply Decisions, Learn From Supply |
| `SupplyExceptionResolved` | Exception ID, resolution | Detect Supply Exceptions | Learn From Supply |
| `SupplyQualityReportPublished` | Report ID, period, metrics | Evaluate Supply Quality | Learn From Supply, Management |
| `SupplyDecisionExplanationGenerated` | Decision ID, traceability chain | Explain Supply Decisions | Audit, AI agents |
| `SupplyImprovementRecommended` | Type, target, benefit | Learn From Supply | Model Training, Planning Teams |

---

## 6.4 Import Interfaces

| Interface | Format | Frequency | Target Capability |
|-----------|--------|-----------|-------------------|
| Supplier Master Import | CSV / JSON via SFTP | Daily | Understand Supply |
| BOM and Routing Import | CSV | On change | Understand Supply, Manage Capacity |
| Capacity Calendar Import | CSV | Weekly | Manage Capacity |
| Supplier Contract Import | CSV | On change | Procure Materials |
| Historical Supply Data Load | Parquet | One‑time | Understand Supply |

---

## 6.5 Export Interfaces

| Interface | Format | Frequency | Source Capability |
|-----------|--------|-----------|-------------------|
| Purchase Order Export to ERP | EDI / API | Real‑time | Procure Materials |
| Production Schedule Export to MES | API | Daily | Schedule Production |
| Transfer Order Export to WMS | API | Real‑time | Manage Distribution |
| Supplier Forecast Share | API / Portal | Weekly | Collaborate with Suppliers |
| Supply Quality Report Distribution | PDF / Email | Weekly | Evaluate Supply Quality |

---

# Chapter 7 — Reports & Dashboards

## 7.1 Purpose

This chapter consolidates every report and dashboard defined across the thirteen Supply Intelligence capabilities. Each entry includes its purpose, source capability, audience, frequency, and key content.

## 7.2 Reports

| Report ID | Name | Source Capability | Audience | Frequency | Content Summary |
|-----------|------|-------------------|----------|-----------|-----------------|
| RPT‑SI‑001 | Supply Data Quality Report | Understand Supply | Supply Data Steward, Supply Manager | Daily, Weekly | Completeness, freshness, accuracy by source |
| RPT‑SI‑002 | Inventory Reconciliation Report | Understand Supply | Inventory Manager | Weekly | Discrepancies, adjustments, root causes |
| RPT‑SI‑003 | Supply Plan Accuracy Report | Plan Supply, Evaluate Supply Quality | Supply Planner, Manager | Weekly, Monthly | Plan adherence, deviation analysis |
| RPT‑SI‑004 | Constraint Utilization Report | Plan Supply | Supply Planner, Capacity Manager | Weekly | Binding constraints, bottleneck analysis |
| RPT‑SI‑005 | Inventory Policy Compliance Report | Manage Inventory | Inventory Manager | Monthly | Adherence to recommended vs. overridden policies |
| RPT‑SI‑006 | Replenishment Action Report | Manage Inventory | Supply Planner | Weekly | Orders generated, deferred, expedited |
| RPT‑SI‑007 | Inventory Health Report | Manage Inventory | Supply Manager, Finance | Monthly | Distribution of health status, aging of excess |
| RPT‑SI‑008 | Capacity Utilization Report | Manage Capacity, Evaluate Supply Quality | Production Manager | Weekly, Monthly | Actual vs. planned utilization |
| RPT‑SI‑009 | Bottleneck Analysis Report | Manage Capacity | Production Manager | Weekly | Constraints and throughput impact |
| RPT‑SI‑010 | Supplier Scorecard Report | Collaborate with Suppliers | Procurement Manager | Monthly | OTD, quality, responsiveness per supplier |
| RPT‑SI‑011 | Supplier Risk Report | Collaborate with Suppliers | Procurement Manager | Monthly | Risk scores, mitigation status |
| RPT‑SI‑012 | Procurement Action Report | Procure Materials | Procurement Manager | Weekly | Requisitions created, POs released, spend summary |
| RPT‑SI‑013 | Supplier Allocation Report | Procure Materials | Procurement Manager | Monthly | Actual vs. quota allocation |
| RPT‑SI‑014 | Schedule Adherence Report | Schedule Production | Production Manager | Weekly | Planned vs. actual completion |
| RPT‑SI‑015 | Changeover Analysis Report | Schedule Production | Production Manager | Weekly | Changeover frequency and duration |
| RPT‑SI‑016 | Network Balance Report | Manage Distribution | Supply Manager | Weekly | Surplus/deficit by node and product |
| RPT‑SI‑017 | Transfer Cost Report | Manage Distribution | Supply Manager | Monthly | Transportation spend for rebalancing |
| RPT‑SI‑018 | Supply Disruption Report | Sense Supply Changes | Supply Manager | Weekly | Frequency, severity, response time |
| RPT‑SI‑019 | Supply Quality Report | Evaluate Supply Quality | Supply Chain Director | Weekly | Consolidated accuracy and health metrics |
| RPT‑SI‑020 | Continuous Improvement Report (Supply) | Learn From Supply | Supply Chain Director | Monthly, Quarterly | Improvements proposed, implemented, verified |

---

## 7.3 Dashboards

| Dashboard ID | Name | Source Capabilities | Audience | Refresh | Panels Summary |
|--------------|------|---------------------|----------|---------|----------------|
| DASH‑SI‑001 | Supply Health Dashboard | Understand Supply | Supply Planner, Manager | Real‑time | Inventory positions, open orders, capacity at a glance |
| DASH‑SI‑002 | Supply Data Quality Monitor | Understand Supply | Supply Data Steward | 5 min | Source reliability trends, stale data alerts |
| DASH‑SI‑003 | Supply Plan Dashboard | Plan Supply | Supply Planner, Manager | Daily | Plan summary, gaps, confidence |
| DASH‑SI‑004 | Constraint Monitor | Plan Supply, Manage Capacity | Capacity Planner | Real‑time | Real‑time constraint status |
| DASH‑SI‑005 | Inventory Optimization Dashboard | Manage Inventory | Inventory Manager | Daily | Policy performance, safety stock vs. actual, service vs. cost |
| DASH‑SI‑006 | Replenishment Workbench | Manage Inventory | Supply Planner | Daily | Planner actions, overdue orders, recommended orders |
| DASH‑SI‑007 | Capacity Control Tower | Manage Capacity | Production Manager | Real‑time | Load/capacity gauges |
| DASH‑SI‑008 | Resource Utilization Heatmap | Manage Capacity | Production Manager | Daily | Visual overload/underload |
| DASH‑SI‑009 | Supplier Collaboration Hub | Collaborate with Suppliers | Procurement Manager | Daily | Commitments, forecast shares, performance trends |
| DASH‑SI‑010 | Supplier Risk Dashboard | Collaborate with Suppliers | Procurement Manager | Weekly | Heatmap of risk factors |
| DASH‑SI‑011 | Procurement Workbench | Procure Materials | Procurement Manager | Daily | Planned orders, requisitions, pending approvals |
| DASH‑SI‑012 | Spend Dashboard | Procure Materials | Finance, Procurement | Monthly | Committed spend by supplier, category, period |
| DASH‑SI‑013 | Production Schedule Board | Schedule Production | Production Scheduler | Real‑time | Gantt view per resource |
| DASH‑SI‑014 | Schedule Risk Dashboard | Schedule Production | Production Scheduler | Daily | Late orders, bottlenecks |
| DASH‑SI‑015 | Distribution Network View | Manage Distribution | Supply Manager | Real‑time | Map with inventory levels, transfers in flight |
| DASH‑SI‑016 | Allocation Dashboard | Manage Distribution | Supply Planner | Daily | Constrained items and allocation decisions |
| DASH‑SI‑017 | Supply Disruption Monitor | Sense Supply Changes | Supply Manager | Real‑time | Live alert feed with severity indicators |
| DASH‑SI‑018 | Supply Exception Monitor | Detect Supply Exceptions | Supply Planner | Real‑time | Active exceptions by severity, aging |
| DASH‑SI‑019 | Supply Performance Dashboard | Evaluate Supply Quality | Supply Chain Director | Daily | Plan adherence, fill rate, inventory health |
| DASH‑SI‑020 | Learning Dashboard (Supply) | Learn From Supply | Supply Chain Director | Monthly | Improvement funnel, learning effectiveness |

---

# Chapter 8 — Appendix

## 8.1 Supply Exception Priority Matrix

The following matrix defines the default mapping from Supply Exception Type and Business Priority to Exception Severity. It is referenced by DE‑SI‑111 (Prioritize Supply Exception) in Section 5.11.

| Exception Type | Critical (Gold) | High (Silver) | Medium (Bronze) | Low (Unclassified) |
|----------------|-----------------|---------------|-----------------|---------------------|
| Shortage | Critical | Critical | High | Medium |
| Late Delivery | Critical | High | High | Medium |
| Capacity Violation | Critical | High | Medium | Medium |
| Quality Failure | Critical | Critical | High | High |
| Excess Inventory | High | Medium | Low | Low |
| Data Gap | High | High | Medium | Low |

**Notes:**
- False Positives are filtered before prioritization; they do not receive a severity.
- The matrix is configurable and may be adjusted via the learning feedback loop (DE‑SI‑131) subject to policy PO‑SI‑102.

---

## 8.2 Enterprise Glossary

A consolidated glossary of all enterprise terms defined across the Supply Intelligence Specification and referenced architecture documents. Each entry includes the unique identifier where applicable.

| Term | ID (if any) | Definition |
|------|-------------|------------|
| Allocation Rule | SE‑SI‑072 | Logic for distributing available supply when demand exceeds supply. |
| Backorder Rate | PI‑SI‑011 | Percentage of demand requests placed on backorder. |
| Bill of Materials (BOM) | SE‑SI‑060 | Structured list of components required to produce a finished good. |
| Bottleneck | SE‑SI‑032 | Resource whose capacity is less than demand, limiting throughput. |
| Capacity | SE‑SI‑004 | Maximum output a resource can achieve in a given period. |
| Capacity Utilization | PI‑SI‑005 | Ratio of actual output to available capacity. |
| Cash‑to‑Cash Cycle Time | PI‑SI‑015 | Days between paying for materials and receiving payment for goods. |
| Changeover | SE‑SI‑065 | Time and cost to switch a resource between products. |
| Days of Supply | PI‑SI‑003 | Number of days on‑hand inventory can satisfy average daily demand. |
| Distribution Lead Time | SE‑SI‑073 | Time to move goods between locations. |
| Distribution Network | SE‑SI‑070 | Set of locations and transportation lanes. |
| Economic Order Quantity (EOQ) | SE‑SI‑023 | Order quantity minimizing ordering and holding cost. |
| Excess Inventory | SE‑SI‑025 | Inventory exceeding reasonable demand coverage. |
| Fill Rate (Supply) | PI‑SI‑004 | Percentage of demand fulfilled from on‑hand inventory. |
| Inventory | SE‑SI‑003 | Stock of goods held by the enterprise. |
| Inventory Policy | SE‑SI‑024 | Rules governing how an item is replenished. |
| Inventory Position | SE‑SI‑020 | On‑hand + on‑order − allocated − backorders. |
| Inventory Turnover | PI‑SI‑002 | Times average inventory is sold and replaced per period. |
| Obsolete Inventory | SE‑SI‑026 | Inventory with no expected future demand. |
| Perfect Order Fulfillment (Supply) | PI‑SI‑009 | Percentage of supply orders fulfilled without any error. |
| Procurement Lead Time | SE‑SI‑053 | Time from requisition creation to goods receipt. |
| Production Order | SE‑SI‑062 | Firm instruction to produce a quantity by a date. |
| Production Schedule | SE‑SI‑063 | Time‑phased sequence of production orders per resource. |
| Purchase Order | SE‑SI‑051 | Binding document to a supplier. |
| Purchase Requisition | SE‑SI‑050 | Internal request for procurement. |
| Reorder Point | SE‑SI‑022 | Inventory position that triggers replenishment. |
| Routing | SE‑SI‑061 | Sequence of operations to produce a product. |
| Safety Stock | SE‑SI‑021 | Inventory buffer against demand and supply variability. |
| Schedule Adherence | PI‑SI‑006 | Percentage of orders completed on time and in full. |
| Stockout Frequency | PI‑SI‑012 | Number of stockout events per period. |
| Supplier | SE‑SI‑005 | External entity providing materials or services. |
| Supplier Commitment | SE‑SI‑041 | Confirmed delivery promise from a supplier. |
| Supplier Lead Time | SE‑SI‑042 | Time from PO placement to goods receipt. |
| Supplier On‑Time Delivery | PI‑SI‑007 | Percentage of deliveries received on time. |
| Supplier Performance | SE‑SI‑040 | Quantified assessment of supplier reliability. |
| Supply | SE‑SI‑001 | Ability and intention to provide products to meet demand. |
| Supply Constraint | SE‑SI‑012 | Limitation restricting production, procurement, or movement. |
| Supply Lead Time | SE‑SI‑014 | Time from recognizing supply need to goods availability. |
| Supply Plan | SE‑SI‑002 | Time‑phased projection of planned supply quantities. |
| Supply Plan Adherence | PI‑SI‑010 | Degree to which execution follows the supply plan. |
| Supply Variability | SE‑SI‑013 | Fluctuation in actual supply versus plan. |
| Total Supply Chain Cost | PI‑SI‑008 | Sum of all costs to plan, source, produce, hold, and deliver. |
| Transfer Order | SE‑SI‑071 | Internal order to move inventory between locations. |

---

## 8.3 Formula Reference

Complete set of formulas used in Chapter 3 (Enterprise Measurement Model).

**PI‑SI‑002 — Inventory Turnover**
```
Inventory Turnover = Cost of Goods Sold (COGS) ÷ Average Inventory Value
```

**PI‑SI‑003 — Days of Supply**
```
Days of Supply = On‑Hand Inventory ÷ Average Daily Demand
Average Daily Demand = Total Demand ÷ Number of Days
```

**PI‑SI‑004 — Fill Rate (Supply)**
```
Fill Rate (%) = (Quantity Fulfilled from On‑Hand ÷ Total Quantity Requested) × 100
```

**PI‑SI‑005 — Capacity Utilization**
```
Capacity Utilization (%) = (Actual Output ÷ Maximum Available Capacity) × 100
```

**PI‑SI‑006 — Schedule Adherence**
```
Schedule Adherence (%) = (Number of Orders Completed OTIF ÷ Total Scheduled Orders) × 100
```

**PI‑SI‑007 — Supplier On‑Time Delivery**
```
Supplier OTD (%) = (Number of Deliveries Received On Time ÷ Total Deliveries) × 100
```

**PI‑SI‑008 — Total Supply Chain Cost**
```
Total Supply Chain Cost = Procurement Cost + Production Cost + Inventory Holding Cost + Distribution Cost + Obsolescence Cost + Planning & Admin Cost
```

**PI‑SI‑009 — Perfect Order Fulfillment (Supply)**
```
Perfect Order Fulfillment (%) = (Number of Perfect Supply Orders ÷ Total Supply Orders) × 100
```

**PI‑SI‑010 — Supply Plan Adherence**
```
Supply Plan Adherence (%) = (Quantity Executed Per Plan ÷ Total Planned Quantity) × 100
```

**PI‑SI‑011 — Backorder Rate**
```
Backorder Rate (%) = (Quantity Backordered ÷ Total Quantity Requested) × 100
```

**PI‑SI‑012 — Stockout Frequency**
```
Stockout Frequency = Count of Stockout Events in period
```

**PI‑SI‑013 — Excess & Obsolete Inventory**
```
E&O (%) = (Value of Excess & Obsolete Inventory ÷ Total Inventory Value) × 100
```

**PI‑SI‑014 — Planning Cycle Time (Supply)**
```
Planning Cycle Time = Time(Plan Published) − Time(Demand Input Received)
```

**PI‑SI‑015 — Cash‑to‑Cash Cycle Time**
```
Cash‑to‑Cash = Days of Inventory Outstanding + Days Sales Outstanding − Days Payable Outstanding
```

---

## 8.4 References

### Architecture Documents
- Medhavi APS Constitution
- Architecture Reference Standard (ARS) v1
- Semantic Model
- Capability Model
- Decision Model
- Rule & Policy Model

### Intelligence Specifications
- Demand Intelligence Specification
- Supply Intelligence Specification (this document)

### Dependency Specifications
- Promise Intelligence Specification (future)
- Scenario Intelligence Specification (future)
- Knowledge Intelligence Specification (future)

---
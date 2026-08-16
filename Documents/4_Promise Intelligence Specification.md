# Promise Intelligence Specification

# Chapter 1 — Purpose & Scope  

## 1.1 Purpose  

Promise Intelligence is the authoritative enterprise domain responsible for translating customer order requests into trusted, feasible, and timely commitments. Every order promise—whether an immediate confirmation, a calculated available‑to‑promise date, a capable‑to‑promise production commitment, an allocation‑based reservation, or an offered substitute—originates from and is governed by this specification.  

Promise Intelligence consumes trusted demand understanding from Demand Intelligence and constrained supply plans and inventory positions from Supply Intelligence. It evaluates real‑time availability, capacity, allocation, and business priorities to generate commitments that balance customer service, revenue, cost, and risk. It provides the customer‑facing commitment layer upon which order fulfillment, exception management, and enterprise learning depend.  

This specification defines every business objective, performance indicator, semantic concept, capability, decision, rule, policy, functional behaviour, interface, report, and dashboard that constitutes the Promise Intelligence domain. It is the single source of enterprise truth for order promising.  

## 1.2 Scope  

**Promise Intelligence includes:**  

- Real‑time and batch order promising (Available‑to‑Promise, Capable‑to‑Promise, Allocation‑based promising)  
- Multi‑level supply search: on‑hand inventory, planned supply, capacity availability, alternate sources, substitution options  
- Promise date setting and delivery commitment management  
- Order prioritization based on customer tier, margin, strategic value, and urgency  
- Allocation management: definition, reservation, and consumption tracking across channels and time buckets  
- Order change management: rescheduling, cancellation, partial delivery, backlog re‑promising  
- Substitution and alternate fulfillment logic (alternate products, alternate locations, alternate shipping points)  
- What‑if promise simulation for large or strategic orders  
- Customer collaboration: order status visibility, commitment updates, exception communication  
- Promise risk sensing: early identification of potential promise breaches  
- Promise quality measurement and performance reporting  
- Promise exception detection, prioritization, and resolution  
- Promise decision explainability and traceability  
- Continuous promise intelligence learning and improvement  

**Promise Intelligence excludes:**  

- Demand forecasting and demand signal processing (Demand Intelligence)  
- Supply planning, inventory policy setting, procurement, production scheduling, and distribution planning (Supply Intelligence)  
- Transportation execution, warehouse execution, and physical fulfillment (Execution systems)  
- Strategic network design and scenario planning (Scenario Intelligence)  
- Customer master data management (Core master data)  

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

## BO‑PI‑001 — Deliver Trusted Order Commitments  

**Business Motivation**  

Every customer order deserves a commitment that the enterprise can and will honor. Promise Intelligence shall transform order requests into trusted, reliable commitments by accurately evaluating supply availability, capacity, and business constraints. An unreliable commitment damages customer trust more than an honest delay.  

**Business Questions**  

- What is the current promised order book, and how reliable are those commitments?  
- Which commitments are at risk of breach, and why?  
- How accurate are our ATP and CTP calculations relative to actual fulfillment?  
- Where are commitments being made without proper feasibility checks?  

**Success Measures**  

| PI | Name |  
|----|------|  
| PI‑PI‑001 | Promise Intelligence Effectiveness (Reserved) |  
| PI‑PI‑005 | Promise Adherence |  
| PI‑PI‑002 | Order Fill Rate (Promise perspective) |  
| PI‑PI‑003 | On‑Time Delivery (to promise date) |  

---

## BO‑PI‑002 — Maximize Customer Service Reliability  

**Business Motivation**  

The enterprise’s reputation depends on fulfilling promises. Promise Intelligence shall maximize the percentage of orders delivered on time and in full as committed. Where perfect fulfillment is not possible, it shall communicate proactively and offer acceptable alternatives.  

**Business Questions**  

- What percentage of orders are fulfilled exactly as promised?  
- Where are the recurring failure points in the promising process?  
- Which customers are experiencing the most promise breaches?  
- How can we improve first‑promise accuracy?  

**Success Measures**  

| PI | Name |  
|----|------|  
| PI‑PI‑010 | Perfect Order Rate (Promise perspective) |  
| PI‑PI‑003 | On‑Time Delivery (to promise date) |  
| PI‑PI‑002 | Order Fill Rate (Promise perspective) |  
| PI‑PI‑011 | Customer Communication Accuracy |  

---

## BO‑PI‑003 — Optimize Order Promising Profitability  

**Business Motivation**  

Not all orders are equally profitable. Promise Intelligence shall evaluate the margin contribution, cost‑to‑serve, and strategic value of each order when making commitment and allocation decisions. Scarce supply shall be directed toward the most profitable and strategically important demand.  

**Business Questions**  

- What is the expected margin of each order committed?  
- Are we allocating constrained supply to the most profitable channels?  
- What is the cost of expediting or substituting versus losing the order?  
- How does our promising strategy impact total revenue and margin?  

**Success Measures**  

| PI | Name |  
|----|------|  
| PI‑PI‑013 | Revenue Impact of Promising |  
| PI‑PI‑015 | Cash Impact (lost sales vs. expedite cost) |  
| PI‑PI‑008 | Allocation Compliance |  

---

## BO‑PI‑004 — Minimize Order Cycle Time  

**Business Motivation**  

Speed of response is a competitive differentiator. Promise Intelligence shall minimize the time from order receipt to promise confirmation, enabling a superior customer experience and reducing order‑to‑cash cycles.  

**Business Questions**  

- How long does it take to provide a promise after order receipt?  
- Which orders are delayed in the promising process?  
- Where are manual interventions slowing down promising?  
- What is the trend in promise cycle time?  

**Success Measures**  

| PI | Name |  
|----|------|  
| PI‑PI‑004 | Order Cycle Time (order to promise) |  
| PI‑PI‑012 | Order Change Cycle Time |  
| PI‑PI‑014 | Planning Cycle Time (Promise) |  

---

## BO‑PI‑005 — Improve Order Visibility and Transparency  

**Business Motivation**  

Customers and internal stakeholders need real‑time visibility into order status, promise dates, and any changes. Promise Intelligence shall provide accurate, timely, and clear communication about every order’s commitment status, backorder position, and expected fulfillment.  

**Business Questions**  

- Do customers have access to accurate, real‑time order status?  
- Are promise changes communicated proactively?  
- What is the accuracy and timeliness of customer communications?  
- Where are visibility gaps causing customer dissatisfaction?  

**Success Measures**  

| PI | Name |  
|----|------|  
| PI‑PI‑011 | Customer Communication Accuracy |  
| PI‑PI‑005 | Promise Adherence (communicated vs. actual) |  
| PI‑PI‑107 | Order Prioritization Effectiveness |  

---

## BO‑PI‑006 — Increase Promising Automation  

**Business Motivation**  

Routine order promising—standard ATP checks, straightforward allocations, low‑risk commitments—shall be fully automated. This frees promising specialists to focus on complex orders, allocation exceptions, and customer collaboration, while accelerating cycle time and reducing manual error.  

**Business Questions**  

- What percentage of orders are promised automatically?  
- Which order types still require manual intervention, and why?  
- What is the error rate of automated versus manual promising?  
- How can we safely increase the touchless promising rate?  

**Success Measures**  

| PI | Name |  
|----|------|  
| PI‑PI‑017 | Promise Automation Rate |  
| PI‑PI‑018 | Manual Override Rate (Promise) |  
| PI‑PI‑019 | Touchless Promising Rate |  

---

## BO‑PI‑007 — Ensure Commitment Feasibility  

**Business Motivation**  

A promise is a commitment to deliver. Promise Intelligence shall ensure that every commitment is feasible given current supply, capacity, lead times, and constraints. Over‑promising to win an order damages trust, while under‑promising loses business. The goal is right‑promising: commitments the enterprise can reliably meet.  

**Business Questions**  

- Are there any commitments that are infeasible given current supply?  
- How frequently are promises broken due to supply failures?  
- Are promises being made against unreliable supply sources?  
- How can we improve the feasibility assessment without slowing promising?  

**Success Measures**  

| PI | Name |  
|----|------|  
| PI‑PI‑005 | Promise Adherence |  
| PI‑PI‑102 | ATP Accuracy |  
| PI‑PI‑103 | CTP Accuracy |  
| PI‑PI‑109 | Commitment Risk Score |  

---

## BO‑PI‑008 — Continuously Improve Promise Intelligence  

**Business Motivation**  

Promise Intelligence shall continuously evolve by learning from promise outcomes, supply reliability, customer feedback, and process performance. This objective ensures that promising becomes progressively more accurate, faster, and more profitable without requiring architectural redesign.  

**Business Questions**  

- Are promise recommendations improving over time?  
- Which promising strategies yield the highest adherence and profitability?  
- Which promising parameters require revision based on recent outcomes?  
- Where should Promise Intelligence evolve next?  

**Success Measures**  

| PI | Name |  
|----|------|  
| PI‑PI‑105 | Recommendation Quality Index (Promise) |  
| PI‑PI‑104 | Decision Confidence Index (Promise) |  
| PI‑PI‑106 | Explainability Score (Promise) |  
| PI‑PI‑108 | Learning Effectiveness Index (Promise) |  

---

# Chapter 3 — Enterprise Measurement Model

## 3.1 Measurement Architecture

The Enterprise Measurement Model defines every performance indicator used to evaluate Promise Intelligence. Each indicator is a first‑class enterprise object with a unique identifier, complete definition, formula, interpretation, worked example, limitations, and relationships.

**Three measurement tiers:**

| Range | Tier | Purpose |
|-------|------|---------|
| PI‑PI‑001 – PI‑PI‑049 | Business Outcome Measures | Measure business value delivered |
| PI‑PI‑050 – PI‑PI‑099 | Reserved | Future expansion |
| PI‑PI‑100 – PI‑PI‑199 | Intelligence Measures | Measure intelligence quality |
| PI‑PI‑200 – PI‑PI‑299 | Operational Measures | Measure system performance |

**PI‑PI‑001** is reserved for a future composite index—Promise Intelligence Effectiveness—to be derived after all underlying measures are defined.

---

## 3.2 Business Outcome Measures

### PI‑PI‑001 — Promise Intelligence Effectiveness [RESERVED]

This identifier is reserved for a future composite indicator that will aggregate Business Outcome Measures, Intelligence Measures, and Operational Measures into a single executive health score for the Promise Intelligence domain. It cannot be defined until all underlying measures exist and their interactions are understood.

---

### PI‑PI‑002 — Order Fill Rate (Promise Perspective)

**Definition**

Order Fill Rate (Promise Perspective) measures the percentage of customer orders that are fulfilled completely from available inventory or planned supply at the time of the initial promise, without backorders or partial shipments. It reflects the accuracy and feasibility of the promising process itself: did the enterprise make a promise it could keep at the first attempt?

Unlike the supply‑side Fill Rate (PI‑SI‑004), which measures supply execution, this metric measures the outcome of the promising decision relative to the customer order.

**Business Objectives**

- BO‑PI‑001 Deliver Trusted Order Commitments
- BO‑PI‑002 Maximize Customer Service Reliability

**Business Interpretation**

| Value | Interpretation |
|-------|----------------|
| 98% – 100% | World‑class promising reliability |
| 95% – 98% | Excellent promising reliability |
| 90% – 95% | Good promising reliability |
| 85% – 90% | Acceptable promising reliability |
| Below 85% | Promising reliability requires investigation |

Thresholds are configurable by customer tier and order type.

**Formula**

Order Fill Rate (Promise) (%) = ( Number of Orders Fulfilled as Promised ÷ Total Number of Promised Orders ) × 100

Where:

- Number of Orders Fulfilled as Promised = count of orders where the full requested quantity was fulfilled completely at the first shipment, on or before the promised date
- Total Number of Promised Orders = all customer orders that received a firm promise during the evaluation period, excluding cancelled orders where cancellation was requested by the customer

**Formula Variables**

| Variable | Type | Definition |
|----------|------|-------------|
| Order | Entity | A unique customer order containing one or more line items |
| Fulfilled as Promised | Boolean | True if every line item in the order was fulfilled in full on or before the promised date at the first shipment |
| Total Number of Promised Orders | Integer | Count of all orders that received a promise during the period |

**Preconditions**

- Every order shall have a recorded promise date and quantity
- Fulfilment status shall be determined at the point of first shipment
- Orders cancelled by the customer before the promise date may be excluded per policy

**Assumptions**

- An order is considered fulfilled as promised only if all line items are fully satisfied at the first shipment and delivered on or before the promised date
- Partial shipments disqualify the order from being counted as fulfilled as promised
- The promise date is the date communicated to the customer; internal planned dates do not constitute a promise for this metric

**Specification**

| Attribute | Value |
|-----------|-------|
| Inputs | Order master, Promise records, Shipment/delivery confirmations |
| Unit | Percentage (%) |
| Precision | One decimal place |
| Rounding | Round Half Up |
| Aggregation Levels | Customer, Customer Group, Product Family, Channel, Location, Business Unit, Enterprise |
| Frequency | Daily, Weekly, Monthly |
| Performance Targets | Target ≥98%, Warning 95–98%, Critical <95% (configurable) |
| Business Owner | Order Management / Customer Service |
| Business Consumers | Promise Manager, Customer Service Manager, Supply Chain Director |
| System Consumers | Dashboards, Reports, Order Promising Services |
| Derived From | Order, Promise, and Fulfilment data |
| Related PIs | PI‑PI‑003 On‑Time Delivery, PI‑PI‑005 Promise Adherence, PI‑PI‑010 Perfect Order Rate |

**Worked Example**

| Order ID | Lines | Promised Date | First Shipment Date | Quantity Fulfilled | Fulfilled as Promised? |
|----------|-------|---------------|----------------------|--------------------|------------------------|
| 1001 | 2 | 10‑Mar | 10‑Mar | Full | Yes |
| 1002 | 1 | 11‑Mar | 12‑Mar | Full | No (late) |
| 1003 | 3 | 12‑Mar | 12‑Mar | Full | Yes |
| 1004 | 1 | 13‑Mar | 13‑Mar | Partial | No (partial) |
| 1005 | 2 | 14‑Mar | 14‑Mar | Full | Yes |

Number of Orders Fulfilled as Promised = 3 (1001, 1003, 1005)

Total Number of Promised Orders = 5

Order Fill Rate (Promise) = (3 ÷ 5) × 100 = **60.0%**

Business Interpretation: **Promising reliability requires investigation** — significantly below 85%.

**Limitations**

- The measure is sensitive to promise date accuracy; if promises are routinely padded with excessive lead time, the fill rate will appear higher than actual service performance
- Partial fulfilment disqualifies the entire order; this penalizes complex multi‑line orders more heavily
- Does not distinguish between promise failures caused by promising inaccuracy versus supply execution failures

**Relationships**

| Relationship | Reference |
|--------------|-----------|
| Supports | BO‑PI‑001, BO‑PI‑002 |
| Compared With | PI‑PI‑003 On‑Time Delivery, PI‑PI‑005 Promise Adherence |
| Complemented By | PI‑PI‑010 Perfect Order Rate |
| Displayed In | Promise Performance Dashboard, Customer Service Dashboard |
| Used By | Promise Accuracy Review, Order Promising Improvement |

---

### PI‑PI‑003 — On‑Time Delivery (to Promise Date)

**Definition**

On‑Time Delivery (to Promise Date) measures the percentage of promised order lines that are delivered on or before the promised delivery date. It is the most direct measure of promise‑keeping: did the enterprise deliver when it said it would?

This metric differs from the supply‑side OTIF (PI‑SI‑012 / PI‑DI‑012) in that it measures delivery against the *promised date*, not the customer‑requested date. It specifically isolates the promising process quality.

**Business Objectives**

- BO‑PI‑001 Deliver Trusted Order Commitments
- BO‑PI‑002 Maximize Customer Service Reliability

**Business Interpretation**

| Value | Interpretation |
|-------|----------------|
| 95% – 100% | Excellent promise‑keeping |
| 90% – 95% | Good promise‑keeping |
| 80% – 90% | Acceptable promise‑keeping |
| Below 80% | Promise‑keeping requires investigation |

Thresholds are configurable.

**Formula**

On‑Time Delivery (Promise) (%) = ( Number of Order Lines Delivered On Time to Promise ÷ Total Number of Promised Order Lines ) × 100

Where:

- Delivered On Time to Promise = actual delivery date ≤ promised delivery date
- Total Number of Promised Order Lines = all order lines that received a firm promise during the evaluation period

**Formula Variables**

| Variable | Type | Definition |
|----------|------|-------------|
| Promised Delivery Date | Date | Date by which delivery was promised to the customer |
| Actual Delivery Date | Date | Date on which delivery was completed |
| Number of Order Lines Delivered On Time | Integer | Count of order lines meeting the on‑time criterion |
| Total Number of Promised Order Lines | Integer | Count of all promised order lines |

**Preconditions**

- A promised delivery date shall exist for every evaluated order line
- Actual delivery date shall be recorded for delivered lines
- Lines not yet delivered at evaluation time are excluded or tracked separately

**Assumptions**

- Early delivery is considered on‑time unless the customer agreement specifies a delivery window with a start date
- The promised date is the date communicated to the customer; internal revised dates that were not communicated do not count as the promise

**Specification**

| Attribute | Value |
|-----------|-------|
| Inputs | Promise records, Delivery confirmations |
| Unit | Percentage (%) |
| Precision | One decimal place |
| Rounding | Round Half Up |
| Aggregation Levels | Customer, Product, Channel, Location, Business Unit, Enterprise |
| Frequency | Daily, Weekly, Monthly |
| Performance Targets | Target ≥95%, Warning 90–95%, Critical <90% (configurable) |
| Business Owner | Order Management / Customer Service |
| Business Consumers | Promise Manager, Customer Service Manager, Logistics Manager |
| System Consumers | Dashboards, Reports |
| Derived From | Promise and delivery data |
| Related PIs | PI‑PI‑002 Order Fill Rate, PI‑PI‑005 Promise Adherence |

**Worked Example**

| Order Line | Promised Date | Actual Delivery | On Time? |
|------------|---------------|-----------------|----------|
| L1 | 05‑Mar | 05‑Mar | Yes |
| L2 | 06‑Mar | 07‑Mar | No |
| L3 | 07‑Mar | 07‑Mar | Yes |
| L4 | 08‑Mar | 08‑Mar | Yes |
| L5 | 09‑Mar | 12‑Mar | No |

Number Delivered On Time = 3

Total Lines = 5

On‑Time Delivery = (3 ÷ 5) × 100 = **60.0%**

Business Interpretation: **Promise‑keeping requires investigation**.

**Limitations**

- Over‑promising (long lead times) inflates this metric but may reduce customer satisfaction and competitiveness
- Does not capture quantity accuracy; a line delivered on time but short is still counted as on‑time in this metric — use Order Fill Rate for quantity completeness

**Relationships**

| Relationship | Reference |
|--------------|-----------|
| Supports | BO‑PI‑001, BO‑PI‑002 |
| Compared With | PI‑PI‑002 Order Fill Rate |
| Complemented By | PI‑PI‑005 Promise Adherence |
| Displayed In | Promise Performance Dashboard |
| Used By | Promise Accuracy Review, Carrier Performance |

---

### PI‑PI‑004 — Order Cycle Time (Order to Promise)

**Definition**

Order Cycle Time measures the elapsed time from order receipt to promise confirmation. It reflects the speed of the promising process itself. Shorter cycle times improve customer experience and reduce order‑to‑cash duration.

**Business Objectives**

- BO‑PI‑004 Minimize Order Cycle Time
- BO‑PI‑006 Increase Promising Automation

**Business Interpretation**

| Value (for standard orders) | Interpretation |
|-----------------------------|----------------|
| < 1 second (real‑time) | Excellent — instant promising |
| < 5 minutes | Very good — near‑real‑time |
| < 1 hour | Good — acceptable for most channels |
| < 24 hours | Adequate — may need improvement |
| > 24 hours | Slow — investigation required |

Thresholds vary by order type (standard vs. complex/CTP) and channel.

**Formula**

Order Cycle Time = Time(Promise Confirmed) − Time(Order Received)

Measured as an average (mean) or median across all orders in the evaluation period. Percentiles (50th, 95th) are recommended for reporting.

**Formula Variables**

| Variable | Type | Definition |
|----------|------|-------------|
| Order Received Timestamp | DateTime | Timestamp when the order was received and validated |
| Promise Confirmed Timestamp | DateTime | Timestamp when the promise was confirmed and communicated to the customer |

**Preconditions**

- Both timestamps shall be recorded with sufficient precision (seconds)
- Orders that are still awaiting promise at evaluation time are excluded from the average but reported as open

**Specification**

| Attribute | Value |
|-----------|-------|
| Inputs | Order timestamps, Promise confirmation timestamps |
| Unit | Time (seconds, minutes, hours) |
| Precision | Seconds |
| Aggregation Levels | Order Type, Channel, Business Unit, Enterprise |
| Frequency | Daily, Weekly, Monthly |
| Performance Targets | Target < 1 sec (standard ATP), < 5 min (CTP), Warning > 1 hour |
| Business Owner | Order Management |
| Business Consumers | Promise Manager, Customer Service, IT Operations |
| System Consumers | Dashboards, Monitoring |
| Derived From | Order and promise timestamps |
| Related PIs | PI‑PI‑012 Order Change Cycle Time, PI‑PI‑014 Planning Cycle Time |

**Worked Example**

| Order | Received | Promise Confirmed | Cycle Time (min) |
|-------|----------|-------------------|------------------|
| 1001 | 08:00:00 | 08:00:02 | 0.03 |
| 1002 | 08:05:00 | 08:07:30 | 2.5 |
| 1003 | 08:10:00 | 08:10:05 | 0.08 |
| 1004 | 08:15:00 | 08:45:00 | 30.0 |
| 1005 | 08:20:00 | 08:20:01 | 0.02 |

Average Cycle Time = (0.03 + 2.5 + 0.08 + 30.0 + 0.02) ÷ 5 = **6.53 minutes**

Median = 0.08 minutes

95th Percentile = 30.0 minutes

Business Interpretation: **Very good** average, but the 95th percentile indicates some orders are slow.

**Limitations**

- Outliers can skew the average significantly; always report median and percentiles alongside mean
- Does not distinguish between automated and manual promising; segmenting by automation type reveals improvement opportunities
- Does not account for business hours; an order received Friday evening may have a longer cycle time due to non‑business hours

**Relationships**

| Relationship | Reference |
|--------------|-----------|
| Supports | BO‑PI‑004, BO‑PI‑006 |
| Compared With | PI‑PI‑012 Order Change Cycle Time |
| Displayed In | Promise Operations Dashboard |
| Used By | Automation Improvement, Process Optimization |

---

### PI‑PI‑005 — Promise Adherence

**Definition**

Promise Adherence measures the degree to which actual fulfillment matches the original promise. It compares the original promised quantity and date against the actual delivered quantity and date. High adherence indicates reliable promising; low adherence indicates over‑promising or supply execution failures.

**Business Objectives**

- BO‑PI‑001 Deliver Trusted Order Commitments
- BO‑PI‑007 Ensure Commitment Feasibility

**Business Interpretation**

| Value | Interpretation |
|-------|----------------|
| 95% – 100% | Excellent adherence — promises are reliable |
| 90% – 95% | Good adherence |
| 80% – 90% | Acceptable adherence |
| Below 80% | Adherence requires investigation |

**Formula**

Promise Adherence (%) = ( Number of Order Lines Delivered as Promised ÷ Total Number of Promised Order Lines ) × 100

Where an order line is Delivered as Promised if: Actual Delivery Date ≤ Promised Date AND Delivered Quantity = Promised Quantity (within tolerance ±5%).

**Specification**

| Attribute | Value |
|-----------|-------|
| Inputs | Promise records, Delivery confirmations |
| Unit | Percentage (%) |
| Precision | One decimal place |
| Rounding | Round Half Up |
| Aggregation Levels | Customer, Product, Channel, Business Unit |
| Frequency | Weekly, Monthly |
| Performance Targets | Target ≥95%, Warning 90–95%, Critical <90% |
| Business Owner | Order Management |
| Related PIs | PI‑PI‑002, PI‑PI‑003 |

**Worked Example**

| Line | Promised Qty | Delivered Qty | Promised Date | Actual Date | Adherent? |
|------|-------------|---------------|---------------|-------------|-----------|
| L1 | 100 | 100 | 05‑Mar | 05‑Mar | Yes |
| L2 | 50 | 50 | 06‑Mar | 08‑Mar | No |
| L3 | 200 | 190 | 07‑Mar | 07‑Mar | No (short) |
| L4 | 75 | 75 | 08‑Mar | 08‑Mar | Yes |

Promise Adherence = (2 ÷ 4) × 100 = **50.0%**

Business Interpretation: **Adherence requires investigation**.

---

### PI‑PI‑006 — Order Rejection Rate

**Definition**

Order Rejection Rate measures the percentage of customer orders that cannot be promised (rejected or turned away) due to supply or capacity unavailability. High rejection rates indicate a mismatch between demand and supply capability, or overly conservative promising.

**Business Objectives**

- BO‑PI‑003 Optimize Order Promising Profitability
- BO‑PI‑007 Ensure Commitment Feasibility

**Business Interpretation**

| Value | Interpretation |
|-------|----------------|
| 0% – 2% | Excellent — minimal rejection |
| 2% – 5% | Good |
| 5% – 10% | Acceptable |
| Above 10% | Rejection rate requires investigation |

**Formula**

Order Rejection Rate (%) = ( Number of Orders Rejected ÷ Total Number of Orders Requested ) × 100

Where Rejected means no promise could be made (no ATP/CTP/Allocation available).

**Specification**

| Attribute | Value |
|-----------|-------|
| Inputs | Order request and promise outcome data |
| Unit | Percentage (%) |
| Frequency | Daily, Weekly |
| Business Owner | Order Management |
| Related PIs | PI‑PI‑002 Order Fill Rate, PI‑PI‑009 Backorder Conversion Rate |

---

### PI‑PI‑007 — Average Promise Lead Time

**Definition**

Average Promise Lead Time measures the average time promised to the customer from order placement to delivery. It reflects the competitiveness and realism of the promising process.

**Formula**

Average Promise Lead Time = Σ (Promised Delivery Date − Order Date) ÷ Number of Orders

**Specification**

| Attribute | Value |
|-----------|-------|
| Inputs | Order date, Promise date |
| Unit | Days |
| Frequency | Weekly, Monthly |
| Business Owner | Order Management |

---

### PI‑PI‑008 — Allocation Compliance

**Definition**

Allocation Compliance measures the degree to which actual order promising respects defined allocation rules. It tracks whether constrained supply was promised according to strategic allocation decisions rather than first‑come‑first‑served.

**Formula**

Allocation Compliance (%) = ( Number of Orders Promised Within Allocation ÷ Total Number of Orders Subject to Allocation ) × 100

**Specification**

| Attribute | Value |
|-----------|-------|
| Inputs | Allocation rules, Promise records |
| Unit | Percentage (%) |
| Frequency | Weekly, Monthly |
| Business Owner | Order Management / Supply Chain |

---

### PI‑PI‑009 — Backorder Conversion Rate

**Definition**

Backorder Conversion Rate measures the percentage of backordered orders that are eventually fulfilled. It reflects the enterprise’s ability to recover from initial promising failures.

**Formula**

Backorder Conversion Rate (%) = ( Number of Backorders Fulfilled ÷ Total Number of Backorders ) × 100

---

### PI‑PI‑010 — Perfect Order Rate (Promise Perspective)

**Definition**

Perfect Order Rate (Promise Perspective) measures the percentage of orders delivered perfectly relative to the promise: correct item, correct quantity, on time to promise date, correct location, accurate documentation, undamaged.

**Formula**

As PI‑DI‑013, but measured against the promised date, not the customer‑requested date.

---

### PI‑PI‑011 — Customer Communication Accuracy

**Definition**

Customer Communication Accuracy measures whether promise‑related communications (confirmations, changes, delays) were accurate, timely, and complete.

**Formula**

Customer Communication Accuracy (%) = ( Number of Communications Accurate and Timely ÷ Total Number of Communications ) × 100

---

### PI‑PI‑012 — Order Change Cycle Time

**Definition**

Order Change Cycle Time measures the time from an order change request (modification, cancellation) to its confirmation.

**Formula**

Order Change Cycle Time = Time(Change Confirmed) − Time(Change Requested)

---

### PI‑PI‑013 — Revenue Impact of Promising

**Definition**

Revenue Impact of Promising estimates the revenue retained or lost due to promising decisions: accepted orders, rejected orders, and orders lost to competition due to excessive lead times.

**Formula**

Revenue Impact = Σ Revenue from Accepted Orders − Σ Estimated Revenue from Rejected Orders (based on historical conversion)

---

### PI‑PI‑014 — Planning Cycle Time (Promise)

**Definition**

Planning Cycle Time measures the total elapsed time for a promise planning cycle (e.g., nightly re‑promise of backlog).

**Formula**

Planning Cycle Time = Time(Cycle Completed) − Time(Cycle Started)

---

### PI‑PI‑015 — Cash Impact (Lost Sales vs. Expedite Cost)

**Definition**

Cash Impact compares the cost of lost sales (rejected orders) against the cost of expediting (promising with premium freight or overtime) to determine the net financial impact of promising decisions.

**Formula**

Cash Impact = Cost of Expedites − Estimated Margin of Lost Sales Avoided

---

## 3.3 Intelligence Measures (Stubs)

| PI | Name | Description |
|----|------|-------------|
| PI‑PI‑101 | Promise Understanding Index | Composite measure of promise data quality and completeness. Reserved. |
| PI‑PI‑102 | ATP Accuracy | Accuracy of ATP quantity and date vs. actual availability. Reserved. |
| PI‑PI‑103 | CTP Accuracy | Accuracy of CTP production commitments vs. actual output. Reserved. |
| PI‑PI‑104 | Decision Confidence Index (Promise) | Average confidence across promise decisions. Reserved. |
| PI‑PI‑105 | Recommendation Quality Index (Promise) | Quality of promise recommendations. Reserved. |
| PI‑PI‑106 | Explainability Score (Promise) | Completeness and quality of promise explanations. Reserved. |
| PI‑PI‑107 | Order Prioritization Effectiveness | How well prioritization aligns promising with business value. Reserved. |
| PI‑PI‑108 | Allocation Optimization Effectiveness | How well allocation rules maximize strategic outcomes. Reserved. |
| PI‑PI‑109 | Commitment Risk Score | Risk of promise breach based on supply reliability. Reserved. |
| PI‑PI‑110 | Customer Collaboration Index | Depth and effectiveness of customer communication. Reserved. |

---

## 3.4 Operational Measures (Stubs)

| PI | Name | Description |
|----|------|-------------|
| PI‑PI‑201 | Promise Response Time | 95th percentile of API response time for promise requests. Reserved. |
| PI‑PI‑202 | ATP Check Latency | Time to complete an ATP check. Reserved. |
| PI‑PI‑203 | CTP Check Latency | Time to complete a CTP check. Reserved. |
| PI‑PI‑204 | Allocation Refresh Time | Time to update allocation consumption data. Reserved. |
| PI‑PI‑205 | System Availability (Promise) | Uptime of promise services. Reserved. |
| PI‑PI‑206 | Event Processing Latency (Promise) | Time to process and publish promise events. Reserved. |

---

# Chapter 4 — Semantic Foundation  

The following concepts establish the enterprise meaning upon which all Promise Intelligence capabilities operate. Each concept is a first‑class enterprise object with a unique identifier and a complete definition. This chapter mirrors the structure of the Demand and Supply Semantic Foundations, specialized for order promising.  

## 4.1 Core Enterprise Concepts  

| ID | Concept | Definition |
|----|---------|-------------|
| SE‑PI‑001 | Order | A customer’s request for one or more products, at specified quantities, to be delivered to a specified location by a requested date. An order is the fundamental unit of Promise Intelligence. It may consist of one or more order lines. |
| SE‑PI‑002 | Promise | A binding commitment made by the enterprise to a customer, specifying the quantity that will be delivered and the date by which delivery will occur. A promise is the primary output of the Promise Intelligence domain. |
| SE‑PI‑003 | Allocation | A pre‑reservation of a defined portion of available supply (inventory or planned supply) for a specific channel, customer group, or order type over a defined time horizon. Allocations constrain and guide the promising process. |
| SE‑PI‑004 | Commitment | A confirmed obligation to deliver. A commitment is the formal record of a promise, including the promised quantity, delivery date, and the source of supply (inventory, planned production, or allocation). |
| SE‑PI‑005 | Promise Status | The current state of a promise in its lifecycle: Requested, Evaluating, Promised, Rejected, Fulfilled, Breached, Cancelled. |

## 4.2 Order Concepts  

| ID | Concept | Definition |
|----|---------|-------------|
| SE‑PI‑010 | Order Line | A single line item within an order, specifying one product, quantity, unit of measure, requested delivery date, and ship‑to location. Each line may be promised independently. |
| SE‑PI‑011 | Order Request | The initial submission of an order before any promise evaluation. The request contains the customer’s desired terms: product, quantity, requested date, location. |
| SE‑PI‑012 | Order Status | The current lifecycle state of an order: Received, Validated, Under Evaluation, Promised, Partially Promised, Rejected, Fulfilled, Cancelled. |
| SE‑PI‑013 | Order Type | A classification of orders by business characteristics: Standard Order, Rush Order, Contract Order, Consignment Order, Intercompany Order, Sample Order. Order type influences promising priority and rules. |
| SE‑PI‑014 | Order Priority | A ranking assigned to an order based on customer tier, margin, strategic value, and urgency. Priority determines the sequence of promising evaluation when supply is constrained. |
| SE‑PI‑015 | Backorder Line | An order line that could not be immediately fulfilled and was placed on backorder. The backorder retains the original or a revised promise date and is subject to re‑promising as supply becomes available. |
| SE‑PI‑016 | Order Split | The division of a single order line into multiple partial shipments, each with its own promise date and quantity. Order splits occur when full quantity cannot be fulfilled from a single source at a single time. |

## 4.3 Promise Concepts  

| ID | Concept | Definition |
|----|---------|-------------|
| SE‑PI‑020 | Promise Date | The date by which the enterprise commits to deliver the promised quantity to the customer. The promise date may be the customer’s requested date, the first available date from ATP, or a later date based on supply constraints. |
| SE‑PI‑021 | Promise Type | The method by which the promise was determined: ATP (Available‑to‑Promise — from on‑hand or inbound supply), CTP (Capable‑to‑Promise — requiring production or capacity check), Allocation (drawn from a pre‑reserved pool), or Substitution (alternative product or location). |
| SE‑PI‑022 | Promise Confidence | A score (0–100%) reflecting the reliability of the promise, derived from the certainty of the underlying supply source, historical supplier reliability, and demand variability. |
| SE‑PI‑023 | Promise Expiry | The date and time after which an unaccepted promise is no longer valid. Promises may have an expiry to allow the enterprise to reallocate supply if the customer does not confirm. |
| SE‑PI‑024 | Promise Revision | A change to an existing promise, initiated either by the enterprise (e.g., due to a supply disruption) or by the customer (e.g., order change request). Each revision creates a new promise version while retaining the previous version for audit. |
| SE‑PI‑025 | Promise Breach | A failure to fulfill a promise: delivery after the promise date, delivery of less than the promised quantity, or both. A breach triggers exception handling and may initiate a revised promise. |

## 4.4 Allocation Concepts  

| ID | Concept | Definition |
|----|---------|-------------|
| SE‑PI‑030 | Allocation Rule | A defined logic that governs how constrained supply is reserved and distributed. Allocation rules specify the source of supply, the eligible demand (channel, customer, product), the quantity or percentage allocated, and the time window. |
| SE‑PI‑031 | Allocation Pool | A reserved quantity of supply (inventory or planned supply) set aside for a specific allocation rule over a defined time horizon. The pool is consumed as orders are promised against it. |
| SE‑PI‑032 | Allocation Consumption | The tracking of how much of an allocation pool has been used by promised orders. Consumption is updated in real‑time or near‑real‑time as promises are made, preventing over‑promising of reserved supply. |
| SE‑PI‑033 | Allocation Period | The time bucket (e.g., day, week) over which an allocation applies. Allocations may be defined for multiple periods, with unconsumed quantities rolling forward or expiring per policy. |
| SE‑PI‑034 | Allocation Exhaustion | The state where an allocation pool has been fully consumed. When exhaustion occurs, subsequent orders that would have drawn from that pool must seek alternative supply sources or be rejected. |

## 4.5 Commitment Concepts  

| ID | Concept | Definition |
|----|---------|-------------|
| SE‑PI‑040 | Commitment Level | The degree of firmness of a commitment: Firm (binding, will be fulfilled), Tentative (subject to final confirmation), Contingent (dependent on a condition, e.g., customer credit approval). |
| SE‑PI‑041 | Commitment Expiry | The date after which a firm commitment is no longer guaranteed if not yet fulfilled. Typically used for allocation‑based commitments where the allocation period has passed. |
| SE‑PI‑042 | Commitment Revision | A formal change to a commitment, initiated by the enterprise (due to supply change) or mutually agreed with the customer. |
| SE‑PI‑046 | Temporary Reservation | A short‑lived hold on a supply source during promise evaluation. It prevents the same supply from being double‑promised. A temporary reservation has a brief expiry (seconds to minutes) and a lifecycle: Created → Confirmed (becomes part of a commitment), Released (evaluation complete without commitment), or Expired (evaluation exceeded time limit). |
| SE‑PI‑047 | Reservation Lifecycle | The allowed state transitions for a Temporary Reservation: Created → Confirmed (when promise is made), Created → Released (when promise is rejected), Created → Expired (when evaluation exceeds the reservation window). Only Confirmed reservations permanently consume supply. |

## 4.6 Customer Concepts  

| ID | Concept | Definition |
|----|---------|-------------|
| SE‑PI‑050 | Customer Order Profile | A summary of a customer’s ordering behavior: average order size, frequency, variability, preferred channels, return rate, and payment history. Used to inform promising risk and priority. |
| SE‑PI‑051 | Customer Tier (Promise) | A classification of customers by business importance for promising purposes: Platinum, Gold, Silver, Bronze. Higher tiers receive priority access to constrained supply and more favorable promise dates. |
| SE‑PI‑052 | Communication Preference | The customer’s preferred channel and frequency for promise‑related communications: email, portal, EDI, SMS, frequency (real‑time, daily summary). |
| SE‑PI‑053 | Customer Communication Template | A standardized message format for promise confirmations, changes, and breach notifications. Templates ensure consistency and completeness. |

## 4.7 Availability Concepts  

| ID | Concept | Definition |
|----|---------|-------------|
| SE‑PI‑060 | ATP Check | Available‑to‑Promise check: a real‑time or batch evaluation of uncommitted inventory, inbound supply, and planned production to determine whether a requested quantity can be promised by the requested date. |
| SE‑PI‑061 | CTP Check | Capable‑to‑Promise check: an evaluation that goes beyond ATP to assess whether production capacity and materials can be made available to fulfill an order if current supply is insufficient. CTP may trigger a mini supply plan generation. |
| SE‑PI‑062 | ATP Check Result | The output of an ATP check: available quantity, earliest available date, source of supply (on‑hand, inbound PO, planned production), and confidence score. |
| SE‑PI‑063 | CTP Check Result | The output of a CTP check: feasible quantity, earliest production completion date, required materials, capacity confirmation, and confidence score. |
| SE‑PI‑064 | Supply Search | The process of scanning multiple supply sources (on‑hand at multiple locations, inbound shipments, planned production, alternate suppliers) to find the best fulfillment option for an order. |
| SE‑PI‑065 | Substitution Option | An alternative product, location, or shipping point offered to the customer when the primary request cannot be fulfilled as requested. Substitution is governed by substitution rules defining allowable alternatives and customer consent requirements. |
| SE‑PI‑066 | Substitution Rule | A rule defining when and how substitution is allowed: which products can substitute for which, whether customer consent is required, and any price or margin constraints. |

## 4.8 Exception Concepts  

| ID | Concept | Definition |
|----|---------|-------------|
| SE‑PI‑070 | Promise Breach | As defined in 4.3. Also an exception that triggers root cause analysis and corrective action. |
| SE‑PI‑071 | Allocation Exhaustion | As defined in 4.4. When exhaustion occurs unexpectedly early, it is treated as an exception. |
| SE‑PI‑072 | Order Change Exception | An exception raised when an order change request cannot be accommodated (e.g., expedite request when no supply exists, cancellation of a shipped order). |
| SE‑PI‑073 | ATP/CTP Failure | An exception raised when the ATP or CTP evaluation cannot be completed due to missing data, system failure, or inconsistent supply information. |

## 4.9 Order Relationships  

| ID | Concept | Definition |
|----|---------|-------------|
| SE‑PI‑080 | Order Dependency | A relationship where one order’s fulfillment depends on another (e.g., a parent order and its component orders, or linked orders from the same customer that must ship together). |
| SE‑PI‑081 | Order Consolidation | The combining of multiple orders from the same customer into a single shipment or promise. Consolidation may reduce transportation cost but may delay fulfillment. |
| SE‑PI‑082 | Order‑Supply Link | The traceable connection between a promised order line and the specific supply source (inventory lot, purchase order, production order) that will fulfill it. This link is critical for promise feasibility and traceability. |

## 4.10 Common Enumerations  

**Promise Status**  

| Value | Description |
|-------|-------------|
| Requested | Order received, promise not yet evaluated |
| Evaluating | ATP/CTP check in progress |
| Promised | Firm commitment made and communicated |
| Partially Promised | Some lines promised, some pending or rejected |
| Rejected | Cannot promise any quantity |
| Fulfilled | Promise fully delivered |
| Breached | Promise not met (late or short) |
| Cancelled | Order cancelled before fulfillment |

**Promise Type**  

| Value | Description |
|-------|-------------|
| ATP | Available‑to‑Promise from on‑hand or inbound supply |
| CTP | Capable‑to‑Promise requiring production |
| Allocation | Drawn from a pre‑reserved allocation pool |
| Substitution | Fulfilled with an alternative product or location |

**Order Status**  

| Value | Description |
|-------|-------------|
| Received | Order submitted and validated |
| Under Evaluation | Promise check in progress |
| Promised | Full promise made |
| Partially Promised | Some lines have promises, others pending |
| Rejected | No promise can be made |
| Fulfilled | All lines delivered |
| Cancelled | Order cancelled |

**Order Type**  

| Value | Description |
|-------|-------------|
| Standard Order | Regular customer order |
| Rush Order | High‑priority, expedited handling |
| Contract Order | Against a long‑term agreement |
| Consignment Order | Inventory held at customer location |
| Intercompany Order | Internal transfer between business units |
| Sample Order | No‑charge evaluation request |

**Commitment Level**  

| Value | Description |
|-------|-------------|
| Firm | Binding commitment |
| Tentative | Subject to final confirmation |
| Contingent | Dependent on a condition |

**Substitution Type**  

| Value | Description |
|-------|-------------|
| Product Substitution | Alternative product with similar function |
| Location Substitution | Ship from alternate warehouse |
| Grade Substitution | Higher or lower grade of same product |
| No Substitution | Substitution not allowed |

---

# Chapter 5 — Enterprise Capability Specifications  

## 5.1 Understand Orders  

### 5.1.1 Purpose  

Establish a trusted, complete, and current picture of the enterprise’s order book and customer promise landscape. Answers: *“What orders are we handling right now, what promises have we made, and what is the status of each?”* The capability serves as the single source of truth for all downstream promise reasoning, consolidating order requests, existing promises, backorders, customer profiles, and communication histories.  

### 5.1.2 Business Objectives Served  

- BO‑PI‑001 Deliver Trusted Order Commitments  
- BO‑PI‑005 Improve Order Visibility and Transparency  

### 5.1.3 Enterprise Measures  

- PI‑PI‑101 Promise Understanding Index  
- PI‑PI‑206 Event Processing Latency (Promise)  

### 5.1.4 Semantic Objects  

| ID | Object | Role |
|----|--------|------|
| SE‑PI‑001 | Order | Core unit |
| SE‑PI‑002 | Promise | Promise record |
| SE‑PI‑010 | Order Line | Detail line |
| SE‑PI‑011 | Order Request | Incoming request |
| SE‑PI‑012 | Order Status | Lifecycle state |
| SE‑PI‑014 | Order Priority | Ranking |
| SE‑PI‑015 | Backorder Line | Unfulfilled line |
| SE‑PI‑050 | Customer Order Profile | Customer context |
| SE‑PI‑051 | Customer Tier | Priority tier |

### 5.1.5 Primitive Capabilities Composed  

- **Observe** – ingests order requests, status changes, and customer updates  
- **Understand** – aggregates and cleanses order data into a unified order book  
- **Assess** – evaluates data completeness and quality  

### 5.1.6 Enterprise Inputs  

- Order requests from e‑commerce, EDI, sales orders, and customer portals  
- Existing promises and their statuses  
- Backorder records and re‑promise candidates  
- Customer master data (tier, profile, communication preferences)  
- Order change requests (modifications, cancellations)  

### 5.1.7 Enterprise Understanding Produced  

- Unified, real‑time order book with current status of every order and line  
- Promise register: all active promises, their dates, types, and confidence scores  
- Backorder queue, prioritized for re‑promising  
- Customer‑order profiles summarizing ordering behavior and risk  

### 5.1.8 Enterprise Outputs  

| ID | Output | Description |
|----|--------|-------------|
| OUT‑PI‑001 | Order Book Snapshot | All orders with statuses, priorities, and promise details |
| OUT‑PI‑002 | Promise Register | Active promises with dates, types, and confidence |
| OUT‑PI‑003 | Backorder Queue | Backordered lines prioritized for re‑promising |
| OUT‑PI‑004 | Customer Order Profile | Aggregated order and promise history per customer |

### 5.1.9 Preconditions  

- Order ingestion channels are operational  
- Customer master data is maintained  
- Order identifiers are unique and consistent across channels  

### 5.1.10 Capability Dependencies  

None. This is the foundational promise capability.

### 5.1.11 Collaborating Capabilities  

- **Promise Orders** – consumes order book for ATP/CTP evaluation  
- **Evaluate Promise Quality** – consumes promise register for accuracy assessment  

### 5.1.12 Business Decisions  

---

#### DE‑PI‑010 — Accept Order Request  

**Purpose:** Validate an incoming order request for completeness, accuracy, and eligibility before entering the promise evaluation process.  

**Decision Alternatives:**  
- Accept (complete, eligible)  
- Reject (invalid, duplicate, blocked customer)  
- Return for clarification (incomplete data)  

**Decision Criteria:** Order has required fields (product, quantity, location, requested date), customer is active and not blocked, order is not a duplicate, order value is within authorized limits.  

**Decision Rationale:** *Explainability Template:* “Order ORD‑890 accepted: all required fields present, customer active, no duplicate detected. Rule BR‑PI‑010 passed.”  

---

##### Rules (for DE‑PI‑010)  

| ID | Rule | Category | Description |
|----|------|----------|-------------|
| BR‑PI‑010 | Order Validation Rule | Validation Rule | An order must contain product ID, quantity > 0, ship‑to location, and requested date. Missing fields → returned for clarification. |
| BR‑PI‑011 | Customer Eligibility Rule | Validation Rule | Customer must be active and not on credit hold. Blocked customers → auto‑rejected with reason. |
| BR‑PI‑012 | Duplicate Detection Rule | Validation Rule | An order with the same customer, product, quantity, and requested date within a 1‑hour window is flagged as potential duplicate and held for review. |

##### Policies (for DE‑PI‑010)  

| ID | Policy | Category | Description |
|----|--------|----------|-------------|
| PO‑PI‑010 | Order Acceptance Automation Policy | Automation Policy | Orders passing all validation rules are automatically accepted and queued for promise evaluation. |

---

#### DE‑PI‑011 — Update Order Status  

**Purpose:** Transition an order’s status based on the outcome of promise evaluation, fulfillment progress, or customer action.  

**Decision Alternatives:** Move to Promised, Partially Promised, Rejected, Fulfilled, Cancelled.  

**Decision Criteria:** Based on promise outcome for all lines (all promised → Promised; some → Partially Promised; none → Rejected).  

**Decision Rationale:** “Order ORD‑890 status set to Promised: all 3 lines have firm promise dates.”  

---

##### Rules (for DE‑PI‑011)  

| ID | Rule | Category | Description |
|----|------|----------|-------------|
| BR‑PI‑013 | Status Transition Rule | Consistency Rule | Status transitions follow allowed paths: Received → Under Evaluation → Promised/Partially Promised/Rejected. Promised → Fulfilled/Breached/Cancelled. |

##### Policies (for DE‑PI‑011)  

| ID | Policy | Category | Description |
|----|--------|----------|-------------|
| PO‑PI‑011 | Order Cancellation Policy | Authorization Policy | Customer‑requested cancellation is allowed only if order status is not yet Fulfilled. Internal cancellation requires Order Manager approval. |

---

### 5.1.13 Functional Behaviour  

1. **Ingest** order requests via APIs, EDI, portals.  
2. **Validate** each order via DE‑PI‑010 — rules BR‑PI‑010/011/012, policy PO‑PI‑010.  
3. **Assign** initial status and queue for promise evaluation.  
4. **Update** statuses as promises are made, fulfilled, or breached via DE‑PI‑011 — rule BR‑PI‑013, policy PO‑PI‑011.  
5. **Maintain** promise register, backorder queue, and customer profiles.  
6. **Raise events:** `OrderAccepted`, `OrderRejected`, `OrderStatusChanged`.  

### 5.1.14 Commands  

| Command | Purpose |
|---------|---------|
| `AcceptOrder` | Validate and accept an order request |
| `UpdateOrderStatus` | Transition order to a new status |
| `CancelOrder` | Cancel an order |

### 5.1.15 Events  

| Event | Payload Highlights |
|-------|-------------------|
| `OrderAccepted` | Order ID, customer, lines, requested date |
| `OrderRejected` | Order ID, reason |
| `OrderStatusChanged` | Order ID, old status, new status, timestamp |

### 5.1.16 Queries  

| Query | Description |
|-------|-------------|
| `GetOrder(orderId)` | Full order details |
| `GetOrderBook(filter)` | Active orders by status, priority, date range |
| `GetPromiseRegister(customerId)` | Active promises for a customer |

### 5.1.17 Reports  

- **Order Book Summary** – counts by status, priority, channel  
- **Promise Register Report** – active promises and confidence scores  

### 5.1.18 Dashboards  

- **Order Book Monitor** – real‑time order volumes by status  
- **Customer Promise View** – per‑customer promise status and history  

### 5.1.19 Software Realization  

```
API → Application Service (OrderAggregate)  
→ Domain Model (Order, OrderLine, Promise)  
→ Event Store → Projections (OrderBook, PromiseRegister) → Read Model  
```  
Ingestion adapters support EDI, REST, and portal channels. The read model is optimized for real‑time order book queries.

---

## 5.2 Promise Orders  

### 5.2.1 Purpose  

Evaluate incoming and backlogged order lines against available supply, capacity, allocations, and substitution options to generate firm, feasible promises. Answers: *“Can we fulfill this order? When? From where? At what confidence?”* The capability executes ATP, CTP, and allocation checks, sets promise dates, offers substitutes when primary fulfillment is not possible, and produces binding commitments.  

### 5.2.2 Business Objectives Served  

- BO‑PI‑001 Deliver Trusted Order Commitments  
- BO‑PI‑002 Maximize Customer Service Reliability  
- BO‑PI‑003 Optimize Order Promising Profitability  
- BO‑PI‑006 Increase Promising Automation  
- BO‑PI‑007 Ensure Commitment Feasibility  

### 5.2.3 Enterprise Measures  

- PI‑PI‑002 Order Fill Rate  
- PI‑PI‑003 On‑Time Delivery  
- PI‑PI‑004 Order Cycle Time  
- PI‑PI‑005 Promise Adherence  
- PI‑PI‑006 Order Rejection Rate  
- PI‑PI‑102 ATP Accuracy  
- PI‑PI‑103 CTP Accuracy  
- PI‑PI‑104 Decision Confidence Index (Promise)  

### 5.2.4 Semantic Objects  

| ID | Object | Role |
|----|--------|------|
| SE‑PI‑002 | Promise | Output |
| SE‑PI‑004 | Commitment | Binding record |
| SE‑PI‑020 | Promise Date | Output date |
| SE‑PI‑021 | Promise Type | ATP, CTP, Allocation, Substitution |
| SE‑PI‑022 | Promise Confidence | Confidence score |
| SE‑PI‑060 | ATP Check | Evaluation method |
| SE‑PI‑061 | CTP Check | Evaluation method |
| SE‑PI‑062 | ATP Check Result | Output of ATP |
| SE‑PI‑063 | CTP Check Result | Output of CTP |
| SE‑PI‑064 | Supply Search | Process |
| SE‑PI‑065 | Substitution Option | Alternative offer |
| SE‑PI‑066 | Substitution Rule | Governing rule |
| SE‑PI‑016 | Order Split | Partial shipment |
| SE‑PI‑032 | Allocation Consumption | Allocation tracking |

### 5.2.5 Primitive Capabilities Composed  

- **Understand** – interprets order requirements and supply availability  
- **Predict** – projects supply availability over time, estimates production completion  
- **Evaluate** – selects best fulfillment option among ATP, CTP, allocation, substitution  
- **Assess** – determines promise feasibility and confidence  

### 5.2.6 Enterprise Inputs  

- Order book and priorities (from Understand Orders)  
- Current inventory positions and open supply orders (from Supply Intelligence — Understand Supply)  
- Supply plan and capacity availability (from Supply Intelligence — Plan Supply, Manage Capacity)  
- Allocation pools and consumption status (from Manage Allocations)  
- Substitution rules (from master data)  
- Customer tier and communication preferences (from Understand Orders)  
- Planning calendars and lead times  

### 5.2.7 Enterprise Understanding Produced  

- For each order line: promise decision (Promised, Rejected, Substituted), promise date, promise type, supply source, confidence score  
- Aggregated promise schedule showing committed supply consumption  
- ATP/CTP evaluation logs with decision rationale  
- Substitution offers with justification  

### 5.2.8 Enterprise Outputs  

| ID | Output | Description |
|----|--------|-------------|
| OUT‑PI‑010 | Promise Decision | Promise status, date, type, source, confidence per line |
| OUT‑PI‑011 | Promise Commitment | Binding commitment record |
| OUT‑PI‑012 | Substitution Offer | Alternative product/location offer |
| OUT‑PI‑013 | ATP/CTP Evaluation Log | Detailed trace of supply search and decision |
| OUT‑PI‑014 | Supply Consumption Update | Consumed inventory/allocation to be transmitted to Supply Intelligence |

### 5.2.9 Preconditions  

- Order data is complete and validated (from Understand Orders)  
- Supply data is current and accessible (from Supply Intelligence)  
- Allocation rules and pools are defined (from Manage Allocations)  
- Substitution rules are configured  

### 5.2.10 Capability Dependencies  

- `CA‑PI‑001 Understand Orders` – for order book and priorities  
- `CA‑SI‑001 Understand Supply` – for inventory and open supply  
- `CA‑SI‑002 Plan Supply` – for planned supply and capacity  
- `CA‑PI‑003 Manage Allocations` – for allocation pools and rules  

### 5.2.11 Collaborating Capabilities  

- **Manage Allocations** – consumes supply consumption to update pools  
- **Explain Promise Decisions** – consumes promise decisions for explanation generation  
- **Evaluate Promise Quality** – consumes promise register for accuracy measurement  

### 5.2.12 Business Decisions  

---

#### DE‑PI‑020 — Evaluate ATP (Available‑to‑Promise)  

**Purpose:** Execute a real‑time ATP check across on‑hand inventory, inbound purchase orders, and planned production to determine the earliest date and quantity that can be promised without a new production order.  

**Required Understanding:** Requested product, quantity, date, location. Current inventory, inbound supply schedule, open promise commitments, allocation consumption.  

**Decision Alternatives:**  
- Full promise (requested quantity available by requested date)  
- Partial promise (some quantity available by requested date, remainder later)  
- Deferred promise (all quantity available but after requested date)  
- No ATP (no supply found within search horizon)  

**Decision Criteria:** Net available supply = on‑hand + inbound − already promised − allocated reserve. ATP date = earliest date net supply ≥ requested quantity.  

**Decision Confidence:** Based on supply data freshness, supplier reliability (for inbound), and demand variability.  

**Decision Rationale:** *Explainability Template:* “Order line L123 promised 100 units on 05‑Mar via ATP. Source: 50 from on‑hand inventory at DC‑A, 50 from inbound PO‑890 expected 03‑Mar. ATP confidence 95%. Rule BR‑PI‑020 applied.”  

---

##### Rules (for DE‑PI‑020)  

| ID | Rule | Category | Description |
|----|------|----------|-------------|
| BR‑PI‑020 | ATP Calculation Rule | Derivation Rule | ATP = On‑Hand + Inbound Supply (confirmed) − Already Promised − Allocation Reserve. Supply is searched in priority order: on‑hand at requested location, on‑hand at alternate locations, inbound PO by expected date, planned production. |
| BR‑PI‑021 | ATP Horizon Rule | Validation Rule | ATP search is limited to a configurable horizon (default 12 weeks). Beyond the horizon, orders are routed to CTP or rejected. |
| BR‑PI‑022 | ATP Confidence Rule | Derivation Rule | ATP confidence = weighted average of source reliability: on‑hand (100%), inbound from supplier with OTD ≥ 95% (95%), inbound from supplier with OTD < 95% (80%), planned production (90%). |
| BR‑PI‑020‑R1 | Temporary Reservation Requirement Rule | Validation Rule | During ATP evaluation, the system must check that the identified supply source(s) can be temporarily reserved. If a source cannot be reserved (already reserved by a concurrent evaluation), the ATP check must re‑compute excluding that source and attempt to satisfy the order from alternative available supply. |

##### Policies (for DE‑PI‑020)  

| ID | Policy | Category | Description |
|----|--------|----------|-------------|
| PO‑PI‑020 | ATP Auto‑Promise Policy | Automation Policy | If ATP confidence ≥ 95% and promise date ≤ requested date + 1 day, the order is automatically promised without manual review. |
| PO‑PI‑021 | ATP Partial Promise Policy | Authorization Policy | Partial promises (split shipments) require customer consent per the customer communication preference. Consent is sought automatically if preference allows. |

---

#### DE‑PI‑021 — Evaluate CTP (Capable‑to‑Promise)  

**Purpose:** When ATP is insufficient, execute a CTP check to determine if production capacity and materials can be made available to fulfill the order within an acceptable lead time.  

**Required Understanding:** Product BOM and routing, available capacity, material availability, production lead time, current production schedule, and backlog.  

**Decision Alternatives:**  
- CTP feasible (capacity and materials available, promise date = production completion + delivery)  
- CTP deferred (feasible but later than acceptable horizon)  
- CTP infeasible (cannot promise — route to substitution or rejection)  

**Decision Criteria:** Earliest production start date considering material availability and capacity. Production duration based on routing and lot size. Promise date = production completion + distribution lead time.  

**Decision Confidence:** Based on capacity plan stability, material availability confidence, and production schedule adherence history.  

**Decision Rationale:** “Order line L456 promised via CTP: production at Plant‑A starting 10‑Mar, completion 12‑Mar, delivery 14‑Mar. Materials confirmed, capacity available. CTP confidence 85%.”  

---

##### Rules (for DE‑PI‑021)  

| ID | Rule | Category | Description |
|----|------|----------|-------------|
| BR‑PI‑023 | CTP Feasibility Rule | Derivation Rule | CTP is feasible if: (1) all required materials are available or on‑order with confirmed delivery before production start; (2) capacity is available within the production horizon; (3) promise date ≤ requested date + configurable CTP acceptance window (default 2 weeks). |
| BR‑PI‑024 | CTP Confidence Rule | Derivation Rule | CTP confidence = weighted average: capacity availability confidence × material availability confidence × production adherence history. |

##### Policies (for DE‑PI‑021)  

| ID | Policy | Category | Description |
|----|--------|----------|-------------|
| PO‑PI‑022 | CTP Approval Policy | Authorization Policy | CTP promises with confidence < 85% require Promise Manager approval. CTP promises extending beyond the CTP acceptance window require customer communication before confirmation. |

---

#### DE‑PI‑022 — Determine Substitution Option  

**Purpose:** When both ATP and CTP cannot fulfill the order as requested, evaluate whether an acceptable substitute product, location, or grade can be offered to the customer.  

**Required Understanding:** Original order details, substitution rules, substitute product availability, customer tier and substitution consent preferences.  

**Decision Alternatives:**  
- Offer product substitution (alternative SKU)  
- Offer location substitution (ship from alternate warehouse)  
- Offer grade substitution (higher/lower grade)  
- No substitution possible (reject order)  

**Decision Criteria:** Substitution rule allows, substitute is available (ATP), price/margin impact within acceptable limits, customer consent obtained if required.  

**Decision Confidence:** Based on substitute supply reliability and customer acceptance likelihood.  

**Decision Rationale:** “Order line L789 cannot be fulfilled as requested. Substitution offered: Product SKU‑B (compatible, in stock) at same price. Customer consent required per policy. Rule BR‑PI‑025 applied.”  

---

##### Rules (for DE‑PI‑022)  

| ID | Rule | Category | Description |
|----|------|----------|-------------|
| BR‑PI‑025 | Substitution Eligibility Rule | Derivation Rule | A substitute is eligible if: (1) substitution rule exists between requested and substitute product; (2) substitute is available (ATP) by the requested date; (3) price difference is within ±10% or customer tier allows. |
| BR‑PI‑026 | Substitution Consent Rule | Validation Rule | If the customer’s communication preference requires consent for substitution, the offer must be sent and acknowledged before the promise is confirmed. |

##### Policies (for DE‑PI‑022)  

| ID | Policy | Category | Description |
|----|--------|----------|-------------|
| PO‑PI‑023 | Auto‑Substitution Policy | Automation Policy | For customers who have opted into automatic substitution (preference), and the substitute is within price tolerance, the substitution is applied automatically. |

---

#### DE‑PI‑023 — Confirm Promise and Create Commitment  

**Purpose:** Finalize the promise decision, create a binding commitment, update supply consumption, and communicate the promise to the customer.  

**Required Understanding:** Promise decision from DE‑PI‑020, 021, or 022.  

**Decision Alternatives:**  
- Confirm promise and create firm commitment  
- Confirm tentative commitment (if confidence < threshold)  
- Hold for manual review  

**Decision Criteria:** Promise confidence ≥ automation threshold (default 90%), all rules satisfied.  

**Decision Rationale:** “Promise for Order ORD‑890 confirmed: firm commitment created, supply consumption updated, customer notified via email. Policy PO‑PI‑024 applied.”  

---

##### Rules (for DE‑PI‑023)  

| ID | Rule | Category | Description |
|----|------|----------|-------------|
| BR‑PI‑027 | Commitment Creation Rule | Derivation Rule | A commitment is created with Commitment Level = Firm if confidence ≥ 90%, otherwise Tentative. The commitment links to the specific supply source. |
| BR‑PI‑028 | Supply Consumption Rule | Validation Rule | Upon commitment, the temporary reservation is confirmed, permanently consuming the supply. If the temporary reservation has expired (evaluation exceeded the reservation window), the confirmation is rejected and the promise must be re‑evaluated. |

##### Policies (for DE‑PI‑023)  

| ID | Policy | Category | Description |
|----|--------|----------|-------------|
| PO‑PI‑024 | Promise Confirmation Automation Policy | Automation Policy | Promises with confidence ≥ 90% and no policy violations are confirmed automatically. Below 90%, Promise Manager approval is required. |
| PO‑PI‑025 | Customer Communication Policy | Compliance Policy | Promise confirmations, substitutions, and rejections must be communicated to the customer within 15 minutes of decision, using their preferred channel and template. |

---

#### DE‑PI‑024 — Simulate Promise (What‑If)  

**Purpose:** For strategic orders, contract negotiations, or large tenders, simulate the promise outcome without creating a binding commitment. Allows the enterprise and customer to explore fulfillment scenarios.  

**Required Understanding:** Simulated order parameters, current and projected supply.  

**Decision Alternatives:** Simulated promise date and confidence. No binding commitment created.  

**Decision Rationale:** “What‑if simulation for 10,000 units of Product P: earliest promise date 20‑Mar via CTP, confidence 78%. No supply consumed.”  

---

##### Rules (for DE‑PI‑024)  

| ID | Rule | Category | Description |
|----|------|----------|-------------|
| BR‑PI‑029 | Simulation Isolation Rule | Validation Rule | Simulations must not consume real supply or affect the order book. Simulated promises are not visible to operational promising. |

##### Policies (for DE‑PI‑024)  

| ID | Policy | Category | Description |
|----|--------|----------|-------------|
| PO‑PI‑026 | Simulation Access Policy | Authorization Policy | Promise simulation is available to Sales Managers and Supply Chain Managers for orders above a configurable value threshold. |

---

### 5.2.13 Functional Behaviour  

1. **Trigger:** On order acceptance (real‑time), on backorder re‑promise (batch), on what‑if request (on‑demand).  
2. **Retrieve** order details, supply picture, allocations, substitution rules, customer preferences.  
3. **Execute DE‑PI‑020** (Evaluate ATP) — rules BR‑PI‑020/021/022, policies PO‑PI‑020/021.  
4. **If ATP insufficient** and order is eligible, execute DE‑PI‑021 (Evaluate CTP) — rules BR‑PI‑023/024, policy PO‑PI‑022.  
5. **If CTP infeasible** and substitution is allowed, execute DE‑PI‑022 (Determine Substitution) — rules BR‑PI‑025/026, policy PO‑PI‑023.  
6. **For successful promises,** create a temporary reservation against the identified supply source(s). The reservation has a configurable expiry (default 30 seconds). If the reservation cannot be placed (source already reserved by a concurrent evaluation), re‑evaluate from step 3 with updated supply data.  
7. **Execute DE‑PI‑023** (Confirm Promise) to convert the temporary reservation into a firm commitment — rules BR‑PI‑027/028, policies PO‑PI‑024/025.  
8. **If the promise is rejected**, release any temporary reservation immediately.  
9. **For what‑if requests**, execute DE‑PI‑024 (Simulate Promise) — rule BR‑PI‑029, policy PO‑PI‑026. No reservation is created for simulations.  
10. **Update** supply consumption (inventory reserve, allocation pool) upon commitment.  
11. **Communicate** promise to customer via preferred channel.  
12. **Raise events:** `ATPResultCalculated`, `CTPResultCalculated`, `SubstitutionOffered`, `TemporaryReservationCreated`, `PromiseConfirmed`, `PromiseRejected`, `SupplyConsumed`.


### 5.2.14 Commands  

| Command | Purpose |
|---------|---------|
| `EvaluatePromise` | Run ATP/CTP evaluation for an order line |
| `ConfirmPromise` | Finalize promise and create commitment |
| `SimulatePromise` | Run what‑if promise simulation |
| `RePromiseBacklog` | Re‑evaluate backordered lines against new supply |

### 5.2.15 Events  

| Event | Payload Highlights |
|-------|-------------------|
| `ATPResultCalculated` | Order line ID, ATP quantity, earliest date, source, confidence |
| `CTPResultCalculated` | Order line ID, CTP feasible, promise date, capacity used |
| `SubstitutionOffered` | Order line ID, substitute product/location, reason |
| `PromiseConfirmed` | Order line ID, promise date, type, commitment ID |
| `PromiseRejected` | Order line ID, reason |
| `SupplyConsumed` | Supply source, quantity reserved, commitment reference |

### 5.2.16 Queries  

| Query | Description |
|-------|-------------|
| `GetPromiseStatus(orderId)` | Current promise status for an order |
| `GetATPResult(orderLineId)` | Detailed ATP calculation trace |
| `GetCommitment(commitmentId)` | Commitment details |
| `SimulatePromise(params)` | What‑if promise simulation |

### 5.2.17 Reports  

- **Promise Decision Report** – promise rate, rejection rate, substitution rate  
- **ATP Accuracy Report** – ATP promised vs. actual fulfillment  

### 5.2.18 Dashboards  

- **Promise Control Tower** – real‑time promise decisions, confidence distribution, active commitments  
- **ATP/CTP Performance Dashboard** – accuracy, cycle time, automation rate  

### 5.2.19 Software Realization  

```
API → Application Service (PromiseEvaluation, Commitment)  
→ Domain Model (ATPEngine, CTPEngine, SubstitutionEngine)  
→ Integration Layer (Supply Intelligence APIs for inventory, capacity)  
→ Event Store → Projections (PromiseRegister, CommitmentBook) → Read Model  
```  
The ATP engine is optimized for sub‑second response. CTP queries are asynchronous for complex checks. Substitution logic uses a configurable rule matrix. All decisions are logged for explainability.  

---

## 5.3 Manage Allocations  

### 5.3.1 Purpose  

Define, maintain, and enforce allocation rules that pre‑reserve constrained supply for priority channels, customers, and order types. Answers: *“How should limited supply be reserved and distributed to best serve strategic goals?”* The capability translates strategic allocation decisions into operational constraints that guide ATP evaluation, ensuring that the most important demand gets preferential access to scarce supply.  

### 5.3.2 Business Objectives Served  

- BO‑PI‑003 Optimize Order Promising Profitability  
- BO‑PI‑006 Increase Promising Automation  
- BO‑PI‑007 Ensure Commitment Feasibility  

### 5.3.3 Enterprise Measures  

- PI‑PI‑008 Allocation Compliance  
- PI‑PI‑108 Allocation Optimization Effectiveness  

### 5.3.4 Semantic Objects  

| ID | Object | Role |
|----|--------|------|
| SE‑PI‑003 | Allocation | Core concept |
| SE‑PI‑030 | Allocation Rule | Defining logic |
| SE‑PI‑031 | Allocation Pool | Reserved supply |
| SE‑PI‑032 | Allocation Consumption | Consumption tracking |
| SE‑PI‑033 | Allocation Period | Time bucket |
| SE‑PI‑034 | Allocation Exhaustion | State |

> *Note:* Allocation pools represent strategic, long‑lived reservations. When a promise is made against a pool, a temporary reservation is first created (see Promise Orders, Section 5.2), then confirmed, consuming from the pool. This prevents pools from being overdrawn by concurrent promises.

### 5.3.5 Primitive Capabilities Composed  

- **Understand** – interprets supply constraints and demand priorities  
- **Evaluate** – determines allocation quantities and rules  
- **Predict** – projects consumption rates  
- **Learn** – improves allocation effectiveness over time  

### 5.3.6 Enterprise Inputs  

- Supply plan and projected scarcity (from Supply Intelligence — Plan Supply)  
- Demand forecast and order history (from Demand Intelligence)  
- Customer tiers and strategic priorities (from Understand Orders)  
- Current allocation rules and pools  
- Business calendar  

### 5.3.7 Enterprise Understanding Produced  

- Defined allocation rules with eligibility criteria, quantities, and time windows  
- Allocation pools: reserved quantities by product, channel, and period  
- Consumption projections indicating when pools will exhaust  
- Allocation compliance summary: actual vs. planned allocation usage  

### 5.3.8 Enterprise Outputs  

| ID | Output | Description |
|----|--------|-------------|
| OUT‑PI‑020 | Allocation Rule Set | Active rules with criteria and quantities |
| OUT‑PI‑021 | Allocation Pool Status | Current reserved quantities and consumption |
| OUT‑PI‑022 | Allocation Consumption Forecast | Projected exhaustion dates |
| OUT‑PI‑023 | Allocation Compliance Report | Adherence to allocation rules |

### 5.3.9 Preconditions  

- Supply plan identifies constrained items  
- Customer tiers and channel priorities are defined  
- Historical order patterns are available  

### 5.3.10 Capability Dependencies  

- `CA‑SI‑002 Plan Supply` – for supply constraints  
- `CA‑DI‑002 Forecast Demand` – for demand forecasts  
- `CA‑PI‑001 Understand Orders` – for customer tiers  

### 5.3.11 Collaborating Capabilities  

- **Promise Orders** – consumes allocations to guide ATP and substitution decisions  
- **Evaluate Promise Quality** – consumes allocation compliance data  

### 5.3.12 Business Decisions  

---

#### DE‑PI‑030 — Define Allocation Rule  

**Purpose:** Create or update an allocation rule that reserves a portion of constrained supply for a defined channel, customer group, or order type over a time horizon.  

**Required Understanding:** Supply constraint (item, quantity, period), demand patterns, business priorities, margin data.  

**Decision Alternatives:**  
- Create firm allocation (guaranteed reserve)  
- Create soft allocation (guideline, overridable)  
- No allocation (use first‑come‑first‑served)  
- Revise existing allocation  

**Decision Criteria:** Based on strategic priority score (customer tier × margin × volume), fairness constraints, and regulatory requirements. Allocations must not exceed available constrained supply.  

**Decision Rationale:** “Allocation created for Product X, Gold customers: 30% of constrained supply reserved for Q3. Rule BR‑PI‑030 applied.”  

---

##### Rules (for DE‑PI‑030)  

| ID | Rule | Category | Description |
|----|------|----------|-------------|
| BR‑PI‑030 | Allocation Quantity Rule | Constraint Rule | Total allocated quantity across all rules for a constrained item must not exceed 100% of available supply for the period. |
| BR‑PI‑031 | Allocation Priority Rule | Derivation Rule | Allocation is prioritized by strategic score: (Customer Tier weight × 0.4) + (Margin contribution × 0.35) + (Volume commitment × 0.25). Higher scores receive allocation first. |
| BR‑PI‑032 | Allocation Review Rule | Consistency Rule | Allocation rules are reviewed quarterly. Rules not adjusted for 2 quarters are flagged for sunset review. |

##### Policies (for DE‑PI‑030)  

| ID | Policy | Category | Description |
|----|--------|----------|-------------|
| PO‑PI‑030 | Allocation Approval Policy | Authorization Policy | Allocations exceeding 20% of constrained supply for a single channel require Supply Chain Director approval. |

---

#### DE‑PI‑031 — Monitor Allocation Consumption  

**Purpose:** Track consumption of allocation pools as promises are made, and trigger actions when pools approach or reach exhaustion.  

**Required Understanding:** Allocation pool status, recent consumption rate, remaining quantity, upcoming demand.  

**Decision Alternatives:**  
- Normal (consumption on track)  
- Warning (pool >80% consumed, earlier than expected)  
- Exhausted (pool fully consumed)  
- Rebalance (shift unconsumed quantity between pools)  

**Decision Rationale:** “Allocation pool for Product X, Gold customers 85% consumed with 4 weeks remaining. Warning raised. Rule BR‑PI‑033 triggered.”  

---

##### Rules (for DE‑PI‑031)  

| ID | Rule | Category | Description |
|----|------|----------|-------------|
| BR‑PI‑033 | Consumption Warning Rule | Derivation Rule | Warning raised when consumption rate projects exhaustion >2 weeks before period end or when pool exceeds 90% consumed. |
| BR‑PI‑034 | Auto‑Rebalance Rule | Derivation Rule | If one pool is exhausted and another has unconsumed quantity >10% with 1 week remaining, auto‑rebalance up to 50% of the remaining quantity. |

##### Policies (for DE‑PI‑031)  

| ID | Policy | Category | Description |
|----|--------|----------|-------------|
| PO‑PI‑031 | Exhaustion Escalation Policy | Exception Policy | Pool exhaustion triggers immediate notification to Supply Chain Manager and affected channel managers. |

---

### 5.3.13 Functional Behaviour  

1. **Scheduled:** Weekly allocation review, daily consumption monitoring.  
2. **Trigger:** On supply constraint identification (from Plan Supply).  
3. **Execute DE‑PI‑030** (Define Allocation Rule) — rules BR‑PI‑030/031/032, policy PO‑PI‑030.  
4. **Execute DE‑PI‑031** (Monitor Allocation Consumption) continuously — rules BR‑PI‑033/034, policy PO‑PI‑031.  
5. **Update** allocation pool status and supply consumption picture.  
6. **Raise events:** `AllocationRuleDefined`, `AllocationPoolUpdated`, `AllocationConsumed`, `AllocationExhausted`.  

### 5.3.14 Commands  

| Command | Purpose |
|---------|---------|
| `DefineAllocation` | Create or update an allocation rule |
| `MonitorConsumption` | Trigger consumption analysis |
| `RebalanceAllocation` | Manually rebalance pools |

### 5.3.15 Events  

| Event | Payload Highlights |
|-------|-------------------|
| `AllocationRuleDefined` | Rule ID, item, channel, quantity, period |
| `AllocationPoolUpdated` | Pool ID, remaining quantity, status |
| `AllocationConsumed` | Promise reference, pool, quantity consumed |
| `AllocationExhausted` | Pool ID, exhaustion time |

### 5.3.16 Queries  

| Query | Description |
|-------|-------------|
| `GetAllocationRules(filter)` | Active allocation rules |
| `GetAllocationPoolStatus(poolId)` | Current pool status and consumption |

### 5.3.17 Reports  

- **Allocation Compliance Report** – adherence to rules  
- **Allocation Pool Health Report** – consumption trends, exhaustion forecasts  

### 5.3.18 Dashboards  

- **Allocation Monitor** – real‑time pool status, consumption gauges  

### 5.3.19 Software Realization  

```
API → Application Service → Domain Model (AllocationRule, AllocationPool)  
→ Rule Engine (priority scoring, auto‑rebalance)  
→ Event Store → Projections (AllocationStatus) → Read Model  
```  

---

## 5.4 Prioritize Orders  

### 5.4.1 Purpose  

Assign a business‑based priority ranking to every order and backorder line to determine the sequence of promise evaluation when supply is constrained. Answers: *“Which orders should get supply first?”* Priority is a composite of customer tier, margin, order urgency, and strategic value.  

### 5.4.2 Business Objectives Served  

- BO‑PI‑003 Optimize Order Promising Profitability  
- BO‑PI‑002 Maximize Customer Service Reliability  

### 5.4.3 Enterprise Measures  

- PI‑PI‑107 Order Prioritization Effectiveness  

### 5.4.4 Semantic Objects  

| ID | Object | Role |
|----|--------|------|
| SE‑PI‑014 | Order Priority | Output |
| SE‑PI‑051 | Customer Tier | Input |
| SE‑PI‑013 | Order Type | Input |
| SE‑PI‑015 | Backorder Line | Prioritized for re‑promising |

### 5.4.5 Primitive Capabilities Composed  

- **Understand** – consolidates order attributes  
- **Assess** – scores and ranks orders  

### 5.4.6 Enterprise Inputs  

- Order book with customer, product, channel, margin data  
- Customer tier and strategic flags  
- Order type (standard, rush, contract) and requested dates  
- Backlog age  

### 5.4.7 Enterprise Understanding Produced  

- Priority score (0–100) and level (Critical, High, Medium, Low) for each order and backorder line  
- Prioritized order queue for ATP/CTP evaluation  

### 5.4.8 Enterprise Outputs  

| ID | Output | Description |
|----|--------|-------------|
| OUT‑PI‑030 | Prioritized Order Queue | Orders ranked by priority score |

### 5.4.9 Preconditions  

- Customer tier and margin data are available  
- Order book is current  

### 5.4.10 Capability Dependencies  

- `CA‑PI‑001 Understand Orders` – for order data  

### 5.4.11 Collaborating Capabilities  

- **Promise Orders** – consumes priority queue for sequencing  

### 5.4.12 Business Decisions  

---

#### DE‑PI‑040 — Compute Order Priority  

**Purpose:** Calculate a numerical priority score for each order or backorder line.  

**Decision Criteria:** Weighted sum of normalized factors: Customer Tier weight (40%), Margin contribution (35%), Order Age/Urgency (15%), Strategic flag (10%).  

**Decision Rationale:** “Order ORD‑890 scored 87 (High): Platinum customer (40 pts), margin 25% (30 pts), rush order (10 pts), strategic product (7 pts).”  

---

##### Rules (for DE‑PI‑040)  

| ID | Rule | Category | Description |
|----|------|----------|-------------|
| BR‑PI‑040 | Priority Scoring Rule | Calculation Rule | Priority Score = Σ (Weight × Normalized Factor). Weights configurable. Levels: ≥85 Critical, 70–84 High, 50–69 Medium, <50 Low. |
| BR‑PI‑041 | Backorder Aging Rule | Derivation Rule | Backorder lines gain +5 priority points for every week in backlog beyond the first week. |

##### Policies (for DE‑PI‑040)  

| ID | Policy | Category | Description |
|----|--------|----------|-------------|
| PO‑PI‑040 | Priority Override Policy | Authorization Policy | Only a Promise Manager may manually override priority with documented justification. |

---

### 5.4.13 Functional Behaviour  

1. **Trigger:** On new order acceptance, on backorder queue update, daily re‑ranking.  
2. **Retrieve** order data and customer profiles.  
3. **Execute DE‑PI‑040** for each order/backorder — rule BR‑PI‑040/041, policy PO‑PI‑040.  
4. **Publish** prioritized queue.  
5. **Raise events:** `OrderPriorityAssigned`, `PrioritizedQueueUpdated`.  

### 5.4.14 Commands  

| Command | Purpose |
|---------|---------|
| `PrioritizeOrders` | Run prioritization for the order book |

### 5.4.15 Events  

| Event | Payload Highlights |
|-------|-------------------|
| `OrderPriorityAssigned` | Order ID, score, level |
| `PrioritizedQueueUpdated` | Queue version, timestamp |

### 5.4.16 Queries  

| Query | Description |
|-------|-------------|
| `GetPrioritizedQueue(filter)` | Orders ranked by priority |

### 5.4.17 Reports  

- **Order Priority Distribution Report**  

### 5.4.18 Dashboards  

- **Order Priority Dashboard**  

### 5.4.19 Software Realization  

```
API → Application Service → Domain Model (OrderPriority)  
→ Scoring Engine → Event Store → Projections → Read Model  
```  

---

## 5.5 Manage Order Changes  

### 5.5.1 Purpose  

Handle customer‑requested modifications to existing orders—quantity changes, date changes, cancellations, and splits—while preserving promise feasibility and updating commitments. Answers: *“Can this order change be accommodated, and what is the impact on the promise?”*  

### 5.5.2 Business Objectives Served  

- BO‑PI‑002 Maximize Customer Service Reliability  
- BO‑PI‑005 Improve Order Visibility and Transparency  

### 5.5.3 Enterprise Measures  

- PI‑PI‑012 Order Change Cycle Time  

### 5.5.4 Semantic Objects  

| ID | Object | Role |
|----|--------|------|
| SE‑PI‑011 | Order Request | Modified request |
| SE‑PI‑016 | Order Split | Output |
| SE‑PI‑024 | Promise Revision | Output |

### 5.5.5 Primitive Capabilities Composed  

- **Understand** – interprets change request  
- **Assess** – evaluates impact on supply and promise  
- **Evaluate** – determines best course  

### 5.5.6 Enterprise Inputs  

- Existing order and promise details  
- Change request (quantity, date, cancellation)  
- Current supply and allocation status  

### 5.5.7 Enterprise Understanding Produced  

- Feasibility of change, impact on promise date/quantity, revised commitment  

### 5.5.8 Enterprise Outputs  

| ID | Output | Description |
|----|--------|-------------|
| OUT‑PI‑040 | Order Change Decision | Approved/Rejected, revised promise |

### 5.5.9 Preconditions  

- Original order exists and is modifiable (not shipped)  

### 5.5.10 Capability Dependencies  

- `CA‑PI‑001 Understand Orders` – for current order state  
- `CA‑PI‑002 Promise Orders` – for re‑evaluation of supply  

### 5.5.11 Collaborating Capabilities  

- **Promise Orders** – used to re‑promise changed orders  

### 5.5.12 Business Decisions  

---

#### DE‑PI‑050 — Evaluate Order Change  

**Purpose:** Determine if the requested change can be accommodated.  

**Decision Alternatives:** Approve change and re‑promise, Approve with revised promise, Reject (supply not available), Partial approval.  

**Decision Criteria:** Supply availability after change, impact on other orders, change cost.  

**Decision Rationale:** “Order ORD‑890 quantity increased from 100 to 150. ATP confirms 150 available by 06‑Mar. Change approved. Rule BR‑PI‑050 applied.”  

---

##### Rules (for DE‑PI‑050)  

| ID | Rule | Category | Description |
|----|------|----------|-------------|
| BR‑PI‑050 | Change Feasibility Rule | Validation Rule | Change is feasible if the new quantity/date can be satisfied via ATP/CTP within the original acceptance window. |
| BR‑PI‑051 | Cancellation Window Rule | Validation Rule | Orders can be cancelled without penalty only if they are not yet shipped and not within the frozen period (default 24 hours before shipment). |

##### Policies (for DE‑PI‑050)  

| ID | Policy | Category | Description |
|----|--------|----------|-------------|
| PO‑PI‑050 | Change Authorization Policy | Authorization Policy | Changes that cause a promise date delay >3 days require customer acknowledgement. |

---

### 5.5.13 Functional Behaviour  

1. **Trigger:** On customer change request.  
2. **Validate** change against feasibility rules.  
3. **Execute DE‑PI‑050** — rules BR‑PI‑050/051, policy PO‑PI‑050.  
4. **If approved**, trigger re‑promise via Promise Orders.  
5. **Update** order and commitment records.  
6. **Communicate** new promise to customer.  
7. **Raise events:** `OrderChangeRequested`, `OrderChangeApproved`, `OrderChangeRejected`.  

### 5.5.14 Commands  

| Command | Purpose |
|---------|---------|
| `RequestOrderChange` | Submit change request |
| `EvaluateOrderChange` | Run feasibility check |

### 5.5.15 Events  

| Event | Payload Highlights |
|-------|-------------------|
| `OrderChangeRequested` | Order ID, change details |
| `OrderChangeApproved` | Order ID, revised promise |
| `OrderChangeRejected` | Order ID, reason |

### 5.5.16 Queries  

| Query | Description |
|-------|-------------|
| `GetOrderChangeHistory(orderId)` | Change log |

### 5.5.17 Reports  

- **Order Change Analysis Report** – frequency, approval rate, cycle time  

### 5.5.18 Dashboards  

- **Order Change Monitor** – pending changes, approval status  

### 5.5.19 Software Realization  

```
API → Application Service → Domain Model (OrderChange)  
→ Integration with Promise Orders for re‑evaluation  
→ Event Store → Read Model
```  

---

## 5.6 Collaborate with Customers  

### 5.6.1 Purpose  

Enable transparent, proactive, and efficient communication with customers throughout the promise lifecycle. Answers: *“How do we keep customers informed, and how do we incorporate their preferences and feedback into the promising process?”* The capability manages communication preferences, delivers promise confirmations and status updates, handles customer inquiries, shares available‑to‑promise options, and captures customer consent for substitutions and partial deliveries. It treats customers as active participants in the promising process, not passive recipients.  

### 5.6.2 Business Objectives Served  

- BO‑PI‑005 Improve Order Visibility and Transparency  
- BO‑PI‑002 Maximize Customer Service Reliability  
- BO‑PI‑006 Increase Promising Automation  

### 5.6.3 Enterprise Measures  

- PI‑PI‑011 Customer Communication Accuracy  
- PI‑PI‑110 Customer Collaboration Index  
- PI‑PI‑005 Promise Adherence (indirectly, via better communication)  

### 5.6.4 Semantic Objects  

| ID | Object | Role |
|----|--------|------|
| SE‑PI‑052 | Communication Preference | Customer preferences |
| SE‑PI‑053 | Customer Communication Template | Standardized messages |
| SE‑PI‑050 | Customer Order Profile | Customer context |
| SE‑PI‑051 | Customer Tier | Priority tier |
| SE‑PI‑002 | Promise | Subject of communication |
| SE‑PI‑025 | Promise Breach | Breach notification |
| SE‑PI‑065 | Substitution Option | Substitution offer |

### 5.6.5 Primitive Capabilities Composed  

- **Observe** – receives customer inquiries and consent responses  
- **Understand** – interprets customer preferences and communication history  
- **Assess** – evaluates communication effectiveness  

### 5.6.6 Enterprise Inputs  

- Customer communication preferences (channel, frequency, language)  
- Customer order profile and tier  
- Promise events: confirmed, revised, breached, fulfilled  
- Substitution offers requiring customer consent  
- Customer inquiries and responses  

### 5.6.7 Enterprise Understanding Produced  

- Communication log: all promise‑related communications with timestamps, channels, and acknowledgements  
- Customer sentiment indicators (response rate, complaint frequency, consent rate)  
- Communication effectiveness metrics (accuracy, timeliness, customer feedback)  

### 5.6.8 Enterprise Outputs  

| ID | Output | Description |
|----|--------|-------------|
| OUT‑PI‑050 | Promise Confirmation Message | Standardized promise confirmation sent to customer |
| OUT‑PI‑051 | Status Update | Promise revision or breach notification |
| OUT‑PI‑052 | Substitution Consent Request | Substitution offer with customer decision required |
| OUT‑PI‑053 | Communication Log | Record of all customer communications |
| OUT‑PI‑054 | Customer Collaboration Score | Composite of communication effectiveness |

### 5.6.9 Preconditions  

- Customer communication preferences are recorded and maintained  
- Communication templates are defined and approved  
- Promise events are published by upstream capabilities  

### 5.6.10 Capability Dependencies  

- `CA‑PI‑001 Understand Orders` – for customer profiles and preferences  
- `CA‑PI‑002 Promise Orders` – for promise decisions to communicate  
- `CA‑PI‑005 Manage Order Changes` – for change confirmations  

### 5.6.11 Collaborating Capabilities  

- **Promise Orders** – consumes consent decisions for substitution and partial delivery  
- **Detect Promise Exceptions** – receives breach notifications for customer communication  
- **Evaluate Promise Quality** – receives communication effectiveness data  

### 5.6.12 Business Decisions  

---

#### DE‑PI‑060 — Determine Communication Channel and Content  

**Purpose:** For each promise‑related event, select the appropriate communication channel (email, portal, EDI, SMS), template, and content based on customer preference and event type.  

**Required Understanding:** Customer communication preference, event type (confirmation, revision, breach, substitution offer), customer tier, and language preference.  

**Decision Alternatives:**  
- Send via preferred channel using standard template  
- Send via fallback channel if primary channel unavailable  
- Escalate for manual communication (high‑tier customer, breach event)  
- Batch with other communications (daily summary for low‑tier, non‑urgent)  

**Decision Criteria:** Match to preference, urgency of event (breach → immediate; routine status → batch), customer tier (Platinum/Gold → personalized; Bronze → automated).  

**Decision Confidence:** High if preferences are current and complete.  

**Decision Rationale:** *Explainability Template:* “Promise confirmation for Order ORD‑890 sent via email (customer preference). Template used: Standard Order Confirmation v2.1. Event: Promise Confirmed.”  

---

##### Rules (for DE‑PI‑060)  

| ID | Rule | Category | Description |
|----|------|----------|-------------|
| BR‑PI‑060 | Channel Selection Rule | Derivation Rule | Communication channel is selected per customer preference. If primary channel fails, fallback is attempted. Platinum/Gold customers receive immediate personalized communication for any promise change. |
| BR‑PI‑061 | Urgency‑Based Communication Rule | Derivation Rule | Breach notifications are always immediate regardless of customer preference. Routine status updates for Bronze customers may be batched into a daily digest. |
| BR‑PI‑062 | Template Matching Rule | Derivation Rule | Communication template is selected based on event type and customer language. Template must include order reference, promise date, quantity, and reason for any change. |

##### Policies (for DE‑PI‑060)  

| ID | Policy | Category | Description |
|----|--------|----------|-------------|
| PO‑PI‑060 | Communication Timing Policy | Compliance Policy | Promise confirmations must be sent within 15 minutes of decision. Breach notifications within 30 minutes of breach detection. Batch summaries by 08:00 daily. |
| PO‑PI‑061 | Manual Escalation Policy | Authorization Policy | Breach events for Platinum customers, or any event involving a value >$100,000, require manual review before automated communication. |

---

#### DE‑PI‑061 — Obtain Customer Consent (Substitution / Partial Delivery)  

**Purpose:** When a promise requires customer consent (substitution offer, partial delivery, delayed promise beyond acceptable window), request and capture that consent before finalizing the commitment.  

**Required Understanding:** Substitution offer or partial delivery details, customer consent preference, response deadline.  

**Decision Alternatives:**  
- Consent requested (awaiting response)  
- Consent auto‑approved (customer pre‑authorized)  
- Consent declined by customer  
- Consent timeout (no response within deadline, default per policy)  

**Decision Criteria:** If customer has pre‑authorized substitution within defined parameters, auto‑approve. Otherwise, send request and wait for response up to the consent deadline.  

**Decision Confidence:** Based on consent status.  

**Decision Rationale:** “Substitution consent auto‑approved for Order ORD‑890: customer pre‑authorized product substitutions within same family and ±10% price variance. Rule BR‑PI‑063 applied.”  

---

##### Rules (for DE‑PI‑061)  

| ID | Rule | Category | Description |
|----|------|----------|-------------|
| BR‑PI‑063 | Auto‑Consent Rule | Derivation Rule | If customer’s profile indicates pre‑authorization for substitution within specified parameters (product family, price variance, delivery date shift ≤2 days), consent is automatically granted. |
| BR‑PI‑064 | Consent Timeout Rule | Derivation Rule | If customer does not respond within the consent window (default 24 hours for standard, 4 hours for rush), the default action is applied: substitution accepted, partial delivery accepted, or order held, depending on policy. |

##### Policies (for DE‑PI‑061)  

| ID | Policy | Category | Description |
|----|--------|----------|-------------|
| PO‑PI‑062 | Consent Default Policy | Authorization Policy | Default action on consent timeout is to accept the best available fulfillment option (substitution or partial) unless the customer has explicitly opted out. |

---

#### DE‑PI‑062 — Share Promise Options (Collaborative Promising)  

**Purpose:** For high‑tier customers or large orders, proactively share available promise options (multiple dates, quantities, substitute products) and allow the customer to select their preferred fulfillment scenario.  

**Required Understanding:** ATP/CTP evaluation results showing multiple feasible scenarios, customer tier, order value.  

**Decision Alternatives:**  
- Share all feasible options (dates, quantities, substitutes)  
- Share filtered options (only those meeting strategic criteria)  
- Do not share options (standard promising)  

**Decision Criteria:** Customer tier Platinum or Gold, order value > configurable threshold, or customer has opted into collaborative promising.  

**Decision Rationale:** “Three promise options shared with Customer C for Order ORD‑999: Option A (full qty, 10‑Mar), Option B (partial 80%, 08‑Mar), Option C (substitute Product B, 09‑Mar). Awaiting customer selection.”  

---

##### Rules (for DE‑PI‑062)  

| ID | Rule | Category | Description |
|----|------|----------|-------------|
| BR‑PI‑065 | Collaborative Promising Eligibility Rule | Derivation Rule | Collaborative promising is activated for customers with tier ≥ Gold, orders with value > $50,000, or customers who have explicitly enabled collaborative promising in their profile. |
| BR‑PI‑066 | Option Filtering Rule | Derivation Rule | Options presented must be feasible (ATP/CTP confirmed), within the customer’s acceptance window, and compliant with allocation rules. Unprofitable options (margin < threshold) may be excluded. |

##### Policies (for DE‑PI‑062)  

| ID | Policy | Category | Description |
|----|--------|----------|-------------|
| PO‑PI‑063 | Collaborative Promising Policy | Compliance Policy | Collaborative promising is the default for Platinum customers. Customer selections are treated as firm promises once confirmed. |

---

### 5.6.13 Functional Behaviour  

1. **Event‑driven:** Listens to promise events from Promise Orders, breach events from Detect Promise Exceptions, and change events from Manage Order Changes.  
2. **Retrieve** customer preferences, profiles, and communication history.  
3. **For each event**, execute DE‑PI‑060 (Determine Communication Channel and Content) — rules BR‑PI‑060/061/062, policies PO‑PI‑060/061.  
4. **If consent required**, execute DE‑PI‑061 (Obtain Customer Consent) — rules BR‑PI‑063/064, policy PO‑PI‑062.  
5. **For eligible orders**, execute DE‑PI‑062 (Share Promise Options) — rules BR‑PI‑065/066, policy PO‑PI‑063.  
6. **Transmit** communications and record in communication log.  
7. **Process** customer responses and route to appropriate capability.  
8. **Raise events:** `CommunicationSent`, `CustomerResponseReceived`, `ConsentObtained`, `PromiseOptionsShared`.  

### 5.6.14 Commands  

| Command | Purpose |
|---------|---------|
| `SendCommunication` | Dispatch a customer communication |
| `RequestCustomerConsent` | Send a consent request |
| `SharePromiseOptions` | Send collaborative promising options |
| `RecordCustomerResponse` | Log a customer response or selection |

### 5.6.15 Events  

| Event | Payload Highlights |
|-------|-------------------|
| `CommunicationSent` | Customer ID, order ID, channel, template, timestamp |
| `CustomerResponseReceived` | Order ID, response type, selection, timestamp |
| `ConsentObtained` | Order ID, consent decision (Approved/Declined/TimedOut) |
| `PromiseOptionsShared` | Order ID, options count, deadline |

### 5.6.16 Queries  

| Query | Description |
|-------|-------------|
| `GetCommunicationHistory(customerId, period)` | All communications for a customer |
| `GetPendingConsents()` | Consent requests awaiting response |
| `GetCustomerCollaborationScore(customerId)` | Effectiveness metrics |

### 5.6.17 Reports  

- **Communication Effectiveness Report** – timeliness, accuracy, response rates  
- **Customer Collaboration Dashboard** – active consents, options shared, customer feedback  

### 5.6.18 Dashboards  

- **Customer Communication Monitor** – real‑time communication log  
- **Collaborative Promising Workbench** – options shared, customer selections pending  

### 5.6.19 Software Realization  

```
API → Application Service → Domain Model (CustomerCommunication, ConsentRequest)  
→ Communication Dispatcher (email, portal, EDI, SMS adapters)  
→ Event Store → Projections (CommunicationLog) → Read Model  
```  
The dispatcher supports multiple channels with fallback. Templates are versioned and stored in a content management system.  

---

## 5.7 Sense Promise Risks  

### 5.7.1 Purpose  

Continuously monitor the promise landscape to detect emerging risks that could cause promise breaches before they occur. Answers: *“Which promises are at risk of being broken, and why?”* The capability provides early warning of supply disruptions that affect promised orders, allocation exhaustion that impacts future promises, demand spikes that strain commitments, and systemic promise performance degradation.  

### 5.7.2 Business Objectives Served  

- BO‑PI‑007 Ensure Commitment Feasibility  
- BO‑PI‑002 Maximize Customer Service Reliability  

### 5.7.3 Enterprise Measures  

- PI‑PI‑109 Commitment Risk Score  
- PI‑PI‑102 ATP Accuracy (indirectly, by identifying ATP degradation)  

### 5.7.4 Semantic Objects  

| ID | Object | Role |
|----|--------|------|
| SE‑PI‑002 | Promise | Subject of risk |
| SE‑PI‑025 | Promise Breach | Risk outcome |
| SE‑PI‑004 | Commitment | Commitment at risk |
| SE‑PI‑034 | Allocation Exhaustion | Early exhaustion risk |
| SE‑PI‑073 | ATP/CTP Failure | Systemic risk |

### 5.7.5 Primitive Capabilities Composed  

- **Observe** – ingests supply events and promise performance data  
- **Understand** – maps supply events to affected promises  
- **Assess** – evaluates risk probability and impact  

### 5.7.6 Enterprise Inputs  

- Active promise register (from Understand Orders / Promise Orders)  
- Supply disruption alerts (from Supply Intelligence — Sense Supply Changes)  
- Inventory position changes and allocation consumption rates  
- Demand spikes (from Demand Intelligence — Sense Demand)  
- Promise adherence trends (from Evaluate Promise Quality)  

### 5.7.7 Enterprise Understanding Produced  

- Risk‑flagged promises with risk score, cause, and estimated impact  
- Promise risk heatmap by customer, product, region  
- Proactive mitigation recommendations (re‑promise, substitution, expedite)  

### 5.7.8 Enterprise Outputs  

| ID | Output | Description |
|----|--------|-------------|
| OUT‑PI‑060 | Promise Risk Alert | Individual promise at risk with cause and impact |
| OUT‑PI‑061 | Risk Heatmap | Aggregate view of risk by dimensions |
| OUT‑PI‑062 | Risk Mitigation Recommendation | Suggested actions |

### 5.7.9 Preconditions  

- Promise register is current  
- Supply and demand event streams are operational  

### 5.7.10 Capability Dependencies  

- `CA‑PI‑001 Understand Orders` – for promise register  
- `CA‑SI‑009 Sense Supply Changes` – for supply disruption events  
- `CA‑DI‑003 Sense Demand` – for demand change events  
- `CA‑PI‑008 Evaluate Promise Quality` – for adherence trends  

### 5.7.11 Collaborating Capabilities  

- **Promise Orders** – may trigger re‑promise of at‑risk orders  
- **Detect Promise Exceptions** – forwards risk alerts that materialize  

### 5.7.12 Business Decisions  

---

#### DE‑PI‑070 — Assess Promise Risk  

**Purpose:** For each supply disruption or adverse event, identify which active promises are affected and assess the probability and impact of a breach.  

**Required Understanding:** Supply event (delay, shortage, allocation exhaustion), the promises linked to that supply, customer tier of affected orders, time until promise date.  

**Decision Alternatives:**  
- No risk (promise unaffected)  
- Low risk (minor delay possible, buffer available)  
- Medium risk (breach likely if no action)  
- High risk (breach imminent, immediate action required)  

**Decision Criteria:** Supply disruption impacts the specific supply source linked to the promise, remaining time until promise date < buffer time, customer tier amplifies severity.  

**Decision Confidence:** Based on disruption certainty and supply chain buffer.  

**Decision Rationale:** “Promise for Order ORD‑890 at High risk: linked PO‑890 delayed by 3 days, promise date 05‑Mar, no buffer remaining. Recommended: re‑promise or substitute. Rule BR‑PI‑070 applied.”  

---

##### Rules (for DE‑PI‑070)  

| ID | Rule | Category | Description |
|----|------|----------|-------------|
| BR‑PI‑070 | Promise‑Supply Linkage Rule | Derivation Rule | A promise is at risk if its linked supply source (inventory lot, PO, production order) is affected by a disruption event (delay, shortage, quality hold). |
| BR‑PI‑071 | Risk Scoring Rule | Derivation Rule | Risk Score = (1 − Buffer Time ÷ Lead Time) × Disruption Severity × Customer Tier Weight. Buffer Time = promise date − current date − remaining lead time. |
| BR‑PI‑072 | Risk Aggregation Rule | Derivation Rule | If multiple promises are linked to the same disrupted supply source, all are flagged at the same base risk level, adjusted by individual buffer times. |

##### Policies (for DE‑PI‑070)  

| ID | Policy | Category | Description |
|----|--------|----------|-------------|
| PO‑PI‑070 | High‑Risk Escalation Policy | Authorization Policy | High‑risk promises are immediately escalated to Promise Manager and the affected customer’s account manager. |
| PO‑PI‑071 | Auto‑Mitigation Policy | Automation Policy | Medium‑risk promises with confidence > 80% may be automatically re‑promised against alternate supply if available. |

---

### 5.7.13 Functional Behaviour  

1. **Event‑driven:** Listens to supply disruption events and demand change events.  
2. **Map** disruptions to affected promises via supply‑linkage.  
3. **Execute DE‑PI‑070** (Assess Promise Risk) for each affected promise — rules BR‑PI‑070/071/072, policies PO‑PI‑070/071.  
4. **Publish** risk alerts and update risk heatmap.  
5. **Raise events:** `PromiseRiskAssessed`, `PromiseAtRiskAlert`.  

### 5.7.14 Commands  

| Command | Purpose |
|---------|---------|
| `AssessPromiseRisk` | Run risk assessment for a scope |
| `AcknowledgeRiskAlert` | Planner acknowledges alert |

### 5.7.15 Events  

| Event | Payload Highlights |
|-------|-------------------|
| `PromiseRiskAssessed` | Promise ID, risk score, cause, impact |
| `PromiseAtRiskAlert` | Promise ID, severity, recommended action |

### 5.7.16 Queries  

| Query | Description |
|-------|-------------|
| `GetAtRiskPromises(filter)` | Active promises with risk score > threshold |
| `GetRiskHeatmap(period)` | Aggregate risk view |

### 5.7.17 Reports  

- **Promise Risk Report** – at‑risk promises by severity, cause, customer  

### 5.7.18 Dashboards  

- **Promise Risk Monitor** – real‑time risk alerts, heatmap  

### 5.7.19 Software Realization  

```
Event Bus → Stream Processor → Domain Service (PromiseRiskAssessor)  
→ Alert Publisher → Read Model (RiskHeatmap)  
```  
The risk assessor queries supply‑linkage data from the promise register and matches disruption events to affected commitments.  

---

## 5.8 Evaluate Promise Quality  

### 5.8.1 Purpose  

Continuously measure and assess the quality of promise decisions, promise adherence, ATP/CTP accuracy, order cycle times, and communication effectiveness. Answers: *“How good are our promises, and where are they failing?”* This capability is the analytical engine behind all Business Outcome Measures for Promise Intelligence.  

### 5.8.2 Business Objectives Served  

- BO‑PI‑001 Deliver Trusted Order Commitments  
- BO‑PI‑002 Maximize Customer Service Reliability  
- BO‑PI‑004 Minimize Order Cycle Time  
- BO‑PI‑008 Continuously Improve Promise Intelligence  

### 5.8.3 Enterprise Measures  

- All Business Outcome Measures (PI‑PI‑002 through PI‑PI‑015) are computed by this capability.  
- Intelligence Measures: PI‑PI‑102 (ATP Accuracy), PI‑PI‑103 (CTP Accuracy), PI‑PI‑105 (Recommendation Quality Index — Promise), PI‑PI‑106 (Decision Confidence Index — Promise).  

### 5.8.4 Semantic Objects  

| ID | Object | Role |
|----|--------|------|
| SE‑PI‑002 | Promise | Evaluated promise |
| SE‑PI‑004 | Commitment | Evaluated commitment |
| SE‑PI‑062 | ATP Check Result | Evaluated ATP |
| SE‑PI‑063 | CTP Check Result | Evaluated CTP |
| SE‑PI‑025 | Promise Breach | Counted breach |
| SE‑PI‑024 | Promise Revision | Change history |

### 5.8.5 Primitive Capabilities Composed  

- **Observe** – collects actual fulfillment outcomes  
- **Understand** – aligns promises with actuals  
- **Assess** – computes accuracy metrics  
- **Evaluate** – compares against targets and trends  

### 5.8.6 Enterprise Inputs  

- Promise register with promise dates, types, and sources (from Understand Orders)  
- Actual fulfillment data: delivery dates, quantities, substitutions (from execution systems via Supply Intelligence)  
- ATP/CTP check logs (from Promise Orders)  
- Communication logs (from Collaborate with Customers)  

### 5.8.7 Enterprise Understanding Produced  

- Promise accuracy metrics: fill rate, on‑time delivery, promise adherence, rejection rate  
- ATP/CTP accuracy: promised vs. actual availability  
- Cycle time metrics: order‑to‑promise, change cycle time  
- Communication accuracy metrics  
- Trends and improvement/deterioration signals  

### 5.8.8 Enterprise Outputs  

| ID | Output | Description |
|----|--------|-------------|
| OUT‑PI‑070 | Promise Quality Report | Consolidated promise performance metrics |
| OUT‑PI‑071 | ATP/CTP Accuracy Report | Accuracy of availability predictions |
| OUT‑PI‑072 | Cycle Time Analysis | Order‑to‑promise and change cycle times |

### 5.8.9 Preconditions  

- Promise register is complete with dates and actuals  
- Fulfillment data is available for the evaluation period  

### 5.8.10 Capability Dependencies  

- `CA‑PI‑001 Understand Orders` – for promise register  
- `CA‑PI‑002 Promise Orders` – for ATP/CTP logs  

### 5.8.11 Collaborating Capabilities  

- **Learn From Promise** – consumes quality reports for improvement  
- **Sense Promise Risks** – consumes adherence trends for risk detection  

### 5.8.12 Business Decisions  

---

#### DE‑PI‑080 — Compute Promise Metrics  

**Purpose:** Calculate the standard set of promise performance metrics for a given evaluation period.  

**Decision Criteria:** Apply formulas defined in Chapter 3.  

**Decision Confidence:** Based on data completeness.  

**Decision Rationale:** “Promise Adherence for W27: 94.3%. ATP accuracy 96.1%. All metrics computed.”  

---

##### Rules (for DE‑PI‑080)  

| ID | Rule | Category | Description |
|----|------|----------|-------------|
| BR‑PI‑080 | Metric Calculation Standard Rule | Calculation Rule | All promise metrics are calculated per Chapter 3 formulas. |
| BR‑PI‑081 | Data Completeness Rule | Validation Rule | Metrics for scopes with <90% data availability are flagged as “low confidence”. |

##### Policies (for DE‑PI‑080)  

| ID | Policy | Category | Description |
|----|--------|----------|-------------|
| PO‑PI‑080 | Metric Calculation Frequency Policy | Compliance Policy | Promise metrics are computed daily (operational view), weekly (tactical), and monthly (strategic). |

---

#### DE‑PI‑081 — Publish Promise Quality Report  

**Purpose:** Compile and distribute the periodic promise quality report.  

**Decision Alternatives:** Publish, Publish with flags, Hold.  

**Decision Criteria:** Data completeness ≥ 90%.  

**Decision Rationale:** “Weekly promise quality report published.”  

---

##### Rules (for DE‑PI‑081)  

| ID | Rule | Category | Description |
|----|------|----------|-------------|
| BR‑PI‑082 | Report Completeness Rule | Validation Rule | Quality report must include all mandatory metrics. |

##### Policies (for DE‑PI‑081)  

| ID | Policy | Category | Description |
|----|--------|----------|-------------|
| PO‑PI‑081 | Report Distribution Policy | Compliance Policy | Report published by 10:00 Monday to Order Management and Supply Chain leadership. |

---

### 5.8.13 Functional Behaviour  

1. **Scheduled:** Daily, weekly, monthly.  
2. **Retrieve** promise register, fulfillment actuals, ATP/CTP logs.  
3. **Execute DE‑PI‑080** (Compute Promise Metrics) — rules BR‑PI‑080/081, policy PO‑PI‑080.  
4. **Execute DE‑PI‑081** (Publish Quality Report) — rule BR‑PI‑082, policy PO‑PI‑081.  
5. **Raise events:** `PromiseMetricsComputed`, `PromiseQualityReportPublished`.  

### 5.8.14 Commands  

| Command | Purpose |
|---------|---------|
| `ComputePromiseMetrics` | Run metric calculation |
| `PublishPromiseQualityReport` | Compile and release |

### 5.8.15 Events  

| Event | Payload Highlights |
|-------|-------------------|
| `PromiseMetricsComputed` | Scope, period, metrics |
| `PromiseQualityReportPublished` | Report ID, period |

### 5.8.16 Queries  

| Query | Description |
|-------|-------------|
| `GetPromiseMetrics(scope, period)` | Current metrics |
| `GetATPAccuracy(period)` | ATP accuracy trend |

### 5.8.17 Reports  

- **Promise Quality Report** – all metrics  
- **ATP/CTP Accuracy Analysis** – predicted vs. actual  

### 5.8.18 Dashboards  

- **Promise Performance Dashboard** – fill rate, on‑time, adherence, cycle time  
- **ATP/CTP Accuracy Dashboard** – accuracy trends, bias  

### 5.8.19 Software Realization  

```
API → Application Service → Domain Model (PromiseMetrics)  
→ Calculation Engine (standard formulas)  
→ Event Store → Projections → Read Model  
```  

---

## 5.9 Detect Scenario Exceptions  

### 5.9.1 Purpose  

Identify, classify, prioritize, and resolve exceptions that arise within the Scenario Intelligence domain—simulation failures, calibration drift, recommendation rejection patterns, trigger failures, data gaps, and systemic quality degradation. Answers: *“What is going wrong in our scenario process, and what needs immediate attention?”*  

### 5.9.2 Business Objectives Served  

- BO‑SN‑001 Deliver Trusted Scenario Analysis  
- BO‑SN‑007 Accelerate Response to Change  

### 5.9.3 Enterprise Measures  

- PI‑SN‑004 Scenario Analysis Cycle Time (indirectly, by reducing exception resolution time)  
- PI‑SN‑205 Event Processing Latency (Scenario)  

### 5.9.4 Semantic Objects  

| ID | Object | Role |
|----|--------|------|
| SE‑SN‑002 | Simulation | Source of failures |
| SE‑SN‑022 | Simulation Result | Source of anomalies |
| SE‑SN‑025 | Simulation Run | Failed run |
| SE‑SN‑005 | Scenario Trigger | Failed trigger |
| SE‑SN‑013 | Scenario Status | Stuck or error state |

### 5.9.5 Primitive Capabilities Composed  

- **Observe** – monitors simulation execution, quality metrics, trigger pipeline  
- **Understand** – interprets error signals and patterns  
- **Assess** – determines exception severity and impact  

### 5.9.6 Enterprise Inputs  

- Simulation run logs, failure records (from Simulate Scenarios)  
- Scenario quality metrics (from Evaluate Scenario Quality)  
- Trigger processing logs (from Sense Scenario Triggers)  
- Recommendation rejection records (from Recommend Scenario)  
- Data freshness and completeness indicators  

### 5.9.7 Enterprise Understanding Produced  

- Exception instances with type, severity, affected scenarios or capabilities, root cause, and timestamp  
- Exception queue prioritized by impact on decision‑making  
- Recommended resolution actions  

### 5.9.8 Enterprise Outputs  

| ID | Output | Description |
|----|--------|-------------|
| OUT‑SN‑080 | Scenario Exception Record | Exception with type, severity, status, assigned owner |
| OUT‑SN‑081 | Exception Resolution Recommendation | Suggested action with rationale |

### 5.9.9 Preconditions  

- Monitoring of simulation execution and quality metrics is operational  
- Exception thresholds are configured  

### 5.9.10 Capability Dependencies  

- `CA‑SN‑002 Simulate Scenarios` – for simulation failures  
- `CA‑SN‑007 Sense Scenario Triggers` – for trigger failures  
- `CA‑SN‑008 Evaluate Scenario Quality` – for quality degradation signals  

### 5.9.11 Collaborating Capabilities  

- **Explain Scenario Decisions** – generates explanations for exceptions  
- **Learn From Scenarios** – consumes resolved exceptions for learning  

### 5.9.12 Business Decisions  

---

#### DE‑SN‑090 — Classify Scenario Exception  

**Purpose:** Determine the nature and type of a detected anomaly in the scenario process.  

**Required Understanding:** Anomaly data, context, affected capabilities, historical patterns.  

**Decision Alternatives:**  
- Simulation Failure (run error, time‑out, implausible output)  
- Calibration Drift (probability calibration degrading)  
- Recommendation Rejection (systematic rejection of recommendations)  
- Trigger Failure (trigger not detected or not actioned)  
- Data Gap (missing inputs for simulation)  
- Quality Degradation (accuracy or calibration below threshold)  
- False Positive  

**Decision Criteria:** Rules‑based classification.  

**Decision Confidence:** Based on data quality and corroboration.  

**Decision Rationale:** *Explainability Template:* “Exception EX‑SN‑5001 classified as Simulation Failure: Run SR‑4450 failed after 45 minutes with memory overflow. Data volume within normal range. Rule BR‑SN‑090 applied.”  

---

##### Rules (for DE‑SN‑090)  

| ID | Rule | Category | Description |
|----|------|----------|-------------|
| BR‑SN‑090 | Exception Classification Rule (Scenario) | Derivation Rule | If simulation run status = Error or Timeout → Simulation Failure. If calibration score degrades >0.1 in one quarter → Calibration Drift. If recommendation rejection rate >50% in a quarter → Recommendation Rejection. If trigger not actioned within SLA → Trigger Failure. If required data not available at simulation start → Data Gap. |
| BR‑SN‑091 | False Positive Filter Rule | Validation Rule | Simulation failures caused by known, temporary infrastructure issues (documented outage) are classified as Transient and not raised as exceptions unless they recur >3 times in 24 hours. |

##### Policies (for DE‑SN‑090)  

| ID | Policy | Category | Description |
|----|--------|----------|-------------|
| PO‑SN‑090 | Exception Logging Policy | Compliance Policy | All classified exceptions are logged immutably with traceability to the originating capability. |

---

#### DE‑SN‑091 — Prioritize Scenario Exception  

**Purpose:** Assign severity and urgency to the exception based on impact on decision‑making.  

**Required Understanding:** Exception type, affected scenarios (strategic vs. operational), decision deadlines.  

**Decision Alternatives:** Critical, High, Medium, Low.  

**Decision Criteria:** If exception blocks an active strategic decision → Critical. If it degrades quality but does not block → High or Medium. If it is informational → Low.  

**Decision Rationale:** “Exception EX‑SN‑5001 prioritized as High: Simulation failure blocks comparison for Q3 strategic supply plan, but deadline is 5 days away.”  

---

##### Rules (for DE‑SN‑091)  

| ID | Rule | Category | Description |
|----|------|----------|-------------|
| BR‑SN‑092 | Exception Priority Rule (Scenario) | Derivation Rule | Critical if exception blocks an active strategic recommendation with decision deadline <3 days. High if blocks any active scenario. Medium if degrades quality but does not block. Low if informational. |
| BR‑SN‑093 | Escalation Rule (Scenario) | Validation Rule | Critical exceptions are escalated immediately to Scenario Manager and affected capability owners. |

##### Policies (for DE‑SN‑091)  

| ID | Policy | Category | Description |
|----|--------|----------|-------------|
| PO‑SN‑091 | Exception Escalation Policy (Scenario) | Authorization Policy | Critical exceptions must be acknowledged within 15 minutes, High within 1 hour. |

---

#### DE‑SN‑092 — Resolve Scenario Exception  

**Purpose:** Determine and execute the appropriate resolution action.  

**Required Understanding:** Exception details, available resources, automation rules.  

**Decision Alternatives:**  
- Auto‑resolve (retry simulation, refresh data, clear transient error)  
- Suggest resolution (generate recommendation for specialist)  
- Manual only (requires investigation)  

**Decision Criteria:** Based on exception type and automation policy.  

**Decision Rationale:** “Exception EX‑SN‑5001 auto‑resolved: Simulation retried with reduced parallel load, completed successfully in 52 minutes. Results validated.”  

---

##### Rules (for DE‑SN‑092)  

| ID | Rule | Category | Description |
|----|------|----------|-------------|
| BR‑SN‑094 | Auto‑Resolution Rule (Scenario) | Validation Rule | Simulation Failures and Data Gaps may be auto‑retried once. Calibration Drift and Recommendation Rejection require manual investigation. |
| BR‑SN‑095 | Resolution Documentation Rule | Compliance Rule | Every resolution action must be logged with timestamp, actor, and outcome. |

##### Policies (for DE‑SN‑092)  

| ID | Policy | Category | Description |
|----|--------|----------|-------------|
| PO‑SN‑092 | Resolution SLA Policy (Scenario) | Compliance Policy | Critical: 1 hour, High: 4 hours, Medium: 24 hours, Low: 5 business days. |

---

### 5.9.13 Functional Behaviour  

1. **Event‑driven:** Listens to simulation failures, quality alerts, trigger gaps, recommendation rejections.  
2. **For each anomaly**, execute DE‑SN‑090 (Classify Exception) — rules BR‑SN‑090/091, policy PO‑SN‑090.  
3. **For each confirmed exception**, execute DE‑SN‑091 (Prioritize Exception) — rules BR‑SN‑092/093, policy PO‑SN‑091.  
4. **For each prioritized exception**, execute DE‑SN‑092 (Resolve Exception) — rules BR‑SN‑094/095, policy PO‑SN‑092.  
5. **Track** resolution SLAs.  
6. **Raise events:** `ScenarioExceptionDetected`, `ScenarioExceptionPrioritized`, `ScenarioExceptionResolved`.  

### 5.9.14 Commands  

| Command | Purpose |
|---------|---------|
| `ClassifyScenarioException` | Analyze and assign a type to a detected anomaly. |
| `ResolveScenarioException` | Apply a resolution action (retry, data refresh, manual fix). |
| `EscalateScenarioException` | Manually escalate an exception to a higher authority. |

### 5.9.15 Events  

| Event | Payload Highlights |
|-------|-------------------|
| `ScenarioExceptionDetected` | Exception ID, type, source capability, affected run/scenario, timestamp |
| `ScenarioExceptionPrioritized` | Exception ID, severity (Critical/High/Medium/Low), justification |
| `ScenarioExceptionResolved` | Exception ID, resolution type, actor, outcome, timestamp |
| `ScenarioExceptionEscalated` | Exception ID, escalated to, reason |

### 5.9.16 Queries  

| Query | Description |
|-------|-------------|
| `GetActiveScenarioExceptions(filter)` | List all unresolved exceptions, optionally filtered by type or severity. |
| `GetScenarioExceptionHistory(period)` | Retrieve the log of past exceptions and their resolutions. |
| `GetScenarioException(exceptionId)` | Full details of a single exception. |

### 5.9.17 Reports  

| Report ID | Name | Source Capability | Audience | Frequency | Content Summary |
|-----------|------|-------------------|----------|-----------|-----------------|
| RPT‑SN‑011 | Scenario Exception Summary Report | Detect Scenario Exceptions | Scenario Manager, Simulation Specialist | Weekly | Counts by type, severity, resolution time, SLA compliance, recurring issues. |

### 5.9.18 Dashboards  

| Dashboard ID | Name | Source Capabilities | Audience | Refresh | Panels Summary |
|--------------|------|---------------------|----------|---------|----------------|
| DASH‑SN‑015 | Scenario Exception Monitor | Detect Scenario Exceptions | Scenario Manager | Real‑time | Live exception feed, aging, resolution status, SLA gauges, trend charts. |

### 5.9.19 Software Realization  

```
Event Bus (SimulationFailed, CalibrationDrift, etc.)  
  → Stream Processor (windowed deduplication, anomaly pattern detection)  
  → Domain Service (ScenarioException aggregate)  
      → Rule Engine (classification, prioritization, auto‑resolution)  
  → Event Store → Projections (ExceptionQueue, ExceptionHistory)  
  → Read Model (for dashboards and queries)
```

The exception detection logic runs as a stateful stream processor that correlates failure events, quality metric anomalies, and trigger gaps. Auto‑resolution rules are configurable and can trigger retries, data refreshes, or escalation. All exceptions are immutably logged for audit and learning.

---

## 5.10 Explain Scenario Decisions  

### 5.10.1 Purpose  

Generate clear, traceable explanations for every scenario definition, simulation result, comparison, risk assessment, recommendation, and trigger action. Answers: *“Why was this scenario defined this way? Why did the simulation produce these results? Why was this plan recommended over others?”* Explanations are derived automatically from the causal traceability chain—assumptions, model logic, rule evaluations, and stakeholder input.  

### 5.10.2 Business Objectives Served  

- BO‑SN‑001 Deliver Trusted Scenario Analysis  
- BO‑SN‑008 Continuously Improve Scenario Intelligence  

### 5.10.3 Enterprise Measures  

- PI‑SN‑106 Explainability Score (Scenario)  

### 5.10.4 Semantic Objects  

| ID | Object | Role |
|----|--------|------|
| SE‑SN‑001 | Scenario | Subject |
| SE‑SN‑004 | Scenario Outcome | Subject |
| SE‑SN‑032 | Recommended Plan | Subject |
| SE‑SN‑083 | Scenario Lineage | Traceability chain |

### 5.10.5 Primitive Capabilities Composed  

- **Understand** – interprets decision logs, rule evaluations, stakeholder inputs  

### 5.10.6 Enterprise Inputs  

- Scenario definitions with assumptions and lineage  
- Simulation results and confidence scores  
- Comparison outputs and ranking rationale  
- Risk assessments  
- Recommendation decisions and stakeholder consensus records  
- Trigger events and scope decisions  

### 5.10.7 Enterprise Understanding Produced  

- Structured explanation objects with natural language and machine‑readable traceability  
- Explanation quality scores  

### 5.10.8 Enterprise Outputs  

| ID | Output | Description |
|----|--------|-------------|
| OUT‑SN‑090 | Scenario Explanation | Structured explanation with traceability |

### 5.10.9 Preconditions  

- Decision logs and lineage data are complete  
- Traceability links are populated  

### 5.10.10 Capability Dependencies  

- All scenario capabilities for their decision logs  

### 5.10.11 Collaborating Capabilities  

- **Collaborate on Scenarios** – consumes explanations for workshop materials  
- **Learn From Scenarios** – consumes explanations for learning  

### 5.10.12 Business Decisions  

---

#### DE‑SN‑100 — Generate Scenario Explanation  

**Purpose:** Produce a human‑ and machine‑readable explanation for any scenario artifact.  

**Required Understanding:** The artifact (scenario, simulation, comparison, recommendation), its lineage, the rules and policies applied, the data used.  

**Decision Alternatives:** Deterministic.  

**Decision Criteria:** Explanation must include: (1) what was evaluated, (2) what assumptions were made, (3) what rules/policies applied, (4) what the outcome was and why.  

**Decision Rationale:** “Scenario ‘Q3 Upside Demand’ explanation: This scenario was defined to test plan robustness against demand exceeding forecast by 15%. Assumption based on Sales pipeline data showing 12% upside risk, rounded to 15% for conservatism. Rule BR‑SN‑010 required at least one distinguishing assumption. Simulation used probabilistic method with 10,000 iterations (rule BR‑SN‑020). The recommended plan ‘Flex Capacity’ was selected because it ranked #1 on robustness (82%) and satisfied all risk appetite constraints (rule BR‑SN‑050). Full traceability: Scenario SN‑003 → Simulation SR‑4401 → Comparison CMP‑012 → Recommendation REC‑005.” (Explainability template.)  

---

##### Rules (for DE‑SN‑100)  

| ID | Rule | Category | Description |
|----|------|----------|-------------|
| BR‑SN‑100 | Explanation Completeness Rule (Scenario) | Validation Rule | Every scenario explanation must include: artifact type, assumptions, rules evaluated, policies applied, and outcome with rationale. |
| BR‑SN‑101 | Traceability Chain Rule (Scenario) | Validation Rule | Explanation must include the full ARS traceability chain: Artifact ID → Decision ID(s) → Rule ID(s) → Policy ID(s) → Capability ID(s). |
| BR‑SN‑102 | Natural Language Rule (Scenario) | Derivation Rule | Explanations follow the standard template: “Scenario {{ID}} was defined to {{purpose}}. Assumptions: {{list}}. Simulation used {{method}} with {{iterations}} iterations. Comparison method: {{method}}. Recommendation: {{variant}} selected because {{rationale}}.” |

##### Policies (for DE‑SN‑100)  

| ID | Policy | Category | Description |
|----|--------|----------|-------------|
| PO‑SN‑100 | Explanation Quality Policy (Scenario) | Compliance Policy | Explanations with quality score <60% are flagged. Below 40%, the artifact is held until explanation is enhanced. |

---

### 5.10.13 Functional Behaviour  

1. **Event‑driven:** On scenario publication, simulation completion, comparison completion, recommendation made, trigger actioned.  
2. **Retrieve** decision logs, lineage, rule evaluations.  
3. **Execute DE‑SN‑100** (Generate Explanation) — rules BR‑SN‑100/101/102, policy PO‑SN‑100.  
4. **Publish** explanation with traceability chain.  
5. **Raise events:** `ScenarioExplanationGenerated`.  

### 5.10.14 Commands  

| Command | Purpose |
|---------|---------|
| `GenerateScenarioExplanation` | Create a structured explanation for a given artifact (scenario, simulation, comparison, recommendation). |
| `RegenerateScenarioExplanation` | Rebuild explanation after template or rule changes. |

### 5.10.15 Events  

| Event | Payload Highlights |
|-------|-------------------|
| `ScenarioExplanationGenerated` | Artifact ID, artifact type, natural language text, causal trace, explainability score, timestamp |

### 5.10.16 Queries  

| Query | Description |
|-------|-------------|
| `GetScenarioExplanation(artifactId)` | Retrieve the full structured explanation for a specific artifact. |
| `GetExplainabilityScore(scope, period)` | Aggregate explainability score by capability, scenario, or time period. |

### 5.10.17 Reports  

| Report ID | Name | Source Capability | Audience | Frequency | Content Summary |
|-----------|------|-------------------|----------|-----------|-----------------|
| RPT‑SN‑017 | Explainability Score Report (Scenario) | Explain Scenario Decisions | Scenario Manager, Data Science | Monthly | Average explainability score, breakdown by capability, low‑score items flagged. |

### 5.10.18 Dashboards  

| Dashboard ID | Name | Source Capabilities | Audience | Refresh | Panels Summary |
|--------------|------|---------------------|----------|---------|----------------|
| DASH‑SN‑017 | Explainability Overview (Scenario) | Explain Scenario Decisions | Scenario Manager, Data Science | Weekly | Score trends, explanation completeness, traceability chain visualizer. |

### 5.10.19 Software Realization  

```
Event Bus (ScenarioPublished, SimulationCompleted, ComparisonCompleted, etc.)  
  → Explanation Service (template engine, traceability resolver)  
  → Domain Model (Explanation)  
  → Event Store → Read Model
```

The explanation service subscribes to all scenario‑related events. It retrieves decision logs, rule evaluations, and lineage data from the respective capabilities’ read models. Templates are versioned and stored in a content repository. The service assembles natural‑language explanations and a machine‑readable traceability chain conforming to the ARS standard.

---

## 5.11 Learn From Scenarios  

### 5.11.1 Purpose  

Continuously improve the Scenario Intelligence domain by analyzing scenario accuracy, simulation calibration, recommendation effectiveness, trigger responsiveness, and process efficiency. Answers: *“How can our scenario analysis become more accurate, faster, and more valuable?”* The capability closes the feedback loop, recommending enhancements to simulation models, assumption frameworks, comparison criteria, risk thresholds, and automation policies.  

### 5.11.2 Business Objectives Served  

- BO‑SN‑008 Continuously Improve Scenario Intelligence  
- BO‑SN‑005 Increase Scenario Planning Automation  

### 5.11.3 Enterprise Measures  

- PI‑SN‑110 Learning Effectiveness Index (Scenario)  
- PI‑SN‑105 Recommendation Quality Index (Scenario)  

### 5.11.4 Semantic Objects  

| ID | Object | Role |
|----|--------|------|
| SE‑SN‑001 | Scenario | Evaluated scenario |
| SE‑SN‑020 | Simulation Engine | Subject of improvement |
| SE‑SN‑051 | Comparison Criteria | Subject of tuning |
| SE‑SN‑043 | Risk Mitigation | Subject of effectiveness tracking |

### 5.11.5 Primitive Capabilities Composed  

- **Observe** – monitors performance trends  
- **Understand** – identifies root causes  
- **Assess** – evaluates improvement opportunities  
- **Predict** – forecasts impact of proposed changes  
- **Evaluate** – compares before/after  
- **Learn** – institutionalizes improvements  

### 5.11.6 Enterprise Inputs  

- Scenario quality metrics (from Evaluate Scenario Quality)  
- Exception logs (from Detect Scenario Exceptions)  
- Simulation performance data (run times, failure rates)  
- Recommendation adoption and outcomes  
- Trigger response metrics  

### 5.11.7 Enterprise Understanding Produced  

- Improvement recommendations with estimated impact  
- Simulation model recalibration recommendations  
- Threshold adjustment recommendations (trigger, risk, automation)  
- Assumption framework enhancements  
- Learning loop closure reports  

### 5.11.8 Enterprise Outputs  

| ID | Output | Description |
|----|--------|-------------|
| OUT‑SN‑100 | Improvement Recommendation | Specific change with rationale and expected benefit |
| OUT‑SN‑101 | Learning Loop Closure Report | Before‑after evaluation |

### 5.11.9 Preconditions  

- Historical performance data available for multiple quarters  

### 5.11.10 Capability Dependencies  

- `CA‑SN‑008 Evaluate Scenario Quality` – for quality metrics  
- `CA‑SN‑009 Detect Scenario Exceptions` – for exception data  

### 5.11.11 Collaborating Capabilities  

- **Simulate Scenarios** – receives model recalibration updates  
- **Define Scenarios** – receives assumption framework enhancements  
- **Recommend Scenario** – receives decision criteria tuning  

### 5.11.12 Business Decisions  

---

#### DE‑SN‑110 — Recommend Simulation Improvement  

**Purpose:** Analyze simulation accuracy and performance to recommend model recalibration, engine upgrades, or parameter tuning.  

**Required Understanding:** Simulation accuracy trends, calibration scores, failure rates, run times, resource utilization.  

**Decision Criteria:** If calibration score <0.8 → recalibration. If failure rate >5% → engine tuning. If average run time growth >20% year‑over‑year → performance optimization.  

**Decision Rationale:** “Simulation engine recalibration recommended: calibration score dropped from 0.87 to 0.76 over 2 quarters. Estimated improvement: +0.10 calibration score. Rule BR‑SN‑110 applied.”  

---

##### Rules (for DE‑SN‑110)  

| ID | Rule | Category | Description |
|----|------|----------|-------------|
| BR‑SN‑110 | Recalibration Trigger Rule | Model Evaluation Rule | If calibration score <0.8 or degrades >0.1 in one quarter, a recalibration recommendation is generated. |
| BR‑SN‑111 | Performance Degradation Rule | Model Evaluation Rule | If simulation failure rate >5% or average run time increases >20% without a corresponding increase in model complexity, a performance review is triggered. |

##### Policies (for DE‑SN‑110)  

| ID | Policy | Category | Description |
|----|--------|----------|-------------|
| PO‑SN‑110 | Recalibration Approval Policy | Approval Policy | Recalibration of the simulation engine requires Data Science team approval and is scheduled during the next maintenance window. |

---

#### DE‑SN‑111 — Recommend Threshold Adjustment (Scenario)  

**Purpose:** Analyze trigger sensitivity, risk thresholds, and automation parameters to recommend adjustments that improve responsiveness and reduce noise.  

**Decision Criteria:** Cost‑benefit analysis of threshold changes.  

**Decision Rationale:** “Recommend lowering trigger threshold for demand deviation from 15% to 12%: expected to detect 3 additional actionable triggers per quarter with 1 additional false positive. Net benefit positive.”  

---

##### Rules (for DE‑SN‑111)  

| ID | Rule | Category | Description |
|----|------|----------|-------------|
| BR‑SN‑112 | Threshold Optimization Rule (Scenario) | Model Evaluation Rule | Thresholds are reviewed quarterly. A recommendation to adjust is generated if expected net benefit exceeds a configurable minimum. |
| BR‑SN‑113 | Threshold Stability Rule (Scenario) | Consistency Rule | A threshold shall not be changed more than once per quarter unless a significant event is documented. |

##### Policies (for DE‑SN‑111)  

| ID | Policy | Category | Description |
|----|--------|----------|-------------|
| PO‑SN‑111 | Threshold Adjustment Approval Policy (Scenario) | Approval Policy | Adjustments to trigger thresholds require Scenario Manager approval. Adjustments to risk appetite thresholds require Risk Committee approval. |

---

#### DE‑SN‑112 — Close the Learning Loop (Scenario)  

**Purpose:** After an improvement is implemented, evaluate whether the expected benefit was realized.  

**Decision Alternatives:** Improvement confirmed, Partially realized, No improvement, Negative impact (rollback).  

**Decision Rationale:** “Recalibration completed in Q3. Post‑recalibration calibration score 0.85 (target 0.83), improvement confirmed. Learning loop closed.”  

---

##### Rules (for DE‑SN‑112)  

| ID | Rule | Category | Description |
|----|------|----------|-------------|
| BR‑SN‑114 | Improvement Verification Rule (Scenario) | Validation Rule | Every implemented improvement must be evaluated after a minimum observation window (one quarter for calibration, one month for thresholds). |
| BR‑SN‑115 | Auto‑Rollback Rule (Scenario) | Model Evaluation Rule | If an implemented change causes statistically significant degradation in scenario accuracy or calibration, automatic rollback is triggered. |

##### Policies (for DE‑SN‑112)  

| ID | Policy | Category | Description |
|----|--------|----------|-------------|
| PO‑SN‑112 | Learning Loop Closure Policy (Scenario) | Compliance Policy | Every improvement is documented with before‑after evaluation. Results reported at the quarterly S&OP review. |

---

### 5.11.13 Functional Behaviour  

1. **Scheduled:** Quarterly deep analysis, monthly threshold review.  
2. **Event‑driven:** After quality report publication, after significant exception clusters.  
3. **Retrieve** quality metrics, exception logs, performance data.  
4. **Execute DE‑SN‑110** (Recommend Simulation Improvement) — rules BR‑SN‑110/111, policy PO‑SN‑110.  
5. **Execute DE‑SN‑111** (Recommend Threshold Adjustment) — rules BR‑SN‑112/113, policy PO‑SN‑111.  
6. **For prior improvements**, execute DE‑SN‑112 (Close Learning Loop) — rules BR‑SN‑114/115, policy PO‑SN‑112.  
7. **Publish** recommendations and loop closure reports.  
8. **Feed** approved improvements back to relevant capabilities.  
9. **Raise events:** `ScenarioImprovementRecommended`, `ScenarioLearningLoopClosed`.  

### 5.11.14 Commands  

| Command | Purpose |
|---------|---------|
| `AnalyzeScenarioPerformance` | Run trend analysis on quality, accuracy, calibration, and trigger metrics. |
| `ProposeImprovement` | Generate a specific improvement recommendation (recalibration, threshold adjustment, new scenario type). |
| `EvaluateImprovement` | Assess the impact of an implemented improvement (before/after comparison). |
| `RollbackImprovement` | Revert a change that caused degradation. |

### 5.11.15 Events  

| Event | Payload Highlights |
|-------|-------------------|
| `ScenarioImprovementRecommended` | Improvement ID, type (calibration, threshold, engine), target, expected benefit, confidence. |
| `ScenarioLearningLoopClosed` | Improvement ID, before/after metrics, verdict (confirmed/partial/rejected), rollback status. |

### 5.11.16 Queries  

| Query | Description |
|-------|-------------|
| `GetImprovementHistory(period)` | All recommended and implemented improvements, including outcomes. |
| `GetActiveImprovements()` | Improvements that are awaiting evaluation or in‑progress. |
| `GetLearningEffectivenessIndex()` | Composite metric of improvement success rate over time. |

### 5.11.17 Reports  

| Report ID | Name | Source Capability | Audience | Frequency | Content Summary |
|-----------|------|-------------------|----------|-----------|-----------------|
| RPT‑SN‑012 | Continuous Improvement Report (Scenario) | Learn From Scenarios | Scenario Manager, Supply Chain Director | Quarterly | Improvements proposed, implemented, verified; before/after metrics; learning loop status. |

### 5.11.18 Dashboards  

| Dashboard ID | Name | Source Capabilities | Audience | Refresh | Panels Summary |
|--------------|------|---------------------|----------|---------|----------------|
| DASH‑SN‑016 | Learning Dashboard (Scenario) | Learn From Scenarios | Scenario Manager, Data Science | Monthly | Improvement funnel (proposed → approved → implemented → verified), learning effectiveness trend, rollback events, recalibration schedule. |

### 5.11.19 Software Realization  

```
Scheduled/Event Triggers → Analytics Engine (trend analysis, optimization)  
  → Domain Service (Improvement aggregate, LearningLoop)  
  → Rule Engine (improvement recommendation rules, rollback rules)  
  → Event Store → Projections → Read Model
```
The analytics engine queries the quality, exception, and execution data from the respective read models. Improvement recommendations that require recalibration are forwarded to the Simulation Engine’s model management pipeline. Threshold changes are applied via a configuration service consumed by the appropriate capabilities. All improvement actions are version‑tracked for audit and reproducibility.

---

# Chapter 6 — External Interfaces  

## 6.1 Purpose  

This chapter defines every external interface that the Promise Intelligence domain exposes to other domains, external systems, and users. Each interface is specified with its purpose, contract, authentication, and the capability that owns it. This chapter is derived from the Commands, Queries, and Events defined in Chapter 5.  

## 6.2 Enterprise APIs  

### 6.2.1 Order Ingestion API  

| Attribute | Value |
|-----------|-------|
| Owner | Understand Orders (5.1) |
| Purpose | Accept new order requests, order changes, and cancellations from e‑commerce, EDI, portals, and sales systems. |
| Protocol | REST (HTTPS) |
| Authentication | OAuth 2.0 (Client Credentials / User) |
| Rate Limit | 5,000 requests/minute |
| Endpoint | `POST /api/v1/orders` |

**Request Body:**  
```json
{
  "source": "ECommerce_Platform",
  "orderType": "Standard",
  "customerId": "CUST456",
  "requestedDate": "2026-07-05",
  "lines": [
    {
      "productId": "SKU123",
      "quantity": 100,
      "unit": "EA",
      "shipToLocation": "LOC01"
    }
  ]
}
```  

**Response (202 Accepted):**  
```json
{
  "orderId": "ORD-890",
  "status": "Received",
  "acceptedAt": "2026-06-28T10:23:00Z"
}
```  

---

### 6.2.2 Promise Status API  

| Attribute | Value |
|-----------|-------|
| Owner | Promise Orders (5.2) |
| Purpose | Retrieve the current promise status, promise date, and commitment details for an order. |
| Protocol | REST (HTTPS) |
| Endpoint | `GET /api/v1/promises/{orderId}` |

**Response (200 OK):**  
```json
{
  "orderId": "ORD-890",
  "status": "Promised",
  "lines": [
    {
      "lineId": "L123",
      "promiseType": "ATP",
      "promisedDate": "2026-07-05",
      "promisedQuantity": 100,
      "confidence": 95,
      "supplySource": "INV-DC-A"
    }
  ]
}
```  

---

### 6.2.3 ATP / CTP Evaluation API  

| Attribute | Value |
|-----------|-------|
| Owner | Promise Orders (5.2) |
| Purpose | Execute an ATP or CTP check on‑demand, returning available quantity and earliest date. |
| Endpoint | `POST /api/v1/promises/evaluate` |

**Request Body:**  
```json
{
  "productId": "SKU123",
  "quantity": 500,
  "locationId": "LOC01",
  "requestedDate": "2026-07-10"
}
```  

**Response:**  
```json
{
  "evaluationType": "ATP",
  "availableQuantity": 350,
  "earliestDate": "2026-07-12",
  "sources": [
    {"type": "ON_HAND", "location": "DC-A", "quantity": 200},
    {"type": "INBOUND_PO", "poNumber": "PO-890", "expectedDate": "2026-07-11", "quantity": 150}
  ],
  "confidence": 95
}
```  

---

### 6.2.4 Allocation API  

| Attribute | Value |
|-----------|-------|
| Owner | Manage Allocations (5.3) |
| Purpose | Retrieve and manage allocation rules and pool status. |
| Endpoints | `GET /api/v1/allocations`, `POST /api/v1/allocations/rules`, `GET /api/v1/allocations/pools/{poolId}` |

---

### 6.2.5 Order Change API  

| Attribute | Value |
|-----------|-------|
| Owner | Manage Order Changes (5.5) |
| Purpose | Submit and evaluate order change requests. |
| Endpoints | `POST /api/v1/orders/{orderId}/changes`, `GET /api/v1/orders/{orderId}/changes` |

---

### 6.2.6 Customer Communication API  

| Attribute | Value |
|-----------|-------|
| Owner | Collaborate with Customers (5.6) |
| Purpose | Retrieve communication history and manage communication preferences. |
| Endpoints | `GET /api/v1/customers/{customerId}/communications`, `PUT /api/v1/customers/{customerId}/preferences` |

---

### 6.2.7 Promise Exception API  

| Attribute | Value |
|-----------|-------|
| Owner | Detect Promise Exceptions (5.9) |
| Purpose | Retrieve active and historical promise exceptions. |
| Endpoint | `GET /api/v1/promises/exceptions` |

---

### 6.2.8 Explanation API  

| Attribute | Value |
|-----------|-------|
| Owner | Explain Promise Decisions (5.10) |
| Purpose | Retrieve structured explanation for any promise artifact. |
| Endpoint | `GET /api/v1/promises/explanations/{artifactId}` |

---

## 6.3 Integration Events  

Promise Intelligence publishes events to the enterprise event bus (Kafka topic: `promise-intelligence-events`). All events use the CloudEvents v1.0 envelope.  

| Event Type | Payload Summary | Publisher Capability | Consumers |
|------------|-----------------|---------------------|-----------|
| `OrderAccepted` | Order ID, customer, lines, requested date | Understand Orders | Promise Orders, Prioritize Orders |
| `OrderRejected` | Order ID, reason | Understand Orders | Customer Communication |
| `OrderStatusChanged` | Order ID, old status, new status | Understand Orders | Customer Communication, Evaluate Promise Quality |
| `ATPResultCalculated` | Line ID, ATP qty, earliest date, sources, confidence | Promise Orders | Explain Promise Decisions |
| `CTPResultCalculated` | Line ID, CTP feasible, date, capacity used | Promise Orders | Explain Promise Decisions |
| `SubstitutionOffered` | Line ID, substitute product/location | Promise Orders | Customer Communication |
| `PromiseConfirmed` | Line ID, promise date, type, commitment ID | Promise Orders | Understand Orders, Customer Communication, Manage Allocations, Supply Intelligence (consumption) |
| `PromiseRejected` | Line ID, reason | Promise Orders | Customer Communication |
| `SupplyConsumed` | Supply source, quantity reserved, commitment ref | Promise Orders | Supply Intelligence (Understand Supply), Manage Allocations |
| `AllocationRuleDefined` | Rule ID, item, channel, qty, period | Manage Allocations | Promise Orders |
| `AllocationConsumed` | Promise ref, pool ID, qty consumed | Manage Allocations | Supply Intelligence |
| `AllocationExhausted` | Pool ID, exhaustion time | Manage Allocations | Promise Orders, Sense Promise Risks |
| `OrderPriorityAssigned` | Order ID, score, level | Prioritize Orders | Promise Orders |
| `OrderChangeRequested` | Order ID, change details | Manage Order Changes | Promise Orders |
| `OrderChangeApproved` | Order ID, revised promise | Manage Order Changes | Customer Communication |
| `PromiseOptionsShared` | Order ID, options count | Collaborate with Customers | Customer |
| `ConsentObtained` | Order ID, consent decision | Collaborate with Customers | Promise Orders |
| `PromiseRiskAssessed` | Promise ID, risk score, cause | Sense Promise Risks | Detect Promise Exceptions |
| `PromiseMetricsComputed` | Scope, period, metrics | Evaluate Promise Quality | Learn From Promise |
| `PromiseExceptionDetected` | Exception ID, type, affected orders | Detect Promise Exceptions | Explain Promise Decisions, Learn From Promise |
| `PromiseExceptionResolved` | Exception ID, resolution | Detect Promise Exceptions | Learn From Promise |
| `PromiseExplanationGenerated` | Artifact ID, explanation, traceability | Explain Promise Decisions | Customer Communication, Audit |
| `PromiseImprovementRecommended` | Type, target, benefit | Learn From Promise | Promise Manager |
| `PromiseLearningLoopClosed` | Improvement ID, verdict | Learn From Promise | All promise capabilities |

---

## 6.4 Import Interfaces  

| Interface | Format | Frequency | Target Capability |
|-----------|--------|-----------|-------------------|
| Customer Master Import | CSV / JSON via SFTP | Daily | Understand Orders |
| Customer Tier Update | CSV | On change | Understand Orders |
| Substitution Rule Import | CSV | On change | Promise Orders |
| Allocation Rule Import | CSV | Weekly | Manage Allocations |
| Communication Template Import | JSON | On change | Collaborate with Customers |

---

## 6.5 Export Interfaces  

| Interface | Format | Frequency | Source Capability |
|-----------|--------|-----------|-------------------|
| Order Promise Export to ERP | API / EDI | Real‑time | Promise Orders |
| Allocation Consumption Export | CSV | Daily | Manage Allocations |
| Promise Quality Report Distribution | PDF / Email | Weekly | Evaluate Promise Quality |
| Exception Report Distribution | PDF | Daily | Detect Promise Exceptions |

---

# Chapter 7 — Reports & Dashboards  

## 7.1 Purpose  

This chapter consolidates every report and dashboard defined across the eleven Promise Intelligence capabilities.  

## 7.2 Reports  

| Report ID | Name | Source Capability | Audience | Frequency | Content Summary |
|-----------|------|-------------------|----------|-----------|-----------------|
| RPT‑PI‑001 | Order Book Summary | Understand Orders | Order Manager | Daily | Orders by status, priority, channel |
| RPT‑PI‑002 | Promise Register Report | Understand Orders | Promise Manager | Daily | Active promises with confidence |
| RPT‑PI‑003 | Promise Decision Report | Promise Orders | Promise Manager | Weekly | Promise rate, rejection rate, substitution rate |
| RPT‑PI‑004 | ATP Accuracy Report | Promise Orders, Evaluate Promise Quality | Supply Chain | Weekly | ATP promised vs. actual availability |
| RPT‑PI‑005 | CTP Accuracy Report | Promise Orders, Evaluate Promise Quality | Supply Chain | Weekly | CTP feasibility accuracy |
| RPT‑PI‑006 | Allocation Compliance Report | Manage Allocations | Supply Chain Director | Monthly | Adherence to allocation rules |
| RPT‑PI‑007 | Allocation Pool Health Report | Manage Allocations | Supply Chain Manager | Weekly | Consumption trends, exhaustion forecasts |
| RPT‑PI‑008 | Order Priority Distribution Report | Prioritize Orders | Order Manager | Weekly | Priority score distribution |
| RPT‑PI‑009 | Order Change Analysis Report | Manage Order Changes | Order Manager | Monthly | Change frequency, approval rate, cycle time |
| RPT‑PI‑010 | Communication Effectiveness Report | Collaborate with Customers | Customer Service Manager | Monthly | Timeliness, accuracy, response rates |
| RPT‑PI‑011 | Promise Risk Report | Sense Promise Risks | Promise Manager | Daily | At‑risk promises by severity |
| RPT‑PI‑012 | Promise Quality Report | Evaluate Promise Quality | Supply Chain Director | Weekly | All metrics: fill rate, on‑time, adherence, cycle time |
| RPT‑PI‑013 | Promise Exception Summary Report | Detect Promise Exceptions | Promise Manager | Daily, Weekly | Frequency by type, severity, resolution time |
| RPT‑PI‑014 | Explainability Score Report | Explain Promise Decisions | Data Science | Monthly | Explanation quality by capability |
| RPT‑PI‑015 | Continuous Improvement Report | Learn From Promise | Supply Chain Director | Monthly | Improvements proposed, implemented, verified |

---

## 7.3 Dashboards  

| Dashboard ID | Name | Source Capabilities | Audience | Refresh | Panels Summary |
|--------------|------|---------------------|----------|---------|----------------|
| DASH‑PI‑001 | Order Book Monitor | Understand Orders | Order Manager | Real‑time | Order volumes by status, channel, priority |
| DASH‑PI‑002 | Customer Promise View | Understand Orders | Customer Service | Real‑time | Per‑customer promise status and history |
| DASH‑PI‑003 | Promise Control Tower | Promise Orders | Promise Manager | Real‑time | Promise decisions, confidence distribution, active commitments |
| DASH‑PI‑004 | ATP/CTP Performance Dashboard | Promise Orders, Evaluate Promise Quality | Supply Chain | Daily | Accuracy, cycle time, automation rate |
| DASH‑PI‑005 | Allocation Monitor | Manage Allocations | Supply Chain Manager | Real‑time | Pool status, consumption gauges, exhaustion alerts |
| DASH‑PI‑006 | Order Priority Dashboard | Prioritize Orders | Order Manager | Daily | Priority distribution, queue depth |
| DASH‑PI‑007 | Order Change Monitor | Manage Order Changes | Order Manager | Real‑time | Pending changes, approval status |
| DASH‑PI‑008 | Customer Communication Monitor | Collaborate with Customers | Customer Service | Real‑time | Communication log, pending consents |
| DASH‑PI‑009 | Collaborative Promising Workbench | Collaborate with Customers | Sales, Promise Manager | Real‑time | Options shared, customer selections pending |
| DASH‑PI‑010 | Promise Risk Monitor | Sense Promise Risks | Promise Manager | Real‑time | Risk alerts, heatmap |
| DASH‑PI‑011 | Promise Performance Dashboard | Evaluate Promise Quality | Supply Chain Director | Daily | Fill rate, on‑time, adherence, cycle time |
| DASH‑PI‑012 | ATP/CTP Accuracy Dashboard | Evaluate Promise Quality | Supply Chain | Daily | Accuracy trends, bias detection |
| DASH‑PI‑013 | Promise Exception Monitor | Detect Promise Exceptions | Promise Manager | Real‑time | Live exception feed, SLA status |
| DASH‑PI‑014 | Explainability Overview (Promise) | Explain Promise Decisions | Data Science | Weekly | Explanation quality trends, gaps |
| DASH‑PI‑015 | Learning Dashboard (Promise) | Learn From Promise | Supply Chain Director | Monthly | Improvement funnel, effectiveness index |

---

# Chapter 8 — Appendix  

## 8.1 Promise Exception Priority Matrix  

The following matrix defines the default mapping from Promise Exception Type and Customer Tier to Exception Severity. It is referenced by DE‑PI‑091 (Prioritize Promise Exception) in Section 5.9.  

| Exception Type | Platinum (Critical) | Gold (High) | Silver (Medium) | Bronze (Low) |
|----------------|----------------------|-------------|-----------------|--------------|
| Promise Breach | Critical | Critical | High | Medium |
| Allocation Exhaustion | Critical | High | Medium | Medium |
| ATP/CTP Failure | Critical | High | High | Medium |
| Order Change Exception | High | High | Medium | Low |
| Data Gap | High | Medium | Medium | Low |
| Minor Deviation | — | — | — | — |

**Notes:**  
- Minor Deviations (delivery within 1 day of promise and quantity ≥ 95%) are filtered by BR‑PI‑090 and not classified as exceptions.  
- The matrix is configurable and may be adjusted via the learning feedback loop (DE‑PI‑110) subject to policy PO‑PI‑110.  

---

## 8.2 Enterprise Glossary  

A consolidated glossary of all enterprise terms defined across the Promise Intelligence Specification.  

| Term | ID (if any) | Definition |
|------|-------------|------------|
| Allocation | SE‑PI‑003 | Pre‑reservation of supply for a specific channel or customer group. |
| Allocation Consumption | SE‑PI‑032 | Tracking of how much of an allocation pool has been used. |
| Allocation Exhaustion | SE‑PI‑034 | State where an allocation pool is fully consumed. |
| Allocation Pool | SE‑PI‑031 | Reserved quantity of supply for an allocation rule. |
| Allocation Rule | SE‑PI‑030 | Logic governing how constrained supply is reserved and distributed. |
| ATP (Available‑to‑Promise) | SE‑PI‑060 | Evaluation of uncommitted inventory and inbound supply for promising. |
| ATP Check Result | SE‑PI‑062 | Output of ATP: available quantity, earliest date, source, confidence. |
| Backorder Line | SE‑PI‑015 | Order line not immediately fulfilled; held for re‑promising. |
| Commitment | SE‑PI‑004 | Confirmed obligation to deliver a promised quantity by a promised date. |
| Commitment Level | SE‑PI‑040 | Firm, Tentative, or Contingent. |
| CTP (Capable‑to‑Promise) | SE‑PI‑061 | Evaluation of production capacity and materials for promising. |
| CTP Check Result | SE‑PI‑063 | Output of CTP: feasible quantity, production date, confidence. |
| Customer Communication Preference | SE‑PI‑052 | Preferred channel, frequency, and language for promise communications. |
| Customer Tier (Promise) | SE‑PI‑051 | Platinum, Gold, Silver, Bronze — determines promising priority. |
| On‑Time Delivery (Promise) | PI‑PI‑003 | Percentage of promised lines delivered on or before the promise date. |
| Order | SE‑PI‑001 | Customer request for products. |
| Order Cycle Time | PI‑PI‑004 | Time from order receipt to promise confirmation. |
| Order Fill Rate (Promise) | PI‑PI‑002 | Percentage of orders fulfilled completely at first shipment as promised. |
| Order Line | SE‑PI‑010 | Single item within an order. |
| Order Priority | SE‑PI‑014 | Ranking for promising sequence. |
| Order Split | SE‑PI‑016 | Division of a line into multiple shipments. |
| Order Type | SE‑PI‑013 | Standard, Rush, Contract, Consignment, Intercompany, Sample. |
| Promise | SE‑PI‑002 | Binding commitment to deliver a quantity by a date. |
| Promise Adherence | PI‑PI‑005 | Degree to which actual fulfillment matches the original promise. |
| Promise Breach | SE‑PI‑025 | Failure to fulfill a promise on time or in full. |
| Promise Confidence | SE‑PI‑022 | Score (0–100%) reflecting promise reliability. |
| Promise Date | SE‑PI‑020 | Date by which delivery is committed. |
| Promise Revision | SE‑PI‑024 | Change to an existing promise. |
| Promise Status | SE‑PI‑005 | Requested, Evaluating, Promised, Rejected, Fulfilled, Breached, Cancelled. |
| Promise Type | SE‑PI‑021 | ATP, CTP, Allocation, Substitution. |
| Substitution Option | SE‑PI‑065 | Alternative product, location, or grade offered when primary unavailable. |
| Substitution Rule | SE‑PI‑066 | Rule defining allowable substitutions and consent requirements. |
| Supply Search | SE‑PI‑064 | Process of scanning multiple supply sources for fulfillment. |

---

## 8.3 Formula Reference  

Complete set of formulas used in Chapter 3 (Enterprise Measurement Model).  

**PI‑PI‑002 — Order Fill Rate (Promise)**  
```
Order Fill Rate (Promise) (%) = (Number of Orders Fulfilled as Promised ÷ Total Number of Promised Orders) × 100
```

**PI‑PI‑003 — On‑Time Delivery (to Promise Date)**  
```
On‑Time Delivery (%) = (Number of Order Lines Delivered On Time to Promise ÷ Total Number of Promised Order Lines) × 100
```

**PI‑PI‑004 — Order Cycle Time**  
```
Order Cycle Time = Time(Promise Confirmed) − Time(Order Received)
```

**PI‑PI‑005 — Promise Adherence**  
```
Promise Adherence (%) = (Number of Order Lines Delivered as Promised ÷ Total Number of Promised Order Lines) × 100
```
Where Delivered as Promised = (Actual Delivery Date ≤ Promised Date) AND (Delivered Quantity = Promised Quantity within ±5%).  

**PI‑PI‑006 — Order Rejection Rate**  
```
Order Rejection Rate (%) = (Number of Orders Rejected ÷ Total Number of Orders Requested) × 100
```

**PI‑PI‑007 — Average Promise Lead Time**  
```
Average Promise Lead Time = Σ(Promised Delivery Date − Order Date) ÷ Number of Orders
```

**PI‑PI‑008 — Allocation Compliance**  
```
Allocation Compliance (%) = (Number of Orders Promised Within Allocation ÷ Total Number of Orders Subject to Allocation) × 100
```

**PI‑PI‑009 — Backorder Conversion Rate**  
```
Backorder Conversion Rate (%) = (Number of Backorders Fulfilled ÷ Total Number of Backorders) × 100
```

**PI‑PI‑013 — Revenue Impact of Promising**  
```
Revenue Impact = Σ Revenue from Accepted Orders − Σ Estimated Revenue from Rejected Orders
```

**PI‑PI‑014 — Planning Cycle Time (Promise)**  
```
Planning Cycle Time = Time(Cycle Completed) − Time(Cycle Started)
```

**PI‑PI‑015 — Cash Impact**  
```
Cash Impact = Cost of Expedites − Estimated Margin of Lost Sales Avoided
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
- Supply Intelligence Specification  
- Promise Intelligence Specification (this document)  

### Dependency Specifications  
- Scenario Intelligence Specification (future)  
- Knowledge Intelligence Specification (future)  

---

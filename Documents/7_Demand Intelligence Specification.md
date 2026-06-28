# Demand Intelligence Specification

# Chapter 1 — Purpose & Scope

## 1.1 Purpose

Demand Intelligence is the authoritative enterprise domain responsible for developing trusted understanding of customer demand. Every forecast, segmentation, classification, prioritisation, quality evaluation, exception detection, explanation, and learning activity related to demand originates from and is governed by this specification.

Demand Intelligence provides the foundation upon which all downstream planning activities—supply planning, inventory planning, production planning, procurement, transportation, and order promising—depend.

This specification defines every business objective, performance indicator, semantic concept, capability, decision, rule, policy, functional behaviour, interface, report, and dashboard that constitutes the Demand Intelligence domain. It is the single source of enterprise truth for demand.

## 1.2 Scope

**Demand Intelligence includes:**

- Customer demand and demand signals
- Demand history and demand data quality
- Demand forecasting at all aggregation levels and time horizons
- Demand segmentation (volume, variability, strategic importance)
- Demand classification (pattern recognition: seasonal, intermittent, trend, lumpy)
- Demand prioritisation (business ranking for planning attention)
- Demand monitoring and continuous evaluation
- Demand quality measurement and reporting
- Demand exception detection and alerting
- Demand explainability (why forecasts changed, what drives demand)
- Demand analytics and continuous learning

**Demand Intelligence excludes:**

- Supply planning and supply network design
- Inventory planning and inventory policy setting
- Production planning and production scheduling
- Procurement planning and purchase order management
- Transportation planning and logistics execution
- Order promising and commitment management

These responsibilities belong to their respective Intelligence Domains.

## 1.3 Traceability

| Reference | Principle |
|-----------|-----------|
| C‑EP‑001 | Enterprise First |
| C‑AI‑001 | AI Ready by Design |
| C‑TR‑001 | End‑to‑End Traceability |
| C‑EX‑001 | Explainability |
| ARS‑SM‑001 | Semantic Consistency |
| ARS‑CP‑001 | Capability Consistency |
| ARS‑DM‑001 | Decision Consistency |
| ARS‑RP‑001 | Rule & Policy Consistency |

---

# Chapter 2 — Business Objectives

## BO‑DI‑001 — Deliver Trusted Demand Understanding

**Business Motivation**

Effective planning begins with trusted demand understanding. Demand Intelligence shall continuously transform demand signals into accurate, explainable, and actionable business understanding that the entire enterprise can depend upon.

**Business Questions**

- What is the current enterprise demand across all products, customers, and locations?
- How complete and trustworthy is our demand understanding at every aggregation level?
- Which demand information is uncertain, inconsistent, or missing?
- Where do planners and downstream domains require additional visibility?

**Success Measures**

| PI | Name |
|----|------|
| PI‑DI‑001 | Demand Intelligence Effectiveness (Reserved) |
| PI‑DI‑002 | Forecast Accuracy |
| PI‑DI‑003 | Weighted Absolute Percentage Error (WAPE) |
| PI‑DI‑004 | Mean Absolute Percentage Error (MAPE) |
| PI‑DI‑005 | Forecast Bias |
| PI‑DI‑101 | Demand Understanding Index |
| PI‑DI‑102 | Demand Signal Quality Index |

---

## BO‑DI‑002 — Improve Planning Effectiveness

**Business Motivation**

Planning decisions shall be timely, consistent, and based upon trusted enterprise understanding rather than assumptions, manual interpretation, or tribal knowledge. Demand Intelligence shall improve planning effectiveness by providing reliable recommendations and highlighting only those situations that genuinely require planner attention.

**Business Questions**

- Which forecasts require planner review and which can be accepted automatically?
- Which recommendations can be executed without human intervention?
- Which planning decisions carry the highest business impact?
- Where should planners focus their limited time and expertise?

**Success Measures**

| PI | Name |
|----|------|
| PI‑DI‑006 | Forecast Value Added (FVA) |
| PI‑DI‑201 | Planning Cycle Time |
| PI‑DI‑007 | Forecast Stability |
| PI‑DI‑106 | Recommendation Acceptance Rate |
| PI‑DI‑017 | Automation Rate |

---

## BO‑DI‑003 — Improve Enterprise Responsiveness

**Business Motivation**

The enterprise shall identify significant demand changes as early as possible in order to proactively respond to changing market conditions rather than react after the fact. Demand Intelligence shall continuously monitor demand behaviour and detect meaningful changes, risks, and opportunities.

**Business Questions**

- What has changed in the demand picture and when did it change?
- Why has demand changed—is the change structural, seasonal, or one‑off?
- Which changes require immediate attention from planners or downstream domains?
- What business impact is expected if no action is taken?

**Success Measures**

| PI | Name |
|----|------|
| PI‑DI‑009 | Demand Change Detection Time |
| PI‑DI‑010 | Exception Response Time |
| PI‑DI‑110 | Exception Detection Accuracy |
| PI‑DI‑111 | Exception Prediction Accuracy |

---

## BO‑DI‑004 — Improve Customer Outcomes

**Business Motivation**

Better demand understanding enables the enterprise to consistently satisfy customer demand while reducing shortages, delays, and missed commitments. Every improvement in demand intelligence quality should translate into measurable improvement in customer service.

**Business Questions**

- Which customers are at risk of service failure?
- Which products should receive priority attention to protect customer outcomes?
- Which future shortages are likely given current demand projections?
- Which customer commitments require proactive intervention?

**Success Measures**

| PI | Name |
|----|------|
| PI‑DI‑010 | Service Level |
| PI‑DI‑012 | On Time In Full (OTIF) |
| PI‑DI‑011 | Order Fill Rate |
| PI‑DI‑013 | Perfect Order Rate |
| PI‑DI‑014 | Customer Request Fulfilment Rate |

---

## BO‑DI‑005 — Increase Planning Automation

**Business Motivation**

Routine analytical activities shall be automated wherever possible, allowing planners to concentrate on collaboration, exception management, and high‑value business decisions that genuinely require human judgment. Automation shall increase planner productivity without reducing decision quality.

**Business Questions**

- Which recommendations can be executed automatically at current confidence levels?
- Which situations genuinely require planner intervention?
- Which exceptions should be escalated and to whom?
- Which manual activities provide little business value and should be automated?

**Success Measures**

| PI | Name |
|----|------|
| PI‑DI‑017 | Automation Rate |
| PI‑DI‑018 | Manual Override Rate |
| PI‑DI‑019 | Touchless Planning Rate |
| PI‑DI‑020 | Planner Productivity Index |

---

## BO‑DI‑006 — Continuously Improve Enterprise Intelligence

**Business Motivation**

Demand Intelligence shall continuously evolve by learning from business outcomes, historical performance, and changing market behaviour. This objective ensures that the intelligence provided by Medhavi becomes progressively more accurate, more explainable, and more valuable over time, without requiring architectural redesign.

**Business Questions**

- Are demand recommendations improving quarter over quarter?
- Which forecasting approaches perform best for which demand patterns?
- Which assumptions require revision based on recent evidence?
- Where should Demand Intelligence evolve next to deliver the greatest business benefit?

**Success Measures**

| PI | Name |
|----|------|
| PI‑DI‑021 | Forecast Improvement Rate |
| PI‑DI‑022 | Planning Accuracy Improvement |
| PI‑DI‑023 | Recommendation Quality Index |
| PI‑DI‑024 | Decision Confidence Index |
| PI‑DI‑025 | Explainability Score |
| PI‑DI‑108 | Learning Effectiveness Index |

---

# Chapter 3 — Enterprise Measurement Model

## 3.1 Measurement Architecture

The Enterprise Measurement Model defines every performance indicator used to evaluate Demand Intelligence. Each indicator is a first‑class enterprise object with a unique identifier, complete definition, formula, interpretation, worked example, limitations, and relationships.

**Three measurement tiers:**

| Range | Tier | Purpose |
|-------|------|---------|
| PI‑DI‑001 – PI‑DI‑049 | Business Outcome Measures | Measure business value delivered |
| PI‑DI‑050 – PI‑DI‑099 | Reserved | Future expansion |
| PI‑DI‑100 – PI‑DI‑199 | Intelligence Measures | Measure intelligence quality |
| PI‑DI‑200 – PI‑DI‑299 | Operational Measures | Measure system performance |

**PI‑DI‑001** is reserved for a future composite index—Demand Intelligence Effectiveness—to be derived after all underlying measures are defined.

---

## 3.2 Business Outcome Measures

### PI‑DI‑001 — Demand Intelligence Effectiveness [RESERVED]

This identifier is reserved for a future composite indicator that will aggregate Business Outcome Measures, Intelligence Measures, and Operational Measures into a single executive health score for the Demand Intelligence domain. It cannot be defined until all underlying measures exist and their interactions are understood.

---

### PI‑DI‑002 — Forecast Accuracy

**Definition**

Forecast Accuracy measures the percentage agreement between Forecast Quantity and Actual Quantity over a defined planning horizon. It represents how closely forecast demand reflects actual customer demand. Higher values indicate more accurate forecasting.

**Business Objectives**

- BO‑DI‑001 Deliver Trusted Demand Understanding
- BO‑DI‑002 Improve Planning Effectiveness

**Business Interpretation**

| Value | Interpretation |
|-------|----------------|
| 95% – 100% | Excellent forecasting performance |
| 90% – 95% | Good forecasting performance |
| 80% – 90% | Acceptable forecasting performance |
| Below 80% | Forecast performance requires investigation |

Thresholds are configurable by enterprise policy.

**Formula**

Forecast Accuracy (%) = 100 − WAPE (%)

Where:

WAPE (%) = ( Σ |Forecast Quantity − Actual Quantity| ÷ Σ Actual Quantity ) × 100

**Formula Variables**

| Variable | Type | Definition |
|----------|------|-------------|
| Forecast Quantity | Decimal | Quantity predicted by the forecasting process for a planning bucket |
| Actual Quantity | Decimal | Actual customer demand recorded for the same planning bucket |
| Absolute Error | Decimal | Absolute value of (Forecast Quantity − Actual Quantity) |
| WAPE | Percentage | Weighted Absolute Percentage Error |

**Preconditions**

- Forecast Quantity shall exist for every evaluated planning bucket
- Actual Quantity shall exist for every evaluated planning bucket
- Forecast and Actual shall use identical units of measure
- Σ Actual Quantity shall be greater than zero

If Σ Actual Quantity equals zero, Forecast Accuracy shall be reported as **Not Applicable**.

**Assumptions**

- Forecast and Actual represent identical planning horizons
- Cancelled demand is excluded unless configured otherwise
- All quantities are expressed using a common unit of measure

**Specification**

| Attribute | Value |
|-----------|-------|
| Inputs | Forecast, Actual Demand, Planning Horizon |
| Unit | Percentage (%) |
| Precision | Two decimal places |
| Rounding | Round Half Up |
| Aggregation Levels | SKU, Product, Product Family, Customer, Customer Group, Location, Region, Business Unit, Enterprise |
| Frequency | Daily, Weekly, Monthly, Quarterly |
| Performance Targets | Target ≥95%, Warning 90–95%, Critical <90% (configurable) |
| Business Owner | Demand Planning |
| Business Consumers | Demand Planner, Demand Manager, Supply Planner, Executive Management |
| System Consumers | Reports, Dashboards, APIs, Planning Services, Analytics Services |
| Derived From | Forecast, Actual Demand |
| Related PIs | PI‑DI‑003 WAPE, PI‑DI‑005 Forecast Bias, PI‑DI‑006 Forecast Value Added |

**Worked Example**

| Period | Forecast | Actual | Absolute Error |
|--------|----------|--------|----------------|
| Week 1 | 100 | 110 | 10 |
| Week 2 | 120 | 100 | 20 |
| Week 3 | 130 | 140 | 10 |
| **Total** | **350** | **350** | **40** |

WAPE = (40 ÷ 350) × 100 = 11.43%

Forecast Accuracy = 100 − 11.43 = **88.57%**

Business Interpretation: **Acceptable Forecasting Performance**

**Limitations**

- Forecast Accuracy is undefined when Σ Actual Quantity is zero
- The measure reflects aggregate performance and may mask poor performance on low‑volume items
- Symmetrical errors (over‑forecasting and under‑forecasting) are treated identically; use Forecast Bias for directional analysis

**Relationships**

| Relationship | Reference |
|--------------|-----------|
| Supports | BO‑DI‑001, BO‑DI‑002 |
| Compared With | PI‑DI‑003 WAPE, PI‑DI‑005 Forecast Bias |
| Complemented By | PI‑DI‑006 Forecast Value Added |
| Displayed In | Forecast Performance Dashboard, Executive Planning Dashboard |
| Used By | Forecast Evaluation, Demand Planning, Scenario Planning |

---

### PI‑DI‑003 — Weighted Absolute Percentage Error (WAPE)

**Definition**

Weighted Absolute Percentage Error (WAPE) measures the total absolute forecasting error as a percentage of the total actual demand over a specified planning horizon. Unlike MAPE, WAPE weights forecasting errors according to actual demand volumes, making it suitable for evaluating forecasting performance across products with significantly different demand quantities. Lower values indicate better forecasting performance.

**Business Objectives**

- BO‑DI‑001 Deliver Trusted Demand Understanding
- BO‑DI‑002 Improve Planning Effectiveness

**Business Interpretation**

| Value | Interpretation |
|-------|----------------|
| 0% – 5% | Excellent forecasting performance |
| 5% – 10% | Good forecasting performance |
| 10% – 20% | Acceptable forecasting performance |
| Above 20% | Forecast performance requires investigation |

Thresholds are configurable by enterprise policy.

**Formula**

WAPE (%) = ( Σ |Forecast Quantity − Actual Quantity| ÷ Σ Actual Quantity ) × 100

**Formula Variables**

| Variable | Type | Definition |
|----------|------|-------------|
| Forecast Quantity | Decimal | Quantity predicted by the forecasting process for a planning bucket |
| Actual Quantity | Decimal | Actual customer demand recorded for the corresponding planning bucket |
| Absolute Error | Decimal | Absolute value of (Forecast Quantity − Actual Quantity) |
| Total Absolute Error | Decimal | Sum of Absolute Error across all evaluated planning buckets |
| Total Actual Quantity | Decimal | Sum of Actual Quantity across all evaluated planning buckets |

**Preconditions**

- Forecast Quantity shall exist for every evaluated planning bucket
- Actual Quantity shall exist for every evaluated planning bucket
- Forecast and Actual shall use identical units of measure
- Total Actual Quantity shall be greater than zero

If Total Actual Quantity equals zero, WAPE shall be reported as **Not Applicable**.

**Assumptions**

- Forecast and Actual represent the same planning horizon
- Forecast and Actual use identical units of measure
- Cancelled demand is excluded unless configured otherwise
- Returns and reverse logistics are excluded unless explicitly included by enterprise policy

**Specification**

| Attribute | Value |
|-----------|-------|
| Inputs | Forecast, Actual Demand, Planning Horizon |
| Unit | Percentage (%) |
| Precision | Two decimal places |
| Rounding | Round Half Up |
| Aggregation Levels | SKU, Product, Product Family, Customer, Customer Group, Location, Region, Business Unit, Enterprise |
| Frequency | Daily, Weekly, Monthly, Quarterly |
| Performance Targets | Excellent ≤5%, Good >5–10%, Acceptable >10–20%, Investigation Required >20% (configurable) |
| Business Owner | Demand Planning |
| Business Consumers | Demand Planner, Demand Manager, Supply Planner, Executive Management |
| System Consumers | Reports, Dashboards, Planning Services, Analytics Services |
| Derived From | Forecast, Actual Demand |
| Related PIs | PI‑DI‑002 Forecast Accuracy, PI‑DI‑004 MAPE, PI‑DI‑005 Forecast Bias |

**Worked Example**

| Period | Forecast | Actual | Absolute Error |
|--------|----------|--------|----------------|
| Week 1 | 100 | 110 | 10 |
| Week 2 | 120 | 100 | 20 |
| Week 3 | 130 | 140 | 10 |
| **Total** | **350** | **350** | **40** |

Total Absolute Error = 10 + 20 + 10 = 40

Total Actual Quantity = 110 + 100 + 140 = 350

WAPE = (40 ÷ 350) × 100 = **11.43%**

Business Interpretation: **Acceptable Forecasting Performance**

**Limitations**

- WAPE is undefined when Total Actual Quantity is zero
- WAPE weights errors by volume; low‑volume product errors may be hidden within aggregate measures
- WAPE does not indicate direction of error; use Forecast Bias for directional analysis
- WAPE can be disproportionately influenced by a small number of high‑volume products

**Relationships**

| Relationship | Reference |
|--------------|-----------|
| Supports | BO‑DI‑001, BO‑DI‑002 |
| Used By | PI‑DI‑002 Forecast Accuracy |
| Compared With | PI‑DI‑004 MAPE |
| Displayed In | Forecast Performance Dashboard, Executive Planning Dashboard |
| Used By Business Processes | Forecast Evaluation, Demand Planning, Scenario Planning, Forecast Model Comparison |

---

### PI‑DI‑004 — Mean Absolute Percentage Error (MAPE)

**Definition**

Mean Absolute Percentage Error (MAPE) is the enterprise standard measure for evaluating the average percentage deviation between Forecast Quantity and Actual Quantity across a defined planning horizon.

MAPE treats every planning bucket equally, regardless of demand volume, and therefore measures average forecasting performance rather than volume-weighted forecasting performance.

Lower values indicate better forecasting performance.

**Business Objectives**

- BO‑DI‑001 Deliver Trusted Demand Understanding
- BO‑DI‑002 Improve Planning Effectiveness

**Business Interpretation**

| Value | Interpretation |
|-------|----------------|
| 0% – 10% | Excellent forecasting performance |
| 10% – 20% | Good forecasting performance |
| 20% – 30% | Acceptable forecasting performance |
| Above 30% | Forecast performance requires investigation |

Thresholds are configurable by enterprise policy.

**Formula**

MAPE shall be calculated as the arithmetic mean of the Percentage Error calculated for each evaluated planning bucket.

For each planning bucket:

Percentage Error (%) = ( |Forecast Quantity − Actual Quantity| ÷ Actual Quantity ) × 100

MAPE (%) = Σ Percentage Error ÷ Number of Planning Buckets

**Formula Variables**

| Variable | Type | Definition |
|----------|------|-------------|
| Forecast Quantity | Decimal | Quantity predicted by the forecasting process for a planning bucket |
| Actual Quantity | Decimal | Actual customer demand recorded for the corresponding planning bucket |
| Absolute Error | Decimal | Absolute value of (Forecast Quantity − Actual Quantity) |
| Percentage Error | Percentage | Absolute Error expressed as a percentage of Actual Quantity for an individual planning bucket |
| Number of Planning Buckets | Integer | Number of planning buckets included in the calculation |

**Preconditions**

Enterprise Preconditions

• Forecast and Actual shall represent the same planning horizon.

• Forecast and Actual shall represent the same enterprise object.

• Forecast and Actual shall use identical units of measure.

• Forecast and Actual shall use identical planning calendars.

• Actual Quantity shall be greater than zero for every evaluated planning bucket.

Validation Rules

• Missing Forecast shall invalidate the calculation.

• Missing Actual shall invalidate the calculation.

• Actual Quantity equal to zero shall be handled according to Forecast Performance Policy.

• Invalid planning buckets shall not contribute to the final calculation.


If Actual Quantity equals zero for any planning bucket, that bucket shall either be excluded from the MAPE calculation or processed according to the enterprise's approved forecasting policy. The selected approach shall be applied consistently throughout the enterprise.

**Assumptions**

- Forecast and Actual represent identical planning horizons
- Forecast and Actual use identical units of measure
- Every planning bucket contributes equally to the final result
- Cancelled demand is excluded unless configured otherwise
- Returns and reverse logistics are excluded unless explicitly included by enterprise policy

**Specification**

| Attribute | Value |
|-----------|-------|
| Inputs | Forecast, Actual Demand, Planning Horizon |
| Unit | Percentage (%) |
| Precision | Two decimal places |
| Rounding | Round Half Up |
| Aggregation Levels | SKU, Product, Product Family, Customer, Customer Group, Location, Region, Business Unit, Enterprise |
| Frequency | Daily, Weekly, Monthly, Quarterly |
| Default Enterprise Targets | Excellent ≤10%, Good >10–20%, Acceptable >20–30%, Investigation Required >30% (configurable) |
| Business Owner | Demand Planning |
| Business Consumers | Demand Planner, Demand Manager, Supply Planner, Executive Management |
| System Consumers | Reports, Dashboards, Planning Services, Analytics Services |
| Derived From | Forecast, Actual Demand |
| Related PIs | PI‑DI‑002 Forecast Accuracy, PI‑DI‑003 WAPE, PI‑DI‑005 Forecast Bias |

**Worked Example**

| Period | Forecast | Actual | Percentage Error |
|--------|----------|--------|-------------------|
| Week 1 | 100 | 110 | 9.09% |
| Week 2 | 120 | 100 | 20.00% |
| Week 3 | 130 | 140 | 7.14% |

Total Percentage Error = 9.09 + 20.00 + 7.14 = 36.23%

Number of Planning Buckets = 3

MAPE = 36.23 ÷ 3 = **12.08%**

Business Interpretation: **Good Forecasting Performance**

Edge Case Example

Week 4

Forecast = 120

Actual = 0

Percentage Error

Undefined

MAPE

Not Applicable

Reason

Division by zero.

Resolution

Business Interpretation: Handled according to Forecast Performance Policy.

**Limitations**

- MAPE cannot be calculated when Actual Quantity is zero unless an enterprise‑specific handling policy is defined
- MAPE assigns equal importance to every planning bucket, regardless of demand volume
- MAPE may overemphasize errors associated with low‑volume products because small absolute errors can result in large percentage errors
- MAPE does not indicate whether forecasts are systematically overestimated or underestimated; use Forecast Bias for directional analysis
- MAPE is undefined when any individual Actual Quantity is zero

**Relationships**

| Relationship | Reference |
|--------------|-----------|
| Supports | BO‑DI‑001, BO‑DI‑002 |
| Compared With | PI‑DI‑003 WAPE |
| Complemented By | PI‑DI‑005 Forecast Bias |
| Used By | Forecast Performance Evaluation, Forecast Model Comparison, Forecast Accuracy Assessment |
| Displayed In | Forecast Performance Dashboard, Executive Planning Dashboard |

---

### PI‑DI‑005 — Forecast Bias

**Definition**

Forecast Bias measures the systematic tendency of forecasts to overestimate or underestimate actual demand. It is the arithmetic mean of forecast errors over a defined planning horizon, expressed in the same unit as the underlying demand quantities. A positive bias indicates systematic over‑forecasting; a negative bias indicates systematic under‑forecasting. A bias of zero represents perfectly balanced forecasting.

This specification adopts Mean Forecast Error as the authoritative enterprise definition of Forecast Bias.

**Business Objectives**

- BO‑DI‑001 Deliver Trusted Demand Understanding
- BO‑DI‑002 Improve Planning Effectiveness

**Business Interpretation**

| Value | Interpretation |
|-------|----------------|
| −2% to +2% of average demand | Excellent (no meaningful bias) |
| −5% to −2% or +2% to +5% | Acceptable (minor bias, monitor) |
| Below −5% or above +5% | Investigation required (systematic error present) |

Thresholds are configurable by enterprise policy.

**Formula**

Forecast Bias = Σ (Forecast Quantity − Actual Quantity) ÷ Number of Planning Buckets

Bias may also be expressed as a percentage:

Forecast Bias (%) = ( Forecast Bias ÷ Average Actual Quantity ) × 100

Where Average Actual Quantity = Σ Actual Quantity ÷ Number of Planning Buckets

**Formula Variables**

| Variable | Type | Definition |
|----------|------|-------------|
| Forecast Quantity | Decimal | Quantity predicted by the forecasting process for a planning bucket |
| Actual Quantity | Decimal | Actual customer demand recorded for the corresponding planning bucket |
| Error | Decimal | Forecast Quantity − Actual Quantity (signed, not absolute) |
| Number of Planning Buckets | Integer | Number of planning buckets included in the calculation |
| Average Actual Quantity | Decimal | Arithmetic mean of Actual Quantity across all evaluated buckets |

**Preconditions**

- Forecast Quantity shall exist for every evaluated planning bucket
- Actual Quantity shall exist for every evaluated planning bucket
- Forecast and Actual shall use identical units of measure
- Number of Planning Buckets shall be greater than zero

**Assumptions**

- Positive errors (over‑forecasting) and negative errors (under‑forecasting) may cancel each other; always review bias alongside an accuracy measure such as WAPE or MAPE
- Cancelled demand is excluded unless configured otherwise
- The measure assumes errors are normally distributed for statistical significance testing

**Specification**

| Attribute | Value |
|-----------|-------|
| Inputs | Forecast, Actual Demand, Planning Horizon |
| Unit | Same unit as demand quantity; also expressible as percentage |
| Precision | Two decimal places for percentage; same as demand unit for absolute |
| Rounding | Round Half Up |
| Aggregation Levels | SKU, Product, Product Family, Customer, Customer Group, Location, Region, Business Unit, Enterprise |
| Frequency | Weekly, Monthly, Quarterly |
| Performance Targets | Target ±2%, Warning ±5%, Critical beyond ±5% (configurable) |
| Business Owner | Demand Planning |
| Business Consumers | Demand Planner, Demand Manager, Supply Planner |
| System Consumers | Reports, Dashboards, Analytics Services |
| Derived From | Forecast, Actual Demand |
| Related PIs | PI‑DI‑002 Forecast Accuracy, PI‑DI‑003 WAPE, PI‑DI‑004 MAPE |

**Worked Example**

| Period | Forecast | Actual | Error |
|--------|----------|--------|-------|
| Week 1 | 100 | 110 | −10 |
| Week 2 | 120 | 100 | +20 |
| Week 3 | 130 | 140 | −10 |

Σ Error = (−10) + 20 + (−10) = 0

Number of Planning Buckets = 3

Forecast Bias = 0 ÷ 3 = **0 units**

Average Actual Quantity = (110 + 100 + 140) ÷ 3 = 116.67

Forecast Bias (%) = (0 ÷ 116.67) × 100 = **0.00%**

Business Interpretation: **Excellent (no meaningful bias)**

However, note that WAPE for this example is 11.43%, indicating that while there is no systematic directional error, the magnitude of individual errors is significant. Bias and accuracy must always be evaluated together.

**Second Worked Example (Biased Forecast)**

| Period | Forecast | Actual | Error |
|--------|----------|--------|-------|
| Week 1 | 130 | 110 | +20 |
| Week 2 | 140 | 100 | +40 |
| Week 3 | 150 | 140 | +10 |

Σ Error = 20 + 40 + 10 = +70

Forecast Bias = 70 ÷ 3 = **+23.33 units**

Average Actual Quantity = (110 + 100 + 140) ÷ 3 = 116.67

Forecast Bias (%) = (23.33 ÷ 116.67) × 100 = **+20.00%**

Business Interpretation: **Investigation required — systematic over‑forecasting detected**

**Limitations**

- Bias measures only directional tendency; it does not measure magnitude of error
- Positive and negative errors cancel; a bias of zero does not imply accurate forecasts
- Bias should always be evaluated alongside an accuracy measure (WAPE or MAPE)
- Bias may be misleading for short evaluation periods with few planning buckets
- A single extreme error can dominate the bias calculation

**Relationships**

| Relationship | Reference |
|--------------|-----------|
| Supports | BO‑DI‑001, BO‑DI‑002 |
| Compared With | PI‑DI‑002 Forecast Accuracy, PI‑DI‑003 WAPE, PI‑DI‑004 MAPE |
| Complemented By | PI‑DI‑007 Forecast Stability |
| Displayed In | Forecast Performance Dashboard, Forecast Bias Tracking Report |
| Used By | Forecast Evaluation, Demand Planning, Forecast Model Selection |

---

### PI‑DI‑006 — Forecast Value Added (FVA)

**Definition**

Forecast Value Added (FVA) measures the incremental improvement in forecast accuracy that each step in the forecasting process contributes relative to a naive reference forecast. A naive forecast is defined as the most recent actual demand value carried forward (lag‑1 persistence). FVA identifies which forecasting activities genuinely improve accuracy and which may be adding complexity without adding value.

A positive FVA indicates the forecasting process improves upon the naive forecast. A negative FVA indicates the process degrades forecast accuracy and should be re‑evaluated.

**Business Objectives**

- BO‑DI‑001 Deliver Trusted Demand Understanding
- BO‑DI‑002 Improve Planning Effectiveness

**Business Interpretation**

| Value | Interpretation |
|-------|----------------|
| Positive (any value) | Process step adds value beyond naive forecast |
| Zero ± 1% | Process step is neutral; evaluate cost‑benefit of retention |
| Negative | Process step degrades accuracy; investigate and redesign or eliminate |

Thresholds are configurable by enterprise policy.

**Formula**

FVA = WAPE(Naive) − WAPE(Process)

Where:

- WAPE(Naive) = WAPE of the naive forecast (lag‑1 persistence) over the evaluation horizon
- WAPE(Process) = WAPE of the evaluated forecasting process step over the same evaluation horizon

A positive FVA indicates improvement over the naive forecast.

**Formula Variables**

| Variable | Type | Definition |
|----------|------|-------------|
| WAPE(Naive) | Percentage | WAPE of the naive lag‑1 persistence forecast |
| WAPE(Process) | Percentage | WAPE of the forecasting process being evaluated |
| Naive Forecast | Decimal | Forecast equal to the most recent actual demand value |
| FVA | Percentage points | The difference in WAPE between naive and process forecasts |

**Preconditions**

- Actual demand data shall exist for the full evaluation horizon
- Naive forecast values shall be computed for every planning bucket in the evaluation horizon
- Process forecast values shall exist for every planning bucket in the evaluation horizon
- Total Actual Quantity shall be greater than zero

**Assumptions**

- Naive forecast is defined as lag‑1 persistence: Forecast(t) = Actual(t−1)
- For the first period in the horizon, a prior actual value must exist
- FVA can be computed for the overall forecasting process or for individual process steps (e.g., statistical forecast, judgmental override, consensus adjustment)
- The evaluation horizon should be long enough to produce statistically meaningful results (recommended minimum: 13 weeks)

**Specification**

| Attribute | Value |
|-----------|-------|
| Inputs | Actual Demand, Naive Forecast, Process Forecast, Evaluation Horizon |
| Unit | Percentage points |
| Precision | Two decimal places |
| Rounding | Round Half Up |
| Aggregation Levels | Product, Product Family, Business Unit, Enterprise |
| Frequency | Monthly, Quarterly |
| Performance Targets | Target >0, Warning 0–1%, Critical <0 (configurable) |
| Business Owner | Demand Planning |
| Business Consumers | Demand Planner, Demand Manager, Supply Chain Director |
| System Consumers | Reports, Dashboards, Analytics Services |
| Derived From | PI‑DI‑003 WAPE |
| Related PIs | PI‑DI‑002 Forecast Accuracy, PI‑DI‑003 WAPE, PI‑DI‑007 Forecast Stability |

**Worked Example**

**Step 1: Compute Naive Forecast**

| Period | Actual (t−1) | Naive Forecast (t) |
|--------|-------------|-------------------|
| Week 1 | 95 (prior) | 95 |
| Week 2 | 110 | 110 |
| Week 3 | 100 | 100 |
| Week 4 | 140 | 140 |

**Step 2: Compute Actuals and Process Forecast**

| Period | Naive Forecast | Process Forecast | Actual |
|--------|---------------|-----------------|--------|
| Week 1 | 95 | 105 | 110 |
| Week 2 | 110 | 115 | 100 |
| Week 3 | 100 | 125 | 140 |
| Week 4 | 140 | 135 | 130 |

**Step 3: Compute WAPE for Naive**

| Period | Naive | Actual | Absolute Error |
|--------|-------|--------|----------------|
| Week 1 | 95 | 110 | 15 |
| Week 2 | 110 | 100 | 10 |
| Week 3 | 100 | 140 | 40 |
| Week 4 | 140 | 130 | 10 |
| **Total** | | **480** | **75** |

WAPE(Naive) = (75 ÷ 480) × 100 = 15.63%

**Step 4: Compute WAPE for Process**

| Period | Process | Actual | Absolute Error |
|--------|---------|--------|----------------|
| Week 1 | 105 | 110 | 5 |
| Week 2 | 115 | 100 | 15 |
| Week 3 | 125 | 140 | 15 |
| Week 4 | 135 | 130 | 5 |
| **Total** | | **480** | **40** |

WAPE(Process) = (40 ÷ 480) × 100 = 8.33%

**Step 5: Compute FVA**

FVA = 15.63 − 8.33 = **+7.30 percentage points**

Business Interpretation: **The forecasting process adds significant value beyond the naive forecast. The process reduces WAPE by 7.30 percentage points.**

**Negative FVA Example**

If WAPE(Process) were 18.50% and WAPE(Naive) were 15.63%:

FVA = 15.63 − 18.50 = **−2.87 percentage points**

Business Interpretation: **The forecasting process degrades accuracy relative to a naive forecast. The process steps should be investigated and simplified or redesigned.**

**Limitations**

- FVA depends on the definition of the naive forecast; lag‑1 persistence may not be the appropriate benchmark for all demand patterns (e.g., highly seasonal products may benefit from a seasonal naive benchmark)
- FVA is a relative measure; a positive FVA does not guarantee the forecast is good in absolute terms—it only indicates improvement over naive
- FVA requires a sufficient evaluation horizon; short horizons produce unreliable estimates
- FVA for individual process steps requires isolated step‑level forecasts, which may not always be available
- FVA can be sensitive to outlier periods; consider using a trimmed evaluation window

**Relationships**

| Relationship | Reference |
|--------------|-----------|
| Supports | BO‑DI‑001, BO‑DI‑002 |
| Derived From | PI‑DI‑003 WAPE |
| Compared With | PI‑DI‑007 Forecast Stability |
| Displayed In | Forecast Value Added Report, Forecast Performance Dashboard |
| Used By | Forecast Process Improvement, Forecast Model Selection, Planner Performance Evaluation |

---

### PI‑DI‑007 — Forecast Stability

**Definition**

Forecast Stability measures the degree to which forecasts for a given future period change as that period approaches. It compares forecasts generated at different lead times for the same target period. A stable forecast exhibits minimal change between successive forecast cycles; an unstable forecast fluctuates significantly, creating uncertainty for downstream planning processes.

Forecast Stability is typically computed by comparing the forecast generated at lead time L with the forecast generated at lead time L−1 for the same target period.

**Business Objectives**

- BO‑DI‑001 Deliver Trusted Demand Understanding
- BO‑DI‑002 Improve Planning Effectiveness

**Business Interpretation**

| Value | Interpretation |
|-------|----------------|
| 0% – 10% | Excellent stability (forecasts change minimally as the period approaches) |
| 10% – 20% | Good stability |
| 20% – 30% | Acceptable stability |
| Above 30% | Stability requires investigation (forecast churn may disrupt planning) |

Thresholds are configurable by enterprise policy.

**Formula**

For each target period t, compute the stability error between forecasts generated in successive cycles:

Stability Error(t) = |Forecast(t, cycle c) − Forecast(t, cycle c−1)| ÷ Forecast(t, cycle c−1)

Where both forecasts are for the same target period t but were generated in different forecast cycles.

Forecast Stability (%) = Σ Stability Error(t) ÷ Number of Target Periods × 100

**Formula Variables**

| Variable | Type | Definition |
|----------|------|-------------|
| Forecast(t, cycle c) | Decimal | Forecast for target period t produced in the current forecast cycle |
| Forecast(t, cycle c−1) | Decimal | Forecast for the same target period t produced in the previous forecast cycle |
| Stability Error(t) | Percentage | Absolute percentage change between successive forecasts for target period t |
| Number of Target Periods | Integer | Number of target periods included in the calculation |

**Preconditions**

- At least two successive forecast cycles shall exist covering the same target periods
- Forecast(t, cycle c−1) shall be greater than zero for every evaluated target period
- Both forecast cycles shall use identical units of measure

If Forecast(t, cycle c−1) equals zero for any target period, that period shall be excluded from the calculation.

**Assumptions**

- Successive forecast cycles represent forecasts generated at regular intervals (e.g., weekly cycles)
- The forecasts compared are for identical target periods
- Large stability errors may be caused by genuine demand signal changes rather than model instability; context is required for interpretation
- Forecast Stability does not measure accuracy; a stable but inaccurate forecast is not desirable

**Specification**

| Attribute | Value |
|-----------|-------|
| Inputs | Current cycle forecast, Previous cycle forecast, Common target periods |
| Unit | Percentage (%) |
| Precision | Two decimal places |
| Rounding | Round Half Up |
| Aggregation Levels | SKU, Product, Product Family, Business Unit |
| Frequency | Weekly, Monthly |
| Performance Targets | Target ≤10%, Warning 10–20%, Critical >30% (configurable) |
| Business Owner | Demand Planning |
| Business Consumers | Demand Planner, Demand Manager, Supply Planner |
| System Consumers | Reports, Dashboards, Analytics Services |
| Derived From | Successive forecast cycle data |
| Related PIs | PI‑DI‑002 Forecast Accuracy, PI‑DI‑005 Forecast Bias, PI‑DI‑006 Forecast Value Added |

**Worked Example**

**Target Period: Week 5**

| Cycle | Forecast for Week 5 |
|-------|---------------------|
| Week 1 (Cycle 1) | 200 |
| Week 2 (Cycle 2) | 220 |
| Week 3 (Cycle 3) | 210 |

Stability Error (Cycle 2 vs Cycle 1) = |220 − 200| ÷ 200 = 20 ÷ 200 = 10.00%

Stability Error (Cycle 3 vs Cycle 2) = |210 − 220| ÷ 220 = 10 ÷ 220 = 4.55%

For Week 5: Average Stability Error = (10.00 + 4.55) ÷ 2 = 7.28%

**Expanded Example Across Multiple Target Periods**

| Target Period | Cycle 1 | Cycle 2 | Stability Error |
|---------------|---------|---------|-----------------|
| Week 5 | 200 | 220 | 10.00% |
| Week 6 | 180 | 190 | 5.56% |
| Week 7 | 150 | 175 | 16.67% |

Σ Stability Error = 10.00 + 5.56 + 16.67 = 32.23%

Number of Target Periods = 3

Forecast Stability = 32.23 ÷ 3 = **10.74%**

Business Interpretation: **Good stability**

**Limitations**

- Forecast Stability measures change but not the cause of change; genuine demand signal changes and model noise both contribute
- A highly stable forecast that is inaccurate provides a false sense of security
- Stability is undefined when the prior forecast is zero
- Short evaluation periods may produce unreliable estimates
- Stability should always be evaluated alongside accuracy measures (WAPE, MAPE)
- The measure assumes forecasts are generated at regular intervals; irregular forecast cycles require adjustment

**Relationships**

| Relationship | Reference |
|--------------|-----------|
| Supports | BO‑DI‑001, BO‑DI‑002 |
| Complemented By | PI‑DI‑002 Forecast Accuracy, PI‑DI‑005 Forecast Bias |
| Compared With | PI‑DI‑006 Forecast Value Added |
| Displayed In | Forecast Stability Dashboard, Forecast Performance Report |
| Used By | Forecast Process Evaluation, Model Selection, Planner Review |

---

### PI‑DI‑008 — Forecast Value Realization

**Definition**

Forecast Value Realization measures the ratio of actual business value captured to the potential business value that perfect forecasting would have enabled. It evaluates whether forecast accuracy improvements translate into tangible business outcomes such as reduced inventory, improved service levels, or lower costs.

Unlike Forecast Accuracy, which measures statistical agreement, Forecast Value Realization measures the economic consequence of forecast quality.

**Business Objectives**

- BO‑DI‑001 Deliver Trusted Demand Understanding
- BO‑DI‑002 Improve Planning Effectiveness
- BO‑DI‑004 Improve Customer Outcomes

**Business Interpretation**

| Value | Interpretation |
|-------|----------------|
| 90% – 100% | Excellent value realization |
| 75% – 90% | Good value realization |
| 50% – 75% | Acceptable value realization |
| Below 50% | Value realization requires investigation — forecast accuracy is not translating into business outcomes |

Thresholds are configurable by enterprise policy.

**Formula**

Forecast Value Realization (%) = ( Actual Business Value Achieved ÷ Maximum Potential Business Value with Perfect Forecast ) × 100

Where:

- Actual Business Value Achieved is a composite measure reflecting service level attainment, inventory efficiency, and cost performance under the actual forecast
- Maximum Potential Business Value is the same composite measure computed under the assumption of perfect foresight (forecast equals actual)

The composite measure may be defined by enterprise policy. A recommended default composite is:

Composite Value = (Service Level × w₁) + (Inventory Efficiency × w₂) + (Cost Efficiency × w₃)

Where w₁, w₂, w₃ are enterprise‑defined weights summing to 1.0.

**Formula Variables**

| Variable | Type | Definition |
|----------|------|-------------|
| Actual Business Value Achieved | Decimal | Composite value score achieved with actual forecasts |
| Maximum Potential Business Value | Decimal | Composite value score achievable with perfect forecasts |
| Service Level | Percentage | Actual service level achieved (PI‑DI‑010) |
| Inventory Efficiency | Percentage | Ratio of minimum required inventory to actual inventory held |
| Cost Efficiency | Percentage | Ratio of minimum achievable cost to actual cost incurred |
| w₁, w₂, w₃ | Decimal | Enterprise‑defined weights (sum to 1.0) |

**Preconditions**

- Service level, inventory, and cost data shall be available for the evaluation period
- A simulation or model shall exist to estimate Maximum Potential Business Value under perfect foresight
- Weights w₁, w₂, w₃ shall be defined by enterprise policy and applied consistently

**Assumptions**

- Perfect foresight simulation accurately represents the supply chain's capability to respond if forecasts were perfect
- The composite value formula adequately captures enterprise priorities
- Business outcomes are attributable to forecast quality after controlling for other factors (lead time variability, supply disruptions, etc.)

**Specification**

| Attribute | Value |
|-----------|-------|
| Inputs | Service Level, Inventory data, Cost data, Perfect‑foresight simulation output |
| Unit | Percentage (%) |
| Precision | One decimal place |
| Rounding | Round Half Up |
| Aggregation Levels | Product Family, Business Unit, Enterprise |
| Frequency | Quarterly, Annually |
| Performance Targets | Target ≥90%, Warning 75–90%, Critical <75% (configurable) |
| Business Owner | Demand Planning |
| Business Consumers | Demand Manager, Supply Chain Director, Executive Management |
| System Consumers | Executive Dashboards, Annual Planning Reports |
| Derived From | PI‑DI‑010 Service Level, Inventory metrics, Cost metrics |
| Related PIs | PI‑DI‑002 Forecast Accuracy, PI‑DI‑010 Service Level, PI‑DI‑011 Order Fill Rate |

**Worked Example**

**Enterprise composite weights:** Service Level = 0.50, Inventory Efficiency = 0.30, Cost Efficiency = 0.20

**Under Actual Forecast:**
- Service Level = 94%
- Inventory Efficiency = 82%
- Cost Efficiency = 88%

Actual Composite Value = (94 × 0.50) + (82 × 0.30) + (88 × 0.20)

= 47.0 + 24.6 + 17.6

= **89.2**

**Under Perfect Forecast:**
- Service Level = 99% (theoretical maximum given supply constraints)
- Inventory Efficiency = 95%
- Cost Efficiency = 96%

Maximum Composite Value = (99 × 0.50) + (95 × 0.30) + (96 × 0.20)

= 49.5 + 28.5 + 19.2

= **97.2**

Forecast Value Realization = (89.2 ÷ 97.2) × 100 = **91.8%**

Business Interpretation: **Excellent value realization — 91.8% of the potential business value is being captured with current forecast accuracy**

**Limitations**

- Requires a credible perfect‑foresight simulation model, which may itself contain assumptions
- Composite weights are subjective and must be aligned with enterprise strategy
- Business outcomes are influenced by many factors beyond forecast quality; isolating forecast impact is inherently approximate
- The measure is computationally intensive and typically computed quarterly or annually
- Results are sensitive to the choice of composite formula and weights

**Relationships**

| Relationship | Reference |
|--------------|-----------|
| Supports | BO‑DI‑001, BO‑DI‑002, BO‑DI‑004 |
| Derived From | PI‑DI‑010 Service Level, Inventory metrics, Cost metrics |
| Complemented By | PI‑DI‑002 Forecast Accuracy, PI‑DI‑006 Forecast Value Added |
| Displayed In | Executive Planning Dashboard, Annual Demand Intelligence Report |
| Used By | Strategic Planning, Demand Intelligence Investment Decisions |

---

### PI‑DI‑009 — Demand Plan Adherence

**Definition**

Demand Plan Adherence measures the degree to which actual enterprise execution follows the agreed demand plan. It compares the demand plan quantities that downstream planning processes committed to use against the actual demand plan quantities that were executed. Adherence below 100% indicates that downstream processes deviated from the demand plan, potentially reducing the value of the forecasting effort.

**Business Objectives**

- BO‑DI‑001 Deliver Trusted Demand Understanding
- BO‑DI‑002 Improve Planning Effectiveness

**Business Interpretation**

| Value | Interpretation |
|-------|----------------|
| 95% – 100% | Excellent adherence |
| 85% – 95% | Good adherence |
| 75% – 85% | Acceptable adherence |
| Below 75% | Adherence requires investigation — demand plan is not being followed |

Thresholds are configurable by enterprise policy.

**Formula**

Demand Plan Adherence (%) = ( Quantity Executed Per Plan ÷ Total Planned Quantity ) × 100

Where:

- Quantity Executed Per Plan = sum of quantities where actual execution matched the demand plan within a tolerance band (default: ±5%)
- Total Planned Quantity = sum of all demand plan quantities

**Formula Variables**

| Variable | Type | Definition |
|----------|------|-------------|
| Demand Plan Quantity | Decimal | Quantity specified in the agreed demand plan for a planning bucket |
| Executed Quantity | Decimal | Quantity actually executed by downstream processes |
| Tolerance Band | Percentage | Allowable deviation within which execution is considered adherent (default ±5%) |
| Quantity Executed Per Plan | Decimal | Sum of demand plan quantities where execution fell within the tolerance band |
| Total Planned Quantity | Decimal | Sum of all demand plan quantities |

**Preconditions**

- An agreed demand plan shall exist for the evaluation period
- Executed quantities shall be recorded for every planning bucket
- Tolerance band shall be defined by enterprise policy

**Assumptions**

- Demand plan represents the output of the demand planning process that downstream domains committed to execute
- Deviations may be caused by supply constraints, inventory decisions, or manual overrides
- Adherence is measured at an aggregate level suitable for identifying systematic non‑adherence

**Specification**

| Attribute | Value |
|-----------|-------|
| Inputs | Demand Plan, Actual Execution Quantities, Tolerance Band |
| Unit | Percentage (%) |
| Precision | One decimal place |
| Rounding | Round Half Up |
| Aggregation Levels | Product Family, Business Unit, Enterprise |
| Frequency | Weekly, Monthly |
| Performance Targets | Target ≥95%, Warning 85–95%, Critical <85% (configurable) |
| Business Owner | Demand Planning |
| Business Consumers | Demand Manager, Supply Chain Director |
| System Consumers | Dashboards, Planning Performance Reports |
| Derived From | Demand Plan, Execution Data |
| Related PIs | PI‑DI‑002 Forecast Accuracy, PI‑DI‑018 Manual Override Rate |

**Worked Example**

| Week | Demand Plan | Executed | Deviation | Within Tolerance (±5%) |
|------|-------------|----------|-----------|------------------------|
| Week 1 | 100 | 98 | −2% | Yes |
| Week 2 | 120 | 130 | +8.3% | No |
| Week 3 | 110 | 108 | −1.8% | Yes |
| Week 4 | 130 | 135 | +3.8% | Yes |

Quantity Executed Per Plan = 100 + 110 + 130 = 340

Total Planned Quantity = 100 + 120 + 110 + 130 = 460

Demand Plan Adherence = (340 ÷ 460) × 100 = **73.9%**

Business Interpretation: **Adherence requires investigation — the demand plan is not being followed in approximately one quarter of planning buckets**

**Limitations**

- Adherence measures conformance, not correctness; high adherence to a poor demand plan is not desirable
- The tolerance band is a policy choice that affects results
- Adherence does not distinguish between supply‑caused deviations and planner‑initiated deviations
- Short evaluation periods may produce volatile results

**Relationships**

| Relationship | Reference |
|--------------|-----------|
| Supports | BO‑DI‑001, BO‑DI‑002 |
| Complemented By | PI‑DI‑002 Forecast Accuracy, PI‑DI‑018 Manual Override Rate |
| Displayed In | Demand Planning Performance Dashboard |
| Used By | Demand‑Supply Alignment Reviews, Planning Process Audits |

---

### PI‑DI‑010 — Service Level

**Definition**

Service Level measures the percentage of customer demand that is fulfilled from available inventory within the agreed service time window. It is the primary indicator of the enterprise's ability to satisfy customer demand as it occurs. Higher values indicate better customer service.

For Demand Intelligence, Service Level is an outcome measure that reflects how well demand understanding supports supply execution. While Service Level is influenced by supply and inventory decisions, persistent shortfalls often indicate demand forecasting issues that prevented adequate preparation.

**Business Objectives**

- BO‑DI‑001 Deliver Trusted Demand Understanding
- BO‑DI‑004 Improve Customer Outcomes

**Business Interpretation**

| Value | Interpretation |
|-------|----------------|
| 98% – 100% | World‑class service |
| 95% – 98% | Excellent service |
| 90% – 95% | Good service |
| 85% – 90% | Acceptable service |
| Below 85% | Service performance requires investigation |

Thresholds are configurable by enterprise policy and may vary by customer tier, product category, or channel.

**Formula**

Service Level (%) = ( Quantity Fulfilled Within Service Window ÷ Total Quantity Demanded ) × 100

Where:

- Quantity Fulfilled Within Service Window = sum of demand quantities that were fulfilled completely within the agreed service time window (e.g., same day, next day, 48 hours)
- Total Quantity Demanded = sum of all customer demand quantities during the evaluation period

**Formula Variables**

| Variable | Type | Definition |
|----------|------|-------------|
| Quantity Fulfilled Within Service Window | Decimal | Demand quantity fulfilled within the agreed time window |
| Total Quantity Demanded | Decimal | All customer demand quantity recorded during the evaluation period |
| Service Time Window | Duration | Agreed time between demand occurrence and fulfilment (defined per customer/product/channel) |

**Preconditions**

- A service time window shall be defined for every evaluated combination of customer, product, and channel
- Fulfilment timestamps shall be recorded for every fulfilled demand line
- Demand timestamps shall be recorded for every customer order

**Assumptions**

- Service Level is measured based on first‑touch fulfilment; backorders and subsequent partial fulfilments are counted as missed if they exceed the service window
- Cancelled demand may be included or excluded per enterprise policy; the default is to include cancelled demand as unfulfilled
- The service time window is defined by customer agreement or enterprise policy

**Specification**

| Attribute | Value |
|-----------|-------|
| Inputs | Demand Quantity, Fulfilment Quantity, Fulfilment Timestamp, Demand Timestamp, Service Time Window |
| Unit | Percentage (%) |
| Precision | Two decimal places |
| Rounding | Round Half Up |
| Aggregation Levels | Customer, Customer Group, Product, Product Family, Channel, Location, Business Unit, Enterprise |
| Frequency | Daily, Weekly, Monthly |
| Performance Targets | Target ≥98%, Warning 95–98%, Critical <95% (configurable) |
| Business Owner | Customer Service / Supply Chain |
| Business Consumers | Demand Planner, Supply Planner, Customer Service Manager, Executive Management |
| System Consumers | Reports, Dashboards, Order Promising Services |
| Derived From | Demand transactions, Fulfilment transactions |
| Related PIs | PI‑DI‑011 Order Fill Rate, PI‑DI‑012 OTIF, PI‑DI‑013 Perfect Order Rate |

**Worked Example**

| Order | Demand Qty | Fulfilled Qty | Fulfilment Time | Service Window | Within Window? |
|-------|-----------|---------------|-----------------|----------------|----------------|
| A | 100 | 100 | 4 hours | 24 hours | Yes |
| B | 50 | 50 | 30 hours | 24 hours | No |
| C | 200 | 180 | 6 hours | 48 hours | Partially (180/200) |
| D | 75 | 75 | 2 hours | 8 hours | Yes |

For the purpose of Service Level, partial fulfilment is counted proportionally if the portion fulfilled was within the window.

Quantity Fulfilled Within Window = 100 (A) + 0 (B missed) + 180 (C partial within window) + 75 (D) = 355

Total Quantity Demanded = 100 + 50 + 200 + 75 = 425

Service Level = (355 ÷ 425) × 100 = **83.53%**

Business Interpretation: **Service performance requires investigation** — below the 85% threshold.

**Limitations**

- Service Level is a fulfilment metric, not purely a forecast metric; however, persistent low service often indicates demand forecast inaccuracy that prevented adequate inventory positioning
- The service time window definition can significantly affect results; ensure consistency
- Partial fulfilment within the window is counted optimistically; alternative counting methods exist (e.g., count only complete orders) and should be specified by policy
- Service Level does not distinguish between stock‑outs due to demand underestimation versus supply disruptions

**Relationships**

| Relationship | Reference |
|--------------|-----------|
| Supports | BO‑DI‑001, BO‑DI‑004 |
| Compared With | PI‑DI‑011 Order Fill Rate, PI‑DI‑012 OTIF |
| Complemented By | PI‑DI‑013 Perfect Order Rate |
| Displayed In | Customer Service Dashboard, Executive Planning Dashboard |
| Used By | Demand‑Supply Balancing, Inventory Planning, Customer Commitment Management |

---

### PI‑DI‑011 — Order Fill Rate

**Definition**

Order Fill Rate measures the percentage of customer orders that are fulfilled completely from available inventory at the time of the initial request, without backorders or partial shipments. Each order is counted as either fully filled or not filled; partial fulfilment counts as a miss. Order Fill Rate is a stricter measure than Service Level, which may credit partial fulfilment.

**Business Objectives**

- BO‑DI‑004 Improve Customer Outcomes

**Business Interpretation**

| Value | Interpretation |
|-------|----------------|
| 95% – 100% | Excellent fill rate |
| 90% – 95% | Good fill rate |
| 80% – 90% | Acceptable fill rate |
| Below 80% | Fill rate requires investigation |

Thresholds are configurable by enterprise policy.

**Formula**

Order Fill Rate (%) = ( Number of Orders Completely Filled ÷ Total Number of Orders ) × 100

Where:

- Number of Orders Completely Filled = count of orders where the full requested quantity was fulfilled from stock at the first attempt
- Total Number of Orders = all customer orders during the evaluation period

**Formula Variables**

| Variable | Type | Definition |
|----------|------|-------------|
| Order | Entity | A unique customer request containing one or more line items |
| Completely Filled | Boolean | True if every line item in the order was fulfilled in full at first attempt |
| Number of Orders Completely Filled | Integer | Count of orders meeting the completely filled criterion |
| Total Number of Orders | Integer | Count of all orders during the evaluation period |

**Preconditions**

- Every order shall be recorded with a unique identifier and fulfilment status
- Fulfilment status shall be determined at the point of first shipment or promise

**Assumptions**

- An order is considered filled only if all line items are fully satisfied; partial shipment of a line item disqualifies the entire order
- Backordered items that are filled later do not count as filled for the initial order
- Orders cancelled before fulfilment attempt may be excluded per enterprise policy; default is to include them as unfilled

**Specification**

| Attribute | Value |
|-----------|-------|
| Inputs | Order master, Fulfilment status, Shipment records |
| Unit | Percentage (%) |
| Precision | One decimal place |
| Rounding | Round Half Up |
| Aggregation Levels | Customer, Customer Group, Product Family, Channel, Location, Business Unit, Enterprise |
| Frequency | Daily, Weekly, Monthly |
| Performance Targets | Target ≥95%, Warning 90–95%, Critical <90% (configurable) |
| Business Owner | Customer Service / Supply Chain |
| Business Consumers | Demand Planner, Supply Planner, Customer Service Manager |
| System Consumers | Order Promising Service, Dashboards, Reports |
| Derived From | Order and fulfilment data |
| Related PIs | PI‑DI‑010 Service Level, PI‑DI‑012 OTIF, PI‑DI‑013 Perfect Order Rate |

**Worked Example**

| Order | Lines | Fully Filled? |
|-------|-------|---------------|
| 1001 | 2 | Yes |
| 1002 | 1 | No (partial) |
| 1003 | 3 | Yes |
| 1004 | 1 | No (backorder) |
| 1005 | 2 | Yes |

Number of Orders Completely Filled = 3 (1001, 1003, 1005)

Total Number of Orders = 5

Order Fill Rate = (3 ÷ 5) × 100 = **60.0%**

Business Interpretation: **Fill rate requires investigation** — well below 80%.

**Limitations**

- Order Fill Rate is binary at the order level and does not reflect the volume of demand satisfied
- A high Order Fill Rate may mask severe supply issues on a few large orders; combine with Service Level for a complete picture
- The measure is sensitive to order size distribution; many small orders with high fill rates can obscure a few large missed orders

**Relationships**

| Relationship | Reference |
|--------------|-----------|
| Supports | BO‑DI‑004 |
| Compared With | PI‑DI‑010 Service Level, PI‑DI‑012 OTIF |
| Complemented By | PI‑DI‑013 Perfect Order Rate |
| Displayed In | Customer Service Dashboard, Order Fulfilment Report |
| Used By | Order Promising, Inventory Allocation, Customer Communication |

---

### PI‑DI‑012 — On Time In Full (OTIF)

**Definition**

On Time In Full (OTIF) measures the percentage of customer order lines that are delivered in the quantity ordered and by the agreed delivery date. It is a composite measure that simultaneously assesses quantity completeness (In Full) and delivery timeliness (On Time). An order line is OTIF only if both conditions are met.

OTIF is a widely used customer‑facing metric and directly reflects the reliability of the entire supply chain, including demand planning.

**Business Objectives**

- BO‑DI‑004 Improve Customer Outcomes

**Business Interpretation**

| Value | Interpretation |
|-------|----------------|
| 95% – 100% | World‑class delivery reliability |
| 90% – 95% | Excellent reliability |
| 80% – 90% | Good reliability |
| 70% – 80% | Acceptable reliability |
| Below 70% | Delivery reliability requires investigation |

Thresholds are configurable per customer, channel, or product category.

**Formula**

OTIF (%) = ( Number of Order Lines Delivered OTIF ÷ Total Number of Order Lines ) × 100

Where:

- An order line is OTIF if: (Delivered Quantity = Ordered Quantity) AND (Delivery Date ≤ Agreed Delivery Date)
- Total Number of Order Lines = all order lines with a committed delivery date during the evaluation period

**Formula Variables**

| Variable | Type | Definition |
|----------|------|-------------|
| Ordered Quantity | Decimal | Quantity ordered by the customer for a specific line |
| Delivered Quantity | Decimal | Quantity physically delivered to the customer for that line |
| Agreed Delivery Date | Date | Date by which delivery was promised to the customer |
| Actual Delivery Date | Date | Date on which delivery was completed |
| Number of Order Lines Delivered OTIF | Integer | Count of order lines meeting both quantity and date criteria |
| Total Number of Order Lines | Integer | Count of all order lines with a committed delivery date |

**Preconditions**

- An agreed delivery date shall exist for every order line evaluated
- Delivered quantity and actual delivery date shall be recorded
- Order lines without a committed delivery date (e.g., held orders) shall be excluded

**Assumptions**

- Early delivery is considered On Time unless the customer agreement specifies a delivery window with a start date
- Partial deliveries on different dates: if the total delivered quantity meets the ordered quantity by the final delivery date, the line may be considered OTIF if the final delivery is within the agreed date; otherwise, count as missed
- Returns and rejections are not considered in OTIF; only successful deliveries count

**Specification**

| Attribute | Value |
|-----------|-------|
| Inputs | Order lines, Committed delivery dates, Shipment/delivery confirmations |
| Unit | Percentage (%) |
| Precision | One decimal place |
| Rounding | Round Half Up |
| Aggregation Levels | Customer, Customer Group, Product, Product Family, Channel, Location, Business Unit, Enterprise |
| Frequency | Weekly, Monthly, Quarterly |
| Performance Targets | Target ≥95%, Warning 90–95%, Critical <90% (configurable) |
| Business Owner | Customer Service / Logistics |
| Business Consumers | Demand Planner, Supply Planner, Logistics Manager, Customer Service Manager, Executive Management |
| System Consumers | OTIF Dashboard, Customer Scorecards, Carrier Performance Reports |
| Derived From | Order lines, delivery confirmations |
| Related PIs | PI‑DI‑010 Service Level, PI‑DI‑011 Order Fill Rate, PI‑DI‑013 Perfect Order Rate |

**Worked Example**

| Order Line | Ordered Qty | Delivered Qty | Agreed Date | Actual Delivery | On Time? | In Full? | OTIF? |
|------------|-------------|---------------|-------------|-----------------|----------|----------|-------|
| L1 | 100 | 100 | 05‑Mar | 05‑Mar | Yes | Yes | Yes |
| L2 | 50 | 50 | 06‑Mar | 08‑Mar | No | Yes | No |
| L3 | 200 | 180 | 07‑Mar | 07‑Mar | Yes | No | No |
| L4 | 75 | 75 | 08‑Mar | 08‑Mar | Yes | Yes | Yes |
| L5 | 30 | 30 | 09‑Mar | 09‑Mar | Yes | Yes | Yes |

Number of Order Lines Delivered OTIF = 3 (L1, L4, L5)

Total Number of Order Lines = 5

OTIF = (3 ÷ 5) × 100 = **60.0%**

Business Interpretation: **Delivery reliability requires investigation** — significantly below 70%.

**Limitations**

- OTIF is sensitive to the agreed delivery date; over‑promising (long lead times) inflates OTIF but may not reflect true customer satisfaction
- The binary nature of OTIF does not capture the magnitude of a miss (e.g., missing by 1 unit or 1 day counts the same as a complete failure)
- OTIF is a lagging indicator; by the time a miss is recorded, the customer has already experienced a service failure

**Relationships**

| Relationship | Reference |
|--------------|-----------|
| Supports | BO‑DI‑004 |
| Compared With | PI‑DI‑010 Service Level, PI‑DI‑011 Order Fill Rate |
| Complemented By | PI‑DI‑013 Perfect Order Rate |
| Displayed In | Customer Service Dashboard, Carrier Scorecard, Executive Operations Dashboard |
| Used By | Demand Planning Feedback, Supply Planning, Logistics Planning |

---

### PI‑DI‑013 — Perfect Order Rate

**Definition**

Perfect Order Rate measures the percentage of customer orders that are fulfilled without any error: the correct product, in the correct quantity, delivered to the correct location, on time, with accurate documentation, and without damage. An order is perfect only if every perfection criterion is satisfied.

This is the most stringent customer service metric and reflects the quality of the entire order‑to‑delivery process.

**Business Objectives**

- BO‑DI‑004 Improve Customer Outcomes

**Business Interpretation**

| Value | Interpretation |
|-------|----------------|
| 90% – 100% | Excellent perfect order performance |
| 80% – 90% | Good perfect order performance |
| 65% – 80% | Acceptable perfect order performance |
| Below 65% | Perfect order performance requires investigation |

Thresholds are configurable by enterprise policy.

**Formula**

Perfect Order Rate (%) = ( Number of Perfect Orders ÷ Total Number of Orders ) × 100

Where a Perfect Order satisfies ALL of the following criteria:
- Delivered item matches ordered item (no substitutions unless approved)
- Delivered quantity matches ordered quantity
- Delivered to the correct ship‑to location
- Delivered on or before the agreed delivery date
- Shipping documentation is complete and accurate
- Goods arrive undamaged and in sellable condition

**Formula Variables**

| Variable | Type | Definition |
|----------|------|-------------|
| Perfect Order | Boolean | True if all perfection criteria are satisfied |
| Total Number of Orders | Integer | Count of all orders during the evaluation period |

**Preconditions**

- Each perfection criterion shall have a recorded pass/fail indicator
- Orders without complete criterion data shall be excluded or counted as imperfect per policy

**Assumptions**

- The perfection criteria are defined by enterprise policy and may be tailored per customer or channel
- Damage is defined as any damage that renders the product unsellable or requires rework

**Specification**

| Attribute | Value |
|-----------|-------|
| Inputs | Order data, delivery confirmation, damage claims, documentation accuracy reports |
| Unit | Percentage (%) |
| Precision | One decimal place |
| Rounding | Round Half Up |
| Aggregation Levels | Customer, Channel, Location, Business Unit, Enterprise |
| Frequency | Monthly, Quarterly |
| Performance Targets | Target ≥90%, Warning 80–90%, Critical <80% (configurable) |
| Business Owner | Customer Service / Supply Chain |
| Business Consumers | Demand Planner, Quality Manager, Logistics Manager, Executive Management |
| System Consumers | Perfect Order Dashboard, Customer Scorecards |
| Derived From | Order fulfilment and quality records |
| Related PIs | PI‑DI‑012 OTIF, PI‑DI‑010 Service Level, PI‑DI‑011 Order Fill Rate |

**Worked Example**

| Order | Correct Item | Correct Qty | On Time | Correct Location | Accurate Docs | No Damage | Perfect? |
|-------|--------------|-------------|---------|------------------|---------------|-----------|----------|
| A | Yes | Yes | Yes | Yes | Yes | Yes | Yes |
| B | Yes | Yes | No (1 day late) | Yes | Yes | Yes | No |
| C | Yes | No (short) | Yes | Yes | Yes | Yes | No |
| D | Yes | Yes | Yes | Yes | No (wrong invoice) | Yes | No |
| E | Yes | Yes | Yes | Yes | Yes | Yes | Yes |

Number of Perfect Orders = 2 (A, E)

Total Number of Orders = 5

Perfect Order Rate = (2 ÷ 5) × 100 = **40.0%**

Business Interpretation: **Perfect order performance requires investigation** — well below 65%, indicating systemic process issues.

**Limitations**

- Perfect Order Rate requires detailed tracking of multiple quality dimensions, which may not be available for all channels
- The measure is highly sensitive to data quality in auxiliary systems (e.g., documentation accuracy, damage claims)
- Different customers may have different perfection criteria; aggregating across diverse criteria requires standardisation

**Relationships**

| Relationship | Reference |
|--------------|-----------|
| Supports | BO‑DI‑004 |
| Compared With | PI‑DI‑012 OTIF, PI‑DI‑011 Order Fill Rate |
| Displayed In | Quality Dashboard, Customer Service Scorecard |
| Used By | Customer Experience Improvement, Root Cause Analysis, Demand Planning Feedback |

---

### PI‑DI‑014 — Customer Request Fulfilment Rate

**Definition**

Customer Request Fulfilment Rate measures the percentage of customer requests (orders, quotes, inquiries) that result in a fulfilled order within a specified time period. It captures the conversion of customer intent into actual revenue, reflecting both demand capture effectiveness and the enterprise's ability to fulfil.

For Demand Intelligence, a low fulfilment rate may indicate that demand is not being properly anticipated, leading to stock‑outs and lost sales.

**Business Objectives**

- BO‑DI‑004 Improve Customer Outcomes

**Business Interpretation**

| Value | Interpretation |
|-------|----------------|
| 95% – 100% | Excellent fulfilment |
| 85% – 95% | Good fulfilment |
| 75% – 85% | Acceptable fulfilment |
| Below 75% | Fulfilment requires investigation |

Thresholds are configurable by enterprise policy.

**Formula**

Customer Request Fulfilment Rate (%) = ( Number of Requests Fulfilled ÷ Total Number of Requests ) × 100

Where:
- A request is considered fulfilled if the customer's requested quantity was delivered (or made available for pickup) within the requested time frame or a mutually agreed alternative
- Requests include firm orders, accepted quotes, and pre‑orders

**Formula Variables**

| Variable | Type | Definition |
|----------|------|-------------|
| Request | Entity | A customer's expressed intent to purchase, including orders and accepted quotes |
| Fulfilled | Boolean | True if the full requested quantity was delivered within the requested/agreed time |
| Number of Requests Fulfilled | Integer | Count of fulfilled requests |
| Total Number of Requests | Integer | Count of all customer requests during the evaluation period |

**Preconditions**

- Every customer request shall be recorded with a timestamp and requested quantity
- Fulfilment status shall be determined based on delivery data
- Requests that are cancelled by the customer before fulfilment attempt may be excluded per policy

**Assumptions**

- Customer‑requested time frame is captured at order entry; if not, the standard service window applies
- A request is not considered fulfilled if the customer accepts a partial quantity; that counts as a miss

**Specification**

| Attribute | Value |
|-----------|-------|
| Inputs | Order entry data, Quotation data, Fulfilment data |
| Unit | Percentage (%) |
| Precision | One decimal place |
| Rounding | Round Half Up |
| Aggregation Levels | Customer, Customer Group, Product, Channel, Location, Enterprise |
| Frequency | Weekly, Monthly |
| Performance Targets | Target ≥95%, Warning 85–95%, Critical <85% (configurable) |
| Business Owner | Customer Service / Demand Planning |
| Business Consumers | Demand Planner, Sales Manager, Supply Chain Manager |
| System Consumers | Customer Request Dashboard, Lost Sales Analysis |
| Derived From | Customer requests and fulfilment records |
| Related PIs | PI‑DI‑010 Service Level, PI‑DI‑015 Demand Satisfaction Rate |

**Worked Example**

| Request | Type | Requested Qty | Fulfilled Qty | On Time? | Fulfilled? |
|---------|------|---------------|---------------|----------|------------|
| R1 | Order | 100 | 100 | Yes | Yes |
| R2 | Quote | 50 | 50 | Yes | Yes |
| R3 | Order | 200 | 200 | No (1 week late, customer accepted) | No (per policy, late fulfilment counts as miss) |
| R4 | Order | 75 | 75 | Yes | Yes |
| R5 | Order | 30 | 0 (lost sale) | N/A | No |

Number of Requests Fulfilled = 3 (R1, R2, R4)

Total Number of Requests = 5

Customer Request Fulfilment Rate = (3 ÷ 5) × 100 = **60.0%**

Business Interpretation: **Fulfilment requires investigation** — significantly below 75%, indicating potential lost sales due to demand‑supply mismatch.

**Limitations**

- Late fulfilments that are accepted by the customer may be counted as fulfilled or not per policy; consistency is critical
- The measure depends on accurate recording of all customer requests, including those that never become formal orders (e.g., inquiries that are turned away due to stock‑out)
- Quote conversion is influenced by pricing and competition, not just fulfilment capability

**Relationships**

| Relationship | Reference |
|--------------|-----------|
| Supports | BO‑DI‑004 |
| Compared With | PI‑DI‑010 Service Level, PI‑DI‑015 Demand Satisfaction Rate |
| Displayed In | Lost Sales Dashboard, Customer Service Report |
| Used By | Demand Forecasting Review, Inventory Policy Setting, Sales & Operations Planning |

---

### PI‑DI‑015 — Demand Satisfaction Rate

**Definition**

Demand Satisfaction Rate measures the percentage of total customer demand quantity (in units) that is satisfied through either immediate fulfilment or an accepted backorder within a specified time window. It differs from Service Level by including backorders that the customer accepts, reflecting the enterprise's ability to ultimately satisfy demand even if not immediately.

**Business Objectives**

- BO‑DI‑004 Improve Customer Outcomes

**Business Interpretation**

| Value | Interpretation |
|-------|----------------|
| 98% – 100% | Excellent demand satisfaction |
| 95% – 98% | Good demand satisfaction |
| 90% – 95% | Acceptable demand satisfaction |
| Below 90% | Demand satisfaction requires investigation |

Thresholds are configurable by enterprise policy.

**Formula**

Demand Satisfaction Rate (%) = ( Total Quantity Satisfied ÷ Total Quantity Demanded ) × 100

Where:
- Total Quantity Satisfied = Quantity fulfilled immediately + Quantity backordered and subsequently delivered within the agreed backorder window
- Total Quantity Demanded = total demand quantity from all customer requests (orders, accepted quotes)

**Formula Variables**

| Variable | Type | Definition |
|----------|------|-------------|
| Immediate Fulfilment Quantity | Decimal | Demand quantity fulfilled at first request |
| Backorder Fulfilment Quantity | Decimal | Quantity backordered but later delivered within the agreed backorder window |
| Total Quantity Satisfied | Decimal | Sum of immediate and backorder fulfilments |
| Total Quantity Demanded | Decimal | Sum of all demand quantities requested |

**Preconditions**

- Backorder fulfilment must be tracked with delivery dates
- A backorder window shall be defined (e.g., 7 days, 14 days) per customer/product policy
- Demand quantities shall be recorded

**Assumptions**

- Backorders that exceed the backorder window are not counted as satisfied
- Cancelled backorders are counted as unsatisfied demand

**Specification**

| Attribute | Value |
|-----------|-------|
| Inputs | Demand quantities, Fulfilment data (immediate and backorder) |
| Unit | Percentage (%) |
| Precision | One decimal place |
| Rounding | Round Half Up |
| Aggregation Levels | Product, Product Family, Customer Group, Business Unit, Enterprise |
| Frequency | Weekly, Monthly |
| Performance Targets | Target ≥98%, Warning 95–98%, Critical <95% (configurable) |
| Business Owner | Demand Planning / Supply Chain |
| Business Consumers | Demand Planner, Supply Planner, Inventory Manager |
| System Consumers | Demand Satisfaction Dashboard, S&OP Reports |
| Derived From | Demand and fulfilment records |
| Related PIs | PI‑DI‑010 Service Level, PI‑DI‑014 Customer Request Fulfilment Rate |

**Worked Example**

| Demand | Requested Qty | Immediate Fulfilment | Backorder (within window) | Backorder (late) | Not Fulfilled | Total Satisfied |
|--------|---------------|----------------------|---------------------------|------------------|---------------|-----------------|
| D1 | 100 | 80 | 20 | 0 | 0 | 100 |
| D2 | 50 | 50 | 0 | 0 | 0 | 50 |
| D3 | 200 | 150 | 0 | 50 | 0 | 150 |
| D4 | 75 | 0 | 60 | 0 | 15 | 60 |

Total Quantity Satisfied = 100 + 50 + 150 + 60 = 360

Total Quantity Demanded = 100 + 50 + 200 + 75 = 425

Demand Satisfaction Rate = (360 ÷ 425) × 100 = **84.71%**

Business Interpretation: **Demand satisfaction requires investigation** — below 90%, indicating that even with backorders, a significant portion of demand is lost.

**Limitations**

- The backorder window definition is critical; a longer window inflates the satisfaction rate
- Demand Satisfaction Rate does not differentiate between immediate and delayed fulfilment; customer experience may still be poor even if demand is eventually satisfied
- The measure can mask persistent service issues if backorder windows are very generous

**Relationships**

| Relationship | Reference |
|--------------|-----------|
| Supports | BO‑DI‑004 |
| Compared With | PI‑DI‑010 Service Level, PI‑DI‑014 Customer Request Fulfilment Rate |
| Displayed In | Demand Satisfaction Dashboard, S&OP Meeting Materials |
| Used By | Demand‑Supply Balancing, Inventory Strategy Review |

---

## 3.2 Intelligence Measures

Intelligence Measures evaluate the quality, confidence, explainability, coverage, and effectiveness of the Demand Intelligence domain's internal reasoning. These measures depend on the detailed design of the Capability Realizations (Chapter 5) and are defined as stubs here. Full specifications will be completed once the capabilities are fully specified.

| PI | Name | Description |
|----|------|-------------|
| PI‑DI‑101 | Demand Understanding Index | Composite measure of how completely and reliably the enterprise understands current demand. Reserved. |
| PI‑DI‑102 | Demand Signal Quality Index | Measures the completeness, timeliness, and accuracy of demand signals ingested. Reserved. |
| PI‑DI‑103 | Forecast Confidence Index | Quantifies the confidence (e.g., based on prediction intervals) of the published forecast. Reserved. |
| PI‑DI‑104 | Decision Confidence Index | Average confidence score across all demand decisions in a period. Reserved. |
| PI‑DI‑105 | Recommendation Quality Index | Evaluates the business quality of recommendations (e.g., forecast publication, override suggestions). Reserved. |
| PI‑DI‑106 | Recommendation Acceptance Rate | Percentage of system recommendations accepted by planners or automated processes. Reserved. |
| PI‑DI‑107 | Explainability Score | Measures the degree to which demand outputs are accompanied by complete, traceable explanations. Reserved. |
| PI‑DI‑108 | Learning Effectiveness Index | Measures the rate at which model performance improves due to learning. Reserved. |
| PI‑DI‑109 | Demand Intelligence Coverage Index | Percentage of products/locations with active demand intelligence. Reserved. |
| PI‑DI‑110 | Exception Detection Accuracy | Precision/recall of detected demand exceptions. Reserved. |
| PI‑DI‑111 | Exception Prediction Accuracy | Accuracy of predicted exceptions (e.g., stock‑out risk). Reserved. |
| PI‑DI‑112 | Demand Segmentation Quality | Measures how well demand segments separate distinct behaviours. Reserved. |
| PI‑DI‑113 | Demand Classification Accuracy | Accuracy of demand pattern classification (intermittent, seasonal, etc.). Reserved. |
| PI‑DI‑114 | Demand Prioritization Effectiveness | Evaluates whether prioritization correctly focuses planner attention on high‑impact items. Reserved. |
| PI‑DI‑115 | AI Recommendation Utilization | Usage rate of AI‑generated recommendations by planners or automated processes. Reserved. |

---

## 3.3 Operational Measures

Operational Measures evaluate the technical performance of the Demand Intelligence system. These are placeholders pending the detailed software realization (Chapter 5). Full specifications will be added when implementation decisions are finalised.

| PI | Name | Description |
|----|------|-------------|
| PI‑DI‑201 | Planning Cycle Time | Total time to complete a full demand planning cycle. Reserved. |
| PI‑DI‑202 | Forecast Generation Time | Time taken to generate all forecasts for a cycle. Reserved. |
| PI‑DI‑203 | Demand Refresh Latency | Time from demand signal receipt to reflection in the demand picture. Reserved. |
| PI‑DI‑204 | Data Freshness | Age of the most recent demand data available. Reserved. |
| PI‑DI‑205 | Data Completeness | Percentage of expected demand data points received. Reserved. |
| PI‑DI‑206 | Data Quality Score | Composite score of demand data quality (completeness, accuracy, timeliness). Reserved. |
| PI‑DI‑207 | Integration Success Rate | Percentage of integration events successfully processed. Reserved. |
| PI‑DI‑208 | Event Processing Latency | Time from event publication to processing completion. Reserved. |
| PI‑DI‑209 | Projection Processing Latency | Time to update read model projections. Reserved. |
| PI‑DI‑210 | API Response Time | 95th percentile API response time. Reserved. |
| PI‑DI‑211 | Dashboard Refresh Time | Time for dashboard queries to return. Reserved. |
| PI‑DI‑212 | Report Generation Time | Time to generate standard reports. Reserved. |
| PI‑DI‑213 | System Availability | Uptime percentage of demand intelligence services. Reserved. |
| PI‑DI‑214 | Planning Throughput | Number of planning items processed per unit time. Reserved. |
| PI‑DI‑215 | Exception Processing Time | Time from exception detection to alert generation. Reserved. |

---

# Chapter 4 — Semantic Foundation

## 4.1 Core Enterprise Concepts

The following concepts establish the enterprise meaning upon which all Demand Intelligence capabilities operate. Each concept is a first‑class enterprise object with a unique identifier and a complete definition.

| ID | Concept | Definition |
|----|---------|-------------|
| SE‑DI‑001 | Demand | A customer's expressed or inferred need for a specific product, at a specific location, within a specific time bucket. Demand may be firm (confirmed order), planned (forecast), or historical (actual). Demand is the fundamental unit of Demand Intelligence. |
| SE‑DI‑002 | Demand Signal | Any observable indicator of future or current demand. Includes firm orders, quotations, point‑of‑sale data, web traffic, promotional calendars, social media sentiment, weather forecasts, and economic indicators. Signals vary in reliability and lead time. |
| SE‑DI‑003 | Forecast | A projection of future demand for a specific product‑location combination over a defined time horizon, expressed as a probability distribution (mean and prediction interval). A forecast is always accompanied by metadata describing the model used, generation timestamp, and confidence score. |
| SE‑DI‑004 | Demand History | Recorded actual demand quantities, cleansed and adjusted for known anomalies, for all product‑location‑time bucket combinations. Serves as the ground truth for model training and evaluation. |
| SE‑DI‑005 | Demand Plan | The agreed‑upon set of forecasts that downstream planning processes (supply, inventory, production) commit to use for a defined planning horizon. The demand plan is the authoritative output of the demand planning cycle. |

## 4.2 Demand Concepts

| ID | Concept | Definition |
|----|---------|-------------|
| SE‑DI‑010 | Demand Quantity | The numeric amount of demand expressed in a standard unit of measure (e.g., units, kg, litres). May be positive (demand), zero (no demand), or negative (returns, cancellations). |
| SE‑DI‑011 | Demand Pattern | A recognisable temporal behaviour of demand: continuous, intermittent, lumpy, seasonal, trend, or stationary. Demand pattern drives the selection of forecasting models. |
| SE‑DI‑012 | Demand Variability | The degree of fluctuation in demand around its mean, typically measured by coefficient of variation (CV = standard deviation / mean). Variability influences safety stock and forecast confidence. |
| SE‑DI‑013 | Demand Segmentation | The grouping of products or customers based on demand characteristics such as volume (ABC), variability (XYZ), or strategic importance. Segmentation determines planning attention and model selection. |
| SE‑DI‑014 | Demand Priority | A relative ranking of demand based on business importance. Priority may be driven by customer tier, product margin, contractual obligations, or strategic value. Priority influences allocation decisions and exception handling. |
| SE‑DI‑015 | Demand Exception | A demand observation or forecast behaviour that deviates significantly from expected norms. Exceptions include outliers, level shifts, trend changes, and model failures. |

## 4.3 Forecast Concepts

| ID | Concept | Definition |
|----|---------|-------------|
| SE‑DI‑020 | Forecast Model | A mathematical or machine learning algorithm trained on demand history and signals to produce forecasts. Each model has a type (e.g., exponential smoothing, ARIMA, neural network), hyperparameters, and a training history. |
| SE‑DI‑021 | Prediction Interval | A range around the forecast mean that is expected to contain the actual demand with a specified probability (e.g., 90%). The prediction interval quantifies forecast uncertainty. |
| SE‑DI‑022 | Forecast Confidence | A scalar value (0‑100%) expressing the reliability of the forecast, derived from the model's historical error distribution and the current prediction interval width. |
| SE‑DI‑023 | Forecast Override | A manual adjustment to a system‑generated forecast, accompanied by a mandatory business justification. Overrides are tracked and analysed to identify systematic planner bias. |
| SE‑DI‑024 | Forecast Cycle | A periodic execution of the forecasting process, producing a new set of forecasts for a rolling horizon. Each cycle has a unique identifier and timestamp. |
| SE‑DI‑025 | Forecast Horizon | The future time span covered by a forecast, expressed as a number of time buckets (e.g., 52 weeks). Different horizons may use different models. |

## 4.4 Customer Concepts

| ID | Concept | Definition |
|----|---------|-------------|
| SE‑DI‑030 | Customer | A legal entity or internal unit that places demand. Customers have attributes such as tier, channel, location, and contractual terms. |
| SE‑DI‑031 | Customer Tier | A classification of customers by business importance (e.g., Platinum, Gold, Silver) used for prioritisation and service level targets. |
| SE‑DI‑032 | Customer Channel | The route through which the customer interacts with the enterprise: direct, retail, e‑commerce, wholesale, distributor. Channel influences demand signal characteristics. |
| SE‑DI‑033 | Ship‑To Location | The physical destination where the customer requires delivery. Ship‑to location is a key dimension for demand disaggregation. |

## 4.5 Product Concepts

| ID | Concept | Definition |
|----|---------|-------------|
| SE‑DI‑040 | Product | A distinct sellable item identified by a SKU or material number. Products have attributes such as family, life‑cycle stage, unit of measure, and planning parameters. |
| SE‑DI‑041 | Product Family | A grouping of related products that share demand characteristics or supply resources. Aggregation at family level is common for tactical forecasting. |
| SE‑DI‑042 | Product Life‑Cycle Stage | The phase of a product's market life: Introduction, Growth, Maturity, Decline, End‑of‑Life. Life‑cycle stage influences forecasting method and new‑product treatment. |
| SE‑DI‑043 | Substitutability | The degree to which one product can replace another in customer demand. Substitution affects demand cannibalisation and forecasting for product groups. |

## 4.6 Time Concepts

| ID | Concept | Definition |
|----|---------|-------------|
| SE‑DI‑050 | Time Bucket | The smallest time interval used for planning and forecasting (e.g., day, week). All demand quantities are aggregated to time buckets. |
| SE‑DI‑051 | Planning Horizon | The total future span for which forecasts are generated, measured from the current date. Horizons are typically defined in time buckets (e.g., 13 weeks, 52 weeks). |
| SE‑DI‑052 | Lead Time | The time between demand recognition and the earliest possible fulfilment. Lead time is critical for determining the forecast horizon needed for supply planning. |
| SE‑DI‑053 | Frozen Period | A near‑term interval within which the demand plan is fixed and cannot be changed without special authorisation. The frozen period stabilises execution. |

## 4.7 Demand Relationships

| ID | Relationship | Definition |
|----|-------------|-------------|
| SE‑DI‑060 | Demand Aggregation | The summation of demand quantities from lower to higher levels of the product, customer, location, or time hierarchies. Aggregated demand typically exhibits lower relative variability. |
| SE‑DI‑061 | Demand Disaggregation | The proportional allocation of a higher‑level forecast to lower‑level entities based on historical mix percentages or user‑defined ratios. |
| SE‑DI‑062 | Demand Correlation | The statistical association between demand for different products, customers, or locations. Positive correlation may amplify risk; negative correlation may provide a natural hedge. |
| SE‑DI‑063 | Demand Dependency | A causal relationship where demand for one item depends on demand for another (e.g., component demand depends on finished‑good demand). Dependent demand is typically planned rather than forecast. |
| SE‑DI‑064 | Demand Cannibalisation | The reduction in demand for one product caused by the introduction or promotion of another. Cannibalisation must be modelled in forecasting to avoid overestimation. |

## 4.8 Common Enumerations

**Demand Pattern Classification**

| Value | Description |
|-------|-------------|
| Continuous | Demand occurs in most periods with low variability; suitable for standard time‑series models |
| Intermittent | Demand occurs infrequently with many zero‑demand periods; requires specialised models (e.g., Croston) |
| Lumpy | Demand occurs with high variability and irregular intervals; challenging to forecast; may require event‑based modelling |
| Seasonal | Demand exhibits regular, predictable calendar‑based patterns; seasonal decomposition is required |
| Trend | Demand shows a sustained upward or downward movement over time; trend‑aware models required |
| Stationary | Demand fluctuates around a stable mean without trend or seasonality; simple smoothing models may suffice |

**Demand Signal Type**

| Value | Description |
|-------|-------------|
| Firm Order | A confirmed customer purchase order with specified quantity, date, and terms |
| Quotation | A customer request for pricing and availability that may convert to an order |
| Point‑of‑Sale | Sell‑through data from retail or e‑commerce channels, indicating actual consumption |
| Promotional Calendar | Planned marketing activities expected to influence demand |
| Market Intelligence | External data such as competitor actions, economic indicators, regulatory changes |
| Weather Forecast | Predicted weather conditions that influence demand for weather‑sensitive products |
| Social Sentiment | Consumer sentiment indicators from social media and online reviews |

**Forecast Model Type**

| Value | Description |
|-------|-------------|
| Statistical | Traditional time‑series models: Exponential Smoothing, ARIMA, Croston |
| Machine Learning | ML models: Random Forest, Gradient Boosting, LSTM, Prophet |
| Judgemental | Planner‑provided estimates based on experience; used when historical data is insufficient |
| Hybrid | Combination of statistical/ML output with judgemental adjustments |
| Naive | Simple benchmark: lag‑1 persistence or seasonal naive |

---

We are now aligned on the decision‑centric structure. The previous two parts remain untouched because they contain domain‑wide objects (objectives, measures, semantic concepts) — they don’t need restructuring. I’ll now build **Chapter 5 – Capability Realizations**, starting with the first capabilities, using the nested model that places Rules and Policies directly under the Decision they govern. This is Part 3.

---

# Chapter 5 — Capability Realizations

## 5.1 Understand Demand

### 5.1.1 Purpose
Establish a trusted, complete, and current picture of enterprise demand by consolidating demand history, demand signals, and ongoing demand observations. Answers the question: *“What is the enterprise demand right now, and what is our basis for knowing it?”* The capability serves as the single source of truth for all downstream demand reasoning, providing cleansed, aggregated demand views and flagging data gaps or quality issues.

### 5.1.2 Business Objectives Served
- BO‑DI‑001 (Deliver Trusted Demand Understanding)
- BO‑DI‑003 (Improve Enterprise Responsiveness)

### 5.1.3 Enterprise Measures
- PI‑DI‑002 (Forecast Accuracy) — indirectly, by providing clean history
- PI‑DI‑102 (Demand Signal Quality Index)
- PI‑DI‑205 (Data Completeness)
- PI‑DI‑206 (Data Quality Score)

### 5.1.4 Semantic Objects

| ID | Object | Role |
|----|--------|------|
| SE‑DI‑001 | Demand | Core unit |
| SE‑DI‑002 | Demand Signal | Ingested, validated, timestamped |
| SE‑DI‑004 | Demand History | Cleansed, adjusted, stored |
| SE‑DI‑010 | Demand Quantity | Numeric value |
| SE‑DI‑015 | Demand Exception | Flagged anomalies |
| SE‑DI‑030 | Customer | Source dimension |
| SE‑DI‑040 | Product | Product dimension |
| SE‑DI‑050 | Time Bucket | Aggregation dimension |

### 5.1.5 Primitive Capabilities Composed
- **Observe** – captures demand signals and actuals
- **Understand** – interprets and cleanses data into a unified demand picture
- **Assess** – evaluates data quality and completeness

### 5.1.6 Enterprise Inputs
- Demand transactions (orders, shipments, POS) from source systems
- Demand signals (promotions, events, external indicators)
- Product and customer master data
- Calendar and time‑bucket definitions

### 5.1.7 Enterprise Understanding Produced
- A unified, time‑bucketed demand history for every product–location combination
- Cleaned demand series with outlier flags and adjustments documented
- Current demand snapshot (last N periods) with freshness timestamp
- Data quality score per series (completeness, outlier rate, signal lag)
- Active demand signals with their type, source, and confidence

### 5.1.8 Preconditions
- Source systems provide demand transactions at least daily
- Product and customer master data are available and maintained
- Time‑bucket definitions are consistent across the enterprise

### 5.1.9 Business Decisions

---
#### DE‑DI‑010 — Accept Demand Signal

**Purpose:** Decide whether an incoming demand signal is trustworthy enough to incorporate into the demand picture.

**Enterprise Understanding Required:** Signal source reliability, signal timestamp, consistency with recent demand patterns.

**Decision Alternatives:**
- Accept and integrate immediately
- Accept with flag (low confidence)
- Quarantine for manual review
- Reject (spam, duplicate, out‑of‑range)

**Decision Criteria:** Source reliability ≥ threshold, timestamp within allowed latency, value within statistical bounds (e.g., ±3σ of recent demand).

**Recommended Decision:** Determined by rule set.

**Decision Confidence:** Derived from source reliability index and signal consistency score.

**Decision Rationale:** “Signal for Product X accepted because source reliability is 95%, timestamp is 12 min old (within 1‑hour window), and value is within 2σ of recent demand. Rule BR‑DI‑010 confirmed criteria.” (Explainability template applied.)

---

##### Rules (for DE‑DI‑010)

| ID | Rule | Category | Description |
|----|------|----------|-------------|
| BR‑DI‑010 | Signal Timeliness Rule | Validation Rule | Signal timestamp shall not be older than the maximum allowed latency (default: 1 hour for POS, 24 hours for market intelligence). |
| BR‑DI‑011 | Signal Range Rule | Validation Rule | Signal quantity shall not deviate by more than 4 standard deviations from the trailing 4‑week mean unless accompanied by an explanatory event tag. |
| BR‑DI‑012 | Signal Source Reliability Rule | Validation Rule | Signals from sources with reliability index < 60% shall be quarantined for manual review. |

##### Policies (for DE‑DI‑010)

| ID | Policy | Category | Description |
|----|--------|----------|-------------|
| PO‑DI‑010 | Signal Acceptance Automation Policy | Automation Policy | If all validation rules pass and source reliability ≥ 90%, accept automatically. If any rule fails, route to Demand Data Steward. |

---

#### DE‑DI‑011 — Adjust Demand History for Anomaly

**Purpose:** Determine whether a flagged historical demand outlier should be adjusted, and if so, how.

**Enterprise Understanding Required:** Outlier score, root cause (known event, data error, genuine spike), business impact if left unadjusted.

**Decision Alternatives:**
- Leave as‑is (genuine business event)
- Replace with statistical estimate (mean/median of surrounding periods)
- Replace with planner‑provided value
- Exclude from training data but retain in history

**Decision Criteria:** Root cause category, outlier magnitude (σ), recurrence pattern, impact on forecast models.

**Recommended Decision:** Output of rule evaluation.

**Decision Confidence:** Based on root cause certainty and statistical justification.

**Decision Rationale:** “Historical outlier in Week 23 adjusted because the spike was caused by a one‑time system error (confirmed by IT). Value replaced with trailing 4‑week median. Rule BR‑DI‑013 validated the adjustment.” (Template.)

---

##### Rules (for DE‑DI‑011)

| ID | Rule | Category | Description |
|----|------|----------|-------------|
| BR‑DI‑013 | Anomaly Adjustment Justification Rule | Validation Rule | Every adjustment must include a documented root cause and an adjustment method. Adjustments without justification are rejected. |
| BR‑DI‑014 | Adjustment Method Rule | Constraint Rule | If the anomaly is due to data error, replace with median of ±2 weeks. If due to a known event, retain unadjusted but tag as “event”. |

##### Policies (for DE‑DI‑011)

| ID | Policy | Category | Description |
|----|--------|----------|-------------|
| PO‑DI‑011 | Anomaly Adjustment Authorization Policy | Authorization Policy | Adjustments exceeding 50% of the original value require Demand Manager approval. |

---

### 5.1.10 Functional Behaviour

1. **Ingest** demand transactions and signals from all channels.
2. **Validate** each signal via DE‑DI‑010 (Accept Demand Signal) — rules BR‑DI‑010/011/012 and policy PO‑DI‑010 determine acceptance.
3. **Aggregate** accepted signals into time‑bucketed demand series.
4. **Detect** historical outliers using statistical methods (rolling mean, standard deviation).
5. **For each outlier**, execute DE‑DI‑011 (Adjust Demand History) — rules BR‑DI‑013/014 and policy PO‑DI‑011 govern adjustment.
6. **Publish** unified demand history with quality flags and metadata.
7. **Raise events:** `DemandSignalAccepted`, `DemandSignalQuarantined`, `DemandHistoryAdjusted`, `DemandPictureUpdated`.

### 5.1.11 Commands

| Command | Purpose |
|---------|---------|
| `IngestDemandSignals` | Accept a batch of demand signals for processing |
| `AdjustDemandHistory` | Apply an anomaly adjustment to a specified demand bucket |
| `RefreshDemandPicture` | Rebuild the current demand snapshot |

### 5.1.12 Events

| Event | Payload Highlights |
|-------|-------------------|
| `DemandSignalAccepted` | Signal ID, product, location, time bucket, quantity, confidence |
| `DemandSignalQuarantined` | Signal ID, reason, assigned reviewer |
| `DemandHistoryAdjusted` | Product, location, time bucket, original value, new value, reason |
| `DemandPictureUpdated` | Snapshot timestamp, coverage percentage, data quality score |

### 5.1.13 Queries

| Query | Description |
|-------|-------------|
| `GetDemandHistory(product, location, start, end)` | Returns cleansed demand series with flags |
| `GetCurrentDemandSnapshot(filters)` | Returns latest demand snapshot with freshness |
| `GetSignalQualityReport(period)` | Signal acceptance/rejection stats by source |

### 5.1.14 Reports
- **Demand Data Quality Report** – completeness, outlier counts, signal latency
- **Signal Source Performance** – acceptance rate by source

### 5.1.15 Dashboards
- **Demand Health Dashboard** – freshness, completeness, signal coverage

### 5.1.16 Software Realization
Implemented as a pure‑domain service:
```
API (REST) → Application Service → Domain Model (DemandSeries, Signal)
→ Repository → Event Store → Projections → Read Model
```
Outlier detection uses statistical models (rolling Z‑score, IQR). Signal validation is rule‑driven and can be hot‑reloaded.

---

## 5.2 Forecast Demand

### 5.2.1 Purpose
Produce trusted, multi‑horizon demand forecasts with quantified uncertainty to drive enterprise planning. Answers *“What will future demand be, and how certain are we?”* The capability generates time‑series demand forecasts at multiple aggregation levels. Every forecast includes a mean and a 90% prediction interval, along with model performance metadata, enabling downstream processes to plan with explicit awareness of uncertainty.

### 5.2.2 Business Objectives Served
- BO‑DI‑001 (Deliver Trusted Demand Understanding)
- BO‑DI‑002 (Improve Planning Effectiveness)
- BO‑DI‑005 (Increase Planning Automation)
- BO‑DI‑006 (Continuously Improve Enterprise Intelligence)

### 5.2.3 Enterprise Measures
- PI‑DI‑002 (Forecast Accuracy)
- PI‑DI‑003 (WAPE)
- PI‑DI‑004 (MAPE)
- PI‑DI‑005 (Forecast Bias)
- PI‑DI‑007 (Forecast Stability)
- PI‑DI‑103 (Forecast Confidence Index)
- PI‑DI‑107 (Explainability Score)
- PI‑DI‑202 (Forecast Generation Time)

### 5.2.4 Semantic Objects

| ID | Object | Role |
|----|--------|------|
| SE‑DI‑003 | Forecast | Output |
| SE‑DI‑004 | Demand History | Input |
| SE‑DI‑020 | Forecast Model | Model used |
| SE‑DI‑021 | Prediction Interval | Uncertainty quantification |
| SE‑DI‑022 | Forecast Confidence | Confidence score |
| SE‑DI‑023 | Forecast Override | Manual adjustment |
| SE‑DI‑024 | Forecast Cycle | Generation cycle metadata |
| SE‑DI‑025 | Forecast Horizon | Time span |
| SE‑DI‑011 | Demand Pattern | Influences model selection |
| SE‑DI‑012 | Demand Variability | Affects confidence intervals |

### 5.2.5 Primitive Capabilities Composed
- **Understand** – interprets history, pattern, signals
- **Predict** – produces future‑oriented projections **with explicit confidence distributions**
- **Evaluate** – compares model performance
- **Learn** – continuously improves models based on evaluation triggers

### 5.2.6 Enterprise Inputs
- Demand history (cleansed) from Understand Demand capability
- Demand signals: planned promotions and events from Understand Demand; real‑time demand signals from Sense Demand capability
- Product/location master data
- Current champion forecast model(s) and challenger models
- Model performance metrics (WAPE, bias, stability) from Evaluate Demand Quality
- Planning calendar and horizon definition

### 5.2.7 Enterprise Understanding Produced
- Time‑series forecasts for every product–location combination, each containing:
  - Mean forecast quantity
  - 90% prediction interval (lower and upper bounds)
  - Forecast confidence score
- Model performance summaries over recent evaluation windows
- Forecast metadata (model ID, generation timestamp, data freshness)

### 5.2.8 Preconditions
- Demand history data must be fresh (not older than 12 hours for operational forecast)
- Product/location master data must be complete
- At least one champion model must be assigned (initial setup)
- Forecast horizon and time buckets are configured

### 5.2.9 Business Decisions

---
#### DE‑DI‑020 — Select Champion Forecast Model

**Purpose:** Evaluate challenger models against the current champion over a recent evaluation window and decide which model should generate the official forecast.

**Required Understanding:** Model performance metrics (WAPE, bias, stability) over a 4‑week evaluation window; stability of candidate models; statistical significance of differences.

**Decision Alternatives:** Model A, Model B, Model C, Maintain current champion.

**Decision Criteria:** Lowest WAPE, lowest absolute bias, highest stability, no degradation on high‑priority items, statistically significant improvement (p ≤ 0.05).

**Discovered Alternatives:** Not applicable for this decision (candidate models are pre‑registered; however, the Learn capability may propose new model architectures, which would be submitted as a separate “Propose New Model” decision with Alternative Validation Rules).

**Decision Confidence:** Based on statistical significance of the performance difference and evaluation window length.

**Decision Rationale:** “We recommend **Model B** as the new champion because it achieved a WAPE of **8.2%** vs. 9.1% for the current champion over the last 4 weeks, with a 95% confidence that the improvement is not random. Rule BR‑DI‑020 confirms evaluation criteria are met, and no degradation on high‑priority items was detected. Policy PO‑DI‑020 governs the promotion.” (Explainability template.)

---
##### Rules (for DE‑DI‑020)

| ID | Rule | Category | Description |
|----|------|----------|-------------|
| BR‑DI‑020 | Champion Selection Significance Rule | Model Evaluation Rule | A challenger model may replace the champion only if it demonstrates a statistically significant reduction in WAPE (p ≤ 0.05) over a minimum 4‑week evaluation period. |
| BR‑DI‑021 | No Harm Rule | Model Evaluation Rule | The challenger must not increase absolute bias by more than 1 percentage point and must not degrade forecast stability by more than 5 percentage points. |
| BR‑DI‑022 | High‑Priority Protection Rule | Consistency Rule | On products classified as high‑priority (from Prioritize Demand), the challenger must not show a WAPE increase exceeding 2 percentage points. |

##### Policies (for DE‑DI‑020)

| ID | Policy | Category | Description |
|----|--------|----------|-------------|
| PO‑DI‑020 | Champion Promotion Approval Policy | Approval Policy | Automatic promotion is permitted if all Model Evaluation Rules pass. If any rule fails, the model selection is routed to a Demand Manager for manual approval. |
| PO‑DI‑021 | Model Rollback Policy | Exception Policy | If within two weeks of promotion the new champion causes a service‑level drop >2% (attributable to forecast), the Demand Manager may rollback to the previous champion without further approval. |

---
#### DE‑DI‑021 — Generate Baseline Forecast

**Purpose:** Produce the initial statistical forecast before any judgmental overrides are applied.

**Required Understanding:** Cleaned demand history, demand signals, selected champion model, calendar.

**Decision Alternatives:**
- Run model with default configuration
- Run model with adjusted parameters (if drift detected)
- Flag series as “unforecastable” due to insufficient history or extreme volatility
- Rule: BR‑DI‑109

**Decision Criteria:** Model must converge, forecast must be within historical bounds (no negative forecasts), data completeness ≥ 95%.

**Recommended Decision:** Determined by rule validation.

**Decision Confidence:** The model’s inherent confidence score (from prediction intervals).

**Decision Rationale:** “Baseline forecast generated using Model B for 98% of series. 2% flagged as unforecastable (new products with <4 periods of history). Rule BR‑DI‑023 passed.” (Template.)

---
##### Rules (for DE‑DI‑021)

| ID | Rule | Category | Description |
|----|------|----------|-------------|
| BR‑DI‑023 | Forecast Validity Rule | Validation Rule | All forecast mean values must be non‑negative. A forecast with any negative bucket is rejected and the series flagged. |
| BR‑DI‑024 | Data Sufficiency Rule | Validation Rule | A minimum of 8 periods of demand history is required to generate a statistical forecast. Series with insufficient history are flagged as “unforecastable”. |
| BR‑DI‑025 | Prediction Interval Completeness Rule | Validation Rule | Every forecast must include a 90% prediction interval. Missing intervals cause the forecast to be marked incomplete. |

##### Policies (for DE‑DI‑021)

| ID | Policy | Category | Description |
|----|--------|----------|-------------|
| PO‑DI‑022 | Unforecastable Series Handling Policy | Exception Policy | Series flagged as unforecastable are automatically assigned a planner‑provided placeholder or a naive forecast, depending on product life‑cycle stage. |

---
#### DE‑DI‑022 — Publish Forecast

**Purpose:** Approve the final forecast (after overrides) for release to downstream planning domains.

**Required Understanding:** Final forecast (baseline + approved overrides), Forecast Confidence Index, data freshness status, exception flags.

**Decision Alternatives:** Publish automatically, Request planner approval, Suppress publication (with reason).

**Decision Criteria:** Forecast Confidence Index ≥ 90% → automatic; completeness check passed; no critical exceptions.

**Recommended Decision:** Determined by Automation Policy PO‑DI‑023.

**Decision Confidence:** Directly derived from Forecast Confidence Index.

**Decision Rationale:** “Forecast published automatically because Forecast Confidence Index is 93%, exceeding the 90% threshold defined in PO‑DI‑023, and data completeness is 100%.” (Template.)

---
##### Rules (for DE‑DI‑022)

| ID | Rule | Category | Description |
|----|------|----------|-------------|
| BR‑DI‑026 | Forecast Completeness for Publication Rule | Validation Rule | A forecast cycle may only be published if ≥ 95% of all mandatory product–location combinations have a valid forecast. |

##### Policies (for DE‑DI‑022)

| ID | Policy | Category | Description |
|----|--------|----------|-------------|
| PO‑DI‑023 | Forecast Auto‑Publication Policy | Automation Policy | If Forecast Confidence Index ≥ 90% and completeness rule (BR‑DI‑026) passes, publish automatically. Otherwise, route to Demand Planner for approval. |
| PO‑DI‑024 | Publication Override Policy | Authorization Policy | A Demand Manager may override the auto‑publication decision and force publication or suppression with a documented business reason. |

---
#### DE‑DI‑023 — Override Forecast

**Purpose:** Allow a planner to replace a system‑generated forecast when they possess business knowledge not yet reflected in the demand signals.

**Required Understanding:** System forecast with confidence interval, planner‑provided override value and justification, impact analysis on service level/inventory.

**Decision Alternatives:** Keep system forecast, Accept override, Request revised override.

**Decision Criteria:** Override justification is non‑empty; override value is within allowed deviation range (default ±50% of system forecast); policy authorization check passes.

**Recommended Decision:** Accept if policy checks pass; otherwise reject or request revision.

**Decision Confidence:** Marked lower when override is applied.

**Decision Rationale:** “Forecast for Product X overridden to 500 units because planner provided a business reason: ‘Confirmed large one‑time order not yet in demand signals’. Policy PO‑DI‑025 permits this override.” (Template.)

---
##### Rules (for DE‑DI‑023)

| ID | Rule | Category | Description |
|----|------|----------|-------------|
| BR‑DI‑027 | Override Justification Rule | Validation Rule | Every override must contain a non‑empty business justification. Overrides without justification are rejected. |
| BR‑DI‑028 | Override Deviation Limit Rule | Constraint Rule | The override value must not deviate from the system forecast mean by more than ±50% unless an exception policy is invoked. |

##### Policies (for DE‑DI‑023)

| ID | Policy | Category | Description |
|----|--------|----------|-------------|
| PO‑DI‑025 | Forecast Override Authorization Policy | Authorization Policy | Only users in the Demand Planner role may submit an override. Overrides exceeding the deviation limit require Demand Manager approval. |
| PO‑DI‑026 | Override Audit Policy | Compliance Policy | All overrides, including justifications, are logged and subject to quarterly review to detect planner bias. |

---
### 5.2.10 Functional Behaviour

1. **Trigger:** Scheduled (e.g., nightly) or event‑driven (new demand signal batch from Sense Demand).
2. **Retrieve** cleansed demand history, signals, and current champion model.
3. **Evaluate challenger models** (if parallel runs completed) and execute **DE‑DI‑020** (Select Champion) — rules BR‑DI‑020/021/022, policies PO‑DI‑020/021.
4. **Generate baseline forecast** using champion model → **DE‑DI‑021** (Generate Baseline) — rules BR‑DI‑023/024/025, policy PO‑DI‑022 handles unforecastable series.
5. **Allow overrides:** Planners submit overrides → **DE‑DI‑023** (Override Forecast) — rules BR‑DI‑027/028, policies PO‑DI‑025/026.
6. **Publish forecast:** Execute **DE‑DI‑022** (Publish Forecast) — rule BR‑DI‑026, policies PO‑DI‑023/024.
7. **Raise events:** `ForecastCycleStarted`, `ForecastGenerated`, `ForecastOverridden`, `ForecastPublished`, `ForecastApprovalRequired`.
8. **Feed** forecast performance data back to Evaluate Demand Quality capability.

“Challenger models are executed asynchronously by the Model Training Pipeline on the same historical data after each forecast cycle. Their results are stored and available for DE‑DI‑020 evaluation.”
### 5.2.11 Commands

| Command | Purpose |
|---------|---------|
| `StartForecastCycle` | Initiates a new forecasting cycle |
| `SelectChampionModel` | Evaluates challengers and selects new champion |
| `OverrideForecast` | Replaces a specific forecast with a planner‑supplied value |
| `ApproveForecastPublication` | Manual approval of a forecast that did not meet auto‑publication criteria |
| `PublishForecast` | Releases the approved forecast to downstream domains |

### 5.2.12 Events

| Event | Payload Highlights |
|-------|-------------------|
| `ForecastCycleStarted` | Cycle ID, timestamp, horizon |
| `ForecastGenerated` | Product, location, time bucket, mean, lower/upper interval, model ID, confidence |
| `ForecastOverridden` | Forecast ID, old value, new value, planner ID, reason |
| `ForecastPublished` | Cycle ID, published timestamp, version |
| `ForecastApprovalRequired` | Cycle ID, reason (low confidence, incomplete) |
| `ModelChampionSelected` | New model ID, old model ID, evaluation metrics |

### 5.2.13 Queries

| Query | Description |
|-------|-------------|
| `GetForecast(product, location, startDate, endDate)` | Current forecast with intervals |
| `GetModelPerformance(modelId, period)` | WAPE, bias, stability |
| `GetForecastOverrides(cycleId)` | List of overrides with reasons |
| `GetPublicationStatus(cycleId)` | Published / Pending / Suppressed |

### 5.2.14 Reports
- **Forecast Accuracy Report** – WAPE, MAPE, bias by product family
- **Model Champion Report** – performance comparison of champion vs. challengers
- **Override Analysis Report** – override frequency, bias trends

### 5.2.15 Dashboards
- **Forecast Performance Dashboard** – PI‑DI‑002, PI‑DI‑003, PI‑DI‑005 drill‑down
- **Forecast Confidence Dashboard** – distribution of confidence scores, auto‑ vs. manual publication rates
- **Planner Override Monitor** – override counts, justification quality

### 5.2.16 Software Realization
Implemented as an event‑driven domain service with pluggable forecasting engines:
```
API → Application Service (ForecastCycle, Forecast, Override aggregates)
→ Domain Model (stateless forecasting functions, uncertainty quantifier)
→ Event Store → Projections → Read Model
```
The `Predict` primitive is backed by a model registry that can run multiple model types (statistical, ML). Models must expose a standard interface returning a probability distribution. Model evaluation is triggered automatically when a challenger has completed a full back‑test, and results are compared using the defined Model Evaluation Rules.

We continue with the next capabilities, applying the same depth and decision‑centric structure, ensuring every element is fully specified and traceable.

---

## 5.3 Sense Demand

### 5.3.1 Purpose
Continuously monitor the enterprise environment to detect new demand signals, short‑term changes, and emerging patterns as early as possible. Answers *“What is changing in demand right now, and which changes require immediate attention?”* The capability provides real‑time situational awareness that can trigger forecast refreshes, exception alerts, and planner interventions.

### 5.3.2 Business Objectives Served
- BO‑DI‑003 (Improve Enterprise Responsiveness)
- BO‑DI‑001 (Deliver Trusted Demand Understanding)

### 5.3.3 Enterprise Measures
- PI‑DI‑009 (Demand Change Detection Time)
- PI‑DI‑010 (Exception Response Time)
- PI‑DI‑110 (Exception Detection Accuracy)
- PI‑DI‑111 (Exception Prediction Accuracy)
- PI‑DI‑203 (Demand Refresh Latency)
- PI‑DI‑208 (Event Processing Latency)

### 5.3.4 Semantic Objects

| ID | Object | Role |
|----|--------|------|
| SE‑DI‑002 | Demand Signal | Input data |
| SE‑DI‑015 | Demand Exception | Output if anomalous |
| SE‑DI‑011 | Demand Pattern | Current pattern for baseline |
| SE‑DI‑050 | Time Bucket | Granularity |
| SE‑DI‑030 | Customer | Source dimension |
| SE‑DI‑040 | Product | Product dimension |
| SE‑DI‑010 | Demand Quantity | Measured change |

### 5.3.5 Primitive Capabilities Composed
- **Observe** – continuous ingestion of streaming signals
- **Understand** – compares signals against historical norms
- **Assess** – determines significance of deviation
- **Predict** – (optional) short‑horizon extrapolation to confirm trend

### 5.3.6 Enterprise Inputs
- Streaming demand signals: POS, e‑commerce clicks, orders, social sentiment, weather, IoT
- Cleansed demand history from Understand Demand
- Current demand pattern classification from Classify Demand
- Pre‑defined thresholds for change detection

### 5.3.7 Enterprise Understanding Produced
- Real‑time demand change alerts with magnitude, direction, and affected product/location
- Short‑term demand projections (1–7 days) updated continuously
- Signal‑to‑noise assessments (is the change real or noise?)
- Demand velocity indicators (rate of change)
- Anomaly scores per product/location

### 5.3.8 Preconditions
- Signal ingestion pipeline operational with latency ≤ 5 minutes for critical signals
- Baseline demand history available for comparison
- Threshold parameters configured for each product category

### 5.3.9 Business Decisions

---
#### DE‑DI‑030 — Detect Demand Change

**Purpose:** Determine whether a deviation from expected demand constitutes a meaningful change requiring action.

**Required Understanding:** Latest demand signal(s), historical baseline (30‑day rolling average and standard deviation), current demand pattern, recent trend.

**Decision Alternatives:**
- No change (within normal variation)
- Minor deviation (log for later analysis, no immediate action)
- Significant change (raise alert, trigger downstream actions)
- Critical change (immediate escalation)

**Discovered Alternatives:** In some cases, the system may identify a completely new demand pattern not previously classified. This triggers a “Propose New Pattern” alternative, which is then validated by an Alternative Validation Rule.

**Decision Criteria:** Deviation magnitude in standard deviations (σ), persistence over consecutive periods, consistency across multiple signals, business impact (e.g., high‑priority customer affected).

**Recommended Decision:** Determined by rules and thresholds.

**Decision Confidence:** Based on signal quality, number of corroborating signals, and statistical significance.

**Decision Rationale:** “Demand change detected for Product X: 7‑day moving average increased by 3.2σ over baseline, confirmed by POS and web traffic signals. Rule BR‑DI‑030 triggered significant change alert. Confidence: 92%.” (Explainability template.)

---
##### Rules (for DE‑DI‑030)

| ID | Rule | Category | Description |
|----|------|----------|-------------|
| BR‑DI‑030 | Deviation Threshold Rule | Validation Rule | A demand change is classified as Significant if the deviation exceeds 2.5σ for ≥ 3 consecutive periods. Critical if ≥ 4σ and sustained for 2 periods. |
| BR‑DI‑031 | Signal Corroboration Rule | Consistency Rule | A change shall be corroborated by at least two independent signal sources (e.g., POS and orders) before triggering a Critical alert. |
| BR‑DI‑032 | High‑Priority Product Sensitivity Rule | Validation Rule | For products classified as high‑priority (from Prioritize Demand), the significant change threshold is lowered to 2.0σ. |
| BR‑DI‑033 | Noise Filter Rule | Validation Rule | If the absolute deviation is less than 1.5σ and the product’s demand pattern is Lumpy, the change is classified as Noise and suppressed. |

##### Policies (for DE‑DI‑030)

| ID | Policy | Category | Description |
|----|--------|----------|-------------|
| PO‑DI‑030 | Alert Escalation Policy | Authorization Policy | Significant alerts are routed to the Demand Planner. Critical alerts are routed to the Demand Manager and trigger a mandatory forecast refresh. |
| PO‑DI‑031 | Automatic Forecast Refresh Policy | Automation Policy | A Critical change automatically triggers a new forecast cycle for the affected product/location. |

---
#### DE‑DI‑031 — Trigger Forecast Refresh

**Purpose:** Decide whether a detected change warrants an immediate forecast regeneration outside the normal cycle.

**Required Understanding:** Demand Change Decision output (DE‑DI‑030), current forecast age, business calendar (e.g., near a planning deadline).

**Decision Alternatives:**
- No refresh (wait for next scheduled cycle)
- Immediate partial refresh (affected products only)
- Immediate full refresh (all products)

**Decision Criteria:** Change severity (Significant/Critical), forecast age > 4 hours, proximity to planning freeze.

**Recommended Decision:** Determined by policy.

**Decision Confidence:** Inherited from DE‑DI‑030.

**Decision Rationale:** “Forecast refresh triggered for Product X due to Critical demand change detected at 09:32. Next scheduled cycle is 18:00; immediate partial refresh reduces risk.” (Template.)

---
##### Rules (for DE‑DI‑031)

| ID | Rule | Category | Description |
|----|------|----------|-------------|
| BR‑DI‑034 | Refresh Benefit Rule | Validation Rule | A refresh is only triggered if the expected improvement in forecast accuracy (estimated from historical change‑refresh correlation) exceeds 2% WAPE reduction. |

##### Policies (for DE‑DI‑031)

| ID | Policy | Category | Description |
|----|--------|----------|-------------|
| PO‑DI‑032 | Refresh Authorization Policy | Authorization Policy | Partial refreshes for Significant changes are executed automatically. Full refreshes require Demand Manager approval unless the change is Critical. |

---
#### DE‑DI‑032 — Accept Streaming Signal

**Purpose:** Validate and incorporate real‑time signals into the short‑term demand picture, similar to DE‑DI‑010 but optimized for low‑latency streaming.

**Required Understanding:** Signal metadata (source, timestamp, value), source reliability, current demand state.

**Decision Alternatives:** Accept, Accept with low confidence flag, Discard (noise/duplicate).

**Decision Criteria:** Source reliability ≥ 70%, latency within defined bounds, value within statistical range (3σ), no duplicate.

**Recommended Decision:** Automated via rules.

**Decision Confidence:** Source reliability score.

**Decision Rationale:** “Signal accepted: POS transaction for Product Y, 2 min latency, within 1.2σ, source reliability 98%.” (Template.)

---
##### Rules (for DE‑DI‑032)

| ID | Rule | Category | Description |
|----|------|----------|-------------|
| BR‑DI‑035 | Streaming Signal Latency Rule | Validation Rule | Signals must have a timestamp ≤ 15 minutes old for POS, ≤ 1 hour for social sentiment. |
| BR‑DI‑036 | Streaming Signal Range Rule | Validation Rule | Signal value outside 3σ is flagged for review but still accepted with a low‑confidence flag unless also identified as duplicate. |
| BR‑DI‑037 | Duplicate Detection Rule | Validation Rule | A signal is discarded if its fingerprint (source, product, location, time bucket, value) matches an already‑processed signal within the same hour. |

##### Policies (for DE‑DI‑032)

| ID | Policy | Category | Description |
|----|--------|----------|-------------|
| PO‑DI‑033 | Streaming Signal Acceptance Policy | Automation Policy | Signals meeting all validation rules are automatically accepted. Low‑confidence flagged signals are batched and presented to a Data Steward every hour. |

---
### 5.3.10 Functional Behaviour

1. **Ingest** continuous signal streams via event bus / API.
2. **Pre‑process** signals: deduplication (BR‑DI‑037), latency check (BR‑DI‑035), range check (BR‑DI‑036). Execute DE‑DI‑032 per signal.
3. **Aggregate** accepted signals into time‑bucketed short‑term demand series.
4. **Continuously compare** updated demand against rolling baseline.
5. **Execute DE‑DI‑030** (Detect Demand Change) whenever deviation exceeds threshold.
6. **If change Significant/Critical**, execute DE‑DI‑031 (Trigger Forecast Refresh).
7. **Publish** change alerts to event bus; notify planners per policies.
8. **Log** all detected changes and refreshes for learning (Learn From Demand).
9. **Events raised:** `DemandChangeDetected`, `ForecastRefreshTriggered`, `SignalAccepted`, `SignalDiscarded`.

### 5.3.11 Commands

| Command | Purpose |
|---------|---------|
| `IngestSignalStream` | Accept a batch of real‑time signals |
| `EvaluateDemandDeviation` | Manually trigger a change detection run |
| `TriggerForecastRefresh` | Force a forecast refresh for given scope |

### 5.3.12 Events

| Event | Payload Highlights |
|-------|-------------------|
| `DemandChangeDetected` | Product, location, severity, magnitude (σ), confidence, timestamp |
| `ForecastRefreshTriggered` | Scope (product list), reason (change ID) |
| `SignalAccepted` | Signal ID, source, latency, value, confidence flag |
| `SignalDiscarded` | Signal ID, reason (duplicate, range, latency) |

### 5.3.13 Queries

| Query | Description |
|-------|-------------|
| `GetCurrentDemandDeviation(product, location)` | Latest deviation from baseline in σ |
| `GetActiveAlerts()` | List of current Significant/Critical alerts |
| `GetSignalHistory(product, location, period)` | Accepted signal stream for forensic analysis |

### 5.3.14 Reports
- **Change Detection Report** – frequency, severity, and accuracy of detected changes
- **Signal Quality Report** – latency, acceptance rate by source

### 5.3.15 Dashboards
- **Real‑Time Demand Dashboard** – live deviation gauges, alert feed
- **Signal Health Monitor** – signal ingestion latency, source reliability

### 5.3.16 Software Realization
Streaming architecture using event‑sourcing and CQRS:
```
Signal Ingestion API → Kafka / Event Bus → Stream Processor (windowed aggregations)
→ Domain Service (DeviationDetector) → Alert Publisher
→ Read Model (real‑time view)
```
Stateful stream processing computes rolling statistics (mean, σ) with low latency. The Domain Service implements the decision logic, referencing current thresholds from a policy store.

---

## 5.4 Segment Demand

### 5.4.1 Purpose
Partition the enterprise’s products and customers into homogeneous groups based on demand characteristics (volume, variability, strategic importance) so that each segment can be planned, forecast, and managed with appropriate attention and methods. Answers *“Which groups of demand behave similarly and should be treated similarly?”*

### 5.4.2 Business Objectives Served
- BO‑DI‑001 (Deliver Trusted Demand Understanding)
- BO‑DI‑002 (Improve Planning Effectiveness)
- BO‑DI‑005 (Increase Planning Automation)

### 5.4.3 Enterprise Measures
- PI‑DI‑112 (Demand Segmentation Quality)
- PI‑DI‑106 (Recommendation Acceptance Rate)
- PI‑DI‑109 (Demand Intelligence Coverage Index)

### 5.4.4 Semantic Objects

| ID | Object | Role |
|----|--------|------|
| SE‑DI‑013 | Demand Segmentation | The concept itself |
| SE‑DI‑014 | Demand Priority | Some segments may be prioritized |
| SE‑DI‑011 | Demand Pattern | Used in XYZ classification |
| SE‑DI‑012 | Demand Variability | Key for XYZ |
| SE‑DI‑010 | Demand Quantity | For volume (ABC) |
| SE‑DI‑040 | Product | Entity to segment |
| SE‑DI‑030 | Customer | Entity to segment |

### 5.4.5 Primitive Capabilities Composed
- **Understand** – uses historical demand to compute segmentation parameters
- **Assess** – evaluates segment stability and separation
- **Learn** – periodically re‑evaluates and suggests new segment boundaries

### 5.4.6 Enterprise Inputs
- Cleansed demand history (volume by product, customer)
- Product and customer master data (strategic classification, lifecycle)
- Current segmentation parameters and rules
- Business‑defined segment definitions (e.g., ABC thresholds)

### 5.4.7 Enterprise Understanding Produced
- For each product and customer, assigned segment labels (e.g., A, B, C for volume; X, Y, Z for variability)
- Segment profiles: average volume, CV, service level targets
- Segmentation quality metrics: segment homogeneity, stability, separation
- Recommendations for segment transitions (e.g., product moved from B to A)

### 5.4.8 Preconditions
- Minimum 12 periods of demand history for variability analysis
- ABC/XYZ thresholds defined by enterprise policy
- Strategic classification attributes available in master data

### 5.4.9 Business Decisions

---
#### DE‑DI‑040 — Assign ABC Segment (Volume)

**Purpose:** Classify products/customers into A, B, C categories based on their contribution to total demand volume (Pareto principle).

**Required Understanding:** Demand history (revenue or units) over a representative period (typically 12 months), cumulative share.

**Decision Alternatives:** A (top 80%), B (next 15%), C (bottom 5%), or custom enterprise thresholds.

**Decision Criteria:** Thresholds defined by policy. Typically, cumulative demand share cutoffs.

**Recommended Decision:** Computed automatically.

**Decision Confidence:** High if data sufficiency is met.

**Decision Rationale:** “Product X classified as A: contributes 12% of total revenue, cumulative share 78% within top 80% cutoff. Rule BR‑DI‑040 applied.” (Template.)

---
##### Rules (for DE‑DI‑040)

| ID | Rule | Category | Description |
|----|------|----------|-------------|
| BR‑DI‑040 | ABC Classification Rule | Derivation Rule | Products are sorted descending by demand volume. A = first items whose cumulative share ≤ X% (default 80%). B = next items up to Y% (default 95%). C = remainder. X, Y configurable. |
| BR‑DI‑041 | Minimum History for ABC Rule | Validation Rule | A product must have at least 6 months of demand history to be ABC classified; otherwise, it is assigned “Unclassified” and flagged for review. |
| BR‑DI‑042 | Re‑classification Stability Rule | Consistency Rule | A product shall not change segment more than once in a 3‑month period unless a strategic reclassification is manually approved. |

##### Policies (for DE‑DI‑040)

| ID | Policy | Category | Description |
|----|--------|----------|-------------|
| PO‑DI‑040 | ABC Override Policy | Authorization Policy | Only a Demand Manager may manually override an automated ABC classification. Override requires a documented business case. |

---
#### DE‑DI‑041 — Assign XYZ Segment (Variability)

**Purpose:** Classify products/customers by demand variability (coefficient of variation, CV) to determine forecastability and inventory requirements.

**Required Understanding:** Demand history (at least 12 periods), computed CV.

**Decision Alternatives:** X (CV ≤ 0.5), Y (0.5 < CV ≤ 1.0), Z (CV > 1.0), or custom thresholds.

**Decision Criteria:** CV thresholds defined by policy.

**Recommended Decision:** Automated.

**Decision Confidence:** High with sufficient history; lower for intermittent items.

**Decision Rationale:** “Product Y classified as Z: CV = 1.4, exceeding 1.0 threshold. Demand is highly variable; forecast method adjusted.” (Template.)

---
##### Rules (for DE‑DI‑041)

| ID | Rule | Category | Description |
|----|------|----------|-------------|
| BR‑DI‑043 | XYZ Classification Rule | Derivation Rule | CV computed as standard deviation / mean over historical period. X if CV ≤ 0.5, Y if 0.5 < CV ≤ 1.0, Z if CV > 1.0. For intermittent demand (ADU < 2), a modified CV using non‑zero periods only may be used as per policy. |
| BR‑DI‑044 | Minimum Data for XYZ Rule | Validation Rule | At least 12 periods of history required. If fewer, product is “Unclassified”. |

##### Policies (for DE‑DI‑041)

| ID | Policy | Category | Description |
|----|--------|----------|-------------|
| PO‑DI‑041 | XYZ Override Policy | Authorization Policy | Same as ABC Override Policy, applied to XYZ. Only Demand Manager may override. |

---
#### DE‑DI‑042 — Assign Strategic Segment

**Purpose:** Overlay a strategic classification based on business importance (e.g., margin, customer tier, contractual criticality) that can override statistical segments.

**Required Understanding:** Product/customer attributes from master data (profit margin, customer tier, contractual obligations).

**Decision Alternatives:** Assign to pre‑defined strategic segments (e.g., Gold, Silver, Bronze) or use default.

**Decision Criteria:** Attribute‑based rules.

**Recommended Decision:** Automated from master data.

**Decision Confidence:** High if master data is reliable.

**Decision Rationale:** “Customer C100 assigned to Gold segment due to Platinum service tier and >15% profit margin.” (Template.)

---
##### Rules (for DE‑DI‑042)

| ID | Rule | Category | Description |
|----|------|----------|-------------|
| BR‑DI‑045 | Strategic Classification Rule | Derivation Rule | If customer tier = Platinum and product margin ≥ 10%, assign Gold. If customer tier = Gold and product is critical (master flag), assign Silver. All others Bronze. |
| BR‑DI‑046 | Master Data Consistency Rule | Validation Rule | If strategic attributes are missing, the item is assigned to a “Data Incomplete” segment and planner is notified. |

##### Policies (for DE‑DI‑042)

| ID | Policy | Category | Description |
|----|--------|----------|-------------|
| PO‑DI‑042 | Strategic Segment Maintenance Policy | Compliance Policy | Strategic segment attributes must be reviewed quarterly by Product Management and Customer Service. |

---
#### DE‑DI‑043 — Publish Segment Master

**Purpose:** Finalize and distribute the segment assignments for use by other capabilities (forecast model selection, inventory policies, prioritization).

**Required Understanding:** Aggregated segments from DE‑DI‑040, 041, 042, combined into a unified segment label (e.g., A‑X‑Gold).

**Decision Alternatives:** Publish as final, Publish with warnings (data issues), Hold for review.

**Decision Criteria:** Completeness of classification, no conflicting overrides.

**Recommended Decision:** Automated if completeness ≥ 95%.

**Decision Confidence:** Overall segment coverage percentage.

**Decision Rationale:** “Segment master published: 98% of active products classified. 2% unclassified due to insufficient history, flagged for manual assignment.” (Template.)

---
##### Rules (for DE‑DI‑043)

| ID | Rule | Category | Description |
|----|------|----------|-------------|
| BR‑DI‑047 | Segment Completeness Rule | Validation Rule | Publish only if ≥ 95% of all active products have a complete segment assignment (ABC, XYZ, Strategic). Below 95%, publication is held. |

##### Policies (for DE‑DI‑043)

| ID | Policy | Category | Description |
|----|--------|----------|-------------|
| PO‑DI‑043 | Segment Master Publication Policy | Automation Policy | If completeness ≥ 95% and no conflicting overrides exist, publish automatically. Otherwise, require Demand Manager approval. |

---
### 5.4.10 Functional Behaviour

1. **Scheduled job** (monthly, or triggered by significant master data changes).
2. **Retrieve** cleansed demand history and product/customer master.
3. **Compute** ABC classification (DE‑DI‑040) for all products and/or customers.
4. **Compute** XYZ classification (DE‑DI‑041) using CV.
5. **Compute** Strategic Segment (DE‑DI‑042) using master data rules.
6. **Combine** into unified segment label.
7. **Check** stability rules (BR‑DI‑042) and completeness (BR‑DI‑047).
8. **Publish** segment master via DE‑DI‑043.
9. **Events:** `SegmentMasterPublished`, `SegmentAssignmentChanged`, `SegmentOverrideCreated`.
10. **Provide** segment data to other capabilities (Forecast Demand for model selection, Prioritize Demand for initial priority).

### 5.4.11 Commands

| Command | Purpose |
|---------|---------|
| `RunSegmentationCycle` | Start a full segmentation run |
| `OverrideSegment` | Manually set a segment for a specific item |
| `PublishSegmentMaster` | Finalize and release segment assignments |

### 5.4.12 Events

| Event | Payload Highlights |
|-------|-------------------|
| `SegmentMasterPublished` | Version, completeness %, timestamp |
| `SegmentAssignmentChanged` | Item ID, old segment, new segment, reason |
| `SegmentOverrideCreated` | Item ID, overridden value, user, reason |

### 5.4.13 Queries

| Query | Description |
|-------|-------------|
| `GetSegment(productId)` | Returns full segment details |
| `GetSegmentDistribution()` | Breakdown of counts by segment |
| `GetSegmentHistory(productId)` | Changes over time |

### 5.4.14 Reports
- **Segmentation Distribution Report** – ABC, XYZ, strategic counts
- **Segment Migration Report** – movement of items between segments
- **Unclassified Products Report** – products with missing classification

### 5.4.15 Dashboards
- **Segmentation Overview Dashboard** – pie charts of ABC, XYZ, strategic
- **Segment Health Dashboard** – stability metrics, data completeness

### 5.4.16 Software Realization
Batch processing with rule engine:
```
API → Application Service → Domain Model (SegmentationRun, SegmentAssignment)
→ Rule Engine (Drools or similar) for classification rules
→ Repository → Event Store → Projections → Read Model
```
Rules for ABC/XYZ thresholds and strategic mapping are stored in a configuration store and can be hot‑updated. The domain aggregates ensure concurrency control when overrides are applied.

---

## 5.5 Classify Demand

### 5.5.1 Purpose
Assign a demand pattern classification to every product–location combination based on statistical properties of the historical demand time series. Answers *“What type of demand behaviour does this item exhibit, and which forecasting approach is best suited?”* The classification guides model selection, exception detection thresholds, and safety‑stock policies, ensuring that each demand stream is treated according to its inherent characteristics.

### 5.5.2 Business Objectives Served
- BO‑DI‑001 (Deliver Trusted Demand Understanding)
- BO‑DI‑002 (Improve Planning Effectiveness)
- BO‑DI‑005 (Increase Planning Automation)

### 5.5.3 Enterprise Measures
- PI‑DI‑113 (Demand Classification Accuracy)
- PI‑DI‑112 (Demand Segmentation Quality) — indirectly, as classification complements segmentation

### 5.5.4 Semantic Objects

| ID | Object | Role |
|----|--------|------|
| SE‑DI‑011 | Demand Pattern | The output classification |
| SE‑DI‑012 | Demand Variability | Used to distinguish intermittent/lumpy |
| SE‑DI‑004 | Demand History | Input time series |
| SE‑DI‑050 | Time Bucket | Granularity |
| SE‑DI‑010 | Demand Quantity | Underlying data |
| SE‑DI‑015 | Demand Exception | May be triggered by mis‑classification |

### 5.5.5 Primitive Capabilities Composed
- **Understand** – computes statistical features from history
- **Assess** – applies classification rules to determine pattern
- **Learn** – periodically re‑evaluates classification and suggests updates

### 5.5.6 Enterprise Inputs
- Cleansed demand history (at least 12 periods, ideally 24+)
- Existing segment information (ABC/XYZ) from Segment Demand
- Pre‑defined classification rules and thresholds

### 5.5.7 Enterprise Understanding Produced
- A demand pattern label for each product–location: Continuous, Intermittent, Lumpy, Seasonal, Trend, Stationary (or combinations such as Seasonal‑Trend)
- Confidence score for the classification
- Metadata: periodicity of seasonality, trend strength, intermittency rate
- Recommendations for appropriate forecasting model types

### 5.5.8 Preconditions
- Sufficient demand history (≥ 12 periods for basic classification; ≥ 24 for seasonal detection)
- Demand history is cleansed and free of unadjusted outliers

### 5.5.9 Business Decisions

---
#### DE‑DI‑050 — Classify Demand Pattern

**Purpose:** Assign a statistical demand pattern classification to a demand series based on historical behaviour.

**Required Understanding:** Time series of demand quantities, statistical measures (mean, variance, autocorrelation, zero‑demand frequency, seasonal indices, trend slope).

**Decision Alternatives:**
- Continuous — regular demand with low variability, few zeros
- Intermittent — frequent zeros, irregular spacing
- Lumpy — high variability with irregular intervals
- Seasonal — recurring calendar‑driven patterns
- Trend — sustained directional movement
- Stationary — stable mean, no trend or seasonality
- Composite — e.g., Seasonal‑Trend, Intermittent‑Seasonal

**Discovered Alternatives:** None in normal operation; if a new pattern emerges that does not fit existing categories, the Learn capability may propose a new category (validated by Alternative Validation Rule).

**Decision Criteria:** Rules‑based: percentage of zero periods, coefficient of variation, significance of autocorrelation at seasonal lags, trend test (Mann‑Kendall or similar), decomposition metrics.

**Recommended Decision:** Automated via rule engine.

**Decision Confidence:** Derived from statistical significance of features, length of history, and fit of pattern to data.

**Decision Rationale:** “Product X classified as Seasonal‑Trend: significant autocorrelation at lag 12 (p<0.01), Mann‑Kendall trend p=0.02, CV=0.8. Rule BR‑DI‑050 applied, confidence 94%.” (Explainability template.)

---
##### Rules (for DE‑DI‑050)

| ID | Rule | Category | Description |
|----|------|----------|-------------|
| BR‑DI‑050 | Demand Pattern Classification Rule | Derivation Rule | Classify using defined statistical criteria: (1) If zero demand >50% of periods → Intermittent; (2) If CV >1.5 and zero periods irregular → Lumpy; (3) Apply seasonal decomposition; if seasonal component explains >20% variance → add Seasonal; (4) Mann‑Kendall trend test p<0.05 → add Trend; (5) Else → Stationary or Continuous based on CV <0.5. Exact thresholds configurable. |
| BR‑DI‑051 | Minimum History for Pattern Rule | Validation Rule | At least 24 periods of non‑zero demand are required for seasonal classification. If fewer, a seasonal flag is not applied and classification defaults to the best non‑seasonal category. |
| BR‑DI‑052 | Re‑classification Stability Rule | Consistency Rule | A pattern classification shall not change more than once in a rolling 6‑month window unless a structural break is detected (via level shift test). |

##### Policies (for DE‑DI‑050)

| ID | Policy | Category | Description |
|----|--------|----------|-------------|
| PO‑DI‑050 | Pattern Override Policy | Authorization Policy | Only a Demand Planning Manager may manually override a pattern classification. Overrides require a business justification and are recorded for audit. |

---
#### DE‑DI‑051 — Recommend Forecasting Model

**Purpose:** Based on the classified demand pattern and other attributes (segment, horizon), recommend the most appropriate forecasting model type.

**Required Understanding:** Demand pattern (from DE‑DI‑050), segment (ABC/XYZ), forecast horizon.

**Decision Alternatives:** Exponential Smoothing, ARIMA, Croston, Neural Network, Prophet, Hybrid, Judgemental, etc.

**Decision Criteria:** Mapping table from pattern/segment to model type, configurable. Preference for simpler model if performance is equivalent.

**Recommended Decision:** Automated via model assignment rules.

**Decision Confidence:** Based on historical performance of the recommended model for similar patterns.

**Decision Rationale:** “Model ‘Holt‑Winters’ recommended for Product X due to Seasonal‑Trend pattern, B‑X segment, horizon 13 weeks. Default mapping rule BR‑DI‑053 applied.” (Template.)

---
##### Rules (for DE‑DI‑051)

| ID | Rule | Category | Description |
|----|------|----------|-------------|
| BR‑DI‑053 | Model Recommendation Mapping Rule | Derivation Rule | A configuration‑driven mapping assigns model types: Intermittent → Croston; Lumpy → Neural Network with weather inputs; Seasonal → Holt‑Winters or Prophet; Trend → ARIMA; Stationary → Simple Exponential Smoothing. High‑priority items (Gold) may default to Hybrid. |
| BR‑DI‑054 | Model Simplicity Preference Rule | Consistency Rule | If multiple models are eligible and historical performance difference is <1% WAPE, the simpler model (lower parameter count) shall be recommended. |

##### Policies (for DE‑DI‑051)

| ID | Policy | Category | Description |
|----|--------|----------|-------------|
| PO‑DI‑051 | Model Recommendation Override Policy | Authorization Policy | A Demand Planner may override the recommended model for a specific item if they provide a justification and the override is reviewed after 2 cycles. |

---
### 5.5.10 Functional Behaviour

1. **Scheduled** (monthly, or when enough new history accumulates) or event‑driven (after demand history cleansing).
2. **For each active product–location**, compute statistical features: zero percentage, mean, CV, autocorrelation at seasonal lags, trend significance.
3. **Execute DE‑DI‑050** (Classify Demand Pattern) using classification rule BR‑DI‑050, respecting minimum history (BR‑DI‑051) and stability constraints (BR‑DI‑052). Policy PO‑DI‑050 governs overrides.
4. **After classification**, execute DE‑DI‑051 (Recommend Forecasting Model) based on pattern, segment, and horizon, applying mapping rule BR‑DI‑053 and simplicity rule BR‑DI‑054.
5. **Store** classification and model recommendation as part of the demand metadata for downstream capabilities (Forecast Demand model selection, Evaluate Demand Quality).
6. **Raise events:** `DemandPatternClassified`, `ForecastingModelRecommended`.
7. **Handle overrides:** log all overrides per policy PO‑DI‑051.

### 5.5.11 Commands

| Command | Purpose |
|---------|---------|
| `ClassifyDemandPattern` | Run classification for a single product or batch |
| `OverridePatternClassification` | Manually assign a pattern |
| `RecommendForecastingModel` | Re‑compute model recommendation for given scope |

### 5.5.12 Events

| Event | Payload Highlights |
|-------|-------------------|
| `DemandPatternClassified` | Product, location, pattern label, confidence, timestamp |
| `ForecastingModelRecommended` | Product, location, recommended model type, reason |
| `PatternClassificationOverridden` | Product, location, old pattern, new pattern, user, justification |

### 5.5.13 Queries

| Query | Description |
|-------|-------------|
| `GetDemandPattern(product, location)` | Returns current classification |
| `GetModelRecommendation(product, location)` | Returns recommended model type |
| `GetPatternDistribution()` | Aggregate counts by pattern |

### 5.5.14 Reports
- **Demand Pattern Summary Report** – distribution of patterns across product families
- **Model Recommendation Compliance Report** – adherence to recommendations

### 5.5.15 Dashboards
- **Demand Pattern Map** – visual display of patterns per product hierarchy
- **Model Recommendation Dashboard** – shows mapping and planner overrides

### 5.5.16 Software Realization
Batch statistical computation with rule‑based classification:
```
API → Application Service → Domain Model (DemandPattern, ModelRecommendation)
→ Statistical Engine (time‑series feature extraction)
→ Rule Engine (classification and mapping rules)
→ Event Store → Projections → Read Model
```
Feature extraction uses standard statistical libraries. Rules can be updated via configuration without code changes.

---

## 5.6 Prioritize Demand

### 5.6.1 Purpose
Assign a business‑based priority ranking to products, customers, and product–customer combinations to direct planner attention, resource allocation, and exception handling towards the most impactful items. Answers *“Which demand items deserve the most attention and why?”* Priority is a composite score derived from volume, variability, strategic importance, margin, and risk.

### 5.6.2 Business Objectives Served
- BO‑DI‑002 (Improve Planning Effectiveness)
- BO‑DI‑004 (Improve Customer Outcomes)
- BO‑DI‑005 (Increase Planning Automation)

### 5.6.3 Enterprise Measures
- PI‑DI‑114 (Demand Prioritization Effectiveness)
- PI‑DI‑106 (Recommendation Acceptance Rate)
- PI‑DI‑018 (Manual Override Rate)

### 5.6.4 Semantic Objects

| ID | Object | Role |
|----|--------|------|
| SE‑DI‑014 | Demand Priority | Output |
| SE‑DI‑013 | Demand Segmentation | Input ABC/XYZ/strategic |
| SE‑DI‑030 | Customer | Source of customer tier |
| SE‑DI‑031 | Customer Tier | Critical for priority |
| SE‑DI‑040 | Product | Product attributes |
| SE‑DI‑042 | Product Life‑Cycle Stage | Influences priority |
| SE‑DI‑043 | Substitutability | Affects risk |

### 5.6.5 Primitive Capabilities Composed
- **Understand** – consolidates attributes
- **Assess** – scores items based on multiple criteria
- **Evaluate** – compares and ranks items

### 5.6.6 Enterprise Inputs
- Segmentation data (ABC/XYZ, strategic segment) from Segment Demand
- Customer tier and contractual obligations
- Product margin, life‑cycle stage, and substitutability
- Historical service level and stock‑out risk
- Configurable priority criteria and weights

### 5.6.7 Enterprise Understanding Produced
- A numeric priority score (0–100) for each item (product, customer, or combination)
- Priority level labels: Critical, High, Medium, Low (based on score thresholds)
- Priority rank within each aggregation level (e.g., product within family)
- Decomposition of priority score into contributing factors (volume, volatility, margin, strategic importance, risk)

### 5.6.8 Preconditions
- Segment data and master data must be current
- Priority criteria and weightings defined and approved by business stakeholders

### 5.6.9 Business Decisions

---
#### DE‑DI‑060 — Compute Priority Score

**Purpose:** Calculate a numerical priority score for every product, customer, or product–customer combination using a multi‑criteria scoring model.

**Required Understanding:** Volume contribution (ABC), variability (XYZ), strategic segment, customer tier, product margin, life‑cycle stage, recent service performance, contract criticality.

**Decision Alternatives:** The decision is deterministic; the output is a score, not a choice. However, different scoring models could be selected (e.g., weighted sum, outranking). The selection of model is also a decision but currently fixed to a configurable weighted sum. Model selection could be a separate decision if multiple algorithms are employed.

**Decision Criteria:** The score is computed as: `PriorityScore = Σ (Weight_i * NormalizedScore_i)` for each factor. Factors include: demand volume, revenue, margin, CV, customer tier, strategic flag, life‑cycle urgency, forecast error risk, shortage risk.

**Decision Confidence:** Dependent on data quality; confidence flag if any input data is missing or stale.

**Decision Rationale:** “Priority score 87 for Product X: volume factor 0.9, margin factor 0.8, customer tier (Gold) 1.0, risk factor 0.7. Weighted sum 0.87 → level ‘High’. Rule BR‑DI‑060 applied.” (Template.)

---
##### Rules (for DE‑DI‑060)

| ID | Rule | Category | Description |
|----|------|----------|-------------|
| BR‑DI‑060 | Priority Scoring Rule | Calculation Rule | The priority score is computed as a weighted sum of normalized factors. Default weights: Volume 20%, Revenue 25%, Margin 20%, Customer Tier 15%, Life‑Cycle Urgency 10%, Forecast Error Risk 10%. Weights are configurable by policy. |
| BR‑DI‑061 | Missing Data Handling Rule | Validation Rule | If any factor data is missing, the score is computed with available factors re‑weighted proportionally, but the item is flagged as “Score Incomplete” and presented for review. |
| BR‑DI‑062 | Priority Level Threshold Rule | Derivation Rule | Priority levels: Critical if score ≥ 85; High if 70–84; Medium if 50–69; Low if <50. Thresholds configurable. |

##### Policies (for DE‑DI‑060)

| ID | Policy | Category | Description |
|----|--------|----------|-------------|
| PO‑DI‑060 | Priority Weight Revision Policy | Compliance Policy | Weights must be reviewed annually by the Demand Planning Council; any change must be documented and approved by the VP Supply Chain. |

---
#### DE‑DI‑061 — Publish Priority List

**Purpose:** Finalize and distribute the priority assignments to planners, dashboards, and other capabilities (e.g., exception management, allocation).

**Required Understanding:** Priority scores from DE‑DI‑060, completeness flags.

**Decision Alternatives:** Publish as is, Publish with exceptions (items flagged for review), Hold publication if data quality is poor.

**Decision Criteria:** Percentage of items with complete scores ≥ 95%.

**Recommended Decision:** Automated if criteria met.

**Decision Confidence:** Aggregate completeness indicator.

**Decision Rationale:** “Priority list published: 97% items have complete scores. 3% flagged for planner review. Rule BR‑DI‑063 passed.” (Template.)

---
##### Rules (for DE‑DI‑061)

| ID | Rule | Category | Description |
|----|------|----------|-------------|
| BR‑DI‑063 | Priority List Completeness Rule | Validation Rule | The priority list shall not be published if >5% of active items have incomplete scores; instead, it is held for data remediation. |

##### Policies (for DE‑DI‑061)

| ID | Policy | Category | Description |
|----|--------|----------|-------------|
| PO‑DI‑061 | Priority List Publication Policy | Automation Policy | If completeness ≥ 95%, the list is published automatically. Otherwise, publication requires Demand Manager approval. |
| PO‑DI‑062 | Priority Override Policy | Authorization Policy | A Demand Planner may override a priority level for a specific item with justification. Overrides are tracked and analyzed quarterly. |

---
### 5.6.10 Functional Behaviour

1. **Trigger:** After every segmentation run and upon master data changes, or scheduled weekly.
2. **Retrieve** segment assignments, customer tiers, product margins, and other attributes from master data and segment master.
3. **For each item** (product/customer combination), compute normalized factor scores.
4. **Execute DE‑DI‑060** (Compute Priority Score) using rule BR‑DI‑060 and missing data handling BR‑DI‑061. Apply priority levels via BR‑DI‑062.
5. **Aggregate** scores and check completeness; execute DE‑DI‑061 (Publish Priority List) with rule BR‑DI‑063 and policy PO‑DI‑061.
6. **Handle overrides** per policy PO‑DI‑062.
7. **Distribute** priority list to planners, dashboards, and other capabilities (Detect Demand Exceptions for prioritization of alerts).
8. **Raise events:** `PriorityScoreComputed`, `PriorityListPublished`, `PriorityOverrideApplied`.

### 5.6.11 Commands

| Command | Purpose |
|---------|---------|
| `ComputePriorityScores` | Run prioritization for given scope |
| `OverridePriority` | Manually set a priority level |
| `PublishPriorityList` | Finalize and distribute |

### 5.6.12 Events

| Event | Payload Highlights |
|-------|-------------------|
| `PriorityScoreComputed` | Item ID, score, level, timestamp |
| `PriorityListPublished` | Version, completeness %, timestamp |
| `PriorityOverrideApplied` | Item ID, old level, new level, user, justification |

### 5.6.13 Queries

| Query | Description |
|-------|-------------|
| `GetPriority(productId, customerId)` | Returns priority score and level |
| `GetPriorityList(levelFilter)` | Returns items by level |
| `GetPriorityHistory(itemId)` | Changes over time |

### 5.6.14 Reports
- **Priority Distribution Report** – count of Critical/High/Medium/Low items
- **Priority Override Analysis Report** – frequency and impact of overrides

### 5.6.15 Dashboards
- **Priority Heatmap** – visual representation of priority across product/customer dimensions
- **Planner Workload Dashboard** – shows volume of Critical/High items assigned

### 5.6.16 Software Realization
Service with rule‑driven scoring:
```
API → Application Service → Domain Model (PriorityScore, PriorityList)
→ Scoring Engine (configurable weighted sum)
→ Event Store → Projections → Read Model
```
Weights and thresholds are stored in a configuration service, enabling dynamic adjustment. The service integrates with master data APIs to fetch attributes.

---

## 5.7 Evaluate Demand Quality

### 5.7.1 Purpose
Continuously measure, compute, and assess the quality of demand forecasts, planning outputs, and planner interventions. Answers *“How good are our demand forecasts and planning decisions, and where are they failing?”* This capability is the analytical engine behind all Business Outcome Measures related to forecast accuracy, bias, stability, and value. It provides the evidence base for model selection, planner coaching, and continuous improvement.

### 5.7.2 Business Objectives Served
- BO‑DI‑001 (Deliver Trusted Demand Understanding)
- BO‑DI‑002 (Improve Planning Effectiveness)
- BO‑DI‑006 (Continuously Improve Enterprise Intelligence)

### 5.7.3 Enterprise Measures
- PI‑DI‑002 (Forecast Accuracy)
- PI‑DI‑003 (WAPE)
- PI‑DI‑004 (MAPE)
- PI‑DI‑005 (Forecast Bias)
- PI‑DI‑006 (Forecast Value Added)
- PI‑DI‑007 (Forecast Stability)
- PI‑DI‑008 (Forecast Value Realization)
- PI‑DI‑101 (Demand Understanding Index) — composite, partially fed by this capability
- PI‑DI‑103 (Forecast Confidence Index)
- PI‑DI‑105 (Recommendation Quality Index)

### 5.7.4 Semantic Objects

| ID | Object | Role |
|----|--------|------|
| SE‑DI‑003 | Forecast | The forecast being evaluated |
| SE‑DI‑004 | Demand History | Actuals for comparison |
| SE‑DI‑020 | Forecast Model | Model whose output is evaluated |
| SE‑DI‑023 | Forecast Override | Planner adjustment to evaluate |
| SE‑DI‑024 | Forecast Cycle | The cycle containing forecasts |
| SE‑DI‑025 | Forecast Horizon | Time span |
| SE‑DI‑011 | Demand Pattern | Context for evaluation thresholds |
| SE‑DI‑013 | Demand Segmentation | Segment‑specific targets |

### 5.7.5 Primitive Capabilities Composed
- **Observe** – collects actual demand as it materialises
- **Understand** – aligns forecasts with actuals
- **Assess** – computes error metrics
- **Evaluate** – compares metrics against targets, benchmarks, and historical trends
- **Learn** – identifies patterns in errors for improvement

### 5.7.6 Enterprise Inputs
- Published forecasts (with prediction intervals) from Forecast Demand
- Actual demand quantities (cleansed) from Understand Demand
- Forecast overrides and their justifications
- Historical accuracy metrics for trend analysis
- Performance targets and thresholds from policy
- Segment and pattern classification for stratified evaluation

### 5.7.7 Enterprise Understanding Produced
- Error metrics (WAPE, MAPE, MPE, RMSE, Bias) at all aggregation levels
- Forecast Value Added (FVA) per process step
- Forecast Stability measurements between cycles
- Forecast Value Realization estimates
- Confidence Index calibration (comparing predicted intervals to actual error distributions)
- Planner intervention effectiveness (did overrides improve or degrade accuracy?)
- Trend and shift detection in accuracy metrics (is performance improving or degrading?)

### 5.7.8 Preconditions
- Actual demand data available for the evaluation period
- Forecasts from the corresponding cycles are stored and accessible
- Forecast horizon and time buckets are consistent with actuals

### 5.7.9 Business Decisions

---
#### DE‑DI‑070 — Compute Accuracy Metrics

**Purpose:** Calculate the standard set of forecast accuracy metrics for a given evaluation window and scope.

**Required Understanding:** Forecast time series, actual demand time series, planning buckets.

**Decision Alternatives:** None — the computation is deterministic. However, the selection of metrics to compute is configurable and could be treated as a system choice; in this specification, we compute the full standard set.

**Decision Criteria:** Statistical formulas as defined in Chapter 3.

**Recommended Decision:** Not applicable; output is the computed metric values.

**Decision Confidence:** Dependent on data completeness; if actuals are missing for some buckets, metrics are adjusted (e.g., exclude those buckets) and a data completeness flag is attached.

**Decision Rationale:** “WAPE for product family PF1 for Q1 2026 computed as 8.5%, based on 100% data completeness. All metrics computed using standard formulas per PI‑DI‑003.” (Template.)

---
##### Rules (for DE‑DI‑070)

| ID | Rule | Category | Description |
|----|------|----------|-------------|
| BR‑DI‑070 | Metric Calculation Standard Rule | Calculation Rule | All accuracy metrics shall be calculated exactly as defined in the Enterprise Measurement Model (Chapter 3). Any deviation requires a documented exception. |
| BR‑DI‑071 | Data Completeness Check Rule | Validation Rule | If actuals are missing for more than 10% of planning buckets in the evaluation window, the metrics for that scope are flagged as “low confidence” and not used for model selection decisions. |
| BR‑DI‑072 | Zero Actuals Handling Rule | Validation Rule | For metrics requiring division by actuals (MAPE, etc.), any bucket with zero actuals is excluded and the count of excluded buckets is reported. If >20% buckets are zero, MAPE is suppressed for that scope. |

##### Policies (for DE‑DI‑070)

| ID | Policy | Category | Description |
|----|--------|----------|-------------|
| PO‑DI‑070 | Metric Calculation Frequency Policy | Compliance Policy | Accuracy metrics shall be computed weekly on a rolling 4‑week window and monthly on a rolling 13‑week window. |

---
#### DE‑DI‑071 — Evaluate Forecast Value Added (FVA)

**Purpose:** Determine the incremental value contributed by each step of the forecasting process relative to a naive baseline.

**Required Understanding:** Naive forecast (lag‑1 persistence) for the same horizon, process forecasts at each step (e.g., statistical output, after override), actuals.

**Decision Alternatives:** For each process step, the result is a positive FVA (adds value), neutral (no significant change), or negative FVA (degrades accuracy).

**Decision Criteria:** FVA > 1 percentage point WAPE improvement → positive; between −1 and +1 → neutral; < −1 → negative.

**Recommended Decision:** Actionable insight: process steps with negative FVA are flagged for investigation.

**Decision Confidence:** Depends on the length of evaluation window (minimum 13 weeks recommended).

**Decision Rationale:** “Statistical forecast step added +7.3 pp WAPE improvement over naive (FVA positive). Planner override step added −1.5 pp (FVA negative), indicating overrides are currently degrading accuracy. Policy PO‑DI‑071 requires review of override process.” (Template.)

---
##### Rules (for DE‑DI‑071)

| ID | Rule | Category | Description |
|----|------|----------|-------------|
| BR‑DI‑073 | FVA Calculation Rule | Calculation Rule | FVA is computed as WAPE(Naive) − WAPE(Process) using the standard WAPE formula. Both WAPEs must be computed over the identical evaluation window and scope. |
| BR‑DI‑074 | FVA Minimum Window Rule | Validation Rule | FVA shall be computed over a minimum of 13 periods (weeks) to ensure statistical reliability. If fewer periods are available, FVA is computed but marked “provisional”. |

##### Policies (for DE‑DI‑071)

| ID | Policy | Category | Description |
|----|--------|----------|-------------|
| PO‑DI‑071 | Negative FVA Escalation Policy | Exception Policy | Any process step with negative FVA in two consecutive monthly evaluations triggers a formal review by the Demand Planning Manager and a documented improvement plan. |

---
#### DE‑DI‑072 — Evaluate Planner Override Effectiveness

**Purpose:** Assess whether manual overrides improve or degrade forecast accuracy, and identify planners whose overrides are systematically biased.

**Required Understanding:** Override instances with planner ID, justification, original forecast, final forecast, actual demand.

**Decision Alternatives:** For each override or planner, classify as “value‑adding” (improved accuracy), “neutral”, or “value‑destroying” (degraded accuracy).

**Decision Criteria:** Compare absolute error of original forecast vs. override forecast relative to actual. If override reduced error → value‑adding. If increased error → value‑destroying.

**Recommended Decision:** Flag planners whose overrides are value‑destroying > 30% of the time for coaching; flag overrides without justification for compliance review.

**Decision Confidence:** Requires actual demand data; confidence proportional to sample size.

**Decision Rationale:** “Planner J. Smith: 45 overrides last quarter, 52% improved accuracy, 33% degraded, 15% neutral. Improvement ratio above threshold, no action.” (Template.)

---
##### Rules (for DE‑DI‑072)

| ID | Rule | Category | Description |
|----|------|----------|-------------|
| BR‑DI‑075 | Override Accuracy Impact Rule | Validation Rule | For each override, the absolute forecast error is computed for original and override. The change in error determines the value impact. |
| BR‑DI‑076 | Planner Bias Detection Rule | Validation Rule | If a planner’s overrides show a consistent directional bias (e.g., >70% in one direction) and degrade accuracy, flag for manager review. |

##### Policies (for DE‑DI‑072)

| ID | Policy | Category | Description |
|----|--------|----------|-------------|
| PO‑DI‑072 | Planner Override Review Policy | Compliance Policy | Planners with value‑destroying override rate > 30% in a quarter must receive coaching and have their override privileges reviewed. |

---
#### DE‑DI‑073 — Publish Quality Report

**Purpose:** Compile and distribute the periodic demand quality report to all stakeholders.

**Required Understanding:** Aggregated accuracy metrics, FVA, stability, planner performance, trends.

**Decision Alternatives:** Publish normal report, Publish with red flags, Delay publication due to data issues.

**Decision Criteria:** Data completeness ≥ 90%, all mandatory metrics computed.

**Recommended Decision:** Automated if criteria met.

**Decision Confidence:** Based on underlying data quality.

**Decision Rationale:** “Weekly quality report published: overall WAPE 9.2% (good), FVA positive for statistical step, 3 planners with override degradation flagged.” (Template.)

---
##### Rules (for DE‑DI‑073)

| ID | Rule | Category | Description |
|----|------|----------|-------------|
| BR‑DI‑077 | Quality Report Completeness Rule | Validation Rule | The quality report must contain all metrics defined in Chapter 3 as applicable to the reporting period. Missing metrics must be explicitly noted. |

##### Policies (for DE‑DI‑073)

| ID | Policy | Category | Description |
|----|--------|----------|-------------|
| PO‑DI‑073 | Quality Report Distribution Policy | Compliance Policy | The quality report shall be published by 10:00 AM every Monday and distributed to Demand Planners, Demand Manager, and Supply Chain Director. |

---
### 5.7.10 Functional Behaviour

1. **Scheduled triggers:** Weekly (Monday morning) and monthly (first business day).
2. **Retrieve** actual demand data for evaluation window.
3. **Retrieve** published forecasts, overrides, and naive forecasts for the same periods.
4. **Execute DE‑DI‑070** (Compute Accuracy Metrics) — apply rules BR‑DI‑070/071/072.
5. **Execute DE‑DI‑071** (Evaluate FVA) — apply rules BR‑DI‑073/074, policy PO‑DI‑071.
6. **Execute DE‑DI‑072** (Evaluate Planner Override Effectiveness) — apply rules BR‑DI‑075/076, policy PO‑DI‑072.
7. **Trend analysis:** Compare current metrics to prior periods; detect significant shifts.
8. **Execute DE‑DI‑073** (Publish Quality Report) — rule BR‑DI‑077, policy PO‑DI‑073.
9. **Feed** results back to Learn From Demand capability (for model retuning, retraining triggers).
10. **Raise events:** `AccuracyMetricsComputed`, `FVAAnalysisCompleted`, `PlannerOverrideEvaluationCompleted`, `QualityReportPublished`.

### 5.7.11 Commands

| Command | Purpose |
|---------|---------|
| `ComputeAccuracyMetrics` | Run accuracy computation for a given scope and window |
| `EvaluateFVA` | Execute FVA analysis |
| `EvaluatePlannerOverrides` | Assess planner override performance |
| `PublishQualityReport` | Compile and release the quality report |

### 5.7.12 Events

| Event | Payload Highlights |
|-------|-------------------|
| `AccuracyMetricsComputed` | Scope, window, WAPE, MAPE, bias, etc., completeness flag |
| `FVAAnalysisCompleted` | Process step, FVA value, interpretation |
| `PlannerOverrideEvaluationCompleted` | Planner ID, override count, value‑adding rate, flag status |
| `QualityReportPublished` | Report ID, period, overall WAPE, red flags |

### 5.7.13 Queries

| Query | Description |
|-------|-------------|
| `GetAccuracyMetrics(scope, window)` | Returns current metrics |
| `GetFVABreakdown(scope, window)` | Returns FVA per process step |
| `GetPlannerOverrideStats(plannerId, period)` | Returns override performance |
| `GetQualityReport(period)` | Returns the full quality report |

### 5.7.14 Reports
- **Weekly Forecast Accuracy Report** — overall and by segment
- **Monthly FVA Report** — step‑by‑step value analysis
- **Planner Performance Scorecard** — override effectiveness per planner

### 5.7.15 Dashboards
- **Forecast Quality Dashboard** — WAPE, bias, stability trendlines
- **FVA Waterfall** — visual breakdown of value added/lost at each step
- **Planner Scorecard Dashboard** — override impact and bias detection

### 5.7.16 Software Realization
Batch computation with statistical engine:
```
API → Application Service → Domain Model (MetricsCalculation, FVAAnalysis, PlannerEvaluation)
→ Statistical Computation Engine (pre‑defined formulas, configurable)
→ Event Store → Projections → Read Model (optimized for dashboards)
```
All metric formulas are implemented as pure functions, verifiable against the specification in Chapter 3. The computation engine can be replaced without affecting the rest of the system.

---

## 5.8 Detect Demand Exceptions

### 5.8.1 Purpose
Continuously monitor demand signals, forecasts, and planning outputs for conditions that deviate from expected norms and require attention. Answers *“Where is something wrong, unusual, or risky in the demand picture?”* Detected exceptions are classified, prioritized, and routed to appropriate handlers or automatic resolution processes.

### 5.8.2 Business Objectives Served
- BO‑DI‑003 (Improve Enterprise Responsiveness)
- BO‑DI‑004 (Improve Customer Outcomes)
- BO‑DI‑005 (Increase Planning Automation)

### 5.8.3 Enterprise Measures
- PI‑DI‑110 (Exception Detection Accuracy)
- PI‑DI‑111 (Exception Prediction Accuracy)
- PI‑DI‑010 (Exception Response Time)
- PI‑DI‑215 (Exception Processing Time)

### 5.8.4 Semantic Objects

| ID | Object | Role |
|----|--------|------|
| SE‑DI‑015 | Demand Exception | The core output |
| SE‑DI‑003 | Forecast | Compared against actuals/signals |
| SE‑DI‑002 | Demand Signal | Real‑time input |
| SE‑DI‑004 | Demand History | Baseline |
| SE‑DI‑014 | Demand Priority | Prioritizes exceptions |
| SE‑DI‑011 | Demand Pattern | Context for thresholds |

### 5.8.5 Primitive Capabilities Composed
- **Observe** – monitors multiple data streams
- **Understand** – interprets deviations in context
- **Assess** – determines severity and type
- **Predict** – (optional) projects impact of exception

### 5.8.6 Enterprise Inputs
- Current demand signals and forecasts
- Historical demand patterns and baseline statistics
- Accuracy metrics and confidence indices from Evaluate Demand Quality
- Priority lists from Prioritize Demand
- Pre‑defined exception rules and thresholds

### 5.8.7 Enterprise Understanding Produced
- Exception instances with type, severity, affected item(s), and timestamp
- Exception categorization: Outlier, Level Shift, Trend Break, Model Failure, Data Gap, Spike, Bias Drift
- Recommended action for each exception: automatic resolution, planner review, escalation
- Exception trends (e.g., increasing frequency of model failures)

### 5.8.8 Preconditions
- Demand signals and forecasts are being updated regularly
- Baseline statistics and thresholds are configured
- Priority data is available

### 5.8.9 Business Decisions

---
#### DE‑DI‑080 — Classify Exception Type

**Purpose:** Determine the nature of a detected anomaly: is it a genuine demand shift, a data error, a model failure, or noise?

**Required Understanding:** The anomalous data point(s), recent history, forecast, confidence interval, demand pattern, external signals.

**Decision Alternatives:**
- Outlier (single period deviation, likely noise or one‑time event)
- Level Shift (sustained change in baseline)
- Trend Break (change in trend direction)
- Model Failure (forecast consistently outside prediction intervals)
- Data Gap (missing or incomplete data)
- False Positive (within normal variation — do nothing)

**Discovered Alternatives:** The Learn capability may propose new exception types not yet in the catalogue.

**Decision Criteria:** Rules‑based: deviation in σ, number of consecutive periods outside threshold, forecast error pattern, data quality flags, signal corroboration.

**Recommended Decision:** Determined by rule engine.

**Decision Confidence:** Based on statistical significance and signal quality.

**Decision Rationale:** “Exception EX‑1034 classified as Level Shift for Product Y. 7‑day mean shifted from 150 to 210, sustained for 8 periods, corroborated by POS signal. Rule BR‑DI‑080 triggered.” (Template.)

---
##### Rules (for DE‑DI‑080)

| ID | Rule | Category | Description |
|----|------|----------|-------------|
| BR‑DI‑080 | Exception Classification Rule | Derivation Rule | If deviation > 3σ for 1 period and returns to normal → Outlier. If > 2.5σ sustained ≥ 5 periods → Level Shift. If forecast errors > prediction interval in ≥ 80% of periods for 4 weeks → Model Failure. If actuals missing → Data Gap. If deviation < 2σ → False Positive. |
| BR‑DI‑081 | Signal Corroboration Rule | Consistency Rule | For a Level Shift or Trend Break, at least one independent signal source must corroborate the direction of change; otherwise, the exception is classified as “Unconfirmed” and held for manual review. |

##### Policies (for DE‑DI‑080)

| ID | Policy | Category | Description |
|----|--------|----------|-------------|
| PO‑DI‑080 | False Positive Filtering Policy | Automation Policy | Exceptions classified as False Positive are logged but not presented to planners unless they recur 3 times in a rolling 7‑day window for the same item. |

---
#### DE‑DI‑081 — Prioritize Exception

**Purpose:** Assign a severity and urgency to the exception based on business impact, guiding response priority.

**Required Understanding:** Exception type, affected item’s priority (from Prioritize Demand), magnitude, potential impact on service level.

**Decision Alternatives:** Critical (immediate action), High (action within 4 hours), Medium (within 24 hours), Low (log for review).

**Decision Criteria:** Exception type × business priority: Level Shift on Gold customer product → Critical; Outlier on C item → Low; Model Failure on A item → High.

**Recommended Decision:** Automated via prioritization matrix.

**Decision Confidence:** Derived from priority score confidence.

**Decision Rationale:** “Exception EX‑1034 prioritized as Critical: Level Shift on A‑X‑Gold product, potential service impact 15%. Rule BR‑DI‑082 applied.” (Template.)

---
##### Rules (for DE‑DI‑081)

| ID | Rule | Category | Description |
|----|------|----------|-------------|
| BR‑DI‑082 | Exception Priority Matrix Rule | Derivation Rule | A configurable matrix maps exception type and business priority to severity levels. Default matrix included in specification appendix. |
| BR‑DI‑083 | Escalation Rule | Validation Rule | Critical exceptions must be escalated to the Demand Manager and trigger an alert (push notification). |

##### Policies (for DE‑DI‑081)

| ID | Policy | Category | Description |
|----|--------|----------|-------------|
| PO‑DI‑081 | Exception Escalation Policy | Authorization Policy | Critical exceptions are automatically escalated. High exceptions are routed to the assigned planner; if not acknowledged within 4 hours, they are escalated to the manager. |

---
#### DE‑DI‑082 — Resolve Exception

**Purpose:** For each confirmed exception, determine the appropriate resolution action — whether it can be resolved automatically or requires human intervention.

**Required Understanding:** Exception classification, priority, available resolution actions, automation rules.

**Decision Alternatives:**
- Auto‑resolve: apply predefined correction (e.g., adjust forecast, fill missing data with interpolation)
- Suggest resolution: generate a recommended action for planner approval
- Manual only: no automated suggestion, requires planner investigation

**Decision Criteria:** Based on exception type, confidence, and automation policy. High‑confidence Level Shifts may auto‑trigger forecast refresh; Model Failures always require manual investigation.

**Recommended Decision:** Derived from policies.

**Decision Confidence:** Based on resolution effectiveness history.

**Decision Rationale:** “Exception EX‑1034 auto‑resolved: Level Shift detected with 95% confidence. Forecast automatically refreshed for affected item. Policy PO‑DI‑082 applied.” (Template.)

---
##### Rules (for DE‑DI‑082)

| ID | Rule | Category | Description |
|----|------|----------|-------------|
| BR‑DI‑084 | Auto‑Resolution Eligibility Rule | Validation Rule | An exception may be auto‑resolved only if: (1) type is Outlier, Data Gap, or Level Shift; (2) confidence ≥ 90%; (3) business priority is not Critical (Critical always requires manual review). |
| BR‑DI‑085 | Resolution Documentation Rule | Compliance Rule | Every resolution, whether auto or manual, must be logged with the action taken, timestamp, and actor. |

##### Policies (for DE‑DI‑082)

| ID | Policy | Category | Description |
|----|--------|----------|-------------|
| PO‑DI‑082 | Auto‑Resolution Policy | Automation Policy | Auto‑resolution is permitted for eligible exceptions (BR‑DI‑084). All other exceptions require planner intervention. |
| PO‑DI‑083 | Resolution Timeliness SLA Policy | Compliance Policy | Critical exceptions must be resolved within 2 hours, High within 8 hours, Medium within 48 hours. SLA performance is reported monthly. |

---
### 5.8.10 Functional Behaviour

1. **Continuous monitoring** of demand signals, forecasts, and quality metrics.
2. **Anomaly detection** via statistical process control (control charts, rolling σ checks).
3. **For each anomaly**, execute DE‑DI‑080 (Classify Exception Type) — rules BR‑DI‑080/081, policy PO‑DI‑080 filters false positives.
4. **For each confirmed exception**, execute DE‑DI‑081 (Prioritize Exception) — rules BR‑DI‑082/083, policy PO‑DI‑081 handles escalation.
5. **For each prioritized exception**, execute DE‑DI‑082 (Resolve Exception) — rules BR‑DI‑084/085, policies PO‑DI‑082/083 determine resolution path.
6. **Raise events:** `ExceptionDetected`, `ExceptionClassified`, `ExceptionPrioritized`, `ExceptionResolved`, `ExceptionEscalated`.
7. **Track** resolution SLAs and feed data to operational measures.

### 5.8.11 Commands

| Command | Purpose |
|---------|---------|
| `ScanForExceptions` | Trigger a manual scan for exceptions |
| `ClassifyException` | Re‑classify or manually classify an exception |
| `ResolveException` | Apply a resolution action |
| `EscalateException` | Manually escalate |

### 5.8.12 Events

| Event | Payload Highlights |
|-------|-------------------|
| `ExceptionDetected` | Item, time, deviation details |
| `ExceptionClassified` | Exception ID, type, confidence |
| `ExceptionPrioritized` | Exception ID, severity, assigned to |
| `ExceptionResolved` | Exception ID, resolution type, timestamp, actor |
| `ExceptionEscalated` | Exception ID, escalated to, reason |

### 5.8.13 Queries

| Query | Description |
|-------|-------------|
| `GetActiveExceptions(filter)` | Current unresolved exceptions |
| `GetExceptionHistory(item, period)` | Exception log |
| `GetExceptionSLAReport(period)` | SLA adherence metrics |

### 5.8.14 Reports
- **Exception Summary Report** — frequency by type, severity, resolution time
- **SLA Compliance Report** — adherence to resolution timeliness targets

### 5.8.15 Dashboards
- **Exception Monitor** — live feed of active exceptions with priority
- **Exception Analytics Dashboard** — trends, hotspots, resolution effectiveness

### 5.8.16 Software Realization
Event‑driven monitoring with rule engine:
```
Signal Ingestion → Stream Processor (anomaly detection) 
→ Domain Service (ExceptionAggregate) 
→ Rule Engine (classification, prioritization, resolution rules)
→ Event Store → Projections (active exceptions view) → Read Model
```
Anomaly detection uses statistical models (control charts, Holt‑Winters residuals). Rules are hot‑reloadable. Planners interact via a workbench that consumes the read model.

---

## 5.9 Explain Demand

### 5.9.1 Purpose
Generate clear, traceable, and business‑meaningful explanations for every forecast, detected change, exception, and recommendation produced by the Demand Intelligence domain. Answers *“Why is this forecast what it is? Why did it change? Why was this decision made?”* Explanations are derived automatically from the causal traceability chain—enterprise meaning, signals, model logic, rules, and policies—without requiring manual documentation. This capability is the engine for AI explainability and planner trust.

### 5.9.2 Business Objectives Served
- BO‑DI‑001 (Deliver Trusted Demand Understanding)
- BO‑DI‑002 (Improve Planning Effectiveness)
- BO‑DI‑006 (Continuously Improve Enterprise Intelligence)

### 5.9.3 Enterprise Measures
- PI‑DI‑025 (Explainability Score)
- PI‑DI‑107 (Explainability Score — intelligence measure)
- PI‑DI‑106 (Recommendation Acceptance Rate) — indirectly, as better explanations increase acceptance

### 5.9.4 Semantic Objects

| ID | Object | Role |
|----|--------|------|
| SE‑DI‑003 | Forecast | Subject of explanation |
| SE‑DI‑015 | Demand Exception | Subject of explanation |
| SE‑DI‑002 | Demand Signal | Causal factor |
| SE‑DI‑020 | Forecast Model | Model that produced forecast |
| SE‑DI‑023 | Forecast Override | Planner intervention to explain |
| SE‑DI‑004 | Demand History | Historical context |

### 5.9.5 Primitive Capabilities Composed
- **Understand** – interprets model internals, feature importance, rule evaluations
- **Assess** – evaluates completeness and quality of explanation
- **Learn** – improves explanation templates based on feedback

### 5.9.6 Enterprise Inputs
- Forecasts, with prediction intervals and confidence scores
- Demand exceptions with classification and priority
- Decision recommendations from any capability (forecast, prioritization, classification, etc.)
- Model metadata (type, features, training window, feature importance)
- Rules and policies that were evaluated for each decision
- Demand signals and their influence weights
- Override justifications

### 5.9.7 Enterprise Understanding Produced
- Structured explanation objects containing:
  - Natural language explanation (human‑readable)
  - Structured causal trace (machine‑readable): factors, weights, rules fired
  - Traceability chain: signal → understanding → forecast → decision → rule → policy → outcome
- Explanation quality score (completeness, clarity, traceability)
- Explanation template version used

### 5.9.8 Preconditions
- All upstream artifacts (forecasts, exceptions, decisions, rules) must be persisted with their traceability metadata
- Model feature importance or SHAP values must be available for models that support them

### 5.9.9 Business Decisions

---
#### DE‑DI‑090 — Generate Forecast Explanation

**Purpose:** Produce a human‑ and machine‑readable explanation for a given forecast, describing why the forecast value is what it is and what factors influenced it.

**Required Understanding:** The forecast (mean, interval), the model that produced it, feature contributions (if available), demand signals, recent history, baseline comparison.

**Decision Alternatives:** Not applicable — explanation is deterministic based on available data. If insufficient data exists, the explanation states that clearly.

**Decision Criteria:** Explanation must cover: (1) historical baseline, (2) key influencing factors ranked by contribution, (3) confidence and uncertainty, (4) comparison to prior forecast (change explanation).

**Recommended Decision:** The explanation is generated; no choice to make.

**Decision Confidence:** Explanation completeness score; lower if feature importance is unavailable for opaque models.

**Decision Rationale:** “Forecast for Product X, Week 15 = 250 units (90% PI: 200‑300). Key drivers: (1) Seasonal uplift for April (+40 units, 45% contribution), (2) Promotional signal for Week 15 (+25 units, 28% contribution), (3) Recent trend (+15 units, 17% contribution). Baseline (deseasonalized) = 170 units. Confidence: 88%. Rule BR‑DI‑090 required all drivers with >10% contribution to be listed.” (Explainability template.)

---
##### Rules (for DE‑DI‑090)

| ID | Rule | Category | Description |
|----|------|----------|-------------|
| BR‑DI‑090 | Explanation Completeness Rule | Validation Rule | Every forecast explanation shall include: baseline, ranked factors with contribution ≥10%, confidence, and a change explanation (vs. prior forecast for same period). Missing any element produces an “Incomplete Explanation” flag. |
| BR‑DI‑091 | Unexplainable Model Rule | Consistency Rule | If a forecast model is opaque and does not provide feature importance, the explanation must state “Generated by model X, which does not provide per‑feature explanations. Confidence intervals are derived from historical error distribution.” The Explainability Score is reduced accordingly. |
| BR‑DI‑092 | Change Driver Rule | Derivation Rule | When comparing to the prior forecast for the same period, the explanation must identify the top factors that drove the change (e.g., “New promotion added, increasing forecast by 30 units”). |

##### Policies (for DE‑DI‑090)

| ID | Policy | Category | Description |
|----|--------|----------|-------------|
| PO‑DI‑090 | Explanation Quality Policy | Compliance Policy | Forecasts with an Explainability Score below 60% may be published but must be flagged. Below 40%, publication is suppressed until a planner provides a manual explanation. |
| PO‑DI‑091 | Model Transparency Policy | Compliance Policy | New forecast models shall be preferred if they provide feature‑level explanations (SHAP, LIME, etc.) over opaque models with equivalent performance. |

---
#### DE‑DI‑091 — Generate Exception Explanation

**Purpose:** Produce an explanation for a detected demand exception: what is abnormal, why it was detected, and what the likely cause is.

**Required Understanding:** Exception details (type, magnitude, item), historical baseline, relevant demand signals, model behaviour around the exception.

**Decision Alternatives:** Not applicable — explanation is deterministic.

**Decision Criteria:** Explanation must cover: (1) what the normal range is, (2) what was observed, (3) which rule triggered the exception, (4) possible causes (signal change, data error, model failure).

**Recommended Decision:** Generated automatically.

**Decision Confidence:** Higher if corroborated by multiple signals.

**Decision Rationale:** “Exception EX‑1034: Level Shift detected on Product Y. Normal range: 130‑170 units/week (2σ band). Observed: 210 units/week for 8 consecutive weeks. Triggered by BR‑DI‑080 (>2.5σ, sustained ≥5 periods). Likely cause: New customer contract started Week 32 (corroborated by order signal). Confidence: 95%.” (Template.)

---
##### Rules (for DE‑DI‑091)

| ID | Rule | Category | Description |
|----|------|----------|-------------|
| BR‑DI‑093 | Exception Explanation Completeness Rule | Validation Rule | Every exception explanation shall include: normal range, observed value, the specific rule that triggered, and at least one likely cause or a statement that cause is unknown. |

##### Policies (for DE‑DI‑091)

| ID | Policy | Category | Description |
|----|--------|----------|-------------|
| PO‑DI‑092 | Exception Explanation Policy | Automation Policy | Exception explanations are auto‑generated and attached to the exception. If the exception is escalated, the explanation is included in the alert. |

---
#### DE‑DI‑092 — Generate Decision Explanation

**Purpose:** Produce a traceable justification for any decision made within Demand Intelligence, referencing the rules and policies that governed it.

**Required Understanding:** The decision (what was decided), the alternatives considered, the rule evaluations that led to the decision, the policy that authorized it.

**Decision Alternatives:** Not applicable.

**Decision Criteria:** Explanation must include: decision statement, criteria, rule evaluation results, policy applied, and a natural language summary.

**Recommended Decision:** Deterministic.

**Decision Confidence:** High if all traceability links are intact.

**Decision Rationale:** “Decision DE‑DI‑022: Forecast for Cycle 45 published automatically. Criteria: Forecast Confidence Index = 93% (threshold 90%), completeness = 100%. Rule BR‑DI‑026 passed. Policy PO‑DI‑023 authorized automatic publication. No planner approval required.” (Template.)

---
##### Rules (for DE‑DI‑092)

| ID | Rule | Category | Description |
|----|------|----------|-------------|
| BR‑DI‑094 | Decision Traceability Rule | Validation Rule | Every decision explanation must include the ARS traceability chain: Decision ID → Rule ID(s) evaluated → Policy ID applied → Capability ID. |
| BR‑DI‑095 | Natural Language Rule | Derivation Rule | The natural language summary must follow the explainability template defined in the Decision Model: “We recommend {{Decision}} because {{Understanding}}, and {{Rule}} confirmed it does not violate {{Constraint}}.” |

##### Policies (for DE‑DI‑092)

| ID | Policy | Category | Description |
|----|--------|----------|-------------|
| PO‑DI‑093 | Decision Explanation Logging Policy | Compliance Policy | Every decision explanation is immutable and stored in the audit log. |

---
### 5.9.10 Functional Behaviour

1. **Event‑driven:** Triggered whenever a forecast is published, an exception is classified, or any demand decision is made.
2. **Retrieve** the subject entity (forecast, exception, decision) and all linked metadata (model, signals, rules, policies).
3. **For each subject:**
   - If forecast → execute DE‑DI‑090, rules BR‑DI‑090/091/092, policies PO‑DI‑090/091.
   - If exception → execute DE‑DI‑091, rule BR‑DI‑093, policy PO‑DI‑092.
   - If decision → execute DE‑DI‑092, rules BR‑DI‑094/095, policy PO‑DI‑093.
4. **Assemble** the structured explanation and store it, linked to the subject by identifier.
5. **Compute** the explainability score based on completeness, model transparency, and traceability linkage.
6. **Publish** explanations to the event bus; they are consumed by dashboards, AI agents, and audit systems.
7. **Raise events:** `ForecastExplanationGenerated`, `ExceptionExplanationGenerated`, `DecisionExplanationGenerated`.

### 5.9.11 Commands

| Command | Purpose |
|---------|---------|
| `GenerateForecastExplanation` | Explicitly generate explanation for a forecast |
| `GenerateExceptionExplanation` | Generate explanation for an exception |
| `GenerateDecisionExplanation` | Generate explanation for a decision |
| `RegenerateAllExplanations` | Rebuild explanations after template or rule changes |

### 5.9.12 Events

| Event | Payload Highlights |
|-------|-------------------|
| `ForecastExplanationGenerated` | Forecast ID, explanation text, causal trace, score |
| `ExceptionExplanationGenerated` | Exception ID, explanation text, rule reference |
| `DecisionExplanationGenerated` | Decision ID, explanation text, ARS trace chain |

### 5.9.13 Queries

| Query | Description |
|-------|-------------|
| `GetForecastExplanation(forecastId)` | Returns the explanation |
| `GetExceptionExplanation(exceptionId)` | Returns the explanation |
| `GetDecisionExplanation(decisionId)` | Returns the explanation |
| `GetExplainabilityScore(scope, period)` | Aggregate score |

### 5.9.14 Reports
- **Explainability Score Report** — by capability, model, period
- **Unexplained Items Report** — items with incomplete or low‑score explanations

### 5.9.15 Dashboards
- **Explainability Overview Dashboard** — overall score, trends
- **Drill‑Down Explanation Viewer** — interactive exploration of causal factors

### 5.9.16 Software Realization
Explanation engine as a cross‑cutting service:
```
Event Bus (ForecastPublished, ExceptionClassified, DecisionMade)
→ Explanation Service (template engine, traceability resolver)
→ Domain Model (Explanation)
→ Event Store → Projections → Read Model
```
Explanation templates are stored as versioned artifacts and can be updated via configuration. The service resolves the full ARS traceability chain by querying the event store and read models of other capabilities.

---

## 5.10 Learn From Demand

### 5.10.1 Purpose
Continuously improve the entire Demand Intelligence domain by analyzing outcomes, detecting patterns in errors and exceptions, and recommending enhancements to models, rules, thresholds, and processes. Answers *“How can we get better?”* Learning is the meta‑capability that closes the feedback loop, ensuring the enterprise’s demand intelligence becomes progressively more accurate, efficient, and autonomous over time.

### 5.10.2 Business Objectives Served
- BO‑DI‑006 (Continuously Improve Enterprise Intelligence)
- BO‑DI‑001 (Deliver Trusted Demand Understanding)
- BO‑DI‑005 (Increase Planning Automation)

### 5.10.3 Enterprise Measures
- PI‑DI‑021 (Forecast Improvement Rate)
- PI‑DI‑022 (Planning Accuracy Improvement)
- PI‑DI‑023 (Recommendation Quality Index)
- PI‑DI‑024 (Decision Confidence Index)
- PI‑DI‑108 (Learning Effectiveness Index)
- PI‑DI‑115 (AI Recommendation Utilization)

### 5.10.4 Semantic Objects

| ID | Object | Role |
|----|--------|------|
| SE‑DI‑003 | Forecast | Evaluated for improvement |
| SE‑DI‑015 | Demand Exception | Analyzed for patterns |
| SE‑DI‑020 | Forecast Model | Subject of retraining or replacement |
| SE‑DI‑011 | Demand Pattern | May evolve over time |
| SE‑DI‑004 | Demand History | Growing training data |

### 5.10.5 Primitive Capabilities Composed
- **Observe** – monitors performance trends
- **Understand** – identifies root causes of poor performance
- **Assess** – evaluates whether changes are improvements
- **Predict** – forecasts the impact of proposed changes
- **Evaluate** – compares before/after
- **Learn** – applies and institutionalizes improvements

### 5.10.6 Enterprise Inputs
- Accuracy metrics and trends from Evaluate Demand Quality
- Exception logs and resolution outcomes from Detect Demand Exceptions
- Model performance history (champion/challenger comparisons)
- Planner override statistics and effectiveness
- Explanation quality scores
- Automation rates and touchless planning metrics
- AI recommendation acceptance/rejection data

### 5.10.7 Enterprise Understanding Produced
- Improvement opportunities with estimated impact and effort
- Recommendations for model retraining, hyperparameter tuning, or replacement
- Recommendations for rule threshold adjustments
- Recommendations for policy changes (e.g., automation thresholds)
- Pattern drift alerts (demand pattern changing, requiring re‑classification)
- Learning loop closure reports (did a change produce the expected improvement?)
- Discovered alternatives: new patterns, new exception types, new segment definitions

### 5.10.8 Preconditions
- Historical performance data available for multiple cycles
- Model training infrastructure accessible
- Rules and policies are configurable (not hard‑coded)

### 5.10.9 Business Decisions

---
#### DE‑DI‑100 — Recommend Model Improvement

**Purpose:** Analyze model performance trends and recommend specific improvements: retrain, tune hyperparameters, switch model type, or propose a new model architecture.

**Required Understanding:** Model performance history (WAPE, bias, stability over recent cycles), challenger evaluation results, feature importance drift, data drift metrics.

**Decision Alternatives:**
- No action (performance stable)
- Retrain with same configuration (data drift detected)
- Tune hyperparameters (performance degrading slowly)
- Switch to different model type (systematic underperformance on current pattern)
- Propose new model architecture (discovered alternative — forwarded to model management)

**Discovered Alternatives:** The Learn capability may propose a new model architecture based on analysis of error residuals and pattern matching. This alternative must pass an Alternative Validation Rule before entering the model catalogue.

**Decision Criteria:** Performance trend direction and significance, error decomposition (bias vs. variance), comparison to benchmark models, ROI of improvement action.

**Recommended Decision:** Determined by rules.

**Decision Confidence:** Based on statistical significance of trend and expected improvement.

**Decision Rationale:** “Model M042 for Product Family PF3 recommended for hyperparameter tuning. WAPE has increased from 7.2% to 9.1% over 6 months (trend p=0.03). No data drift detected — likely parameter staleness. Estimated improvement: 1‑2 pp WAPE reduction. Rule BR‑DI‑100 triggered.” (Template.)

---
##### Rules (for DE‑DI‑100)

| ID | Rule | Category | Description |
|----|------|----------|-------------|
| BR‑DI‑100 | Model Performance Degradation Rule | Model Evaluation Rule | If a champion model’s WAPE has increased by more than 2 percentage points over a rolling 6‑month period (statistically significant, p≤0.05), a retrain or tune recommendation is triggered. |
| BR‑DI‑101 | Data Drift Detection Rule | Validation Rule | If feature distributions have shifted significantly (Population Stability Index > 0.25), recommend retraining before tuning. |
| BR‑DI‑102 | Model Switch Rule | Model Evaluation Rule | If the champion model’s WAPE exceeds the benchmark naive forecast for 3 consecutive months, recommend switching model type. |
| BR‑DI‑109 | New Model Validation Rule | Alternative Validation Rule | A discovered model architecture must achieve a WAPE at least 5% lower than the current champion on a holdout set, and must be reproducible. |

##### Policies (for DE‑DI‑100)

| ID | Policy | Category | Description |
|----|--------|----------|-------------|
| PO‑DI‑100 | Automatic Retraining Policy | Automation Policy | If data drift is detected and retraining is recommended, it may be executed automatically during the next forecast cycle. All other improvements require Demand Manager approval. |
| PO‑DI‑101 | New Model Proposal Policy | Exception Policy | A discovered model architecture must be reviewed by the Data Science team before it can be registered as a challenger. |

---
#### DE‑DI‑101 — Recommend Threshold Adjustment

**Purpose:** Analyze whether rule thresholds (e.g., for exception detection, automation, significance) are optimally tuned and recommend adjustments.

**Required Understanding:** False positive / false negative rates for exception detection, automation rates vs. error rates, planner workload metrics, business impact of missed vs. false alarms.

**Decision Alternatives:**
- No change (thresholds optimal)
- Tighten threshold (reduce false positives/negatives)
- Loosen threshold (increase sensitivity or automation)
- Change threshold structure (e.g., add a new tier)

**Decision Criteria:** Cost‑benefit analysis of threshold change, impact on automation rate, impact on planner workload, impact on service risk.

**Recommended Decision:** Derived from optimization analysis.

**Decision Confidence:** Based on historical data volume and stability of the metric.

**Decision Rationale:** “Recommend tightening the Critical alert threshold from 4σ to 3.5σ. Current false positive rate is 22%, and analysis shows that 3.5σ captures 98% of true Critical events while reducing false positives to 8%. Estimated planner time savings: 5 hours/week. Rule BR‑DI‑103 applied.” (Template.)

---
##### Rules (for DE‑DI‑101)

| ID | Rule | Category | Description |
|----|------|----------|-------------|
| BR‑DI‑103 | Threshold Optimization Rule | Model Evaluation Rule | Thresholds shall be reviewed quarterly using a cost‑benefit framework. A recommendation to adjust is generated if the expected net benefit (false alarm reduction value minus missed event cost) exceeds a configurable minimum. |
| BR‑DI‑104 | Threshold Stability Rule | Consistency Rule | A threshold shall not be changed more than once per quarter unless a major event (e.g., a structural demand shift) is documented. |

##### Policies (for DE‑DI‑101)

| ID | Policy | Category | Description |
|----|--------|----------|-------------|
| PO‑DI‑102 | Threshold Adjustment Approval Policy | Approval Policy | Threshold adjustments that affect Critical alerting or automation require Demand Manager and Supply Chain Director approval. |

---
#### DE‑DI‑102 — Propose New Pattern or Segment

**Purpose:** Detect when demand behaviour has fundamentally shifted and propose a new demand pattern, exception type, or segment definition.

**Required Understanding:** Long‑term demand history, recent pattern stability, cluster analysis of residual errors, outlier clusters that recur.

**Decision Alternatives:**
- No new pattern (current catalogue sufficient)
- Propose new pattern (e.g., “Bi‑modal”, “Event‑driven spike”)
- Propose new segment (e.g., new strategic category)
- Propose new exception type

**Discovered Alternatives:** The output of this decision is itself a discovered alternative, which must be validated by rule BR‑DI‑105 and governed by policy PO‑DI‑103.

**Decision Criteria:** Statistical distinctiveness, recurrence, volume of affected items, improvement in classification accuracy if adopted.

**Recommended Decision:** Proposal is generated; governance follows.

**Decision Confidence:** Based on cluster separation metrics and sample size.

**Decision Rationale:** “Proposed new demand pattern ‘Event‑Driven Spike’: detected in 15 products, characterized by 2‑3 periods of extreme demand (>5σ) followed by return to baseline, correlated with external event calendar. Adding this pattern is estimated to improve forecast accuracy for affected items by 12%. Rule BR‑DI‑105 requires review.” (Template.)

---
##### Rules (for DE‑DI‑102)

| ID | Rule | Category | Description |
|----|------|----------|-------------|
| BR‑DI‑105 | New Pattern Validation Rule | Alternative Validation Rule | A proposed new pattern must be supported by at least 10 independent product‑location series, show statistical distinctiveness (cluster separation index > 0.7), and be associated with an identifiable business cause. |
| BR‑DI‑106 | Segment Proposal Rule | Alternative Validation Rule | A proposed new segment must demonstrate that it improves the homogeneity of demand behaviour within the segment (reduction in within‑segment variance ≥ 20%) compared to the existing segmentation. |

##### Policies (for DE‑DI‑102)

| ID | Policy | Category | Description |
|----|--------|----------|-------------|
| PO‑DI‑103 | Catalogue Evolution Policy | Exception Policy | All new patterns, exception types, and segments must be reviewed and approved by the Demand Planning Council before addition to the enterprise catalogue. The catalogue is versioned and changes are communicated to all stakeholders. |

---
#### DE‑DI‑103 — Close the Learning Loop

**Purpose:** After an improvement has been implemented (model retrained, threshold changed, pattern added), evaluate whether the expected benefit was realized.

**Required Understanding:** Before‑and‑after metrics for the specific improvement, control group if applicable, observation window of sufficient length.

**Decision Alternatives:**
- Improvement confirmed (benefit realized)
- Improvement partially realized (benefit smaller than expected)
- No improvement (revert or further investigate)
- Negative impact (revert immediately)

**Decision Criteria:** Comparison of actual vs. predicted improvement, statistical significance, observation window adequacy.

**Recommended Decision:** Automated comparison; action determined by rules.

**Decision Confidence:** Proportional to observation window length.

**Decision Rationale:** “Model retraining for M042 completed on Jan 15. Post‑retraining WAPE (Feb‑Mar): 6.8%, pre‑retraining (Oct‑Dec): 9.1%. Improvement of 2.3 pp confirmed (p=0.01). Expected improvement was 1‑2 pp — outcome exceeded expectation. Learning loop closed successfully.” (Template.)

---
##### Rules (for DE‑DI‑103)

| ID | Rule | Category | Description |
|----|------|----------|-------------|
| BR‑DI‑107 | Improvement Verification Rule | Validation Rule | Every implemented improvement must be evaluated after a minimum observation window (4 weeks for model changes, 8 weeks for threshold changes). |
| BR‑DI‑108 | Automatic Rollback Rule | Model Evaluation Rule | If an implemented change causes a statistically significant degradation (p≤0.05) in WAPE or service level, an automatic rollback recommendation is generated. |

##### Policies (for DE‑DI‑103)

| ID | Policy | Category | Description |
|----|--------|----------|-------------|
| PO‑DI‑104 | Learning Loop Closure Policy | Compliance Policy | Every improvement initiative must be documented with a before‑after evaluation. Results are reported in the monthly Demand Intelligence performance review. |
| PO‑DI‑105 | Rollback Authorization Policy | Authorization Policy | Rollbacks due to degradation are executed immediately by the Demand Manager. |

---
### 5.10.10 Functional Behaviour

1. **Scheduled triggers:** Weekly trend analysis, monthly deep analysis, quarterly threshold review.
2. **Event‑driven triggers:** After each forecast cycle and quality report publication.
3. **Retrieve** performance data, error patterns, exception logs, planner feedback.
4. **Execute DE‑DI‑100** (Recommend Model Improvement) — rules BR‑DI‑100/101/102, policies PO‑DI‑100/101.
5. **Execute DE‑DI‑101** (Recommend Threshold Adjustment) — rules BR‑DI‑103/104, policy PO‑DI‑102.
6. **Execute DE‑DI‑102** (Propose New Pattern or Segment) — rules BR‑DI‑105/106, policy PO‑DI‑103.
7. **For all prior improvements** that have passed their observation window, execute DE‑DI‑103 (Close the Learning Loop) — rules BR‑DI‑107/108, policies PO‑DI‑104/105.
8. **Publish** learning recommendations and loop closure reports.
9. **Feed** approved improvements back to the relevant capabilities (model registry, rule engine, pattern catalogue).
10. **Raise events:** `ImprovementRecommended`, `NewPatternProposed`, `ThresholdAdjustmentRecommended`, `LearningLoopClosed`.

### 5.10.11 Commands

| Command | Purpose |
|---------|---------|
| `AnalyzePerformanceTrends` | Trigger analysis for improvement opportunities |
| `ProposeImprovement` | Generate a specific improvement recommendation |
| `EvaluateImprovement` | Evaluate an implemented improvement |
| `RollbackImprovement` | Revert a change that caused degradation |

### 5.10.12 Events

| Event | Payload Highlights |
|-------|-------------------|
| `ImprovementRecommended` | Type, target, expected benefit, confidence |
| `NewPatternProposed` | Pattern details, evidence summary |
| `ThresholdAdjustmentRecommended` | Rule ID, old threshold, new threshold, rationale |
| `LearningLoopClosed` | Improvement ID, before metrics, after metrics, verdict |

### 5.10.13 Queries

| Query | Description |
|-------|-------------|
| `GetImprovementHistory(period)` | All improvements proposed and implemented |
| `GetActiveImprovements()` | Improvements awaiting evaluation |
| `GetLearningEffectivenessIndex()` | Composite metric of improvement success rate |

### 5.10.14 Reports
- **Continuous Improvement Report** — list of all improvements with before‑after results
- **Model Health Report** — degradation alerts, retraining schedule

### 5.10.15 Dashboards
- **Learning Dashboard** — improvement funnel (proposed → approved → implemented → verified)
- **Model Performance Trend** — champion model metrics over time with retraining markers

### 5.10.16 Software Realization
Analytics service with feedback loops:
```
Scheduled/Event Triggers → Analytics Engine (trend analysis, optimization)
→ Domain Service (ImprovementAggregate, LearningLoop)
→ Rule Engine (improvement rules, rollback rules)
→ Event Store → Projections → Read Model
```
The analytics engine queries all other capabilities via their read models and APIs. Improvements that require model retraining interface with the Model Registry and training pipeline. Threshold changes are applied via a configuration service.

---

# Chapter 6 — External Interfaces

## 6.1 Purpose

This chapter defines every external interface that the Demand Intelligence domain exposes to other domains, external systems, and users. Each interface is specified with its purpose, contract, authentication, and the capability that owns it. This chapter is derived from the Commands, Queries, and Events defined in Chapter 5.

## 6.2 Enterprise APIs

### 6.2.1 Demand Data Ingestion API

| Attribute | Value |
|-----------|-------|
| Owner | Understand Demand (5.1) |
| Purpose | Accept demand transactions and demand signals from source systems. |
| Protocol | REST (HTTPS) |
| Authentication | OAuth 2.0 (Client Credentials) |
| Rate Limit | 10,000 requests/minute |
| Endpoint | `POST /api/v1/demand/signals` |

**Request Body:**
```json
{
  "source": "POS_System",
  "batchId": "uuid",
  "signals": [
    {
      "productId": "SKU123",
      "locationId": "LOC01",
      "customerId": "CUST456",
      "quantity": 25,
      "unit": "EA",
      "timestamp": "2026-06-28T10:23:00Z",
      "signalType": "POS",
      "orderReference": "ORD-98765"
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

**Error Codes:** 400 (Invalid payload), 401 (Unauthorized), 429 (Rate limit exceeded).

---

### 6.2.2 Demand Query API

| Attribute | Value |
|-----------|-------|
| Owner | Understand Demand (5.1) |
| Purpose | Serve cleansed demand history and current demand snapshots. |
| Protocol | REST (HTTPS) |
| Authentication | OAuth 2.0 (Client Credentials) |
| Endpoint | `GET /api/v1/demand/history` |

**Query Parameters:** `productId`, `locationId`, `startDate`, `endDate`, `aggregation` (DAILY, WEEKLY).

**Response (200 OK):**
```json
{
  "productId": "SKU123",
  "locationId": "LOC01",
  "buckets": [
    {
      "periodStart": "2026-06-22",
      "demandQuantity": 110,
      "isCleansed": true,
      "flags": []
    }
  ]
}
```

---

### 6.2.3 Forecast Query API

| Attribute | Value |
|-----------|-------|
| Owner | Forecast Demand (5.2) |
| Purpose | Retrieve current published forecasts for downstream planning. |
| Protocol | REST (HTTPS) |
| Authentication | OAuth 2.0 |
| Endpoint | `GET /api/v1/forecasts` |

**Query Parameters:** `productId`, `locationId`, `startDate`, `endDate`.

**Response:**
```json
{
  "forecastId": "FC-2026-06-28-001",
  "productId": "SKU123",
  "locationId": "LOC01",
  "generatedAt": "2026-06-28T06:00:00Z",
  "modelId": "M042",
  "confidenceScore": 93,
  "buckets": [
    {
      "periodStart": "2026-07-05",
      "mean": 250,
      "lowerBound": 200,
      "upperBound": 300
    }
  ]
}
```

---

### 6.2.4 Forecast Override API

| Attribute | Value |
|-----------|-------|
| Owner | Forecast Demand (5.2) |
| Purpose | Submit a manual forecast override. |
| Protocol | REST (HTTPS) |
| Authentication | OAuth 2.0 (User) |
| Endpoint | `POST /api/v1/forecasts/overrides` |

**Request Body:**
```json
{
  "forecastId": "FC-2026-06-28-001",
  "productId": "SKU123",
  "locationId": "LOC01",
  "periodStart": "2026-07-05",
  "overrideValue": 500,
  "justification": "Confirmed large one-time order from Customer X"
}
```

**Response (201 Created):**
```json
{
  "overrideId": "OVR-501",
  "status": "approved"
}
```

**Error Codes:** 422 (Rule violation, justification missing or deviation exceeded).

---

### 6.2.5 Exception Query API

| Attribute | Value |
|-----------|-------|
| Owner | Detect Demand Exceptions (5.8) |
| Purpose | Retrieve active and historical exceptions. |
| Protocol | REST (HTTPS) |
| Endpoint | `GET /api/v1/exceptions` |

**Response:**
```json
{
  "exceptionId": "EX-1034",
  "type": "LevelShift",
  "severity": "Critical",
  "productId": "PROD-Y",
  "detectedAt": "2026-06-28T09:32:00Z",
  "status": "active"
}
```

---

### 6.2.6 Segmentation & Classification API

| Attribute | Value |
|-----------|-------|
| Owner | Segment Demand (5.4), Classify Demand (5.5) |
| Purpose | Retrieve current segment and pattern assignments. |
| Endpoint | `GET /api/v1/demand/metadata/{productId}` |

**Response:**
```json
{
  "productId": "SKU123",
  "segment": "A-X-Gold",
  "pattern": "Seasonal-Trend",
  "recommendedModel": "Holt-Winters"
}
```

---

### 6.2.7 Explanation API

| Attribute | Value |
|-----------|-------|
| Owner | Explain Demand (5.9) |
| Purpose | Retrieve structured explanation for any artifact. |
| Endpoint | `GET /api/v1/explanations/{artifactId}` |

**Response:**
```json
{
  "artifactId": "FC-2026-06-28-001",
  "type": "Forecast",
  "naturalLanguage": "Forecast for SKU123 is 250 units because...",
  "causalFactors": [
    { "factor": "Seasonal uplift", "contribution": 45 },
    { "factor": "Promotion", "contribution": 28 }
  ],
  "explainabilityScore": 88
}
```

---

## 6.3 Integration Events

Demand Intelligence publishes events to an enterprise event bus (Kafka topic: `demand-intelligence-events`). All events use the CloudEvents v1.0 envelope.

| Event Type | Payload Summary | Publisher Capability | Consumers |
|------------|-----------------|---------------------|-----------|
| `ForecastPublished` | Forecast ID, cycle ID, timestamp | Forecast Demand | Supply, Promise, Scenario |
| `ForecastOverridden` | Forecast ID, old value, new value, planner ID | Forecast Demand | Evaluate Quality, Explain |
| `DemandChangeDetected` | Product, severity, magnitude | Sense Demand | Forecast Demand, Detect Exceptions |
| `ExceptionDetected` | Exception ID, type, item | Detect Exceptions | Explain, Learn |
| `ExceptionResolved` | Exception ID, resolution | Detect Exceptions | Learn |
| `SegmentMasterPublished` | Version, coverage | Segment Demand | Prioritize, Classify, Forecast |
| `DemandPatternClassified` | Product, pattern, confidence | Classify Demand | Forecast Demand |
| `PriorityListPublished` | Version, coverage | Prioritize Demand | Detect Exceptions, Planners |
| `QualityReportPublished` | Period, WAPE, flags | Evaluate Quality | Learn, Management |
| `ImprovementRecommended` | Type, target, benefit | Learn From Demand | Model Training Pipeline |
| `LearningLoopClosed` | Improvement ID, verdict | Learn From Demand | All stakeholders |
| `DecisionExplanationGenerated` | Decision ID, traceability chain | Explain Demand | Audit, AI agents |

---

## 6.4 Import Interfaces

Batch data loading from external files or systems.

| Interface | Format | Frequency | Target Capability |
|-----------|--------|-----------|-------------------|
| Product Master Import | CSV / JSON via SFTP | Daily | Understand Demand (via Core master data) |
| Customer Master Import | CSV | Daily | Understand Demand |
| Promotion Calendar Import | CSV | Weekly | Understand Demand |
| Historical Demand Load | Parquet | One‑time / ad‑hoc | Understand Demand |

All imports are validated, cleansed, and ingested via the Demand Data Ingestion API or a dedicated batch ingestion service.

---

## 6.5 Export Interfaces

Data pushed to external consumers.

| Interface | Format | Frequency | Source Capability |
|-----------|--------|-----------|-------------------|
| Forecast Export to ERP | CSV | After each cycle | Forecast Demand |
| Demand Plan Export to S&OP | Excel | Monthly | Understand + Forecast |
| Exception Report Distribution | PDF | Daily | Detect Exceptions |
| Quality Report Distribution | PDF / Email | Weekly | Evaluate Quality |

---

# Chapter 7 — Reports & Dashboards

## 7.1 Purpose
This chapter consolidates every report and dashboard defined across the ten Demand Intelligence capabilities. Each entry includes its purpose, source capability, audience, frequency, and key content. This catalogue enables consistent implementation and ensures every stakeholder receives the right information at the right time.

---

## 7.2 Reports

### 7.2.1 Demand Data Quality Report
| Attribute | Value |
|-----------|-------|
| Report ID | RPT‑DI‑001 |
| Source Capability | Understand Demand (5.1) |
| Purpose | Summarize the completeness, timeliness, and quality of incoming demand signals and demand history. |
| Audience | Demand Data Steward, Demand Manager |
| Frequency | Daily, Weekly |
| Content | Signal acceptance rate by source, outlier counts, data completeness %, data freshness (age), signal latency distribution. |
| Format | PDF, CSV |
| Delivery | Email, Dashboard embed |

### 7.2.2 Signal Source Performance Report
| Attribute | Value |
|-----------|-------|
| Report ID | RPT‑DI‑002 |
| Source Capability | Understand Demand (5.1) |
| Purpose | Rank signal sources by reliability, latency, and volume. |
| Audience | Demand Data Steward, IT Integration |
| Frequency | Weekly |
| Content | Source name, total signals, accepted %, average latency, trend. |
| Format | PDF |

### 7.2.3 Forecast Accuracy Report
| Attribute | Value |
|-----------|-------|
| Report ID | RPT‑DI‑003 |
| Source Capability | Evaluate Demand Quality (5.7) |
| Purpose | Standard accuracy metrics report for the official forecast. |
| Audience | Demand Planner, Demand Manager, Supply Planner, Executive |
| Frequency | Weekly, Monthly |
| Content | Overall WAPE, MAPE, bias; breakdown by product family, segment, location; trend vs. prior period. |
| Format | PDF, Excel |
| Delivery | Email, Dashboard |

### 7.2.4 Forecast Value Added (FVA) Report
| Attribute | Value |
|-----------|-------|
| Report ID | RPT‑DI‑004 |
| Source Capability | Evaluate Demand Quality (5.7) |
| Purpose | Show incremental value contributed by each forecasting process step. |
| Audience | Demand Manager, Supply Chain Director |
| Frequency | Monthly |
| Content | WAPE(Naive), WAPE after each step (statistical, override), FVA per step, items with negative FVA. |
| Format | PDF |

### 7.2.5 Model Champion Report
| Attribute | Value |
|-----------|-------|
| Report ID | RPT‑DI‑005 |
| Source Capability | Forecast Demand (5.2), Evaluate Demand Quality (5.7) |
| Purpose | Compare champion and challenger model performance. |
| Audience | Demand Manager, Data Science |
| Frequency | Monthly, on champion change |
| Content | Model name, WAPE, bias, stability, significance test results, promotion/demotion history. |
| Format | PDF |

### 7.2.6 Override Analysis Report
| Attribute | Value |
|-----------|-------|
| Report ID | RPT‑DI‑006 |
| Source Capability | Forecast Demand (5.2), Evaluate Demand Quality (5.7) |
| Purpose | Monitor manual forecast overrides and their impact on accuracy. |
| Audience | Demand Manager, Demand Planner |
| Frequency | Weekly, Monthly |
| Content | Override count, % of items overridden, value‑adding vs. value‑destroying %, planner bias trends, override justifications audit. |
| Format | PDF, Excel |

### 7.2.7 Planner Performance Scorecard
| Attribute | Value |
|-----------|-------|
| Report ID | RPT‑DI‑007 |
| Source Capability | Evaluate Demand Quality (5.7) |
| Purpose | Individual planner override effectiveness and response metrics. |
| Audience | Demand Manager, Planner (own scorecard) |
| Frequency | Monthly, Quarterly |
| Content | Override count, value‑add rate, bias detection flags, average response time to exceptions, SLA compliance. |
| Format | PDF |

### 7.2.8 Change Detection Report
| Attribute | Value |
|-----------|-------|
| Report ID | RPT‑DI‑008 |
| Source Capability | Sense Demand (5.3) |
| Purpose | Summarize demand changes detected and response effectiveness. |
| Audience | Demand Planner, Demand Manager |
| Frequency | Weekly |
| Content | Count of changes by severity, detection‑to‑response time, false positive rate, forecast refreshes triggered. |
| Format | PDF |

### 7.2.9 Segmentation Distribution Report
| Attribute | Value |
|-----------|-------|
| Report ID | RPT‑DI‑009 |
| Source Capability | Segment Demand (5.4) |
| Purpose | Distribution of products/customers across segments. |
| Audience | Demand Manager, Supply Chain |
| Frequency | Monthly |
| Content | Count and volume % by ABC, XYZ, strategic segment; unclassified items. |
| Format | PDF, Excel |

### 7.2.10 Segment Migration Report
| Attribute | Value |
|-----------|-------|
| Report ID | RPT‑DI‑010 |
| Source Capability | Segment Demand (5.4) |
| Purpose | Track items that changed segment between periods. |
| Audience | Demand Manager |
| Frequency | Monthly |
| Content | Items moved, old segment, new segment, reason. |
| Format | PDF |

### 7.2.11 Demand Pattern Summary Report
| Attribute | Value |
|-----------|-------|
| Report ID | RPT‑DI‑011 |
| Source Capability | Classify Demand (5.5) |
| Purpose | Distribution of demand patterns across products. |
| Audience | Demand Planner, Data Science |
| Frequency | Monthly |
| Content | Count and % by pattern (Continuous, Intermittent, Lumpy, Seasonal, Trend), confidence distribution. |
| Format | PDF |

### 7.2.12 Model Recommendation Compliance Report
| Attribute | Value |
|-----------|-------|
| Report ID | RPT‑DI‑012 |
| Source Capability | Classify Demand (5.5), Forecast Demand (5.2) |
| Purpose | Compare recommended model vs. actual model used. |
| Audience | Demand Manager |
| Frequency | Monthly |
| Content | Adherence %, items with deviation, justifications. |
| Format | PDF |

### 7.2.13 Priority Distribution Report
| Attribute | Value |
|-----------|-------|
| Report ID | RPT‑DI‑013 |
| Source Capability | Prioritize Demand (5.6) |
| Purpose | Distribution of items across priority levels. |
| Audience | Demand Planner, Demand Manager |
| Frequency | Weekly |
| Content | Critical / High / Medium / Low counts by product family and customer. |
| Format | PDF |

### 7.2.14 Priority Override Analysis Report
| Attribute | Value |
|-----------|-------|
| Report ID | RPT‑DI‑014 |
| Source Capability | Prioritize Demand (5.6) |
| Purpose | Audit manual priority overrides. |
| Audience | Demand Manager |
| Frequency | Monthly |
| Content | Override count, justification review, trend. |
| Format | PDF |

### 7.2.15 Exception Summary Report
| Attribute | Value |
|-----------|-------|
| Report ID | RPT‑DI‑015 |
| Source Capability | Detect Demand Exceptions (5.8) |
| Purpose | Overview of exceptions raised, resolved, and outstanding. |
| Audience | Demand Planner, Demand Manager |
| Frequency | Daily, Weekly |
| Content | Count by type and severity, resolution rate, average resolution time, SLA compliance. |
| Format | PDF |

### 7.2.16 SLA Compliance Report
| Attribute | Value |
|-----------|-------|
| Report ID | RPT‑DI‑016 |
| Source Capability | Detect Demand Exceptions (5.8) |
| Purpose | Measure adherence to exception resolution SLAs. |
| Audience | Demand Manager, Supply Chain Director |
| Frequency | Monthly |
| Content | SLA attainment % by severity, trends, worst offenders. |
| Format | PDF |

### 7.2.17 Explainability Score Report
| Attribute | Value |
|-----------|-------|
| Report ID | RPT‑DI‑017 |
| Source Capability | Explain Demand (5.9) |
| Purpose | Measure the completeness and quality of generated explanations. |
| Audience | Demand Manager, Data Science |
| Frequency | Monthly |
| Content | Average explainability score, % incomplete explanations, breakdown by capability. |
| Format | PDF |

### 7.2.18 Unexplained Items Report
| Attribute | Value |
|-----------|-------|
| Report ID | RPT‑DI‑018 |
| Source Capability | Explain Demand (5.9) |
| Purpose | List of forecast/decision items with missing or low‑score explanations. |
| Audience | Demand Manager |
| Frequency | Weekly |
| Content | Item ID, type, missing explanation reason. |
| Format | PDF |

### 7.2.19 Continuous Improvement Report
| Attribute | Value |
|-----------|-------|
| Report ID | RPT‑DI‑019 |
| Source Capability | Learn From Demand (5.10) |
| Purpose | Track all proposed and implemented improvements with results. |
| Audience | Demand Manager, Supply Chain Director |
| Frequency | Monthly, Quarterly |
| Content | Improvement type, description, expected benefit, actual benefit, status, learning loop closure. |
| Format | PDF |

### 7.2.20 Model Health Report
| Attribute | Value |
|-----------|-------|
| Report ID | RPT‑DI‑020 |
| Source Capability | Learn From Demand (5.10) |
| Purpose | Monitor champion model performance and degradation alerts. |
| Audience | Data Science, Demand Manager |
| Frequency | Monthly |
| Content | Model performance trend, degradation alerts, retraining schedule, drift metrics. |
| Format | PDF |

---

## 7.3 Dashboards

### 7.3.1 Demand Health Dashboard
| Attribute | Value |
|-----------|-------|
| Dashboard ID | DASH‑DI‑001 |
| Source Capabilities | Understand Demand, Sense Demand |
| Purpose | Real‑time view of data ingestion health and demand picture freshness. |
| Audience | Demand Data Steward, Demand Manager |
| Refresh | Near real‑time (1 min) |
| Panels | Signal acceptance gauge, data freshness indicator (age), completeness % by source, error/outlier trend, active signal streams. |

### 7.3.2 Forecast Performance Dashboard
| Attribute | Value |
|-----------|-------|
| Dashboard ID | DASH‑DI‑002 |
| Source Capabilities | Evaluate Demand Quality |
| Purpose | Standard forecast accuracy KPIs with drill‑down. |
| Audience | Demand Planner, Demand Manager |
| Refresh | Daily |
| Panels | WAPE trend (actual vs. target), MAPE, bias gauge, accuracy by segment/family, stability chart. |

### 7.3.3 Forecast Confidence Dashboard
| Attribute | Value |
|-----------|-------|
| Dashboard ID | DASH‑DI‑003 |
| Source Capabilities | Forecast Demand |
| Purpose | Monitor confidence scores and auto‑publication rates. |
| Audience | Demand Planner, Demand Manager |
| Refresh | Daily |
| Panels | Confidence score distribution, auto‑ vs. manual publication %, items below confidence threshold. |

### 7.3.4 FVA Waterfall Dashboard
| Attribute | Value |
|-----------|-------|
| Dashboard ID | DASH‑DI‑004 |
| Source Capabilities | Evaluate Demand Quality |
| Purpose | Visual breakdown of forecast value added/lost at each step. |
| Audience | Demand Manager, Supply Chain Director |
| Refresh | Monthly |
| Panels | Waterfall chart (Naive → Statistical → Override → Final), FVA trend, steps with negative FVA highlighted. |

### 7.3.5 Planner Scorecard Dashboard
| Attribute | Value |
|-----------|-------|
| Dashboard ID | DASH‑DI‑005 |
| Source Capabilities | Evaluate Demand Quality |
| Purpose | Individual planner override performance. |
| Audience | Demand Manager, Planners (own view) |
| Refresh | Weekly |
| Panels | Value‑add rate, override count, bias direction indicator, SLA response time. |

### 7.3.6 Real‑Time Demand Dashboard
| Attribute | Value |
|-----------|-------|
| Dashboard ID | DASH‑DI‑006 |
| Source Capabilities | Sense Demand |
| Purpose | Live demand deviation monitoring. |
| Audience | Demand Planner |
| Refresh | Near real‑time (1 min) |
| Panels | Deviation gauge (σ), alert feed (Critical/Significant), affected product list, refresh trigger log. |

### 7.3.7 Signal Health Monitor
| Attribute | Value |
|-----------|-------|
| Dashboard ID | DASH‑DI‑007 |
| Source Capabilities | Sense Demand, Understand Demand |
| Purpose | Signal ingestion latency and source reliability. |
| Audience | IT Operations, Demand Data Steward |
| Refresh | 5 min |
| Panels | Signal latency distribution, source reliability scores, duplicate/rejected signal trend. |

### 7.3.8 Segmentation Overview Dashboard
| Attribute | Value |
|-----------|-------|
| Dashboard ID | DASH‑DI‑008 |
| Source Capabilities | Segment Demand |
| Purpose | Distribution of ABC, XYZ, and strategic segments. |
| Audience | Demand Manager, Supply Chain |
| Refresh | Monthly |
| Panels | Pie charts (ABC, XYZ, Strategic), segment coverage %, unclassified items alert. |

### 7.3.9 Segment Health Dashboard
| Attribute | Value |
|-----------|-------|
| Dashboard ID | DASH‑DI‑009 |
| Source Capabilities | Segment Demand |
| Purpose | Monitor segment stability and data completeness. |
| Audience | Demand Manager |
| Refresh | Monthly |
| Panels | Segment churn rate, items reclassified this month, data completeness for segmentation. |

### 7.3.10 Demand Pattern Map
| Attribute | Value |
|-----------|-------|
| Dashboard ID | DASH‑DI‑010 |
| Source Capabilities | Classify Demand |
| Purpose | Visual map of demand patterns across product hierarchy. |
| Audience | Demand Planner, Data Science |
| Refresh | Monthly |
| Panels | Treemap of patterns by product family, pattern counts, confidence histogram. |

### 7.3.11 Model Recommendation Dashboard
| Attribute | Value |
|-----------|-------|
| Dashboard ID | DASH‑DI‑011 |
| Source Capabilities | Classify Demand, Forecast Demand |
| Purpose | Show recommended model vs. actual model usage. |
| Audience | Demand Manager |
| Refresh | Monthly |
| Panels | Adherence rate, items with deviation, model performance comparison. |

### 7.3.12 Priority Heatmap
| Attribute | Value |
|-----------|-------|
| Dashboard ID | DASH‑DI‑012 |
| Source Capabilities | Prioritize Demand |
| Purpose | Visual representation of priority across product/customer dimensions. |
| Audience | Demand Planner, Demand Manager |
| Refresh | Weekly |
| Panels | Heatmap (Products × Customers), priority distribution bar, override flags. |

### 7.3.13 Planner Workload Dashboard
| Attribute | Value |
|-----------|-------|
| Dashboard ID | DASH‑DI‑013 |
| Source Capabilities | Prioritize Demand, Detect Exceptions |
| Purpose | Show volume of Critical/High items per planner. |
| Audience | Demand Manager |
| Refresh | Daily |
| Panels | Items per planner by priority, open exceptions, overdue items. |

### 7.3.14 Exception Monitor
| Attribute | Value |
|-----------|-------|
| Dashboard ID | DASH‑DI‑014 |
| Source Capabilities | Detect Demand Exceptions |
| Purpose | Live feed and management of active exceptions. |
| Audience | Demand Planner, Demand Manager |
| Refresh | Near real‑time (5 min) |
| Panels | Exception queue by severity, aging (time open), resolution actions, escalation log. |

### 7.3.15 Exception Analytics Dashboard
| Attribute | Value |
|-----------|-------|
| Dashboard ID | DASH‑DI‑015 |
| Source Capabilities | Detect Demand Exceptions |
| Purpose | Trend analysis of exception frequency and resolution effectiveness. |
| Audience | Demand Manager |
| Refresh | Daily |
| Panels | Exceptions over time by type, false positive trend, resolution time distribution, SLA attainment. |

### 7.3.16 Explainability Overview Dashboard
| Attribute | Value |
|-----------|-------|
| Dashboard ID | DASH‑DI‑016 |
| Source Capabilities | Explain Demand |
| Purpose | Overall explainability health. |
| Audience | Demand Manager, Data Science |
| Refresh | Weekly |
| Panels | Average explainability score, trend, % low‑score items, breakdown by capability. |

### 7.3.17 Drill‑Down Explanation Viewer
| Attribute | Value |
|-----------|-------|
| Dashboard ID | DASH‑DI‑017 |
| Source Capabilities | Explain Demand |
| Purpose | Interactive exploration of causal factors for a specific forecast or decision. |
| Audience | Demand Planner, Manager, AI Agent |
| Refresh | On‑demand |
| Panels | Causal factor waterfall, traceability chain visualization, explanation text. |

### 7.3.18 Learning Dashboard
| Attribute | Value |
|-----------|-------|
| Dashboard ID | DASH‑DI‑018 |
| Source Capabilities | Learn From Demand |
| Purpose | Track improvement funnel and effectiveness. |
| Audience | Demand Manager, Supply Chain Director |
| Refresh | Monthly |
| Panels | Improvement funnel (Proposed → Approved → Implemented → Verified), learning effectiveness index trend, rollback events. |

### 7.3.19 Model Performance Trend Dashboard
| Attribute | Value |
|-----------|-------|
| Dashboard ID | DASH‑DI‑019 |
| Source Capabilities | Learn From Demand |
| Purpose | Long‑term model performance monitoring. |
| Audience | Data Science, Demand Manager |
| Refresh | Weekly |
| Panels | WAPE trend per model, retraining markers, drift indicator (PSI), degradation alerts. |

---

# Chapter 8 — Appendix

## 8.1 Exception Priority Matrix

The following matrix defines the default mapping from Exception Type and Business Priority (Strategic Segment) to Exception Severity. It is referenced by DE‑DI‑081 (Prioritize Exception) in Section 5.8.

| Exception Type | Critical (Gold) | High (Silver) | Medium (Bronze) | Low (Unclassified) |
|----------------|-----------------|---------------|-----------------|---------------------|
| Level Shift    | Critical        | Critical      | High            | Medium              |
| Trend Break    | Critical        | High          | High            | Medium              |
| Model Failure  | Critical        | High          | Medium          | Medium              |
| Data Gap       | High            | High          | Medium          | Low                 |
| Outlier        | Medium          | Low           | Low             | Low                 |
| False Positive | —               | —             | —               | —                   |

**Notes:**
- False Positives are filtered by BR‑DI‑080 and policy PO‑DI‑080 before prioritization; they do not receive a severity.
- The matrix is configurable. Thresholds and mappings may be adjusted via the learning feedback loop (DE‑DI‑101) subject to policy PO‑DI‑102.

---

## 8.2 Enterprise Glossary

A consolidated glossary of all enterprise terms defined across the Demand Intelligence Specification and referenced architecture documents. Each entry includes the unique identifier where applicable.

| Term | ID (if any) | Definition |
|------|-------------|------------|
| ABC Classification | — | Volume‑based segmentation: A (top 80%), B (next 15%), C (bottom 5%). |
| Actual Quantity | — | The real customer demand recorded for a given planning bucket. |
| Alternative Validation Rule | — | A rule category that validates newly discovered decision alternatives before they enter the standard catalogue. |
| Automation Policy | — | A policy category defining when AI‑generated recommendations may be executed automatically. |
| Business Objective | BO‑DI‑xxx | A desired business outcome for the Demand Intelligence domain. |
| Capability | CA‑DI‑xxx | A reusable enterprise reasoning ability that transforms Enterprise Meaning into Enterprise Understanding. |
| Customer Tier | SE‑DI‑031 | Classification of customers by business importance. |
| Decision | DE‑DI‑xxx | A governed enterprise choice derived from Enterprise Understanding. |
| Demand | SE‑DI‑001 | A customer's expressed or inferred need for a product at a location in a time bucket. |
| Demand Cannibalisation | SE‑DI‑064 | Reduction in demand for one product due to introduction/promotion of another. |
| Demand Classification | — | Assignment of a demand pattern (Continuous, Intermittent, Lumpy, Seasonal, Trend, Stationary) to a demand series. |
| Demand Correlation | SE‑DI‑062 | Statistical association between demand for different items. |
| Demand Disaggregation | SE‑DI‑061 | Proportional allocation of a higher‑level forecast to lower levels. |
| Demand Exception | SE‑DI‑015 | A demand observation or forecast behaviour that deviates from expected norms. |
| Demand History | SE‑DI‑004 | Recorded actual demand quantities, cleansed and adjusted. |
| Demand Pattern | SE‑DI‑011 | Recognisable temporal behaviour of demand (Continuous, Intermittent, Lumpy, Seasonal, Trend, Stationary). |
| Demand Plan | SE‑DI‑005 | The agreed‑upon set of forecasts used by downstream planning processes. |
| Demand Priority | SE‑DI‑014 | Relative ranking of demand by business importance. |
| Demand Satisfaction Rate | PI‑DI‑015 | Percentage of total demand quantity satisfied (immediate + backorder within window). |
| Demand Segmentation | SE‑DI‑013 | Grouping of products/customers by demand characteristics (ABC, XYZ, Strategic). |
| Demand Signal | SE‑DI‑002 | Observable indicator of future or current demand. |
| Demand Variability | SE‑DI‑012 | Degree of fluctuation in demand, measured by coefficient of variation (CV). |
| Discovered Alternative | — | A new decision alternative proposed by the Learn or Evaluate primitive, not yet in the standard catalogue. |
| Enterprise Meaning | — | Formal definition of concepts that exist within the enterprise (Semantic Model). |
| Enterprise Understanding | — | Actionable knowledge about the enterprise, produced by a Capability. |
| Exception Type | — | Classification of an exception: Outlier, Level Shift, Trend Break, Model Failure, Data Gap. |
| Explainability Score | PI‑DI‑025 | Measure of completeness and quality of generated explanations. |
| Forecast | SE‑DI‑003 | Projection of future demand with mean and prediction interval. |
| Forecast Accuracy | PI‑DI‑002 | 100 − WAPE. |
| Forecast Bias | PI‑DI‑005 | Systematic tendency to over‑ or under‑forecast. |
| Forecast Confidence | SE‑DI‑022 | Scalar value (0‑100%) expressing forecast reliability. |
| Forecast Cycle | SE‑DI‑024 | Periodic execution of the forecasting process. |
| Forecast Horizon | SE‑DI‑025 | Future time span covered by a forecast. |
| Forecast Model | SE‑DI‑020 | Algorithm or method that generates forecasts. |
| Forecast Override | SE‑DI‑023 | Manual adjustment to a system‑generated forecast. |
| Forecast Stability | PI‑DI‑007 | Degree to which forecasts for the same period change between cycles. |
| Forecast Value Added (FVA) | PI‑DI‑006 | Improvement in forecast accuracy over a naive benchmark. |
| Forecast Value Realization | PI‑DI‑008 | Ratio of actual business value achieved to potential value with perfect forecasting. |
| Frozen Period | SE‑DI‑053 | Near‑term interval where the demand plan is fixed. |
| Intelligence Domain | — | Architectural owner of enterprise questions and related capabilities (e.g., Demand Intelligence). |
| Lead Time | SE‑DI‑052 | Time between demand recognition and earliest possible fulfilment. |
| Learning Loop | — | Process of implementing an improvement and verifying its benefit. |
| MAPE | PI‑DI‑004 | Mean Absolute Percentage Error. |
| Model Evaluation Rule | — | A rule category that governs model champion/challenger selection and model performance thresholds. |
| Naive Forecast | — | Lag‑1 persistence forecast used as FVA benchmark. |
| OTIF | PI‑DI‑012 | On Time In Full — percentage of order lines delivered complete and on time. |
| Order Fill Rate | PI‑DI‑011 | Percentage of orders completely filled from stock at first attempt. |
| Perfect Order Rate | PI‑DI‑013 | Percentage of orders delivered without any error. |
| Performance Indicator | PI‑DI‑xxx | A measurable value that quantifies achievement of a business objective. |
| Planning Horizon | SE‑DI‑051 | Total future span for which forecasts are generated. |
| Prediction Interval | SE‑DI‑021 | Range around the forecast mean expected to contain actual demand with specified probability. |
| Primitive Capability | — | Smallest reusable enterprise reasoning ability (Observe, Understand, Assess, Predict, Evaluate, Learn). |
| Product | SE‑DI‑040 | Distinct sellable item (SKU). |
| Product Family | SE‑DI‑041 | Group of related products. |
| Product Life‑Cycle Stage | SE‑DI‑042 | Phase of a product's market life (Introduction, Growth, Maturity, Decline, End‑of‑Life). |
| Rule (Enterprise Rule) | BR‑DI‑xxx | Declarative statement of enterprise knowledge that validates, derives, calculates, or constrains. |
| Policy (Enterprise Policy) | PO‑DI‑xxx | Declarative statement of enterprise governance that authorizes, approves, delegates, or enforces. |
| Semantic Object | SE‑DI‑xxx | A concept formally defined in the Semantic Foundation. |
| Service Level | PI‑DI‑010 | Percentage of demand fulfilled within the agreed service time window. |
| Ship‑To Location | SE‑DI‑033 | Physical destination for customer delivery. |
| Substitutability | SE‑DI‑043 | Degree to which one product can replace another. |
| Time Bucket | SE‑DI‑050 | Smallest time interval for planning and forecasting. |
| WAPE | PI‑DI‑003 | Weighted Absolute Percentage Error. |
| XYZ Classification | — | Variability‑based segmentation: X (CV ≤ 0.5), Y (0.5 < CV ≤ 1.0), Z (CV > 1.0). |

---

## 8.3 Formula Reference

Complete set of formulas used in Chapter 3 (Enterprise Measurement Model).

### PI‑DI‑002 — Forecast Accuracy
```
Forecast Accuracy (%) = 100 − WAPE (%)
```
Where WAPE is computed as defined in PI‑DI‑003.

### PI‑DI‑003 — Weighted Absolute Percentage Error (WAPE)
```
WAPE (%) = ( Σ |Forecast Quantity − Actual Quantity| ÷ Σ Actual Quantity ) × 100
```
Variables:
- `Forecast Quantity` — Forecast value for a planning bucket.
- `Actual Quantity` — Actual demand for the same bucket.
- Summation across all evaluated planning buckets.

### PI‑DI‑004 — Mean Absolute Percentage Error (MAPE)
For each bucket:
```
Percentage Error (%) = ( |Forecast Quantity − Actual Quantity| ÷ Actual Quantity ) × 100
MAPE (%) = Σ Percentage Error ÷ Number of Planning Buckets
```
If Actual Quantity = 0, the bucket is handled per enterprise policy.

### PI‑DI‑005 — Forecast Bias
```
Forecast Bias (units) = Σ (Forecast Quantity − Actual Quantity) ÷ Number of Planning Buckets
Forecast Bias (%) = ( Forecast Bias ÷ Average Actual Quantity ) × 100
Average Actual Quantity = Σ Actual Quantity ÷ Number of Planning Buckets
```

### PI‑DI‑006 — Forecast Value Added (FVA)
```
FVA (pp) = WAPE(Naive) − WAPE(Process)
```
Where `WAPE(Naive)` uses lag‑1 persistence forecasts and `WAPE(Process)` uses the evaluated process step forecasts.

### PI‑DI‑007 — Forecast Stability
```
Stability Error(t) = |Forecast(t, cycle c) − Forecast(t, cycle c−1)| ÷ Forecast(t, cycle c−1)
Forecast Stability (%) = ( Σ Stability Error(t) ÷ Number of Target Periods ) × 100
```

### PI‑DI‑008 — Forecast Value Realization
```
Forecast Value Realization (%) = ( Actual Composite Value ÷ Maximum Composite Value ) × 100
```
Where composite value is defined by enterprise policy (default: weighted sum of Service Level, Inventory Efficiency, Cost Efficiency).

### PI‑DI‑009 — Demand Plan Adherence
```
Demand Plan Adherence (%) = ( Quantity Executed Per Plan ÷ Total Planned Quantity ) × 100
```
Where `Quantity Executed Per Plan` sums planned quantities that were executed within a tolerance band.

### PI‑DI‑010 — Service Level
```
Service Level (%) = ( Quantity Fulfilled Within Service Window ÷ Total Quantity Demanded ) × 100
```

### PI‑DI‑011 — Order Fill Rate
```
Order Fill Rate (%) = ( Number of Orders Completely Filled ÷ Total Number of Orders ) × 100
```

### PI‑DI‑012 — On Time In Full (OTIF)
```
OTIF (%) = ( Number of Order Lines Delivered OTIF ÷ Total Number of Order Lines ) × 100
```
An order line is OTIF if Delivered Quantity = Ordered Quantity AND Delivery Date ≤ Agreed Delivery Date.

### PI‑DI‑013 — Perfect Order Rate
```
Perfect Order Rate (%) = ( Number of Perfect Orders ÷ Total Number of Orders ) × 100
```
A Perfect Order satisfies all perfection criteria (item, quantity, location, time, documentation, condition).

### PI‑DI‑014 — Customer Request Fulfilment Rate
```
Customer Request Fulfilment Rate (%) = ( Number of Requests Fulfilled ÷ Total Number of Requests ) × 100
```
A request is fulfilled if the full quantity is delivered within the requested/agreed time.

### PI‑DI‑015 — Demand Satisfaction Rate
```
Demand Satisfaction Rate (%) = ( Total Quantity Satisfied ÷ Total Quantity Demanded ) × 100
```
Total Quantity Satisfied = Immediate Fulfilment + Backorder Fulfilment (within window).

---

## 8.4 References

### Architecture Documents
- Medhavi APS Constitution
- Architecture Reference Standard (ARS) v1
- Semantic Model
- Capability Model
- Decision Model
- Rule & Policy Model

### Intelligence Specification
- Demand Intelligence Specification (this document)

### Dependency Specifications
- Supply Intelligence Specification (future)
- Promise Intelligence Specification (future)
- Scenario Intelligence Specification (future)
- Knowledge Intelligence Specification (future)

---
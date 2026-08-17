# Enterprise Semantic Model

**Status:** Authorized
**Supersedes:** Core Semantic Model v1
**Traceability:** CN-001, CN-003, CN-004, CN-006, ARS §2, §3, §16

---

# Chapter 1 – Foundation

## 1.1 Purpose
The Enterprise Semantic Model defines the authoritative, enterprise-wide business concepts shared by all Medhavi Intelligence domains.

## 1.2 Scope
Enterprise-wide objects used across multiple domains. Domain-specific concepts live in their respective Domain Semantic Models.

## 1.3 Architectural Position
```
Constitution
        ↓
Architecture Reference Standard
        ↓
Enterprise Semantic Model
        ↓
Capability Model
        ↓
Domain Specifications
        ↓
Implementation
```

## 1.4 Enterprise Semantic Principles
- Single Semantic Ownership
- Enterprise Vocabulary
- Semantic Stability
- Reference before Behavior
- Consumer Completeness

---

# Chapter 2 – Enterprise Semantic Architecture

## 2.1 Semantic Layers
Enterprise Semantic Model → Domain Semantic Model → Capability Consumption

## 2.2 Enterprise Vocabulary Rule
A concept belongs here if its meaning is enterprise-wide and independent of any single Intelligence domain.

## 2.3 Semantic Ownership Model

| Ownership Dimension | Question                                                                         |
| ------------------- | -------------------------------------------------------------------------------- |
| Semantic Authority  | Who defines what this concept means?                                             |
| Steward Domain      | Which domain evolves the definition?                                             |
| Mutation Authority  | Which authoritative mutation archetype governs creation and change of instances? |
| Primary Consumers   | Which domains and capabilities depend on it?                                     |

## 2.4 Semantic Dependency Principles
- Every algorithm input has one authoritative source.
- Policies govern interpretation; they never own enterprise facts.
- Capabilities depend only on published enterprise knowledge.

## 2.5 Semantic Completeness Standard
A semantic object is complete when all mandatory ARS contracts are satisfied, every declared consumer can implement without inventing semantics, and no unresolved ownership or placeholders remain.

## 2.6 Mutation Authority Archetypes

The Enterprise Semantic Model does not assign concrete capabilities. Capability assignment belongs to the Capability Model.

Each Enterprise Semantic Object declares a Mutation Authority archetype. The Capability Model later maps concrete capabilities to these archetypes.

| Archetype | Meaning |
| --- | --- |
| External System of Record | The authoritative instance source exists outside the enterprise boundary. The enterprise consumes governed changes through integration. |
| Global Reference Standard | The concept adopts an externally governed global standard, such as IANA, ISO, or ISO4217. |
| Enterprise-Governed Master Data | The enterprise governs, maintains, and owns the master definition internally. |
| Enterprise-Derived Planning Fact | The enterprise constructs the object internally from observations, events, or domain intelligence. |
| Enterprise-Governed Transactional State | The enterprise creates or mutates the object internally as the result of a governed planning, promise, governance, or execution transaction. |
| Not Applicable | The object is an immutable semantic value contract and has no governed instance mutation. |

## 2.7 Lifecycle Transition Trigger Standard

Lifecycle transitions in the Enterprise Semantic Model are governed by archetype-based business triggers.

The Capability Model shall map Aggregate Behaviors to these triggers. The Semantic Model does not name Aggregate Behaviors.

| Mutation Authority Archetype | Business Trigger |
| --- | --- |
| External System of Record | External Master Data Change Accepted |
| Global Reference Standard | Reference Standard Adoption Changed |
| Enterprise-Governed Master Data | Governed Stewardship Change Approved |
| Enterprise-Derived Planning Fact | Domain Observation Accepted |
| Enterprise-Governed Transactional State | Planning Transaction Committed |
| Not Applicable | Not Applicable |

A lifecycle transition is valid only when it is caused by the Business Trigger associated with the object’s Mutation Authority archetype.

## 2.8 Consumer Declaration Standard

A consumer may depend on an Enterprise Semantic Object only if the consumption is declared.

At enterprise level, declared consumers are governed in Chapter 5.2, the Declared Consumer Matrix.

At domain level, each consuming Domain Semantic Model must declare:

- the consuming capability
- the business responsibility for consuming the object
- the required attributes
- the interpretation constraints

Until a consumer declaration exists, the consumer shall not implement against the Enterprise Semantic Object.

Enterprise-level Required Attributes are:

1. The immutable identifier of the object.
2. All mandatory attributes in the object’s Information Model.
3. Attributes required by declared structural relationships.

Domain-specific required attributes must be declared in the consuming Domain Semantic Model.

---

# Chapter 3 – Enterprise Semantic Patterns

| Pattern               | Purpose                                      | ARS Reference |
| --------------------- | -------------------------------------------- | ------------- |
| Snapshot              | Point-in-time capture of enterprise facts    | ARS §3.4      |
| Continuous Assessment | Continuously maintained interpretation       | ARS App. A.2  |
| Published Knowledge   | Versioned, periodically published assessment | ARS App. A.3  |
| Observation           | Immutable record of received fact            | ARS App. A.4  |

No additional enterprise semantic patterns are defined beyond those ratified in the Architecture Reference Standard.

---

# Chapter 4 – Enterprise Semantic Catalogue

## 4.1 Aggregate Roots

### SE-C-001 – Item

**Business Intent:** Provide the single authoritative enterprise identity for any distinct thing the enterprise procures, manufactures, stores, sells, or uses.

**Enterprise Meaning:** An Item is an enterprise-recognised entity that is planned, procured, produced, stored, sold, or consumed. It answers “what is the thing we deal with?”. The item identity is immutable; its structural classification and business roles may evolve under governance. An item may be sellable, consumable, or both; those are business roles, not separate identities.

**Identity:** Item Identifier is the immutable enterprise identity of the Item.

**Applied Semantic Patterns:** Aggregate Root

**Semantic Ownership**

- Item owns: enterprise identity, base unit of measure, structural classification, business roles, lifecycle.
- Item excludes: inventory positions, costs, planning parameters, role-specific operational details.

**Authority Specification Contract**

| Section                      | Value                                                                                                        |
| ---------------------------- | ------------------------------------------------------------------------------------------------------------ |
| Semantic Authority           | Core Domain                                                                                                  |
| Steward Domain               | Core                                                                                                         |
| Mutation Authority           | External System of Record                                                                                    |
| Authoritative Representation | The enterprise definition of items.                                                                          |
| Authority Scope              | Enterprise-wide                                                                                              |
| Intended Consumers           | All capabilities that reference any enterprise item.                                                         |
| Non-Intended Consumers       | None                                                                                                         |
| Business Responsibility      | Preserve the authoritative enterprise meaning, identity, lifecycle integrity, and published contract of this semantic object. |
| Supersedes                   | None                                                                                                         |
| Superseded By                | None                                                                                                         |

**Consumer Specification Contract**

| Section                 | Value                                                                                                                                                                 |
| ----------------------- | --------------------------------------------------------------------------------------------------------------------------------------------------------------------- |
| Business Guarantees     | Every Item has a permanent, unique identifier. Structural type and business roles are governed classifications; they may change over time without affecting identity. |
| Required Interpretation | Consumers shall reference the Item solely by its immutable identifier. Roles indicate how the enterprise classifies the item for business purposes.                   |
| Known Limitations       | Does not define role-specific operational parameters.                                                                                                                 |
| Declared Consumers      | Governed by Chapter 5.2 Declared Consumer Matrix.                                                                                                                     |
| Consumer Responsibility | The consumer shall reference this object solely by its immutable identifier and shall not infer semantics not published in this contract.                          |
| Required Attributes     | Immutable identifier; mandatory attributes; attributes required by declared structural relationships. Domain-specific required attributes must be declared in the consuming Domain Semantic Model. |
| Authoritative Source    | SE-C-001 Item.                                                                                                                                                        |

**Lifecycle Specification Contract**

| State    | Description                                           |
| -------- | ----------------------------------------------------- |
| Active   | Item is recognised and may be referenced.             |
| Inactive | Item is temporarily not available for new references. |
| Retired  | Item is permanently removed from enterprise use.      |

- Permitted Transitions: Active ↔ Inactive; Active → Retired; Inactive → Retired.
- Terminal State: Retired.
- History Preservation: State changes recorded for audit.

**Transition Trigger Standard:** Transitions are triggered only by the Business Trigger associated with this object’s Mutation Authority archetype, as defined in Section 2.7. Capability specifications shall map Aggregate Behaviors to these triggers.

**Information Model**

| Attribute                      | Type                                                                                | Mandatory | Description                                                                                                                                                                                                                    |
| ------------------------------ | ----------------------------------------------------------------------------------- | --------- | ------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------ |
| Item Identifier                | ID (immutable)                                                                      | Yes       | Unique enterprise identity.                                                                                                                                                                                                    |
| Enterprise Business Identifier | String                                                                              | No        | Human-readable business code.                                                                                                                                                                                                  |
| Item Name                      | String                                                                              | Yes       | Enterprise-recognised name.                                                                                                                                                                                                    |
| Item Type                      | Governed Identifier Reference (SE-C-037)                                            | No        | Structural classification of the item. This describes what the item is physically or functionally; it is not a business role.                                                                                                   |
| Item Roles                     | List of Governed Identifier References (SE-C-037)                                   | No        | Business roles assigned to this item (e.g., “Sellable”, “Consumable”, “Transferable”, “Manufacturable”). The set of recognised roles is governed by the Steward Domain. A single item may carry multiple roles simultaneously. |
| Unit of Measure                | Reference (UnitOfMeasure)                                                           | Yes       | Base unit.                                                                                                                                                                                                                     |
| Lifecycle State                | Enum (Active, Inactive, Retired)                                                    | Yes       | Current state.                                                                                                                                                                                                                 |

**Relationships**

| Relationship  | Target Object                                                                       | Cardinality  | Description                                              |
| ------------- | ----------------------------------------------------------------------------------- | ------------ | -------------------------------------------------------- |
| quantified in | Unit of Measure                                                                     | Many-to-One  | Base unit.                                               |
| classified by | Enterprise Governed Vocabulary (SE-C-037)                                           | Many-to-Many | The governed roles this item plays in the enterprise.    |
| referenced by | Bill of Materials, Inventory, Demand, Supply, Commitment, Enterprise Picture | One-to-Many  | All downstream objects reference the same Item identity. |

**Invariants**
- Item Identifier is immutable.
- Item Name and Unit of Measure are mandatory.
- A Retired Item cannot be referenced by new objects.

**Dependencies**

| Dependency Type       | Description      |
| --------------------- | ---------------- |
| Semantic Dependency   | Unit of Measure. |
| Conceptual Dependency | None.            |
| SE-C-037 |  Enterprise Governed Vocabulary |

**Decomposition Review:** Item remained a single enterprise concept. Structural type and business roles are governed classifications, not independent objects. No decomposition required.

**Traceability**

| Direction | Reference                                                                              |
| --------- | -------------------------------------------------------------------------------------- |
| Upward    | Constitution CN-003, CN-004; ARS §3, §4, §16; Enterprise Semantic Model Chapter 4.      |
| Admission | Enterprise Vocabulary admission record.                                                 |
| Downward  | Declared consuming domains and capabilities.                                            |

---

### SE-C-002 – Location

**Business Intent:** Provide the single authoritative enterprise identity and core descriptive attributes for any physical place the enterprise uses for planning, production, storage, distribution, or delivery.

**Enterprise Meaning:** A Location is a distinct physical site that the enterprise recognises. It answers “where” for all planning and execution activities. Its identity is immutable. The concept is independent of how a site is classified and of any operational details. Address information is a downstream concern owned by operational systems.

**Identity:** Location Identifier is the immutable enterprise identity of the Location.

**Applied Semantic Patterns:** Aggregate Root

**Semantic Ownership**

- **Location owns:**
  - Physical sites: plants, distribution centres, warehouses, depots, terminals, retail outlets, customer delivery points, supplier pickup points, ports, cross-dock facilities.
  - Sites that have a permanent physical presence and an enterprise-recognised identity.
- **Location excludes:**
  - Logical network nodes (Node).
  - Transportation lanes between locations (Transportation Lane).
  - Regions, zones, or territories that group multiple locations.
  - Operational attributes such as inventory levels, capacity, operating hours.
  - Address details — these belong to operational master data, not the enterprise semantic identity of a location.

**Authority Specification Contract**

| Section                      | Value                                                                                  |
| ---------------------------- | -------------------------------------------------------------------------------------- |
| Semantic Authority           | Enterprise Reference Data                                                              |
| Steward Domain               | Core                                                                                   |
| Mutation Authority           | External System of Record                                                              |
| Authoritative Representation | The enterprise definition of what physical sites exist and their governing attributes. |
| Authority Scope              | Enterprise-wide.                                                                       |
| Intended Consumers           | All Intelligence domains.                                                              |
| Non-Intended Consumers       | None.                                                                                  |
| Business Responsibility      | Preserve the authoritative enterprise meaning, identity, lifecycle integrity, and published contract of this semantic object. |
| Supersedes                   | None.                                                                                  |
| Superseded By                | None.                                                                                  |

**Consumer Specification Contract**

| Section                 | Value                                                                                                                                                                                                                  |
| ----------------------- | ---------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------- |
| Business Guarantees     | The Location identifier is permanent and unique across the enterprise. Core attributes such as name, type, and time zone are accurate and governed. No planning capability may redefine a location’s identity or type. |
| Required Interpretation | Consumers shall reference locations solely by their immutable identifier. Any operational meaning associated with a location shall be obtained from the owning domain objects.                                         |
| Known Limitations       | Does not contain address details, planning-specific parameters, or inventory positions. Location lifecycle transitions do not automatically trigger planning actions.                                                  |
| Version Expectations    | Not versioned; the record reflects the current state of the location. Historical state changes are preserved by the Steward Domain.                                                                                    |
| Freshness Expectations  | Location records are maintained continuously by the Steward Domain.                                                                                                                                                    |
| Intended Consumers      | All capabilities.                                                                                                                                                                                                      |
| Non-Intended Consumers  | None.                                                                                                                                                                                                                  |
| Declared Consumers      | Governed by Chapter 5.2 Declared Consumer Matrix.                                                                                                                                                                      |
| Consumer Responsibility | The consumer shall reference this object solely by its immutable identifier and shall not infer semantics not published in this contract.                                                                           |
| Required Attributes     | Immutable identifier; mandatory attributes; attributes required by declared structural relationships. Domain-specific required attributes must be declared in the consuming Domain Semantic Model.                  |
| Authoritative Source    | SE-C-002 Location.                                                                                                                                                                                                     |

**Lifecycle Specification Contract**

| State    | Description                                                                                                             |
| -------- | ----------------------------------------------------------------------------------------------------------------------- |
| Active   | Location is operational and available for planning.                                                                     |
| Inactive | Location is temporarily suspended. It may return to Active.                                                             |
| Closed   | Location is permanently removed from enterprise operations. No new supply, demand, or inventory plans may reference it. |

**Permitted Transitions and Semantic Triggers:**
- Active → Inactive — Location temporarily suspended from operations.
- Inactive → Active — Location reinstated to operations.
- Active → Closed — Location permanently decommissioned.
- Inactive → Closed — A suspended location is permanently closed.
- **Terminal State:** Closed.
- **History Preservation:** State changes recorded.

**Transition Trigger Standard:** Transitions are triggered only by the Business Trigger associated with this object’s Mutation Authority archetype, as defined in Section 2.7. Capability specifications shall map Aggregate Behaviors to these triggers.

**Information Model**

| Attribute           | Type                                                                                                         | Mandatory | Description                                     |
| ------------------- | ------------------------------------------------------------------------------------------------------------ | --------- | ----------------------------------------------- |
| Location Identifier | ID (immutable)                                                                                               | Yes       | Unique enterprise identity.                     |
| Location Name       | String                                                                                                       | Yes       | Descriptive name.                               |
| Location Type       | Enum (Plant, DistributionCenter, Warehouse, Store, CustomerSite, SupplierSite, Port, Depot, Terminal, Other) | Yes       | Classification of the physical site.            |
| Time Zone           | Reference (TimeZone)                                                                                         | Yes       | The IANA time zone applicable to this location. |
| Lifecycle State     | Enum (Active, Inactive, Closed)                                                                              | Yes       | Current state.                                  |

**Relationships**

| Relationship    | Target Object                                                                          | Cardinality | Description                                                                   |
| --------------- | -------------------------------------------------------------------------------------- | ----------- | ----------------------------------------------------------------------------- |
| uses            | Time Zone (SE-C-031)                                                                   | Many-to-One | Governs local time interpretation for the location.                           |
| referenced by   | Transportation Lane, Network, Demand, Supply, Inventory, Commitment, Physical Resource | One-to-Many | Planning and resource objects reference Location by its immutable identifier. |
| parent Location | Location                                                                               | Zero-or-One | Optional hierarchical grouping.                                               |

**Invariants**
- Location Identifier is immutable once assigned.
- Location Name must be unique among Active locations.
- A Location in Closed state must not be referenced by any new planning transaction.
- A Location cannot transition from Closed to Active.

**Dependencies:** SE-C-031 Time Zone.

**Decomposition Review:** No decomposition required.

**Traceability:** CN-003, CN-004; ARS §3, §4, §16.
| Direction | Reference                                                                          |
| --------- | ---------------------------------------------------------------------------------- |
| Upward    | Constitution CN-003, CN-004; ARS §3, §4, §16; Enterprise Semantic Model Chapter 4. |
| Admission | Phase 5 Enterprise Vocabulary, admitted as Aggregate Root Candidate.               |
| Downward  | All Intelligence domain specifications; Capability Model.                          |


### SE-C-003 – Customer

**Business Intent:** Provide the single authoritative enterprise identity for any external entity that places demand, receives commitments, or participates in commercial relationships with the enterprise.

**Enterprise Meaning:** A Customer is an external party recognized by the enterprise as a recipient of products, services, or commercial obligations. Customer identity is immutable. Its core descriptive attributes and lifecycle are governed by the Core Domain. Planning capabilities reference the customer to associate demand, orders, and commitments. A customer is not defined by its current commercial relationship; it is the enduring enterprise identity of the counterparty.

**Identity:** Customer Identifier is the immutable enterprise identity of the Customer.

**Applied Semantic Patterns:** Aggregate Root

**Semantic Ownership**

- **Customer owns:**
  - External commercial entities (businesses, government bodies, consumers).
  - Entities that receive products, services, or commitments.
- **Customer excludes:**
  - Internal organisational units (those are Business Unit, if modelled).
  - Suppliers (Supplier is a separate Aggregate Root).
  - Contact details, address books, or operational account information (these are domain-specific extensions or operational master data).

**Authority Specification Contract**

| Section                      | Value                                                                                                                      |
| ---------------------------- | -------------------------------------------------------------------------------------------------------------------------- |
| Semantic Authority           | Core Domain                                                                                                                |
| Steward Domain               | Core                                                                                                                       |
| Mutation Authority           | External System of Record                                                                                                  |
| Authoritative Representation | The enterprise definition of customers.                                                                                    |
| Authority Scope              | Enterprise-wide.                                                                                                           |
| Intended Consumers           | Demand Intelligence, Promise Intelligence, Supply Intelligence (for allocation and prioritisation), Scenario Intelligence. |
| Non-Intended Consumers       | None.                                                                                                                      |
| Business Responsibility      | Preserve the authoritative enterprise meaning, identity, lifecycle integrity, and published contract of this semantic object. |
| Supersedes                   | None.                                                                                                                      |
| Superseded By                | None.                                                                                                                      |

**Consumer Specification Contract**

| Section                 | Value                                                                                                                                                                                              |
| ----------------------- | -------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------- |
| Business Guarantees     | Every Customer has a permanent, unique identifier. Core attributes are governed and accurate.                                                                                                      |
| Required Interpretation | Consumers shall reference the Customer solely by its immutable identifier. Operational details such as credit, contact, or account hierarchy belong to domain objects that reference the Customer. |
| Known Limitations       | Does not include operational account data, delivery preferences, or contractual terms. Those are maintained by domain capabilities.                                                                |
| Version Expectations    | Not versioned; the record reflects the current state. Historical changes are preserved by the Steward Domain.                                                                                      |
| Freshness Expectations  | Maintained continuously by the Steward Domain.                                                                                                                                                     |
| Intended Consumers      | All capabilities that reference a customer.                                                                                                                                                        |
| Non-Intended Consumers  | None.                                                                                                                                                                                              |
| Declared Consumers      | Governed by Chapter 5.2 Declared Consumer Matrix.                                                                                                                                                  |
| Consumer Responsibility | The consumer shall reference this object solely by its immutable identifier and shall not infer semantics not published in this contract.                                                       |
| Required Attributes     | Immutable identifier; mandatory attributes; attributes required by declared structural relationships. Domain-specific required attributes must be declared in the consuming Domain Semantic Model. |
| Authoritative Source    | SE-C-003 Customer.                                                                                                                                                                                 |

**Lifecycle Specification Contract**

| State    | Description                                                                                                            |
| -------- | ---------------------------------------------------------------------------------------------------------------------- |
| Active   | Customer is recognised and may participate in commercial relationships.                                                |
| Inactive | Customer is temporarily suspended (e.g., credit hold, dormant).                                                        |
| Retired  | A retired customer shall not be referenced by new enterprise transaction. Existing historical references remain valid. |

- **Terminal State:** Retired.
- **History Preservation:** State changes recorded.
- **Versioning Rules:** Not applicable.

**Transition Trigger Standard:** Transitions are triggered only by the Business Trigger associated with this object’s Mutation Authority archetype, as defined in Section 2.7. Capability specifications shall map Aggregate Behaviors to these triggers.

**Information Model**

| Attribute           | Type                             | Mandatory | Description                 |
| ------------------- | -------------------------------- | --------- | --------------------------- |
| Customer Identifier | ID (immutable)                   | Yes       | Unique enterprise identity. |
| Customer Name       | String                           | Yes       | The recognised name.        |
| Customer Class      | Enum (A, B, C, D)                | No        | Optional classification.    |
| Lifecycle State     | Enum (Active, Inactive, Retired) | Yes       | Current state.              |

**Relationships**

| Relationship  | Target Object             | Cardinality | Description                              |
| ------------- | ------------------------- | ----------- | ---------------------------------------- |
| referenced by | Demand, Commitment | One-to-Many | Planning objects reference the customer. |

**Invariants:** Identifier immutable; Retired Customer cannot be referenced by new transactions.

**Dependencies:**

| Dependency Type       | Description |
| --------------------- | ----------- |
| Semantic Dependency   | None.       |
| Conceptual Dependency | None.       |

**Traceability:** CN-003, CN-004; ARS §3, §4, §16.

| Direction | Reference                                                                              |
| --------- | -------------------------------------------------------------------------------------- |
| Upward    | Constitution CN-003, CN-004; ARS §3, §4, §16; Enterprise Semantic Model Chapter 4.     |
| Admission | Phase 5 Enterprise Vocabulary, admitted as Aggregate Root Candidate.                   |
| Downward  | Demand Intelligence, Promise Intelligence, Supply Intelligence, Scenario Intelligence. |


### SE-C-004 – Supplier

**Business Intent:** Provide the single authoritative enterprise identity for every external party that the enterprise recognises as a source of products, materials, services, capabilities, or other obligations required to operate the business.

**Enterprise Meaning:** A Supplier is an enterprise-recognised external party that is capable of fulfilling one or more obligations to the enterprise. It establishes **who provides**, independently of what is provided, where from, under which commercial terms, how it performs, or which planning capability consumes it. The Supplier identity is the enduring enterprise anchor for all enterprise relationships involving external provision.

**Identity:** Supplier Identifier is the immutable enterprise identity of the Supplier.

**Applied Semantic Patterns:** Aggregate Root

**Semantic Ownership**

- **Supplier owns:**
  - The authoritative enterprise identity of an external party.
  - Enterprise recognition of that external party.
  - The enduring identity used consistently across all planning domains.
  - The semantic anchor for external enterprise relationships.

- **Supplier excludes:**
  - Contracts, commercial agreements, procurement terms, pricing.
  - Supplier performance, risk, quality.
  - Approved supplier lists, supplier locations, contact information, banking information.
  - Planning relationships and material sourcing relationships (these belong to specialised semantic objects or Domain Semantic Models).

**Authority Specification Contract**

| Section                      | Value                                                       |
| ---------------------------- | ----------------------------------------------------------- |
| Semantic Authority           | Core Domain                                                 |
| Steward Domain               | Core                                                        |
| Mutation Authority           | External System of Record                                   |
| Authoritative Representation | The enterprise definition of recognised external suppliers. |
| Authority Scope              | Enterprise-wide                                             |
| Intended Consumers           | All domains requiring identification of external providers  |
| Non-Intended Consumers       | None                                                        |
| Business Responsibility      | Preserve the authoritative enterprise meaning, identity, lifecycle integrity, and published contract of this semantic object. |
| Supersedes                   | None                                                        |
| Superseded By                | None                                                        |

**Consumer Specification Contract**

| Section                 | Value                                                                                                                                                                                                                                                                                 |
| ----------------------- | ------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------- |
| Business Guarantees     | Every Supplier has one immutable enterprise identity, unique across the enterprise, and stable regardless of commercial or operational relationships. Every consumer references the same enterprise Supplier.                                                                         |
| Required Interpretation | Consumers shall interpret Supplier solely as the enterprise identity of an external provider. Consumers shall not infer contracts, capabilities, approved products, locations, commercial relationships, planning behavior, or operational status. Those semantics belong elsewhere. |
| Known Limitations       | Supplier defines identity only. It does not determine what may be supplied, whether supply is approved, whether supply is currently active, how the supplier performs, or whether the supplier is selected for planning.                                                              |
| Declared Consumers      | Governed by Chapter 5.2 Declared Consumer Matrix.                                                                                                                                                                                                                                     |
| Consumer Responsibility | The consumer shall reference this object solely by its immutable identifier and shall not infer semantics not published in this contract.                                                                                                                                             |
| Required Attributes     | Immutable identifier; mandatory attributes; attributes required by declared structural relationships. Domain-specific required attributes must be declared in the consuming Domain Semantic Model.                                                                                    |
| Authoritative Source    | SE-C-004 Supplier.                                                                                                                                                                                                                                                                    |

**Lifecycle Specification Contract**

| State    | Description                                                                                                           |
| -------- | --------------------------------------------------------------------------------------------------------------------- |
| Active   | Supplier identity is available for new enterprise relationships.                                                      |
| Inactive | Supplier identity is retained but unavailable for new enterprise relationships.                                       |
| Retired  | Supplier identity is permanently closed to new enterprise relationships and retained solely for historical integrity. |

- **Permitted Transitions:** Active ↔ Inactive; Active → Retired; Inactive → Retired.
- **Terminal State:** Retired.
- **History Preservation:** State changes are recorded for audit.
- **Versioning Rules:** Not applicable.

**Transition Trigger Standard:** Transitions are triggered only by the Business Trigger associated with this object’s Mutation Authority archetype, as defined in Section 2.7. Capability specifications shall map Aggregate Behaviors to these triggers.

**Information Model**

| Attribute           | Type                             | Mandatory | Description                          |
| ------------------- | -------------------------------- | --------- | ------------------------------------ |
| Supplier Identifier | ID (immutable)                   | Yes       | Unique enterprise identity.          |
| Supplier Name       | String                           | Yes       | The enterprise name of the supplier. |
| Lifecycle State     | Enum (Active, Inactive, Retired) | Yes       | Current semantic state.              |

**Relationships**

| Relationship  | Target Object          | Cardinality | Description                              |
| ------------- | ---------------------- | ----------- | ---------------------------------------- |
| referenced by | Commitment (SE-C-017)  | One-to-Many | Inbound commitments reference Supplier.  |

**Invariants:**
- Supplier Identifier is immutable.
- Supplier identity is unique within the enterprise.
- A Supplier cannot exist without a recognised identity.
- Enterprise identity shall never change during the lifetime of the Supplier.

**Dependencies:**

| Dependency Type       | Description |
| --------------------- | ----------- |
| Semantic Dependency   | None.       |
| Conceptual Dependency | None.       |


**Traceability:** CN-003, CN-004; ARS §3, §4, §16.

| Direction | Reference                                                           |
| --------- | ------------------------------------------------------------------- |
| Upward    | Constitution, ARS, Enterprise Semantic Model                        |
| Admission | Phase 5 Enterprise Vocabulary, admitted as Aggregate Root Candidate |
| Downward  | All Intelligence domain specifications that reference a supplier    |


### SE-C-005 – Resource Group

**Business Intent:** Provide the enterprise identity for a governed collection of resources that are planned, managed, or operated as a single planning unit.

**Enterprise Meaning:** A Resource Group is an enterprise object that defines a set of physical resources that share a common planning context. It may own a calendar, planning policies, and provide an aggregate capacity view for rough-cut or finite capacity planning. The group is not merely a container; it is the planning boundary within which resource capacity is collectively understood. Its membership is governed; the group itself does not have independent capacity but represents the aggregate of its members.

**Identity:** Resource Group Identifier is the immutable enterprise identity of the Resource Group.

**Applied Semantic Patterns:** Aggregate Root

**Semantic Ownership**

- **Resource Group owns:**
  - Named planning groups such as work centers, production lines, labor pools.
  - The planning calendar and policies that apply collectively to its members.

- **Resource Group excludes:**
  - Individual resource identities (Physical Resource).
  - Resource capability definitions (Standard Resource).
  - Capacity values (capacity is derived from members by planning capabilities, not stored on the group).

**Authority Specification Contract**

| Section                      | Value                                                                             |
| ---------------------------- | --------------------------------------------------------------------------------- |
| Semantic Authority           | Enterprise Resource Governance Authority                                          |
| Steward Domain               | Core                                                                              |
| Mutation Authority           | Enterprise-Governed Master Data                                                            |
| Authoritative Representation | The enterprise definition of resource groupings for planning.                     |
| Authority Scope              | Enterprise-wide.                                                                  |
| Intended Consumers           | Supply Planning, Capacity Planning, Production Scheduling, Scenario Intelligence. |
| Non-Intended Consumers       | None.                                                                             |
| Business Responsibility      | Preserve the authoritative enterprise meaning, identity, lifecycle integrity, and published contract of this semantic object. |
| Supersedes                   | None.                                                                             |
| Superseded By                | None.                                                                             |

**Consumer Specification Contract**

| Section                 | Value                                                                                                                                                                                               |
| ----------------------- | --------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------- |
| Business Guarantees     | Every Resource Group has a unique, immutable identifier and a governed membership.                                                                                                                  |
| Required Interpretation | Consumers shall use the group to reference a set of resources for aggregate planning. The group’s capacity is understood as the sum of its members’ capacities, not an independently defined value. |
| Known Limitations       | Does not own capacity directly. The group’s calendar is a default for its members; individual members may reference a different calendar.                                                           |
| Version Expectations    | Not versioned; membership changes are governed separately.                                                                                                                                          |
| Freshness Expectations  | Membership and calendar are maintained by the Steward Domain.                                                                                                                                       |
| Intended Consumers      | Planning capabilities.                                                                                                                                                                              |
| Non-Intended Consumers  | None.                                                                                                                                                                                               |
| Declared Consumers      | Governed by Chapter 5.2 Declared Consumer Matrix.                                                                                                                                                   |
| Consumer Responsibility | The consumer shall reference this object solely by its immutable identifier and shall not infer semantics not published in this contract.                                                         |
| Required Attributes     | Immutable identifier; mandatory attributes; attributes required by declared structural relationships. Domain-specific required attributes must be declared in the consuming Domain Semantic Model. |
| Authoritative Source    | SE-C-005 Resource Group.                                                                                                                                                                            |

**Lifecycle Specification Contract**

| State    | Description                                 |
| -------- | ------------------------------------------- |
| Active   | Group is in use for planning.               |
| Inactive | Group is temporarily not used for planning. |
| Retired  | Group has been permanently dissolved.       |

- **Terminal States:** Retired.

**Transition Trigger Standard:** Transitions are triggered only by the Business Trigger associated with this object’s Mutation Authority archetype, as defined in Section 2.7. Capability specifications shall map Aggregate Behaviors to these triggers.

**Information Model**

| Attribute                 | Type                                                             | Mandatory | Description                                                           |
| ------------------------- | ---------------------------------------------------------------- | --------- | --------------------------------------------------------------------- |
| Resource Group Identifier | ID (immutable)                                                   | Yes       | Unique enterprise identity.                                           |
| Resource Group Name       | String                                                           | Yes       | Descriptive name.                                                     |
| Resource Group Type       | Governed Identifier Reference (SE-C-037)                         | Yes       | Classification of the group.                                          |
| Calendar                  | Reference (Calendar)                                             | Yes       | The planning calendar that provides default availability for members. |
| Lifecycle State           | Enum (Active, Inactive, Retired)                                 | Yes       | Current state.                                                        |

**Relationships**

| Relationship | Target Object               | Cardinality | Description                                                |
| ------------ | --------------------------- | ----------- | ---------------------------------------------------------- |
| contains     | PhysicalResource (SE-C-007) | One-to-Many | The group consists of zero or more Physical Resources.     |
| governed by  | Calendar (SE-C-033)         | Many-to-One | The calendar providing default availability for the group. |

**Invariants:**
  - Resource Group Identifier is immutable.
  - A Resource Group must be associated with exactly one Calendar.
  - A retired group cannot contain active Physical Resources.

**Dependencies:**
| Dependency Type       | Description       |
| --------------------- | ----------------- |
| Semantic Dependency   | SE-C-033 Calendar |
| Conceptual Dependency | None.             |
| SE-C-037 |  Enterprise Governed Vocabulary |

**Decomposition Review:** No decomposition required.

**Traceability:** CN-003, CN-004; ARS §3, §4, §16.

| Direction | Reference                                                                               |
| --------- | --------------------------------------------------------------------------------------- |
| Upward    | Constitution CN-003, CN-004; ARS §3, §4, §16; Enterprise Semantic Model Chapter 4.      |
| Admission | Phase 5 Enterprise Vocabulary (derived from Resource decomposition).                    |
| Downward  | Supply Intelligence, Demand Intelligence, Production Scheduling, Scenario Intelligence. |


### SE-C-006 – Standard Resource

**Business Intent:** Provide the enterprise definition of a reusable production capability, specifying the type of work, reference capacity, and default calendar for a class of resource.

**Enterprise Meaning:** A Standard Resource is an enterprise object that defines what a particular kind of resource can do. It answers “what capability does this class of resource provide” and “at what nominal rate.” It is not a schedulable entity; rather, it is the reference capability from which Physical Resources may derive their default attributes. The capability itself is part of the Standard Resource’s enterprise meaning, expressed through its type and capacity.

**Identity:** Standard Resource Identifier is the immutable enterprise identity of the Standard Resource.

**Applied Semantic Patterns:** Aggregate Root

**Semantic Ownership**

- **Standard Resource owns:**
  - Capability definitions such as “High-Speed Filler”, “CNC Machining Center”, “Heavy Press”.
  - The nominal capacity rate (output per time period under standard conditions).
  - The default working calendar for this resource type.

- **Standard Resource excludes:**
  - Physical resource instances.
  - Resource groupings.
  - Location-specific details.

**Authority Specification Contract**

| Section                      | Value                                                      |
| ---------------------------- | ---------------------------------------------------------- |
| Semantic Authority           | Enterprise Resource Governance Authority                   |
| Steward Domain               | Core                                                       |
| Mutation Authority           | Enterprise-Governed Master Data                                     |
| Authoritative Representation | The enterprise definition of resource capabilities.        |
| Authority Scope              | Enterprise-wide.                                           |
| Intended Consumers           | Supply Planning, Capacity Planning, Scenario Intelligence. |
| Non-Intended Consumers       | None.                                                      |
| Business Responsibility      | Preserve the authoritative enterprise meaning, identity, lifecycle integrity, and published contract of this semantic object. |
| Supersedes                   | None.                                                      |
| Superseded By                | None.                                                      |

**Consumer Specification Contract**

| Section                 | Value                                                                                                                                                                                                       |
| ----------------------- | ----------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------- |
| Business Guarantees     | Every Standard Resource has a unique, immutable identifier and a governed capability description, reference capacity, and default calendar.                                                                 |
| Required Interpretation | Consumers shall use Standard Resources for capability-based planning. The reference capacity is a nominal value; actual available capacity is determined by the Physical Resource and the planning context. |
| Known Limitations       | Does not represent actual available capacity at a specific time; that is the role of Physical Resource and scheduling.                                                                                      |
| Version Expectations    | Not versioned; the definition may be updated by the Steward Domain.                                                                                                                                         |
| Freshness Expectations  | Maintained by the Steward Domain.                                                                                                                                                                           |
| Intended Consumers      | Planning capabilities.                                                                                                                                                                                      |
| Non-Intended Consumers  | None.                                                                                                                                                                                                       |
| Declared Consumers      | Governed by Chapter 5.2 Declared Consumer Matrix.                                                                                                                                                           |
| Consumer Responsibility | The consumer shall reference this object solely by its immutable identifier and shall not infer semantics not published in this contract.                                                                 |
| Required Attributes     | Immutable identifier; mandatory attributes; attributes required by declared structural relationships. Domain-specific required attributes must be declared in the consuming Domain Semantic Model.         |
| Authoritative Source    | SE-C-006 Standard Resource.                                                                                                                                                                                 |

**Lifecycle Specification Contract**

| State    | Description                                      |
| -------- | ------------------------------------------------ |
| Active   | Capability definition is available for planning. |
| Inactive | Definition is temporarily not used.              |
| Retired  | Definition has been permanently removed.         |

- Terminal State: Retired.

**Transition Trigger Standard:** Transitions are triggered only by the Business Trigger associated with this object’s Mutation Authority archetype, as defined in Section 2.7. Capability specifications shall map Aggregate Behaviors to these triggers.

**Information Model**

| Attribute                    | Type                                    | Mandatory | Description                                                                                                                                         |
| ---------------------------- | --------------------------------------- | --------- | --------------------------------------------------------------------------------------------------------------------------------------------------- |
| Standard Resource Identifier | ID (immutable)                          | Yes       | Unique enterprise identity.                                                                                                                         |
| Standard Resource Name       | String                                  | Yes       | Descriptive name.                                                                                                                                   |
| Capability Description       | String (or structured)                  | Yes       | The type of production capability (e.g., “Cutting”, “Assembly”, “Painting”, “Packing”). This is the enterprise meaning of what the resource can do. |
| Resource Type                | Governed Identifier Reference (SE-C-037) | Yes       | Physical classification.                                                                                                                            |
| Reference Capacity           | Capacity (Value Object)                 | Yes       | The nominal output rate under standard conditions.                                                                                                  |
| Calendar                     | Reference (Calendar)                    | Yes       | Default working pattern for this resource type.                                                                                                     |
| Lifecycle State              | Enum (Active, Inactive, Retired)        | Yes       | Current state.                                                                                                                                      |

**Relationships**

| Relationship | Target Object               | Cardinality | Description                                  |
| ------------ | --------------------------- | ----------- | -------------------------------------------- |
| realized by  | PhysicalResource (SE-C-007) | One-to-Many | Physical Resources based on this definition. |
| governed by  | Calendar (SE-C-033)         | Many-to-One | Default calendar.                            |

**Invariants:**
- Standard Resource Identifier is immutable.
- Reference Capacity must be a valid Capacity value (non-negative quantity, valid UoM, positive time period).
- A retired Standard Resource cannot be referenced as the basis for active Physical Resources.

**Dependencies:**

| Dependency Type       | Description                                                                    |
| --------------------- | ------------------------------------------------------------------------------ |
| Semantic Dependency   | SE-C-033 Calendar, SE-C-026 Capacity, SE-C-032 Unit of Measure (via Capacity). |
| Conceptual Dependency | None.                                                                          |
| SE-C-037 |  Enterprise Governed Vocabulary |

**Traceability:** CN-003, CN-004; ARS §3, §4, §16.

| Direction | Reference                                                                          |
| --------- | ---------------------------------------------------------------------------------- |
| Upward    | Constitution CN-003, CN-004; ARS §3, §4, §16; Enterprise Semantic Model Chapter 4. |
| Admission | Phase 5 Enterprise Vocabulary (derived from Resource decomposition).               |
| Downward  | Supply Intelligence, Capacity Planning, Scenario Intelligence.                     |


### SE-C-007 – Physical Resource

**Business Intent:** Provide the enterprise identity for an actual, schedulable resource with a specific location, calendar, and capacity.

**Enterprise Meaning:** A Physical Resource is the real machine, line, cell, or labor pool that can be assigned work. It has a location, a calendar that governs its availability, and a capacity that defines its output potential. It may be associated with a Resource Group (for planning context) and may be based on a Standard Resource (from which it inherits default attributes). The resource’s assigned calendar and capacity are direct attributes; any resolution between group, standard, and resource-specific values is a planning policy concern, not a semantic fact.

**Identity:** Physical Resource Identifier is the immutable enterprise identity of the Physical Resource.

**Applied Semantic Patterns:** Aggregate Root

**Semantic Ownership**

- **Physical Resource owns:**
  - Individual machines, production lines, work cells, labor groups.
  - Resources that can be scheduled and whose capacity can be consumed.
- **Physical Resource excludes:**
  - Abstract capability templates (Standard Resource).
  - Resource collections (Resource Group).
  - Operational states such as maintenance, downtime, or current load (these are planning facts, not part of the resource identity).

**Authority Specification Contract**

| Section                      | Value                                                                                |
| ---------------------------- | ------------------------------------------------------------------------------------ |
| Semantic Authority           | Enterprise Resource Governance Authority                                             |
| Steward Domain               | Core                                                                                 |
| Mutation Authority           | Enterprise-Governed Master Data                                                               |
| Authoritative Representation | The enterprise definition of actual, schedulable resources.                          |
| Authority Scope              | Enterprise-wide.                                                                     |
| Intended Consumers           | Production Scheduling, Capacity Assessment, Supply Execution, Scenario Intelligence. |
| Non-Intended Consumers       | None.                                                                                |
| Business Responsibility      | Preserve the authoritative enterprise meaning, identity, lifecycle integrity, and published contract of this semantic object. |
| Supersedes                   | None.                                                                                |
| Superseded By                | None.                                                                                |

**Consumer Specification Contract**

| Section                 | Value                                                                                                                                                                                                  |
| ----------------------- | ------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------ |
| Business Guarantees     | Every Physical Resource has a unique, immutable identifier, a location, a calendar, and a capacity.                                                                                                    |
| Required Interpretation | Consumers shall treat the Physical Resource as the actual entity that can be loaded. Its calendar and capacity are the constraints that scheduling must respect.                                       |
| Known Limitations       | Does not include real-time status or maintenance events. Capacity is a nominal value; available capacity is derived by planning capabilities considering the calendar and any operational constraints. |
| Version Expectations    | Not versioned; attributes may be updated by the Steward Domain.                                                                                                                                        |
| Freshness Expectations  | Maintained by the Steward Domain.                                                                                                                                                                      |
| Intended Consumers      | Scheduling and execution capabilities.                                                                                                                                                                 |
| Non-Intended Consumers  | None.                                                                                                                                                                                                  |
| Declared Consumers      | Governed by Chapter 5.2 Declared Consumer Matrix.                                                                                                                                                      |
| Consumer Responsibility | The consumer shall reference this object solely by its immutable identifier and shall not infer semantics not published in this contract.                                                           |
| Required Attributes     | Immutable identifier; mandatory attributes; attributes required by declared structural relationships. Domain-specific required attributes must be declared in the consuming Domain Semantic Model.                  |
| Authoritative Source    | SE-C-007 Physical Resource.                                                                                                                                                                            |

**Lifecycle Specification Contract**

| State    | Description                                            |
| -------- | ------------------------------------------------------ |
| Active   | Resource is operational and available for scheduling.  |
| Inactive | Resource is temporarily unavailable for scheduling.    |
| Retired  | Resource has been permanently removed from operations. |

- Terminal State: Retired.

**Transition Trigger Standard:** Transitions are triggered only by the Business Trigger associated with this object’s Mutation Authority archetype, as defined in Section 2.7. Capability specifications shall map Aggregate Behaviors to these triggers.

**Information Model**

| Attribute                    | Type                             | Mandatory | Description                                                                                        |
| ---------------------------- | -------------------------------- | --------- | -------------------------------------------------------------------------------------------------- |
| Physical Resource Identifier | ID (immutable)                   | Yes       | Unique enterprise identity.                                                                        |
| Physical Resource Name       | String                           | Yes       | Descriptive name.                                                                                  |
| Location                     | Reference (SE-C-002)             | Yes       | The physical site where the resource is located.                                                   |
| Resource Group               | Reference (SE-C-005)             | No        | The group to which this resource belongs.                                                          |
| Standard Resource            | Reference (SE-C-006)             | No        | The capability definition from which this resource’s default calendar and capacity may be derived. |
| Calendar                     | Reference (SE-C-033)             | Yes       | The calendar that governs this resource’s availability.                                            |
| Assigned Capacity            | Capacity (SE-C-026)              | Yes       | The output rate for this resource under current operating conditions.                              |
| Lifecycle State              | Enum (Active, Inactive, Retired) | Yes       | Current state.                                                                                     |

**Relationships**

| Relationship | Target Object               | Cardinality | Description            |
| ------------ | --------------------------- | ----------- | ---------------------- |
| belongs to   | ResourceGroup (SE-C-005)    | Many-to-One | Planning context.      |
| based on     | StandardResource (SE-C-006) | Many-to-One | Capability definition. |
| located at   | Location (SE-C-002)         | Many-to-One | Physical site.         |
| governed by  | Calendar (SE-C-033)         | Many-to-One | Availability.          |

**Invariants:**
- Physical Resource Identifier is immutable.
- Calendar must reference an active Calendar.
- Assigned Capacity must be a valid Capacity value.
- A retired Physical Resource cannot be assigned to new work.

**Dependencies:** SE-C-002 Location, SE-C-033 Calendar, SE-C-026 Capacity, SE-C-032 Unit of Measure (via Capacity). Optional: SE-C-005, SE-C-006.

| Dependency Type       | Description                                                                                                                                                                              |
| --------------------- | ---------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------- |
| Semantic Dependency   | Location (SE-C-002), Calendar (SE-C-033), Capacity (SE-C-026), Unit of Measure (SE-C-032). ResourceGroup and StandardResource are optional references defined in the same decomposition. |
| Conceptual Dependency | None.                                                                                                                                                                                    |

**Traceability:** CN-003, CN-004; ARS §3, §4, §16.

| Direction | Reference                                                                            |
| --------- | ------------------------------------------------------------------------------------ |
| Upward    | Constitution CN-003, CN-004; ARS §3, §4, §16; Enterprise Semantic Model Chapter 4.   |
| Admission | Phase 5 Enterprise Vocabulary (derived from Resource decomposition).                 |
| Downward  | Production Scheduling, Capacity Assessment, Supply Execution, Scenario Intelligence. |


#### Resource Hierarchy (Explicit)

```
Resource Group
    │
    ├── contains ──── Physical Resource
    │
    └── governs planning context

Standard Resource
    │
    ├── defines capability
    │
    └── realized by ──── Physical Resource

Physical Resource
    │
    ├── belongs to Resource Group (optional)
    ├── based on Standard Resource (optional)
    ├── located at Location
    ├── governed by Calendar
    └── has Assigned Capacity
```


### SE-C-008 – Transportation Lane

**Business Intent:** Provide the single authoritative enterprise identity for a governed movement path that authorises and identifies the movement of goods between two enterprise locations.

**Enterprise Meaning:** A Transportation Lane is an enterprise‑recognised, directed path that connects a source location to a destination location. It answers “which governed route links these sites” and establishes the **permitted movement relationship** between them. The lane identity is stable; operational characteristics such as lead time, transport mode, cost, capacity, minimum shipment quantity, or carrier assignment are domain‑specific extensions, not part of the core lane identity. A lane is not merely a geographic connection; it is an enterprise authorisation that goods may move along this path.

**Identity:** Transportation Lane Identifier is the immutable enterprise identity of the Transportation Lane.

**Applied Semantic Patterns:** Aggregate Root

**Semantic Ownership**

- **Transportation Lane owns:**
  - The enterprise identity of a directed movement path.
  - The recognition that a governed movement relationship exists between two locations.
  - Directionality (from-location, to-location).
- **Transportation Lane excludes:**
  - Operational planning attributes – lead time, transit duration, transport mode, capacity, cost, minimum quantity, carrier assignments, service policies.
  - Network membership – a Lane may participate in one or more Networks, but the Network owns the topology.
  - Actual shipments or movements – those are operational facts.
  - Bidirectional relationships – a reverse direction is a separate lane.

**Authority Specification Contract**

| Section                      | Value                                                                                      |
| ---------------------------- | ------------------------------------------------------------------------------------------ |
| Semantic Authority           | Core Domain                                                                                |
| Steward Domain               | Core                                                                                       |
| Mutation Authority           | External System of Record                                                                  |
| Authoritative Representation | The enterprise definition of recognised transportation lanes.                              |
| Authority Scope              | Enterprise‑wide                                                                            |
| Intended Consumers           | Supply Planning, Distribution Planning, Scenario Intelligence, Network design capabilities |
| Non‑Intended Consumers       | None                                                                                       |
| Business Responsibility      | Preserve the authoritative enterprise meaning, identity, lifecycle integrity, and published contract of this semantic object. |
| Supersedes                   | None                                                                                       |
| Superseded By                | None                                                                                       |

**Consumer Specification Contract**

| Section                 | Value                                                                                                                                                                                                                                |
| ----------------------- | ------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------ |
| Business Guarantees     | Every Transportation Lane has a unique, immutable identifier. Its identity and direction are stable regardless of operational parameter changes.                                                                                     |
| Required Interpretation | Consumers shall interpret the lane as the recognised enterprise movement relationship between two locations. Operational properties must be obtained from domain objects that reference the lane, not inferred from the lane itself. |
| Known Limitations       | Defines only the existence and direction of an authorised connection. Does not specify how, when, or at what cost goods may move, nor minimum shipment constraints.                                                                  |
| Declared Consumers      | Governed by Chapter 5.2 Declared Consumer Matrix.                                                                                                                                                                                    |
| Consumer Responsibility | The consumer shall reference this object solely by its immutable identifier and shall not infer semantics not published in this contract.                                                                                         |
| Required Attributes     | Immutable identifier; mandatory attributes; attributes required by declared structural relationships. Domain-specific required attributes must be declared in the consuming Domain Semantic Model.                                 |
| Authoritative Source    | SE-C-008 Transportation Lane.                                                                                                                                                                                                        |

**Lifecycle Specification Contract**

| State    | Description                                                                         |
| -------- | ----------------------------------------------------------------------------------- |
| Active   | Lane is authorised and available for planning.                                      |
| Inactive | Lane is temporarily unavailable for new planning references.                        |
| Retired  | Lane is permanently removed from enterprise use; retained for historical reference. |

- Permitted Transitions: Active ↔ Inactive; Active → Retired; Inactive → Retired.
- Terminal State: Retired.
- History Preservation: State changes are recorded for audit.
- Versioning Rules: Not applicable.

**Transition Trigger Standard:** Transitions are triggered only by the Business Trigger associated with this object’s Mutation Authority archetype, as defined in Section 2.7. Capability specifications shall map Aggregate Behaviors to these triggers.

**Information Model**

| Attribute                      | Type                             | Mandatory | Description                                                                                                                |
| ------------------------------ | -------------------------------- | --------- | -------------------------------------------------------------------------------------------------------------------------- |
| Transportation Lane Identifier | ID (immutable)                   | Yes       | Unique enterprise identity.                                                                                                |
| Lane Name                      | String                           | No        | Human‑readable label (e.g., “DC‑West to DC‑East”). May be derived from the from/to locations when not explicitly assigned. |
| From Location                  | Reference (Location)             | Yes       | The source location.                                                                                                       |
| To Location                    | Reference (Location)             | Yes       | The destination location.                                                                                                  |
| Lifecycle State                | Enum (Active, Inactive, Retired) | Yes       | Current semantic state.                                                                                                    |

**Relationships**

| Relationship    | Target Object       | Cardinality  | Description                                                                                                                                |
| --------------- | ------------------- | ------------ | ------------------------------------------------------------------------------------------------------------------------------------------ |
| originates at   | Location (SE-C-002) | Many-to-One  | Starting point.                                                                                                                            |
| terminates at   | Location (SE-C-002) | Many-to-One  | Ending point.                                                                                                                              |
| participates in | Network (SE-C-009)  | Many-to-Many | Transportation lane may participate in one or more enterprise Networks. Network owns the topology; the Lane owns the movement relationship |

**Invariants:**
- Transportation Lane Identifier is immutable.
- From Location and To Location must reference distinct locations.
- A lane cannot be Active if either referenced Location is retired.

**Dependencies:**

| Dependency Type       | Description        |
| --------------------- | ------------------ |
| Semantic Dependency   | SE-C-002 Location. |
| Conceptual Dependency | None.              |


**Traceability:** CN-003, CN-004; ARS §3, §4, §16.

| Direction | Reference                                                                          |
| --------- | ---------------------------------------------------------------------------------- |
| Upward    | Constitution CN‑003, CN‑004; ARS §3, §4, §16; Enterprise Semantic Model Chapter 4. |
| Admission | Phase 5 Enterprise Vocabulary, admitted as Aggregate Root Candidate.               |
| Downward  | Supply Intelligence, Distribution Planning, Scenario Intelligence.                 |


### SE-C-009 – Network

**Business Intent:** Provide the authoritative enterprise definition of a governed set of locations and the transportation lanes that connect them, forming a recognised enterprise topology.

**Enterprise Meaning:** A Network is an enterprise‑recognised topology of locations and transportation lanes that describes how goods may flow through the enterprise. It answers “what is the structure of our supply network?”. A Network is a coherent topology whose participating Transportation Lanes connect participating Locations. It is a governed enterprise definition that explicitly declares which Locations and Transportation Lanes constitute a recognised topology. Membership is enterprise knowledge, not a derived relationship. The network identity is stable; the specific locations and lanes that participate are governed and may evolve over time. Multiple networks may exist for different purposes, and a location or lane may participate in multiple networks.

**Identity:** Network Identifier is the immutable enterprise identity of the Network.

**Applied Semantic Patterns:** Aggregate Root

**Semantic Ownership**

- **Network owns:** identity, name, governed membership of locations and lanes, lifecycle.
- **Network excludes:** locations and lanes themselves, flow quantities, operational parameters.

**Authority Specification Contract**

| Section                      | Value                                                               |
| ---------------------------- | ------------------------------------------------------------------- |
| Semantic Authority           | Core Domain                                                         |
| Steward Domain               | Core                                                                |
| Mutation Authority           | Enterprise-Governed Master Data                                              |
| Authoritative Representation | The enterprise definition of network topologies.                    |
| Authority Scope              | Enterprise‑wide                                                     |
| Intended Consumers           | Any capability requiring an enterprise‑recognised network topology. |
| Non‑Intended Consumers       | None                                                                |
| Business Responsibility      | Preserve the authoritative enterprise meaning, identity, lifecycle integrity, and published contract of this semantic object. |
| Supersedes                   | None                                                                |
| Superseded By                | None                                                                |

**Consumer Specification Contract**

| Section                 | Value                                                                                                                                                                                                                          |
| ----------------------- | ------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------ |
| Business Guarantees     | Every Network has a unique, immutable identifier and a governed set of participating locations and lanes. The topology accurately reflects the enterprise‑recognised structure.                                                |
| Required Interpretation | Consumers shall interpret the network as the authoritative set of locations and connecting lanes. Operational parameters (capacities, costs, lead times) are properties of the lanes and locations, not of the network itself. |
| Known Limitations       | The Network defines the recognised topology at a point in governance. Changes occur only through governed lifecycle and membership updates. It does not define flow quantities, transport modes, or operational constraints.   |
| Declared Consumers      | Governed by Chapter 5.2 Declared Consumer Matrix.                                                                                                                                                                              |
| Consumer Responsibility | The consumer shall reference this object solely by its immutable identifier and shall not infer semantics not published in this contract.                                                                                   |
| Required Attributes     | Immutable identifier; mandatory attributes; attributes required by declared structural relationships. Domain-specific required attributes must be declared in the consuming Domain Semantic Model.                           |
| Authoritative Source    | SE-C-009 Network.                                                                                                                                                                                                              |

**Lifecycle Specification Contract**

| State    | Description                                                  |
| -------- | ------------------------------------------------------------ |
| Active   | Recognised by the enterprise as a valid topology.            |
| Inactive | Recognition temporarily suspended.                           |
| Retired  | Permanently withdrawn; retained for historical traceability. |

- Permitted Transitions: Active ↔ Inactive; Active → Retired; Inactive → Retired.
- Terminal State: Retired.
- History Preservation: Lifecycle changes are preserved as enterprise history.

**Transition Trigger Standard:** Transitions are triggered only by the Business Trigger associated with this object’s Mutation Authority archetype, as defined in Section 2.7. Capability specifications shall map Aggregate Behaviors to these triggers.

**Information Model**

| Attribute                          | Type                                          | Mandatory | Description                                                                                                                                       |
| ---------------------------------- | --------------------------------------------- | --------- | ------------------------------------------------------------------------------------------------------------------------------------------------- |
| Network Identifier                 | ID (immutable)                                | Yes       | Unique enterprise identity.                                                                                                                       |
| Network Name                       | String                                        | Yes       | Enterprise‑recognised name (e.g., “EMEA Distribution Network”).                                                                                   |
| Participating Locations            | List of Location references (1..*)            | Yes       | The locations that are part of this network. At least one location must be included.                                                              |
| Participating Transportation Lanes | List of Transportation Lane references (0..*) | No        | The lanes that connect locations within this network. All lanes must have their from‑ and to‑locations present among the participating locations. |
| Lifecycle State                    | Enum (Active, Inactive, Retired)              | Yes       | Current state.                                                                                                                                    |

**Relationships**

| Relationship      | Target Object                  | Cardinality  | Description                                                                                                                                                                                |
| ----------------- | ------------------------------ | ------------ | ------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------ |
| has participating | Location (SE-C-002)            | Many‑to‑Many | The locations that are part of this network. A location may participate in multiple networks.                                                                                              |
| has participating | Transportation Lane (SE-C-008) | Many‑to‑Many | The lanes that connect locations within this network. Each lane's from‑ and to‑locations must be among the network's participating locations. A lane may participate in multiple networks. |

**Invariants:** Identifier immutable; lanes must have both from- and to-locations in the network; Retired Network cannot be referenced by new records.

**Dependencies:** SE-C-002 Location, SE-C-008 Transportation Lane.

**Decomposition Review:** No decomposition required.

**Traceability:** CN-003, CN-004; ARS §3, §4, §16.

| Direction | Reference                                                                                 |
| --------- | ----------------------------------------------------------------------------------------- |
| Upward    | Constitution CN‑003, CN‑004; ARS §3, §4, §16; Enterprise Semantic Model Chapter 4.        |
| Admission | Phase 5 Enterprise Vocabulary, admitted as Aggregate Root Candidate.                      |
| Downward  | Downstream traceability will be established when consuming Semantic Objects are authored. |


### SE-C-010 – Planning Scope

**Business Intent:** Provide the single authoritative enterprise definition of the business boundary within which a planning activity reasons about enterprise reality.

**Enterprise Meaning:** A Planning Scope is the enterprise‑defined business boundary that establishes what part of the enterprise is included in a planning activity. It answers “what part of the enterprise are we planning?” The scope does not own the entities that participate; it defines the boundary that determines their participation. The boundary is the scope’s semantic identity.

**Identity:** Planning Scope Identifier is the immutable enterprise identity of the Planning Scope.

**Applied Semantic Patterns:** Aggregate Root

**Semantic Ownership**

- **Planning Scope owns:** identity, name, boundary definition, lifecycle.
- **Planning Scope excludes:** temporal dimensions, planning assumptions, ownership of participating entities.

**Authority Specification Contract**

| Section                      | Value                                                                                                                                                         |
| ---------------------------- | ------------------------------------------------------------------------------------------------------------------------------------------------------------- |
| Semantic Authority           | Core Domain                                                                                                                                                   |
| Steward Domain               | Core                                                                                                                                                          |
| Mutation Authority           | Enterprise-Governed Master Data                                                                                                                                        |
| Authoritative Representation | Enterprise Planning Scope                                                                                                                                     |
| Authority Scope              | Enterprise‑wide                                                                                                                                               |
| Intended Consumers           | Plan, Scenario, Supply Planning, Demand Planning, Production Planning, Distribution Planning, and any capability requiring an authoritative planning boundary |
| Non‑Intended Consumers       | None                                                                                                                                                          |
| Business Responsibility      | Preserve the authoritative enterprise meaning, identity, lifecycle integrity, and published contract of this semantic object. |
| Supersedes                   | None                                                                                                                                                          |
| Superseded By                | None                                                                                                                                                          |

**Consumer Specification Contract**

| Section                 | Value                                                                                                                                                                                       |
| ----------------------- | ------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------- |
| Business Guarantees     | Every Planning Scope represents one authoritative enterprise planning boundary. The scope is the single source of truth for determining planning participation.                             |
| Required Interpretation | Consumers shall interpret the scope as the authoritative business boundary. Participating enterprise entities are determined by evaluating the boundary against current enterprise reality. |
| Known Limitations | Planning Scope does not define temporal boundaries, planning assumptions, algorithms, or results. Boundary Statement is human-readable only; Boundary Rules are authoritative for machine evaluation. |
| Declared Consumers      | Governed by Chapter 5.2 Declared Consumer Matrix.                                                                                                                                           |
| Consumer Responsibility | The consumer shall reference this object solely by its immutable identifier and shall not infer semantics not published in this contract.                                                 |
| Required Attributes     | Immutable identifier; mandatory attributes; attributes required by declared structural relationships. Domain-specific required attributes must be declared in the consuming Domain Semantic Model. |
| Authoritative Source    | SE-C-010 Planning Scope.                                                                                                                                                                    |

**Lifecycle Specification Contract**

| State    | Description                                                  |
| -------- | ------------------------------------------------------------ |
| Active   | Available for planning activities.                           |
| Inactive | Temporarily unavailable.                                     |
| Retired  | Permanently withdrawn; retained for historical traceability. |

- Permitted Transitions: Active ↔ Inactive; Active → Retired; Inactive → Retired.
- Terminal State: Retired.
- History Preservation: All lifecycle transitions preserved.

**Transition Trigger Standard:** Transitions are triggered only by the Business Trigger associated with this object’s Mutation Authority archetype, as defined in Section 2.7. Capability specifications shall map Aggregate Behaviors to these triggers.

**Information Model**

| Attribute                 | Type                             | Mandatory | Description                                                                                                        |
| ------------------------- | -------------------------------- | --------- | ------------------------------------------------------------------------------------------------------------------ |
| Planning Scope Identifier | ID (immutable)                   | Yes       | Immutable enterprise identifier.                                                                                   |
| Scope Name                | String                           | Yes       | Enterprise‑recognised business name.                                                                               |
| Boundary Statement        | String                           | No        | Human-readable business description of the planning boundary. This is not authoritative for machine evaluation.    |
| Boundary Rules            | List of Scope Boundary Rule (SE-C-038) | Yes | Deterministic inclusion and exclusion rules for the Planning Scope. At least one rule is required.               |
| Lifecycle State           | Enum (Active, Inactive, Retired) | Yes       | Current lifecycle state.                                                                                           |

**Relationships:** None. Participation is derived by evaluating the Boundary Rules against enterprise reality; it is not stored as a direct structural relationship in the Enterprise Semantic Model.

**Invariants:** Identifier immutable; Boundary Definition must be non-empty and unique among active scopes; a boundary with no inclusion has no enterprise meaning; Retired scope cannot be referenced by new activities; Boundary Rules must contain at least one Scope Boundary Rule; Boundary Statement is not authoritative for planning participation; Planning participation is determined by evaluating Boundary Rules.

**Dependencies:** SE-C-038 Scope Boundary Rule, SE-C-037 Enterprise Governed Vocabulary via Scope Boundary Rule.

**Traceability:** CN-003, CN-004; ARS §3, §4, §16.


### SE-C-011 – Scenario

**Business Intent:** Provide the single authoritative enterprise definition of the assumptions under which enterprise planning reasons about future business reality.

**Enterprise Meaning:** A Scenario is the enterprise‑defined set of planning assumptions that establishes the business context under which enterprise planning is performed. It answers: “Under what assumptions are we planning?” A Scenario does not define the planning boundary, temporal extent, or planning results. It provides the authoritative assumptions that influence how planning interprets future enterprise reality. Multiple Scenarios may exist simultaneously, allowing evaluation of different assumptions while preserving the same Planning Scope and Planning Horizon.

**Identity:** Scenario Identifier is the immutable enterprise identity of the Scenario.

**Applied Semantic Patterns:** Aggregate Root

**Semantic Ownership**

- **Scenario owns:** identity, name, planning assumptions, lifecycle.
- **Scenario excludes:** Planning Scope, Planning Horizon, Planning Period, Calendar, algorithms, execution, results, enterprise operational facts.

**Authority Specification Contract**

| Section                      | Value                                                                                                                      |
| ---------------------------- | -------------------------------------------------------------------------------------------------------------------------- |
| Semantic Authority           | Core Domain                                                                                                                |
| Steward Domain               | Core                                                                                                                       |
| Mutation Authority           | Enterprise-Governed Master Data                                                                                                     |
| Authoritative Representation | Enterprise Scenario                                                                                                        |
| Authority Scope              | Enterprise‑wide                                                                                                            |
| Intended Consumers           | Plan, Supply Planning, Demand Planning, Production Planning, Distribution Planning, Planning Analysis, Planning Comparison |
| Non‑Intended Consumers       | Operational execution capabilities                                                                                         |
| Business Responsibility      | Preserve the authoritative enterprise meaning, identity, lifecycle integrity, and published contract of this semantic object. |
| Supersedes                   | None                                                                                                                       |
| Superseded By                | None                                                                                                                       |

**Consumer Specification Contract**

| Section                 | Value                                                                                                                                                                                             |
| ----------------------- | ------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------- |
| Business Guarantees     | Every Scenario represents one coherent set of enterprise planning assumptions.                                                                                                                    |
| Required Interpretation | Consumers shall interpret the Scenario as the authoritative assumptions under which planning is performed. Planning results shall remain traceable to the Scenario from which they were produced. |
| Known Limitations | A Scenario does not represent operational truth, enterprise commitments, or planning outcomes. Assumption Statement is human-readable only; Scenario Adjustments are authoritative for machine evaluation. |
| Declared Consumers      | Governed by Chapter 5.2 Declared Consumer Matrix.                                                                                                                                                 |
| Consumer Responsibility | The consumer shall reference this object solely by its immutable identifier and shall not infer semantics not published in this contract.                                                       |
| Required Attributes     | Immutable identifier; mandatory attributes; attributes required by declared structural relationships. Domain-specific required attributes must be declared in the consuming Domain Semantic Model. |
| Authoritative Source    | SE-C-011 Scenario.                                                                                                                                                                                |

**Lifecycle Specification Contract**

| State    | Description                                   |
| -------- | --------------------------------------------- |
| Draft    | Being prepared; may change.                   |
| Active   | Approved for planning activities.             |
| Archived | Retained for historical comparison and audit. |

- Permitted Transitions: Draft → Active; Active → Archived.
- Terminal State: Archived.
- History Preservation: All lifecycle transitions preserved.

**Transition Trigger Standard:** Transitions are triggered only by the Business Trigger associated with this object’s Mutation Authority archetype, as defined in Section 2.7. Capability specifications shall map Aggregate Behaviors to these triggers.

**Information Model**

| Attribute            | Type                           | Mandatory | Description                                                                                                        |
| -------------------- | ------------------------------ | --------- | ------------------------------------------------------------------------------------------------------------------ |
| Scenario Identifier  | ID (immutable)                 | Yes       | Immutable enterprise identifier.                                                                                   |
| Scenario Name        | String                         | Yes       | Enterprise‑recognised name.                                                                                        |
| Assumption Statement | String                         | No        | Human-readable explanation of the Scenario assumptions. This is not authoritative for machine evaluation.          |
| Scenario Adjustments | List of Scenario Adjustment (SE-C-039) | Yes | Deterministic adjustments under which planning is performed. At least one adjustment is required.               |
| Lifecycle State      | Enum (Draft, Active, Archived) | Yes       | Current lifecycle state.                                                                                           |

**Relationships**

| Relationship  | Target Object             | Cardinality | Description                                                                                                                                  |
| ------------- | ------------------------- | ----------- | -------------------------------------------------------------------------------------------------------------------------------------------- |
| applies to    | Planning Scope (SE-C-010) | Many-to-One | The Scenario provides assumptions for a specific Planning Scope. The Planning Scope is not owned by the Scenario.                            |
| referenced by | Plan (SE-C-012)           | One-to-Many | When Plan is authored, each Plan shall identify the Scenario under which it was produced. This relationship will be formalised at that time. |

**Invariants:**
- Scenario Identifier is immutable.
- Scenario Adjustments must contain at least one Scenario Adjustment.
- Assumption Statement is not authoritative for planning computation.
- Planning computation is governed by Scenario Adjustments.
- Archived Scenarios shall not be used for new activities.

**Dependencies:** SE-C-010 Planning Scope, SE-C-039 Scenario Adjustment, SE-C-037 Enterprise Governed Vocabulary via Scenario Adjustment.

**Decomposition Review:** No decomposition required.

**Traceability:** CN-003, CN-004; ARS §3, §4, §16.

| Direction | Reference                                                                              |
| --------- | -------------------------------------------------------------------------------------- |
| Upward    | Constitution CN‑003, CN‑004; ARS §3, §4, §16; Enterprise Semantic Model Chapter 4.     |
| Admission | Phase 5 Enterprise Vocabulary, admitted as Aggregate Root Candidate.                   |
| Downward  | Plan, Supply Plan, Demand Plan, Production Plan, Distribution Plan, Planning Analysis. |


### SE-C-012 – Plan

**Business Intent:** Provide the single authoritative enterprise definition of an intended future operating state established through planning.

**Enterprise Meaning:** A Plan is the enterprise-defined intended future operating state that expresses how the enterprise intends to satisfy future business objectives within a defined Planning Scope, Planning Horizon, and Scenario. It represents enterprise planning intent; it is neither a prediction nor a commitment to execution.

**Identity:** Plan Identifier is the immutable enterprise identity of the Plan.

**Applied Semantic Patterns:** Aggregate Root

**Semantic Ownership**

- **Plan owns:** identity, name, references to scope/horizon/scenario, lifecycle, planning intent.
- **Plan excludes:** algorithms, solver configuration, execution, historical facts, ownership of other enterprise objects.

**Authority Specification Contract**

| Section                      | Value                                                                                                                                     |
| ---------------------------- | ----------------------------------------------------------------------------------------------------------------------------------------- |
| Semantic Authority           | Core Domain                                                                                                                               |
| Steward Domain               | Core                                                                                                                                      |
| Mutation Authority           | Enterprise-Governed Transactional State                                                                                                            |
| Authoritative Representation | Enterprise Plan                                                                                                                           |
| Authority Scope              | Enterprise‑wide                                                                                                                           |
| Intended Consumers           | Production Planning, Procurement Planning, Distribution Planning, Inventory Planning, Scheduling, Execution Planning, Enterprise Analysis |
| Non‑Intended Consumers       | None                                                                                                                                      |
| Business Responsibility      | Preserve the authoritative enterprise meaning, identity, lifecycle integrity, and published contract of this semantic object. |
| Supersedes                   | None                                                                                                                                      |
| Superseded By                | None                                                                                                                                      |

**Consumer Specification Contract**

| Section                 | Value                                                                                           |
| ----------------------- | ----------------------------------------------------------------------------------------------- |
| Business Guarantees     | One coherent intended future enterprise operating state. Only Approved Plans are authoritative. |
| Required Interpretation | Interpret as enterprise planning intent, not execution or commitment.                           |
| Known Limitations       | Does not guarantee feasibility or operational outcome.                                          |
| Declared Consumers      | Governed by Chapter 5.2 Declared Consumer Matrix.                                               |
| Consumer Responsibility | The consumer shall reference this object solely by its immutable identifier and shall not infer semantics not published in this contract. |
| Required Attributes     | Immutable identifier; mandatory attributes; attributes required by declared structural relationships. Domain-specific required attributes must be declared in the consuming Domain Semantic Model. |
| Authoritative Source    | SE-C-012 Plan.                                                                                  |

**Lifecycle Specification Contract**

| State      | Description                                      |
| ---------- | ------------------------------------------------ |
| Draft      | Being developed; may change.                     |
| Approved   | Authorised as the authoritative planning intent. |
| Superseded | Replaced by a newer Approved Plan.               |
| Archived   | Retained for historical traceability.            |

- Permitted Transitions: Draft → Approved; Approved → Superseded; Approved → Archived; Superseded → Archived.
- Terminal State: Archived.

**Transition Trigger Standard:** Transitions are triggered only by the Business Trigger associated with this object’s Mutation Authority archetype, as defined in Section 2.7. Capability specifications shall map Aggregate Behaviors to these triggers.

**Information Model**

| Attribute        | Type                                         | Mandatory | Description                    |
| ---------------- | -------------------------------------------- | --------- | ------------------------------ |
| Plan Identifier  | ID (immutable)                               | Yes       | Immutable enterprise identity. |
| Plan Name        | String                                       | Yes       | Enterprise-recognised name.    |
| Planning Scope   | Reference (SE-C-010)                         | Yes       | Business boundary.             |
| Planning Horizon | Reference (SE-C-027)                         | Yes       | Temporal extent.               |
| Scenario         | Reference (SE-C-011)                         | Yes       | Planning assumptions.          |
| Lifecycle State  | Enum (Draft, Approved, Superseded, Archived) | Yes       | Current state.                 |

**Relationships**

| Relationship | Target Object               | Cardinality | Description        |
| ------------ | --------------------------- | ----------- | ------------------ |
| references   | Planning Scope (SE-C-010)   | Many-to-One | Scope of the Plan. |
| references   | Planning Horizon (SE-C-027) | Many-to-One | Temporal extent.   |
| references   | Scenario (SE-C-011)         | Many-to-One | Assumptions.       |

**Invariants:** Identifier immutable; all references must be valid and active; a Plan without intent has no meaning; only Approved Plans are authoritative.

**Dependencies:** SE-C-010 Planning Scope, SE-C-027 Planning Horizon, SE-C-011 Scenario.

**Decomposition Review:** No decomposition required at enterprise level; domain specialisations may exist.

**Traceability:** CN-003, CN-004; ARS §3, §4, §16.

| Direction | Reference                                                                                     |
| --------- | --------------------------------------------------------------------------------------------- |
| Upward    | Constitution CN‑003, CN‑004; ARS §3, §4, §16; Enterprise Semantic Model Chapter 4.            |
| Admission | Phase 5 Enterprise Vocabulary, admitted as Aggregate Root Candidate.                          |
| Downward  | Supply Plan, Demand Plan, Production Plan, Distribution Plan, Scheduling, Execution Planning. |


### SE-C-013 – Demand

**Business Intent:** Provide the authoritative enterprise definition of a statement of need for an item.

**Enterprise Meaning:** Demand is an enterprise-recognised statement that a specific item is needed. It answers “what is required, where, and by when?”. Demand is not a forecast, plan, or commitment.

Partial satisfaction of Demand is not represented by mutating the enterprise Demand quantity or by adding partial states to Demand. Partial fulfillment is recorded by domain-level allocation or fulfillment objects. Demand transitions to Satisfied only when the whole enterprise need has been satisfied.

**Identity:** Demand Identifier is the immutable enterprise identity of the Demand.

**Applied Semantic Patterns:** Aggregate Root

**Semantic Ownership**

- **Demand owns:** identity, item, quantity, location, need window, origin, lifecycle.
- **Demand excludes:** forecasts, plans, commitments, allocation/fulfilment status, ownership of item or location.

**Authority Specification Contract**

| Section                      | Value                                                                 |
| ---------------------------- | --------------------------------------------------------------------- |
| Semantic Authority           | Core Domain                                                           |
| Steward Domain               | Core                                                                  |
| Mutation Authority           | Enterprise-Derived Planning Fact                                             |
| Authoritative Representation | Enterprise definition of a requirement for an item.                   |
| Authority Scope              | Enterprise-wide                                                       |
| Intended Consumers           | Any capability that creates, analyses, or satisfies enterprise needs. |
| Non-Intended Consumers       | None                                                                  |
| Business Responsibility      | Preserve the authoritative enterprise meaning, identity, lifecycle integrity, and published contract of this semantic object. |
| Supersedes                   | None                                                                  |
| Superseded By                | None                                                                  |

**Consumer Specification Contract**

| Section                 | Value                                                                                                            |
| ----------------------- | ---------------------------------------------------------------------------------------------------------------- |
| Business Guarantees     | Accurately records item, quantity, location, need window, origin; lifecycle reflects active/satisfied/cancelled. |
| Required Interpretation | Authoritative statement of need; need window defines acceptable fulfilment range.                                |
| Known Limitations       | Does not define priority, service level, allocation, or supply assignment.                                       |
| Declared Consumers      | Governed by Chapter 5.2 Declared Consumer Matrix.                                                                |
| Consumer Responsibility | The consumer shall reference this object solely by its immutable identifier and shall not infer semantics not published in this contract. |
| Required Attributes     | Immutable identifier; mandatory attributes; attributes required by declared structural relationships. Domain-specific required attributes must be declared in the consuming Domain Semantic Model. |
| Authoritative Source    | SE-C-013 Demand.                                                                                                 |

**Lifecycle Specification Contract**

| State     | Description                                       |
| --------- | ------------------------------------------------- |
| Active    | The need exists and has not been fully satisfied. |
| Satisfied | The need has been met.                            |
| Cancelled | The need has been withdrawn.                      |

- Permitted Transitions: Active → Satisfied; Active → Cancelled.
- Terminal States: Satisfied, Cancelled.

**Transition Trigger Standard:** Transitions are triggered only by the Business Trigger associated with this object’s Mutation Authority archetype, as defined in Section 2.7. Capability specifications shall map Aggregate Behaviors to these triggers.

**Information Model**

| Attribute         | Type                                                                   | Mandatory | Description                          |
| ----------------- | ---------------------------------------------------------------------- | --------- | ------------------------------------ |
| Demand Identifier | ID (immutable)                                                         | Yes       | Unique enterprise identity.          |
| Item              | Reference (SE-C-001)                                                   | Yes       | The item needed.                     |
| Quantity          | Quantity (SE-C-023)                                                    | Yes       | Required quantity; must be positive. |
| Location          | Reference (SE-C-002)                                                   | Yes       | Where the item is needed.            |
| Need Window       | Need Window (SE-C-029)                                                 | Yes       | Acceptable fulfilment interval.      |
| Demand Origin     | Enum (CustomerOrder, Forecast, ProductionRequirement, Transfer, Other) | Yes       | Enterprise context of the demand.    |
| Customer          | Reference (SE-C-003)                                                   | No        | Customer, if applicable.             |
| Parent Demand     | Reference (SE-C-013)                                                   | No        | Derived demand parent.               |
| Lifecycle State   | Enum (Active, Satisfied, Cancelled)                                    | Yes       | Current state.                       |

**Relationships**

| Relationship    | Target Object          | Cardinality            | Description           |
| --------------- | ---------------------- | ---------------------- | --------------------- |
| is need for     | Item (SE-C-001)        | Many-to-One            | The item.             |
| is needed at    | Location (SE-C-002)    | Many-to-One            | The location.         |
| is needed for   | Customer (SE-C-003)    | Many-to-One (optional) | Customer association. |
| is derived from | Demand (SE-C-013)      | Many-to-One (optional) | Parent demand.        |
| has need window | Need Window (SE-C-029) | Many-to-One            | Fulfilment interval.  |

**Invariants:** Identifier immutable; quantity positive; Demand Quantity is immutable after creation; Need Window valid; Demand Origin recognised; single need per object.

**Dependencies:** SE-C-001 Item, SE-C-002 Location, SE-C-003 Customer, SE-C-023 Quantity, SE-C-022 Timestamp (via Need Window), SE-C-029 Need Window.

**Decomposition Review:** No decomposition required.

**Traceability:** CN-003, CN-004; ARS §3, §4, §16.


### SE-C-014 – Supply

**Business Intent:** Provide the authoritative enterprise definition of an available quantity of an item that may satisfy enterprise demand.

**Enterprise Meaning:** Supply is an enterprise‑recognised availability of an item. It answers “what is available to meet needs, where, and for what time period?” Supply is distinct from inventory (physical stock), from plans (intentions), and from commitments (obligations). It represents the existence of a quantity that can be applied to demands, independent of how that availability was sourced.

Partial consumption of Supply is not represented by mutating the enterprise Supply quantity or by adding partial states to Supply. Partial consumption is recorded by domain-level allocation or consumption objects. Supply transitions to Consumed only when the whole enterprise availability has been consumed.

**Identity:** Supply Identifier is the immutable enterprise identity of the Supply.

**Applied Semantic Patterns:** Aggregate Root

**Semantic Ownership**

- **Supply owns:** identity, item, quantity, location, temporal window, provenance classification, lifecycle.
- **Supply excludes:** plans, commitments, allocation, ownership of item/location/source.

**Authority Specification Contract**

| Section                      | Value                                                                   |
| ---------------------------- | ----------------------------------------------------------------------- |
| Semantic Authority           | Core Domain                                                             |
| Steward Domain               | Core                                                                    |
| Mutation Authority           | Enterprise-Derived Planning Fact                                               |
| Authoritative Representation | The enterprise definition of an available item quantity.                |
| Authority Scope              | Enterprise‑wide                                                         |
| Intended Consumers           | Any capability that needs to know what is available to satisfy demands. |
| Non‑Intended Consumers       | None                                                                    |
| Business Responsibility      | Preserve the authoritative enterprise meaning, identity, lifecycle integrity, and published contract of this semantic object. |
| Supersedes                   | None                                                                    |
| Superseded By                | None                                                                    |

**Consumer Specification Contract**

| Section                 | Value                                                                                                                                                                                                                          |
| ----------------------- | ------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------ |
| Business Guarantees     | Every Supply accurately records the item, quantity, location, and the temporal window during which it is available. The lifecycle state indicates whether the supply is currently available, consumed, or no longer available. |
| Required Interpretation | Consumers shall interpret Supply as an authoritative record of available quantity within the stated window. It does not prescribe allocation or prioritisation.                                                                |
| Known Limitations       | Does not define the rules for how supply is matched to demand. The availability is a statement of fact, not a guarantee of delivery.                                                                                           |
| Declared Consumers      | Governed by Chapter 5.2 Declared Consumer Matrix.                                                                                                                                                                              |
| Consumer Responsibility | The consumer shall reference this object solely by its immutable identifier and shall not infer semantics not published in this contract.                                                                                   |
| Required Attributes     | Immutable identifier; mandatory attributes; attributes required by declared structural relationships. Domain-specific required attributes must be declared in the consuming Domain Semantic Model.                           |
| Authoritative Source    | SE-C-014 Supply.                                                                                                                                                                                                               |

**Lifecycle Specification Contract**

| State     | Description                                                                                                        |
| --------- | ------------------------------------------------------------------------------------------------------------------ |
| Available | The supply exists and is available for use within its temporal window.                                             |
| Consumed  | The supply has been fully allocated or used.                                                                       |
| Withdrawn | The supply is no longer available before its temporal window expired (e.g., order cancelled, production scrapped). |
| Expired   | The temporal window has passed and the supply was not used.                                                        |

- Permitted Transitions: Available → Consumed; Available → Withdrawn; Available → Expired.
- Terminal States: Consumed, Withdrawn, Expired.
- History Preservation: State changes are preserved as enterprise history.

**Transition Trigger Standard:** Transitions are triggered only by the Business Trigger associated with this object’s Mutation Authority archetype, as defined in Section 2.7. Capability specifications shall map Aggregate Behaviors to these triggers.

**Information Model**

| Attribute                        | Type                                           | Mandatory | Description                                                                                                                                                           |
| -------------------------------- | ---------------------------------------------- | --------- | --------------------------------------------------------------------------------------------------------------------------------------------------------------------- |
| Supply Identifier                | ID (immutable)                                 | Yes       | Unique identity.                                                                                                                                                      |
| Item                             | Reference (SE-C-001)                           | Yes       | The item available.                                                                                                                                                   |
| Quantity                         | Quantity (SE-C-023)                            | Yes       | Available quantity; must be positive.                                                                                                                                 |
| Location                         | Reference (SE-C-002)                           | Yes       | Where the supply is available.                                                                                                                                        |
| Availability Window              | Temporal Window (SE-C-028)                     | Yes       | Time interval of availability.                                                                                                                                        |
| Supply Provenance Classification | Governed Identifier Reference (SE-C-037)       | Yes       | The category of how the supply came into existence (e.g., “OnHand”, “ScheduledReceipt”, “PlannedProduction”). The allowed values are governed by the relevant domain. |
| Lifecycle State                  | Enum (Available, Consumed, Withdrawn, Expired) | Yes       | Current state.                                                                                                                                                        |

**Relationships**

| Relationship            | Target Object              | Cardinality | Description   |
| ----------------------- | -------------------------- | ----------- | ------------- |
| is supply of            | Item (SE-C-001)            | Many-to-One | The item.     |
| is available at         | Location (SE-C-002)        | Many-to-One | The location. |
| has availability window | Temporal Window (SE-C-028) | Many-to-One | Time window.  |

**Invariants:** Identifier immutable; quantity positive; Supply Quantity is immutable after creation; Availability Window valid; provenance classification recognised; single availability per object.

**Dependencies:** SE-C-001 Item, SE-C-002 Location, SE-C-023 Quantity, SE-C-022 Timestamp (via Temporal Window), SE-C-028 Temporal Window., SE-C-037 Enterprise Governed Vocabulary

**Decomposition Review:** No decomposition required.

**Traceability:** CN-003, CN-004; ARS §3, §4, §16.

| Direction | Reference                                                                                 |
| --------- | ----------------------------------------------------------------------------------------- |
| Upward    | Constitution CN‑003, CN‑004; ARS §3, §4, §16; Enterprise Semantic Model Chapter 4.        |
| Admission | Phase 5 Enterprise Vocabulary, admitted as Realization Hypothesis.                        |
| Downward  | Downstream traceability will be established when consuming Semantic Objects are authored. |


### SE-C-015 – Inventory

**Business Intent:** Provide the authoritative enterprise definition of the physical stock of an item held at a location.

**Enterprise Meaning:** Inventory is the physical stock of a specific item at a specific location. It represents the quantity of that item that physically exists at that location. Inventory is independent of how the enterprise observes, records, or plans for that stock. It reflects the most recent valid physical observation without owning the observation itself.

Inventory is a Current Physical Stock Fact. It records the most recent valid physical observation of stock for a specific Item at a specific Location. Inventory is not available stock, allocatable stock, projected stock, safety stock, inventory position, or inventory health.

**Identity:** Inventory is uniquely identified by the combination of Item and Location and Batch. There is exactly one Inventory aggregate for a given Item at a given Location.

**Applied Semantic Patterns:** Aggregate Root

**Semantic Ownership**

- **Inventory owns:** identity (item + location), on-hand quantity, observation timestamp.
- **Inventory excludes:** allocated/reserved quantities, available quantity, future projections, safety stock, inventory position, inventory health, financial valuation, ownership of item/location.

**Authority Specification Contract**

| Section                      | Value                                                   |
| ---------------------------- | ------------------------------------------------------- |
| Semantic Authority           | Core Domain                                             |
| Steward Domain               | Core                                                    |
| Mutation Authority           | External System of Record                               |
| Authoritative Representation | The enterprise definition of physical stock.            |
| Authority Scope              | Enterprise‑wide                                         |
| Intended Consumers           | Any capability requiring current physical stock levels. |
| Non‑Intended Consumers       | None                                                    |
| Business Responsibility      | Preserve the authoritative enterprise meaning, identity, lifecycle integrity, and published contract of this semantic object. |
| Supersedes                   | None                                                    |
| Superseded By                | None                                                    |

**Consumer Specification Contract**

| Section                 | Value                                                                                                                                                                                                                   |
| ----------------------- | ----------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------- |
| Business Guarantees     | Every Inventory record reflects the physical stock for a given item and location according to the most recent valid observation. The on‑hand quantity is non‑negative.                                                  |
| Required Interpretation | Consumers shall interpret the on‑hand quantity as the physical stock that exists. Allocated, reserved, and available quantities are separate enterprise facts derived from commitments and are not part of this record. |
| Known Limitations       | Does not include future receipts, planned orders, or allocations. Physical stock may differ from system‑recorded quantities due to unobserved losses, gains, or counting errors.                                        |
| Declared Consumers      | Governed by Chapter 5.2 Declared Consumer Matrix.                                                                                                                                                                       |
| Consumer Responsibility | The consumer shall reference this object solely by its immutable identifier and shall not infer semantics not published in this contract.                                                                           |
| Required Attributes     | Immutable identifier; mandatory attributes; attributes required by declared structural relationships. Domain-specific required attributes must be declared in the consuming Domain Semantic Model.                  |
| Authoritative Source    | SE-C-015 Inventory.                                                                                                                                                                                                     |

**Lifecycle Specification Contract:** No governed lifecycle. The record exists while item and location are recognised. Historical values are retained.

**Information Model**

| Attribute             | Type                 | Mandatory | Description                                  |
| --------------------- | -------------------- | --------- | -------------------------------------------- |
| Item                  | Reference (SE-C-001) | Yes       | The item whose stock is recorded.            |
| Batch Identifier | String | Yes | The enterprise-recognised batch identifier for the item. |
| Location              | Reference (SE-C-002) | Yes       | The location.                                |
| On-Hand Quantity      | Quantity (SE-C-023)  | Yes       | Physical stock present; non-negative.        |
| Observation Timestamp | Timestamp (SE-C-022) | Yes       | When the last physical observation occurred. |

**Relationships**

| Relationship | Target Object       | Cardinality | Description   |
| ------------ | ------------------- | ----------- | ------------- |
| is stock of  | Item (SE-C-001)     | Many-to-One | The item.     |
| is held at   | Location (SE-C-002) | Many-to-One | The location. |

**Invariants:** Item + Location + Batch Identifier is unique; On-Hand Quantity ≥ 0; Observation Timestamp valid.

**Dependencies:** SE-C-001 Item, SE-C-002 Location, SE-C-023 Quantity, SE-C-022 Timestamp, SE-C-032 Unit of Measure (via Quantity).

**Decomposition Review:** No decomposition required.

**Traceability:** CN-003, CN-004; ARS §3, §4, §16.


### SE-C-017 – Commitment

**Business Intent:** Provide the authoritative enterprise definition of a binding obligation to deliver or receive a specified quantity of an item at a specified location by a specified time.

**Enterprise Meaning:** A Commitment is an enterprise‑recognised binding obligation. It answers “what has been promised, between whom, where, and by when?”. The obligation may involve an external customer, an external supplier, or another enterprise entity. The commitment state reflects whether the obligation is confirmed, fulfilled, or cancelled. A Commitment does not represent a plan, a forecast, or an unconfirmed request.

Partial fulfillment of a Commitment is not represented by mutating the enterprise Commitment quantity or by adding partial states to Commitment. Partial fulfillment is recorded by domain-level fulfillment objects. Commitment transitions to Fulfilled only when the whole obligation has been satisfied.

**Identity:** Commitment Identifier is the immutable enterprise identity of the Commitment.

**Applied Semantic Patterns:** Aggregate Root

**Semantic Ownership**

- **Commitment owns:** identity, item, quantity, location, dates, parties, direction, lifecycle.
- **Commitment excludes:** drafts, plans, forecasts, delivery execution, ownership of item/location/parties.

**Authority Specification Contract**

| Section                      | Value                                                           |
| ---------------------------- | --------------------------------------------------------------- |
| Semantic Authority           | Core Domain                                                     |
| Steward Domain               | Core                                                            |
| Mutation Authority           | Enterprise-Governed Transactional State                         |
| Authoritative Representation | The enterprise definition of binding obligations.               |
| Authority Scope              | Enterprise‑wide                                                 |
| Intended Consumers           | Any capability that creates, monitors, or fulfills obligations. |
| Non‑Intended Consumers       | None                                                            |
| Business Responsibility      | Preserve the authoritative enterprise meaning, identity, lifecycle integrity, and published contract of this semantic object. |
| Supersedes                   | None                                                            |
| Superseded By                | None                                                            |

**Consumer Specification Contract**

| Section                 | Value                                                                                                                                                                                                                                                                                                                                        |
| ----------------------- | -------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------- |
| Business Guarantees     | Every Commitment accurately records the item, quantity, location, parties, and dates. The lifecycle state reflects the current status of the obligation. A Committed commitment is binding.                                                                                                                                                  |
| Required Interpretation | Consumers shall interpret the Commitment as a binding enterprise obligation. The committed date (if present) is the agreed‑upon due date; the requested date is the date the receiving party needs the item. Fulfilment or cancellation are separate enterprise facts that transition the state, not modifications of the commitment itself. |
| Known Limitations       | Does not define the demand, supply, or plan that generated the commitment. Does not include delivery instructions, quality requirements, or commercial terms.                                                                                                                                                                                |
| Declared Consumers      | Governed by Chapter 5.2 Declared Consumer Matrix.                                                                                                                                                                                                                                                                                            |
| Consumer Responsibility | The consumer shall reference this object solely by its immutable identifier and shall not infer semantics not published in this contract.                                                                                                                                                                                                 |
| Required Attributes     | Immutable identifier; mandatory attributes; attributes required by declared structural relationships. Domain-specific required attributes must be declared in the consuming Domain Semantic Model.                                                                                                                                        |
| Authoritative Source    | SE-C-017 Commitment.                                                                                                                                                                                                                                                                                                                         |

**Lifecycle Specification Contract**

| State     | Description                   |
| --------- | ----------------------------- |
| Committed | Confirmed and binding.        |
| Fulfilled | Obligation satisfied.         |
| Cancelled | Terminated before fulfilment. |

- Permitted Transitions: Committed → Fulfilled; Committed → Cancelled.
- Terminal States: Fulfilled, Cancelled.
- History Preservation: State changes are preserved as enterprise history.

**Transition Trigger Standard:** Transitions are triggered only by the Business Trigger associated with this object’s Mutation Authority archetype, as defined in Section 2.7. Capability specifications shall map Aggregate Behaviors to these triggers.

**Information Model**

| Attribute             | Type                                   | Mandatory | Description                                                    |
| --------------------- | -------------------------------------- | --------- | -------------------------------------------------------------- |
| Commitment Identifier | ID (immutable)                         | Yes       | Unique identity.                                               |
| Obligation Direction  | Enum (Inbound, Outbound)               | Yes       | Inbound = enterprise receives; Outbound = enterprise delivers. |
| Item                  | Reference (SE-C-001)                   | Yes       | The item.                                                      |
| Quantity              | Quantity (SE-C-023)                    | Yes       | Committed quantity; positive.                                  |
| Location              | Reference (SE-C-002)                   | Yes       | Delivery location.                                             |
| Requested Date        | Timestamp (SE-C-022)                   | Yes       | Date needed by receiving party.                                |
| Committed Date        | Timestamp (SE-C-022)                   | No        | Agreed due date.                                               |
| Customer              | Reference (SE-C-003)                   | No        | For Outbound.                                                  |
| Supplier              | Reference (SE-C-004)                   | No        | For Inbound.                                                   |
| Lifecycle State       | Enum (Committed, Fulfilled, Cancelled) | Yes       | Current state.                                                 |

**Relationships**

| Relationship      | Target Object       | Cardinality            | Description        |
| ----------------- | ------------------- | ---------------------- | ------------------ |
| involves item     | Item (SE-C-001)     | Many-to-One            | The item.          |
| involves location | Location (SE-C-002) | Many-to-One            | Delivery location. |
| involves customer | Customer (SE-C-003) | Many-to-One (optional) | Customer.          |
| involves supplier | Supplier (SE-C-004) | Many-to-One (optional) | Supplier.          |

**Invariants:**
- Commitment Identifier is immutable.
- Obligation Direction must be Inbound or Outbound.
- For an Inbound commitment, Supplier must be present and Customer must be absent.
- For an Outbound commitment, Customer must be present and Supplier must be absent.
- Quantity must be positive.
- Commitment Quantity is immutable after creation.
- Requested Date must be a valid UTC timestamp.
- Committed Date, if present, must be a valid UTC timestamp and must not be earlier than the Requested Date unless agreed otherwise.
- A single Commitment represents a single obligation for one item, one location, and one set of parties. It shall not represent multiple distinct obligations.

**Dependencies:** SE-C-001 Item, SE-C-002 Location, SE-C-003 Customer, SE-C-004 Supplier, SE-C-023 Quantity, SE-C-022 Timestamp.

**Traceability:** CN-003, CN-004; ARS §3, §4, §16.


### SE-C-018 – Bill of Materials

**Business Intent:** Provide the authoritative enterprise definition of the governed composition of one enterprise item in terms of other enterprise items.

**Enterprise Meaning:** A Bill of Materials (BOM) describes “what items, and in what quantities, constitute this parent item?” Multiple governed compositions may exist over time for the same parent item, each version being an immutable record valid for a specific period. The parent item is a Product or Material; the components are also Products or Materials. The BOM is the enterprise reference for component structure, not for inventory, routing, procurement, or manufacturing processes.

**Identity:** BOM Version Identifier is the immutable enterprise identity of this Bill of Materials version.

**Applied Semantic Patterns:** Aggregate Root (the version is the root)

**Semantic Ownership**

- **BOM Version owns:** identity, parent item, version number, effective dates, component lines, lifecycle.
- **BOM Version excludes:** parent item’s identity ownership, inventory, routing, procurement.

**Authority Specification Contract**

| Section                      | Value                                                    |
| ---------------------------- | -------------------------------------------------------- |
| Semantic Authority           | Core Domain                                              |
| Steward Domain               | Core                                                     |
| Mutation Authority           | External System of Record                                |
| Authoritative Representation | A governed, versioned composition of an enterprise item. |
| Authority Scope              | Enterprise‑wide                                          |
| Intended Consumers           | Production domain, Supply Planning, Costing              |
| Non‑Intended Consumers       | None                                                     |
| Business Responsibility      | Preserve the authoritative enterprise meaning, identity, lifecycle integrity, and published contract of this semantic object. |
| Supersedes                   | None                                                     |
| Superseded By                | None                                                     |

**Consumer Specification Contract**

| Section                 | Value                                                                                                                                                                                      |
| ----------------------- | ------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------ |
| Business Guarantees     | Each active BOM version is immutable and accurately represents the component structure of its parent item for the defined validity period. Component identities and quantities are stable. |
| Required Interpretation | The version defines the authoritative component composition applicable during its validity period. The BOM does not prescribe sourcing, routing, or inventory actions.                     |
| Known Limitations       | Does not include yield factors, scrap rates, or alternative components.                                                                                                                    |
| Declared Consumers      | Governed by Chapter 5.2 Declared Consumer Matrix.                                                                                                                                          |
| Consumer Responsibility | The consumer shall reference this object solely by its immutable identifier and shall not infer semantics not published in this contract.                                               |
| Required Attributes     | Immutable identifier; mandatory attributes; attributes required by declared structural relationships. Domain-specific required attributes must be declared in the consuming Domain Semantic Model. |
| Authoritative Source    | SE-C-018 Bill of Materials.                                                                                                                                                                |

**Lifecycle Specification Contract**

| State      | Description                                          |
| ---------- | ---------------------------------------------------- |
| Draft      | Under development.                                   |
| Active     | Approved and immutable; effective when date reached. |
| Superseded | Replaced by a newer Active version.                  |
| Archived   | Retained for audit.                                  |

- Permitted Transitions: Draft → Active; Active → Superseded; Active → Archived; Superseded → Archived.
- Terminal State: Archived.
- History Preservation: Each version is an immutable record once active; all versions are preserved.

**Transition Trigger Standard:** Transitions are triggered only by the Business Trigger associated with this object’s Mutation Authority archetype, as defined in Section 2.7. Capability specifications shall map Aggregate Behaviors to these triggers.

**Information Model (BOM Version – Aggregate Root)**

| Attribute              | Type                                       | Mandatory | Description                                  |
| ---------------------- | ------------------------------------------ | --------- | -------------------------------------------- |
| BOM Version Identifier | ID (immutable)                             | Yes       | Unique enterprise identity for this version. |
| Parent Item            | Reference (SE-C-001)                       | Yes       | The item whose composition is defined.       |
| Version Number         | Integer                                    | Yes       | Monotonically increasing within the series.  |
| Effective Date         | Timestamp (SE-C-022)                       | No        | When version becomes applicable.             |
| End Date               | Timestamp (SE-C-022)                       | No        | When version ceases to be applicable.        |
| BOM Lines              | List of BOMLine                            | Yes       | At least one component line.                 |
| Lifecycle State        | Enum (Draft, Active, Superseded, Archived) | Yes       | Current state.                               |

**BOMLine (Entity)**

| Attribute       | Type                 | Mandatory | Description                              |
| --------------- | -------------------- | --------- | ---------------------------------------- |
| Line Identifier | ID (immutable)       | Yes       | Stable identity independent of ordering. |
| Sequence Number | Integer              | No        | Display ordering.                        |
| Component Item  | Reference (SE-C-001) | Yes       | The component.                           |
| Quantity        | Quantity (SE-C-023)  | Yes       | Amount per parent unit.                  |

**Relationships**

| Relationship            | Target Object   | Cardinality | Description            |
| ----------------------- | --------------- | ----------- | ---------------------- |
| defines composition for | Item (SE-C-001) | Many-to-One | Parent item.           |
| references component    | Item (SE-C-001) | One-to-Many | Each line’s component. |

**Invariants:** Identifier immutable; Parent Item must exist and be active; lines non-empty for Active version; each line quantity positive; Active version immutable; validity periods of Active versions must not overlap for same parent.

**Dependencies:** SE-C-001 Item, SE-C-023 Quantity, SE-C-022 Timestamp, SE-C-032 Unit of Measure (via Quantity).

**Traceability:** CN-003, CN-004; ARS §3, §4, §16.


### SE-C-019 – Exception

**Business Intent:** Provide the authoritative enterprise definition of a condition in which an enterprise constraint is not satisfied.

**Enterprise Meaning:** An Exception is an enterprise‑recognised condition indicating that a constraint applicable to the enterprise is breached. Enterprise constraints may originate from governance policies, business rules, semantic invariants, or enterprise consistency requirements. The Exception answers “which constraint is currently unsatisfied for which scope?”. It is a persistent record of the condition; it does not prescribe actions, nor does it require that attention has been paid. It is an enterprise fact that a governed expectation is violated for a specific scope.

**Identity:** Exception Identifier is the immutable enterprise identity of the Exception.

**Applied Semantic Patterns:** Aggregate Root

**Semantic Ownership**

- **Exception owns:** identity, constraint reference, classification, scope, evidence reference, lifecycle.
- **Exception excludes:** policies/rules themselves, response actions, investigation workflows, ownership of evidence.

**Authority Specification Contract**

| Section                      | Value                                                                                       |
| ---------------------------- | ------------------------------------------------------------------------------------------- |
| Semantic Authority           | Core Domain                                                                                 |
| Steward Domain               | Core                                                                                        |
| Mutation Authority           | Enterprise-Derived Planning Fact                                                                   |
| Authoritative Representation | The enterprise definition of unsatisfied enterprise constraints.                            |
| Authority Scope              | Enterprise‑wide                                                                             |
| Intended Consumers           | Any capability that monitors, reports, or resolves deviations from enterprise expectations. |
| Non‑Intended Consumers       | None                                                                                        |
| Business Responsibility      | Preserve the authoritative enterprise meaning, identity, lifecycle integrity, and published contract of this semantic object. |
| Supersedes                   | None                                                                                        |
| Superseded By                | None                                                                                        |

**Consumer Specification Contract**

| Section                 | Value                                                                                                                                                                                                                                                                         |
| ----------------------- | ----------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------- |
| Business Guarantees     | Every Exception uniquely identifies the constraint that is breached, the affected scope, and the exception classification. Its lifecycle state indicates whether the condition is active or resolved. A resolved Exception is immutable and retained as an enterprise record. |
| Required Interpretation | Consumers shall interpret the Exception as the authoritative record that an enterprise constraint is currently unsatisfied. The Exception does not prescribe actions; it identifies that a governed boundary is breached.                                                     |
| Known Limitations       | Does not include the full operational context; only references to the triggering evidence and the affected scope are provided. Does not define response workflows or prioritisation.                                                                                          |
| Declared Consumers      | Governed by Chapter 5.2 Declared Consumer Matrix.                                                                                                                                                                                                                             |
| Consumer Responsibility | The consumer shall reference this object solely by its immutable identifier and shall not infer semantics not published in this contract.                                                                                                                                     |
| Required Attributes     | Immutable identifier; mandatory attributes; attributes required by declared structural relationships. Domain-specific required attributes must be declared in the consuming Domain Semantic Model.                                                                         |
| Authoritative Source    | SE-C-019 Exception.                                                                                                                                                                                                                                                           |

**Lifecycle Specification Contract**

| State    | Description                                       |
| -------- | ------------------------------------------------- |
| Active   | Constraint is currently not satisfied.            |
| Resolved | No longer active; retained for historical record. |

- Permitted Transitions: Active → Resolved.
- Terminal State: Resolved.
- History Preservation: State transitions and evidence updates are preserved as enterprise history.

**Transition Trigger Standard:** Transitions are triggered only by the Business Trigger associated with this object’s Mutation Authority archetype, as defined in Section 2.7. Capability specifications shall map Aggregate Behaviors to these triggers.

**Information Model**

| Attribute                 | Type                    | Mandatory | Description                                                                                                                                                                                              |
| ------------------------- | ----------------------- | --------- | -------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------- |
| Exception Identifier      | ID (immutable)          | Yes       | Unique enterprise identity.                                                                                                                                                                              |
| Constraint Reference      | External Identifier     | Yes       | The identifier of the governance policy, business rule, semantic invariant, or enterprise consistency requirement that is breached.                                                                      |
| Exception Classification  | Governed Identifier Reference (SE-C-037) | Yes       | The category of the unsatisfied condition (e.g., “InventoryConstraintViolation”, “SupplyContinuityBreach”). The taxonomy shall be consistent and governed by the relevant domain.                        |
| Affected Scope Type       | Governed Identifier Reference (SE-C-037) | Yes       | The type of enterprise entity affected (e.g., “Item”, “Location”, “Supplier”, “PlanningScope”). This refers to an enterprise concept that exists in the Semantic Model or as a governed external entity. |
| Affected Scope Identifier | External Identifier     | Yes       | The unique identifier of the specific enterprise entity that is affected, consistent with the scope type.                                                                                                |
| Evidence Reference        | External Identifier     | No        | A reference to the enterprise record (e.g., an Observation, Assessment, or Snapshot) that provided the basis for detecting the condition.                                                                |
| Lifecycle State           | Enum (Active, Resolved) | Yes       | Current state.                                                                                                                                                                                           |

**Relationships:** None. Exception references external governance artifacts, affected subjects, and evidence through typed external identifiers; those references are not ratified Enterprise Semantic Objects in this model.

**Invariants:** Identifier immutable; Constraint Reference non-empty; Classification recognised; Scope Type and Identifier consistent; Resolved Exception is immutable.

**Dependencies:** None directly; references external governance and enterprise entities. SE-C-037  Enterprise Governed Vocabulary

**Traceability:** CN-003, CN-004, CN-009; ARS §3, §4, §16.


### SE-C-020 – Risk

**Business Intent:** Provide the authoritative enterprise identity for a recognised potential adverse condition or event that may affect enterprise objectives.

**Enterprise Meaning:** A Risk is an enterprise‑recognised potential adverse condition. It answers “what could negatively affect the enterprise?” independently of any specific evaluation of likelihood or impact at a point in time. The risk itself is distinct from its assessment; the same risk can be evaluated multiple times as new information becomes available. The risk identity is stable; its assessments evolve over time.

**Identity:** Risk Identifier is the immutable enterprise identity of the Risk.

**Applied Semantic Patterns:** Aggregate Root

**Semantic Ownership**

- **Risk owns:** identity, risk type, subject reference, lifecycle, and the collection of Risk Assessments attached to the Risk.
- **Risk excludes:** the evidence used to produce assessments, mitigation actions, ownership of the risk subject.

**Authority Specification Contract**

| Section                      | Value                                                                |
| ---------------------------- | -------------------------------------------------------------------- |
| Semantic Authority           | Core Domain                                                          |
| Steward Domain               | Core                                                                 |
| Mutation Authority           | Enterprise-Derived Planning Fact                                            |
| Authoritative Representation | The enterprise definition of a potential adverse condition.          |
| Authority Scope              | Enterprise‑wide                                                      |
| Intended Consumers           | Any capability that identifies, tracks, or reports enterprise risks. |
| Non‑Intended Consumers       | None                                                                 |
| Business Responsibility      | Preserve the authoritative enterprise meaning, identity, lifecycle integrity, and published contract of this semantic object. |
| Supersedes                   | None                                                                 |
| Superseded By                | None                                                                 |

**Consumer Specification Contract**

| Section                 | Value                                                                                                                                                                                                  |
| ----------------------- | ------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------ |
| Business Guarantees     | Every Risk has a unique, immutable identifier and a stable classification and subject. The most recent Risk Assessment (when available) provides the current enterprise view of likelihood and impact. |
| Required Interpretation | Consumers shall interpret the Risk as the enduring identity of a potential adverse condition. The assessment history records how the enterprise’s evaluation has changed over time.                    |
| Known Limitations       | Does not prescribe mitigation or response. The subject of the risk is referenced generically; the owning entity is external to Risk.                                                                   |
| Declared Consumers      | Governed by Chapter 5.2 Declared Consumer Matrix.                                                                                                                                                      |
| Consumer Responsibility | The consumer shall reference this object solely by its immutable identifier and shall not infer semantics not published in this contract.                                                           |
| Required Attributes     | Immutable identifier; mandatory attributes; attributes required by declared structural relationships. Domain-specific required attributes must be declared in the consuming Domain Semantic Model.                  |
| Authoritative Source    | SE-C-020 Risk.                                                                                                                                                                                         |

**Lifecycle Specification Contract**

| State   | Description                                                          |
| ------- | -------------------------------------------------------------------- |
| Active  | Recognised and may be assessed.                                      |
| Retired | No longer considered relevant; retained for historical traceability. |

- Permitted Transitions: Active → Retired.
- Terminal State: Retired.
- History Preservation: State changes are preserved; Risk Assessments are retained historically.

**Transition Trigger Standard:** Transitions are triggered only by the Business Trigger associated with this object’s Mutation Authority archetype, as defined in Section 2.7. Capability specifications shall map Aggregate Behaviors to these triggers.

**Information Model**

| Attribute       | Type                   | Mandatory | Description                                                    |
| --------------- | ---------------------- | --------- | -------------------------------------------------------------- |
| Risk Identifier | ID (immutable)         | Yes       | Unique identity.                                               |
| Risk Type       | Governed Identifier Reference (SE-C-037) | Yes       | Classification (e.g., “SupplyDisruption”, “DemandVolatility”). |
| Risk Subject    | Enterprise Reference   | Yes       | The enterprise entity at risk (semantic type + identifier).    |
| Lifecycle State | Enum (Active, Retired) | Yes       | Current state.                                                 |

*Note: The Enterprise Reference for Risk Subject is represented by a semantic type (e.g., "Item", "Location", "Supplier") and the corresponding unique identifier of that entity. This is a standard pattern for cross‑aggregate references within the Semantic Model and does not imply implementation ownership.*

**Relationships**

| Relationship | Target Object              | Cardinality | Description             |
| ------------ | -------------------------- | ----------- | ----------------------- |
| contains | Risk Assessment (SE-C-030) | One-to-Many | Immutable risk assessments owned by this Risk. |

**Invariants:** Identifier immutable; Risk Type recognised; Subject valid; Retired Risk shall not receive new assessments.

**Dependencies:** SE-C-037 Enterprise Governed Vocabulary

**Traceability:** CN-003, CN-004; ARS §3, §4, §16.


### SE-C-021 – Enterprise Picture

**Business Intent:** Provide the authoritative enterprise definition of the combined state of demand, supply, and inventory within a specific Planning Scope at a specific point in time.

**Enterprise Meaning:** An Enterprise Picture is an enterprise‑recognised, point‑in‑time snapshot of the planning‑relevant state of the enterprise for a defined Planning Scope. It answers “what is the current enterprise reality for this scope?” by referencing the current demand, supply, and inventory that fall within the scope’s boundary. The Picture itself is the enduring enterprise object for a given Planning Scope; it owns a series of immutable Published versions. Each Published version is the authoritative reference for downstream planning capabilities at the time of its publication. The Picture does not own the underlying demand, supply, or inventory records—only the membership of those references within the snapshot.

**Identity:** Planning Scope Identifier is the aggregate identity of the Enterprise Picture. Exactly one Enterprise Picture exists for each Planning Scope.

**Applied Semantic Patterns:** Aggregate Root / Snapshot

**Semantic Ownership**

- **Enterprise Picture owns:** identity (Planning Scope), snapshot versions, publication lifecycle.
- **Enterprise Picture excludes:** underlying demand, supply, inventory records; plans, forecasts, decisions; ownership of Planning Scope.

**Authority Specification Contract**

| Section                      | Value                                                                                                |
| ---------------------------- | ---------------------------------------------------------------------------------------------------- |
| Semantic Authority           | Core Domain                                                                                          |
| Steward Domain               | Core                                                                                                 |
| Mutation Authority           | Enterprise-Derived Planning Fact                                                                            |
| Authoritative Representation | The enterprise definition of a planning‑scope snapshot series.                                       |
| Authority Scope              | Per Planning Scope. Exactly one Published version per Planning Scope is authoritative at any moment. |
| Intended Consumers           | Any planning capability that requires a point‑in‑time view of enterprise reality.                    |
| Non‑Intended Consumers       | None                                                                                                 |
| Business Responsibility      | Preserve the authoritative enterprise meaning, identity, lifecycle integrity, and published contract of this semantic object. |
| Supersedes                   | None                                                                                                 |
| Superseded By                | None                                                                                                 |

**Consumer Specification Contract**

| Section                 | Value                                                                                                                                                                                                                                  |
| ----------------------- | -------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------- |
| Business Guarantees     | Each Published version is an immutable, complete snapshot of the demand, supply, and inventory for the Planning Scope at the time of publication. Exactly one Published version exists per Planning Scope at any moment.               |
| Required Interpretation | Consumers shall treat the Published version as the single authoritative picture of enterprise reality for that scope. Previous versions are historical records and must not be used for active planning.                               |
| Known Limitations       | The picture is a snapshot, not a live view. Changes to underlying records after publication are not reflected until a new version is published. The picture does not define what actions should be taken; it only records what exists. |
| Declared Consumers      | Governed by Chapter 5.2 Declared Consumer Matrix.                                                                                                                                                                                      |
| Consumer Responsibility | The consumer shall reference this object solely by its immutable identifier and shall not infer semantics not published in this contract.                                                                                           |
| Required Attributes     | Immutable identifier; mandatory attributes; attributes required by declared structural relationships. Domain-specific required attributes must be declared in the consuming Domain Semantic Model.                           |
| Authoritative Source    | SE-C-021 Enterprise Picture.                                                                                                                                                                                                           |

**Lifecycle Specification Contract (Version Lifecycle)**

| State      | Description                                           | Transition Trigger       |
| ---------- | ----------------------------------------------------- | ------------------------ |
| Draft      | Being prepared; not authoritative.                    | Creation / revision.     |
| Published  | Authoritative; previous Published becomes Superseded. | Publication.             |
| Superseded | Replaced by a newer Published version.                | Newer version Published. |

- Terminal State: Superseded (retained permanently).
- Version Number: Monotonic, scoped to Planning Scope, never reset.
- Versioning Rules: Each version within the aggregate receives a monotonically increasing Version Number, scoped to the Planning Scope and never reset.
- History Preservation: All versions are retained permanently. Superseded versions are immutable.

**Transition Trigger Standard:** Transitions are triggered only by the Business Trigger associated with this object’s Mutation Authority archetype, as defined in Section 2.7. Capability specifications shall map Aggregate Behaviors to these triggers.

**Information Model**

| Attribute                 | Type                   | Mandatory | Description           |
| ------------------------- | ---------------------- | --------- | --------------------- |
| Planning Scope Identifier | Reference (SE-C-010)   | Yes       | Aggregate identity.   |
| Versions                  | List of PictureVersion | Yes       | At least one version. |

**PictureVersion (Entity, identity = Version Number)**

| Attribute            | Type                                | Mandatory | Description                                                                                            |
| -------------------- | ----------------------------------- | --------- | ------------------------------------------------------------------------------------------------------ |
| Version Number       | Integer                             | Yes       | Monotonically increasing within the Planning Scope. This is the identity of the PictureVersion Entity. |
| Demand References    | List of Demand references           | Yes       | The Demand records that fall within the Planning Scope at the time of publication. May be empty.       |
| Supply References    | List of Supply references           | Yes       | The Supply records that fall within the Planning Scope at the time of publication. May be empty.       |
| Inventory References | List of Inventory references        | Yes       | The Inventory records that fall within the Planning Scope at the time of publication. May be empty.    |
| Publication Time     | Timestamp                           | No        | When the version was published. Absent for Draft versions.                                             |
| Lifecycle State      | Enum (Draft, Published, Superseded) | Yes       | Current state of this version.                                                                         |

**Relationships**

| Relationship                          | Target Object             | Cardinality  | Description            |
| ------------------------------------- | ------------------------- | ------------ | ---------------------- |
| snapshots reality for                 | Planning Scope (SE-C-010) | Many-to-One  | Scope.                 |
| snapshot version references demand    | Demand (SE-C-013)         | Many-to-Many | Demand in snapshot.    |
| snapshot version references supply    | Supply (SE-C-014)         | Many-to-Many | Supply in snapshot.    |
| snapshot version references inventory | Inventory (SE-C-015)      | Many-to-Many | Inventory in snapshot. |

**Invariants:** Planning Scope is aggregate identity; exactly one Published version per scope; Published version immutable; all references satisfy scope boundary at publication time.

**Dependencies:** SE-C-010 Planning Scope, SE-C-013 Demand, SE-C-014 Supply, SE-C-015 Inventory, SE-C-022 Timestamp.

**Traceability:** CN-003, CN-004; ARS §3.4, §4, §16.


### SE‑C‑031 – Time Zone

**Business Intent:** Provide the single authoritative enterprise reference for time zones used across all planning domains.

**Enterprise Meaning:** A Time Zone is a named geographical region that observes a uniform standard time. The enterprise adopts the IANA Time Zone Database as its standard. The time zone identity is immutable and globally recognised.

**Identity:** Time Zone Identifier is the immutable enterprise identity of the Time Zone. It must be a valid IANA time zone name.

**Applied Semantic Patterns:** Aggregate Root

**Semantic Ownership**

- **Time Zone owns:** the enterprise recognition of an IANA time zone.
- **Time Zone excludes:** business hours, working calendars.

**Authority Specification Contract**

| Section                      | Value                                                                                                                  |
| ---------------------------- | ---------------------------------------------------------------------------------------------------------------------- |
| Semantic Authority           | Enterprise Reference Data                                                                                              |
| Steward Domain               | Core                                                                                                                   |
| Mutation Authority           | Global Reference Standard                                                                                              |
| Authoritative Representation | The enterprise’s list of recognised time zones.                                                                        |
| Authority Scope              | Enterprise‑wide.                                                                                                       |
| Intended Consumers           | All domains that record or interpret timestamps (Location, Calendar, any object with Business Time or Effective Time). |
| Non-Intended Consumers       | None.                                                                                                                  |
| Business Responsibility      | Preserve the authoritative enterprise meaning, identity, lifecycle integrity, and published contract of this semantic object. |
| Supersedes                   | None.                                                                                                                  |
| Superseded By                | None.                                                                                                                  |

**Consumer Specification Contract**

| Section                 | Value                                                                                                                                                     |
| ----------------------- | --------------------------------------------------------------------------------------------------------------------------------------------------------- |
| Business Guarantees     | The time zone identifier is a valid IANA time zone name. The mapping from identifier to UTC offset is maintained externally and consumed by the platform. |
| Required Interpretation | Consumers shall use the IANA identifier. UTC conversions are performed using the standard database.                                                       |
| Known Limitations       | Does not define business hours or working calendars; those are governed separately.                                                                       |
| Version Expectations    | IANA identifiers are stable. New identifiers may be added; existing ones are not altered.                                                                 |
| Freshness Expectations  | The reference list is updated when the enterprise adopts a new IANA database version.                                                                     |
| Intended Consumers      | All capabilities.                                                                                                                                         |
| Non-Intended Consumers  | None.                                                                                                                                                     |
| Declared Consumers      | Governed by Chapter 5.2 Declared Consumer Matrix.                                                                                                         |
| Consumer Responsibility | The consumer shall reference this object solely by its immutable identifier and shall not infer semantics not published in this contract.                             |
| Required Attributes     | Immutable identifier; mandatory attributes; attributes required by declared structural relationships. Domain-specific required attributes must be declared in the consuming Domain Semantic Model. |
| Authoritative Source    | SE-C-031 Time Zone.                                                                                                                                       |

**Lifecycle Specification Contract:** No governed business lifecycle. Time Zone entries are immutable reference entries. Enterprise recognition may be updated by the Steward Domain, but an individual Time Zone identity does not transition through business states.

**Information Model**

| Attribute            | Type           | Mandatory | Description                                                                                        |
| -------------------- | -------------- | --------- | -------------------------------------------------------------------------------------------------- |
| Time Zone Identifier | ID (immutable) | Yes       | The IANA time zone name (e.g., “Europe/Berlin”).                                                   |
| Display Name         | String         | Yes       | Human‑readable label.                                                                              |
| UTC Offset           | String         | No        | Standard offset (e.g., “+01:00”) for reference; not authoritative for daylight saving transitions. |

**Relationships:** None.

**Invariants:** Identifier must be a valid, non‑empty IANA time zone name; unique across the enterprise.

**Dependencies:** None.

**Traceability**

| Direction | Reference                                                                              |
| --------- | -------------------------------------------------------------------------------------- |
| Upward    | Constitution CN-003, CN-004; ARS §3, §4, §16; Enterprise Semantic Model Chapter 4.      |
| Admission | Enterprise Vocabulary admission record.                                                 |
| Downward  | Declared consuming domains and capabilities.                                            |


### SE‑C‑032 – Unit of Measure

**Business Intent:** Provide the single authoritative enterprise reference of all units of measure recognised for planning, execution, and reporting.

**Enterprise Meaning:** A Unit of Measure is a standardised quantity used to express the magnitude of an enterprise fact. It answers “in what unit” for every quantity. The set of units is governed externally and adopted by the enterprise. A unit is the reference, not the value. Its semantic meaning is immutable; the enterprise’s adoption of a particular unit may evolve over time under governance.

**Identity:** Unit Identifier is the immutable enterprise identity of the Unit of Measure.

**Applied Semantic Patterns:** Aggregate Root

**Semantic Ownership**

- **Unit of Measure owns:** the enterprise recognition of a measurement unit.
- **Unit of Measure excludes:** quantities, conversion factors, currency codes.

**Authority Specification Contract**

| Section                      | Value                                                 |
| ---------------------------- | ----------------------------------------------------- |
| Semantic Authority           | Enterprise Measurement Standards Authority            |
| Steward Domain               | Core                                                  |
| Mutation Authority           | Global Reference Standard                             |
| Authoritative Representation | The enterprise’s list of recognised units of measure. |
| Authority Scope              | Enterprise‑wide                                       |
| Intended Consumers           | All capabilities that record or compute quantities.   |
| Non-Intended Consumers       | None.                                                 |
| Business Responsibility      | Preserve the authoritative enterprise meaning, identity, lifecycle integrity, and published contract of this semantic object. |
| Supersedes                   | None.                                                 |
| Superseded By                | None.                                                 |

**Consumer Specification Contract**

| Section                 | Value                                                                                                                                           |
| ----------------------- | ----------------------------------------------------------------------------------------------------------------------------------------------- |
| Business Guarantees     | Every unit has a unique, immutable identifier. The list is complete for the enterprise’s planning purposes.                                     |
| Required Interpretation | Consumers shall reference units solely by their identifier. The unit itself does not define conversion logic.                                   |
| Known Limitations       | Does not define relationships between units or conversion factors. A unit’s presence does not guarantee suitability for any particular context. |
| Version Expectations    | The reference list is updated when a new unit is adopted. Existing identifiers are never altered. Retired units remain for historical data.     |
| Freshness Expectations  | Maintained continuously by the Steward Domain.                                                                                                  |
| Intended Consumers      | All capabilities.                                                                                                                               |
| Non‑Intended Consumers  | None.                                                                                                                                           |
| Declared Consumers      | Governed by Chapter 5.2 Declared Consumer Matrix.                                                                                               |
| Consumer Responsibility | The consumer shall reference this object solely by its immutable identifier and shall not infer semantics not published in this contract.     |
| Required Attributes     | Immutable identifier; mandatory attributes; attributes required by declared structural relationships. Domain-specific required attributes must be declared in the consuming Domain Semantic Model. |
| Authoritative Source    | SE-C-032 Unit of Measure.                                                                                                                       |

**Lifecycle Specification Contract**

The semantic meaning of a unit (e.g., “kilogram”) is immutable. Enterprise adoption of a unit is governed by the Steward Domain and may evolve:

| Adoption State | Description                                                                  |
| -------------- | ---------------------------------------------------------------------------- |
| Admitted       | Unit is recognised and available for use.                                    |
| Deprecated     | Unit is still valid for historical data but should not be used in new plans. |
| Retired        | Unit is no longer recognised; retained for audit only.                       |

The Steward Domain manages the adoption lifecycle.

**Transition Trigger Standard:** Transitions are triggered only by the Business Trigger associated with this object’s Mutation Authority archetype, as defined in Section 2.7. Capability specifications shall map Aggregate Behaviors to these triggers. The underlying semantic meaning does not change.

**Information Model**

| Attribute           | Type           | Mandatory | Description                                                                                                                                                                                                                            |
| ------------------- | -------------- | --------- | -------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------- |
| Unit Identifier     | ID (immutable) | Yes       | Enterprise identifier for the unit.                                                                                                                                                                                                    |
| Unit Name           | String         | Yes       | Human‑readable name (e.g., “Kilogram”, “Metre”, “Piece”, “Hour”).                                                                                                                                                                      |
| Unit Classification | String         | Yes       | The physical quantity dimension (e.g., “Mass”, “Length”, “Time”, “Count”). The set of recognised classifications is governed by the Steward Domain and may be formalised as a Measurement Dimension object when required by consumers. |

**Relationships**

| Relationship  | Target Object                       | Cardinality | Description                                                     |
| ------------- | ----------------------------------- | ----------- | --------------------------------------------------------------- |
| referenced by | Item, Quantity, Duration, Capacity | One‑to‑Many | Quantified enterprise facts reference a unit by its identifier. |

**Invariants:** Unit Identifier is immutable once admitted; must be unique across all units; Unit Classification must be consistent with the physical quantity it represents.

**Dependencies**

| Dependency Type       | Description                                                                                                                                                       |
| --------------------- | ----------------------------------------------------------------------------------------------------------------------------------------------------------------- |
| Semantic Dependency   | None.                                                                                                                                                             |
| Conceptual Dependency | Depends on the enterprise definition of measurable dimensions (physical quantities). This is a conceptual foundation, not a reference to another Semantic Object. |

**Traceability**

| Direction | Reference                                                                              |
| --------- | -------------------------------------------------------------------------------------- |
| Upward    | Constitution CN-003, CN-004; ARS §3, §4, §16; Enterprise Semantic Model Chapter 4.      |
| Admission | Enterprise Vocabulary admission record.                                                 |
| Downward  | Declared consuming domains and capabilities.                                            |


### SE‑C‑033 – Calendar

**Business Intent:** Provide the authoritative enterprise definition of recurring and exceptional business time structures used to distinguish available and unavailable time across all planning, scheduling, and execution activities.

**Enterprise Meaning:** A Calendar defines when business time is available or unavailable. It is an enterprise reference that answers “is this time period available for planning?” Its pattern of working and non‑working intervals is governed; the underlying semantic meaning of a working day or a holiday is immutable. Calendars do not contain actual events or resource assignments; they define the temporal framework within which those facts occur.

**Identity:** Calendar Identifier uniquely identifies the Calendar. Calendar Version Number identifies a governed version of that Calendar. The Active version is authoritative for new planning references.

**Applied Semantic Patterns:** Aggregate Root

**Semantic Ownership**

- **Calendar owns:**
  - Calendar identity.
  - Versioned availability definition.
  - Recurring working day and shift definitions.
  - Holiday and non‑working day lists.
  - Exceptional closures and one‑time availability changes.
  - Maintenance windows that block availability.
  - Adoption state of each calendar version.
- **Calendar excludes:**
  - Actual resource utilization or load.
  - Time zone definitions; the Calendar references Time Zone.
  - Business events or transactions.

**Authority Specification Contract**

| Section                      | Value                                                                                                                     |
| ---------------------------- | ------------------------------------------------------------------------------------------------------------------------- |
| Semantic Authority           | Enterprise Time Governance Authority                                                                                      |
| Steward Domain               | Core                                                                                                                      |
| Mutation Authority           |                                                      |
| Authoritative Representation | The enterprise’s definition of available and unavailable business time.                                                   |
| Authority Scope              | Enterprise‑wide                                                                                                           |
| Intended Consumers           | Resource, Capacity, Supply Plan, Transportation Lane, and any capability that schedules or constrains activities by time. |
| Non‑Intended Consumers       | None.                                                                                                                     |
| Business Responsibility      | Preserve the authoritative enterprise meaning, identity, lifecycle integrity, and published contract of this semantic object. |
| Supersedes                   | None.                                                                                                                     |
| Superseded By                | None.                                                                                                                     |

**Consumer Specification Contract**

| Section                 | Value                                                                                                                                                                                                     |
| ----------------------- | --------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------- |
| Business Guarantees     | Every Calendar has a unique, immutable identifier. Its pattern of available and unavailable time is accurate and governed.                                                                                |
| Required Interpretation | Consumers shall use the Calendar to determine whether a time point or interval is available for planning. The Calendar defines time availability; it does not guarantee that capacity or resources exist. |
| Known Limitations       | Does not include actual utilization or load. Calendars are time definitions; dynamic changes (e.g., unplanned downtime) are recorded as operational facts in the owning domain.                           |
| Version Expectations    | Calendar versions reflect changes to the pattern. Historical versions remain available for past planning periods.                                                                                         |
| Freshness Expectations  | Maintained by the Steward Domain. Stale calendars may cause incorrect availability assessments.                                                                                                           |
| Intended Consumers      | All capabilities that schedule or evaluate time‑dependent activities.                                                                                                                                     |
| Non‑Intended Consumers  | None.                                                                                                                                                                                                     |
| Declared Consumers      | Governed by Chapter 5.2 Declared Consumer Matrix.                                                                                                                                                         |
| Consumer Responsibility | The consumer shall reference this object solely by its immutable identifier and shall not infer semantics not published in this contract.                                                               |
| Required Attributes     | Immutable identifier; mandatory attributes; attributes required by declared structural relationships. Domain-specific required attributes must be declared in the consuming Domain Semantic Model.     |
| Authoritative Source    | SE-C-033 Calendar.                                                                                                                                                                                        |

**Lifecycle Specification Contract**

The semantic meaning of a calendar’s availability definition is immutable. The enterprise’s adoption of a specific calendar version is governed.

| Adoption State | Description                                                           |
| -------------- | --------------------------------------------------------------------- |
| Active         | Calendar version is available for use.                                |
| Superseded     | A newer version has replaced this one; retained for historical plans. |
| Retired        | Calendar is no longer recognised; retained for audit.                 |

The Steward Domain manages the adoption lifecycle.

**Transition Trigger Standard:** Transitions are triggered only by the Business Trigger associated with this object’s Mutation Authority archetype, as defined in Section 2.7. Capability specifications shall map Aggregate Behaviors to these triggers.

**Information Model**

| Attribute           | Type                                                                              | Mandatory | Description                                                                                                                                                                                                                                                 |
| ------------------- | --------------------------------------------------------------------------------- | --------- | ----------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------- |
| Calendar Identifier | ID (immutable)                                                                    | Yes       | Unique enterprise identity for the calendar.                                                                                                                                                                                                                |
| Calendar Name       | String                                                                            | Yes       | Descriptive name.                                                                                                                                                                                                                                           |
| Time Zone           | Reference (SE-C-031)                                                              | Yes       | The Time Zone in which calendar availability is interpreted.                                                                                                                                                                                                |
| Calendar Definition | Calendar Definition (structured description of working and non‑working intervals) | Yes       | The enterprise specification of available and unavailable time. This includes recurring patterns (e.g., weekdays, shifts), named non‑working periods (e.g., holidays), and one‑time exceptions. The exact representation is governed by the Steward Domain. |
| Version Number      | Integer                                                                           | Yes       | Monotonically increasing; identifies the version of the calendar definition.                                                                                                                                                                                |
| Adoption State      | Enum (Active, Superseded, Retired)                                                | Yes       | Current state.                                                                                                                                                                                                                                              |

**Relationships**

| Relationship  | Target Object                                        | Cardinality | Description                                                           |
| ------------- | ---------------------------------------------------- | ----------- | --------------------------------------------------------------------- |
| uses          | Time Zone (SE-C-031)                                 | Many-to-One | The time zone in which calendar availability is interpreted.          |
| referenced by | Resource, Capacity, Production Schedule, Supply Plan | One‑to‑Many | Planning objects reference a Calendar to determine time availability. |

**Invariants:**
- Calendar Identifier immutable once created; must define at least one available interval; Version Number increases monotonically; a retired calendar version cannot be referenced by new planning objects.
- At most one Active version shall exist for a given Calendar Identifier.

**Dependencies**

| Dependency Type       | Description         |
| --------------------- | ------------------- |
| Semantic Dependency   | SE-C-031 Time Zone. |
| Conceptual Dependency | Depends on the enterprise concept of time intervals and the definition of business availability. |

**Decomposition Review**

**Question:** Does Calendar Version possess independent enterprise meaning?

**Answer:** Not at this stage. Calendar Version is a historical record of a calendar’s definition; it does not have an independent lifecycle or identity separate from the Calendar itself. The versioning mechanism is a governance concern, not an independent semantic concept. If future evidence demonstrates otherwise, this review will be reopened.

**Traceability**

| Direction | Reference                                                                              |
| --------- | -------------------------------------------------------------------------------------- |
| Upward    | Constitution CN-003, CN-004; ARS §3, §4, §16; Enterprise Semantic Model Chapter 4.      |
| Admission | Enterprise Vocabulary admission record.                                                 |
| Downward  | Declared consuming domains and capabilities.                                            |


### SE‑C‑034 – Planning Period

**Business Intent:** Provide the enterprise‑governed reference of all standard planning time units used across the organisation to organise, aggregate, and reason about planning information.

**Enterprise Meaning:** A Planning Period is an enterprise‑recognised unit of planning time. It answers “what is the basic time interval we plan in?”. The set of recognised units is governed and immutable; individual periods are referenced by their identifier, exactly as Currency or Unit of Measure are referenced.

**Identity:** Planning Period Identifier is the immutable enterprise identity of the Planning Period.

**Applied Semantic Patterns:** Aggregate Root

**Semantic Ownership**

- **Planning Period owns:** the enterprise recognition of a standard planning time unit.
- **Planning Period excludes:** mixed bucket sequences, calendar alignment, temporal extent of a plan.

**Authority Specification Contract**

| Section                      | Value                                                         |
| ---------------------------- | ------------------------------------------------------------- |
| Semantic Authority           | Core Domain                                                   |
| Steward Domain               | Core                                                          |
| Mutation Authority           | Enterprise-Governed Maste                                     |
| Authoritative Representation | The enterprise‑governed list of standard planning time units. |
| Authority Scope              | Enterprise‑wide                                               |
| Intended Consumers           | Planning Scope, Plan, Supply Plan, Demand Plan, Scenario.     |
| Non‑Intended Consumers       | None.                                                         |
| Business Responsibility      | Preserve the authoritative enterprise meaning, identity, lifecycle integrity, and published contract of this semantic object. |
| Supersedes                   | None.                                                         |
| Superseded By                | None.                                                         |

**Consumer Specification Contract**

| Section                 | Value                                                                                                                                                                                                                                                       |
| ----------------------- | ----------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------- |
| Business Guarantees     | Every Planning Period has a unique, immutable identifier. The list is complete and governed. Consumers always reference the same enterprise definition for “Week”, “Month”, etc.                                                                            |
| Required Interpretation | Consumers shall use the Planning Period identifier to specify the grain at which planning data is aggregated or displayed. The actual calendar duration of any specific instance is determined by a referenced Calendar, not by the Planning Period itself. |
| Known Limitations       | Does not define the length of any specific instance of the period (e.g., “February” vs “March”). Does not prescribe how many periods are used in a planning activity.                                                                                       |
| Version Expectations    | The reference list is updated when a new period is adopted. Existing identifiers are never altered. Retired periods remain for historical data.                                                                                                             |
| Freshness Expectations  | Maintained continuously by the Steward Domain.                                                                                                                                                                                                              |
| Intended Consumers      | All capabilities that require a planning time unit.                                                                                                                                                                                                         |
| Non‑Intended Consumers  | None.                                                                                                                                                                                                                                                       |
| Declared Consumers      | Governed by Chapter 5.2 Declared Consumer Matrix.                                                                                                                                                                                                           |
| Consumer Responsibility | The consumer shall reference this object solely by its immutable identifier and shall not infer semantics not published in this contract.                                                                                                                   |
| Required Attributes     | Immutable identifier; mandatory attributes; attributes required by declared structural relationships. Domain-specific required attributes must be declared in the consuming Domain Semantic Model.                                                       |
| Authoritative Source    | SE-C-034 Planning Period.                                                                                                                                                                                                                                   |

**Lifecycle Specification Contract**

The semantic meaning of a planning period (e.g., “Month”) is immutable. Enterprise adoption of a specific period is governed by the Steward Domain and may evolve.

| Adoption State | Description                                                                                 |
| -------------- | ------------------------------------------------------------------------------------------- |
| Admitted       | Period is recognised and available for use.                                                 |
| Deprecated     | Period remains valid for historical data but should not be used in new planning activities. |
| Retired        | Period is no longer recognised; retained for audit only.                                    |

The Steward Domain manages the adoption lifecycle.

**Transition Trigger Standard:** Transitions are triggered only by the Business Trigger associated with this object’s Mutation Authority archetype, as defined in Section 2.7. Capability specifications shall map Aggregate Behaviors to these triggers.

**Information Model**

| Attribute                  | Type                                 | Mandatory | Description                                                                  |
| -------------------------- | ------------------------------------ | --------- | ---------------------------------------------------------------------------- |
| Planning Period Identifier | ID (immutable)                       | Yes       | The enterprise‑recognised planning time unit (e.g., “Day”, “Week”, “Month”). |
| Display Name               | String                               | Yes       | Human‑readable label.                                                        |
| Adoption State             | Enum (Admitted, Deprecated, Retired) | Yes       | Current state.                                                               |

**Relationships:** None. Self‑contained reference; planning objects reference a Planning Period by its identifier.

**Invariants:** Planning Period Identifier immutable once admitted; must be unique across all planning periods; a retired period cannot be referenced by new planning activities.

**Dependencies:** None.

**Decomposition Review:** Not applicable.

**Traceability**

| Direction | Reference                                                                              |
| --------- | -------------------------------------------------------------------------------------- |
| Upward    | Constitution CN-003, CN-004; ARS §3, §4, §16; Enterprise Semantic Model Chapter 4.      |
| Admission | Enterprise Vocabulary admission record.                                                 |
| Downward  | Declared consuming domains and capabilities.                                            |


### SE‑C‑035 – Performance Indicator Catalog

**Business Intent:** Provide the single authoritative, governed collection of all Performance Indicator definitions recognised by the enterprise, ensuring that every measure used across all Intelligence domains has a unique identity, stable semantic meaning, and governed lifecycle.

**Enterprise Meaning:** The Performance Indicator Catalog is the enterprise’s master registry of what is measured, why it is measured, and what each measure means. It does not contain measured values; it owns the definitions that every domain‑level Performance Indicator specification instantiates. The catalog enforces uniqueness of identifiers and governs the lifecycle of each definition.

**Identity:** Catalog Identifier uniquely identifies this version of the Performance Indicator Catalog.

**Applied Semantic Patterns:** Aggregate Root

**Semantic Ownership**

| Ownership Dimension    | Value                                                                                                                       |
| ---------------------- | --------------------------------------------------------------------------------------------------------------------------- |
| Semantic Authority     | Core Domain                  |
| Steward Domain         | Core Domain                                                                                      |
| Mutation Authority | Enterprise-Governed Master Data |
| Primary Consumers      | All Intelligence domains (Supply, Demand, Promise, Scenario, Knowledge) that define domain‑specific Performance Indicators. |

**Authority Specification Contract**

| Section                      | Value                                                                                                  |
| ---------------------------- | ------------------------------------------------------------------------------------------------------ |
| Semantic Authority           | Core Domain   |
| Steward Domain               | Core                                                                                                   |
| Mutation Authority           | Enterprise-Governed Master Data                                                                                 |
| Authoritative Representation | The enterprise’s single, governed collection of Performance Indicator definitions.                     |
| Authority Scope              | Enterprise‑wide                                                                                        |
| Intended Consumers           | All Intelligence domains; all capabilities that compute, publish, or consume performance measurements. |
| Non‑Intended Consumers       | None.                                                                                                  |
| Business Responsibility      | Preserve the authoritative enterprise meaning, identity, lifecycle integrity, and published contract of this semantic object. |
| Supersedes                   | None.                                                                                                  |
| Superseded By                | None.                                                                                                  |

**Consumer Specification Contract**

| Section                 | Value                                                                                                                                                                                                                                                       |
| ----------------------- | ----------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------- |
| Business Guarantees     | Every Performance Indicator definition has a unique, permanent identifier. The catalog ensures no two definitions share the same identity. Definitions are versioned; changes to a definition’s semantic core create a new version, preserving the old one. |
| Required Interpretation | Consumers shall reference a Performance Indicator solely by its immutable identifier. The definition provides the formula and semantic dependencies; the actual measured values are published separately as Knowledge Artifacts.                            |
| Known Limitations       | The catalog does not contain measured values, publication schedules, or interpretation thresholds. Those belong to domain specifications and policies.                                                                                                      |
| Version Expectations    | Each PI definition carries a version number. The catalog retains all historical versions.                                                                                                                                                                   |
| Freshness Expectations  | The catalog is maintained by the Steward Domain. New PI definitions are added through governed admission; existing definitions are evolved through versioning.                                                                                              |
| Intended Consumers      | All Intelligence domains and any capability that requires an authoritative Performance Indicator definition.                                                                                                                                                |
| Non‑Intended Consumers  | None.                                                                                                                                                                                                                                                       |
| Declared Consumers      | Governed by Chapter 5.2 Declared Consumer Matrix.                                                                                                                                                                                                           |
| Consumer Responsibility | The consumer shall reference this object solely by its immutable identifier and shall not infer semantics not published in this contract.                                                                                                                   |
| Required Attributes     | Immutable identifier; mandatory attributes; attributes required by declared structural relationships. Domain-specific required attributes must be declared in the consuming Domain Semantic Model.                                                       |
| Authoritative Source    | SE-C-035 Performance Indicator Catalog.                                                                                                                                                                                                                    |

**Lifecycle Specification Contract**

| State      | Description                                                                                   |
| ---------- | --------------------------------------------------------------------------------------------- |
| Active     | The catalog is the authoritative source of PI definitions.                                    |
| Deprecated | A newer version of the catalog has superseded this one; retained for historical traceability. |
| Retired    | The catalog is no longer in use; retained for audit.                                          |

- **Terminal States:** Retired.
- **History Preservation:** All versions of the catalog and its contained PI definitions are retained permanently.

**Transition Trigger Standard:** Transitions are triggered only by the Business Trigger associated with this object’s Mutation Authority archetype, as defined in Section 2.7. Capability specifications shall map Aggregate Behaviors to these triggers.

**Information Model**

| Attribute                         | Type                               | Mandatory | Description                                                 |
| --------------------------------- | ---------------------------------- | --------- | ----------------------------------------------------------- |
| Catalog Identifier                | ID (immutable)                     | Yes       | Unique enterprise identity for this version of the catalog. |
| Version Number                    | Integer                            | Yes       | Monotonically increasing.                                   |
| Performance Indicator Definitions | List of SE‑C‑036                   | Yes       | The collection of governed PI definitions in this version.  |
| Lifecycle State                   | Enum (Active, Deprecated, Retired) | Yes       | Current state.                                              |

**Relationships**

| Relationship | Target Object                    | Cardinality | Description                                  |
| ------------ | -------------------------------- | ----------- | -------------------------------------------- |
| contains     | Performance Indicator (SE‑C‑036) | One‑to‑Many | The PI definitions governed by this catalog. |

**Invariants:**
- Each PI identifier within the catalog is unique.
- A PI definition can only be modified by creating a new version of the definition within a new catalog version.
- Once Active, a catalog version is immutable.

**Dependencies:** None.

**Traceability:** CN‑003, CN‑004; ARS §3, §4, §16.

---

### SE-C-037 — Enterprise Governed Vocabulary

**Business Intent:** Provide the single authoritative governed registry for controlled vocabularies and governed identifiers used by Enterprise Semantic Objects.

**Enterprise Meaning:** The Enterprise Governed Vocabulary is the enterprise registry that owns governed identifier entries. It provides stable, explainable, and machine-consumable classifications for semantic attributes without hard-coding values into the specification.

**Applied Semantic Patterns:** Aggregate Root

**Identity:** Catalog Identifier uniquely identifies this version of the Enterprise Governed Vocabulary.

**Mutation Authority:** Enterprise-Governed Master Data

**Semantic Ownership**

- **Enterprise Governed Vocabulary owns:**
  - Vocabulary identity.
  - Vocabulary categories.
  - Governed identifier entries.
  - Entry lifecycle states.
  - Versioned catalog state.

- **Enterprise Governed Vocabulary excludes:**
  - Business facts.
  - Planning transactions.
  - Operational decisions.
  - Policy interpretation thresholds.

**Authority Specification Contract**

| Section | Value |
| --- | --- |
| Semantic Authority | Core Domain |
| Steward Domain | Core |
| Mutation Authority | Enterprise-Governed Master Data |
| Business Responsibility | Preserve the authoritative enterprise meaning, identity, lifecycle integrity, and published contract of this semantic object. |
| Authoritative Representation | The enterprise registry of governed identifier entries. |
| Authority Scope | Enterprise-wide |
| Intended Consumers | All objects and capabilities using governed identifiers. |
| Non-Intended Consumers | None |
| Supersedes | None |
| Superseded By | None |

**Consumer Specification Contract**

| Section | Value |
| --- | --- |
| Business Guarantees | Every governed identifier entry has a stable identity within its vocabulary category. |
| Required Interpretation | Consumers shall reference governed identifiers by entry identity, not by display name. |
| Known Limitations | The vocabulary does not define business behavior or interpretation thresholds. |
| Declared Consumers | Governed by Chapter 5.2 Declared Consumer Matrix. |
| Consumer Responsibility | The consumer shall reference this object solely by its immutable identifier and shall not infer semantics not published in this contract. |
| Required Attributes | Immutable identifier; mandatory attributes; attributes required by declared structural relationships. Domain-specific required attributes must be declared in the consuming Domain Semantic Model. |
| Authoritative Source | SE-C-037 Enterprise Governed Vocabulary. |

**Lifecycle Specification Contract**

| State | Description |
| --- | --- |
| Active | The vocabulary version is authoritative. |
| Deprecated | A newer vocabulary version has superseded this version. |
| Retired | The vocabulary version is no longer in use. |

- Permitted Transitions: Active → Deprecated; Active → Retired; Deprecated → Retired.
- Terminal State: Retired.
- History Preservation: All vocabulary versions are retained.
- **Transition Trigger Standard:** Transitions are triggered only by the Business Trigger associated with this object’s Mutation Authority archetype, as defined in Section 2.7. Capability specifications shall map Aggregate Behaviors to these triggers.

**Information Model**

| Attribute | Type | Mandatory | Description |
| --- | --- | --- | --- |
| Catalog Identifier | ID (immutable) | Yes | Unique identity for this vocabulary version. |
| Version Number | Integer | Yes | Monotonically increasing. |
| Vocabulary Entries | List of Vocabulary Entry | Yes | Governed identifier entries. |
| Lifecycle State | Enum (Active, Deprecated, Retired) | Yes | Current state. |

**Relationships**

| Relationship | Target Object | Cardinality | Description |
| --- | --- | --- | --- |
| contains | Vocabulary Entry | One-to-Many | Governed identifier entries owned by this catalog. |

**Invariants**

- Catalog Identifier is immutable.
- Vocabulary entries are unique within a vocabulary category.
- An Active vocabulary version is immutable.
- A Retired vocabulary cannot be referenced by new semantic objects.

**Dependencies**

| Dependency Type | Description |
| --- | --- |
| Semantic Dependency | None. |
| Conceptual Dependency | Enterprise governance of controlled vocabularies. |

**Traceability**

| Direction | Reference |
| --- | --- |
| Upward | Constitution CN-003, CN-004; ARS §3, §4, §16; Enterprise Semantic Model Chapter 4. |
| Admission | Enterprise Vocabulary governance. |
| Downward | All Enterprise Semantic Objects using governed identifiers. |

---

### SE-C-040 – Item Transition

**Business Intent:** Provide the single authoritative enterprise identity for a governed succession 
relationship between two enterprise items, establishing that one item replaces another.

**Enterprise Meaning:** An Item Transition is an enterprise-recognised succession relationship between two items. It answers "which item replaces which item?" The transition identity is stable; planning-specific parameters such as history mapping rules, substitution eligibility, and phase-in/phase-out curves are domain-specific extensions, not part of the core transition identity. A transition is not merely a reference; it is an enterprise authorisation that one item succeeds another.

**Identity:** Transition Identifier is the immutable enterprise identity of the Item Transition.

**Applied Semantic Patterns:** Aggregate Root

**Semantic Ownership**
- Item Transition owns: identity, superseded/superseding item references, 
  transition type, effective/end dates, lifecycle.
- Item Transition excludes: item identities (owned by SE-C-001), 
  demand/supply/inventory records, allocation decisions, execution actions,
  history mapping rules, substitution eligibility, phase-in/phase-out parameters.

**Authority Specification Contract**

| Section                      | Value                                                                                                        |
| ---------------------------- | ------------------------------------------------------------------------------------------------------------ |
| Semantic Authority           | Core Domain                                                                                                  |
| Steward Domain               | Core                                                                                                         |
| Mutation Authority           | External System of Record                                                                                    |
| Authoritative Representation | The enterprise definition of recognised item succession relationships.                                       |
| Authority Scope              | Enterprise-wide                                                                                              |
| Intended Consumers           | Demand Intelligence, Supply Intelligence, Promise Intelligence, Scenario Intelligence, Inventory Planning.   |
| Non-Intended Consumers       | None                                                                                                         |
| Business Responsibility      | Preserve the authoritative enterprise meaning, identity, lifecycle integrity, and published contract of this semantic object. |
| Supersedes                   | None                                                                                                         |
| Superseded By                | None                                                                                                         |

**Consumer Specification Contract**

| Section                 | Value                                                                                                                                                                                                                          |
| ----------------------- | ------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------ |
| Business Guarantees     | Every Item Transition has a unique, immutable identifier. Its identity and direction are stable regardless of planning parameter changes. At most one Active transition exists per Superseded Item at any moment.               |
| Required Interpretation | Consumers shall interpret the transition as the recognised enterprise succession relationship between two items. Planning-specific parameters must be obtained from domain objects that reference the transition, not inferred from the transition itself. |
| Known Limitations       | Defines only the existence and direction of a succession. Does not specify how demand history transfers, whether substitution is permitted, or how procurement phases in/out. Those are domain-specific planning concerns.    |
| Declared Consumers      | Governed by Chapter 5.2 Declared Consumer Matrix.                                                                                                                                                                              |
| Consumer Responsibility | The consumer shall reference this object solely by its immutable identifier and shall not infer semantics not published in this contract.                                                                                     |
| Required Attributes     | Immutable identifier; mandatory attributes; attributes required by declared structural relationships. Domain-specific required attributes must be declared in the consuming Domain Semantic Model.                            |
| Authoritative Source    | SE-C-040 Item Transition.                                                                                                                                                                                                      |

**Lifecycle Specification Contract**

| State    | Description                                                                         |
| -------- | ----------------------------------------------------------------------------------- |
| Active   | Transition is recognised and governs planning behavior.                             |
| Inactive | Transition is temporarily suspended from planning references.                       |
| Retired  | Transition is permanently removed from enterprise use; retained for historical reference. |

- Permitted Transitions: Active ↔ Inactive; Active → Retired; Inactive → Retired.
- Terminal State: Retired.
- History Preservation: State changes are recorded for audit.
- Versioning Rules: Not applicable.

**Transition Trigger Standard:** Transitions are triggered only by the Business Trigger associated with this object's Mutation Authority archetype (External Master Data Change Accepted), as defined in Section 2.7. Capability specifications shall map Aggregate Behaviors to these triggers.

**Information Model**

| Attribute              | Type                                        | Mandatory | Description                                                                                     |
| ---------------------- | ------------------------------------------- | --------- | ----------------------------------------------------------------------------------------------- |
| Transition Identifier  | ID (immutable)                              | Yes       | Unique enterprise identity.                                                                     |
| Superseded Item        | Reference (SE-C-001)                        | Yes       | The item being replaced.                                                                        |
| Superseding Item       | Reference (SE-C-001)                        | Yes       | The item that replaces it.                                                                      |
| Transition Type        | Governed Identifier Reference (SE-C-037)    | Yes       | The category of succession (e.g., "DirectReplacement", "PhaseInPhaseOut", "Merge"). Governed by SE-C-037. |
| Effective Date         | Timestamp (SE-C-022)                        | Yes       | When the succession becomes applicable for planning.                                            |
| End Date               | Timestamp (SE-C-022)                        | No        | When the succession ceases to be applicable. Absent means open-ended.                           |
| Lifecycle State        | Enum (Active, Inactive, Retired)            | Yes       | Current semantic state.                                                                         |

**Relationships**

| Relationship          | Target Object       | Cardinality | Description                                      |
| --------------------- | ------------------- | ----------- | ------------------------------------------------ |
| references superseded | Item (SE-C-001)     | Many-to-One | The item being replaced.                         |
| references superseding| Item (SE-C-001)     | Many-to-One | The item that replaces it.                       |

**Invariants:**
- Transition Identifier is immutable.
- Superseded Item and Superseding Item must reference distinct items.
- Superseded Item must be in Active or Inactive state (cannot supersede a Retired item).
- Superseding Item must be in Active state.
- At most one Active transition exists per Superseded Item at any moment.
- Effective Date must be a valid UTC timestamp.
- End Date, if present, must be after Effective Date.
- A Retired transition cannot be referenced by new planning activities.

**Dependencies:**

| Dependency Type       | Description                                                    |
| --------------------- | -------------------------------------------------------------- |
| Semantic Dependency   | SE-C-001 Item, SE-C-022 Timestamp, SE-C-037 Enterprise Governed Vocabulary. |
| Conceptual Dependency  | None.                                                          |

**Decomposition Review:** No decomposition required. The succession fact is a single enterprise concept. Planning parameters are domain-specific and belong in their respective Domain Semantic Models.

**Traceability:**

| Direction | Reference                                                                              |
| --------- | -------------------------------------------------------------------------------------- |
| Upward    | Constitution CN-003, CN-004; ARS §3, §4, §16; Enterprise Semantic Model Chapter 4.   |
| Admission | Enterprise Vocabulary admission record.                                                |
| Downward  | Demand Intelligence, Supply Intelligence, Promise Intelligence, Scenario Intelligence. |

---

## 4.2 Entities
- **BOMLine** (within SE-C-018)
- **PictureVersion** (within SE-C-021)
- **Performance Indicator** (within SE-C-035)
- **Vocabulary Entry** (within SE-C-037)

### BOMLine — Entity within SE-C-018 Bill of Materials

**Business Intent:** Represent one governed component line within a Bill of Materials version.

**Identity:** Line Identifier is the immutable identity of the BOMLine within its owning BOM version.

**Owning Aggregate Root:** SE-C-018 Bill of Materials

**Information Model**

| Attribute       | Type                 | Mandatory | Description                              |
| --------------- | -------------------- | --------- | ---------------------------------------- |
| Line Identifier | ID (immutable)       | Yes       | Stable identity independent of ordering. |
| Sequence Number | Integer              | No        | Display ordering.                        |
| Component Item  | Reference (SE-C-001) | Yes       | The component item.                      |
| Quantity        | Quantity (SE-C-023)  | Yes       | Amount per parent unit.                  |

**Relationships**

| Relationship     | Target Object       | Cardinality | Description                         |
| ---------------- | ------------------- | ----------- | ----------------------------------- |
| references       | Item (SE-C-001)     | Many-to-One | The component item on this line.    |
| belongs to       | BOM (SE-C-018)      | Many-to-One | The owning Bill of Materials version. |

**Invariants**

- Line Identifier is immutable within the owning BOM version.
- Component Item must be valid.
- Quantity must be positive.
- A BOMLine cannot exist outside its owning Bill of Materials version.
- When the owning BOM version is Active, the BOMLine is immutable.

**Lifecycle Specification Contract**

BOMLine has no independent lifecycle. Its existence and immutability are governed by the lifecycle of its owning Bill of Materials version.

**Traceability**

| Direction | Reference                                                                              |
| --------- | -------------------------------------------------------------------------------------- |
| Upward    | Constitution CN-003, CN-004; ARS §3, §4, §16; Enterprise Semantic Model Chapter 4.      |
| Owner     | SE-C-018 Bill of Materials                                                             |
| Downward  | Production domain, Supply Planning, Costing                                            |

---

### PictureVersion — Entity within SE-C-021 Enterprise Picture

**Business Intent:** Preserve one immutable point-in-time snapshot version of enterprise demand, supply, and inventory for a Planning Scope.

**Identity:** Version Number uniquely identifies the PictureVersion within its owning Enterprise Picture aggregate.

**Owning Aggregate Root:** SE-C-021 Enterprise Picture

**Information Model**

| Attribute            | Type                                | Mandatory | Description                                                                                            |
| -------------------- | ----------------------------------- | --------- | ------------------------------------------------------------------------------------------------------ |
| Version Number       | Integer                             | Yes       | Monotonically increasing within the Planning Scope. This is the identity of the PictureVersion Entity. |
| Demand References    | List of Demand references           | Yes       | The Demand records that fall within the Planning Scope at the time of publication. May be empty.       |
| Supply References    | List of Supply references           | Yes       | The Supply records that fall within the Planning Scope at the time of publication. May be empty.       |
| Inventory References | List of Inventory references        | Yes       | The Inventory records that fall within the Planning Scope at the time of publication. May be empty.    |
| Publication Time     | Timestamp                           | No        | When the version was published. Absent for Draft versions.                                             |
| Lifecycle State      | Enum (Draft, Published, Superseded) | Yes       | Current state of this version.                                                                         |

**Relationships**

| Relationship     | Target Object             | Cardinality  | Description                                  |
| ---------------- | ------------------------- | ------------ | -------------------------------------------- |
| belongs to       | Enterprise Picture (SE-C-021) | Many-to-One | The owning Enterprise Picture aggregate. |
| references       | Demand (SE-C-013)         | Many-to-Many | Demand records included in this snapshot.    |
| references       | Supply (SE-C-014)         | Many-to-Many | Supply records included in this snapshot.    |
| references       | Inventory (SE-C-015)      | Many-to-Many | Inventory records included in this snapshot. |

**Invariants**

- Version Number is monotonically increasing within the owning Enterprise Picture.
- Exactly one PictureVersion is Published per Planning Scope at any time.
- A Published PictureVersion is immutable.
- A Superseded PictureVersion is immutable.
- All references in a Published PictureVersion must satisfy the Planning Scope boundary at publication time.

**Lifecycle Specification Contract**

| State      | Description                                           |
| ---------- | ----------------------------------------------------- |
| Draft      | Being prepared; not authoritative.                    |
| Published  | Authoritative; previous Published becomes Superseded. |
| Superseded | Replaced by a newer Published version.                |

- Permitted Transitions: Draft → Published; Published → Superseded.
- Terminal State: Superseded.
- History Preservation: All versions are retained permanently.

**Traceability**

| Direction | Reference                                                                              |
| --------- | -------------------------------------------------------------------------------------- |
| Upward    | Constitution CN-003, CN-004; ARS §3, §4, §16; Enterprise Semantic Model Chapter 4.      |
| Owner     | SE-C-021 Enterprise Picture                                                            |
| Downward  | Any planning capability that consumes a published Enterprise Picture.                  |

---

### SE‑C‑036 – Performance Indicator

**Business Intent:** Provide the authoritative enterprise definition of a single governed, measurable performance dimension—what is measured, why, and how it is calculated—so that every capability that computes, publishes, or consumes the measure works from a single, stable definition.

**Enterprise Meaning:** A Performance Indicator is a governed definition of what the enterprise measures. It answers: “What enterprise knowledge does this measure represent, and how is it derived?” The definition captures the semantic core—identity, meaning, formula, and dependencies. It does not contain measured values, publication schedules, or interpretation thresholds. Measured values are published as Knowledge Artifacts; interpretation is governed by policy. The definition belongs to the Performance Indicator Catalog (SE‑C‑035).

**Applied Semantic Patterns:** Entity (Governed Definition)

**Identity:** Performance Indicator Identifier (e.g., `PI-C-002`), unique within the owning catalog. Immutable once assigned. Enterprise Performance Indicator definitions governed by the Enterprise Performance Indicator Catalog use Core domain identifiers (`PI-C-xxx`).

**Owning Aggregate Root:** SE‑C‑035 – Performance Indicator Catalog

**Authority Specification Contract**

| Section                      | Value                                                                                                        |
| ---------------------------- | ------------------------------------------------------------------------------------------------------------ |
| Semantic Authority           | Core Domain                                                                                                  |
| Steward Domain               | Core Domain                                                                                                  |
| Mutation Authority           | Enterprise-Governed Master Data                                                                              |
| Authoritative Representation | The enterprise definition of a single performance dimension.                                                 |
| Authority Scope              | Enterprise‑wide                                                                                              |
| Intended Consumers           | All capabilities that compute, publish, or consume performance measurements.                                 |
| Non-Intended Consumers       | None                                                                                                         |
| Business Responsibility      | Preserve the authoritative enterprise meaning, identity, lifecycle integrity, and published contract of this semantic object. |
| Supersedes                   | None                                                                                                         |
| Superseded By                | None                                                                                                         |

**Consumer Specification Contract**

| Section                 | Value                                                                                                                                                                             |
| ----------------------- | --------------------------------------------------------------------------------------------------------------------------------------------------------------------------------- |
| Business Guarantees     | The definition is unique, stable, and governed. The formula and semantic dependencies are immutable for a given version. The definition does not embed interpretation thresholds. |
| Required Interpretation | Consumers shall reference the PI by its identifier. The definition provides the formula and dependencies; measured values are separate Knowledge Artifact instances.              |
| Known Limitations       | Does not contain measured values, publication cadences, or policy thresholds. Those are owned by the domain specification and the governing policy.                               |
| Declared Consumers      | Governed by Chapter 5.2 Declared Consumer Matrix.                                                                                                                                 |
| Consumer Responsibility | The consumer shall reference this object solely by its immutable identifier and shall not infer semantics not published in this contract.                                       |
| Required Attributes     | Immutable identifier; mandatory attributes; attributes required by declared structural relationships. Domain-specific required attributes must be declared in the consuming Domain Semantic Model. |
| Authoritative Source    | SE-C-036 Performance Indicator.                                                                                                                                                   |

**Lifecycle Specification Contract** (delegated to the owning catalog; the definition’s lifecycle mirrors the catalog version’s lifecycle)

**Information Model (Semantic Core)**

| Attribute                        | Type                                     | Mandatory | Description                                                                                                                                                   |
| -------------------------------- | ---------------------------------------- | --------- | ------------------------------------------------------------------------------------------------------------------------------------------------------------- |
| Performance Indicator Identifier | ID (immutable)                           | Yes       | Unique enterprise identity, e.g., `PI‑D‑002`.                                                                                                                 |
| Name                             | String                                   | Yes       | Enterprise‑recognised name.                                                                                                                                   |
| Measure Category                 | Governed Identifier Reference (SE-C-037) | Yes       | Governed classification of the measure.                                                                                                                       |
| Measure Nature                   | Governed Identifier Reference (SE-C-037) | Yes       | Governed nature of the measure.                                                                                                                               |
| Enterprise Question              | String                                   | Yes       | The enterprise question the measure answers.                                                                                                                  |
| Business Objectives Served       | List of BO-xxx external governance artifacts | Yes | The enterprise objectives the indicator supports. Business Objectives are external governance artifacts and are not owned by the Enterprise Semantic Model. |
| Enterprise Meaning               | String                                   | Yes       | Business‑language description of what the measure represents.                                                                                                 |
| Formula                          | String (governed)                        | Yes       | The mathematical expression and variable definitions.                                                                                                         |
| Semantic Dependencies            | Structured table                         | Yes       | Mapping of every variable to its authoritative source (SE‑xxx, or external).                                                                                  |

**Relationships**

| Relationship                | Target Object               | Cardinality  | Description                                                                                                 |
| --------------------------- | --------------------------- | ------------ | ----------------------------------------------------------------------------------------------------------- |
| references enterprise facts | various SE‑xxx              | Many‑to‑Many | The semantic objects that provide the input variables.                                                      |
| governed by policy          | PO‑xxx                      | Many‑to‑One  | The policy that defines interpretation thresholds.                                                          |
| implemented by              | Business Algorithm (BA-xxx) | Zero-or-One  | The traceable Business Algorithm that computes the Performance Indicator where the indicator is computable. |

A Performance Indicator formula is a governed semantic expression. Where a Performance Indicator is computable, a traceable Business Algorithm shall implement the formula. The Business Algorithm owns computation; the Performance Indicator owns meaning.

**Invariants:**
- The PI Identifier is unique within the owning catalog.
- The formula and semantic dependencies are immutable for a given version.
- The definition does not contain concrete threshold values; those belong to the referenced policy.

**Dependencies:** None directly; references enterprise semantic objects and policies.

**Traceability:** CN‑003, CN‑004; ARS §3, §4, §16.

---

### Vocabulary Entry — Entity within SE-C-037 Enterprise Governed Vocabulary

**Business Intent:** Represent one governed identifier entry within a governed vocabulary category.

**Identity:** Entry Identifier is the immutable identity of the vocabulary entry within its vocabulary category.

**Owning Aggregate Root:** SE-C-037 Enterprise Governed Vocabulary

**Information Model**

| Attribute | Type | Mandatory | Description |
| --- | --- | --- | --- |
| Vocabulary Category Identifier | ID (immutable) | Yes | The governed vocabulary category. |
| Entry Identifier | ID (immutable) | Yes | The governed identifier entry. |
| Entry Name | String | Yes | Human-readable name. |
| Lifecycle State | Enum (Active, Deprecated, Retired) | Yes | Current entry state. |

**Relationships**

| Relationship | Target Object | Cardinality | Description |
| --- | --- | --- | --- |
| belongs to | SE-C-037 Enterprise Governed Vocabulary | Many-to-One | Owning vocabulary catalog. |

**Invariants**

- Entry Identifier is immutable.
- Entry Identifier is unique within its Vocabulary Category Identifier.
- A Retired entry cannot be referenced by new semantic objects.

**Lifecycle Specification Contract**

| State | Description |
| --- | --- |
| Active | Entry may be referenced by new objects. |
| Deprecated | Entry remains valid for historical data but should not be used in new objects. |
| Retired | Entry is no longer valid for new references. |

- Permitted Transitions: Active → Deprecated; Active → Retired; Deprecated → Retired.
- Terminal State: Retired.
- **Transition Trigger Standard:** Transitions are triggered only by the Business Trigger associated with this object’s Mutation Authority archetype, as defined in Section 2.7. Capability specifications shall map Aggregate Behaviors to these triggers.

**Traceability**

| Direction | Reference |
| --- | --- |
| Upward | Constitution CN-003, CN-004; ARS §3, §4, §16; Enterprise Semantic Model Chapter 4. |
| Owner | SE-C-037 Enterprise Governed Vocabulary |
| Downward | All objects using governed identifier entries. |

---

## 4.3 Value Objects

### SE‑C‑022 – Timestamp

**Business Intent:** Provide the single authoritative representation of a point in time, expressed in UTC, for all enterprise facts that record when something occurred or was observed.

**Enterprise Meaning:** A Timestamp is an instantaneous point on the UTC time scale. It answers the question “when” for any enterprise event, observation, or state change. The UTC standard is the enterprise‑wide convention; no other time zone interpretation is required at the semantic level.

**Applied Semantic Patterns:** Value Object

**Semantic Ownership**

- **Timestamp owns:** the UTC instant.
- **Timestamp excludes:** local time representations, durations, business time semantics (the meaning of the timestamp—observation time, effective time, etc.—is defined by the object that uses it).

**Authority Specification Contract**

| Section                      | Value                                                   |
| ---------------------------- | ------------------------------------------------------- |
| Semantic Authority           | Core Domain                                             |
| Steward Domain               | Core                                                    |
| Mutation Authority           | Not Applicable                                          |
| Authoritative Representation | The enterprise definition of a UTC point in time.       |
| Authority Scope              | Enterprise‑wide.                                        |
| Intended Consumers           | All capabilities that record or compare temporal facts. |
| Non‑Intended Consumers       | None.                                                   |
| Business Responsibility      | Preserve the authoritative enterprise meaning, identity, lifecycle integrity, and published contract of this semantic object. |
| Supersedes                   | None.                                                   |
| Superseded By                | None.                                                   |

**Consumer Specification Contract**

| Section                 | Value                                                                                                                                                           |
| ----------------------- | --------------------------------------------------------------------------------------------------------------------------------------------------------------- |
| Business Guarantees     | Every Timestamp represents an unambiguous UTC instant. It does not drift or change with time zone rules.                                                        |
| Required Interpretation | Consumers shall interpret the value as UTC. Any local time display or conversion is the consumer’s responsibility and shall use a governed Time Zone reference. |
| Known Limitations       | Does not carry time zone or locality information. Precision is limited to the enterprise standard.                                                              |
| Version Expectations    | Not applicable; the semantic meaning of a UTC instant is immutable.                                                                                             |
| Freshness Expectations  | Not applicable; a Timestamp records a fixed point in time.                                                                                                      |
| Intended Consumers      | All capabilities.                                                                                                                                               |
| Non‑Intended Consumers  | None.                                                                                                                                                           |
| Declared Consumers      | Governed by Chapter 5.2 Declared Consumer Matrix.                                                                                                               |
| Consumer Responsibility | The consumer shall reference this object solely by its immutable identifier and shall not infer semantics not published in this contract.                             |
| Required Attributes     | Immutable identifier; mandatory attributes; attributes required by declared structural relationships. Domain-specific required attributes must be declared in the consuming Domain Semantic Model. |
| Authoritative Source    | SE-C-022 Timestamp.                                                                                                                                             |

**Lifecycle Specification Contract:** Not applicable. Value Object; semantic meaning is immutable.

**Information Model**

| Attribute | Type        | Mandatory | Description            |
| --------- | ----------- | --------- | ---------------------- |
| Instant   | UTC Instant | Yes       | The UTC point in time. |

**Relationships:** None.

**Invariants:** Instant must be a valid UTC date/time; expressible in enterprise precision.

**Dependencies:** None (conceptual dependency on UTC standard).

---

### SE‑C‑023 – Quantity

**Business Intent:** Provide the single authoritative representation of a measured amount, pairing a numeric value with a governed unit of measure.

**Enterprise Meaning:** A Quantity is a value that answers “how much” for any measurable enterprise fact. It is defined by a number and a unit of measure. The meaning of the number is inseparable from the unit. A Quantity is immutable once created; its value and unit do not change.

**Applied Semantic Patterns:** Value Object

**Semantic Ownership**

- **Quantity owns:** the numeric value and the unit reference.
- **Quantity excludes:** contextual meaning, conversion logic, tolerance.

**Authority Specification Contract**

| Section                      | Value                                            |
| ---------------------------- | ------------------------------------------------ |
| Semantic Authority           | Core Domain                                      |
| Steward Domain               | Core                                             |
| Mutation Authority           | Not Applicable                                   |
| Authoritative Representation | The enterprise definition of a measured amount.  |
| Authority Scope              | Enterprise‑wide.                                 |
| Intended Consumers           | All capabilities that record or compute amounts. |
| Non‑Intended Consumers       | None.                                            |
| Business Responsibility      | Preserve the authoritative enterprise meaning, identity, lifecycle integrity, and published contract of this semantic object. |
| Supersedes                   | None.                                            |
| Superseded By                | None.                                            |

**Consumer Specification Contract**

| Section                 | Value                                                                                                                                    |
| ----------------------- | ---------------------------------------------------------------------------------------------------------------------------------------- |
| Business Guarantees     | Every Quantity has a numeric value and a valid, active unit of measure. The meaning of the value is stable.                              |
| Required Interpretation | Consumers shall treat the value and unit as an inseparable pair. Any arithmetic operation on quantities must respect unit compatibility. |
| Known Limitations       | Does not define conversion factors or arithmetic rules.                                                                                  |
| Version Expectations    | Not applicable.                                                                                                                          |
| Freshness Expectations  | Not applicable.                                                                                                                          |
| Intended Consumers      | All capabilities.                                                                                                                        |
| Non‑Intended Consumers  | None.                                                                                                                                    |
| Declared Consumers      | Governed by Chapter 5.2 Declared Consumer Matrix.                                                                                        |
| Consumer Responsibility | The consumer shall reference this object solely by its immutable identifier and shall not infer semantics not published in this contract.     |
| Required Attributes     | Immutable identifier; mandatory attributes; attributes required by declared structural relationships. Domain-specific required attributes must be declared in the consuming Domain Semantic Model. |
| Authoritative Source    | SE-C-023 Quantity.                                                                                                                       |

**Lifecycle Specification Contract:** Not applicable.

**Information Model**

| Attribute       | Type                 | Mandatory | Description                                                            |
| --------------- | -------------------- | --------- | ---------------------------------------------------------------------- |
| Value           | Decimal              | Yes       | Magnitude; zero or positive; negative only where owning object allows. |
| Unit of Measure | Reference (SE‑C‑032) | Yes       | The unit in which the value is expressed.                              |

**Relationships**

| Relationship | Target Object              | Cardinality | Description                                      |
| ------------ | -------------------------- | ----------- | ------------------------------------------------ |
| uses         | Unit of Measure (SE‑C‑032) | Many‑to‑One | Every Quantity is expressed in exactly one unit. |

**Invariants:** Unit must be active; value + unit combination is immutable.

**Dependencies:** SE‑C‑032 Unit of Measure.

**Decomposition Review:** Not applicable.

---

### SE‑C‑024 – Duration

**Business Intent:** Provide the single authoritative representation of a time interval, expressed as a numeric value with a time unit of measure, for all enterprise facts that involve elapsed time or time spans.

**Enterprise Meaning:** A Duration is a measure of the length of time between two points, or an interval defined by a magnitude and a time unit. It answers “how long” for any planning, scheduling, or measurement context. Durations are always expressed in terms of standard time units. They do not carry calendar context or time zone; they are pure lengths of UTC time.

**Applied Semantic Patterns:** Value Object

**Semantic Ownership**

- **Duration owns:** numeric value and time unit.
- **Duration excludes:** start/end points, calendar‑dependent intervals.

**Authority Specification Contract**

| Section                      | Value                                               |
| ---------------------------- | --------------------------------------------------- |
| Semantic Authority           | Core Domain                                         |
| Steward Domain               | Core                                                |
| Mutation Authority           | Not Applicable                                      |
| Authoritative Representation | The enterprise definition of a time interval.       |
| Authority Scope              | Enterprise‑wide.                                    |
| Intended Consumers           | All capabilities that compute or record time spans. |
| Non‑Intended Consumers       | None.                                               |
| Business Responsibility      | Preserve the authoritative enterprise meaning, identity, lifecycle integrity, and published contract of this semantic object. |
| Supersedes                   | None.                                               |
| Superseded By                | None.                                               |

**Consumer Specification Contract**

| Section                 | Value                                                                                                                                       |
| ----------------------- | ------------------------------------------------------------------------------------------------------------------------------------------- |
| Business Guarantees     | Every Duration has a non‑negative numeric value and a valid time unit. The interval is a fixed length of UTC time.                          |
| Required Interpretation | Consumers shall treat the value and time unit as an inseparable pair. Durations are additive and comparable only when units are compatible. |
| Known Limitations       | Does not account for calendar effects. Those must be resolved by the consumer using Calendars.                                              |
| Version Expectations    | Not applicable.                                                                                                                             |
| Freshness Expectations  | Not applicable.                                                                                                                             |
| Intended Consumers      | All capabilities.                                                                                                                           |
| Non‑Intended Consumers  | None.                                                                                                                                       |
| Declared Consumers      | Governed by Chapter 5.2 Declared Consumer Matrix.                                                                                           |
| Consumer Responsibility | The consumer shall reference this object solely by its immutable identifier and shall not infer semantics not published in this contract.     |
| Required Attributes     | Immutable identifier; mandatory attributes; attributes required by declared structural relationships. Domain-specific required attributes must be declared in the consuming Domain Semantic Model. |
| Authoritative Source    | SE-C-024 Duration.                                                                                                                          |

**Lifecycle Specification Contract:** Not applicable.

**Information Model**

| Attribute       | Type                 | Mandatory | Description                                    |
| --------------- | -------------------- | --------- | ---------------------------------------------- |
| Value           | Decimal              | Yes       | Length; non‑negative.                          |
| Unit of Measure | Reference (SE‑C‑032) | Yes       | A time‑dimension unit (e.g., “h”, “min”, “d”). |

**Relationships**

| Relationship | Target Object              | Cardinality | Description    |
| ------------ | -------------------------- | ----------- | -------------- |
| uses         | Unit of Measure (SE‑C‑032) | Many‑to‑One | The time unit. |

**Invariants:**
- Value must be non‑negative.
- Unit of Measure must be an active, valid unit of the Time dimension.

**Dependencies:** SE‑C‑032 Unit of Measure.

---

### SE‑C‑025 – Money

**Business Intent:** Provide the single authoritative representation of a monetary amount, pairing a numeric value with an ISO 4217 currency code, for all enterprise facts that involve financial amounts.

**Enterprise Meaning:** Money is a value that answers “how much in financial terms” for costs, prices, or financial impacts. It is defined by a decimal number and a currency code that identifies the denomination. The meaning of the amount is inseparable from the currency. Money is immutable once created.

**Applied Semantic Patterns:** Value Object

**Semantic Ownership**

- **Money owns:** amount and currency code.
- **Money excludes:** exchange rates, conversion logic, financial transactions.

**Authority Specification Contract**

| Section                      | Value                                                     |
| ---------------------------- | --------------------------------------------------------- |
| Semantic Authority           | Core Domain                                               |
| Steward Domain               | Core                                                      |
| Mutation Authority           | Not Applicable                                            |
| Authoritative Representation | The enterprise definition of a monetary amount.           |
| Authority Scope              | Enterprise‑wide.                                          |
| Intended Consumers           | All capabilities that record or compute financial values. |
| Non‑Intended Consumers       | None.                                                     |
| Business Responsibility      | Preserve the authoritative enterprise meaning, identity, lifecycle integrity, and published contract of this semantic object. |
| Supersedes                   | None.                                                     |
| Superseded By                | None.                                                     |

**Consumer Specification Contract**

| Section                 | Value                                                                                                                                          |
| ----------------------- | ---------------------------------------------------------------------------------------------------------------------------------------------- |
| Business Guarantees     | Every Money value has a valid ISO 4217 currency code and a numeric amount. The meaning of the amount is stable.                                |
| Required Interpretation | Consumers shall treat the amount and currency as an inseparable pair. Comparison or arithmetic across currencies requires explicit conversion. |
| Known Limitations       | Does not define exchange rates or conversion rules.                                                                                            |
| Version Expectations    | Not applicable.                                                                                                                                |
| Freshness Expectations  | Not applicable.                                                                                                                                |
| Intended Consumers      | All capabilities.                                                                                                                              |
| Non‑Intended Consumers  | None.                                                                                                                                          |
| Declared Consumers      | Governed by Chapter 5.2 Declared Consumer Matrix.                                                                                              |
| Consumer Responsibility | The consumer shall reference this object solely by its immutable identifier and shall not infer semantics not published in this contract.     |
| Required Attributes     | Immutable identifier; mandatory attributes; attributes required by declared structural relationships. Domain-specific required attributes must be declared in the consuming Domain Semantic Model. |
| Authoritative Source    | SE-C-025 Money.                                                                                                                                |

**Lifecycle Specification Contract:** Not applicable.

**Information Model**

| Attribute     | Type              | Mandatory | Description            |
| ------------- | ----------------- | --------- | ---------------------- |
| Amount        | Decimal           | Yes       | Monetary value.        |
| Currency Code | String (ISO 4217) | Yes       | Currency denomination. |

**Relationships:** None.

**Invariants:** Currency Code must be valid; Amount may be zero, positive, or negative per owning object.

**Dependencies:** None (conceptual dependency on ISO 4217).

---

### SE‑C‑026 – Capacity

**Business Intent:** Define the enterprise concept of output potential over a governed time interval, providing a single authoritative meaning that can be used by any planning capability to express what can be achieved.

**Enterprise Meaning:** Capacity is the measure of how much output can be generated within a defined time period under normal conditions. It answers “how much can be done” independently of which resource does it. The measure itself is a quantity expressed in a unit of measure per unit of time. Capacity is independent of its owning enterprise object; it is the abstract enterprise fact that resources, networks, and plans will later quantify.

**Applied Semantic Patterns:** Value Object

**Semantic Ownership**

- **Capacity owns:** output quantity, unit of measure, time period.
- **Capacity excludes:** actual output, resource identity, constraints.

**Authority Specification Contract**

| Section                      | Value                                                                                                                       |
| ---------------------------- | --------------------------------------------------------------------------------------------------------------------------- |
| Semantic Authority           | Core Domain                                                                                                                 |
| Steward Domain               | Core                                                                                                                        |
| Mutation Authority           | Not Applicable                                                                                                              |
| Authoritative Representation | The enterprise definition of output potential per unit of time.                                                             |
| Authority Scope              | Enterprise‑wide.                                                                                                            |
| Intended Consumers           | Resource, Supply Plan, Production Schedule, Distribution Plan, Scenario, and any capability that evaluates or plans output. |
| Non‑Intended Consumers       | None.                                                                                                                       |
| Business Responsibility      | Preserve the authoritative enterprise meaning, identity, lifecycle integrity, and published contract of this semantic object. |
| Supersedes                   | None.                                                                                                                       |
| Superseded By                | None.                                                                                                                       |

**Consumer Specification Contract**

| Section                 | Value                                                                                                                                                    |
| ----------------------- | -------------------------------------------------------------------------------------------------------------------------------------------------------- |
| Business Guarantees     | Every Capacity value is expressed in an immutable combination of quantity, unit, and time period. The meaning of the value does not change with context. |
| Required Interpretation | Consumers shall interpret Capacity as a nominal potential, not as a commitment or a forecast. Actual performance may differ.                             |
| Known Limitations       | Does not account for variability, efficiency losses, or actual conditions. Those are domain‑specific adjustments.                                        |
| Version Expectations    | Not versioned; the semantic definition is stable. Specific capacity values are owned by the objects that hold them.                                      |
| Freshness Expectations  | Not applicable at the reference level. Freshness of specific capacity values is the responsibility of the owning domain.                                 |
| Intended Consumers      | Resource, Plan, Schedule.                                                                                                                                |
| Non‑Intended Consumers  | None.                                                                                                                                                    |
| Declared Consumers      | Governed by Chapter 5.2 Declared Consumer Matrix.                                                                                                        |
| Consumer Responsibility | The consumer shall reference this object solely by its immutable identifier and shall not infer semantics not published in this contract.     |
| Required Attributes     | Immutable identifier; mandatory attributes; attributes required by declared structural relationships. Domain-specific required attributes must be declared in the consuming Domain Semantic Model. |
| Authoritative Source    | SE-C-026 Capacity.                                                                                                                                       |

**Lifecycle Specification Contract:** Not applicable.

**Information Model**

| Attribute        | Type                    | Mandatory | Description                                                                 |
| ---------------- | ----------------------- | --------- | --------------------------------------------------------------------------- |
| Capacity Measure | Governed Identifier Reference (SE-C-037) | Yes       | What capability/output the value represents.                                |
| Output Quantity  | Quantity (SE-C-023)     | Yes       | Quantity of output, expressed with a governed unit of measure.              |
| Time Period      | Duration (SE-C-024)     | Yes       | Time interval over which the output potential is expressed.                 |

**Relationships**

| Relationship | Target Object          | Cardinality | Description                                             |
| ------------ | ---------------------- | ----------- | ------------------------------------------------------- |
| uses         | Quantity (SE-C-023)    | Many-to-One | Output quantity carries its unit through Quantity.      |

**Invariants:** Output Quantity value must be non-negative; Output Quantity unit must be valid and active; Time Period must be positive.

**Dependencies:** SE-C-023 Quantity, SE-C-024 Duration, SE-C-037 Enterprise Governed Vocabulary

---

### SE‑C‑027 – Planning Horizon

**Business Intent:** Provide the enterprise definition of the temporal extent within which a planning activity is intended to reason, expressed as a bounded interval in UTC.

**Enterprise Meaning:** A Planning Horizon defines the temporal extent within which a planning activity reasons about enterprise reality. It answers “how far into the future does this planning activity extend?” independently of the resolution used inside the horizon. The horizon is an immutable value once defined; it does not carry calendar semantics or bucket arrangements.

**Applied Semantic Patterns:** Value Object

**Semantic Ownership**

- **Planning Horizon owns:** start and end timestamps.
- **Planning Horizon excludes:** granularity, calendar alignment, actual plan data.

**Authority Specification Contract**

| Section                      | Value                                                                                                                                          |
| ---------------------------- | ---------------------------------------------------------------------------------------------------------------------------------------------- |
| Semantic Authority           | Core Domain                                                                                                                                    |
| Steward Domain               | Core                                                                                                                                           |
| Mutation Authority           | Not Applicable                                                                                                                                 |
| Authoritative Representation | The enterprise definition of a planning time window.                                                                                           |
| Authority Scope              | Enterprise‑wide                                                                                                                                |
| Intended Consumers           | Planning Scope, Plan, Supply Plan, Demand Plan, Scenario, and any capability that defines or operates within a time‑bounded planning activity. |
| Non‑Intended Consumers       | None                                                                                                                                           |
| Business Responsibility      | Preserve the authoritative enterprise meaning, identity, lifecycle integrity, and published contract of this semantic object. |
| Supersedes                   | None                                                                                                                                           |
| Superseded By                | None                                                                                                                                           |

**Consumer Specification Contract**

| Section                 | Value                                                                                                                                                   |
| ----------------------- | ------------------------------------------------------------------------------------------------------------------------------------------------------- |
| Business Guarantees     | Every Planning Horizon has an immutable start and end timestamp. The interval is a fixed span of UTC time.                                              |
| Required Interpretation | Consumers shall treat the horizon as the inclusive time boundary for planning. All planning facts within a planning activity fall inside this interval. |
| Known Limitations       | Does not define the granularity of time buckets within the horizon. Does not imply any calendar or working/non‑working periods.                         |
| Declared Consumers      | Governed by Chapter 5.2 Declared Consumer Matrix.                                                                                                       |
| Consumer Responsibility | The consumer shall reference this object solely by its immutable identifier and shall not infer semantics not published in this contract.     |
| Required Attributes     | Immutable identifier; mandatory attributes; attributes required by declared structural relationships. Domain-specific required attributes must be declared in the consuming Domain Semantic Model. |
| Authoritative Source    | SE-C-027 Planning Horizon.                                                                                                                              |

**Lifecycle Specification Contract:** Not applicable.

**Information Model**

| Attribute | Type                 | Mandatory | Description           |
| --------- | -------------------- | --------- | --------------------- |
| Start     | Timestamp (SE‑C‑022) | Yes       | Inclusive start, UTC. |
| End       | Timestamp (SE‑C‑022) | Yes       | Exclusive end, UTC.   |

**Relationships:** None.

**Invariants:** End strictly after Start; both valid UTC timestamps.

**Dependencies:** SE‑C‑022 Timestamp.

**Decomposition Review:** Not applicable.

---

### SE‑C‑028 – Temporal Window

**Business Intent:** Provide the authoritative enterprise definition of a neutral temporal interval during which an enterprise fact, availability, obligation, or condition is valid or in effect.

**Enterprise Meaning:** A Temporal Window is an enterprise‑recognised time interval defined by an earliest and latest bound. It answers “during what time period is this true?” independently of whether the context is availability, fulfilment, validity, or obligation. The window specifies when something begins to be valid and when it ceases to be valid. The concept is reusable across any enterprise object that requires a time‑bounded definition.

**Applied Semantic Patterns:** Value Object

**Semantic Ownership**

- **Temporal Window owns:** earliest and latest bounds.
- **Temporal Window excludes:** underlying fact, scheduling logic.

**Authority Specification Contract**

| Section                      | Value                                                                                      |
| ---------------------------- | ------------------------------------------------------------------------------------------ |
| Semantic Authority           | Core Domain                                                                                |
| Steward Domain               | Core                                                                                       |
| Mutation Authority           | Not Applicable                                                                             |
| Authoritative Representation | The enterprise definition of a time interval.                                              |
| Authority Scope              | Enterprise‑wide                                                                            |
| Intended Consumers           | Any concept requiring a time‑bounded definition (availability, validity, obligation, etc.) |
| Non‑Intended Consumers       | None                                                                                       |
| Business Responsibility      | Preserve the authoritative enterprise meaning, identity, lifecycle integrity, and published contract of this semantic object. |
| Supersedes                   | None                                                                                       |
| Superseded By                | None                                                                                       |

**Consumer Specification Contract**

| Section                 | Value                                                                                                                                                                           |
| ----------------------- | ------------------------------------------------------------------------------------------------------------------------------------------------------------------------------- |
| Business Guarantees     | The Temporal Window defines a valid interval with a non‑empty latest bound. Earliest is optional; if absent, the interval has no start constraint.                              |
| Required Interpretation | Consumers shall interpret the window as the inclusive period during which the associated fact is true or valid. Occurrence outside the window means the fact is not applicable. |
| Known Limitations       | Does not define the granularity or cost of deviation. Does not define a preferred point; that is a specialisation provided by Need Window.                                      |
| Declared Consumers      | Governed by Chapter 5.2 Declared Consumer Matrix.                                                                                                                               |
| Consumer Responsibility | The consumer shall reference this object solely by its immutable identifier and shall not infer semantics not published in this contract.                                     |
| Required Attributes     | Immutable identifier; mandatory attributes; attributes required by declared structural relationships. Domain-specific required attributes must be declared in the consuming Domain Semantic Model. |
| Authoritative Source    | SE-C-028 Temporal Window.                                                                                                                                                       |

**Lifecycle Specification Contract:** Not applicable.

**Information Model**

| Attribute | Type                 | Mandatory | Description        |
| --------- | -------------------- | --------- | ------------------ |
| Earliest  | Timestamp (SE‑C‑022) | No        | Start of interval. |
| Latest    | Timestamp (SE‑C‑022) | Yes       | End of interval.   |

**Relationships:** None.

**Invariants:** Latest valid UTC; if Earliest present, Latest strictly after Earliest.

**Dependencies:** SE‑C‑022 Timestamp.

---

### SE‑C‑029 – Need Window

**Business Intent:** Provide the authoritative enterprise definition of a temporal interval within which an enterprise obligation, activity, or need is acceptably constrained, including an optional preferred point.

**Enterprise Meaning:** A Need Window is a specialisation of Temporal Window that adds an optional preferred time point. It answers “within what time boundaries must this need or obligation be satisfied, and when would be ideal?” The window defines the earliest acceptable time, the latest acceptable time, and optionally a preferred time. The concept is reusable where fulfilment optimisation or preference matters, particularly for demands and commitments.

**Applied Semantic Patterns:** Value Object (specialisation of Temporal Window)

**Semantic Ownership**

- **Need Window owns:** earliest, latest, preferred times.
- **Need Window excludes:** underlying need, scheduling logic.

**Authority Specification Contract**

| Section                      | Value                                                                          |
| ---------------------------- | ------------------------------------------------------------------------------ |
| Semantic Authority           | Core Domain                                                                    |
| Steward Domain               | Core                                                                           |
| Mutation Authority           | Not Applicable                                                                 |
| Authoritative Representation | The enterprise definition of a fulfilment‑preference time interval.            |
| Authority Scope              | Enterprise‑wide                                                                |
| Intended Consumers           | Demand, Commitment, and any concept that involves a preferred fulfilment time. |
| Non‑Intended Consumers       | None                                                                           |
| Business Responsibility      | Preserve the authoritative enterprise meaning, identity, lifecycle integrity, and published contract of this semantic object. |
| Supersedes                   | None                                                                           |
| Superseded By                | None                                                                           |

**Consumer Specification Contract**

| Section                 | Value                                                                                                                                                                              |
| ----------------------- | ---------------------------------------------------------------------------------------------------------------------------------------------------------------------------------- |
| Business Guarantees     | The Need Window defines a valid interval with a non‑empty latest acceptable time. Earliest is optional. Preferred is optional.                                                     |
| Required Interpretation | Consumers shall interpret the window as the acceptable range for the associated need or obligation. The Preferred time, if present, indicates the ideal point within the interval. |
| Known Limitations       | Does not define the cost of deviation from the preferred time.                                                                                                                     |
| Declared Consumers      | Governed by Chapter 5.2 Declared Consumer Matrix.                                                                                                                                  |
| Consumer Responsibility | The consumer shall reference this object solely by its immutable identifier and shall not infer semantics not published in this contract.                                       |
| Required Attributes     | Immutable identifier; mandatory attributes; attributes required by declared structural relationships. Domain-specific required attributes must be declared in the consuming Domain Semantic Model. |
| Authoritative Source    | SE-C-029 Need Window.                                                                                                                                                              |

**Lifecycle Specification Contract:** Not applicable.

**Information Model**

| Attribute           | Type                 | Mandatory | Description               |
| ------------------- | -------------------- | --------- | ------------------------- |
| Earliest Acceptable | Timestamp (SE‑C‑022) | No        | Earliest acceptable time. |
| Preferred           | Timestamp (SE‑C‑022) | No        | Ideal time.               |
| Latest Acceptable   | Timestamp (SE‑C‑022) | Yes       | Latest acceptable time.   |

**Relationships:** None. Inherits semantics from Temporal Window.

**Invariants:**
- Latest Acceptable must be a valid UTC timestamp.
- If Earliest Acceptable is present, Latest Acceptable must be strictly after Earliest Acceptable.
- If Preferred is present, it must fall within [Earliest Acceptable, Latest Acceptable] if Earliest Acceptable is present, or before Latest Acceptable otherwise.

**Dependencies:** SE‑C‑022 Timestamp.

---

### SE‑C‑030 – Risk Assessment

**Business Intent:** Provide the authoritative enterprise definition of the evaluated likelihood and business impact of a recognised risk at a specific point in time.

**Enterprise Meaning:** A Risk Assessment is the enterprise’s evaluation of a specific risk at a point in time. It answers “how likely is this risk to occur, and what would be the impact if it does?” The assessment is derived from enterprise evidence that exists elsewhere; the assessment itself does not own that evidence. The assessment is immutable once made; a new assessment is created when the enterprise re‑evaluates the risk.

**Applied Semantic Patterns:** Value Object

**Semantic Ownership**

- **Risk Assessment owns:** likelihood, impact, timestamp, rationale.
- **Risk Assessment excludes:** identity of the risk, underlying evidence.

**Authority Specification Contract**

| Section                      | Value                                                     |
| ---------------------------- | --------------------------------------------------------- |
| Semantic Authority           | Core Domain                                               |
| Steward Domain               | Core                                                      |
| Mutation Authority           | Not Applicable                                            |
| Authoritative Representation | The enterprise’s evaluation of a risk at a point in time. |
| Authority Scope              | Enterprise‑wide                                           |
| Intended Consumers           | Any capability that records or consumes risk evaluations. |
| Non‑Intended Consumers       | None                                                      |
| Business Responsibility      | Preserve the authoritative enterprise meaning, identity, lifecycle integrity, and published contract of this semantic object. |
| Supersedes                   | None                                                      |
| Superseded By                | None                                                      |

**Consumer Specification Contract**

| Section                 | Value                                                                                                                                                                                                                    |
| ----------------------- | ------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------ |
| Business Guarantees     | Each Risk Assessment captures the likelihood and impact using governed classifications. The assessment is immutable.                                                                                                     |
| Required Interpretation | Consumers shall interpret the Risk Assessment as the enterprise’s view of the risk at the time it was made. The assessment is based on enterprise evidence; that evidence is not contained within the assessment itself. |
| Known Limitations       | Does not contain the underlying evidence; only the resulting evaluation and rationale are recorded.                                                                                                                      |
| Declared Consumers      | Governed by Chapter 5.2 Declared Consumer Matrix.                                                                                                                                                                        |
| Consumer Responsibility | The consumer shall reference this object solely by its immutable identifier and shall not infer semantics not published in this contract.                                                                               |
| Required Attributes     | Immutable identifier; mandatory attributes; attributes required by declared structural relationships. Domain-specific required attributes must be declared in the consuming Domain Semantic Model.                     |
| Authoritative Source    | SE-C-030 Risk Assessment.                                                                                                                                                                                                |

**Lifecycle Specification Contract:** Not applicable.

**Information Model**

| Attribute            | Type                | Mandatory | Description                                                                                                                     |
| -------------------- | ------------------- | --------- | ------------------------------------------------------------------------------------------------------------------------------- |
| Likelihood           | Governed Identifier Reference (SE-C-037) | Yes       | The assessed probability of occurrence (e.g., “Low”, “Medium”, “High”).                                                         |
| Impact               | Governed Identifier Reference (SE-C-037) | Yes       | The assessed business impact if the risk materialises (e.g., “Negligible”, “Moderate”, “Severe”).                               |
| Assessment Timestamp | Timestamp           | Yes       | When this assessment was made.                                                                                                  |
| Rationale            | String              | No        | A description of the reasoning and evidence basis for the assessment. The evidence itself is owned by other enterprise objects. |

**Relationships:** None. Owned by a Risk.

**Invariants:** Likelihood and Impact must be recognised values; Assessment Timestamp valid.

**Dependencies:** SE‑C‑022 Timestamp, SE-C-037 Enterprise Governed Vocabulary

---

### SE-C-038 — Scope Boundary Rule

**Business Intent:** Provide a deterministic inclusion or exclusion rule for a Planning Scope.

**Enterprise Meaning:** A Scope Boundary Rule is a structured rule that determines whether a class or instance of enterprise objects participates in a Planning Scope. It is machine-readable and governed. It is not free text.

**Applied Semantic Patterns:** Value Object

**Mutation Authority:** Not Applicable

**Semantic Ownership**

- **Scope Boundary Rule owns:**
  - Rule identity.
  - Target semantic type.
  - Inclusion indicator.
  - Target instances.
  - Target categories.

- **Scope Boundary Rule excludes:**
  - Planning results.
  - Scenario assumptions.
  - Algorithm logic.

**Information Model**

| Attribute | Type | Mandatory | Description |
| --- | --- | --- | --- |
| Rule Identifier | ID (immutable) | Yes | Immutable identity of the rule within the Planning Scope. |
| Target Semantic Type | Governed Identifier Reference (SE-C-037) | Yes | The enterprise concept being filtered, such as Item, Location, Customer, Supplier. |
| Inclusion Indicator | Boolean | Yes | True means include. False means exclude. |
| Target Instance Identifiers | List of External Identifiers | No | Specific instances included or excluded. |
| Target Category Identifiers | List of Governed Identifier References (SE-C-037) | No | Governed categories included or excluded. |

**Invariants**

- Rule Identifier is immutable.
- Target Semantic Type must be recognized.
- At least one Target Instance Identifier or Target Category Identifier must be present.
- Scope Boundary Rules are immutable once the owning Planning Scope is Active.

**Dependencies**

| Dependency Type | Description |
| --- | --- |
| Semantic Dependency | SE-C-037 Enterprise Governed Vocabulary. |
| Conceptual Dependency | Planning Scope boundary governance. |
| SE-C-037 |  Enterprise Governed Vocabulary |

---

### SE-C-039 — Scenario Adjustment

**Business Intent:** Provide a deterministic adjustment applied under a Scenario for planning evaluation.

**Enterprise Meaning:** A Scenario Adjustment is a structured enterprise instruction that modifies how planning interprets enterprise facts under a Scenario. It is machine-readable and governed. It does not alter enterprise truth; it alters planning assumptions.

**Applied Semantic Patterns:** Value Object

**Mutation Authority:** Not Applicable

**Semantic Ownership**

- **Scenario Adjustment owns:**
  - Adjustment identity.
  - Target semantic type.
  - Target instances or categories.
  - Adjustment type.
  - Adjustment magnitude or textual governed value.

- **Scenario Adjustment excludes:**
  - Enterprise truth.
  - Planning results.
  - Algorithm implementation.

**Information Model**

| Attribute | Type | Mandatory | Description |
| --- | --- | --- | --- |
| Adjustment Identifier | ID (immutable) | Yes | Immutable identity of the adjustment within the Scenario. |
| Target Semantic Type | Governed Identifier Reference (SE-C-037) | Yes | The enterprise concept affected by the adjustment. |
| Target Instance Identifiers | List of External Identifiers | No | Specific instances affected. |
| Target Category Identifiers | List of Governed Identifier References (SE-C-037) | No | Governed categories affected. |
| Adjustment Type | Governed Identifier Reference (SE-C-037) | Yes | The governed type of adjustment. |
| Adjustment Quantity | Quantity (SE-C-023) | No | Numeric magnitude where the adjustment is quantitative. |
| Adjustment Text | String | No | Governed non-numeric adjustment description where quantity is not applicable. |

**Invariants**

- Adjustment Identifier is immutable.
- At least one Target Instance Identifier or Target Category Identifier must be present.
- At least one of Adjustment Quantity or Adjustment Text must be present.
- Scenario Adjustments are immutable once the owning Scenario is Active.
- Scenario Adjustments do not mutate enterprise facts.

**Dependencies**

| Dependency Type | Description |
| --- | --- |
| Semantic Dependency | SE-C-037 Enterprise Governed Vocabulary, SE-C-023 Quantity. |
| Conceptual Dependency | Scenario assumption governance. |
| SE-C-037 |  Enterprise Governed Vocabulary |

---

## 4.4 Reference Objects

No Reference Objects are defined in the Enterprise Semantic Model.

Objects previously classified as Reference Objects have been reclassified as Aggregate Roots where the enterprise owns their governance, identity, and lifecycle.

External references are declared as typed external identifiers in object contracts.


## 4.5 Knowledge Artifacts

No standalone Knowledge Artifact aggregates are defined at the Enterprise Semantic Model level. Knowledge Artifacts are domain-specific, governed concepts that carry confidence, evidence trails, and expiry. They are defined in the respective Domain Semantic Models (e.g., Supply Intelligence, Demand Intelligence). The Enterprise Semantic Model provides only the stable enterprise vocabulary that Knowledge Artifacts may reference, including enterprise-level Performance Indicator definitions but not their published values.

---

# Chapter 5 – Enterprise Relationships

This chapter consolidates the structural relationships declared in each object’s Relationships section that resolve directly to ratified Enterprise Semantic Objects. Typed external references declared in source contracts are intentionally not duplicated here.

## 5.1 Structural Relationships

| Source Object                | Relationship                          | Target Object                | Cardinality            |
| ---------------------------- | ------------------------------------- | ---------------------------- | ---------------------- |
| SE-C-001 Item                | quantified in                         | SE-C-032 Unit of Measure     | Many-to-One            |
| SE-C-001 Item                | classified by                         | SE-C-037 Enterprise Governed Vocabulary | Many-to-Many |
| SE-C-002 Location            | uses                                  | SE-C-031 Time Zone           | Many-to-One            |
| SE-C-002 Location            | parent Location                       | SE-C-002 Location            | Zero-or-One            |
| SE-C-005 Resource Group      | contains                              | SE-C-007 Physical Resource   | One-to-Many            |
| SE-C-005 Resource Group      | governed by                           | SE-C-033 Calendar            | Many-to-One            |
| SE-C-006 Standard Resource   | realized by                           | SE-C-007 Physical Resource   | One-to-Many            |
| SE-C-006 Standard Resource   | governed by                           | SE-C-033 Calendar            | Many-to-One            |
| SE-C-007 Physical Resource   | belongs to                            | SE-C-005 Resource Group      | Many-to-One            |
| SE-C-007 Physical Resource   | based on                              | SE-C-006 Standard Resource   | Many-to-One            |
| SE-C-007 Physical Resource   | located at                            | SE-C-002 Location            | Many-to-One            |
| SE-C-007 Physical Resource   | governed by                           | SE-C-033 Calendar            | Many-to-One            |
| SE-C-008 Transportation Lane | originates at                         | SE-C-002 Location            | Many-to-One            |
| SE-C-008 Transportation Lane | terminates at                         | SE-C-002 Location            | Many-to-One            |
| SE-C-008 Transportation Lane | participates in                       | SE-C-009 Network             | Many-to-Many           |
| SE-C-009 Network             | has participating                     | SE-C-002 Location            | Many-to-Many           |
| SE-C-009 Network             | has participating                     | SE-C-008 Transportation Lane | Many-to-Many           |
| SE-C-010 Planning Scope      | contains                              | SE-C-038 Scope Boundary Rule | One-to-Many            |
| SE-C-011 Scenario            | applies to                            | SE-C-010 Planning Scope      | Many-to-One            |
| SE-C-011 Scenario            | contains                              | SE-C-039 Scenario Adjustment | One-to-Many            |
| SE-C-012 Plan                | references                            | SE-C-010 Planning Scope      | Many-to-One            |
| SE-C-012 Plan                | references                            | SE-C-027 Planning Horizon    | Many-to-One            |
| SE-C-012 Plan                | references                            | SE-C-011 Scenario            | Many-to-One            |
| SE-C-013 Demand              | is need for                           | SE-C-001 Item                | Many-to-One            |
| SE-C-013 Demand              | is needed at                          | SE-C-002 Location            | Many-to-One            |
| SE-C-013 Demand              | is needed for                         | SE-C-003 Customer            | Many-to-One (optional) |
| SE-C-013 Demand              | is derived from                       | SE-C-013 Demand              | Many-to-One (optional) |
| SE-C-013 Demand              | has need window                       | SE-C-029 Need Window         | Many-to-One            |
| SE-C-014 Supply              | is supply of                          | SE-C-001 Item                | Many-to-One            |
| SE-C-014 Supply              | is available at                       | SE-C-002 Location            | Many-to-One            |
| SE-C-014 Supply              | has availability window               | SE-C-028 Temporal Window     | Many-to-One            |
| SE-C-015 Inventory           | is stock of                           | SE-C-001 Item                | Many-to-One            |
| SE-C-015 Inventory           | is held at                            | SE-C-002 Location            | Many-to-One            |
| SE-C-017 Commitment          | involves item                         | SE-C-001 Item                | Many-to-One            |
| SE-C-017 Commitment          | involves location                     | SE-C-002 Location            | Many-to-One            |
| SE-C-017 Commitment          | involves customer                     | SE-C-003 Customer            | Many-to-One (optional) |
| SE-C-017 Commitment          | involves supplier                     | SE-C-004 Supplier            | Many-to-One (optional) |
| SE-C-018 Bill of Materials   | defines composition for               | SE-C-001 Item                | Many-to-One            |
| SE-C-018 Bill of Materials   | references component                  | SE-C-001 Item                | One-to-Many            |
| SE-C-020 Risk                | contains                              | SE-C-030 Risk Assessment     | One-to-Many            |
| SE-C-021 Enterprise Picture  | snapshots reality for                 | SE-C-010 Planning Scope      | Many-to-One            |
| SE-C-021 Enterprise Picture  | snapshot version references demand    | SE-C-013 Demand              | Many-to-Many           |
| SE-C-021 Enterprise Picture  | snapshot version references supply    | SE-C-014 Supply              | Many-to-Many           |
| SE-C-021 Enterprise Picture  | snapshot version references inventory | SE-C-015 Inventory           | Many-to-Many           |
| SE-C-035 Performance Indicator Catalog | contains | SE-C-036 Performance Indicator | One-to-Many |
| SE-C-037 Enterprise Governed Vocabulary | contains | Vocabulary Entry             | One-to-Many            |
| SE-C-038 Scope Boundary Rule | uses                                  | SE-C-037 Enterprise Governed Vocabulary | Many-to-One |
| SE-C-039 Scenario Adjustment | uses                                  | SE-C-037 Enterprise Governed Vocabulary | Many-to-One |
| SE-C-039 Scenario Adjustment | uses                                  | SE-C-023 Quantity            | Many-to-One (optional) |
| SE-C-023 Quantity            | uses                                  | SE-C-032 Unit of Measure     | Many-to-One            |
| SE-C-024 Duration            | uses                                  | SE-C-032 Unit of Measure     | Many-to-One            |
| SE-C-026 Capacity            | uses                                  | SE-C-032 Unit of Measure     | Many-to-One            |
| SE-C-026 Capacity | uses | SE-C-023 Quantity | Many-to-One |
| SE-C-026 Capacity | uses | SE-C-024 Duration | Many-to-One |
| SE-C-026 Capacity | uses | SE-C-037 Enterprise Governed Vocabulary | Many-to-One |
| SE-C-033 Calendar            | uses                                  | SE-C-031 Time Zone           | Many-to-One            |

## 5.2 Declared Consumer Matrix

Every object declared as a consumer of another object is listed here. This matrix is derived from the Consumer Specification Contracts and the downward traceability declared in each object.

| Consumed Object                        | Consuming Objects |
| -------------------------------------- | ---------------------------------------------------------------------------------------------------------------------------------------------------------------------- |
| SE-C-001 Item                          | SE-C-013 Demand, SE-C-014 Supply, SE-C-015 Inventory, SE-C-017 Commitment, SE-C-018 BOM, SE-C-021 Enterprise Picture, SE-C-038 Scope Boundary Rule, SE-C-039 Scenario Adjustment |
| SE-C-002 Location                      | SE-C-007 Physical Resource, SE-C-008 Transportation Lane, SE-C-009 Network, SE-C-013 Demand, SE-C-014 Supply, SE-C-015 Inventory, SE-C-017 Commitment, SE-C-021 Enterprise Picture, SE-C-038 Scope Boundary Rule, SE-C-039 Scenario Adjustment |
| SE-C-003 Customer                      | SE-C-013 Demand, SE-C-017 Commitment, SE-C-038 Scope Boundary Rule, SE-C-039 Scenario Adjustment |
| SE-C-004 Supplier                      | SE-C-017 Commitment, SE-C-038 Scope Boundary Rule, SE-C-039 Scenario Adjustment |
| SE-C-005 Resource Group                | SE-C-007 Physical Resource |
| SE-C-006 Standard Resource             | SE-C-007 Physical Resource |
| SE-C-007 Physical Resource             | No declared enterprise consumers; domain capabilities consume per domain models. |
| SE-C-008 Transportation Lane           | SE-C-009 Network |
| SE-C-009 Network                       | No declared enterprise consumers; domain capabilities consume per domain models. |
| SE-C-010 Planning Scope                | SE-C-011 Scenario, SE-C-012 Plan, SE-C-021 Enterprise Picture |
| SE-C-011 Scenario                      | SE-C-012 Plan |
| SE-C-012 Plan                          | No declared enterprise consumers; domain capabilities consume per domain models. |
| SE-C-013 Demand                        | SE-C-021 Enterprise Picture |
| SE-C-014 Supply                        | SE-C-021 Enterprise Picture |
| SE-C-015 Inventory                     | SE-C-021 Enterprise Picture |
| SE-C-017 Commitment                    | No declared enterprise consumers; domain capabilities consume per domain models. |
| SE-C-018 Bill of Materials             | No declared enterprise consumers; domain capabilities consume per domain models. |
| SE-C-019 Exception                     | No declared enterprise consumers; domain capabilities consume per domain models. |
| SE-C-020 Risk                          | No declared enterprise consumers; domain capabilities consume per domain models. |
| SE-C-021 Enterprise Picture            | No declared enterprise consumers; domain capabilities consume per domain models. |
| SE-C-022 Timestamp                     | SE-C-015, SE-C-017, SE-C-018, SE-C-021, SE-C-027, SE-C-028, SE-C-029, SE-C-030 |
| SE-C-023 Quantity                      | SE-C-013, SE-C-014, SE-C-015, SE-C-017, SE-C-018, SE-C-026, SE-C-039  |
| SE-C-024 Duration                      | SE-C-026         |
| SE-C-025 Money                         | No declared enterprise consumers; domain capabilities consume per domain models. |
| SE-C-026 Capacity                      | SE-C-006, SE-C-007  |
| SE-C-027 Planning Horizon              | SE-C-012 Plan    |
| SE-C-028 Temporal Window               | SE-C-014 Supply  |
| SE-C-029 Need Window                   | SE-C-013 Demand  |
| SE-C-030 Risk Assessment               | SE-C-020 Risk |
| SE-C-031 Time Zone                     | SE-C-002 Location, SE-C-033 Calendar |
| SE-C-032 Unit of Measure               | SE-C-001 Item, SE-C-023 Quantity, SE-C-024 Duration, SE-C-026 Capacity |
| SE-C-033 Calendar                      | SE-C-005 Resource Group, SE-C-006 Standard Resource, SE-C-007 Physical Resource |
| SE-C-034 Planning Period | No declared enterprise consumers; domain capabilities consume per domain models. |
| SE-C-035 PI Catalog                    | No declared enterprise consumers; domain capabilities consume per domain models. |
| SE-C-036 Performance Indicator         | SE-C-035 Performance Indicator Catalog |
| SE-C-037 Enterprise Governed Vocabulary| SE-C-001, SE-C-005, SE-C-006, SE-C-014, SE-C-019, SE-C-020, SE-C-026, SE-C-030, SE-C-036, SE-C-038, SE-C-039 |
| SE-C-038 Scope Boundary Rule           | SE-C-010 Planning Scope |
| SE-C-039 Scenario Adjustment           | SE-C-011 Scenario |

## 5.3 Relationship Governance

All relationships are owned by the source object. When a source object is retired or archived, its relationships are retained as historical facts. Cardinalities are enforced at the aggregate boundary; cross-aggregate invariants are governed by domain policies, not by the Semantic Model.

---

# Chapter 6 – Enterprise Dependency Model

## 6.1 Semantic Dependency Graph

The dependency graph below captures every direct semantic dependency among Enterprise Semantic Objects. It is derived from the Dependencies sections of each object contract.

- SE‑C‑001 Item → SE‑C‑032 Unit of Measure, (optional) SE-C-037 Enterprise Governed Vocabulary
- SE‑C‑002 Location → SE‑C‑031 Time Zone
- SE‑C‑005 Resource Group → SE‑C‑033 Calendar, (optional) SE-C-037 Enterprise Governed Vocabulary
- SE‑C‑006 Standard Resource → SE‑C‑033 Calendar, SE‑C‑026 Capacity, (optional) SE-C-037 Enterprise Governed Vocabulary
- SE‑C‑007 Physical Resource → SE‑C‑002 Location, SE‑C‑033 Calendar, SE‑C‑026 Capacity, (optional) SE‑C‑005, SE‑C‑006
- SE‑C‑008 Transportation Lane → SE‑C‑002 Location
- SE‑C‑009 Network → SE‑C‑002 Location, SE‑C‑008 Transportation Lane
- SE-C-010 Planning Scope → SE-C-038 Scope Boundary Rule, SE-C-037 Enterprise Governed Vocabulary
- SE‑C‑011 Scenario → SE‑C‑010 Planning Scope, SE-C-039 Scenario Adjustment
- SE‑C‑012 Plan → SE‑C‑010 Planning Scope, SE‑C‑027 Planning Horizon, SE‑C‑011 Scenario
- SE‑C‑013 Demand → SE‑C‑001 Item, SE‑C‑002 Location, (optional) SE‑C‑003 Customer, SE‑C‑023 Quantity, SE‑C‑029 Need Window
- SE‑C‑014 Supply → SE‑C‑001 Item, SE‑C‑002 Location, SE‑C‑023 Quantity, SE‑C‑028 Temporal Window, SE-C-037 Enterprise Governed Vocabulary
- SE‑C‑015 Inventory → SE‑C‑001 Item, SE‑C‑002 Location, SE‑C‑023 Quantity, SE‑C‑022 Timestamp
- SE‑C‑017 Commitment → SE‑C‑001 Item, SE‑C‑002 Location, (optional) SE‑C‑003 Customer, (optional) SE‑C‑004 Supplier, SE‑C‑023 Quantity, SE‑C‑022 Timestamp
- SE‑C‑018 Bill of Materials → SE‑C‑001 Item, SE‑C‑023 Quantity, SE‑C‑022 Timestamp
- SE‑C‑019 Exception → SE-C-037 Enterprise Governed Vocabulary
- SE‑C‑020 Risk → SE‑C‑030 Risk Assessment, SE-C-037 Enterprise Governed Vocabulary
- SE‑C‑021 Enterprise Picture → SE‑C‑010 Planning Scope, SE‑C‑013 Demand, SE‑C‑014 Supply, SE‑C‑015 Inventory
- SE-C-023 Quantity → SE-C-032 Unit of Measure
- SE-C-024 Duration → SE-C-032 Unit of Measure
- SE-C-026 Capacity → SE-C-024 Duration, SE-C-032 Unit of Measure, SE-C-037 Enterprise Governed Vocabulary
- SE-C-030 Risk Assessment → SE-C-022 Timestamp, SE-C-037 Enterprise Governed Vocabulary
- SE-C-033 Calendar → SE-C-031 Time Zone
- SE-C-036 Performance Indicator → SE-C-037 Enterprise Governed Vocabulary
- SE-C-037 Enterprise Governed Vocabulary → Vocabulary Entry
- SE-C-038 Scope Boundary Rule → SE-C-037 Enterprise Governed Vocabulary
- SE-C-039 Scenario Adjustment → SE-C-037 Enterprise Governed Vocabulary, (optional) SE-C-023 Quantity

## 6.2 Consumer Matrix

The Consumer Matrix is maintained in Chapter 5, Section 5.2. It lists every declared consumer of each Enterprise Semantic Object.

## 6.3 Capability / Decision / Algorithm / Policy Dependencies

This section will be populated when the Domain Specifications are aligned with the Enterprise Semantic Model. At that point, each Capability, Decision, Business Algorithm, and Policy will declare its consumed Enterprise Semantic Objects, completing the traceability chain from implementation to enterprise meaning.

---

# Chapter 7 – Enterprise Governance

## 7.1 Ownership Governance

Unless a more specific provisional authority is explicitly recorded in an object contract, Enterprise Semantic Objects are assigned Semantic Authority to the Core Domain until a formal governance model is established and ratified by the APS Architecture Governance Board. Mutation Authority is explicitly recorded per object contract using the archetypes defined in Section 2.6. Concrete capability assignment belongs to the Capability Model.

## 7.2 Extension Rules

Domain Semantic Models may extend Enterprise Semantic Objects by adding domain‑specific attributes and relationships. Extensions shall never:

- Redefine enterprise meaning.
- Redefine enterprise identity.
- Redefine enterprise‑level invariants.
- Redefine Semantic Authority.
- Remove, rename, or alter the type of any enterprise attribute.
- Alter enterprise lifecycle.
- Alter enterprise relationships.

Any violation of these rules is an architectural defect and must be corrected before the domain specification can be frozen.

## 7.3 Versioning Rules

Enterprise Semantic Objects that carry versioned state (Bill of Materials, Enterprise Picture, Calendar, and Performance Indicator Catalog, including its contained Performance Indicator definitions) use monotonic version numbers scoped to their aggregate identity. Non‑versioned objects reflect their current state; historical state changes are preserved for audit by the Steward Domain.

## 7.4 Change Management

Changes to any Enterprise Semantic Object follow the Architecture Review Guidelines. A change that alters enterprise meaning, identity, or invariants requires re‑verification against the Semantic Completeness Standard and the Consumer Completeness check before ratification.

## 7.5 Semantic Quality Standards

The Enterprise Semantic Completion Standard (Chapter 10) defines the target quality bar for this model. This draft does not yet assert that every object has passed Consumer Verification (Phase 7), and any unresolved placeholders or provisional ownership assignments must be eliminated before the document can be promoted from Governance Review Draft to Authoritative status.

## 7.6 Cross-Aggregate Lifecycle Enforcement Standard

Cross-aggregate lifecycle dependencies are enforced using a hybrid governance model.

1. Eligibility Enforcement  
   New references to Enterprise Semantic Objects shall target only objects in states that permit new references. Retired, Closed, or terminal objects shall not be referenced by new enterprise transactions unless historical reference is explicitly governed.

2. Exception Detection  
   Existing references that become invalid because of lifecycle changes shall be detected and represented as Exceptions.

3. No Automatic Cascade  
   Lifecycle changes shall not automatically cascade to dependent aggregates unless an explicitly governed domain behavior exists.

4. Explainability  
   Every detected cross-aggregate lifecycle violation shall identify the affected objects, the violated lifecycle rule, and the detecting capability.

---

# Chapter 8 – Traceability

Every Enterprise Semantic Object traces directly to:

- **Constitution:** CN‑003 (Single Source of Truth), CN‑004 (Single Semantic Ownership).
- **Architecture Reference Standard:** §3 (Semantic Model Architecture), §4 (Business Intent), §16 (Ownership).

The canonical traceability path for any Enterprise Semantic Object is:

```
Constitution (CN‑003, CN‑004)
        ↓
ARS (§3, §4, §16)
        ↓
Enterprise Semantic Model (this document)
        ↓
Capability Model
        ↓
Domain Specifications
        ↓
Implementation
```

Per‑object traceability is documented in the Traceability section of each object contract and is not duplicated here.

---

# Chapter 9 – Cross‑Referencing Rules

The following rules govern all cross‑references within the Enterprise Semantic Model and from Domain Specifications:

1. Every enterprise concept has exactly one authoritative definition. No concept is defined in more than one place.
2. All cross‑references to ratified semantic and architectural artifacts use architectural identifiers only (SE‑C‑xxx, CA‑xxx, DE‑xxx, BR‑xxx, PO‑xxx, FS‑xxx, BA‑xxx). External references shall be explicitly typed as external identifiers or enterprise references and shall not be treated as semantic object definitions.
3. Semantic Objects define lifecycles; later chapters and domain specifications reference lifecycle states by name but never redefine them.
4. Rules, Decisions, Policies, Functional Specifications, and Business Algorithms consume Semantic Objects; they do not redefine them.
5. Functional Specifications orchestrate Aggregate Behaviors rather than directly executing Decisions, Rules, or Algorithms.
6. Every dependency declared in a Domain Specification must resolve to an authoritative artifact in the Enterprise Semantic Model or a ratified Domain Semantic Model before implementation begins.

---

# Chapter 10 – Enterprise Semantic Completion Standard

An Enterprise Semantic Object is complete when all of the following are true:

1. Enterprise meaning is uniquely defined and documented in the Business Intent and Enterprise Meaning sections.
2. Semantic Authority is identified (Core Domain for enterprise‑wide objects; Steward Domain is Core).
3. Mutation Authority is explicitly stated using one of the archetypes defined in Section 2.6.
4. The Authority Specification Contract is complete.
5. The Consumer Specification Contract is complete, with every declared consumer identified.
6. The Lifecycle Specification Contract is complete (for stateful objects) or explicitly declared not applicable.
7. The Information Model is complete, with every attribute defined by type, mandatory/optional, and business description.
8. All relationships to other Semantic Objects are explicitly declared.
9. All semantic dependencies are traced to their authoritative source.
10. Traceability to the Constitution and ARS is recorded.
11. Every declared consumer can fulfil its responsibilities using only the published semantic contract, without inventing enterprise meaning (Consumer Verification, Phase 7).
12. No TODOs, placeholders, provisional semantics, or unresolved ownership remain in any section of the contract.

---

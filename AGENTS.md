# Medhavi APS Implementation Governance

This repository follows the active implementation governance defined in [Documents/6_Implementation_Agent_Governance.md](Documents/6_Implementation_Agent_Governance.md) and [Documents/Implementation Rules.md](Documents/Implementation%20Rules.md).

Before changing any specification, contract, code, test, workflow, or integration artifact:

1. Build the required evidence register and identify the applicable `CN`, ARS, ESM, domain-specification, and implementation artifacts.
2. Complete the applicable Gate A review before design and Gate B review before implementation.
3. Obtain explicit human approval after Gate B and before changing implementation artifacts.
4. Complete the applicable Gates C, D, and E before stating that work is ready for human governance review.

Non-negotiable constraints:

- Do not invent requirements, policies, values, lifecycle transitions, identities, consumers, or external contracts.
- Do not use placeholders, TODOs, stubs, simplified fallback behavior, or ungoverned error outcomes.
- Keep domain code functional: immutable values, discriminated unions, pure rules/decisions/algorithms/behaviors, and explicit dependencies.
- Do not introduce inheritance, abstract base classes, mutable domain state, getter/setter state models, singletons, or mutable object factories.
- Pure module-level F# composition functions such as `Context.create`, `Capabilities.create`, `CommandPipeline.create`, and value-object `create` functions are allowed when their dependencies are explicit and they contain no hidden mutable state.
- Keep `Medhavi.Contracts` strictly at the ACL and integration boundary (`ACL.fs`, `Projections.fs`, `Capabilities.fs`, `Workflows.fs`, and `Context.fs`). It must never be opened or referenced in pure domain files (`Model.fs`, `Policies.fs`, `Algorithms.fs`, `Rules.fs`, `Decisions.fs`, or `Behaviors.fs`).
- Never auto-open (`[<AutoOpen>]`) modules containing `Medhavi.Contracts` or DTO mapping logic in domain projects.
- Never directly import or use one aggregate root's internal types or state into another aggregate. As per DDD, aggregate boundaries are strictly isolated:
  - Cross-aggregate references must be by Identifier only (e.g. `ItemId`, `PlanningScopeId`, `ProcurementRequirementId`), or via shared immutable Value Objects in `Medhavi.SemanticModel` or domain-level `Types.fs`.
  - When an aggregate requires information from other aggregates for assessment, planning, or decision-making, that information must be provided via explicit Fact types and injected Ports/providers (`FactProvider`), never by reading another aggregate's internal entity state directly.
- Use `ArsIdentifiers` for architectural identifiers represented in code and preserve end-to-end provenance.
- Aggregate Behaviors emit Enterprise Events only. Business Notifications are emitted only by governed Business Workflow Notification Nodes after their referenced events exist.
- Stop and request authoritative clarification if an applicable semantic object, rule, policy, decision, event, notification, consumer, or provenance requirement is missing or conflicts with a higher authority.
namespace Medhavi.Client.SemanticModel

// ============================================================================
// R1.3 — Semantic component binding (validated construction boundary).
// Pure and registry-free. ID resolution happens in the application
// composition layer; this module evaluates semantic compatibility and
// constructs validated bindings only.
// ============================================================================

/// TRANSITIONAL governed capability vocabulary.
/// STATUS: transitional while I1–I5 remain frozen with string capabilities.
/// REPLACEMENT TARGET: typed SchemaCapability DU (future semantic refinement).
/// GOVERNANCE: adding a capability is a change-control entry, never a data edit.
module SchemaCapabilityVocabulary =
    let known: Set<string> = Set.ofList [ "Tabular"; "TimeSeries"; "Kpi"; "Detail" ]

type CompatibilityError =
    /// A required capability is not understood by the semantic model.
    | UnknownRequiredCapability of capability: string
    /// A known required capability is absent from the schema's declarations.
    | MissingCapability of required: string * provided: string list

module SchemaCompatibility =
    /// Capability-inclusion validation: R ⊆ K ∧ R ⊆ P.
    /// Unknown schema-declared capabilities are ignored (Capability Extension
    /// Monotonicity: adding capabilities to a SchemaDefinition must not
    /// invalidate existing ComponentBindings whose requirements remain satisfied).
    let validate (comp: ComponentDefinition) (schema: SchemaDefinition) : Result<unit, CompatibilityError list> =
        let unknownRequired =
            comp.RequiredSchemaCapabilities
            |> List.filter(fun c -> not(Set.contains c SchemaCapabilityVocabulary.known))
            |> List.map UnknownRequiredCapability

        let missing =
            comp.RequiredSchemaCapabilities
            |> List.filter(fun c -> Set.contains c SchemaCapabilityVocabulary.known)
            |> List.filter(fun c -> not(List.contains c schema.Capabilities))
            |> List.map(fun c -> MissingCapability(c, schema.Capabilities))

        match unknownRequired @ missing with
        | [] -> Ok()
        | errors -> Error errors

/// A semantically validated association between a ComponentDefinition and
/// SchemaDefinition under the currently defined compatibility relation.
/// Store and Configuration are carried values whose independent semantics are validated by future contracts.
type ComponentBinding =
    { Definition: ComponentDefinition
      Schema: SchemaDefinition
      Store: DataStoreId
      Configuration: ComponentConfiguration }

module ComponentBinding =
    let create
        (definition: ComponentDefinition)
        (schema: SchemaDefinition)
        (store: DataStoreId)
        (configuration: ComponentConfiguration)
        : Result<ComponentBinding, CompatibilityError list> =
        SchemaCompatibility.validate definition schema
        |> Result.map(fun () ->
            { Definition = definition
              Schema = schema
              Store = store
              Configuration = configuration })

    let componentDefId (b: ComponentBinding) = b.Definition.Id
    let schemaDefId (b: ComponentBinding) = b.Schema.Id
    let store (b: ComponentBinding) = b.Store
    let configuration (b: ComponentBinding) = b.Configuration

    // Constructs the View-local occurrence from a validated component binding.
    let toViewComponent (b: ComponentBinding) : ViewComponent =
        { ComponentDefId = b.Definition.Id
          Binding =
            { StoreId = b.Store
              SchemaDefId = b.Schema.Id }
          Configuration = b.Configuration }

namespace Medhavi.Foundation.Contracts

/// ARS artifact categories as defined in the Architecture Reference Standard
type ArsCategory =
    | Capability
    | Responsibility
    | Decision
    | Rule
    | Policy
    | SemanticObject
    | EnterpriseEvent
    | BusinessNotification
    | BusinessAlgorithm
    | BusinessWorkflow
    | FunctionalSpecification
    | PerformanceIndicator

/// Canonical representation of an architectural artifact
type ArsArtifact =
    {
        /// ARS identifier (e.g., "BR-C-1901", "DE-D-001")
        Id: string
        /// Category of the artifact (Rule, Decision, Capability, etc.)
        Category: ArsCategory
        /// Authoritative business explanation for AI explainability and governance
        Explanation: string
    }

module ArsArtifact =
    /// Internal factory function
    let private create category id explanation =
        { Id = id
          Category = category
          Explanation = explanation }

    let ofCapability id explanation = create Capability id explanation
    let ofResponsibility id explanation = create Responsibility id explanation
    let ofDecision id explanation = create Decision id explanation
    let ofRule id explanation = create Rule id explanation
    let ofPolicy id explanation = create Policy id explanation
    let ofSemanticObject id explanation = create SemanticObject id explanation
    let ofEnterpriseEvent id explanation = create EnterpriseEvent id explanation
    let ofBusinessNotification id explanation = create BusinessNotification id explanation
    let ofBusinessAlgorithm id explanation = create BusinessAlgorithm id explanation
    let ofBusinessWorkflow id explanation = create BusinessWorkflow id explanation
    let ofFunctionalSpecification id explanation = create FunctionalSpecification id explanation
    let ofPerformanceIndicator id explanation = create PerformanceIndicator id explanation

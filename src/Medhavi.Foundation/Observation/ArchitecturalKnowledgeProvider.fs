namespace Medhavi.Foundation.Observation

type KnowledgeRepresentation = ArchitecturalKnowledge -> unit

module KnowledgeRepresentation =
    /// Pure functional decorator: enriches emitted knowledge with the bounded context identity.
    let withContext (boundedContext: string) (publish: KnowledgeRepresentation) : KnowledgeRepresentation =
        fun knowledge ->
            let enriched =
                { knowledge with
                    Attributes = knowledge.Attributes |> Map.add "BoundedContext" (box boundedContext) }
            publish enriched

    /// Pure functional decorator: enriches emitted knowledge with an arbitrary key-value attribute.
    let withAttribute (key: string) (value: obj) (publish: KnowledgeRepresentation) : KnowledgeRepresentation =
        fun knowledge ->
            let enriched =
                { knowledge with
                    Attributes = knowledge.Attributes |> Map.add key value }
            publish enriched

type ArchitecturalKnowledgeProvider = KnowledgeRepresentation list

module ArchitecturalKnowledgeProvider =
    let publish (provider: ArchitecturalKnowledgeProvider) (knowledge: ArchitecturalKnowledge) =
        provider |> List.iter(fun rep -> rep knowledge)

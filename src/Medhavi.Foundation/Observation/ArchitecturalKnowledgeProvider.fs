namespace Medhavi.Foundation.Observation

type KnowledgeRepresentation = ArchitecturalKnowledge -> unit

type ArchitecturalKnowledgeProvider = KnowledgeRepresentation list

module ArchitecturalKnowledgeProvider =
    let publish (provider: ArchitecturalKnowledgeProvider) (knowledge: ArchitecturalKnowledge) =
        provider |> List.iter(fun rep -> rep knowledge)

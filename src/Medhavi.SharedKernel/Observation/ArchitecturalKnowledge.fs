namespace Medhavi.SharedKernel.Observation

open System

type ArchitecturalKnowledge = {
    Name: string
    Timestamp: DateTimeOffset
    Attributes: Map<string, obj>
}

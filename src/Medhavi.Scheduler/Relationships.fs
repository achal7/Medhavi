module Medhavi.Scheduler.Relationships

type DemandRelationshipType =
    | ForecastConsumption
    | BomExplosion
    | TransferDemand
    | Substitution

type DemandRelationship =
    { RelationshipId: string
      ParentDemandLineId: string
      ChildDemandLineId: string
      RelationshipType: DemandRelationshipType
      QuantityFactor: decimal }

type RelationshipType =
    | ForecastConsumption
    | BomExplosion
    | TransferDemand
    | Substitution
    | Pegging
    | Reservation
    | CapacityAllocation

type PlanningRelationship =
    { RelationshipId: string
      SourceEntityId: string
      TargetEntityId: string
      RelationshipType: RelationshipType
      QuantityFactor: decimal }

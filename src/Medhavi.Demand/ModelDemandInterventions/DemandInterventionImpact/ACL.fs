/// Model Demand Interventions Anti-Corruption Layer (ACL)
/// Applicative Validation for incoming Requests to Domain Commands
module Medhavi.Demand.ModelDemandInterventions.DemandInterventionImpact.ACL

open Medhavi.Common.Validation
open Medhavi.Foundation.Failure
open Medhavi.Foundation.Validations
open Medhavi.SemanticModel
open Medhavi.Contracts.Demand
open Medhavi.Demand
open Medhavi.Demand.Helpers
open Model

/// Translates AssessInterventionImpactReq into AssessInterventionImpactCmd using Applicative validation
let toAssessImpactCmd (req: AssessInterventionImpactReq) : Validation<AssessInterventionImpactCmd, DomainError> =
    let create impactId ref item loc mag startTs endTs =
        let window: TemporalWindow =
            { Start = startTs
              End = endTs }

        { ImpactId = impactId
          InterventionReference = ref
          Item = item
          Location = loc
          InterventionType = InterventionType.FromString req.InterventionType
          InterventionMagnitude = mag
          TemporalValidity = window
          HistoricalPairs = req.HistoricalPairs
          BaselineDemand = req.BaselineDemand
          Timestamp = Timestamp.now () }

    create <!> validateDemandInterventionImpactId req.ImpactId
    <*> validateScenarioAdjustmentId req.InterventionReference
    <*> validateItemId req.Item
    <*> validateLocationId req.Location
    <*> nonNegative "InterventionMagnitude" req.InterventionMagnitude
    <*> validateTimestamp req.TemporalValidityStart
    <*> validateTimestamp req.TemporalValidityEnd

/// Translates PublishInterventionImpactReq into PublishInterventionImpactCmd using Applicative validation
let toPublishImpactCmd (req: PublishInterventionImpactReq) : Validation<PublishInterventionImpactCmd, DomainError> =
    let create impactId =
        { ImpactId = impactId
          Timestamp = Timestamp.now () }

    create <!> validateDemandInterventionImpactId req.ImpactId

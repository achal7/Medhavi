namespace Medhavi.Client

open System
open System.Globalization
open Medhavi.Contracts.Core.Demand
open Medhavi.Contracts.Core.Exception
open Medhavi.Contracts.Core.ItemTransition
open Medhavi.Contracts.Demand
open Medhavi.Client.Registries

// ============================================================================
// Core Domain Services (Medhavi.Core)
// ============================================================================

type DemandManagementService =
    { Commands: DemandApi
      Queries: DemandQueries }

type ExceptionManagementService =
    { Commands: ExceptionApi
      Queries: ExceptionQueries }

type ItemTransitionManagementService =
    { Commands: ItemTransitionApi
      Queries: ItemTransitionQueries }

type CoreDomainServices =
    { DemandManagement: DemandManagementService
      ExceptionManagement: ExceptionManagementService
      ItemTransitionManagement: ItemTransitionManagementService }

// ============================================================================
// Demand Domain Services (Medhavi.Demand)
// ============================================================================

type DemandObservationService =
    { Commands: DemandObservationApi
      Queries: DemandObservationQueries }

type DemandUnderstandingService =
    { Commands: DemandUnderstandingApi
      Queries: DemandUnderstandingQueries }

type UnderstandDemandService =
    { Observation: DemandObservationService
      Understanding: DemandUnderstandingService }

type DemandDomainServices =
    { UnderstandDemand: UnderstandDemandService }

// ============================================================================
// Platform Domain Services Bundle
// ============================================================================

type DomainServices =
    { Core: CoreDomainServices
      Demand: DemandDomainServices }

open Medhavi.Client.ViewComposition.Identity
open Medhavi.Client.ViewComposition.Data

// ============================================================================
// Data Store Semantics
// ============================================================================

type DataStore =
    { Id: DataStoreId
      Fetch: ApplicationContext -> Async<DataPayload> }

and DataStoreRegistry = Map<DataStoreId, DataStore>

and ApplicationContext =
    { PlanningContextService: Services.PlanningContextService
      ComponentRegistry: ComponentRegistry
      SchemaRegistry: SchemaRegistry
      DataStoreRegistry: DataStoreRegistry
      DomainServices: DomainServices option }

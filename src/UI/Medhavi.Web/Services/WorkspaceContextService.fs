namespace Medhavi.Web.Services

open System
open Medhavi.Web

type WorkspaceContextService() =
    let mutable currentScope : QueryScope = {
        ScenarioId = Some "BASELINE"
        PlantId = None
        StockingPointId = None
        HorizonStart = DateTime.Today.AddDays(-7.0).Date
        HorizonEnd = DateTime.Today.AddDays(90.0).Date
    }
    
    let scopeChangedEvent = Event<QueryScope>()
    
    member _.CurrentScope = currentScope
    
    member _.SetScope(scope: QueryScope) =
        currentScope <- scope
        scopeChangedEvent.Trigger(currentScope)
        
    [<CLIEvent>]
    member _.OnScopeChanged = scopeChangedEvent.Publish

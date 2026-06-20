namespace Medhavi.Scheduler.Planning.Application

open Medhavi.Contracts.Scenario
open Medhavi.Scheduler.Planning.Domain

type ReadinessSeverity =
    | BlockingError
    | Warning

type ReadinessIssue =
    { Code: string
      Severity: ReadinessSeverity
      Message: string }

module ReadinessIssue =
    let blocking code msg =
        { Code = code
          Severity = BlockingError
          Message = msg }

    let warning code msg =
        { Code = code
          Severity = Warning
          Message = msg }

module ScenarioReadinessValidator =
    let private checkScenarioStatus (scenarioStatus: ScenarioStatus) : ReadinessIssue list =
        match scenarioStatus with
        | ScenarioStatus.Draft ->
            [ ReadinessIssue.blocking "SCEN-001" "Scenario is in Draft status. Complete configuration before planning." ]
        | ScenarioStatus.PlanningRunning ->
            [ ReadinessIssue.blocking "SCEN-002" "A planning run is already in progress for this scenario." ]
        | ScenarioStatus.Archived ->
            [ ReadinessIssue.blocking "SCEN-003" "Cannot start planning on an Archived scenario." ]
        | ScenarioStatus.Published _ ->
            [ ReadinessIssue.blocking "SCEN-004" "Cannot start planning on a Published scenario." ]
        | _ -> []

    let private checkConfiguration (config: PlanRunHorizon) : ReadinessIssue list =
        [
            if config.EndDate <= config.StartDate then
                yield ReadinessIssue.blocking "CFG-001" (sprintf "Planning horizon is empty or inverted: start=%A, end=%A." config.StartDate config.EndDate)

            let days = (config.EndDate - config.StartDate).TotalDays
            if days < 7.0 then
                yield ReadinessIssue.warning "CFG-002" (sprintf "Planning horizon is very short (%g days). Consider extending." days)
        ]

    let private checkDemandData (demands: DemandBucket list) : ReadinessIssue list =
        if demands.IsEmpty then
            [ ReadinessIssue.blocking "DATA-001" "No demand data found for this scenario." ]
        else
            []

    let private checkInventoryData (inventory: SupplyBucket list) : ReadinessIssue list =
        if inventory.IsEmpty then
            [ ReadinessIssue.warning "DATA-002" "No inventory data found. Planning will assume zero on-hand stock." ]
        else
            []

    let validate
        (scenarioStatus: ScenarioStatus)
        (config: PlanRunHorizon)
        (demands: DemandBucket list)
        (inventory: SupplyBucket list)
        : ReadinessIssue list =
        [ yield! checkScenarioStatus scenarioStatus
          yield! checkConfiguration config
          yield! checkDemandData demands
          yield! checkInventoryData inventory ]
        |> List.sortBy (fun issue ->
            match issue.Severity with
            | BlockingError -> 0
            | Warning -> 1)

    let isReady (issues: ReadinessIssue list) : bool =
        issues |> List.forall (fun i -> i.Severity = Warning)

    let validateResult
        (scenarioStatus: ScenarioStatus)
        (config: PlanRunHorizon)
        (demands: DemandBucket list)
        (inventory: SupplyBucket list)
        : Result<unit, ReadinessIssue list> =
        let issues = validate scenarioStatus config demands inventory
        let blocking = issues |> List.filter (fun i -> i.Severity = BlockingError)
        if blocking.IsEmpty then Ok() else Error blocking

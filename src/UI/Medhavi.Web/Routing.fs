namespace Medhavi.Web

open Bolero

type Page =
    | [<EndPoint "/">] Dashboard
    | [<EndPoint "/demand">] Demand
    | [<EndPoint "/supply">] Supply
    | [<EndPoint "/capacity">] Capacity
    | [<EndPoint "/scenarios">] Scenarios

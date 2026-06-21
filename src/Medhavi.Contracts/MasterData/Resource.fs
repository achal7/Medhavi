namespace Medhavi.Contracts.MasterData.Resource

open System
open System.Threading.Tasks
open Medhavi.Contracts

type ResourceGroup =
    { Id: string
      PlantId: string option
      Name: string
      Description: string option
      DefaultCalendarId: string option
      IsActive: bool
      Created: DateTimeOffset
      Modified: DateTimeOffset }

type StandardResource =
    { Id: string
      ResourceGroupId: string
      Name: string
      Description: string option
      DefaultEfficiency: decimal
      DefaultCostRateAmount: decimal option
      DefaultCostRateCurrency: string option
      IsActive: bool
      Created: DateTimeOffset
      Modified: DateTimeOffset }

type PhysicalResource =
    { Id: string
      StandardResourceId: string
      Name: string
      SerialNumber: string option
      Location: string option
      EfficiencyOverride: decimal option
      CostRateOverrideAmount: decimal option
      CostRateOverrideCurrency: string option
      CalendarId: string option
      IsActive: bool
      Created: DateTimeOffset
      Modified: DateTimeOffset }

type ResourceGroupDefined = ResourceGroup
type StandardResourceDefined = StandardResource
type PhysicalResourceDefined = PhysicalResource

type ResourceGroupEvent =
    | ResourceGroupDefined of ResourceGroupDefined
    | ResourceGroupRenamed of id: string * name: string
    | ResourceGroupRetired of id: string

type StandardResourceEvent =
    | StandardResourceDefined of StandardResourceDefined
    | StandardResourceRenamed of id: string * name: string
    | StandardResourceRetired of id: string

type PhysicalResourceEvent =
    | PhysicalResourceDefined of PhysicalResourceDefined
    | PhysicalResourceRenamed of id: string * name: string
    | PhysicalResourceRetired of id: string

type ResourceGroupDefineReq =
    { Id: string
      PlantId: string option
      Name: string
      Description: string option
      DefaultCalendarId: string option
      IsActive: bool
      Created: DateTimeOffset }

type StandardResourceDefineReq =
    { Id: string
      ResourceGroupId: string
      Name: string
      Description: string option
      DefaultEfficiency: decimal
      DefaultCostRateAmount: decimal option
      DefaultCostRateCurrency: string option
      IsActive: bool
      Created: DateTimeOffset }

type PhysicalResourceDefineReq =
    { Id: string
      StandardResourceId: string
      Name: string
      SerialNumber: string option
      Location: string option
      EfficiencyOverride: decimal option
      CostRateOverrideAmount: decimal option
      CostRateOverrideCurrency: string option
      CalendarId: string option
      IsActive: bool
      Created: DateTimeOffset }

type ResourceGroupRenameReq = { Id: string; NewName: string }
type ResourceGroupRetireReq = { Id: string }

type StandardResourceRenameReq = { Id: string; NewName: string }
type StandardResourceRetireReq = { Id: string }

type PhysicalResourceRenameReq = { Id: string; NewName: string }
type PhysicalResourceRetireReq = { Id: string }

type ResourceGroupApi =
    { Define: ResourceGroupDefineReq -> Task<Result<ResourceGroup, ApiError>>
      DefineBulk: ResourceGroupDefineReq list -> Task<Result<ResourceGroup list, ApiError>>
      Rename: ResourceGroupRenameReq -> Task<Result<ResourceGroup, ApiError>>
      Retire: ResourceGroupRetireReq -> Task<Result<ResourceGroup, ApiError>> }

type ResourceGroupQueryService = QueryService<ResourceGroup, string>

type StandardResourceApi =
    { Define: StandardResourceDefineReq -> Task<Result<StandardResource, ApiError>>
      DefineBulk: StandardResourceDefineReq list -> Task<Result<StandardResource list, ApiError>>
      Rename: StandardResourceRenameReq -> Task<Result<StandardResource, ApiError>>
      Retire: StandardResourceRetireReq -> Task<Result<StandardResource, ApiError>> }

type StandardResourceQueryService = QueryService<StandardResource, string>

type PhysicalResourceApi =
    { Define: PhysicalResourceDefineReq -> Task<Result<PhysicalResource, ApiError>>
      DefineBulk: PhysicalResourceDefineReq list -> Task<Result<PhysicalResource list, ApiError>>
      Rename: PhysicalResourceRenameReq -> Task<Result<PhysicalResource, ApiError>>
      Retire: PhysicalResourceRetireReq -> Task<Result<PhysicalResource, ApiError>> }

type PhysicalResourceQueryService = QueryService<PhysicalResource, string>

namespace Medhavi.SharedKernel.Projections

open System.Threading.Tasks

type QueryService<'Entity, 'Id> =
    { GetAll: unit -> Task<'Entity list>
      GetById: 'Id -> Task<'Entity option>
      Exists: 'Id -> Task<bool>
      Filter: ('Entity -> bool) -> Task<'Entity list> }

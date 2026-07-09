namespace Medhavi.Infrastructure.Observation

type IngestionLogger =
    { LogInfo: string -> unit
      LogSuccess: string -> unit
      LogError: string -> unit }

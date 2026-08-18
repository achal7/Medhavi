namespace Medhavi.SemanticModel

open System

type TimeZoneId = private TimeZoneId of string

module TimeZoneId =
    let create (id: string) = Invariants.createStringId TimeZoneId "TimeZoneId" id
    let value (TimeZoneId id) = id

/// SE-C-031 Time Zone
type TimeZone =
    { TimeZoneIdentifier: TimeZoneId
      DisplayName: string
      UtcOffset: TimeSpan }

module TimeZone =
    let validate (timeZone: TimeZone) : Result<unit, SemanticValidationError> =
        let minOffset = TimeSpan.FromHours -14.0
        let maxOffset = TimeSpan.FromHours 14.0

        let offsetCheck =
            if timeZone.UtcOffset < minOffset || timeZone.UtcOffset > maxOffset then
                Error(InvariantViolation("TimeZone", "UtcOffset must be within the valid global UTC offset range."))
            else
                Ok()

        Invariants.firstError
            [ Invariants.nonEmptyIdentifier "TimeZoneId" (TimeZoneId.value timeZone.TimeZoneIdentifier)
              Invariants.nonEmptyField "TimeZone" "DisplayName" timeZone.DisplayName
              offsetCheck ]

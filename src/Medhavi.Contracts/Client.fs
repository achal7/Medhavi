module Medhavi.Contracts.Client

open System
open System.Collections.Concurrent

type LanguageTag = private LanguageTag of string
type CultureName = private CultureName of string
type TimeZoneIdentifier = private TimeZoneIdentifier of string

type TextDirection =
    | LeftToRight
    | RightToLeft

type DateFormatPreference =
    | CultureDefault
    | DayMonthYear
    | MonthDayYear
    | YearMonthDay

type TimeFormatPreference =
    | TwelveHour
    | TwentyFourHour

type DurationFormat =
    | DecimalHours
    | HoursAndMinutes

type MeasurementSystem =
    | Metric
    | Imperial

type CurrencyDisplay =
    | CurrencySymbol
    | CurrencyCode

type PlantTimeZoneDisplay =
    | PlantTimeOnly
    | UserTimeOnly
    | PlantAndUserTime

type VisualTheme =
    | Material
    | MaterialDark
    | Standard
    | StandardDark
    | Default
    | DefaultDark
    | Humanistic
    | HumanisticDark
    | Software
    | SoftwareDark

type Density =
    | Compact
    | Comfortable

type ContrastMode =
    | SystemContrast
    | StandardContrast
    | EnhancedContrast

type CulturalPreferences =
    { LanguageTag: LanguageTag
      FormattingCulture: CultureName
      DateFormat: DateFormatPreference
      TimeFormat: TimeFormatPreference
      UserTimeZone: TimeZoneIdentifier
      PlantTimeZoneDisplay: PlantTimeZoneDisplay
      FirstDayOfWeek: DayOfWeek
      DurationFormat: DurationFormat
      MeasurementSystem: MeasurementSystem
      CurrencyDisplay: CurrencyDisplay
      TextDirection: TextDirection }

type AppearancePreferences =
    { Theme: VisualTheme
      Density: Density
      ContrastMode: ContrastMode
      ReducedMotion: bool
      FontScalePercent: int }

type AccessibilityPreferences =
    { AnnounceRefreshes: bool
      PreferTextualCharts: bool
      MinimumTouchTarget: bool }

type PreferenceSource =
    | SystemDefault
    | OrganizationDefault
    | RoleDefault
    | UserOverride
    | SessionOverride

type EffectivePreference<'value> =
    { Value: 'value
      Source: PreferenceSource
      IsLocked: bool }

type UserProfile =
    { UserId: string
      DisplayName: string
      RoleName: string
      Cultural: EffectivePreference<CulturalPreferences>
      Appearance: EffectivePreference<AppearancePreferences>
      Accessibility: EffectivePreference<AccessibilityPreferences> }

type PresentationValidationError =
    | MissingLanguageTag
    | MissingCultureName
    | MissingTimeZone
    | FontScaleOutOfRange of Value: int
    | HorizonOutOfRange of Value: int
    | PageSizeOutOfRange of Value: int

module LanguageTag =
    let create (value: string) : Result<LanguageTag, PresentationValidationError> =
        if String.IsNullOrWhiteSpace value then
            Error MissingLanguageTag
        else
            Ok(LanguageTag(value.Trim()))

    let value (LanguageTag value: LanguageTag) : string = value

    let enUS () : LanguageTag = LanguageTag("en-US")

module CultureName =
    let create (value: string) : Result<CultureName, PresentationValidationError> =
        if String.IsNullOrWhiteSpace value then
            Error MissingCultureName
        else
            Ok(CultureName(value.Trim()))

    let value (CultureName value: CultureName) : string = value

    let enUS () : CultureName = CultureName("en-US")

module TimeZoneIdentifier =
    let create (value: string) : Result<TimeZoneIdentifier, PresentationValidationError> =
        if String.IsNullOrWhiteSpace value then
            Error MissingTimeZone
        else
            Ok(TimeZoneIdentifier(value.Trim()))

    let value (TimeZoneIdentifier value: TimeZoneIdentifier) : string = value

    let utc () : TimeZoneIdentifier = TimeZoneIdentifier("UTC")

module AppearancePreferences =
    let validate (preferences: AppearancePreferences) : Result<AppearancePreferences, PresentationValidationError> =
        if preferences.FontScalePercent < 80 || preferences.FontScalePercent > 200 then
            Error(FontScaleOutOfRange preferences.FontScalePercent)
        else
            Ok preferences

module VisualTheme =
    let cssName (theme: VisualTheme) : string =
        match theme with
        | Material -> "material"
        | MaterialDark -> "material-dark"
        | Standard -> "standard"
        | StandardDark -> "standard-dark"
        | Default -> "default"
        | DefaultDark -> "default-dark"
        | Humanistic -> "humanistic"
        | HumanisticDark -> "humanistic-dark"
        | Software -> "software"
        | SoftwareDark -> "software-dark"

type ClientConfiguration =
    { ProductName: string
      EnvironmentLabel: string
      DevelopmentAdapterEnabled: bool
      MaximumRiskRows: int }

type Clock = unit -> DateTimeOffset

let system: Clock = fun () -> DateTimeOffset.UtcNow

let fixedAt (instant: DateTimeOffset) : Clock = fun () -> instant

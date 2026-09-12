namespace Medhavi.Client

open System
open System.Globalization
open Medhavi.Contracts.Client

module Formatting =

    let defaultCulturalPreferences: CulturalPreferences =
        { LanguageTag = LanguageTag.enUS()
          FormattingCulture = CultureName.enUS()
          DateFormat = DateFormatPreference.CultureDefault
          TimeFormat = TimeFormatPreference.TwentyFourHour
          UserTimeZone = TimeZoneIdentifier.utc()
          PlantTimeZoneDisplay = PlantTimeZoneDisplay.PlantAndUserTime
          FirstDayOfWeek = DayOfWeek.Monday
          DurationFormat = DurationFormat.HoursAndMinutes
          MeasurementSystem = MeasurementSystem.Metric
          CurrencyDisplay = CurrencyDisplay.CurrencySymbol
          TextDirection = TextDirection.LeftToRight }

    let defaultAppearancePreferences: AppearancePreferences =
        { Theme = VisualTheme.SoftwareDark
          Density = Density.Compact
          ContrastMode = ContrastMode.StandardContrast
          ReducedMotion = false
          FontScalePercent = 100 }

    type CulturePreset =
        { Code: string
          DisplayName: string
          RegionName: string
          DefaultCurrency: string
          Symbol: string
          SampleNumber: string }

    let availableCultures: CulturePreset list =
        [ { Code = "en-US"; DisplayName = "English (United States)"; RegionName = "North America"; DefaultCurrency = "USD"; Symbol = "$"; SampleNumber = "1,234,567.89" }
          { Code = "en-GB"; DisplayName = "English (United Kingdom)"; RegionName = "United Kingdom"; DefaultCurrency = "GBP"; Symbol = "£"; SampleNumber = "1,234,567.89" }
          { Code = "de-DE"; DisplayName = "Deutsch (Deutschland)"; RegionName = "European Union (Central)"; DefaultCurrency = "EUR"; Symbol = "€"; SampleNumber = "1.234.567,89" }
          { Code = "fr-FR"; DisplayName = "Français (France)"; RegionName = "European Union (Western)"; DefaultCurrency = "EUR"; Symbol = "€"; SampleNumber = "1 234 567,89" }
          { Code = "ja-JP"; DisplayName = "日本語 (日本)"; RegionName = "Asia Pacific (Japan)"; DefaultCurrency = "JPY"; Symbol = "¥"; SampleNumber = "1,234,568" }
          { Code = "hi-IN"; DisplayName = "हिन्दी (भारत)"; RegionName = "South Asia (India)"; DefaultCurrency = "INR"; Symbol = "₹"; SampleNumber = "12,34,567.89" }
          { Code = "zh-CN"; DisplayName = "中文 (中国)"; RegionName = "East Asia (China)"; DefaultCurrency = "CNY"; Symbol = "¥"; SampleNumber = "1,234,567.89" }
          { Code = "es-ES"; DisplayName = "Español (España)"; RegionName = "European Union (Southern)"; DefaultCurrency = "EUR"; Symbol = "€"; SampleNumber = "1.234.567,89" } ]

    let toCultureInfo (prefs: CulturalPreferences) : CultureInfo =
        let cultureStr = CultureName.value prefs.FormattingCulture
        try
            CultureInfo.GetCultureInfo(cultureStr)
        with _ ->
            CultureInfo.InvariantCulture

    let formatNumber (prefs: CulturalPreferences) (decimals: int) (value: decimal) : string =
        let ci = toCultureInfo prefs
        let formatSpecifier = if decimals > 0 then "N" + string decimals else "N0"
        value.ToString(formatSpecifier, ci)

    let currencySymbolForCode (currencyCode: string) : string =
        match currencyCode.ToUpperInvariant() with
        | "USD" -> "$"
        | "EUR" -> "€"
        | "GBP" -> "£"
        | "JPY" | "CNY" -> "¥"
        | "INR" -> "₹"
        | "CHF" -> "CHF"
        | other -> other

    /// Formats a financial amount in a specific ledger currency, adhering to the user's punctuation rules while preserving the asset currency.
    let formatLedgerCurrency (prefs: CulturalPreferences) (ledgerCurrencyCode: string) (value: decimal) : string =
        let ci = toCultureInfo prefs
        let cultureCode = CultureName.value prefs.FormattingCulture
        let num = value.ToString("N2", ci)

        match prefs.CurrencyDisplay with
        | CurrencyDisplay.CurrencyCode ->
            $"{num} {ledgerCurrencyCode}"
        | CurrencyDisplay.CurrencySymbol ->
            let symbol = currencySymbolForCode ledgerCurrencyCode
            if cultureCode.StartsWith("de") || cultureCode.StartsWith("fr") || cultureCode.StartsWith("es") then
                $"{num} {symbol}"
            else
                $"{symbol}{num}"

    let formatCurrency (prefs: CulturalPreferences) (value: decimal) : string =
        let cultureCode = CultureName.value prefs.FormattingCulture
        let preset = availableCultures |> List.tryFind (fun c -> c.Code = cultureCode)
        let defaultCode = preset |> Option.map (fun p -> p.DefaultCurrency) |> Option.defaultValue "EUR"
        formatLedgerCurrency prefs defaultCode value

    let formatDate (prefs: CulturalPreferences) (instant: DateTimeOffset) : string =
        let ci = toCultureInfo prefs
        match prefs.DateFormat with
        | DateFormatPreference.DayMonthYear -> instant.ToString("dd/MM/yyyy", ci)
        | DateFormatPreference.MonthDayYear -> instant.ToString("MM/dd/yyyy", ci)
        | DateFormatPreference.YearMonthDay -> instant.ToString("yyyy-MM-dd", ci)
        | DateFormatPreference.CultureDefault -> instant.ToString("d", ci)

    let formatTime (prefs: CulturalPreferences) (instant: DateTimeOffset) : string =
        let ci = toCultureInfo prefs
        match prefs.TimeFormat with
        | TimeFormatPreference.TwelveHour -> instant.ToString("hh:mm:ss tt", ci)
        | TimeFormatPreference.TwentyFourHour -> instant.ToString("HH:mm:ss", ci)

    let formatDateTime (prefs: CulturalPreferences) (instant: DateTimeOffset) : string =
        let d = formatDate prefs instant
        let t = formatTime prefs instant
        $"{d} {t}"

    let formatPlantAndUserDateTime (prefs: CulturalPreferences) (plantTimeZone: TimeZoneInfo) (instant: DateTimeOffset) : string =
        let plantTime = TimeZoneInfo.ConvertTime(instant, plantTimeZone)
        let plantStr = formatDateTime prefs plantTime

        match prefs.PlantTimeZoneDisplay with
        | PlantTimeZoneDisplay.PlantTimeOnly ->
            $"{plantStr} ({plantTimeZone.DisplayName})"
        | PlantTimeZoneDisplay.UserTimeOnly ->
            formatDateTime prefs instant
        | PlantTimeZoneDisplay.PlantAndUserTime ->
            let userStr = formatDateTime prefs instant
            $"{plantStr} [{plantTimeZone.Id}] / {userStr} [User]"

    let formatDuration (prefs: CulturalPreferences) (duration: TimeSpan) : string =
        match prefs.DurationFormat with
        | DurationFormat.DecimalHours ->
            let hours = duration.TotalHours
            let ci = toCultureInfo prefs
            hours.ToString("0.0", ci) + "h"
        | DurationFormat.HoursAndMinutes ->
            let hours = int duration.TotalHours
            let mins = duration.Minutes
            $"{hours}h {mins}m"

    let formatQuantity (prefs: CulturalPreferences) (qty: decimal) (baseUnit: string) : string =
        let ci = toCultureInfo prefs
        match prefs.MeasurementSystem with
        | MeasurementSystem.Metric ->
            let formatted = qty.ToString("N2", ci)
            $"{formatted} {baseUnit}"
        | MeasurementSystem.Imperial ->
            let converted, unitLabel =
                if baseUnit = "MT" || baseUnit = "tons" then
                    (qty * 1.10231m), "US tons"
                elif baseUnit = "kg" then
                    (qty * 2.20462m), "lbs"
                else
                    qty, baseUnit
            let formatted = converted.ToString("N2", ci)
            $"{formatted} {unitLabel}"

    type FormattingPreview =
        { CurrencySample: string
          DateSample: string
          TimeSample: string
          DateTimeSample: string
          QuantitySample: string
          NumberSample: string }

    let generatePreview (prefs: CulturalPreferences) : FormattingPreview =
        let sampleInstant = DateTimeOffset(2026, 9, 1, 14, 30, 0, TimeSpan.Zero)
        let sampleAmount = 1234567.89m
        let sampleQty = 12500.00m

        { CurrencySample = formatCurrency prefs sampleAmount
          DateSample = formatDate prefs sampleInstant
          TimeSample = formatTime prefs sampleInstant
          DateTimeSample = formatDateTime prefs sampleInstant
          QuantitySample = formatQuantity prefs sampleQty "MT"
          NumberSample = formatNumber prefs 2 sampleAmount }

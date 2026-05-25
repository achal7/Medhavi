namespace Medhavi.Common.Enrichment

open System
open System.Text.RegularExpressions

// ==========================================
// ENRICHMENT VALIDATION - Category Theory Validation Patterns
// ==========================================

/// Advanced validation patterns using Category Theory
module ValidationPatterns =

    // Validation result type (different from EnrichmentResult)
    type ValidationResult<'T> =
        | Valid of 'T
        | Invalid of ValidationError list

    and ValidationError =
        { Field: string
          Message: string
          Code: string
          Severity: ValidationSeverity }

    and ValidationSeverity =
        | Info
        | Warning
        | Error
        | Critical

    // Validation rule type
    type ValidationRule<'T> =
        { Name: string
          Description: string
          Validator: 'T -> ValidationResult<'T>
          Severity: ValidationSeverity }

    // Validation applicative functor
    module ValidationApplicative =

        let return_ (x: 'T) : ValidationResult<'T> = Valid x

        let apply (f: ValidationResult<'T -> 'U>) (x: ValidationResult<'T>) : ValidationResult<'U> =
            match f, x with
            | Valid f', Valid x' -> Valid(f' x')
            | Invalid e1, Valid _ -> Invalid e1
            | Valid _, Invalid e2 -> Invalid e2
            | Invalid e1, Invalid e2 -> Invalid(e1 @ e2)

        let liftA2 f x y = apply (apply (return_ f) x) y

    // Validation monoid (combines validation results)
    module ValidationMonoid =

        let empty<'T> : ValidationResult<'T> = Valid Unchecked.defaultof<'T>

        let combine (a: ValidationResult<'T>) (b: ValidationResult<'T>) : ValidationResult<'T> =
            match a, b with
            | Valid x, Valid _ -> Valid x
            | Valid _, Invalid e -> Invalid e
            | Invalid e, Valid _ -> Invalid e
            | Invalid e1, Invalid e2 -> Invalid(e1 @ e2)

    // Common validation rules
    module ValidationRules =

        // String validations
        let notEmpty fieldName (value: string) =
            if String.IsNullOrWhiteSpace value then
                Invalid
                    [ { Field = fieldName
                        Message = $"{fieldName} cannot be empty"
                        Code = "REQUIRED"
                        Severity = Error } ]
            else
                Valid value

        let maxLength fieldName maxLen (value: string) =
            if value.Length > maxLen then
                Invalid
                    [ { Field = fieldName
                        Message = $"{fieldName} cannot exceed {maxLen} characters"
                        Code = "MAX_LENGTH"
                        Severity = Error } ]
            else
                Valid value

        let minLength fieldName minLen (value: string) =
            if value.Length < minLen then
                Invalid
                    [ { Field = fieldName
                        Message = $"{fieldName} must be at least {minLen} characters"
                        Code = "MIN_LENGTH"
                        Severity = Error } ]
            else
                Valid value

        let regex fieldName pattern (value: string) =
            if Regex.IsMatch(value, pattern) then
                Valid value
            else
                Invalid
                    [ { Field = fieldName
                        Message = $"{fieldName} does not match required pattern"
                        Code = "REGEX"
                        Severity = Error } ]

        // Numeric validations
        let range fieldName minVal maxVal (value: 'T when 'T: comparison) =
            if value >= minVal && value <= maxVal then
                Valid value
            else
                Invalid
                    [ { Field = fieldName
                        Message = $"{fieldName} must be between {minVal} and {maxVal}"
                        Code = "RANGE"
                        Severity = Error } ]

        let positive fieldName (value: 'T when 'T: comparison and 'T :> IComparable) =
            if value > Unchecked.defaultof<'T> then
                Valid value
            else
                Invalid
                    [ { Field = fieldName
                        Message = $"{fieldName} must be positive"
                        Code = "POSITIVE"
                        Severity = Error } ]

        // Date/Time validations
        let notFuture fieldName (value: DateTimeOffset) =
            if value <= DateTimeOffset.UtcNow then
                Valid value
            else
                Invalid
                    [ { Field = fieldName
                        Message = $"{fieldName} cannot be in the future"
                        Code = "NOT_FUTURE"
                        Severity = Error } ]

        let notPast fieldName (value: DateTimeOffset) =
            if value >= DateTimeOffset.UtcNow then
                Valid value
            else
                Invalid
                    [ { Field = fieldName
                        Message = $"{fieldName} cannot be in the past"
                        Code = "NOT_PAST"
                        Severity = Error } ]

        // Collection validations
        let notEmptyCollection fieldName (value: 'T seq) =
            if Seq.isEmpty value then
                Invalid
                    [ { Field = fieldName
                        Message = $"{fieldName} cannot be empty"
                        Code = "NOT_EMPTY"
                        Severity = Error } ]
            else
                Valid value

        let maxCount fieldName maxCount (value: 'T seq) =
            if Seq.length value > maxCount then
                Invalid
                    [ { Field = fieldName
                        Message = $"{fieldName} cannot have more than {maxCount} items"
                        Code = "MAX_COUNT"
                        Severity = Error } ]
            else
                Valid value

    // Validation pipeline
    type ValidationPipeline<'T> =
        { Rules: ValidationRule<'T> list
          StopOnFirstError: bool }

    let createValidationPipeline rules stopOnFirstError =
        { Rules = rules
          StopOnFirstError = stopOnFirstError }

    let validateWithPipeline (pipeline: ValidationPipeline<'T>) (input: 'T) =
        let rec validateRules remainingRules errors =
            match remainingRules with
            | [] -> if List.isEmpty errors then Valid input else Invalid errors
            | rule :: rest ->
                match rule.Validator input with
                | Valid _ -> validateRules rest errors
                | Invalid newErrors ->
                    let combinedErrors = errors @ newErrors

                    if pipeline.StopOnFirstError then
                        Invalid combinedErrors
                    else
                        validateRules rest combinedErrors

        validateRules pipeline.Rules []

    // Validation rule builder
    type ValidationRuleBuilder<'T>() =

        member this.CreateRule(name, description, validator, ?severity) =
            { Name = name
              Description = description
              Validator = validator
              Severity = defaultArg severity Error }

        member this.NotEmpty(fieldName) =
            this.CreateRule(
                $"{fieldName}_not_empty",
                $"{fieldName} must not be empty",
                ValidationRules.notEmpty fieldName
            )

        member this.MaxLength(fieldName, maxLen) =
            this.CreateRule(
                $"{fieldName}_max_length",
                $"{fieldName} must not exceed {maxLen} characters",
                ValidationRules.maxLength fieldName maxLen
            )

        member this.MinLength(fieldName, minLen) =
            this.CreateRule(
                $"{fieldName}_min_length",
                $"{fieldName} must be at least {minLen} characters",
                ValidationRules.minLength fieldName minLen
            )

        member this.Range(fieldName, minVal, maxVal) =
            this.CreateRule(
                $"{fieldName}_range",
                $"{fieldName} must be between {minVal} and {maxVal}",
                ValidationRules.range fieldName minVal maxVal
            )

    let validationRuleBuilder<'T> () = ValidationRuleBuilder<'T>()

    // Validation result utilities
    module ValidationResult =

        let isValid =
            function
            | Valid _ -> true
            | Invalid _ -> false

        let isInvalid =
            function
            | Valid _ -> false
            | Invalid _ -> true

        let getValue =
            function
            | Valid value -> Some value
            | Invalid _ -> None

        let getErrors =
            function
            | Valid _ -> []
            | Invalid errors -> errors

        let map f =
            function
            | Valid value -> Valid(f value)
            | Invalid errors -> Invalid errors

        let bind f =
            function
            | Valid value -> f value
            | Invalid errors -> Invalid errors

        let defaultValue defaultValue =
            function
            | Valid value -> value
            | Invalid _ -> defaultValue

        let toEnrichmentResult =
            function
            | Valid value -> Success value
            | Invalid errors ->
                let errorMessages = errors |> List.map (fun e -> e.Message) |> String.concat "; "
                ValidationError errorMessages

    // Cross-field validation
    type CrossFieldValidator<'T> =
        { Name: string
          Fields: string list
          Validator: 'T -> ValidationResult<'T> }

    let createCrossFieldValidator name fields validator =
        { Name = name
          Fields = fields
          Validator = validator }

    // Conditional validation
    type ConditionalValidator<'T> =
        { Condition: 'T -> bool
          Validator: 'T -> ValidationResult<'T> }

    let createConditionalValidator condition validator =
        { Condition = condition
          Validator = validator }

    let validateConditional (validator: ConditionalValidator<'T>) input =
        if validator.Condition input then
            validator.Validator input
        else
            Valid input

    // Validation group for organizing related validations
    type ValidationGroup<'T> =
        { Name: string
          Description: string
          Validators: ('T -> ValidationResult<'T>) list
          IsRequired: bool }

    let createValidationGroup name description validators isRequired =
        { Name = name
          Description = description
          Validators = validators
          IsRequired = isRequired }

    let validateGroup (group: ValidationGroup<'T>) input =
        let results = group.Validators |> List.map (fun validator -> validator input)

        let errors =
            results
            |> List.choose (function
                | Invalid errors -> Some errors
                | _ -> None)
            |> List.concat

        if List.isEmpty errors then Valid input else Invalid errors

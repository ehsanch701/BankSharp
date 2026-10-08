using FluentValidation;

namespace BankSharp.FluentValidation;

/// <summary>
/// Provides extension methods for FluentValidation to simplify IBAN validation.
/// </summary>
public static class IbanValidationExtensions
{
    /// <summary>
    /// Validates an IBAN string, checking for format, valid length, and MOD 97 checksum.
    /// </summary>
    /// <typeparam name="T">The type of the object being validated.</typeparam>
    /// <param name="ruleBuilder">The rule builder on which the validator is configured.</param>
    /// <param name="allowedCountries">Optional list of allowed ISO country codes (e.g., "IR", "DE").</param>
    /// <returns>Rule builder options to enable fluent chaining.</returns>
    public static IRuleBuilderOptions<T, string?> MustBeValidIban<T>(
        this IRuleBuilder<T, string?> ruleBuilder,
        params string[] allowedCountries)
    {
        return (IRuleBuilderOptions<T, string?>)ruleBuilder.Custom((rawIban, context) =>
        {
            // Skip null or whitespace; let NotEmpty() or NotNull() handle presence checks.
            if (string.IsNullOrWhiteSpace(rawIban))
                return;

            // Execute core validation logic
            var result = IbanValidator.Validate(rawIban);

            if (!result.IsValid)
            {
                context.AddFailure(context.DisplayName, GetErrorMessage(context.DisplayName, result.Error));
                return;
            }

            // Apply country-specific filtering if requested
            CheckAllowedCountries(context, result.CountryCode, allowedCountries);
        });
    }

    /// <summary>
    /// Validates a strongly-typed <see cref="Iban"/> instance against allowed country constraints.
    /// </summary>
    /// <typeparam name="T">The type of the object being validated.</typeparam>
    /// <param name="ruleBuilder">The rule builder on which the validator is configured.</param>
    /// <param name="allowedCountries">Optional list of allowed ISO country codes (e.g., "IR", "DE").</param>
    /// <returns>Rule builder options to enable fluent chaining.</returns>
    public static IRuleBuilderOptions<T, Iban> MustBeValidIban<T>(
        this IRuleBuilder<T, Iban> ruleBuilder,
        params string[] allowedCountries)
    {
        return (IRuleBuilderOptions<T, Iban>)ruleBuilder.Custom((iban, context) =>
        {
            if (iban.IsEmpty)
            {
                context.AddFailure(context.DisplayName, $"'{context.DisplayName}' must not be an empty IBAN.");
                return;
            }

            CheckAllowedCountries(context, iban.CountryCode, allowedCountries);
        });
    }

    /// <summary>
    /// Validates a nullable strongly-typed <see cref="Iban"/> instance against allowed country constraints.
    /// </summary>
    /// <typeparam name="T">The type of the object being validated.</typeparam>
    /// <param name="ruleBuilder">The rule builder on which the validator is configured.</param>
    /// <param name="allowedCountries">Optional list of allowed ISO country codes (e.g., "IR", "DE").</param>
    /// <returns>Rule builder options to enable fluent chaining.</returns>
    public static IRuleBuilderOptions<T, Iban?> MustBeValidIban<T>(
        this IRuleBuilder<T, Iban?> ruleBuilder,
        params string[] allowedCountries)
    {
        return (IRuleBuilderOptions<T, Iban?>)ruleBuilder.Custom((iban, context) =>
        {
            if (iban is null || iban.Value.IsEmpty)
                return;

            CheckAllowedCountries(context, iban.Value.CountryCode, allowedCountries);
        });
    }

    /// <summary>
    /// Validates whether the given country code is in the allowed countries list.
    /// </summary>
    private static void CheckAllowedCountries<T>(
        ValidationContext<T> context,
        string? countryCode,
        string[]? allowedCountries)
    {
        if (allowedCountries is not { Length: > 0 } || countryCode is null)
            return;

        for (var i = 0; i < allowedCountries.Length; i++)
        {
            if (string.Equals(allowedCountries[i], countryCode, StringComparison.OrdinalIgnoreCase))
            {
                return;
            }
        }

        context.AddFailure(
            context.DisplayName,
            $"'{context.DisplayName}' with country '{countryCode}' is not allowed. " +
            $"Allowed countries: {string.Join(", ", allowedCountries)}.");
    }

    /// <summary>
    /// Maps internal validation errors to human-readable error messages.
    /// </summary>
    private static string GetErrorMessage(string propertyName, IbanValidationError error) => error switch
    {
        IbanValidationError.InvalidLength => $"'{propertyName}' has an invalid IBAN length.",
        IbanValidationError.InvalidCountryCode => $"'{propertyName}' has an invalid country code.",
        IbanValidationError.UnsupportedCountry => $"'{propertyName}' contains an unsupported country code.",
        IbanValidationError.InvalidCharacters => $"'{propertyName}' contains invalid characters.",
        IbanValidationError.InvalidCheckDigits => $"'{propertyName}' failed MOD 97 checksum validation.",
        _ => $"'{propertyName}' is not a valid IBAN."
    };
}

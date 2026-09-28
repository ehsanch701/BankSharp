using System.Runtime.CompilerServices;
using BankSharp.Engines;
using BankSharp.Registry;

namespace BankSharp;

/// <summary>
/// Provides high-performance, allocation-free IBAN validation mechanisms.
/// </summary>
public static class IbanValidator
{
    /// <summary>
    /// Validates an IBAN string input.
    /// </summary>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static IbanValidationResult Validate(string? iban)
    {
        if (string.IsNullOrWhiteSpace(iban))
        {
            return IbanValidationResult.Failure(IbanValidationError.EmptyOrNull);
        }

        return Validate(iban.AsSpan());
    }

    /// <summary>
    /// Validates an IBAN represented as a read-only character span without heap allocations.
    /// </summary>
    public static IbanValidationResult Validate(ReadOnlySpan<char> iban)
    {
        // 1. Trim leading and trailing whitespace
        iban = iban.Trim();

        if (iban.IsEmpty)
        {
            return IbanValidationResult.Failure(IbanValidationError.EmptyOrNull);
        }

        // 2. Length check: Minimal length for any standard IBAN is 5 (e.g., 'A1100')
        if (iban.Length < 5)
        {
            return IbanValidationResult.Failure(IbanValidationError.InvalidLength);
        }

        // 3. Basic Alphanumeric check for the entire IBAN
        foreach (char c in iban)
        {
            if (!char.IsAsciiLetterOrDigit(c))
            {
                return IbanValidationResult.Failure(IbanValidationError.InvalidCharacters);
            }
        }

        // 4. Country code check (first 2 chars)
        ReadOnlySpan<char> countryCodeSpan = iban[..2];
        if (!char.IsAsciiLetter(countryCodeSpan[0]) || !char.IsAsciiLetter(countryCodeSpan[1]))
        {
            return IbanValidationResult.Failure(IbanValidationError.InvalidCountryCode);
        }

        // 5. Registry Lookup
        if (!IbanRegistry.TryGetRule(countryCodeSpan, out var rule))
        {
            return IbanValidationResult.Failure(IbanValidationError.UnsupportedCountry);
        }

        // 6. Validate country-specific length constraint
        if (iban.Length != rule.TotalLength)
        {
            return IbanValidationResult.Failure(IbanValidationError.InvalidLength, rule.CountryCode);
        }

        // 7. Check if ISO digits (3 and 4) are actually digits
        if (!char.IsAsciiDigit(iban[2]) || !char.IsAsciiDigit(iban[3]))
        {
            return IbanValidationResult.Failure(IbanValidationError.InvalidCheckDigits, rule.CountryCode);
        }

        // 8. Compute ISO 7064 Mod-97 checksum
        // According to ISO 7064, a valid IBAN must result in a remainder of 1.
        int remainder = Mod97Engine.CalculateIbanMod97(iban);
        if (remainder != 1)
        {
            return IbanValidationResult.Failure(IbanValidationError.InvalidCheckDigits, rule.CountryCode);
        }

        return IbanValidationResult.Success(rule.CountryCode);
    }
}

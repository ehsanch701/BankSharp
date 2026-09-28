using System;
using BankSharp.Registry;

namespace BankSharp.Parsing;

/// <summary>
/// Provides high-performance parsing capabilities to extract structural components (country code, bank code, branch code, and account number) from IBAN spans.
/// </summary>
public static class IbanParser
{
    /// <summary>
    /// Parses a normalized IBAN character span into its structural components based on registered country rules.
    /// </summary>
    /// <param name="iban">The normalized IBAN character span to parse.</param>
    /// <returns>A <see cref="ParsedIban"/> instance containing the extracted components.</returns>
    /// <exception cref="ArgumentException">Thrown when <paramref name="iban"/> has an invalid format, unsupported country code, or length mismatch.</exception>
    public static ParsedIban Parse(ReadOnlySpan<char> iban)
    {
        if (TryParse(iban, out var parsed))
        {
            return parsed;
        }

        throw new ArgumentException("Invalid IBAN format or unknown country code.", nameof(iban));
    }

    /// <summary>
    /// Attempts to parse an IBAN character span into its structural components without throwing exceptions on validation failure.
    /// </summary>
    /// <param name="iban">The normalized IBAN character span to parse.</param>
    /// <param name="result">When this method returns <see langword="true"/>, contains the extracted <see cref="ParsedIban"/> components; otherwise, contains <see langword="default"/>.</param>
    /// <returns><see langword="true"/> if the IBAN was successfully matched and parsed against a registered country rule; otherwise, <see langword="false"/>.</returns>
    public static bool TryParse(ReadOnlySpan<char> iban, out ParsedIban result)
    {
        if (TryParseSpan(iban, out ParsedIbanSpan spanResult))
        {
            result = new ParsedIban(
                spanResult.CountryCode.ToString(),
                spanResult.BankCode.ToString(),
                spanResult.BranchCode.IsEmpty ? null : spanResult.BranchCode.ToString(),
                spanResult.AccountNumber.ToString()
            );
            return true;
        }

        result = default;
        return false;
    }

    /// <summary>
    /// Parses a normalized IBAN character span into its zero-allocation structural span components based on registered country rules.
    /// </summary>
    /// <param name="iban">The normalized IBAN character span to parse.</param>
    /// <returns>A <see cref="ParsedIbanSpan"/> instance referencing the extracted components directly from the input span.</returns>
    /// <exception cref="ArgumentException">Thrown when <paramref name="iban"/> has an invalid format, unsupported country code, or length mismatch.</exception>
    public static ParsedIbanSpan ParseSpan(ReadOnlySpan<char> iban)
    {
        if (TryParseSpan(iban, out ParsedIbanSpan parsed))
        {
            return parsed;
        }

        throw new ArgumentException("Invalid IBAN format or unknown country code.", nameof(iban));
    }

    /// <summary>
    /// Attempts to parse an IBAN character span into its zero-allocation structural span components without throwing exceptions or allocating memory on the managed heap.
    /// </summary>
    /// <param name="iban">The normalized IBAN character span to parse.</param>
    /// <param name="result">When this method returns <see langword="true"/>, contains the extracted <see cref="ParsedIbanSpan"/> components; otherwise, contains <see langword="default"/>.</param>
    /// <returns><see langword="true"/> if the IBAN was successfully matched and parsed against a registered country rule; otherwise, <see langword="false"/>.</returns>
    public static bool TryParseSpan(ReadOnlySpan<char> iban, out ParsedIbanSpan result)
    {
        result = default;

        // Ensure minimum length for country code (2)
        if (iban.Length < 2)
        {
            return false;
        }

        var countrySpan = iban.Slice(0, 2);

        // Resolve rule
        if (!IbanRegistry.TryGetRule(countrySpan, out var rule))
        {
            return false;
        }

        // Validate length and parsability
        if (iban.Length != rule.TotalLength || !rule.IsParsable)
        {
            return false;
        }

        // Slice components without allocating
        var bankCode = iban.Slice(rule.BankCodeOffset, rule.BankCodeLength);

        var branchCode = rule.BranchCodeLength > 0
            ? iban.Slice(rule.BranchCodeOffset, rule.BranchCodeLength)
            : ReadOnlySpan<char>.Empty;

        var accountNumber = iban.Slice(rule.AccountNumberOffset, rule.AccountNumberLength);

        result = new ParsedIbanSpan(countrySpan, bankCode, branchCode, accountNumber);
        return true;
    }
}

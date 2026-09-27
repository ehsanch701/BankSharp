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
        result = default;

        // Ensure minimum length for country code (2)
        if (iban.Length < 2) return false;

        // Resolve rule
        if (!IbanRegistry.TryGetRule(iban.Slice(0, 2), out var rule)) return false;

        // Validate length
        if (iban.Length != rule.TotalLength) return false;

        // Slice components
        var bankCode = iban.Slice(rule.BankCodeOffset, rule.BankCodeLength).ToString();

        string? branchCode = null;
        if (rule.BranchCodeLength > 0)
        {
            branchCode = iban.Slice(rule.BranchCodeOffset, rule.BranchCodeLength).ToString();
        }

        var accountNumber = iban.Slice(rule.AccountNumberOffset, rule.AccountNumberLength).ToString();

        result = new ParsedIban(rule.CountryCode, bankCode, branchCode, accountNumber);
        return true;
    }
}

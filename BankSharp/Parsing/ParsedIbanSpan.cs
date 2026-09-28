using System;

namespace BankSharp.Parsing;

/// <summary>
/// Represents the zero-allocation, span-based decomposed components of an IBAN.
/// </summary>
public readonly ref struct ParsedIbanSpan
{
    /// <summary>
    /// Gets the two-letter ISO 3166-1 alpha-2 country code span.
    /// </summary>
    public ReadOnlySpan<char> CountryCode { get; }

    /// <summary>
    /// Gets the financial institution identifier span.
    /// </summary>
    public ReadOnlySpan<char> BankCode { get; }

    /// <summary>
    /// Gets the optional branch identifier span, or an empty span if not defined for the country.
    /// </summary>
    public ReadOnlySpan<char> BranchCode { get; }

    /// <summary>
    /// Gets the domestic bank account number span.
    /// </summary>
    public ReadOnlySpan<char> AccountNumber { get; }

    /// <summary>
    /// Initializes a new instance of the <see cref="ParsedIbanSpan"/> struct.
    /// </summary>
    /// <param name="countryCode">The two-letter country code span.</param>
    /// <param name="bankCode">The financial institution identifier span.</param>
    /// <param name="branchCode">The optional branch identifier span.</param>
    /// <param name="accountNumber">The domestic bank account number span.</param>
    public ParsedIbanSpan(
        ReadOnlySpan<char> countryCode,
        ReadOnlySpan<char> bankCode,
        ReadOnlySpan<char> branchCode,
        ReadOnlySpan<char> accountNumber)
    {
        CountryCode = countryCode;
        BankCode = bankCode;
        BranchCode = branchCode;
        AccountNumber = accountNumber;
    }
}

namespace BankSharp.Parsing;

/// <summary>
/// Represents the decomposed, structural components of an IBAN, including country, bank, branch, and account details.
/// </summary>
/// <param name="CountryCode">The two-letter ISO 3166-1 alpha-2 country code (e.g., <c>"IR"</c>, <c>"DE"</c>, <c>"GB"</c>).</param>
/// <param name="BankCode">The bank or financial institution identifier extracted from the BBAN portion.</param>
/// <param name="BranchCode">The branch identifier, or <see langword="null"/> if the country structure does not define a branch code.</param>
/// <param name="AccountNumber">The domestic bank account number extracted from the BBAN portion.</param>
public readonly record struct ParsedIban(
    string CountryCode,
    string BankCode,
    string? BranchCode,
    string AccountNumber);

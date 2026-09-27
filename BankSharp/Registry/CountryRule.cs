namespace BankSharp.Registry;

/// <summary>
/// Represents the IBAN validation and parsing rules for a specific country, including length, format pattern, and BBAN offset layout.
/// </summary>
public readonly record struct CountryRule
{
    /// <summary>
    /// Gets the two-letter ISO 3166-1 alpha-2 country code (e.g., <c>"IR"</c>, <c>"DE"</c>, <c>"GB"</c>).
    /// </summary>
    public required string CountryCode { get; init; }

    /// <summary>
    /// Gets the total character length expected for a valid IBAN of this country.
    /// </summary>
    public required int TotalLength { get; init; }

    /// <summary>
    /// Gets the regular expression or format pattern describing the valid structure of the IBAN.
    /// </summary>
    public required string Pattern { get; init; }

    /// <summary>
    /// Gets the zero-based start index of the bank code within the full IBAN string.
    /// </summary>
    public int BankCodeOffset { get; init; }

    /// <summary>
    /// Gets the character length of the bank code.
    /// </summary>
    public int BankCodeLength { get; init; }

    /// <summary>
    /// Gets the zero-based start index of the branch code within the full IBAN string.
    /// </summary>
    public int BranchCodeOffset { get; init; }

    /// <summary>
    /// Gets the character length of the branch code, or <c>0</c> if no branch code is defined.
    /// </summary>
    public int BranchCodeLength { get; init; }

    /// <summary>
    /// Gets the zero-based start index of the domestic account number within the full IBAN string.
    /// </summary>
    public int AccountNumberOffset { get; init; }

    /// <summary>
    /// Gets the character length of the domestic account number.
    /// </summary>
    public int AccountNumberLength { get; init; }

    /// <summary>
    /// Gets a value indicating whether this country rule defines a valid BBAN layout suitable for structural parsing.
    /// </summary>
    public bool IsParsable => BankCodeLength > 0;
}

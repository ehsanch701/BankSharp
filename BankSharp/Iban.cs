using System.Diagnostics.CodeAnalysis;
using BankSharp.Formatting;
using BankSharp.Parsing;

namespace BankSharp;

/// <summary>
/// Represents an immutable, strongly-typed International Bank Account Number (IBAN).
/// Offers zero-allocation parsing, formatting, and comparison operations.
/// </summary>
public readonly record struct Iban :
    IParsable<Iban>,
    ISpanParsable<Iban>,
    IEquatable<Iban>,
    IComparable<Iban>,
    IFormattable
{
    private readonly string? _value;

    private Iban(string value) => _value = value;

    /// <summary>
    /// Gets the normalized IBAN string value in uppercase without spaces.
    /// Returns an empty string if this instance is uninitialized or empty.
    /// </summary>
    public string Value => _value ?? string.Empty;

    /// <summary>
    /// Gets a value indicating whether this IBAN instance represents an empty or uninitialized value.
    /// </summary>
    public bool IsEmpty => string.IsNullOrEmpty(_value);

    /// <summary>
    /// Gets the two-letter ISO 3166-1 country code (e.g., "IR", "DE", "GB").
    /// Returns an empty string if uninitialized.
    /// </summary>
    public string CountryCode => Value.Length >= 2 ? Value[..2] : string.Empty;

    /// <summary>
    /// Gets the two-digit checksum digits of the IBAN.
    /// Returns an empty string if uninitialized.
    /// </summary>
    public string CheckDigits => Value.Length >= 4 ? Value.Substring(2, 2) : string.Empty;

    /// <summary>
    /// Gets an empty, uninitialized <see cref="Iban"/> instance.
    /// </summary>
    public static Iban Empty => default;

    /// <summary>
    /// Parses a string into an <see cref="Iban"/> instance.
    /// </summary>
    /// <param name="s">The string representation of the IBAN to parse.</param>
    /// <param name="provider">An optional format provider.</param>
    /// <returns>A validated <see cref="Iban"/> instance.</returns>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="s"/> is <see langword="null"/>.</exception>
    /// <exception cref="FormatException">Thrown when the input string is not a valid IBAN.</exception>
    public static Iban Parse(string s, IFormatProvider? provider = null)
    {
        ArgumentNullException.ThrowIfNull(s);

        if (TryParse(s, provider, out var result))
            return result;

        throw new FormatException("The provided value is not a valid international bank account number (IBAN).");
    }

    /// <summary>
    /// Parses a span of characters into an <see cref="Iban"/> instance.
    /// </summary>
    /// <param name="s">The span of characters representing the IBAN to parse.</param>
    /// <param name="provider">An optional format provider.</param>
    /// <returns>A validated <see cref="Iban"/> instance.</returns>
    /// <exception cref="FormatException">Thrown when the input span is not a valid IBAN.</exception>
    public static Iban Parse(ReadOnlySpan<char> s, IFormatProvider? provider = null)
    {
        if (TryParse(s, provider, out var result))
            return result;

        throw new FormatException("The provided value is not a valid international bank account number (IBAN).");
    }

    /// <summary>
    /// Tries to parse a string into an <see cref="Iban"/> instance.
    /// </summary>
    /// <param name="s">The string representation of the IBAN to parse.</param>
    /// <param name="provider">An optional format provider.</param>
    /// <param name="result">When this method returns, contains the parsed <see cref="Iban"/> if successful; otherwise, <see cref="Empty"/>.</param>
    /// <returns><see langword="true"/> if the string was successfully parsed and validated; otherwise, <see langword="false"/>.</returns>
    public static bool TryParse([NotNullWhen(true)] string? s, IFormatProvider? provider, out Iban result)
    {
        if (string.IsNullOrWhiteSpace(s))
        {
            result = default;
            return false;
        }

        return TryParse(s.AsSpan(), provider, out result);
    }

    /// <summary>
    /// Tries to parse a character span into an <see cref="Iban"/> instance.
    /// </summary>
    /// <param name="s">The span of characters representing the IBAN to parse.</param>
    /// <param name="provider">An optional format provider.</param>
    /// <param name="result">When this method returns, contains the parsed <see cref="Iban"/> if successful; otherwise, <see cref="Empty"/>.</param>
    /// <returns><see langword="true"/> if the character span was successfully parsed and validated; otherwise, <see langword="false"/>.</returns>
    public static bool TryParse(ReadOnlySpan<char> s, IFormatProvider? provider, out Iban result)
    {
        if (s.IsWhiteSpace() || s.IsEmpty)
        {
            result = default;
            return false;
        }

        var normalized = IbanFormatter.Normalize(s);
        var validation = IbanValidator.Validate(normalized);

        if (!validation.IsValid)
        {
            result = default;
            return false;
        }

        result = new Iban(normalized);
        return true;
    }

    /// <summary>
    /// Explicitly converts an <see cref="Iban"/> instance to its normalized string representation.
    /// </summary>
    /// <param name="iban">The <see cref="Iban"/> instance to convert.</param>
    public static explicit operator string(Iban iban) => iban.Value;

    /// <summary>
    /// Explicitly converts a string to an <see cref="Iban"/> instance.
    /// </summary>
    /// <param name="value">The string value to convert.</param>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="value"/> is <see langword="null"/>.</exception>
    /// <exception cref="FormatException">Thrown when the string is not a valid IBAN.</exception>
    public static explicit operator Iban(string value) => Parse(value);

    /// <summary>
    /// Deconstructs the IBAN into its standard ISO 13616 structural components.
    /// </summary>
    /// <param name="countryCode">The two-letter ISO 3166-1 alpha-2 country code.</param>
    /// <param name="checkDigits">The two check digits.</param>
    /// <param name="bban">The Basic Bank Account Number (BBAN).</param>
    public void Deconstruct(out string countryCode, out string checkDigits, out string bban)
    {
        countryCode = Value[..2];
        checkDigits = Value[2..4];
        bban = Value[4..];
    }

    /// <summary>
    /// Parses and segments the IBAN into its structural components based on country rules.
    /// </summary>
    /// <returns>A <see cref="ParsedIban"/> containing detailed segmented components.</returns>
    public ParsedIban GetComponents() => IbanParser.Parse(Value);

    /// <summary>
    /// Formats the IBAN into a grouped, human-readable presentation format (separated into 4-character blocks).
    /// </summary>
    /// <returns>The print-formatted IBAN string.</returns>
    public string ToPrintFormat() => IbanFormatter.ToPrintFormat(Value);

    /// <summary>
    /// Formats the IBAN in a masked representation to protect sensitive account details.
    /// </summary>
    /// <param name="maskChar">The character used for masking. Default is <c>'*'</c>.</param>
    /// <param name="visiblePrefixLength">The count of characters remaining visible at the start. Default is 4.</param>
    /// <param name="visibleSuffixLength">The count of characters remaining visible at the end. Default is 4.</param>
    /// <returns>A masked IBAN string, or an empty string if this instance is empty.</returns>
    public string ToMaskedFormat(char maskChar = '*', int visiblePrefixLength = 4, int visibleSuffixLength = 4)
    {
        if (IsEmpty)
            return string.Empty;

        return IbanFormatter.ToMaskedFormat(Value, maskChar, visiblePrefixLength, visibleSuffixLength);
    }

    /// <summary>
    /// Attempts to format the IBAN in a masked representation into the specified character span.
    /// </summary>
    /// <param name="destination">The destination buffer.</param>
    /// <param name="charsWritten">When this method returns, contains the number of characters written.</param>
    /// <param name="maskChar">The character used for masking. Default is <c>'*'</c>.</param>
    /// <param name="visiblePrefixLength">The count of characters remaining visible at the start. Default is 4.</param>
    /// <param name="visibleSuffixLength">The count of characters remaining visible at the end. Default is 4.</param>
    /// <returns><see langword="true"/> if formatting succeeded; otherwise, <see langword="false"/>.</returns>
    public bool TryToMaskedFormat(
        Span<char> destination,
        out int charsWritten,
        char maskChar = '*',
        int visiblePrefixLength = 4,
        int visibleSuffixLength = 4)
    {
        if (IsEmpty)
        {
            charsWritten = 0;
            return true;
        }

        return IbanFormatter.TryToMaskedFormat(
            Value.AsSpan(),
            destination,
            out charsWritten,
            maskChar,
            visiblePrefixLength,
            visibleSuffixLength);
    }

    /// <summary>
    /// Formats the value of the current IBAN instance using the specified format specifier.
    /// </summary>
    /// <param name="format">
    /// The format specifier to use:
    /// <list type="bullet">
    /// <item><description><c>"G"</c>, <c>"N"</c>, or <c>null</c>/empty: Normalized compact format (e.g., <c>"IR020120..."</c>).</description></item>
    /// <item><description><c>"P"</c>: Print / Presentation format grouped in 4-character blocks (e.g., <c>"IR02 0120..."</c>).</description></item>
    /// <item><description><c>"M"</c>: Masked representation protecting sensitive digits (e.g., <c>"IR02****************6819"</c>).</description></item>
    /// </list>
    /// </param>
    /// <param name="formatProvider">An optional format provider (currently unused, adheres to <see cref="IFormattable"/>).</param>
    /// <returns>The formatted IBAN string.</returns>
    /// <exception cref="FormatException">Thrown when an unsupported format specifier is passed.</exception>
    public string ToString(string? format, IFormatProvider? formatProvider = null)
    {
        if (IsEmpty)
            return string.Empty;

        // Support null/empty, "G" (General), and "N" (Normalized) as the compact standard representation
        if (string.IsNullOrEmpty(format) ||
            format.Equals("G", StringComparison.OrdinalIgnoreCase) ||
            format.Equals("N", StringComparison.OrdinalIgnoreCase))
        {
            return Value;
        }

        // Support "P" (Print/Presentation) with 4-character block groupings
        if (format.Equals("P", StringComparison.OrdinalIgnoreCase))
        {
            return ToPrintFormat();
        }

        // Support "M" (Masked) for obfuscated display
        if (format.Equals("M", StringComparison.OrdinalIgnoreCase))
        {
            return ToMaskedFormat();
        }

        throw new FormatException($"The format string '{format}' is not supported. Supported formats are 'G', 'N', 'P', and 'M'.");
    }


    /// <summary>
    /// Returns the normalized string representation of this IBAN.
    /// </summary>
    /// <returns>The compact IBAN string.</returns>
    public override string ToString() => ToString("G", null);

    /// <summary>
    /// Compares this instance with another <see cref="Iban"/> instance ordinally.
    /// </summary>
    /// <param name="other">The other <see cref="Iban"/> instance to compare with.</param>
    /// <returns>A value indicating the relative order of the instances.</returns>
    public int CompareTo(Iban other) => string.Compare(Value, other.Value, StringComparison.Ordinal);
}

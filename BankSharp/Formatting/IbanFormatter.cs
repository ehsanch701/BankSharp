using System.Buffers;
using System.Runtime.CompilerServices;

namespace BankSharp.Formatting;

/// <summary>
/// Provides high-performance, zero/low-allocation formatting, normalization, and masking utilities for IBAN strings.
/// </summary>
public static class IbanFormatter
{
    private const int MaxIbanLength = 34;
    private const int MaxPrintFormatLength = 42;
    private const int MaxAllowedInputLength = 128; // Safe ceiling to mitigate StackOverflow and DoS attacks

    private static readonly SearchValues<char> Separators = SearchValues.Create(" -");

    #region Normalize

    /// <summary>
    /// Normalizes the specified IBAN character span by removing spaces/hyphens and converting all letters to uppercase.
    /// </summary>
    /// <param name="iban">The raw IBAN characters to normalize.</param>
    /// <returns>A normalized, uppercase IBAN string without whitespace or hyphens, or an empty string if invalid or exceeding length limits.</returns>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static string Normalize(ReadOnlySpan<char> iban)
    {
        if (iban.Length > MaxAllowedInputLength) return string.Empty;

        Span<char> buffer = stackalloc char[MaxIbanLength];
        return TryNormalize(iban, buffer, out int charsWritten)
            ? new string(buffer[..charsWritten])
            : string.Empty;
    }

    /// <summary>
    /// Normalizes the specified IBAN string by removing spaces/hyphens and converting all letters to uppercase.
    /// </summary>
    /// <param name="iban">The raw IBAN string to normalize.</param>
    /// <returns>A normalized, uppercase IBAN string, or an empty string if <paramref name="iban"/> is <see langword="null"/> or invalid.</returns>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static string Normalize(string? iban) => iban is null ? string.Empty : Normalize(iban.AsSpan());

    /// <summary>
    /// Attempts to normalize the raw IBAN into the provided destination span without heap allocations.
    /// </summary>
    /// <param name="source">The source IBAN character span.</param>
    /// <param name="destination">The destination buffer where the normalized IBAN characters will be written.</param>
    /// <param name="charsWritten">When this method returns, contains the number of characters written into <paramref name="destination"/>.</param>
    /// <returns><see langword="true"/> if the normalization succeeded and fit within the destination span; otherwise, <see langword="false"/>.</returns>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static bool TryNormalize(ReadOnlySpan<char> source, Span<char> destination, out int charsWritten)
    {
        charsWritten = 0;
        if (source.Length > MaxAllowedInputLength) return false;

        ReadOnlySpan<char> trimmed = source.Trim();
        if (trimmed.IsEmpty || destination.Length < MaxIbanLength) return false;

        int written = 0;
        foreach (char c in trimmed)
        {
            if (Separators.Contains(c)) continue;

            if (written >= MaxIbanLength) return false;

            destination[written++] = char.ToUpperInvariant(c);
        }

        charsWritten = written;
        return true;
    }

    #endregion

    #region Print Format

    /// <summary>
    /// Formats an IBAN character span into human-readable electronic print format (groups of 4 characters separated by spaces).
    /// </summary>
    /// <param name="iban">The raw or normalized IBAN character span.</param>
    /// <returns>A print-formatted IBAN string (e.g., <c>"IR12 3456 7890 ..."</c>), or an empty string if invalid.</returns>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static string ToPrintFormat(ReadOnlySpan<char> iban)
    {
        if (iban.Length > MaxAllowedInputLength) return string.Empty;

        Span<char> buffer = stackalloc char[MaxPrintFormatLength];
        return TryToPrintFormat(iban, buffer, out int charsWritten)
            ? new string(buffer[..charsWritten])
            : string.Empty;
    }

    /// <summary>
    /// Formats an IBAN string into human-readable electronic print format (groups of 4 characters separated by spaces).
    /// </summary>
    /// <param name="iban">The raw or normalized IBAN string.</param>
    /// <returns>A print-formatted IBAN string, or an empty string if <paramref name="iban"/> is <see langword="null"/> or invalid.</returns>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static string ToPrintFormat(string? iban) => iban is null ? string.Empty : ToPrintFormat(iban.AsSpan());

    /// <summary>
    /// Attempts to format an IBAN into human-readable print format in the destination buffer without heap allocations.
    /// </summary>
    /// <param name="source">The source IBAN character span.</param>
    /// <param name="destination">The destination buffer where the formatted characters will be written.</param>
    /// <param name="charsWritten">When this method returns, contains the count of characters written to <paramref name="destination"/>.</param>
    /// <returns><see langword="true"/> if the formatting was successful and fit in the destination; otherwise, <see langword="false"/>.</returns>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static bool TryToPrintFormat(ReadOnlySpan<char> source, Span<char> destination, out int charsWritten)
    {
        charsWritten = 0;
        if (source.Length > MaxAllowedInputLength) return false;

        Span<char> normalized = stackalloc char[MaxIbanLength];
        if (!TryNormalize(source, normalized, out int normalizedLength)) return false;

        int spaces = (normalizedLength - 1) / 4;
        if (destination.Length < normalizedLength + spaces) return false;

        int destIndex = 0;
        for (int i = 0; i < normalizedLength; i++)
        {
            if (i > 0 && i % 4 == 0) destination[destIndex++] = ' ';
            destination[destIndex++] = normalized[i];
        }

        charsWritten = destIndex;
        return true;
    }

    #endregion

    #region Masked Format

    /// <summary>
    /// Masks the middle portion of an IBAN for security and privacy display purposes.
    /// </summary>
    /// <param name="iban">The raw or normalized IBAN character span.</param>
    /// <param name="maskChar">The masking character to replace sensitive characters with. Defaults to <c>'*'</c>.</param>
    /// <param name="visiblePrefixLength">The number of characters at the beginning to keep unmasked. Defaults to <c>4</c>.</param>
    /// <param name="visibleSuffixLength">The number of characters at the end to keep unmasked. Defaults to <c>4</c>.</param>
    /// <returns>A masked IBAN string (e.g., <c>"IR12****************1234"</c>), or an empty string if invalid.</returns>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static string ToMaskedFormat(
        ReadOnlySpan<char> iban,
        char maskChar = '*',
        int visiblePrefixLength = 4,
        int visibleSuffixLength = 4)
    {
        if (iban.Length > MaxAllowedInputLength) return string.Empty;

        Span<char> buffer = stackalloc char[MaxIbanLength];
        return TryToMaskedFormat(iban, buffer, out int charsWritten, maskChar, visiblePrefixLength, visibleSuffixLength)
            ? new string(buffer[..charsWritten])
            : string.Empty;
    }

    /// <summary>
    /// Masks the middle portion of an IBAN string for security and privacy display purposes.
    /// </summary>
    /// <param name="iban">The raw or normalized IBAN string.</param>
    /// <param name="maskChar">The masking character to replace sensitive characters with. Defaults to <c>'*'</c>.</param>
    /// <param name="visiblePrefixLength">The number of characters at the beginning to keep unmasked. Defaults to <c>4</c>.</param>
    /// <param name="visibleSuffixLength">The number of characters at the end to keep unmasked. Defaults to <c>4</c>.</param>
    /// <returns>A masked IBAN string, or an empty string if <paramref name="iban"/> is <see langword="null"/> or invalid.</returns>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static string ToMaskedFormat(
        string? iban,
        char maskChar = '*',
        int visiblePrefixLength = 4,
        int visibleSuffixLength = 4) =>
        iban is null ? string.Empty : ToMaskedFormat(iban.AsSpan(), maskChar, visiblePrefixLength, visibleSuffixLength);

    /// <summary>
    /// Attempts to write a masked IBAN directly into the destination buffer without heap allocations.
    /// </summary>
    /// <param name="source">The source IBAN character span.</param>
    /// <param name="destination">The destination buffer where the masked IBAN will be written.</param>
    /// <param name="charsWritten">When this method returns, contains the number of characters written into <paramref name="destination"/>.</param>
    /// <param name="maskChar">The masking character. Defaults to <c>'*'</c>.</param>
    /// <param name="visiblePrefixLength">The count of unmasked leading characters. Defaults to <c>4</c>.</param>
    /// <param name="visibleSuffixLength">The count of unmasked trailing characters. Defaults to <c>4</c>.</param>
    /// <returns><see langword="true"/> if the masking succeeded and fit in the destination; otherwise, <see langword="false"/>.</returns>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static bool TryToMaskedFormat(
        ReadOnlySpan<char> source,
        Span<char> destination,
        out int charsWritten,
        char maskChar = '*',
        int visiblePrefixLength = 4,
        int visibleSuffixLength = 4)
    {
        charsWritten = 0;
        if (source.Length > MaxAllowedInputLength) return false;

        Span<char> normalized = stackalloc char[MaxIbanLength];
        if (!TryNormalize(source, normalized, out int normalizedLength)) return false;

        if (destination.Length < normalizedLength) return false;

        // Clamping logic
        visiblePrefixLength = Math.Max(0, visiblePrefixLength);
        visibleSuffixLength = Math.Max(0, visibleSuffixLength);

        if (visiblePrefixLength + visibleSuffixLength >= normalizedLength)
        {
            visiblePrefixLength = Math.Min(2, normalizedLength);
            visibleSuffixLength = 0;
        }

        int maskStart = visiblePrefixLength;
        int maskEnd = normalizedLength - visibleSuffixLength;

        // Copy and Fill
        if (visiblePrefixLength > 0) normalized[..visiblePrefixLength].CopyTo(destination[..visiblePrefixLength]);
        destination[maskStart..maskEnd].Fill(maskChar);
        if (visibleSuffixLength > 0) normalized[maskEnd..normalizedLength].CopyTo(destination[maskEnd..normalizedLength]);

        charsWritten = normalizedLength;
        return true;
    }

    #endregion

    #region Formatting API

    /// <summary>
    /// Formats the given IBAN using standard format specifiers:
    /// <list type="bullet">
    /// <item><description><c>"G"</c> or <c>"N"</c>: Normalized format (compact, uppercase, no spaces).</description></item>
    /// <item><description><c>"P"</c>: Print format (chunked in 4-character blocks).</description></item>
    /// <item><description><c>"M"</c>: Masked format (privacy-protected).</description></item>
    /// </list>
    /// </summary>
    /// <param name="iban">The raw IBAN characters.</param>
    /// <param name="format">The format specifier span (<c>"G"</c>, <c>"N"</c>, <c>"P"</c>, or <c>"M"</c>). Defaults to normalized.</param>
    /// <returns>The formatted IBAN string.</returns>
    /// <exception cref="FormatException">Thrown when an unsupported format specifier is provided.</exception>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static string Format(ReadOnlySpan<char> iban, ReadOnlySpan<char> format = default)
    {
        return format switch
        {
            _ when format.IsEmpty || format.Equals("G", StringComparison.OrdinalIgnoreCase) || format.Equals("N", StringComparison.OrdinalIgnoreCase)
                => Normalize(iban),
            _ when format.Equals("P", StringComparison.OrdinalIgnoreCase)
                => ToPrintFormat(iban),
            _ when format.Equals("M", StringComparison.OrdinalIgnoreCase)
                => ToMaskedFormat(iban),
            _ => throw new FormatException($"Unsupported format '{format.ToString()}'.")
        };
    }

    /// <summary>
    /// Attempts to format the IBAN into the destination buffer using the specified format specifier without heap allocations.
    /// </summary>
    /// <param name="source">The source IBAN character span.</param>
    /// <param name="destination">The destination span where formatted output is written.</param>
    /// <param name="charsWritten">When this method returns, contains the count of characters written.</param>
    /// <param name="format">The format specifier span (<c>"G"</c>, <c>"N"</c>, <c>"P"</c>, or <c>"M"</c>).</param>
    /// <returns><see langword="true"/> if formatting succeeded and fit inside destination; otherwise, <see langword="false"/>.</returns>
    /// <exception cref="FormatException">Thrown when an unsupported format specifier is provided.</exception>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static bool TryFormat(ReadOnlySpan<char> source, Span<char> destination, out int charsWritten, ReadOnlySpan<char> format = default)
    {
        // Default to G/N if empty
        if (format.IsEmpty) format = "G";

        if (format.Equals("G", StringComparison.OrdinalIgnoreCase) || format.Equals("N", StringComparison.OrdinalIgnoreCase))
            return TryNormalize(source, destination, out charsWritten);

        if (format.Equals("P", StringComparison.OrdinalIgnoreCase))
            return TryToPrintFormat(source, destination, out charsWritten);

        if (format.Equals("M", StringComparison.OrdinalIgnoreCase))
            return TryToMaskedFormat(source, destination, out charsWritten);

        throw new FormatException($"Unsupported format '{format.ToString()}'.");
    }

    #endregion
}

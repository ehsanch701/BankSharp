using BankSharp.Formatting;

namespace BankSharp.Tests;

/// <summary>
/// Provides comprehensive unit tests for the <see cref="IbanFormatter"/> class.
/// Covers normalization, print/presentation formatting, masking logic, and standard specifier validation.
/// </summary>
public class IbanFormatterTests
{
    #region Normalize Tests

    /// <summary>
    /// Verifies that normalization correctly removes common separators (spaces, dashes) 
    /// and ensures the resulting string is in uppercase.
    /// </summary>
    /// <param name="input">The raw IBAN input string.</param>
    /// <param name="expected">The expected normalized IBAN string.</param>
    [Theory]
    [InlineData("IR02 0120 0000 0000 1083 7583 62", "IR020120000000001083758362")]
    [InlineData("ir02-0120-0000-0000-1083-7583-62", "IR020120000000001083758362")]
    [InlineData(" DE89370400440532013000 ", "DE89370400440532013000")]
    public void Normalize_CleansSpacesAndConvertsToUpper(string input, string expected)
    {
        // Span overload
        string actualSpan = IbanFormatter.Normalize(input.AsSpan());
        Assert.Equal(expected, actualSpan);

        // String overload
        string actualString = IbanFormatter.Normalize(input);
        Assert.Equal(expected, actualString);
    }

    /// <summary>
    /// Ensures that null, empty, or whitespace-only inputs result in an empty string 
    /// without throwing exceptions.
    /// </summary>
    /// <param name="input">The invalid or empty input.</param>
    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("    ")]
    public void Normalize_NullOrWhitespace_ReturnsEmptyString(string? input)
    {
        Assert.Equal(string.Empty, IbanFormatter.Normalize(input));
        Assert.Equal(string.Empty, IbanFormatter.Normalize(input.AsSpan()));
    }

    /// <summary>
    /// Tests that the formatter rejects inputs exceeding the maximum permissible length 
    /// by returning an empty string.
    /// </summary>
    [Fact]
    public void Normalize_InputExceedsMaxAllowedLength_ReturnsEmptyString()
    {
        string longInput = new('A', 129);
        Assert.Equal(string.Empty, IbanFormatter.Normalize(longInput));
    }

    /// <summary>
    /// Checks that <see cref="IbanFormatter.TryNormalize"/> correctly signals failure 
    /// when the destination buffer is insufficient.
    /// </summary>
    [Fact]
    public void TryNormalize_DestinationTooSmall_ReturnsFalse()
    {
        const string input = "IR02 0120 0000 0000 1083 7583 62";
        Span<char> smallBuffer = stackalloc char[10];

        bool success = IbanFormatter.TryNormalize(input.AsSpan(), smallBuffer, out int charsWritten);

        Assert.False(success);
        Assert.Equal(0, charsWritten);
    }

    #endregion

    #region Print Format Tests

    /// <summary>
    /// Validates the IBAN 'Print' format, which organizes the characters into human-readable 
    /// 4-character blocks.
    /// </summary>
    /// <param name="input">The unformatted or raw IBAN string.</param>
    /// <param name="expected">The expected human-readable format string.</param>
    [Theory]
    [InlineData("GB82WEST12345698765432", "GB82 WEST 1234 5698 7654 32")]
    [InlineData("IR020120000000001083758362", "IR02 0120 0000 0000 1083 7583 62")]
    [InlineData("ir02-0120-0000-0000-1083-7583-62", "IR02 0120 0000 0000 1083 7583 62")]
    public void ToPrintFormat_SplitsIntoFourCharBlocks(string input, string expected)
    {
        // Span overload
        string actualSpan = IbanFormatter.ToPrintFormat(input.AsSpan());
        Assert.Equal(expected, actualSpan);

        // String overload
        string actualString = IbanFormatter.ToPrintFormat(input);
        Assert.Equal(expected, actualString);
    }

    /// <summary>
    /// Ensures that <see cref="IbanFormatter.TryToPrintFormat"/> gracefully handles 
    /// buffer overflows by returning false.
    /// </summary>
    [Fact]
    public void TryToPrintFormat_DestinationBufferTooSmall_ReturnsFalse()
    {
        const string input = "GB82WEST12345698765432";
        Span<char> smallBuffer = stackalloc char[10];

        bool success = IbanFormatter.TryToPrintFormat(input.AsSpan(), smallBuffer, out int charsWritten);

        Assert.False(success);
        Assert.Equal(0, charsWritten);
    }

    #endregion

    #region Masked Format Tests

    /// <summary>
    /// Validates the masking logic, ensuring default parameters effectively 
    /// hide the middle section of the IBAN.
    /// </summary>
    /// <param name="input">The IBAN string to mask.</param>
    /// <param name="expected">The expected masked representation.</param>
    [Theory]
    [InlineData("IR020120000000001083758362", "IR02******************8362")]
    [InlineData("ir02 0120-0000-0000-1083-7583-62", "IR02******************8362")]
    [InlineData("DE89370400440532013000", "DE89**************3000")]
    [InlineData("GB82WEST12345698765432", "GB82**************5432")]
    public void ToMaskedFormat_DefaultParameters_MasksMiddleCorrectly(string input, string expected)
    {
        // Span overload
        string resultSpan = IbanFormatter.ToMaskedFormat(input.AsSpan());
        Assert.Equal(expected, resultSpan);

        // String overload
        string resultString = IbanFormatter.ToMaskedFormat(input);
        Assert.Equal(expected, resultString);
    }

    /// <summary>
    /// Ensures that custom mask characters and explicit prefix/suffix lengths 
    /// are respected during masking.
    /// </summary>
    [Fact]
    public void ToMaskedFormat_CustomMaskCharAndLengths_MasksAccurately()
    {
        const string iban = "DE89370400440532013000";

        string result = IbanFormatter.ToMaskedFormat(iban, maskChar: '#', visiblePrefixLength: 2, visibleSuffixLength: 3);

        Assert.Equal("DE#################000", result);
    }

    /// <summary>
    /// Confirms that negative lengths for prefix or suffix are treated as zero.
    /// </summary>
    [Theory]
    [InlineData(-3, 4, "******************3000")]
    [InlineData(4, -2, "DE89******************")]
    public void ToMaskedFormat_NegativeLengths_ClampedToZero(int prefixLen, int suffixLen, string expected)
    {
        const string iban = "DE89370400440532013000";

        string result = IbanFormatter.ToMaskedFormat(iban, visiblePrefixLength: prefixLen, visibleSuffixLength: suffixLen);

        Assert.Equal(expected, result);
    }

    /// <summary>
    /// Verifies behavior when the requested visible characters exceed the total string length;
    /// it should fall back to a safe default.
    /// </summary>
    [Fact]
    public void ToMaskedFormat_VisibleLengthsExceedTotalLength_FallsBackToCountryCodeOnly()
    {
        const string iban = "DE89370400440532013000";

        string result = IbanFormatter.ToMaskedFormat(iban, visiblePrefixLength: 15, visibleSuffixLength: 10);

        Assert.Equal("DE********************", result);
    }

    /// <summary>
    /// Ensures null/empty inputs return an empty string for masking.
    /// </summary>
    /// <param name="input">The invalid/empty input.</param>
    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("   ")]
    public void ToMaskedFormat_NullOrWhitespace_ReturnsEmptyString(string? input)
    {
        Assert.Equal(string.Empty, IbanFormatter.ToMaskedFormat(input));
        Assert.Equal(string.Empty, IbanFormatter.ToMaskedFormat(input.AsSpan()));
    }

    /// <summary>
    /// Validates that <see cref="IbanFormatter.TryToMaskedFormat"/> handles small buffers correctly.
    /// </summary>
    [Fact]
    public void TryToMaskedFormat_DestinationBufferTooSmall_ReturnsFalse()
    {
        const string iban = "DE89370400440532013000";
        Span<char> smallBuffer = stackalloc char[10];

        bool success = IbanFormatter.TryToMaskedFormat(iban.AsSpan(), smallBuffer, out int charsWritten);

        Assert.False(success);
        Assert.Equal(0, charsWritten);
    }

    /// <summary>
    /// Validates that <see cref="IbanFormatter.TryToMaskedFormat"/> succeeds 
    /// and writes correct output to valid buffers.
    /// </summary>
    [Fact]
    public void TryToMaskedFormat_ValidBuffer_WritesMaskedIbanWithoutAllocation()
    {
        const string iban = "DE89370400440532013000";
        Span<char> buffer = stackalloc char[34];

        bool success = IbanFormatter.TryToMaskedFormat(iban.AsSpan(), buffer, out int charsWritten);

        Assert.True(success);
        Assert.Equal(22, charsWritten);
        Assert.Equal("DE89**************3000", new string(buffer[..charsWritten]));
    }

    #endregion

    #region Format and TryFormat Specifier Tests

    /// <summary>
    /// Verifies support for the custom "M" (Masked) format specifier.
    /// </summary>
    /// <param name="format">The format specifier.</param>
    [Theory]
    [InlineData("M")]
    [InlineData("m")]
    public void Format_MaskedSpecifier_ReturnsMaskedFormat(string format)
    {
        const string iban = "DE89370400440532013000";

        string result = IbanFormatter.Format(iban.AsSpan(), format.AsSpan());

        Assert.Equal("DE89**************3000", result);
    }

    /// <summary>
    /// Validates standard format specifiers (Printable, General, Normalized) 
    /// against expected outputs.
    /// </summary>
    /// <param name="format">The format specifier.</param>
    /// <param name="expected">The expected output.</param>
    [Theory]
    [InlineData("P", "DE89 3704 0044 0532 0130 00")]
    [InlineData("G", "DE89370400440532013000")]
    [InlineData("N", "DE89370400440532013000")]
    [InlineData("", "DE89370400440532013000")]
    public void Format_StandardSpecifiers_ReturnExpectedResult(string format, string expected)
    {
        const string iban = "de89-3704 0044 0532 0130 00";

        string result = IbanFormatter.Format(iban.AsSpan(), format.AsSpan());

        Assert.Equal(expected, result);
    }

    /// <summary>
    /// Confirms that using unsupported format specifiers throws a <see cref="FormatException"/>.
    /// </summary>
    [Fact]
    public void Format_InvalidSpecifier_ThrowsFormatException()
    {
        const string iban = "DE89370400440532013000";

        Assert.Throws<FormatException>(() => IbanFormatter.Format(iban.AsSpan(), "INVALID".AsSpan()));
    }

    /// <summary>
    /// Checks that <see cref="IbanFormatter.TryFormat"/> correctly processes the "M" 
    /// specifier into a provided buffer.
    /// </summary>
    [Fact]
    public void TryFormat_MaskedSpecifier_WritesToDestination()
    {
        const string iban = "DE89370400440532013000";
        Span<char> buffer = stackalloc char[34];

        bool success = IbanFormatter.TryFormat(iban.AsSpan(), buffer, out int charsWritten, "M".AsSpan());

        Assert.True(success);
        Assert.Equal(22, charsWritten);
        Assert.Equal("DE89**************3000", new string(buffer[..charsWritten]));
    }

    #endregion
}

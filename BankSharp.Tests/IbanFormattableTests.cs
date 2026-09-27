using System;
using BankSharp.Formatting;
using Xunit;

namespace BankSharp.Tests;

/// <summary>
/// Contains unit tests verifying the formatting capabilities and <see cref="IFormattable"/>
/// implementation of the <see cref="Iban"/> struct.
/// </summary>
public class IbanFormattableTests
{
    private const string RawIban = "IR02 0120 0000 0000 1083 7583 62";
    private const string NormalizedIban = "IR020120000000001083758362";
    private const string PrintIban = "IR02 0120 0000 0000 1083 7583 62";

    /// <summary>
    /// Verifies that default formatting or standard format specifiers ("G", "N", and null/empty)
    /// produce the normalized compact IBAN string regardless of casing.
    /// </summary>
    [Fact]
    public void ToString_DefaultAndGAndN_ShouldReturnNormalizedString()
    {
        var iban = Iban.Parse(RawIban);

        Assert.Equal(NormalizedIban, iban.ToString());
        Assert.Equal(NormalizedIban, iban.ToString("G", null));
        Assert.Equal(NormalizedIban, iban.ToString("g", null));
        Assert.Equal(NormalizedIban, iban.ToString("N", null));
        Assert.Equal(NormalizedIban, iban.ToString("n", null));
        Assert.Equal(NormalizedIban, iban.ToString(null, null));
    }

    /// <summary>
    /// Verifies that the "P" (Print/Presentation) format specifier returns
    /// the human-readable, 4-character block segmented IBAN string.
    /// </summary>
    [Fact]
    public void ToString_P_ShouldReturnPrintFormattedString()
    {
        var iban = Iban.Parse(NormalizedIban);

        Assert.Equal(PrintIban, iban.ToString("P", null));
        Assert.Equal(PrintIban, iban.ToString("p", null));
    }

    /// <summary>
    /// Verifies that formatting an empty or uninitialized <see cref="Iban"/> instance
    /// returns an empty string across supported format specifiers.
    /// </summary>
    [Fact]
    public void ToString_EmptyIban_ShouldReturnEmptyString()
    {
        var iban = Iban.Empty;

        Assert.Equal(string.Empty, iban.ToString("G", null));
        Assert.Equal(string.Empty, iban.ToString("P", null));
    }

    /// <summary>
    /// Verifies that passing an unknown or unsupported format specifier
    /// throws a <see cref="FormatException"/>.
    /// </summary>
    [Fact]
    public void ToString_UnsupportedFormat_ShouldThrowFormatException()
    {
        var iban = Iban.Parse(NormalizedIban);

        // Note: Make sure your Iban.ToString implementation supports 'N' now
        Assert.Throws<FormatException>(() => iban.ToString("INVALID", null));
        Assert.Throws<FormatException>(() => iban.ToString("X", null));
    }

    /// <summary>
    /// Verifies that string interpolation correctly triggers the <see cref="IFormattable"/>
    /// implementation with given format specifiers (e.g. {iban:G} and {iban:P}).
    /// </summary>
    [Fact]
    public void StringInterpolation_WithFormatSpecifiers_ShouldFormatCorrectly()
    {
        var iban = Iban.Parse(RawIban);

        Assert.Equal($"IBAN: {NormalizedIban}", $"IBAN: {iban:G}");
        Assert.Equal($"IBAN: {PrintIban}", $"IBAN: {iban:P}");
    }

    /// <summary>
    /// Verifies that the internal normalization logic handles overly long inputs safely.
    /// </summary>
    [Fact]
    public void Normalize_WithExcessiveInputLength_ReturnsEmptyStringWithoutThrowing()
    {
        // Arrange
        string maliciousLongInput = new string('A', 500);

        // Act
        string result = IbanFormatter.Normalize(maliciousLongInput);

        // Assert
        Assert.Equal(string.Empty, result);
    }
}

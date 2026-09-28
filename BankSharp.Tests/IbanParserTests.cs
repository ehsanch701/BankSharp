using System;
using BankSharp.Parsing;
using Xunit;

namespace BankSharp.Tests;

/// <summary>
/// Provides unit tests for the <see cref="IbanParser"/> class.
/// Ensures that IBAN strings are correctly decomposed into their constituent components 
/// (Country Code, Bank Code, Branch Code, Account Number) using both standard and span-based APIs.
/// </summary>
public class IbanParserTests
{
    /// <summary>
    /// Validates that various valid IBAN strings are parsed into the correct structural components via <see cref="IbanParser.Parse"/>.
    /// </summary>
    /// <param name="iban">The input IBAN string.</param>
    /// <param name="expectedCountry">The expected country code.</param>
    /// <param name="expectedBank">The expected bank identifier code.</param>
    /// <param name="expectedBranch">The expected branch code (if applicable).</param>
    /// <param name="expectedAccount">The expected account number segment.</param>
    [Theory]
    [InlineData("IR120120000000012345678901", "IR", "012", null, "0000000012345678901")]
    [InlineData("DE89370400440532013000", "DE", "37040044", null, "0532013000")]
    public void Parse_ValidIbans_ReturnsCorrectStructure(
        string iban,
        string expectedCountry,
        string expectedBank,
        string? expectedBranch,
        string expectedAccount)
    {
        // Act
        var result = IbanParser.Parse(iban);

        // Assert
        Assert.Equal(expectedCountry, result.CountryCode);
        Assert.Equal(expectedBank, result.BankCode);
        Assert.Equal(expectedBranch, result.BranchCode);
        Assert.Equal(expectedAccount, result.AccountNumber);
    }

    /// <summary>
    /// Validates that various valid IBAN spans are parsed into the correct structural components via <see cref="IbanParser.ParseSpan"/>.
    /// </summary>
    [Theory]
    [InlineData("IR120120000000012345678901", "IR", "012", "", "0000000012345678901")]
    [InlineData("DE89370400440532013000", "DE", "37040044", "", "0532013000")]
    public void ParseSpan_ValidIbans_ReturnsCorrectStructure(
        string iban,
        string expectedCountry,
        string expectedBank,
        string expectedBranch,
        string expectedAccount)
    {
        // Act
        var result = IbanParser.ParseSpan(iban.AsSpan());

        // Assert
        Assert.True(result.CountryCode.SequenceEqual(expectedCountry.AsSpan()));
        Assert.True(result.BankCode.SequenceEqual(expectedBank.AsSpan()));
        Assert.True(result.BranchCode.SequenceEqual(expectedBranch.AsSpan()));
        Assert.True(result.AccountNumber.SequenceEqual(expectedAccount.AsSpan()));
    }

    /// <summary>
    /// Validates that <see cref="IbanParser.TryParseSpan"/> succeeds and extracts correct components for valid IBAN spans.
    /// </summary>
    [Fact]
    public void TryParseSpan_ValidIban_ReturnsTrueAndDecomposes()
    {
        // Arrange
        const string iban = "IR120120000000012345678901";

        // Act
        bool success = IbanParser.TryParseSpan(iban.AsSpan(), out var result);

        // Assert
        Assert.True(success);
        Assert.True(result.CountryCode.SequenceEqual("IR".AsSpan()));
        Assert.True(result.BankCode.SequenceEqual("012".AsSpan()));
        Assert.True(result.BranchCode.IsEmpty);
        Assert.True(result.AccountNumber.SequenceEqual("0000000012345678901".AsSpan()));
    }

    /// <summary>
    /// Verifies that <see cref="IbanParser.TryParse"/> returns false when 
    /// provided with an invalid or malformed IBAN string.
    /// </summary>
    [Fact]
    public void TryParse_InvalidIban_ReturnsFalse()
    {
        bool success = IbanParser.TryParse("INVALID_IBAN", out _);
        Assert.False(success);
    }

    /// <summary>
    /// Verifies that <see cref="IbanParser.TryParseSpan"/> returns false when 
    /// provided with an invalid or malformed IBAN span.
    /// </summary>
    [Fact]
    public void TryParseSpan_InvalidIban_ReturnsFalse()
    {
        bool success = IbanParser.TryParseSpan("INVALID_IBAN".AsSpan(), out var result);
        Assert.False(success);
        Assert.True(result.CountryCode.IsEmpty);
    }

    /// <summary>
    /// Ensures that the <see cref="IbanParser.Parse"/> method throws an <see cref="ArgumentException"/> 
    /// when the input string is too short to be a valid IBAN.
    /// </summary>
    [Fact]
    public void Parse_TooShortIban_ThrowsArgumentException()
    {
        Assert.Throws<ArgumentException>(() => IbanParser.Parse("IR1"));
    }

    /// <summary>
    /// Ensures that the <see cref="IbanParser.ParseSpan"/> method throws an <see cref="ArgumentException"/> 
    /// when the input span is too short to be a valid IBAN.
    /// </summary>
    [Fact]
    public void ParseSpan_TooShortIban_ThrowsArgumentException()
    {
        Assert.Throws<ArgumentException>(() => IbanParser.ParseSpan("IR1".AsSpan()));
    }
}

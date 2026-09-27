using BankSharp.Parsing;

namespace BankSharp.Tests;

/// <summary>
/// Provides unit tests for the <see cref="IbanParser"/> class.
/// Ensures that IBAN strings are correctly broken down into their constituent components 
/// (Country Code, Bank Code, Branch Code, Account Number).
/// </summary>
public class IbanParserTests
{
    /// <summary>
    /// Validates that various valid IBAN strings are parsed into the correct structural components.
    /// </summary>
    /// <param name="iban">The input IBAN string.</param>
    /// <param name="expectedCountry">The expected country code.</param>
    /// <param name="expectedBank">The expected bank identifier code.</param>
    /// <param name="expectedBranch">The expected branch code (if applicable).</param>
    /// <param name="expectedAccount">The expected account number segment.</param>
    [Theory]
    [InlineData("IR120120000000012345678901", "IR", "012", null, "0000000012345678901")]
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
    /// Ensures that the <see cref="IbanParser.Parse"/> method throws an <see cref="ArgumentException"/> 
    /// when the input string is too short to be a valid IBAN.
    /// </summary>
    [Fact]
    public void Parse_TooShortIban_ThrowsArgumentException()
    {
        Assert.Throws<ArgumentException>(() => IbanParser.Parse("IR1"));
    }
}

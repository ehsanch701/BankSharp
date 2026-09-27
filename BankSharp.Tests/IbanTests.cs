using BankSharp;
using Xunit;

namespace BankSharp.Tests;

/// <summary>
/// Contains unit tests for the <see cref="Iban"/> class, including parsing, 
/// deconstruction, and string normalization logic.
/// </summary>
public class IbanTests
{
    private const string ValidIranIban = "IR270170000000100324200001";
    private const string ValidGermanyIban = "DE89370400440532013000";
    private const string ValidUnitedKingdomIban = "GB29NWBK60161331926819";
    private const string ValidFranceIban = "FR1420041010050500013M02606";

    /// <summary>
    /// Validates that parsing a valid IBAN string correctly constructs the object state,
    /// extracting the country code and check digits accurately.
    /// </summary>
    /// <param name="rawIban">The raw IBAN string.</param>
    /// <param name="expectedCountry">The expected two-letter country code.</param>
    /// <param name="expectedCheckDigits">The expected two-digit check sum.</param>
    [Theory]
    [InlineData(ValidIranIban, "IR", "27")]
    [InlineData(ValidGermanyIban, "DE", "89")]
    [InlineData(ValidUnitedKingdomIban, "GB", "29")]
    [InlineData(ValidFranceIban, "FR", "14")]
    public void Parse_ValidIbans_ConstructsCorrectState(string rawIban, string expectedCountry, string expectedCheckDigits)
    {
        // Act
        var iban = Iban.Parse(rawIban);

        // Assert
        Assert.False(iban.IsEmpty);
        Assert.Equal(rawIban, iban.Value);
        Assert.Equal(expectedCountry, iban.CountryCode);
        Assert.Equal(expectedCheckDigits, iban.CheckDigits);
    }

    /// <summary>
    /// Ensures that invalid IBAN strings or malformed inputs trigger a <see cref="FormatException"/>.
    /// </summary>
    /// <param name="invalidIban">The malformed IBAN string.</param>
    [Theory]
    [InlineData("IR020170000000100324200001")] // Invalid Check Digits
    [InlineData("DE89370400440532013009")]     // Tampered Mod97
    [InlineData("INVALID_IBAN_STRING")]
    [InlineData("")]
    public void Parse_InvalidIbans_ThrowsFormatException(string invalidIban)
    {
        // Assert
        Assert.Throws<FormatException>(() => Iban.Parse(invalidIban));
    }

    /// <summary>
    /// Verifies that the parser correctly normalizes IBANs with spacing or separators,
    /// ensuring they match the expected canonical format.
    /// </summary>
    /// <param name="input">The raw, unformatted IBAN input.</param>
    /// <param name="expectedNormalized">The expected normalized IBAN string.</param>
    [Theory]
    [InlineData(" ir 27-0170 0000 0010 0324 2000 01 ", ValidIranIban)]
    [InlineData("de89 3704 0044 0532 0130 00", ValidGermanyIban)]
    public void TryParse_UnformattedValidString_NormalizesAndSucceeds(string input, string expectedNormalized)
    {
        // Act
        var success = Iban.TryParse(input, null, out var iban);

        // Assert
        Assert.True(success);
        Assert.Equal(expectedNormalized, iban.Value);
    }

    /// <summary>
    /// Tests the C# Deconstruction capability to ensure components (Country, Bank, Account) 
    /// are correctly extracted from an IBAN instance.
    /// </summary>
    [Fact]
    public void Deconstruct_ValidIban_ReturnsParsedComponents()
    {
        // Arrange
        const string rawIban = "IR270170000000100324200001";
        var iban = Iban.Parse(rawIban);

        // Act
        var (countryCode, checkDigits, bban) = iban;

        // Assert
        Assert.Equal("IR", countryCode);
        Assert.Equal("27", checkDigits);
        Assert.Equal("0170000000100324200001", bban);
    }

    /// <summary>
    /// Verifies that an empty IBAN instance represents a valid 'empty' state.
    /// </summary>
    [Fact]
    public void Empty_DefaultInstance_PropertiesMatchEmptyState()
    {
        // Arrange & Act
        var iban = Iban.Empty;

        // Assert
        Assert.True(iban.IsEmpty);
        Assert.Equal(string.Empty, iban.Value);
        Assert.Equal(string.Empty, iban.CountryCode);
        Assert.Equal(string.Empty, iban.CheckDigits);
    }

    /// <summary>
    /// Validates that two Iban instances with the same underlying value are considered equal.
    /// </summary>
    [Fact]
    public void Equality_SameValues_AreEqual()
    {
        // Arrange
        var iban1 = Iban.Parse(ValidIranIban);
        var iban2 = Iban.Parse(" ir 27-0170 0000 0010 0324 2000 01 ");

        // Assert
        Assert.Equal(iban1, iban2);
        Assert.True(iban1 == iban2);
    }
}

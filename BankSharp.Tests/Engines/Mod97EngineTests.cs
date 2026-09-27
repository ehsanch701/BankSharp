using BankSharp.Engines;
using Xunit;

namespace BankSharp.Tests.Engines;

/// <summary>
/// Contains unit tests for verifying ISO 7064 Mod 97-10 IBAN checksum computations in <see cref="Mod97Engine"/>.
/// </summary>
public sealed class Mod97EngineTests
{
    [Theory]
    // Iranian IBANs (Length: 26, format: IR + 2 check digits + 22 BBAN digits)
    [InlineData("IR000000000000000000000000", false)] // Invalid dummy check digits
    [InlineData("IR270170000000100324200001", true)]  // Valid National Bank of Iran (Melli)

    // UK IBANs (Length: 22, alphanumeric bank code + sort code + account number)
    [InlineData("GB29NWBK60161331926819", true)]      // Valid NatWest
    [InlineData("GB29NWBK60161331926818", false)]     // Invalid checksum digit

    // German IBANs (Length: 22, numeric BLZ + account number)
    [InlineData("DE89370400440532013000", true)]      // Valid Deutsche Bank
    [InlineData("DE89370400440532013001", false)]     // Invalid checksum digit

    // French IBANs (Length: 27, mixed alphanumeric BBAN)
    [InlineData("FR1420041010050500013M02606", true)]  // Valid French IBAN
    public void CalculateIbanMod97_ShouldValidateCorrectly(string iban, bool expectedIsValid)
    {
        // Act
        int remainder = Mod97Engine.CalculateIbanMod97(iban.AsSpan());

        // Assert: A remainder of 1 indicates a mathematically valid ISO 7064 check
        bool isValid = remainder == 1;
        Assert.Equal(expectedIsValid, isValid);
    }

    [Theory]
    [InlineData("")]
    [InlineData("I")]
    [InlineData("IR")]
    [InlineData("IR1")]
    public void CalculateIbanMod97_ShouldReturnMinusOne_WhenInputLengthIsLessThanFour(string shortIban)
    {
        // Act
        int remainder = Mod97Engine.CalculateIbanMod97(shortIban.AsSpan());

        // Assert: Inputs shorter than 4 characters cannot form a valid IBAN header and must return -1
        Assert.Equal(-1, remainder);
    }

    [Theory]
    [InlineData("GB29 NWBK 6016 1331 9268 19")] // Contains whitespace
    [InlineData("GB29-NWBK-6016-1331-9268-19")] // Contains hyphens
    [InlineData("GB29NWBK@0161331926819")]     // Contains invalid symbol '@'
    [InlineData("GB29NWBK_0161331926819")]     // Contains underscore
    [InlineData("GB29NWBK\t0161331926819")]    // Contains tab character
    public void CalculateIbanMod97_ShouldReturnMinusOne_WhenInputContainsIllegalCharacters(string invalidIban)
    {
        // Act
        int remainder = Mod97Engine.CalculateIbanMod97(invalidIban.AsSpan());

        // Assert: Engine must reject any non-alphanumeric ASCII character by returning -1
        Assert.Equal(-1, remainder);
    }

    [Fact]
    public void CalculateIbanMod97_ShouldBeCaseInsensitive()
    {
        // Arrange: Lowercase variant of a valid UK IBAN
        const string lowerIban = "gb29nwbk60161331926819";

        // Act
        int remainder = Mod97Engine.CalculateIbanMod97(lowerIban.AsSpan());

        // Assert: Letters must be normalized to uppercase values (a -> A -> 10)
        Assert.Equal(1, remainder);
    }

    [Fact]
    public void CalculateIbanMod97_ShouldHandleExactLengthFourWithInvalidModulo()
    {
        // Arrange: 4-character input is evaluated without BBAN payload
        const string headerOnly = "GB00";

        // Act
        int remainder = Mod97Engine.CalculateIbanMod97(headerOnly.AsSpan());

        // Assert: "GB00" -> 161100 % 97 = 85 != 1
        Assert.NotEqual(1, remainder);
    }
}

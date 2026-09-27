namespace BankSharp.Tests;

/// <summary>
/// Unit tests for <see cref="IbanValidator"/> verifying format, country codes, structure, and checksums.
/// </summary>
public class IbanValidatorTests
{
    [Theory]
    [InlineData("IR020120000000001083758362", "IR")] // Valid Iran IBAN
    [InlineData("DE89370400440532013000", "DE")]       // Valid Germany IBAN
    [InlineData("GB29NWBK60161331926819", "GB")]       // Valid UK IBAN
    [InlineData("FR1420041010050500013M02606", "FR")] // Valid France IBAN
    public void Validate_ValidIban_ShouldReturnSuccess(string iban, string expectedCountry)
    {
        // Act: Validate a properly formatted and valid IBAN
        var result = IbanValidator.Validate(iban);

        // Assert: Ensure validation passes with matching country code
        Assert.True(result.IsValid);
        Assert.Equal(IbanValidationError.None, result.Error);
        Assert.Equal(expectedCountry, result.CountryCode);
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("   ")]
    public void Validate_NullOrWhitespace_ShouldReturnEmptyOrNullError(string? iban)
    {
        // Act: Validate null, empty, or whitespace string
        var result = IbanValidator.Validate(iban);

        // Assert: Validation must fail with EmptyOrNull error
        Assert.False(result.IsValid);
        Assert.Equal(IbanValidationError.EmptyOrNull, result.Error);
    }

    [Theory]
    [InlineData("IR02")] // Less than minimum required length (at least 5 characters)
    public void Validate_TooShortIban_ShouldReturnInvalidLength(string iban)
    {
        // Act: Validate an IBAN string shorter than minimum requirement
        var result = IbanValidator.Validate(iban);

        // Assert: Validation must fail with InvalidLength error
        Assert.False(result.IsValid);
        Assert.Equal(IbanValidationError.InvalidLength, result.Error);
    }

    [Theory]
    [InlineData("12020120000000001083758362")] // Numeric characters instead of ISO country code letters
    [InlineData("1A020120000000001083758362")] // Alphanumeric mix in country code position
    public void Validate_InvalidCountryCode_ShouldReturnInvalidCountryCodeError(string iban)
    {
        // Act: Validate IBAN where the first two characters are not valid uppercase letters
        var result = IbanValidator.Validate(iban);

        // Assert: Validation must fail with InvalidCountryCode error
        Assert.False(result.IsValid);
        Assert.Equal(IbanValidationError.InvalidCountryCode, result.Error);
    }

    [Theory]
    [InlineData("#$020120000000001083758362")] // Special symbols in country code position
    [InlineData("IR02012000@000001083758362")] // Special symbol '@' within the BBAN part
    public void Validate_InvalidCharacters_ShouldReturnInvalidCharactersError(string iban)
    {
        // Act: Validate IBAN containing illegal/special non-alphanumeric characters
        var result = IbanValidator.Validate(iban);

        // Assert: Validation must fail with InvalidCharacters error
        Assert.False(result.IsValid);
        Assert.Equal(IbanValidationError.InvalidCharacters, result.Error);
    }

    [Fact]
    public void Validate_UnsupportedCountry_ShouldReturnUnsupportedCountryError()
    {
        // Act: Validate IBAN with ISO country code that is not supported in the registry (e.g., 'ZZ')
        var result = IbanValidator.Validate("ZZ020120000000001083758362");

        // Assert: Validation must fail with UnsupportedCountry error
        Assert.False(result.IsValid);
        Assert.Equal(IbanValidationError.UnsupportedCountry, result.Error);
    }

    [Theory]
    [InlineData("IR02012000000000108375836")]    // 23 characters instead of registered 26
    [InlineData("IR02012000000000108375836299")]  // 28 characters instead of registered 26
    public void Validate_WrongLengthForCountry_ShouldReturnInvalidLengthError(string iban)
    {
        // Act: Validate IBAN with valid country code but mismatched total length
        var result = IbanValidator.Validate(iban);

        // Assert: Validation must fail with InvalidLength error while identifying the country
        Assert.False(result.IsValid);
        Assert.Equal(IbanValidationError.InvalidLength, result.Error);
        Assert.Equal("IR", result.CountryCode);
    }

    [Theory]
    [InlineData("IRXX0120000000001083758362")] // Letters in check digits position (must be 2 digits)
    [InlineData("IR000120000000001083758362")] // Incorrect check digits (MOD-97 checksum != 1)
    public void Validate_InvalidCheckDigits_ShouldReturnInvalidCheckDigitsError(string iban)
    {
        // Act: Validate IBAN with invalid check digits or failed MOD-97 calculation
        var result = IbanValidator.Validate(iban);

        // Assert: Validation must fail with InvalidCheckDigits error
        Assert.False(result.IsValid);
        Assert.Equal(IbanValidationError.InvalidCheckDigits, result.Error);
    }
}

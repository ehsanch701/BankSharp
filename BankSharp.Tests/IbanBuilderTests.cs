using System;
using System.Linq;
using BankSharp.Registry;
using Xunit;

namespace BankSharp.Tests;

/// <summary>
/// Unit tests for <see cref="IbanBuilder"/> covering deterministic fluent building,
/// edge cases, zero-padding, and smart random IBAN generation.
/// </summary>
public class IbanBuilderTests
{
    #region Standard Fluent Builder Tests

    /// <summary>
    /// Verifies building a standard Iranian IBAN (Bank Mellat) using country code,
    /// bank code, and 19-digit account number.
    /// </summary>
    [Fact]
    public void Build_IranIban_ShouldGenerateValidIban()
    {
        // Arrange: Construct builder with real Iranian bank account details (Bank Mellat)
        var builder = new IbanBuilder()
            .WithCountry("IR")
            .WithBankCode("012")
            .WithAccountNumber("00000001083758362");

        // Act: Generate the final IBAN string
        string iban = builder.Build();

        // Assert: Ensure exact value match and verify full cryptographic/structural validity
        Assert.Equal("IR020120000000001083758362", iban);

        var validationResult = IbanValidator.Validate(iban);
        Assert.True(validationResult.IsValid);
        Assert.Equal(IbanValidationError.None, validationResult.Error);
    }

    /// <summary>
    /// Verifies constructing a valid German (DE) IBAN.
    /// </summary>
    [Fact]
    public void Build_GermanyIban_ShouldGenerateValidIban()
    {
        // Arrange: German IBAN parameters (Bankleitzahl and Account Number)
        var builder = new IbanBuilder()
            .WithCountry("DE")
            .WithBankCode("37040044")
            .WithAccountNumber("0532013000");

        // Act
        string iban = builder.Build();

        // Assert: Validate exact match and check digits integrity
        Assert.Equal("DE89370400440532013000", iban);

        var validationResult = IbanValidator.Validate(iban);
        Assert.True(validationResult.IsValid);
        Assert.Equal(IbanValidationError.None, validationResult.Error);
    }

    /// <summary>
    /// Verifies constructing a valid United Kingdom (GB) IBAN with 4-letter bank code and 6-digit sort code.
    /// </summary>
    [Fact]
    public void Build_UnitedKingdomIban_ShouldGenerateValidIban()
    {
        // Arrange: NatWest bank IBAN parameters (Bank Code, Sort/Branch Code, Account Number)
        var builder = new IbanBuilder()
            .WithCountry("GB")
            .WithBankCode("NWBK")
            .WithBranchCode("601613")
            .WithAccountNumber("31926819");

        // Act
        string iban = builder.Build();

        // Assert: Ensure format and checksum match expected GB standard
        Assert.Equal("GB29NWBK60161331926819", iban);

        var validationResult = IbanValidator.Validate(iban);
        Assert.True(validationResult.IsValid);
        Assert.Equal(IbanValidationError.None, validationResult.Error);
    }

    /// <summary>
    /// Verifies building a French (FR) IBAN including 2-digit national check digits (Clé RIB).
    /// </summary>
    [Fact]
    public void Build_FranceIban_WithBranchCode_ShouldGenerateValidIban()
    {
        // Arrange: French IBAN components including Bank Code, Branch Code, Account, and National Check Digits
        var builder = new IbanBuilder()
            .WithCountry("FR")
            .WithBankCode("20041")
            .WithBranchCode("01005")
            .WithAccountNumber("0500013M026")
            .WithNationalCheckDigits("06");

        // Act
        string iban = builder.Build();

        // Assert: Verify expected output and Mod97 compliance
        Assert.Equal("FR1420041010050500013M02606", iban);

        var validationResult = IbanValidator.Validate(iban);
        Assert.True(validationResult.IsValid);
        Assert.Equal(IbanValidationError.None, validationResult.Error);
    }

    /// <summary>
    /// Verifies that invoking <see cref="IbanBuilder.Build"/> without setting a country code
    /// throws an <see cref="InvalidOperationException"/>.
    /// </summary>
    [Fact]
    public void Build_WithoutCountryCode_ShouldThrowInvalidOperationException()
    {
        // Arrange: Configure builder without country specification
        var builder = new IbanBuilder()
            .WithBankCode("012")
            .WithAccountNumber("123456789");

        // Act & Assert: Must fail fast when country context is missing
        Assert.Throws<InvalidOperationException>(() => builder.Build());
    }

    /// <summary>
    /// Verifies that configuring an unsupported or non-existent country code throws <see cref="ArgumentException"/>.
    /// </summary>
    [Fact]
    public void Build_WithUnsupportedCountry_ShouldThrowArgumentException()
    {
        // Arrange
        var builder = new IbanBuilder();

        // Act & Assert: Immediate rejection when country is not registered
        Assert.Throws<ArgumentException>(() => builder.WithCountry("ZZ"));
    }

    /// <summary>
    /// Verifies that short account numbers are automatically left-padded with zeros
    /// according to the country rule requirements.
    /// </summary>
    [Fact]
    public void Build_WithShortAccountNumber_ShouldPadLeftWithZeros()
    {
        // Arrange: Iran requires 19 digits for the account number part; provide only 10 digits
        var builder = new IbanBuilder()
            .WithCountry("IR")
            .WithBankCode("012")
            .WithAccountNumber("1083758362");

        // Act
        string iban = builder.Build();

        // Assert: Should automatically prepend 9 zeros and yield the standard IBAN
        Assert.Equal("IR020120000000001083758362", iban);

        var validationResult = IbanValidator.Validate(iban);
        Assert.True(validationResult.IsValid);
        Assert.Equal(IbanValidationError.None, validationResult.Error);
    }

    /// <summary>
    /// Verifies that attempting to supply an account number exceeding the country's max length throws <see cref="ArgumentException"/>.
    /// </summary>
    [Fact]
    public void Build_WithAccountNumberExceedingLength_ShouldThrowArgumentException()
    {
        // Arrange: Iran maximum account length is 19 characters
        var builder = new IbanBuilder()
            .WithCountry("IR")
            .WithBankCode("012");

        // Act & Assert: Supplying 21 digits must immediately throw an ArgumentException
        Assert.Throws<ArgumentException>(() => builder.WithAccountNumber("000000000108375836200"));
    }

    /// <summary>
    /// Verifies that calling <see cref="IbanBuilder.Reset"/> clears internal buffers and requires re-configuration.
    /// </summary>
    [Fact]
    public void Reset_ShouldClearAllInternalFields()
    {
        // Arrange: Setup fully valid builder instance
        var builder = new IbanBuilder()
            .WithCountry("IR")
            .WithBankCode("012")
            .WithAccountNumber("00000001083758362");

        // Act: Reset internal state
        builder.Reset();

        // Assert: Attempting to build after reset must fail due to cleared country context
        Assert.Throws<InvalidOperationException>(() => builder.Build());
    }

    #endregion

    #region Smart Fuzzer & Random Generator Tests

    /// <summary>
    /// Verifies that generating random IBANs for various supported countries produces strings with
    /// correct total length, country prefix, and passing Mod-97 checksums.
    /// </summary>
    [Theory]
    [InlineData("IR", 26)]
    [InlineData("DE", 22)]
    [InlineData("GB", 22)]
    [InlineData("FR", 27)]
    [InlineData("AE", 23)]
    [InlineData("CH", 21)]
    [InlineData("TR", 26)]
    public void GenerateRandom_SupportedCountries_ShouldGenerateValidIban(string countryCode, int expectedLength)
    {
        // Act: Generate random IBAN for country
        string randomIban = IbanBuilder.GenerateRandom(countryCode);

        // Assert: Check basic metadata (length, null check, prefix)
        Assert.NotNull(randomIban);
        Assert.Equal(expectedLength, randomIban.Length);
        Assert.StartsWith(countryCode, randomIban, StringComparison.Ordinal);

        // Assert: Must pass ISO 7064 Mod 97-10 check digits validation
        var validationResult = IbanValidator.Validate(randomIban);
        Assert.True(validationResult.IsValid, $"Generated random IBAN '{randomIban}' failed validation with error: {validationResult.Error}");
        Assert.Equal(IbanValidationError.None, validationResult.Error);
    }

    /// <summary>
    /// Verifies random IBAN generation directly using an existing <see cref="CountryRule"/> instance.
    /// </summary>
    [Fact]
    public void GenerateRandom_WithCountryRule_ShouldProduceValidIban()
    {
        // Arrange: Resolve rule for Iran
        Assert.True(IbanRegistry.TryGetRule("IR", out var iranRule));

        // Act: Generate IBAN directly from CountryRule
        string randomIban = IbanBuilder.GenerateRandom(iranRule);

        // Assert: Verify expected total length and mathematical validity
        Assert.Equal(iranRule.TotalLength, randomIban.Length);

        var validationResult = IbanValidator.Validate(randomIban);
        Assert.True(validationResult.IsValid);
        Assert.Equal(IbanValidationError.None, validationResult.Error);
    }

    /// <summary>
    /// Verifies that requesting a random IBAN for an unregistered country throws <see cref="ArgumentException"/>.
    /// </summary>
    [Fact]
    public void GenerateRandom_UnsupportedCountry_ShouldThrowArgumentException()
    {
        // Act & Assert
        Assert.Throws<ArgumentException>(() => IbanBuilder.GenerateRandom("XX"));
    }

    /// <summary>
    /// Verifies that generating batches of random IBANs satisfies exact requested counts,
    /// maintains high entropy (no duplicates), and all items pass checksum validation.
    /// </summary>
    [Theory]
    [InlineData("IR", 20)]
    [InlineData("DE", 50)]
    public void GenerateRandomBatch_ShouldGenerateExactCountOfUniqueValidIbans(string countryCode, int count)
    {
        // Act: Materialize the lazy sequence of random IBANs
        var ibans = IbanBuilder.GenerateRandomBatch(countryCode, count).ToList();

        // Assert: Verify generated count
        Assert.Equal(count, ibans.Count);

        // Assert: High entropy check (all items in the batch must be unique)
        var uniqueIbans = ibans.Distinct().ToList();
        Assert.Equal(count, uniqueIbans.Count);

        // Assert: Verify that every single generated IBAN passes validation
        foreach (var iban in ibans)
        {
            var result = IbanValidator.Validate(iban);
            Assert.True(result.IsValid, $"IBAN '{iban}' generated in batch failed validation: {result.Error}");
        }
    }

    /// <summary>
    /// Verifies that passing zero or negative count to <see cref="IbanBuilder.GenerateRandomBatch"/>
    /// throws an <see cref="ArgumentOutOfRangeException"/>.
    /// </summary>
    [Fact]
    public void GenerateRandomBatch_WithInvalidCount_ShouldThrowArgumentOutOfRangeException()
    {
        // Act & Assert: Call ToList() to force iterator execution and trigger argument checks
        Assert.Throws<ArgumentOutOfRangeException>(() => IbanBuilder.GenerateRandomBatch("IR", 0).ToList());
        Assert.Throws<ArgumentOutOfRangeException>(() => IbanBuilder.GenerateRandomBatch("IR", -5).ToList());
    }

    #endregion
}

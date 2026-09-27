using BankSharp.Registry;
using Xunit;

namespace BankSharp.Tests;

/// <summary>
/// Contains unit tests ensuring integrity, bounds safety, and ISO compliance 
/// for country-specific IBAN metadata registered in <see cref="IbanRegistry"/>.
/// </summary>
public sealed class CountryRuleDataTests
{
    [Fact]
    public void AllRules_FieldsMustFitWithinTotalLengthAndNotOverlap()
    {
        foreach (var rule in IbanRegistry.Rules.Values)
        {
            // IBAN invariant: The first 4 characters are always reserved for CountryCode (2) and CheckDigits (2)
            const int MinValidOffset = 4;

            // 1. Bank Code Slicing Verification
            if (rule.BankCodeLength > 0)
            {
                Assert.True(rule.BankCodeOffset >= MinValidOffset,
                    $"{rule.CountryCode}: BankCodeOffset ({rule.BankCodeOffset}) cannot overlap the 4-character IBAN header.");

                Assert.True(rule.BankCodeOffset + rule.BankCodeLength <= rule.TotalLength,
                    $"{rule.CountryCode}: BankCode slice exceeds TotalLength ({rule.TotalLength}).");
            }

            // 2. Branch Code Slicing Verification
            if (rule.BranchCodeLength > 0)
            {
                Assert.True(rule.BranchCodeOffset >= MinValidOffset,
                    $"{rule.CountryCode}: BranchCodeOffset ({rule.BranchCodeOffset}) cannot overlap the 4-character IBAN header.");

                Assert.True(rule.BranchCodeOffset + rule.BranchCodeLength <= rule.TotalLength,
                    $"{rule.CountryCode}: BranchCode slice exceeds TotalLength ({rule.TotalLength}).");
            }

            // 3. Account Number Slicing Verification
            if (rule.AccountNumberLength > 0)
            {
                Assert.True(rule.AccountNumberOffset >= MinValidOffset,
                    $"{rule.CountryCode}: AccountNumberOffset ({rule.AccountNumberOffset}) cannot overlap the 4-character IBAN header.");

                Assert.True(rule.AccountNumberOffset + rule.AccountNumberLength <= rule.TotalLength,
                    $"{rule.CountryCode}: AccountNumber slice exceeds TotalLength ({rule.TotalLength}).");
            }

            // 4. Structural Integrity: Segments should be parsable when lengths are defined
            if (rule.BankCodeLength > 0)
            {
                Assert.True(rule.IsParsable,
                    $"{rule.CountryCode}: Rule defines BankCodeLength but IsParsable is false.");
            }
        }
    }

    [Theory]
    // Middle East & Asia
    [InlineData("IR", 26)] // Iran
    [InlineData("AE", 23)] // United Arab Emirates
    [InlineData("TR", 26)] // Turkey

    // Europe (Western & Central)
    [InlineData("DE", 22)] // Germany
    [InlineData("GB", 22)] // United Kingdom
    [InlineData("FR", 27)] // France
    [InlineData("NL", 18)] // Netherlands
    [InlineData("PT", 25)] // Portugal
    [InlineData("IT", 27)] // Italy
    [InlineData("ES", 24)] // Spain
    [InlineData("CH", 21)] // Switzerland
    public void RuleLengths_MatchOfficialIbanRegistry(string code, int expectedLength)
    {
        // Act
        bool exists = IbanRegistry.Rules.TryGetValue(code, out var rule);

        // Assert
        Assert.True(exists, $"Country code '{code}' was expected to be registered in standard rules.");
        Assert.Equal(expectedLength, rule.TotalLength);
        Assert.Equal(code, rule.CountryCode);
    }

    [Fact]
    public void AllRules_CountryCodesMustMatchDictionaryKeys()
    {
        // Act & Assert: Ensure key and internal country code are consistently uppercase and identical
        foreach (var (key, rule) in IbanRegistry.Rules)
        {
            Assert.Equal(key, rule.CountryCode);
            Assert.Equal(2, rule.CountryCode.Length);
            Assert.True(rule.CountryCode.All(char.IsAsciiLetterUpper),
                $"{rule.CountryCode}: CountryCode must be two uppercase ASCII letters.");
        }
    }
}

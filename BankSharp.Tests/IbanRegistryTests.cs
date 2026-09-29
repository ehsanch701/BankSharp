using BankSharp.Registry;

namespace BankSharp.Tests.Registry;

/// <summary>
/// Contains unit and concurrency tests for the <see cref="IbanRegistry"/> class.
/// Validates default country specifications, field length constraints, 
/// zero-allocation span lookups, and dynamic custom rule registrations.
/// </summary>
[Collection("StaticRegistryTests")]
public class IbanRegistryTests
{
    /// <summary>
    /// Verifies that standard core country rules (e.g., IR, DE, GB, FR) 
    /// are pre-loaded into the registry upon initialization.
    /// </summary>
    [Fact]
    public void Rules_ShouldContainStandardCountries()
    {
        // Assert
        Assert.NotNull(IbanRegistry.Rules);
        Assert.True(IbanRegistry.Rules.ContainsKey("IR"));
        Assert.True(IbanRegistry.Rules.ContainsKey("DE"));
        Assert.True(IbanRegistry.Rules.ContainsKey("GB"));
        Assert.True(IbanRegistry.Rules.ContainsKey("FR"));
    }

    /// <summary>
    /// Validates that total IBAN lengths defined in the registry match the official ISO 13616 specifications.
    /// </summary>
    /// <param name="countryCode">The two-letter ISO country code.</param>
    /// <param name="expectedLength">The expected standard IBAN total length.</param>
    [Theory]
    [InlineData("IR", 26)]
    [InlineData("DE", 22)]
    [InlineData("GB", 22)]
    [InlineData("FR", 27)]
    [InlineData("PT", 25)]
    [InlineData("NL", 18)]
    [InlineData("TR", 26)]
    [InlineData("AE", 23)]
    [InlineData("IT", 27)]
    [InlineData("ES", 24)]
    [InlineData("CH", 21)]
    public void RuleLengths_MatchOfficialIbanRegistry(string countryCode, int expectedLength)
    {
        // Act
        var found = IbanRegistry.Rules.TryGetValue(countryCode, out var rule);

        // Assert
        Assert.True(found, $"Country {countryCode} was not found in registry.");
        Assert.Equal(expectedLength, rule.TotalLength);
        Assert.Equal(countryCode, rule.CountryCode);
    }

    /// <summary>
    /// Invariant check ensuring that for all parsable rules, each field segment slice 
    /// (BankCode, BranchCode, AccountNumber) fits within the declared total length.
    /// </summary>
    [Fact]
    public void AllRules_FieldsMustFitWithinTotalLength()
    {
        foreach (var (code, rule) in IbanRegistry.Rules)
        {
            if (rule.IsParsable)
            {
                Assert.True(
                    rule.BankCodeOffset + rule.BankCodeLength <= rule.TotalLength,
                    $"{code}: BankCode slice exceeds TotalLength"
                );

                if (rule.BranchCodeLength > 0)
                {
                    Assert.True(
                        rule.BranchCodeOffset + rule.BranchCodeLength <= rule.TotalLength,
                        $"{code}: BranchCode slice exceeds TotalLength"
                    );
                }

                Assert.True(
                    rule.AccountNumberOffset + rule.AccountNumberLength <= rule.TotalLength,
                    $"{code}: AccountNumber slice exceeds TotalLength"
                );
            }
        }
    }

    /// <summary>
    /// Ensures that <see cref="IbanRegistry.TryGetRule(ReadOnlySpan{char}, out CountryRule)"/> 
    /// performs case-insensitive lookups using memory spans without heap allocations.
    /// </summary>
    /// <param name="code">The country code slice to lookup.</param>
    /// <param name="expectedSuccess">Expected lookup outcome.</param>
    [Theory]
    [InlineData("IR", true)]
    [InlineData("ir", true)]
    [InlineData("Ir", true)]
    [InlineData("DE", true)]
    [InlineData("de", true)]
    [InlineData("ZZ", false)]
    [InlineData("XX", false)]
    public void TryGetRule_SpanLookup_ShouldWorkCaseInsensitive(string code, bool expectedSuccess)
    {
        // Act - Perform zero-allocation span lookup
        var success = IbanRegistry.TryGetRule(code.AsSpan(), out var rule);

        // Assert
        Assert.Equal(expectedSuccess, success);
        if (expectedSuccess)
        {
            Assert.Equal(code.ToUpperInvariant(), rule.CountryCode);
            Assert.True(rule.TotalLength > 0);
        }
    }

    /// <summary>
    /// Verifies that dynamically registered custom rules can be retrieved successfully 
    /// via both dictionary lookup and zero-allocation span lookup.
    /// </summary>
    [Fact]
    public void RegisterCustomRule_ShouldAddNewRuleAndBeRetrievable()
    {
        // Arrange
        var customRule = new CountryRule
        {
            CountryCode = "TS",
            TotalLength = 30,
            Pattern = @"^TS\d{28}$",
            BankCodeOffset = 4,
            BankCodeLength = 4,
            AccountNumberOffset = 8,
            AccountNumberLength = 22
        };

        // Act
        IbanRegistry.RegisterCustomRule(customRule);

        // Assert
        var foundInDict = IbanRegistry.Rules.TryGetValue("TS", out var ruleFromDict);
        var foundInSpan = IbanRegistry.TryGetRule("TS".AsSpan(), out var ruleFromSpan);

        Assert.True(foundInDict);
        Assert.True(foundInSpan);
        Assert.Equal(30, ruleFromDict.TotalLength);
        Assert.Equal(30, ruleFromSpan.TotalLength);
        Assert.True(ruleFromSpan.IsParsable);
    }

    /// <summary>
    /// Ensures that attempting to register a custom rule with missing or empty 
    /// country code identifiers throws an <see cref="ArgumentException"/>.
    /// </summary>
    [Fact]
    public void RegisterCustomRule_NullOrWhiteSpace_ThrowsException()
    {
        // Arrange
        var invalidRule = new CountryRule
        {
            CountryCode = "",
            TotalLength = 20,
            Pattern = "^$"
        };

        // Act & Assert
        Assert.Throws<ArgumentException>(() => IbanRegistry.RegisterCustomRule(invalidRule));
    }

    /// <summary>
    /// Stress tests parallel rule registrations to ensure thread-safety, 
    /// atomic copy-on-write snapshot updates, and registry data integrity under high concurrency.
    /// Uses 2-letter ISO-compliant country codes to avoid polluting shared registry state.
    /// </summary>
    [Fact]
    public void RegisterCustomRule_ConcurrentAccess_MaintainsThreadSafetyAndDataIntegrity()
    {
        // Arrange
        const int concurrentWriters = 20;

        // Act - Concurrently register valid 2-character country codes (XA, XB, ..., XT)
        Parallel.For(0, concurrentWriters, i =>
        {
            var code = $"X{(char)('A' + i)}"; // 2-letter format: XA, XB, ..., XT
            var customRule = new CountryRule
            {
                CountryCode = code,
                TotalLength = 20 + i,
                Pattern = $@"^{code}\d{{{18 + i}}}$"
            };

            IbanRegistry.RegisterCustomRule(customRule);
        });

        // Assert - Verify no updates were dropped and all rules persist in the final snapshot
        for (int i = 0; i < concurrentWriters; i++)
        {
            var code = $"X{(char)('A' + i)}";
            var success = IbanRegistry.TryGetRule(code.AsSpan(), out var rule);
            Assert.True(success, $"Concurrent rule {code} failed to register.");
            Assert.Equal(20 + i, rule.TotalLength);
        }
    }
}

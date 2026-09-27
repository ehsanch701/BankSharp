using System.Collections.Frozen;

namespace BankSharp.Registry;

/// <summary>
/// Provides a high-throughput, thread-safe, and lock-free registry for IBAN country rules.
/// Optimized for zero-allocation span-based lookups and atomic snapshot updates.
/// </summary>
public static class IbanRegistry
{
    private static readonly FrozenDictionary<string, CountryRule> _standardRules;
    private static volatile FrozenDictionary<string, CountryRule> _rulesSnapshot;

    /// <summary>
    /// Gets all currently registered country rules (both built-in standards and custom user overrides).
    /// </summary>
    /// <remarks>
    /// Performs a lock-free snapshot read with <c>O(1)</c> lookup time and zero heap allocations.
    /// </remarks>
    public static IReadOnlyDictionary<string, CountryRule> Rules => _rulesSnapshot;

    static IbanRegistry()
    {
        var dict = new Dictionary<string, CountryRule>(StringComparer.OrdinalIgnoreCase)
        {
            ["IR"] = new()
            {
                CountryCode = "IR",
                TotalLength = 26,
                Pattern = @"^IR\d{24}$",
                BankCodeOffset = 4,
                BankCodeLength = 3,
                AccountNumberOffset = 7,
                AccountNumberLength = 19
            },
            ["DE"] = new()
            {
                CountryCode = "DE",
                TotalLength = 22,
                Pattern = @"^DE\d{20}$",
                BankCodeOffset = 4,
                BankCodeLength = 8,
                AccountNumberOffset = 12,
                AccountNumberLength = 10
            },
            ["GB"] = new()
            {
                CountryCode = "GB",
                TotalLength = 22,
                Pattern = @"^GB\d{2}[A-Z]{4}\d{14}$",
                BankCodeOffset = 4,
                BankCodeLength = 4,
                BranchCodeOffset = 8,
                BranchCodeLength = 6,
                AccountNumberOffset = 14,
                AccountNumberLength = 8
            },
            ["FR"] = new()
            {
                CountryCode = "FR",
                TotalLength = 27,
                Pattern = @"^FR\d{2}\d{10}[A-Z0-9]{11}\d{2}$",
                BankCodeOffset = 4,
                BankCodeLength = 5,
                BranchCodeOffset = 9,
                BranchCodeLength = 5,
                AccountNumberOffset = 14,
                AccountNumberLength = 11
            },
            ["PT"] = new()
            {
                CountryCode = "PT",
                TotalLength = 25,
                Pattern = @"^PT\d{23}$",
                BankCodeOffset = 4,
                BankCodeLength = 4,
                BranchCodeOffset = 8,
                BranchCodeLength = 4,
                AccountNumberOffset = 12,
                AccountNumberLength = 11
            },
            ["NL"] = new()
            {
                CountryCode = "NL",
                TotalLength = 18,
                Pattern = @"^NL\d{2}[A-Z]{4}\d{10}$",
                BankCodeOffset = 4,
                BankCodeLength = 4,
                AccountNumberOffset = 8,
                AccountNumberLength = 10
            },
            ["TR"] = new()
            {
                CountryCode = "TR",
                TotalLength = 26,
                Pattern = @"^TR\d{24}$",
                BankCodeOffset = 4,
                BankCodeLength = 5,
                AccountNumberOffset = 10,
                AccountNumberLength = 16
            },
            ["AE"] = new()
            {
                CountryCode = "AE",
                TotalLength = 23,
                Pattern = @"^AE\d{21}$",
                BankCodeOffset = 4,
                BankCodeLength = 3,
                AccountNumberOffset = 7,
                AccountNumberLength = 16
            },
            ["IT"] = new()
            {
                CountryCode = "IT",
                TotalLength = 27,
                Pattern = @"^IT\d{2}[A-Z]\d{10}[A-Z0-9]{12}$",
                BankCodeOffset = 5,
                BankCodeLength = 5,
                BranchCodeOffset = 10,
                BranchCodeLength = 5,
                AccountNumberOffset = 15,
                AccountNumberLength = 12
            },
            ["ES"] = new()
            {
                CountryCode = "ES",
                TotalLength = 24,
                Pattern = @"^ES\d{22}$",
                BankCodeOffset = 4,
                BankCodeLength = 4,
                BranchCodeOffset = 8,
                BranchCodeLength = 4,
                AccountNumberOffset = 12,
                AccountNumberLength = 12
            },
            ["CH"] = new()
            {
                CountryCode = "CH",
                TotalLength = 21,
                Pattern = @"^CH\d{19}$",
                BankCodeOffset = 4,
                BankCodeLength = 5,
                AccountNumberOffset = 9,
                AccountNumberLength = 12
            }
        };

        _standardRules = dict.ToFrozenDictionary(StringComparer.OrdinalIgnoreCase);
        _rulesSnapshot = _standardRules;
    }

    /// <summary>
    /// Registers a new rule or overrides an existing rule for a specific country code.
    /// Uses an atomic Compare-And-Swap (CAS) loop to maintain thread safety without blocking reader threads.
    /// </summary>
    /// <param name="rule">The country rule definition to register or override.</param>
    /// <exception cref="ArgumentException">Thrown when <see cref="CountryRule.CountryCode"/> is <see langword="null"/>, empty, or whitespace.</exception>
    public static void RegisterCustomRule(CountryRule rule)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(rule.CountryCode);

        var spinWait = new SpinWait();

        while (true)
        {
            var oldSnapshot = _rulesSnapshot;

            var newDict = new Dictionary<string, CountryRule>(oldSnapshot, StringComparer.OrdinalIgnoreCase)
            {
                [rule.CountryCode] = rule
            };

            var newSnapshot = newDict.ToFrozenDictionary(StringComparer.OrdinalIgnoreCase);

            if (ReferenceEquals(Interlocked.CompareExchange(ref _rulesSnapshot, newSnapshot, oldSnapshot), oldSnapshot))
            {
                break;
            }

            spinWait.SpinOnce();
        }
    }

    /// <summary>
    /// Attempts to retrieve a country rule using a character span representing the country code.
    /// </summary>
    /// <param name="countryCode">A two-letter ISO country code span (case-insensitive).</param>
    /// <param name="rule">When this method returns <see langword="true"/>, contains the matched <see cref="CountryRule"/>; otherwise, the default value.</param>
    /// <returns><see langword="true"/> if a rule exists for the specified country code; otherwise, <see langword="false"/>.</returns>
    /// <remarks>
    /// In .NET 9 and later, this method utilizes <c>GetAlternateLookup</c> for zero-allocation span lookups. In .NET 8, it performs an allocation fallback to convert the span to a string.
    /// </remarks>
    [System.Runtime.CompilerServices.MethodImpl(System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining)]
    public static bool TryGetRule(ReadOnlySpan<char> countryCode, out CountryRule rule)
    {
#if NET9_0_OR_GREATER
        // Zero-allocation span-based lookup on .NET 9 and .NET 10
        return _rulesSnapshot.GetAlternateLookup<ReadOnlySpan<char>>().TryGetValue(countryCode, out rule);
#else
        // Fallback for .NET 8 (allocates string only if span lookup is not natively supported)
        return _rulesSnapshot.TryGetValue(countryCode.ToString(), out rule);
#endif
    }
}

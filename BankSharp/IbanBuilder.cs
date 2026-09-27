using System.Text.RegularExpressions;
using BankSharp.Engines;
using BankSharp.Registry;

namespace BankSharp;

/// <summary>
/// Provides a high-performance fluent builder and test data generator for standard IBAN values.
/// </summary>
public sealed class IbanBuilder
{
    private string? _countryCode;
    private CountryRule _rule;
    private bool _hasRule;
    private string? _bankCode;
    private string? _branchCode;
    private string? _accountNumber;
    private string? _nationalCheckDigits;

    /// <summary>
    /// Sets the two-letter ISO 3166-1 country code and loads its formatting rules.
    /// </summary>
    /// <param name="countryCode">The two-letter country code.</param>
    /// <returns>The builder instance.</returns>
    /// <exception cref="ArgumentException">Thrown when the country code is invalid or unsupported.</exception>
    public IbanBuilder WithCountry(string countryCode)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(countryCode);
        string normalized = countryCode.Trim().ToUpperInvariant();

        if (!IbanRegistry.TryGetRule(normalized, out var rule))
        {
            throw new ArgumentException($"Country code '{countryCode}' is not supported.", nameof(countryCode));
        }

        _countryCode = normalized;
        _rule = rule;
        _hasRule = true;
        return this;
    }

    /// <summary>
    /// Sets the bank or financial institution code.
    /// </summary>
    /// <param name="bankCode">The bank code.</param>
    /// <returns>The builder instance.</returns>
    public IbanBuilder WithBankCode(string bankCode)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(bankCode);
        string normalized = bankCode.Trim().ToUpperInvariant();

        if (_hasRule && _rule.BankCodeLength > 0 && normalized.Length > _rule.BankCodeLength)
        {
            throw new ArgumentException($"Bank code exceeds maximum length of {_rule.BankCodeLength}.", nameof(bankCode));
        }

        _bankCode = normalized;
        return this;
    }

    /// <summary>
    /// Sets the branch or sort code.
    /// </summary>
    /// <param name="branchCode">The branch code.</param>
    /// <returns>The builder instance.</returns>
    public IbanBuilder WithBranchCode(string branchCode)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(branchCode);
        string normalized = branchCode.Trim().ToUpperInvariant();

        if (_hasRule && _rule.BranchCodeLength > 0 && normalized.Length > _rule.BranchCodeLength)
        {
            throw new ArgumentException($"Branch code exceeds maximum length of {_rule.BranchCodeLength}.", nameof(branchCode));
        }

        _branchCode = normalized;
        return this;
    }

    /// <summary>
    /// Sets the account number.
    /// </summary>
    /// <param name="accountNumber">The account number.</param>
    /// <returns>The builder instance.</returns>
    /// <exception cref="ArgumentException">Thrown when account number exceeds the country's allowed length.</exception>
    public IbanBuilder WithAccountNumber(string accountNumber)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(accountNumber);
        string normalized = accountNumber.Trim().ToUpperInvariant();

        if (_hasRule && _rule.AccountNumberLength > 0 && normalized.Length > _rule.AccountNumberLength)
        {
            throw new ArgumentException($"Account number exceeds maximum length of {_rule.AccountNumberLength}.", nameof(accountNumber));
        }

        _accountNumber = normalized;
        return this;
    }

    /// <summary>
    /// Sets optional national check digits (e.g., French RIB key or Spanish control digits).
    /// </summary>
    /// <param name="nationalCheckDigits">The national check digits.</param>
    /// <returns>The builder instance.</returns>
    public IbanBuilder WithNationalCheckDigits(string nationalCheckDigits)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(nationalCheckDigits);
        _nationalCheckDigits = nationalCheckDigits.Trim().ToUpperInvariant();
        return this;
    }

    /// <summary>
    /// Resets all internal fields and states to allow reusing the builder.
    /// </summary>
    /// <returns>The builder instance.</returns>
    public IbanBuilder Reset()
    {
        _countryCode = null;
        _rule = default;
        _hasRule = false;
        _bankCode = null;
        _branchCode = null;
        _accountNumber = null;
        _nationalCheckDigits = null;
        return this;
    }

    /// <summary>
    /// Builds the IBAN string by calculating ISO 7064 Mod 97-10 check digits and validating against country rules.
    /// </summary>
    /// <returns>The generated valid IBAN string.</returns>
    /// <exception cref="InvalidOperationException">Thrown when mandatory fields are missing.</exception>
    /// <exception cref="ArgumentException">Thrown when country rules or formats are violated.</exception>
    public string Build()
    {
        if (string.IsNullOrWhiteSpace(_countryCode) || !_hasRule)
        {
            throw new InvalidOperationException("Country code must be specified.");
        }

        Span<char> buffer = stackalloc char[_rule.TotalLength];
        buffer.Fill('0');

        // Copy country code (first 2 chars)
        _countryCode.AsSpan().CopyTo(buffer[..2]);

        // Bank code
        if (_rule.BankCodeLength > 0)
        {
            if (string.IsNullOrWhiteSpace(_bankCode))
            {
                throw new InvalidOperationException($"Bank code is required for country {_countryCode}.");
            }

            if (_bankCode.Length > _rule.BankCodeLength)
            {
                throw new ArgumentException($"Bank code exceeds maximum length of {_rule.BankCodeLength}.", nameof(_bankCode));
            }

            var dest = buffer.Slice(_rule.BankCodeOffset, _rule.BankCodeLength);
            dest.Fill('0');
            // Right-aligned padded with 0 if shorter
            _bankCode.AsSpan().CopyTo(dest[(_rule.BankCodeLength - _bankCode.Length)..]);
        }

        // Branch code
        if (_rule.BranchCodeLength > 0)
        {
            if (string.IsNullOrWhiteSpace(_branchCode))
            {
                throw new InvalidOperationException($"Branch code is required for country {_countryCode}.");
            }

            if (_branchCode.Length > _rule.BranchCodeLength)
            {
                throw new ArgumentException($"Branch code exceeds maximum length of {_rule.BranchCodeLength}.", nameof(_branchCode));
            }

            var dest = buffer.Slice(_rule.BranchCodeOffset, _rule.BranchCodeLength);
            dest.Fill('0');
            _branchCode.AsSpan().CopyTo(dest[(_rule.BranchCodeLength - _branchCode.Length)..]);
        }

        // Account number (Left-padded with '0' inside its allocated section)
        if (_rule.AccountNumberLength > 0)
        {
            if (string.IsNullOrWhiteSpace(_accountNumber))
            {
                throw new InvalidOperationException($"Account number is required for country {_countryCode}.");
            }

            if (_accountNumber.Length > _rule.AccountNumberLength)
            {
                throw new ArgumentException($"Account number exceeds maximum length of {_rule.AccountNumberLength}.", nameof(_accountNumber));
            }

            var dest = buffer.Slice(_rule.AccountNumberOffset, _rule.AccountNumberLength);
            dest.Fill('0');
            _accountNumber.AsSpan().CopyTo(dest[(_rule.AccountNumberLength - _accountNumber.Length)..]);
        }

        // National check digits (if provided, placed immediately after AccountNumber)
        if (!string.IsNullOrEmpty(_nationalCheckDigits))
        {
            int nationalDigitsOffset = _rule.AccountNumberOffset + _rule.AccountNumberLength;
            int remainingSpace = _rule.TotalLength - nationalDigitsOffset;

            if (remainingSpace > 0)
            {
                int copyLength = Math.Min(_nationalCheckDigits.Length, remainingSpace);
                var dest = buffer.Slice(nationalDigitsOffset, copyLength);
                _nationalCheckDigits.AsSpan(0, copyLength).CopyTo(dest);
            }
        }

        // Placeholder '00' for ISO check digit calculation
        buffer[2] = '0';
        buffer[3] = '0';

        int remainder = Mod97Engine.CalculateIbanMod97(buffer);
        if (remainder < 0)
        {
            throw new ArgumentException("The generated IBAN contains invalid characters.");
        }

        int checkDigits = 98 - remainder;
        buffer[2] = (char)('0' + (checkDigits / 10));
        buffer[3] = (char)('0' + (checkDigits % 10));

        string iban = new string(buffer);

        // Validate complete IBAN against registry regex pattern if defined
        if (!string.IsNullOrEmpty(_rule.Pattern) && !Regex.IsMatch(iban, _rule.Pattern, RegexOptions.None, TimeSpan.FromMilliseconds(200)))
        {
            throw new ArgumentException($"Generated IBAN '{iban}' does not match the rule pattern for {_countryCode}.");
        }

        return iban;
    }

    /// <summary>
    /// Builds and returns a strongly-typed <see cref="Iban"/> instance.
    /// </summary>
    /// <returns>A validated <see cref="Iban"/> struct instance.</returns>
    public Iban BuildIban() => Iban.Parse(Build());

    #region Smart Fuzzer & Random Generator

    /// <summary>
    /// Generates a syntactically and mathematically valid random IBAN for the specified country.
    /// Ideal for integration tests, fuzzing, and mock data generation.
    /// </summary>
    /// <param name="countryCode">ISO 3166-1 alpha-2 country code (e.g., "IR", "DE", "GB").</param>
    /// <returns>A valid IBAN string satisfying ISO 7064 Mod 97-10 check digits and BBAN layout.</returns>
    /// <exception cref="ArgumentException">Thrown when the country code is unsupported or null/empty.</exception>
    public static string GenerateRandom(string countryCode)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(countryCode);

        if (!IbanRegistry.TryGetRule(countryCode.AsSpan(), out var rule))
        {
            throw new ArgumentException($"Country code '{countryCode}' is not supported by the registry.", nameof(countryCode));
        }

        return GenerateRandom(rule);
    }

    /// <summary>
    /// Generates a syntactically and mathematically valid random IBAN using an existing <see cref="CountryRule"/>.
    /// </summary>
    /// <param name="rule">The pre-resolved country rule.</param>
    /// <returns>A valid IBAN string satisfying ISO 7064 Mod 97-10 check digits.</returns>
    public static string GenerateRandom(CountryRule rule)
    {
        Span<char> buffer = stackalloc char[rule.TotalLength];
        buffer.Fill('0');

        // 1. Copy Country Code
        rule.CountryCode.AsSpan().CopyTo(buffer[..2]);

        // 2. Set Placeholder Check Digits '00'
        buffer[2] = '0';
        buffer[3] = '0';

        // 3. Fill Bank Code
        if (rule.BankCodeLength > 0)
        {
            FillRandomNumeric(buffer.Slice(rule.BankCodeOffset, rule.BankCodeLength));
        }

        // 4. Fill Branch Code (if applicable)
        if (rule.BranchCodeLength > 0)
        {
            FillRandomNumeric(buffer.Slice(rule.BranchCodeOffset, rule.BranchCodeLength));
        }

        // 5. Fill Account Number
        if (rule.AccountNumberLength > 0)
        {
            FillRandomNumeric(buffer.Slice(rule.AccountNumberOffset, rule.AccountNumberLength));
        }

        // 6. Compute Mod 97-10 Check Digits
        int remainder = Mod97Engine.CalculateIbanMod97(buffer);
        if (remainder < 0)
        {
            throw new InvalidOperationException("Failed to calculate Mod-97 checksum during random IBAN generation.");
        }

        int checkDigits = 98 - remainder;
        buffer[2] = (char)('0' + (checkDigits / 10));
        buffer[3] = (char)('0' + (checkDigits % 10));

        return new string(buffer);
    }

    /// <summary>
    /// Generates an enumerable batch of valid random IBANs for high-throughput testing and fuzzing.
    /// </summary>
    /// <param name="countryCode">ISO 3166-1 alpha-2 country code.</param>
    /// <param name="count">The number of valid IBANs to generate.</param>
    /// <returns>A lazy sequence of valid IBAN strings.</returns>
    public static IEnumerable<string> GenerateRandomBatch(string countryCode, int count)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(countryCode);
        ArgumentOutOfRangeException.ThrowIfNegativeOrZero(count);

        if (!IbanRegistry.TryGetRule(countryCode.AsSpan(), out var rule))
        {
            throw new ArgumentException($"Country code '{countryCode}' is not supported by the registry.", nameof(countryCode));
        }

        for (int i = 0; i < count; i++)
        {
            yield return GenerateRandom(rule);
        }
    }

    /// <summary>
    /// In-place random digit generator using <see cref="Random.Shared"/> without allocating heap memory.
    /// </summary>
    private static void FillRandomNumeric(Span<char> destination)
    {
        for (int i = 0; i < destination.Length; i++)
        {
            destination[i] = (char)('0' + Random.Shared.Next(0, 10));
        }
    }

    #endregion
}

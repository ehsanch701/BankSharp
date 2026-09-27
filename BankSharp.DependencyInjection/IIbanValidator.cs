using System;
using System.Diagnostics.CodeAnalysis;

namespace BankSharp.Extensions.DependencyInjection;

/// <summary>
/// Defines methods for validating and parsing International Bank Account Numbers (IBAN).
/// Provides zero-allocation span-based overloads alongside string-based APIs.
/// </summary>
public interface IIbanValidator
{
    /// <summary>
    /// Determines whether the specified string IBAN is valid.
    /// </summary>
    /// <param name="iban">The IBAN string to validate.</param>
    /// <returns><see langword="true"/> if the IBAN is valid; otherwise, <see langword="false"/>.</returns>
    bool IsValid(string? iban);

    /// <summary>
    /// Determines whether the specified span of characters is a valid IBAN without allocating.
    /// </summary>
    /// <param name="iban">The IBAN character span to validate.</param>
    /// <returns><see langword="true"/> if the IBAN is valid; otherwise, <see langword="false"/>.</returns>
    bool IsValid(ReadOnlySpan<char> iban);

    /// <summary>
    /// Attempts to parse an IBAN string into a validated <see cref="Iban"/> instance.
    /// </summary>
    /// <param name="input">The IBAN string representation to parse.</param>
    /// <param name="iban">
    /// When this method returns, contains the parsed <see cref="Iban"/> if successful;
    /// otherwise, the default value.
    /// </param>
    /// <returns><see langword="true"/> if parsing succeeded; otherwise, <see langword="false"/>.</returns>
    bool TryParse([NotNullWhen(true)] string? input, out Iban iban);

    /// <summary>
    /// Attempts to parse an IBAN character span into a validated <see cref="Iban"/> instance without allocating.
    /// </summary>
    /// <param name="input">The IBAN character span to parse.</param>
    /// <param name="iban">
    /// When this method returns, contains the parsed <see cref="Iban"/> if successful;
    /// otherwise, the default value.
    /// </param>
    /// <returns><see langword="true"/> if parsing succeeded; otherwise, <see langword="false"/>.</returns>
    bool TryParse(ReadOnlySpan<char> input, out Iban iban);

    /// <summary>
    /// Validates an IBAN string and returns a detailed validation result with error breakdown.
    /// </summary>
    /// <param name="iban">The IBAN string to validate.</param>
    /// <returns>An <see cref="IbanValidationResult"/> describing the outcome and specific validation errors.</returns>
    IbanValidationResult Validate(string? iban);

    /// <summary>
    /// Validates an IBAN character span and returns a detailed validation result without allocating strings.
    /// </summary>
    /// <param name="iban">The IBAN character span to validate.</param>
    /// <returns>An <see cref="IbanValidationResult"/> describing the outcome and specific validation errors.</returns>
    IbanValidationResult Validate(ReadOnlySpan<char> iban);
}

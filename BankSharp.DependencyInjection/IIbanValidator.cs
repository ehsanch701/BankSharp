using System.Diagnostics.CodeAnalysis;

namespace BankSharp.Extensions.DependencyInjection;

/// <summary>
/// Defines an abstraction for IBAN validation and parsing operations.
/// </summary>
public interface IIbanValidator
{
    /// <summary>
    /// Determines whether the specified IBAN string is valid.
    /// </summary>
    /// <param name="iban">The IBAN string to validate.</param>
    /// <returns><see langword="true"/> if the IBAN is valid; otherwise, <see langword="false"/>.</returns>
    bool IsValid(string? iban);

    /// <summary>
    /// Attempts to parse the specified string representation into an <see cref="Iban"/> instance.
    /// </summary>
    /// <param name="input">The string to parse.</param>
    /// <param name="iban">When this method returns, contains the parsed IBAN if successful; otherwise, the default value.</param>
    /// <returns><see langword="true"/> if parsing succeeded; otherwise, <see langword="false"/>.</returns>
    bool TryParse([NotNullWhen(true)] string? input, out Iban iban);

    /// <summary>
    /// Performs comprehensive validation on the specified IBAN string and returns detailed error information.
    /// </summary>
    /// <param name="iban">The IBAN string to validate.</param>
    /// <returns>An <see cref="IbanValidationResult"/> describing the outcome of the validation.</returns>
    IbanValidationResult Validate(string? iban);
}

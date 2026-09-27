using System.Diagnostics.CodeAnalysis;

namespace BankSharp.Extensions.DependencyInjection;

/// <summary>
/// Default implementation of <see cref="IIbanValidator"/> backed by high-performance BankSharp core components.
/// </summary>
public sealed class BankSharpIbanValidator : IIbanValidator
{
    /// <inheritdoc />
    public bool IsValid(string? iban)
    {
        return IbanValidator.Validate(iban).IsValid;
    }

    /// <inheritdoc />
    public bool IsValid(ReadOnlySpan<char> iban)
    {
        return IbanValidator.Validate(iban).IsValid;
    }

    /// <inheritdoc />
    public bool TryParse([NotNullWhen(true)] string? input, out Iban iban)
    {
        // Passes null as IFormatProvider to apply standard invariant IBAN formatting rules
        return Iban.TryParse(input, provider: null, out iban);
    }

    /// <inheritdoc />
    public bool TryParse(ReadOnlySpan<char> input, out Iban iban)
    {
        return Iban.TryParse(input, provider: null, out iban);
    }

    /// <inheritdoc />
    public IbanValidationResult Validate(string? iban)
    {
        return IbanValidator.Validate(iban);
    }

    /// <inheritdoc />
    public IbanValidationResult Validate(ReadOnlySpan<char> iban)
    {
        return IbanValidator.Validate(iban);
    }
}

using System;
using System.Diagnostics.CodeAnalysis;
using System.Runtime.CompilerServices;

namespace BankSharp.Extensions.DependencyInjection;

/// <summary>
/// Default implementation of <see cref="IIbanValidator"/> backed by high-performance BankSharp core components.
/// Optimized for hot-path execution with inline hints.
/// </summary>
public sealed class BankSharpIbanValidator : IIbanValidator
{
    /// <inheritdoc />
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public bool IsValid(string? iban) => IbanValidator.Validate(iban).IsValid;

    /// <inheritdoc />
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public bool IsValid(ReadOnlySpan<char> iban) => IbanValidator.Validate(iban).IsValid;

    /// <inheritdoc />
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public bool TryParse([NotNullWhen(true)] string? input, out Iban iban)
        => Iban.TryParse(input, provider: null, out iban);

    /// <inheritdoc />
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public bool TryParse(ReadOnlySpan<char> input, out Iban iban)
        => Iban.TryParse(input, provider: null, out iban);

    /// <inheritdoc />
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public IbanValidationResult Validate(string? iban) => IbanValidator.Validate(iban);

    /// <inheritdoc />
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public IbanValidationResult Validate(ReadOnlySpan<char> iban) => IbanValidator.Validate(iban);
}

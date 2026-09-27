using System.Linq.Expressions;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;

namespace BankSharp.EntityFrameworkCore;

/// <summary>
/// Converts <see cref="Iban"/> to its normalized string representation for database persistence and vice versa.
/// </summary>
public class IbanValueConverter : ValueConverter<Iban, string>
{
    private static readonly ConverterMappingHints DefaultHints = new(
        size: 34,
        unicode: false);

    private static readonly Expression<Func<Iban, string>> ToProviderExpr =
        iban => iban.IsEmpty ? string.Empty : iban.Value;

    private static readonly Expression<Func<string, Iban>> FromProviderExpr =
        value => string.IsNullOrWhiteSpace(value) ? default : Iban.Parse(value, null);

    /// <summary>
    /// Initializes a new instance of the <see cref="IbanValueConverter"/> class.
    /// </summary>
    public IbanValueConverter()
        : this(null)
    {
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="IbanValueConverter"/> class with specified mapping hints.
    /// </summary>
    /// <param name="mappingHints">Optional mapping hints for database providers.</param>
    public IbanValueConverter(ConverterMappingHints? mappingHints)
        : base(
            ToProviderExpr,
            FromProviderExpr,
            DefaultHints.With(mappingHints))
    {
    }
}

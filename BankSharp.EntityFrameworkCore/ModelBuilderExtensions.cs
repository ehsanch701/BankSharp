using Microsoft.EntityFrameworkCore;

namespace BankSharp.EntityFrameworkCore;

/// <summary>
/// Provides extension methods for <see cref="ModelBuilder"/> to configure EF Core mappings for BankSharp types.
/// </summary>
public static class ModelBuilderExtensions
{
    /// <summary>
    /// Configures all entity properties of type <see cref="Iban"/> to use <see cref="IbanValueConverter"/>.
    /// </summary>
    /// <param name="modelBuilder">The <see cref="ModelBuilder"/> being configured.</param>
    /// <returns>The same <paramref name="modelBuilder"/> instance so that multiple calls can be chained.</returns>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="modelBuilder"/> is <see langword="null"/>.</exception>
    public static ModelBuilder UseIbanConversions(this ModelBuilder modelBuilder)
    {
        ArgumentNullException.ThrowIfNull(modelBuilder);

        foreach (var entityType in modelBuilder.Model.GetEntityTypes())
        {
            var ibanProperties = entityType.GetProperties()
                .Where(p => p.ClrType == typeof(Iban) || p.ClrType == typeof(Iban?));

            foreach (var property in ibanProperties)
            {
                property.SetValueConverter(typeof(IbanValueConverter));
            }
        }

        return modelBuilder;
    }
}

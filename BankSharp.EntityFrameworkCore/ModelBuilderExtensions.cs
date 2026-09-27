using Microsoft.EntityFrameworkCore;

namespace BankSharp.EntityFrameworkCore;

public static class ModelBuilderExtensions
{
    /// <summary>
    /// Configures all properties of type <see cref="Iban"/> to use <see cref="IbanValueConverter"/>.
    /// </summary>
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

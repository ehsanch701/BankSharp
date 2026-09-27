using BankSharp.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;

// Placing extension methods directly under Microsoft.Extensions.DependencyInjection
// provides first-class discoverability in Program.cs without requiring extra using directives.
namespace Microsoft.Extensions.DependencyInjection;

/// <summary>
/// Extension methods for setting up BankSharp services in an <see cref="IServiceCollection"/>.
/// </summary>
public static class BankSharpServiceCollectionExtensions
{
    /// <summary>
    /// Registers BankSharp IBAN services (<see cref="IIbanValidator"/>) as a Singleton in the service collection.
    /// </summary>
    /// <param name="services">The <see cref="IServiceCollection"/> to add services to.</param>
    /// <returns>The same service collection so that multiple calls can be chained.</returns>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="services"/> is null.</exception>
    public static IServiceCollection AddBankSharp(this IServiceCollection services)
    {
        ArgumentNullException.ThrowIfNull(services);

        services.TryAddSingleton<IIbanValidator, BankSharpIbanValidator>();

        return services;
    }
}

using System;
using Microsoft.Extensions.DependencyInjection;

#if NET9_0_OR_GREATER
using Microsoft.AspNetCore.OpenApi;
#else
using Swashbuckle.AspNetCore.SwaggerGen;
#endif

namespace BankSharp.OpenApi;

/// <summary>
/// Extension methods for configuring OpenAPI and Swagger options with BankSharp IBAN schema support.
/// </summary>
public static class OpenApiOptionsExtensions
{
#if NET9_0_OR_GREATER
    /// <summary>
    /// Adds IBAN schema transformer and metadata support to ASP.NET Core native OpenAPI (NET 9 / NET 10).
    /// </summary>
    /// <param name="options">The OpenAPI options to configure.</param>
    /// <returns>The same <see cref="OpenApiOptions"/> instance so that multiple calls can be chained.</returns>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="options"/> is null.</exception>
    public static OpenApiOptions AddIbanSupport(this OpenApiOptions options)
    {
        ArgumentNullException.ThrowIfNull(options);
        options.AddSchemaTransformer<IbanSchemaTransformer>();
        return options;
    }

    /// <summary>
    /// Adds IBAN schema transformer and metadata support to ASP.NET Core native OpenAPI (NET 9 / NET 10).
    /// Alias for <see cref="AddIbanSupport(OpenApiOptions)"/>.
    /// </summary>
    /// <param name="options">The OpenAPI options to configure.</param>
    /// <returns>The same <see cref="OpenApiOptions"/> instance so that multiple calls can be chained.</returns>
    public static OpenApiOptions AddIbanSchemaTransformer(this OpenApiOptions options)
        => options.AddIbanSupport();

#else
    /// <summary>
    /// Adds IBAN schema filter to Swashbuckle SwaggerGen (NET 8).
    /// </summary>
    /// <param name="options">The SwaggerGen options to configure.</param>
    /// <returns>The same <see cref="SwaggerGenOptions"/> instance so that multiple calls can be chained.</returns>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="options"/> is null.</exception>
    public static SwaggerGenOptions AddIbanSupport(this SwaggerGenOptions options)
    {
        ArgumentNullException.ThrowIfNull(options);
        options.SchemaFilter<IbanSchemaTransformer>();
        return options;
    }

    /// <summary>
    /// Adds IBAN schema filter to Swashbuckle SwaggerGen (NET 8).
    /// Alias for <see cref="AddIbanSupport(SwaggerGenOptions)"/>.
    /// </summary>
    /// <param name="options">The SwaggerGen options to configure.</param>
    /// <returns>The same <see cref="SwaggerGenOptions"/> instance so that multiple calls can be chained.</returns>
    public static SwaggerGenOptions AddIbanSchemaTransformer(this SwaggerGenOptions options)
        => options.AddIbanSupport();
#endif
}

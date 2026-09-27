using System.Reflection;
using BankSharp.Validation;

#if NET10_0_OR_GREATER
using Microsoft.AspNetCore.OpenApi;
using Microsoft.OpenApi;
using System.Text.Json.Nodes;
#elif NET9_0
using Microsoft.AspNetCore.OpenApi;
using Microsoft.OpenApi.Models;
using Microsoft.OpenApi.Any;
#elif NET8_0
using Swashbuckle.AspNetCore.SwaggerGen;
using Microsoft.OpenApi.Models;
using Microsoft.OpenApi.Any;
#endif

namespace BankSharp.OpenApi;

/// <summary>
/// OpenAPI schema transformer/filter for IBAN types and properties annotated with [Iban].
/// Automatically injects IBAN schema metadata (format, min/length, description, and sample value)
/// across .NET 8 (Swashbuckle), .NET 9, and .NET 10 (Native OpenAPI).
/// </summary>
#if NET9_0_OR_GREATER
public sealed class IbanSchemaTransformer : IOpenApiSchemaTransformer
{
    private const string SampleIban = "IR660170000000123456789012";

    /// <inheritdoc />
    public Task TransformAsync(OpenApiSchema schema, OpenApiSchemaTransformerContext context, CancellationToken cancellationToken)
    {
        var targetType = context.JsonTypeInfo.Type;
        var memberInfo = context.JsonPropertyInfo?.AttributeProvider as MemberInfo;

        var hasIbanAttribute = memberInfo?.GetCustomAttribute<IbanAttribute>() != null;
        var isIbanType = targetType == typeof(Iban) || targetType == typeof(Iban?);

        if (hasIbanAttribute || isIbanType)
        {
            ApplyIbanMetadata(schema);
        }

        return Task.CompletedTask;
    }

    private static void ApplyIbanMetadata(OpenApiSchema schema)
    {
        schema.Format = "iban";
        schema.MinLength = 15;
        schema.MaxLength = 34;
        schema.Description = "International Bank Account Number (ISO 13616 standard).";

#if NET10_0_OR_GREATER
        // In .NET 10 with Microsoft.OpenApi 2.x, types use JsonSchemaType and Examples use IList<JsonNode>
        schema.Type = JsonSchemaType.String;
        schema.Examples = [JsonValue.Create(SampleIban)!];
#else
        // In .NET 9 with Microsoft.OpenApi 1.6.x object model
        schema.Type = "string";
        schema.Example = new OpenApiString(SampleIban);
#endif
    }
}
#else
// Implementation for .NET 8 using Swashbuckle ISchemaFilter
public sealed class IbanSchemaTransformer : ISchemaFilter
{
    private const string SampleIban = "IR660170000000123456789012";

    /// <inheritdoc />
    public void Apply(OpenApiSchema schema, SchemaFilterContext context)
    {
        var targetType = context.Type;
        var memberInfo = context.MemberInfo;

        var hasIbanAttribute = memberInfo?.GetCustomAttribute<IbanAttribute>() != null;
        var isIbanType = targetType == typeof(Iban) || targetType == typeof(Iban?);

        if (hasIbanAttribute || isIbanType)
        {
            schema.Format = "iban";
            schema.MinLength = 15;
            schema.MaxLength = 34;
            schema.Description = "International Bank Account Number (ISO 13616 standard).";
            schema.Type = "string";
            schema.Example = new OpenApiString(SampleIban);
        }
    }
}
#endif

using System.ComponentModel.DataAnnotations;

namespace BankSharp.Validation;

/// <summary>
/// Specifies that a data field value must be a valid International Bank Account Number (IBAN).
/// Optimized for zero-allocation model validation with built-in ASP.NET Core DataAnnotations integration.
/// </summary>
/// <remarks>
/// Null or whitespace values are treated as valid to allow composition with <see cref="RequiredAttribute"/>.
/// </remarks>
[AttributeUsage(AttributeTargets.Property | AttributeTargets.Field | AttributeTargets.Parameter, AllowMultiple = false)]
public sealed class IbanAttribute : ValidationAttribute
{
    /// <summary>
    /// Initializes a new instance of the <see cref="IbanAttribute"/> class with a default error message.
    /// </summary>
    public IbanAttribute()
        : base("The field {0} is not a valid IBAN.")
    {
    }

    /// <summary>
    /// Determines whether the specified value of the object is a valid IBAN.
    /// </summary>
    /// <param name="value">The value of the object to validate.</param>
    /// <returns>
    /// <see langword="true"/> if the specified value is <see langword="null"/>, whitespace, or a valid IBAN; otherwise, <see langword="false"/>.
    /// </returns>
    public override bool IsValid(object? value)
    {
        if (value is null)
        {
            // Nulls are considered valid; use [Required] alongside [Iban] if needed.
            return true;
        }

        if (value is string ibanString)
        {
            if (string.IsNullOrWhiteSpace(ibanString))
            {
                return true;
            }

            return IbanValidator.Validate(ibanString.AsSpan()).IsValid;
        }

        return false;
    }

    /// <summary>
    /// Validates the specified value with respect to the current validation attribute and execution context.
    /// </summary>
    /// <param name="value">The value to validate.</param>
    /// <param name="validationContext">The context information about the validation operation.</param>
    /// <returns>
    /// An instance of <see cref="ValidationResult"/> indicating success or containing an error message.
    /// </returns>
    protected override ValidationResult? IsValid(object? value, ValidationContext validationContext)
    {
        if (IsValid(value))
        {
            return ValidationResult.Success;
        }

        var memberNames = validationContext.MemberName is not null
            ? new[] { validationContext.MemberName }
            : null;

        return new ValidationResult(FormatErrorMessage(validationContext.DisplayName), memberNames);
    }
}

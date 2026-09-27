using System.ComponentModel.DataAnnotations;
using BankSharp.Validation;

namespace BankSharp.Tests;

/// <summary>
/// Contains unit tests for <see cref="IbanAttribute"/> to verify standard validation behavior.
/// </summary>
public class IbanAttributeTests
{
    private readonly IbanAttribute _attribute = new();

    [Fact]
    public void IsValid_WithValidIban_ReturnsTrue()
    {
        // Act: Validate a correctly formatted IBAN
        var result = _attribute.IsValid("IR020120000000001083758362");

        // Assert: Should be valid
        Assert.True(result);
    }

    [Fact]
    public void IsValid_WithInvalidIban_ReturnsFalse()
    {
        // Act: Validate an incorrectly formatted IBAN
        var result = _attribute.IsValid("IR000120000000001083758362");

        // Assert: Should be invalid
        Assert.False(result);
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData(" ")]
    [InlineData("   \t ")]
    public void IsValid_WithNullOrWhitespace_ReturnsTrue(string? value)
    {
        // Note: Validation attributes generally treat null/empty as valid.
        // Use [Required] attribute alongside [Iban] for strict enforcement.
        Assert.True(_attribute.IsValid(value));
    }

    [Fact]
    public void IsValid_WithNonString_ReturnsFalse()
    {
        // Act & Assert: Ensure it gracefully handles non-string types
        Assert.False(_attribute.IsValid(12345));
        Assert.False(_attribute.IsValid(new object()));
    }

    [Fact]
    public void IsValid_WithValidationContext_WhenInvalid_ReturnsValidationResultWithDetails()
    {
        // Arrange
        var model = new { AccountIban = "INVALID_IBAN" };
        var context = new ValidationContext(model)
        {
            MemberName = nameof(model.AccountIban),
            DisplayName = "Account IBAN"
        };

        // Act
        var result = _attribute.GetValidationResult(model.AccountIban, context);

        // Assert: Verify failure and error details
        Assert.NotNull(result);
        Assert.NotEqual(ValidationResult.Success, result);
        Assert.Equal("The field Account IBAN is not a valid IBAN.", result.ErrorMessage);
        Assert.Contains(nameof(model.AccountIban), result.MemberNames);
    }

    [Fact]
    public void IsValid_WithValidationContext_WhenValid_ReturnsSuccess()
    {
        // Arrange
        var model = new { AccountIban = "IR020120000000001083758362" };
        var context = new ValidationContext(model)
        {
            MemberName = nameof(model.AccountIban)
        };

        // Act
        var result = _attribute.GetValidationResult(model.AccountIban, context);

        // Assert: Should return success
        Assert.Equal(ValidationResult.Success, result);
    }
}

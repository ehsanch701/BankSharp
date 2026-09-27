using BankSharp.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection;

namespace BankSharp.Tests;

/// <summary>
/// Unit tests for the Dependency Injection registration logic.
/// Verifies that services are correctly registered and resolved via <see cref="IIbanValidator"/>.
/// </summary>
public class DependencyInjectionTests
{
    /// <summary>
    /// Verifies that the validator is registered as a singleton instance in the DI container.
    /// </summary>
    [Fact]
    public void AddBankSharp_RegistersIbanValidatorAsSingleton()
    {
        // Arrange: Prepare an empty service collection
        var services = new ServiceCollection();

        // Act: Register BankSharp services and build the provider
        services.AddBankSharp();
        var provider = services.BuildServiceProvider();

        // Assert: Ensure both resolved instances are exactly the same object
        var validator1 = provider.GetService<IIbanValidator>();
        var validator2 = provider.GetService<IIbanValidator>();

        Assert.NotNull(validator1);
        Assert.NotNull(validator2);
        Assert.Same(validator1, validator2); // Must be a Singleton
    }

    /// <summary>
    /// Ensures that calling AddBankSharp with a null collection throws an ArgumentNullException.
    /// </summary>
    [Fact]
    public void AddBankSharp_NullServices_ThrowsArgumentNullException()
    {
        // Arrange: Define null services
        IServiceCollection services = null!;

        // Act & Assert: Verify that the expected exception is thrown
        Assert.Throws<ArgumentNullException>(() => services.AddBankSharp());
    }

    /// <summary>
    /// Checks the accuracy of the resolved <see cref="IIbanValidator"/> for various IBAN inputs.
    /// </summary>
    /// <param name="iban">The IBAN string to validate.</param>
    /// <param name="expected">Expected validation result (true/false).</param>
    [Theory]
    [InlineData("GB82WEST12345698765432", true)]
    [InlineData("IR020120000000001083758362", true)]
    [InlineData("IR000000000000000000000000", false)]
    public void IIbanValidator_IsValid_WorksAccurately(string iban, bool expected)
    {
        // Arrange: Resolve validator from DI
        var services = new ServiceCollection();
        services.AddBankSharp();
        var provider = services.BuildServiceProvider();
        var validator = provider.GetRequiredService<IIbanValidator>();

        // Act & Assert: Validate accuracy
        Assert.Equal(expected, validator.IsValid(iban));
    }

    /// <summary>
    /// Verifies that the resolved validator returns a comprehensive <see cref="IbanValidationResult"/>.
    /// </summary>
    [Fact]
    public void IIbanValidator_Validate_ReturnsDetailedResult()
    {
        // Arrange: Resolve validator
        var services = new ServiceCollection();
        services.AddBankSharp();
        var provider = services.BuildServiceProvider();
        var validator = provider.GetRequiredService<IIbanValidator>();

        // Act: Perform validation
        var result = validator.Validate("IR020120000000001083758362");

        // Assert: Ensure result properties are correct
        Assert.True(result.IsValid);
        Assert.Equal(IbanValidationError.None, result.Error);
    }

    /// <summary>
    /// Ensures that the resolved validator's <see cref="IIbanValidator.TryParse"/> handles formatted strings correctly.
    /// </summary>
    [Fact]
    public void IIbanValidator_TryParse_ReturnsParsedIban()
    {
        // Arrange: Resolve validator
        var services = new ServiceCollection();
        services.AddBankSharp();
        var provider = services.BuildServiceProvider();
        var validator = provider.GetRequiredService<IIbanValidator>();

        // Act: Attempt to parse an IBAN with spaces
        bool success = validator.TryParse("IR02 0120 0000 0000 1083 7583 62", out var iban);

        // Assert: Verify parsing success and integrity
        Assert.True(success);
        Assert.Equal("IR", iban.CountryCode);
        Assert.Equal("02", iban.CheckDigits);
        // Verify that the BBAN part is extracted correctly after country+checkDigits
        Assert.Equal("0120000000001083758362", iban.Value[4..]);
    }
}

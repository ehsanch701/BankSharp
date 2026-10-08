using FluentValidation;
using FluentValidation.TestHelper;
using BankSharp.FluentValidation;

namespace BankSharp.Tests
{
    /// <summary>
    /// Unit tests for FluentValidation extensions related to IBAN validation.
    /// </summary>
    public class IbanValidationExtensionsTests
    {
        // Valid IBANs using the MOD-97 algorithm for testing purposes
        private const string ValidGermanIban = "DE89370400440532013000";
        private const string ValidIranianIban = "IR820540102680020817909002";

        // Simple DTO representing a model that requires IBAN validation
        public class AccountModel
        {
            public string? Iban { get; set; }
        }

        // Validator implementation using the extension method under test
        public class AccountValidator : AbstractValidator<AccountModel>
        {
            public AccountValidator(params string[] allowedCountries)
            {
                RuleFor(x => x.Iban).MustBeValidIban(allowedCountries);
            }
        }

        [Fact]
        public void Validate_ValidIban_ShouldNotHaveValidationError()
        {
            // Arrange: Initialize validator and valid model
            var validator = new AccountValidator();
            var model = new AccountModel { Iban = ValidGermanIban };

            // Act: Perform validation
            var result = validator.TestValidate(model);

            // Assert: Verify no errors occurred for a valid IBAN
            result.ShouldNotHaveValidationErrorFor(x => x.Iban);
        }

        [Fact]
        public void Validate_InvalidChecksum_ShouldHaveValidationError()
        {
            // Arrange: Initialize validator with an IBAN having invalid check digits
            var validator = new AccountValidator();
            var model = new AccountModel { Iban = "DE00370400440532013000" };

            // Act: Perform validation
            var result = validator.TestValidate(model);

            // Assert: Verify that validation fails due to checksum error
            result.ShouldHaveValidationErrorFor(x => x.Iban);
        }

        [Fact]
        public void Validate_RestrictedAllowedCountries_ShouldRejectUnauthorizedCountry()
        {
            // Arrange: Validator configured to only accept Iran (IR)
            var validator = new AccountValidator("IR");
            var model = new AccountModel { Iban = ValidGermanIban };

            // Act: Attempt to validate a German IBAN
            var result = validator.TestValidate(model);

            // Assert: Validation should fail because the country is not authorized
            result.ShouldHaveValidationErrorFor(x => x.Iban);
        }

        [Fact]
        public void Validate_AllowedCountry_ShouldPass()
        {
            // Arrange: Validator configured to accept Germany (DE) or France (FR)
            var validator = new AccountValidator("DE", "FR");
            var model = new AccountModel { Iban = ValidGermanIban };

            // Act: Perform validation
            var result = validator.TestValidate(model);

            // Assert: Validation should pass for authorized countries
            result.ShouldNotHaveValidationErrorFor(x => x.Iban);
        }
    }
}

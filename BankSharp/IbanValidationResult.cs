namespace BankSharp;

/// <summary>
/// Specifies the specific reason why an IBAN validation failed.
/// </summary>
public enum IbanValidationError
{
    /// <summary>
    /// The IBAN is valid and passed all checks.
    /// </summary>
    None = 0,

    /// <summary>
    /// The provided IBAN is null, empty, or consists only of white-space characters.
    /// </summary>
    EmptyOrNull = 1,

    /// <summary>
    /// The length of the IBAN does not match the required standard or the country-specific length.
    /// </summary>
    InvalidLength = 2,

    /// <summary>
    /// The country code prefix does not consist of two uppercase letters.
    /// </summary>
    InvalidCountryCode = 3,

    /// <summary>
    /// The country code is not registered or supported in the IBAN registry.
    /// </summary>
    UnsupportedCountry = 4,

    /// <summary>
    /// The IBAN contains illegal non-alphanumeric characters.
    /// </summary>
    InvalidCharacters = 5,

    /// <summary>
    /// The internal structure or BBAN does not match the country's specific pattern.
    /// </summary>
    InvalidStructure = 6,

    /// <summary>
    /// The ISO 7064 Mod 97-10 checksum digits are mathematically invalid.
    /// </summary>
    InvalidCheckDigits = 7
}

/// <summary>
/// Represents an immutable, zero-allocation validation result containing status and diagnostic details.
/// </summary>
public readonly record struct IbanValidationResult
{
    /// <summary>
    /// Gets a value indicating whether the IBAN validation was successful.
    /// </summary>
    public bool IsValid => Error == IbanValidationError.None;

    /// <summary>
    /// Gets the specific validation error reason, or <see cref="IbanValidationError.None"/> if valid.
    /// </summary>
    public IbanValidationError Error { get; }

    /// <summary>
    /// Gets the two-letter ISO country code associated with the evaluated IBAN, if resolvable.
    /// </summary>
    public string? CountryCode { get; }

    /// <summary>
    /// Gets a human-readable English description of the validation error.
    /// </summary>
    public string ErrorMessage => Error switch
    {
        IbanValidationError.None => "The IBAN is valid.",
        IbanValidationError.EmptyOrNull => "The IBAN cannot be null, empty, or contain only whitespace.",
        IbanValidationError.InvalidLength => "The IBAN length is invalid.",
        IbanValidationError.InvalidCountryCode => "The IBAN country code is invalid.",
        IbanValidationError.UnsupportedCountry => $"The country code '{CountryCode}' is not supported.",
        IbanValidationError.InvalidCharacters => "The IBAN contains invalid characters.",
        IbanValidationError.InvalidStructure => "The IBAN format does not match the country structure requirements.",
        IbanValidationError.InvalidCheckDigits => "The IBAN Mod-97 check digits are invalid.",
        _ => "An unknown validation error occurred."
    };

    private IbanValidationResult(IbanValidationError error, string? countryCode = null)
    {
        Error = error;
        CountryCode = countryCode;
    }

    /// <summary>
    /// Creates a successful validation result.
    /// </summary>
    /// <param name="countryCode">The validated two-letter country code.</param>
    /// <returns>A successful <see cref="IbanValidationResult"/>.</returns>
    public static IbanValidationResult Success(string countryCode) =>
        new(IbanValidationError.None, countryCode);

    /// <summary>
    /// Creates a failed validation result with the specified error reason.
    /// </summary>
    /// <param name="error">The validation error reason.</param>
    /// <param name="countryCode">The optional country code identified during validation.</param>
    /// <returns>A failed <see cref="IbanValidationResult"/>.</returns>
    public static IbanValidationResult Failure(IbanValidationError error, string? countryCode = null) =>
        new(error, countryCode);
}

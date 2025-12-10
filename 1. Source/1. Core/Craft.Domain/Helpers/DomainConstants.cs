namespace Craft.Domain;

/// <summary>
/// Provides constant values for domain validation, including error messages and regular expressions.
/// </summary>
public static class DomainConstants
{
    #region Error Messages

    /// <summary>
    /// Error message format for date/time validation when a value must occur after another.
    /// </summary>
    public const string AfterError = "{0} must be after {1}";

    /// <summary>
    /// Error message format for length validation when a value exceeds maximum length.
    /// </summary>
    public const string LengthError = "{0} cannot be more than {1}";

    /// <summary>
    /// Error message format for required field validation.
    /// </summary>
    public const string RequiredError = "{0} is required";

    /// <summary>
    /// Error message for date range validation when start date must precede end date.
    /// </summary>
    public const string StartDateGreaterError = "Start must be before End";

    /// <summary>
    /// Error message format for range validation.
    /// </summary>
    public const string RangeError = "{0} must be between {1} and {2}";

    /// <summary>
    /// Error message format for minimum length validation.
    /// </summary>
    public const string MinLengthError = "{0} must be at least {1} characters";

    /// <summary>
    /// Error message format for maximum length validation.
    /// </summary>
    public const string MaxLengthError = "{0} must not exceed {1} characters";

    /// <summary>
    /// Error message format for alphabetic and special character validation.
    /// </summary>
    public const string AlphabetAndSpecialCharError = "{0} must contain only alphabets (1st Uppercase) and ,.'() ";

    /// <summary>
    /// Error message format for alphanumeric validation with first character uppercase.
    /// </summary>
    public const string AlphaNumericError = "{0} must contain only alphanumeric characters (1st uppercase)";

    /// <summary>
    /// Error message format for uppercase letter validation.
    /// </summary>
    public const string CapitalAlphabetError = "{0} must contain only uppercase letters";

    /// <summary>
    /// Error message format for whole number validation.
    /// </summary>
    public const string OnlyDigitError = "{0} must be a whole number";

    /// <summary>
    /// Error message format for numeric validation.
    /// </summary>
    public const string OnlyNumberError = "{0} must be a valid number";

    /// <summary>
    /// Error message format for alphanumeric code with hyphen validation.
    /// </summary>
    public const string AlphaNumCodeWithHyphenRegExError = "{0} must contain only capital letters, digits and hyphen";

    /// <summary>
    /// Error message format for general format validation.
    /// </summary>
    public const string FormatError = "{0} not in valid format";

    /// <summary>
    /// Error message for duplicate value validation.
    /// </summary>
    public const string DuplicateError = "Value Should not be Same";

    #endregion

    #region Regular Expressions

    /// <summary>
    /// Regular expression for validating names (first character uppercase, allows alphabets and special characters like .'()).
    /// </summary>
    public const string NameRegExpr = @"^[A-Z][A-Za-z\.\'\(\) ]*";

    /// <summary>
    /// Regular expression for validating uppercase alphabetic codes.
    /// </summary>
    public const string AlphaCodeRegEx = "^[A-Z]+";

    /// <summary>
    /// Regular expression for validating alphanumeric codes (first character uppercase).
    /// </summary>
    public const string AlphaNumCodeRegEx = "^[A-Z][A-Z0-9]*";

    /// <summary>
    /// Regular expression for validating digit-only strings.
    /// </summary>
    public const string DigitsRegEx = "^[0-9]+";

    /// <summary>
    /// Regular expression for validating positive integers (no leading zeros).
    /// </summary>
    public const string DigitsGreaterThanZero = "^[1-9]+";

    /// <summary>
    /// Regular expression for validating numbers (integers and decimals).
    /// </summary>
    public const string NumberRegEx = "(^[0-9]*[.])?[0-9]+";

    /// <summary>
    /// Regular expression for validating alphanumeric codes with hyphens.
    /// </summary>
    public const string AlphaNumCodeWithHyphenRegEx = @"[A-Z]*[0-9\-]*";

    /// <summary>
    /// Regular expression for validating email addresses according to RFC standards.
    /// </summary>
    public const string EmailRegExpr = @"^[a-zA-Z0-9.!#$%&'*+/=?^_`{|}~-]+@[a-zA-Z0-9](?:[a-zA-Z0-9-]{0,61}[a-zA-Z0-9])?(?:\.[a-zA-Z0-9](?:[a-zA-Z0-9-]{0,61}[a-zA-Z0-9])?)*$";

    /// <summary>
    /// Regular expression for validating phone numbers (allows +, digits, spaces, hyphens, and parentheses).
    /// </summary>
    public const string PhoneRegExpr = @"^\+?[\d\s\-\(\)]+$";

    /// <summary>
    /// Regular expression for validating URLs (HTTP/HTTPS).
    /// </summary>
    public const string UrlRegExpr = @"^https?://[^\s]+$";

    #endregion
}

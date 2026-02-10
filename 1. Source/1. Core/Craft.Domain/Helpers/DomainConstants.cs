using Craft.Domain.Resources;

namespace Craft.Domain;

/// <summary>
/// Provides constant values for domain validation, including error messages and regular expressions.
/// </summary>
/// <remarks>
/// <para><b>Error Messages:</b> The <c>const</c> fields are provided for use with validation attributes 
/// (e.g., [Required(ErrorMessage = ...)]) which require compile-time constants.</para>
/// <para><b>Localization:</b> For runtime localization, use <see cref="DomainResources"/> directly 
/// or the <see cref="Localized"/> nested class which provides the same messages backed by resources.</para>
/// </remarks>
public static class DomainConstants
{
    #region Error Messages (Compile-time Constants for Attributes)

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
    public const string DuplicateError = "Value should not be same";

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

    #region Localized Error Messages

    /// <summary>
    /// Provides localized versions of error messages backed by <see cref="DomainResources"/>.
    /// </summary>
    /// <remarks>
    /// <para>Use this class when you need runtime localization of error messages.</para>
    /// <para>For validation attributes, use the parent class constants instead.</para>
    /// <para>
    /// <b>Usage example:</b>
    /// <code>
    /// // For attributes (compile-time constant required):
    /// [Required(ErrorMessage = DomainConstants.RequiredError)]
    /// 
    /// // For runtime localization:
    /// var message = DomainConstants.Localized.RequiredError;
    /// var formatted = DomainConstants.Localized.FormatRequired("FirstName");
    /// </code>
    /// </para>
    /// </remarks>
    public static class Localized
    {
        /// <summary>Gets the localized AfterError message.</summary>
        public static string AfterError => DomainResources.AfterError;

        /// <summary>Gets the localized LengthError message.</summary>
        public static string LengthError => DomainResources.LengthError;

        /// <summary>Gets the localized RequiredError message.</summary>
        public static string RequiredError => DomainResources.RequiredError;

        /// <summary>Gets the localized StartDateGreaterError message.</summary>
        public static string StartDateGreaterError => DomainResources.StartDateGreaterError;

        /// <summary>Gets the localized RangeError message.</summary>
        public static string RangeError => DomainResources.RangeError;

        /// <summary>Gets the localized MinLengthError message.</summary>
        public static string MinLengthError => DomainResources.MinLengthError;

        /// <summary>Gets the localized MaxLengthError message.</summary>
        public static string MaxLengthError => DomainResources.MaxLengthError;

        /// <summary>Gets the localized AlphabetAndSpecialCharError message.</summary>
        public static string AlphabetAndSpecialCharError => DomainResources.AlphabetAndSpecialCharError;

        /// <summary>Gets the localized AlphaNumericError message.</summary>
        public static string AlphaNumericError => DomainResources.AlphaNumericError;

        /// <summary>Gets the localized CapitalAlphabetError message.</summary>
        public static string CapitalAlphabetError => DomainResources.CapitalAlphabetError;

        /// <summary>Gets the localized OnlyDigitError message.</summary>
        public static string OnlyDigitError => DomainResources.OnlyDigitError;

        /// <summary>Gets the localized OnlyNumberError message.</summary>
        public static string OnlyNumberError => DomainResources.OnlyNumberError;

        /// <summary>Gets the localized AlphaNumCodeWithHyphenRegExError message.</summary>
        public static string AlphaNumCodeWithHyphenRegExError => DomainResources.AlphaNumCodeWithHyphenRegExError;

        /// <summary>Gets the localized FormatError message.</summary>
        public static string FormatError => DomainResources.FormatError;

        /// <summary>Gets the localized DuplicateError message.</summary>
        public static string DuplicateError => DomainResources.DuplicateError;

        #region Formatting Helper Methods

        /// <summary>Formats the AfterError message with the provided parameters.</summary>
        public static string FormatAfter(string fieldName, string comparisonFieldName)
            => DomainResources.FormatAfterError(fieldName, comparisonFieldName);

        /// <summary>Formats the LengthError message with the provided parameters.</summary>
        public static string FormatLength(string fieldName, int maxLength)
            => DomainResources.FormatLengthError(fieldName, maxLength);

        /// <summary>Formats the RequiredError message with the provided parameters.</summary>
        public static string FormatRequired(string fieldName)
            => DomainResources.FormatRequiredError(fieldName);

        /// <summary>Formats the RangeError message with the provided parameters.</summary>
        public static string FormatRange(string fieldName, object minValue, object maxValue)
            => DomainResources.FormatRangeError(fieldName, minValue, maxValue);

        /// <summary>Formats the MinLengthError message with the provided parameters.</summary>
        public static string FormatMinLength(string fieldName, int minLength)
            => DomainResources.FormatMinLengthError(fieldName, minLength);

        /// <summary>Formats the MaxLengthError message with the provided parameters.</summary>
        public static string FormatMaxLength(string fieldName, int maxLength)
            => DomainResources.FormatMaxLengthError(fieldName, maxLength);

        #endregion
    }

    #endregion
}

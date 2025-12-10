namespace Craft.Domain;

public static class DomainConstants
{
    // Error Messages
    public const string AfterError = "{0} must be after {1}";
    public const string LengthError = "{0} cannot be more than {1}";
    public const string RequiredError = "{0} is required";
    public const string StartDateGreaterError = "Start must be before End";
    public const string RangeError = "{0} must be between {1} and {2}";
    public const string MinLengthError = "{0} must be at least {1} characters";
    public const string MaxLengthError = "{0} must not exceed {1} characters";

    public const string AlphabetAndSpecialCharError = "{0} must contain only alphabets (1st Uppercase) and ,.'() ";
    public const string AlphaNumericError = "{0} must contain only alphanumberic characters (1st uppercase)";
    public const string CapitalAlphabetError = "{0} must contan only uppercase letters";
    public const string OnlyDigitError = "{0} must be a whole number";
    public const string OnlyNumberError = "{0} must be a valid number";
    public const string AlphaNumCodeWithHyphenRegExError = "{0} must contain only capital letters, digits and hyphen";
    public const string FormatError = "{0} not in valid format";
    public const string DuplicateError = "Value Should not be Same";

    // Regular Expressions
    public const string NameRegExpr = @"^[A-Z][A-Za-z\.\'\(\) ]*";
    public const string AlphaCodeRegEx = "^[A-Z]+";
    public const string AlphaNumCodeRegEx = "^[A-Z][A-Z0-9]*";
    public const string DigitsRegEx = "^[0-9]+";
    public const string DigitsGreaterThanZero = "^[1-9]+";
    public const string NumberRegEx = "(^[0-9]*[.])?[0-9]+";
    public const string AlphaNumCodeWithHyphenRegEx = @"[A-Z]*[0-9\-]*";
    public const string EmailRegExpr = @"^[a-zA-Z0-9.!#$%&'*+/=?^_`{|}~-]+@[a-zA-Z0-9](?:[a-zA-Z0-9-]{0,61}[a-zA-Z0-9])?(?:\.[a-zA-Z0-9](?:[a-zA-Z0-9-]{0,61}[a-zA-Z0-9])?)*$";
    public const string PhoneRegExpr = @"^\+?[\d\s\-\(\)]+$";
    public const string UrlRegExpr = @"^https?://[^\s]+$";
}

# Proposed Utility Classes for Craft.Utilities

This document provides detailed implementation guidance for 10 additional utility classes that would complement the existing Craft.Utilities project.

---

## 1. StringHelper

### Purpose
Common string manipulation operations that are frequently needed across applications.

### Proposed Implementation

```csharp
namespace Craft.Utilities.Helpers;

/// <summary>
/// Provides common string manipulation utilities.
/// </summary>
public static class StringHelper
{
    /// <summary>
    /// Truncates a string to a maximum length and adds a suffix if truncated.
    /// </summary>
    public static string Truncate(string value, int maxLength, string suffix = "...")
    {
        if (string.IsNullOrEmpty(value) || value.Length <= maxLength)
            return value ?? string.Empty;

        return value[..(maxLength - suffix.Length)] + suffix;
    }

    /// <summary>
    /// Converts text to URL-friendly slug format.
    /// </summary>
    public static string ToSlug(string text)
    {
        if (string.IsNullOrWhiteSpace(text))
            return string.Empty;

        text = RemoveAccents(text.ToLowerInvariant());
        text = Regex.Replace(text, @"[^a-z0-9\s-]", string.Empty);
        text = Regex.Replace(text, @"\s+", " ").Trim();
        text = text.Replace(" ", "-");
        text = Regex.Replace(text, @"-+", "-");

        return text;
    }

    /// <summary>
    /// Converts string to title case (e.g., "hello world" ? "Hello World").
    /// </summary>
    public static string ToTitleCase(string text)
    {
        if (string.IsNullOrWhiteSpace(text))
            return string.Empty;

        return CultureInfo.CurrentCulture.TextInfo.ToTitleCase(text.ToLower());
    }

    /// <summary>
    /// Removes all HTML tags from a string.
    /// </summary>
    public static string RemoveHtmlTags(string html)
    {
        if (string.IsNullOrEmpty(html))
            return string.Empty;

        return Regex.Replace(html, "<.*?>", string.Empty);
    }

    /// <summary>
    /// Checks if all provided strings are null or whitespace.
    /// </summary>
    public static bool AreAllNullOrWhiteSpace(params string?[] values)
    {
        return values?.All(string.IsNullOrWhiteSpace) ?? true;
    }

    /// <summary>
    /// Joins non-empty strings with the specified separator.
    /// </summary>
    public static string JoinNonEmpty(string separator, params string?[] values)
    {
        return string.Join(separator, values?.Where(v => !string.IsNullOrWhiteSpace(v)) ?? []);
    }

    /// <summary>
    /// Repeats a string a specified number of times.
    /// </summary>
    public static string Repeat(string value, int count)
    {
        ArgumentOutOfRangeException.ThrowIfNegative(count);
        return count == 0 ? string.Empty : string.Concat(Enumerable.Repeat(value, count));
    }

    /// <summary>
    /// Reverses a string.
    /// </summary>
    public static string Reverse(string value)
    {
        if (string.IsNullOrEmpty(value))
            return value ?? string.Empty;

        return new string(value.ToCharArray().Reverse().ToArray());
    }

    /// <summary>
    /// Removes accents and diacritical marks from characters.
    /// </summary>
    public static string RemoveAccents(string text)
    {
        if (string.IsNullOrWhiteSpace(text))
            return text ?? string.Empty;

        var normalizedString = text.Normalize(NormalizationForm.FormD);
        var stringBuilder = new StringBuilder(capacity: normalizedString.Length);

        foreach (var c in normalizedString)
        {
            var unicodeCategory = CharUnicodeInfo.GetUnicodeCategory(c);
            if (unicodeCategory != UnicodeCategory.NonSpacingMark)
                stringBuilder.Append(c);
        }

        return stringBuilder.ToString().Normalize(NormalizationForm.FormC);
    }

    /// <summary>
    /// Masks a string, showing only the last N characters.
    /// </summary>
    public static string Mask(string value, int visibleChars = 4, char maskChar = '*')
    {
        if (string.IsNullOrEmpty(value))
            return value ?? string.Empty;

        if (value.Length <= visibleChars)
            return value;

        return new string(maskChar, value.Length - visibleChars) + value[^visibleChars..];
    }
}
```

### Test Coverage Requirements
- Truncate with various lengths and suffixes
- ToSlug with special characters, accents, spaces
- ToTitleCase with various cases
- RemoveHtmlTags with nested tags
- AreAllNullOrWhiteSpace with mixed inputs
- JoinNonEmpty with nulls and empty strings
- Repeat with 0, 1, and multiple counts
- Reverse with empty, single, multiple characters
- RemoveAccents with various accented characters
- Mask with various visible character counts

---

## 2. DateTimeHelper

### Purpose
Common date and time operations for business logic.

### Proposed Implementation

```csharp
namespace Craft.Utilities.Helpers;

/// <summary>
/// Provides common date and time utilities.
/// </summary>
public static class DateTimeHelper
{
    public static DateTime StartOfDay(DateTime date)
        => date.Date;

    public static DateTime EndOfDay(DateTime date)
        => date.Date.AddDays(1).AddTicks(-1);

    public static DateTime StartOfWeek(DateTime date, DayOfWeek startOfWeek = DayOfWeek.Monday)
    {
        int diff = (7 + (date.DayOfWeek - startOfWeek)) % 7;
        return date.AddDays(-1 * diff).Date;
    }

    public static DateTime EndOfWeek(DateTime date, DayOfWeek startOfWeek = DayOfWeek.Monday)
        => StartOfWeek(date, startOfWeek).AddDays(7).AddTicks(-1);

    public static DateTime StartOfMonth(DateTime date)
        => new(date.Year, date.Month, 1);

    public static DateTime EndOfMonth(DateTime date)
        => StartOfMonth(date).AddMonths(1).AddTicks(-1);

    public static int GetWeekOfYear(DateTime date, CalendarWeekRule rule = CalendarWeekRule.FirstFourDayWeek)
        => CultureInfo.CurrentCulture.Calendar.GetWeekOfYear(date, rule, DayOfWeek.Monday);

    public static bool IsWeekend(DateTime date)
        => date.DayOfWeek is DayOfWeek.Saturday or DayOfWeek.Sunday;

    public static bool IsBusinessDay(DateTime date, IEnumerable<DateTime>? holidays = null)
    {
        if (IsWeekend(date))
            return false;

        return holidays?.All(h => h.Date != date.Date) ?? true;
    }

    public static IEnumerable<DateTime> GetDateRange(DateTime start, DateTime end)
    {
        for (var date = start.Date; date <= end.Date; date = date.AddDays(1))
            yield return date;
    }

    public static string ToRelativeTime(DateTime date)
    {
        var timeSpan = DateTime.UtcNow - date.ToUniversalTime();

        return timeSpan switch
        {
            { TotalSeconds: < 60 } => "just now",
            { TotalMinutes: < 2 } => "1 minute ago",
            { TotalMinutes: < 60 } => $"{(int)timeSpan.TotalMinutes} minutes ago",
            { TotalHours: < 2 } => "1 hour ago",
            { TotalHours: < 24 } => $"{(int)timeSpan.TotalHours} hours ago",
            { TotalDays: < 2 } => "yesterday",
            { TotalDays: < 30 } => $"{(int)timeSpan.TotalDays} days ago",
            { TotalDays: < 60 } => "1 month ago",
            { TotalDays: < 365 } => $"{(int)(timeSpan.TotalDays / 30)} months ago",
            { TotalDays: < 730 } => "1 year ago",
            _ => $"{(int)(timeSpan.TotalDays / 365)} years ago"
        };
    }

    public static int GetAge(DateTime birthDate)
    {
        var today = DateTime.Today;
        var age = today.Year - birthDate.Year;
        if (birthDate.Date > today.AddYears(-age))
            age--;
        return age;
    }

    public static DateTime GetNextBusinessDay(DateTime date, IEnumerable<DateTime>? holidays = null)
    {
        var nextDay = date.AddDays(1);
        while (!IsBusinessDay(nextDay, holidays))
            nextDay = nextDay.AddDays(1);
        return nextDay;
    }
}
```

---

## 3. ValidationHelper

### Purpose
Common validation operations for user input and data.

### Proposed Implementation

```csharp
namespace Craft.Utilities.Validators;

/// <summary>
/// Provides common validation utilities.
/// </summary>
public static class ValidationHelper
{
    private static readonly Regex EmailRegex = new(
        @"^[^@\s]+@[^@\s]+\.[^@\s]+$",
        RegexOptions.Compiled | RegexOptions.IgnoreCase);

    public static bool IsValidEmail(string email)
    {
        if (string.IsNullOrWhiteSpace(email))
            return false;

        return EmailRegex.IsMatch(email) && MailAddress.TryCreate(email, out _);
    }

    public static bool IsValidPhoneNumber(string phoneNumber, string countryCode = "US")
    {
        if (string.IsNullOrWhiteSpace(phoneNumber))
            return false;

        var cleanNumber = Regex.Replace(phoneNumber, @"[\s\-\(\)\.]", string.Empty);

        return countryCode.ToUpper() switch
        {
            "US" => Regex.IsMatch(cleanNumber, @"^(\+?1)?[2-9]\d{9}$"),
            "UK" => Regex.IsMatch(cleanNumber, @"^(\+?44|0)7\d{9}$"),
            _ => !string.IsNullOrEmpty(cleanNumber) && cleanNumber.Length >= 10 && cleanNumber.Length <= 15
        };
    }

    public static bool IsValidCreditCard(string cardNumber)
    {
        if (string.IsNullOrWhiteSpace(cardNumber))
            return false;

        var cleanNumber = Regex.Replace(cardNumber, @"[\s\-]", string.Empty);

        if (!Regex.IsMatch(cleanNumber, @"^\d{13,19}$"))
            return false;

        // Luhn algorithm
        int sum = 0;
        bool alternate = false;
        for (int i = cleanNumber.Length - 1; i >= 0; i--)
        {
            int digit = cleanNumber[i] - '0';
            if (alternate)
            {
                digit *= 2;
                if (digit > 9)
                    digit -= 9;
            }
            sum += digit;
            alternate = !alternate;
        }

        return sum % 10 == 0;
    }

    public static bool IsValidIPAddress(string ipAddress)
        => !string.IsNullOrWhiteSpace(ipAddress) && IPAddress.TryParse(ipAddress, out _);

    public static bool IsStrongPassword(string password, int minLength = 8)
    {
        if (string.IsNullOrEmpty(password) || password.Length < minLength)
            return false;

        return password.Any(char.IsUpper) &&
               password.Any(char.IsLower) &&
               password.Any(char.IsDigit) &&
               password.Any(c => !char.IsLetterOrDigit(c));
    }

    public static bool IsValidGuid(string guid)
        => !string.IsNullOrWhiteSpace(guid) && Guid.TryParse(guid, out _);

    public static bool IsNumeric(string value)
        => !string.IsNullOrWhiteSpace(value) && double.TryParse(value, out _);

    public static bool IsAlphanumeric(string value)
        => !string.IsNullOrWhiteSpace(value) && value.All(char.IsLetterOrDigit);

    public static bool IsValidPostalCode(string postalCode, string countryCode = "US")
    {
        if (string.IsNullOrWhiteSpace(postalCode))
            return false;

        return countryCode.ToUpper() switch
        {
            "US" => Regex.IsMatch(postalCode, @"^\d{5}(-\d{4})?$"),
            "UK" => Regex.IsMatch(postalCode, @"^[A-Z]{1,2}\d{1,2}[A-Z]?\s?\d[A-Z]{2}$", RegexOptions.IgnoreCase),
            "CA" => Regex.IsMatch(postalCode, @"^[A-Z]\d[A-Z]\s?\d[A-Z]\d$", RegexOptions.IgnoreCase),
            _ => !string.IsNullOrWhiteSpace(postalCode)
        };
    }
}
```

---

## 4. FileHelper

### Purpose
File system operations and path utilities.

### Proposed Implementation

```csharp
namespace Craft.Utilities.Helpers;

/// <summary>
/// Provides file system and path utilities.
/// </summary>
public static class FileHelper
{
    public static string GetUniqueFileName(string directory, string fileName)
    {
        var name = Path.GetFileNameWithoutExtension(fileName);
        var extension = Path.GetExtension(fileName);
        var uniqueName = fileName;
        var counter = 1;

        while (File.Exists(Path.Combine(directory, uniqueName)))
        {
            uniqueName = $"{name}_{counter}{extension}";
            counter++;
        }

        return uniqueName;
    }

    public static long GetFileSize(string path)
    {
        ArgumentException.ThrowIfNullOrEmpty(path);
        return new FileInfo(path).Length;
    }

    public static string GetFileHash(string path, HashAlgorithmName algorithm = default)
    {
        ArgumentException.ThrowIfNullOrEmpty(path);

        var algorithmName = algorithm == default ? HashAlgorithmName.SHA256 : algorithm;

        using var stream = File.OpenRead(path);
        using var hashAlgorithm = HashAlgorithm.Create(algorithmName.Name!)!;
        var hash = hashAlgorithm.ComputeHash(stream);

        return Convert.ToHexString(hash);
    }

    public static bool IsFileLocked(string path)
    {
        try
        {
            using var stream = File.Open(path, FileMode.Open, FileAccess.Read, FileShare.None);
            return false;
        }
        catch (IOException)
        {
            return true;
        }
    }

    public static void EnsureDirectoryExists(string path)
    {
        ArgumentException.ThrowIfNullOrEmpty(path);
        Directory.CreateDirectory(path);
    }

    public static IEnumerable<string> GetFilesRecursive(string directory, string pattern = "*.*")
    {
        ArgumentException.ThrowIfNullOrEmpty(directory);
        return Directory.EnumerateFiles(directory, pattern, SearchOption.AllDirectories);
    }

    public static string GetRelativePath(string fromPath, string toPath)
    {
        ArgumentException.ThrowIfNullOrEmpty(fromPath);
        ArgumentException.ThrowIfNullOrEmpty(toPath);

        return Path.GetRelativePath(fromPath, toPath);
    }

    public static string SanitizeFileName(string fileName)
    {
        if (string.IsNullOrWhiteSpace(fileName))
            return string.Empty;

        var invalidChars = Path.GetInvalidFileNameChars();
        return string.Concat(fileName.Where(c => !invalidChars.Contains(c)));
    }

    public static string GetReadableFileSize(long bytes)
    {
        string[] sizes = ["B", "KB", "MB", "GB", "TB"];
        double len = bytes;
        int order = 0;

        while (len >= 1024 && order < sizes.Length - 1)
        {
            order++;
            len /= 1024;
        }

        return $"{len:0.##} {sizes[order]}";
    }
}
```

---

## 5. RetryHelper

### Purpose
Retry logic for operations that may fail transiently.

### Proposed Implementation

```csharp
namespace Craft.Utilities.Helpers;

/// <summary>
/// Provides retry utilities for transient failures.
/// </summary>
public static class RetryHelper
{
    public static T Retry<T>(Func<T> action, int maxAttempts = 3, int delayMs = 1000)
    {
        ArgumentNullException.ThrowIfNull(action);
        ArgumentOutOfRangeException.ThrowIfLessThan(maxAttempts, 1);
        ArgumentOutOfRangeException.ThrowIfNegative(delayMs);

        Exception? lastException = null;

        for (int attempt = 1; attempt <= maxAttempts; attempt++)
        {
            try
            {
                return action();
            }
            catch (Exception ex)
            {
                lastException = ex;
                if (attempt < maxAttempts)
                    Thread.Sleep(delayMs);
            }
        }

        throw new InvalidOperationException(
            $"Operation failed after {maxAttempts} attempts.", lastException);
    }

    public static async Task<T> RetryAsync<T>(
        Func<Task<T>> action,
        int maxAttempts = 3,
        int delayMs = 1000,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(action);
        ArgumentOutOfRangeException.ThrowIfLessThan(maxAttempts, 1);
        ArgumentOutOfRangeException.ThrowIfNegative(delayMs);

        Exception? lastException = null;

        for (int attempt = 1; attempt <= maxAttempts; attempt++)
        {
            try
            {
                return await action();
            }
            catch (Exception ex) when (ex is not OperationCanceledException)
            {
                lastException = ex;
                if (attempt < maxAttempts)
                    await Task.Delay(delayMs, cancellationToken);
            }
        }

        throw new InvalidOperationException(
            $"Operation failed after {maxAttempts} attempts.", lastException);
    }

    public static T RetryOnException<TException>(
        Func<T> action,
        int maxAttempts = 3,
        int delayMs = 1000)
        where TException : Exception
    {
        ArgumentNullException.ThrowIfNull(action);
        ArgumentOutOfRangeException.ThrowIfLessThan(maxAttempts, 1);
        ArgumentOutOfRangeException.ThrowIfNegative(delayMs);

        Exception? lastException = null;

        for (int attempt = 1; attempt <= maxAttempts; attempt++)
        {
            try
            {
                return action();
            }
            catch (TException ex)
            {
                lastException = ex;
                if (attempt < maxAttempts)
                    Thread.Sleep(delayMs);
            }
        }

        throw new InvalidOperationException(
            $"Operation failed after {maxAttempts} attempts.", lastException);
    }

    public static T RetryWithExponentialBackoff<T>(
        Func<T> action,
        int maxAttempts = 3,
        int initialDelayMs = 1000)
    {
        ArgumentNullException.ThrowIfNull(action);
        ArgumentOutOfRangeException.ThrowIfLessThan(maxAttempts, 1);
        ArgumentOutOfRangeException.ThrowIfNegative(initialDelayMs);

        Exception? lastException = null;

        for (int attempt = 1; attempt <= maxAttempts; attempt++)
        {
            try
            {
                return action();
            }
            catch (Exception ex)
            {
                lastException = ex;
                if (attempt < maxAttempts)
                {
                    var delayMs = initialDelayMs * (int)Math.Pow(2, attempt - 1);
                    Thread.Sleep(delayMs);
                }
            }
        }

        throw new InvalidOperationException(
            $"Operation failed after {maxAttempts} attempts.", lastException);
    }
}
```

---

## 6. EnumHelper

### Purpose
Enum manipulation and conversion utilities.

### Proposed Implementation

```csharp
namespace Craft.Utilities.Helpers;

/// <summary>
/// Provides enum utilities.
/// </summary>
public static class EnumHelper
{
    public static IEnumerable<T> GetValues<T>() where T : struct, Enum
        => Enum.GetValues<T>();

    public static string GetDescription<T>(T enumValue) where T : struct, Enum
    {
        var field = typeof(T).GetField(enumValue.ToString());
        if (field == null)
            return enumValue.ToString();

        var attribute = field.GetCustomAttribute<DescriptionAttribute>();
        return attribute?.Description ?? enumValue.ToString();
    }

    public static T Parse<T>(string value, bool ignoreCase = true) where T : struct, Enum
    {
        ArgumentException.ThrowIfNullOrEmpty(value);
        return Enum.Parse<T>(value, ignoreCase);
    }

    public static bool TryParse<T>(string? value, out T result, bool ignoreCase = true)
        where T : struct, Enum
    {
        return Enum.TryParse(value, ignoreCase, out result);
    }

    public static Dictionary<int, string> ToDictionary<T>() where T : struct, Enum
    {
        return Enum.GetValues<T>()
            .ToDictionary(e => Convert.ToInt32(e), e => e.ToString());
    }

    public static Dictionary<int, string> ToDictionaryWithDescriptions<T>() where T : struct, Enum
    {
        return Enum.GetValues<T>()
            .ToDictionary(e => Convert.ToInt32(e), e => GetDescription(e));
    }

    public static bool IsDefined<T>(T value) where T : struct, Enum
        => Enum.IsDefined(typeof(T), value);

    public static string GetName<T>(T value) where T : struct, Enum
        => Enum.GetName(typeof(T), value) ?? value.ToString();

    public static IEnumerable<string> GetNames<T>() where T : struct, Enum
        => Enum.GetNames<T>();
}
```

---

## Implementation Priority

### High Priority (Immediate Value)
1. **ValidationHelper** - Widely used across applications
2. **DateTimeHelper** - Common business logic requirements
3. **StringHelper** - Frequent string operations

### Medium Priority (Nice to Have)
4. **FileHelper** - File operations enhancement
5. **RetryHelper** - Resilience patterns
6. **EnumHelper** - Enum manipulation

### Low Priority (Specialized)
7. **CollectionHelper** - LINQ extensions
8. **JsonHelper** - JSON utilities
9. **CompressionHelper** - Compression needs
10. **HtmlHelper** - Web-specific operations

---

## Dependencies Required

Most utilities use only .NET BCL, but some may require:
- System.ComponentModel.Annotations (for DescriptionAttribute in EnumHelper)
- System.Text.RegularExpressions (already available)
- System.IO.Compression (for CompressionHelper)

---

## Testing Strategy

For each utility class:
1. Create corresponding test class in Craft.Utilities.Tests
2. Follow AAA pattern (Arrange-Act-Assert)
3. Use Theory/InlineData for parameter variations
4. Test edge cases (null, empty, boundary values)
5. Aim for 95%+ code coverage

---

## Documentation Requirements

Each utility method should include:
- XML documentation comments
- Parameter descriptions
- Return value description
- Exception documentation
- Usage examples in XML remarks

Example:
```csharp
/// <summary>
/// Truncates a string to a maximum length and adds a suffix if truncated.
/// </summary>
/// <param name="value">The string to truncate.</param>
/// <param name="maxLength">The maximum length of the result string.</param>
/// <param name="suffix">The suffix to append if truncated (default is "...").</param>
/// <returns>The truncated string with suffix if applicable, or the original string if not truncated.</returns>
/// <remarks>
/// <code>
/// var result = StringHelper.Truncate("Hello World", 8, "...");
/// // result = "Hello..."
/// </code>
/// </remarks>
public static string Truncate(string value, int maxLength, string suffix = "...")
```

---

## Conclusion

These 10 utility classes would significantly enhance the Craft.Utilities project by providing commonly-needed functionality across various domains. They follow the same patterns and quality standards as the existing utilities in the project.

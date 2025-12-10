using System.Globalization;

#pragma warning disable IDE0130 // Namespace does not match folder structure
namespace System;
#pragma warning restore IDE0130 // Namespace does not match folder structure

public static class NumericExtensions
{
    /// <summary>
    /// Clamps the value between the specified minimum and maximum values.
    /// </summary>
    public static int Clamp(this int value, int min, int max)
        => Math.Clamp(value, min, max);

    /// <summary>
    /// Clamps the value between the specified minimum and maximum values.
    /// </summary>
    public static long Clamp(this long value, long min, long max)
        => Math.Clamp(value, min, max);

    /// <summary>
    /// Clamps the value between the specified minimum and maximum values.
    /// </summary>
    public static decimal Clamp(this decimal value, decimal min, decimal max)
        => Math.Clamp(value, min, max);

    /// <summary>
    /// Clamps the value between the specified minimum and maximum values.
    /// </summary>
    public static double Clamp(this double value, double min, double max)
        => Math.Clamp(value, min, max);

    /// <summary>
    /// Clamps the value between the specified minimum and maximum values.
    /// </summary>
    public static float Clamp(this float value, float min, float max)
        => Math.Clamp(value, min, max);

    /// <summary>
    /// Determines whether the value is between the specified minimum and maximum values (inclusive).
    /// </summary>
    public static bool IsBetween(this int value, int min, int max)
        => value >= min && value <= max;

    /// <summary>
    /// Determines whether the value is between the specified minimum and maximum values (inclusive).
    /// </summary>
    public static bool IsBetween(this long value, long min, long max)
        => value >= min && value <= max;

    /// <summary>
    /// Determines whether the value is between the specified minimum and maximum values (inclusive).
    /// </summary>
    public static bool IsBetween(this decimal value, decimal min, decimal max)
        => value >= min && value <= max;

    /// <summary>
    /// Determines whether the value is between the specified minimum and maximum values (inclusive).
    /// </summary>
    public static bool IsBetween(this double value, double min, double max)
        => value >= min && value <= max;

    /// <summary>
    /// Determines whether the value is between the specified minimum and maximum values (inclusive).
    /// </summary>
    public static bool IsBetween(this float value, float min, float max)
        => value >= min && value <= max;

    /// <summary>
    /// Determines whether the value is positive.
    /// </summary>
    public static bool IsPositive(this int value)
        => value > 0;

    /// <summary>
    /// Determines whether the value is positive.
    /// </summary>
    public static bool IsPositive(this long value)
        => value > 0;

    /// <summary>
    /// Determines whether the value is positive.
    /// </summary>
    public static bool IsPositive(this decimal value)
        => value > 0;

    /// <summary>
    /// Determines whether the value is positive.
    /// </summary>
    public static bool IsPositive(this double value)
        => value > 0;

    /// <summary>
    /// Determines whether the value is positive.
    /// </summary>
    public static bool IsPositive(this float value)
        => value > 0;

    /// <summary>
    /// Determines whether the value is negative.
    /// </summary>
    public static bool IsNegative(this int value)
        => value < 0;

    /// <summary>
    /// Determines whether the value is negative.
    /// </summary>
    public static bool IsNegative(this long value)
        => value < 0;

    /// <summary>
    /// Determines whether the value is negative.
    /// </summary>
    public static bool IsNegative(this decimal value)
        => value < 0;

    /// <summary>
    /// Determines whether the value is negative.
    /// </summary>
    public static bool IsNegative(this double value)
        => value < 0;

    /// <summary>
    /// Determines whether the value is negative.
    /// </summary>
    public static bool IsNegative(this float value)
        => value < 0;

    /// <summary>
    /// Determines whether the value is zero.
    /// </summary>
    public static bool IsZero(this int value)
        => value == 0;

    /// <summary>
    /// Determines whether the value is zero.
    /// </summary>
    public static bool IsZero(this long value)
        => value == 0;

    /// <summary>
    /// Determines whether the value is zero.
    /// </summary>
    public static bool IsZero(this decimal value)
        => value == 0;

    /// <summary>
    /// Determines whether the value is approximately zero (within the specified tolerance).
    /// </summary>
    public static bool IsZero(this double value, double tolerance = 1e-10)
        => Math.Abs(value) < tolerance;

    /// <summary>
    /// Determines whether the value is approximately zero (within the specified tolerance).
    /// </summary>
    public static bool IsZero(this float value, float tolerance = 1e-6f)
        => Math.Abs(value) < tolerance;

    /// <summary>
    /// Determines whether the value is even.
    /// </summary>
    public static bool IsEven(this int value)
        => value % 2 == 0;

    /// <summary>
    /// Determines whether the value is even.
    /// </summary>
    public static bool IsEven(this long value)
        => value % 2 == 0;

    /// <summary>
    /// Determines whether the value is odd.
    /// </summary>
    public static bool IsOdd(this int value)
        => value % 2 != 0;

    /// <summary>
    /// Determines whether the value is odd.
    /// </summary>
    public static bool IsOdd(this long value)
        => value % 2 != 0;

    /// <summary>
    /// Formats the integer with thousand separators using the current culture.
    /// </summary>
    public static string ToFormattedString(this int value)
        => value.ToString("N0", CultureInfo.CurrentCulture);

    /// <summary>
    /// Formats the integer with thousand separators using the current culture.
    /// </summary>
    public static string ToFormattedString(this long value)
        => value.ToString("N0", CultureInfo.CurrentCulture);

    /// <summary>
    /// Formats the decimal with the specified number of decimal places using the current culture.
    /// </summary>
    public static string ToFormattedString(this decimal value, int decimalPlaces = 2)
        => value.ToString($"N{decimalPlaces}", CultureInfo.CurrentCulture);

    /// <summary>
    /// Formats the double with the specified number of decimal places using the current culture.
    /// </summary>
    public static string ToFormattedString(this double value, int decimalPlaces = 2)
        => value.ToString($"N{decimalPlaces}", CultureInfo.CurrentCulture);

    /// <summary>
    /// Formats the value as a currency string using the current culture.
    /// </summary>
    public static string ToCurrency(this decimal value)
        => value.ToString("C", CultureInfo.CurrentCulture);

    /// <summary>
    /// Formats the value as a currency string using the current culture.
    /// </summary>
    public static string ToCurrency(this double value)
        => value.ToString("C", CultureInfo.CurrentCulture);

    /// <summary>
    /// Formats the value as a currency string using the current culture.
    /// </summary>
    public static string ToCurrency(this int value)
        => value.ToString("C", CultureInfo.CurrentCulture);

    /// <summary>
    /// Rounds the decimal to the specified number of decimal places.
    /// </summary>
    public static decimal RoundTo(this decimal value, int decimalPlaces)
        => Math.Round(value, decimalPlaces);

    /// <summary>
    /// Rounds the double to the specified number of decimal places.
    /// </summary>
    public static double RoundTo(this double value, int decimalPlaces)
        => Math.Round(value, decimalPlaces);

    /// <summary>
    /// Returns the absolute value.
    /// </summary>
    public static int Abs(this int value)
        => Math.Abs(value);

    /// <summary>
    /// Returns the absolute value.
    /// </summary>
    public static long Abs(this long value)
        => Math.Abs(value);

    /// <summary>
    /// Returns the absolute value.
    /// </summary>
    public static decimal Abs(this decimal value)
        => Math.Abs(value);

    /// <summary>
    /// Returns the absolute value.
    /// </summary>
    public static double Abs(this double value)
        => Math.Abs(value);

    /// <summary>
    /// Returns the absolute value.
    /// </summary>
    public static float Abs(this float value)
        => Math.Abs(value);

    /// <summary>
    /// Converts bytes to kilobytes.
    /// </summary>
    public static double ToKilobytes(this long bytes)
        => bytes / 1024.0;

    /// <summary>
    /// Converts bytes to megabytes.
    /// </summary>
    public static double ToMegabytes(this long bytes)
        => bytes / (1024.0 * 1024.0);

    /// <summary>
    /// Converts bytes to gigabytes.
    /// </summary>
    public static double ToGigabytes(this long bytes)
        => bytes / (1024.0 * 1024.0 * 1024.0);
}

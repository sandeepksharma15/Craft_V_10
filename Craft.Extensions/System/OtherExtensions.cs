using System.Globalization;

#pragma warning disable IDE0130 // Namespace does not match folder structure
namespace System;
#pragma warning restore IDE0130 // Namespace does not match folder structure

public static class OtherExtensions
{
    /// <summary>
    /// Converts a byte array to its hexadecimal representation.
    /// </summary>
    public static string? BytesToHex(this byte[]? bytes) =>
        bytes is null
            ? null
            : string.Create(bytes.Length * 2, bytes, (span, b) =>
            {
                for (int i = 0; i < b.Length; i++)
                    b[i].ToString("X2").AsSpan().CopyTo(span.Slice(i * 2, 2));
            });

    /// <summary>
    /// Converts a hexadecimal string to a byte array.
    /// </summary>
    /// <exception cref="FormatException">
    /// Thrown if parsing fails for any hex code within the string, or string is not in proper format
    /// </exception>
    public static byte[] HexToBytes(this string? hex)
    {
        if (string.IsNullOrWhiteSpace(hex))
            return Array.Empty<byte>();

        hex = hex.Trim();

        if (hex.Length % 2 != 0)
            throw new FormatException("Hex string must have an even number of characters.");

        var bytes = new byte[hex.Length / 2];
        for (int i = 0; i < bytes.Length; i++)
        {
            var code = hex.Substring(i * 2, 2);
            if (!byte.TryParse(code, NumberStyles.HexNumber, CultureInfo.InvariantCulture, out var result))
                throw new FormatException($"Failed to parse hex string at position {i * 2}: '{code}'");
            bytes[i] = result;
        }
        return bytes;
    }

    /// <summary>
    /// Converts a decimal value to its percentage representation with up to two decimal places.
    /// </summary>
    public static string ToPercentage(this decimal value) =>
        (value * 100).ToString("0.##", CultureInfo.CurrentCulture) + "%";

    /// <summary>
    /// Converts a double value to its percentage representation with up to two decimal places.
    /// </summary>
    public static string ToPercentage(this double value) =>
        (value * 100).ToString("0.##", CultureInfo.CurrentCulture) + "%";

    /// <summary>
    /// Converts a float value to its percentage representation with up to two decimal places.
    /// </summary>
    public static string ToPercentage(this float value) =>
        (value * 100).ToString("0.##", CultureInfo.CurrentCulture) + "%";
}

using System.Globalization;
using System.Text;

#pragma warning disable IDE0130 // Namespace does not match folder structure
namespace System;
#pragma warning restore IDE0130 // Namespace does not match folder structure

public static class OtherExtensions
{
    extension(byte[]? bytes)
    {
        /// <summary>
        /// Converts a byte array to its hexadecimal representation.
        /// </summary>
        /// <param name="bytes">The byte array to convert.</param>
        /// <returns>The hexadecimal string representation of the byte array.</returns>
        public string? BytesToHex()
        {
            if (bytes is null) return null;

            var hex = new StringBuilder(bytes.Length * 2);

            foreach (byte b in bytes)
                hex.AppendFormat("{0:X2}", b);

            return hex.ToString();
        }

    }

    extension(string? hex)
    {
        /// <summary>
        /// Converts a hexadecimal string to a byte array.
        /// </summary>
        /// <param name="hex">The hexadecimal string to convert.</param>
        /// <returns>
        /// A byte array representing the converted values.
        /// Returns null if the input string is null or empty.
        /// </returns>
        /// <exception cref="FormatException">
        /// Thrown if parsing fails for any hex code within the string, or string is not in proper format
        /// </exception>
        public byte[] HexToBytes()
        {
            if (hex is null) return [];

            hex = hex.Trim();

            if (hex.Length % 2 != 0)
                throw new FormatException("Hex string must have an even number of characters.");

            byte[] bytes = new byte[hex.Length / 2];

            for (int i = 0; i < hex.Length / 2; i++)
            {
                string code = hex.Substring(i * 2, 2);

                if (byte.TryParse(code, NumberStyles.HexNumber, CultureInfo.InvariantCulture, out byte result))
                    bytes[i] = result;
                else
                    throw new FormatException($"Failed to parse hex string at position {i * 2}: '{code}'");
            }

            return bytes;
        }
    }


    extension(decimal value)
    {
        /// <summary>
        /// Converts a decimal value to its percentage representation with two decimal places.
        /// </summary>
        /// <param name="value">The decimal value to be converted.</param>
        /// <returns>The percentage representation of the decimal value.</returns>
        public string ToPercentage()
            => (value * 100).ToString("0.##") + "%";
    }

    extension(double value)
    {
        /// <summary>
        /// Converts a double value to its percentage representation with two decimal places.
        /// </summary>
        /// <param name="value">The double value to be converted.</param>
        /// <returns>The percentage representation of the double value.</returns>
        public string ToPercentage()
            => (value * 100).ToString("0.##") + "%";
    }

    extension(float value)
    {
        /// <summary>
        /// Converts a float value to its percentage representation with two decimal places.
        /// </summary>
        /// <param name="value">The float value to be converted.</param>
        /// <returns>The percentage representation of the float value.</returns>
        public string ToPercentage()
            => (value * 100).ToString("0.##") + "%";
    }
}

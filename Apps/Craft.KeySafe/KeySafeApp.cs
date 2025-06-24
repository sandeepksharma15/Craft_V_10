using System.Security.Cryptography;
using Craft.Utilities.Services;

namespace Craft.KeySafe
{
    public class KeySafeApp
    {
        private readonly IKeySafeService _keySafeService;
        public KeySafeApp(IKeySafeService? keySafeService = null)
        {
            _keySafeService = keySafeService ?? new KeySafeService();
        }

        /// <summary>
        /// Generates a cryptographic key and initialization vector (IV) using the AES algorithm.
        /// </summary>
        /// <remarks>This method uses the AES algorithm to generate a random key and IV, which are
        /// returned as Base64-encoded strings. The generated values can be used for encryption and decryption
        /// operations.</remarks>
        /// <returns>A tuple containing the success status and a message.  The <see langword="true"/> value of <c>Success</c>
        /// indicates that the key and IV were successfully generated. The <c>Message</c> provides the generated key and
        /// IV as Base64-encoded strings.</returns>
        public static (bool Success, string Message) Generate()
        {
            // Generate a new AES key and IV
            using var aes = Aes.Create();

            aes.GenerateKey();
            aes.GenerateIV();

            // Convert the key and IV to Base64 strings for easy storage and transmission
            var genKey = Convert.ToBase64String(aes.Key);
            var genIV = Convert.ToBase64String(aes.IV);

            // Return the generated key and IV as a success message
            return (true, $"Generated Key: {genKey}\nGenerated IV: {genIV}");
        }

        /// <summary>
        /// Encrypts the specified plain text using the provided encryption key and initialization vector (IV), or
        /// defaults to internal encryption settings if no key or IV is provided.
        /// </summary>
        /// <remarks>If both <paramref name="key"/> and <paramref name="iv"/> are provided, they will be
        /// used for encryption. Otherwise, the method defaults to using internal encryption settings.</remarks>
        /// <param name="plainText">The plain text to be encrypted. This value cannot be null or empty.</param>
        /// <param name="key">An optional encryption key to use for the operation. If null, the method uses a default key.</param>
        /// <param name="iv">An optional initialization vector (IV) to use for the operation. If null, the method uses a default IV.</param>
        /// <returns>A tuple containing a success flag and a message. If encryption succeeds, the success flag is <see
        /// langword="true"/>  and the message contains the encrypted text. If encryption fails, the success flag is
        /// <see langword="false"/>  and the message contains an error description.</returns>
        public (bool Success, string Message) Encrypt(string plainText, byte[]? key = null, byte[]? iv = null)
        {
            try
            {
                string encrypted = (key != null && iv != null)
                    ? KeySafeService.Encrypt(plainText, key, iv)
                    : _keySafeService.Encrypt(plainText);

                return (true, encrypted);
            }
            catch (Exception ex)
            {
                return (false, $"Encryption error: {ex.Message}");
            }
        }

        /// <summary>
        /// Decrypts the specified cipher text using the provided key and initialization vector (IV),  or a default key
        /// and IV if none are supplied.
        /// </summary>
        /// <remarks>If both <paramref name="key"/> and <paramref name="iv"/> are provided, they will be
        /// used for decryption. Otherwise, the method will use a default key and IV managed by the service.</remarks>
        /// <param name="cipherText">The encrypted text to be decrypted. This parameter cannot be null or empty.</param>
        /// <param name="key">An optional byte array representing the encryption key. If null, a default key will be used.</param>
        /// <param name="iv">An optional byte array representing the initialization vector (IV). If null, a default IV will be used.</param>
        /// <returns>A tuple containing a boolean indicating success and a string containing the decrypted message. If decryption
        /// succeeds, <see langword="true"/> is returned along with the decrypted text. If decryption fails, <see
        /// langword="false"/> is returned along with an error message.</returns>
        public (bool Success, string Message) Decrypt(string cipherText, byte[]? key = null, byte[]? iv = null)
        {
            try
            {
                string decrypted = (key != null && iv != null)
                    ? KeySafeService.Decrypt(cipherText, key, iv)
                    : _keySafeService.Decrypt(cipherText);

                return (true, decrypted);
            }
            catch (Exception ex)
            {
                return (false, $"Decryption error: {ex.Message}");
            }
        }
    }
}

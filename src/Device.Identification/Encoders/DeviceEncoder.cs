namespace Device.Identification.Encoders
{

    using System;

    public static class DeviceEncoder
    {
        /// <summary>
        ///     Encrypt a non-empty string.
        /// </summary>
        /// <param name="plaintext">Text to encrypt.</param>
        /// <returns>Encrypted text.</returns>
        public static string Encrypt(string plaintext)
        {
            if (string.IsNullOrEmpty(plaintext))
            {
                throw new ArgumentException("Plaintext cannot be null or empty", nameof(plaintext));
            }

            var encryptedText = Rc4.Encrypt(Configurations.DeviceIdEncryptKey, plaintext);
            return Hex.Encode(encryptedText);
        }

        /// <summary>
        ///     Decrypt a non-empty encrypted string.
        /// </summary>
        /// <param name="encryptedText">Encrypted text to decrypt.</param>
        /// <returns>Decrypted text.</returns>
        public static string Decrypt(string encryptedText)
        {
            if (string.IsNullOrEmpty(encryptedText))
            {
                throw new ArgumentException("Encrypted text cannot be null or empty", nameof(encryptedText));
            }

            var decodedText = Hex.Decode(encryptedText);
            var decryptedText = Rc4.Decrypt(Configurations.DeviceIdEncryptKey, decodedText);
            return decryptedText;
        }
    }
}

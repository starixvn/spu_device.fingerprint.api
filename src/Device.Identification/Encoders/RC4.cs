namespace Device.Identification.Encoders
{
    using System;
    using System.Text;

    public static class Rc4
    {
        /// <summary>
        ///     Encrypt method.
        /// </summary>
        /// <param name="key">The key for encryption.</param>
        /// <param name="value">The value to encrypt.</param>
        /// <returns>Encrypted value as a string.</returns>
        public static string Encrypt(string key, string value)
        {
            if (string.IsNullOrEmpty(key))
            {
                throw new ArgumentException("Key cannot be null or empty", nameof(key));
            }

            if (string.IsNullOrEmpty(value))
            {
                throw new ArgumentException("Value cannot be null or empty", nameof(value));
            }

            var keyBytes = Encoding.UTF8.GetBytes(key);
            var valueBytes = Encoding.UTF8.GetBytes(value);

            var map = new int[256];
            for (var i = 0; i < 256; i++)
            {
                map[i] = i;
            }

            for (int i = 0, j = 0; i < 256; i++)
            {
                j = (j + map[i] + keyBytes[i % keyBytes.Length]) % 256;
                Swap(map, i, j);
            }

            var encryptedBytes = new byte[valueBytes.Length];
            for (int y = 0, i = 0, j = 0; y < valueBytes.Length; y++)
            {
                i = (i + 1) % 256;
                j = (j + map[i]) % 256;
                Swap(map, i, j);

                var k = map[(map[i] + map[j]) % 256];
                encryptedBytes[y] = (byte)(valueBytes[y] ^ k);
            }

            return Convert.ToBase64String(encryptedBytes);
        }

        /// <summary>
        ///     Decrypt method (same as encrypt for RC4).
        /// </summary>
        /// <param name="key">The key for decryption.</param>
        /// <param name="value">The value to decrypt.</param>
        /// <returns>Decrypted value as a string.</returns>
        public static string Decrypt(string key, string value)
        {
            if (string.IsNullOrEmpty(key))
            {
                throw new ArgumentException("Key cannot be null or empty", nameof(key));
            }

            if (string.IsNullOrEmpty(value))
            {
                throw new ArgumentException("Value cannot be null or empty", nameof(value));
            }

            var encryptedBytes = Convert.FromBase64String(value);
            var keyBytes = Encoding.UTF8.GetBytes(key);

            var map = new int[256];
            for (var i = 0; i < 256; i++)
            {
                map[i] = i;
            }

            for (int i = 0, j = 0; i < 256; i++)
            {
                j = (j + map[i] + keyBytes[i % keyBytes.Length]) % 256;
                Swap(map, i, j);
            }

            var decryptedBytes = new byte[encryptedBytes.Length];
            for (int y = 0, i = 0, j = 0; y < encryptedBytes.Length; y++)
            {
                i = (i + 1) % 256;
                j = (j + map[i]) % 256;
                Swap(map, i, j);

                var k = map[(map[i] + map[j]) % 256];
                decryptedBytes[y] = (byte)(encryptedBytes[y] ^ k);
            }

            return Encoding.UTF8.GetString(decryptedBytes);
        }

        /// <summary>
        ///     Swap two elements in an array.
        /// </summary>
        /// <param name="array">The array to swap elements in.</param>
        /// <param name="i">The first index.</param>
        /// <param name="j">The second index.</param>
        private static void Swap(int[] array, int i, int j)
        {
            var temp = array[i];
            array[i] = array[j];
            array[j] = temp;
        }
    }

}

namespace Device.Identification.Encoders
{
    using System;
    using System.Collections.Generic;
    using System.Linq;

    public static class Hex
    {
        private const string HexDigits = "0123456789abcdef";

        /// <summary>
        ///     Hex encoder.
        /// </summary>
        /// <param name="data">The data for encoding.</param>
        /// <returns>The encoded data as a hex string.</returns>
        public static string Encode(string data)
        {
            if (string.IsNullOrEmpty(data))
            {
                throw new ArgumentException("Data cannot be null or empty", nameof(data));
            }

            var map = new string[256];
            for (var i = 0; i < 256; i++)
            {
                map[i] = string.Concat(HexDigits[i >> 4], HexDigits[i & 15]);
            }

            return string.Join(string.Empty, data.Select(c => map[Convert.ToInt32(c)]));
        }

        /// <summary>
        ///     Hex decoder.
        /// </summary>
        /// <param name="data">The hex-encoded string to decode.</param>
        /// <returns>The decoded data as a string.</returns>
        public static string Decode(string data)
        {
            if (string.IsNullOrEmpty(data))
            {
                throw new ArgumentException("Data cannot be null or empty", nameof(data));
            }

            if (data.Length % 2 != 0)
            {
                data = '0' + data; // Ensure even-length hex string
            }

            var map = new Dictionary<string, char>();
            for (var i = 0; i < 256; i++)
            {
                var hexValue = string.Concat(HexDigits[i >> 4], HexDigits[i & 15]);
                map[hexValue] = Convert.ToChar(i);
            }

            var result = new List<char>();
            for (var i = 0; i < data.Length; i += 2)
            {
                var hexPair = data.Substring(i, 2);
                if (map.TryGetValue(hexPair, out var decodedChar))
                {
                    result.Add(decodedChar);
                }
                else
                {
                    throw new FormatException($"Invalid hex character pair: {hexPair}");
                }
            }

            return new string(result.ToArray());
        }
    }

}

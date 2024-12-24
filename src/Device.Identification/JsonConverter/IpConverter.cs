namespace Device.Identification.JsonConverter
{
    using System;
    using System.Net;
    using Newtonsoft.Json;

    /// <summary>
    ///     IP convertor.
    /// </summary>
    public class IpConverter : JsonConverter
    {
        public override bool CanConvert(Type objectType)
        {
            return objectType == typeof(byte[]);
        }

        /// <summary>
        ///     Read JSON object.
        /// </summary>
        /// <param name="reader">Reader object.</param>
        /// <param name="objectType">Object type.</param>
        /// <param name="existingValue">Current value.</param>
        /// <param name="serializer">Setting object.</param>
        /// <returns>Return deserialized object.</returns>
        public override object ReadJson(JsonReader reader, Type objectType, object existingValue, JsonSerializer serializer)
        {
            var bytes16 = new byte[] { 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0 };
            var value = reader.Value as string;
            if (string.IsNullOrEmpty(value))
            {
                return bytes16;
            }

            var bytes4 = IPAddress.Parse(value).GetAddressBytes();
            if (bytes4.Length != 4)
            {
                return bytes16;
            }

            for (var i = 0; i < 4; i++)
            {
                bytes16[i] = bytes4[i];
            }

            return bytes16;
        }

        public override void WriteJson(JsonWriter writer, object value, JsonSerializer serializer)
        {
            var bytes = (byte[])value;
            var address = new IPAddress(bytes);
            writer.WriteRaw(address.ToString());
        }
    }
}
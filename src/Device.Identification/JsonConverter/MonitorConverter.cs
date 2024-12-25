
namespace Device.Identification.JsonConverter
{
    using Newtonsoft.Json;
    using System;
    using System.Collections.Generic;
    using System.Globalization;

    /// <summary>
    ///     Monitor convertor.
    /// </summary>
    public class MonitorConverter() : JsonConverter
    {
        private const int NumberOfItemsInMonitor = 3;
        private readonly string _separator = "@";

        public override bool CanConvert(Type objectType)
        {
            return objectType == typeof(byte[]);
        }

        public override object ReadJson(JsonReader reader, Type objectType, object existingValue, JsonSerializer serializer)
        {
            var monitorItems = GetMonitorItems(reader);
            return MonitorIsValid(monitorItems.Length) ? GetRepresentationNumberFromMonitorItems(monitorItems) : 0;
        }

        public override void WriteJson(JsonWriter writer, object value, JsonSerializer serializer)
        {
            var representationNumber = (long)value;
            writer.WriteValue(
                GetWidthFromRepresentationNumber(representationNumber)
                + _separator
                + GetHeightFromRepresentationNumber(representationNumber)
                + _separator
                + GetPixelDepthFromRepresentationNumber(representationNumber));
        }

        private static bool MonitorIsValid(int length)
        {
            return length == NumberOfItemsInMonitor;
        }

        private static long GetRepresentationNumberFromMonitorItems(IList<string> monitorItems)
        {
            var width = long.Parse(monitorItems[0], CultureInfo.InvariantCulture);
            var height = long.Parse(monitorItems[1], CultureInfo.InvariantCulture);
            var pixelDepth = long.Parse(monitorItems[2], CultureInfo.InvariantCulture);
            return (((width << 13) | height) << 6) | pixelDepth;
        }

        private static long GetPixelDepthFromRepresentationNumber(long representationNumber)
        {
            return (representationNumber << 58) >> 58;
        }

        private static long GetHeightFromRepresentationNumber(long representationNumber)
        {
            return (representationNumber << 45) >> 51;
        }

        private static long GetWidthFromRepresentationNumber(long representationNumber)
        {
            return (representationNumber >> 45) >> 38;
        }

        private string[] GetMonitorItems(JsonReader reader)
        {
            string value = reader.Value as string ?? string.Empty;
            return value.Split(new[] { _separator }, StringSplitOptions.None);
        }
    }
}
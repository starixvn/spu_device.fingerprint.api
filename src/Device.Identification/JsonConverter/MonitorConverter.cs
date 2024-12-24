using Device.Identification.Model;
using System.Globalization;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace Device.Identification.JsonConverter
{
    public class MonitorConverter(IAppSettings appSettings) : JsonConverter<long>
    {
        private const int NumberOfItemsInMonitor = 3;
        private readonly string _separator = appSettings.Separator;

        public override long Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
        {
            var monitorItems = GetMonitorItems(ref reader);
            return MonitorIsValid(monitorItems.Length)
                ? GetRepresentationNumberFromMonitorItems(monitorItems)
                : 0;
        }

        public override void Write(Utf8JsonWriter writer, long value, JsonSerializerOptions options)
        {
            var width = GetWidthFromRepresentationNumber(value);
            var height = GetHeightFromRepresentationNumber(value);
            var pixelDepth = GetPixelDepthFromRepresentationNumber(value);

            writer.WriteStringValue($"{width}{_separator}{height}{_separator}{pixelDepth}");
        }

        private static bool MonitorIsValid(int length)
        {
            return length == NumberOfItemsInMonitor;
        }

        private static long GetRepresentationNumberFromMonitorItems(string[] monitorItems)
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

        private string[] GetMonitorItems(ref Utf8JsonReader reader)
        {
            var stringValue = reader.GetString();
            return stringValue.Split(new[] { _separator }, StringSplitOptions.None);
        }
    }
}
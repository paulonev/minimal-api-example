using System.Globalization;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace CarParking.Api.Json;

public class DateTimeCustomFormatConverter : JsonConverter<DateTime>
{
    public override DateTime Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
    {
        var value = reader.GetString();

        if (DateTime.TryParseExact(
                value,
                "d MMMM yyyy HH:mm:ss",
                CultureInfo.CurrentCulture,
                DateTimeStyles.AssumeLocal,
                out var parsedValue))
        {
            return parsedValue;
        }

        return DateTime.Parse(value!, CultureInfo.CurrentCulture, DateTimeStyles.AssumeLocal);
    }

    public override void Write(Utf8JsonWriter writer, DateTime value, JsonSerializerOptions options)
    {
        writer.WriteStringValue(value.ToLocalTime().ToString("d MMMM yyyy HH:mm:ss"));
    }
}
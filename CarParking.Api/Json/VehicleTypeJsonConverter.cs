using System.Text.Json;
using System.Text.Json.Serialization;
using CarParking.Api.Data.Entities;

namespace CarParking.Api.Json;

public sealed class VehicleTypeJsonConverter : JsonConverter<VehicleType>
{
    public override VehicleType Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
    {
        return reader.TokenType switch
        {
            JsonTokenType.String when Enum.TryParse<VehicleType>(reader.GetString(), ignoreCase: true, out var parsedValue) => parsedValue,
            JsonTokenType.Number when reader.TryGetInt32(out var numericValue) && Enum.IsDefined(typeof(VehicleType), numericValue) => (VehicleType)numericValue,
            _ => default
        };
    }

    public override void Write(Utf8JsonWriter writer, VehicleType value, JsonSerializerOptions options)
    {
        writer.WriteStringValue(value.ToString());
    }
}
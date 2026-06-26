using System.Text.Json.Serialization;
using CarParking.Api.Data.Entities;

namespace CarParking.Api.Models.Request;

public class ParkVehicleRequest : IVehicleRegistrable
{
    public required string VehicleReg { get; set; }

    [JsonConverter(typeof(JsonStringEnumConverter))]
    public VehicleType VehicleType { get; set; }
}
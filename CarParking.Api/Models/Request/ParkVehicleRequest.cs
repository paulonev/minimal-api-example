using CarParking.Api.Data.Entities;
using CarParking.Api.Json;
using System.Text.Json.Serialization;

namespace CarParking.Api.Models.Request;

public class ParkVehicleRequest : IVehicleRegistrable
{
    public required string VehicleReg { get; set; }

    [JsonConverter(typeof(VehicleTypeJsonConverter))]
    public VehicleType VehicleType { get; set; }
}
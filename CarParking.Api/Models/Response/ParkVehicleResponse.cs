using System.Text.Json.Serialization;
using CarParking.Api.Json;
using CarParking.Api.Models.Request;

namespace CarParking.Api.Models.Response;

public class ParkVehicleResponse : IVehicleRegistrable
{
    public required string VehicleReg { get; set; }
    public int SpaceNumber { get; set; }
    
    [JsonConverter(typeof(DateTimeCustomFormatConverter))]
    public DateTime TimeIn { get; set; }
}
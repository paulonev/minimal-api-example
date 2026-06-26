using System.Text.Json.Serialization;
using CarParking.Api.Json;
using CarParking.Api.Models.Request;

namespace CarParking.Api.Models.Response;

public class ExitParkingResponse : IVehicleRegistrable
{
    public required string VehicleReg { get; set; }
    public double VehicleCharge { get; set; }
    
    [JsonConverter(typeof(DateTimeCustomFormatConverter))]
    public DateTime TimeIn { get; set; }
    
    [JsonConverter(typeof(DateTimeCustomFormatConverter))]
    public DateTime TimeOut { get; set; }
}
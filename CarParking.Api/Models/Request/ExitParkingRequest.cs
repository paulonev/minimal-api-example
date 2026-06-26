namespace CarParking.Api.Models.Request;

public class ExitParkingRequest : IVehicleRegistrable
{
    public required string VehicleReg { get; set; }
}
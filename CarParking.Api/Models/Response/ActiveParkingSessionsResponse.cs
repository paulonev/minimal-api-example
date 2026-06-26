namespace CarParking.Api.Models.Response;

public class ActiveParkingSessionsResponse
{
    public List<ActiveParkingSessionResponseItem> Sessions { get; set; } = [];
}

public class ActiveParkingSessionResponseItem
{
    public string VehicleReg { get; set; } = string.Empty;
    public int SpaceNumber { get; set; }
}

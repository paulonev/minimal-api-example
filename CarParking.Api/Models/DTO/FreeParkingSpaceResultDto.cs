namespace CarParking.Api.Models.DTO;

public record FreeParkingSpaceResultDto(string VehicleReg, double VehicleCharge, DateTime TimeIn, DateTime TimeOut);
namespace CarParking.Api.Models.DTO;

public record AllocateParkingSpaceResultDto(int SpaceNumber, DateTime TimeIn, string VehicleReg);
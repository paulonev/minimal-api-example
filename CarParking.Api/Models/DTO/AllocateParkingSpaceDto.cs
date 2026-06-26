using CarParking.Api.Data.Entities;

namespace CarParking.Api.Models.DTO;

public record AllocateParkingSpaceDto(string VehicleReg, VehicleType VehicleType);
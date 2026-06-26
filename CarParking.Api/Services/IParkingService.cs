using CarParking.Api.Models.DTO;

namespace CarParking.Api.Services;

public interface IParkingService
{
    // Get all parking spaces
    Task<Result<ParkingSpacesDto>> GetAllParkingSpaces();

    // Free up this vehicles space and return its final charge from its parking time until now
    Task<Result<FreeParkingSpaceResultDto>> FreeParkingSpace(FreeParkingSpaceDto freeParkingSpaceDto);

    // Check if parking space is available, returns object with { SpaceNumber, TimeIn, VehicleReg } or ErrorMessage
    Task<Result<AllocateParkingSpaceResultDto>> AllocateParkingSpace(AllocateParkingSpaceDto allocateParkingSpaceDto);

    Task<Result<List<ActiveParkingSessionDto>>> GetActiveParkingSessions();
}
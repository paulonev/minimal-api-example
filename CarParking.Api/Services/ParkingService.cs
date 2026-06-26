using CarParking.Api.Data;
using CarParking.Api.Data.Entities;
using CarParking.Api.Data.Extensions;
using CarParking.Api.Models;
using CarParking.Api.Models.DTO;

namespace CarParking.Api.Services;

public class ParkingService(
    CarParkingDbContext dbContext, 
    ILogger<ParkingService> logger,
    TimeProvider timeProvider) : IParkingService
{
    /// <summary>
    /// Get number of available and occupied parking spaces.
    /// </summary>
    /// <returns>A result containing the number of available and occupied parking spaces.</returns>
    public async Task<Result<ParkingSpacesDto>> GetAllParkingSpaces()
    {
        var parkingSpaces = dbContext.GetAllParkingSpaces();
        var availableSpaces = parkingSpaces.Count(ps => ps.IsAvailable);
        var occupiedSpaces = parkingSpaces.Count(ps => !ps.IsAvailable);

        logger.LogInformation("Retrieved parking spaces: {AvailableSpaces} available, {OccupiedSpaces} occupied",
            availableSpaces, occupiedSpaces);


        return Result<ParkingSpacesDto>.SuccessResult(new ParkingSpacesDto(availableSpaces, occupiedSpaces));
    }

    /// <summary>
    /// Allocate new parking space given vehicle registration number.
    /// </summary>
    /// <param name="allocateParkingSpaceDto">The DTO containing vehicle registration and type information.</param>
    /// <returns>A result indicating success or failure of the allocation.</returns>
    public async Task<Result<AllocateParkingSpaceResultDto>> AllocateParkingSpace(AllocateParkingSpaceDto allocateParkingSpaceDto)
    {
        // Check if vehicle reg has active parking session (early return with error message, when vehicle is already parked)
        if (dbContext.HasActiveParkingSession(allocateParkingSpaceDto.VehicleReg))
        {
            return Result<AllocateParkingSpaceResultDto>.Failure("Vehicle is already parked.");
        }
        
        // Get first available parking space (early return with error message, when no spaces are available)
        var availableParkingSpace = dbContext.GetFirstAvailableParkingSpace();
        if (availableParkingSpace == null)
        {
            return Result<AllocateParkingSpaceResultDto>.Failure("No available parking spaces.");
        }

        // Create new parking session taking first available parking space, vehicle reg, vehicle type and return success
        var timeIn = timeProvider.GetUtcNow().UtcDateTime;

        await dbContext.AddAsync(new ParkingSession
        {
            ParkingSpace = availableParkingSpace,
            VehicleReg = allocateParkingSpaceDto.VehicleReg,
            VehicleType = allocateParkingSpaceDto.VehicleType,
            IsActive = true,
            TimeIn = timeIn
        });

        availableParkingSpace.IsAvailable = false;

        await dbContext.SaveChangesAsync();

        logger.LogInformation("Allocated parking space {SpaceNumber} to vehicle {VehicleReg} at {TimeIn}",
            availableParkingSpace.SpaceNumber,
            allocateParkingSpaceDto.VehicleReg,
            timeIn);

        return Result<AllocateParkingSpaceResultDto>.SuccessResult(new AllocateParkingSpaceResultDto(
            availableParkingSpace.SpaceNumber,
            timeIn, 
            allocateParkingSpaceDto.VehicleReg
        ));
    }

    /// <summary>
    /// Free parking space given vehicle registration information.
    /// </summary>
    /// <param name="freeParkingSpaceDto">The DTO containing vehicle registration information.</param>
    /// <returns>A result indicating success or failure of freeing the parking space.</returns>
    public async Task<Result<FreeParkingSpaceResultDto>> FreeParkingSpace(FreeParkingSpaceDto freeParkingSpaceDto)
    {
        // Check if vehicle reg has active parking session (early return with error message, when vehicle is not parked)
        var parkingSession = dbContext.GetActiveParkingSession(freeParkingSpaceDto.VehicleReg);
        if (parkingSession == null)
        {
            return Result<FreeParkingSpaceResultDto>.Failure("Vehicle is not parked.");
        }

        // Calculate parking charge based on time in and time out
        var timeOut = timeProvider.GetUtcNow().UtcDateTime;
        var parkingCharge = new ParkingCharge(parkingSession.TimeIn, timeOut, parkingSession.VehicleType);

        // Change Parking Space status to available, update parking session to active=false and return success with parking charge
        parkingSession.ParkingSpace.IsAvailable = true;
        parkingSession.IsActive = false;
        parkingSession.TimeOut = timeOut;

        await dbContext.SaveChangesAsync();

        logger.LogInformation("Freed parking space {SpaceNumber} from vehicle {VehicleReg} at {TimeOut}",
            parkingSession.ParkingSpace.SpaceNumber,
            freeParkingSpaceDto.VehicleReg,
            timeOut);

        return Result<FreeParkingSpaceResultDto>.SuccessResult(new FreeParkingSpaceResultDto(
            parkingSession.VehicleReg,
            parkingCharge.Amount,
            parkingSession.TimeIn,
            timeOut
        ));
    }

    /// <summary>
    /// Helper method to getall active parking sessions.
    /// </summary>
    /// <returns>A result containing a list of active parking sessions.</returns>
    public Task<Result<List<ActiveParkingSessionDto>>> GetActiveParkingSessions()
    {
        var activeSessions = dbContext.GetActiveParkingSessions();

        return Task.FromResult(Result<List<ActiveParkingSessionDto>>.SuccessResult(activeSessions));
    }
}
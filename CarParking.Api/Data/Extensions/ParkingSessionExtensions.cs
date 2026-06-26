using CarParking.Api.Data.Entities;
using CarParking.Api.Models.DTO;
using Microsoft.EntityFrameworkCore;

namespace CarParking.Api.Data.Extensions;

public static class ParkingSessionExtensions
{
    public static bool HasActiveParkingSession(this CarParkingDbContext dbContext, string vehicleReg)
    {
        return dbContext.ParkingSessions.Any(ps => ps.VehicleReg == vehicleReg && ps.IsActive);
    }

    public static ParkingSession? GetActiveParkingSession(this CarParkingDbContext dbContext, string vehicleReg)
    {
        return dbContext.ParkingSessions
            .Include(ps => ps.ParkingSpace)
            .FirstOrDefault(ps => ps.VehicleReg == vehicleReg && ps.IsActive);
    }

    public static List<ActiveParkingSessionDto> GetActiveParkingSessions(this CarParkingDbContext dbContext)
    {
        return dbContext.ParkingSessions
            .Include(ps => ps.ParkingSpace)
            .Where(ps => ps.TimeOut == null)
            .Select(ps => new ActiveParkingSessionDto(ps.VehicleReg, ps.ParkingSpace.SpaceNumber))
            .ToList();
    }
}
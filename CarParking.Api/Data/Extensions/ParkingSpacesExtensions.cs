using CarParking.Api.Data.Entities;
using Microsoft.EntityFrameworkCore;

namespace CarParking.Api.Data.Extensions;

public static class ParkingSpacesExtensions
{
    public static ParkingSpace? GetFirstAvailableParkingSpace(this CarParkingDbContext dbContext)
    {
        return dbContext.ParkingSpaces.FirstOrDefault(ps => ps.IsAvailable);
    }

    public static IEnumerable<ParkingSpace> GetAllParkingSpaces(this CarParkingDbContext dbContext)
    {
        return dbContext.ParkingSpaces.AsNoTracking().ToList();
    }
}
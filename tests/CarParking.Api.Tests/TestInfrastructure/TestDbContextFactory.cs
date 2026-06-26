using CarParking.Api.Data;
using Microsoft.EntityFrameworkCore;

namespace CarParking.Api.Tests.TestInfrastructure;

public static class TestDbContextFactory
{
    public static CarParkingDbContext Create()
    {
        var options = new DbContextOptionsBuilder<CarParkingDbContext>()
            .UseInMemoryDatabase($"car-parking-tests-{Guid.NewGuid():N}")
            .Options;

        return new CarParkingDbContext(options);
    }
}
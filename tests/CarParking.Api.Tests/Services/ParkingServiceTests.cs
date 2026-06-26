using CarParking.Api.Data.Entities;
using CarParking.Api.Models.DTO;
using CarParking.Api.Services;
using CarParking.Api.Tests.TestInfrastructure;
using Microsoft.Extensions.Logging;
using Moq;

namespace CarParking.Api.Tests.Services;

public class ParkingServiceTests
{
    [Fact]
    public async Task GetAllParkingSpaces_ShouldReturnAvailableAndOccupiedCounts()
    {
        await using var dbContext = TestDbContextFactory.Create();
        dbContext.ParkingSpaces.AddRange(
            new ParkingSpace { SpaceNumber = 1, IsAvailable = true },
            new ParkingSpace { SpaceNumber = 2, IsAvailable = false },
            new ParkingSpace { SpaceNumber = 3, IsAvailable = true });
        await dbContext.SaveChangesAsync();

        var service = CreateService(dbContext, new FakeTimeProvider(DateTimeOffset.UtcNow));

        var result = await service.GetAllParkingSpaces();

        Assert.True(result.Success);
        Assert.Equal(2, result.Data!.AvailableSpaces);
        Assert.Equal(1, result.Data.OccupiedSpaces);
    }

    [Fact]
    public async Task AllocateParkingSpace_ShouldFail_WhenVehicleAlreadyHasActiveSession()
    {
        await using var dbContext = TestDbContextFactory.Create();
        var parkingSpace = new ParkingSpace { SpaceNumber = 1, IsAvailable = false };
        dbContext.ParkingSpaces.Add(parkingSpace);
        dbContext.ParkingSessions.Add(new ParkingSession
        {
            ParkingSpace = parkingSpace,
            VehicleReg = "ABC123",
            VehicleType = VehicleType.SmallCar,
            TimeIn = new DateTime(2026, 6, 26, 10, 0, 0, DateTimeKind.Utc),
            IsActive = true
        });
        await dbContext.SaveChangesAsync();

        var service = CreateService(dbContext, new FakeTimeProvider(DateTimeOffset.UtcNow));

        var result = await service.AllocateParkingSpace(new AllocateParkingSpaceDto("ABC123", VehicleType.SmallCar));

        Assert.False(result.Success);
        Assert.Equal("Vehicle is already parked.", result.ErrorMessage);
    }

    [Fact]
    public async Task AllocateParkingSpace_ShouldFail_WhenNoSpacesAreAvailable()
    {
        await using var dbContext = TestDbContextFactory.Create();
        dbContext.ParkingSpaces.Add(new ParkingSpace { SpaceNumber = 1, IsAvailable = false });
        await dbContext.SaveChangesAsync();

        var service = CreateService(dbContext, new FakeTimeProvider(DateTimeOffset.UtcNow));

        var result = await service.AllocateParkingSpace(new AllocateParkingSpaceDto("XYZ987", VehicleType.MediumCar));

        Assert.False(result.Success);
        Assert.Equal("No available parking spaces.", result.ErrorMessage);
    }

    [Fact]
    public async Task AllocateParkingSpace_ShouldPersistSessionAndReserveSpace()
    {
        await using var dbContext = TestDbContextFactory.Create();
        dbContext.ParkingSpaces.Add(new ParkingSpace { SpaceNumber = 5, IsAvailable = true });
        await dbContext.SaveChangesAsync();

        var fakeTimeProvider = new FakeTimeProvider(new DateTimeOffset(2026, 6, 26, 12, 30, 0, TimeSpan.Zero));
        var service = CreateService(dbContext, fakeTimeProvider);

        var result = await service.AllocateParkingSpace(new AllocateParkingSpaceDto("XYZ987", VehicleType.LargeCar));

        Assert.True(result.Success);
        Assert.Equal(5, result.Data!.SpaceNumber);
        Assert.Equal("XYZ987", result.Data.VehicleReg);
        Assert.Equal(fakeTimeProvider.GetUtcNow().UtcDateTime, result.Data.TimeIn);

        var persistedSpace = dbContext.ParkingSpaces.Single();
        var persistedSession = dbContext.ParkingSessions.Single();
        Assert.False(persistedSpace.IsAvailable);
        Assert.True(persistedSession.IsActive);
        Assert.Equal("XYZ987", persistedSession.VehicleReg);
    }

    [Fact]
    public async Task FreeParkingSpace_ShouldFail_WhenVehicleIsNotParked()
    {
        await using var dbContext = TestDbContextFactory.Create();
        var service = CreateService(dbContext, new FakeTimeProvider(DateTimeOffset.UtcNow));

        var result = await service.FreeParkingSpace(new FreeParkingSpaceDto("MISSING"));

        Assert.False(result.Success);
        Assert.Equal("Vehicle is not parked.", result.ErrorMessage);
    }

    [Fact]
    public async Task FreeParkingSpace_ShouldReleaseSpaceAndReturnCharge()
    {
        await using var dbContext = TestDbContextFactory.Create();
        var parkingSpace = new ParkingSpace { SpaceNumber = 8, IsAvailable = false };
        var timeIn = new DateTime(2026, 6, 26, 10, 0, 0, DateTimeKind.Utc);
        dbContext.ParkingSpaces.Add(parkingSpace);
        dbContext.ParkingSessions.Add(new ParkingSession
        {
            ParkingSpace = parkingSpace,
            VehicleReg = "EXIT1",
            VehicleType = VehicleType.SmallCar,
            TimeIn = timeIn,
            IsActive = true
        });
        await dbContext.SaveChangesAsync();

        var fakeTimeProvider = new FakeTimeProvider(new DateTimeOffset(timeIn.AddMinutes(5)));
        var service = CreateService(dbContext, fakeTimeProvider);

        var result = await service.FreeParkingSpace(new FreeParkingSpaceDto("EXIT1"));

        Assert.True(result.Success);
        Assert.Equal("EXIT1", result.Data!.VehicleReg);
        Assert.Equal(1.50, result.Data.VehicleCharge);
        Assert.Equal(timeIn, result.Data.TimeIn);
        Assert.Equal(fakeTimeProvider.GetUtcNow().UtcDateTime, result.Data.TimeOut);

        var persistedSession = dbContext.ParkingSessions.Single();
        var persistedSpace = dbContext.ParkingSpaces.Single();
        Assert.False(persistedSession.IsActive);
        Assert.Equal(fakeTimeProvider.GetUtcNow().UtcDateTime, persistedSession.TimeOut);
        Assert.True(persistedSpace.IsAvailable);
    }

    [Fact]
    public async Task GetActiveParkingSessions_ShouldReturnMappedSessions()
    {
        await using var dbContext = TestDbContextFactory.Create();
        dbContext.ParkingSpaces.AddRange(
            new ParkingSpace { Id = 1, SpaceNumber = 3, IsAvailable = false },
            new ParkingSpace { Id = 2, SpaceNumber = 4, IsAvailable = false });

        var activeSpace = dbContext.ParkingSpaces.Local.Single(space => space.SpaceNumber == 3);
        var endedSpace = dbContext.ParkingSpaces.Local.Single(space => space.SpaceNumber == 4);
        dbContext.ParkingSessions.AddRange(
            new ParkingSession
            {
                ParkingSpace = activeSpace,
                VehicleReg = "ACTIVE1",
                VehicleType = VehicleType.SmallCar,
                TimeIn = new DateTime(2026, 6, 26, 9, 0, 0, DateTimeKind.Utc),
                IsActive = true,
                TimeOut = null
            },
            new ParkingSession
            {
                ParkingSpace = endedSpace,
                VehicleReg = "ENDED1",
                VehicleType = VehicleType.MediumCar,
                TimeIn = new DateTime(2026, 6, 26, 8, 0, 0, DateTimeKind.Utc),
                TimeOut = new DateTime(2026, 6, 26, 8, 30, 0, DateTimeKind.Utc),
                IsActive = false
            });
        await dbContext.SaveChangesAsync();

        var service = CreateService(dbContext, new FakeTimeProvider(DateTimeOffset.UtcNow));

        var result = await service.GetActiveParkingSessions();

        Assert.True(result.Success);
        var session = Assert.Single(result.Data!);
        Assert.Equal(new ActiveParkingSessionDto("ACTIVE1", 3), session);
    }

    private static ParkingService CreateService(Data.CarParkingDbContext dbContext, TimeProvider timeProvider)
    {
        return new ParkingService(dbContext, Mock.Of<ILogger<ParkingService>>(), timeProvider);
    }
}
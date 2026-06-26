using CarParking.Api.Data.Entities;
using CarParking.Api.Models;

namespace CarParking.Api.Tests.Models;

public class ParkingChargeTests
{
    [Theory]
    [InlineData(0, VehicleType.SmallCar, 0.00)]
    [InlineData(30, VehicleType.SmallCar, 0.00)]
    [InlineData(60, VehicleType.SmallCar, 0.10)]
    [InlineData(300, VehicleType.SmallCar, 1.50)]
    [InlineData(600, VehicleType.SmallCar, 3.00)]
    [InlineData(300, VehicleType.MediumCar, 2.00)]
    [InlineData(300, VehicleType.LargeCar, 3.00)]
    [InlineData(599, VehicleType.SmallCar, 1.90)]
    [InlineData(1980, VehicleType.SmallCar, 9.30)]
    public void Constructor_ShouldCalculateExpectedCharge(int elapsedSeconds, VehicleType vehicleType, double expectedAmount)
    {
        var timeIn = new DateTime(2026, 6, 26, 10, 0, 0, DateTimeKind.Utc);
        var timeOut = timeIn.AddSeconds(elapsedSeconds);

        var charge = new ParkingCharge(timeIn, timeOut, vehicleType);

        Assert.Equal(expectedAmount, charge.Amount);
    }

    [Fact]
    public void Constructor_ShouldThrow_WhenTimeOutIsNull()
    {
        var timeIn = new DateTime(2026, 6, 26, 10, 0, 0, DateTimeKind.Utc);

        Assert.Throws<ArgumentNullException>(() => new ParkingCharge(timeIn, null, VehicleType.SmallCar));
    }

    [Fact]
    public void Constructor_ShouldThrow_WhenTimeOutIsEarlierThanTimeIn()
    {
        var timeIn = new DateTime(2026, 6, 26, 10, 0, 0, DateTimeKind.Utc);
        var timeOut = timeIn.AddMinutes(-1);

        Assert.Throws<ArgumentException>(() => new ParkingCharge(timeIn, timeOut, VehicleType.SmallCar));
    }
}
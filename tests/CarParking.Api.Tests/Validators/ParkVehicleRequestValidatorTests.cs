using CarParking.Api.Data.Entities;
using CarParking.Api.Models.Request;
using CarParking.Api.Validators;

namespace CarParking.Api.Tests.Validators;

public class ParkVehicleRequestValidatorTests
{
    private readonly ParkVehicleRequestValidator _validator = new();

    [Fact]
    public void Validate_ShouldPass_ForValidRequest()
    {
        var request = new ParkVehicleRequest
        {
            VehicleReg = "ABC123",
            VehicleType = VehicleType.MediumCar
        };

        var result = _validator.Validate(request);

        Assert.True(result.IsValid);
    }

    [Fact]
    public void Validate_ShouldFail_WhenVehicleRegistrationIsTooLong()
    {
        var request = new ParkVehicleRequest
        {
            VehicleReg = "12345678901",
            VehicleType = VehicleType.SmallCar
        };

        var result = _validator.Validate(request);

        Assert.False(result.IsValid);
        Assert.Contains(result.Errors, error => error.PropertyName == nameof(ParkVehicleRequest.VehicleReg));
    }

    [Theory]
    [InlineData((VehicleType)0)]
    [InlineData((VehicleType)99)]
    public void Validate_ShouldFail_WhenVehicleTypeIsOutsideEnum(VehicleType vehicleType)
    {
        var request = new ParkVehicleRequest
        {
            VehicleReg = "ABC123",
            VehicleType = vehicleType
        };

        var result = _validator.Validate(request);

        Assert.False(result.IsValid);
        Assert.Contains(result.Errors, error => error.PropertyName == nameof(ParkVehicleRequest.VehicleType));
    }

    [Theory]
    [InlineData(VehicleType.SmallCar)]
    [InlineData(VehicleType.MediumCar)]
    [InlineData(VehicleType.LargeCar)]
    public void Validate_ShouldPass_ForSupportedVehicleTypes(VehicleType vehicleType)
    {
        var request = new ParkVehicleRequest
        {
            VehicleReg = "ABC123",
            VehicleType = vehicleType
        };

        var result = _validator.Validate(request);

        Assert.True(result.IsValid);
    }
}
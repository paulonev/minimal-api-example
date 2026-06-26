using CarParking.Api.Models.Request;
using CarParking.Api.Validators;

namespace CarParking.Api.Tests.Validators;

public class ExitParkingRequestValidatorTests
{
    private readonly ExitParkingRequestValidator _validator = new();

    [Fact]
    public void Validate_ShouldPass_ForValidRequest()
    {
        var result = _validator.Validate(new ExitParkingRequest { VehicleReg = "ABC123" });

        Assert.True(result.IsValid);
    }

    [Theory]
    [InlineData("")]
    [InlineData("   ")]
    [InlineData("12345678901")]
    public void Validate_ShouldFail_ForInvalidVehicleRegistration(string vehicleReg)
    {
        var result = _validator.Validate(new ExitParkingRequest { VehicleReg = vehicleReg });

        Assert.False(result.IsValid);
        Assert.Contains(result.Errors, error => error.PropertyName == nameof(ExitParkingRequest.VehicleReg));
    }
}
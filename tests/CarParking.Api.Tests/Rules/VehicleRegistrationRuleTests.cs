using CarParking.Core.Rules;

namespace CarParking.Api.Tests.Rules;

public class VehicleRegistrationRuleTests
{
    [Theory]
    [InlineData(null, false)]
    [InlineData("", false)]
    [InlineData("ABC123", true)]
    [InlineData("1234567890", true)]
    [InlineData("12345678901", false)]
    public void IsValid_ShouldMatchExpectedOutcome(string? vehicleReg, bool expected)
    {
        var isValid = VehicleRegistrationRule.IsValid(vehicleReg!);

        Assert.Equal(expected, isValid);
    }
}
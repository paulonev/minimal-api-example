using CarParking.Api.Models.Request;
using CarParking.Api.Validators.Common;

namespace CarParking.Api.Validators;

public class ExitParkingRequestValidator : VehicleRegistrationValidator<ExitParkingRequest>
{
    public ExitParkingRequestValidator()
    {}
}
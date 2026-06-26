using CarParking.Api.Models.Request;
using CarParking.Api.Validators.Common;
using FluentValidation;

namespace CarParking.Api.Validators;

public class ParkVehicleRequestValidator : VehicleRegistrationValidator<ParkVehicleRequest>
{
    public ParkVehicleRequestValidator()
    {
        RuleFor(x => x.VehicleType)
            .NotEmpty()
            .WithMessage("Vehicle type is required.")
            .IsInEnum()
            .WithMessage("Invalid vehicle type. Allowed values are: SmallCar, MediumCar, LargeCar.");
    }
}
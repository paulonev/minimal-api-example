using CarParking.Api.Models.Request;
using CarParking.Core.Rules;
using FluentValidation;

namespace CarParking.Api.Validators.Common;

public class VehicleRegistrationValidator<T> : AbstractValidator<T> where T : IVehicleRegistrable
{
    public VehicleRegistrationValidator()
    {
        RuleFor(x => x.VehicleReg)
            .NotEmpty()
            .WithMessage("Vehicle registration is required.")
            .Must(VehicleRegistrationRule.IsValid)
            .WithMessage($"Vehicle registration must be at most {VehicleRegistrationRule.MaxLength} characters long.");
    }
}
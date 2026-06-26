namespace CarParking.Core.Rules;

public static class VehicleRegistrationRule
{
    public static readonly int MaxLength = 10;

    public static bool IsValid(string vehicleReg)
    {
        return !string.IsNullOrEmpty(vehicleReg) && vehicleReg.Length <= MaxLength;
    }
}
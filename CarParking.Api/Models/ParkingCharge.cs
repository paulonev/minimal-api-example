using CarParking.Api.Data.Entities;

namespace CarParking.Api.Models;

/// <summary>
/// Stateless parking charge with embedded calculator.
/// 
/// Formula
///   wholeMinutes         = floor(elapsedSeconds / 60)
///   perMinuteCharge      = wholeMinutes * rateForVehicleType
///   surchargeIncrements  = floor(wholeMinutes / 5)
///   totalCharge          = perMinuteCharge + (surchargeIncrements * 1.00)
///   result               = Math.Round(totalCharge, 2, MidpointRounding.AwayFromZero)
/// </summary>
public class ParkingCharge
{
    private static readonly Dictionary<int, double> VehicleTypeRates = new()
    {
        [1] = 0.10,
        [2] = 0.20,
        [3] = 0.40
    };

    public readonly double Amount;

    public ParkingCharge(DateTime timeIn, DateTime? timeOut, VehicleType vehicleType)
    {
        ArgumentNullException.ThrowIfNull(timeOut);
        if (timeOut < timeIn)
        {
            throw new ArgumentException("timeOut cannot be earlier than timeIn");
        }

        var elapsedSeconds = (timeOut.Value - timeIn).TotalSeconds;
        var wholeMinutes = Math.Floor(elapsedSeconds / 60);
        var rateForVehicleType = VehicleTypeRates[(int)vehicleType];
        var perMinuteCharge = wholeMinutes * rateForVehicleType;
        var surchargeIncrements = Math.Floor(wholeMinutes / 5);
        var totalCharge = perMinuteCharge + (surchargeIncrements * 1.00);
        Amount = Math.Round(totalCharge, 2, MidpointRounding.AwayFromZero);
    }
}

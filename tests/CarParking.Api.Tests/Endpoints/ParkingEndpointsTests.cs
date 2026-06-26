using CarParking.Api.Data.Entities;
using CarParking.Api.Endpoints;
using CarParking.Api.Models.DTO;
using CarParking.Api.Models.Request;
using CarParking.Api.Models.Response;
using CarParking.Api.Services;
using CarParking.Api.Tests.TestInfrastructure;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Moq;

namespace CarParking.Api.Tests.Endpoints;

public class ParkingEndpointsTests
{
    [Fact]
    public async Task GetAllParkingSpaces_ShouldReturnOkPayload()
    {
        var parkingService = new Mock<IParkingService>();
        parkingService
            .Setup(service => service.GetAllParkingSpaces())
            .ReturnsAsync(Result<ParkingSpacesDto>.SuccessResult(new ParkingSpacesDto(7, 3)));

        var result = await ParkingEndpoints.GetAllParkingSpaces(parkingService.Object);
        var executed = await ResultExecutor.ExecuteAsync(result);
        var payload = ResultExecutor.Deserialize<ParkingLotStatsResponse>(executed);

        Assert.Equal(StatusCodes.Status200OK, executed.StatusCode);
        Assert.Equal(7, payload.AvailableSpaces);
        Assert.Equal(3, payload.OccupiedSpaces);
    }

    [Fact]
    public async Task CreateParkingSpace_ShouldReturnValidationProblem_WhenRequestIsInvalid()
    {
        var request = new ParkVehicleRequest
        {
            VehicleReg = "",
            VehicleType = VehicleType.SmallCar
        };

        var result = await ParkingEndpoints.CreateParkingSpace(request, new CarParking.Api.Validators.ParkVehicleRequestValidator(), Mock.Of<IParkingService>());
        var executed = await ResultExecutor.ExecuteAsync(result);
        var payload = ResultExecutor.Deserialize<ValidationProblemDetails>(executed);

        Assert.Equal(StatusCodes.Status400BadRequest, executed.StatusCode);
        Assert.Contains(nameof(ParkVehicleRequest.VehicleReg), payload.Errors.Keys);
    }

    [Fact]
    public async Task CreateParkingSpace_ShouldReturnProblem_WhenServiceFails()
    {
        var parkingService = new Mock<IParkingService>();
        parkingService
            .Setup(service => service.AllocateParkingSpace(It.IsAny<AllocateParkingSpaceDto>()))
            .ReturnsAsync(Result<AllocateParkingSpaceResultDto>.Failure("No available parking spaces."));

        var request = new ParkVehicleRequest
        {
            VehicleReg = "ABC123",
            VehicleType = VehicleType.SmallCar
        };

        var result = await ParkingEndpoints.CreateParkingSpace(request, new CarParking.Api.Validators.ParkVehicleRequestValidator(), parkingService.Object);
        var executed = await ResultExecutor.ExecuteAsync(result);
        var payload = ResultExecutor.Deserialize<ProblemDetails>(executed);

        Assert.Equal(StatusCodes.Status400BadRequest, executed.StatusCode);
        Assert.Equal("Error trying to allocate parking space", payload.Title);
        Assert.Equal("No available parking spaces.", payload.Detail);
    }

    [Fact]
    public async Task CreateParkingSpace_ShouldReturnMappedResponse_WhenServiceSucceeds()
    {
        var timeIn = new DateTime(2026, 6, 26, 12, 0, 0, DateTimeKind.Utc);
        var parkingService = new Mock<IParkingService>();
        parkingService
            .Setup(service => service.AllocateParkingSpace(It.IsAny<AllocateParkingSpaceDto>()))
            .ReturnsAsync(Result<AllocateParkingSpaceResultDto>.SuccessResult(new AllocateParkingSpaceResultDto(5, timeIn, "ABC123")));

        var request = new ParkVehicleRequest
        {
            VehicleReg = "ABC123",
            VehicleType = VehicleType.SmallCar
        };

        var result = await ParkingEndpoints.CreateParkingSpace(request, new CarParking.Api.Validators.ParkVehicleRequestValidator(), parkingService.Object);
        var executed = await ResultExecutor.ExecuteAsync(result);
        var payload = ResultExecutor.Deserialize<ParkVehicleResponse>(executed);

        Assert.Equal(StatusCodes.Status200OK, executed.StatusCode);
        Assert.Equal("ABC123", payload.VehicleReg);
        Assert.Equal(5, payload.SpaceNumber);
        Assert.Equal(timeIn, payload.TimeIn.ToUniversalTime());
    }

    [Fact]
    public async Task ExitParkingSpace_ShouldReturnValidationProblem_WhenRequestIsInvalid()
    {
        var request = new ExitParkingRequest { VehicleReg = "" };

        var result = await ParkingEndpoints.ExitParkingSpace(request, new CarParking.Api.Validators.ExitParkingRequestValidator(), Mock.Of<IParkingService>());
        var executed = await ResultExecutor.ExecuteAsync(result);
        var payload = ResultExecutor.Deserialize<ValidationProblemDetails>(executed);

        Assert.Equal(StatusCodes.Status400BadRequest, executed.StatusCode);
        Assert.Contains(nameof(ExitParkingRequest.VehicleReg), payload.Errors.Keys);
    }

    [Fact]
    public async Task ExitParkingSpace_ShouldReturnProblem_WhenServiceFails()
    {
        var parkingService = new Mock<IParkingService>();
        parkingService
            .Setup(service => service.FreeParkingSpace(It.IsAny<FreeParkingSpaceDto>()))
            .ReturnsAsync(Result<FreeParkingSpaceResultDto>.Failure("Vehicle is not parked."));

        var request = new ExitParkingRequest { VehicleReg = "EXIT1" };

        var result = await ParkingEndpoints.ExitParkingSpace(request, new CarParking.Api.Validators.ExitParkingRequestValidator(), parkingService.Object);
        var executed = await ResultExecutor.ExecuteAsync(result);
        var payload = ResultExecutor.Deserialize<ProblemDetails>(executed);

        Assert.Equal(StatusCodes.Status400BadRequest, executed.StatusCode);
        Assert.Equal("Error trying to free parking space", payload.Title);
        Assert.Equal("Vehicle is not parked.", payload.Detail);
    }

    [Fact]
    public async Task ExitParkingSpace_ShouldReturnMappedResponse_WhenServiceSucceeds()
    {
        var timeIn = new DateTime(2026, 6, 26, 10, 0, 0, DateTimeKind.Utc);
        var timeOut = timeIn.AddMinutes(5);
        var parkingService = new Mock<IParkingService>();
        parkingService
            .Setup(service => service.FreeParkingSpace(It.IsAny<FreeParkingSpaceDto>()))
            .ReturnsAsync(Result<FreeParkingSpaceResultDto>.SuccessResult(new FreeParkingSpaceResultDto("EXIT1", 1.50, timeIn, timeOut)));

        var request = new ExitParkingRequest { VehicleReg = "EXIT1" };

        var result = await ParkingEndpoints.ExitParkingSpace(request, new CarParking.Api.Validators.ExitParkingRequestValidator(), parkingService.Object);
        var executed = await ResultExecutor.ExecuteAsync(result);
        var payload = ResultExecutor.Deserialize<ExitParkingResponse>(executed);

        Assert.Equal(StatusCodes.Status200OK, executed.StatusCode);
        Assert.Equal("EXIT1", payload.VehicleReg);
        Assert.Equal(1.50, payload.VehicleCharge);
        Assert.Equal(timeIn, payload.TimeIn.ToUniversalTime());
        Assert.Equal(timeOut, payload.TimeOut.ToUniversalTime());
    }

    [Fact]
    public async Task GetActiveParkingSessions_ShouldReturnMappedSessions()
    {
        var parkingService = new Mock<IParkingService>();
        parkingService
            .Setup(service => service.GetActiveParkingSessions())
            .ReturnsAsync(Result<List<ActiveParkingSessionDto>>.SuccessResult(
                [new ActiveParkingSessionDto("ABC123", 2)]));

        var result = await ParkingEndpoints.GetActiveParkingSessions(parkingService.Object);
        var executed = await ResultExecutor.ExecuteAsync(result);
        var payload = ResultExecutor.Deserialize<ActiveParkingSessionsResponse>(executed);

        Assert.Equal(StatusCodes.Status200OK, executed.StatusCode);
        var session = Assert.Single(payload.Sessions);
        Assert.Equal("ABC123", session.VehicleReg);
        Assert.Equal(2, session.SpaceNumber);
    }
}
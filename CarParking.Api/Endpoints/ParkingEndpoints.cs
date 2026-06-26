using CarParking.Api.Models.DTO;
using CarParking.Api.Models.Request;
using CarParking.Api.Models.Response;
using CarParking.Api.Services;
using FluentValidation;
using Microsoft.AspNetCore.Mvc;

namespace CarParking.Api.Endpoints;

public static class ParkingEndpoints
{
    public static void MapParkingEndpoints(this IEndpointRouteBuilder app)
    {
        var group = app.MapGroup("/parking");

        group
            .MapGet("", GetAllParkingSpaces)
            .Produces<ParkingLotStatsResponse>();

        group
            .MapPost("", CreateParkingSpace)
            .Produces<ParkVehicleResponse>()
            .ProducesValidationProblem();

        group.MapPost("/exit", ExitParkingSpace)
            .Produces<ExitParkingResponse>()
            .ProducesValidationProblem();
        
        group.MapGet("/active", GetActiveParkingSessions)
            .Produces<ActiveParkingSessionsResponse>();
    }

    public static async Task<IResult> GetAllParkingSpaces(
        [FromServices] IParkingService parkingService
    )
    {
        var result = await parkingService.GetAllParkingSpaces();

        return Results.Ok(new ParkingLotStatsResponse()
        {
            AvailableSpaces = result.Data!.AvailableSpaces,
            OccupiedSpaces = result.Data!.OccupiedSpaces
        });
    }

    public static async Task<IResult> CreateParkingSpace(
        [FromBody] ParkVehicleRequest request,
        [FromServices] IValidator<ParkVehicleRequest> validator,
        [FromServices] IParkingService parkingService
    )
    {
        var validationResult = validator.Validate(request);
        if (!validationResult.IsValid)
        {
            return Results.ValidationProblem(validationResult.ToDictionary());
        }

        var result = await parkingService.AllocateParkingSpace(
            new AllocateParkingSpaceDto(request.VehicleReg, request.VehicleType));

        if (!result.Success)
        {
            return Results.Problem(
                type: "Bad Request",
                title: "Error trying to allocate parking space",
                detail: result.ErrorMessage,
                statusCode: StatusCodes.Status400BadRequest);
        }

        return Results.Ok(new ParkVehicleResponse()
        {
            VehicleReg = request.VehicleReg,
            SpaceNumber = result.Data!.SpaceNumber,
            TimeIn = result.Data!.TimeIn
        });
    }

    public static async Task<IResult> ExitParkingSpace(
        [FromBody] ExitParkingRequest request,
        [FromServices] IValidator<ExitParkingRequest> validator,
        [FromServices] IParkingService parkingService
    )
    {
        var validationResult = validator.Validate(request);
        if (!validationResult.IsValid)
        {
            return Results.ValidationProblem(validationResult.ToDictionary());
        }

        var result = await parkingService.FreeParkingSpace(
            new FreeParkingSpaceDto(request.VehicleReg));

        if (!result.Success)
        {
            return Results.Problem(
                type: "Bad Request",
                title: "Error trying to free parking space",
                detail: result.ErrorMessage,
                statusCode: StatusCodes.Status400BadRequest);
        }

        return Results.Ok(new ExitParkingResponse()
        {
            VehicleReg = request.VehicleReg,
            VehicleCharge = result.Data!.VehicleCharge,
            TimeIn = result.Data!.TimeIn,
            TimeOut = result.Data!.TimeOut
        });
    }

    public static async Task<IResult> GetActiveParkingSessions(
        [FromServices] IParkingService parkingService
    )
    {
        var result = await parkingService.GetActiveParkingSessions();

        if (!result.Success)
        {
            return Results.Problem(
                type: "Bad Request",
                title: "Error trying to retrieve active parking sessions",
                detail: result.ErrorMessage,
                statusCode: StatusCodes.Status400BadRequest);
        }

        return Results.Ok(new ActiveParkingSessionsResponse()
        {
            Sessions = result.Data!.Select(x => new ActiveParkingSessionResponseItem
            {
                VehicleReg = x.VehicleReg,
                SpaceNumber = x.SpaceNumber
            }).ToList()
        });
    }
}
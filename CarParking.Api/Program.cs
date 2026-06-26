using System.Diagnostics;
using CarParking.Api.Data;
using CarParking.Api.Endpoints;
using CarParking.Api.Models.Request;
using CarParking.Api.Services;
using CarParking.Api.Validators.Common;
using FluentValidation;
using Microsoft.AspNetCore.Http.Features;
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
// Learn more about configuring OpenAPI at https://aka.ms/aspnet/openapi
builder.Services.AddOpenApi("parking");

builder.Services.AddApiVersioning(options =>
{
    options.DefaultApiVersion = new Asp.Versioning.ApiVersion(1, 0);
    options.AssumeDefaultVersionWhenUnspecified = true;
    options.ReportApiVersions = true;
    options.ApiVersionReader = new Asp.Versioning.UrlSegmentApiVersionReader();
});

// may not work as expected yet
builder.Services.AddValidatorsFromAssemblyContaining<VehicleRegistrationValidator<IVehicleRegistrable>>();

builder.Services.AddScoped<IParkingService, ParkingService>();
builder.Services.AddSingleton(TimeProvider.System);

builder.Services.AddDbContext<CarParkingDbContext>(options =>
{
    options.UseNpgsql(builder.Configuration.GetConnectionString("Postgres"), npgsqlOpts =>
    {
        npgsqlOpts.EnableRetryOnFailure(
            maxRetryCount: 3,
            maxRetryDelay: TimeSpan.FromSeconds(5),
            errorCodesToAdd: null
        );
    });
});

builder.Services.AddProblemDetails(options =>
{
    options.CustomizeProblemDetails = context =>
    {
        context.ProblemDetails.Instance =
            $"{context.HttpContext.Request.Method} {context.HttpContext.Request.Path}";

        context.ProblemDetails.Extensions.TryAdd("requestId", context.HttpContext.TraceIdentifier);

        Activity? activity = context.HttpContext.Features.Get<IHttpActivityFeature>()?.Activity;
        context.ProblemDetails.Extensions.TryAdd("traceId", activity?.Id);
    };
});

builder.Services.AddExceptionHandler<GlobalExceptionHandler>();

var app = builder.Build();

// Configure the HTTP request pipeline.
var enableSwagger = builder.Configuration.GetValue<bool>("EnableSwagger");
if (app.Environment.IsDevelopment() || enableSwagger)
{
    app.MapOpenApi("/openapi/{documentName}.json");
    app.UseSwaggerUI(options =>
    {
        options.SwaggerEndpoint("/openapi/parking.json", "Car Parking API");
    });
}

app.UseExceptionHandler();
app.UseHttpsRedirection();

app.MapParkingEndpoints();

app.Run();


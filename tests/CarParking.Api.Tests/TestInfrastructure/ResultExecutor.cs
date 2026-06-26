using System.Text.Json;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.DependencyInjection;

namespace CarParking.Api.Tests.TestInfrastructure;

public static class ResultExecutor
{
    private static readonly JsonSerializerOptions JsonOptions = new(JsonSerializerDefaults.Web);
    private static readonly IServiceProvider Services = BuildServices();

    public static async Task<ExecutedResult> ExecuteAsync(IResult result)
    {
        var httpContext = new DefaultHttpContext();
        httpContext.Response.Body = new MemoryStream();
        httpContext.RequestServices = Services;

        await result.ExecuteAsync(httpContext);

        httpContext.Response.Body.Position = 0;
        using var reader = new StreamReader(httpContext.Response.Body);
        var body = await reader.ReadToEndAsync();

        return new ExecutedResult(httpContext.Response.StatusCode, body);
    }

    public static T Deserialize<T>(ExecutedResult result)
    {
        return JsonSerializer.Deserialize<T>(result.Body, JsonOptions)!;
    }

    private static IServiceProvider BuildServices()
    {
        return new ServiceCollection()
            .AddLogging()
            .AddProblemDetails()
            .BuildServiceProvider();
    }
}

public sealed record ExecutedResult(int StatusCode, string Body);
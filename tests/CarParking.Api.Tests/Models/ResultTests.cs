using CarParking.Api.Models.DTO;

namespace CarParking.Api.Tests.Models;

public class ResultTests
{
    [Fact]
    public void Failure_ShouldCreateFailedResult()
    {
        var result = Result<string>.Failure("boom");

        Assert.False(result.Success);
        Assert.Null(result.Data);
        Assert.Equal("boom", result.ErrorMessage);
    }

    [Fact]
    public void SuccessResult_ShouldCreateSuccessfulResult()
    {
        var result = Result<int>.SuccessResult(42);

        Assert.True(result.Success);
        Assert.Equal(42, result.Data);
        Assert.Null(result.ErrorMessage);
    }
}
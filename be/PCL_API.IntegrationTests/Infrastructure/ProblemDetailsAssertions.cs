using System.Net;
using System.Net.Http.Json;
using System.Text.Json;
using Microsoft.AspNetCore.Mvc;
using Xunit;

namespace PCL_API.IntegrationTests.Infrastructure;

public static class ProblemDetailsAssertions
{
    public static async Task<ProblemDetails> ShouldBeProblemAsync(
        this HttpResponseMessage response,
        HttpStatusCode expectedStatus,
        string expectedCode)
    {
        Assert.Equal(expectedStatus, response.StatusCode);

        ProblemDetails? problem = await response.Content
            .ReadFromJsonAsync<ProblemDetails>();

        Assert.NotNull(problem);
        Assert.Equal(expectedCode, problem.Title);
        Assert.Equal((int)expectedStatus, problem.Status);

        return problem;
    }

    public static async Task<ProblemDetails> ShouldBeProblemAsync(
        this HttpResponseMessage response,
        HttpStatusCode expectedStatus,
        string expectedCode,
        string expectedDetail)
    {
        ProblemDetails problem = await response.ShouldBeProblemAsync(expectedStatus, expectedCode);
        Assert.Equal(expectedDetail, problem.Detail);
        return problem;
    }

    public static void ShouldContainValidationErrors(this ProblemDetails problem)
    {
        Assert.True(problem.Extensions.TryGetValue("errors", out object? value));
        JsonElement errors = Assert.IsType<JsonElement>(value);
        Assert.Equal(JsonValueKind.Array, errors.ValueKind);
        Assert.NotEqual(0, errors.GetArrayLength());
    }
}

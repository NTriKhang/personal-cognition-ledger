using System.Net;
using System.Net.Http.Json;
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
}

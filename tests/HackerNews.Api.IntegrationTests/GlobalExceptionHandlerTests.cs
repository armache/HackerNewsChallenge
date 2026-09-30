using FluentAssertions;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Testing;
using System.Net;
using System.Net.Http.Json;

namespace HackerNews.Api.IntegrationTests;

public class GlobalExceptionHandlerTests
{
    [Fact]
    public async Task GetBestStories_ReturnsGenericProblemDetails_ForUnknownException()
    {
        var handler = new FakeHackerNewsHandler(1)
        {
            ThrowOnFetchingBestStoryIds = new InvalidOperationException("some bug..."),
        };

        await using var factory = new HackerNewsApiFactory(handler);

        var client = factory.CreateHttpClient();

        var response = await client.GetAsync("/api/stories/best/3", CancellationToken.None);
        response.StatusCode.Should().Be(HttpStatusCode.InternalServerError);

        var problem = await response.Content.ReadFromJsonAsync<ProblemDetails>(CancellationToken.None);

        problem!.Title.Should().Be("An unexpected error occurred");
        problem!.Detail.Should().Contain("some bug...");
    }

    [Fact]
    public async Task GetBestStories_DoesNotLeakExceptionDetails_OutsideDevelopment()
    {
        var handler = new FakeHackerNewsHandler(1)
        {
            ThrowOnFetchingBestStoryIds = new InvalidOperationException("some bug..."),
        };

        await using var factory = new HackerNewsApiFactory(handler).WithWebHostBuilder(builder => builder.UseEnvironment("Production"));
        var client = factory.CreateClient(new WebApplicationFactoryClientOptions { BaseAddress = new Uri("https://localhost") });

        var response = await client.GetAsync("/api/stories/best/3", CancellationToken.None);

        response.StatusCode.Should().Be(HttpStatusCode.InternalServerError);
        var problem = await response.Content.ReadFromJsonAsync<ProblemDetails>(CancellationToken.None);

        // AM: exception message must never leak to the caller in Production environment
        problem!.Detail.Should().NotContain("some bug...");
    }
}

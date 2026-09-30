using HackerNews.Infrastructure;
using Microsoft.Extensions.Diagnostics.HealthChecks;
using Microsoft.Extensions.Options;

namespace HackerNews.Api.Extensions;

public static class HealthCheckExtensions
{
    public static IServiceCollection AddApiHealthChecks(this IServiceCollection services)
    {
        services.AddHttpClient(HackerNewsHealthCheck.HttpClientName, (sp, client) =>
        {
            var options = sp.GetRequiredService<IOptions<HackerNewsOptions>>().Value;
            client.BaseAddress = new Uri(options.BaseUrl);
            client.Timeout = TimeSpan.FromSeconds(5);
        });

        services.AddHealthChecks().AddCheck<HackerNewsHealthCheck>("hackernews", tags: ["ready"]);

        return services;
    }
}

public sealed class HackerNewsHealthCheck(IHttpClientFactory httpClientFactory) : IHealthCheck
{
    public const string HttpClientName = "HackerNewsHealthCheck";
    private readonly HttpClient httpClient = httpClientFactory.CreateClient(HttpClientName);

    public async Task<HealthCheckResult> CheckHealthAsync(HealthCheckContext context, CancellationToken cancellationToken = default)
    {
        try
        {
            //AM: maxitem.json - cheapest endpoint on Hacker News API, returns the current largest item id.
            using var response = await httpClient.GetAsync("maxitem.json", cancellationToken);
            return response.IsSuccessStatusCode
                ? HealthCheckResult.Healthy()
                : HealthCheckResult.Unhealthy($"Hacker News API responded with status {(int)response.StatusCode}.");
        }
        catch (Exception ex)
        {
            return HealthCheckResult.Unhealthy("Hacker News API is unreachable.", ex);
        }
    }
}

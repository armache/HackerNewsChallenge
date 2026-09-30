using HackerNews.Infrastructure;
using HackerNews.Infrastructure.HttpClients;
using Microsoft.Extensions.Options;

namespace HackerNews.Api.Extensions;

public static class HackerNewsExtension
{
    public static IServiceCollection AddHackerNewsHttpClient(this IServiceCollection services)
    {
        services.AddHttpClient<HackerNewsClient>((sp, client) =>
        {
            var options = sp.GetRequiredService<IOptions<HackerNewsOptions>>().Value;
            client.BaseAddress = new Uri(options.BaseUrl);
        })
        .AddStandardResilienceHandler(); //AM: using default values

        return services;
    }
}

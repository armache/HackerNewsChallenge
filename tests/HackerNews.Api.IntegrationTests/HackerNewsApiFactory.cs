using HackerNews.Infrastructure.HttpClients;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Extensions.DependencyInjection;

namespace HackerNews.Api.IntegrationTests;

public sealed class HackerNewsApiFactory(FakeHackerNewsHandler handler) : WebApplicationFactory<Program>
{
    protected override void ConfigureWebHost(IWebHostBuilder builder)
    {
        builder.ConfigureServices(services =>
        {
            services.AddHttpClient<HackerNewsClient>().ConfigurePrimaryHttpMessageHandler(() => handler);
        });
    }

    public HttpClient CreateHttpClient()
    {
        return CreateClient(new WebApplicationFactoryClientOptions 
        { 
            BaseAddress = new Uri("https://localhost") 
        });
    }
}
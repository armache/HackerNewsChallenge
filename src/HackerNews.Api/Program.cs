using HackerNews.Api.Exceptions;
using HackerNews.Api.Extensions;
using HackerNews.Domain.Interfaces;
using HackerNews.Domain.Services;
using HackerNews.Infrastructure;
using HackerNews.Infrastructure.Cache;
using HackerNews.Infrastructure.HttpClients;
using Microsoft.AspNetCore.Diagnostics.HealthChecks;
using Microsoft.Extensions.Caching.Hybrid;
using Microsoft.Extensions.Options;
using Scalar.AspNetCore;

var builder = WebApplication.CreateBuilder(args);

// Add services to the 
builder.Services.AddControllers();
builder.Services.AddOpenApi();
builder.Services.AddHybridCache();

builder.Services.AddProblemDetails();

//AM: order matters - generic exception handler should be last in the chain
builder.Services.AddExceptionHandler<HackerNewsExceptionHandler>();
builder.Services.AddExceptionHandler<GlobalExceptionHandler>();

//AM: fail fast if config is missing
builder.Services.AddOptions<HackerNewsOptions>()
    .Bind(builder.Configuration.GetSection("HackerNews"))
    .Validate(o => !string.IsNullOrWhiteSpace(o.BaseUrl), "HackerNews:BaseUrl is required.")
    .Validate(o => o.BestStoryIdsCacheDuration > TimeSpan.Zero, "HackerNews:BestStoryIdsCacheDuration must be greater than zero.")
    .Validate(o => o.StoryItemCacheDuration > TimeSpan.Zero, "HackerNews:StoryItemCacheDuration must be greater than zero.")
    .Validate(o => Convert.ToInt32(o.MaxConcurrentUpstreamRequests) > 0, "HackerNews:MaxConcurrentUpstreamRequests has invalid value.")
    .ValidateOnStart();

//AM: rate limiting for API endpoints (100 request per minute)
builder.Services.AddApiRateLimiting(builder.Configuration);

//AM: request timeouts for API endpoints (35 seconds)
builder.Services.AddRequestTimeouts(builder.Configuration);

//AM: app-wide singleton for limiting concurrent upstream requests to HackerNews API (20 concurrent requests)
builder.Services.AddSingleton<HackerNewsUpstreamThrottle>();

//AM: register the HackerNewsClient with resilience policies
builder.Services.AddHackerNewsHttpClient();

//AM: register the client with caching and throttling
builder.Services.AddScoped<IHackerNewsClient>(sp => new CachedHackerNewsClient(
    sp.GetRequiredService<HackerNewsClient>(),
    sp.GetRequiredService<HybridCache>(),
    sp.GetRequiredService<IOptions<HackerNewsOptions>>()));

//AM: health checks
builder.Services.AddApiHealthChecks();

//AM: register domain services
builder.Services.AddScoped<IStoryService, StoryService>();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
    app.MapScalarApiReference();
}

app.UseExceptionHandler();
app.UseHttpsRedirection();
app.UseRequestTimeouts();
app.UseRateLimiter();

app.MapControllers();

//AM: map health check endpoints
app.MapHealthChecks("/health/live", new HealthCheckOptions
{
    Predicate = _ => false,
});

app.MapHealthChecks("/health/ready", new HealthCheckOptions
{
    Predicate = check => check.Tags.Contains("ready"),
});

app.Run();

public partial class Program { }

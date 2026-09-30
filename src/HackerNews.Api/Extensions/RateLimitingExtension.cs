using HackerNews.Infrastructure;
using System.Threading.RateLimiting;

namespace HackerNews.Api.Extensions;

public static class RateLimitingExtension
{
    public static IServiceCollection AddApiRateLimiting(this IServiceCollection services, IConfiguration configuration)
    {
        var rateLimitingOptions = configuration.GetSection("RateLimiting").Get<RateLimitingOptions>();

        services.AddRateLimiter(options =>
        {
            options.RejectionStatusCode = StatusCodes.Status429TooManyRequests;
            options.GlobalLimiter = CreateGlobalLimiter(rateLimitingOptions!);
        });

        return services;
    }

    public static PartitionedRateLimiter<HttpContext> CreateGlobalLimiter(RateLimitingOptions options)
    {
        return PartitionedRateLimiter.CreateChained(
            PartitionedRateLimiter.Create<HttpContext, string>(httpContext =>
            {
                var clientKey = httpContext.Connection.RemoteIpAddress?.ToString() ?? "unknown";

                return RateLimitPartition.GetFixedWindowLimiter(clientKey, _ => new FixedWindowRateLimiterOptions
                {
                    PermitLimit = options.PermitLimit,
                    Window = TimeSpan.FromSeconds(options.WindowSeconds),
                    QueueLimit = options.QueueLimit,
                    QueueProcessingOrder = QueueProcessingOrder.OldestFirst,
                });
            }),
            PartitionedRateLimiter.Create<HttpContext, string>(_ =>
                RateLimitPartition.GetConcurrencyLimiter("global", _ => new ConcurrencyLimiterOptions
                {
                    PermitLimit = options.MaxConcurrentRequests,
                    QueueLimit = options.ConcurrencyQueueLimit,
                    QueueProcessingOrder = QueueProcessingOrder.OldestFirst,
                })
            )
        );
    }
}

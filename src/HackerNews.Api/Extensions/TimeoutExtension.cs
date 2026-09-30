using Microsoft.AspNetCore.Http.Timeouts;

namespace HackerNews.Api.Extensions;

public static class TimeoutExtension
{
    public static IServiceCollection AddRequestTimeouts(this IServiceCollection services, IConfiguration configuration)
    {
        services.AddRequestTimeouts(options =>
        {
            var timeoutSeconds = configuration.GetValue<int>("RequestTimeoutSeconds");

            if (timeoutSeconds <= 0) 
                throw new InvalidOperationException("RequestTimeoutSeconds must be configured with a positive integer.");

            options.DefaultPolicy = new RequestTimeoutPolicy
            {
                Timeout = TimeSpan.FromSeconds(timeoutSeconds),
                TimeoutStatusCode = StatusCodes.Status503ServiceUnavailable,
            };
        });

        return services;
    }
}

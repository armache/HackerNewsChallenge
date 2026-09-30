using Microsoft.AspNetCore.Diagnostics;
using Microsoft.AspNetCore.Mvc;
using Polly.CircuitBreaker;
using Polly.Timeout;

namespace HackerNews.Api.Exceptions
{
    public sealed class HackerNewsExceptionHandler(
        IProblemDetailsService problemDetailsService,
        ILogger<HackerNewsExceptionHandler> logger) : IExceptionHandler
    {
        public async ValueTask<bool> TryHandleAsync(HttpContext httpContext, Exception exception, CancellationToken cancellationToken)
        {
            if (exception is not (HttpRequestException or BrokenCircuitException or TimeoutRejectedException))
            {
                return false;
            }

            logger.LogError(exception, "Hacker News API request failed");

            httpContext.Response.StatusCode = StatusCodes.Status502BadGateway;

            return await problemDetailsService.TryWriteAsync(new ProblemDetailsContext
            {
                HttpContext = httpContext,
                Exception = exception,
                ProblemDetails = new ProblemDetails
                {
                    Title = "Hacker News is currently unavailable",
                    Detail = "The upstream Hacker News API could not be reached. Please try again later.",
                    Status = StatusCodes.Status502BadGateway
                },
            });
        }
    }
}

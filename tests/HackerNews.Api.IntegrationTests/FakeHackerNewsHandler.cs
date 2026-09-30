using HackerNews.Domain.Models;
using System.Collections.Concurrent;
using System.Net;
using System.Net.Http.Json;

namespace HackerNews.Api.IntegrationTests;

public sealed class FakeHackerNewsHandler(params int[] bestStoryIds) : HttpMessageHandler
{
    public Exception? ThrowOnFetchingBestStoryIds { get; set; }
    public HttpStatusCode MaxItemStatus { get; set; } = HttpStatusCode.OK; //AM: used for health checks


    protected override Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
    {
        var path = request.RequestUri!.AbsolutePath;

        if (path.EndsWith("beststories.json", StringComparison.Ordinal))
        {
            if (ThrowOnFetchingBestStoryIds is not null)
            {
                throw ThrowOnFetchingBestStoryIds;
            }

            return Respond(HttpStatusCode.OK, bestStoryIds);
        }

        if (path.EndsWith("maxitem.json", StringComparison.Ordinal))
        {
            return Respond(MaxItemStatus, 12_345_678);
        }

        throw new NotSupportedException($"FakeHackerNewsHandler has no route configured for '{path}'.");
    }

    private Task<HttpResponseMessage> Respond<T>(HttpStatusCode status, T body)
    {
        return Task.FromResult(status == HttpStatusCode.OK
            ? new HttpResponseMessage(HttpStatusCode.OK) { Content = JsonContent.Create(body) }
            : new HttpResponseMessage(status));
    }
}
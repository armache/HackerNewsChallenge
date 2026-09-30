# HackerNews Best Stories API

A small web API that sits in front of the public Hacker News API and provides the best N stories right now. The official Hacker News API doesn't give you that directly — it only returns a list of a few hundred "best" story IDs, and you have to fetch each story one by one to find out its score before you can sort and return it. You can achieve it by sending a GET request to: "/api/stories/best/{count}" endpoint and get back a ranked list of stories with title, author, score, comment count, and link, sorted from highest score to lowest.

Under the hood, the code goes further than a bare "fetch and sort" solution. Calls to the upstream Hacker News API are cached, rate-limited, and capped in concurrency, so the service doesn't overwhelm (or get overwhelmed by) an external API it doesn't control. Failures are handled deliberately at every layer instead of one catch-all: a broken upstream call returns a 502, a slow one times out with a 503, and a single bad story is skipped rather than failing the whole request. Configuration is validated on startup so a bad setting fails loudly when the app starts. The code itself is organized into separated layers (domain, infrastructure, API), with both unit and integration tests.

## Running it

```powershell
dotnet run --project src/HackerNews.Api
```

In Development mode, an interactive API reference is available at `/scalar`.

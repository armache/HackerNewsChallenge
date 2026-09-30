using HackerNews.Domain.Interfaces;
using HackerNews.Domain.Models;
using Microsoft.AspNetCore.Mvc;

namespace HackerNews.Api.Controllers;

[Route("api/[controller]")]
[ApiController]
public class StoriesController(IStoryService storyService) : ControllerBase
{
    [HttpGet("best/{count:int}")]
    [ProducesResponseType<IReadOnlyList<StoryDto>>(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status429TooManyRequests)] //AM: returned by RateLimiter
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status502BadGateway)] //AM: returned by HackerNewsExceptionHandler
    [ProducesResponseType(StatusCodes.Status503ServiceUnavailable)] //AM: returned by TimeoutExtension
    [ProducesDefaultResponseType(typeof(ProblemDetails))] //AM: returned by GlobalExceptionHandler
    public async Task<ActionResult<IReadOnlyList<StoryDto>>> GetBestStoriesAsync(int count, CancellationToken ct)
    {
        if (count > 0)
        {
            var stories = await storyService.GetBestStoriesAsync(count, ct);
            return Ok(stories);
        }
        else
        {
            return BadRequest(new ProblemDetails
            {
                Title = "Invalid count",
                Detail = "count must be greater than zero.",
                Status = StatusCodes.Status400BadRequest,
            });
        }
    }
}

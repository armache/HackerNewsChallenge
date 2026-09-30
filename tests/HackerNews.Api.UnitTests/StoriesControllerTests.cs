using FluentAssertions;
using HackerNews.Api.Controllers;
using HackerNews.Domain.Interfaces;
using HackerNews.Domain.Models;
using Microsoft.AspNetCore.Mvc;
using Moq;

namespace HackerNews.Api.UnitTests;

public class StoriesControllerTests
{
    private readonly Mock<IStoryService> service = new();
    private StoriesController CreateSut() => new(service.Object);

    [Fact]
    public async Task GetBestStories_ReturnsOkWithStories_ForValidCount()
    {
        var stories = new List<StoryDto>
        {
            new()
            {
                Title = "Title",
                Uri = "https://sample.com",
                PostedBy = "some author",
                Time = DateTimeOffset.UtcNow,
                Score = 100,
                CommentCount = 5,
            },
        };
        service.Setup(s => s.GetBestStoriesAsync(5, It.IsAny<CancellationToken>())).ReturnsAsync(stories);

        var result = await CreateSut().GetBestStoriesAsync(5, CancellationToken.None);

        var okResult = result.Result.Should().BeOfType<OkObjectResult>().Subject;
        okResult.Value.Should().BeSameAs(stories);
    }

    [Theory]
    [InlineData(0)]
    [InlineData(-1)]
    public async Task GetBestStories_ReturnsBadRequest_ForNonPositiveCount(int count)
    {
        var result = await CreateSut().GetBestStoriesAsync(count, CancellationToken.None);

        result.Result.Should().BeOfType<BadRequestObjectResult>();
        service.Verify(s => s.GetBestStoriesAsync(It.IsAny<int>(), It.IsAny<CancellationToken>()), Times.Never);
    }
}

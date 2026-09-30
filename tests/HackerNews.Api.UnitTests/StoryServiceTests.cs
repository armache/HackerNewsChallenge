using FluentAssertions;
using HackerNews.Domain.Interfaces;
using HackerNews.Domain.Models;
using HackerNews.Domain.Services;
using Microsoft.Extensions.Logging.Abstractions;
using Moq;

namespace HackerNews.Api.UnitTests;

public class StoryServiceTests
{
    private readonly Mock<IHackerNewsClient> client = new();

    private StoryService CreateSut() => new(client.Object, NullLogger<StoryService>.Instance);

    [Fact]
    public async Task CorrectlyMapsFields()
    {
        client.Setup(c => c.GetBestStoryIdsAsync(It.IsAny<CancellationToken>())).ReturnsAsync([99999]);
        client.Setup(c => c.GetStoryItemAsync(99999, It.IsAny<CancellationToken>())).ReturnsAsync(new StoryItem
        {
            Title = "Some Title",
            Url = "https://some-url.com",
            By = "Some Author",
            Time = 1631460000, //"2021-09-12T15:20:00+00:00"
            Score = 999,
            CommentCount = 800
        });

        var result = await CreateSut().GetBestStoriesAsync(1, CancellationToken.None);

        var story = result.Should().ContainSingle().Subject;
        story.Title.Should().Be("Some Title");
        story.Uri.Should().Be("https://some-url.com");
        story.PostedBy.Should().Be("Some Author");
        story.Time.Should().Be(DateTimeOffset.Parse("2021-09-12T15:20:00+00:00"));
        story.Score.Should().Be(999);
        story.CommentCount.Should().Be(800);
    }

    [Fact]
    public async Task ReturnsStoriesOrderedByScoreDescending()
    {
        client.Setup(c => c.GetBestStoryIdsAsync(It.IsAny<CancellationToken>())).ReturnsAsync([1, 2, 3]);
        client.Setup(c => c.GetStoryItemAsync(1, It.IsAny<CancellationToken>())).ReturnsAsync(CreateStoryItem(1, 50));
        client.Setup(c => c.GetStoryItemAsync(2, It.IsAny<CancellationToken>())).ReturnsAsync(CreateStoryItem(2, 200));
        client.Setup(c => c.GetStoryItemAsync(3, It.IsAny<CancellationToken>())).ReturnsAsync(CreateStoryItem(3, 100));

        var result = await CreateSut().GetBestStoriesAsync(3, CancellationToken.None);

        result.Select(s => s.Score).Should().ContainInOrder(200, 100, 50);
    }

    [Theory]
    [InlineData(0)]
    [InlineData(-1)]
    public async Task ReturnsEmptyForNonPositiveCount(int count)
    {
        var result = await CreateSut().GetBestStoriesAsync(count, CancellationToken.None);

        result.Should().BeEmpty();
        client.Verify(c => c.GetBestStoryIdsAsync(It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact]
    public async Task FiltersOutDeadAndDeletedItems()
    {
        client.Setup(c => c.GetBestStoryIdsAsync(It.IsAny<CancellationToken>())).ReturnsAsync([1, 2, 3]);
        client.Setup(c => c.GetStoryItemAsync(1, It.IsAny<CancellationToken>())).ReturnsAsync(CreateStoryItem(1, 500, dead: true));
        client.Setup(c => c.GetStoryItemAsync(2, It.IsAny<CancellationToken>())).ReturnsAsync(CreateStoryItem(2, 400, deleted: true));
        client.Setup(c => c.GetStoryItemAsync(3, It.IsAny<CancellationToken>())).ReturnsAsync(CreateStoryItem(3, 10));

        var result = await CreateSut().GetBestStoriesAsync(10, CancellationToken.None);

        result.Should().ContainSingle().Which.Score.Should().Be(10);
    }

    [Fact]
    public async Task ContinuesWhenSomeItemFetchFails()
    {
        client.Setup(c => c.GetBestStoryIdsAsync(It.IsAny<CancellationToken>())).ReturnsAsync([1, 2]);
        client.Setup(c => c.GetStoryItemAsync(1, It.IsAny<CancellationToken>())).ThrowsAsync(new HttpRequestException("some error..."));
        client.Setup(c => c.GetStoryItemAsync(2, It.IsAny<CancellationToken>())).ReturnsAsync(CreateStoryItem(2, 999));

        var result = await CreateSut().GetBestStoriesAsync(10, CancellationToken.None);

        result.Should().ContainSingle().Which.Score.Should().Be(999);
    }

    private StoryItem CreateStoryItem(int id, int score, bool dead = false, bool deleted = false) => new()
    {
        Id = id,
        Type = "story",
        Title = $"Story {id}",
        Url = $"https://example.com/{id}",
        By = "someone",
        Time = 1_600_000_000,
        Score = score,
        CommentCount = score / 2,
        Dead = dead,
        Deleted = deleted,
    };
}

using System.Text.Json.Serialization;

namespace HackerNews.Domain.Models;

public sealed class StoryItem
{
    public int Id { get; set; }
    public string? Type { get; set; }
    public string? Title { get; set; }
    public string? Url { get; set; }
    public string? By { get; set; }
    public long Time { get; set; } //Unix timestamp in seconds
    public int Score { get; set; }
    public bool Dead { get; set; }
    public bool Deleted { get; set; }

    [JsonPropertyName("descendants")]
    public int CommentCount { get; set; }
}

namespace MyApi.Dtos;

public class SongReactionSummaryDto
{
    public int UserId { get; set; }
    public string Username { get; set; } = null!;
    public string? ProfileImage { get; set; }
    public bool IsLike { get; set; }
    public DateTime CreatedAt { get; set; }
}
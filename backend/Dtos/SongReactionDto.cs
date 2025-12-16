namespace MyApi.Dtos;

public class SongReactionDto
{
    public int PlaylistId { get; set; }
    public int SongId { get; set; }
    public int UserId { get; set; }
    public bool IsLike { get; set; }
}

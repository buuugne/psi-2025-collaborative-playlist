namespace MyApi.Dtos;

public class RemoveSongReactionDto
{
    public int PlaylistId { get; set; }
    public int SongId { get; set; }
    public int UserId { get; set; }
}

namespace MyApi.Models;

/// <summary>
/// Tracks likes/dislikes from users for songs in playlists
/// </summary>
public class PlaylistSongReaction
{
    public int Id { get; set; }
    
    public int PlaylistId { get; set; }
    
    public int SongId { get; set; }
    
    public int UserId { get; set; }
    
    /// <summary>
    /// True = Like, False = Dislike
    /// </summary>
    public bool IsLike { get; set; }
    
    public DateTime CreatedAt { get; set; }
    
    // Navigation properties
    public virtual Playlist Playlist { get; set; } = null!;
    public virtual Song Song { get; set; } = null!;
    public virtual User User { get; set; } = null!;
}
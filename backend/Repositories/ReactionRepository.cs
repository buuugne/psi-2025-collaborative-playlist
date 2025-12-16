using Microsoft.EntityFrameworkCore;
using MyApi.Data;
using MyApi.Models;

namespace MyApi.Repositories;

public interface IReactionRepository
{
    Task<PlaylistSongReaction?> GetReactionAsync(int playlistId, int songId, int userId);
    Task<List<PlaylistSongReaction>> GetReactionsForPlaylistSongAsync(int playlistId, int songId);
    Task AddReactionAsync(PlaylistSongReaction reaction);
    Task UpdateReactionAsync(PlaylistSongReaction reaction);
    Task DeleteReactionAsync(PlaylistSongReaction reaction);
}

public class ReactionRepository : IReactionRepository
{
    private readonly PlaylistAppContext _context;

    public ReactionRepository(PlaylistAppContext context)
    {
        _context = context;
    }

    public async Task<PlaylistSongReaction?> GetReactionAsync(int playlistId, int songId, int userId)
    {
        return await _context.PlaylistSongReactions
            .FirstOrDefaultAsync(r => 
                r.PlaylistId == playlistId && 
                r.SongId == songId && 
                r.UserId == userId);
    }

    public async Task<List<PlaylistSongReaction>> GetReactionsForPlaylistSongAsync(int playlistId, int songId)
    {
        return await _context.PlaylistSongReactions
            .Where(r => r.PlaylistId == playlistId && r.SongId == songId)
            .ToListAsync();
    }

    public async Task AddReactionAsync(PlaylistSongReaction reaction)
    {
        _context.PlaylistSongReactions.Add(reaction);
        await _context.SaveChangesAsync();
    }

    public async Task UpdateReactionAsync(PlaylistSongReaction reaction)
    {
        _context.PlaylistSongReactions.Update(reaction);
        await _context.SaveChangesAsync();
    }

    public async Task DeleteReactionAsync(PlaylistSongReaction reaction)
    {
        _context.PlaylistSongReactions.Remove(reaction);
        await _context.SaveChangesAsync();
    }
}
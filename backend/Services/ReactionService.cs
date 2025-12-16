using MyApi.Dtos;
using MyApi.Models;
using MyApi.Repositories;

namespace MyApi.Services;

public class ReactionService : IReactionService
{
    private readonly IReactionRepository _reactionRepo;
    private readonly IUserRepository _userRepo;

    public ReactionService(IReactionRepository reactionRepo, IUserRepository userRepo)
    {
        _reactionRepo = reactionRepo;
        _userRepo = userRepo;
    }

    public async Task<(bool Success, string? Error)> ToggleReactionAsync(
        int playlistId, 
        int songId, 
        int userId, 
        bool isLike)
    {
        try
        {
            var existing = await _reactionRepo.GetReactionAsync(playlistId, songId, userId);

            if (existing != null)
            {
                // If same reaction, remove it (toggle off)
                if (existing.IsLike == isLike)
                {
                    await _reactionRepo.DeleteReactionAsync(existing);
                    return (true, null);
                }
                
                // Different reaction, update it
                existing.IsLike = isLike;
                await _reactionRepo.UpdateReactionAsync(existing);
                return (true, null);
            }

            // Create new reaction
            var reaction = new PlaylistSongReaction
            {
                PlaylistId = playlistId,
                SongId = songId,
                UserId = userId,
                IsLike = isLike,
                CreatedAt = DateTime.UtcNow
            };

            await _reactionRepo.AddReactionAsync(reaction);
            return (true, null);
        }
        catch (Exception ex)
        {
            return (false, $"Failed to toggle reaction: {ex.Message}");
        }
    }

    public async Task<List<SongReactionSummaryDto>> GetReactionsForSongAsync(int playlistId, int songId)
    {
        var reactions = await _reactionRepo.GetReactionsForPlaylistSongAsync(playlistId, songId);
        
        var userIds = reactions.Select(r => r.UserId).Distinct().ToList();
        var users = await _userRepo.GetByIdsAsync(userIds);
        var userDict = users.ToDictionary(u => u.Id);

        return reactions
            .Where(r => userDict.ContainsKey(r.UserId))
            .Select(r => new SongReactionSummaryDto
            {
                UserId = r.UserId,
                Username = userDict[r.UserId].Username,
                ProfileImage = userDict[r.UserId].ProfileImage,
                IsLike = r.IsLike,
                CreatedAt = r.CreatedAt
            })
            .ToList();
    }
}
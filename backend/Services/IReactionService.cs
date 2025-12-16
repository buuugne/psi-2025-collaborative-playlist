using MyApi.Dtos;

namespace MyApi.Services;

public interface IReactionService
{
    Task<(bool Success, string? Error)> ToggleReactionAsync(int playlistId, int songId, int userId, bool isLike);
    Task<List<SongReactionSummaryDto>> GetReactionsForSongAsync(int playlistId, int songId);
}
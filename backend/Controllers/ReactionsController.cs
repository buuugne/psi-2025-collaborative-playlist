using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using MyApi.Dtos;
using MyApi.Services;
using System.Security.Claims;

namespace MyApi.Controllers;

[ApiController]
[Route("api/playlists/{playlistId}/songs/{songId}/reactions")]
public class ReactionsController : ControllerBase
{
    private readonly IReactionService _reactionService;

    public ReactionsController(IReactionService reactionService)
    {
        _reactionService = reactionService;
    }

    [Authorize]
    [HttpPost]
    public async Task<IActionResult> ToggleReaction(
        int playlistId,
        int songId,
        [FromBody] SongReactionDto dto)
    {
        var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        if (string.IsNullOrEmpty(userIdClaim) || !int.TryParse(userIdClaim, out int userId))
            return Unauthorized("Invalid token");

        var (success, error) = await _reactionService.ToggleReactionAsync(
            playlistId, 
            songId, 
            userId, 
            dto.IsLike);

        if (!success)
            return BadRequest(new { message = error });

        return Ok(new { message = "Reaction toggled successfully" });
    }

    [HttpGet]
    public async Task<IActionResult> GetReactions(int playlistId, int songId)
    {
        var reactions = await _reactionService.GetReactionsForSongAsync(playlistId, songId);
        return Ok(reactions);
    }
}
using Microsoft.AspNetCore.Mvc;
using MyApi.Services;

namespace MyApi.Controllers
{
    [ApiController]
    [Route("api/spotify")] 
    public class SpotifyController : ControllerBase
    {
        private readonly ISpotifyService _spotifyService;

        // dependency injection
        // sukria reikiama objekta
        // sureguliuoja priklausomybes  
        public SpotifyController(ISpotifyService spotifyService)
        {
            _spotifyService = spotifyService;
        }

        // GET /api/spotify/search/{query}
        [HttpGet("search/{query}")]
        public async Task<IActionResult> Search(string query)
        {
            var (success, error, jsonResult) = await _spotifyService.SearchTracks(query);

            if (!success)
                return StatusCode(500, error ?? "Spotify search failed");

            return Content(jsonResult!, "application/json");
        }
    }
}
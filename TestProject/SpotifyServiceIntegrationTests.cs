using System.Net.Http;
using System.Threading.Tasks;
using Microsoft.Extensions.Configuration;
using MyApi.Services;
using Xunit;

namespace TestProject.Integration
{
    /// <summary>
    /// Integration tests for SpotifyService that make real HTTP calls.
    /// These tests require valid Spotify credentials in appsettings.json or environment variables.
    /// 
    /// To run these tests:
    /// 1. Set your Spotify Client ID and Secret in test configuration
    /// 2. Run with: dotnet test --filter Category=Integration
    /// 
    /// To skip integration tests:
    /// dotnet test --filter Category!=Integration
    /// </summary>
    [Trait("Category", "Integration")]
    public class SpotifyServiceIntegrationTests
    {
        private readonly IConfiguration _configuration;
        private readonly HttpClient _httpClient;

        public SpotifyServiceIntegrationTests()
        {
            // Build configuration from appsettings or environment variables
            _configuration = new ConfigurationBuilder()
                .AddJsonFile("appsettings.json", optional: true)
                .AddJsonFile("appsettings.Test.json", optional: true)
                .AddEnvironmentVariables()
                .Build();

            _httpClient = new HttpClient();
        }

        [Fact]
        public async Task SearchTracks_WithRealCredentials_ReturnsResults()
        {
            // Arrange
            var service = new SpotifyService(_httpClient, _configuration);
            var query = "Metallica";

            // Act
            var (success, error, json) = await service.SearchTracks(query);

            // Assert
            Assert.True(success, $"Search failed: {error}");
            Assert.Null(error);
            Assert.NotNull(json);
            Assert.Contains("tracks", json);
            Assert.Contains("items", json);
        }

        [Fact]
        public async Task SearchTracks_WithSpecialCharacters_HandlesCorrectly()
        {
            // Arrange
            var service = new SpotifyService(_httpClient, _configuration);
            var query = "AC/DC Back in Black";

            // Act
            var (success, error, json) = await service.SearchTracks(query);

            // Assert
            Assert.True(success, $"Search failed: {error}");
            Assert.Null(error);
            Assert.NotNull(json);
        }

        [Fact]
        public async Task SearchTracks_WithEmptyQuery_ReturnsResults()
        {
            // Arrange
            var service = new SpotifyService(_httpClient, _configuration);
            var query = "a"; // Minimal query

            // Act
            var (success, error, json) = await service.SearchTracks(query);

            // Assert
            Assert.True(success, $"Search failed: {error}");
        }

        [Fact]
        public async Task GetTrackDetails_WithValidSpotifyId_ReturnsTrackInfo()
        {
            // Arrange
            var service = new SpotifyService(_httpClient, _configuration);
            // Using a well-known track ID (Bohemian Rhapsody by Queen)
            var spotifyId = "6l8GvAyoUZwWDgF1e4822w";

            // Act
            var (success, error, details) = await service.GetTrackDetails(spotifyId);

            // Assert
            Assert.True(success, $"GetTrackDetails failed: {error}");
            Assert.Null(error);
            Assert.NotNull(details);
            Assert.Equal(spotifyId, details.SpotifyId);
            Assert.NotNull(details.Title);
            Assert.NotNull(details.SpotifyUri);
            Assert.NotEmpty(details.Artists);
            Assert.NotNull(details.AlbumInfo);
            Assert.NotNull(details.AlbumInfo.Name);
        }

        [Fact]
        public async Task GetTrackDetails_WithInvalidSpotifyId_ReturnsError()
        {
            // Arrange
            var service = new SpotifyService(_httpClient, _configuration);
            var invalidId = "invalid_track_id_12345";

            // Act
            var (success, error, details) = await service.GetTrackDetails(invalidId);

            // Assert
            Assert.False(success);
            Assert.NotNull(error);
            Assert.Null(details);
            Assert.Contains("400", error); // Spotify returns 400 for invalid IDs
        }

        [Fact]
        public async Task GetTrackDetails_MultipleSequentialCalls_WorksCorrectly()
        {
            // Arrange
            var service = new SpotifyService(_httpClient, _configuration);
            var trackIds = new[]
            {
                "3n3Ppam7vgaVa1iaRUc9Lp", // Mr. Brightside - The Killers
                "0VjIjW4GlUZAMYd2vXMi3b", // Blinding Lights - The Weeknd
                "2Fxmhks0bxGSBdJ92vM42m"  // bad guy - Billie Eilish
            };

            // Act & Assert
            foreach (var trackId in trackIds)
            {
                var (success, error, details) = await service.GetTrackDetails(trackId);
                
                Assert.True(success, $"Failed for track {trackId}: {error}");
                Assert.NotNull(details);
                Assert.Equal(trackId, details.SpotifyId);
            }
        }

        [Fact]
        public void GenerateLoginUrl_ReturnsValidUrl()
        {
            // Arrange
            var service = new SpotifyService(_httpClient, _configuration);

            // Act
            var url = service.GenerateLoginUrl();

            // Assert
            Assert.NotNull(url);
            Assert.StartsWith("https://accounts.spotify.com/authorize", url);
            Assert.Contains("response_type=code", url);
            Assert.Contains("client_id=", url);
            Assert.Contains("redirect_uri=", url);
            Assert.Contains("scope=", url);
        }

        [Fact]
        public async Task ExchangeCodeForToken_WithInvalidCode_ReturnsError()
        {
            // Arrange
            var service = new SpotifyService(_httpClient, _configuration);
            var invalidCode = "invalid_authorization_code";

            // Act
            var result = await service.ExchangeCodeForToken(invalidCode);

            // Assert
            Assert.False(result.Success);
            Assert.NotNull(result.Error);
            Assert.Null(result.AccessToken);
        }

        [Fact]
        public async Task RefreshAccessToken_WithInvalidToken_ReturnsError()
        {
            // Arrange
            var service = new SpotifyService(_httpClient, _configuration);
            var invalidRefreshToken = "invalid_refresh_token";

            // Act
            var result = await service.RefreshAccessToken(invalidRefreshToken);

            // Assert
            Assert.False(result.Success);
            Assert.NotNull(result.Error);
        }

        // Stress test - optional
        [Fact(Skip = "Long running test - enable manually")]
        public async Task SearchTracks_MultipleParallelRequests_HandlesCorrectly()
        {
            // Arrange
            var service = new SpotifyService(_httpClient, _configuration);
            var queries = new[] { "Beatles", "Queen", "Pink Floyd", "Metallica", "Nirvana" };

            // Act
            var tasks = queries.Select(q => service.SearchTracks(q));
            var results = await Task.WhenAll(tasks);

            // Assert
            foreach (var (success, error, json) in results)
            {
                Assert.True(success, $"Parallel request failed: {error}");
                Assert.NotNull(json);
            }
        }
    }
}
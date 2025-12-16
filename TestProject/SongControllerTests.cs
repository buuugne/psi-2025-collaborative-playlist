using Microsoft.AspNetCore.Mvc;
using Moq;
using MyApi.Controllers;
using MyApi.Dtos;
using MyApi.Services;
using Xunit;
using System.Text.Json;

namespace MyApi.Tests.Controllers
{
    public class SongControllerTests
    {
        private readonly Mock<ISongService> _mockSongService;
        private readonly SongController _controller;

        public SongControllerTests()
        {
            _mockSongService = new Mock<ISongService>();
            _controller = new SongController(_mockSongService.Object);
        }

        #region AddSongToPlaylist Tests

        [Fact]
        public async Task AddSongToPlaylist_Success_ReturnsOkWithSongId()
        {
            // Arrange
            var request = new AddSongToPlaylistDto
            {
                PlaylistId = 1,
                SpotifyId = "spotify123",
                Title = "Test Song",
                SpotifyUri = "spotify:track:123"
            };
            _mockSongService.Setup(s => s.AddSongToPlaylistAsync(request))
                .ReturnsAsync((true, null, 5));

            // Act
            var result = await _controller.AddSongToPlaylist(request);

            // Assert
            var okResult = Assert.IsType<OkObjectResult>(result);
            var json = JsonSerializer.Serialize(okResult.Value);
            var data = JsonSerializer.Deserialize<JsonElement>(json);
            Assert.Equal("Song added successfully", data.GetProperty("message").GetString());
            Assert.Equal(5, data.GetProperty("songId").GetInt32());
        }

        [Fact]
        public async Task AddSongToPlaylist_NotFound_ReturnsNotFound()
        {
            // Arrange
            var request = new AddSongToPlaylistDto
            {
                PlaylistId = 999,
                SpotifyId = "spotify123",
                Title = "Test Song",
                SpotifyUri = "spotify:track:123"
            };
            _mockSongService.Setup(s => s.AddSongToPlaylistAsync(request))
                .ReturnsAsync((false, "Playlist with ID 999 not found.", null));

            // Act
            var result = await _controller.AddSongToPlaylist(request);

            // Assert
            var notFoundResult = Assert.IsType<NotFoundObjectResult>(result);
            Assert.Equal("Playlist with ID 999 not found.", notFoundResult.Value);
        }

        [Fact]
        public async Task AddSongToPlaylist_AlreadyInPlaylist_ReturnsConflict()
        {
            // Arrange
            var request = new AddSongToPlaylistDto
            {
                PlaylistId = 1,
                SpotifyId = "spotify123",
                Title = "Test Song",
                SpotifyUri = "spotify:track:123"
            };
            _mockSongService.Setup(s => s.AddSongToPlaylistAsync(request))
                .ReturnsAsync((false, "This song is already in the playlist.", null));

            // Act
            var result = await _controller.AddSongToPlaylist(request);

            // Assert
            var conflictResult = Assert.IsType<ConflictObjectResult>(result);
            Assert.Equal("This song is already in the playlist.", conflictResult.Value);
        }

        [Fact]
        public async Task AddSongToPlaylist_OtherError_ReturnsBadRequest()
        {
            // Arrange
            var request = new AddSongToPlaylistDto
            {
                PlaylistId = 1,
                SpotifyId = "",
                Title = "Test Song",
                SpotifyUri = "spotify:track:123"
            };
            _mockSongService.Setup(s => s.AddSongToPlaylistAsync(request))
                .ReturnsAsync((false, "SpotifyId is required to add a track.", null));

            // Act
            var result = await _controller.AddSongToPlaylist(request);

            // Assert
            var badRequestResult = Assert.IsType<BadRequestObjectResult>(result);
            Assert.Equal("SpotifyId is required to add a track.", badRequestResult.Value);
        }

        #endregion

        #region GetAll Tests

        [Fact]
        public async Task GetAll_ReturnsOkWithSongs()
        {
            // Arrange
            var songs = new List<SongDto>
            {
                new SongDto { Id = 1, Title = "Song 1", SpotifyId = "spot1", SpotifyUri = "uri1" },
                new SongDto { Id = 2, Title = "Song 2", SpotifyId = "spot2", SpotifyUri = "uri2" }
            };
            _mockSongService.Setup(s => s.GetAllAsync())
                .ReturnsAsync(songs);

            // Act
            var result = await _controller.GetAll();

            // Assert
            var okResult = Assert.IsType<OkObjectResult>(result);
            var returnedSongs = Assert.IsAssignableFrom<IEnumerable<SongDto>>(okResult.Value);
            Assert.Equal(2, returnedSongs.Count());
        }

        [Fact]
        public async Task GetAll_NoSongs_ReturnsEmptyList()
        {
            // Arrange
            _mockSongService.Setup(s => s.GetAllAsync())
                .ReturnsAsync(new List<SongDto>());

            // Act
            var result = await _controller.GetAll();

            // Assert
            var okResult = Assert.IsType<OkObjectResult>(result);
            var returnedSongs = Assert.IsAssignableFrom<IEnumerable<SongDto>>(okResult.Value);
            Assert.Empty(returnedSongs);
        }

        #endregion

        #region GetById Tests

        [Fact]
        public async Task GetById_ExistingSong_ReturnsOkWithSong()
        {
            // Arrange
            var song = new SongDto 
            { 
                Id = 1, 
                Title = "Test Song", 
                SpotifyId = "spot1", 
                SpotifyUri = "uri1" 
            };
            _mockSongService.Setup(s => s.GetByIdAsync(1))
                .ReturnsAsync(song);

            // Act
            var result = await _controller.GetById(1);

            // Assert
            var okResult = Assert.IsType<OkObjectResult>(result);
            var returnedSong = Assert.IsType<SongDto>(okResult.Value);
            Assert.Equal(1, returnedSong.Id);
            Assert.Equal("Test Song", returnedSong.Title);
        }

        [Fact]
        public async Task GetById_NonExistingSong_ReturnsNotFound()
        {
            // Arrange
            _mockSongService.Setup(s => s.GetByIdAsync(999))
                .ReturnsAsync((SongDto?)null);

            // Act
            var result = await _controller.GetById(999);

            // Assert
            var notFoundResult = Assert.IsType<NotFoundObjectResult>(result);
            Assert.Equal("Song with ID 999 not found.", notFoundResult.Value);
        }

        #endregion

        #region Delete Tests

        [Fact]
        public async Task Delete_ExistingSong_ReturnsOk()
        {
            // Arrange
            _mockSongService.Setup(s => s.DeleteAsync(1))
                .ReturnsAsync((true, null));

            // Act
            var result = await _controller.Delete(1);

            // Assert
            var okResult = Assert.IsType<OkObjectResult>(result);
            var json = JsonSerializer.Serialize(okResult.Value);
            var data = JsonSerializer.Deserialize<JsonElement>(json);
            Assert.Equal("Song deleted successfully", data.GetProperty("message").GetString());
        }

        [Fact]
        public async Task Delete_NonExistingSong_ReturnsNotFound()
        {
            // Arrange
            _mockSongService.Setup(s => s.DeleteAsync(999))
                .ReturnsAsync((false, "Song with ID 999 not found."));

            // Act
            var result = await _controller.Delete(999);

            // Assert
            var notFoundResult = Assert.IsType<NotFoundObjectResult>(result);
            Assert.Equal("Song with ID 999 not found.", notFoundResult.Value);
        }

        [Fact]
        public async Task Delete_OtherError_ReturnsBadRequest()
        {
            // Arrange
            _mockSongService.Setup(s => s.DeleteAsync(1))
                .ReturnsAsync((false, "Some other error occurred"));

            // Act
            var result = await _controller.Delete(1);

            // Assert
            var badRequestResult = Assert.IsType<BadRequestObjectResult>(result);
            Assert.Equal("Some other error occurred", badRequestResult.Value);
        }

        #endregion
    }
}
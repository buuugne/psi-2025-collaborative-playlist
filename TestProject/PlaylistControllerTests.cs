using Xunit;
using Moq;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Http;
using MyApi.Controllers;
using MyApi.Services;
using MyApi.Dtos;
using MyApi.Models;
using System.Security.Claims;

namespace TestProject.Controllers
{
    public class PlaylistsControllerTests
    {
        private readonly Mock<IPlaylistService> _serviceMock;
        private readonly PlaylistsController _controller;

        public PlaylistsControllerTests()
        {
            _serviceMock = new Mock<IPlaylistService>();
            _controller = new PlaylistsController(_serviceMock.Object);
        }

        // Helper method to set up authenticated user context
        private void SetupAuthenticatedUser(int userId)
        {
            var claims = new List<Claim>
            {
                new Claim(ClaimTypes.NameIdentifier, userId.ToString())
            };
            var identity = new ClaimsIdentity(claims, "TestAuth");
            var user = new ClaimsPrincipal(identity);
            _controller.ControllerContext = new ControllerContext
            {
                HttpContext = new DefaultHttpContext { User = user }
            };
        }

        // =============================
        // GetPlaylists Tests
        // =============================

        [Fact]
        public async Task GetPlaylists_AuthenticatedUser_ReturnsOkWithPlaylists()
        {
            // Arrange
            SetupAuthenticatedUser(1);
            var playlists = new List<PlaylistResponseDto>
            {
                new PlaylistResponseDto { Id = 1, Name = "Playlist 1", HostId = 1 },
                new PlaylistResponseDto { Id = 2, Name = "Playlist 2", HostId = 1 }
            };

            _serviceMock
                .Setup(s => s.GetAllAsync(1))
                .ReturnsAsync(playlists);

            // Act
            var result = await _controller.GetPlaylists();

            // Assert
            var okResult = Assert.IsType<OkObjectResult>(result);
            var returnedPlaylists = Assert.IsAssignableFrom<IEnumerable<PlaylistResponseDto>>(okResult.Value);
            Assert.Equal(2, returnedPlaylists.Count());
            _serviceMock.Verify(s => s.GetAllAsync(1), Times.Once);
        }

        [Fact]
        public async Task GetPlaylists_NoUserClaim_ReturnsUnauthorized()
        {
            // Arrange
            _controller.ControllerContext = new ControllerContext
            {
                HttpContext = new DefaultHttpContext()
            };

            // Act
            var result = await _controller.GetPlaylists();

            // Assert
            var unauthorizedResult = Assert.IsType<UnauthorizedObjectResult>(result);
            Assert.Equal("Invalid token", unauthorizedResult.Value);
            _serviceMock.Verify(s => s.GetAllAsync(It.IsAny<int>()), Times.Never);
        }

        [Fact]
        public async Task GetPlaylists_InvalidUserIdClaim_ReturnsUnauthorized()
        {
            // Arrange
            var claims = new List<Claim>
            {
                new Claim(ClaimTypes.NameIdentifier, "not-a-number")
            };
            var identity = new ClaimsIdentity(claims, "TestAuth");
            var user = new ClaimsPrincipal(identity);
            _controller.ControllerContext = new ControllerContext
            {
                HttpContext = new DefaultHttpContext { User = user }
            };

            // Act
            var result = await _controller.GetPlaylists();

            // Assert
            Assert.IsType<UnauthorizedObjectResult>(result);
        }

        // =============================
        // GetPlaylistById Tests
        // =============================

        [Fact]
        public async Task GetPlaylistById_ExistingId_ReturnsOkWithPlaylist()
        {
            // Arrange
            var playlist = new PlaylistResponseDto
            {
                Id = 1,
                Name = "Test Playlist",
                Description = "Test Description",
                HostId = 1
            };

            _serviceMock
                .Setup(s => s.GetByIdAsync(1))
                .ReturnsAsync(playlist);

            // Act
            var result = await _controller.GetPlaylistById(1);

            // Assert
            var okResult = Assert.IsType<OkObjectResult>(result);
            var returnedPlaylist = Assert.IsType<PlaylistResponseDto>(okResult.Value);
            Assert.Equal("Test Playlist", returnedPlaylist.Name);
            _serviceMock.Verify(s => s.GetByIdAsync(1), Times.Once);
        }

        [Fact]
        public async Task GetPlaylistById_NonExistentId_ReturnsNotFound()
        {
            // Arrange
            _serviceMock
                .Setup(s => s.GetByIdAsync(999))
                .ReturnsAsync((PlaylistResponseDto?)null);

            // Act
            var result = await _controller.GetPlaylistById(999);

            // Assert
            var notFoundResult = Assert.IsType<NotFoundObjectResult>(result);
            Assert.Equal("Playlist with ID 999 not found.", notFoundResult.Value);
        }

        // =============================
        // Create Tests
        // =============================

        [Fact]
        public async Task Create_ValidPlaylist_ReturnsCreatedAtAction()
        {
            // Arrange
            var formDto = new PlaylistCreateFormDto
            {
                Name = "New Playlist",
                Description = "Description",
                HostId = 1,
                CoverImage = null
            };

            var createdPlaylist = new PlaylistResponseDto
            {
                Id = 1,
                Name = "New Playlist",
                Description = "Description",
                HostId = 1
            };

            _serviceMock
                .Setup(s => s.CreateAsync(It.IsAny<PlaylistCreateDto>()))
                .ReturnsAsync((true, null, createdPlaylist));

            // Act
            var result = await _controller.Create(formDto);

            // Assert
            var createdResult = Assert.IsType<CreatedAtActionResult>(result);
            Assert.Equal(nameof(_controller.GetPlaylistById), createdResult.ActionName);
            var returnedPlaylist = Assert.IsType<PlaylistResponseDto>(createdResult.Value);
            Assert.Equal("New Playlist", returnedPlaylist.Name);
        }

        [Fact]
        public async Task Create_ServiceFails_ReturnsBadRequest()
        {
            // Arrange
            var formDto = new PlaylistCreateFormDto
            {
                Name = "Duplicate",
                Description = "Description",
                HostId = 1
            };

            _serviceMock
                .Setup(s => s.CreateAsync(It.IsAny<PlaylistCreateDto>()))
                .ReturnsAsync((false, "Playlist name already exists", null));

            // Act
            var result = await _controller.Create(formDto);

            // Assert
            var badRequestResult = Assert.IsType<BadRequestObjectResult>(result);
            var response = badRequestResult.Value;
            var messageProp = response?.GetType().GetProperty("message");
            Assert.Equal("Playlist name already exists", messageProp?.GetValue(response)?.ToString());
        }

        // =============================
        // UpdatePlaylistById Tests
        // =============================

        [Fact]
        public async Task UpdatePlaylistById_ValidUpdate_ReturnsOkWithUpdatedPlaylist()
        {
            // Arrange
            var updateDto = new PlaylistUpdateDto
            {
                Name = "Updated Name",
                Description = "Updated Description"
            };

            var updatedPlaylist = new PlaylistResponseDto
            {
                Id = 1,
                Name = "Updated Name",
                Description = "Updated Description",
                HostId = 1
            };

            _serviceMock
                .Setup(s => s.UpdateByIdAsync(1, updateDto))
                .ReturnsAsync((true, null, updatedPlaylist));

            // Act
            var result = await _controller.UpdatePlaylistById(1, updateDto);

            // Assert
            var okResult = Assert.IsType<OkObjectResult>(result);
            var returnedPlaylist = Assert.IsType<PlaylistResponseDto>(okResult.Value);
            Assert.Equal("Updated Name", returnedPlaylist.Name);
        }

        [Fact]
        public async Task UpdatePlaylistById_PlaylistNotFound_ReturnsNotFound()
        {
            // Arrange
            var updateDto = new PlaylistUpdateDto
            {
                Name = "Updated Name",
                Description = "Updated Description"
            };

            _serviceMock
                .Setup(s => s.UpdateByIdAsync(999, updateDto))
                .ReturnsAsync((false, "Playlist with ID 999 not found", null));

            // Act
            var result = await _controller.UpdatePlaylistById(999, updateDto);

            // Assert
            var notFoundResult = Assert.IsType<NotFoundObjectResult>(result);
            Assert.Contains("not found", notFoundResult.Value?.ToString());
        }

        [Fact]
        public async Task UpdatePlaylistById_NotHost_ReturnsForbidden()
        {
            // Arrange
            var updateDto = new PlaylistUpdateDto
            {
                Name = "Updated Name",
                Description = "Updated Description"
            };

            _serviceMock
                .Setup(s => s.UpdateByIdAsync(1, updateDto))
                .ReturnsAsync((false, "Only hosts can update playlists", null));

            // Act
            var result = await _controller.UpdatePlaylistById(1, updateDto);

            // Assert
            var statusCodeResult = Assert.IsType<ObjectResult>(result);
            Assert.Equal(403, statusCodeResult.StatusCode);
        }

        // =============================
        // EditPlaylist Tests
        // =============================

        [Fact]
        public async Task EditPlaylist_ValidPatch_ReturnsOkWithUpdatedPlaylist()
        {
            // Arrange
            var patchDto = new PlaylistPatchDto
            {
                Name = "Patched Name"
            };

            var updatedPlaylist = new PlaylistResponseDto
            {
                Id = 1,
                Name = "Patched Name",
                HostId = 1
            };

            _serviceMock
                .Setup(s => s.EditAsync(1, patchDto))
                .ReturnsAsync((true, null, updatedPlaylist));

            // Act
            var result = await _controller.EditPlaylist(1, patchDto);

            // Assert
            var okResult = Assert.IsType<OkObjectResult>(result);
            var returnedPlaylist = Assert.IsType<PlaylistResponseDto>(okResult.Value);
            Assert.Equal("Patched Name", returnedPlaylist.Name);
        }

        [Fact]
        public async Task EditPlaylist_PlaylistNotFound_ReturnsNotFound()
        {
            // Arrange
            var patchDto = new PlaylistPatchDto
            {
                Description = "New Description"
            };

            _serviceMock
                .Setup(s => s.EditAsync(999, patchDto))
                .ReturnsAsync((false, "Playlist with ID 999 not found", null));

            // Act
            var result = await _controller.EditPlaylist(999, patchDto);

            // Assert
            Assert.IsType<NotFoundObjectResult>(result);
        }

        // =============================
        // DeletePlaylistById Tests
        // =============================

        [Fact]
        public async Task DeletePlaylistById_ValidId_ReturnsNoContent()
        {
            // Arrange
            _serviceMock
                .Setup(s => s.DeleteAsync(1))
                .ReturnsAsync((true, null));

            // Act
            var result = await _controller.DeletePlaylistById(1);

            // Assert
            Assert.IsType<NoContentResult>(result);
            _serviceMock.Verify(s => s.DeleteAsync(1), Times.Once);
        }

        [Fact]
        public async Task DeletePlaylistById_PlaylistNotFound_ReturnsNotFound()
        {
            // Arrange
            _serviceMock
                .Setup(s => s.DeleteAsync(999))
                .ReturnsAsync((false, "Playlist with ID 999 not found"));

            // Act
            var result = await _controller.DeletePlaylistById(999);

            // Assert
            Assert.IsType<NotFoundObjectResult>(result);
        }

        [Fact]
        public async Task DeletePlaylistById_NotHost_ReturnsForbidden()
        {
            // Arrange
            _serviceMock
                .Setup(s => s.DeleteAsync(1))
                .ReturnsAsync((false, "Only hosts can delete playlists"));

            // Act
            var result = await _controller.DeletePlaylistById(1);

            // Assert
            var statusCodeResult = Assert.IsType<ObjectResult>(result);
            Assert.Equal(403, statusCodeResult.StatusCode);
        }

        // =============================
        // RemoveSongFromPlaylist Tests
        // =============================

        [Fact]
        public async Task RemoveSongFromPlaylist_ValidIds_ReturnsOkWithMessage()
        {
            // Arrange
            _serviceMock
                .Setup(s => s.RemoveSongFromPlaylistAsync(1, 1))
                .ReturnsAsync((true, null));

            // Act
            var result = await _controller.RemoveSongFromPlaylist(1, 1);

            // Assert
            var okResult = Assert.IsType<OkObjectResult>(result);
            var response = okResult.Value;
            var messageProp = response?.GetType().GetProperty("message");
            Assert.Equal("Song removed from playlist successfully", messageProp?.GetValue(response)?.ToString());
            _serviceMock.Verify(s => s.RemoveSongFromPlaylistAsync(1, 1), Times.Once);
        }

        [Fact]
        public async Task RemoveSongFromPlaylist_PlaylistNotFound_ReturnsNotFound()
        {
            // Arrange
            _serviceMock
                .Setup(s => s.RemoveSongFromPlaylistAsync(999, 1))
                .ReturnsAsync((false, "Playlist with ID 999 not found"));

            // Act
            var result = await _controller.RemoveSongFromPlaylist(999, 1);

            // Assert
            Assert.IsType<NotFoundObjectResult>(result);
        }

        [Fact]
        public async Task RemoveSongFromPlaylist_SongNotFound_ReturnsNotFound()
        {
            // Arrange
            _serviceMock
                .Setup(s => s.RemoveSongFromPlaylistAsync(1, 999))
                .ReturnsAsync((false, "Song with ID 999 not found in playlist"));

            // Act
            var result = await _controller.RemoveSongFromPlaylist(1, 999);

            // Assert
            Assert.IsType<NotFoundObjectResult>(result);
        }

        [Fact]
        public async Task RemoveSongFromPlaylist_ServiceError_ReturnsBadRequest()
        {
            // Arrange
            _serviceMock
                .Setup(s => s.RemoveSongFromPlaylistAsync(1, 1))
                .ReturnsAsync((false, "Database error occurred"));

            // Act
            var result = await _controller.RemoveSongFromPlaylist(1, 1);

            // Assert
            Assert.IsType<BadRequestObjectResult>(result);
        }
    }
}
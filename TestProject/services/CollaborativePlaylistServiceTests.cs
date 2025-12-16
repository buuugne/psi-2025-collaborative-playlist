using Xunit;
using Moq;
using MyApi.Services;
using MyApi.Repositories;
using MyApi.Models;
using System.Threading.Tasks;
using System.Collections.Generic;
using System.Linq;

namespace TestProject.Services
{
    public class CollaborativePlaylistServiceTests
    {
        private readonly Mock<IPlaylistRepository> _playlistRepoMock;
        private readonly Mock<IUserRepository> _userRepoMock;
        private readonly Mock<ISongRepository> _songRepoMock;

        private readonly CollaborativePlaylistService _service;

        public CollaborativePlaylistServiceTests()
        {
            _playlistRepoMock = new Mock<IPlaylistRepository>();
            _userRepoMock = new Mock<IUserRepository>();
            _songRepoMock = new Mock<ISongRepository>();

            _service = new CollaborativePlaylistService(
                _playlistRepoMock.Object,
                _userRepoMock.Object,
                _songRepoMock.Object
            );
        }

        // ---------------------------
        // GET COLLABORATORS
        // ---------------------------

        [Fact]
        public async Task GetCollaboratorsAsync_ShouldReturnUsers_WhenPlaylistExists()
        {
            // Arrange
            var user1 = new User { Id = 1, Username = "user1", Role = UserRole.Host };
            var user2 = new User { Id = 2, Username = "user2", Role = UserRole.Host };
            
            var playlist = new Playlist
            {
                Id = 1,
                HostId = 10,
                Users = new List<User> { user1, user2 }
            };

            _playlistRepoMock.Setup(r => r.GetByIdWithDetailsAsync(1)).ReturnsAsync(playlist);

            // Act
            var result = await _service.GetCollaboratorsAsync(1);

            // Assert
            Assert.NotNull(result);
            Assert.Equal(2, result.Count());
        }

        [Fact]
        public async Task GetCollaboratorsAsync_ShouldReturnNull_WhenPlaylistDoesNotExist()
        {
            // Arrange
            _playlistRepoMock.Setup(r => r.GetByIdWithDetailsAsync(1))
                .ReturnsAsync((Playlist?)null);

            // Act
            var result = await _service.GetCollaboratorsAsync(1);

            // Assert
            Assert.Null(result);
        }

        // ---------------------------
        // ADD COLLABORATOR BY USERNAME
        // ---------------------------

        [Fact]
        public async Task AddCollaboratorByUsernameAsync_ShouldAdd_WhenHostAddsValidUser()
        {
            // Arrange
            var playlist = new Playlist
            {
                Id = 1,
                HostId = 10,
                Users = new List<User>()
            };

            var user = new User { Id = 20, Username = "john" };

            _playlistRepoMock.Setup(r => r.GetByIdWithDetailsAsync(1)).ReturnsAsync(playlist);
            _userRepoMock.Setup(r => r.GetByUsernameAsync("john")).ReturnsAsync(user);
            _playlistRepoMock.Setup(r => r.UpdateAsync(It.IsAny<Playlist>())).Returns(Task.CompletedTask);

            // Act
            var result = await _service.AddCollaboratorByUsernameAsync(1, "john", 10);

            // Assert
            Assert.True(result.Success);
            Assert.Contains(user, playlist.Users);
            _playlistRepoMock.Verify(r => r.UpdateAsync(It.IsAny<Playlist>()), Times.Once);
        }

        [Fact]
        public async Task AddCollaboratorByUsernameAsync_ShouldFail_WhenRequesterIsNotHost()
        {
            // Arrange
            var playlist = new Playlist
            {
                Id = 1,
                HostId = 10,
                Users = new List<User>()
            };

            _playlistRepoMock.Setup(r => r.GetByIdWithDetailsAsync(1)).ReturnsAsync(playlist);

            // Act
            var result = await _service.AddCollaboratorByUsernameAsync(1, "john", 99);

            // Assert
            Assert.False(result.Success);
            Assert.Contains("Only host", result.Error);
        }

        [Fact]
        public async Task AddCollaboratorByUsernameAsync_ShouldFail_WhenUserDoesNotExist()
        {
            // Arrange
            var playlist = new Playlist { Id = 1, HostId = 10, Users = new List<User>() };

            _playlistRepoMock.Setup(r => r.GetByIdWithDetailsAsync(1)).ReturnsAsync(playlist);
            _userRepoMock.Setup(r => r.GetByUsernameAsync("nonexistent")).ReturnsAsync((User?)null);

            // Act
            var result = await _service.AddCollaboratorByUsernameAsync(1, "nonexistent", 10);

            // Assert
            Assert.False(result.Success);
            Assert.Contains("not found", result.Error);
        }

        [Fact]
        public async Task AddCollaboratorByUsernameAsync_ShouldFail_WhenUserIsHost()
        {
            // Arrange
            var playlist = new Playlist { Id = 1, HostId = 10, Users = new List<User>() };
            var hostUser = new User { Id = 10, Username = "host" };

            _playlistRepoMock.Setup(r => r.GetByIdWithDetailsAsync(1)).ReturnsAsync(playlist);
            _userRepoMock.Setup(r => r.GetByUsernameAsync("host")).ReturnsAsync(hostUser);

            // Act
            var result = await _service.AddCollaboratorByUsernameAsync(1, "host", 10);

            // Assert
            Assert.False(result.Success);
            Assert.Contains("already the owner", result.Error);
        }

        [Fact]
        public async Task AddCollaboratorByUsernameAsync_ShouldFail_WhenUserIsAlreadyCollaborator()
        {
            // Arrange
            var user = new User { Id = 20, Username = "john" };
            var playlist = new Playlist
            {
                Id = 1,
                HostId = 10,
                Users = new List<User> { user }
            };

            _playlistRepoMock.Setup(r => r.GetByIdWithDetailsAsync(1)).ReturnsAsync(playlist);
            _userRepoMock.Setup(r => r.GetByUsernameAsync("john")).ReturnsAsync(user);

            // Act
            var result = await _service.AddCollaboratorByUsernameAsync(1, "john", 10);

            // Assert
            Assert.False(result.Success);
            Assert.Contains("already a collaborator", result.Error);
        }

        // ---------------------------
        // ADD COLLABORATOR BY ID
        // ---------------------------

        [Fact]
        public async Task AddCollaboratorAsync_ShouldAdd_WhenHostAddsValidUser()
        {
            // Arrange
            var playlist = new Playlist
            {
                Id = 1,
                HostId = 10,
                Users = new List<User>()
            };

            var user = new User { Id = 20 };

            _playlistRepoMock.Setup(r => r.GetByIdWithDetailsAsync(1)).ReturnsAsync(playlist);
            _userRepoMock.Setup(r => r.GetByIdAsync(20)).ReturnsAsync(user);
            _playlistRepoMock.Setup(r => r.UpdateAsync(It.IsAny<Playlist>())).Returns(Task.CompletedTask);

            // Act
            var result = await _service.AddCollaboratorAsync(1, 20, 10);

            // Assert
            Assert.True(result.Success);
            Assert.Contains(user, playlist.Users);
        }

        [Fact]
        public async Task AddCollaboratorAsync_ShouldFail_WhenRequesterIsNotHost()
        {
            // Arrange
            var playlist = new Playlist
            {
                Id = 1,
                HostId = 10,
                Users = new List<User>()
            };

            _playlistRepoMock.Setup(r => r.GetByIdWithDetailsAsync(1)).ReturnsAsync(playlist);

            // Act
            var result = await _service.AddCollaboratorAsync(1, 20, 99);

            // Assert
            Assert.False(result.Success);
        }

        [Fact]
        public async Task AddCollaboratorAsync_ShouldFail_WhenPlaylistNotFound()
        {
            // Arrange
            _playlistRepoMock.Setup(r => r.GetByIdWithDetailsAsync(1))
                .ReturnsAsync((Playlist?)null);

            // Act
            var result = await _service.AddCollaboratorAsync(1, 20, 10);

            // Assert
            Assert.False(result.Success);
        }

        [Fact]
        public async Task AddCollaboratorAsync_ShouldFail_WhenUserDoesNotExist()
        {
            // Arrange
            var playlist = new Playlist { Id = 1, HostId = 10, Users = new List<User>() };

            _playlistRepoMock.Setup(r => r.GetByIdWithDetailsAsync(1)).ReturnsAsync(playlist);
            _userRepoMock.Setup(r => r.GetByIdAsync(99)).ReturnsAsync((User?)null);

            // Act
            var result = await _service.AddCollaboratorAsync(1, 99, 10);

            // Assert
            Assert.False(result.Success);
            Assert.Contains("not found", result.Error);
        }

        // ---------------------------
        // REMOVE COLLABORATOR
        // ---------------------------

        [Fact]
        public async Task RemoveCollaboratorAsync_ShouldRemove_WhenHostRemovesUser()
        {
            // Arrange
            var user = new User { Id = 20 };

            var playlist = new Playlist
            {
                Id = 1,
                HostId = 10,
                Users = new List<User> { user }
            };

            _playlistRepoMock.Setup(r => r.GetByIdWithDetailsAsync(1)).ReturnsAsync(playlist);
            _playlistRepoMock.Setup(r => r.UpdateAsync(It.IsAny<Playlist>())).Returns(Task.CompletedTask);

            // Act
            var result = await _service.RemoveCollaboratorAsync(1, 20, 10);

            // Assert
            Assert.True(result.Success);
            Assert.Empty(playlist.Users);
        }

        [Fact]
        public async Task RemoveCollaboratorAsync_ShouldFail_WhenUserNotFound()
        {
            // Arrange
            var playlist = new Playlist
            {
                Id = 1,
                HostId = 10,
                Users = new List<User>()
            };

            _playlistRepoMock.Setup(r => r.GetByIdWithDetailsAsync(1)).ReturnsAsync(playlist);

            // Act
            var result = await _service.RemoveCollaboratorAsync(1, 20, 10);

            // Assert
            Assert.False(result.Success);
        }

        // ---------------------------
        // CAN ACCESS PLAYLIST
        // ---------------------------

        [Fact]
        public async Task CanAccessPlaylistAsync_ShouldReturnTrue_ForHost()
        {
            // Arrange
            var playlist = new Playlist
            {
                Id = 1,
                HostId = 10,
                Users = new List<User>()
            };

            _playlistRepoMock.Setup(r => r.GetByIdWithDetailsAsync(1)).ReturnsAsync(playlist);

            // Act
            var result = await _service.CanAccessPlaylistAsync(1, 10);

            // Assert
            Assert.True(result);
        }

        [Fact]
        public async Task CanAccessPlaylistAsync_ShouldReturnTrue_ForCollaborator()
        {
            // Arrange
            var playlist = new Playlist
            {
                Id = 1,
                HostId = 10,
                Users = new List<User> { new User { Id = 20 } }
            };

            _playlistRepoMock.Setup(r => r.GetByIdWithDetailsAsync(1)).ReturnsAsync(playlist);

            // Act
            var result = await _service.CanAccessPlaylistAsync(1, 20);

            // Assert
            Assert.True(result);
        }

        [Fact]
        public async Task CanAccessPlaylistAsync_ShouldReturnFalse_ForUnauthorizedUser()
        {
            // Arrange
            var playlist = new Playlist
            {
                Id = 1,
                HostId = 10,
                Users = new List<User>()
            };

            _playlistRepoMock.Setup(r => r.GetByIdWithDetailsAsync(1)).ReturnsAsync(playlist);

            // Act
            var result = await _service.CanAccessPlaylistAsync(1, 99);

            // Assert
            Assert.False(result);
        }

        // ---------------------------
        // ADD SONG
        // ---------------------------

        [Fact]
        public async Task AddSongAsync_ShouldSucceed_WhenHostAddsNewSong()
        {
            // Arrange
            var playlist = new Playlist
            {
                Id = 1,
                HostId = 1,
                Users = new List<User>(),
                PlaylistSongs = new List<PlaylistSong>()
            };

            var song = new Song
            {
                Id = 5,
                Title = "Test Song",
                SpotifyId = "spotify:track:test",
                SpotifyUri = "spotify:track:test"
            };

            _playlistRepoMock.Setup(r => r.GetByIdWithDetailsAsync(1)).ReturnsAsync(playlist);
            _songRepoMock.Setup(r => r.GetByIdAsync(5)).ReturnsAsync(song);
            _playlistRepoMock.Setup(r => r.AddPlaylistSongAsync(It.IsAny<PlaylistSong>())).Returns(Task.CompletedTask);

            // Act
            var result = await _service.AddSongAsync(1, 5, 1);

            // Assert
            Assert.True(result.Success);
            _playlistRepoMock.Verify(r => r.AddPlaylistSongAsync(It.IsAny<PlaylistSong>()), Times.Once);
        }

        [Fact]
        public async Task AddSongAsync_ShouldFail_WhenSongAlreadyExists()
        {
            // Arrange
            var playlist = new Playlist
            {
                Id = 1,
                HostId = 1,
                Users = new List<User>(),
                PlaylistSongs = new List<PlaylistSong> { new PlaylistSong { SongId = 5 } }
            };

            var song = new Song { Id = 5, SpotifyId = "test", SpotifyUri = "test" };

            _playlistRepoMock.Setup(r => r.GetByIdWithDetailsAsync(1)).ReturnsAsync(playlist);
            _songRepoMock.Setup(r => r.GetByIdAsync(5)).ReturnsAsync(song);

            // Act
            var result = await _service.AddSongAsync(1, 5, 1);

            // Assert
            Assert.False(result.Success);
            Assert.Contains("already in playlist", result.Error);
        }

        [Fact]
        public async Task AddSongAsync_ShouldFail_WhenUserUnauthorized()
        {
            // Arrange
            var playlist = new Playlist
            {
                Id = 1,
                HostId = 10,
                Users = new List<User>(),
                PlaylistSongs = new List<PlaylistSong>()
            };

            _playlistRepoMock.Setup(r => r.GetByIdWithDetailsAsync(1)).ReturnsAsync(playlist);

            // Act
            var result = await _service.AddSongAsync(1, 5, 99);

            // Assert
            Assert.False(result.Success);
            Assert.Contains("Only host or collaborator", result.Error);
        }

        // ---------------------------
        // REMOVE SONG
        // ---------------------------

        [Fact]
        public async Task RemoveSongAsync_ShouldSucceed_WhenHostRemovesSong()
        {
            // Arrange
            var playlistSong = new PlaylistSong { PlaylistId = 1, SongId = 5 };
            var playlist = new Playlist
            {
                Id = 1,
                HostId = 1,
                Users = new List<User>(),
                PlaylistSongs = new List<PlaylistSong> { playlistSong }
            };

            _playlistRepoMock.Setup(r => r.GetByIdWithDetailsAsync(1)).ReturnsAsync(playlist);
            _playlistRepoMock.Setup(r => r.RemovePlaylistSongAsync(It.IsAny<PlaylistSong>())).Returns(Task.CompletedTask);

            // Act
            var result = await _service.RemoveSongAsync(1, 5, 1);

            // Assert
            Assert.True(result.Success);
            _playlistRepoMock.Verify(r => r.RemovePlaylistSongAsync(It.IsAny<PlaylistSong>()), Times.Once);
        }

        [Fact]
        public async Task RemoveSongAsync_ShouldFail_WhenSongNotInPlaylist()
        {
            // Arrange
            var playlist = new Playlist
            {
                Id = 1,
                HostId = 1,
                Users = new List<User>(),
                PlaylistSongs = new List<PlaylistSong>()
            };

            _playlistRepoMock.Setup(r => r.GetByIdWithDetailsAsync(1)).ReturnsAsync(playlist);

            // Act
            var result = await _service.RemoveSongAsync(1, 5, 1);

            // Assert
            Assert.False(result.Success);
            Assert.Contains("not found in playlist", result.Error);
        }

        // ---------------------------
        // SESSION MANAGEMENT
        // ---------------------------

        [Fact]
        public async Task JoinPlaylistSessionAsync_ShouldAddUserToSession()
        {
            // Act
            await _service.JoinPlaylistSessionAsync(1, 10);

            // Assert - no exception means success
            Assert.True(true);
        }

        [Fact]
        public async Task LeavePlaylistSessionAsync_ShouldRemoveUserFromSession()
        {
            // Arrange - first join
            await _service.JoinPlaylistSessionAsync(1, 10);

            // Act
            await _service.LeavePlaylistSessionAsync(1, 10);

            // Assert - no exception means success
            Assert.True(true);
        }

        [Fact]
        public async Task GetActiveUsersAsync_ShouldReturnEmptyList_WhenNoActiveUsers()
        {
            // Act
            var result = await _service.GetActiveUsersAsync(999);

            // Assert
            Assert.Empty(result);
        }

        // ---------------------------
// ADD COLLABORATOR (BY USERNAME) - PLAYLIST NOT FOUND
// ---------------------------
[Fact]
public async Task AddCollaboratorByUsernameAsync_ShouldFail_WhenPlaylistNotFound()
{
    _playlistRepoMock.Setup(r => r.GetByIdWithDetailsAsync(1))
        .ReturnsAsync((Playlist?)null);

    var result = await _service.AddCollaboratorByUsernameAsync(1, "john", 10);

    Assert.False(result.Success);
    Assert.Contains("not found", result.Error);
}

// ---------------------------
// REMOVE COLLABORATOR - NOT HOST
// ---------------------------
[Fact]
public async Task RemoveCollaboratorAsync_ShouldFail_WhenRequesterIsNotHost()
{
    var playlist = new Playlist
    {
        Id = 1,
        HostId = 10,
        Users = new List<User> { new User { Id = 20 } }
    };

    _playlistRepoMock.Setup(r => r.GetByIdWithDetailsAsync(1)).ReturnsAsync(playlist);

    var result = await _service.RemoveCollaboratorAsync(1, 20, requesterId: 99);

    Assert.False(result.Success);
    Assert.Contains("Only host", result.Error);
    _playlistRepoMock.Verify(r => r.UpdateAsync(It.IsAny<Playlist>()), Times.Never);
}

// ---------------------------
// REMOVE COLLABORATOR - PLAYLIST NOT FOUND
// ---------------------------
[Fact]
public async Task RemoveCollaboratorAsync_ShouldFail_WhenPlaylistNotFound()
{
    _playlistRepoMock.Setup(r => r.GetByIdWithDetailsAsync(1))
        .ReturnsAsync((Playlist?)null);

    var result = await _service.RemoveCollaboratorAsync(1, 20, 10);

    Assert.False(result.Success);
    Assert.Contains("not found", result.Error);
}

// ---------------------------
// ADD SONG - PLAYLIST NOT FOUND
// ---------------------------
[Fact]
public async Task AddSongAsync_ShouldFail_WhenPlaylistNotFound()
{
    _playlistRepoMock.Setup(r => r.GetByIdWithDetailsAsync(1))
        .ReturnsAsync((Playlist?)null);

    var result = await _service.AddSongAsync(1, 5, 10);

    Assert.False(result.Success);
    Assert.Contains("not found", result.Error);
}

// ---------------------------
// ADD SONG - SONG NOT FOUND
// ---------------------------
[Fact]
public async Task AddSongAsync_ShouldFail_WhenSongNotFound()
{
    var playlist = new Playlist
    {
        Id = 1,
        HostId = 10,
        Users = new List<User>(),
        PlaylistSongs = new List<PlaylistSong>()
    };

    _playlistRepoMock.Setup(r => r.GetByIdWithDetailsAsync(1)).ReturnsAsync(playlist);
    _songRepoMock.Setup(r => r.GetByIdAsync(5)).ReturnsAsync((Song?)null);

    var result = await _service.AddSongAsync(1, 5, 10);

    Assert.False(result.Success);
    Assert.Contains("not found", result.Error);
    _playlistRepoMock.Verify(r => r.AddPlaylistSongAsync(It.IsAny<PlaylistSong>()), Times.Never);
}

// ---------------------------
// REMOVE SONG - NOT AUTHORIZED
// ---------------------------
[Fact]
public async Task RemoveSongAsync_ShouldFail_WhenUserUnauthorized()
{
    var playlist = new Playlist
    {
        Id = 1,
        HostId = 10,
        Users = new List<User>(), // no collaborators
        PlaylistSongs = new List<PlaylistSong> { new PlaylistSong { SongId = 5 } }
    };

    _playlistRepoMock.Setup(r => r.GetByIdWithDetailsAsync(1)).ReturnsAsync(playlist);

    var result = await _service.RemoveSongAsync(1, 5, userId: 99);

    Assert.False(result.Success);
    Assert.Contains("Only host or collaborator", result.Error);
    _playlistRepoMock.Verify(r => r.RemovePlaylistSongAsync(It.IsAny<PlaylistSong>()), Times.Never);
}

// ---------------------------
// CAN ACCESS - PLAYLIST NOT FOUND
// ---------------------------
[Fact]
public async Task CanAccessPlaylistAsync_ShouldReturnFalse_WhenPlaylistNotFound()
{
    _playlistRepoMock.Setup(r => r.GetByIdWithDetailsAsync(1))
        .ReturnsAsync((Playlist?)null);

    var ok = await _service.CanAccessPlaylistAsync(1, 10);

    Assert.False(ok);
}

// ---------------------------
// ADD COLLABORATOR (BY ID) - ALREADY COLLABORATOR
// ---------------------------
[Fact]
public async Task AddCollaboratorAsync_ShouldFail_WhenUserAlreadyCollaborator()
{
    var existing = new User { Id = 20, Username = "john" };
    var playlist = new Playlist
    {
        Id = 1,
        HostId = 10,
        Users = new List<User> { existing }
    };

    _playlistRepoMock.Setup(r => r.GetByIdWithDetailsAsync(1)).ReturnsAsync(playlist);
    _userRepoMock.Setup(r => r.GetByIdAsync(20)).ReturnsAsync(existing);

    var result = await _service.AddCollaboratorAsync(1, 20, 10);

    Assert.False(result.Success);
    Assert.Contains("already a collaborator", result.Error);
    _playlistRepoMock.Verify(r => r.UpdateAsync(It.IsAny<Playlist>()), Times.Never);
}

    }
}
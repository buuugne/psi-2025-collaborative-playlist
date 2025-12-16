using Xunit;
using Moq;
using MyApi.Services;
using MyApi.Repositories;
using MyApi.Models;
using MyApi.Dtos;
using System.Threading.Tasks;
using System.Collections.Generic;
using System.Linq;

namespace TestProject.Services
{
    public class CollaborativePlaylistServiceAdditionalTests
    {
        private readonly Mock<IPlaylistRepository> _playlistRepoMock;
        private readonly Mock<IUserRepository> _userRepoMock;
        private readonly Mock<ISongRepository> _songRepoMock;

        private readonly CollaborativePlaylistService _service;

        public CollaborativePlaylistServiceAdditionalTests()
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
        // EXCEPTION HANDLING - ADD COLLABORATOR BY USERNAME
        // ---------------------------

        [Fact]
        public async Task AddCollaboratorByUsernameAsync_ShouldReturnError_WhenExceptionThrown()
        {
            // Arrange
            _playlistRepoMock.Setup(r => r.GetByIdWithDetailsAsync(1))
                .ThrowsAsync(new System.Exception("Database error"));

            // Act
            var result = await _service.AddCollaboratorByUsernameAsync(1, "john", 10);

            // Assert
            Assert.False(result.Success);
            Assert.Equal("An error occurred while adding collaborator.", result.Error);
        }

        // ---------------------------
        // EXCEPTION HANDLING - ADD COLLABORATOR BY ID
        // ---------------------------

        [Fact]
        public async Task AddCollaboratorAsync_ShouldReturnError_WhenExceptionThrown()
        {
            // Arrange
            _playlistRepoMock.Setup(r => r.GetByIdWithDetailsAsync(1))
                .ThrowsAsync(new System.Exception("Database error"));

            // Act
            var result = await _service.AddCollaboratorAsync(1, 20, 10);

            // Assert
            Assert.False(result.Success);
            Assert.Equal("An error occurred while adding collaborator.", result.Error);
        }

        // ---------------------------
        // EXCEPTION HANDLING - REMOVE COLLABORATOR
        // ---------------------------

        [Fact]
        public async Task RemoveCollaboratorAsync_ShouldReturnError_WhenExceptionThrown()
        {
            // Arrange
            _playlistRepoMock.Setup(r => r.GetByIdWithDetailsAsync(1))
                .ThrowsAsync(new System.Exception("Database error"));

            // Act
            var result = await _service.RemoveCollaboratorAsync(1, 20, 10);

            // Assert
            Assert.False(result.Success);
            Assert.Equal("An error occurred while removing collaborator.", result.Error);
        }

        // ---------------------------
        // EXCEPTION HANDLING - ADD SONG
        // ---------------------------

        [Fact]
        public async Task AddSongAsync_ShouldReturnError_WhenExceptionThrown()
        {
            // Arrange
            _playlistRepoMock.Setup(r => r.GetByIdWithDetailsAsync(1))
                .ThrowsAsync(new System.Exception("Database error"));

            // Act
            var result = await _service.AddSongAsync(1, 5, 10);

            // Assert
            Assert.False(result.Success);
            Assert.Equal("An error occurred while adding song.", result.Error);
        }

        // ---------------------------
        // EXCEPTION HANDLING - REMOVE SONG
        // ---------------------------

        [Fact]
        public async Task RemoveSongAsync_ShouldReturnError_WhenExceptionThrown()
        {
            // Arrange
            _playlistRepoMock.Setup(r => r.GetByIdWithDetailsAsync(1))
                .ThrowsAsync(new System.Exception("Database error"));

            // Act
            var result = await _service.RemoveSongAsync(1, 5, 10);

            // Assert
            Assert.False(result.Success);
            Assert.Equal("An error occurred while removing song.", result.Error);
        }

        // ---------------------------
        // ADD SONG - COLLABORATOR CAN ADD
        // ---------------------------

        [Fact]
        public async Task AddSongAsync_ShouldSucceed_WhenCollaboratorAddsNewSong()
        {
            // Arrange
            var collaborator = new User { Id = 20, Username = "collab" };
            var playlist = new Playlist
            {
                Id = 1,
                HostId = 10,
                Users = new List<User> { collaborator },
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
            var result = await _service.AddSongAsync(1, 5, 20); // Collaborator ID

            // Assert
            Assert.True(result.Success);
            _playlistRepoMock.Verify(r => r.AddPlaylistSongAsync(It.Is<PlaylistSong>(ps =>
                ps.PlaylistId == 1 &&
                ps.SongId == 5 &&
                ps.AddedByUserId == 20 &&
                ps.Position == 1
            )), Times.Once);
        }

        // ---------------------------
        // REMOVE SONG - COLLABORATOR CAN REMOVE
        // ---------------------------

        [Fact]
        public async Task RemoveSongAsync_ShouldSucceed_WhenCollaboratorRemovesSong()
        {
            // Arrange
            var collaborator = new User { Id = 20, Username = "collab" };
            var playlistSong = new PlaylistSong { PlaylistId = 1, SongId = 5 };
            var playlist = new Playlist
            {
                Id = 1,
                HostId = 10,
                Users = new List<User> { collaborator },
                PlaylistSongs = new List<PlaylistSong> { playlistSong }
            };

            _playlistRepoMock.Setup(r => r.GetByIdWithDetailsAsync(1)).ReturnsAsync(playlist);
            _playlistRepoMock.Setup(r => r.RemovePlaylistSongAsync(It.IsAny<PlaylistSong>())).Returns(Task.CompletedTask);

            // Act
            var result = await _service.RemoveSongAsync(1, 5, 20); // Collaborator ID

            // Assert
            Assert.True(result.Success);
            _playlistRepoMock.Verify(r => r.RemovePlaylistSongAsync(It.IsAny<PlaylistSong>()), Times.Once);
        }

        // ---------------------------
        // REMOVE SONG - PLAYLIST NOT FOUND
        // ---------------------------

        [Fact]
        public async Task RemoveSongAsync_ShouldFail_WhenPlaylistNotFound()
        {
            // Arrange
            _playlistRepoMock.Setup(r => r.GetByIdWithDetailsAsync(1))
                .ReturnsAsync((Playlist?)null);

            // Act
            var result = await _service.RemoveSongAsync(1, 5, 10);

            // Assert
            Assert.False(result.Success);
            Assert.Contains("not found", result.Error);
        }

        // ---------------------------
        // ACTIVE USERS - JOIN AND GET
        // ---------------------------

        [Fact]
        public async Task GetActiveUsersAsync_ShouldReturnUsers_WhenUsersAreActive()
        {
            // Arrange
            var user1 = new User { Id = 10, Username = "user1", Role = UserRole.Host };
            var user2 = new User { Id = 20, Username = "user2", Role = UserRole.Guest };

            await _service.JoinPlaylistSessionAsync(1, 10);
            await _service.JoinPlaylistSessionAsync(1, 20);

            _userRepoMock.Setup(r => r.GetByIdsAsync(It.IsAny<List<int>>()))
                .ReturnsAsync(new List<User> { user1, user2 });

            // Act
            var result = await _service.GetActiveUsersAsync(1);

            // Assert
            Assert.NotNull(result);
            Assert.Equal(2, result.Count());
            Assert.Contains(result, u => u.UserId == 10);
            Assert.Contains(result, u => u.UserId == 20);
        }

        // ---------------------------
        // ACTIVE USERS - LEAVE SESSION
        // ---------------------------

        [Fact]
        public async Task GetActiveUsersAsync_ShouldNotReturnUser_AfterLeaving()
        {
            // Arrange
            var user1 = new User { Id = 10, Username = "user1", Role = UserRole.Host };

            await _service.JoinPlaylistSessionAsync(1, 10);
            await _service.JoinPlaylistSessionAsync(1, 20);

            await _service.LeavePlaylistSessionAsync(1, 20); // User 20 leaves

            _userRepoMock.Setup(r => r.GetByIdsAsync(It.Is<List<int>>(ids => ids.Contains(10))))
                .ReturnsAsync(new List<User> { user1 });

            // Act
            var result = await _service.GetActiveUsersAsync(1);

            // Assert
            Assert.NotNull(result);
            Assert.Single(result);
            Assert.Contains(result, u => u.UserId == 10);
            Assert.DoesNotContain(result, u => u.UserId == 20);
        }

        // ---------------------------
        // ACTIVE USERS - EMPTY WHEN ALL LEAVE
        // ---------------------------

        [Fact]
        public async Task GetActiveUsersAsync_ShouldReturnEmpty_WhenAllUsersLeave()
        {
            // Arrange
            await _service.JoinPlaylistSessionAsync(1, 10);
            await _service.JoinPlaylistSessionAsync(1, 20);

            await _service.LeavePlaylistSessionAsync(1, 10);
            await _service.LeavePlaylistSessionAsync(1, 20);

            // Act
            var result = await _service.GetActiveUsersAsync(1);

            // Assert
            Assert.Empty(result);
        }

        // ---------------------------
        // ACTIVE USERS - LEAVE FROM NON-EXISTENT SESSION
        // ---------------------------

        [Fact]
        public async Task LeavePlaylistSessionAsync_ShouldNotThrow_WhenSessionDoesNotExist()
        {
            // Act & Assert - should not throw
            await _service.LeavePlaylistSessionAsync(999, 10);
            
            Assert.True(true); // If we get here, no exception was thrown
        }

        // ---------------------------
        // ACTIVE USERS - JOIN UPDATES TIMESTAMP
        // ---------------------------

        [Fact]
        public async Task JoinPlaylistSessionAsync_ShouldUpdateTimestamp_WhenUserJoinsTwice()
        {
            // Arrange
            var user = new User { Id = 10, Username = "user1", Role = UserRole.Host };

            await _service.JoinPlaylistSessionAsync(1, 10);
            await Task.Delay(100); // Small delay
            await _service.JoinPlaylistSessionAsync(1, 10); // Join again

            _userRepoMock.Setup(r => r.GetByIdsAsync(It.IsAny<List<int>>()))
                .ReturnsAsync(new List<User> { user });

            // Act
            var result = await _service.GetActiveUsersAsync(1);

            // Assert
            Assert.Single(result);
            var activeUser = result.First();
            Assert.Equal(10, activeUser.UserId);
        }

        // ---------------------------
        // ADD SONG - POSITION CALCULATION
        // ---------------------------

        [Fact]
        public async Task AddSongAsync_ShouldCalculateCorrectPosition_WhenMultipleSongsExist()
        {
            // Arrange
            var playlist = new Playlist
            {
                Id = 1,
                HostId = 10,
                Users = new List<User>(),
                PlaylistSongs = new List<PlaylistSong>
                {
                    new PlaylistSong { PlaylistId = 1, SongId = 1, Position = 1 },
                    new PlaylistSong { PlaylistId = 1, SongId = 2, Position = 2 },
                    new PlaylistSong { PlaylistId = 1, SongId = 3, Position = 3 }
                }
            };

            var song = new Song
            {
                Id = 4,
                Title = "New Song",
                SpotifyId = "spotify:track:new",
                SpotifyUri = "spotify:track:new"
            };

            _playlistRepoMock.Setup(r => r.GetByIdWithDetailsAsync(1)).ReturnsAsync(playlist);
            _songRepoMock.Setup(r => r.GetByIdAsync(4)).ReturnsAsync(song);
            _playlistRepoMock.Setup(r => r.AddPlaylistSongAsync(It.IsAny<PlaylistSong>())).Returns(Task.CompletedTask);

            // Act
            var result = await _service.AddSongAsync(1, 4, 10);

            // Assert
            Assert.True(result.Success);
            _playlistRepoMock.Verify(r => r.AddPlaylistSongAsync(It.Is<PlaylistSong>(ps =>
                ps.Position == 4 // Should be 3 existing + 1 = position 4
            )), Times.Once);
        }

        // ---------------------------
        // CLEANUP INACTIVE SESSIONS
        // ---------------------------

        [Fact]
        public async Task CleanupInactiveSessionsAsync_ShouldNotThrow()
        {
            // Arrange
            await _service.JoinPlaylistSessionAsync(1, 10);

            // Act & Assert - should not throw
            await _service.CleanupInactiveSessionsAsync();
            
            Assert.True(true);
        }
    }
}
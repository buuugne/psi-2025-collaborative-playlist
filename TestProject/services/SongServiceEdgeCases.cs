using Xunit;
using Moq;
using MyApi.Services;
using MyApi.Repositories;
using MyApi.Models;
using MyApi.Dtos;
using System.Collections.Generic;
using System.Linq;

namespace TestProject
{
    public class SongServiceEdgeCaseTests
    {
        [Fact]
        public async Task AddSongToPlaylistAsync_WithEmptySpotifyId_ShouldReturnError()
        {
            // Arrange
            var mockSongRepo = new Mock<ISongRepository>();
            var mockPlaylistRepo = new Mock<IPlaylistRepository>();
            var mockSpotifyService = new Mock<ISpotifyService>();

            mockPlaylistRepo.Setup(x => x.GetByIdWithDetailsAsync(1))
                .ReturnsAsync(new Playlist
                {
                    Id = 1,
                    Name = "Test",
                    Description = "desc",
                    HostId = 1,
                    Host = new User { Id = 1, Username = "host", Role = UserRole.Host, PasswordHash = "hash" },
                    PlaylistSongs = new List<PlaylistSong>(),
                    Users = new List<User>()
                });

            var service = new SongService(mockSongRepo.Object, mockPlaylistRepo.Object, mockSpotifyService.Object);

            var dto = new AddSongToPlaylistDto
            {
                PlaylistId = 1,
                SpotifyId = "", // EMPTY!
                SpotifyUri = "uri",
                Title = "Test"
            };

            // Act
            var (success, error, songId) = await service.AddSongToPlaylistAsync(dto);

            // Assert
            Assert.False(success);
            Assert.Equal("SpotifyId is required to add a track.", error);
            Assert.Null(songId);
        }

        [Fact]
        public async Task AddSongToPlaylistAsync_WithNullSpotifyId_ShouldReturnError()
        {
            // Arrange
            var mockSongRepo = new Mock<ISongRepository>();
            var mockPlaylistRepo = new Mock<IPlaylistRepository>();
            var mockSpotifyService = new Mock<ISpotifyService>();

            mockPlaylistRepo.Setup(x => x.GetByIdWithDetailsAsync(1))
                .ReturnsAsync(new Playlist
                {
                    Id = 1,
                    Name = "Test",
                    Description = "desc",
                    HostId = 1,
                    Host = new User { Id = 1, Username = "host", Role = UserRole.Host, PasswordHash = "hash" },
                    PlaylistSongs = new List<PlaylistSong>(),
                    Users = new List<User>()
                });

            var service = new SongService(mockSongRepo.Object, mockPlaylistRepo.Object, mockSpotifyService.Object);

            var dto = new AddSongToPlaylistDto
            {
                PlaylistId = 1,
                SpotifyId = null!, // NULL!
                SpotifyUri = "uri",
                Title = "Test"
            };

            // Act
            var (success, error, songId) = await service.AddSongToPlaylistAsync(dto);

            // Assert
            Assert.False(success);
            Assert.Equal("SpotifyId is required to add a track.", error);
            Assert.Null(songId);
        }

        [Fact]
        public async Task AddSongToPlaylistAsync_WhenSpotifyFails_ShouldReturnError()
        {
            // Arrange
            var mockSongRepo = new Mock<ISongRepository>();
            var mockPlaylistRepo = new Mock<IPlaylistRepository>();
            var mockSpotifyService = new Mock<ISpotifyService>();

            mockPlaylistRepo.Setup(x => x.GetByIdWithDetailsAsync(1))
                .ReturnsAsync(new Playlist
                {
                    Id = 1,
                    Name = "Test",
                    Description = "desc",
                    HostId = 1,
                    Host = new User { Id = 1, Username = "host", Role = UserRole.Host, PasswordHash = "hash" },
                    PlaylistSongs = new List<PlaylistSong>(),
                    Users = new List<User>()
                });

            // Spotify returns FAILURE
            mockSpotifyService.Setup(x => x.GetTrackDetails("bad_id"))
                .ReturnsAsync((false, "Track not found on Spotify", null));

            var service = new SongService(mockSongRepo.Object, mockPlaylistRepo.Object, mockSpotifyService.Object);

            var dto = new AddSongToPlaylistDto
            {
                PlaylistId = 1,
                SpotifyId = "bad_id",
                SpotifyUri = "uri",
                Title = "Test"
            };

            // Act
            var (success, error, songId) = await service.AddSongToPlaylistAsync(dto);

            // Assert
            Assert.False(success);
            Assert.Contains("Failed to retrieve song details from Spotify", error);
            Assert.Null(songId);
        }

        [Fact]
        public async Task AddSongToPlaylistAsync_WhenSpotifyReturnsNull_ShouldReturnError()
        {
            // Arrange
            var mockSongRepo = new Mock<ISongRepository>();
            var mockPlaylistRepo = new Mock<IPlaylistRepository>();
            var mockSpotifyService = new Mock<ISpotifyService>();

            mockPlaylistRepo.Setup(x => x.GetByIdWithDetailsAsync(1))
                .ReturnsAsync(new Playlist
                {
                    Id = 1,
                    Name = "Test",
                    Description = "desc",
                    HostId = 1,
                    Host = new User { Id = 1, Username = "host", Role = UserRole.Host, PasswordHash = "hash" },
                    PlaylistSongs = new List<PlaylistSong>(),
                    Users = new List<User>()
                });

            // Spotify returns SUCCESS but NULL details
            mockSpotifyService.Setup(x => x.GetTrackDetails("test_id"))
                .ReturnsAsync((true, null, null));

            var service = new SongService(mockSongRepo.Object, mockPlaylistRepo.Object, mockSpotifyService.Object);

            var dto = new AddSongToPlaylistDto
            {
                PlaylistId = 1,
                SpotifyId = "test_id",
                SpotifyUri = "uri",
                Title = "Test"
            };

            // Act
            var (success, error, songId) = await service.AddSongToPlaylistAsync(dto);

            // Assert
            Assert.False(success);
            Assert.Contains("Failed to retrieve song details from Spotify", error);
            Assert.Null(songId);
        }

        [Fact]
        public async Task AddSongToPlaylistAsync_WhenSpotifyReturnsEmptySpotifyId_ShouldReturnError()
        {
            // Arrange
            var mockSongRepo = new Mock<ISongRepository>();
            var mockPlaylistRepo = new Mock<IPlaylistRepository>();
            var mockSpotifyService = new Mock<ISpotifyService>();

            mockPlaylistRepo.Setup(x => x.GetByIdWithDetailsAsync(1))
                .ReturnsAsync(new Playlist
                {
                    Id = 1,
                    Name = "Test",
                    Description = "desc",
                    HostId = 1,
                    Host = new User { Id = 1, Username = "host", Role = UserRole.Host, PasswordHash = "hash" },
                    PlaylistSongs = new List<PlaylistSong>(),
                    Users = new List<User>()
                });

            // Spotify returns track with EMPTY SpotifyId
            mockSpotifyService.Setup(x => x.GetTrackDetails("test_id"))
                .ReturnsAsync((true, null, new SpotifyTrackDetails
                {
                    SpotifyId = "", // EMPTY!
                    SpotifyUri = "uri",
                    Title = "Test Song"
                }));

            var service = new SongService(mockSongRepo.Object, mockPlaylistRepo.Object, mockSpotifyService.Object);

            var dto = new AddSongToPlaylistDto
            {
                PlaylistId = 1,
                SpotifyId = "test_id",
                SpotifyUri = "uri",
                Title = "Test"
            };

            // Act
            var (success, error, songId) = await service.AddSongToPlaylistAsync(dto);

            // Assert
            Assert.False(success);
            Assert.Equal("Spotify returned invalid track ID.", error);
            Assert.Null(songId);
        }

        [Fact]
        public async Task AddSongToPlaylistAsync_WhenSpotifyReturnsEmptySpotifyUri_ShouldReturnError()
        {
            // Arrange
            var mockSongRepo = new Mock<ISongRepository>();
            var mockPlaylistRepo = new Mock<IPlaylistRepository>();
            var mockSpotifyService = new Mock<ISpotifyService>();

            mockPlaylistRepo.Setup(x => x.GetByIdWithDetailsAsync(1))
                .ReturnsAsync(new Playlist
                {
                    Id = 1,
                    Name = "Test",
                    Description = "desc",
                    HostId = 1,
                    Host = new User { Id = 1, Username = "host", Role = UserRole.Host, PasswordHash = "hash" },
                    PlaylistSongs = new List<PlaylistSong>(),
                    Users = new List<User>()
                });

            // Spotify returns track with EMPTY SpotifyUri
            mockSpotifyService.Setup(x => x.GetTrackDetails("test_id"))
                .ReturnsAsync((true, null, new SpotifyTrackDetails
                {
                    SpotifyId = "valid_id",
                    SpotifyUri = "", // EMPTY!
                    Title = "Test Song"
                }));

            var service = new SongService(mockSongRepo.Object, mockPlaylistRepo.Object, mockSpotifyService.Object);

            var dto = new AddSongToPlaylistDto
            {
                PlaylistId = 1,
                SpotifyId = "test_id",
                SpotifyUri = "uri",
                Title = "Test"
            };

            // Act
            var (success, error, songId) = await service.AddSongToPlaylistAsync(dto);

            // Assert
            Assert.False(success);
            Assert.Equal("Spotify returned invalid track URI.", error);
            Assert.Null(songId);
        }

        [Fact]
        public async Task AddSongToPlaylistAsync_WithEmptyArtists_ShouldUseEmptyList()
        {
            // Arrange
            var mockSongRepo = new Mock<ISongRepository>();
            var mockPlaylistRepo = new Mock<IPlaylistRepository>();
            var mockSpotifyService = new Mock<ISpotifyService>();

            mockPlaylistRepo.Setup(x => x.GetByIdWithDetailsAsync(1))
                .ReturnsAsync(new Playlist
                {
                    Id = 1,
                    Name = "Test",
                    Description = "desc",
                    HostId = 1,
                    Host = new User { Id = 1, Username = "host", Role = UserRole.Host, PasswordHash = "hash" },
                    PlaylistSongs = new List<PlaylistSong>(),
                    Users = new List<User>()
                });

            // Spotify returns track with EMPTY Artists list
            mockSpotifyService.Setup(x => x.GetTrackDetails("test_id"))
                .ReturnsAsync((true, null, new SpotifyTrackDetails
                {
                    SpotifyId = "valid_id",
                    SpotifyUri = "valid_uri",
                    Title = "Test Song",
                    Artists = new List<SpotifyArtistDetails>(), // EMPTY list
                    DurationMs = 180000
                }));

            mockSongRepo.Setup(x => x.EnsureSongWithArtistsAsync(
                    It.IsAny<string>(),
                    It.IsAny<string>(),
                    It.IsAny<int?>(),
                    It.Is<IEnumerable<string>>(list => !list.Any()), // Should be empty
                    It.IsAny<string>(),
                    It.IsAny<string>()
                ))
                .ReturnsAsync(new Song
                {
                    Id = 99,
                    Title = "Test Song",
                    SpotifyId = "valid_id",
                    SpotifyUri = "valid_uri",
                    Artists = new List<Artist>()
                });

            var service = new SongService(mockSongRepo.Object, mockPlaylistRepo.Object, mockSpotifyService.Object);

            var dto = new AddSongToPlaylistDto
            {
                PlaylistId = 1,
                SpotifyId = "test_id",
                SpotifyUri = "uri",
                Title = "Test"
            };

            // Act
            var (success, error, songId) = await service.AddSongToPlaylistAsync(dto);

            // Assert
            Assert.True(success);
            Assert.Null(error);
            Assert.Equal(99, songId);
        }

        [Fact]
        public async Task AddSongToPlaylistAsync_WithNullDuration_ShouldHandleGracefully()
        {
            // Arrange
            var mockSongRepo = new Mock<ISongRepository>();
            var mockPlaylistRepo = new Mock<IPlaylistRepository>();
            var mockSpotifyService = new Mock<ISpotifyService>();

            mockPlaylistRepo.Setup(x => x.GetByIdWithDetailsAsync(1))
                .ReturnsAsync(new Playlist
                {
                    Id = 1,
                    Name = "Test",
                    Description = "desc",
                    HostId = 1,
                    Host = new User { Id = 1, Username = "host", Role = UserRole.Host, PasswordHash = "hash" },
                    PlaylistSongs = new List<PlaylistSong>(),
                    Users = new List<User>()
                });

            // Spotify returns track with NULL Duration
            mockSpotifyService.Setup(x => x.GetTrackDetails("test_id"))
                .ReturnsAsync((true, null, new SpotifyTrackDetails
                {
                    SpotifyId = "valid_id",
                    SpotifyUri = "valid_uri",
                    Title = "Test Song",
                    Artists = new List<SpotifyArtistDetails>
                    {
                        new SpotifyArtistDetails { Name = "Artist" }
                    },
                    DurationMs = null // NULL!
                }));

            mockSongRepo.Setup(x => x.EnsureSongWithArtistsAsync(
                    It.IsAny<string>(),
                    It.IsAny<string>(),
                    null, // Duration should be null
                    It.IsAny<IEnumerable<string>>(),
                    It.IsAny<string>(),
                    It.IsAny<string>()
                ))
                .ReturnsAsync(new Song
                {
                    Id = 99,
                    Title = "Test Song",
                    SpotifyId = "valid_id",
                    SpotifyUri = "valid_uri",
                    DurationSeconds = null,
                    Artists = new List<Artist>()
                });

            var service = new SongService(mockSongRepo.Object, mockPlaylistRepo.Object, mockSpotifyService.Object);

            var dto = new AddSongToPlaylistDto
            {
                PlaylistId = 1,
                SpotifyId = "test_id",
                SpotifyUri = "uri",
                Title = "Test"
            };

            // Act
            var (success, error, songId) = await service.AddSongToPlaylistAsync(dto);

            // Assert
            Assert.True(success);
            Assert.Null(error);
            Assert.Equal(99, songId);
        }

        [Fact]
        public async Task GetAllAsync_WithNullDuration_ShouldReturnNullDurationInDto()
        {
            // Arrange
            var mockSongRepo = new Mock<ISongRepository>();
            var mockPlaylistRepo = new Mock<IPlaylistRepository>();
            var mockSpotifyService = new Mock<ISpotifyService>();

            var songs = new List<Song>
            {
                new Song
                {
                    Id = 1,
                    Title = "Song Without Duration",
                    SpotifyId = "spot1",
                    SpotifyUri = "uri1",
                    DurationSeconds = null, // NULL!
                    Artists = new List<Artist>()
                }
            };

            mockSongRepo.Setup(x => x.GetAllAsync()).ReturnsAsync(songs);

            var service = new SongService(mockSongRepo.Object, mockPlaylistRepo.Object, mockSpotifyService.Object);

            // Act
            var result = await service.GetAllAsync();

            // Assert
            var songDto = result.First();
            Assert.Null(songDto.Duration);
            Assert.Null(songDto.DurationFormatted);
        }

        [Fact]
        public async Task GetByIdAsync_WithNullDuration_ShouldReturnNullDurationInDto()
        {
            // Arrange
            var mockSongRepo = new Mock<ISongRepository>();
            var mockPlaylistRepo = new Mock<IPlaylistRepository>();
            var mockSpotifyService = new Mock<ISpotifyService>();

            mockSongRepo.Setup(x => x.GetByIdAsync(1))
                .ReturnsAsync(new Song
                {
                    Id = 1,
                    Title = "Song Without Duration",
                    SpotifyId = "spot1",
                    SpotifyUri = "uri1",
                    DurationSeconds = null, // NULL!
                    Artists = new List<Artist>()
                });

            var service = new SongService(mockSongRepo.Object, mockPlaylistRepo.Object, mockSpotifyService.Object);

            // Act
            var result = await service.GetByIdAsync(1);

            // Assert
            Assert.NotNull(result);
            Assert.Null(result.Duration);
            Assert.Null(result.DurationFormatted);
        }

        [Fact]
public async Task AddSongToPlaylistAsync_SuccessfulAdd_ShouldReturnSongId()
{
    // Arrange
    var mockSongRepo = new Mock<ISongRepository>();
    var mockPlaylistRepo = new Mock<IPlaylistRepository>();
    var mockSpotifyService = new Mock<ISpotifyService>();

    var playlistId = 1;
    var newSongId = 100;

    mockPlaylistRepo.Setup(x => x.GetByIdWithDetailsAsync(playlistId))
        .ReturnsAsync(new Playlist
        {
            Id = playlistId,
            Name = "Test Playlist",
            Description = "desc",
            HostId = 1,
            Host = new User { Id = 1, Username = "host", Role = UserRole.Host, PasswordHash = "hash" },
            PlaylistSongs = new List<PlaylistSong>(), // Empty - no songs yet
            Users = new List<User>()
        });

    mockSpotifyService.Setup(x => x.GetTrackDetails("spotify123"))
        .ReturnsAsync((true, null, new SpotifyTrackDetails
        {
            SpotifyId = "spotify123",
            SpotifyUri = "spotify:track:123",
            Title = "New Song",
            Artists = new List<SpotifyArtistDetails>
            {
                new SpotifyArtistDetails { Name = "Artist 1" },
                new SpotifyArtistDetails { Name = "Artist 2" }
            },
            AlbumInfo = new SpotifyAlbumDetails { Name = "Album Name" },
            DurationMs = 210000
        }));

    mockSongRepo.Setup(x => x.EnsureSongWithArtistsAsync(
            "New Song",
            "Album Name",
            210, // 210000 / 1000
            It.IsAny<IEnumerable<string>>(),
            "spotify123",
            "spotify:track:123"
        ))
        .ReturnsAsync(new Song
        {
            Id = newSongId,
            Title = "New Song",
            Album = "Album Name",
            SpotifyId = "spotify123",
            SpotifyUri = "spotify:track:123",
            DurationSeconds = 210,
            Artists = new List<Artist>()
        });

    mockPlaylistRepo.Setup(x => x.AddPlaylistSongAsync(It.IsAny<PlaylistSong>()))
        .Returns(Task.CompletedTask);

    var service = new SongService(mockSongRepo.Object, mockPlaylistRepo.Object, mockSpotifyService.Object);

    var dto = new AddSongToPlaylistDto
    {
        PlaylistId = playlistId,
        SpotifyId = "spotify123",
        SpotifyUri = "spotify:track:123",
        Title = "New Song",
        AddedByUserId = 5
    };

    // Act
    var (success, error, songId) = await service.AddSongToPlaylistAsync(dto);

    // Assert
    Assert.True(success);
    Assert.Null(error);
    Assert.Equal(newSongId, songId);
    
    mockPlaylistRepo.Verify(x => x.AddPlaylistSongAsync(It.Is<PlaylistSong>(ps =>
        ps.PlaylistId == playlistId &&
        ps.SongId == newSongId &&
        ps.AddedByUserId == 5 &&
        ps.Position == 1
    )), Times.Once);
}

[Fact]
public async Task GetAllAsync_WithDuration_ShouldConvertToMilliseconds()
{
    // Arrange
    var mockSongRepo = new Mock<ISongRepository>();
    var mockPlaylistRepo = new Mock<IPlaylistRepository>();
    var mockSpotifyService = new Mock<ISpotifyService>();

    var songs = new List<Song>
    {
        new Song
        {
            Id = 1,
            Title = "Song With Duration",
            SpotifyId = "spot1",
            SpotifyUri = "uri1",
            DurationSeconds = 180, // 3 minutes
            Artists = new List<Artist>
            {
                new Artist { Id = 1, Name = "Artist 1" }
            }
        }
    };

    mockSongRepo.Setup(x => x.GetAllAsync()).ReturnsAsync(songs);

    var service = new SongService(mockSongRepo.Object, mockPlaylistRepo.Object, mockSpotifyService.Object);

    // Act
    var result = await service.GetAllAsync();

    // Assert
    var songDto = result.First();
    Assert.Equal(180000, songDto.Duration); // 180 * 1000
    Assert.NotNull(songDto.DurationFormatted);
}

[Fact]
public async Task GetByIdAsync_WithDuration_ShouldConvertToMilliseconds()
{
    // Arrange
    var mockSongRepo = new Mock<ISongRepository>();
    var mockPlaylistRepo = new Mock<IPlaylistRepository>();
    var mockSpotifyService = new Mock<ISpotifyService>();

    mockSongRepo.Setup(x => x.GetByIdAsync(1))
        .ReturnsAsync(new Song
        {
            Id = 1,
            Title = "Song With Duration",
            SpotifyId = "spot1",
            SpotifyUri = "uri1",
            DurationSeconds = 240, // 4 minutes
            Artists = new List<Artist>
            {
                new Artist { Id = 1, Name = "Artist 1" }
            }
        });

    var service = new SongService(mockSongRepo.Object, mockPlaylistRepo.Object, mockSpotifyService.Object);

    // Act
    var result = await service.GetByIdAsync(1);

    // Assert
    Assert.NotNull(result);
    Assert.Equal(240000, result.Duration); // 240 * 1000
    Assert.NotNull(result.DurationFormatted);
}
    }
}
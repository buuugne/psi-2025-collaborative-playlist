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
    public class SongServiceTests
    {

        [Fact]
        public async Task DeleteAsync_ShouldFail_WhenIdDoesNotExist()
        {
            // --- ARRANGE ---
            var mockPlaylistRepo = new Mock<IPlaylistRepository>();
            var mockSongRepo = new Mock<ISongRepository>();

            var id = 42;
            mockSongRepo.Setup(x => x.GetByIdAsync(id)).ReturnsAsync((Song?)null);

            var mockSpotifyService = new Mock<ISpotifyService>();
            var service = new SongService(mockSongRepo.Object, mockPlaylistRepo.Object, mockSpotifyService.Object);

            // --- ACT ---
            var (success, error) = await service.DeleteAsync(id);

            // --- ASSERT ---
            Assert.False(success);
            Assert.Equal($"Song with ID {id} not found.", error);
        }


        [Fact]
        public async Task DeleteAsync_ShouldSucceed_WhenIdExists()
        {
            // --- ARRANGE ---
            var mockSongRepo = new Mock<ISongRepository>();
            var mockPlaylistRepo = new Mock<IPlaylistRepository>();

            var id = 42;

            mockSongRepo.Setup(x => x.GetByIdAsync(id))
                .ReturnsAsync(new Song
                {
                    Id = id,
                    Title = "Test Song",
                    Album = "Test Album",
                    SpotifyId = "spotify:track:test123",
                    SpotifyUri = "spotify:track:test123",
                    Artists = new List<Artist>()
                });

            mockSongRepo.Setup(x => x.DeleteAsync(It.IsAny<Song>()))
                .Returns(Task.CompletedTask);

            var mockSpotifyService = new Mock<ISpotifyService>();
            var service = new SongService(mockSongRepo.Object, mockPlaylistRepo.Object, mockSpotifyService.Object);

            // --- ACT ---
            var (success, error) = await service.DeleteAsync(id);

            // --- ASSERT ---
            Assert.True(success);
            Assert.Null(error);
            mockSongRepo.Verify(x => x.DeleteAsync(It.IsAny<Song>()), Times.Once);
        }

        [Fact]
        public async Task AddSongToPlaylistAsync_ShouldFail_WhenSongAlreadyInPlaylist()
        {
            // --- ARRANGE ---
            var mockPlaylistRepo = new Mock<IPlaylistRepository>();
            var mockSongRepo = new Mock<ISongRepository>();
            var mockSpotifyService = new Mock<ISpotifyService>();

            var playlistId = 42;
            var songId = 10;
            var spotifyId = "spotify:track:existing123";
            var spotifyUri = "spotify:track:existing123";

            // Mock: Playlist EXISTS with the song ALREADY in it
            mockPlaylistRepo.Setup(x => x.GetByIdWithDetailsAsync(playlistId))
                .ReturnsAsync(new Playlist
                {
                    Id = playlistId,
                    Name = "Test Playlist",
                    Description = "desc",
                    HostId = 1,
                    Host = new User { Id = 1, Username = "host", Role = UserRole.Host, PasswordHash = "hash" },
                    PlaylistSongs = new List<PlaylistSong>
                    {
                        // Song is ALREADY in the playlist
                        new PlaylistSong
                        {
                            PlaylistId = playlistId,
                            SongId = songId,
                            Position = 1,
                            Song = new Song
                            {
                                Id = songId,
                                Title = "Existing Song",
                                Album = "Album",
                                SpotifyId = spotifyId,
                                SpotifyUri = spotifyUri,
                                Artists = new List<Artist>()
                            }
                        }
                    },
                    Users = new List<User>()
                });

            // Mock: Spotify API returns track details using REAL DTO
            mockSpotifyService.Setup(x => x.GetTrackDetails(spotifyId))
                .ReturnsAsync((true, null, new SpotifyTrackDetails
                {
                    SpotifyId = spotifyId,
                    SpotifyUri = spotifyUri,
                    Title = "Existing Song",
                    Artists = new List<SpotifyArtistDetails>
                    {
                        new SpotifyArtistDetails { Name = "Artist 1" }
                    },
                    AlbumInfo = new SpotifyAlbumDetails { Name = "Album" },
                    DurationMs = 180000
                }));

            // Mock: EnsureSongWithArtistsAsync returns the existing song
            mockSongRepo.Setup(x => x.EnsureSongWithArtistsAsync(
                    It.IsAny<string>(),   // title
                    It.IsAny<string>(),   // album
                    It.IsAny<int?>(),     // durationSeconds
                    It.IsAny<IEnumerable<string>>(), // artistNames
                    spotifyId,            // spotifyId
                    It.IsAny<string>()    // spotifyUri
                ))
                .ReturnsAsync(new Song
                {
                    Id = songId,  // Same ID as the song already in playlist
                    Title = "Existing Song",
                    Album = "Album",
                    SpotifyId = spotifyId,
                    SpotifyUri = spotifyUri,
                    Artists = new List<Artist>()
                });

            var service = new SongService(mockSongRepo.Object, mockPlaylistRepo.Object, mockSpotifyService.Object);

            var dto = new AddSongToPlaylistDto
            {
                PlaylistId = playlistId,
                SpotifyId = spotifyId,
                SpotifyUri = spotifyUri,
                Title = "Existing Song",
                ArtistNames = new List<string> { "Artist 1" },
                Album = "Album",
                Url = null,
                DurationMs = 180000
            };

            // --- ACT ---
            var (success, error, returnedSongId) = await service.AddSongToPlaylistAsync(dto);

            // --- ASSERT ---
            Assert.False(success);
            Assert.Equal("This song is already in the playlist.", error);
            Assert.Null(returnedSongId);

            // Verify AddPlaylistSongAsync was NEVER called (song already exists)
            mockPlaylistRepo.Verify(
                x => x.AddPlaylistSongAsync(It.IsAny<PlaylistSong>()),
                Times.Never
            );
        }
        
        [Fact]
        public async Task GetAllAsync_ShouldReturnAllSongs()
        {
            // --- ARRANGE ---
            var mockSongRepo = new Mock<ISongRepository>();
            var mockPlaylistRepo = new Mock<IPlaylistRepository>();

            var songs = new List<Song>
            {
                new Song
                {
                    Id = 1,
                    Title = "Song 1",
                    Album = "Album 1",
                    SpotifyId = "spotify:track:song1",
                    SpotifyUri = "spotify:track:song1",
                    DurationSeconds = 180,
                    Artists = new List<Artist>
                    {
                        new Artist { Id = 1, Name = "Artist 1" }
                    }
                },
                new Song
                {
                    Id = 2,
                    Title = "Song 2",
                    Album = "Album 2",
                    SpotifyId = "spotify:track:song2",
                    SpotifyUri = "spotify:track:song2",
                    DurationSeconds = 200,
                    Artists = new List<Artist>()
                }
            };

            mockSongRepo.Setup(x => x.GetAllAsync()).ReturnsAsync(songs);

            var mockSpotifyService = new Mock<ISpotifyService>();
            var service = new SongService(mockSongRepo.Object, mockPlaylistRepo.Object, mockSpotifyService.Object);

            // --- ACT ---
            var result = await service.GetAllAsync();

            // --- ASSERT ---
            Assert.NotNull(result);
            Assert.Equal(2, result.Count());
            Assert.Equal("Song 1", result.First().Title);
        }
        
        [Fact]
        public async Task GetByIdAsync_WithValidId_ShouldReturnSong()
        {
            // --- ARRANGE ---
            var mockSongRepo = new Mock<ISongRepository>();
            var mockPlaylistRepo = new Mock<IPlaylistRepository>();

            var id = 1;

            mockSongRepo.Setup(x => x.GetByIdAsync(id))
                .ReturnsAsync(new Song
                {
                    Id = id,
                    Title = "Test Song",
                    Album = "Test Album",
                    SpotifyId = "spotify:track:test1",
                    SpotifyUri = "spotify:track:test1",
                    DurationSeconds = 180,
                    Artists = new List<Artist>
                    {
                        new Artist { Id = 1, Name = "Test Artist" }
                    }
                });

            var mockSpotifyService = new Mock<ISpotifyService>();
            var service = new SongService(mockSongRepo.Object, mockPlaylistRepo.Object, mockSpotifyService.Object);

            // --- ACT ---
            var result = await service.GetByIdAsync(id);

            // --- ASSERT ---
            Assert.NotNull(result);
            Assert.Equal("Test Song", result.Title);
            Assert.Equal("Test Album", result.Album);
            Assert.Single(result.Artists);
        }
        
        [Fact]
        public async Task GetByIdAsync_WithNonExistentId_ShouldReturnNull()
        {
            // --- ARRANGE ---
            var mockSongRepo = new Mock<ISongRepository>();
            var mockPlaylistRepo = new Mock<IPlaylistRepository>();

            var id = 999;

            mockSongRepo.Setup(x => x.GetByIdAsync(id))
                .ReturnsAsync((Song?)null);

            var mockSpotifyService = new Mock<ISpotifyService>();
            var service = new SongService(mockSongRepo.Object, mockPlaylistRepo.Object, mockSpotifyService.Object);

            // --- ACT ---
            var result = await service.GetByIdAsync(id);

            // --- ASSERT ---
            Assert.Null(result);
        }
        
        [Fact]
        public async Task AddSongToPlaylistAsync_WithNonExistentPlaylist_ShouldReturnError()
        {
            // --- ARRANGE ---
            var mockSongRepo = new Mock<ISongRepository>();
            var mockPlaylistRepo = new Mock<IPlaylistRepository>();

            var playlistId = 999;

            mockPlaylistRepo.Setup(x => x.GetByIdWithDetailsAsync(playlistId))
                .ReturnsAsync((Playlist?)null);

            var mockSpotifyService = new Mock<ISpotifyService>();
            var service = new SongService(mockSongRepo.Object, mockPlaylistRepo.Object, mockSpotifyService.Object);

            var dto = new AddSongToPlaylistDto
            {
                PlaylistId = playlistId,
                SpotifyId = "spotify:track:test999",
                SpotifyUri = "spotify:track:test999",
                Title = "Test Song",
                ArtistNames = new List<string> { "Artist" },
                Album = null,
                Url = null,
                DurationMs = null
            };

            // --- ACT ---
            var (success, error, songId) = await service.AddSongToPlaylistAsync(dto);

            // --- ASSERT ---
            Assert.False(success);
            Assert.Equal($"Playlist with ID {playlistId} not found.", error);
            Assert.Null(songId);
        }

    }
}
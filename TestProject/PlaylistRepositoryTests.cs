using Xunit;
using Microsoft.EntityFrameworkCore;
using MyApi.Data;
using MyApi.Models;
using MyApi.Repositories;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace TestProject
{
    public class PlaylistRepositoryTests
    {
        // Helper method to create an in-memory database context
        private PlaylistAppContext CreateDbContext()
        {
            var options = new DbContextOptionsBuilder<PlaylistAppContext>()
                .UseInMemoryDatabase(databaseName: System.Guid.NewGuid().ToString())
                .Options;

            return new PlaylistAppContext(options);
        }

        [Fact]
        public async Task GetAllAsync_ShouldReturnAllPlaylists_WithRelatedData()
        {
            // --- ARRANGE ---
            using var context = CreateDbContext();
            var repo = new PlaylistRepository(context);

            var host = new User { Id = 1, Username = "host", PasswordHash = "hashed", Role = UserRole.Host };
            var song = new Song
            {
                Id = 1,
                Title = "Test Song",
                SpotifyId = "spotify:track:test1",
                SpotifyUri = "spotify:track:test1"
            };
            var artist = new Artist { Id = 1, Name = "Test Artist" };
            song.Artists = new List<Artist> { artist };

            var playlist = new Playlist
            {
                Id = 1,
                Name = "Test Playlist",
                Description = "Test",
                HostId = 1,
                Host = host
            };

            context.Users.Add(host);
            context.Songs.Add(song);
            context.Playlists.Add(playlist);
            await context.SaveChangesAsync();

            var playlistSong = new PlaylistSong { PlaylistId = 1, SongId = 1 };
            context.PlaylistSongs.Add(playlistSong);
            await context.SaveChangesAsync();

            // --- ACT ---
            var result = await repo.GetAllAsync();

            // --- ASSERT ---
            Assert.NotNull(result);
            Assert.Single(result);
            var firstPlaylist = result.First();
            Assert.Equal("Test Playlist", firstPlaylist.Name);
            Assert.NotNull(firstPlaylist.Host);
            Assert.NotNull(firstPlaylist.PlaylistSongs);
            Assert.Single(firstPlaylist.PlaylistSongs);
        }

        [Fact]
        public async Task GetByIdAsync_WithExistingId_ShouldReturnPlaylist()
        {
            // --- ARRANGE ---
            using var context = CreateDbContext();
            var repo = new PlaylistRepository(context);

            var playlist = new Playlist
            {
                Id = 1,
                Name = "Test Playlist",
                Description = "Test",
                HostId = 1
            };

            context.Playlists.Add(playlist);
            await context.SaveChangesAsync();

            // --- ACT ---
            var result = await repo.GetByIdAsync(1);

            // --- ASSERT ---
            Assert.NotNull(result);
            Assert.Equal(1, result.Id);
            Assert.Equal("Test Playlist", result.Name);
        }

        [Fact]
        public async Task GetByIdAsync_WithNonExistentId_ShouldReturnNull()
        {
            // --- ARRANGE ---
            using var context = CreateDbContext();
            var repo = new PlaylistRepository(context);

            // --- ACT ---
            var result = await repo.GetByIdAsync(999);

            // --- ASSERT ---
            Assert.Null(result);
        }

        [Fact]
        public async Task GetByIdWithDetailsAsync_ShouldIncludeAllRelations()
        {
            // --- ARRANGE ---
            using var context = CreateDbContext();
            var repo = new PlaylistRepository(context);

            var host = new User { Id = 1, Username = "host", PasswordHash = "hashed", Role = UserRole.Host };
            var user = new User { Id = 2, Username = "user", PasswordHash = "hashed", Role = UserRole.Guest };

            var song = new Song
            {
                Id = 1,
                Title = "Test Song",
                SpotifyId = "spotify:track:test1",
                SpotifyUri = "spotify:track:test1"
            };
            var artist = new Artist { Id = 1, Name = "Test Artist" };
            song.Artists = new List<Artist> { artist };

            var playlist = new Playlist
            {
                Id = 1,
                Name = "Test Playlist",
                Description = "Test",
                HostId = 1,
                Host = host,
                Users = new List<User> { user }
            };

            context.Users.AddRange(host, user);
            context.Songs.Add(song);
            context.Playlists.Add(playlist);
            await context.SaveChangesAsync();

            var playlistSong = new PlaylistSong { PlaylistId = 1, SongId = 1 };
            context.PlaylistSongs.Add(playlistSong);
            await context.SaveChangesAsync();

            // --- ACT ---
            var result = await repo.GetByIdWithDetailsAsync(1);

            // --- ASSERT ---
            Assert.NotNull(result);
            Assert.NotNull(result.Host);
            Assert.Equal("host", result.Host.Username);
            Assert.NotNull(result.PlaylistSongs);
            Assert.Single(result.PlaylistSongs);
            Assert.NotNull(result.PlaylistSongs.First().Song);
            Assert.NotNull(result.PlaylistSongs.First().Song.Artists);
            Assert.Single(result.Users);
        }

        [Fact]
        public async Task AddAsync_ShouldAddPlaylistToDatabase()
        {
            // --- ARRANGE ---
            using var context = CreateDbContext();
            var repo = new PlaylistRepository(context);

            var playlist = new Playlist
            {
                Name = "New Playlist",
                Description = "Test",
                HostId = 1
            };

            // --- ACT ---
            await repo.AddAsync(playlist);

            // --- ASSERT ---
            var saved = await context.Playlists.FirstOrDefaultAsync();
            Assert.NotNull(saved);
            Assert.Equal("New Playlist", saved.Name);
        }

        [Fact]
        public async Task UpdateAsync_ShouldModifyExistingPlaylist()
        {
            // --- ARRANGE ---
            using var context = CreateDbContext();
            var repo = new PlaylistRepository(context);

            var playlist = new Playlist
            {
                Name = "Original Name",
                Description = "Original",
                HostId = 1
            };

            context.Playlists.Add(playlist);
            await context.SaveChangesAsync();

            // --- ACT ---
            playlist.Name = "Updated Name";
            playlist.Description = "Updated";
            await repo.UpdateAsync(playlist);

            // --- ASSERT ---
            var updated = await context.Playlists.FirstOrDefaultAsync();
            Assert.NotNull(updated);
            Assert.Equal("Updated Name", updated.Name);
            Assert.Equal("Updated", updated.Description);
        }

        [Fact]
        public async Task DeleteAsync_ShouldRemovePlaylistFromDatabase()
        {
            // --- ARRANGE ---
            using var context = CreateDbContext();
            var repo = new PlaylistRepository(context);

            var playlist = new Playlist
            {
                Name = "To Delete",
                Description = "Test",
                HostId = 1
            };

            context.Playlists.Add(playlist);
            await context.SaveChangesAsync();

            // --- ACT ---
            await repo.DeleteAsync(playlist);

            // --- ASSERT ---
            var deleted = await context.Playlists.FirstOrDefaultAsync();
            Assert.Null(deleted);
        }

        [Fact]
        public async Task ExistsAsync_WithExistingId_ShouldReturnTrue()
        {
            // --- ARRANGE ---
            using var context = CreateDbContext();
            var repo = new PlaylistRepository(context);

            var playlist = new Playlist
            {
                Id = 1,
                Name = "Test",
                Description = "Test",
                HostId = 1
            };

            context.Playlists.Add(playlist);
            await context.SaveChangesAsync();

            // --- ACT ---
            var result = await repo.ExistsAsync(1);

            // --- ASSERT ---
            Assert.True(result);
        }

        [Fact]
        public async Task ExistsAsync_WithNonExistentId_ShouldReturnFalse()
        {
            // --- ARRANGE ---
            using var context = CreateDbContext();
            var repo = new PlaylistRepository(context);

            // --- ACT ---
            var result = await repo.ExistsAsync(999);

            // --- ASSERT ---
            Assert.False(result);
        }

        [Fact]
        public async Task ExistsByNameAsync_WithExistingName_ShouldReturnTrue()
        {
            // --- ARRANGE ---
            using var context = CreateDbContext();
            var repo = new PlaylistRepository(context);

            var playlist = new Playlist
            {
                Name = "Unique Name",
                Description = "Test",
                HostId = 1
            };

            context.Playlists.Add(playlist);
            await context.SaveChangesAsync();

            // --- ACT ---
            var result = await repo.ExistsByNameAsync("Unique Name");

            // --- ASSERT ---
            Assert.True(result);
        }

        [Fact]
        public async Task ExistsByNameAsync_WithNonExistentName_ShouldReturnFalse()
        {
            // --- ARRANGE ---
            using var context = CreateDbContext();
            var repo = new PlaylistRepository(context);

            // --- ACT ---
            var result = await repo.ExistsByNameAsync("Nonexistent Name");

            // --- ASSERT ---
            Assert.False(result);
        }

        [Fact]
        public async Task GetPlaylistSongAsync_WithExistingRelation_ShouldReturnPlaylistSong()
        {
            // --- ARRANGE ---
            using var context = CreateDbContext();
            var repo = new PlaylistRepository(context);

            var playlistSong = new PlaylistSong { PlaylistId = 1, SongId = 1 };
            context.PlaylistSongs.Add(playlistSong);
            await context.SaveChangesAsync();

            // --- ACT ---
            var result = await repo.GetPlaylistSongAsync(1, 1);

            // --- ASSERT ---
            Assert.NotNull(result);
            Assert.Equal(1, result.PlaylistId);
            Assert.Equal(1, result.SongId);
        }

        [Fact]
        public async Task GetPlaylistSongAsync_WithNonExistentRelation_ShouldReturnNull()
        {
            // --- ARRANGE ---
            using var context = CreateDbContext();
            var repo = new PlaylistRepository(context);

            // --- ACT ---
            var result = await repo.GetPlaylistSongAsync(999, 999);

            // --- ASSERT ---
            Assert.Null(result);
        }

        [Fact]
        public async Task AddPlaylistSongAsync_ShouldAddRelationToDatabase()
        {
            // --- ARRANGE ---
            using var context = CreateDbContext();
            var repo = new PlaylistRepository(context);

            var playlistSong = new PlaylistSong { PlaylistId = 1, SongId = 1 };

            // --- ACT ---
            await repo.AddPlaylistSongAsync(playlistSong);

            // --- ASSERT ---
            var saved = await context.PlaylistSongs.FirstOrDefaultAsync();
            Assert.NotNull(saved);
            Assert.Equal(1, saved.PlaylistId);
            Assert.Equal(1, saved.SongId);
        }

        [Fact]
        public async Task RemovePlaylistSongAsync_ShouldRemoveRelationFromDatabase()
        {
            // --- ARRANGE ---
            using var context = CreateDbContext();
            var repo = new PlaylistRepository(context);

            var playlistSong = new PlaylistSong { PlaylistId = 1, SongId = 1 };
            context.PlaylistSongs.Add(playlistSong);
            await context.SaveChangesAsync();

            // --- ACT ---
            await repo.RemovePlaylistSongAsync(playlistSong);

            // --- ASSERT ---
            var deleted = await context.PlaylistSongs.FirstOrDefaultAsync();
            Assert.Null(deleted);
        }

        [Fact]
        public async Task RemovePlaylistSongsRangeAsync_ShouldRemoveMultipleRelations()
        {
            // --- ARRANGE ---
            using var context = CreateDbContext();
            var repo = new PlaylistRepository(context);

            var playlistSongs = new List<PlaylistSong>
            {
                new PlaylistSong { PlaylistId = 1, SongId = 1 },
                new PlaylistSong { PlaylistId = 1, SongId = 2 },
                new PlaylistSong { PlaylistId = 1, SongId = 3 }
            };

            context.PlaylistSongs.AddRange(playlistSongs);
            await context.SaveChangesAsync();

            // --- ACT ---
            await repo.RemovePlaylistSongsRangeAsync(playlistSongs);

            // --- ASSERT ---
            var remaining = await context.PlaylistSongs.ToListAsync();
            Assert.Empty(remaining);
        }

        [Fact]
        public async Task GetUserByIdAsync_WithExistingUser_ShouldReturnUser()
        {
            // --- ARRANGE ---
            using var context = CreateDbContext();
            var repo = new PlaylistRepository(context);

            var user = new User
            {
                Id = 1,
                Username = "testuser",
                PasswordHash = "hashed",
                Role = UserRole.Guest
            };

            context.Users.Add(user);
            await context.SaveChangesAsync();

            // --- ACT ---
            var result = await repo.GetUserByIdAsync(1);

            // --- ASSERT ---
            Assert.NotNull(result);
            Assert.Equal("testuser", result.Username);
        }

        [Fact]
        public async Task GetUserByIdAsync_WithNonExistentUser_ShouldReturnNull()
        {
            // --- ARRANGE ---
            using var context = CreateDbContext();
            var repo = new PlaylistRepository(context);

            // --- ACT ---
            var result = await repo.GetUserByIdAsync(999);

            // --- ASSERT ---
            Assert.Null(result);
        }

        [Fact]
        public async Task SongExistsAsync_WithExistingSong_ShouldReturnTrue()
        {
            // --- ARRANGE ---
            using var context = CreateDbContext();
            var repo = new PlaylistRepository(context);

            var song = new Song
            {
                Id = 1,
                Title = "Test Song",
                SpotifyId = "spotify:track:test1",
                SpotifyUri = "spotify:track:test1"
            };
            context.Songs.Add(song);
            await context.SaveChangesAsync();

            // --- ACT ---
            var result = await repo.SongExistsAsync(1);

            // --- ASSERT ---
            Assert.True(result);
        }

        [Fact]
        public async Task SongExistsAsync_WithNonExistentSong_ShouldReturnFalse()
        {
            // --- ARRANGE ---
            using var context = CreateDbContext();
            var repo = new PlaylistRepository(context);

            // --- ACT ---
            var result = await repo.SongExistsAsync(999);

            // --- ASSERT ---
            Assert.False(result);
        }
    }
}
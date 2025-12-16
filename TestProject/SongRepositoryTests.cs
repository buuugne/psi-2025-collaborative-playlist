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
    public class SongRepositoryTests
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
        public async Task GetAllAsync_ShouldReturnAllSongs_WithArtists()
        {
            // --- ARRANGE ---
            using var context = CreateDbContext();
            var repo = new SongRepository(context);

            var artist1 = new Artist { Id = 1, Name = "Artist 1" };
            var artist2 = new Artist { Id = 2, Name = "Artist 2" };

            var song1 = new Song
            {
                Id = 1,
                Title = "Song 1",
                Album = "Album 1",
                DurationSeconds = 180,
                SpotifyId = "spotify:track:song1",
                SpotifyUri = "spotify:track:song1",
                Artists = new List<Artist> { artist1 }
            };

            var song2 = new Song
            {
                Id = 2,
                Title = "Song 2",
                Album = "Album 2",
                DurationSeconds = 200,
                SpotifyId = "spotify:track:song2",
                SpotifyUri = "spotify:track:song2",
                Artists = new List<Artist> { artist2 }
            };

            context.Songs.AddRange(song1, song2);
            await context.SaveChangesAsync();

            // --- ACT ---
            var result = await repo.GetAllAsync();

            // --- ASSERT ---
            Assert.NotNull(result);
            Assert.Equal(2, result.Count());
            Assert.All(result, song => Assert.NotNull(song.Artists));
        }

        [Fact]
        public async Task GetAllAsync_WithNoSongs_ShouldReturnEmptyList()
        {
            // --- ARRANGE ---
            using var context = CreateDbContext();
            var repo = new SongRepository(context);

            // --- ACT ---
            var result = await repo.GetAllAsync();

            // --- ASSERT ---
            Assert.NotNull(result);
            Assert.Empty(result);
        }

        [Fact]
        public async Task GetByIdAsync_WithExistingId_ShouldReturnSongWithArtists()
        {
            // --- ARRANGE ---
            using var context = CreateDbContext();
            var repo = new SongRepository(context);

            var artist = new Artist { Id = 1, Name = "Test Artist" };
            var song = new Song
            {
                Id = 1,
                Title = "Test Song",
                Album = "Test Album",
                DurationSeconds = 180,
                SpotifyId = "spotify:track:test1",
                SpotifyUri = "spotify:track:test1",
                Artists = new List<Artist> { artist }
            };

            context.Songs.Add(song);
            await context.SaveChangesAsync();

            // --- ACT ---
            var result = await repo.GetByIdAsync(1);

            // --- ASSERT ---
            Assert.NotNull(result);
            Assert.Equal("Test Song", result.Title);
            Assert.NotNull(result.Artists);
            Assert.Single(result.Artists);
            Assert.Equal("Test Artist", result.Artists.First().Name);
        }

        [Fact]
        public async Task GetByIdAsync_WithNonExistentId_ShouldReturnNull()
        {
            // --- ARRANGE ---
            using var context = CreateDbContext();
            var repo = new SongRepository(context);

            // --- ACT ---
            var result = await repo.GetByIdAsync(999);

            // --- ASSERT ---
            Assert.Null(result);
        }

        [Fact]
        public async Task ExistsAsync_WithExistingId_ShouldReturnTrue()
        {
            // --- ARRANGE ---
            using var context = CreateDbContext();
            var repo = new SongRepository(context);

            var song = new Song
            {
                Id = 1,
                Title = "Test Song",
                DurationSeconds = 180,
                SpotifyId = "spotify:track:test1",
                SpotifyUri = "spotify:track:test1"
            };

            context.Songs.Add(song);
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
            var repo = new SongRepository(context);

            // --- ACT ---
            var result = await repo.ExistsAsync(999);

            // --- ASSERT ---
            Assert.False(result);
        }

        [Fact]
        public async Task AddAsync_ShouldAddSongToDatabase()
        {
            // --- ARRANGE ---
            using var context = CreateDbContext();
            var repo = new SongRepository(context);

            var song = new Song
            {
                Title = "New Song",
                Album = "New Album",
                DurationSeconds = 200,
                SpotifyId = "spotify:track:new1",
                SpotifyUri = "spotify:track:new1"
            };

            // --- ACT ---
            await repo.AddAsync(song);

            // --- ASSERT ---
            var saved = await context.Songs.FirstOrDefaultAsync();
            Assert.NotNull(saved);
            Assert.Equal("New Song", saved.Title);
            Assert.Equal("New Album", saved.Album);
        }

        [Fact]
        public async Task UpdateAsync_ShouldModifyExistingSong()
        {
            // --- ARRANGE ---
            using var context = CreateDbContext();
            var repo = new SongRepository(context);

            var song = new Song
            {
                Title = "Original Title",
                Album = "Original Album",
                DurationSeconds = 180,
                SpotifyId = "spotify:track:original",
                SpotifyUri = "spotify:track:original"
            };

            context.Songs.Add(song);
            await context.SaveChangesAsync();

            // --- ACT ---
            song.Title = "Updated Title";
            song.Album = "Updated Album";
            await repo.UpdateAsync(song);

            // --- ASSERT ---
            var updated = await context.Songs.FirstOrDefaultAsync();
            Assert.NotNull(updated);
            Assert.Equal("Updated Title", updated.Title);
            Assert.Equal("Updated Album", updated.Album);
        }

        [Fact]
        public async Task DeleteAsync_ShouldRemoveSongFromDatabase()
        {
            // --- ARRANGE ---
            using var context = CreateDbContext();
            var repo = new SongRepository(context);

            var song = new Song
            {
                Title = "To Delete",
                Album = "Test Album",
                DurationSeconds = 180,
                SpotifyId = "spotify:track:delete1",
                SpotifyUri = "spotify:track:delete1"
            };

            context.Songs.Add(song);
            await context.SaveChangesAsync();

            // --- ACT ---
            await repo.DeleteAsync(song);

            // --- ASSERT ---
            var deleted = await context.Songs.FirstOrDefaultAsync();
            Assert.Null(deleted);
        }

        [Fact]
        public async Task FindByTitleAndAlbumAsync_WithExistingSong_ShouldReturnSong()
        {
            // --- ARRANGE ---
            using var context = CreateDbContext();
            var repo = new SongRepository(context);

            var artist = new Artist { Id = 1, Name = "Test Artist" };
            var song = new Song
            {
                Title = "Unique Song",
                Album = "Unique Album",
                DurationSeconds = 180,
                SpotifyId = "spotify:track:unique1",
                SpotifyUri = "spotify:track:unique1",
                Artists = new List<Artist> { artist }
            };

            context.Songs.Add(song);
            await context.SaveChangesAsync();

            // --- ACT ---
            var result = await repo.FindByTitleAndAlbumAsync("Unique Song", "Unique Album");

            // --- ASSERT ---
            Assert.NotNull(result);
            Assert.Equal("Unique Song", result.Title);
            Assert.Equal("Unique Album", result.Album);
            Assert.NotNull(result.Artists);
        }

        [Fact]
        public async Task FindByTitleAndAlbumAsync_WithNonExistentSong_ShouldReturnNull()
        {
            // --- ARRANGE ---
            using var context = CreateDbContext();
            var repo = new SongRepository(context);

            // --- ACT ---
            var result = await repo.FindByTitleAndAlbumAsync("Nonexistent", "Album");

            // --- ASSERT ---
            Assert.Null(result);
        }

        [Fact]
        public async Task FindByTitleAndAlbumAsync_WithNullAlbum_ShouldFindSongWithNullAlbum()
        {
            // --- ARRANGE ---
            using var context = CreateDbContext();
            var repo = new SongRepository(context);

            var song = new Song
            {
                Title = "Song Without Album",
                Album = null,
                DurationSeconds = 180,
                SpotifyId = "spotify:track:noalbum1",
                SpotifyUri = "spotify:track:noalbum1"
            };

            context.Songs.Add(song);
            await context.SaveChangesAsync();

            // --- ACT ---
            var result = await repo.FindByTitleAndAlbumAsync("Song Without Album", null);

            // --- ASSERT ---
            Assert.NotNull(result);
            Assert.Equal("Song Without Album", result.Title);
            Assert.Null(result.Album);
        }

        [Fact]
        public async Task EnsureSongWithArtistsAsync_WithNewSong_ShouldCreateSongAndArtists()
        {
            // --- ARRANGE ---
            using var context = CreateDbContext();
            var repo = new SongRepository(context);

            var artistNames = new List<string> { "Artist 1", "Artist 2" };

            // --- ACT ---
            var result = await repo.EnsureSongWithArtistsAsync(
                title: "New Song",
                album: "New Album",
                durationSeconds: 180,
                artistNames: artistNames,
                spotifyId: "spotify:track:new123",
                spotifyUri: "spotify:track:new123"
            );

            // --- ASSERT ---
            Assert.NotNull(result);
            Assert.Equal("New Song", result.Title);
            Assert.Equal("New Album", result.Album);
            Assert.Equal(180, result.DurationSeconds);
            Assert.Equal(2, result.Artists.Count);

            // Verify artists were created in database
            var dbArtists = await context.Artists.ToListAsync();
            Assert.Equal(2, dbArtists.Count);
            Assert.Contains(dbArtists, a => a.Name == "Artist 1");
            Assert.Contains(dbArtists, a => a.Name == "Artist 2");
        }

        [Fact]
        public async Task EnsureSongWithArtistsAsync_WithExistingSong_ShouldReturnExistingSong()
        {
            // --- ARRANGE ---
            using var context = CreateDbContext();
            var repo = new SongRepository(context);

            var artist = new Artist { Id = 1, Name = "Existing Artist" };
            var existingSong = new Song
            {
                Title = "Existing Song",
                Album = "Existing Album",
                DurationSeconds = 200,
                SpotifyId = "spotify:track:existing1",
                SpotifyUri = "spotify:track:existing1",
                Artists = new List<Artist> { artist }
            };

            context.Songs.Add(existingSong);
            await context.SaveChangesAsync();

            var originalSongCount = await context.Songs.CountAsync();

            // --- ACT ---
            var result = await repo.EnsureSongWithArtistsAsync(
                title: "Updated Title",
                album: "Updated Album",
                durationSeconds: 200,
                artistNames: new List<string> { "New Artist" },
                spotifyId: "spotify:track:existing1",
                spotifyUri: "spotify:track:existing1"
            );

            // --- ASSERT ---
            Assert.NotNull(result);
            Assert.Equal(existingSong.Id, result.Id);
            Assert.Equal("Updated Title", result.Title); // Should update title

            // Verify no new song was created
            var finalSongCount = await context.Songs.CountAsync();
            Assert.Equal(originalSongCount, finalSongCount);
        }

        [Fact]
        public async Task EnsureSongWithArtistsAsync_WithExistingArtists_ShouldReuseArtists()
        {
            // --- ARRANGE ---
            using var context = CreateDbContext();
            var repo = new SongRepository(context);

            // Add existing artist to database
            var existingArtist = new Artist { Id = 1, Name = "Existing Artist" };
            context.Artists.Add(existingArtist);
            await context.SaveChangesAsync();

            var artistNames = new List<string> { "Existing Artist", "New Artist" };

            // --- ACT ---
            var result = await repo.EnsureSongWithArtistsAsync(
                title: "Song Title",
                album: "Album",
                durationSeconds: 180,
                artistNames: artistNames,
                spotifyId: "spotify:track:reuse1",
                spotifyUri: "spotify:track:reuse1"
            );

            // --- ASSERT ---
            Assert.NotNull(result);
            Assert.Equal(2, result.Artists.Count);

            // Verify only one new artist was created (not two)
            var dbArtists = await context.Artists.ToListAsync();
            Assert.Equal(2, dbArtists.Count);
            Assert.Contains(dbArtists, a => a.Name == "Existing Artist");
            Assert.Contains(dbArtists, a => a.Name == "New Artist");
        }

        [Fact]
        public async Task EnsureSongWithArtistsAsync_WithDuplicateArtistNames_ShouldCreateOnlyUniqueArtists()
        {
            // --- ARRANGE ---
            using var context = CreateDbContext();
            var repo = new SongRepository(context);

            var artistNames = new List<string> { "Artist 1", "artist 1", "ARTIST 1", "Artist 2" };

            // --- ACT ---
            var result = await repo.EnsureSongWithArtistsAsync(
                title: "Test Song",
                album: "Test Album",
                durationSeconds: 180,
                artistNames: artistNames,
                spotifyId: "spotify:track:dupes1",
                spotifyUri: "spotify:track:dupes1"
            );

            // --- ASSERT ---
            Assert.NotNull(result);
            // Should only have 2 unique artists (case-insensitive)
            Assert.Equal(2, result.Artists.Count);

            var dbArtists = await context.Artists.ToListAsync();
            Assert.Equal(2, dbArtists.Count);
        }

        [Fact]
        public async Task EnsureSongWithArtistsAsync_WithEmptyOrWhitespaceArtists_ShouldIgnoreThem()
        {
            // --- ARRANGE ---
            using var context = CreateDbContext();
            var repo = new SongRepository(context);

            var artistNames = new List<string> { "Valid Artist", "", "  ", null! };

            // --- ACT ---
            var result = await repo.EnsureSongWithArtistsAsync(
                title: "Test Song",
                album: "Test Album",
                durationSeconds: 180,
                artistNames: artistNames,
                spotifyId: "spotify:track:test5",
                spotifyUri: "spotify:track:test5"
            );

            // --- ASSERT ---
            Assert.NotNull(result);
            // Should only have 1 artist (the valid one)
            Assert.Single(result.Artists);
            Assert.Equal("Valid Artist", result.Artists.First().Name);
        }

        [Fact]
        public async Task EnsureSongWithArtistsAsync_WithTrimmedArtistNames_ShouldTrimWhitespace()
        {
            // --- ARRANGE ---
            using var context = CreateDbContext();
            var repo = new SongRepository(context);

            var artistNames = new List<string> { "  Artist 1  ", "Artist 2   " };

            // --- ACT ---
            var result = await repo.EnsureSongWithArtistsAsync(
                title: "New Song",
                album: "New Album",
                durationSeconds: 180,
                artistNames: artistNames,
                spotifyId: "spotify:track:trim1",
                spotifyUri: "spotify:track:trim1"
            );

            // --- ASSERT ---
            Assert.NotNull(result);
            Assert.Equal(2, result.Artists.Count);

            var dbArtists = await context.Artists.ToListAsync();
            Assert.Contains(dbArtists, a => a.Name == "Artist 1");
            Assert.Contains(dbArtists, a => a.Name == "Artist 2");
        }

        [Fact]
        public async Task EnsureSongWithArtistsAsync_WithNullDuration_ShouldCreateSongWithNullDuration()
        {
            // --- ARRANGE ---
            using var context = CreateDbContext();
            var repo = new SongRepository(context);

            var artistNames = new List<string> { "Artist 1" };

            // --- ACT ---
            var result = await repo.EnsureSongWithArtistsAsync(
                title: "Song Without Duration",
                album: "Album",
                durationSeconds: null,
                artistNames: artistNames,
                spotifyId: "spotify:track:noduration1",
                spotifyUri: "spotify:track:noduration1"
            );

            // --- ASSERT ---
            Assert.NotNull(result);
            Assert.Equal("Song Without Duration", result.Title);
            Assert.Null(result.DurationSeconds);
        }
    }
}
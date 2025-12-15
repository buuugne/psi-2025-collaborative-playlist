using Xunit;
using Moq;
using MyApi.Services;
using MyApi.Repositories;
using MyApi.Models;
using MyApi.Dtos;
using System.Threading.Tasks;
using System.Collections.Generic;
using System.Linq;


namespace TestProject
{
    public class PlaylisServiceTests
    {

        [Fact]
        public async Task CreateAsync_ShouldFail_WhenPlaylistNameAlreadyExists()
        {
            // --- ARRANGE ---
            var mockPlaylistRepo = new Mock<IPlaylistRepository>();
            var mockUserRepo = new Mock<IUserRepository>();
            var mockSongRepo = new Mock<ISongRepository>();

            mockPlaylistRepo.Setup(x => x.ExistsByNameAsync("Whatever")).ReturnsAsync(true);

            var service = new PlaylistService(
            mockPlaylistRepo.Object,
            mockUserRepo.Object,
            mockSongRepo.Object
            );

            var dto = new PlaylistCreateDto
            {
                Name = "Whatever",
                Description = "test",
                HostId = 1
            };

            // --- ACT ---
            var (success, error, created) = await service.CreateAsync(dto);

            // --- ASSERT ---
            Assert.False(success);
            Assert.Equal($"A playlist named '{dto.Name}' already exists.", error);
            Assert.Null(created);

        }

        [Fact]
        public async Task CreateAsync_ShouldSucceed_WhenPlaylistNameDoesntExist()
        {
            // --- ARRANGE ---
            var mockPlaylistRepo = new Mock<IPlaylistRepository>();
            var mockUserRepo = new Mock<IUserRepository>();
            var mockSongRepo = new Mock<ISongRepository>();

            //host exists and is admin/host
            mockUserRepo.Setup(x => x.GetByIdAsync(1))
            .ReturnsAsync(new User { Id = 1, Username = "u", Role = UserRole.Host }); //its a get, so it returns sth (user)

            mockPlaylistRepo.Setup(x => x.AddAsync(It.IsAny<Playlist>()))
            .Returns(Task.CompletedTask);

            mockPlaylistRepo
            .Setup(x => x.GetByIdWithDetailsAsync(It.IsAny<int>()))
            .ReturnsAsync(new Playlist
            {
                Id = 1,
                Name = "Whatever",
                Description = "test",
                HostId = 1,
                Host = new User { Id = 1, Username = "u", Role = UserRole.Host },
                PlaylistSongs = new List<PlaylistSong>(),
                Users = new List<User>()
            });


            var service = new PlaylistService(
            mockPlaylistRepo.Object,
            mockUserRepo.Object,
            mockSongRepo.Object
            );

            var dto = new PlaylistCreateDto
            {
                Name = "Whatever",
                Description = "test",
                HostId = 1
            };

            // --- ACT ---
            var (success, error, created) = await service.CreateAsync(dto);

            // --- ASSERT ---
            Assert.True(success);
            Assert.Null(error);
            Assert.NotNull(created);

        }

        [Fact]
        public async Task CreateAsync_ShouldFail_WhenRoleNotAuthorized()
        {
            // --- ARRANGE ---
            var mockPlaylistRepo = new Mock<IPlaylistRepository>();

            // Playlist name does NOT exist
            mockPlaylistRepo.Setup(x => x.ExistsByNameAsync("Whatever"))
                .ReturnsAsync(false);

            var mockUserRepo = new Mock<IUserRepository>();

            // User exists but is a GUEST (not allowed)
            mockUserRepo.Setup(x => x.GetByIdAsync(1))
                .ReturnsAsync(new User
                {
                    Id = 1,
                    Username = "u",
                    Role = UserRole.Guest
                });

            var mockSongRepo = new Mock<ISongRepository>();

            var service = new PlaylistService(
                mockPlaylistRepo.Object,
                mockUserRepo.Object,
                mockSongRepo.Object
            );

            var dto = new PlaylistCreateDto
            {
                Name = "Whatever",
                Description = "test",
                HostId = 1
            };

            // --- ACT ---
            var (success, error, created) = await service.CreateAsync(dto);

            // --- ASSERT ---
            Assert.False(success);
            Assert.Equal("User not authorized to create playlists.", error);
            Assert.Null(created);

            // AddAsync must NOT be called
            mockPlaylistRepo.Verify(
                x => x.AddAsync(It.IsAny<Playlist>()),
                Times.Never
            );
        }

        [Fact]
        public async Task CreateAsync_WithValidHost_ShouldCreatePlaylist()
        {
            var mockPlaylistRepo = new Mock<IPlaylistRepository>();
            var mockUserRepo = new Mock<IUserRepository>();
            var mockSongRepo = new Mock<ISongRepository>();

            mockPlaylistRepo.Setup(x => x.ExistsByNameAsync(It.IsAny<string>()))
                .ReturnsAsync(false);

            mockUserRepo.Setup(x => x.GetByIdAsync(It.IsAny<int>()))
                .ReturnsAsync(new User { Id = 1, Username = "host", Role = UserRole.Host });

            mockPlaylistRepo.Setup(x => x.AddAsync(It.IsAny<Playlist>()))
                .Returns(Task.CompletedTask);

            // Mock GetByIdWithDetailsAsync to return the created playlist
            mockPlaylistRepo.Setup(x => x.GetByIdWithDetailsAsync(It.IsAny<int>()))
                .ReturnsAsync(new Playlist
                {
                    Id = 1,
                    Name = "Test",
                    Description = "test",
                    HostId = 1,
                    Host = new User { Id = 1, Username = "host", Role = UserRole.Host },
                    PlaylistSongs = new List<PlaylistSong>(),
                    Users = new List<User>()
                });

            var service = new PlaylistService(mockPlaylistRepo.Object, mockUserRepo.Object, mockSongRepo.Object);

            var dto = new PlaylistCreateDto { Name = "Test", Description = "test", HostId = 1 };

            var (success, error, created) = await service.CreateAsync(dto);

            Assert.True(success);
            Assert.Null(error);
            Assert.NotNull(created);
            Assert.Equal("Test", created.Name);

            mockPlaylistRepo.Verify(x => x.AddAsync(It.IsAny<Playlist>()), Times.Once);
        }

        [Fact]
        public async Task UpdateByIdAsync_WithNonExistentPlaylist_ShouldReturnError()
        {
            // --- ARRANGE ---
            var mockPlaylistRepo = new Mock<IPlaylistRepository>();
            var mockUserRepo = new Mock<IUserRepository>();
            var mockSongRepo = new Mock<ISongRepository>();

            var id = 1;

            // Playlist name does NOT exist
            mockPlaylistRepo.Setup(x => x.GetByIdWithDetailsAsync(id))
                .ReturnsAsync((Playlist?)null);


            var service = new PlaylistService(
                mockPlaylistRepo.Object,
                mockUserRepo.Object,
                mockSongRepo.Object
            );

            var dto = new PlaylistUpdateDto
            {
                Name = "Whatever",
                Description = "test",
                SongIds = null
            };

            // --- ACT ---
            var (success, error, updated) = await service.UpdateByIdAsync(id, dto);

            // --- ASSERT ---
            Assert.False(success);
            Assert.Equal($"Playlist with ID {id} not found.", error);
            Assert.Null(updated);

            // AddAsync must NOT be called
            mockPlaylistRepo.Verify(
                x => x.UpdateAsync(It.IsAny<Playlist>()),
                Times.Never
            );

        }
        [Fact]
        public async Task UpdateByIdAsync_WithGuestRoleHost_ShouldReturnUnauthorized()
        {
            // --- ARRANGE ---
            var mockPlaylistRepo = new Mock<IPlaylistRepository>();
            var mockUserRepo = new Mock<IUserRepository>();
            var mockSongRepo = new Mock<ISongRepository>();

            var id = 1;

            // Playlist EXISTS but has a Guest as host (not allowed)
            mockPlaylistRepo.Setup(x => x.GetByIdWithDetailsAsync(id))
                .ReturnsAsync(new Playlist
                {
                    Id = 1,
                    Name = "Existing Playlist",
                    Description = "desc",
                    HostId = 1,
                    Host = new User
                    {
                        Id = 1,
                        Username = "guest_user",
                        Role = UserRole.Guest // ← The problem!
                    },
                    PlaylistSongs = new List<PlaylistSong>(),
                    Users = new List<User>()
                });

            var service = new PlaylistService(
                mockPlaylistRepo.Object,
                mockUserRepo.Object,
                mockSongRepo.Object
            );

            var dto = new PlaylistUpdateDto
            {
                Name = "Updated Name",
                Description = "Updated desc",
                SongIds = null
            };

            // --- ACT ---
            var (success, error, updated) = await service.UpdateByIdAsync(id, dto);

            // --- ASSERT ---
            Assert.False(success);
            Assert.Equal("Only hosts or admins can update playlists.", error);
            Assert.Null(updated);

            // UpdateAsync must NOT be called
            mockPlaylistRepo.Verify(
                x => x.UpdateAsync(It.IsAny<Playlist>()),
                Times.Never
            );
        }

        [Fact]
        public async Task GetAllAsync_ShouldReturnAllPlaylists()
        {
            var mockPlaylistRepo = new Mock<IPlaylistRepository>();
            var mockUserRepo = new Mock<IUserRepository>();
            var mockSongRepo = new Mock<ISongRepository>();

            var playlists = new List<Playlist>
        {
        new Playlist
        {
            Id = 1,
            Name = "Playlist 1",
            Description = "desc1",
            HostId = 1,
            Host = new User { Id = 1, Username = "user1", Role = UserRole.Host },
            PlaylistSongs = new List<PlaylistSong>(),
            Users = new List<User>()
        },
        new Playlist
        {
            Id = 2,
            Name = "Playlist 2",
            Description = "desc2",
            HostId = 2,
            Host = new User { Id = 2, Username = "user2", Role = UserRole.Admin },
            PlaylistSongs = new List<PlaylistSong>(),
            Users = new List<User>()
        }
        };

            mockPlaylistRepo.Setup(x => x.GetAllAsync())
                .ReturnsAsync(playlists);

            var service = new PlaylistService(mockPlaylistRepo.Object, mockUserRepo.Object, mockSongRepo.Object);

            var result = await service.GetAllAsync();

            Assert.NotNull(result);
            Assert.Equal(2, result.Count());
            Assert.Equal("Playlist 1", result.First().Name);
        }

        [Fact]
        public async Task GetByIdAsync_WithValidId_ShouldReturnPlaylist()
        {
            var mockPlaylistRepo = new Mock<IPlaylistRepository>();
            var mockUserRepo = new Mock<IUserRepository>();
            var mockSongRepo = new Mock<ISongRepository>();

            mockPlaylistRepo.Setup(x => x.GetByIdWithDetailsAsync(1))
                .ReturnsAsync(new Playlist
                {
                    Id = 1,
                    Name = "Test Playlist",
                    Description = "desc",
                    HostId = 1,
                    Host = new User { Id = 1, Username = "host", Role = UserRole.Host },
                    PlaylistSongs = new List<PlaylistSong>(),
                    Users = new List<User>()
                });

            var service = new PlaylistService(mockPlaylistRepo.Object, mockUserRepo.Object, mockSongRepo.Object);

            var result = await service.GetByIdAsync(1);

            Assert.NotNull(result);
            Assert.Equal("Test Playlist", result.Name);
        }
        [Fact]
        public async Task DeleteAsync_WithValidPlaylist_ShouldDeleteSuccessfully()
        {
            var mockPlaylistRepo = new Mock<IPlaylistRepository>();
            var mockUserRepo = new Mock<IUserRepository>();
            var mockSongRepo = new Mock<ISongRepository>();

            mockPlaylistRepo.Setup(x => x.GetByIdWithDetailsAsync(1))
                .ReturnsAsync(new Playlist
                {
                    Id = 1,
                    Name = "Test",
                    HostId = 1,
                    Host = new User { Id = 1, Role = UserRole.Host },
                    PlaylistSongs = new List<PlaylistSong>(),
                    Users = new List<User>()
                });

            mockPlaylistRepo.Setup(x => x.DeleteAsync(It.IsAny<Playlist>()))
                .Returns(Task.CompletedTask);

            var service = new PlaylistService(mockPlaylistRepo.Object, mockUserRepo.Object, mockSongRepo.Object);

            var (success, error) = await service.DeleteAsync(1);

            Assert.True(success);
            Assert.Null(error);
            mockPlaylistRepo.Verify(x => x.DeleteAsync(It.IsAny<Playlist>()), Times.Once);
        }

        [Fact]
        public async Task RemoveSongFromPlaylistAsync_WithNonExistentSong_ShouldReturnError()
        {
            // --- ARRANGE ---
            var mockPlaylistRepo = new Mock<IPlaylistRepository>();
            var mockUserRepo = new Mock<IUserRepository>();
            var mockSongRepo = new Mock<ISongRepository>();

            var playlistId = 1;
            var songId = 999;

            mockPlaylistRepo.Setup(x => x.GetPlaylistSongAsync(playlistId, songId))
                .ReturnsAsync((PlaylistSong?)null);

            var service = new PlaylistService(
                mockPlaylistRepo.Object,
                mockUserRepo.Object,
                mockSongRepo.Object
            );

            // --- ACT ---
            var (success, error) = await service.RemoveSongFromPlaylistAsync(playlistId, songId);

            // --- ASSERT ---
            Assert.False(success);
            Assert.Equal("This song is not found in the playlist.", error);

            mockPlaylistRepo.Verify(x => x.RemovePlaylistSongAsync(It.IsAny<PlaylistSong>()), Times.Never);
        }
        [Fact]
        public async Task RemoveSongFromPlaylistAsync_WithValidSong_ShouldRemoveSuccessfully()
        {
            // --- ARRANGE ---
            var mockPlaylistRepo = new Mock<IPlaylistRepository>();
            var mockUserRepo = new Mock<IUserRepository>();
            var mockSongRepo = new Mock<ISongRepository>();

            var playlistId = 1;
            var songId = 10;

            mockPlaylistRepo.Setup(x => x.GetPlaylistSongAsync(playlistId, songId))
                .ReturnsAsync(new PlaylistSong
                {
                    PlaylistId = playlistId,
                    SongId = songId,
                    Position = 1
                });

            mockPlaylistRepo.Setup(x => x.RemovePlaylistSongAsync(It.IsAny<PlaylistSong>()))
                .Returns(Task.CompletedTask);

            var service = new PlaylistService(
                mockPlaylistRepo.Object,
                mockUserRepo.Object,
                mockSongRepo.Object
            );

            // --- ACT ---
            var (success, error) = await service.RemoveSongFromPlaylistAsync(playlistId, songId);

            // --- ASSERT ---
            Assert.True(success);
            Assert.Null(error);

            mockPlaylistRepo.Verify(x => x.RemovePlaylistSongAsync(It.IsAny<PlaylistSong>()), Times.Once);
        }
        [Fact]
        public async Task EditAsync_WithGuestRoleHost_ShouldReturnUnauthorized()
        {
            // --- ARRANGE ---
            var mockPlaylistRepo = new Mock<IPlaylistRepository>();
            var mockUserRepo = new Mock<IUserRepository>();
            var mockSongRepo = new Mock<ISongRepository>();

            var id = 1;

            mockPlaylistRepo.Setup(x => x.GetByIdWithDetailsAsync(id))
                .ReturnsAsync(new Playlist
                {
                    Id = id,
                    Name = "Test",
                    HostId = 1,
                    Host = new User { Id = 1, Username = "guest", Role = UserRole.Guest, PasswordHash = "hash" },
                    PlaylistSongs = new List<PlaylistSong>(),
                    Users = new List<User>()
                });

            var service = new PlaylistService(
                mockPlaylistRepo.Object,
                mockUserRepo.Object,
                mockSongRepo.Object
            );

            var dto = new PlaylistPatchDto { Name = "New Name" };

            // --- ACT ---
            var (success, error, updated) = await service.EditAsync(id, dto);

            // --- ASSERT ---
            Assert.False(success);
            Assert.Equal("Only hosts or admins can edit playlists.", error);
            Assert.Null(updated);

            mockPlaylistRepo.Verify(x => x.UpdateAsync(It.IsAny<Playlist>()), Times.Never);
        }
        [Fact]
        public async Task EditAsync_WithPartialData_ShouldUpdateOnlyProvidedFields()
        {
            // --- ARRANGE ---
            var mockPlaylistRepo = new Mock<IPlaylistRepository>();
            var mockUserRepo = new Mock<IUserRepository>();
            var mockSongRepo = new Mock<ISongRepository>();

            var id = 1;

            mockPlaylistRepo.Setup(x => x.GetByIdWithDetailsAsync(id))
                .ReturnsAsync(new Playlist
                {
                    Id = id,
                    Name = "Old Name",
                    Description = "Old Description",
                    HostId = 1,
                    Host = new User { Id = 1, Username = "host", Role = UserRole.Host, PasswordHash = "hash" },
                    PlaylistSongs = new List<PlaylistSong>(),
                    Users = new List<User>()
                });

            mockPlaylistRepo.Setup(x => x.UpdateAsync(It.IsAny<Playlist>()))
                .Returns(Task.CompletedTask);

            // Mock GetByIdAsync for the updated result
            mockPlaylistRepo.Setup(x => x.GetByIdWithDetailsAsync(id))
                .ReturnsAsync(new Playlist
                {
                    Id = id,
                    Name = "New Name",
                    Description = "Old Description",
                    HostId = 1,
                    Host = new User { Id = 1, Username = "host", Role = UserRole.Host, PasswordHash = "hash" },
                    PlaylistSongs = new List<PlaylistSong>(),
                    Users = new List<User>()
                });

            var service = new PlaylistService(
                mockPlaylistRepo.Object,
                mockUserRepo.Object,
                mockSongRepo.Object
            );

            var dto = new PlaylistPatchDto
            {
                Name = "New Name",
                Description = null // Only update name
            };

            // --- ACT ---
            var (success, error, updated) = await service.EditAsync(id, dto);

            // --- ASSERT ---
            Assert.True(success);
            Assert.Null(error);
            Assert.NotNull(updated);
            Assert.Equal("New Name", updated.Name);

            mockPlaylistRepo.Verify(x => x.UpdateAsync(It.IsAny<Playlist>()), Times.Once);
        }

        [Fact]
public async Task CreateAsync_ShouldFail_WhenHostNotFound()
{
    var mockPlaylistRepo = new Mock<IPlaylistRepository>();
    var mockUserRepo = new Mock<IUserRepository>();
    var mockSongRepo = new Mock<ISongRepository>();

    mockPlaylistRepo.Setup(x => x.ExistsByNameAsync(It.IsAny<string>()))
        .ReturnsAsync(false);

    mockUserRepo.Setup(x => x.GetByIdAsync(It.IsAny<int>()))
        .ReturnsAsync((User?)null);

    var service = new PlaylistService(mockPlaylistRepo.Object, mockUserRepo.Object, mockSongRepo.Object);

    var dto = new PlaylistCreateDto { Name = "X", Description = "d", HostId = 123 };

    var (success, error, created) = await service.CreateAsync(dto);

    Assert.False(success);
    Assert.Equal($"Host with ID {dto.HostId} not found.", error);
    Assert.Null(created);

    mockPlaylistRepo.Verify(x => x.AddAsync(It.IsAny<Playlist>()), Times.Never);
}

[Fact]
public async Task GetAllAsync_ShouldReturnOnlyAccessiblePlaylists_ForRequester()
{
    var mockPlaylistRepo = new Mock<IPlaylistRepository>();
    var mockUserRepo = new Mock<IUserRepository>();
    var mockSongRepo = new Mock<ISongRepository>();

    var requesterId = 5;

    var playlists = new List<Playlist>
    {
        // accessible as host
        new Playlist
        {
            Id = 1, Name = "Host playlist", HostId = requesterId,
            Host = new User { Id = requesterId, Username = "me", Role = UserRole.Host },
            PlaylistSongs = new List<PlaylistSong>(),
            Users = new List<User>()
        },
        // accessible as collaborator
        new Playlist
        {
            Id = 2, Name = "Collab playlist", HostId = 99,
            Host = new User { Id = 99, Username = "other", Role = UserRole.Host },
            PlaylistSongs = new List<PlaylistSong>(),
            Users = new List<User> { new User { Id = requesterId, Username="me", Role = UserRole.Guest } }
        },
        // NOT accessible
        new Playlist
        {
            Id = 3, Name = "Not mine", HostId = 77,
            Host = new User { Id = 77, Username = "nope", Role = UserRole.Host },
            PlaylistSongs = new List<PlaylistSong>(),
            Users = new List<User>()
        }
    };

    mockPlaylistRepo.Setup(x => x.GetAllAsync()).ReturnsAsync(playlists);

    var service = new PlaylistService(mockPlaylistRepo.Object, mockUserRepo.Object, mockSongRepo.Object);

    var result = (await service.GetAllAsync(requesterId)).ToList();

    Assert.Equal(2, result.Count);
    Assert.Contains(result, p => p.Id == 1);
    Assert.Contains(result, p => p.Id == 2);
    Assert.DoesNotContain(result, p => p.Id == 3);
}

[Fact]
public async Task UpdateByIdAsync_ShouldFail_WhenHostRoleNotAuthorized()
{
    var mockPlaylistRepo = new Mock<IPlaylistRepository>();
    var mockUserRepo = new Mock<IUserRepository>();
    var mockSongRepo = new Mock<ISongRepository>();

    var id = 1;

    mockPlaylistRepo.Setup(x => x.GetByIdWithDetailsAsync(id))
        .ReturnsAsync(new Playlist
        {
            Id = id,
            Name = "Old",
            HostId = 1,
            Host = new User { Id = 1, Role = UserRole.Guest }, // not allowed
            PlaylistSongs = new List<PlaylistSong>(),
            Users = new List<User>()
        });

    var service = new PlaylistService(mockPlaylistRepo.Object, mockUserRepo.Object, mockSongRepo.Object);

    var dto = new PlaylistUpdateDto { Name = "New", Description = "D", SongIds = null, ImageUrl = null };

    var (success, error, updated) = await service.UpdateByIdAsync(id, dto);

    Assert.False(success);
    Assert.Equal("Only hosts or admins can update playlists.", error);
    Assert.Null(updated);

    mockPlaylistRepo.Verify(x => x.UpdateAsync(It.IsAny<Playlist>()), Times.Never);
}

[Fact]
public async Task UpdateByIdAsync_WithSongIds_ShouldReplaceSongs_AndSetPositions()
{
    var mockPlaylistRepo = new Mock<IPlaylistRepository>();
    var mockUserRepo = new Mock<IUserRepository>();
    var mockSongRepo = new Mock<ISongRepository>();

    var id = 1;

    var existingSongs = new List<PlaylistSong>
    {
        new PlaylistSong { PlaylistId = id, SongId = 10, Position = 1 },
        new PlaylistSong { PlaylistId = id, SongId = 11, Position = 2 }
    };

    mockPlaylistRepo.Setup(x => x.GetByIdWithDetailsAsync(id))
        .ReturnsAsync(new Playlist
        {
            Id = id,
            Name = "P",
            HostId = 1,
            Host = new User { Id = 1, Role = UserRole.Host },
            PlaylistSongs = existingSongs,
            Users = new List<User>()
        });

    // song repo returns songs for ids
    mockSongRepo.Setup(x => x.GetByIdAsync(100)).ReturnsAsync(new Song { Id = 100, Title = "A", Artists = new List<Artist>() });
    mockSongRepo.Setup(x => x.GetByIdAsync(200)).ReturnsAsync(new Song { Id = 200, Title = "B", Artists = new List<Artist>() });

    // return something for GetByIdAsync at the end (service calls GetByIdAsync to map)
    mockPlaylistRepo.Setup(x => x.GetByIdWithDetailsAsync(id))
        .ReturnsAsync(new Playlist
        {
            Id = id,
            Name = "NewName",
            Description = "NewDesc",
            HostId = 1,
            Host = new User { Id = 1, Role = UserRole.Host },
            PlaylistSongs = new List<PlaylistSong>
            {
                new PlaylistSong { PlaylistId = id, SongId = 100, Position = 1, Song = new Song { Id=100, Title="A", Artists = new List<Artist>() } },
                new PlaylistSong { PlaylistId = id, SongId = 200, Position = 2, Song = new Song { Id=200, Title="B", Artists = new List<Artist>() } }
            },
            Users = new List<User>()
        });

    mockPlaylistRepo.Setup(x => x.RemovePlaylistSongsRangeAsync(It.IsAny<IEnumerable<PlaylistSong>>()))
        .Returns(Task.CompletedTask);

    mockPlaylistRepo.Setup(x => x.AddPlaylistSongAsync(It.IsAny<PlaylistSong>()))
        .Returns(Task.CompletedTask);

    mockPlaylistRepo.Setup(x => x.UpdateAsync(It.IsAny<Playlist>()))
        .Returns(Task.CompletedTask);

    var service = new PlaylistService(mockPlaylistRepo.Object, mockUserRepo.Object, mockSongRepo.Object);

    var dto = new PlaylistUpdateDto
    {
        Name = "NewName",
        Description = "NewDesc",
        ImageUrl = null,
        SongIds = new List<int> { 100, 200 }
    };

    var (success, error, updated) = await service.UpdateByIdAsync(id, dto);

    Assert.True(success);
    Assert.Null(error);
    Assert.NotNull(updated);
    Assert.Equal(2, updated!.Songs.Count);
    Assert.Equal(1, updated.Songs[0].Position);
    Assert.Equal(2, updated.Songs[1].Position);

    mockPlaylistRepo.Verify(x => x.RemovePlaylistSongsRangeAsync(It.Is<IEnumerable<PlaylistSong>>(s => s.Count() == 2)), Times.Once);

    mockPlaylistRepo.Verify(x => x.AddPlaylistSongAsync(It.Is<PlaylistSong>(ps => ps.SongId == 100 && ps.Position == 1)), Times.Once);
    mockPlaylistRepo.Verify(x => x.AddPlaylistSongAsync(It.Is<PlaylistSong>(ps => ps.SongId == 200 && ps.Position == 2)), Times.Once);

    mockPlaylistRepo.Verify(x => x.UpdateAsync(It.IsAny<Playlist>()), Times.Once);
}

[Fact]
public async Task DeleteAsync_ShouldFail_WhenHostNotAuthorized()
{
    var mockPlaylistRepo = new Mock<IPlaylistRepository>();
    var mockUserRepo = new Mock<IUserRepository>();
    var mockSongRepo = new Mock<ISongRepository>();

    mockPlaylistRepo.Setup(x => x.GetByIdWithDetailsAsync(1))
        .ReturnsAsync(new Playlist
        {
            Id = 1,
            Name = "X",
            HostId = 1,
            Host = new User { Id = 1, Role = UserRole.Guest },
            PlaylistSongs = new List<PlaylistSong>(),
            Users = new List<User>()
        });

    var service = new PlaylistService(mockPlaylistRepo.Object, mockUserRepo.Object, mockSongRepo.Object);

    var (success, error) = await service.DeleteAsync(1);

    Assert.False(success);
    Assert.Equal("Only hosts or admins can delete playlists.", error);

    mockPlaylistRepo.Verify(x => x.DeleteAsync(It.IsAny<Playlist>()), Times.Never);
}

[Fact]
public async Task DeleteAsync_WhenHasSongs_ShouldRemoveSongsRange_BeforeDelete()
{
    var mockPlaylistRepo = new Mock<IPlaylistRepository>();
    var mockUserRepo = new Mock<IUserRepository>();
    var mockSongRepo = new Mock<ISongRepository>();

    var playlist = new Playlist
    {
        Id = 1,
        Name = "X",
        HostId = 1,
        Host = new User { Id = 1, Role = UserRole.Host },
        PlaylistSongs = new List<PlaylistSong>
        {
            new PlaylistSong { PlaylistId = 1, SongId = 10, Position = 1 },
            new PlaylistSong { PlaylistId = 1, SongId = 11, Position = 2 },
        },
        Users = new List<User>()
    };

    mockPlaylistRepo.Setup(x => x.GetByIdWithDetailsAsync(1)).ReturnsAsync(playlist);
    mockPlaylistRepo.Setup(x => x.RemovePlaylistSongsRangeAsync(It.IsAny<IEnumerable<PlaylistSong>>()))
        .Returns(Task.CompletedTask);
    mockPlaylistRepo.Setup(x => x.DeleteAsync(It.IsAny<Playlist>()))
        .Returns(Task.CompletedTask);

    var service = new PlaylistService(mockPlaylistRepo.Object, mockUserRepo.Object, mockSongRepo.Object);

    var (success, error) = await service.DeleteAsync(1);

    Assert.True(success);
    Assert.Null(error);

    mockPlaylistRepo.Verify(x => x.RemovePlaylistSongsRangeAsync(It.Is<IEnumerable<PlaylistSong>>(s => s.Count() == 2)), Times.Once);
    mockPlaylistRepo.Verify(x => x.DeleteAsync(It.IsAny<Playlist>()), Times.Once);
}

    }
}

using Microsoft.EntityFrameworkCore;
using Xunit;
using MyApi.Data;
using MyApi.Models;
using MyApi.Repositories;

namespace TestProject.Repositories
{
    public class UserRepositoryTests
    {
        private static PlaylistAppContext CreateDb()
        {
            var opts = new DbContextOptionsBuilder<PlaylistAppContext>()
                .UseInMemoryDatabase(databaseName: $"TestDb_{Guid.NewGuid()}")
                .Options;

            return new PlaylistAppContext(opts);
        }

        [Fact]
        public async Task AddAsync_ShouldPersistUser()
        {
            await using var db = CreateDb();
            var repo = new UserRepository(db);

            var user = new User { Username = "marius", PasswordHash = "hash", Role = UserRole.Guest };

            await repo.AddAsync(user);

            Assert.True(user.Id != 0);
            Assert.Equal(1, await db.Users.CountAsync());
        }

        [Fact]
        public async Task GetByIdAsync_ShouldReturnNull_WhenMissing()
        {
            await using var db = CreateDb();
            var repo = new UserRepository(db);

            var result = await repo.GetByIdAsync(123);

            Assert.Null(result);
        }

        [Fact]
        public async Task GetByUsernameAsync_ShouldReturnUser_WhenExists()
        {
            await using var db = CreateDb();
            db.Users.Add(new User { Username = "john", PasswordHash = "h", Role = UserRole.Host });
            await db.SaveChangesAsync();

            var repo = new UserRepository(db);

            var user = await repo.GetByUsernameAsync("john");

            Assert.NotNull(user);
            Assert.Equal("john", user!.Username);
        }

        [Fact]
        public async Task ExistsByUsernameAsync_ShouldReturnTrue_WhenExists()
        {
            await using var db = CreateDb();
            db.Users.Add(new User { Username = "john", PasswordHash = "h", Role = UserRole.Guest });
            await db.SaveChangesAsync();

            var repo = new UserRepository(db);

            var exists = await repo.ExistsByUsernameAsync("john");

            Assert.True(exists);
        }

        [Fact]
        public async Task ExistsByUsernameAsync_ShouldReturnFalse_WhenMissing()
        {
            await using var db = CreateDb();
            var repo = new UserRepository(db);

            var exists = await repo.ExistsByUsernameAsync("nope");

            Assert.False(exists);
        }

        [Fact]
        public async Task ExistsAsync_ShouldReturnTrue_WhenExists()
        {
            await using var db = CreateDb();
            var u = new User { Username = "u1", PasswordHash = "h", Role = UserRole.Guest };
            db.Users.Add(u);
            await db.SaveChangesAsync();

            var repo = new UserRepository(db);

            Assert.True(await repo.ExistsAsync(u.Id));
        }

        [Fact]
        public async Task GetByIdsAsync_ShouldReturnOnlyMatchingUsers()
        {
            await using var db = CreateDb();
            var u1 = new User { Username = "u1", PasswordHash = "h", Role = UserRole.Guest };
            var u2 = new User { Username = "u2", PasswordHash = "h", Role = UserRole.Guest };
            var u3 = new User { Username = "u3", PasswordHash = "h", Role = UserRole.Guest };
            db.Users.AddRange(u1, u2, u3);
            await db.SaveChangesAsync();

            var repo = new UserRepository(db);

            var result = (await repo.GetByIdsAsync(new[] { u1.Id, u3.Id })).ToList();

            Assert.Equal(2, result.Count);
            Assert.Contains(result, x => x.Id == u1.Id);
            Assert.Contains(result, x => x.Id == u3.Id);
            Assert.DoesNotContain(result, x => x.Id == u2.Id);
        }

        [Fact]
        public async Task UpdateProfileImageAsync_ShouldReturnFalse_WhenUserMissing()
        {
            await using var db = CreateDb();
            var repo = new UserRepository(db);

            var ok = await repo.UpdateProfileImageAsync(999, "/img.png");

            Assert.False(ok);
        }

        [Fact]
        public async Task UpdateProfileImageAsync_ShouldUpdateAndReturnTrue_WhenUserExists()
        {
            await using var db = CreateDb();
            var u = new User { Username = "u1", PasswordHash = "h", Role = UserRole.Guest };
            db.Users.Add(u);
            await db.SaveChangesAsync();

            var repo = new UserRepository(db);

            var ok = await repo.UpdateProfileImageAsync(u.Id, "/profiles/u1.png");

            Assert.True(ok);

            var fromDb = await db.Users.FindAsync(u.Id);
            Assert.Equal("/profiles/u1.png", fromDb!.ProfileImage);
        }

        [Fact]
        public async Task SearchByUsernameAsync_ShouldReturnEmpty_WhenQueryNullOrWhitespace()
        {
            await using var db = CreateDb();
            db.Users.Add(new User { Username = "marius", PasswordHash = "h", Role = UserRole.Guest });
            await db.SaveChangesAsync();

            var repo = new UserRepository(db);

            var r1 = await repo.SearchByUsernameAsync(null!);
            var r2 = await repo.SearchByUsernameAsync("   ");

            Assert.Empty(r1);
            Assert.Empty(r2);
        }

        [Fact]
        public async Task SearchByUsernameAsync_ShouldBeCaseInsensitive_AndRespectLimit()
        {
            await using var db = CreateDb();
            db.Users.AddRange(
                new User { Username = "Marius", PasswordHash = "h", Role = UserRole.Guest },
                new User { Username = "mariuk", PasswordHash = "h", Role = UserRole.Guest },
                new User { Username = "XxMaRiUsxX", PasswordHash = "h", Role = UserRole.Guest },
                new User { Username = "john", PasswordHash = "h", Role = UserRole.Guest }
            );
            await db.SaveChangesAsync();

            var repo = new UserRepository(db);

            var result = (await repo.SearchByUsernameAsync("mArIuS", limit: 2)).ToList();

            Assert.Equal(2, result.Count);          // limit
            Assert.All(result, u => Assert.Contains("marius", u.Username.ToLower()));
        }

        [Fact]
        public async Task DeleteAsync_ShouldRemoveUser()
        {
            await using var db = CreateDb();
            var u = new User { Username = "del", PasswordHash = "h", Role = UserRole.Guest };
            db.Users.Add(u);
            await db.SaveChangesAsync();

            var repo = new UserRepository(db);

            await repo.DeleteAsync(u);

            Assert.Equal(0, await db.Users.CountAsync());
        }

        [Fact]
        public async Task UpdateAsync_ShouldPersistChanges()
        {
            await using var db = CreateDb();
            var u = new User { Username = "old", PasswordHash = "h", Role = UserRole.Guest };
            db.Users.Add(u);
            await db.SaveChangesAsync();

            u.Username = "new";

            var repo = new UserRepository(db);
            await repo.UpdateAsync(u);

            var fromDb = await db.Users.AsNoTracking().FirstAsync(x => x.Id == u.Id);
            Assert.Equal("new", fromDb.Username);
        }
    }
}

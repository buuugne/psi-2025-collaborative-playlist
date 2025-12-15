using System.Text;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.Internal;
using Moq;
using Xunit;
using MyApi.Services;
using MyApi.Repositories;
using MyApi.Models;

namespace TestProject.services
{
    public class UserServiceTests
    {
        private readonly Mock<IUserRepository> _repoMock;
        private readonly UserService _sut;

        public UserServiceTests()
        {
            _repoMock = new Mock<IUserRepository>(MockBehavior.Strict);
            _sut = new UserService(_repoMock.Object);
        }

        [Fact]
        public async Task GetAllAsync_ReturnsDtos()
        {
            // Arrange
            var users = new List<User>
            {
                new User { Id = 1, Username = "a", Role = UserRole.User, ProfileImage = "/profiles/a.png" },
                new User { Id = 2, Username = "b", Role = UserRole.Admin, ProfileImage = null }
            };

            _repoMock.Setup(r => r.GetAllAsync()).ReturnsAsync(users);

            // Act
            var result = (await _sut.GetAllAsync()).ToList();

            // Assert
            Assert.Equal(2, result.Count);
            Assert.Equal(1, result[0].Id);
            Assert.Equal("a", result[0].Username);
            Assert.Equal(UserRole.User, result[0].Role);
            Assert.Equal("/profiles/a.png", result[0].ProfileImage);

            _repoMock.VerifyAll();
        }

        [Fact]
        public async Task GetByIdAsync_WhenUserNotFound_ReturnsNull()
        {
            _repoMock.Setup(r => r.GetByIdAsync(123)).ReturnsAsync((User?)null);

            var result = await _sut.GetByIdAsync(123);

            Assert.Null(result);
            _repoMock.VerifyAll();
        }

        [Fact]
        public async Task GetByIdAsync_WhenUserFound_ReturnsDto()
        {
            var user = new User { Id = 5, Username = "john", Role = UserRole.User, ProfileImage = "/profiles/x.png" };
            _repoMock.Setup(r => r.GetByIdAsync(5)).ReturnsAsync(user);

            var result = await _sut.GetByIdAsync(5);

            Assert.NotNull(result);
            Assert.Equal(5, result!.Id);
            Assert.Equal("john", result.Username);
            Assert.Equal(UserRole.User, result.Role);

            _repoMock.VerifyAll();
        }

        [Fact]
        public async Task DeleteAsync_WhenUserNotFound_ReturnsFalseAndError()
        {
            _repoMock.Setup(r => r.GetByIdAsync(9)).ReturnsAsync((User?)null);

            var (success, error) = await _sut.DeleteAsync(9);

            Assert.False(success);
            Assert.Equal("User not found", error);

            _repoMock.VerifyAll();
        }

        [Fact]
        public async Task DeleteAsync_WhenUserFound_DeletesAndReturnsTrue()
        {
            var user = new User { Id = 9, Username = "del", Role = UserRole.User };
            _repoMock.Setup(r => r.GetByIdAsync(9)).ReturnsAsync(user);
            _repoMock.Setup(r => r.DeleteAsync(user)).Returns(Task.CompletedTask);

            var (success, error) = await _sut.DeleteAsync(9);

            Assert.True(success);
            Assert.Null(error);

            _repoMock.VerifyAll();
        }

        [Fact]
        public async Task ChangeRoleAsync_WhenUserNotFound_ReturnsFalse()
        {
            _repoMock.Setup(r => r.GetByIdAsync(1)).ReturnsAsync((User?)null);

            var (success, error) = await _sut.ChangeRoleAsync(1, UserRole.Admin);

            Assert.False(success);
            Assert.Equal("User not found", error);

            _repoMock.VerifyAll();
        }

        [Fact]
        public async Task ChangeRoleAsync_WhenUserFound_UpdatesRole()
        {
            var user = new User { Id = 1, Username = "u", Role = UserRole.User };
            _repoMock.Setup(r => r.GetByIdAsync(1)).ReturnsAsync(user);
            _repoMock.Setup(r => r.UpdateAsync(It.Is<User>(x => x.Id == 1 && x.Role == UserRole.Admin)))
                     .Returns(Task.CompletedTask);

            var (success, error) = await _sut.ChangeRoleAsync(1, UserRole.Admin);

            Assert.True(success);
            Assert.Null(error);
            Assert.Equal(UserRole.Admin, user.Role);

            _repoMock.VerifyAll();
        }

        [Theory]
        [InlineData(null)]
        [InlineData("")]
        [InlineData(" ")]
        [InlineData("a")] // < 2
        public async Task SearchUsersAsync_WhenQueryTooShort_ReturnsEmpty(string? query)
        {
            var result = await _sut.SearchUsersAsync(query ?? "");

            Assert.Empty(result);
            _repoMock.VerifyNoOtherCalls();
        }

        [Fact]
        public async Task SearchUsersAsync_WhenValidQuery_ReturnsDtos()
        {
            var users = new List<User>
            {
                new User { Id = 1, Username = "marius", Role = UserRole.User },
                new User { Id = 2, Username = "mari", Role = UserRole.Admin }
            };

            _repoMock.Setup(r => r.SearchByUsernameAsync("mar", 10)).ReturnsAsync(users);

            var result = (await _sut.SearchUsersAsync("mar")).ToList();

            Assert.Equal(2, result.Count);
            Assert.Equal("marius", result[0].Username);

            _repoMock.VerifyAll();
        }

        [Fact]
        public async Task SearchUsersAsync_UsesDefaultLimit10()
        {
            _repoMock.Setup(r => r.SearchByUsernameAsync("ma", 10))
                    .ReturnsAsync(new List<User>());

            var result = await _sut.SearchUsersAsync("ma");

            Assert.Empty(result);

            _repoMock.VerifyAll();
        }


        [Fact]
        public async Task UpdateProfileImageAsync_WhenUserNotFound_ReturnsNull()
        {
            _repoMock.Setup(r => r.GetByIdAsync(99)).ReturnsAsync((User?)null);

            var file = CreateFormFile("photo.png", "fake");
            var result = await _sut.UpdateProfileImageAsync(99, file, CreateTempWebRoot());

            Assert.Null(result);
            _repoMock.VerifyAll();
        }

        [Fact]
        public async Task UpdateProfileImageAsync_WhenInvalidExtension_Throws()
        {
            var user = new User { Id = 1, Username = "u", Role = UserRole.User };
            _repoMock.Setup(r => r.GetByIdAsync(1)).ReturnsAsync(user);

            var file = CreateFormFile("bad.exe", "fake");

            await Assert.ThrowsAsync<ArgumentException>(() =>
                _sut.UpdateProfileImageAsync(1, file, CreateTempWebRoot()));

            _repoMock.VerifyAll();
        }

        [Fact]
        public async Task UpdateProfileImageAsync_WhenTooLarge_Throws()
        {
            var user = new User { Id = 1, Username = "u", Role = UserRole.User };
            _repoMock.Setup(r => r.GetByIdAsync(1)).ReturnsAsync(user);

            var file = CreateFormFile("photo.png", new string('a', 6 * 1024 * 1024)); // > 5MB

            await Assert.ThrowsAsync<ArgumentException>(() =>
                _sut.UpdateProfileImageAsync(1, file, CreateTempWebRoot()));

            _repoMock.VerifyAll();
        }

        [Fact]
        public async Task UpdateProfileImageAsync_WhenRepoUpdateFails_Throws()
        {
            var user = new User { Id = 1, Username = "u", Role = UserRole.User };
            _repoMock.Setup(r => r.GetByIdAsync(1)).ReturnsAsync(user);
            _repoMock.Setup(r => r.UpdateProfileImageAsync(1, It.IsAny<string>())).ReturnsAsync(false);

            var file = CreateFormFile("photo.png", "fake");

            await Assert.ThrowsAsync<ArgumentException>(() =>
                _sut.UpdateProfileImageAsync(1, file, CreateTempWebRoot()));

            _repoMock.VerifyAll();
        }

        [Fact]
        public async Task UpdateProfileImageAsync_WhenOk_UpdatesAndReturnsDto()
        {
            var user = new User { Id = 1, Username = "u", Role = UserRole.User };
            _repoMock.Setup(r => r.GetByIdAsync(1)).ReturnsAsync(user);
            _repoMock.Setup(r => r.UpdateProfileImageAsync(1, It.IsAny<string>())).ReturnsAsync(true);

            var webRoot = CreateTempWebRoot();
            var file = CreateFormFile("photo.png", "fakecontent");

            var dto = await _sut.UpdateProfileImageAsync(1, file, webRoot);

            Assert.NotNull(dto);
            Assert.Equal(1, dto!.Id);
            Assert.Equal("u", dto.Username);
            Assert.Contains("/profiles/", dto.ProfileImage);

            // user object updated too
            Assert.Contains("/profiles/", user.ProfileImage);

            _repoMock.VerifyAll();
        }

        // ---------------- helpers ----------------

        private static string CreateTempWebRoot()
        {
            var path = Path.Combine(Path.GetTempPath(), "musichub_tests", Guid.NewGuid().ToString("N"));
            Directory.CreateDirectory(path);
            return path;
        }

        private static IFormFile CreateFormFile(string fileName, string content)
        {
            var bytes = Encoding.UTF8.GetBytes(content);
            var stream = new MemoryStream(bytes);

            // FormFile iš Microsoft.AspNetCore.Http
            return new FormFile(stream, 0, stream.Length, "file", fileName)
            {
                Headers = new HeaderDictionary(),
                ContentType = "application/octet-stream"
            };
        }
    }
}

using Xunit;
using Moq;
using MyApi.Services;
using MyApi.Repositories;
using MyApi.Models;
using MyApi.Dtos;
using System.Threading.Tasks;

namespace TestProject
{
    public class AuthServiceTests
    {
        // ========== LOGIN TESTS ==========

        [Fact]
        public async Task Login_ShouldFail_WhenPasswordIsWrong()
        {
            // --- ARRANGE ---
            var mockUserRepo = new Mock<IUserRepository>();
            var mockTokenService = new Mock<ITokenService>();

            var storedUser = new User
            {
                Id = 1,
                Username = "testuser",
                PasswordHash = BCrypt.Net.BCrypt.HashPassword("correctpassword"),
                Role = UserRole.Host
            };

            mockUserRepo.Setup(x => x.GetByUsernameAsync("testuser"))
                        .ReturnsAsync(storedUser);

            var authService = new AuthService(mockUserRepo.Object, mockTokenService.Object);

            var loginDto = new LoginUserDto
            {
                Username = "testuser",
                Password = "wrongpassword"
            };

            // --- ACT ---
            var (success, error, result) = await authService.LoginAsync(loginDto);

            // --- ASSERT ---
            Assert.False(success);
            Assert.Equal("Invalid credentials", error);
            Assert.Null(result);
            mockTokenService.Verify(x => x.Generate(It.IsAny<User>()), Times.Never);
        }
        
        [Fact]
        public async Task Login_ShouldSucceed_WhenCredentialsAreCorrect()
        {
            // --- ARRANGE ---
            var mockUserRepo = new Mock<IUserRepository>();
            var mockTokenService = new Mock<ITokenService>();

            var storedUser = new User
            {
                Id = 1,
                Username = "testuser",
                PasswordHash = BCrypt.Net.BCrypt.HashPassword("correctpassword"),
                Role = UserRole.Host
            };

            mockUserRepo.Setup(x => x.GetByUsernameAsync("testuser"))
                        .ReturnsAsync(storedUser);

            mockTokenService.Setup(x => x.Generate(storedUser))
                            .Returns("fake-jwt-token");

            var authService = new AuthService(mockUserRepo.Object, mockTokenService.Object);

            var loginDto = new LoginUserDto
            {
                Username = "testuser",
                Password = "correctpassword"
            };

            // --- ACT ---
            var (success, error, result) = await authService.LoginAsync(loginDto);

            // --- ASSERT ---
            Assert.True(success);
            Assert.Null(error);
            Assert.NotNull(result);
            Assert.Equal("fake-jwt-token", result.Token);
            Assert.Equal("testuser", result.User.Username);
            mockTokenService.Verify(x => x.Generate(storedUser), Times.Once);
        }

        [Fact]
        public async Task Login_ShouldFail_WhenUsernameDoesNotExist()
        {
            // --- ARRANGE ---
            var mockUserRepo = new Mock<IUserRepository>();
            var mockTokenService = new Mock<ITokenService>();

            // Return null when user doesn't exist
            mockUserRepo.Setup(x => x.GetByUsernameAsync("nonexistent"))
                        .ReturnsAsync((User?)null);

            var authService = new AuthService(mockUserRepo.Object, mockTokenService.Object);

            var loginDto = new LoginUserDto
            {
                Username = "nonexistent",
                Password = "anypassword"
            };

            // --- ACT ---
            var (success, error, result) = await authService.LoginAsync(loginDto);

            // --- ASSERT ---
            Assert.False(success);
            Assert.Equal("Invalid credentials", error);
            Assert.Null(result);
            mockTokenService.Verify(x => x.Generate(It.IsAny<User>()), Times.Never);
        }

        [Fact]
        public async Task Login_ShouldFail_WhenUsernameIsEmpty()
        {
            // --- ARRANGE ---
            var mockUserRepo = new Mock<IUserRepository>();
            var mockTokenService = new Mock<ITokenService>();
            var authService = new AuthService(mockUserRepo.Object, mockTokenService.Object);

            var loginDto = new LoginUserDto
            {
                Username = "",
                Password = "password123"
            };

            // --- ACT ---
            var (success, error, result) = await authService.LoginAsync(loginDto);

            // --- ASSERT ---
            Assert.False(success);
            Assert.Equal("Username and password are required", error);
            Assert.Null(result);
            mockUserRepo.Verify(x => x.GetByUsernameAsync(It.IsAny<string>()), Times.Never);
        }

        [Fact]
        public async Task Login_ShouldFail_WhenPasswordIsEmpty()
        {
            // --- ARRANGE ---
            var mockUserRepo = new Mock<IUserRepository>();
            var mockTokenService = new Mock<ITokenService>();
            var authService = new AuthService(mockUserRepo.Object, mockTokenService.Object);

            var loginDto = new LoginUserDto
            {
                Username = "testuser",
                Password = ""
            };

            // --- ACT ---
            var (success, error, result) = await authService.LoginAsync(loginDto);

            // --- ASSERT ---
            Assert.False(success);
            Assert.Equal("Username and password are required", error);
            Assert.Null(result);
        }

        [Fact]
        public async Task Login_ShouldTrimUsernameWhitespace()
        {
            // --- ARRANGE ---
            var mockUserRepo = new Mock<IUserRepository>();
            var mockTokenService = new Mock<ITokenService>();

            var storedUser = new User
            {
                Id = 1,
                Username = "testuser",
                PasswordHash = BCrypt.Net.BCrypt.HashPassword("password123"),
                Role = UserRole.Host
            };

            mockUserRepo.Setup(x => x.GetByUsernameAsync("testuser"))
                        .ReturnsAsync(storedUser);

            mockTokenService.Setup(x => x.Generate(storedUser))
                            .Returns("token");

            var authService = new AuthService(mockUserRepo.Object, mockTokenService.Object);

            var loginDto = new LoginUserDto
            {
                Username = "  testuser  ", // with whitespace
                Password = "password123"
            };

            // --- ACT ---
            var (success, error, result) = await authService.LoginAsync(loginDto);

            // --- ASSERT ---
            Assert.True(success);
            mockUserRepo.Verify(x => x.GetByUsernameAsync("testuser"), Times.Once);
        }

        // ========== REGISTER TESTS ==========

        [Fact]
        public async Task Register_ShouldSucceed_WhenDataIsValid()
        {
            // --- ARRANGE ---
            var mockUserRepo = new Mock<IUserRepository>();
            var mockTokenService = new Mock<ITokenService>();

            mockUserRepo.Setup(x => x.ExistsByUsernameAsync("newuser"))
                        .ReturnsAsync(false);

            mockUserRepo.Setup(x => x.AddAsync(It.IsAny<User>()))
                        .Callback<User>(u => u.Id = 100)
                        .Returns(Task.CompletedTask);

            mockTokenService.Setup(x => x.Generate(It.IsAny<User>()))
                            .Returns("new-user-token");

            var authService = new AuthService(mockUserRepo.Object, mockTokenService.Object);

            var registerDto = new RegisterUserDto
            {
                Username = "newuser",
                Password = "password123",
                ConfirmPassword = "password123"
            };

            // --- ACT ---
            var (success, error, result) = await authService.RegisterAsync(registerDto);

            // --- ASSERT ---
            Assert.True(success);
            Assert.Null(error);
            Assert.NotNull(result);
            Assert.Equal("new-user-token", result.Token);
            Assert.Equal("newuser", result.User.Username);
            Assert.Equal(UserRole.Host, result.User.Role);

            mockUserRepo.Verify(x => x.AddAsync(It.Is<User>(u => 
                u.Username == "newuser" && 
                u.Role == UserRole.Host
            )), Times.Once);
        }

        [Fact]
        public async Task Register_ShouldFail_WhenUsernameAlreadyExists()
        {
            // --- ARRANGE ---
            var mockUserRepo = new Mock<IUserRepository>();
            var mockTokenService = new Mock<ITokenService>();

            mockUserRepo.Setup(x => x.ExistsByUsernameAsync("existinguser"))
                        .ReturnsAsync(true);

            var authService = new AuthService(mockUserRepo.Object, mockTokenService.Object);

            var registerDto = new RegisterUserDto
            {
                Username = "existinguser",
                Password = "password123",
                ConfirmPassword = "password123"
            };

            // --- ACT ---
            var (success, error, result) = await authService.RegisterAsync(registerDto);

            // --- ASSERT ---
            Assert.False(success);
            Assert.Equal("Username already exists", error);
            Assert.Null(result);
            mockUserRepo.Verify(x => x.AddAsync(It.IsAny<User>()), Times.Never);
        }

        [Fact]
        public async Task Register_ShouldFail_WhenPasswordsDoNotMatch()
        {
            // --- ARRANGE ---
            var mockUserRepo = new Mock<IUserRepository>();
            var mockTokenService = new Mock<ITokenService>();
            var authService = new AuthService(mockUserRepo.Object, mockTokenService.Object);

            var registerDto = new RegisterUserDto
            {
                Username = "newuser",
                Password = "password123",
                ConfirmPassword = "differentpassword"
            };

            // --- ACT ---
            var (success, error, result) = await authService.RegisterAsync(registerDto);

            // --- ASSERT ---
            Assert.False(success);
            Assert.Equal("Passwords do not match", error);
            Assert.Null(result);
            mockUserRepo.Verify(x => x.ExistsByUsernameAsync(It.IsAny<string>()), Times.Never);
        }

        [Fact]
        public async Task Register_ShouldFail_WhenUsernameIsEmpty()
        {
            // --- ARRANGE ---
            var mockUserRepo = new Mock<IUserRepository>();
            var mockTokenService = new Mock<ITokenService>();
            var authService = new AuthService(mockUserRepo.Object, mockTokenService.Object);

            var registerDto = new RegisterUserDto
            {
                Username = "",
                Password = "password123",
                ConfirmPassword = "password123"
            };

            // --- ACT ---
            var (success, error, result) = await authService.RegisterAsync(registerDto);

            // --- ASSERT ---
            Assert.False(success);
            Assert.Equal("Username and password are required", error);
            Assert.Null(result);
        }

        [Fact]
        public async Task Register_ShouldFail_WhenPasswordIsEmpty()
        {
            // --- ARRANGE ---
            var mockUserRepo = new Mock<IUserRepository>();
            var mockTokenService = new Mock<ITokenService>();
            var authService = new AuthService(mockUserRepo.Object, mockTokenService.Object);

            var registerDto = new RegisterUserDto
            {
                Username = "newuser",
                Password = "",
                ConfirmPassword = ""
            };

            // --- ACT ---
            var (success, error, result) = await authService.RegisterAsync(registerDto);

            // --- ASSERT ---
            Assert.False(success);
            Assert.Equal("Username and password are required", error);
            Assert.Null(result);
        }

        [Fact]
        public async Task Register_ShouldHashPassword()
        {
            // --- ARRANGE ---
            var mockUserRepo = new Mock<IUserRepository>();
            var mockTokenService = new Mock<ITokenService>();

            User? capturedUser = null;
            mockUserRepo.Setup(x => x.ExistsByUsernameAsync(It.IsAny<string>()))
                        .ReturnsAsync(false);

            mockUserRepo.Setup(x => x.AddAsync(It.IsAny<User>()))
                        .Callback<User>(u => { 
                            capturedUser = u;
                            u.Id = 200;
                        })
                        .Returns(Task.CompletedTask);

            mockTokenService.Setup(x => x.Generate(It.IsAny<User>()))
                            .Returns("token");

            var authService = new AuthService(mockUserRepo.Object, mockTokenService.Object);

            var registerDto = new RegisterUserDto
            {
                Username = "secureuser",
                Password = "myplaintextpassword",
                ConfirmPassword = "myplaintextpassword"
            };

            // --- ACT ---
            await authService.RegisterAsync(registerDto);

            // --- ASSERT ---
            Assert.NotNull(capturedUser);
            Assert.NotEqual("myplaintextpassword", capturedUser.PasswordHash);
            
            // Verify the hash is valid BCrypt hash
            var isValidHash = BCrypt.Net.BCrypt.Verify("myplaintextpassword", capturedUser.PasswordHash);
            Assert.True(isValidHash);
        }

        [Fact]
        public async Task Register_ShouldTrimUsernameWhitespace()
        {
            // --- ARRANGE ---
            var mockUserRepo = new Mock<IUserRepository>();
            var mockTokenService = new Mock<ITokenService>();

            mockUserRepo.Setup(x => x.ExistsByUsernameAsync("trimmeduser"))
                        .ReturnsAsync(false);

            mockUserRepo.Setup(x => x.AddAsync(It.IsAny<User>()))
                        .Callback<User>(u => u.Id = 300)
                        .Returns(Task.CompletedTask);

            mockTokenService.Setup(x => x.Generate(It.IsAny<User>()))
                            .Returns("token");

            var authService = new AuthService(mockUserRepo.Object, mockTokenService.Object);

            var registerDto = new RegisterUserDto
            {
                Username = "  trimmeduser  ",
                Password = "password123",
                ConfirmPassword = "password123"
            };

            // --- ACT ---
            var (success, error, result) = await authService.RegisterAsync(registerDto);

            // --- ASSERT ---
            Assert.True(success);
            mockUserRepo.Verify(x => x.ExistsByUsernameAsync("trimmeduser"), Times.Once);
            Assert.Equal("trimmeduser", result!.User.Username);
        }
    }
}
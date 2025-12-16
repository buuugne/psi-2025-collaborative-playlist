using Xunit;
using Moq;
using Microsoft.AspNetCore.Mvc;
using MyApi.Controllers;
using MyApi.Services;
using MyApi.Dtos;
using MyApi.Models;

namespace TestProject.Controllers
{
    public class AuthControllerTests
    {
        private readonly Mock<IAuthService> _serviceMock;
        private readonly AuthController _controller;

        public AuthControllerTests()
        {
            _serviceMock = new Mock<IAuthService>();
            _controller = new AuthController(_serviceMock.Object);
        }

        // =============================
        // Register Tests
        // =============================

        [Fact]
        public async Task Register_ValidDto_ReturnsOkWithResult()
        {
            // Arrange
            var dto = new RegisterUserDto
            {
                Username = "testuser",
                Password = "Password123!",
                ConfirmPassword = "Password123!"
            };

            var expectedResult = new LoginResponseDto
            {
                Token = "test-token",
                User = new UserDto
                {
                    Id = 1,
                    Username = "testuser",
                    Role = UserRole.Guest
                }
            };

            _serviceMock
                .Setup(s => s.RegisterAsync(dto))
                .ReturnsAsync((true, null, expectedResult));

            // Act
            var result = await _controller.Register(dto);

            // Assert
            var okResult = Assert.IsType<OkObjectResult>(result);
            Assert.Equal(expectedResult, okResult.Value);
            _serviceMock.Verify(s => s.RegisterAsync(dto), Times.Once);
        }

        [Fact]
        public async Task Register_ServiceFails_ReturnsBadRequest()
        {
            // Arrange
            var dto = new RegisterUserDto
            {
                Username = "testuser",
                Password = "Password123!",
                ConfirmPassword = "Password123!"
            };

            _serviceMock
                .Setup(s => s.RegisterAsync(dto))
                .ReturnsAsync((false, "Username already exists", null));

            // Act
            var result = await _controller.Register(dto);

            // Assert
            var badRequestResult = Assert.IsType<BadRequestObjectResult>(result);
            var response = badRequestResult.Value;
            var messageProperty = response?.GetType().GetProperty("message");
            var message = messageProperty?.GetValue(response)?.ToString();
            Assert.Equal("Username already exists", message);
        }

        [Fact]
        public async Task Register_InvalidModelState_ReturnsBadRequest()
        {
            // Arrange
            var dto = new RegisterUserDto
            {
                Username = "testuser",
                Password = "Password123!",
                ConfirmPassword = "DifferentPassword"
            };

            _controller.ModelState.AddModelError("ConfirmPassword", "Passwords do not match");

            // Act
            var result = await _controller.Register(dto);

            // Assert
            var badRequestResult = Assert.IsType<BadRequestObjectResult>(result);
            Assert.IsType<SerializableError>(badRequestResult.Value);
            _serviceMock.Verify(s => s.RegisterAsync(It.IsAny<RegisterUserDto>()), Times.Never);
        }

        // =============================
        // Login Tests
        // =============================

        [Fact]
        public async Task Login_ValidCredentials_ReturnsOkWithToken()
        {
            // Arrange
            var dto = new LoginUserDto
            {
                Username = "testuser",
                Password = "Password123!"
            };

            var expectedResult = new LoginResponseDto
            {
                Token = "jwt-token-here",
                User = new UserDto
                {
                    Id = 1,
                    Username = "testuser",
                    Role = UserRole.Guest
                }
            };

            _serviceMock
                .Setup(s => s.LoginAsync(dto))
                .ReturnsAsync((true, null, expectedResult));

            // Act
            var result = await _controller.Login(dto);

            // Assert
            var okResult = Assert.IsType<OkObjectResult>(result);
            Assert.Equal(expectedResult, okResult.Value);
            _serviceMock.Verify(s => s.LoginAsync(dto), Times.Once);
        }

        [Fact]
        public async Task Login_InvalidCredentials_ReturnsUnauthorized()
        {
            // Arrange
            var dto = new LoginUserDto
            {
                Username = "testuser",
                Password = "WrongPassword"
            };

            _serviceMock
                .Setup(s => s.LoginAsync(dto))
                .ReturnsAsync((false, "Invalid credentials", null));

            // Act
            var result = await _controller.Login(dto);

            // Assert
            var unauthorizedResult = Assert.IsType<UnauthorizedObjectResult>(result);
            var response = unauthorizedResult.Value;
            var messageProperty = response?.GetType().GetProperty("message");
            var message = messageProperty?.GetValue(response)?.ToString();
            Assert.Equal("Invalid credentials", message);
        }

        [Fact]
        public async Task Login_NullUsername_ReturnsUnauthorized()
        {
            // Arrange
            var dto = new LoginUserDto
            {
                Username = null,
                Password = "Password123!"
            };

            _serviceMock
                .Setup(s => s.LoginAsync(dto))
                .ReturnsAsync((false, "Username is required", null));

            // Act
            var result = await _controller.Login(dto);

            // Assert
            Assert.IsType<UnauthorizedObjectResult>(result);
        }

        [Fact]
        public async Task Login_EmptyPassword_ReturnsUnauthorized()
        {
            // Arrange
            var dto = new LoginUserDto
            {
                Username = "testuser",
                Password = ""
            };

            _serviceMock
                .Setup(s => s.LoginAsync(dto))
                .ReturnsAsync((false, "Password is required", null));

            // Act
            var result = await _controller.Login(dto);

            // Assert
            Assert.IsType<UnauthorizedObjectResult>(result);
        }
    }
}
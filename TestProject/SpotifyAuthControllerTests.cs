using Xunit;
using Moq;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using MyApi.Controllers;
using MyApi.Services;
using MyApi.Dtos;
using MyApi.Data;

namespace TestProject.Controllers
{
    public class SpotifyAuthControllerTests
    {
        private readonly Mock<ISpotifyService> _spotifyMock;
        private readonly PlaylistAppContext _db;
        private readonly SpotifyAuthController _controller;

        public SpotifyAuthControllerTests()
        {
            _spotifyMock = new Mock<ISpotifyService>();
            
            // Create in-memory database for testing
            var options = new DbContextOptionsBuilder<PlaylistAppContext>()
                .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
                .Options;
            
            _db = new PlaylistAppContext(options);
            _controller = new SpotifyAuthController(_spotifyMock.Object, _db);
        }

        // =============================
        // GetLoginUrl Tests
        // =============================

        [Fact]
        public void GetLoginUrl_ReturnsOkWithUrl()
        {
            // Arrange
            var expectedUrl = "https://accounts.spotify.com/authorize?client_id=test&response_type=code";
            _spotifyMock
                .Setup(s => s.GenerateLoginUrl())
                .Returns(expectedUrl);

            // Act
            var result = _controller.GetLoginUrl();

            // Assert
            var okResult = Assert.IsType<OkObjectResult>(result);
            var response = okResult.Value;
            var urlProperty = response?.GetType().GetProperty("url");
            var url = urlProperty?.GetValue(response)?.ToString();
            Assert.Equal(expectedUrl, url);
            _spotifyMock.Verify(s => s.GenerateLoginUrl(), Times.Once);
        }

        [Fact]
        public void GetLoginUrl_CalledMultipleTimes_GeneratesNewUrls()
        {
            // Arrange
            _spotifyMock
                .Setup(s => s.GenerateLoginUrl())
                .Returns("https://accounts.spotify.com/authorize?state=random");

            // Act
            var result1 = _controller.GetLoginUrl();
            var result2 = _controller.GetLoginUrl();

            // Assert
            Assert.IsType<OkObjectResult>(result1);
            Assert.IsType<OkObjectResult>(result2);
            _spotifyMock.Verify(s => s.GenerateLoginUrl(), Times.Exactly(2));
        }

        // =============================
        // Callback Tests
        // =============================

        [Fact]
        public async Task Callback_ValidCode_ReturnsOkWithTokens()
        {
            // Arrange
            var dto = new SpotifyCallbackDto { Code = "valid-code" };
            var tokenResult = new SpotifyTokenResult
            {
                Success = true,
                AccessToken = "access-token",
                RefreshToken = "refresh-token"
            };

            _spotifyMock
                .Setup(s => s.ExchangeCodeForToken(dto.Code))
                .ReturnsAsync(tokenResult);

            // Act
            var result = await _controller.Callback(dto);

            // Assert
            var okResult = Assert.IsType<OkObjectResult>(result);
            var response = okResult.Value;
            var accessTokenProp = response?.GetType().GetProperty("accessToken");
            var refreshTokenProp = response?.GetType().GetProperty("refreshToken");
            
            Assert.Equal("access-token", accessTokenProp?.GetValue(response)?.ToString());
            Assert.Equal("refresh-token", refreshTokenProp?.GetValue(response)?.ToString());
            _spotifyMock.Verify(s => s.ExchangeCodeForToken(dto.Code), Times.Once);
        }

        [Fact]
        public async Task Callback_InvalidCode_ReturnsBadRequest()
        {
            // Arrange
            var dto = new SpotifyCallbackDto { Code = "invalid-code" };
            var tokenResult = new SpotifyTokenResult
            {
                Success = false,
                Error = "Invalid authorization code"
            };

            _spotifyMock
                .Setup(s => s.ExchangeCodeForToken(dto.Code))
                .ReturnsAsync(tokenResult);

            // Act
            var result = await _controller.Callback(dto);

            // Assert
            var badRequestResult = Assert.IsType<BadRequestObjectResult>(result);
            var response = badRequestResult.Value;
            var errorProp = response?.GetType().GetProperty("error");
            Assert.Equal("Invalid authorization code", errorProp?.GetValue(response)?.ToString());
        }

        [Fact]
        public async Task Callback_EmptyCode_ReturnsBadRequest()
        {
            // Arrange
            var dto = new SpotifyCallbackDto { Code = "" };
            var tokenResult = new SpotifyTokenResult
            {
                Success = false,
                Error = "Code is required"
            };

            _spotifyMock
                .Setup(s => s.ExchangeCodeForToken(dto.Code))
                .ReturnsAsync(tokenResult);

            // Act
            var result = await _controller.Callback(dto);

            // Assert
            Assert.IsType<BadRequestObjectResult>(result);
        }

        // =============================
        // RefreshToken Tests
        // =============================

        [Fact]
        public async Task RefreshToken_ValidToken_ReturnsOkWithNewAccessToken()
        {
            // Arrange
            var dto = new RefreshTokenDto { RefreshToken = "valid-refresh-token" };
            var tokenResult = new SpotifyTokenResult
            {
                Success = true,
                AccessToken = "new-access-token"
            };

            _spotifyMock
                .Setup(s => s.RefreshAccessToken(dto.RefreshToken))
                .ReturnsAsync(tokenResult);

            // Act
            var result = await _controller.RefreshToken(dto);

            // Assert
            var okResult = Assert.IsType<OkObjectResult>(result);
            var response = okResult.Value;
            var accessTokenProp = response?.GetType().GetProperty("accessToken");
            Assert.Equal("new-access-token", accessTokenProp?.GetValue(response)?.ToString());
            _spotifyMock.Verify(s => s.RefreshAccessToken(dto.RefreshToken), Times.Once);
        }

        [Fact]
        public async Task RefreshToken_ExpiredToken_ReturnsUnauthorized()
        {
            // Arrange
            var dto = new RefreshTokenDto { RefreshToken = "expired-token" };
            var tokenResult = new SpotifyTokenResult
            {
                Success = false,
                Error = "Refresh token expired"
            };

            _spotifyMock
                .Setup(s => s.RefreshAccessToken(dto.RefreshToken))
                .ReturnsAsync(tokenResult);

            // Act
            var result = await _controller.RefreshToken(dto);

            // Assert
            var unauthorizedResult = Assert.IsType<UnauthorizedObjectResult>(result);
            var response = unauthorizedResult.Value;
            var errorProp = response?.GetType().GetProperty("error");
            Assert.Equal("Refresh token expired", errorProp?.GetValue(response)?.ToString());
        }

        [Fact]
        public async Task RefreshToken_EmptyToken_ReturnsBadRequest()
        {
            // Arrange
            var dto = new RefreshTokenDto { RefreshToken = "" };

            // Act
            var result = await _controller.RefreshToken(dto);

            // Assert
            var badRequestResult = Assert.IsType<BadRequestObjectResult>(result);
            var response = badRequestResult.Value;
            var errorProp = response?.GetType().GetProperty("error");
            Assert.Equal("Refresh token is required", errorProp?.GetValue(response)?.ToString());
            _spotifyMock.Verify(s => s.RefreshAccessToken(It.IsAny<string>()), Times.Never);
        }

        [Fact]
        public async Task RefreshToken_NullToken_ReturnsBadRequest()
        {
            // Arrange
            var dto = new RefreshTokenDto { RefreshToken = null! };

            // Act
            var result = await _controller.RefreshToken(dto);

            // Assert
            var badRequestResult = Assert.IsType<BadRequestObjectResult>(result);
            _spotifyMock.Verify(s => s.RefreshAccessToken(It.IsAny<string>()), Times.Never);
        }

        [Fact]
        public async Task RefreshToken_ServiceReturnsNull_ReturnsUnauthorized()
        {
            // Arrange
            var dto = new RefreshTokenDto { RefreshToken = "some-token" };
            var tokenResult = new SpotifyTokenResult
            {
                Success = false,
                Error = "Service unavailable"
            };

            _spotifyMock
                .Setup(s => s.RefreshAccessToken(dto.RefreshToken))
                .ReturnsAsync(tokenResult);

            // Act
            var result = await _controller.RefreshToken(dto);

            // Assert
            Assert.IsType<UnauthorizedObjectResult>(result);
        }
    }
}
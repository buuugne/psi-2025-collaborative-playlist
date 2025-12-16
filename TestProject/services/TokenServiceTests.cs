using Xunit;
using Moq;
using MyApi.Services;
using MyApi.Models;
using Microsoft.Extensions.Configuration;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Linq;

namespace TestProject
{
    public class TokenServiceTests
    {
        private IConfiguration GetMockConfiguration()
        {
            var inMemorySettings = new Dictionary<string, string>
            {
                {"Jwt:Key", "this-is-a-very-secure-test-key-with-at-least-32-characters"},
                {"Jwt:Issuer", "TestIssuer"},
                {"Jwt:Audience", "TestAudience"}
            };

            return new ConfigurationBuilder()
                .AddInMemoryCollection(inMemorySettings!)
                .Build();
        }

        [Fact]
        public void Generate_ShouldReturnValidJwtToken()
        {
            // --- ARRANGE ---
            var config = GetMockConfiguration();
            var tokenService = new TokenService(config);

            var user = new User
            {
                Id = 123,
                Username = "testuser",
                Role = UserRole.Host
            };

            // --- ACT ---
            var token = tokenService.Generate(user);

            // --- ASSERT ---
            Assert.NotNull(token);
            Assert.NotEmpty(token);
            
            // Token should have 3 parts separated by dots (header.payload.signature)
            var parts = token.Split('.');
            Assert.Equal(3, parts.Length);
        }

        [Fact]
        public void Generate_ShouldIncludeCorrectClaims()
        {
            // --- ARRANGE ---
            var config = GetMockConfiguration();
            var tokenService = new TokenService(config);

            var user = new User
            {
                Id = 456,
                Username = "johndoe",
                Role = UserRole.Host
            };

            // --- ACT ---
            var token = tokenService.Generate(user);

            // --- ASSERT ---
            var handler = new JwtSecurityTokenHandler();
            var jwtToken = handler.ReadJwtToken(token);

            // Check NameIdentifier claim (user ID)
            var userIdClaim = jwtToken.Claims.FirstOrDefault(c => c.Type == ClaimTypes.NameIdentifier);
            Assert.NotNull(userIdClaim);
            Assert.Equal("456", userIdClaim.Value);

            // Check Name claim (username)
            var nameClaim = jwtToken.Claims.FirstOrDefault(c => c.Type == ClaimTypes.Name);
            Assert.NotNull(nameClaim);
            Assert.Equal("johndoe", nameClaim.Value);

            // Check Role claim
            var roleClaim = jwtToken.Claims.FirstOrDefault(c => c.Type == ClaimTypes.Role);
            Assert.NotNull(roleClaim);
            Assert.Equal("Host", roleClaim.Value);
        }

        [Fact]
        public void Generate_ShouldIncludeCorrectIssuerAndAudience()
        {
            // --- ARRANGE ---
            var config = GetMockConfiguration();
            var tokenService = new TokenService(config);

            var user = new User
            {
                Id = 789,
                Username = "alice",
                Role = UserRole.Host
            };

            // --- ACT ---
            var token = tokenService.Generate(user);

            // --- ASSERT ---
            var handler = new JwtSecurityTokenHandler();
            var jwtToken = handler.ReadJwtToken(token);

            Assert.Equal("TestIssuer", jwtToken.Issuer);
            Assert.Contains("TestAudience", jwtToken.Audiences);
        }

        [Fact]
        public void Generate_ShouldSetExpirationTo7Days()
        {
            // --- ARRANGE ---
            var config = GetMockConfiguration();
            var tokenService = new TokenService(config);

            var user = new User
            {
                Id = 999,
                Username = "expirytest",
                Role = UserRole.Host
            };

            var beforeGeneration = DateTime.UtcNow;

            // --- ACT ---
            var token = tokenService.Generate(user);

            var afterGeneration = DateTime.UtcNow;

            // --- ASSERT ---
            var handler = new JwtSecurityTokenHandler();
            var jwtToken = handler.ReadJwtToken(token);

            // Token should expire approximately 7 days from now
            var expectedExpiry = beforeGeneration.AddDays(7);
            var actualExpiry = jwtToken.ValidTo;

            // Allow 10 seconds tolerance for test execution time
            var timeDifference = Math.Abs((actualExpiry - expectedExpiry).TotalSeconds);
            Assert.True(timeDifference < 10, $"Expected expiry around {expectedExpiry}, but got {actualExpiry}");
        }

        [Fact]
        public void Generate_ShouldCreateDifferentTokensForDifferentUsers()
        {
            // --- ARRANGE ---
            var config = GetMockConfiguration();
            var tokenService = new TokenService(config);

            var user1 = new User { Id = 1, Username = "user1", Role = UserRole.Host };
            var user2 = new User { Id = 2, Username = "user2", Role = UserRole.Host };

            // --- ACT ---
            var token1 = tokenService.Generate(user1);
            var token2 = tokenService.Generate(user2);

            // --- ASSERT ---
            Assert.NotEqual(token1, token2);
        }
    }
}
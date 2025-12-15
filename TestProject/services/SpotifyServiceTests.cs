using System.Net;
using System.Text;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Moq;
using MyApi.Services;
using Microsoft.Extensions.Configuration;
using Xunit;

namespace TestProject.Services
{
    public class SpotifyServiceTests
    {
        // -----------------------------
        // SENI TESTAI (be dublikatų)
        // -----------------------------

        [Fact]
        public async Task SearchTracks_WithMissingCredentials_ShouldReturnError()
        {
            var http = new HttpClient();
            var cfg = new Mock<IConfiguration>();

            cfg.Setup(x => x["Spotify:ClientID"]).Returns((string?)null);
            cfg.Setup(x => x["Spotify:ClientSecret"]).Returns((string?)null);

            var service = new SpotifyService(http, cfg.Object);

            var (success, error, json) = await service.SearchTracks("test");

            Assert.False(success);
            Assert.Equal("Spotify credentials not configured", error);
            Assert.Null(json);
        }

        [Fact]
        public async Task SearchTracks_WithOnlyClientIdMissing_ShouldReturnError()
        {
            var http = new HttpClient();
            var cfg = new Mock<IConfiguration>();

            cfg.Setup(x => x["Spotify:ClientID"]).Returns((string?)null);
            cfg.Setup(x => x["Spotify:ClientSecret"]).Returns("secret");

            var service = new SpotifyService(http, cfg.Object);

            var (success, error, json) = await service.SearchTracks("test");

            Assert.False(success);
            Assert.Equal("Spotify credentials not configured", error);
            Assert.Null(json);
        }

        [Fact]
        public async Task SearchTracks_WithOnlyClientSecretMissing_ShouldReturnError()
        {
            var http = new HttpClient();
            var cfg = new Mock<IConfiguration>();

            cfg.Setup(x => x["Spotify:ClientID"]).Returns("id");
            cfg.Setup(x => x["Spotify:ClientSecret"]).Returns((string?)null);

            var service = new SpotifyService(http, cfg.Object);

            var (success, error, json) = await service.SearchTracks("test");

            Assert.False(success);
            Assert.Equal("Spotify credentials not configured", error);
            Assert.Null(json);
        }

        [Fact]
        public async Task SearchTracks_WithEmptyClientId_ShouldReturnError()
        {
            var http = new HttpClient();
            var cfg = new Mock<IConfiguration>();

            cfg.Setup(x => x["Spotify:ClientID"]).Returns("");
            cfg.Setup(x => x["Spotify:ClientSecret"]).Returns("secret");

            var service = new SpotifyService(http, cfg.Object);

            var (success, error, json) = await service.SearchTracks("test");

            Assert.False(success);
            Assert.Equal("Spotify credentials not configured", error);
            Assert.Null(json);
        }

        [Fact]
        public async Task SearchTracks_WithEmptyClientSecret_ShouldReturnError()
        {
            var http = new HttpClient();
            var cfg = new Mock<IConfiguration>();

            cfg.Setup(x => x["Spotify:ClientID"]).Returns("id");
            cfg.Setup(x => x["Spotify:ClientSecret"]).Returns("");

            var service = new SpotifyService(http, cfg.Object);

            var (success, error, json) = await service.SearchTracks("test");

            Assert.False(success);
            Assert.Equal("Spotify credentials not configured", error);
            Assert.Null(json);
        }

        // -----------------------------
        // NAUJI TESTAI
        // -----------------------------

        [Fact]
        public void GenerateLoginUrl_ShouldContainClientIdAndRedirect()
        {
            var cfg = BuildConfig(
                ("Spotify:ClientID", "abc"),
                ("Spotify:RedirectUri", "https://example.com/callback")
            );

            var service = new SpotifyService(new HttpClient(), cfg);

            var url = service.GenerateLoginUrl();

            Assert.Contains("accounts.spotify.com/authorize", url);
            Assert.Contains("client_id=abc", url);
            Assert.Contains("redirect_uri=", url);
            Assert.Contains("response_type=code", url);
        }

        [Fact]
        public async Task ExchangeCodeForToken_ShouldSucceed()
        {
            var cfg = BuildConfig(
                ("Spotify:ClientID", "id"),
                ("Spotify:ClientSecret", "secret"),
                ("Spotify:RedirectUri", "https://cb")
            );

            var json = """
            {
              "access_token": "A",
              "refresh_token": "R",
              "expires_in": 3600
            }
            """;

            var http = CreateHttpClient(_ =>
                new HttpResponseMessage(HttpStatusCode.OK)
                {
                    Content = new StringContent(json, Encoding.UTF8, "application/json")
                });

            var service = new SpotifyService(http, cfg);

            var result = await service.ExchangeCodeForToken("CODE");

            Assert.True(result.Success);
            Assert.Equal("A", result.AccessToken);
            Assert.Equal("R", result.RefreshToken);
        }

        [Fact]
        public async Task RefreshAccessToken_ShouldFail_OnUnauthorized()
        {
            var cfg = BuildConfig(
                ("Spotify:ClientID", "id"),
                ("Spotify:ClientSecret", "secret")
            );

            var http = CreateHttpClient(_ =>
                new HttpResponseMessage(HttpStatusCode.Unauthorized));

            var service = new SpotifyService(http, cfg);

            var result = await service.RefreshAccessToken("BAD");

            Assert.False(result.Success);
            Assert.NotNull(result.Error);
        }

        // -----------------------------
        // HELPERS
        // -----------------------------

        private static IConfiguration BuildConfig(params (string Key, string Value)[] pairs)
        {
            var dict = pairs.ToDictionary(x => x.Key, x => x.Value);
            return new ConfigurationBuilder()
                .AddInMemoryCollection(dict!)
                .Build();
        }

        private static HttpClient CreateHttpClient(Func<HttpRequestMessage, HttpResponseMessage> responder)
        {
            var handler = new FakeHttpMessageHandler(responder);
            return new HttpClient(handler);
        }

        private sealed class FakeHttpMessageHandler : HttpMessageHandler
        {
            private readonly Func<HttpRequestMessage, HttpResponseMessage> _responder;

            public FakeHttpMessageHandler(Func<HttpRequestMessage, HttpResponseMessage> responder)
                => _responder = responder;

            protected override Task<HttpResponseMessage> SendAsync(
                HttpRequestMessage request,
                CancellationToken cancellationToken)
                => Task.FromResult(_responder(request));
        }
    }
}

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
        // GetTrackDetails TESTAI
        // -----------------------------

        [Fact]
        public async Task GetTrackDetails_MissingCredentials_ShouldReturnError()
        {
            var http = new HttpClient();
            var cfg = new Mock<IConfiguration>();

            cfg.Setup(x => x["Spotify:ClientID"]).Returns((string?)null);
            cfg.Setup(x => x["Spotify:ClientSecret"]).Returns((string?)null);

            var service = new SpotifyService(http, cfg.Object);

            var (success, error, details) = await service.GetTrackDetails("trackId123");

            Assert.False(success);
            Assert.Equal("Spotify credentials not configured", error);
            Assert.Null(details);
        }

        [Fact]
        public async Task GetTrackDetails_MissingClientId_ShouldReturnError()
        {
            var http = new HttpClient();
            var cfg = new Mock<IConfiguration>();

            cfg.Setup(x => x["Spotify:ClientID"]).Returns((string?)null);
            cfg.Setup(x => x["Spotify:ClientSecret"]).Returns("secret");

            var service = new SpotifyService(http, cfg.Object);

            var (success, error, details) = await service.GetTrackDetails("trackId123");

            Assert.False(success);
            Assert.Equal("Spotify credentials not configured", error);
            Assert.Null(details);
        }

        [Fact]
        public async Task GetTrackDetails_MissingClientSecret_ShouldReturnError()
        {
            var http = new HttpClient();
            var cfg = new Mock<IConfiguration>();

            cfg.Setup(x => x["Spotify:ClientID"]).Returns("id");
            cfg.Setup(x => x["Spotify:ClientSecret"]).Returns((string?)null);

            var service = new SpotifyService(http, cfg.Object);

            var (success, error, details) = await service.GetTrackDetails("trackId123");

            Assert.False(success);
            Assert.Equal("Spotify credentials not configured", error);
            Assert.Null(details);
        }

        [Fact]
        public async Task GetTrackDetails_WithValidCredentials_AttemptsToGetToken()
        {
            // This test verifies that the method attempts to get a token
            // but will fail because GetSpotifyToken creates its own HttpClient
            var cfg = BuildConfig(
                ("Spotify:ClientID", "testId"),
                ("Spotify:ClientSecret", "testSecret")
            );

            var http = new HttpClient();
            var service = new SpotifyService(http, cfg);

            var (success, error, details) = await service.GetTrackDetails("track123");

            // Will fail to get token since we can't mock the internal HttpClient
            Assert.False(success);
            Assert.Contains("Failed to get Spotify access token", error);
            Assert.Null(details);
        }

        [Fact]
        public async Task GetTrackDetails_WithValidCredentials_FailsToGetToken()
        {
            // Since GetSpotifyToken creates its own HttpClient internally,
            // we cannot mock it, so this test verifies the failure path
            var cfg = BuildConfig(
                ("Spotify:ClientID", "testId"),
                ("Spotify:ClientSecret", "testSecret")
            );

            var http = new HttpClient();
            var service = new SpotifyService(http, cfg);

            var (success, error, details) = await service.GetTrackDetails("invalidTrack");

            Assert.False(success);
            Assert.Contains("Failed to get Spotify access token", error);
            Assert.Null(details);
        }

        // -----------------------------
        // SearchTracks TESTAI
        // -----------------------------

        [Fact]
        public async Task SearchTracks_MissingCredentials_ShouldReturnError()
        {
            var http = new HttpClient();
            var cfg = new Mock<IConfiguration>();

            cfg.Setup(x => x["Spotify:ClientID"]).Returns((string?)null);
            cfg.Setup(x => x["Spotify:ClientSecret"]).Returns((string?)null);

            var service = new SpotifyService(http, cfg.Object);

            var (success, error, json) = await service.SearchTracks("test query");

            Assert.False(success);
            Assert.Equal("Spotify credentials not configured", error);
            Assert.Null(json);
        }

        [Fact]
        public async Task SearchTracks_MissingClientId_ShouldReturnError()
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
        public async Task SearchTracks_MissingClientSecret_ShouldReturnError()
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
        public async Task SearchTracks_EmptyClientId_ShouldReturnError()
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
        public async Task SearchTracks_EmptyClientSecret_ShouldReturnError()
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

        [Fact]
        public async Task SearchTracks_WithValidCredentials_FailsToGetToken()
        {
            // GetSpotifyToken creates its own HttpClient, so we can't mock it
            // This test verifies the error path when token retrieval fails
            var cfg = BuildConfig(
                ("Spotify:ClientID", "testId"),
                ("Spotify:ClientSecret", "testSecret")
            );

            var http = new HttpClient();
            var service = new SpotifyService(http, cfg);

            var (success, error, json) = await service.SearchTracks("test song");

            Assert.False(success);
            Assert.Contains("Failed to get Spotify access token", error);
            Assert.Null(json);
        }

        [Fact]
        public async Task SearchTracks_WithValidCredentials_FailsToGetToken2()
        {
            // GetSpotifyToken creates its own HttpClient internally
            // This test verifies the error handling when token retrieval fails
            var cfg = BuildConfig(
                ("Spotify:ClientID", "testId"),
                ("Spotify:ClientSecret", "testSecret")
            );

            var http = new HttpClient();
            var service = new SpotifyService(http, cfg);

            var (success, error, json) = await service.SearchTracks("test");

            Assert.False(success);
            Assert.Contains("Failed to get Spotify access token", error);
            Assert.Null(json);
        }

        // -----------------------------
        // OAUTH TESTAI
        // -----------------------------

        [Fact]
        public void GenerateLoginUrl_ShouldContainClientIdAndRedirectUri()
        {
            var cfg = BuildConfig(
                ("Spotify:ClientID", "abc123"),
                ("Spotify:RedirectUri", "https://example.com/callback")
            );

            var service = new SpotifyService(new HttpClient(), cfg);

            var url = service.GenerateLoginUrl();

            Assert.Contains("https://accounts.spotify.com/authorize", url);
            Assert.Contains("response_type=code", url);
            Assert.Contains("client_id=abc123", url);
            Assert.Contains("redirect_uri=", url);
            Assert.Contains("scope=", url);
        }

        [Fact]
        public async Task ExchangeCodeForToken_ShouldReturnSuccess_WhenSpotifyReturnsTokens()
        {
            var cfg = BuildConfig(
                ("Spotify:ClientID", "id"),
                ("Spotify:ClientSecret", "secret"),
                ("Spotify:RedirectUri", "https://example.com/callback")
            );

            var json = """
            {
              "access_token": "ACCESS_X",
              "token_type": "Bearer",
              "expires_in": 3600,
              "refresh_token": "REFRESH_Y"
            }
            """;

            var http = CreateHttpClient(req =>
            {
                Assert.Equal(HttpMethod.Post, req.Method);
                Assert.Equal("https://accounts.spotify.com/api/token", req.RequestUri!.ToString());
                return new HttpResponseMessage(HttpStatusCode.OK)
                {
                    Content = new StringContent(json, Encoding.UTF8, "application/json")
                };
            });

            var service = new SpotifyService(http, cfg);

            var result = await service.ExchangeCodeForToken("CODE123");

            Assert.True(result.Success);
            Assert.Equal("ACCESS_X", result.AccessToken);
            Assert.Equal("REFRESH_Y", result.RefreshToken);
            Assert.Null(result.Error);
        }

        [Fact]
        public async Task ExchangeCodeForToken_ShouldReturnFail_WhenSpotifyReturnsNonSuccess()
        {
            var cfg = BuildConfig(
                ("Spotify:ClientID", "id"),
                ("Spotify:ClientSecret", "secret"),
                ("Spotify:RedirectUri", "https://example.com/callback")
            );

            var http = CreateHttpClient(_ => new HttpResponseMessage(HttpStatusCode.BadRequest)
            {
                Content = new StringContent("{\"error\":\"invalid_grant\"}")
            });

            var service = new SpotifyService(http, cfg);

            var result = await service.ExchangeCodeForToken("BAD_CODE");

            Assert.False(result.Success);
            Assert.NotNull(result.Error);
            Assert.Null(result.AccessToken);
        }

        [Fact]
        public async Task RefreshAccessToken_ShouldReturnSuccess_WhenSpotifyReturnsAccessToken()
        {
            var cfg = BuildConfig(
                ("Spotify:ClientID", "id"),
                ("Spotify:ClientSecret", "secret")
            );

            var json = """
            {
              "access_token": "NEW_ACCESS",
              "token_type": "Bearer",
              "expires_in": 3600
            }
            """;

            var http = CreateHttpClient(req =>
            {
                Assert.Equal(HttpMethod.Post, req.Method);
                Assert.Equal("https://accounts.spotify.com/api/token", req.RequestUri!.ToString());
                return new HttpResponseMessage(HttpStatusCode.OK)
                {
                    Content = new StringContent(json, Encoding.UTF8, "application/json")
                };
            });

            var service = new SpotifyService(http, cfg);

            var result = await service.RefreshAccessToken("REFRESH_TOKEN");

            Assert.True(result.Success);
            Assert.Equal("NEW_ACCESS", result.AccessToken);
            Assert.Null(result.Error);
        }

        [Fact]
        public async Task RefreshAccessToken_ShouldReturnFail_WhenSpotifyReturnsNonSuccess()
        {
            var cfg = BuildConfig(
                ("Spotify:ClientID", "id"),
                ("Spotify:ClientSecret", "secret")
            );

            var http = CreateHttpClient(_ => new HttpResponseMessage(HttpStatusCode.Unauthorized)
            {
                Content = new StringContent("{\"error\":\"invalid_client\"}")
            });

            var service = new SpotifyService(http, cfg);

            var result = await service.RefreshAccessToken("whatever");

            Assert.False(result.Success);
            Assert.NotNull(result.Error);
            Assert.Null(result.AccessToken);
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
            public FakeHttpMessageHandler(Func<HttpRequestMessage, HttpResponseMessage> responder) => _responder = responder;

            protected override Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
                => Task.FromResult(_responder(request));
        }
    }
}
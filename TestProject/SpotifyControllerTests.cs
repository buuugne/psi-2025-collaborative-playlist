using Microsoft.AspNetCore.Mvc;
using Moq;
using Xunit;
using MyApi.Controllers;
using MyApi.Services;

namespace TestProject
{
    public class SpotifyControllerTests
    {
        [Fact]
        public async Task Search_ShouldReturnContentResult_WhenSuccess()
        {
            var svc = new Mock<ISpotifyService>();
            svc.Setup(x => x.SearchTracks("hello"))
               .ReturnsAsync((true, (string?)null, "{\"ok\":true}"));

            var controller = new SpotifyController(svc.Object);

            var result = await controller.Search("hello");

            var content = Assert.IsType<ContentResult>(result);
            Assert.Equal("application/json", content.ContentType);
            Assert.Equal("{\"ok\":true}", content.Content);
            Assert.Null(content.StatusCode); // default 200
        }

        [Fact]
        public async Task Search_ShouldReturn500_WhenFail_WithErrorMessage()
        {
            var svc = new Mock<ISpotifyService>();
            svc.Setup(x => x.SearchTracks("hello"))
               .ReturnsAsync((false, "boom", (string?)null));

            var controller = new SpotifyController(svc.Object);

            var result = await controller.Search("hello");

            var obj = Assert.IsType<ObjectResult>(result);
            Assert.Equal(500, obj.StatusCode);
            Assert.Equal("boom", obj.Value);
        }

        [Fact]
        public async Task Search_ShouldReturn500_WhenFail_WithNullError_UsesDefaultMessage()
        {
            var svc = new Mock<ISpotifyService>();
            svc.Setup(x => x.SearchTracks("hello"))
               .ReturnsAsync((false, (string?)null, (string?)null));

            var controller = new SpotifyController(svc.Object);

            var result = await controller.Search("hello");

            var obj = Assert.IsType<ObjectResult>(result);
            Assert.Equal(500, obj.StatusCode);
            Assert.Equal("Spotify search failed", obj.Value);
        }
    }
}

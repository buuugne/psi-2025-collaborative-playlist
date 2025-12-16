using Xunit;
using MyApi.Controllers;

namespace TestProject.Controllers
{
    public class CollaborativePlaylistControllerAdditionalTests
    {
        [Fact]
        public void AddCollaboratorRequest_CanSetAndGetProperties()
        {
            var dto = new AddCollaboratorRequest
            {
                UserId = 123,
                RequesterId = 456
            };

            Assert.Equal(123, dto.UserId);
            Assert.Equal(456, dto.RequesterId);
        }

        [Fact]
        public void AddSongRequest_CanSetAndGetProperties()
        {
            var dto = new AddSongRequest
            {
                SongId = 77,
                UserId = 88
            };

            Assert.Equal(77, dto.SongId);
            Assert.Equal(88, dto.UserId);
        }
    }
}

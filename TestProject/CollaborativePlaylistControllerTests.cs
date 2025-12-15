using Xunit;
using Moq;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Http;
using MyApi.Controllers;
using MyApi.Services;
using MyApi.Dtos;
using System.Collections.Generic;
using System.Threading.Tasks;
using System.Security.Claims;

namespace TestProject.Controllers
{
    public class CollaborativePlaylistControllerTests
    {
        private readonly Mock<ICollaborativePlaylistService> _serviceMock;
        private readonly CollaborativePlaylistController _controller;

        public CollaborativePlaylistControllerTests()
        {
            _serviceMock = new Mock<ICollaborativePlaylistService>(MockBehavior.Strict);
            _controller = new CollaborativePlaylistController(_serviceMock.Object);
        }

        // ---------------------------
        // Helpers
        // ---------------------------
        private void SetUser(int userId)
        {
            _controller.ControllerContext = new ControllerContext
            {
                HttpContext = new DefaultHttpContext
                {
                    User = new ClaimsPrincipal(
                        new ClaimsIdentity(
                            new[] { new Claim(ClaimTypes.NameIdentifier, userId.ToString()) },
                            "TestAuth"))
                }
            };
        }

        // ---------------------------
        // GET COLLABORATORS
        // ---------------------------

        [Fact]
        public async Task GetCollaborators_ReturnsOk_WhenPlaylistExists()
        {
            var collaborators = new List<UserDto>
            {
                new UserDto { Id = 1, Username = "user1" }
            };

            _serviceMock.Setup(s => s.GetCollaboratorsAsync(1))
                        .ReturnsAsync((IEnumerable<UserDto>)collaborators);

            var result = await _controller.GetCollaborators(1);

            var ok = Assert.IsType<OkObjectResult>(result);
            var users = Assert.IsAssignableFrom<IEnumerable<UserDto>>(ok.Value);
            Assert.Single(users);

            _serviceMock.VerifyAll();
        }

        [Fact]
        public async Task GetCollaborators_ReturnsNotFound_WhenPlaylistDoesNotExist()
        {
            _serviceMock.Setup(s => s.GetCollaboratorsAsync(1))
                        .ReturnsAsync((IEnumerable<UserDto>?)null);

            var result = await _controller.GetCollaborators(1);

            Assert.IsType<NotFoundObjectResult>(result);
            _serviceMock.VerifyAll();
        }

        // ---------------------------
        // ADD COLLABORATOR
        // ---------------------------

        [Fact]
        public async Task AddCollaborator_ReturnsOk_WhenSuccess()
        {
            SetUser(1);

            _serviceMock.Setup(s => s.AddCollaboratorByUsernameAsync(1, "john", 1))
                        .ReturnsAsync((true, (string?)null));

            var request = new AddCollaboratorByUsernameRequest { Username = "john" };
            var result = await _controller.AddCollaborator(1, request);

            Assert.IsType<OkObjectResult>(result);
            _serviceMock.VerifyAll();
        }

        [Fact]
        public async Task AddCollaborator_ReturnsBadRequest_WhenServiceFails()
        {
            SetUser(1);

            _serviceMock.Setup(s => s.AddCollaboratorByUsernameAsync(1, "john", 1))
                        .ReturnsAsync((false, "Error message"));

            var request = new AddCollaboratorByUsernameRequest { Username = "john" };
            var result = await _controller.AddCollaborator(1, request);

            Assert.IsType<BadRequestObjectResult>(result);
            _serviceMock.VerifyAll();
        }

        [Fact]
        public async Task AddCollaborator_ReturnsBadRequest_WhenUsernameMissing()
        {
            SetUser(1);

            var request = new AddCollaboratorByUsernameRequest { Username = " " };
            var result = await _controller.AddCollaborator(1, request);

            Assert.IsType<BadRequestObjectResult>(result);
            _serviceMock.VerifyNoOtherCalls();
        }

        [Fact]
        public async Task AddCollaborator_ReturnsUnauthorized_WhenTokenMissing()
        {
            var request = new AddCollaboratorByUsernameRequest { Username = "john" };
            var result = await _controller.AddCollaborator(1, request);

            Assert.IsType<UnauthorizedObjectResult>(result);
            _serviceMock.VerifyNoOtherCalls();
        }

        // ---------------------------
        // REMOVE COLLABORATOR
        // ---------------------------

        [Fact]
        public async Task RemoveCollaborator_ReturnsOk_WhenSuccess()
        {
            SetUser(1);

            _serviceMock.Setup(s => s.RemoveCollaboratorAsync(1, 2, 1))
                        .ReturnsAsync((true, (string?)null));

            var result = await _controller.RemoveCollaborator(1, 2);

            Assert.IsType<OkObjectResult>(result);
            _serviceMock.VerifyAll();
        }

        [Fact]
        public async Task RemoveCollaborator_ReturnsBadRequest_WhenServiceFails()
        {
            SetUser(1);

            _serviceMock.Setup(s => s.RemoveCollaboratorAsync(1, 2, 1))
                        .ReturnsAsync((false, "Error message"));

            var result = await _controller.RemoveCollaborator(1, 2);

            Assert.IsType<BadRequestObjectResult>(result);
            _serviceMock.VerifyAll();
        }

        [Fact]
        public async Task RemoveCollaborator_ReturnsUnauthorized_WhenTokenMissing()
        {
            var result = await _controller.RemoveCollaborator(1, 2);

            Assert.IsType<UnauthorizedObjectResult>(result);
            _serviceMock.VerifyNoOtherCalls();
        }

        // ---------------------------
        // ADD SONG
        // ---------------------------

        [Fact]
        public async Task AddSongToPlaylist_ReturnsOk_WhenSuccess()
        {
            SetUser(1);

            _serviceMock.Setup(s => s.AddSongAsync(1, 2, 1))
                        .ReturnsAsync((true, (string?)null));

            var request = new AddSongToCollaborativePlaylistRequest { SongId = 2 };
            var result = await _controller.AddSongToPlaylist(1, request);

            Assert.IsType<OkObjectResult>(result);
            _serviceMock.VerifyAll();
        }

        [Fact]
        public async Task AddSongToPlaylist_ReturnsBadRequest_WhenServiceFails()
        {
            SetUser(1);

            _serviceMock.Setup(s => s.AddSongAsync(1, 2, 1))
                        .ReturnsAsync((false, "Error message"));

            var request = new AddSongToCollaborativePlaylistRequest { SongId = 2 };
            var result = await _controller.AddSongToPlaylist(1, request);

            Assert.IsType<BadRequestObjectResult>(result);
            _serviceMock.VerifyAll();
        }

        [Fact]
        public async Task AddSongToPlaylist_ReturnsBadRequest_WhenSongIdInvalid()
        {
            SetUser(1);

            var request = new AddSongToCollaborativePlaylistRequest { SongId = 0 };
            var result = await _controller.AddSongToPlaylist(1, request);

            Assert.IsType<BadRequestObjectResult>(result);
            _serviceMock.VerifyNoOtherCalls();
        }

        [Fact]
        public async Task AddSongToPlaylist_ReturnsUnauthorized_WhenTokenMissing()
        {
            var request = new AddSongToCollaborativePlaylistRequest { SongId = 2 };
            var result = await _controller.AddSongToPlaylist(1, request);

            Assert.IsType<UnauthorizedObjectResult>(result);
            _serviceMock.VerifyNoOtherCalls();
        }

        // ---------------------------
        // REMOVE SONG
        // ---------------------------

        [Fact]
        public async Task RemoveSongFromPlaylist_ReturnsOk_WhenSuccess()
        {
            SetUser(1);

            _serviceMock.Setup(s => s.RemoveSongAsync(1, 2, 1))
                        .ReturnsAsync((true, (string?)null));

            var result = await _controller.RemoveSongFromPlaylist(1, 2);

            Assert.IsType<OkObjectResult>(result);
            _serviceMock.VerifyAll();
        }

        [Fact]
        public async Task RemoveSongFromPlaylist_ReturnsBadRequest_WhenServiceFails()
        {
            SetUser(1);

            _serviceMock.Setup(s => s.RemoveSongAsync(1, 2, 1))
                        .ReturnsAsync((false, "Error message"));

            var result = await _controller.RemoveSongFromPlaylist(1, 2);

            Assert.IsType<BadRequestObjectResult>(result);
            _serviceMock.VerifyAll();
        }

        [Fact]
        public async Task RemoveSongFromPlaylist_ReturnsUnauthorized_WhenTokenMissing()
        {
            var result = await _controller.RemoveSongFromPlaylist(1, 2);

            Assert.IsType<UnauthorizedObjectResult>(result);
            _serviceMock.VerifyNoOtherCalls();
        }

        // ---------------------------
        // CHECK ACCESS
        // ---------------------------

        [Fact]
        public async Task CheckAccess_ReturnsOk_WithHasAccess()
        {
            SetUser(1);

            _serviceMock.Setup(s => s.CanAccessPlaylistAsync(1, 1))
                        .ReturnsAsync(true);

            var result = await _controller.CheckAccess(1);

            Assert.IsType<OkObjectResult>(result);
            _serviceMock.VerifyAll();
        }

        [Fact]
        public async Task CheckAccess_ReturnsUnauthorized_WhenTokenMissing()
        {
            var result = await _controller.CheckAccess(1);

            Assert.IsType<UnauthorizedObjectResult>(result);
            _serviceMock.VerifyNoOtherCalls();
        }

        // ---------------------------
        // JOIN / LEAVE SESSION
        // ---------------------------

        [Fact]
        public async Task JoinSession_ReturnsOk_WhenTokenValid()
        {
            SetUser(1);

            _serviceMock.Setup(s => s.JoinPlaylistSessionAsync(1, 1))
                        .Returns(Task.CompletedTask);

            var result = await _controller.JoinSession(1);

            Assert.IsType<OkObjectResult>(result);
            _serviceMock.VerifyAll();
        }

        [Fact]
        public async Task JoinSession_ReturnsUnauthorized_WhenTokenMissing()
        {
            var result = await _controller.JoinSession(1);

            Assert.IsType<UnauthorizedObjectResult>(result);
            _serviceMock.VerifyNoOtherCalls();
        }

        [Fact]
        public async Task LeaveSession_ReturnsOk_WhenTokenValid()
        {
            SetUser(1);

            _serviceMock.Setup(s => s.LeavePlaylistSessionAsync(1, 1))
                        .Returns(Task.CompletedTask);

            var result = await _controller.LeaveSession(1);

            Assert.IsType<OkObjectResult>(result);
            _serviceMock.VerifyAll();
        }

        [Fact]
        public async Task LeaveSession_ReturnsUnauthorized_WhenTokenMissing()
        {
            var result = await _controller.LeaveSession(1);

            Assert.IsType<UnauthorizedObjectResult>(result);
            _serviceMock.VerifyNoOtherCalls();
        }

        // ---------------------------
        // GET ACTIVE USERS
        // ---------------------------

        [Fact]
        public async Task GetActiveUsers_ReturnsOk()
        {
            var activeUsers = new List<ActiveUserDto>
            {
                new ActiveUserDto { UserId = 1 }
            };

            _serviceMock.Setup(s => s.GetActiveUsersAsync(1))
                        .ReturnsAsync((IEnumerable<ActiveUserDto>)activeUsers);

            var result = await _controller.GetActiveUsers(1);

            var ok = Assert.IsType<OkObjectResult>(result);
            var users = Assert.IsAssignableFrom<IEnumerable<ActiveUserDto>>(ok.Value);
            Assert.Single(users);

            _serviceMock.VerifyAll();
        }
    }
}
